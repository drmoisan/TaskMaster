using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBFunctions
{
    public static class ComputerInfo
    {
        public static ulong AvailablePhysicalMemory => new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory;
        public static ulong TotalPhysicalMemory => new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        public static ulong AvailableVirtualMemory => new Microsoft.VisualBasic.Devices.ComputerInfo().AvailableVirtualMemory;
        public static ulong TotalVirtualMemory => new Microsoft.VisualBasic.Devices.ComputerInfo().TotalVirtualMemory;
    }
}
