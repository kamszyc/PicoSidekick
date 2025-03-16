import usb_cdc
import usb_hid
import usb_midi
import storage
import microcontroller

usb_midi.disable()

usb_cdc.enable(console=True, data=True)

usb_hid.enable((usb_hid.Device.CONSUMER_CONTROL,))

if microcontroller.nvm[0] == 1:
  storage.enable_usb_drive()
else:
  storage.disable_usb_drive()
