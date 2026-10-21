# Code Review — Issue #782 Delivery Record

Timestamp: 2026-09-05T23-00

Command:

```powershell
git log --oneline pre-782-base..HEAD
git show --stat --oneline <each delivery commit>
git diff --name-status pre-782-base..HEAD
```

EXIT_CODE: 0

Output Summary:

This artifact records the disposition of every finding identifier in the specification's
traceability table together with the no-action set, and then records the nine labelled entries the
plan requires. Every commit named below is an ancestor of HEAD and a descendant of the
`pre-782-base` tag.

## Delivery commits

| SHA | Subject | Phase |
|---|---|---|
| `351a242c` | tighten dispatcher contract and callers | Phase 0 baselines and Phase 1 production edits |
| `92c43665` | withdraw finding C03 after a measured regression | Phase 1, SD18 |
| `11056a63` | re-anchor the plan after the branch was rebased | Phase 0, SD23 |
| `945beb84` | route dispatcher throws through a shared constant and drop dead guards | Phase 1 |
| `587cdf16` | split ProgressTracker_Tests and separate its class attributes | Phase 2 |
| `d5e192b3` | centralize UiThread dispatcher reflection in a shared install scope | Phase 3 |
| `06b6677a` | add AC7 regression tests and fix test-hygiene residuals | Phase 4 |
| `e858bc49` | correct the #584 documentation and evidence residuals | Phase 5 |

## Finding disposition — the twenty-six C identifiers

| ID | File changed, or the reason none changed | Commit |
|---|---|---|
| C01 | `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | `351a242c` |
| C02 | `UtilitiesCS/Threading/UiThread.cs` — the getter now reads the static once into a local | `351a242c` |
| C03 | **None. Withdrawn from this delivery under SD18.** `Init()` in `UtilitiesCS/Threading/UiThread.cs` keeps its `pre-782-base` body. See the labelled C03 entry below. | `92c43665` records the withdrawal |
| C04 | None. No-action finding: a pre-existing non-blocking latch race that PR #778 did not touch. | none |
| C05 | `UtilitiesCS/Threading/UiThread.cs` | `351a242c` |
| C06 | `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | `351a242c`, `d5e192b3` |
| C07 | None. No-action finding: the expression-bodied-getter premise is refuted; the `.editorconfig` preference is silent on it. | none |
| C08 | `UtilitiesCS/Threading/UiThread.cs` | `351a242c` |
| C09 | `UtilitiesCS/Threading/UiThread.cs` — message half only. The behavioral half, making `UiThread.Init()` reject non-STA callers, is promoted as its own follow-up entry under AC8 and is not implemented here. | `351a242c` |
| C10 | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — the sentinel now comes from a shut-down STA host instead of the pooled MTA worker | `d5e192b3` |
| C11 | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — the assertion lambda is expression-bodied | `d5e192b3` |
| C12 | `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | `d5e192b3` |
| C13 | `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | `d5e192b3` |
| C14 | `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | `06b6677a` |
| C15 | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — the class attributes are on separate lines | `587cdf16` |
| C16 | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | `587cdf16` |
| C17 | None. No-action finding: class-level `[DoNotParallelize]` is defensible per the plan rationale and repository precedent. | none |
| C18 | `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | `d5e192b3` |
| C19 | `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` — the three P27-T2 passages now describe the synchronous path | `d5e192b3` |
| C20 | `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | `351a242c`, `945beb84`, `06b6677a` |
| C21 | `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` — new production-fallback test | `06b6677a` |
| C22 | None. No-action finding: the `ProgressTrackerPane` setter is private and set-once, so no production path can swap the value between the two reads. | none |
| C23 | `UtilitiesCS/Threading/ProgressTracker.cs`, `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | `351a242c` |
| C24 | None. No-action finding: `WpfUiDispatcher` sees an exception-type change only. | none |
| C25 | `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — the two stale "avoid WindowsBase" clauses are removed | `d5e192b3` |
| C26 | `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | `06b6677a` |

## Finding disposition — the S identifiers

