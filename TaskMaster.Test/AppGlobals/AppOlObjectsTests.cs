using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.Dialogs;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.ReusableTypeClasses;
using OutlookApplication = Microsoft.Office.Interop.Outlook.Application;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class AppOlObjectsTests
    {
        private MockRepository mockRepository = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TestMethod]
        public void TryGetSmtpAddress_ReturnsNull_WhenAddressEntryIsNull()
        {
            // Arrange
            AddressEntry addressEntry = null!;

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void TryGetSmtpAddress_ReturnsExchangePrimarySmtpAddress_WhenAvailable()
        {
            // Arrange
            var expectedAddress = "user@contoso.com";
            var exchangeUser = mockRepository.Create<ExchangeUser>();
            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Returns(expectedAddress);

            var addressEntry = mockRepository.Create<AddressEntry>();
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry.Object);

            // Assert
            result.Should().Be(expectedAddress);
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TryGetSmtpAddress_ReturnsAddressProperty_WhenExchangeUserIsUnavailable()
        {
            // Arrange
            var expectedAddress = "fallback@contoso.com";
            var addressEntry = mockRepository.Create<AddressEntry>();
            addressEntry.Setup(x => x.GetExchangeUser()).Returns((ExchangeUser)null!);
            addressEntry.SetupGet(x => x.Address).Returns(expectedAddress);

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry.Object);

            // Assert
            result.Should().Be(expectedAddress);
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TryGetSmtpAddress_ReturnsNull_WhenOutlookInteropCallsThrowComException()
        {
            // Arrange
            var addressEntry = mockRepository.Create<AddressEntry>();
            addressEntry
                .Setup(x => x.GetExchangeUser())
                .Throws(new COMException("The operation failed."));
            addressEntry.SetupGet(x => x.Address).Throws(new COMException("The operation failed."));

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry.Object);

            // Assert
            result.Should().BeNull();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ReadJunkPotentialSetting_ReturnsJunkPotentialValue()
        {
            // Arrange
            var original = Properties.Settings.Default.JunkPotential;
            var expected = "Inbox\\Junk Suspects SB";
            Properties.Settings.Default.JunkPotential = expected;

            try
            {
                // Act
                var result = AppOlObjects.ReadJunkPotentialSetting();

                // Assert
                result.Should().Be(expected);
            }
            finally
            {
                Properties.Settings.Default.JunkPotential = original;
            }
        }

        [TestMethod]
        public void WriteJunkPotentialSetting_UpdatesJunkPotentialValue()
        {
            // Arrange
            var original = Properties.Settings.Default.JunkPotential;
            var expected = "Inbox\\Junk Suspects SB";

            try
            {
                // Act
                AppOlObjects.WriteJunkPotentialSetting(expected);

                // Assert
                Properties.Settings.Default.JunkPotential.Should().Be(expected);
            }
            finally
            {
                Properties.Settings.Default.JunkPotential = original;
            }
        }

        [TestMethod]
        public void LoadJunkCertain_KeepsStoredValue_WhenReplacementSelectionIsCancelled()
        {
            var originalSetting = Properties.Settings.Default.OlJunkCertain;
            var dialogInvokerProperty = typeof(MyBox).GetProperty(
                "DialogInvoker",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;
            var originalDialogInvoker = dialogInvokerProperty.GetValue(null);
            var expected = "Missing\\Junk Email";
            var namespaceMapi = mockRepository.Create<NameSpace>();
            namespaceMapi.Setup(x => x.PickFolder()).Returns((MAPIFolder)null!);
            var application = mockRepository.Create<OutlookApplication>();
            application.SetupGet(x => x.Application).Returns(application.Object);
            application.Setup(x => x.GetNamespace("MAPI")).Returns(namespaceMapi.Object);
            var root = CreateRootFolder();
            var sut = new AppOlObjects(application.Object, Mock.Of<IApplicationGlobals>());
            SetPrivateField(sut, "_root", root.Object);

            try
            {
                Properties.Settings.Default.OlJunkCertain = expected;
                dialogInvokerProperty.SetValue(
                    null,
                    new Func<MyBoxViewer, System.Windows.Forms.DialogResult>(_ =>
                        System.Windows.Forms.DialogResult.OK
                    )
                );

                sut.LoadJunkCertain().Should().BeNull();
                AppOlObjects.ReadJunkCertainSetting().Should().Be(expected);
            }
            finally
            {
                Properties.Settings.Default.OlJunkCertain = originalSetting;
                dialogInvokerProperty.SetValue(null, originalDialogInvoker);
            }
        }

        [TestMethod]
        public async Task LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes()
        {
            var application = mockRepository.Create<OutlookApplication>();
            var storesWrapperLoader = new SmartSerializableLoader();
            var configuration = new ConcurrentDictionary<string, SmartSerializableLoader>();
            var globals = new StubApplicationGlobals();
            var intelligenceConfig = new StubIntelligenceConfig(globals, configuration);
            var storesWrapper = new StoresWrapper();
            var rewireAwaitStarted = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var rewireCanFinish = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var smartSerializable = mockRepository.Create<ISmartSerializableNonTyped>();

            configuration.TryAdd("StoresWrapper", storesWrapperLoader);
            globals.IntelResInstance = intelligenceConfig;
            smartSerializable
                .Setup(x =>
                    x.Deserialize<StoresWrapper, SmartSerializableLoader>(
                        It.IsAny<SmartSerializable<SmartSerializableLoader>>()
                    )
                )
                .Returns(storesWrapper);

            var sut = new TestableAppOlObjects(
                application.Object,
                globals,
                async _ =>
                {
                    rewireAwaitStarted.SetResult(true);
                    await rewireCanFinish.Task;
                    storesWrapper.Stores = new System.Collections.Generic.List<StoreWrapper>();
                }
            )
            {
                SmartSerializable = smartSerializable.Object,
            };

            var loadTask = sut.LoadStoresAsync();
            await rewireAwaitStarted.Task;

            try
            {
                loadTask
                    .IsCompleted.Should()
                    .BeFalse("LoadStoresAsync should await store rewire completion.");
            }
            finally
            {
                rewireCanFinish.SetResult(true);
                await loadTask;
            }
        }

        [TestMethod]
        public void AwaitStoreRewireAsync_ReturnsCompletedTaskWhenStoresWrapperIsNull()
        {
            // Arrange
            var application = mockRepository.Create<OutlookApplication>();
            var sut = new BaseAwaitingAppOlObjects(
                application.Object,
                new StubApplicationGlobals()
            );

            // Act
            var task = sut.InvokeBaseAwaitStoreRewireAsync(null!);

            // Assert
            task.Should().BeSameAs(Task.CompletedTask);
            task.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public void AwaitStoreRewireAsync_DoesNotInvokeWrapperWhenStoresWrapperIsNull()
        {
            // Arrange
            var application = mockRepository.Create<OutlookApplication>();
            var sut = new BaseAwaitingAppOlObjects(
                application.Object,
                new StubApplicationGlobals()
            );

            // Act
            var task = sut.InvokeBaseAwaitStoreRewireAsync(null!);

            // Assert
            task.Should().BeSameAs(Task.CompletedTask);
            task.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task AwaitStoreRewireAsync_AwaitsStoresWrapperInvocation()
        {
            // Arrange
            var application = mockRepository.Create<OutlookApplication>();
            var completion = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var wrapper = new DelayedRewireStoresWrapper(completion.Task);
            var sut = new BaseAwaitingAppOlObjects(
                application.Object,
                new StubApplicationGlobals()
            );

            // Act
            var task = sut.InvokeBaseAwaitStoreRewireAsync(wrapper);

            // Assert
            wrapper.RewireInvocationCount.Should().Be(1);
            task.IsCompleted.Should().BeFalse("the returned task should await wrapper rewiring.");

            completion.SetResult(true);
            await task;
            task.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task AwaitStoreRewireAsync_InvokesWrappedStoreRewireWhenWrapperExists()
        {
            // Arrange
            var application = mockRepository.Create<OutlookApplication>();
            var completion = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var wrapper = new DelayedRewireStoresWrapper(completion.Task);
            var sut = new BaseAwaitingAppOlObjects(
                application.Object,
                new StubApplicationGlobals()
            );

            // Act
            var task = sut.InvokeBaseAwaitStoreRewireAsync(wrapper);

            // Assert
            wrapper.RewireInvocationCount.Should().Be(1);
            task.IsCompleted.Should()
                .BeFalse("the wrapper task should be awaited before completion.");

            completion.SetResult(true);
            await task;
            task.IsCompleted.Should().BeTrue();
        }

        private sealed class TestableAppOlObjects : AppOlObjects
        {
            private readonly Func<StoresWrapper, Task> awaitStoreRewireAsync;

            public TestableAppOlObjects(
                OutlookApplication olApplication,
                IApplicationGlobals appGlobals,
                Func<StoresWrapper, Task> awaitStoreRewireAsync
            )
                : base(olApplication, appGlobals)
            {
                this.awaitStoreRewireAsync = awaitStoreRewireAsync;
            }

            protected internal override Task AwaitStoreRewireAsync(StoresWrapper storesWrapper)
            {
                return awaitStoreRewireAsync(storesWrapper);
            }
        }

        private sealed class BaseAwaitingAppOlObjects : AppOlObjects
        {
            public BaseAwaitingAppOlObjects(
                OutlookApplication olApplication,
                IApplicationGlobals appGlobals
            )
                : base(olApplication, appGlobals) { }

            public Task InvokeBaseAwaitStoreRewireAsync(StoresWrapper storesWrapper)
            {
                return base.AwaitStoreRewireAsync(storesWrapper);
            }
        }

        private sealed class DelayedRewireStoresWrapper : StoresWrapper
        {
            private readonly Task delayedTask;

            public DelayedRewireStoresWrapper(Task delayedTask)
            {
                this.delayedTask = delayedTask;
            }

            public int RewireInvocationCount { get; private set; }

            public override Task RewireAfterDeserializeAsync()
            {
                RewireInvocationCount++;
                return delayedTask;
            }
        }

        private Mock<Folder> CreateRootFolder()
        {
            var folders = mockRepository.Create<Folders>();
            folders.SetupGet(x => x.Count).Returns(0);
            folders
                .As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(Array.Empty<MAPIFolder>().GetEnumerator());

            var root = mockRepository.Create<Folder>();
            root.SetupGet(x => x.Name).Returns("Mailbox");
            root.SetupGet(x => x.FolderPath).Returns(@"\\Mailbox");
            root.SetupGet(x => x.Folders).Returns(folders.Object);
            return root;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            typeof(AppOlObjects)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(target, value);
        }

        private sealed class StubApplicationGlobals : IApplicationGlobals
        {
            public IntelligenceConfig IntelResInstance { get; set; } = null!;

            public Task LoadAsync(bool parallel) => throw new NotSupportedException();

            public IFileSystemFolderPaths FS => throw new NotSupportedException();

            public IOlObjects Ol => throw new NotSupportedException();

            public IToDoObjects TD => throw new NotSupportedException();

            public IAppAutoFileObjects AF => throw new NotSupportedException();

            public IAppEvents Events => throw new NotSupportedException();

            public IAppQuickFilerSettings QfSettings => throw new NotSupportedException();

            public IAppItemEngines Engines => throw new NotSupportedException();

            public IntelligenceConfig IntelRes => IntelResInstance;

            public IStoreDisableService StoreDisable => throw new NotSupportedException();
        }

        private sealed class StubIntelligenceConfig : IntelligenceConfig
        {
            public StubIntelligenceConfig(
                IApplicationGlobals globals,
                ConcurrentDictionary<string, SmartSerializableLoader> config
            )
                : base(globals)
            {
                Config = config;
            }
        }
    }
}
