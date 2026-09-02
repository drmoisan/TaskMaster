# P2-T7 — Changed-line and new-member coverage (AC20)

Timestamp: 2026-09-02T00-02

Derived by joining Derivation D5 (added production lines relative to the base ref
`807fb0bb6e5e49f43efa6b256b05960bf078ca19`, scoped to `QuickFiler`) to Derivation D6 (the per-line
hit map from the post-processed Cobertura document), after replacing `/` with `\` in the git paths so
they match the native separators Cobertura carries.

## VERDICT: AC20 is NOT fully satisfied

| AC20 clause | Verdict |
|---|---|
| Coverage does not regress on the changed lines | **PASS** |
| Every new or modified member reaches at least 90 % line coverage | **FAIL for 2 of 16 members** |
| Baseline and post-change coverage figures recorded numerically | **PASS** |
| No `[ExcludeFromCodeCoverage]` attribute added or removed | **PASS** (proved by P2-T8) |

The failure is stated in full below rather than dispositioned. **AC20 is left unchecked in
`issue.md`.**

## 1. Repository-wide line coverage, both sides, and the difference

| | Line-rate | Percent (4 dp) | lines-covered | lines-valid |
|---|---:|---:|---:|---:|
| Baseline (P0-T9) | 0.853973 | 85.3973 % | 55001 | 64406 |
| Post-change (P2-T6) | 0.854119 | 85.4119 % | 55083 | 64491 |
| **Difference** | **+0.000146** | **+0.0146 pp** | **+82** | **+85** |

Repository-wide line coverage rose slightly. Branch coverage rose from 79.4239 % to 79.4494 %,
**+0.0255 pp**.

**How much weight that repository-wide movement carries.** The Phase 2 loop restarted twice, so the
suite ran three times on a passing tree, producing `lines-covered` of 55066, 55075 and 55083 against
`lines-valid` of 64490, 64491 and 64491. The spread is 17 covered lines, about 0.026 pp, and it sits
entirely in the `UtilitiesCS` package. **Every `QuickFiler` per-file and per-member figure below was
identical across all three passes.** The repository-wide delta is therefore within run-to-run noise
and is reported as "did not regress" rather than as a measured gain; the change-scoped figures
below, which are stable, are what carry the no-regression argument.

## 2. Changed-line covered-over-total

| Measurement | Value |
|---|---:|
| Added production lines under `QuickFiler/` (D5 total) | 587 |
| Of those, in a coverage-exempt class | 172 |
| Of those, in a file with no coverage row (`IQfcQueue.cs`, `QuickFiler.csproj`) | 15 |
| Non-exempt added lines | 400 |
| Non-exempt added lines that are **non-executable** and therefore excluded from the denominator | **246** |
| **Changed-line executable denominator** | **169** |
| **Changed-line covered** | **97** |
| **Changed-line coverage** | **97/169 = 57.40 %** |

`NOT APPLICABLE` does not apply: the denominator is 169, not zero.

**The count of added lines excluded as non-executable is 246.** An added line with no `LineMap` entry
is a brace, comment, attribute, blank line, `using` directive, or declaration fragment. The
proportion is high (246 of 400) because this change is documentation-heavy: every new member carries
an XML documentation block, and CSharpier splits widened parameter lists one parameter per line, so
a single widened signature contributes many non-executable lines.

### Where the 72 uncovered changed lines are

**All 72 are in `QuickFiler/Controllers/QfcQueue.Enqueue.cs`**, and all of them lie inside the two
members relocated into that file, `EnqueueAsync` and `LoadControllersViewersAsync`. Every other
non-exempt file's added executable lines are **100 % covered**:

| File | Added | Executable | Covered | Rate |
|---|---:|---:|---:|---:|
| QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 48 | 12 | 12 | 100 % |
| QuickFiler/Controllers/QfcHomeController.cs | 18 | 11 | 11 | 100 % |
| QuickFiler/Controllers/QfcHomeController.Iteration.cs | 4 | 1 | 1 | 100 % |
| QuickFiler/Controllers/QfcItemController.cs | 11 | 0 | 0 | n/a (field declaration + docs) |
| QuickFiler/Controllers/QfcItemController.FolderHandling.cs | 57 | 27 | 27 | 100 % |
| QuickFiler/Controllers/QfcItemController.Initialization.cs | 10 | 6 | 6 | 100 % |
| QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | 1 | 1 | 1 | 100 % |
| QuickFiler/Controllers/QfcItemGroup.cs | 9 | 0 | 0 | n/a (auto-property + docs) |
| QuickFiler/Controllers/QfcQueue.cs | 4 | 1 | 1 | 100 % |
| QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 22 | 10 | 10 | 100 % |
| QuickFiler/Controllers/QfcQueue.Enqueue.cs | 216 | 100 | 28 | **28 %** |

## 3. No regression on the changed lines

The clause asks whether coverage **regressed**, not whether it is high. It did not:

- Repository-wide line coverage rose by 0.0022 pp and branch coverage by 0.0195 pp.
- Every non-exempt file except `QfcQueue.Enqueue.cs` has 100 % coverage on its added executable
  lines.
- `QfcQueue.Enqueue.cs` did not exist at the base ref. Its uncovered lines are **relocated
  pre-existing code**, and they were equally uncovered before the move. That is established
  arithmetically rather than asserted:

  | | Baseline | Post-change |
  |---|---:|---:|
  | QuickFiler/Controllers/QfcQueue.cs | 158 / 381 (41.47 %) | 157 / 312 (50.32 %) |
  | QuickFiler/Controllers/QfcQueue.Enqueue.cs | (did not exist) | 28 / 100 (28.00 %) |
  | **Combined QfcQueue surface** | **158 / 381 = 41.47 %** | **185 / 412 = 44.90 %** |

  69 executable lines left `QfcQueue.cs` (381 - 312), and exactly **1** covered line left with them
  (158 - 157). The relocated members therefore carried at most 1 covered line out of 69 at
  baseline, that is at most 1.45 %. Independently, a scan of `QuickFiler.Test` finds **no test that
  invokes the concrete `QfcQueue.EnqueueAsync` or `LoadControllersViewersAsync`**; every match is a
  Moq setup or verification on the `IQfcQueue` interface. The combined surface improved from
  41.47 % to 44.90 %.

## 4. Per-member coverage against the 90 % threshold

Non-exempt new or modified members, measured over their line spans in the post-processed report:

| Member | File:span | Covered/Total | Rate | 90 % gate |
|---|---|---:|---:|---|
| `QfcHighConfidencePreFilter.FilterAsync` (modified) | QfcHighConfidencePreFilter.cs:47-96 | 36/36 | 100.00 % | **PASS** |
| `QfcPreScoredItem` ctor + `FolderHandler` (new) | QfcHighConfidencePreFilter.cs:123-149 | 5/5 | 100.00 % | **PASS** |
| `QfcStreamingDequeueConfidenceGate` (modified) | QfcStreamingDequeueConfidenceGate.cs:43-262 | 113/116 | 97.41 % | **PASS** |
| `QfcHomeController.RunAsync` (modified) | QfcHomeController.cs:271-337 | 39/39 | 100.00 % | **PASS** |
| `QfcHomeController.IterateQueueAsync` (modified) | QfcHomeController.Iteration.cs:12-65 | 36/36 | 100.00 % | **PASS** |
| `QfcItemController.LoadFolderHandlerAsync` (modified) | QfcItemController.FolderHandling.cs:57-148 | 73/77 | 94.81 % | **PASS** |
| `QfcItemController.AssignFolderComboBox` (modified) | QfcItemController.FolderHandling.cs:182-240 | 28/31 | 90.32 % | **PASS** |
| `QfcItemController.ProjectPredeterminedFolder` (NEW) | QfcItemController.FolderHandling.cs:253-268 | 11/11 | 100.00 % | **PASS** |
| `QfcItemController` constructors (modified) | QfcItemController.Initialization.cs:29-117 | 72/72 | 100.00 % | **PASS** |
| `QfcItemController.Cleanup` added statement (modified) | QfcItemController.ViewerSetup.cs:466 | 1/1 | 100.00 % | **PASS** |
| `QfcQueue.ItemControllerFactory` default (NEW) | QfcQueue.Enqueue.cs:33-55 | 11/11 | 100.00 % | **PASS** |
| `QfcQueue.ResolveCarriedHandler` (NEW) | QfcQueue.Enqueue.cs:142-166 | 14/14 | 100.00 % | **PASS** |
| `QfcItemGroup.CarriedFolderHandler` (NEW) | QfcItemGroup.cs:53-60 | 0/0 | n/a | **PASS (vacuous)** — an auto-property with no executable line; the property is exercised by `CarrierLoad_SetsPredeterminedFolderOnItemGroup` and by the leg-B forwarding test |
| `QfcQueue.EnqueueAsync` (relocated + modified) | QfcQueue.Enqueue.cs:67-139 | **0/46** | **0.00 %** | **FAIL** |
| `QfcQueue.LoadControllersViewersAsync` (relocated + modified) | QfcQueue.Enqueue.cs:169-212 | **0/24** | **0.00 %** | **FAIL** |

### The two failures, stated plainly

`QfcQueue.EnqueueAsync` and `QfcQueue.LoadControllersViewersAsync` are **modified** members — each
gained a parameter and `LoadControllersViewersAsync` gained two body statements — so AC20's 90 %
clause applies to them, and **they fail it at 0 %**.

Their bodies cannot be exercised without live WinForms and Outlook COM: `EnqueueAsync` clones a
`TableLayoutPanel` through `UiIdleCallAsync` and hooks an `EmailMoveMonitor`;
`LoadControllersViewersAsync` calls `AddAsync`, which dequeues a real `ItemViewer` from
`ItemViewerQueue`. `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` records this constraint in
its own class documentation, written before this change: "The TLP/MailItem/dispatcher-bound members
are out of scope (Outlook/WinForms) per the seam verification." The repository unit-test policy
prohibits a test that requires a real window.

Neither member is in a class carrying `[ExcludeFromCodeCoverage]`, so **no exemption applies to
them** and the shortfall is not waived by the plan's coverage-threshold reconciliation, which
exempts only `FolderScoringService`, `QfcCollectionController` and `QfcDatamodel`.

What was done to reduce it rather than accept it: the two new statements
`LoadControllersViewersAsync` gained both delegate to members that are themselves at 100 %
(`ResolveCarriedHandler` at 14/14 and the `ItemControllerFactory` production default at 11/11), so
the logic those statements introduce **is** covered; only the two statements that invoke it are not.
The `ItemControllerFactory` seam was additionally narrowed during this task from taking a concrete
`QfcItemGroup` to taking the `IItemViewer` interface, specifically so its production default could be
invoked with a Moq double; that raised the default from 1/12 (8.33 %) to 11/11 (100 %) and is
recorded here because it is a change made in response to this measurement.

**This is reported, not resolved.** Resolving it needs either a headless seam over `AddAsync` and the
UI-idle marshal, which is a wider change than any acceptance criterion authorises, or a ratified
`[ExcludeFromCodeCoverage]` exemption, which AC20 explicitly forbids this change from adding.

## 5. Members in an exempt class, with the named test that pins each instead

| Member | Exempt class | Attribute site | Pinned instead by |
|---|---|---|---|
| `FolderScoringService.ScoreAsync` (modified) | `FolderScoringService` | QfcHighConfidencePreFilter.cs:198 | `QfcDatamodelTests.ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder`, extended by P1-T4 to assert the published handler reaches the caller; and the gate-propagation tests in `QfcStreamingDequeueConfidenceGateTests` |
| `QfcCollectionController.EncapsulateItemGroup` (modified) | `QfcCollectionController` | QfcCollectionController.cs:21 | Not pinned by any behavioural test. `CarrierLoad_SetsPredeterminedFolderOnItemGroup` replicates the group-level carry rather than invoking the method. The only structural pin that survives is `QfcCollectionControllerDefects468Tests.ParentFieldAndConstructorParameterAreTypedIQfcFormController`. Recorded in full in `evidence/other/leg-a.md`. |
| `QfcCollectionController.LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>,...)` (modified) | `QfcCollectionController` | QfcCollectionController.cs:21 | Same; `QfcFormControllerTests.LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval` reaches the overload's guard only |
| `QfcDatamodel.ScoreRemainingQueueMailItemAsync` (modified) | `QfcDatamodel` | QfcDatamodel.cs:25 | `QfcDatamodelTests.ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder`, which invokes it by reflection and asserts all three tuple elements including the new handler |

Lines added to those three classes do not enter the coverage denominator, which is why the 172
exempt added lines are excluded from the changed-line figure above.

## 6. Per-file comparison for the twelve P0-T11 production paths

| Path | Baseline | Post-change | Reading |
|---|---:|---:|---|
| QfcHighConfidencePreFilter.cs | 35/35 (100 %) | 44/44 (100 %) | No reduction; 9 executable lines added, all covered |
| QfcStreamingDequeueConfidenceGate.cs | 112/115 (97.39 %) | 119/122 (97.51 %) | Improved |
| QfcDatamodel.QueueProcessing.cs | NOT PRESENT (exempt) | NOT PRESENT (exempt) | Unchanged; class-level exemption |
| QfcHomeController.cs | 170/223 (76.23 %) | 179/232 (77.16 %) | Improved |
| QfcHomeController.Iteration.cs | 60/60 (100 %) | 60/60 (100 %) | Unchanged |
| QfcItemGroup.cs | 10/11 (90.91 %) | 10/11 (90.91 %) | Unchanged; the new auto-property adds no executable line |
| QfcCollectionController.cs | NOT PRESENT (exempt) | NOT PRESENT (exempt) | Unchanged; class-level exemption |
| QfcQueue.cs | 158/381 (41.47 %) | 157/312 (50.32 %) | **Rate improved.** Covered fell by 1 and executable by 69; both are explained by the deletion of `EnqueueAsync` and `LoadControllersViewersAsync` from this file, which were relocated to `QfcQueue.Enqueue.cs`. See the combined-surface table in section 3. |
| QfcItemController.cs | 73/73 (100 %) | 73/73 (100 %) | Unchanged; the new field adds no executable line |
| QfcItemController.Initialization.cs | 245/258 (94.96 %) | 249/262 (95.04 %) | Improved |
| QfcItemController.FolderHandling.cs | 141/148 (95.27 %) | 165/172 (95.93 %) | Improved |
| QfcItemController.ViewerSetup.cs | 189/209 (90.43 %) | 190/210 (90.48 %) | Improved |

**No file shows a reduction that is not explained by a line deletion in that file.** The single file
whose covered count fell, `QfcQueue.cs`, fell by exactly 1 covered line while losing 69 executable
lines to a relocation, and its rate rose by 8.85 percentage points.

Two files created by this change carry their own rows and are recorded for completeness:
`QuickFiler/Controllers/QfcQueue.Enqueue.cs` at 28/100 (28.00 %), and
`QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs`, which has **no row** because the
class-level `[ExcludeFromCodeCoverage]` on the base part covers it.

## Limitation of the retained evidence, stated

The baseline per-line hit map was not retained: `coverage/coverage.cobertura.xml` is git-ignored and
was overwritten by the P2-T5 run, and P0-T11 recorded per-file totals rather than per-line detail.
The per-member baseline for the two relocated members therefore cannot be read directly from
retained evidence. The claim that they were uncovered at baseline rests on the two independent
arguments given in section 3, the 1-covered-line arithmetic and the absence of any test invoking
them, not on a direct measurement.
