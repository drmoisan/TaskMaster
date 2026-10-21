# Phase 6 — Change-scoped coverage gate (AC18)

Timestamp: 2026-09-09T14-02
Task: [P6-T10]

## Query output — the same query as `[P0-T12]`, re-run against the post-change Cobertura

EXIT_CODE: 0

Complete verbatim output:

```text
QfcHomeController.cs :: QuickFiler.Controllers.QfcHomeController :: line-rate=0.77907 :: zero-hit-lines=27,40,44,45,46,47,50,51,54,55,56,59,60,61,62,63,64,65,66,67,69,70,71,72,73,74,75,76,77,78,80,81,83,84,170,179,217,218,219,220,221,222,223,224,225,226,241,346,350,352,354,355,356,382,383,384,385
EfcHomeController.cs :: QuickFiler.EfcHomeController :: line-rate=0.982609 :: zero-hit-lines=52,288,289,420
ProgressViewer.cs :: UtilitiesCS.ProgressViewer :: line-rate=1 :: zero-hit-lines=
ProgressPane.cs :: UtilitiesCS.EmailIntelligence.TaskPane.ProgressPane :: line-rate=0.9574468085106383 :: zero-hit-lines=32,33
```

## Per-class line-rate, reported alongside both conditions

| File | Baseline `[P0-T12]` | Post-change | Movement |
|---|---|---|---|
| `QfcHomeController.cs` | 0.777344 | **0.77907** | +0.001726 |
| `EfcHomeController.cs` | 0.982456 | **0.982609** | +0.000153 |
| `ProgressViewer.cs` | 1 | **1** | unchanged |
| `ProgressPane.cs` | 0.8947368421052632 | **0.9574468085106383** | +0.062710 |

No class regressed. All four rose or held.

## Source line ranges of the eight members AC18 names, as delivered

| # | File | Member | Delivered range |
|---|---|---|---|
| 1 | `QuickFiler/Controllers/QfcHomeController.cs` | `Cleanup` | 371-410 |
| 2 | `QuickFiler/Controllers/EfcHomeController.cs` | `Cleanup` | 342-352 |
| 3 | `UtilitiesCS/Threading/ProgressViewer.cs` | `SetCancellationTokenSource` | 68-74 |
| 4 | `UtilitiesCS/Threading/ProgressViewer.cs` | `RequestCancel` | 82-101 |
| 5 | `UtilitiesCS/Threading/ProgressViewer.cs` | `CancelButton_Click` | 103-121 |
| 6 | `UtilitiesCS/Threading/ProgressPane.cs` | `SetCancellationTokenSource` | 50-58 |
| 7 | `UtilitiesCS/Threading/ProgressPane.cs` | `RequestCancel` | 66-85 |
| 8 | `UtilitiesCS/Threading/ProgressPane.cs` | `CancelButton_Click` | 87-105 |

Plus the two logger field initializers added by `[P3-T1]` and `[P3-T5]`, which condition (a) also
covers: `ProgressViewer.cs` 18-20 and `ProgressPane.cs` 17-19.

## Condition (a) — BLOCKING, change-scoped: every line changed by this fix reports non-zero hits

### The two cleanup sites, line by line

| File | Line | Delivered statement | Hits |
|---|---|---|---|
| `QfcHomeController.cs` | 405 | `System.Action parentCleanup = ParentCleanup;` | **1** |
| `QfcHomeController.cs` | 406 | `ParentCleanup = null;` | **1** |
| `QfcHomeController.cs` | 407 | `parentCleanup?.Invoke();` | **1** |
| `EfcHomeController.cs` | 349 | `System.Action parentCleanup = _parentCleanup;` | **1** |
| `EfcHomeController.cs` | 350 | `_parentCleanup = null;` | **1** |
| `EfcHomeController.cs` | 351 | `parentCleanup?.Invoke();` | **1** |

All six changed lines report **non-zero** hits.

### The new and rewritten members in the two progress surfaces

| Region | Measured lines | Covered | Rate | Zero-hit lines in range |
|---|---|---|---|---|
| `ProgressViewer.cs` logger field 18-20 | 3 | 3 | **1.0000** | none |
| `ProgressViewer.cs` `SetCancellationTokenSource` 68-74 | 3 | 3 | **1.0000** | none |
| `ProgressViewer.cs` `RequestCancel` 82-101 | 17 | 17 | **1.0000** | none |
| `ProgressViewer.cs` `CancelButton_Click` 103-121 | 12 | 12 | **1.0000** | none |
| `ProgressPane.cs` logger field 17-19 | 3 | 3 | **1.0000** | none |
| `ProgressPane.cs` `SetCancellationTokenSource` 50-58 | 4 | 4 | **1.0000** | none |
| `ProgressPane.cs` `RequestCancel` 66-85 | 17 | 17 | **1.0000** | none |
| `ProgressPane.cs` `CancelButton_Click` 87-105 | 12 | 12 | **1.0000** | none |

