# Feature Audit — Issue #791 (quickfiler-high-confidence-cancel-teardown-and-deadline-defects)

- **Date:** 2026-09-06
- **Reviewer:** feature-review agent (cycle 1)
- **Companion artifacts:** `policy-audit.2026-09-06T15-31.md`, `code-review.2026-09-06T15-31.md`

## Scope and Baseline

- **Base branch:** `main`, resolved to `origin/main` @ `7c8ac9ae34b8b3dda9134a5e310f39742fd2f0b6`.
- **Merge base:** recomputed by this reviewer with `git merge-base HEAD origin/main` =
  `7c8ac9ae34b8b3dda9134a5e310f39742fd2f0b6`, identical to the caller-supplied value and to the value
  recorded in `artifacts/pr_context.summary.txt`.
- **Head:** `bug/quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791` @
  `59536368756d979f3f72268dfb4dfd0d4b2f7d9f`, 11 commits ahead of the base.
- **Diff scope:** the full branch diff against the merge base — 72 changed paths: 7 production `.cs`,
  9 test `.cs`, 1 test `.csproj`, 40 documentation and evidence markdown files, 6 agent-memory
  markdown files, 2 promoted potential entries, and the atomic plan. No caller instruction narrowed
  this scope, and none was disregarded on scope grounds.
- **Work mode:** `full-bug`, from the persisted marker at `issue.md:12`.
- **Authoritative acceptance-criteria source:** `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/spec.md`,
  section `## Acceptance Criteria`, criteria AC1 through AC6 at lines 255, 257, 260, 262, 266 and 269.
  Per the `full-bug` rule in `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `spec.md` is the
  only AC source; `user-story.md` is narrative operator context and carries no criteria, and
  `issue.md` carries a narrative copy of AC1 and AC2 that `spec.md:9` and the delivery's own
  `issue.md:154-156` deliberately leave unchecked so there is a single place of record. This reviewer
  verified `user-story.md` contains no `- [ ]` or `- [x]` acceptance item.
- **PR context artifacts:** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`,
  generated 2026-09-06 19:19:04 UTC with `Head SHA: 59536368756d979f3f72268dfb4dfd0d4b2f7d9f`, which
  equals `git rev-parse HEAD`. Not stale; no regeneration was required.
- **Baseline for behavioral comparison:** the pre-fix state at `7c8ac9ae` as captured by the four
  fail-before artifacts under `evidence/regression-testing/` and by the baseline test and coverage
  runs under `evidence/baseline/`.

## Acceptance Criteria Inventory

| ID | Source | Line | Criterion (abbreviated) | Checkbox state at review time |
|---|---|---|---|---|
| AC1 | `spec.md` | 255 | The zero-acceptance first-batch deadline becomes an advisory checkpoint; the scan continues to first acceptance, genuine exhaustion, or a hard bound (item cap plus time ceiling); an empty dialog is permitted only on exhaustion or at the bound; the bound decision, the cutoff, and the scanned/accepted counts are logged at launch and at each deadline decision; covered by deterministic MSTest tests using a fake time provider. | `- [x]` |
| AC2 | `spec.md` | 257 | The Cancel teardown completes cleanly and in order: loader stopped and awaited before any datamodel field is nulled; form and item keyboard handlers unregistered before item rows are removed; keyboard-active flag reset; WebView2 focus parked and breadcrumb dropdown cancelled on the Cancel path; ribbon release callback under a `finally`; every stage including any exception logged. Live-Outlook confirmation is human-interaction exception HI-1 and does not gate the automated review. | `- [x]` |
| AC3 | `spec.md` | 260 | Every regression test named in Test Strategy exists in the file listed for it and passes; fail-before/pass-after evidence recorded under `evidence/regression-testing/` for at least the two named tests. | `- [x]` |
| AC4 | `spec.md` | 262 | The C# toolchain passes in the CLAUDE.md order with no failures in the final pass; coverage XML produced at `artifacts/csharp/coverage.xml`; coverage on the changed files at or above the policy target with no regression on changed lines. | `- [x]` |
| AC5 | `spec.md` | 266 | The branch diff touches no file outside the Write Set other than test files under `QuickFiler.Test/Controllers` and `<Compile Include>` entries; the five named files are unmodified. | `- [x]` |
| AC6 | `spec.md` | 269 | The superseded #424 and #608 criteria are recorded as superseded in this spec under both named sections, and #446 AC-6 is verifiably preserved by an unmodified `QfcHomeController.Iteration.cs`. | `- [x]` |

Total AC items: **6**. Non-checkbox criteria: none. Phantom criteria added by any agent: none.

