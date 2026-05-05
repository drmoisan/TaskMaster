using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Test.AppGlobals;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster.Test.OutlookObjects.Store
{
    [TestClass]
    public class StoresWrapperTests
    {
        [TestMethod]
        public void RewireOlObjectsAsync_PreservesStoreOrderAcrossYieldedIterations()
        {
            // This regression inspects the store-rewire coordinator source directly because
            // the pre-fix production path does not expose a narrow awaitable seam in this
            // test project. The contract we need to lock in is explicit: preserve the
            // single-store iteration order while adding cooperative yield boundaries
            // between expensive iterations.
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

        [TestMethod]
        public void RewireAfterDeserializeAsync_UsesStoreAdapterForWrappedStores()
        {
            // This regression inspects the rewire coordinator source because the contract is an
            // explicit branch choice: when a serialized wrapper already exists for the matching
            // display name, the coordinator should restore that wrapper instead of creating a new
            // one.
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

            Regex
                .IsMatch(
                    methodBody,
                    @"if\s*\(storeWrapper is null\)\s*\{[\s\S]*?new StoreWrapper\(store\)\.Init\(\);[\s\S]*?\}\s*else\s*\{[\s\S]*?storeWrapper\.Restore\(store\);"
                )
                .Should()
                .BeTrue(
                    "rewiring should restore an existing wrapped store instead of recreating it."
                );
        }

        [TestMethod]
        public void RewireAfterDeserializeAsync_SingleStoreCompletesWithoutExtraYield()
        {
            // This regression locks in the first-iteration guard so a single store can complete
            // without an unnecessary yield hop.
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

            Regex
                .IsMatch(
                    methodBody,
                    @"if\s*\(processedStoreCount > 0\)\s*\{\s*await\s+Task\.Yield\s*\(\s*\)\s*;\s*\}"
                )
                .Should()
                .BeTrue(
                    "the first store should not incur a yield before any work has been processed."
                );
        }

        [TestMethod]
        public void RewireAfterDeserializeAsync_MultiStoreYieldsBetweenStores()
        {
            // This regression locks in the cooperative-yield behavior between store iterations so
            // multi-store rewiring remains responsive without yielding before the first store.
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

            Regex
                .IsMatch(
                    methodBody,
                    @"processedStoreCount\s*=\s*0;[\s\S]*?foreach \(var store in stores\)[\s\S]*?if \(processedStoreCount > 0\)[\s\S]*?await\s+Task\.Yield\s*\(\s*\)\s*;[\s\S]*?processedStoreCount\+\+;"
                )
                .Should()
                .BeTrue(
                    "multi-store rewiring should yield only after the first store has already been processed."
                );
        }

        [TestMethod]
        public void RewireAfterDeserializeAsync_IncrementsProcessedStoreCountForEachWrappedStore()
        {
            // This regression locks in the iteration bookkeeping so every rewired store advances
            // the processed-store counter exactly once within the rewire loop.
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

            Regex
                .IsMatch(
                    methodBody,
                    @"foreach \(var store in stores\)[\s\S]*?(storeWrapper\.Restore\(store\)|Stores\.Add\(storeWrapper\);)[\s\S]*?processedStoreCount\+\+;"
                )
                .Should()
                .BeTrue(
                    "each rewired store should advance the processed-store counter before the next iteration."
                );
            Regex.Matches(methodBody, @"processedStoreCount\+\+;").Count.Should().Be(1);
        }

        [TestMethod]
        public void RewireAfterDeserializeAsync_YieldsBetweenAdapterWrappedStores()
        {
            // This regression locks in the wrapped-store restore path so subsequent iterations
            // yield before restoring an already wrapped store during multi-store rewiring.
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

            Regex
                .IsMatch(
                    methodBody,
                    @"processedStoreCount\s*=\s*0;[\s\S]*?foreach \(var store in stores\)[\s\S]*?if \(processedStoreCount > 0\)\s*\{\s*await\s+Task\.Yield\s*\(\s*\)\s*;\s*\}[\s\S]*?storeWrapper = Stores\.Find\(x => x\.DisplayName == storeDisplayName\);[\s\S]*?else\s*\{[\s\S]*?storeWrapper\.Restore\(store\);"
                )
                .Should()
                .BeTrue(
                    "multi-store restore iterations should yield before reusing an existing wrapped store."
                );
        }

        [TestMethod]
        public async Task RewireAfterDeserializeAsync_PublicEntryRewiresWrappedStores()
        {
            // Arrange
            var restoredStore = CreateStoreProxy(displayName: "Primary Store");
            var existingWrapper = new StoreWrapper(CreateStoreProxy(displayName: "Primary Store"))
            {
                DisplayName = "Primary Store",
            };
            var storesWrapper = new StoresWrapper(
                new StubApplicationGlobals(
                    new StubFileSystemFolderPaths(),
                    CreateOlObjectsProxy(CreateNamespaceProxy(restoredStore))
                )
            )
            {
                Stores = [existingWrapper],
            };

            // Act
            await storesWrapper.RewireAfterDeserializeAsync();

            // Assert
            storesWrapper.Stores.Should().ContainSingle();
            storesWrapper.Stores[0].Should().BeSameAs(existingWrapper);
            existingWrapper.RootFolder.Should().NotBeNull();
            existingWrapper.RootFolder.FolderPath.Should().Be("\\Primary Store");
        }

        [TestMethod]
        public async Task RewireAfterDeserializeAsync_PublicEntryMultiStoreHitsInnerYieldBranch()
        {
            // Arrange
            var firstRestoredStore = CreateStoreProxy(displayName: "Primary Store");
            var secondRestoredStore = CreateStoreProxy(displayName: "Archive Store");
            var firstWrapper = new StoreWrapper(CreateStoreProxy(displayName: "Primary Store"))
            {
                DisplayName = "Primary Store",
            };
            var secondWrapper = new StoreWrapper(CreateStoreProxy(displayName: "Archive Store"))
            {
                DisplayName = "Archive Store",
            };
            var storesWrapper = new StoresWrapper(
                new StubApplicationGlobals(
                    new StubFileSystemFolderPaths(),
                    CreateOlObjectsProxy(
                        CreateNamespaceProxy(firstRestoredStore, secondRestoredStore)
                    )
                )
            )
            {
                Stores = [firstWrapper, secondWrapper],
            };
            var originalContext = SynchronizationContext.Current;
            var controlledContext = new ControlledSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(controlledContext);

            try
            {
                // Act
                var rewireTask = storesWrapper.RewireAfterDeserializeAsync();

                // Assert
                rewireTask
                    .IsCompleted.Should()
                    .BeFalse(
                        "the multi-store public entry should suspend at the inner yield before the second store is rewired."
                    );
                firstWrapper.RootFolder.Should().NotBeNull();
                firstWrapper.RootFolder.FolderPath.Should().Be("\\Primary Store");
                secondWrapper
                    .RootFolder.Should()
                    .BeNull(
                        "the second store should not be rewired until the posted continuation runs."
                    );
                controlledContext.PendingCallbackCount.Should().BeGreaterThan(0);

                controlledContext.RunPostedCallbacks();
                await rewireTask;

                secondWrapper.RootFolder.Should().NotBeNull();
                secondWrapper.RootFolder.FolderPath.Should().Be("\\Archive Store");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(originalContext);
            }
        }

        private static string GetRepositoryRoot()
        {
            var assemblyDirectory = new DirectoryInfo(
                Path.GetDirectoryName(typeof(ThisAddIn).Assembly.Location)!
            );
            var repositoryRoot = assemblyDirectory.Parent?.Parent?.Parent?.FullName;

            repositoryRoot.Should().NotBeNullOrEmpty();
            File.Exists(Path.Combine(repositoryRoot!, "README.md")).Should().BeTrue();

            return repositoryRoot!;
        }

        private static string ExtractMethodBody(string source, string methodSignature)
        {
            var signatureIndex = source.IndexOf(methodSignature, System.StringComparison.Ordinal);
            signatureIndex
                .Should()
                .BeGreaterThanOrEqualTo(0, $"source should contain '{methodSignature}'");

            var bodyStart = source.IndexOf('{', signatureIndex);
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

            throw new AssertFailedException($"Unable to extract body for '{methodSignature}'.");
        }

        private static IOlObjects CreateOlObjectsProxy(Outlook.NameSpace nameSpace)
        {
            return (IOlObjects)
                new ReflectionRealProxy(
                    typeof(IOlObjects),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_NamespaceMAPI" => nameSpace,
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
        }

        private static Outlook.NameSpace CreateNamespaceProxy(params Outlook.Store[] stores)
        {
            var storesCollection = CreateStoresProxy(stores);

            return (Outlook.NameSpace)
                new ReflectionRealProxy(
                    typeof(Outlook.NameSpace),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_Stores" => storesCollection,
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
        }

        private static Outlook.Stores CreateStoresProxy(params Outlook.Store[] stores)
        {
            return (Outlook.Stores)
                new ReflectionRealProxy(
                    typeof(Outlook.Stores),
                    (method, _) =>
                        method.Name switch
                        {
                            "GetEnumerator" => ((IEnumerable)stores).GetEnumerator(),
                            "get_Count" => stores.Length,
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
        }

        private static Outlook.Store CreateStoreProxy(
            string displayName,
            Outlook.Folder? rootFolder = null,
            Outlook.OlExchangeStoreType exchangeStoreType =
                Outlook.OlExchangeStoreType.olPrimaryExchangeMailbox
        )
        {
            var resolvedRootFolder = rootFolder ?? CreateFolderProxy($"\\{displayName}");

            return (Outlook.Store)
                new ReflectionRealProxy(
                    typeof(Outlook.Store),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_DisplayName" => displayName,
                            "GetRootFolder" => resolvedRootFolder,
                            "get_ExchangeStoreType" => exchangeStoreType,
                            "GetDefaultFolder" => CreateFolderProxy($"\\{displayName}\\Inbox"),
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
        }

        private static Outlook.Folder CreateFolderProxy(string folderPath)
        {
            return (Outlook.Folder)
                new ReflectionRealProxy(
                    typeof(Outlook.Folder),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_FolderPath" => folderPath,
                            "get_Session" => null,
                            "get_Parent" => null,
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
        }

        private sealed class ControlledSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<(SendOrPostCallback callback, object state)> pendingCallbacks =
                new Queue<(SendOrPostCallback callback, object state)>();

            internal int PendingCallbackCount => pendingCallbacks.Count;

            public override void Post(SendOrPostCallback d, object state)
            {
                pendingCallbacks.Enqueue((d, state));
            }

            internal void RunPostedCallbacks()
            {
                while (pendingCallbacks.Count > 0)
                {
                    var (callback, state) = pendingCallbacks.Dequeue();
                    callback(state);
                }
            }
        }
    }
}
