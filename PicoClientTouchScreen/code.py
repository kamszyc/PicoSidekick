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

async def render_display():
    displayio.release_displays()

    TFT_WIDTH = 320
    TFT_HEIGHT = 240

    tft_spi_clk = board.GP6
    tft_spi_mosi = board.GP7
    tft_cs = board.GP13
    tft_dc = board.GP15
    tft_res = board.GP14

    touch_spi_clk = board.GP10
    touch_spi_mosi = board.GP11
    touch_spi_miso = board.GP8

    touch_cs = board.GP12

    touch_x_min = 120
    touch_x_max = 1847
    touch_y_min = 148
    touch_y_max = 1914

    touch_spi = busio.SPI(touch_spi_clk, MOSI=touch_spi_mosi, MISO=touch_spi_miso)
    touch = Touch(touch_spi, cs=touch_cs,
                x_min=touch_x_min, x_max=touch_x_max,
                y_min=touch_y_min, y_max=touch_y_max)

    tft_spi = busio.SPI(tft_spi_clk, MOSI=tft_spi_mosi)


    display_bus = displayio.FourWire(
        tft_spi, command=tft_dc, chip_select=tft_cs, reset=tft_res)

    display = adafruit_ili9341.ILI9341(display_bus,
                        width=TFT_WIDTH, height=TFT_HEIGHT)
    display.rotation = 90
    scrWidth = display.width
    scrHeight = display.height

    splash = displayio.Group()
    display.root_group = splash

    text_group = displayio.Group(scale=2, x=10, y=10)
    artist_label = scrolling_label.ScrollingLabel(terminalio.FONT, text="Waiting...", max_characters = 20, animate_time=0.5)
    text_group.append(artist_label) 
    title_label = scrolling_label.ScrollingLabel(terminalio.FONT, text="Play some music!", max_characters = 20, animate_time=0.5)
    title_label.y = 20
    text_group.append(title_label)
    splash.append(text_group)

    while True:
        if usb_cdc.data.in_waiting > 0:
            try:
                data_in = usb_cdc.data.readline()
                if data_in:
                    try:
                        request = json.loads(data_in)
                        if artist_label.text.strip() != request["artist"]:
                            artist_label.text = request["artist"]
                        if title_label.text.strip() != request["title"]:
                            title_label.text = request["title"]
                    except ValueError:
                        pass
            except Exception as e:
                print("error")

        artist_label.update()
        title_label.update()

        display.refresh()

        await asyncio.sleep(0.1)

async def main():
    render_display_task = asyncio.create_task(render_display())
    await asyncio.gather(render_display_task)


asyncio.run(main())