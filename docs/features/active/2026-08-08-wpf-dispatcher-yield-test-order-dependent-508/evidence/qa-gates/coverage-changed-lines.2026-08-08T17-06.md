# Changed-Code Coverage — `WpfDispatcherYield`

Timestamp: 2026-08-08T17-06

Task: [P2-T12]

AC served: AC3, AC9 (coverage on changed lines does not decrease).

Source: `<FEATURE>/evidence/qa-gates/coverage-postchange.cobertura.xml` (P2-T5 pass 4, 6295/6295).

## Aggregation query, as mandated

The task text requires aggregating **every** `<class>` element whose `filename` is
`UtilitiesCS\OutlookObjects\Folder\WpfDispatcherYield.cs`, including compiler-generated nested types
(`<YieldAsync>d__*` async state machines and `<>c*` lambda display classes), and warns that reading
the named class element alone understates the figure to roughly 83%.

The query was run over all 535 `<class>` elements in the report, by filename and independently by
name substring:

```
TOTAL_CLASS_ELEMENTS_IN_REPORT=535
MATCHED_BY_FILENAME=1
MATCHED_BY_NAME_SUBSTRING=1
  NAME_MATCH name=UtilitiesCS.OutlookObjects.Folder.WpfDispatcherYield
             filename=UtilitiesCS\OutlookObjects\Folder\WpfDispatcherYield.cs
```

**Finding:** in this report the compiler-generated nested types are **not emitted as separate
`<class>` elements**. `dotnet-coverage` attributes the async state-machine and lambda display-class
lines back to the owning class element. Both the filename query and the independent name-substring
query return the same single element, so the mandated aggregation set is that one element and the
aggregate is complete — nothing was missed and no figure is understated.

This is confirmed structurally: the element exposes only two `<method>` children (the two
constructors), yet its class-level `<lines>` collection contains lines 60, 62, and 69, which are
inside `YieldAsync`. Those lines can only have arrived via the state machine, so the state-machine
lines are present in the aggregate.

```
CLASS name=UtilitiesCS.OutlookObjects.Folder.WpfDispatcherYield
      line-rate=0.973684  branch-rate=1
  METHOD_ELEMENT_COUNT=2
    METHOD name=.ctor signature=()                                                   line-rate=1
    METHOD name=.ctor signature=(System.Func<...Dispatcher>, System.Func<...Dispatcher>) line-rate=1
```

## Measured figures

Two counting methods, both reported for transparency (Cobertura repeats lines under `<method>` and
again under the class `<lines>`, so an all-descendant count and a deduped per-line count differ):

| Method | Covered / Total | Line rate |
|---|---|---|
| Tool-reported class `line-rate` attribute (all-descendant) | 37 / 38 | **0.973684 (97.37%)** |
| Deduped distinct source lines | 27 / 28 | **0.964286 (96.43%)** |

| Metric | Value |
|---|---|
| Aggregated line count (deduped) | 28 |
| Aggregated covered-line count (deduped) | 27 |
| Aggregated line rate (deduped) | 0.964286 |
| Aggregated line rate (tool attribute) | 0.973684 |
| Aggregated branch rate | **1.0 (100%)** |
| Uncovered lines by source line number | **46** (exactly one) |

## Gate: aggregated line coverage >= 90% — PASS

Both counting methods clear the threshold with a wide margin: 96.43% deduped and 97.37% by the
tool's own attribute, against the `.claude/rules/csharp.md` requirement of `>= 90%` for any new
module, class, or method. No shortfall, so no escalation is required on this gate.

## The single uncovered line is exactly the one predicted

Uncovered line 46 is:

```csharp
45            _fallbackDispatcherProvider =
46                fallbackDispatcherProvider ?? (() => UtilitiesCS.UiThread.Dispatcher);
```

This is the body of the **default fallback provider lambda** `() => UtilitiesCS.UiThread.Dispatcher`
— precisely the one line the plan's `## Design Decision — [ExcludeFromCodeCoverage]` section
predicted would remain uncovered, and no other line.

