using System;
using System.Collections.Concurrent;
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

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
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
        public async Task LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing()
        {
            var application = new Mock<OutlookApplication>();
            var configuration = new ConcurrentDictionary<string, SmartSerializableLoader>();
            var globals = new StubApplicationGlobals();
            globals.IntelResInstance = new StubIntelligenceConfig(globals, configuration);

            var sut = new AppOlObjects(application.Object, globals);

            await sut.LoadStoresAsync();

            sut.StoresWrapper.Should().BeNull();
        }

        private sealed class TestableAppOlObjects : AppOlObjects
        {
            private readonly Func<StoresWrapper, Task> awaitStoreRewireAsync;

            internal TestableAppOlObjects(
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
