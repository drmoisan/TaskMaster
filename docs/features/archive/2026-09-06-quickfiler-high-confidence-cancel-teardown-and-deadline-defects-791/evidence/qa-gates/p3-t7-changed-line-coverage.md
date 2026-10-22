# [P3-T7] Changed-line coverage on the production Write Set

Timestamp: 2026-09-06T15-09

Command: the anchored `git diff --unified=0 $BaseSha -- <path>` in the task's command block,
preceded in the same block by the R10 `$BaseSha` binding (resolved to
`51b557dfe35702090fec778febfd4049e0e0fed4`), a `git add --intent-to-add -- '*.cs'` companion and a
`git status --porcelain --untracked-files=all` companion over
`QuickFiler/Controllers` and `QuickFiler/Interfaces`. The porcelain output listed the seven Write
Set production paths as ` M` and nothing else.

EXIT_CODE: 0

## Method

For each measurable path, changed line numbers are the added lines of every hunk. `hits` is read
from `artifacts/csharp/coverage.xml` ([P3-T5]) through a **de-duplicated per-line map** that merges
`./lines/line` with `./methods/method/lines/line` for every `class` element whose `filename` ends
with a directory separator followed by the file name, keyed by line number and resolved by maximum
`hits`. The merge is required because a Cobertura document produced by this collector emits the same
source line under both branches, and an async method's state machine emits lines under `./lines`
that have no `<method>` parent.

Where a hunk's added and removed line counts are **equal**, new line `c+i` maps to old line `a+i`
and the baseline `hits` is read from `coverage\791-baseline.cobertura.xml` ([P0-T11]) through the
same merged map. Where the counts are unequal no one-to-one mapping exists, so the line is recorded
`baseline=none` and excluded from the regression count rather than being attributed borrowed
coverage.

A changed line carrying no `line` element in either branch of the merged map is non-executable —
an XML doc comment, a blank line, a `using` directive, a brace, an enum member or an interface
method declaration — and is recorded `hits=non-executable`, excluded from both the `hits = 0` count
and the regression count.

## Scope: the [P0-T12] determination is authoritative

[P0-T12] reported five `MEASURABLE:` paths and two `UNMEASURABLE:` paths. That set is identical to
the five paths this task's command block enumerates, so there is no divergence to record.

### UNMEASURABLE paths

CHANGED-LINE-COVERAGE: NOT MEASURABLE — `QuickFiler/Controllers/QfcDatamodel.cs`
CHANGED-LINE-COVERAGE: NOT MEASURABLE — `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`

Citation (D1): `QuickFiler/Controllers/QfcDatamodel.cs` line 25 carries `[ExcludeFromCodeCoverage]`
on the partial class declaration. The attribute applies to the whole type, so members declared in
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, which declares
`public partial class QfcDatamodel` at line 12, are excluded too.
`QuickFiler/Controllers/QfcScanProgressBandMapper.cs` line 12 records the same fact in prose.
[P0-T12] confirmed the consequence empirically: both files produce **zero** `class` elements in the
baseline Cobertura document. Changed-line coverage for them is structurally unmeasurable, not merely
low.

Substitute evidence — the passing tests that exercise those changed lines, all recorded
`PASS-AFTER` by [P2-T14]:

| Changed member | Exercising test |
|---|---|
| `QuiesceLoaderAsync` completion path | `QfcDatamodelTeardownTests.QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout` |
| `QuiesceLoaderAsync` bound path and `LogQuiesceOutcome` | `QfcDatamodelTeardownTests.QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` |
| `TryQueueRemainingMailItemAsync` and `TryCreateRemainingQueueAdmission` refusal | `QfcDatamodelTeardownTests.TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` |
| `TryQueueRemainingMailItemAsync` admission (non-refusal) | `QfcDatamodelTests.TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate` (green in [P2-T15]) |
| `Worker_DoWork` loader-task capture | `QfcDatamodelTeardownTests.Worker_DoWork_CapturesRemainingLoadTask` |
| `Cleanup()` null guards | `QfcDatamodelTeardownTests.Cleanup_CalledTwice_DoesNotThrow` |
| `_remainingLoadActive` liveness, unchanged | `QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` (green in [P2-T15]) |

