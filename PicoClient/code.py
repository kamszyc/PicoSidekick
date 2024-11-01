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
from adafruit_st7789 import ST7789

async def monitor_media_buttons(pin_play_pause, pin_prev, pin_next, pin_volume_up, pin_volume_down): 
    cc = ConsumerControl(usb_hid.devices)
    with keypad.Keys(
        (pin_play_pause, pin_prev, pin_next, pin_volume_up, pin_volume_down), value_when_pressed=False, pull=True
    ) as keys:
        while True:
            key_event = keys.events.get()
            if key_event and key_event.pressed:
                if key_event.key_number == 0:
                    cc.send(ConsumerControlCode.PLAY_PAUSE)
                elif key_event.key_number == 1:
                    cc.send(ConsumerControlCode.SCAN_PREVIOUS_TRACK)
                elif key_event.key_number == 2:
                    cc.send(ConsumerControlCode.SCAN_NEXT_TRACK)
                elif key_event.key_number == 3:
                    cc.send(ConsumerControlCode.VOLUME_DECREMENT)
                elif key_event.key_number == 4:
                    cc.send(ConsumerControlCode.VOLUME_INCREMENT)
            await asyncio.sleep(0)

async def render_display():
    displayio.release_displays()

    tft_dc = board.GP8
    tft_cs = board.GP9
    spi_clk = board.GP10
    spi_mosi = board.GP11
    tft_rst = board.GP12
    backlight = board.GP13
    spi = busio.SPI(spi_clk, spi_mosi)

    display_bus = FourWire(spi, command=tft_dc, chip_select=tft_cs, reset=tft_rst)

    display = ST7789(
        display_bus,
        rotation=270,
        width=240,
        height=240,
        rowstart=80,
        backlight_pin=backlight,
        auto_refresh=False
    )

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
    media_buttons_task = asyncio.create_task(monitor_media_buttons(board.GP3, board.GP15, board.GP17, board.GP19, board.GP21))
    render_display_task = asyncio.create_task(render_display())
    await asyncio.gather(media_buttons_task, render_display_task)


asyncio.run(main())