## Acceptance Criteria Evaluation

### AC1 — advisory checkpoint with two hard bounds — **PASS**

Every clause was verified against the code and against a test, not against the delivery's summary.

- *Continues to first acceptance.* `QfcStreamingDequeueConfidenceGate.cs:236-240` replaces the return
  with a log-and-reset-interval that falls through to the take. Pinned by
  `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance`, which places the single
  qualifier at position 41 behind 40 below-cutoff candidates at 1 s per score against the default 12 s
  interval, and asserts `Scanned == 41` and `QuantitySatisfied`. Under the pre-change code the same
  fixture returned empty after 12 scans; `evidence/regression-testing/p1-t16-gate-fail-before.md`
  records it failing with an empty accepted collection.
- *Genuine exhaustion.* `DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted` — neither
  bound reached, producer dead, `SourceExhausted`, source empty.
- *Item cap.* `:230` checks `scanned >= MaxScanWithoutAcceptance` **before** the take.
  `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached` asserts `Scanned == 4`,
  `takeCount == 4` and `source.HaveCount(6)`, so the bounded scan provably does not consume an extra
  candidate.
- *Time ceiling.* `DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling` drives
  `sourceActive: () => true` with `tryTakeNext` always null and asserts the task is incomplete before
  the fake clock advances past 120 s. This reviewer confirmed the ceiling is necessary rather than
  redundant: the empty-queue wait path at `:244-257` does not increment `scanned`, so the item cap
  alone cannot terminate that loop.
- *Empty dialog only on exhaustion or at the bound.* The only two `return` statements that can produce
  an empty batch inside the `accepted.Count == 0` region are `ScanCapReached` at `:233` and
  `SourceExhausted` at `:249`. There is no third exit.
- *Bound decision logged.* `LogScanBoundReached` at `:346-361` emits the bound name, the counts, the
  cutoff, the elapsed time and `Decision=stop`. The line executes on both bound paths and is covered.
  The one gap is that no test asserts its *content*; see finding N3 in
  `code-review.2026-09-06T15-31.md`. The criterion's requirement is that the decision be logged, which
  is implemented and executed, so this is a verification gap rather than an unmet clause.
- *Cutoff and counts logged at launch and at each decision.* `LogLaunch` at `:310-324` and
  `LogZeroAcceptanceCheckpoint` at `:326-344`, both pinned on content:
  `DequeueAsync_Launch_LogsCutoffQuantityAndBounds` asserts `Cutoff=900`, `0.9`, `Quantity=7`,
  `ScanCap=250` and `Ceiling=00:02:00`; `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts` asserts
  `Accepted=0`, `Scanned=3` and `Cutoff=900` and that exactly three checkpoints occur in a
  ten-candidate scan across a 3 s interval. The reported defect was in part that "the cutoff (900) is
  never logged"; it is now logged twice.
- *Deterministic MSTest with a fake time provider.* All seven AC1 tests use `FakeTimeProvider`. The
  ceiling test asserts incompleteness before advancing the clock, which proves the fake clock is what
  releases the wait rather than a real delay.

### AC2 — ordered, logged, exception-safe teardown — **PASS**

- *Loader stopped and awaited before any datamodel field is nulled.* `ActionCancelAsync` awaits
  `_parent?.DataModel?.QuiesceLoaderAsync(LoaderQuiesceBound)` at `EventHandlers.cs:150-164`, before
  the `groups-cleanup` and `controller-cleanup` stages that lead to field release.
  `QuiesceLoaderAsync` cancels the token, snapshots `_remainingLoadTask`, and awaits
  `Task.WhenAny(loader, bound)`. Pinned by `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup`
  (ordering) and by `QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout` and
  `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` (both outcomes). `Worker_DoWork` now captures
  the task so there is something to await, pinned by `Worker_DoWork_CapturesRemainingLoadTask`.
- *Handlers unregistered before rows are removed.* `UnregisterCancelPathHandlers` runs at stage 6,
  `_groups?.Cleanup()` at stage 9. `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup` asserts
  both the navigation-ledger drain and the form-handler unregistration precede the row removal, by
  first index of each marker, with each marker's presence separately asserted so it cannot pass
  vacuously.
- *Keyboard-active flag reset.* `ResetKeyboardActive` toggles only when the flag is set, pinned by
  `ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive` and its negative control
  `_DoesNotToggle_WhenInactive` — the negative control matters, because an unconditional toggle would
  *activate* an inactive dialog.