| ID | File changed, or the reason none changed | Commit |
|---|---|---|
| S2-1 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | `06b6677a` |
| S3-1 | The four #584 artifacts named in the traceability table, softened to drop the ordering assertion. The `Timestamp:` semantics half is an upstream follow-up, recorded in this delivery's upstream follow-up artifact. | `e858bc49` |
| S3-2 | `#584/policy-audit.2026-09-04T04-05.md`, `#584/feature-audit.2026-09-04T04-05.md` | `e858bc49` |
| S3-3 | `#584/policy-audit.2026-09-04T04-05.md` | `e858bc49` |
| S3-4 | `#584/evidence/issue-updates/issue-584.2026-09-02T09-02.md` | `e858bc49` |
| S3-5 | The fifteen #584 evidence files enumerated in the specification's S3-5 member set | `e858bc49` |
| S3-6 | `#584/spec.md` | `e858bc49` |
| S3-7 | `#584/spec.md` | `e858bc49` |
| S3-8 | `#584/feature-audit.2026-09-04T04-05.md`, `#584/code-review.2026-09-04T04-05.md`, `#584/policy-audit.2026-09-04T04-05.md`, `#584/evidence/qa-gates/p2-t3-file-size.md` | `e858bc49` |
| S3-9 | `#584/code-review.2026-09-04T04-05.md`, `#584/policy-audit.2026-09-04T04-05.md` | `e858bc49` |
| S4-1 | None in this repository. The stale notes live under `.claude/agent-memory/task-researcher/`, which is push-down-owned from drm-copilot; recorded as an upstream follow-up instead. | none |
| S4-2 | None. No-action finding: an evidence-scope observation only; CI ran every test assembly. | none |

## (a) C03 OMITTED: latch re-arm not implemented

**Discharge route.** C03 is discharged through the omission branch that AC2 carries, not by an
implementation. `UtilitiesCS/Threading/UiThread.cs` keeps its `pre-782-base` `Init()` body; no
re-arm of the `_loaded` latch ships in this delivery.

**Measured regression.** The re-arm made
`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` fail
reproducibly at a 21-second duration against the 500 ms `CancelAfter` budget declared at
`UtilitiesCS/Extensions/DictionaryExtensions.cs` line 177.

**Bisect.** `UtilitiesCS.Test` plus `TaskMaster.Test` returns 5179/5180 with the single line
`_loaded = new ThreadSafeSingleShotGuard();` present in the catch, and 5180/5180 with that one line
removed and nothing else changed. The branch base returns 6992/6992 over the nine assemblies both
before and after the failing runs. The failure is therefore attributable to this delivery and is not
the issue #780 flake. **All three of those figures were measured at the superseded base `b95a5252`
and are recorded here verbatim as the measurement that was taken; they are deliberately not
restated against the re-anchored baseline of 6997.**

**Mechanism.** The `UiSyncContext` getter at `UtilitiesCS/Threading/UiThread.cs` lines 128-131 and
the `AutoScaleFactor` getter at lines 194-197 both call `Init()` lazily when their own backing field
is null. A re-armed latch therefore makes every later read of either accessor retry the WinForms
`SyncContextForm` construction inside `Initialize()` and throw again, starving the thread pool.

**No coverage claim is made.** This entry does not claim that a unit test covers the re-arm branch
and does not claim the branch exists. It does not exist in the delivered tree.

**Follow-up.** The retry semantics C03 asks for are promoted as a separate follow-up entry through
the promotion lifecycle by the orchestrator, whose state P8-T21 records.

**What SD18 supersedes in `spec.md`, and what it does not.** The amendment made to `spec.md` under
SD18 is confined to the AC2 C03 clause. Three further passages still describe the re-arm and are
superseded by SD18 as a recorded decision rather than left standing as an oversight:

- the Behavioral Contract subsection headed `UiThread.Init()`;
- the C03 cell in the `UtilitiesCS/Threading/UiThread.cs` Write Set row;
- the C03 row of the traceability table.

A reader comparing the specification against the shipped tree will find each of those three
describing a re-arm that is not present. That divergence is accounted for here.

`user-story.md` AC-U2 needs no amendment. It bounds the permitted production behaviour changes from
above rather than requiring both of the two it names, so a delivery that ships one of them and not
the other still satisfies it.

## (b) SD5 — the removed message tail

The `WpfDispatcherYield` message's tail "before yielding folder tree work" is intentionally gone
under SD5. Both throw sites now share the single `UiThread.DispatcherNotInitializedMessage`
constant, whose text is domain-neutral and names no caller-specific operation. This is an accepted
and reviewed change rather than a regression. The assertion P4-T3 added to
`YieldAsync_WithoutDispatcher_RemainsStrict` now reads
`WithMessage(UiThread.DispatcherNotInitializedMessage)`. FluentAssertions treats `*` and `?` as its
only wildcards, so that pattern is compared against the entire message and a caller-specific tail
appended at this throw site fails the test. The wildcard form this entry previously cited,
`WithMessage("*UiThread.Init()*")`, did not have that property: the pre-782 message also contained
`UiThread.Init()`, so the wildcard matched it too. Neither this assertion nor its sibling in
`UtilitiesCS.Test/Threading/UiThread_Tests.cs` detects an edit to the constant's own wording, because
an assertion written against the constant moves with the constant; the only part of that wording a
test holds is the substring `UiThread.Init()`, which `WpfDispatcherYieldTests.cs:196` asserts with
`Message.Should().Contain("UiThread.Init()")`.

