using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first native drop-down ownership and focus contracts for issue #400.</summary>
    [TestClass]
    public sealed partial class BreadcrumbDropDownHostTests
    {
        [TestMethod]
        public void Constructor_OwnsAutoClosingToolStripDropDownWithoutGlobalTopmostForm()
        {
            // Arrange and act
            using (Harness harness = CreateHarness())
            {
                ToolStripDropDown dropDown = Property<ToolStripDropDown>(harness.Host, "DropDown");

                // Assert
                dropDown.AutoClose.Should().BeTrue();
                Property<Control>(harness.Host, "Anchor").Should().BeSameAs(harness.Anchor);
                harness
                    .Host.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Should()
                    .NotContain(field => typeof(Form).IsAssignableFrom(field.FieldType));
            }
        }

        [TestMethod]
        public void OpenAsync_CreatesToolStripControlHostAndUsesCalculatedScreenPlacement()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                var anchorBounds = new Rectangle(750, 100, 40, 25);
                var workingArea = new Rectangle(0, 0, 800, 600);

                // Act
                Open(harness.Host, anchorBounds, workingArea, new Size(300, 200))
                    .Should()
                    .BeTrue();

                // Assert
                ToolStripDropDown dropDown = Property<ToolStripDropDown>(harness.Host, "DropDown");
                dropDown.Items.Count.Should().Be(1);
                dropDown.Items[0].Should().BeOfType<ToolStripControlHost>();
                harness.ShownOwner.Should().BeSameAs(harness.Anchor);
                harness.ShownLocation.Should().Be(new Point(500, 125));
                ((ToolStripControlHost)dropDown.Items[0]).Size.Should().Be(new Size(300, 200));
            }
        }

        [TestMethod]
        public void ExplicitCommitAndUncommittedClose_HaveDistinctCallbacks()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                Rectangle anchor = new Rectangle(100, 100, 200, 25);
                Rectangle work = new Rectangle(0, 0, 800, 600);
                Open(harness.Host, anchor, work, new Size(300, 200)).Should().BeTrue();

                // Act and assert explicit commit
                Close(harness.Host, "ExplicitCommit").Should().BeTrue();
                harness.CancelCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(1);

                // Act and assert uncommitted automatic close semantics
                Open(harness.Host, anchor, work, new Size(300, 200)).Should().BeTrue();
                Close(harness.Host, "Uncommitted").Should().BeTrue();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(2);
            }
        }

        [TestMethod]
        public void OpenAndClose_TransferFocusIntoPendingOptionAndBackToAnchor()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act
                Open(
                        harness.Host,
                        new Rectangle(100, 100, 200, 25),
                        new Rectangle(0, 0, 800, 600),
                        new Size(300, 200)
                    )
                    .Should()
                    .BeTrue();

                // Assert open focus
                harness.FocusPendingCount.Should().Be(1);
                Property<bool>(harness.Host, "IsOpen").Should().BeTrue();

                // Act and assert close focus
                Close(harness.Host, "Uncommitted").Should().BeTrue();
                harness.FocusAnchorCount.Should().Be(1);
                Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
            }
        }

        [TestMethod]
        public void SetTheme_RetainsLatestThemeForTheReusablePopupSurface()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act
                Invoke(harness.Host, "SetTheme", "dark");
                Invoke(harness.Host, "SetTheme", "light");

                // Assert
                Property<string>(harness.Host, "Theme").Should().Be("light");
            }
        }

        [TestMethod]
        public void SetTheme_BlankTheme_RejectsExplicitly()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act
                Action setBlankTheme = () => Invoke(harness.Host, "SetTheme", " ");

                // Assert
                setBlankTheme
                    .Should()
                    .Throw<TargetInvocationException>()
                    .WithInnerException<ArgumentException>();
            }
        }

        [TestMethod]
        public void OpenAsync_WhenAlreadyOpen_FocusesPendingWithoutRecreatingOrShowing()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                Rectangle anchor = new Rectangle(100, 100, 200, 25);
                Rectangle work = new Rectangle(0, 0, 800, 600);
                Open(harness.Host, anchor, work, new Size(300, 200)).Should().BeTrue();

                // Act
                bool reopened = Open(harness.Host, anchor, work, new Size(300, 200));

                // Assert
                reopened.Should().BeTrue();
                harness.FactoryCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(2);
            }
        }

        [TestMethod]
        public void OpenAsync_ZeroWorkingArea_RestoresSelectionAndFocus()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                // Act
                bool opened = Open(
                    harness.Host,
                    new Rectangle(100, 100, 200, 25),
                    Rectangle.Empty,
                    new Size(300, 200)
                );

                // Assert
                opened.Should().BeFalse();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                Property<Exception>(harness.Host, "LastInitializationException")
                    .Should()
                    .BeOfType<InvalidOperationException>();
            }
        }

        [TestMethod]
        public void OpenAsync_ShowFailure_ClosesUncommittedAndRetainsTheFailure()
        {
            // Arrange
            using (
                Harness harness = CreateHarness(
                    (dropDown, owner, point) => throw new InvalidOperationException("show failed")
                )
            )
            {
                // Act
                bool opened = Open(
                    harness.Host,
                    new Rectangle(100, 100, 200, 25),
                    new Rectangle(0, 0, 800, 600),
                    new Size(300, 200)
                );

                // Assert
                opened.Should().BeFalse();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
                Property<Exception>(harness.Host, "LastInitializationException")
                    .Message.Should()
                    .Be("show failed");
            }
        }

        [TestMethod]
        public void NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                Open(
                        harness.Host,
                        new Rectangle(100, 100, 200, 25),
                        new Rectangle(0, 0, 800, 600),
                        new Size(300, 200)
                    )
                    .Should()
                    .BeTrue();
                var args = new ToolStripDropDownClosedEventArgs(
                    ToolStripDropDownCloseReason.AppClicked
                );

                // Act
                InvokePrivate(harness.Host, "OnDropDownClosed", harness.Host, args);
                InvokePrivate(harness.Host, "OnDropDownClosed", harness.Host, args);
                InvokePrivateCloseWhenClosed(harness.Host);

                // Assert
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
            }
        }

        [TestMethod]
        public void ResetAndDispose_HandleOpenOrPartialStateAndRejectLaterUse()
        {
            // Arrange
            Harness harness = CreateHarness();
            Open(
                    harness.Host,
                    new Rectangle(100, 100, 200, 25),
                    new Rectangle(0, 0, 800, 600),
                    new Size(300, 200)
                )
                .Should()
                .BeTrue();

            // Act
            Invoke(harness.Host, "Reset");
            ((IDisposable)harness.Host).Dispose();
            ((IDisposable)harness.Host).Dispose();
            Action resetAfterDispose = () => Invoke(harness.Host, "Reset");

            // Assert
            harness.CancelCount.Should().Be(1);
            harness.FocusAnchorCount.Should().Be(1);
            resetAfterDispose
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ObjectDisposedException>();
            harness.DisposeAnchorOnly();
        }

        [TestMethod]
        public void Reset_DisposesAnOrphanedPartialSurface()
        {
            // Arrange
            using (Harness harness = CreateHarness())
            {
                var control = new TrackingControl();
                var messenger = new DisposableTrackingMessenger();
                SetField(harness.Host, "_popupControl", control);
                SetField(harness.Host, "_popupMessenger", messenger);

                // Act
                Invoke(harness.Host, "Reset");

                // Assert
                control.WasDisposed.Should().BeTrue();
                messenger.WasDisposed.Should().BeTrue();
            }
        }

        [TestMethod]
        public void ProductionConstructor_RejectsMissingInitializerOrHtml()
        {
            // Arrange
            using (var anchor = new Panel())
            {
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                var initializer = new Mock<IWebViewCoreInitializer>(MockBehavior.Strict).Object;
                Action noOp = () => { };

                // Act
                Action missingInitializer = () =>
                    new BreadcrumbDropDownHost(anchor, environment, null, "html", noOp, noOp, noOp);
                Action missingHtml = () =>
                    new BreadcrumbDropDownHost(
                        anchor,
                        environment,
                        initializer,
                        null,
                        noOp,
                        noOp,
                        noOp
                    );

                // Assert
                missingInitializer.Should().Throw<ArgumentNullException>();
                missingHtml.Should().Throw<ArgumentNullException>();
            }
        }

        private static Harness CreateHarness(Action<ToolStripDropDown, Control, Point> show = null)
        {
            Type type = typeof(BreadcrumbBridgeCoordinator).Assembly.GetType(
                "QuickFiler.Viewers.BreadcrumbDropDownHost",
                false
            );
            type.Should().NotBeNull("issue #400 requires an owned native drop-down host");
            var harness = new Harness(type, show);
            harness.Create();
            return harness;
        }

        private static bool Open(object host, Rectangle anchor, Rectangle work, Size desired) =>
            (
                (Task<bool>)
                    host.GetType()
                        .GetMethod("OpenAsync")
                        .Invoke(host, new object[] { anchor, work, desired })
            )
                .GetAwaiter()
                .GetResult();

        private static bool Close(object host, string reason)
        {
            MethodInfo method = host.GetType().GetMethod("Close");
            object value = Enum.Parse(method.GetParameters()[0].ParameterType, reason);
            return (bool)method.Invoke(host, new[] { value });
        }

        private static void Invoke(object host, string method, params object[] arguments) =>
            host.GetType().GetMethod(method).Invoke(host, arguments);

        private static void InvokePrivate(object host, string method, params object[] arguments) =>
            host.GetType()
                .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(host, arguments);

        private static void InvokePrivateCloseWhenClosed(object host)
        {
            MethodInfo method = host.GetType()
                .GetMethod("CompleteClose", BindingFlags.Instance | BindingFlags.NonPublic);
            object reason = Enum.Parse(method.GetParameters()[0].ParameterType, "Uncommitted");
            method.Invoke(host, new[] { reason, (object)false });
        }

        private static void SetField(object host, string name, object value) =>
            host.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(host, value);

        private static T Property<T>(object host, string property) =>
            (T)host.GetType().GetProperty(property).GetValue(host);

        private sealed class Harness : IDisposable
        {
            private readonly Type _type;
            private readonly Action<ToolStripDropDown, Control, Point> _show;

            public Harness(Type type, Action<ToolStripDropDown, Control, Point> show)
            {
                _type = type;
                _show = show;
            }

            public object Host { get; private set; }
            public Panel Anchor { get; } = new Panel();
            public Point ShownLocation { get; private set; }
            public Control ShownOwner { get; private set; }
            public int FocusPendingCount { get; private set; }
            public int FocusAnchorCount { get; private set; }
            public int CancelCount { get; private set; }
            public int FactoryCount { get; private set; }
            public int ShowCount { get; private set; }

            public void Create()
            {
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger>>> factory =
                    supplied =>
                    {
                        FactoryCount++;
                        return Task.FromResult(
                            Tuple.Create<Control, IWebViewMessenger>(
                                new Panel(),
                                new TrackingMessenger()
                            )
                        );
                    };
                Action<ToolStripDropDown, Control, Point> show = (dropDown, owner, point) =>
                {
                    ShowCount++;
                    _show?.Invoke(dropDown, owner, point);
                    ShownOwner = owner;
                    ShownLocation = point;
                };
                ConstructorInfo constructor = _type
                    .GetConstructors()
                    .Single(candidate =>
                        candidate
                            .GetParameters()
                            .Any(parameter => parameter.ParameterType == factory.GetType())
                    );
                Host = constructor.Invoke(
                    new object[]
                    {
                        Anchor,
                        environment,
                        factory,
                        new Action(() => FocusPendingCount++),
                        new Action(() => FocusAnchorCount++),
                        new Action(() => CancelCount++),
                        show,
                    }
                );
            }

            public void Dispose()
            {
                (Host as IDisposable)?.Dispose();
                DisposeAnchorOnly();
            }

            public void DisposeAnchorOnly()
            {
                if (!Anchor.IsDisposed)
                {
                    Anchor.Dispose();
                }
            }
        }

        private sealed class TrackingControl : Panel
        {
            public bool WasDisposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                WasDisposed = true;
                base.Dispose(disposing);
            }
        }

        private sealed class TrackingMessenger : IWebViewMessenger
        {
            public event EventHandler<string> MessageReceived
            {
                add { }
                remove { }
            }

            public void PostJson(string json) { }
        }

        private sealed class DisposableTrackingMessenger : IWebViewMessenger, IDisposable
        {
            public bool WasDisposed { get; private set; }

            public event EventHandler<string> MessageReceived
            {
                add { }
                remove { }
            }

            public void PostJson(string json) { }

            public void Dispose() => WasDisposed = true;
        }
    }
}