- *WebView2 focus parked and breadcrumb dropdown cancelled on the Cancel path.*
  `ParkFocusAndCancelSelectors()` is extracted from `FormViewer_Deactivated` and called at stage 5,
  while the item groups still exist. Pinned by
  `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors`, which verifies both the viewer's
  `ParkFocusOffWebView2()` and `CancelBreadcrumbSelector()` on each of two item controllers. The #677
  bodies and the `Form.Deactivate` wiring are unchanged; only the extraction is new.
- *Ribbon release callback under a `finally`.* `RunTeardownStage("controller-cleanup", Cleanup)` sits
  in the `finally` at `EventHandlers.cs:168-172`, and `QfcHomeController.Cleanup()` invokes
  `ParentCleanup` in its own `finally` at `:396-402`. Pinned by
  `ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup` and
  `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup`, and by
  `ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce` for the "exactly once" half. A residual gap
  in the middle link is recorded as finding N2 in the code review: `QfcFormController.Cleanup()` calls
  `_parentCleanup?.Invoke()` without a `finally`. That file is an explicit AC5 non-goal, so the
  criterion as scoped is met; the residual is tracked rather than counted against AC2.
- *Every stage, including any exception, logged.* `RunTeardownStage` logs completion at DEBUG and any
  escaping exception at ERROR with the stage name, for all seven wrapped stages plus the two
  `QfcHomeController.Cleanup()` blocks and the quiesce await. Entry and completion are logged at INFO.
  The 37-minute silent gap the issue reports is directly addressed.
- *Deterministic MSTest.* All eight Cancel-teardown tests and all five datamodel-teardown tests are
  headless: mocked viewer, no window shown, no handle created, `FormatterServices.GetUninitializedObject`
  to bypass COM constructors, `FakeTimeProvider` for the quiesce bound.
- *HI-1.* The live-Outlook confirmation is outstanding. AC2's own text states it "does not gate the
  automated review", the runbook exists at
  `runbooks/live-outlook-cancel-teardown-verification.runbook.md`, and it is carried forward as an
  unchecked item at `issue.md:109` and in `spec.md` Rollout & Follow-up. It is correctly excluded from
  this evaluation and is recorded as PA-5 in the policy audit as owed follow-up.

### AC3 — every Test Strategy test exists and passes, with the two named fail-before/pass-after pairs — **PASS**

