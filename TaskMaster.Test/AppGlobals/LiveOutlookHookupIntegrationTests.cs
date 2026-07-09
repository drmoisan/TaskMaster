using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Developer-only, opt-in live-Outlook integration harness for the Issue #207 readiness-gate
    /// hookup (AC13). It exercises <see cref="OutlookReadinessGate"/> driving a
    /// <see cref="HookReadinessCoordinator"/> against a live
    /// <see cref="Outlook.Application"/> on an STA thread, logs the readiness wait duration and the
    /// per-hookup latency via <see cref="Stopwatch"/>, and asserts (a) the coordinator reaches
    /// Completed and (b) no single poll-tick handler blocks the STA beyond a small bound.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a smoke/integration check, NOT a deadlock reproduction. It requires a live Outlook
    /// profile and a running STA message pump, so it MUST NOT run in CI. The single test method is
    /// marked <c>[TestCategory("LiveOutlook")]</c>; the standard QC/CI run excludes it via
    /// <c>/TestCaseFilter:"TestCategory!=LiveOutlook"</c>, and it is excluded from the coverage
    /// denominator.
    /// </para>
    /// <para>
    /// The Outlook-availability decision is scoped to the construction phase, not to an HRESULT
    /// whitelist. The harness routes its two phases through
    /// <see cref="LiveOutlookHarnessRunner.Run{T}(System.Func{T}, System.Action{T})"/>: constructing
    /// the Outlook COM <see cref="Outlook.Application"/> is the construction phase, and ANY
    /// <see cref="COMException"/> thrown there — regardless of HRESULT, explicitly including
    /// <c>0x80010100</c> RPC_E_SYS_CALL_FAILED as well as class-availability HRESULTs such as
    /// <c>0x80040154</c> REGDB_E_CLASSNOTREG — reports <see cref="Assert.Inconclusive(string)"/>
    /// (skipped) rather than failing, because no code-under-test has run yet and there is no Outlook
    /// to exercise. Exceptions thrown in the exercise phase (gate construction,
    /// <c>coordinator.Tick()</c>, and the hookup callback), INCLUDING COMExceptions, are captured
    /// and still fail the test. When Outlook IS available the harness runs the real assertion path
    /// unchanged.
    /// </para>
    /// <para>
    /// Developer run command:
    /// <c>vstest.console.exe &lt;TaskMaster.Test assembly path&gt; /TestCaseFilter:"TestCategory=LiveOutlook"</c>
    /// </para>
    /// <para>
    /// The harness pumps the STA with <see cref="Application.DoEvents"/> (WinForms, already
    /// referenced by this test project) rather than a WPF <c>Dispatcher</c>, to avoid adding a new
    /// assembly reference outside the fix's scope lock. The production poll uses a
    /// <c>System.Windows.Threading.DispatcherTimer</c> in <c>AppEvents.Hook()</c>; this harness
    /// verifies the same coordinator + gate decision path that the timer drives.
    /// </para>
    /// </remarks>
    [TestClass]
    public class LiveOutlookHookupIntegrationTests
    {
        // Upper bound (ms) on any single coordinator tick. A ready gate + hookup tick must not
        // block the STA for a prolonged period; the cheap probe and the hookup should each return
        // promptly. Generous to avoid false failures on a cold profile while still catching a
        // multi-second STA block.
        private const int MaxSingleTickBlockMs = 2000;

        // Overall ceiling (ms) on the readiness wait before the harness gives up (developer
        // convenience only; the production coordinator never gives up).
        private const int ReadinessWaitCeilingMs = 120000;

        // Poll cadence (ms) for the developer harness loop.
        private const int PollIntervalMs = 250;

        [TestMethod]
        [TestCategory("LiveOutlook")]
        public void LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold()
        {
            Exception captured = null;
            string skipReason = null;
            bool completed = false;
            long maxTickBlockMs = 0;
            long readinessWaitMs = 0;
            long hookupLatencyMs = 0;

            var thread = new Thread(() =>
            {
                // Construction-scoped skip: constructing the Outlook Application is Phase 1. A
                // COMException there (any HRESULT, e.g. 0x80010100 RPC_E_SYS_CALL_FAILED) is a pure
                // environment/launch failure and reports a skip. Everything inside the exercise
                // delegate is Phase 2 (gate construction, coordinator ticks, hookup callback);
                // exceptions there — including COMExceptions — are captured failures.
                var outcome = LiveOutlookHarnessRunner.Run(
                    () => new Outlook.Application(),
                    app =>
                    {
                        // Arrange: live Outlook on this STA, wrapped by the production gate + coordinator.
                        var gate = new OutlookReadinessGate(app);

                        var hookupStopwatch = new Stopwatch();
                        var coordinator = new HookReadinessCoordinator(
                            gate,
                            () =>
                            {
                                // Real hookup: read the default-store inbox folder once to prove the
                                // readiness-dependent COM access succeeds, timing its latency.
                                hookupStopwatch.Restart();
                                var inbox = app.Session.DefaultStore.GetDefaultFolder(
                                    Outlook.OlDefaultFolders.olFolderInbox
                                );
                                inbox.Should().NotBeNull("the inbox must be reachable once ready");
                                hookupStopwatch.Stop();
                            }
                        );

                        // Act: drive the coordinator through an STA-pumped poll loop until Completed.
                        var waitStopwatch = Stopwatch.StartNew();
                        while (waitStopwatch.ElapsedMilliseconds < ReadinessWaitCeilingMs)
                        {
                            var tickStopwatch = Stopwatch.StartNew();
                            var result = coordinator.Tick();
                            tickStopwatch.Stop();
                            maxTickBlockMs = Math.Max(
                                maxTickBlockMs,
                                tickStopwatch.ElapsedMilliseconds
                            );

                            if (result == HookReadinessTickResult.Completed)
                            {
                                completed = true;
                                break;
                            }

                            // Pump the STA message queue, then wait briefly before the next tick. The
                            // STA stays responsive throughout (no blocking sleep on the pump).
                            var pumpStopwatch = Stopwatch.StartNew();
                            while (pumpStopwatch.ElapsedMilliseconds < PollIntervalMs)
                            {
                                Application.DoEvents();
                                Thread.Yield();
                            }
                        }

                        readinessWaitMs = waitStopwatch.ElapsedMilliseconds;
                        hookupLatencyMs = hookupStopwatch.ElapsedMilliseconds;
                    }
                );

                captured = outcome.Captured;
                skipReason = outcome.SkipReason;
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            thread.Join();

            // Skip (do not fail) when Outlook is unavailable in this environment. A developer
            // machine with Outlook registered leaves skipReason null and runs the real assertions
            // below; a headless agent (no Outlook) reports Inconclusive and stops here.
            if (skipReason != null)
            {
                Assert.Inconclusive($"[LiveOutlook] Skipped: {skipReason}");
            }

            // Assert: surface any STA-thread exception, then verify completion + responsiveness.
            captured.Should().BeNull("the live hookup must not throw on the STA");
            Console.WriteLine(
                $"[LiveOutlook] readinessWaitMs={readinessWaitMs}; hookupLatencyMs={hookupLatencyMs}; maxTickBlockMs={maxTickBlockMs}"
            );
            completed
                .Should()
                .BeTrue("the coordinator must reach Completed once the live store is ready");
            maxTickBlockMs
                .Should()
                .BeLessThanOrEqualTo(
                    MaxSingleTickBlockMs,
                    "no single coordinator tick may block the STA beyond the threshold"
                );
        }
    }
}
