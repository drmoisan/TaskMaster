using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SmartSerializableLoader_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            // Act
            var loader = new SmartSerializableLoader();

            // Assert
            loader.Should().NotBeNull();
            loader.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithGlobals_SetsGlobals()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(
                mockApp.Object,
                true,
                variable => variable == "OneDriveCommercial" ? @"C:\OneDrive" : null
            );

            // Act
            var loader = new SmartSerializableLoader(globals);

            // Assert
            loader.Should().NotBeNull();
            loader.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void ApplicationGlobals_WithInjectedEnvironmentReader_LoadsOneDriveWithoutProcessEnvironment()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            const string oneDriveRoot = @"C:\OneDrive";

            // Act
            var globals = new TaskMaster.ApplicationGlobals(
                mockApp.Object,
                loadBasic: true,
                readEnvironmentVariable: variable =>
                    variable == "OneDriveCommercial" ? oneDriveRoot : null
            );

            // Assert
            globals.FS.SpecialFolders["OneDrive"].Should().Be(oneDriveRoot);
        }

        [TestMethod]
        public void Engine_SetAndGet_Works()
        {
            // Arrange
            var loader = new SmartSerializableLoader();

            // Act
            loader.Engine = true;

            // Assert
            loader.Engine.Should().BeTrue();
        }

        [TestMethod]
        public void T_SetAndGet_Works()
        {
            // Arrange
            var loader = new SmartSerializableLoader();

            // Act
            loader.T = typeof(string);

            // Assert
            loader.T.Should().Be(typeof(string));
        }

        [TestMethod]
        public async Task DeserializeAsync_NullGlobals_ThrowsArgumentNullException()
        {
            // Act
            Func<Task> act = async () => await SmartSerializableLoader.DeserializeAsync(null, "{}");

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task DeserializeAsync_Binary_NullGlobals_ThrowsArgumentNullException()
        {
            // Arrange
            var binary = Encoding.UTF8.GetBytes("{}");

            // Act
            Func<Task> act = async () =>
                await SmartSerializableLoader.DeserializeAsync(null, binary);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void TryConvertBinaryToJson_ValidUtf8_ReturnsString()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(
                mockApp.Object,
                true,
                variable => variable == "OneDriveCommercial" ? @"C:\OneDrive" : null
            );
            var loader = new SmartSerializableLoader(globals);
            var json = "{\"Name\":\"test\"}";
            var binary = Encoding.UTF8.GetBytes(json);

            // Act
            var result = loader.TryConvertBinaryToJson(binary);

            // Assert
            result.Should().Be(json);
        }

        [TestMethod]
        public void DeserializeConfig_EmptyBinary_ReturnsNull()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(
                mockApp.Object,
                true,
                variable => variable == "OneDriveCommercial" ? @"C:\OneDrive" : null
            );
            var loader = new SmartSerializableLoader(globals);
            var binary = new byte[0];

            // Act
            var result = loader.DeserializeConfig(binary);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task DeserializeAsync_CancelledToken_ReturnsNull()
        {
            // Arrange
            var mockRepo = new MockRepository(MockBehavior.Loose);
            var mockApp = mockRepo.Create<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(
                mockApp.Object,
                true,
                variable => variable == "OneDriveCommercial" ? @"C:\OneDrive" : null
            );
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await SmartSerializableLoader.DeserializeAsync(globals, "{}", cts.Token);

            // Assert
            result.Should().BeNull();
        }
    }
}
