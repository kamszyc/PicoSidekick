from config import *
import microcontroller
import usb_hid
from constants import TFT_HEIGHT, TFT_WIDTH
from touch_context import TouchContext, EVT_NO, EVT_PenDown, EVT_PenUp
import usb_cdc
from adafruit_hid.consumer_control import ConsumerControl
from adafruit_hid.consumer_control_code import ConsumerControlCode
import time
import asyncio
import busio
import terminalio
import displayio
import json
from adafruit_display_text import label, scrolling_label
import adafruit_ili9341
from adafruit_button import Button
from adafruit_button.sprite_button import SpriteButton
from adafruit_display_shapes.rect import Rect
from adafruit_displayio_layout.layouts.page_layout import PageLayout
from pinout import *
import pwmio
import fourwire

SHUTDOWN_BUTTON_TEXT = "SHUTDOWN"
CONFIRMATION_TEXT = "SURE?"

class DisplayController:
    def __init__(self, touch_context):
        self.touch_context = touch_context
        self.page_layout = PageLayout(x=0, y=0)

        self.play_button = create_sprite_button(136, 95, "play")
        self.pause_button = create_sprite_button(136, 95, "pause")
        self.prev_button = create_sprite_button(78, 95, "previous")
        self.next_button = create_sprite_button(194, 95, "next")
        self.vol_minus_button = create_sprite_button(20, 95, "volume_down")
        self.vol_plus_button = create_sprite_button(252, 95, "volume_up")
        self.settings_button = create_text_button(240, 180, "SETTINGS")

        self.shutdown_button = create_text_button(135, 90, SHUTDOWN_BUTTON_TEXT)
        self.back_button = create_text_button(135, 170, "BACK")
        devmode_text = "DEVMODE OFF" if dev_mode_enabled() else "DEVMODE ON"
        self.devmode_button = create_text_button(135, 10, devmode_text)

        self.cc = ConsumerControl(usb_hid.devices)

        self.pwm = None
        self.display = None

    async def handle_touch(self):
        shutdown_pressed = False
        while True:
            curTick = time.monotonic()
            if curTick >= self.touch_context.NxTick:
                self.touch_context.NxTick = curTick + self.touch_context.taskInterval_50ms
                self.touch_context.touch_det_task()

            # handle touch event
            if self.touch_context.touchEvent != EVT_NO:
                if self.touch_context.touchEvent == EVT_PenDown:
                    print('ev PenDown - ', self.touch_context.touchedX, " : ", self.touch_context.touchedY)

                if self.touch_context.touchEvent == EVT_PenUp:
                    print('ev PenUp - ')

                    if self.page_layout.showing_page_name == "main_page":
                        if self.play_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)) or self.pause_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                            print('play/pause')
                            self.cc.send(ConsumerControlCode.PLAY_PAUSE)
                        elif self.prev_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                            print('prev')
                            self.cc.send(ConsumerControlCode.SCAN_PREVIOUS_TRACK)
                        elif self.next_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                            print('next')
                            self.cc.send(ConsumerControlCode.SCAN_NEXT_TRACK)
                        elif self.vol_minus_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                            print('vol-')
                            self.cc.send(ConsumerControlCode.VOLUME_DECREMENT)
                            self.cc.send(ConsumerControlCode.VOLUME_DECREMENT)
                        elif self.vol_plus_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                            print('vol+')
                            self.cc.send(ConsumerControlCode.VOLUME_INCREMENT)
                            self.cc.send(ConsumerControlCode.VOLUME_INCREMENT)
                        elif self.settings_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                            print('settings')
                            self.page_layout.show_page("settings_page")

                    if self.page_layout.showing_page_name == "settings_page":
                        if self.shutdown_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                            print('shutdown')
                            if not shutdown_pressed:
                                self.shutdown_button.label = CONFIRMATION_TEXT
                                shutdown_pressed = True
                            else:
                                self.shutdown_button.label = SHUTDOWN_BUTTON_TEXT
                                usb_cdc.data.write(json.dumps({'command':'shutdown'}) + '\n')
                                shutdown_pressed = False
                        else:
                            self.shutdown_button.label = SHUTDOWN_BUTTON_TEXT
                            shutdown_pressed = False
                            if self.devmode_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                                print('devmode on/off')
                                toggle_dev_mode()
                            elif self.back_button.contains((self.touch_context.touchedY, self.touch_context.touchedX)):
                                print('back')
                                self.page_layout.show_page("main_page")

                self.touch_context.touchEvent = EVT_NO

            await asyncio.sleep(0)

    async def render_display(self):
        displayio.release_displays()

        self.pwm = pwmio.PWMOut(TFT_LED)
        self.pwm.duty_cycle = percent_to_duty_cycle(get_brightness_percent() or 50)

        tft_spi = busio.SPI(TFT_SPI_CLK, MOSI=TFT_SPI_MOSI)

        display_bus = fourwire.FourWire(
            tft_spi, command=TFT_DC, chip_select=TFT_CS, reset=TFT_RES)

        self.display = adafruit_ili9341.ILI9341(
            display_bus,
            width=TFT_WIDTH,
            height=TFT_HEIGHT,
            auto_refresh=False,
            rotation=180 if display_rotated() else 0)

        root = displayio.Group()
        self.display.root_group = root
        root.append(self.page_layout)

        main_group = displayio.Group()
        idle_group = create_idle_page()
        settings_group = create_settings_page(self.shutdown_button, self.devmode_button, self.back_button)
        self.page_layout.add_content(idle_group, "idle_page")
        self.page_layout.add_content(main_group, "main_page")
        self.page_layout.add_content(settings_group, "settings_page")

        IDLE_MUSIC = "No media playing"

        text_group = displayio.Group(scale=2, x=10, y=10)

        time_label = label.Label(terminalio.FONT, text="xx:xx", scale=2)
        time_label.x = 45
        time_label.y = 10
        text_group.append(time_label)

        music_label = scrolling_label.ScrollingLabel(terminalio.FONT, text=IDLE_MUSIC, max_characters=25, animate_time=0.5, color=0x0)
        music_label.x = 25
        music_label.y = 30
        text_group.append(music_label)

        cpu_label = label.Label(terminalio.FONT, text="CPU: unknown", color=0xFFFFFF)
        cpu_label.y = 90
        text_group.append(cpu_label)

        ram_label = label.Label(terminalio.FONT, text="RAM: unknown", color=0xFFFFFF)
        ram_label.y = 105
        text_group.append(ram_label)

        rect1 = Rect(0, 0, 320, 55, fill=0x0000FF)
        main_group.append(rect1)

        rect2 = Rect(0, 55, 320, 100, fill=0xFFFFFF)
        main_group.append(rect2)

        main_group.append(text_group)
        main_group.append(self.play_button)
        main_group.append(self.pause_button)
        main_group.append(self.prev_button)
        main_group.append(self.next_button)
        main_group.append(self.vol_minus_button)
        main_group.append(self.vol_plus_button)
        main_group.append(self.settings_button)

        iterations_without_update = 0
        while True:
            if usb_cdc.data.in_waiting > 0:
                try:
                    data_in = usb_cdc.data.readline()
                    request = json.loads(data_in)

                    updated_settings = request["updated_settings"]
                    apply_settings(self.pwm, self.display, updated_settings)

                    request_artist_val = request["artist"]
                    request_title_val = request["title"]
                    is_media_active = request["is_media_active"]
                    is_playing = request["is_playing"]

                    if not is_media_active:
                        if self.play_button in main_group:
                            main_group.remove(self.play_button)
                        if self.pause_button in main_group:
                            main_group.remove(self.pause_button)
                        if self.prev_button in main_group:
                            main_group.remove(self.prev_button)
                        if self.next_button in main_group:
                            main_group.remove(self.next_button)
                    else:
                        if self.prev_button not in main_group:
                            main_group.append(self.prev_button)
                        if self.next_button not in main_group:
                            main_group.append(self.next_button)
                        if is_playing:
                            if self.play_button in main_group:
                                main_group.remove(self.play_button)
                            if self.pause_button not in main_group:
                                main_group.append(self.pause_button)
                        else:
                            if self.pause_button in main_group:
                                main_group.remove(self.pause_button)
                            if self.play_button not in main_group:
                                main_group.append(self.play_button)

                    if request_artist_val is None and request_title_val is None:
                        music_label.text = IDLE_MUSIC
                    else:
                        if not request_artist_val:
                            music_val = request_title_val
                        else:
                            music_val = request_artist_val + " - " + request_title_val
                        if music_label.text.strip() != music_val:
                            music_label.text = music_val

                    music_label.x = int(150 / 2 - music_label.width / 2)

                    cpu_label_value = "CPU: " + str(request["used_cpu_percent"]) + "%"
                    if cpu_label.text.strip() != cpu_label_value:
                        cpu_label.text = cpu_label_value

                    ram_label_value = "RAM: " + str(request["used_ram_gigabytes"]) + "/" + str(request["total_ram_gigabytes"]) + "GB"
                    if ram_label.text.strip() != ram_label_value:
                        ram_label.text = ram_label_value

                    time_label.text = request["time"]

                    if self.page_layout.showing_page_name == "idle_page":
                        self.page_layout.show_page("main_page")

                    send_current_settings(self.pwm)
                except Exception as e:
                    print(e)
                iterations_without_update = 0
            else:
                iterations_without_update = iterations_without_update + 1
                if iterations_without_update == 10:
                    self.page_layout.show_page("idle_page")
                    iterations_without_update = 0

            music_label.update()

            self.display.refresh()

            await asyncio.sleep(0.1)

