from micropython import const
from config import display_rotated
from constants import TFT_HEIGHT, TFT_WIDTH
from xpt2046 import Touch
import busio
import time
from pinout import *

touchSt_Idle_0     = const(0)
touchSt_DnDeb_1    = const(1)
touchSt_Touching_2 = const(2)
touchSt_UpDeb_3    = const(3)

EVT_NO      = const(0)
EVT_PenDown = const(1)
EVT_PenUp   = const(2)
EVT_PenRept = const(3)

touchDb_NUM = const(3)

class TouchContext:
    def __init__(self):
        touch_x_min = 120
        touch_x_max = 1847
        touch_y_min = 148
        touch_y_max = 1914

        touch_spi = busio.SPI(TOUCH_SPI_CLK, MOSI=TOUCH_SPI_MOSI, MISO=TOUCH_SPI_MISO)
        self.touch = Touch(touch_spi, cs=TOUCH_CS,
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
        if display_rotated():
            normalizedX = scrWidth - normalizedX
            normalizedY = scrHeight - normalizedY

        if (normalizedX < 0 or normalizedX >= scrWidth
                or normalizedY < 0 or normalizedY >= scrHeight):
                return None

        return (normalizedX, normalizedY)