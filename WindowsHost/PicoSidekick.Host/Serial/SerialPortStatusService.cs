using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Serial
{
    public class SerialPortStatusService
    {
        private SerialPortStatus _serialPortStatus;
        private Lock _lockObj = new();

        public SerialPortStatusService()
        {
            _serialPortStatus = new SerialPortStatus(false, null);
        }

        public SerialPortStatus GetStatus()
        {
            lock (_lockObj)
            {
                return _serialPortStatus;
            }
        }

        public void SetStatus(SerialPortStatus status)
        {
            lock (_lockObj)
            {
                _serialPortStatus = status;
            }
        }
    }
}
