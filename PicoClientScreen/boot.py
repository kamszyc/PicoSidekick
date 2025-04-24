from config import dev_mode_enabled
import usb_cdc
import usb_hid
import usb_midi
import storage

usb_midi.disable()

dev_mode = dev_mode_enabled()
usb_cdc.enable(console=dev_mode, data=True)

usb_hid.enable((usb_hid.Device.CONSUMER_CONTROL,))

if dev_mode:
  storage.enable_usb_drive()
else:
  storage.disable_usb_drive()