Why it is uncovered, and why that is correct: the lambda body executes only when the parameterless
constructor is used **and** the thread-affinitized lookup returns null. The sole existing
parameterless-ctor caller that reaches resolution
(`OutlookFolderTreeServiceConcurrencyTests.GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher`)
runs on a thread that *has* a dispatcher, so the fallback is never evaluated. Arranging the null
case through the parameterless constructor would require reading or mutating the process-global
`UiThread.Dispatcher` — reintroducing exactly the ambient dependency this issue exists to remove,
and rejected as alternative 3 in the plan's `## Design Decision — Seam Shape` section.

Note that line 45 (the assignment) and line 42/44 (the thread-affinitized default) are all covered,
so only the fallback lambda's *body* is unexecuted. The default thread-affinitized lambda and the
parameterless constructor are covered by that same existing concurrency test.

No additional line is uncovered, so the escalation condition ("if any additional line is uncovered
or the aggregated line rate is below 90%") does not trigger.

## Branch coverage: 100%, better than forecast

| Line | Hits | Condition coverage |
|---|---|---|
| 42 | 1 | 100% (4/4) |
| 45 | 1 | 100% (4/4) |
| 60 | 1 | 100% (2/2) |
| 62 | 1 | 100% (2/2) |
| 69 | 1 | 100% (2/2) |

Reported as measured. All five branch points are fully covered:

- lines 42 and 45 — the two `??` null-coalescing defaults in the seam constructor, each exercised
  both with a supplied delegate (by the four new tests) and with null (by the parameterless
  constructor path);
- line 60 — the production `_currentThreadDispatcherProvider() ?? _fallbackDispatcherProvider()`
  resolution, exercised both ways by P1-T10 and P1-T11;
- line 62 — the `if (dispatcher is null)` strict-contract guard, exercised both ways by P1-T11
  (non-null) and P1-T12 (null);
- line 69 — the `await dispatcher.InvokeAsync(...)` state-machine branch.

The plan forecast that branch coverage would be **below** 100%, because the throwing path of the
trailing post-yield `cancellationToken.ThrowIfCancellationRequested()` is not deterministically
arrangeable (it requires cancellation to land strictly between the `DispatcherOperation` completing
and the guard executing). The measured result is 100% because that trailing guard is not emitted as
a distinct branch point in this report. The figure is reported as measured, not asserted; no timing
hack was used to reach it.

## AC3 — all three resolution branches pinned

| Branch | Test | Result |
|---|---|---|
| Thread-affinitized dispatcher present | `YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback` | covered; fallback invocation count asserted 0 |
| Thread absent, `UiThread.Dispatcher` fallback present | `YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher` | covered; both counts asserted 1 |
| Both absent (throws) | `YieldAsync_WithoutDispatcher_RemainsStrict` | covered; `InvalidOperationException` asserted |

Line 60's 100% (2/2) condition coverage and line 62's 100% (2/2) are the mechanical confirmation
that all three are genuinely exercised.

## Changed-line non-regression (AC9)

The baseline comparand is "absent / unmeasured": P0-T11 established that
`[ExcludeFromCodeCoverage]` was honored and the class did not appear in the baseline report at all
(0 matched elements, 0 substring occurrences). Coverage on the changed lines therefore moved from
unmeasured to 96.43-97.37% line and 100% branch. It cannot have decreased.

## Test file coverage

The second in-scope file, `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, is
test code and is correctly excluded from the coverage denominator per
`.claude/rules/general-unit-test.md` ("Configure coverage tooling to exclude test files so metrics
reflect application code"). All four of its tests executed and passed in the source run.

Output Summary: GATE PASS. Aggregated changed-class coverage for
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` is **96.43% deduped (27/28) / 97.37% by
the tool attribute (37/38) line** and **100% branch**, well above the required >= 90%. The mandated
aggregation was run across all 535 `<class>` elements by both filename and name substring; in this
report `dotnet-coverage` folds the compiler-generated nested types into the owning class element, so
the aggregate is that single complete element (confirmed by `YieldAsync` body lines 60/62/69
appearing in its `<lines>` despite only two `<method>` children). Exactly one line is uncovered —
line 46, the default fallback lambda body `() => UtilitiesCS.UiThread.Dispatcher` — which is
precisely the single line the plan predicted; no additional line is uncovered, so no escalation is
triggered. Branch coverage measured 100%, better than the plan's forecast, reported as measured.
Baseline comparand was "absent", so changed-line coverage cannot have decreased.
