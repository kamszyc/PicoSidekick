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
        private readonly PerformanceCounter cpuCounter;
        private readonly PerformanceCounter availableRamCounter;
        private readonly float totalRamInGigabytes;
        private readonly float totalRamInGigabytesRounded;

        public float TotalRamInGigabytes => totalRamInGigabytesRounded;

        public PerformanceService()
        {
            var computerInfo = new ComputerInfo();

            totalRamInGigabytes = BytesToGigabytes(computerInfo.TotalPhysicalMemory);
            totalRamInGigabytesRounded = (float)Math.Round(totalRamInGigabytes, 1);
            cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            availableRamCounter = new PerformanceCounter("Memory", "Available Bytes");
        }

        public PerformanceReading Read()
        {
            float cpu = (float)Math.Round(cpuCounter.NextValue());
            float usedRamInGigabytes = CalculateUsedRam(totalRamInGigabytes, availableRamCounter);
            return new PerformanceReading { Cpu = cpu, UsedRamInGigabytes = usedRamInGigabytes };
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
