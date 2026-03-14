using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class TaskPriority_Tests
    {
        [TestMethod]
        public void TaskPriority_ShouldNotExposeALiveRuntimeType_BecauseTheProductionFileContainsOnlyCommentedCode()
        {
            // Arrange
            Type nonGenericType = typeof(TimeOutTask).Assembly.GetType("UtilitiesCS.Threading.TaskPriority");
            Type genericType = typeof(TimeOutTask).Assembly.GetType("UtilitiesCS.Threading.TaskPriority`1");

            // Assert
            nonGenericType.Should().BeNull();
            genericType.Should().BeNull();
        }
    }
}
