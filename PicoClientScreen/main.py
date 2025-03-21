from devmode import devmode_enabled, toggle_devmode
import microcontroller
import usb_hid
from constants import TFT_HEIGHT, TFT_WIDTH
from touch_context import TouchContext, EVT_NO, EVT_PenDown, EVT_PenUp
import usb_cdc
from adafruit_hid.consumer_control import ConsumerControl
from adafruit_hid.consumer_control_code import ConsumerControlCode
import time
import board
import asyncio
import busio
import terminalio
import displayio
import json
from adafruit_display_text import label, scrolling_label
import adafruit_ili9341
from adafruit_button import Button
from adafruit_display_shapes.rect import Rect
from adafruit_displayio_layout.layouts.page_layout import PageLayout

SHUTDOWN_BUTTON_TEXT = "SHUTDOWN"
CONFIRMATION_TEXT = "SURE?"

async def handle_touch(touch_context, page_layout, play_button, shutdown_button, devmode_button, settings_button, back_button):
    cc = ConsumerControl(usb_hid.devices)
    shutdown_pressed = False
    while True:
        curTick = time.monotonic()
        if curTick >= touch_context.NxTick:
            touch_context.NxTick = curTick + touch_context.taskInterval_50ms
            touch_context.touch_det_task()

        #handle touch event
        if touch_context.touchEvent != EVT_NO:
            if touch_context.touchEvent == EVT_PenDown:
                print('ev PenDown - ', touch_context.touchedX, " : ", touch_context.touchedY)

            if touch_context.touchEvent == EVT_PenUp:
                print('ev PenUp - ')

                if devmode_button.contains((touch_context.touchedY, touch_context.touchedX)):
                    print('devmode on/off')
                    toggle_devmode()
                    microcontroller.reset()

                
                if page_layout.showing_page_name == "settings_page" and back_button.contains((touch_context.touchedY, touch_context.touchedX)):
                    print('back')
                    page_layout.show_page("main_page")
                elif settings_button.contains((touch_context.touchedY, touch_context.touchedX)):
                    print('settings')
                    page_layout.show_page("settings_page")

                if play_button.contains((touch_context.touchedY, touch_context.touchedX)):
                    print('play/pause')
                    cc.send(ConsumerControlCode.PLAY_PAUSE)
                    
                if shutdown_button.contains((touch_context.touchedY, touch_context.touchedX)):
                    print('shutdown')
                    if not shutdown_pressed:
                        shutdown_button.label = CONFIRMATION_TEXT
                        shutdown_pressed = True
                    else:
                        shutdown_button.label = SHUTDOWN_BUTTON_TEXT
                        usb_cdc.data.write(json.dumps({'command':'shutdown'}) + '\n')
                        shutdown_pressed = False
                else:
                    shutdown_button.label = SHUTDOWN_BUTTON_TEXT
                    shutdown_pressed = False
                
            touch_context.touchEvent = EVT_NO

        await asyncio.sleep(0)

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

async def render_display(page_layout, play_button, shutdown_button, devmode_button, settings_button, back_button):
    displayio.release_displays()

    tft_spi_clk = board.GP6
    tft_spi_mosi = board.GP7
    tft_cs = board.GP13
    tft_dc = board.GP15
    tft_res = board.GP14

    tft_spi = busio.SPI(tft_spi_clk, MOSI=tft_spi_mosi)

    display_bus = displayio.FourWire(
        tft_spi, command=tft_dc, chip_select=tft_cs, reset=tft_res)

    display = adafruit_ili9341.ILI9341(
        display_bus,
        width=TFT_WIDTH,
        height=TFT_HEIGHT,
        auto_refresh=False)

    root = displayio.Group()
    display.root_group = root
    
    root.append(page_layout)

    main_group = displayio.Group()
    idle_group = create_idle_page()
    settings_group = create_settings_page(shutdown_button, devmode_button, back_button)
    page_layout.add_content(idle_group, "idle_page")
    page_layout.add_content(main_group, "main_page")
    page_layout.add_content(settings_group, "settings_page")

    IDLE_MUSIC = "Play some music!"

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
    main_group.append(play_button)
    main_group.append(settings_button)

    iterations_without_update = 0
    while True:
        if usb_cdc.data.in_waiting > 0:
            try:
                data_in = usb_cdc.data.readline()
                request = json.loads(data_in)

                request_artist_val = request["artist"]
                request_title_val = request["title"]

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

                cpu_label_value = "CPU: " + str(request["usedCPUPercent"]) + "%"
                if cpu_label.text.strip() != cpu_label_value:
                    cpu_label.text = cpu_label_value

                ram_label_value = "RAM: " + str(request["usedRAMGigabytes"]) + "/" + str(request["totalRAMGigabytes"]) + "GB"
                if ram_label.text.strip() != ram_label_value:
                    ram_label.text = ram_label_value

                time_label.text = request["time"]

                if page_layout.showing_page_name == "idle_page":
                    page_layout.show_page("main_page")
            except Exception as e:
                print(e)
            iterations_without_update = 0
        else:
            iterations_without_update = iterations_without_update + 1
            if iterations_without_update == 10:
                page_layout.show_page("idle_page")
                iterations_without_update = 0

        music_label.update()

        display.refresh()

        await asyncio.sleep(0.1)

def create_button(x, y, label):
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

async def main():
    touch_context = TouchContext()
    
    page_layout = PageLayout(x=0, y=0)

    play_button = create_button(125, 95, "PLAY")
    settings_button = create_button(240, 180, "SETTINGS")

    shutdown_button = create_button(135, 90, SHUTDOWN_BUTTON_TEXT)
    back_button = create_button(135, 170, "BACK")
    devmode_text = "DEVMODE OFF" if devmode_enabled() else "DEVMODE ON"
    devmode_button = create_button(135, 10, devmode_text)

    handle_touch_task = asyncio.create_task(handle_touch(touch_context, page_layout, play_button, shutdown_button, devmode_button, settings_button, back_button))
    render_display_task = asyncio.create_task(render_display(page_layout, play_button, shutdown_button, devmode_button, settings_button, back_button))
    await asyncio.gather(handle_touch_task, render_display_task)

asyncio.run(main())