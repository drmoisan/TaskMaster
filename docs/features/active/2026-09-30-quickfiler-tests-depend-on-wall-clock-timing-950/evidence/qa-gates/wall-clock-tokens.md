# AC4 wall-clock census (P6-T8)

Timestamp: 2026-10-02T01-23
Command: one pwsh -NoProfile -Command payload after PREFIX: CMD-TOKEN-COUNT on each of THREE (TOKENS "SpinWait", ".Wait(", "WaitForState", "Task.Wait(") and CMD-TIMESPAN, bodies verbatim.
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
QfcDatamodelLivenessTests.cs: SpinWait 0, .Wait( 0, WaitForState 0, Task.Wait( 0
QfcDatamodelTeardownTests.cs: SpinWait 0, .Wait( 0, WaitForState 0, Task.Wait( 0
QfcInitEmailQueueZeroBatchTests.cs: SpinWait 0, .Wait( 0, WaitForState 0, Task.Wait( 0
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:139 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:141 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs:156 CLASSIFIED :: fake.Advance(TimeSpan.FromMilliseconds(200));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:124 CLASSIFIED :: Task pending = model.QuiesceLoaderAsync(TimeSpan.FromSeconds(5));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:155 CLASSIFIED :: Task pending = model.QuiesceLoaderAsync(TimeSpan.FromSeconds(5));
TIMESPAN QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs:160 CLASSIFIED :: fake.Advance(TimeSpan.FromSeconds(6));
TIMESPAN-UNCLASSIFIED: 0

Surviving TimeSpan lines: the teardown file's two QuiesceLoaderAsync( production arguments and one fake.Advance(, and the liveness file's three fake.Advance( lines; none is a blocking wait.

Positive control (P0-T12, FEATURE/evidence/baseline/census-baseline.md): before the change the same census read SpinWait 1 (liveness) and 1 (teardown); .Wait( 2, 1 and 1; WaitForState 5 and 2; TIMESPAN-UNCLASSIFIED: 6. The census therefore detects these tokens when present.
