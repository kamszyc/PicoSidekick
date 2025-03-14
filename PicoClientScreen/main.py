import usb_hid
import usb_cdc
from adafruit_hid.consumer_control import ConsumerControl
from adafruit_hid.consumer_control_code import ConsumerControlCode
import time
import board
from digitalio import DigitalInOut, Direction, Pull
import asyncio
import keypad
import busio
import terminalio
import displayio
import json
from fourwire import FourWire
from adafruit_display_text import label, scrolling_label
import adafruit_ili9341
from xpt2046 import Touch
from adafruit_button import Button

TFT_WIDTH = const(320)
TFT_HEIGHT = const(240)

touchSt_Idle_0     = const(0)
touchSt_DnDeb_1    = const(1)
touchSt_Touching_2 = const(2)
touchSt_UpDeb_3    = const(3)

EVT_NO = const(0)
EVT_PenDown = const(1)
EVT_PenUp   = const(2)
EVT_PenRept = const(3)

touchDb_NUM = const(3)

class TouchContext:
    def __init__(self):
        touch_spi_clk = board.GP10
        touch_spi_mosi = board.GP11
        touch_spi_miso = board.GP8

        touch_cs = board.GP12

        touch_x_min = 120
        touch_x_max = 1847
        touch_y_min = 148
        touch_y_max = 1914

        touch_spi = busio.SPI(touch_spi_clk, MOSI=touch_spi_mosi, MISO=touch_spi_miso)
        self.touch = Touch(touch_spi, cs=touch_cs,
                    x_min=touch_x_min, x_max=touch_x_max,
                    y_min=touch_y_min, y_max=touch_y_max)

        self.taskInterval_50ms = 0.050
        self.NxTick = time.monotonic() + self.taskInterval_50ms
        self.touchEvent  = EVT_NO
        self.touchedX = 0
        self.touchedY = 0

        self.touchSt = touchSt_Idle_0

        self.touchDb = touchDb_NUM
        self.touching = False

    def touch_det_task(self):
        validXY = self.valid_touch(TFT_HEIGHT, TFT_WIDTH)

        if self.touchSt == touchSt_Idle_0:
            if validXY != None:
                self.touchDb = touchDb_NUM
                self.touchSt = touchSt_DnDeb_1
        
        elif self.touchSt == touchSt_DnDeb_1:
            if validXY != None:
                self.touchDb = self.touchDb-1
                if self.touchDb==0:
                    self.touchSt = touchSt_Touching_2
                    self.touchEvent = EVT_PenDown
                    self.touchedX, self.touchedY = validXY
                    self.touching = True
            else:
                self.touchSt = touchSt_Idle_0
                
        elif self.touchSt == touchSt_Touching_2:
            if validXY != None:
                self.touchedX, self.touchedY = validXY
                self.touchEvent = EVT_PenRept
            else:
                self.touchDb = touchDb_NUM
                self.touchSt = touchSt_UpDeb_3
                
        elif self.touchSt == touchSt_UpDeb_3:
            if validXY != None:
                self.touchSt = touchSt_Touching_2
            else:
                self.touchDb = self.touchDb-1
                if self.touchDb == 0:
                    self.touchSt = touchSt_Idle_0
                    self.touchEvent = EVT_PenUp
                    self.touching = False


    def valid_touch(self, scrWidth, scrHeight):
        xy = self.touch.raw_touch()
        
        if xy == None:
            return None
        
        normalizedX, normalizedY = self.touch.normalize(*xy)
        if (normalizedX < 0 or normalizedX >= scrWidth
                or normalizedY < 0 or normalizedY >= scrHeight):
                return None
            
        return (normalizedX, normalizedY)
    

async def handle_touch(touch_context, play_button):
    cc = ConsumerControl(usb_hid.devices)
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
                
                if play_button.contains((touch_context.touchedY, touch_context.touchedX)):
                    print('play/pause')
                    cc.send(ConsumerControlCode.PLAY_PAUSE)
                
            touch_context.touchEvent = EVT_NO

        await asyncio.sleep(0)


async def render_display(play_button):
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

    splash = displayio.Group()
    display.root_group = splash

    IDLE_TIME = "xx:xx"
    IDLE_MUSIC = "Play some music!"
    IDLE_CPU = "CPU: unknown"
    IDLE_RAM = "RAM: unknown"

    text_group = displayio.Group(scale=2, x=10, y=10)

    time_label = label.Label(terminalio.FONT, text=IDLE_TIME, scale=2)
    time_label.x = 45
    time_label.y = 10
    text_group.append(time_label)

    music_label = scrolling_label.ScrollingLabel(terminalio.FONT, text=IDLE_MUSIC, max_characters = 25, animate_time=0.5)
    music_label.y = 30
    text_group.append(music_label) 

    cpu_label = label.Label(terminalio.FONT, text=IDLE_CPU)
    cpu_label.y = 80
    text_group.append(cpu_label)

    ram_label = label.Label(terminalio.FONT, text=IDLE_RAM)
    ram_label.y = 100
    text_group.append(ram_label)

    splash.append(text_group)
    splash.append(play_button)

    iterations_without_update = 0
    while True:
        if usb_cdc.data.in_waiting > 0:
            try:
                data_in = usb_cdc.data.readline()
                if data_in:
                    try:
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
                    except ValueError:
                        pass
            except Exception as e:
                print(e)
            iterations_without_update = 0
        else:
            iterations_without_update = iterations_without_update + 1
            if iterations_without_update == 10:
                music_label.text = IDLE_MUSIC
                cpu_label.text = IDLE_CPU
                ram_label.text = IDLE_RAM
                time_label.text = IDLE_TIME
                iterations_without_update = 0

        music_label.update()

        display.refresh()

        await asyncio.sleep(0.1)

def create_play_button():
    BUTTON_X = 135
    BUTTON_Y = 95
    BUTTON_WIDTH = 50
    BUTTON_HEIGHT = 50
    BUTTON_STYLE = Button.RECT
    BUTTON_FILL_COLOR = 0x00FFFF
    BUTTON_OUTLINE_COLOR = 0xFF00FF
    BUTTON_LABEL = "PLAY"
    BUTTON_LABEL_COLOR = 0x000000

    return Button(
        x=BUTTON_X,
        y=BUTTON_Y,
        width=BUTTON_WIDTH,
        height=BUTTON_HEIGHT,
        style=BUTTON_STYLE,
        fill_color=BUTTON_FILL_COLOR,
        outline_color=BUTTON_OUTLINE_COLOR,
        label=BUTTON_LABEL,
        label_font=terminalio.FONT,
        label_color=BUTTON_LABEL_COLOR,
    )

async def main():
    touch_context = TouchContext()
    play_button = create_play_button()
    handle_touch_task = asyncio.create_task(handle_touch(touch_context, play_button))
    render_display_task = asyncio.create_task(render_display(play_button))
    await asyncio.gather(handle_touch_task, render_display_task)

asyncio.run(main())