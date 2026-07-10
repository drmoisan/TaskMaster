using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.ReusableTypeClasses;
using OutlookApplication = Microsoft.Office.Interop.Outlook.Application;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    [DoNotParallelize]
    public class AppOlObjectsCoverageTests
    {
        [TestMethod]
        public async Task LoadAsync_AssignsStoresWrapperFromConfigAndCompletes()
        {
            var application = new Mock<OutlookApplication>();
            var storesWrapperLoader = new SmartSerializableLoader();
            var configuration = new ConcurrentDictionary<string, SmartSerializableLoader>();
            var globals = new StubApplicationGlobals();
            var expectedWrapper = new StoresWrapper();
            var rewireStarted = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var rewireCanFinish = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var smartSerializable = new Mock<ISmartSerializableNonTyped>(MockBehavior.Strict);

            configuration.TryAdd("StoresWrapper", storesWrapperLoader);
            globals.IntelResInstance = new StubIntelligenceConfig(globals, configuration);
            smartSerializable
                .Setup(x =>
                    x.Deserialize<StoresWrapper, SmartSerializableLoader>(
                        It.IsAny<SmartSerializable<SmartSerializableLoader>>()
                    )
                )
                .Returns(expectedWrapper);

            var sut = new TestableAppOlObjects(
                application.Object,
                globals,
                async storesWrapper =>
                {
                    storesWrapper.Should().BeSameAs(expectedWrapper);
                    rewireStarted.SetResult(true);
                    await rewireCanFinish.Task;
                }
            )
            {
                SmartSerializable = smartSerializable.Object,
            };

            var loadTask = sut.LoadAsync();
            await rewireStarted.Task;

            sut.StoresWrapper.Should().BeSameAs(expectedWrapper);
            loadTask
                .IsCompleted.Should()
                .BeFalse("LoadAsync should remain incomplete until store rewire finishes.");

            rewireCanFinish.SetResult(true);
            await loadTask;

            loadTask.IsCompleted.Should().BeTrue();
            smartSerializable.VerifyAll();
        }

        [TestMethod]
        public async Task LoadStoresAsync_WhenConfigMissing_BuildsFreshStoresWrapper()
        {
            // Arrange: no "StoresWrapper" key in config (Path 1 - config missing). The fix must fall
            // back to building a fresh model from live stores, not leave StoresWrapper null (AC1, AC5).
            var application = new Mock<OutlookApplication>();
            var configuration = new ConcurrentDictionary<string, SmartSerializableLoader>();
            var globals = new StubApplicationGlobals();
            globals.IntelResInstance = new StubIntelligenceConfig(globals, configuration);
            var freshWrapper = new StoresWrapper();

            var sut = new TestableAppOlObjects(
                application.Object,
                globals,
                _ => Task.CompletedTask,
                freshWrapper
            );

            // Act
            await sut.LoadStoresAsync();

            // Assert
            sut.StoresWrapper.Should().BeSameAs(freshWrapper);
            sut.BuildFreshStoresWrapperInvocationCount.Should().Be(1);
        }

        [TestMethod]
        public async Task LoadStoresAsync_WhenConfigDeserializesToNull_BuildsFreshStoresWrapper()
        {
            // Arrange: "StoresWrapper" key present but deserialize returns null (Path 2). The fix must
            // apply the same fresh-build fallback and must NOT invoke AwaitStoreRewireAsync on that
            // path (AC2, AC5).
            var application = new Mock<OutlookApplication>();
            var configuration = new ConcurrentDictionary<string, SmartSerializableLoader>();
            var globals = new StubApplicationGlobals();
            configuration.TryAdd("StoresWrapper", new SmartSerializableLoader());
            globals.IntelResInstance = new StubIntelligenceConfig(globals, configuration);
            var freshWrapper = new StoresWrapper();
            var smartSerializable = new Mock<ISmartSerializableNonTyped>();
            smartSerializable
                .Setup(x =>
                    x.Deserialize<StoresWrapper, SmartSerializableLoader>(
                        It.IsAny<SmartSerializable<SmartSerializableLoader>>()
                    )
                )
                .Returns((StoresWrapper)null);

            var sut = new TestableAppOlObjects(
                application.Object,
                globals,
                _ => Task.CompletedTask,
                freshWrapper
            )
            {
                SmartSerializable = smartSerializable.Object,
            };

            // Act
            await sut.LoadStoresAsync();

            // Assert
            sut.StoresWrapper.Should().BeSameAs(freshWrapper);
            sut.BuildFreshStoresWrapperInvocationCount.Should().Be(1);
            sut.AwaitStoreRewireInvocationCount.Should()
                .Be(0, "the fresh-build path bypasses store rewire.");
        }

        [TestMethod]
        public async Task LoadStoresAsync_WhenDeserializeThrows_AbsorbsExceptionAndLeavesStoresWrapperNull()
        {
            // Arrange: deserialize throws (Path 3 - genuine failure, matching the SmartSerializableBase
            // throw site). The fix must absorb the exception at the method boundary, leave
            // StoresWrapper null, and NOT attempt a fresh build after a mid-deserialize failure
            // (AC3, AC5).
            var application = new Mock<OutlookApplication>();
            var configuration = new ConcurrentDictionary<string, SmartSerializableLoader>();
            var globals = new StubApplicationGlobals();
            configuration.TryAdd("StoresWrapper", new SmartSerializableLoader());
            globals.IntelResInstance = new StubIntelligenceConfig(globals, configuration);
            var smartSerializable = new Mock<ISmartSerializableNonTyped>();
            smartSerializable
                .Setup(x =>
                    x.Deserialize<StoresWrapper, SmartSerializableLoader>(
                        It.IsAny<SmartSerializable<SmartSerializableLoader>>()
                    )
                )
                .Throws<InvalidOperationException>();

            var sut = new TestableAppOlObjects(
                application.Object,
                globals,
                _ => Task.CompletedTask,
                new StoresWrapper()
            )
            {
                SmartSerializable = smartSerializable.Object,
            };

            // Act
            Func<Task> act = async () => await sut.LoadStoresAsync();

            // Assert
            await act.Should().NotThrowAsync();
            sut.StoresWrapper.Should().BeNull();
            sut.BuildFreshStoresWrapperInvocationCount.Should()
                .Be(0, "there is no fresh-build retry after a mid-deserialize exception.");
        }

        [TestMethod]
        public void BuildFreshStoresWrapper_WhenLiveStoresAvailable_ReturnsInitializedWrapper()
        {
            // Arrange: a Moq IApplicationGlobals -> IOlObjects -> NameSpace -> Stores chain with one
            // includable store, mirroring the proven CreateGlobalsWithStores pattern in
            // UtilitiesCS.Test StoresWrapperTests (mocked COM enumerator; no live Outlook, no temp
            // files). Exercises the REAL (non-overridden) BuildFreshStoresWrapper() seam body so the
            // new method reaches its coverage target (AC7).
            var includedStore = CreateStore("Mailbox", @"C:\Data\mailbox.ost", "user@example.com");
            var globals = CreateGlobalsWithStores(includedStore.Object);
            var application = new Mock<OutlookApplication>();
            var sut = new TestableAppOlObjects(
                application.Object,
                globals.Object,
                _ => Task.CompletedTask
            );

            // Act
            var result = sut.InvokeBaseBuildFreshStoresWrapper();

            // Assert
            result.Should().NotBeNull();
            result.Stores.Should().ContainSingle();
            result.Stores.Single().DisplayName.Should().Be("Mailbox");
        }

        private sealed class TestableAppOlObjects : AppOlObjects
        {
            private readonly Func<StoresWrapper, Task> awaitStoreRewireAsync;
            private readonly StoresWrapper freshStoresWrapperSentinel;

            internal int BuildFreshStoresWrapperInvocationCount { get; private set; }
            internal int AwaitStoreRewireInvocationCount { get; private set; }

            internal TestableAppOlObjects(
                OutlookApplication olApplication,
                IApplicationGlobals appGlobals,
                Func<StoresWrapper, Task> awaitStoreRewireAsync,
                StoresWrapper freshStoresWrapperSentinel = null
            )
                : base(olApplication, appGlobals)
            {
                this.awaitStoreRewireAsync = awaitStoreRewireAsync;
                this.freshStoresWrapperSentinel = freshStoresWrapperSentinel;
            }

            protected internal override Task AwaitStoreRewireAsync(StoresWrapper storesWrapper)
            {
                AwaitStoreRewireInvocationCount++;
                return awaitStoreRewireAsync(storesWrapper);
            }

            protected internal override StoresWrapper BuildFreshStoresWrapper()
            {
                BuildFreshStoresWrapperInvocationCount++;
                return freshStoresWrapperSentinel;
            }

            internal StoresWrapper InvokeBaseBuildFreshStoresWrapper() =>
                base.BuildFreshStoresWrapper();
        }

        private static Mock<IApplicationGlobals> CreateGlobalsWithStores(
            params OutlookStore[] stores
        )
        {
            var storesCollection = new Mock<Stores>();
            storesCollection
                .As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => stores.Cast<object>().GetEnumerator());

            var nameSpace = new Mock<NameSpace>();
            nameSpace.SetupGet(x => x.Stores).Returns(storesCollection.Object);

            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.NamespaceMAPI).Returns(nameSpace.Object);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static Mock<OutlookStore> CreateStore(
            string displayName,
            string filePath,
            string primarySmtpAddress
        )
        {
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress(primarySmtpAddress);

            store.SetupGet(x => x.DisplayName).Returns(displayName);
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                .Returns(new Mock<Folder>().Object);
            store.SetupGet(x => x.FilePath).Returns(filePath);

            return store;
        }

        private static Mock<Folder> CreateRootFolderWithPrimarySmtpAddress(
            string primarySmtpAddress
        )
        {
            var rootFolder = new Mock<Folder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<Recipient>();
            var addressEntry = new Mock<AddressEntry>();
            var exchangeUser = new Mock<ExchangeUser>();

            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Returns(primarySmtpAddress);
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            rootFolder.SetupGet(x => x.Session).Returns(session.Object);

            return rootFolder;
        }

        private sealed class StubApplicationGlobals : IApplicationGlobals
        {
            internal IntelligenceConfig IntelResInstance { get; set; } = null!;

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
            internal StubIntelligenceConfig(
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
