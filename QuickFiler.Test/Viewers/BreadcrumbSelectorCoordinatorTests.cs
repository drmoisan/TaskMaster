using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first host-neutral selector routing contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbSelectorCoordinatorTests
    {
        private const string AIdentity = "plain:0:A";
        private const string BIdentity = "plain:2:B";

        [TestMethod]
        public void ClosedDown_CommitsNextSelectableAndRaisesOneSelection()
        {
            // Arrange
            Harness harness = CreateHarness();
            int selections = 0;
            harness.Coordinator.SelectionChanged += (sender, args) => selections++;

            // Act
            bool handled = HandleKey(harness.Coordinator, BreadcrumbSelectorKey.Down);

            // Assert
            handled.Should().BeTrue();
            harness.Coordinator.GetSelectedFolder().Should().Be("B");
            selections.Should().Be(1);
        }

        [TestMethod]
        public void OpenDown_ChangesPendingOnlyThenEnterCommitsAndCloses()
        {
            // Arrange
            Harness harness = CreateHarness();
            Counter openTransitions = SubscribeOpenTransitions(harness.Coordinator);
            int selections = 0;
            harness.Coordinator.SelectionChanged += (sender, args) => selections++;

            // Act
            Open(harness.Coordinator).Should().BeTrue();
            HandleKey(harness.Coordinator, BreadcrumbSelectorKey.Down).Should().BeTrue();

            // Assert pending state
            harness.Coordinator.GetSelectedFolder().Should().Be("A");
            Property<string>(harness.Coordinator, "PendingIdentity").Should().Be(BIdentity);
            selections.Should().Be(0);

            // Act and assert commit
            HandleKey(harness.Coordinator, BreadcrumbSelectorKey.Enter).Should().BeTrue();
            harness.Coordinator.GetSelectedFolder().Should().Be("B");
            Property<bool>(harness.Coordinator, "IsSelectorOpen").Should().BeFalse();
            selections.Should().Be(1);
            openTransitions.Value.Should().Be(2, "open and explicit close each publish once");
        }

        [TestMethod]
        public void EscapeAndUncommittedClose_RestoreOpeningSelectionWithoutNotification()
        {
            // Arrange
            Harness harness = CreateHarness();
            int selections = 0;
            harness.Coordinator.SelectionChanged += (sender, args) => selections++;
            Open(harness.Coordinator).Should().BeTrue();
            HandleKey(harness.Coordinator, BreadcrumbSelectorKey.Down).Should().BeTrue();

            // Act
            HandleKey(harness.Coordinator, BreadcrumbSelectorKey.Escape).Should().BeTrue();

            // Assert
            harness.Coordinator.GetSelectedFolder().Should().Be("A");
            Property<bool>(harness.Coordinator, "IsSelectorOpen").Should().BeFalse();
            selections.Should().Be(0);
        }

        [TestMethod]
        public void MouseActivation_CommitsStableIdentityExactlyOnce()
        {
            // Arrange
            Harness harness = CreateHarness();
            int selections = 0;
            harness.Coordinator.SelectionChanged += (sender, args) => selections++;
            Open(harness.Coordinator).Should().BeTrue();

            // Act
            bool changed = InvokeBool(harness.Coordinator, "ActivateSelector", BIdentity);

            // Assert
            changed.Should().BeTrue();
            harness.Coordinator.GetSelectedFolder().Should().Be("B");
            selections.Should().Be(1);
        }

        [TestMethod]
        public void InvalidSelectorMessage_IsNoOpAndDoesNotRaiseTransitions()
        {
            // Arrange
            Harness harness = CreateHarness();
            int selections = 0;
            harness.Coordinator.SelectionChanged += (sender, args) => selections++;

            // Act
            harness.Messenger.Raise(
                messenger => messenger.MessageReceived += null,
                harness.Messenger.Object,
                "{\"type\":\"selectorKey\",\"key\":\"left\"}"
            );
            harness.Coordinator.LastDispatch.GetAwaiter().GetResult();

            // Assert
            harness.Coordinator.GetSelectedFolder().Should().Be("A");
            selections.Should().Be(0);
        }

        [TestMethod]
        public void ExistingLeftAndRightMessages_StillForwardOnce()
        {
            // Arrange
            Harness harness = CreateHarness();
            int synthetic = 0;
            int unhandled = 0;
            harness.Coordinator.FolderArrowKeyDown += (sender, direction) => synthetic++;
            harness.Coordinator.UnhandledArrow += (sender, direction) => unhandled++;

            // Act
            harness.Messenger.Raise(
                messenger => messenger.MessageReceived += null,
                harness.Messenger.Object,
                "{\"type\":\"arrowKey\",\"direction\":\"left\"}"
            );
            harness.Coordinator.LastDispatch.GetAwaiter().GetResult();

            // Assert
            synthetic.Should().Be(1);
            unhandled.Should().Be(1);
        }

        [TestMethod]
        public void TransitionPublicationsAndEvents_RunAfterRouterLockIsReleased()
        {
            // Arrange
            var messenger = new Mock<IWebViewMessenger>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var coordinator = CreateCoordinator(messenger.Object, provider.Object);
            int posts = 0;
            int selections = 0;
            messenger
                .Setup(value => value.PostJson(It.IsAny<string>()))
                .Callback(() =>
                {
                    AssertRouterAvailable(coordinator);
                    posts++;
                });
            coordinator.SelectionChanged += (sender, args) =>
            {
                AssertRouterAvailable(coordinator);
                selections++;
            };

            // Act
            coordinator.AddItems(new[] { "A" });
            coordinator.SelectRow(0);

            // Assert
            posts.Should().Be(2);
            selections.Should().Be(1);
        }

        [TestMethod]
        public void SelectorView_ContainsRowAlignedStableIdentityAndSelectabilityOptions()
        {
            // Arrange
            using (var hub = new BreadcrumbMessengerHub())
            {
                var posted = new List<string>();
                var surface = new Mock<IWebViewMessenger>();
                surface
                    .Setup(messenger => messenger.PostJson(It.IsAny<string>()))
                    .Callback<string>(posted.Add);
                hub.Attach(surface.Object, BreadcrumbSelectorViewMode.Collapsed);
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                var coordinator = CreateCoordinator(hub, provider.Object);

                // Act
                coordinator.AddItems(new[] { "A", "===== label =====", "B" });
                coordinator.SelectRow(0);

                // Assert
                string selectorView = posted.Last(json =>
                    json.Contains("\"type\":\"selectorView\"")
                );
                selectorView
                    .Should()
                    .Contain(
                        "\"options\":[{\"identity\":\"plain:0:A\",\"isSelectable\":true},"
                            + "{\"identity\":\"plain:1:===== label =====\",\"isSelectable\":false},"
                            + "{\"identity\":\"plain:2:B\",\"isSelectable\":true}]"
                    );
                selectorView.Should().Contain("\"committedIdentity\":\"plain:0:A\"");
            }
        }

        [TestMethod]
        public void BoundaryAndInvalidOperations_AreDeterministicNoOps()
        {
            // Arrange
            Harness harness = CreateHarness();

            // Act
            bool previousAtFirst = HandleKey(harness.Coordinator, BreadcrumbSelectorKey.Up);
            bool enterWhileClosed = HandleKey(harness.Coordinator, BreadcrumbSelectorKey.Enter);
            bool invalidKey = HandleKey(harness.Coordinator, (BreadcrumbSelectorKey)999);
            bool invalidActivation = InvokeBool(
                harness.Coordinator,
                "ActivateSelector",
                "missing-folder"
            );

            // Assert
            previousAtFirst.Should().BeFalse();
            enterWhileClosed.Should().BeFalse();
            invalidKey.Should().BeFalse();
            invalidActivation.Should().BeFalse();
            harness.Coordinator.CommittedIdentity.Should().Be(AIdentity);
        }

        [TestMethod]
        public void OpenTwiceThenClear_RejectsSecondOpenAndPublishesOneCloseTransition()
        {
            // Arrange
            Harness harness = CreateHarness();
            Counter transitions = SubscribeOpenTransitions(harness.Coordinator);
            Open(harness.Coordinator).Should().BeTrue();

            // Act
            bool openedTwice = Open(harness.Coordinator);
            harness.Coordinator.Clear();

            // Assert
            openedTwice.Should().BeFalse();
            harness.Coordinator.GetFolderItems().Should().BeEmpty();
            harness.Coordinator.IsSelectorOpen.Should().BeFalse();
            transitions.Value.Should().Be(2, "open and clear-close each publish once");
        }

        [TestMethod]
        public void InboundValidSelectorMessages_RouteToggleKeyAndActivationBranches()
        {
            // Arrange
            Harness harness = CreateHarness();

            // Act and assert toggle open
            Receive(harness, "{\"type\":\"selectorToggle\"}");
            harness.Coordinator.IsSelectorOpen.Should().BeTrue();

            // Act and assert toggle cancellation
            Receive(harness, "{\"type\":\"selectorToggle\"}");
            harness.Coordinator.IsSelectorOpen.Should().BeFalse();

            // Act and assert key and activation routing
            Receive(harness, "{\"type\":\"selectorKey\",\"key\":\"down\"}");
            Receive(harness, "{\"type\":\"selectorActivate\",\"identity\":\"plain:0:A\"}");
            harness.Coordinator.GetSelectedFolder().Should().Be("A");
        }

        [TestMethod]
        public void InboundSelectorKeyUp_MovesPendingPastSeparatorAndClampsWithoutDuplicatePublication()
        {
            // Arrange
            using (var hub = new BreadcrumbMessengerHub())
            {
                var posted = new List<string>();
                var surface = new Mock<IWebViewMessenger>();
                surface
                    .Setup(messenger => messenger.PostJson(It.IsAny<string>()))
                    .Callback<string>(posted.Add);
                hub.Attach(surface.Object, BreadcrumbSelectorViewMode.Expanded);
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                var coordinator = CreateCoordinator(hub, provider.Object);
                coordinator.AddItems(new[] { "A", "===== label =====", "B" });
                coordinator.SelectRow(2);
                int selectionPublications = 0;
                coordinator.SelectionChanged += (sender, args) => selectionPublications++;
                coordinator.GetFolderItems().Should().Equal("A", "===== label =====", "B");
                coordinator.OpenSelector().Should().BeTrue();
                int selectorViewsBeforeUp = posted.Count(json =>
                    json.Contains("\"type\":\"selectorView\"")
                );

                // Act
                surface.Raise(
                    messenger => messenger.MessageReceived += null,
                    surface.Object,
                    "{\"type\":\"selectorKey\",\"key\":\"up\"}"
                );

                // Assert movement skips the separator without committing.
                coordinator.PendingIdentity.Should().Be(AIdentity);
                coordinator.CommittedIdentity.Should().Be(BIdentity);
                coordinator.GetSelectedFolder().Should().Be("B");

                // Act at the first selectable boundary.
                surface.Raise(
                    messenger => messenger.MessageReceived += null,
                    surface.Object,
                    "{\"type\":\"selectorKey\",\"key\":\"up\"}"
                );

                // Assert the clamp produces no duplicate state or selection publication.
                coordinator.PendingIdentity.Should().Be(AIdentity);
                coordinator.CommittedIdentity.Should().Be(BIdentity);
                coordinator.GetSelectedFolder().Should().Be("B");
                posted
                    .Count(json => json.Contains("\"type\":\"selectorView\""))
                    .Should()
                    .Be(selectorViewsBeforeUp + 1);
                selectionPublications.Should().Be(0);
            }
        }

        private static Harness CreateHarness()
        {
            var messenger = new Mock<IWebViewMessenger>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var coordinator = CreateCoordinator(messenger.Object, provider.Object);
            coordinator.AddItems(new[] { "A", "===== label =====", "B" });
            coordinator.SelectRow(0);
            return new Harness(messenger, coordinator);
        }

        private static bool Open(BreadcrumbBridgeCoordinator coordinator) =>
            InvokeBool(coordinator, "OpenSelector");

        private static bool HandleKey(
            BreadcrumbBridgeCoordinator coordinator,
            BreadcrumbSelectorKey key
        ) => InvokeBool(coordinator, "HandleSelectorKey", key);

        private static Counter SubscribeOpenTransitions(BreadcrumbBridgeCoordinator coordinator)
        {
            var counter = new Counter();
            EventInfo eventInfo = coordinator.GetType().GetEvent("SelectorOpenStateChanged");
            eventInfo
                .Should()
                .NotBeNull("issue #400 requires one observable open/close transition");
            eventInfo.AddEventHandler(
                coordinator,
                new EventHandler((sender, args) => counter.Value++)
            );
            return counter;
        }

        private static void Receive(Harness harness, string json)
        {
            harness.Messenger.Raise(
                messenger => messenger.MessageReceived += null,
                harness.Messenger.Object,
                json
            );
            harness.Coordinator.LastDispatch.GetAwaiter().GetResult();
        }

        private static void AssertRouterAvailable(BreadcrumbBridgeCoordinator coordinator)
        {
            Task read = Task.Run(() => coordinator.GetFolderItems());
            read.Wait(TimeSpan.FromSeconds(1))
                .Should()
                .BeTrue("external work must run after the router releases its lock");
        }

        private static BreadcrumbBridgeCoordinator CreateCoordinator(
            IWebViewMessenger messenger,
            IFolderHierarchyProvider provider
        )
        {
            return new BreadcrumbBridgeCoordinator(
                messenger,
                provider,
                BreadcrumbUiDispatcher.CreateForCurrentThreadTests()
            );
        }

        private static bool InvokeBool(object target, string method, params object[] arguments)
        {
            MethodInfo methodInfo = target.GetType().GetMethod(method);
            methodInfo.Should().NotBeNull($"issue #400 requires coordinator method {method}");
            return (bool)methodInfo.Invoke(target, arguments);
        }

        private static T Property<T>(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name);
            property.Should().NotBeNull($"issue #400 requires coordinator property {name}");
            return (T)property.GetValue(target);
        }

        private sealed class Harness
        {
            public Harness(
                Mock<IWebViewMessenger> messenger,
                BreadcrumbBridgeCoordinator coordinator
            )
            {
                Messenger = messenger;
                Coordinator = coordinator;
            }

            public Mock<IWebViewMessenger> Messenger { get; }
            public BreadcrumbBridgeCoordinator Coordinator { get; }
        }

        private sealed class Counter
        {
            public int Value { get; set; }
        }
    }
}