def create_idle_page():
    idle_page_group = displayio.Group()

    lbl1 = label.Label(terminalio.FONT, text="Connecting...", scale=2)
    lbl1.x = 80
    lbl1.y = 110
    idle_page_group.append(lbl1)
    return idle_page_group

def create_settings_page(shutdown_button, devmode_button, back_button):
    settings_page_group = displayio.Group()
    settings_page_group.append(shutdown_button)
    settings_page_group.append(devmode_button)
    settings_page_group.append(back_button)
    return settings_page_group

def send_current_settings(pwm):
    brightness = duty_cycle_to_percent(pwm.duty_cycle)
    rotated = display_rotated()
    usb_cdc.data.write(
        json.dumps(
        {
            'command':'settings',
            'dev_mode_enabled' : dev_mode_enabled(),
            'brightness' : brightness,
            'display_rotated' : rotated
        }) + '\n')

def duty_cycle_to_percent(duty_cycle):
    return int(duty_cycle / (2**16 - 1) * 100)

def percent_to_duty_cycle(percent):
    return int(percent / 100 * (2**16 - 1))
 
def create_text_button(x, y, label):
    return Button(
        x=x,
        y=y,
        width=70,
        height=50,
        style=Button.RECT,
        fill_color=0xCCCCCC,
        label=label,
        label_font=terminalio.FONT,
        label_color=0x000000,
    )


