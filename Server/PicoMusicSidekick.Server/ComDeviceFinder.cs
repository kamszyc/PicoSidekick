using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PicoMusicSidekick.Server
{
    internal static class ComDeviceFinder
    {
        public static string GetCircuitPythonDataSerialPortName()
        {
            string pnpDeviceId;
            using (var searcher = new ManagementObjectSearcher
                        ("SELECT * FROM WIN32_PnPEntity"))
            {
                var ports = searcher.Get()
                                    .Cast<ManagementObject>()
                                    .Where(m => GetDeviceName(m)?.Contains("CircuitPython CDC2") ?? false)
                                    .ToList();
                pnpDeviceId = ports.First().GetPropertyValue("DeviceID") as string;
            }

            string portName;
            using (var searcher = new ManagementObjectSearcher
                        ("SELECT * FROM WIN32_SerialPort"))
            {
                var serialPort = searcher.Get()
                                    .Cast<ManagementBaseObject>()
                                    .First(m => m.GetPropertyValue("PNPDeviceID") as string == pnpDeviceId);
                portName = serialPort.GetPropertyValue("DeviceID") as string;
            }

            return portName;
        }

        private static string GetDeviceName(ManagementObject mo)
        {
            var args = new object[] { new string[] { "DEVPKEY_Device_BusReportedDeviceDesc" }, null };
            mo.InvokeMethod("GetDeviceProperties", args);
            var mbos = (ManagementBaseObject[])args[1];
            if (mbos.Length > 0)
            {
                var data = mbos[0].Properties.OfType<PropertyData>().FirstOrDefault(p => p.Name == "Data");
                if (data != null)
                {
                    return data.Value as string;
                }
            }

            return null;
        }
    }
}