`evidence/qa-gates/p3-t13-ac3-test-inventory.md` maps all 26 Test Strategy names: 26 mapped to an
existing file, 25 with a passing result, and the 26th being the RibbonController test that Test
Strategy explicitly declines to propose (`spec.md:240`, "Not proposed: any test of
RibbonController.ReleaseQuickFiler"). This reviewer spot-checked the mapping by reading the four new
test files and confirming every named method exists in the file the strategy assigns it to, including
`ActionCancelAsync_DoesNotToggle_WhenInactive`, which is named as a suffix in `spec.md:237` and is
present at `QfcFormControllerCancelTeardownTests.cs:164`.

Both required fail-before/pass-after pairs are recorded under
`evidence/regression-testing/`:

- `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance` —
  `p1-t16-gate-fail-before.md` (exit 1, 12 failures, this test failing with an empty accepted
  collection) paired with `p2-t14-pass-after.md` (exit 0).
- `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` —
  `p1-t19-datamodel-teardown-fail-before.md` (exit 1, 5 of 5 red, this test failing with
  `System.ArgumentException: Delegate to an instance method cannot have null 'this'`) paired with the
  same pass-after artifact.

The second pair is the stronger form of RED-first evidence: the failure message reproduces the
production log at `issue.md:65` character-for-character, deterministically and without Outlook. The
delivery also discloses honestly that two of the five datamodel tests fail one step earlier than the
plan predicted — in Arrange, on the fail-closed reflective field lookup, rather than in Act on a
`NotImplementedException`. Both remain red before and green after, so the pair's validity is
unaffected, but this reviewer notes those two carry weaker RED-first proof than the two above.

`p2-t14-pass-after.md` was re-run verbatim against the final build after the `[P2-T15]` architecture
repair, with identical counts, so its `PASS-AFTER` lines describe the delivered code rather than an
intermediate one. This reviewer independently re-ran the whole `QuickFiler.Test` assembly at head:
`Test Run Successful. Total tests: 1362`, exit 0.

### AC4 — toolchain green, coverage XML produced, changed-file coverage at target with no regression — **PASS**

- *Toolchain in the CLAUDE.md order with no failure in the final pass.*
  `evidence/qa-gates/p3-t6-loop-closure.md` records one restart caused by the first
  `csharpier format` rewriting files, then five green steps in one uninterrupted pass with
  `FINAL-PASS-ANY-FILE-REWRITTEN: NO`. This reviewer independently re-executed three of the four
  steps at head: `csharpier check` (`Checked 1587 files`, exit 0), the analyzer `/t:Rebuild` (exit 0),
  the nullable `/t:Rebuild` (`0 Warning(s) 0 Error(s)`, exit 0). `/t:Rebuild` was used rather than
  `/t:Build`, so neither gate was skipped by MSBuild incrementality.
- *Coverage XML at `artifacts/csharp/coverage.xml`.* Present, Cobertura, 18,167,952 bytes, written
  2026-09-06 15:05:41, parsed successfully by this reviewer. The path is explicitly permitted by
  `.claude/hooks/enforce-evidence-locations.ps1` and is git-ignored, so it is a tool output rather
  than committed evidence.
- *Coverage on the changed files at or above the policy target.* 131 executable changed lines, 12 with
  zero hits, **90.8%** covered, at or above the `>= 90%` target the repository unit-test policy sets
  for new and changed code. All 12 uncovered lines were individually checked by this reviewer against
  the code and each is host-bound or contract-defence: the UI `SynchronizationContext` marshal, two
  defensive `catch` blocks with no injectable throw source, and one `log.Debug` on the live-Outlook
  completion branch.
- *No regression on changed lines.* `CHANGED-LINES-WITH-COVERAGE-REGRESSION: 0`. This reviewer
  corroborated it independently at file granularity: aggregating both Cobertura documents with the
  same selection, all five measurable changed production files improved or held both their line and
  their branch rate (gate 97.54% -> 98.10%, deactivate 100% -> 100% with branch 90% -> 91.67%,
  interface 100% -> 100%, `EventHandlers.cs` 49.61% -> 58.12%, `QfcHomeController.cs` 75.85% ->
  76.36%). No file fell.
- *Deviation.* Coverage is collected with `dotnet-coverage collect --output-format cobertura --
  <vstest> ...` rather than `vstest /EnableCodeCoverage`, because the latter writes a binary
  `.coverage` file and not the Cobertura XML this same criterion requires, and the two collectors
  conflict when combined. The wrapper uses the same `vstest.console.exe`, the same nine assemblies and
  the same switches, and both sides of the comparison were produced by one collector and one
  configuration. Disclosed by name as deviation 4 in `spec.md` Rollout & Follow-up. This reviewer
  reproduced the delivery's derived percentages from the resulting document by an independent
  selection, which is the substantive check. Accepted; AC4's substantive requirement is met.
- *Recorded but not counted against AC4.* Repository-wide first-party line coverage is 84.51%, below
  the 85% floor in `.claude/rules/quality-tiers.md` though above the 80% floor in `CLAUDE.md` UT2.
  AC4 is scoped to "coverage on the changed files", not to the repository figure, and the branch moves
  the repository figure upward (84.50% -> 84.51% line, 79.14% -> 79.19% branch). The FAIL row and its
  non-blocking disposition are in `policy-audit.2026-09-06T15-31.md` section 1.2.1.

### AC5 — scope boundary — **PASS**

`git diff --name-only 7c8ac9ae..HEAD -- '*.cs' '*.csproj'` returns exactly 17 paths, independently
re-derived by this reviewer: the seven Write Set production files, four new and five modified test
files under `QuickFiler.Test/Controllers`, and `QuickFiler.Test/QuickFiler.Test.csproj` with four
`<Compile Include>` additions. `QuickFiler/QuickFiler.csproj` is unchanged, correctly, because the
implementation introduces no new production file.

All five named exclusions are verifiably absent from the diff:
`QuickFiler/Controllers/QfcCollectionController.cs`,
`QuickFiler/Controllers/QfcHomeController.Iteration.cs`,
`TaskMaster/Ribbon/RibbonController.cs`,
`TaskMaster/Properties/Settings.Designer.cs`,
`TaskMaster/AppGlobals/AppQuickFilerSettings.cs`.
`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`, the sixth non-goal named in
`spec.md:85`, is also absent.

The criterion is evaluated over the pathspec `'*.cs' '*.csproj'`. That narrowing is disclosed in the
criterion's own evidence bullet at `spec.md:268` rather than left implicit, and it is necessary:
delivering the fix requires writing evidence artifacts and checking these very boxes, so the criterion
read literally over the whole tree is unsatisfiable by construction. Outside the pathspec the branch
changes only the plan, the spec, `issue.md`, the runbook, the research note, the evidence artifacts,
two promoted potential entries and six agent-memory notes — all of which are the plan's own required
outputs. This reviewer judges the disclosed narrowing correct handling of an over-broad criterion
rather than an unstated relaxation, and records it as Observation N15 in the code review so a later
reader evaluating AC5 literally does not misread it.

### AC6 — superseded criteria recorded, #446 AC-6 preserved — **PASS**

- *Recorded under Proposed Fix.* `spec.md:103-105` carries the heading "Superseded prior criteria,
  stated deliberately rather than regressed silently" and names both: the #424 criterion at
  `docs/features/archive/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md:231` and
  the #608 criterion at
  `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/spec.md:184`.
- *Recorded under Data / API / Config Impact.* `spec.md:214` repeats both citations for the reviewer.
  Both sections are present, as the criterion requires, and this reviewer read both.
- *#608's surviving criteria protected.* `spec.md:105` states that #608's other criteria (`:181-183`,
  `:185`) concern the non-empty prefix and must remain green.
  `DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint` is the pin, and it has real force: it injects a
  cap of 2 that is deliberately smaller than the 21 candidates it scans, so a guard widened to
  evaluate the bounds after an acceptance would stop early and fail the test.