def create_sprite_button(x, y, png_name):
    return SpriteButton(
        x=x,
        y=y,
        width=48,
        height=48,
        bmp_path=f'pngs/{png_name}.png',
        transparent_index=0,
    )

def apply_settings(pwm, display, updated_settings):
    if updated_settings:
        brightness_percent = int(updated_settings["brightness"])
        brightness_duty_cycle = int(brightness_percent / 100 * (2**16 - 1))
        pwm.duty_cycle = brightness_duty_cycle

        display_rotated = updated_settings["display_rotated"]
        if display_rotated:
            display.rotation = 180
        else:
            display.rotation = 0

        dev_mode_enabled = updated_settings["dev_mode_enabled"]
        restart_in_uf2_mode =  updated_settings["restart_in_uf2_mode"]
        reset_needed = dev_mode_setting_changed(dev_mode_enabled) or restart_in_uf2_mode

        save_settings(dev_mode_enabled, brightness_percent, display_rotated)

        if updated_settings["restart_in_uf2_mode"]:
            microcontroller.on_next_reset(microcontroller.RunMode.UF2)
        
        if reset_needed:
            microcontroller.reset()

async def main():
    touch_context = TouchContext()
    controller = DisplayController(touch_context)
    handle_touch_task = asyncio.create_task(controller.handle_touch())
    render_display_task = asyncio.create_task(controller.render_display())
    await asyncio.gather(handle_touch_task, render_display_task)

asyncio.run(main())