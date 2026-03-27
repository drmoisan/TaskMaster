using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

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

            var computerInfo = new ComputerInfo();
            totalPhysicalMemory = computerInfo.TotalPhysicalMemory;
            availablePhysicalMemory = computerInfo.AvailablePhysicalMemory;
            totalVirtualMemory = computerInfo.TotalVirtualMemory;
            availableVirtualMemory = computerInfo.AvailableVirtualMemory;

            var jagged = new List<string[]>
            {
                new string[]
                {
                    "AvailablePhysicalMemory",
                    $"{availablePhysicalMemory / (double)Math.Pow(10, 9):N2} GB",
                },
                new string[]
                {
                    "TotalPhysicalMemory",
                    $"{totalPhysicalMemory / (double)Math.Pow(10, 9):N2} GB",
                },
                new string[]
                {
                    "AvailableVirtualMemory",
                    $"{availableVirtualMemory / (double)Math.Pow(10, 9):N2} GB",
                },
                new string[]
                {
                    "TotalVirtualMemory",
                    $"{totalVirtualMemory / (double)Math.Pow(10, 9):N2} GB",
                },
            };

            Console.WriteLine(
                jagged
                    .ToArray()
                    .ToFormattedText(
                        ["Property", "Value"],
                        [Enums.Justification.Left, Enums.Justification.Right],
                        "Memory Stats"
                    )
            );

            // Assert

            Assert.IsTrue(
                totalPhysicalMemory > availablePhysicalMemory && availablePhysicalMemory > 0
            );
            Assert.IsTrue(
                totalVirtualMemory > availableVirtualMemory && availableVirtualMemory > 0
            );
            Assert.IsTrue(totalVirtualMemory > totalPhysicalMemory);
        }
    }
}
