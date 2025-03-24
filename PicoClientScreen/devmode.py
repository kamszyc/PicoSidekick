import microcontroller

def devmode_enabled():
    return microcontroller.nvm[0] == 1

def toggle_devmode():
    microcontroller.nvm[0] = int(not microcontroller.nvm[0])
    microcontroller.reset()
    
def enable_devmode():
    if microcontroller.nvm[0]:
        return
    
    microcontroller.nvm[0] = 1
    microcontroller.reset()

def disable_devmode():
    if not microcontroller.nvm[0]:
        return
    
    microcontroller.nvm[0] = 0
    microcontroller.reset()