## Measurable paths — per-file result

| Path | Changed lines | Non-executable | Executable | `hits = 0` | Baseline-mapped | Regressions |
|---|---|---|---|---|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 121 | 77 | 44 | 0 | 5 | 0 |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 36 | 36 | 0 | 0 | 0 | 0 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 88 | 31 | 57 | 8 | 0 | 0 |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | 16 | 15 | 1 | 0 | 2 | 0 |
| `QuickFiler/Controllers/QfcHomeController.cs` | 33 | 4 | 29 | 4 | 0 | 0 |
| **Total** | **294** | **163** | **131** | **12** | **7** | **0** |

CHANGED-LINES-TOTAL: 294
CHANGED-LINES-NON-EXECUTABLE: 163
CHANGED-LINES-EXECUTABLE: 131
CHANGED-EXECUTABLE-LINES-WITH-ZERO-HITS: 12
CHANGED-LINES-WITH-COVERAGE-REGRESSION: 0

Every changed production line in the measurable set is recorded with either a post-change `hits`
value or the `hits=non-executable` marker. The `hits = 0` count is stated over executable lines only.

## `QuickFiler/Interfaces/IQfcDatamodel.cs` — every changed line is non-executable

All 36 changed lines in this file are non-executable, which is the case [P3-T7] predicts explicitly.
[P1-T1] adds only the `ScanCapReached` enum member inside `QfcDequeueStop`, the
`QuiesceLoaderAsync` declaration on the interface, and XML documentation including the rewritten
`DeadlineExpired` doc. None of those emits IL. The file still reports a `class` element — the
`QfcDequeueBatch` struct does — which is why [P0-T12] reports it `MEASURABLE:` at file level; the
per-line marker resolves the question one level lower and the two determinations do not contradict.

## The 12 zero-hit executable changed lines, named

None is a regression: each is a line with no baseline counterpart, so `hits = 0` here means "new
code not reached by a test", not "coverage lost".

**`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` (8 lines)**

- 139-141 — the `{ await uiContext; }` block inside `ActionCancelAsync`. Reached only when
  `_formViewer?.UiSyncContext` is non-null. Every test in
  `QfcFormControllerCancelTeardownTests` drives a `Mock<IQfcFormViewer>` whose `UiSyncContext`
  resolves to null, because a real `SynchronizationContext` on the viewer would require a WinForms
  message loop, which the headless-test policy forbids. The guard around it is covered; the marshal
  itself is host-bound.
- 160-163 — the `catch (System.Exception e)` around the awaited quiesce. Reached only when
  `IQfcDatamodel.QuiesceLoaderAsync` returns a **faulted** task. The interface contract states it
  never throws for the timeout case, and `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs`
  pins that, so this catch is defence against a future contract violation rather than a reachable
  path from any current implementation.
- 289 — the completion-path `log.Debug` line [P2-T11] adds inside `MoveAndIterate`. That branch is
  reached only after a real `BackGroundMoveAsync` over live Outlook items, which the headless test
  suite does not drive; the surrounding `MoveAndIterate` else-branch is uncovered at baseline for
  the same reason.

**`QuickFiler/Controllers/QfcHomeController.cs` (4 lines)**

- 382-385 — the `catch (System.Exception e)` around the worker-completed detach. Reached only when
  detaching a `BackgroundWorker` event handler throws.
  `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` covers the success path of that block; a
  throwing `-=` on a real `BackgroundWorker` has no seam to inject.

## Regression determination

CHANGED-LINES-WITH-COVERAGE-REGRESSION: 0

Seven changed lines had an equal-count hunk and therefore a one-to-one baseline mapping (five in the
gate, two in the deactivate partial). For none of them is the post-change `hits` lower than the
baseline `hits`. The remaining 287 changed lines sit in unequal-count hunks — overwhelmingly pure
insertions — and are recorded `baseline=none` and excluded from the regression count, because
attributing a baseline `hits` value across an unequal hunk would be borrowed coverage rather than a
measurement.

The count of changed lines whose post-change `hits` is lower than their baseline `hits` is **0**,
which is this task's acceptance.
