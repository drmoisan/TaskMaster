# Phase 4 — Coordinator Size Contingency (P4-T3)

Timestamp: 2026-09-03T03-06
Task: [P4-T3]
Command: `@(Get-Content -LiteralPath TaskMaster/Ribbon/EngineToggleStateCoordinator.cs).Count` before and after the extraction, with a re-run of P4-T1 and P4-T2 in between.
EXIT_CODE: 0

## CONTINGENCY: BRANCH B TAKEN

Exactly one branch is recorded. Branch A is not applicable.

| Measurement | Value |
|---|---|
| `EngineToggleStateCoordinator.cs` after the first format pass (P4-T2) | **515 lines** |
| The 500-line ceiling | 500 |
| Over by | 15 |
| `EngineToggleStateCoordinator.cs` after the extraction and the re-run format pass | **415 lines** |
| Final count at or below 500 | **Yes** |

The research record projected roughly 455 to 465 lines. The realised figure before extraction was
515. The projection was an estimate made before the code and its XML documentation existed; the plan
required this task to measure rather than assume, and the measurement triggered branch B.

Trimming documentation to fit was not an available option — the plan states it explicitly, and the
paragraphs in question are the ones that explain why the cache value must be a reference type and
why an explicit compare-and-swap loop is used instead of an add-or-update factory. Both are exactly
the kind of non-obvious rationale the repository's comment policy requires.

## What was extracted

A new `internal sealed class EngineTogglePressedStateCache` in
`TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` (157 lines), holding the five members the plan
names:

1. the nested state type `PressedState` (private, sealed, reference type),
2. the sequence field `_stateSequence`,
3. the next-sequence helper `NextSequence()`,
4. the compare-and-apply helper `TryApplyState(...)`,
5. the dictionary itself, keyed with `StringComparer.Ordinal`.

One member was added rather than moved: `TryGetActive(string engineName, out bool active)`. The
coordinator's synchronous reader previously read the dictionary directly; with the dictionary now
private to the cache type, the read needs a named accessor. It is a dictionary read only and, like
the reader that calls it, never awaits, blocks or throws.

The coordinator retains the field, now typed as the cache, and both writers call
`_pressedState.NextSequence()` and `_pressedState.TryApplyState(...)`. No behavior changed: the
ticket is still taken immediately before the activation read on both paths, the invalidation is
still inside the conditional, and update-before-invalidate ordering is preserved.

## SCOPE AMENDMENT — reported to the orchestrator

This branch extends the plan's write set. The plan authorizes the extension and requires it to be
reported here. **Four paths beyond the original ten are added:**

| Path | Change | Reason |
|---|---|---|
| `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | created, 157 lines | the extracted cache class |
| `TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` | created, 213 lines | the matching test file the branch requires |
| `TaskMaster/TaskMaster.csproj` | one added compile item | already in the write set; this is a second added line in it |
| `TaskMaster.Test/TaskMaster.Test.csproj` | one added compile item | already in the write set; this is a second added line in it |

Only the first two are genuinely new paths; the two project files were already in the write set and
each simply gains one further compile-item line. No prohibited path is touched:
`TaskMaster/AppGlobals/AppOlObjects.cs`, `TaskMaster/AppGlobals/NonBlockingDelay.cs`,
`TaskMaster/Ribbon/RibbonViewer.cs` and
`TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs` all remain untouched.

The compile-item lines added are:

```
    <Compile Include="Ribbon\EngineTogglePressedStateCache.cs" />
    <Compile Include="Ribbon\EngineTogglePressedStateCacheTests.cs" />
```

Both were confirmed present on the `csc.exe` command line in the post-extraction build log, at 2
hits each, so the registrations took effect.

## Re-run of P4-T1 and P4-T2

Required by branch B and performed. The re-run format pass covered ten paths rather than eight.
Rewritten-file count on the re-run: **2** (`EngineTogglePressedStateCache.cs` and
`EngineTogglePressedStateCacheTests.cs`, both newly authored). Every other path, including
`EngineToggleStateCoordinator.cs` itself, was byte-identical before and after, so no
sibling-invalidation re-run was triggered by the second pass either.

Re-measured line counts, all at or below 500 except the deliberately excluded resource document:

| File | Lines |
|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | **415** |
| `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | 157 |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 444 |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | 141 |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 496 |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 459 |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | 277 |
| `TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` | 213 |
| `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | 326 |

## Behavior preservation evidence

The whole ribbon fixture set was re-run after the extraction:
`docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/evidence/qa-gates/p4-t3/p4-t3.trx`
records total 134, passed 134, failed 0.

That population is the previous 109 (the P1-T8 figure) plus the 9 gate tests, the 6 race tests and
the 9 new cache tests: 109 + 9 + 6 + 9 = 133. The remaining one is
`GetPressed_WhenEnginesAccessorReturnsNull_ReturnsFalseAndStartsNothing`, which the earlier scoped
runs also executed; the 134 total is the complete `TaskMaster.Test.Ribbon` namespace after this
change.

Every test the earlier phases recorded as passing still passes, including the six race tests, the
nine gate tests, the two Finding 1 tests, the pre-existing update-before-invalidate ordering test
and the pre-existing faulted-prime test. The extraction is therefore behavior-preserving.

## Nine tests covering the extracted class

`TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` adds nine tests, all recorded as
Passed in the run above: two on ticket monotonicity, three on the synchronous read including ordinal
case-sensitivity, and four on the compare-and-apply store covering first write, newer ticket, older
ticket (the #525 defect in miniature), equal ticket, and per-key independence.

Output Summary: Branch B taken. `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` measured 515
lines after the first format pass, above the 500-line ceiling, so the versioned cache was extracted
into a new `EngineTogglePressedStateCache` class with a matching test file, both registered as
compile items. After the required re-run of P4-T1 and P4-T2 the coordinator measures 415 lines and
every measured file is at or below 500. The scope amendment adds two new paths and one further
compile-item line in each of the two project files already in the write set, and is reported above.
All 134 ribbon tests pass after the extraction.
