using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSidekick.Host.Performance
{
    public class PerformanceService
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _availableRamCounter;
        private readonly float _totalRamInGigabytes;
        private readonly float _totalRamInGigabytesRounded;

        public float TotalRamInGigabytes => _totalRamInGigabytesRounded;

        public PerformanceService()
        {
            var computerInfo = new ComputerInfo();

            _totalRamInGigabytes = BytesToGigabytes(computerInfo.TotalPhysicalMemory);
            _totalRamInGigabytesRounded = (float)Math.Round(_totalRamInGigabytes, 1);
            _cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            _availableRamCounter = new PerformanceCounter("Memory", "Available Bytes");
        }

        public PerformanceReading Read()
        {
            float cpu = (float)Math.Round(_cpuCounter.NextValue());
            float usedRamInGigabytes = CalculateUsedRam(_totalRamInGigabytes, _availableRamCounter);
            return new PerformanceReading(cpu, usedRamInGigabytes);
        }

        private static float CalculateUsedRam(float totalRamInGigabytes, PerformanceCounter availableRamCounter)
        {
            float usedRamInGigabytes = totalRamInGigabytes - BytesToGigabytes(availableRamCounter.NextValue());
            return (float)Math.Round(usedRamInGigabytes, 1);
        }

        private static float BytesToGigabytes(float bytes)
        {
            return bytes / 1024f / 1024f / 1024f;
        }
    }
}
