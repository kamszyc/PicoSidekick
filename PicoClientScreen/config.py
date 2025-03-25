import microcontroller

def dev_mode_enabled():
    return microcontroller.nvm[0] == 1

def toggle_dev_mode():
    microcontroller.nvm[0] = int(not microcontroller.nvm[0])
    microcontroller.reset()
    
def enable_dev_mode():
    if microcontroller.nvm[0]:
        return
    
    microcontroller.nvm[0] = 1

def disable_dev_mode():
    if not microcontroller.nvm[0]:
        return
    
    microcontroller.nvm[0] = 0

def set_restart_in_uf2_mode():
    microcontroller.on_next_reset(microcontroller.RunMode.UF2)

def restart():
    microcontroller.reset()