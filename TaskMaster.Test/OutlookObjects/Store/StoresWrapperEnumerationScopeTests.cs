using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Test.AppGlobals;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.Threading;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster.Test.OutlookObjects.Store
{
    /// <summary>
    /// Focused regression tests for issue #292: the raw <c>Namespace.Stores</c> materialization must
    /// run inside an ambient <see cref="CurrentStoreContext"/> phase-identity scope so a stall during
    /// the first <c>IEnumVARIANT::Next()</c> produces a non-blank watchdog attribution, while the
    /// healthy-path included set/order and scope-restore invariants are preserved. Kept in a sibling
    /// file so <c>StoresWrapperTests.cs</c> stays under the 500-line cap. All boundaries use the
    /// existing <see cref="ReflectionRealProxy"/> seams; no live Outlook, no temp files, no waits.
    /// </summary>
    [TestClass]
    public class StoresWrapperEnumerationScopeTests
    {
        // ---- T1 -------------------------------------------------------------------------------

        [TestMethod]
        public void Init_MaterializingStores_ObservesEnumerationPhaseIdentityInsideMoveNext()
        {
            // Arrange: a stores collection whose enumerator records the ambient CurrentStoreContext
            // value on each MoveNext(). The single store is excluded by the filter, so no per-store
            // StoreWrapper.Init COM work runs; only the materialization enumeration is exercised.
            var observed = new List<string>();
            var stores = CreateRecordingStoresProxy(
                observed,
                CreateExcludedStoreProxy("Stalling Store")
            );
            var wrapper = new StoresWrapper(CreateStubGlobals(stores));

            // Act
            wrapper.Init();

            // Assert: the enumeration ran, and every MoveNext observed the enumeration-phase identity
            // (RED on HEAD, where each observation is null because no scope wraps the materialization).
            observed
                .Should()
                .NotBeEmpty("Init must enumerate Namespace.Stores to materialize the filtered set");
            observed
                .Should()
                .OnlyContain(
                    value => value == CurrentStoreContext.StoresEnumerationPhaseIdentity,
                    "each MoveNext during Init's Namespace.Stores materialization must observe the "
                        + "enumeration-phase identity so a stall is attributed instead of blank"
                );
        }

        // ---- T2 -------------------------------------------------------------------------------

        [TestMethod]
        public async Task RewireOlObjectsAsync_MaterializingStores_ObservesEnumerationPhaseIdentityInsideMoveNext()
        {
            // Arrange: same recording seam, driven through the public RewireAfterDeserializeAsync
            // entry so the StoresWrapper.cs:89 materialization is exercised.
            var observed = new List<string>();
            var stores = CreateRecordingStoresProxy(
                observed,
                CreateExcludedStoreProxy("Stalling Store")
            );
            var wrapper = new StoresWrapper(CreateStubGlobals(stores));

            // Act
            await wrapper.RewireAfterDeserializeAsync();

            // Assert: RED on HEAD (observations are null); GREEN once the rewire materialization is
            // wrapped in the enumeration-phase scope.
            observed
                .Should()
                .NotBeEmpty(
                    "RewireOlObjectsAsync must enumerate Namespace.Stores to materialize the filtered set"
                );
            observed
                .Should()
                .OnlyContain(
                    value => value == CurrentStoreContext.StoresEnumerationPhaseIdentity,
                    "each MoveNext during the rewire Namespace.Stores materialization must observe the "
                        + "enumeration-phase identity so a stall is attributed instead of blank"
                );
        }

        // ---- T4 -------------------------------------------------------------------------------

        [TestMethod]
        public void Init_HealthyMultiStore_PreservesIncludedSetAndOrder_AndClearsContextAfterReturn()
        {
            // Arrange: two healthy, included stores in a fixed order.
            var stores = CreateStoresProxy(
                CreateIncludedStoreProxy("Primary Store"),
                CreateIncludedStoreProxy("Archive Store")
            );
            var wrapper = new StoresWrapper(CreateStubGlobals(stores));

            // Act
            wrapper.Init();

            // Assert: identical included set and order (behavior-preserving), and the ambient context
            // is null after Init returns because the materialization scope was disposed. GREEN on HEAD
            // and after the fix.
            wrapper.Stores.Should().HaveCount(2, "the included set and order must be unchanged");
            wrapper
                .Stores.Select(store => store.DisplayName)
                .Should()
                .Equal(new[] { "Primary Store", "Archive Store" });
            CurrentStoreContext
                .Current.Should()
                .BeNull("the materialization scope must be disposed once Init returns");
        }

        // ---- T5 -------------------------------------------------------------------------------

        [TestMethod]
        public void Init_EnumerationThrowsMidStream_LeavesCurrentStoreContextNull()
        {
            // Arrange: a stores collection whose enumerator throws on MoveNext, simulating a COM
            // failure mid-materialization.
            var stores = CreateThrowingStoresProxy();
            var wrapper = new StoresWrapper(CreateStubGlobals(stores));

            // Act
            Action act = () => wrapper.Init();

            // Assert: the exception propagates, and the ambient context is restored to null afterward
            // (the using-scope guarantees restore-on-failure). GREEN on HEAD and after the fix.
            act.Should().Throw<InvalidOperationException>();
            CurrentStoreContext
                .Current.Should()
                .BeNull(
                    "a thrown enumeration must not leak the phase identity into a later attribution"
                );
        }

        // ---- Seams ----------------------------------------------------------------------------

        private static IApplicationGlobals CreateStubGlobals(Outlook.Stores stores)
        {
            return new StubApplicationGlobals(
                new StubFileSystemFolderPaths(),
                CreateOlObjectsProxy(CreateNamespaceProxy(stores))
            );
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

        private static Outlook.NameSpace CreateNamespaceProxy(Outlook.Stores stores)
        {
            return (Outlook.NameSpace)
                new ReflectionRealProxy(
                    typeof(Outlook.NameSpace),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_Stores" => stores,
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
        }

        private static Outlook.Stores CreateRecordingStoresProxy(
            List<string> observed,
            params Outlook.Store[] stores
        )
        {
            return (Outlook.Stores)
                new ReflectionRealProxy(
                    typeof(Outlook.Stores),
                    (method, _) =>
                        method.Name switch
                        {
                            "GetEnumerator" => new ContextRecordingEnumerator(
                                ((IEnumerable)stores).GetEnumerator(),
                                observed
                            ),
                            "get_Count" => stores.Length,
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
        }

        private static Outlook.Stores CreateThrowingStoresProxy()
        {
            return (Outlook.Stores)
                new ReflectionRealProxy(
                    typeof(Outlook.Stores),
                    (method, _) =>
                        method.Name switch
                        {
                            "GetEnumerator" => new ThrowingEnumerator(),
                            "get_Count" => 1,
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

        private static Outlook.Store CreateIncludedStoreProxy(string displayName)
        {
            return CreateStoreProxy(
                displayName,
                Outlook.OlExchangeStoreType.olPrimaryExchangeMailbox
            );
        }

        private static Outlook.Store CreateExcludedStoreProxy(string displayName)
        {
            // A public-folder store is excluded by the default ExcludePublicFolderStores filter, so
            // the enumeration runs without triggering per-store StoreWrapper.Init COM reads.
            return CreateStoreProxy(
                displayName,
                Outlook.OlExchangeStoreType.olExchangePublicFolder
            );
        }

        private static Outlook.Store CreateStoreProxy(
            string displayName,
            Outlook.OlExchangeStoreType exchangeStoreType
        )
        {
            var rootFolder = CreateFolderProxy($"\\{displayName}");

            return (Outlook.Store)
                new ReflectionRealProxy(
                    typeof(Outlook.Store),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_DisplayName" => displayName,
                            "GetRootFolder" => rootFolder,
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

        private sealed class ContextRecordingEnumerator : IEnumerator
        {
            private readonly IEnumerator _inner;
            private readonly List<string> _observed;

            internal ContextRecordingEnumerator(IEnumerator inner, List<string> observed)
            {
                _inner = inner;
                _observed = observed;
            }

            public object Current => _inner.Current;

            public bool MoveNext()
            {
                _observed.Add(CurrentStoreContext.Current);
                return _inner.MoveNext();
            }

            public void Reset() => _inner.Reset();
        }

        private sealed class ThrowingEnumerator : IEnumerator
        {
            public object Current => null;

            public bool MoveNext() =>
                throw new InvalidOperationException("simulated store enumeration failure");

            public void Reset() { }
        }
    }
}
