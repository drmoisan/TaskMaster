using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first composed subfolder-activation contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbSubfolderActivationTests
    {
        [TestMethod]
        public void OpenSelector_SubfolderActivationThenEnter_PublishesAndClosesExactlyOnce()
        {
            AssertDurableActivation(FollowupAction.Enter);
        }

        [TestMethod]
        public void OpenSelector_SubfolderActivationThenEscape_PublishesAndClosesExactlyOnce()
        {
            AssertDurableActivation(FollowupAction.Escape);
        }

        [TestMethod]
        public void OpenSelector_SubfolderActivationThenNativeClose_PublishesAndClosesExactlyOnce()
        {
            AssertDurableActivation(FollowupAction.NativeAutomaticClose);
        }

        [TestMethod]
        public void OpenSelector_InvalidIdentityAndIndexes_DoNotPublishCloseFocusOrMutate()
        {
            // Arrange
            using (var harness = new SubfolderActivationHarness())
            {
                int selectionChanges = 0;
                harness.Coordinator.SelectionChanged += (sender, args) => selectionChanges++;
                harness.OpenSelector();
                string rowIdentity = harness.Coordinator.CommittedIdentity;
                string encodedIdentity = JsonString(rowIdentity);
                var invalidMessages = new[]
                {
                    "{\"type\":\"selectorSubfolderActivate\",\"subfolderIndex\":0}",
                    "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\" \" ,\"subfolderIndex\":0}",
                    "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":\"missing-row\",\"subfolderIndex\":0}",
                    "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":"
                        + encodedIdentity
                        + ",\"subfolderIndex\":-1}",
                    "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":"
                        + encodedIdentity
                        + ",\"subfolderIndex\":1}",
                };

                // Act
                foreach (string message in invalidMessages)
                {
                    harness.Receive(message);
                }

                // Assert
                selectionChanges.Should().Be(0);
                harness.Coordinator.IsSelectorOpen.Should().BeTrue();
                harness
                    .Coordinator.GetSelectedFolder()
                    .Should()
                    .Be(SubfolderActivationHarness.ParentPath);
                harness.CloseReasons.Should().BeEmpty();
                harness.FocusReturnCount.Should().Be(0);
            }
        }

        [TestMethod]
        public void OpenSelector_SubfolderActivationForPlainRow_IsDeterministicNoOp()
        {
            // Arrange
            using (var harness = new SubfolderActivationHarness())
            {
                harness.Coordinator.AddItems(new[] { "Recent without children" });
                harness.Coordinator.SelectRow(1);
                string plainIdentity = harness.Coordinator.CommittedIdentity;
                int selectionChanges = 0;
                harness.Coordinator.SelectionChanged += (sender, args) => selectionChanges++;
                harness.OpenSelector();

                // Act
                harness.Receive(
                    "{\"type\":\"selectorSubfolderActivate\",\"rowIdentity\":"
                        + JsonString(plainIdentity)
                        + ",\"subfolderIndex\":0}"
                );

                // Assert
                selectionChanges.Should().Be(0);
                harness.Coordinator.IsSelectorOpen.Should().BeTrue();
                harness.Coordinator.GetSelectedFolder().Should().Be("Recent without children");
                harness.CloseReasons.Should().BeEmpty();
                harness.FocusReturnCount.Should().Be(0);
            }
        }

        private static void AssertDurableActivation(FollowupAction followup)
        {
            // Arrange
            using (var harness = new SubfolderActivationHarness())
            {
                int selectionChanges = 0;
                harness.Coordinator.SelectionChanged += (sender, args) => selectionChanges++;
                harness.OpenSelector();

                // Act
                harness.Receive(harness.CreateSubfolderActivationJson());
                string readbackAtActivation = harness.Coordinator.GetSelectedFolder();
                bool openAtActivation = harness.Coordinator.IsSelectorOpen;
                int explicitCloseAtActivation = harness.CloseReasons.Count(reason =>
                    reason == BreadcrumbDropDownCloseReason.ExplicitCommit
                );
                int focusReturnAtActivation = harness.FocusReturnCount;
                bool followupHandled = ApplyFollowup(harness, followup);

                // Assert
                readbackAtActivation.Should().Be(SubfolderActivationHarness.SubfolderPath);
                openAtActivation.Should().BeFalse("activation commits and ends the open session");
                explicitCloseAtActivation.Should().Be(1);
                focusReturnAtActivation.Should().Be(1);
                followupHandled
                    .Should()
                    .BeFalse("Enter, Escape, and native close are no-ops after activation");
                harness.Coordinator.IsSelectorOpen.Should().BeFalse();
                harness
                    .Coordinator.GetSelectedFolder()
                    .Should()
                    .Be(SubfolderActivationHarness.SubfolderPath);
                selectionChanges.Should().Be(1);
                harness.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.ExplicitCommit);
                harness.FocusReturnCount.Should().Be(1);
            }
        }

        private static bool ApplyFollowup(
            SubfolderActivationHarness harness,
            FollowupAction followup
        )
        {
            switch (followup)
            {
                case FollowupAction.Enter:
                    return harness.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Enter);
                case FollowupAction.Escape:
                    return harness.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Escape);
                case FollowupAction.NativeAutomaticClose:
                    return harness.RaiseNativeAutomaticClose();
                default:
                    throw new ArgumentOutOfRangeException(nameof(followup), followup, null);
            }
        }

        private static string JsonString(string value)
        {
            value.Should().NotBeNull();
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private enum FollowupAction
        {
            Enter,
            Escape,
            NativeAutomaticClose,
        }
    }

    internal sealed class SubfolderActivationHarness : IDisposable
    {
        internal const string ParentPath = "\\Inbox\\Projects\\Apollo";
        internal const string SubfolderPath = ParentPath + "\\Alpha";

        private readonly SynchronizationContext _previousContext;
        private readonly TrackingSubfolderMessenger _messenger;
        private bool _hostOpen;

        internal SubfolderActivationHarness()
        {
            _previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(
                new InlineTestSynchronizationContext()
            );
            try
            {
                var leafKey = new FolderTreeNodeKey("store-a", "apollo", ParentPath);
                var childKey = new FolderTreeNodeKey("store-a", "alpha", SubfolderPath);
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                provider
                    .Setup(value =>
                        value.ResolveLeafKeyAsync(ParentPath, It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(leafKey);
                provider
                    .Setup(value =>
                        value.GetAncestorChainAsync(leafKey, It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(
                        new[] { new FolderBreadcrumbSegment(leafKey, "Apollo", ParentPath, true) }
                    );
                provider
                    .Setup(value =>
                        value.GetImmediateSubfoldersAsync(leafKey, It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(
                        new[]
                        {
                            new FolderBreadcrumbSegment(childKey, "Alpha", SubfolderPath, false),
                        }
                    );

                Viewer = new QuickFiler.ItemViewer();
                Viewer.InitializeBreadcrumbPipeline(provider.Object);
                Host = new Mock<IBreadcrumbDropDownHost>();
                Host.SetupGet(value => value.IsOpen).Returns(() => _hostOpen);
                Host.SetupGet(value => value.PopupMessenger).Returns((IWebViewMessenger)null);
                Host.Setup(value =>
                        value.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>()
                        )
                    )
                    .Returns(() =>
                    {
                        _hostOpen = true;
                        return Task.FromResult(true);
                    });
                Host.Setup(value => value.Close(It.IsAny<BreadcrumbDropDownCloseReason>()))
                    .Returns<BreadcrumbDropDownCloseReason>(reason =>
                    {
                        if (!_hostOpen)
                        {
                            return false;
                        }
                        _hostOpen = false;
                        CloseReasons.Add(reason);
                        FocusReturnCount++;
                        return true;
                    });
                Viewer.ConfigureBreadcrumbDropDown(
                    Host.Object,
                    () => new Rectangle(120, 240, 390, 25),
                    () => new Rectangle(0, 0, 1920, 1040)
                );
                _messenger = new TrackingSubfolderMessenger();
                Viewer.AttachBreadcrumbMessenger(_messenger);
                Coordinator
                    .SetSuggestionsAsync(
                        new[]
                        {
                            new FolderRow(
                                ParentPath,
                                FolderRowKind.Suggestion,
                                new FolderScore(ParentPath, 1000, 0.73)
                            ),
                        },
                        CancellationToken.None
                    )
                    .GetAwaiter()
                    .GetResult();
                Coordinator.SelectRow(0);
                Receive("{\"type\":\"affordanceToggle\",\"rowIndex\":0}");
            }
            catch
            {
                SynchronizationContext.SetSynchronizationContext(_previousContext);
                throw;
            }
        }

        internal QuickFiler.ItemViewer Viewer { get; }
        internal Mock<IBreadcrumbDropDownHost> Host { get; }
        internal BreadcrumbBridgeCoordinator Coordinator => Viewer.BreadcrumbCoordinator;
        internal List<BreadcrumbDropDownCloseReason> CloseReasons { get; } =
            new List<BreadcrumbDropDownCloseReason>();
        internal int FocusReturnCount { get; private set; }

        internal void OpenSelector()
        {
            Viewer.SetBreadcrumbDropDownState(true);
            Coordinator.IsSelectorOpen.Should().BeTrue();
            _hostOpen.Should().BeTrue();
        }

        internal void Receive(string json)
        {
            _messenger.Receive(json);
            Coordinator.LastDispatch.GetAwaiter().GetResult();
        }

        internal string CreateSubfolderActivationJson()
        {
            Type messageType = typeof(BreadcrumbStateModel).Assembly.GetType(
                "UtilitiesCS.OutlookObjects.Folder.BreadcrumbSelectorSubfolderActivationMessage",
                false
            );
            if (messageType == null)
            {
                return "{\"type\":\"selectionChange\",\"rowIndex\":0,\"subfolderIndex\":0}";
            }

            object message = Activator.CreateInstance(
                messageType,
                Coordinator.CommittedIdentity,
                0
            );
            MethodInfo serialize = typeof(BreadcrumbSelectorMessageSerializer).GetMethod(
                "Serialize"
            );
            return (string)serialize.Invoke(null, new[] { message });
        }

        internal bool RaiseNativeAutomaticClose()
        {
            if (!_hostOpen)
            {
                return Coordinator.CancelSelector();
            }

            _hostOpen = false;
            CloseReasons.Add(BreadcrumbDropDownCloseReason.Uncommitted);
            FocusReturnCount++;
            return Coordinator.CancelSelector();
        }

        public void Dispose()
        {
            try
            {
                Viewer.Dispose();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(_previousContext);
            }
        }

        private sealed class InlineTestSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object state)
            {
                callback(state);
            }

            public override void Send(SendOrPostCallback callback, object state)
            {
                callback(state);
            }
        }
    }

    internal sealed class TrackingSubfolderMessenger : IWebViewMessenger
    {
        private EventHandler<string> _messageReceived;

        public event EventHandler<string> MessageReceived
        {
            add { _messageReceived += value; }
            remove { _messageReceived -= value; }
        }

        public void PostJson(string json) { }

        internal void Receive(string json)
        {
            _messageReceived?.Invoke(this, json);
        }
    }
}
