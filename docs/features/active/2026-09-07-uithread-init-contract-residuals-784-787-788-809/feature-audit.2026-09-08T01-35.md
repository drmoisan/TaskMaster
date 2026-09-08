# Feature Audit — issue #809, `uithread-init-contract-residuals-784-787-788`

- Artifact timestamp: 2026-09-08T01-35
- Work mode: `full-bug` (marker at `issue.md:12`). **`spec.md` is the sole acceptance-criteria source.**
  `user-story.md` is intentionally absent; `issue.md` carries a mirrored AC1-AC4 block that is not the
  authoritative source and was not used to derive verdicts.
- Baseline: `04a54e681bd21e841e124c016df30672ee701b75`. Head: `ef431e6a`.
- Plan: `plan.2026-09-07T20-14.md`, 7 phases, 72 tasks, 72 checked, 0 unchecked (verified by count).

**Blocking findings: 0.**

## AC evaluation table

| AC | Verdict | Basis |
|---|---|---|
| AC1 | PASS | Precondition present as the first statement of `Init()` at `UtilitiesCS/Threading/UiThread.cs:30-34`, ahead of the four monitoring assignments at `:36-45` and of `lock (InitLock)` at `:51`. Message constant at `:230-234`. Three tests assert it from a dedicated MTA thread, all red before and green after. The MTA caller at `QfcHomeControllerRunAsyncTests.cs:329` is removed and replaced with a pumping-dispatcher transaction; that test passes twice in `p4-t3-quickfiler-tests.md`. |
| AC2 | PASS | `_initialized` is set at `UiThread.cs:58`, after `Initialize()` returns at `:57`; a throw propagates with the flag false. `Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` was red before (`Expected working not to be <null>`) and is green after. The "#782 regression scenario reproduced as a test" clause is discharged by the forced-throw scenario plus the invocation-count anti-storm test, with the substitution reasoned in `p6-t4`. The reviewer verified the underlying design argument structurally against the head tree (see code review §3). The `lock` additionally closes the pre-existing C04 race, covered by the two-racer test. |
| AC3 | PASS | Predicate replaced at `UiThread.cs:155-190`. `IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` is the defect test: red before (`Expected result to be True, but found False`), green after. Six sibling cases pin the false paths, including the foreign-WinForms-context guard. Ordering-sensitive callers still pass: both `WinFormsPumpHostTests` marshal tests and `EfcFormControllerTests.ActionDeleteAsync_AwaitedTwice_...`, each recorded green in two passes. The predicate honours the `BreadcrumbUiDispatcher.cs:263-272` constraint — see code review §2. |
| AC4 | PASS | 17 tests added; discovered total moves 7120 → 7137, difference 0 against expectation. Seams are `SyncContextFormFactory` and `ResetForTesting()`, wrapped by `UiThreadStateScope`. STA/MTA rejection, retry-after-throw, and inline-vs-post are each covered. No live Outlook host: zero `Microsoft.Office.Interop` references and zero `TestCategory` attributes in the three touched test files, and `NoLiveFormInTestAssemblyTests` passes, so no new `Form`-derived type exists in `UtilitiesCS.Test`. |
| AC5 | **PARTIAL** | The second and third clauses are met: `p5-t6-tryaddvalues-rep1/2/3.md` record `REPETITIONS_PASSED: 3` and the full-suite row for `TryAddValuesAsync_UpdatesExistingValue` is `Passed`, so no single failure is attributed to this delivery. The first clause is not established. `evidence/other/p0-t15-mta-synccontextform-measurement.md:42` labels the run `MTA` by inference from research R4, and `p2-t10-fail-before.md` records the direct measurement that falsifies that premise. The reviewer's analysis (code review §3) is that the run most likely executed STA, in which case no MTA measurement was taken and the stated refutation of the #782 narrative in `p6-t4-ac2-regression-reconciliation.md` is unsupported. Non-blocking: the AC2 design and its tests do not depend on the value, which decision D5 required and the reviewer verified structurally. Remedy is an artifact correction, not code or test rework. |
| AC6 | **PARTIAL** | Every measurable clause is met and was independently recomputed by the reviewer from the raw Cobertura documents rather than read from the artifacts: `UiThread.cs` line coverage 121/126 = **96.03%**, above the 80% `CLAUDE.md` floor and above the 76.83% baseline (63/82, also reviewer-recomputed); each newly added member at or above 90%, worst row `SynchronizationContextAwaiter.IsCompleted` at 18/20 = **90.00%**; changed-line coverage 46/48 = 95.83%; no changed line loses coverage, since every changed line is added or deleted and the added set is measured. Two literal clauses are unmet: the report was produced by `dotnet-coverage collect` rather than `vstest.console.exe ... /EnableCodeCoverage`, and the raw report is git-ignored under `coverage/` rather than stored under `evidence/qa-gates/`, where three derived markdown artifacts are stored instead. See the adjudication below. Non-blocking. |

Legend: PASS = delivered and verified; PARTIAL = substance delivered with a stated clause unmet;
FAIL = not delivered; UNVERIFIED = evidence unavailable. No AC is FAIL or UNVERIFIED.

## Adjudication of the AC6 collector substitution

The substitution is **legitimate as an instrumentation route** and makes AC6 unmet **only as literally
worded**. Reasoning:

