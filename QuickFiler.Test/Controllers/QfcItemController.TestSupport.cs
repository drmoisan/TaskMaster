using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Shared, reusable test harness for the cycle-2 de-exemption work (research §3.6, §6). Exposes
    /// the protected parameterless <see cref="QfcItemController"/> constructor and reflection-based
    /// private-field injection so the previously-exempted members can be exercised without live
    /// WinForms/Outlook infrastructure. This mirrors the established <c>_kbdHandler</c> reflection
    /// pattern already used across the existing per-cluster test files.
    /// </summary>
    internal sealed class HarnessController : QfcItemController
    {
        internal HarnessController()
            : base() { }
    }

    /// <summary>
    /// Reflection helpers and handle-less <see cref="Theme"/> builders shared by the Phase 5/6 test
    /// files. Kept in one place to avoid copy-paste of the reflection field-access boilerplate.
    /// </summary>
    internal static class QfcItemControllerTestSupport
    {
        internal static void SetField(QfcItemController controller, string name, object value)
        {
            FieldInfo field = typeof(QfcItemController).GetField(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            field
                .Should()
                .NotBeNull(because: "field '" + name + "' must exist on QfcItemController");
            field.SetValue(controller, value);
        }

        internal static object GetField(QfcItemController controller, string name)
        {
            FieldInfo field = typeof(QfcItemController).GetField(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            field
                .Should()
                .NotBeNull(because: "field '" + name + "' must exist on QfcItemController");
            return field.GetValue(controller);
        }

        /// <summary>
        /// Invokes a non-public (private/internal) instance method by name. Used to exercise the
        /// private WinForms event handlers directly, since a live control cannot raise their events
        /// in a unit test. All targeted handlers have unique names (no overload ambiguity).
        /// </summary>
        internal static object InvokeNonPublic(
            QfcItemController controller,
            string name,
            params object[] args
        )
        {
            MethodInfo method = typeof(QfcItemController).GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            method
                .Should()
                .NotBeNull(because: "method '" + name + "' must exist on QfcItemController");
            return method.Invoke(controller, args);
        }

        /// <summary>
        /// Guarantees a non-null ambient <see cref="SynchronizationContext"/> so the defensive
        /// <c>if (SynchronizationContext.Current is null)</c> guard in the checkbox/button handlers is
        /// exercised as a deterministic no-op (never constructing a WinForms sync context).
        /// </summary>
        internal static void EnsureSynchronizationContext()
        {
            if (SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            }
        }

        /// <summary>
        /// P6-T3/§6: builds a <see cref="Mock{IUiDispatcher}"/> whose non-generic members execute the
        /// supplied delegate synchronously (mirroring the ambient <c>_itemViewer.Invoke</c> pattern), so
        /// members routed through the Phase 6 dispatch seam become deterministically unit-testable. The
        /// generic <c>InvokeAsync&lt;TResult&gt;</c> overload is configured per-test for the specific
        /// result type it needs (Moq cannot set up an open generic method).
        /// </summary>
        internal static Mock<IUiDispatcher> BuildSyncDispatcher()
        {
            var mock = new Mock<IUiDispatcher>();
            mock.Setup(d => d.Invoke(It.IsAny<Action>())).Callback<Action>(a => a());
            mock.Setup(d => d.InvokeAsync(It.IsAny<Action>()))
                .Returns(
                    (Action a) =>
                    {
                        a();
                        return Task.CompletedTask;
                    }
                );
            mock.Setup(d =>
                    d.InvokeAsync(
                        It.IsAny<Action>(),
                        It.IsAny<DispatcherPriority>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(
                    (Action a, DispatcherPriority p, CancellationToken t) =>
                    {
                        a();
                        return Task.CompletedTask;
                    }
                );
            mock.Setup(d => d.BeginInvoke(It.IsAny<Action>()))
                .Returns(
                    (Action a) =>
                    {
                        a();
                        return Task.CompletedTask;
                    }
                );
            return mock;
        }

        /// <summary>
        /// P5-T1: inject a <c>_themes</c> dictionary and set the active theme key via reflection,
        /// mirroring the <c>_kbdHandler</c> injection pattern.
        /// </summary>
        internal static void InjectThemes(
            QfcItemController controller,
            Dictionary<string, Theme> themes,
            string activeTheme
        )
        {
            SetField(controller, "_themes", themes);
            SetField(controller, "_activeTheme", activeTheme);
        }

        /// <summary>
        /// Builds a lightweight <see cref="Theme"/> carrying only the three button colors read by the
        /// mouse-enter/leave handlers. The parameterless <see cref="Theme"/> constructor requires no
        /// live window handle, so the instance is safe to construct in a unit test.
        /// </summary>
        /// <remarks>
        /// Cycle-3: the parameterless <see cref="Theme"/> constructor leaves <c>_uiDispatcher</c> null
        /// (P10-T21 deliberately does not default it there). <see cref="QfcItemController.SetThemeDark"/>
        /// / <c>SetThemeLight</c> route through <c>Theme.SetQfcTheme(async: true)</c>, which now reads
        /// <c>_uiDispatcher</c>, so a non-executing dispatcher mock (queues the delegate without running
        /// it, preserving the pre-cycle-3 "queued but never pumped" behavior these callers rely on) is
        /// injected here so every caller of this shared builder keeps working without a live handle.
        /// </remarks>
        internal static Theme BuildColorTheme(Color mouseOver, Color clicked, Color back)
        {
            Theme theme = new Theme();
            theme.ButtonMouseOverColor = mouseOver;
            theme.ButtonClickedColor = clicked;
            theme.ButtonBackColor = back;
            Mock<IUiDispatcher> dispatcher = new Mock<IUiDispatcher>();
            dispatcher.Setup(d => d.InvokeAsync(It.IsAny<Action>())).Returns(Task.CompletedTask);
            typeof(Theme)
                .GetField("_uiDispatcher", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(theme, dispatcher.Object);
            return theme;
        }

        /// <summary>
        /// Builds a single-entry <c>_themes</c> dictionary keyed by <paramref name="activeTheme"/>
        /// carrying the supplied color theme.
        /// </summary>
        internal static Dictionary<string, Theme> BuildThemeDictionary(
            string activeTheme,
            Theme theme
        )
        {
            Dictionary<string, Theme> themes = new Dictionary<string, Theme>();
            themes[activeTheme] = theme;
            return themes;
        }

        /// <summary>
        /// Cycle-3 (P10-T31): builds a handle-less <see cref="Theme"/> whose <c>_uiDispatcher</c> and
        /// <c>_lblSender</c> private fields are reflection-injected, mirroring
        /// <c>Theme_DispatcherTests.SetField</c> in <c>UtilitiesCS.Test</c>. <c>_lblSender</c> is set to
        /// a handle-less <see cref="Label"/> so <see cref="Theme.SetMailRead(bool)"/>'s null guard is
        /// satisfied.
        /// </summary>
        internal static Theme BuildDispatchableTheme(IUiDispatcher dispatcher)
        {
            Theme theme = new Theme();
            typeof(Theme)
                .GetField("_uiDispatcher", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(theme, dispatcher);
            typeof(Theme)
                .GetField("_lblSender", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(theme, new Label());
            return theme;
        }

        /// <summary>
        /// Deterministically pumps <paramref name="dispatcher"/> until <paramref name="task"/>
        /// completes, then observes the task (rethrowing any fault). Used to exercise members that
        /// route through a real <c>_itemViewer.UiDispatcher</c> (a sealed WPF Dispatcher that cannot
        /// be mocked) without a running WinForms/WPF message loop. The frame is stopped by posting
        /// its termination back onto the same dispatcher when the task finishes, so there is no
        /// polling, sleeping, or timing dependency.
        /// </summary>
        private static Dispatcher _dedicatedDispatcher;
        private static readonly object _dedicatedDispatcherLock = new object();

        /// <summary>
        /// Ensures the static <c>UiThread.Dispatcher</c> is non-null by seeding it (only when unset)
        /// with a dedicated dispatcher hosted on a parked background thread that is never pumped.
        /// Needed for members that still delegate to a callee using the static
        /// <c>UiThread.Dispatcher</c> before the Phase 6 <c>IUiDispatcher</c> seam replaces it.
        /// <para>
        /// A dedicated (non-<c>CurrentDispatcher</c>) instance is used deliberately for test
        /// isolation: fire-and-forget <c>BeginInvoke</c>/<c>InvokeAsync</c> operations posted by these
        /// tests are enqueued on the parked dispatcher and never execute, so they cannot leak onto the
        /// test thread's own dispatcher and be run (and fault on a handle-less control) by an unrelated
        /// later test that pumps <c>Dispatcher.CurrentDispatcher</c>. Becomes moot once the callee
        /// routes through the injectable dispatcher seam.
        /// </para>
        /// </summary>
        internal static void EnsureUiThreadDispatcher()
        {
            FieldInfo field = typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist");
            if (field.GetValue(null) == null)
            {
                field.SetValue(null, GetDedicatedDispatcher());
            }
        }

        /// <summary>
        /// Lazily creates a single dispatcher hosted on a background thread that grabs its dispatcher
        /// and then parks indefinitely without ever running a dispatcher frame, so any operation posted
        /// to it stays queued and never executes. The thread is a background thread reclaimed at process
        /// exit; no message loop, WinForms form, or timing dependency is created.
        /// </summary>
        private static Dispatcher GetDedicatedDispatcher()
        {
            lock (_dedicatedDispatcherLock)
            {
                if (_dedicatedDispatcher == null)
                {
                    using (var ready = new ManualResetEventSlim(false))
                    {
                        // Parked forever; keeps the thread (and its dispatcher) alive without pumping.
                        var park = new ManualResetEventSlim(false);
                        var thread = new Thread(() =>
                        {
                            _dedicatedDispatcher = Dispatcher.CurrentDispatcher;
                            ready.Set();
                            park.Wait();
                        })
                        {
                            IsBackground = true,
                            Name = "QfcItemControllerTestSupport.ParkedDispatcher",
                        };
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        ready.Wait();
                    }
                }

                return _dedicatedDispatcher;
            }
        }

        /// <summary>
        /// Creates a dispatcher hosted on a dedicated, running STA background thread and returns it.
        /// Operations posted to this dispatcher execute on its own thread, isolated from the shared
        /// test-thread <c>Dispatcher.CurrentDispatcher</c>. This lets a test exercise a member that
        /// dispatches through a real <c>UiDispatcher</c> without pumping (and inadvertently executing)
        /// fire-and-forget operations that other tests may have posted to the shared thread dispatcher.
        /// The caller must call <see cref="ShutdownDispatcher"/> when finished. The thread is a
        /// background thread reclaimed at process exit; completion is observed deterministically by
        /// awaiting the dispatched task, not by polling or sleeping.
        /// </summary>
        internal static Dispatcher StartRunningDispatcher()
        {
            using (var ready = new ManualResetEventSlim(false))
            {
                Dispatcher dispatcher = null;
                var thread = new Thread(() =>
                {
                    dispatcher = Dispatcher.CurrentDispatcher;
                    ready.Set();
                    Dispatcher.Run();
                })
                {
                    IsBackground = true,
                    Name = "QfcItemControllerTestSupport.RunningDispatcher",
                };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                ready.Wait();
                return dispatcher;
            }
        }

        /// <summary>
        /// Stops the message loop of a dispatcher created by <see cref="StartRunningDispatcher"/>,
        /// allowing its background thread to exit.
        /// </summary>
        internal static void ShutdownDispatcher(Dispatcher dispatcher)
        {
            dispatcher?.InvokeShutdown();
        }
    }

    /// <summary>
    /// P5-T1 smoke test: confirms the reusable <c>_themes</c> reflection injection makes a subsequent
    /// <c>_themes[_activeTheme]</c> read return the injected instance.
    /// </summary>
    [TestClass]
    public class QfcItemController_TestSupportSmokeTests
    {
        [TestMethod]
        public void InjectThemes_ThenActiveThemeRead_ReturnsInjectedInstance()
        {
            // Arrange
            HarnessController controller = new HarnessController();
            Theme injected = QfcItemControllerTestSupport.BuildColorTheme(
                Color.Aqua,
                Color.Blue,
                Color.Beige
            );
            Dictionary<string, Theme> themes = QfcItemControllerTestSupport.BuildThemeDictionary(
                "LightNormal",
                injected
            );

            // Act
            QfcItemControllerTestSupport.InjectThemes(controller, themes, "LightNormal");
            object readBack = QfcItemControllerTestSupport.GetField(controller, "_themes");
            Dictionary<string, Theme> readThemes = readBack as Dictionary<string, Theme>;

            // Assert
            readThemes.Should().NotBeNull();
            readThemes["LightNormal"].Should().BeSameAs(injected);
            QfcItemControllerTestSupport
                .GetField(controller, "_activeTheme")
                .Should()
                .Be("LightNormal");
        }
    }
}
