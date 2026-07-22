using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;
using CapturingContext = QuickFiler.Test.Viewers.BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext;
using Messenger = QuickFiler.Viewers.IWebViewMessenger;
using OpenLifetime = QuickFiler.Viewers.BreadcrumbDropDownOpenLifetime;
using PopupHost = QuickFiler.Viewers.BreadcrumbDropDownHost;
using PopupOperations = QuickFiler.Viewers.BreadcrumbPopupUiOperations;
using WebEnvironment = Microsoft.Web.WebView2.Core.CoreWebView2Environment;

namespace QuickFiler.Test.Viewers
{
    [TestClass]
    public sealed class BreadcrumbDropDownLifecycleCoverageTests
    {
        private readonly LifecycleHarness _harness = new LifecycleHarness();

        [TestCleanup]
        public void Cleanup() => _harness.Dispose();

        [TestMethod]
        public void OpenLifetime_SharedOpenWithoutPlacement_CompletesFalseAndCleansSurface()
        {
            _harness.ReadyAction = _harness.InvalidateOnFirstPlacement;
            Task<bool> first = _harness.OpenAsync();
            Task<bool> shared = _harness.OpenAsync();
            shared.Should().BeSameAs(first);
            _harness.Drain(first).Should().BeFalse();
            _harness.FactoryCount.Should().Be(1);
            _harness.ShowCount.Should().Be(0);
            _harness.FocusPendingCount.Should().Be(0);
            AssertSurfaceDisposed(_harness);
            AssertClosedWithoutSurface(_harness);
        }

        [TestMethod]
        public void OpenLifetime_ScheduleOverloads_RunSuccessAndContainReportedFaults()
        {
            var actionFailure = new InvalidOperationException("action");
            var taskFailure = new InvalidOperationException("task");
            int actionRuns = 0;
            int taskRuns = 0;
            _harness.Lifetime.Schedule(() => actionRuns++);
            _harness.Lifetime.Schedule((Action)(() => throw actionFailure));
            _harness.Lifetime.Schedule(() => Task.FromResult(taskRuns++));
            _harness.Lifetime.Schedule((Func<Task>)(() => throw taskFailure));
            _harness.Context.DrainAll();
            actionRuns.Should().Be(1);
            taskRuns.Should().Be(1);
            Exception[] errors = _harness.ErrorSnapshot;
            errors.Should().HaveCount(2);
            errors.Should().Contain(error => ReferenceEquals(error, actionFailure));
            errors.Should().Contain(error => ReferenceEquals(error, taskFailure));
        }

        [TestMethod]
        public void OpenLifetime_DisposeIsIdempotentAndSuppressesLaterSchedules()
        {
            var lifetime = new OpenLifetime(_harness.Host, _harness.Operations);
            int executions = 0;
            lifetime.Dispose();
            lifetime.Dispose();
            lifetime.Schedule(() => executions++);
            lifetime.Schedule(() => Task.FromResult(executions++));
            _harness.Context.DrainAll();
            executions.Should().Be(0);
            _harness.ErrorSnapshot.Should().BeEmpty();
        }

        [TestMethod]
        public void OpenLifetime_RollbackReporterFailure_IsContainedAndPrimaryIsRetained()
        {
            var primary = new InvalidOperationException("factory");
            var rollback = new InvalidOperationException("rollback");
            var focus = new InvalidOperationException("focus");
            _harness.FactoryFailure = primary;
            _harness.CancelAction = () => throw rollback;
            _harness.FocusAnchorAction = () => throw focus;
            _harness.ThrowFromErrorSink = true;
            _harness.Open().Should().BeFalse();
            _harness.Host.LastInitializationException.Should().BeSameAs(primary);
            _harness.CancelCount.Should().Be(1);
            _harness.FocusAnchorCount.Should().Be(1);
            _harness.ErrorSnapshot.Should().Equal(focus, rollback);
        }

        [TestMethod]
        public void OpenLifetime_StaleAndFailedRetention_CleansEachSurfaceExactlyOnce()
        {
            _harness.ReadyAction = _harness.Host.Reset;
            _harness.Open().Should().BeFalse();
            AssertSurfaceDisposed(_harness);
            AssertClosedWithoutSurface(_harness);
            using (var failed = new LifecycleHarness())
            {
                var failure = new InvalidOperationException("ready event");
                failed.ReadyAction = () => throw failure;
                failed.Open().Should().BeFalse();
                failed.Host.LastInitializationException.Should().BeSameAs(failure);
                failed.ErrorSnapshot.Should().ContainSingle().Which.Should().BeSameAs(failure);
                AssertSurfaceDisposed(failed);
                AssertClosedWithoutSurface(failed);
            }
        }