- The reason recorded is verifiable in the tree. `scripts/vscode/TaskMaster.cli.runsettings` carries
  only an `<MSTest><Parallelize>` block and no data collector, and
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1:19-26` states that the omission is deliberate because
  the outer `dotnet-coverage` instrumentation and the built-in Code Coverage collector conflict. The
  reviewer read both files.
- `dotnet-coverage` and `/EnableCodeCoverage` drive the same Microsoft coverage engine. The substitution
  changes the invocation shape and the output format, not the measurement, and it yields Cobertura
  directly rather than a `.coverage` binary requiring a later conversion step. The delivery therefore
  produced strictly more auditable evidence than the literal command would have.
- The substitution was declared as `AC6-COLLECTOR-SUBSTITUTION` in two artifacts rather than adopted
  silently, which is the behaviour the policy expects when a named command cannot be used as written.
- Note that `CLAUDE.md` CUT3 step 4 also names `/EnableCodeCoverage`, so the deviation is from the
  policy's toolchain wording as well as from the AC's. It is the same deviation, not two.

On the storage clause: the raw 18 MB Cobertura documents are deliberately kept out of git
(`p6-t12-untracked-output-check.md` shows `coverage/` holds only a tracked `.gitkeep`, and
`DELIVERY_ADDED_RESULTS_FILE_COUNT: 0`). That is the better engineering choice — committing them would
leave unreachable multi-megabyte blobs in history — but it means the artifact AC6 names is not where
AC6 says it will be. What is under `evidence/qa-gates/` is `p6-t1`, `p6-t2` and `p6-t3`, which record
the derived figures with their counting method pinned. The reviewer was able to reproduce every figure
from the ignored documents, so nothing is unverifiable in practice.

**Recommendation:** the maintainer may reasonably elect to treat AC6 as satisfied and record the
deviation, in which case the verdict becomes PASS. This review does not recommend remediation. What it
does recommend is reconciling the AC wording (and `CLAUDE.md` CUT3 step 4) with the collection route the
repository actually uses, so the next delivery does not have to declare the same substitution.

## Baseline comparison

| Quantity | Baseline | Head | Source of the head figure |
|---|---|---|---|
| `UiThread.cs` line coverage | 76.83% (63/82) | 96.03% (121/126) | Reviewer-recomputed from both Cobertura documents |
| `UiThread.cs` uncovered lines | 19 | 5 (`38,39,40,177,178`) | Reviewer-recomputed; matches the artifacts exactly |
| First-party line coverage | 84.58% | 84.62% | Reviewer-recomputed: 56248/66471 |
| First-party branch coverage | 79.34% | 79.38% reported; 77.03% reviewer-recomputed | Method difference explained in policy audit F7; both clear 75% |
| Discovered tests | 7120 | 7137 (+17) | `p5-t5`, reconciles to the 17 added methods counted in the patch |
| Test failures | 0 | 0 | `p5-t5` TRX counters |
| csharpier checked files | 1608 | 1611 (+3) | `p5-t2`, reconciles to the three new files |
| msbuild projects | 18 | 18 | `p5-t3` |
| `FolderPredictorTests.cs` lines | 1066 | 1067 | Reviewer-verified with `awk 'END{print NR}'` |

Coverage did not regress on any measured axis. The delivery closes 17 of the 19 baseline-uncovered
lines in the file in scope and adds no new uncovered construct other than the two-line predicate arm
discussed in the code review.

## Scope conformance

The head tree matches the Write Set in `spec.md` exactly: four production files (three source plus one
`.csproj`) and eight test files (six source plus one `.csproj`, plus the three one-attribute
additions the Write Set amendment adds). No file outside the Write Set was touched. The declared
non-goals were all honoured: `ThreadSafeSingleShotGuard` is retained and only `UiThread`'s own use of it
is removed; no `IUiDispatcher` routing was added to `QfcHomeController`; no `InternalsVisibleTo` grant
was added to `QuickFiler.Test`; no `.runsettings` apartment setting was changed;
`Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` keeps its
deliberately inaccurate name; and the reflected field names `_uiSyncContext` and `_dispatcher` are
unchanged.

## Residual risks carried forward (not defects in this delivery)

1. Eleven production await sites can change execution ordering under the new predicate, and no test
   asserts ordering at any of them. Recorded as residual by the delivery, which is correct. Live-host
   verification is explicitly not an acceptance criterion.
2. Four production sites gain a possible new throw. All are unreachable in production once
   `ThisAddIn.cs:35` has run on the Outlook STA; at `AppOlObjects.cs:367` the new throw is an
   improvement on the current behaviour.
3. The stale-`_uiThreadId` residual in the second true branch of `IsCompleted` (code review §2),
   unreachable in production.
4. `FolderPredictorTests.cs` remains 567 lines over the 500-line limit.

## AC check-off handling

All six criteria are already `- [x]` in `spec.md` and AC1-AC4 are mirrored `- [x]` in `issue.md`. Per
the reviewer's instructions for this run, **no checkbox was modified**. Under
`.claude/skills/acceptance-criteria-tracking`, a PARTIAL verdict would ordinarily leave the item
unchecked, so the check-off state of AC5 and AC6 is reported here for the maintainer to adjudicate
rather than changed unilaterally. The reviewer's position is that AC6 is a wording reconciliation and
AC5 is an artifact correction; neither warrants unchecking if the corrections in the code review's
recommendations 1 and 2 are made.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md`
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none. Two checked items are graded PARTIAL by this review — AC5 (the MTA measurement
  clause is not established) and AC6 (collector and storage-location wording) — and both are
  non-blocking.

## Verdict

**PASS. 0 blocking findings. No remediation cycle is required.**
