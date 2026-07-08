using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.Interfaces;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.Test.TestHelpers;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Unit tests for <see cref="StoreDisableService"/> (issue #261). All tests use MSTest + Moq +
    /// FluentAssertions, no live Outlook, no temporary files, and observe persistence through the
    /// existing <c>SmartSerializable</c> injectable-timer seam (no <c>Thread.Sleep</c>/<c>Task.Delay</c>
    /// /real timer). Serialization is observed as a single deferred-write request via a
    /// <see cref="ManualFireTimerWrapper"/> that is never fired, so no write ever reaches disk.
    /// </summary>
    [TestClass]
    public class StoreDisableServiceTests
    {
        private const string StoreName = "MyStore";

        // ---- DisableSessionOnly -------------------------------------------------------------

        [TestMethod]
        public void DisableSessionOnly_AddsToSessionSetOnly_AndDoesNotPersist()
        {
            var (model, timer) = CreateModel();
            var service = CreateService(model);

            service.DisableSessionOnly(StoreIdentity.Resolve(StoreName));

            model.SessionDisabledStoreIdentities.Should().Contain(StoreName);
            model.DisabledStoreIdentities.Should().BeEmpty();
            timer.StartCount.Should().Be(0, "session-only disable must not request serialization");
            service.IsDisabled(StoreIdentity.Resolve(StoreName)).Should().BeTrue();
        }

        [TestMethod]
        public void DisableSessionOnly_CalledTwice_IsIdempotent()
        {
            var (model, timer) = CreateModel();
            var service = CreateService(model);

            service.DisableSessionOnly(StoreIdentity.Resolve(StoreName));
            service.DisableSessionOnly(StoreIdentity.Resolve(StoreName));

            model.SessionDisabledStoreIdentities.Should().ContainSingle();
            timer.StartCount.Should().Be(0);
        }

        // ---- DisableForFutureSessions -------------------------------------------------------

        [TestMethod]
        public void DisableForFutureSessions_AddsToPersistedList_AndSerializesOnce()
        {
            var (model, timer) = CreateModel();
            var service = CreateService(model);

            service.DisableForFutureSessions(StoreIdentity.Resolve(StoreName));

            model.DisabledStoreIdentities.Should().Contain(StoreName);
            timer.StartCount.Should().Be(1, "persistent disable must request serialization once");
        }

        [TestMethod]
        public void DisableForFutureSessions_RendersStoreDisabledForCurrentSessionViaUnion()
        {
            var (model, _) = CreateModel();
            var service = CreateService(model);

            service.DisableForFutureSessions(StoreIdentity.Resolve(StoreName));

            // The persisted list participates in the effective (union) disabled set, so the store is
            // disabled for the current session with no session-set write.
            model.SessionDisabledStoreIdentities.Should().BeEmpty();
            service.IsDisabled(StoreIdentity.Resolve(StoreName)).Should().BeTrue();
        }

        [TestMethod]
        public void DisableForFutureSessions_CalledTwice_NoDuplicateAndNoSecondSerialize()
        {
            var (model, timer) = CreateModel();
            var service = CreateService(model);

            service.DisableForFutureSessions(StoreIdentity.Resolve(StoreName));
            service.DisableForFutureSessions(StoreIdentity.Resolve(StoreName));

            model
                .DisabledStoreIdentities.Count(x =>
                    string.Equals(x, StoreName, StringComparison.OrdinalIgnoreCase)
                )
                .Should()
                .Be(1);
            timer
                .StartCount.Should()
                .Be(1, "a duplicate persistent disable must not serialize again");
        }

        // ---- ReenableAsync ------------------------------------------------------------------

        [TestMethod]
        public async Task ReenableAsync_WhenDisabledInBothScopes_ClearsBothAndSerializesOnce()
        {
            var (model, timer) = CreateModel();
            model.DisabledStoreIdentities.Add(StoreName);
            model.SessionDisabledStoreIdentities.Add(StoreName);

            var rehook = new Mock<IStoreRehookService>();
            var clearedBeforeRehook = false;
            rehook
                .Setup(x => x.RehookAsync(It.IsAny<StoreIdentity>()))
                .Returns(Task.CompletedTask)
                .Callback<StoreIdentity>(_ =>
                    clearedBeforeRehook =
                        model.SessionDisabledStoreIdentities.Count == 0
                        && model.DisabledStoreIdentities.Count == 0
                );

            var service = CreateService(model, rehook.Object);

            await service.ReenableAsync(StoreIdentity.Resolve(StoreName));

            model.SessionDisabledStoreIdentities.Should().BeEmpty();
            model.DisabledStoreIdentities.Should().BeEmpty();
            timer
                .StartCount.Should()
                .Be(1, "clearing the persisted list must serialize exactly once");
            clearedBeforeRehook.Should().BeTrue("rehook must be awaited AFTER state is cleared");
            rehook.Verify(x => x.RehookAsync(It.IsAny<StoreIdentity>()), Times.Once);
        }

        [TestMethod]
        public async Task ReenableAsync_WhenNotDisabled_SerializesZeroTimesButStillAwaitsRehook()
        {
            var (model, timer) = CreateModel();
            var rehook = new Mock<IStoreRehookService>();
            rehook.Setup(x => x.RehookAsync(It.IsAny<StoreIdentity>())).Returns(Task.CompletedTask);
            var service = CreateService(model, rehook.Object);

            await service.ReenableAsync(StoreIdentity.Resolve(StoreName));

            timer.StartCount.Should().Be(0, "a non-disabled reenable must not serialize");
            rehook.Verify(x => x.RehookAsync(It.IsAny<StoreIdentity>()), Times.Once);
        }

        [TestMethod]
        public async Task ReenableAsync_WithNoOpDefaultRehook_LeavesStateClearedAndCompletes()
        {
            var (model, _) = CreateModel();
            model.DisabledStoreIdentities.Add(StoreName);
            model.SessionDisabledStoreIdentities.Add(StoreName);
            var service = CreateService(model); // default NoOpStoreRehookService

            await service.ReenableAsync(StoreIdentity.Resolve(StoreName));

            model.SessionDisabledStoreIdentities.Should().BeEmpty();
            model.DisabledStoreIdentities.Should().BeEmpty();
        }

        // ---- IsDisabled / GetDisabledStores -------------------------------------------------

        [TestMethod]
        [DataRow("mystore")]
        [DataRow("MYSTORE")]
        public void IsDisabled_IsCaseInsensitive_AcrossBothScopes(string lookup)
        {
            var (sessionModel, _) = CreateModel();
            sessionModel.SessionDisabledStoreIdentities.Add(StoreName);
            var sessionService = CreateService(sessionModel);
            sessionService.IsDisabled(StoreIdentity.Resolve(lookup)).Should().BeTrue();

            var futureModel = CreateModel().model;
            futureModel.DisabledStoreIdentities.Add(StoreName);
            var futureService = CreateService(futureModel);
            futureService.IsDisabled(StoreIdentity.Resolve(lookup)).Should().BeTrue();
        }

        [TestMethod]
        public void GetDisabledStores_ReportsScopes_AndDeDuplicatesBothScopesAsFutureSessions()
        {
            var (model, _) = CreateModel();
            model.DisabledStoreIdentities.Add("PersistedStore");
            model.SessionDisabledStoreIdentities.Add("SessionStore");
            model.DisabledStoreIdentities.Add("BothStore");
            model.SessionDisabledStoreIdentities.Add("BothStore");
            var service = CreateService(model);

            var entries = service.GetDisabledStores();

            entries.Should().HaveCount(3);
            entries
                .Single(e => e.Identity.Value == "PersistedStore")
                .Scope.Should()
                .Be(DisableScope.FutureSessions);
            entries
                .Single(e => e.Identity.Value == "SessionStore")
                .Scope.Should()
                .Be(DisableScope.SessionOnly);
            entries
                .Single(e => e.Identity.Value == "BothStore")
                .Scope.Should()
                .Be(DisableScope.FutureSessions, "the persisted scope is the stronger scope");
        }

        // ---- Identity validation ------------------------------------------------------------

        [TestMethod]
        public void Writes_ThrowArgumentException_ForSentinelIdentity()
        {
            var (model, _) = CreateModel();
            var service = CreateService(model);
            var sentinel = StoreIdentity.Resolve(null, null);

            service
                .Invoking(s => s.DisableSessionOnly(sentinel))
                .Should()
                .Throw<ArgumentException>();
            service
                .Invoking(s => s.DisableForFutureSessions(sentinel))
                .Should()
                .Throw<ArgumentException>();
            service
                .Invoking(s => s.ReenableAsync(sentinel))
                .Should()
                .ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public void Writes_ThrowArgumentException_ForDefaultUnresolvedIdentity()
        {
            var (model, _) = CreateModel();
            var service = CreateService(model);
            var unresolved = default(StoreIdentity); // Value is null

            service
                .Invoking(s => s.DisableSessionOnly(unresolved))
                .Should()
                .Throw<ArgumentException>();
        }

        // ---- Null-model safety --------------------------------------------------------------

        [TestMethod]
        public void Writes_ThrowInvalidOperation_WhenModelIsNull()
        {
            var service = CreateService(model: null);

            service
                .Invoking(s => s.DisableSessionOnly(StoreIdentity.Resolve(StoreName)))
                .Should()
                .Throw<InvalidOperationException>();
            service
                .Invoking(s => s.DisableForFutureSessions(StoreIdentity.Resolve(StoreName)))
                .Should()
                .Throw<InvalidOperationException>();
            service
                .Invoking(s => s.ReenableAsync(StoreIdentity.Resolve(StoreName)))
                .Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [TestMethod]
        public void Reads_AreSafeAndEmpty_WhenModelIsNull()
        {
            var service = CreateService(model: null);

            service.IsDisabled(StoreIdentity.Resolve(StoreName)).Should().BeFalse();
            service.GetDisabledStores().Should().NotBeNull().And.BeEmpty();
        }

        // ---- Harness ------------------------------------------------------------------------

        private static StoreDisableService CreateService(
            StoresWrapper model,
            IStoreRehookService rehook = null
        )
        {
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.StoresWrapper).Returns(model);
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return new StoreDisableService(globals.Object, rehook);
        }

        /// <summary>
        /// Creates a serialization-observable store model: a <see cref="TestableStoresWrapper"/> whose
        /// injectable timer factory returns a manual (never-fired) timer, and whose
        /// <c>Config.Disk.FilePath</c> is non-empty so <c>Serialize()</c> proceeds to request a
        /// deferred write. The returned timer's <see cref="ManualFireTimerWrapper.StartCount"/> counts
        /// serialization requests.
        /// </summary>
        private static (TestableStoresWrapper model, ManualFireTimerWrapper timer) CreateModel()
        {
            var timer = new ManualFireTimerWrapper();
            var model = new TestableStoresWrapper();
            model.SetTimerFactory(_ => timer);
            model.Config.Disk.FilePath = @"C:\Smart\stores.json";
            return (model, timer);
        }

        private sealed class TestableStoresWrapper : StoresWrapper
        {
            public void SetTimerFactory(Func<TimeSpan, ITimerWrapper> timerFactory) =>
                TimerFactory = timerFactory;
        }
    }
}