        [TestMethod]
        public void Host_FourForwardingConstructors_CreateWithoutInvokingSurfaceAdapters()
        {
            IWebViewCoreInitializer initializer = new Mock<IWebViewCoreInitializer>().Object;
            ConstructorInfo[] constructors = typeof(PopupHost)
                .GetConstructors(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                )
                .Where(candidate =>
                    candidate.GetParameters().Length == 7
                    || candidate.GetParameters().Length == 8
                        && candidate.GetParameters()[2].ParameterType
                            == typeof(IWebViewCoreInitializer)
                )
                .ToArray();
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(_harness.Context);
                foreach (ConstructorInfo constructor in constructors)
                {
                    object[] arguments = constructor
                        .GetParameters()
                        .Select(parameter =>
                            _harness.Argument(parameter.ParameterType, initializer)
                        )
                        .ToArray();
                    ((PopupHost)constructor.Invoke(arguments)).Dispose();
                }
                constructors.Should().HaveCount(4);
            }
            finally
            {
                _harness.Context.DrainAll();
                SynchronizationContext.SetSynchronizationContext(previous);
            }
            _harness.ErrorSnapshot.Should().BeEmpty();
        }

        [TestMethod]
        public void Host_InstalledMessengerAndAlreadyOpenPath_ReuseAndFocusCurrentSurface()
        {
            _harness.Open().Should().BeTrue();
            _harness.Host.InstalledPopupMessenger.Should().BeSameAs(_harness.Messenger);
            _harness.Host.PopupMessenger.Should().BeSameAs(_harness.Messenger);
            _harness.Open().Should().BeTrue();
            _harness.FactoryCount.Should().Be(1);
            _harness.ShowCount.Should().Be(1);
            _harness.FocusPendingCount.Should().Be(2);
        }

