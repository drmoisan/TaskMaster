using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskVisualization;

namespace TaskVisualization.Test
{
    [TestClass]
    public class TaskPriorityMapperTests
    {
        [TestMethod]
        public void ToDisplayString_High_ReturnsHigh()
        {
            TaskPriorityMapper
                .ToDisplayString(OlImportance.olImportanceHigh)
                .Should()
                .Be(TaskPriorityMapper.High);
        }

        [TestMethod]
        public void ToDisplayString_Low_ReturnsLow()
        {
            TaskPriorityMapper
                .ToDisplayString(OlImportance.olImportanceLow)
                .Should()
                .Be(TaskPriorityMapper.Low);
        }

        [TestMethod]
        public void ToDisplayString_Normal_ReturnsNormal()
        {
            TaskPriorityMapper
                .ToDisplayString(OlImportance.olImportanceNormal)
                .Should()
                .Be(TaskPriorityMapper.Normal);
        }

        [TestMethod]
        public void FromDisplayString_High_ReturnsHighImportance()
        {
            TaskPriorityMapper.FromDisplayString("High").Should().Be(OlImportance.olImportanceHigh);
        }

        [TestMethod]
        public void FromDisplayString_Low_ReturnsLowImportance()
        {
            TaskPriorityMapper.FromDisplayString("Low").Should().Be(OlImportance.olImportanceLow);
        }

        [TestMethod]
        public void FromDisplayString_Normal_ReturnsNormalImportance()
        {
            TaskPriorityMapper
                .FromDisplayString("Normal")
                .Should()
                .Be(OlImportance.olImportanceNormal);
        }

        [TestMethod]
        public void FromDisplayString_UnknownInput_FallsBackToNormal()
        {
            TaskPriorityMapper
                .FromDisplayString("SomethingElse")
                .Should()
                .Be(OlImportance.olImportanceNormal);
        }
    }
}
