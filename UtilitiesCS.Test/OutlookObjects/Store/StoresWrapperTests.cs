using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    [TestClass]
    [DoNotParallelize]
    public class StoresWrapperTests
    {
        [TestMethod]
        public async Task CreateAsync_WhenCancellationAlreadyRequested_ThrowsOperationCanceledException()
        {
            var globals = new Mock<IApplicationGlobals>();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Func<Task> act = async () =>
                await StoresWrapper.CreateAsync(globals.Object, cancellation.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task CreateAsync_WhenGlobalsAreNull_ThrowsNullReferenceException()
        {
            Func<Task> act = async () =>
                await StoresWrapper.CreateAsync(null, CancellationToken.None);

            await act.Should().ThrowAsync<NullReferenceException>();
        }

        [TestMethod]
        public async Task CreateAsync_WhenInputsValid_ReturnsInitializedStoresWrapper()
        {
            var includedStore = CreateStore("Mailbox", @"C:\Data\mailbox.ost", "user@example.com");
            var secondStore = CreateStore(
                "Archive Mailbox",
                @"C:\Data\archive.ost",
                "archive@example.com"
            );
            var globals = CreateGlobalsWithStores(includedStore.Object, secondStore.Object);

            var result = await StoresWrapper.CreateAsync(globals.Object, CancellationToken.None);

            result.Should().NotBeNull();
            result.Globals.Should().BeSameAs(globals.Object);
            result.Stores.Should().HaveCount(2);
            result.Stores.Select(x => x.DisplayName).Should().Equal("Mailbox", "Archive Mailbox");
            result
                .Stores.Select(x => x.UserEmailAddress)
                .Should()
                .Equal("user@example.com", "archive@example.com");
        }

        [TestMethod]
        public void Constructor_WithoutGlobals_SetsDefaultFlagsAndCollections()
        {
            var wrapper = new StoresWrapper();

            wrapper.Globals.Should().BeNull();
            wrapper.Stores.Should().BeNull();
            wrapper.ExcludePublicFolderStores.Should().BeTrue();
            wrapper.ExcludeGwsoStores.Should().BeTrue();
            wrapper
                .GwsoFilePathContains.Should()
                .Equal(@"\Google\Google Apps Sync\", @"\Google\Google Workspace Sync\");
            wrapper.ExcludedStoreNameContains.Should().BeEmpty();
            wrapper.ExcludedStoreFilePathContains.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithGlobals_PreservesGlobalsReference()
        {
            var globals = new Mock<IApplicationGlobals>();

            var wrapper = new StoresWrapper(globals.Object);

            wrapper.Globals.Should().BeSameAs(globals.Object);
        }

        [TestMethod]
        public void Init_WhenStoresMatchFilters_ProjectsOnlyIncludedStores()
        {
            var includedStore = CreateStore("Mailbox", @"C:\Data\mailbox.ost", "owner@example.com");
            var excludedByName = CreateStore(
                "Archive Mailbox",
                @"C:\Data\archive.ost",
                "archive@example.com"
            );
            var excludedPublicFolder = CreateStore(
                "Public Folders",
                @"C:\Data\public.ost",
                "public@example.com",
                OlExchangeStoreType.olExchangePublicFolder
            );
            var excludedByPath = CreateStore(
                "Temp Store",
                @"C:\Temp\store.pst",
                "temp@example.com"
            );
            var excludedGwso = CreateStore(
                "Google Workspace",
                @"C:\Users\Dan\Google\Google Workspace Sync\sync.ost",
                "gwso@example.com"
            );

            var wrapper = new StoresWrapper(
                CreateGlobalsWithStores(
                    includedStore.Object,
                    excludedByName.Object,
                    excludedPublicFolder.Object,
                    excludedByPath.Object,
                    excludedGwso.Object
                ).Object
            )
            {
                ExcludedStoreNameContains = new List<string> { "Archive" },
                ExcludedStoreFilePathContains = new List<string> { "Temp" },
            };

            var result = wrapper.Init();

            result.Should().BeSameAs(wrapper);
            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].DisplayName.Should().Be("Mailbox");
            wrapper.Stores[0].UserEmailAddress.Should().Be("owner@example.com");
        }

        [TestMethod]
        public async Task RewireOlObjectsAsync_WhenStoresCollectionIsNull_InitializesAndAddsFilteredStores()
        {
            var includedStore = CreateStore("Mailbox", @"C:\Data\mailbox.ost", "owner@example.com");
            var excludedStore = CreateStore(
                "Archive",
                @"C:\Temp\archive.pst",
                "archive@example.com"
            );

            var wrapper = new TestableStoresWrapper(
                CreateGlobalsWithStores(includedStore.Object, excludedStore.Object).Object
            )
            {
                Stores = null,
                ExcludedStoreNameContains = new List<string> { "Archive" },
                ExcludedStoreFilePathContains = new List<string> { "Temp" },
            };

            await wrapper.RewireOlObjectsAsync(new StreamingContext());

            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].DisplayName.Should().Be("Mailbox");
            wrapper.Stores[0].UserEmailAddress.Should().Be("owner@example.com");
        }

        [TestMethod]
        public async Task RewireOlObjectsAsync_WhenStoreDisplayNameAlreadyExists_RestoresExistingWrapperWithoutAddingDuplicate()
        {
            var originalStore = CreateStore("Mailbox", @"C:\Data\old.ost", "old@example.com");
            var updatedStore = CreateStore("Mailbox", @"C:\Data\new.ost", "new@example.com");
            var existingWrapper = new StoreWrapper(originalStore.Object).Init();

            var wrapper = new TestableStoresWrapper(
                CreateGlobalsWithStores(updatedStore.Object).Object
            )
            {
                Stores = new List<StoreWrapper> { existingWrapper },
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false,
                ExcludePublicFolderStores = false,
            };

            await wrapper.RewireOlObjectsAsync(new StreamingContext());

            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].Should().BeSameAs(existingWrapper);
            existingWrapper.InnerStore.Should().BeSameAs(updatedStore.Object);
            existingWrapper.UserEmailAddress.Should().Be("new@example.com");
        }

        [TestMethod]
        public async Task RewireAfterDeserializeAsync_PublicEntryHitsRealMethodBody()
        {
            var originalStore = CreateStore("Mailbox", @"C:\Data\old.ost", "old@example.com");
            var updatedStore = CreateStore("Mailbox", @"C:\Data\new.ost", "new@example.com");
            var existingWrapper = new StoreWrapper(originalStore.Object).Init();
            var wrapper = new StoresWrapper(CreateGlobalsWithStores(updatedStore.Object).Object)
            {
                Stores = new List<StoreWrapper> { existingWrapper },
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false,
                ExcludePublicFolderStores = false,
            };

            await wrapper.RewireAfterDeserializeAsync();

            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].Should().BeSameAs(existingWrapper);
            existingWrapper.InnerStore.Should().BeSameAs(updatedStore.Object);
            existingWrapper.UserEmailAddress.Should().Be("new@example.com");
        }

        [TestMethod]
        public async Task RewireOlObjects_OnDeserializedAdapterUsesExplicitCompletionContract()
        {
            var wrapper = new AdapterObservingStoresWrapper();

            wrapper.RewireOlObjects(new StreamingContext());

            var completedTask = await Task.WhenAny(wrapper.RewireInvoked.Task, Task.Delay(5000));

            completedTask
                .Should()
                .BeSameAs(
                    wrapper.RewireInvoked.Task,
                    "the retained deserialization hook should delegate to the explicit awaitable rewire contract."
                );
            wrapper.InvocationCount.Should().Be(1);
        }

        [TestMethod]
        public void RewireOlObjectsAsync_PreservesStoreOrderAcrossYieldedIterations()
        {
            // This regression inspects the store-rewire coordinator source directly because
            // the production method currently performs the entire restore loop synchronously.
            // The fix contract is explicit: keep the single-store iteration order intact while
            // inserting cooperative yield boundaries between iterations.
            var source = File.ReadAllText(
                Path.Combine(
                    GetRepositoryRoot(),
                    "UtilitiesCS",
                    "OutlookObjects",
                    "Store",
                    "StoresWrapper.cs"
                )
            );
            var methodBody = ExtractMethodBody(
                source,
                "RewireOlObjectsAsync(StreamingContext context)"
            );

            methodBody.Should().Contain("foreach (var store in stores)");
            Regex
                .IsMatch(methodBody, @"Task\.(WhenAll|Run)\s*\(")
                .Should()
                .BeFalse("store rewire should remain ordered instead of parallelizing iterations.");
            Regex
                .IsMatch(methodBody, @"await\s+Task\.Yield\s*\(\s*\)\s*;")
                .Should()
                .BeTrue(
                    "the store rewire loop should yield between expensive store iterations without reordering them."
                );
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

        private static string GetRepositoryRoot()
        {
            var assemblyDirectory = new DirectoryInfo(
                Path.GetDirectoryName(typeof(StoresWrapper).Assembly.Location)!
            );
            var repositoryRoot = assemblyDirectory.Parent?.Parent?.Parent?.FullName;

            repositoryRoot.Should().NotBeNullOrEmpty();
            File.Exists(Path.Combine(repositoryRoot!, "README.md")).Should().BeTrue();

            return repositoryRoot!;
        }

        private static string ExtractMethodBody(string source, string methodName)
        {
            var methodIndex = source.IndexOf(methodName, StringComparison.Ordinal);
            methodIndex.Should().BeGreaterThanOrEqualTo(0, $"source should contain '{methodName}'");

            var bodyStart = source.IndexOf('{', methodIndex);
            bodyStart.Should().BeGreaterThanOrEqualTo(0, "the target method should have a body");

            var braceDepth = 0;
            for (var index = bodyStart; index < source.Length; index++)
            {
                if (source[index] == '{')
                {
                    braceDepth++;
                }
                else if (source[index] == '}')
                {
                    braceDepth--;
                    if (braceDepth == 0)
                    {
                        return source.Substring(bodyStart + 1, index - bodyStart - 1);
                    }
                }
            }

            throw new AssertFailedException($"Unable to extract body for '{methodName}'.");
        }

        private static Mock<OutlookStore> CreateStore(
            string displayName,
            string filePath,
            string primarySmtpAddress,
            OlExchangeStoreType exchangeStoreType = OlExchangeStoreType.olPrimaryExchangeMailbox,
            bool throwOnFilePathAccess = false
        )
        {
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress(primarySmtpAddress);

            store.SetupGet(x => x.DisplayName).Returns(displayName);
            store.SetupGet(x => x.ExchangeStoreType).Returns(exchangeStoreType);
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);

            if (exchangeStoreType != OlExchangeStoreType.olExchangePublicFolder)
            {
                store
                    .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                    .Returns(new Mock<OutlookFolder>().Object);
            }

            if (throwOnFilePathAccess)
            {
                store
                    .SetupGet(x => x.FilePath)
                    .Throws(new InvalidOperationException("FilePath unavailable"));
            }
            else
            {
                store.SetupGet(x => x.FilePath).Returns(filePath);
            }

            return store;
        }

        private static Mock<OutlookFolder> CreateRootFolderWithPrimarySmtpAddress(
            string primarySmtpAddress
        )
        {
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var addressEntry = new Mock<AddressEntry>();
            var exchangeUser = new Mock<ExchangeUser>();

            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Returns(primarySmtpAddress);
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            rootFolder.SetupGet(x => x.Session).Returns(session.Object);

            return rootFolder;
        }

        private sealed class TestableStoresWrapper : StoresWrapper
        {
            public TestableStoresWrapper(IApplicationGlobals globals)
                : base(globals) { }

            public new Task RewireOlObjectsAsync(StreamingContext context)
            {
                return base.RewireOlObjectsAsync(context);
            }
        }

        private sealed class AdapterObservingStoresWrapper : StoresWrapper
        {
            public TaskCompletionSource<bool> RewireInvoked { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            public int InvocationCount { get; private set; }

            public override Task RewireAfterDeserializeAsync()
            {
                InvocationCount++;
                RewireInvoked.TrySetResult(true);
                return Task.CompletedTask;
            }
        }
    }
}
