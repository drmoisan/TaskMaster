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
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first popup initialization, reuse, reset, and disposal contracts.</summary>
    [TestClass]
    public sealed class BreadcrumbDropDownLifecycleTests
    {
        [TestMethod]
        public void OpenAsync_IsLazyUsesSuppliedEnvironmentAndReusesOneSurfaceAcrossOpens()
        {
            // Arrange
            using (var harness = new Harness())
            {
                int ready = 0;
                Subscribe(harness.Host, "PopupMessengerReady", (sender, args) => ready++);
                harness.FactoryCount.Should().Be(0);

                // Act
                harness.Open().Should().BeTrue();
                object firstMessenger = Property<object>(harness.Host, "PopupMessenger");
                harness.Close("ExplicitCommit").Should().BeTrue();
                harness.Open().Should().BeTrue();

                // Assert
                harness.FactoryCount.Should().Be(1);
                harness.SuppliedEnvironment.Should().BeSameAs(harness.Environment);
                Property<object>(harness.Host, "PopupMessenger").Should().BeSameAs(firstMessenger);
                ready.Should().Be(1);
            }
        }

        [TestMethod]
        public void Reset_DisposesSurfaceClearsHostAndAllowsOneFreshInitialization()
        {
            // Arrange
            using (var harness = new Harness())
            {
                harness.Open().Should().BeTrue();
                TrackingControl first = harness.CreatedControls.Single();
                harness.Close("ExplicitCommit").Should().BeTrue();

                // Act
                Invoke(harness.Host, "Reset");

                // Assert reset
                first.WasDisposed.Should().BeTrue();
                Property<ToolStripDropDown>(harness.Host, "DropDown").Items.Count.Should().Be(0);
                Property<object>(harness.Host, "PopupMessenger").Should().BeNull();

                // Act and assert reuse after reset
                harness.Open().Should().BeTrue();
                harness.FactoryCount.Should().Be(2);
            }
        }

        [TestMethod]
        public void OpenAsync_PartialInitializationFailureDisposesAndRestoresFocusAndSelection()
        {
            // Arrange
            var partial = new TrackingControl();
            Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger>>> factory =
                environment =>
                    Task.FromResult(Tuple.Create<Control, IWebViewMessenger>(partial, null));
            using (var harness = new Harness(factory))
            {
                // Act
                bool opened = harness.Open();

                // Assert
                opened.Should().BeFalse();
                partial.WasDisposed.Should().BeTrue();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
                Property<ToolStripDropDown>(harness.Host, "DropDown").Items.Count.Should().Be(0);
            }
        }

        [TestMethod]
        public void Dispose_ClosesUncommittedDisposesSurfaceAndPreventsLaterCallbacks()
        {
            // Arrange
            var harness = new Harness();
            harness.Open().Should().BeTrue();
            TrackingControl control = harness.CreatedControls.Single();

            // Act
            ((IDisposable)harness.Host).Dispose();
            int cancelAfterDispose = harness.CancelCount;
            int focusAfterDispose = harness.FocusAnchorCount;
            bool closedAgain = harness.Close("Uncommitted");

            // Assert
            control.WasDisposed.Should().BeTrue();
            cancelAfterDispose.Should().Be(1);
            focusAfterDispose.Should().Be(1);
            closedAgain.Should().BeFalse();
            harness.CancelCount.Should().Be(cancelAfterDispose);
            harness.FocusAnchorCount.Should().Be(focusAfterDispose);
            harness.DisposeAnchorOnly();
        }

        [TestMethod]
        public void FailedFactoryTask_ClosesWithoutLeavingAHostOrCallbackSubscription()
        {
            // Arrange
            Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger>>> factory =
                environment =>
                    Task.FromException<Tuple<Control, IWebViewMessenger>>(
                        new InvalidOperationException("runtime unavailable")
                    );
            using (var harness = new Harness(factory))
            {
                // Act
                bool opened = harness.Open();

                // Assert
                opened.Should().BeFalse();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                Property<Exception>(harness.Host, "LastInitializationException")
                    .Message.Should()
                    .Be("runtime unavailable");
                Property<ToolStripDropDown>(harness.Host, "DropDown").Items.Count.Should().Be(0);
            }
        }

        private static void Subscribe(object host, string eventName, EventHandler handler) =>
            host.GetType().GetEvent(eventName).AddEventHandler(host, handler);

        private static void Invoke(object host, string method) =>
            host.GetType().GetMethod(method).Invoke(host, null);

        private static T Property<T>(object host, string property) =>
            (T)host.GetType().GetProperty(property).GetValue(host);

        private sealed class Harness : IDisposable
        {
            private readonly Func<
                CoreWebView2Environment,
                Task<Tuple<Control, IWebViewMessenger>>
            > _factory;

            public Harness(
                Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger>>> factory =
                    null
            )
            {
                Type type = typeof(BreadcrumbBridgeCoordinator).Assembly.GetType(
                    "QuickFiler.Viewers.BreadcrumbDropDownHost",
                    false
                );
                type.Should()
                    .NotBeNull("issue #400 requires deterministic popup lifecycle ownership");
                Environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                _factory = factory ?? CreateSurface;
                ConstructorInfo constructor = type.GetConstructors()
                    .Single(candidate =>
                        candidate
                            .GetParameters()
                            .Any(parameter => parameter.ParameterType == _factory.GetType())
                    );
                Host = constructor.Invoke(
                    new object[]
                    {
                        Anchor,
                        Environment,
                        _factory,
                        new Action(() => FocusPendingCount++),
                        new Action(() => FocusAnchorCount++),
                        new Action(() => CancelCount++),
                        new Action<ToolStripDropDown, Control, Point>(
                            (dropDown, owner, point) => { }
                        ),
                    }
                );
            }

            public object Host { get; }
            public Panel Anchor { get; } = new Panel();
            public CoreWebView2Environment Environment { get; }
            public CoreWebView2Environment SuppliedEnvironment { get; private set; }
            public int FactoryCount { get; private set; }
            public int FocusPendingCount { get; private set; }
            public int FocusAnchorCount { get; private set; }
            public int CancelCount { get; private set; }
            public System.Collections.Generic.List<TrackingControl> CreatedControls { get; } =
                new System.Collections.Generic.List<TrackingControl>();

            public bool Open() =>
                (
                    (Task<bool>)
                        Host.GetType()
                            .GetMethod("OpenAsync")
                            .Invoke(
                                Host,
                                new object[]
                                {
                                    new Rectangle(100, 100, 200, 25),
                                    new Rectangle(0, 0, 800, 600),
                                    new Size(300, 200),
                                }
                            )
                )
                    .GetAwaiter()
                    .GetResult();

            public bool Close(string reason)
            {
                MethodInfo method = Host.GetType().GetMethod("Close");
                object value = Enum.Parse(method.GetParameters()[0].ParameterType, reason);
                return (bool)method.Invoke(Host, new[] { value });
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

            private Task<Tuple<Control, IWebViewMessenger>> CreateSurface(
                CoreWebView2Environment environment
            )
            {
                FactoryCount++;
                SuppliedEnvironment = environment;
                var control = new TrackingControl();
                CreatedControls.Add(control);
                return Task.FromResult(
                    Tuple.Create<Control, IWebViewMessenger>(control, new TrackingMessenger())
                );
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
    }
}
