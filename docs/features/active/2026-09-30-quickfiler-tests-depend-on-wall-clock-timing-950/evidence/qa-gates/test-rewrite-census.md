# Test-rewrite census (P3-T1 to P3-T15)

Timestamp: 2026-10-02T01-04
Command: one pwsh -NoProfile -Command payload after PREFIX: CMD-TOKEN-COUNT on each of THREE with the fourteen P3-T15 tokens; a second CMD-TOKEN-COUNT on each of THREE with the seven reason-literal tokens; CMD-TOKEN-COUNT on the R4 file (the P0-T12 R4 tokens plus the three R-DOC tokens); CMD-SPAN-TOKEN-COUNT on R4SPAN, R4PRE, R4HEAD and R4TAIL (the P0-T12 token list of each); CMD-TIMESPAN. Bodies verbatim, wrapped in local functions `Tok` and `Span` (each Tok call also prints a ROW line joining its counts); a missing span anchor exits 4 at the end.
EXIT_CODE: 0

Edits applied: L1, L2, L3, L4, L5 (liveness); T1, T2 (teardown); Z-H, Z0, Z1, Z2, Z-R (zero-batch); R-BODY then R-DOC (R4 file).

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4

THREE rows, fourteen tokens in the order SpinWait, .Wait(, WaitForState, new BackgroundWorker(), new SynchronousBackgroundWorker(), WorkerStarter = StartSynchronously;, private sealed class SynchronousBackgroundWorker : BackgroundWorker, private sealed class DrainableSynchronizationContext : SynchronizationContext, pump.Drain();, SynchronizationContext.SetSynchronizationContext(previous);, TaskCreationOptions.RunContinuationsAsynchronously, IsBusy, starts a real, so no test starts a:
- Liveness: 0, 0, 0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 0, 0
- Teardown: 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0
- Zero-batch: 0, 0, 0, 0, 3, 3, 1, 0, 0, 0, 0, 0, 0, 1

Reason-literal rows, seven tokens in the order "the synchronous starter must reach the injected RemainingEmailLoader", "the synchronous starter must reach the injected loader before returning", "the finally around the awaited loader must clear the flag once it completes", "the finally must clear the flag on the throwing path too", "private static void StartSynchronously(BackgroundWorker worker)", "the injected RemainingEmailLoader must be invoked by the started worker", "bounded timeout":
- Liveness: 1, 1, 1, 1, 1, 0, 0
- Teardown: 1, 0, 0, 0, 1, 0, 0
- Zero-batch: 0, 0, 0, 0, 1, 1, 0

R4 file: flake-watch 0; Append an observation 0; Issue #950: 1; [Timeout(GateTimeoutMs)] 8; private const int GateTimeoutMs = 60000; 1; "The gate-free fixture method EnsureDispatcher seeds the parked dispatcher" 1; "cannot restore a null previous value between the pin" 1; "(W2), and UiThread.Initialize (W5) must not latch" 1

Spans:
- R4SPAN SPAN: 212-283; IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher() 1; EnsureUiThreadDispatcher() 1; using ( 2; .BeSameAs( 1; .NotBeSameAs( 1
- R4PRE SPAN: 212-218; EnsureUiThreadDispatcher() 0; using ( 0
- R4HEAD SPAN: 212-224; EnsureUiThreadDispatcher() 1; using ( 1
- R4TAIL SPAN: 266-272; } 3

CMD-TIMESPAN:
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:139 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:141 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:156 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:124 CLASSIFIED :: Task pending = model.QuiesceLoaderAsync(TimeSpan.FromSeconds(5));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:155 CLASSIFIED :: Task pending = model.QuiesceLoaderAsync(TimeSpan.FromSeconds(5));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:160 CLASSIFIED :: fake.Advance(TimeSpan.FromSeconds(6));
TIMESPAN-UNCLASSIFIED: 0

Verdict: every value equals the P3-T15 acceptance values.