Every line of every new or rewritten member reports non-zero hits. **Condition (a) is MET.**

## Condition (b) — BLOCKING, per-member: at least 0.90 line coverage on each of the eight members

Computed over each delivered member range:

| # | File | Member | Measured | Covered | Rate | >= 0.90 directly |
|---|---|---|---|---|---|---|
| 1 | `QfcHomeController.cs` | `Cleanup` | 35 | 31 | **0.8857** | no |
| 2 | `EfcHomeController.cs` | `Cleanup` | 10 | 10 | **1.0000** | yes |
| 3 | `ProgressViewer.cs` | `SetCancellationTokenSource` | 3 | 3 | **1.0000** | yes |
| 4 | `ProgressViewer.cs` | `RequestCancel` | 17 | 17 | **1.0000** | yes |
| 5 | `ProgressViewer.cs` | `CancelButton_Click` | 12 | 12 | **1.0000** | yes |
| 6 | `ProgressPane.cs` | `SetCancellationTokenSource` | 4 | 4 | **1.0000** | yes |
| 7 | `ProgressPane.cs` | `RequestCancel` | 17 | 17 | **1.0000** | yes |
| 8 | `ProgressPane.cs` | `CancelButton_Click` | 12 | 12 | **1.0000** | yes |

Seven of the eight members reach **1.0000**. Member 1, `QfcHomeController.Cleanup`, reaches 0.8857.

### Member 1 discharged by the alternative route the plan specifies

The Cobertura document reports `line-rate` **per class, not per method** — the shape `[P0-T12]`
observed and recorded. The plan therefore directs that condition (b) be discharged by enumerating
every zero-hit line inside each member range and showing, for each, that it is a line this fix did
not change and that already reported zero hits in the `[P0-T12]` baseline.

`QfcHomeController.Cleanup` [371-410] has exactly four zero-hit lines: **382, 383, 384, 385**.

| Line | Delivered content | Changed by this fix | Zero-hit at `[P0-T12]` baseline |
|---|---|---|---|
| 382 | `catch (System.Exception e)` | **no** | **yes** |
| 383 | `{` | **no** | **yes** |
| 384 | `logger.Error("Home cleanup stage failed. Stage=detach-worker-completed", e);` | **no** | **yes** |
| 385 | `}` | **no** | **yes** |

All four lie in the pre-existing first `catch` block, whose `logger.Error(...)` at line 384 is
reachable only if `_formViewer?.Worker` throws — a throw no test drives. The baseline zero-hit list
recorded at `[P0-T12]` ends `...,382,383,384,385`, identical. These four lines were zero-hit before
this change and are zero-hit after it, and the fix's edit region begins at line 405, below all four,
so `[P2-T5]` confirms they did not even move.

**No zero-hit line inside any member range was changed by this fix**, so no zero-hit line fails
condition (a). **Condition (b) is MET** by the alternative route.

### Why the whole-member formulation would have been unsatisfiable

A gate demanding no zero-hit line anywhere inside `QfcHomeController.Cleanup` could never pass, before
or after this change, because line 384 is unreachable without a `_formViewer?.Worker` throw. The plan
records this and supplies the alternative route rather than weakening the threshold. This artifact
uses that route; it does not lower the 90% figure.

## Two apparent movements in the zero-hit lists, both explained by line shift

| File | Baseline zero-hit | Post-change zero-hit | Explanation |
|---|---|---|---|
| `ProgressPane.cs` | 28, 29 | **32, 33** | The logger field added 4 lines at the top of the class. Delivered lines 32-33 are `get => _dispatcher;` and `set => _dispatcher = value;`, the `UiDispatcher` accessors — the same source lines that were 28-29 at baseline. Pre-existing, untouched by this fix. |
| `EfcHomeController.cs` | 52, 288, 289, 418 | 52, 288, 289, **420** | The Site A' idiom added 2 lines at line 349. Delivered line 420 is `get => _uiSyncContext;`, the `UiSyncContext` accessor — the same source line that was 418 at baseline. Pre-existing, untouched by this fix. |

Neither is a new uncovered line; both are the same pre-existing uncovered accessor at a shifted
number. `ProgressPane.cs`'s class line-rate nonetheless **rose** from 0.8947 to 0.9574, because the
new covered lines enlarged the numerator faster than the denominator.

Output Summary: **condition (a) MET** — all six changed cleanup-site lines and all 71 measured lines
across the eight new or rewritten progress-surface regions report non-zero hits, with zero exceptions.
**Condition (b) MET** — seven of the eight AC18 members reach a line-rate of 1.0000; the eighth,
`QfcHomeController.Cleanup` at 0.8857, is discharged by the plan's stated alternative route, its only
four zero-hit lines being 382-385, unchanged by this fix and already zero-hit in the `[P0-T12]`
baseline. Per-class line-rates are reported alongside and no class regressed.
