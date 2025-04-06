# Pico Sidekick

![Build Host](https://github.com/kamszyc/PicoSidekick/actions/workflows/build-host-and-release.yml/badge.svg)

DIY helper screen for Windows PC.

![Sidekick photo](sidekick.jpg)

Built with:
- Raspberry Pi Pico
- cheap 2.8" TFT touch screen, with ILI9341 and XPT2046 controllers and 240x320 resolution

Software components:
- CircuitPython software on Pico
- Windows PC host built with C#/.NET 9

Components are connected using USB CDC serial port

## Features
- Clock
- Currently played music/other media
- CPU and RAM usage
- Media buttons: Play/Pause, Volume up/down
- Shutdown computer button
- Settings
    - TFT screen brightness adjustment
    - Enabling dev mode (visibility of CIRCUITPY drive)
    - Restarting board in UF2 mode - for dumping Pico memory as uf2 file

## Installation on Raspberry Pi Pico

### From released uf2 file
1. Get `.uf2` file from [latest release](https://github.com/kamszyc/PicoSidekick/releases/latest)
2. Hold `BOOTSEL` button and connect Pi to PC
3. Put `.uf2` file on `RPI-RP2` drive

### From scratch
1. Install CircuitPython on Raspberry Pi Pico \
https://learn.adafruit.com/getting-started-with-raspberry-pi-pico-circuitpython/circuitpython
2. Connect Pico to USB port, `CIRCUITPY` drive should appear
3. Install Adafruit libraries. This can be done using `circup` utility \
https://github.com/adafruit/circup \
`circup install adafruit_button adafruit_hid asyncio adafruit_display_text adafruit_ili9341 adafruit_displayio_layout`
4. Remove preinstalled `code.py` file and put files from `PicoClientScreen` directory into `CIRCUITPY` drive
5. Disconnect Pico from USB and connect it again

## Pinout

| **TFT**   | **Pico** |
|-----------|----------|
| VCC       | 3V3      |
| GND       | GND      |
| CS        | GP13     |
| RESET     | GP14     |
| DC        | GP15     |
| SDI(MOSI) | GP7      |
| SCK       | GP6      |
| LED       | GP16     |
| SDO(MISO) | -        |
| T_CLK     | GP10     |
| T_CS      | GP12     |
| T_DIN     | GP11     |
| T_DO      | GP8      |
| T_IRQ     | -        |

## References
1. https://helloraspberrypi.blogspot.com/2021/04/raspberry-pi-picocircuitpython-ili9341.html
2. https://fonts.google.com/icons
