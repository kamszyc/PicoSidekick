import microcontroller

def devmode_enabled():
    return microcontroller.nvm[0] == 1

def toggle_devmode():
    microcontroller.nvm[0] = int(not microcontroller.nvm[0])