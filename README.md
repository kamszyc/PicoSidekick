# Pico Sidekick

![Build Host](https://github.com/kamszyc/PicoSidekick/actions/workflows/build-and-release-host.yml/badge.svg)

### Installation on Raspberry Pi Pico
1. Install CircuitPython on Raspberry Pi Pico \
https://learn.adafruit.com/getting-started-with-raspberry-pi-pico-circuitpython/circuitpython
2. Connect Pico to USB port, `CIRCUITPY` drive should appear
3. Install Adafruit libraries. This can be done using `circup` utility \
https://github.com/adafruit/circup \
`circup install adafruit_button adafruit_hid asyncio adafruit_display_text adafruit_ili9341`
4. Remove preinstalled `code.py` file and put files from `PicoClientScreen` directory into `CIRCUITPY` drive
5. Disconnect Pico from USB and connect it again

### References:
1. https://helloraspberrypi.blogspot.com/2021/04/raspberry-pi-picocircuitpython-ili9341.html
