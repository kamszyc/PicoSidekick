using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PicoMusicSidekick.Host
{
    internal static class ComDeviceFinder
    {
        public static string GetCircuitPythonDataSerialPortName()
        {
            Dictionary<string, string> serialPortsPnpDeviceIdToDeviceId;
            using (var searcher = new ManagementObjectSearcher
                        ($"SELECT * FROM WIN32_SerialPort"))
            {
                serialPortsPnpDeviceIdToDeviceId = searcher
                    .Get()
                    .Cast<ManagementBaseObject>()
                    .ToDictionary(m => m.GetPropertyValue("PNPDeviceID") as string,
                                  m => m.GetPropertyValue("DeviceID") as string);
            }

            if (serialPortsPnpDeviceIdToDeviceId.Count == 0)
                return null;

            string whereClause = BuildWhereClause(serialPortsPnpDeviceIdToDeviceId);
            string pnpDeviceId;
            using (var searcher = new ManagementObjectSearcher
                        ($"SELECT * FROM WIN32_PnPEntity WHERE {whereClause}"))
            {
                pnpDeviceId = searcher.Get()
                                      .Cast<ManagementObject>()
                                      .FirstOrDefault(m => GetDeviceName(m)?.Contains("CircuitPython CDC2") ?? false)?
                                      .GetPropertyValue("DeviceID") as string;
            }

            if (pnpDeviceId == null)
                return null;

            return serialPortsPnpDeviceIdToDeviceId[pnpDeviceId];
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

        private static string BuildWhereClause(Dictionary<string, string> serialPortsPnpDeviceIdToDeviceId)
        {
            // WQL is subset of SQL and doesn't support IN operator
            return string.Join(" OR ", serialPortsPnpDeviceIdToDeviceId.Keys
                                                                       .Select(id => id.Replace("\\", "\\\\"))
                                                                       .Select(id => $"(DeviceID = \"{id}\")"));
        }
    }
}
