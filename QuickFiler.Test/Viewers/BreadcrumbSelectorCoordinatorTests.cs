using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            Property<string>(harness.Coordinator, "PendingIdentity").Should().Be("B");
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
            bool changed = InvokeBool(harness.Coordinator, "ActivateSelector", "B");

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
                var coordinator = new BreadcrumbBridgeCoordinator(hub, provider.Object);

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
                        "\"options\":[{\"identity\":\"A\",\"isSelectable\":true},"
                            + "{\"identity\":\"===== label =====\",\"isSelectable\":false},"
                            + "{\"identity\":\"B\",\"isSelectable\":true}]"
                    );
                selectorView.Should().Contain("\"committedIdentity\":\"A\"");
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
            harness.Coordinator.CommittedIdentity.Should().Be("A");
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
            Receive(harness, "{\"type\":\"selectorActivate\",\"identity\":\"A\"}");
            harness.Coordinator.GetSelectedFolder().Should().Be("A");
        }

        private static Harness CreateHarness()
        {
            var messenger = new Mock<IWebViewMessenger>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var coordinator = new BreadcrumbBridgeCoordinator(messenger.Object, provider.Object);
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