## (c) SD4 — a residual naming inaccuracy that is deliberately retained

The test method `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`
now asserts a message naming `UiThread.Init()`, so the `NamingInitialize` suffix of its own name is
inaccurate. The name is nonetheless retained. Its fully-qualified name is quoted inside a
`TestCaseFilter` expression in a committed #584 regression-testing evidence artifact, and renaming
the method would make that recorded command resolve to zero tests, converting a reproducible
evidence record into an unreproducible one. The inaccuracy is confined to a test method name and is
recorded here rather than repaired.

## (d) SD10 — a divergence from the PR #778 review body

This delivery adopts the figure **49 live reads across 25 production files**, measured against the
`pre-782-base` tag, with 64 textual occurrences across 30 files of which 15 are comments, XML
documentation, commented-out code, or the exception message literal. The derivation is cited
wherever the figure appears.

The PR #778 review body states **26 files**. The review body publishes no member set, so the source
of the extra file cannot be established: there is no list to diff against. The divergence is
recorded rather than reconciled, and this delivery's figure is the one carried into `#584/spec.md`
because it is the one whose derivation is reproducible.

## (e) SD9 — #584 finding F5

#584 finding F5 asks for synchronization around the existing unsynchronized reflective mutation of
`UiThread._dispatcher`. It is discharged by C12 and C13, which migrate all four `UtilitiesCS.Test`
reflection sites onto a single shared `UiThreadDispatcherScope` install scope, and not by C26, which
adds a new test and changes no existing mutation. C26 is adjacent coverage rather than the
discharging item.

F5 was never promoted. At the time of the #782 review there was no potential entry and no active
feature folder covering it, and both of the recommendations it recorded remained open. The
disposition is now written into both `#584/code-review.2026-09-04T04-05.md` and
`#584/policy-audit.2026-09-04T04-05.md`.

## (f) SD14 — supersession of a `spec.md` Constraint 8 clause

`spec.md` Constraint 8 carries a clause leaving the `ForceDispatcherNull` docstring at
`UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` lines 150-164 untouched. That clause is
superseded by SD14. The docstring described the pre-#778 mechanism — that reading
`UiThread.Dispatcher` with a null backing field returns null — which is false after PR #778, so
leaving it untouched would have left a false statement in the tree that C13's own migration made
more visible rather than less. P3-T7 rewrote it.

## (g) SD14 — supersession of the lines 155-160 clause

The `spec.md` Constraint 8 clause naming `IdleAsyncQueue_Tests.cs` lines 155-160 as deliberately
left is superseded for the same reason. Those lines are the `Purpose:` body of the `<summary>` block
at lines 150-164 that P3-T7 rewrites in full; they cannot be preserved inside a block that is
rewritten. The supersession is a recorded decision rather than an omission.

## (h) SD7 — `[DoNotParallelize]` added to `IdleActionQueue_Tests`

`evidence/baseline/p0-t11-idle-serialization-census.md` records that the two sibling classes sharing
`ApplicationIdleTimer` global state — `IdleAsyncQueue_Tests` and `ApplicationIdleTimer_Tests` — both
already carry `[DoNotParallelize]`, and that `IdleActionQueue_Tests` did not. The attribute is
required rather than optional here because the `[TestCleanup]` that C14 adds calls
`ApplicationIdleTimer.Unsubscribe`, which calls `Stop()` when the invocation list empties, touching
process-global `System.Windows.Forms.Application.Idle` and `ApplicationIdleTimer.Guard` state shared
with both siblings.

## (i) SD17 — `/EnableCodeCoverage` is not passed

No test invocation in this delivery passes `/EnableCodeCoverage`. The reason is that the baseline
coverage figures and the final coverage figures must be produced by one method in order to be
comparable, and `/EnableCodeCoverage` produces a `.coverage` binary that would require a separate
conversion step with its own denominator behaviour. Coverage is instead collected by
`dotnet-coverage collect` with the derived configuration, in P0-T7 for the baseline and in P7-T5 for
the final figures, so both sides of every coverage comparison in this delivery come from the same
collector, the same configuration, and the same selection.