- *#446 AC-6 preserved.* `QuickFiler/Controllers/QfcHomeController.Iteration.cs` is absent from
  `git diff --name-only` at head, independently confirmed by this reviewer, so it is byte-identical to
  the base and `CompleteAddingAsync` remains reachable only under `SourceExhausted`. The behavioral
  pin is `IterateQueueAsync_EmptyBatchWithScanCapReached_DoesNotCompleteAdding`, which asserts the new
  stop reason does not close the queue, alongside its negative control
  `IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce`, which asserts genuine
  exhaustion still does. The pair is what makes the preservation verifiable rather than merely
  asserted.

## Summary

| AC | Verdict | Basis |
|---|---|---|
| AC1 | PASS | Seven deterministic tests covering continuation, exhaustion, both bounds, both log lines, and the #608 pin; the bound-log content is unpinned (finding N3) but the clause is implemented and executed |
| AC2 | PASS | Eight Cancel-teardown and five datamodel-teardown tests covering ordering, the quiesce boundary in both outcomes, the keyboard reset with its negative control, focus parking, exception safety and repeat invocation; HI-1 excluded by the criterion's own text |
| AC3 | PASS | 26 of 26 Test Strategy names mapped to an existing file; 25 passing and the 26th explicitly not proposed; both required fail-before/pass-after pairs recorded, one reproducing the production exception verbatim |
| AC4 | PASS | Toolchain green in one uninterrupted pass with three of four steps re-executed by this reviewer at head; Cobertura present and parsed; 90.8% changed-line coverage; zero changed-line regressions, corroborated at file granularity by an independent aggregation |
| AC5 | PASS | 17 code paths, exactly the Write Set plus permitted test paths; all six named exclusions absent; evaluation pathspec disclosed in the spec |
| AC6 | PASS | Both supersession statements present in both required sections; `QfcHomeController.Iteration.cs` unmodified; the preservation pinned by a test and its negative control |

**6 of 6 acceptance criteria PASS. 0 PARTIAL. 0 FAIL. 0 UNVERIFIED.**

Two items are owed but do not affect any verdict:

1. **HI-1**, the live-Outlook confirmation, is outstanding. AC2 states explicitly that it does not
   gate the automated review, and it is carried as an unchecked item at `issue.md:109`. Until it is
   performed, the claim that the Outlook keyboard is usable after Cancel in the field is supported by
   the mechanism and by unit tests, not by observation.
2. Three defect classes surfaced by this review have no tracked issue and should be promoted before
   merge: the disposed-but-not-nulled `_tokenSource` (finding N1), the unprotected
   `_parentCleanup?.Invoke()` in `QfcFormController.Cleanup()` (finding N2), and the coverage
   exclusion on `QfcDatamodel` (finding N5). All three are recorded with recommendations in
   `code-review.2026-09-06T15-31.md`.

**Recommendation: GO for PR.**

## Acceptance Criteria Check-off

All six criteria in the authoritative source file were already `- [x]` at review time, checked off by
the executor with per-criterion evidence bullets. This reviewer re-verified each check-off against the
code and the evidence independently and found every one of them accurate. No criterion required
checking off by this reviewer, and none required un-checking.

No source file was modified by this review. Per rule 5 of the check-off protocol, no criterion was
added. Per rule 3, no criterion text was altered.

The narrative copies of AC1 and AC2 at `issue.md:101-102` remain `- [ ]` by design, so that `spec.md`
is the single place of record for `full-bug` work mode. This reviewer confirms that is the correct
state and did not check them.

### Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/spec.md
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none
```
