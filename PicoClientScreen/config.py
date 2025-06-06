import microcontroller

def dev_mode_enabled():
    return microcontroller.nvm[0] == 1

def get_brightness_percent():
    return min(microcontroller.nvm[1], 100)

def display_rotated():
    return microcontroller.nvm[2] == 1

def toggle_dev_mode():
    microcontroller.nvm[0] = int(not microcontroller.nvm[0])
    microcontroller.reset()

def dev_mode_setting_changed(new_setting_bool):
    new_setting_int = 1 if new_setting_bool else 0
    return microcontroller.nvm[0] != new_setting_int
    
def save_settings(dev_mode, brightness_percent, display_rotated):
    arr = bytearray([int(dev_mode), brightness_percent, int(display_rotated)])
    microcontroller.nvm[0:3] = arr
