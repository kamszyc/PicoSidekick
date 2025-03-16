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

async def handle_touch(touch_context, play_button, shutdown_button):
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
                    
                if shutdown_button.contains((touch_context.touchedY, touch_context.touchedX)):
                    print('shutdown')
                    usb_cdc.data.write(json.dumps({'command':'shutdown'}) + '\n')
                
            touch_context.touchEvent = EVT_NO

        await asyncio.sleep(0)


async def render_display(play_button, shutdown_button):
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

    music_label = scrolling_label.ScrollingLabel(terminalio.FONT, text=IDLE_MUSIC, max_characters=25, animate_time=0.5, color=0x0)
    music_label.y = 30
    text_group.append(music_label) 

    cpu_label = label.Label(terminalio.FONT, text=IDLE_CPU, color=0xFFFFFF)
    cpu_label.y = 80
    text_group.append(cpu_label)

    ram_label = label.Label(terminalio.FONT, text=IDLE_RAM, color=0xFFFFFF)
    ram_label.y = 100
    text_group.append(ram_label)
    
    rect1 = Rect(0, 0, 320, 55, fill=0x0000FF)
    splash.append(rect1)
    
    rect2 = Rect(0, 55, 320, 100, fill=0xFFFFFF)
    splash.append(rect2)

    splash.append(text_group)
    splash.append(play_button)
    splash.append(shutdown_button)

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

def create_button(x, y, label):
    BUTTON_WIDTH = 70
    BUTTON_HEIGHT = 50
    BUTTON_STYLE = Button.RECT
    BUTTON_FILL_COLOR = 0xCCCCCC
    BUTTON_LABEL_COLOR = 0x000000

    return Button(
        x=x,
        y=y,
        width=BUTTON_WIDTH,
        height=BUTTON_HEIGHT,
        style=BUTTON_STYLE,
        fill_color=BUTTON_FILL_COLOR,
        label=label,
        label_font=terminalio.FONT,
        label_color=BUTTON_LABEL_COLOR,
    )

async def main():
    touch_context = TouchContext()
    play_button = create_button(125, 95, "PLAY")
    shutdown_button = create_button(240, 165, "SHUTDOWN")
    handle_touch_task = asyncio.create_task(handle_touch(touch_context, play_button, shutdown_button))
    render_display_task = asyncio.create_task(render_display(play_button, shutdown_button))
    await asyncio.gather(handle_touch_task, render_display_task)

asyncio.run(main())