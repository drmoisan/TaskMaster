using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config;

namespace UtilitiesCS.Test.ReusableTypeClasses.NewSmartSerializable
{
    [TestClass]
    public class ConfigController_Tests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_WithGlobalsAndConfig_SetsProperties()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockConfig = new Mock<ISmartSerializableConfig>();
            var mockCopy = new Mock<ISmartSerializableConfig>();
            mockConfig.Setup(c => c.DeepCopy()).Returns(mockCopy.Object);

            var controller = new ConfigController(mockGlobals.Object, mockConfig.Object);

            controller.Config.Should().BeSameAs(mockConfig.Object);
            controller.ConfigCopy.Should().BeSameAs(mockCopy.Object);
            controller.Globals.Should().BeSameAs(mockGlobals.Object);
        }

        #endregion
    }
}
