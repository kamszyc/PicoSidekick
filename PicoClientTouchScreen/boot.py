import usb_cdc
import usb_hid
import usb_midi

usb_midi.disable()

usb_cdc.enable(console=True, data=True)

usb_hid.enable((usb_hid.Device.CONSUMER_CONTROL,))