        [TestMethod]
        public void Host_CloseFalseTrueReasonsAndRepeatedClose_HaveExactCallbacks()
        {
            _harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted).Should().BeFalse();
            _harness.Open().Should().BeTrue();
            _harness.Close(BreadcrumbDropDownCloseReason.ExplicitCommit).Should().BeTrue();
            _harness.Host.Close(BreadcrumbDropDownCloseReason.ExplicitCommit).Should().BeFalse();
            _harness.CancelCount.Should().Be(0);
            _harness.FocusAnchorCount.Should().Be(1);
            _harness.Open().Should().BeTrue();
            _harness.Close(BreadcrumbDropDownCloseReason.Uncommitted).Should().BeTrue();
            _harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted).Should().BeFalse();
            _harness.CancelCount.Should().Be(1);
            _harness.FocusAnchorCount.Should().Be(2);
            _harness.NativeCloseCount.Should().Be(2);
        }

        [TestMethod]
        public void Host_SetTheme_ValidAndBlankValues_FollowExactContract()
        {
            _harness.Open().Should().BeTrue();
            _harness.Host.SetTheme("dark");
            _harness.Host.Theme.Should().Be("dark");
            Throws<ArgumentException>(() => _harness.Host.SetTheme(" "))
                .ParamName.Should()
                .Be("theme");
        }

        [TestMethod]
        public void Host_DisposeAndUseAfterDispose_FollowDeterministicContract()
        {
            _harness.Open().Should().BeTrue();
            _harness.DisposeHost();
            _harness.DisposeHost();
            AssertSurfaceDisposed(_harness);
            _harness.Host.DropDown.IsDisposed.Should().BeTrue();
            Throws<ObjectDisposedException>(_harness.Host.Reset);
            Throws<ObjectDisposedException>(() => _harness.Host.SetTheme("light"));
            Throws<ObjectDisposedException>(() =>
                _harness.Host.OpenAsync(Rectangle.Empty, Rectangle.Empty, Size.Empty)
            );
            _harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted).Should().BeFalse();
        }

        [TestMethod]
        public void Host_NativeClosedCallback_CancelsOnceAndIgnoresRepeatedNotification()
        {
            _harness.Open().Should().BeTrue();
            RaiseNativeClosed(_harness.Host);
            _harness.Context.DrainAll();
            RaiseNativeClosed(_harness.Host);
            _harness.Context.DrainAll();
            _harness.Host.IsOpen.Should().BeFalse();
            _harness.CancelCount.Should().Be(1);
            _harness.FocusAnchorCount.Should().Be(1);
            _harness.NativeCloseCount.Should().Be(0);
        }

        [TestMethod]
        public void Host_CoreConstructorNullDependencies_UseExactParameterContracts()
        {
            Throws<ArgumentNullException>(() => new OpenLifetime(null, _harness.Operations))
                .ParamName.Should()
                .Be("host");
            Throws<ArgumentNullException>(() => new OpenLifetime(_harness.Host, null))
                .ParamName.Should()
                .Be("uiOperations");
            ConstructorInfo constructor = typeof(PopupHost)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(candidate => candidate.GetParameters().Length == 9);
            ParameterInfo[] parameters = constructor.GetParameters();
            object[] arguments = parameters
                .Select(parameter => _harness.Argument(parameter.ParameterType, null))
                .ToArray();
            for (int index = 0; index < arguments.Length; index++)
            {
                object retained = arguments[index];
                arguments[index] = null;
                TargetInvocationException thrown = Throws<TargetInvocationException>(() =>
                    constructor.Invoke(arguments)
                );
                ((ArgumentNullException)thrown.InnerException)
                    .ParamName.Should()
                    .Be(parameters[index].Name);
                arguments[index] = retained;
            }
        }

        private static readonly MethodInfo ClosedCallback = typeof(PopupHost).GetMethod(
            "OnDropDownClosed",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        private static readonly object[] ClosedArguments =
        {
            null,
            new ToolStripDropDownClosedEventArgs(ToolStripDropDownCloseReason.AppClicked),
        };

        private static void RaiseNativeClosed(PopupHost host) =>
            ClosedCallback.Invoke(host, ClosedArguments);

        private static void AssertSurfaceDisposed(LifecycleHarness harness)
        {
            harness.SurfaceDisposeCount.Should().Be(1);
            harness.MessengerDisposeCount.Should().Be(1);
        }

        private static void AssertClosedWithoutSurface(LifecycleHarness harness)
        {
            harness.Host.IsOpen.Should().BeFalse();
            harness.Host.ControlHost.Should().BeNull();
            harness.Host.PopupMessenger.Should().BeNull();
            harness.Host.DropDown.Items.Count.Should().Be(0);
        }

        private static T Uninitialized<T>()
            where T : class => (T)FormatterServices.GetUninitializedObject(typeof(T));

        private static T Throws<T>(Action operation)
            where T : Exception => operation.Should().Throw<T>().Which;

        private sealed class LifecycleHarness : IDisposable
        {
            private readonly ConcurrentQueue<Exception> _errors = new ConcurrentQueue<Exception>();

            internal LifecycleHarness()
            {
                Context = new CapturingContext();
                Anchor = new Panel();
                Surface.Disposed += (sender, args) => SurfaceDisposeCount++;
                var messenger = new Mock<Messenger>();
                messenger
                    .As<IDisposable>()
                    .Setup(value => value.Dispose())
                    .Callback(() => MessengerDisposeCount++);
                Messenger = messenger.Object;
                Environment = Uninitialized<WebEnvironment>();
                Operations = new PopupOperations(new BreadcrumbUiDispatcher(Context, Report));
                Host = new PopupHost(
                    Anchor,
                    Environment,
                    CreateSurfaceAsync,
                    FocusPending,
                    FocusAnchor,
                    CancelSelection,
                    ShowPopup,
                    Operations,
                    ClosePopup
                );
                Host.PopupMessengerReady += OnReady;
                Lifetime = (OpenLifetime)
                    typeof(PopupHost)
                        .GetField("_openLifetime", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(Host);
            }

            internal CapturingContext Context { get; }
            internal Panel Anchor { get; }
            internal WebEnvironment Environment { get; }
            internal PopupOperations Operations { get; }
            internal PopupHost Host { get; }
            internal OpenLifetime Lifetime { get; }
            internal Panel Surface { get; } = new Panel();
            internal Messenger Messenger { get; }
            internal Func<WebEnvironment, Task<Tuple<Control, Messenger, Task>>> Factory =>
                CreateSurfaceAsync;
            internal Func<WebEnvironment, Task<Tuple<Control, Messenger>>> LegacyFactory =>
                environment => Task.FromResult<Tuple<Control, Messenger>>(null);
            internal Action<ToolStripDropDown, Control, Point> Show => ShowPopup;
            internal Action<ToolStripDropDown, ToolStripDropDownCloseReason> CloseAction =>
                ClosePopup;
            internal Exception FactoryFailure { get; set; }
            internal Action ReadyAction { get; set; } = () => { };
            internal Action CancelAction { get; set; } = () => { };
            internal Action FocusAnchorAction { get; set; } = () => { };
            internal bool ThrowFromErrorSink { get; set; }
            internal int FactoryCount { get; private set; }
            internal int ShowCount { get; private set; }
            internal int FocusPendingCount { get; private set; }
            internal int FocusAnchorCount { get; private set; }
            internal int CancelCount { get; private set; }
            internal int NativeCloseCount { get; private set; }
            internal int MessengerDisposeCount { get; private set; }
            internal int SurfaceDisposeCount { get; private set; }
            internal Exception[] ErrorSnapshot => _errors.ToArray();

            internal object Argument(Type type, IWebViewCoreInitializer initializer)
            {
                if (type == typeof(Control))
                    return Anchor;
                if (type == typeof(WebEnvironment))
                    return Environment;
                if (type == typeof(IWebViewCoreInitializer))
                    return initializer;
                if (type == typeof(string))
                    return "html";
                if (type == typeof(Action))
                    return new Action(() => { });
                if (type == typeof(PopupOperations))
                    return Operations;
                if (type == LegacyFactory.GetType())
                    return LegacyFactory;
                if (type == Factory.GetType())
                    return Factory;
                return type == Show.GetType() ? (object)Show : CloseAction;
            }

            internal Task<bool> OpenAsync() =>
                Host.OpenAsync(
                    new Rectangle(120, 240, 390, 25),
                    new Rectangle(0, 0, 1920, 1040),
                    new Size(390, 180)
                );

            internal bool Open() => Drain(OpenAsync());

            internal T Drain<T>(Task<T> operation)
            {
                Context.DrainUntil(operation);
                return operation.GetAwaiter().GetResult();
            }

            internal bool Close(BreadcrumbDropDownCloseReason reason)
            {
                bool closed = Host.Close(reason);
                Context.DrainAll();
                return closed;
            }

            internal void DisposeHost()
            {
                Host.Dispose();
                Context.DrainAll();
            }

            internal void InvalidateOnFirstPlacement() => Surface.SizeChanged += ResetOnSizeChanged;

            public void Dispose()
            {
                Host.PopupMessengerReady -= OnReady;
                DisposeHost();
                if (!Surface.IsDisposed)
                    Surface.Dispose();
                if (MessengerDisposeCount == 0)
                    ((IDisposable)Messenger).Dispose();
                Anchor.Dispose();
            }

            private Task<Tuple<Control, Messenger, Task>> CreateSurfaceAsync(
                WebEnvironment environment
            )
            {
                FactoryCount++;
                return FactoryFailure == null
                    ? Task.FromResult(
                        Tuple.Create<Control, Messenger, Task>(
                            Surface,
                            Messenger,
                            Task.CompletedTask
                        )
                    )
                    : Task.FromException<Tuple<Control, Messenger, Task>>(FactoryFailure);
            }

            private void OnReady(object sender, EventArgs args) => ReadyAction();

            private void ResetOnSizeChanged(object sender, EventArgs args)
            {
                Surface.SizeChanged -= ResetOnSizeChanged;
                Host.Reset();
            }

            private void ShowPopup(ToolStripDropDown popup, Control owner, Point location) =>
                ShowCount++;

            private void ClosePopup(ToolStripDropDown popup, ToolStripDropDownCloseReason reason) =>
                NativeCloseCount++;

            private void FocusPending() => FocusPendingCount++;

            private void FocusAnchor()
            {
                FocusAnchorCount++;
                FocusAnchorAction();
            }

            private void CancelSelection()
            {
                CancelCount++;
                CancelAction();
            }

            private void Report(Exception exception)
            {
                _errors.Enqueue(exception);
                if (ThrowFromErrorSink)
                    throw new InvalidOperationException("error sink");
            }
        }
    }
}
