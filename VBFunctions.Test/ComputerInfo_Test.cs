using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VBFunctions;
using UtilitiesCS;
using System.Collections.Generic;

namespace VBFunctions.Test
{
    [TestClass]
    public class ComputerInfo_Test
    {
        [TestInitialize]
        public void Initialize()
        {
            Console.SetOut(new DebugTextWriter());
        }

        [TestMethod]
        public void ComputerInfo_PropertyTests()
        {
            // Arrange

            ulong totalPhysicalMemory;
            ulong availablePhysicalMemory;
            ulong totalVirtualMemory;
            ulong availableVirtualMemory;

            // Act

            totalPhysicalMemory = ComputerInfo.TotalPhysicalMemory;
            availablePhysicalMemory = ComputerInfo.AvailablePhysicalMemory;
            totalVirtualMemory = ComputerInfo.TotalVirtualMemory;
            availableVirtualMemory = ComputerInfo.AvailableVirtualMemory;
            
            var jagged = new List<string[]> 
            { 
                new string[] { "AvailablePhysicalMemory", $"{availablePhysicalMemory / (double)1000000:N0} MB" },
                new string[] { "TotalPhysicalMemory", $"{totalPhysicalMemory / (double)1000000:N0} MB" },
                new string[] { "AvailableVirtualMemory", $"{availableVirtualMemory / (double)1000000:N0} MB" },
                new string[] { "TotalVirtualMemory", $"{totalVirtualMemory / (double)1000000:N0} MB" }
            };

            Console.WriteLine(jagged.ToArray().ToFormattedText(
                ["Property", "Value"], 
                [Enums.Justification.Left, Enums.Justification.Right], 
                "Memory Stats"));

            // Assert

            Assert.IsTrue(totalPhysicalMemory > availablePhysicalMemory && availablePhysicalMemory > 0);
            Assert.IsTrue(totalVirtualMemory > availableVirtualMemory && availableVirtualMemory > 0);
            Assert.IsTrue(totalVirtualMemory > totalPhysicalMemory);

        }
    }
}
