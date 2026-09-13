# Feature Audit — Issue #743: QuickFiler `ItemViewer` UI-marshalling seam

- **Feature folder:** `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743`
- **Audit timestamp:** 2026-09-13T15-30
- **Work mode:** `full-bug` (`issue.md` line 12). AC source per the `acceptance-criteria-tracking` skill: `spec.md` only, section `## Acceptance Criteria` (lines 409-488). `user-story.md` exists but is non-authoritative and carries no checkboxes.
- **Companion artifacts:** `policy-audit.2026-09-13T15-30.md`, `code-review.2026-09-13T15-30.md` (same folder, same timestamp).

## Scope and Baseline

- **Branch:** `bug/quickfiler-itemviewer-ui-marshalling-seam-743`, head `06773349ad8861d18bd0dd1265aa8732cc19037a` (21 commits over `origin/main`).
- **Base:** `origin/main` at `39ce2892b90ce9e8d7a4311c12195f1a06392f5b`; merged into the branch at `c358b2d809ca58db0197eb10229f872f2e9a924e` (sibling item #583's `KaStringAsync` files; no Write Set file touched). The plan's self-anchored diff base `refs/plan/issue-743-base` was re-pointed to that merge commit before any source edit (`evidence/baseline/phase0-diff-base.2026-09-12T16-30.md`, addendum). Base SHAs are as supplied by the orchestrator; this review had no shell and did not recompute them.
- **Changed files (from the orchestrator's name-status, cross-checked against the executor's anchored diff audit and the current tree):** seven source paths equal to the spec's binding `## Write Set` (lines 492-502): `QuickFiler/Viewers/IItemViewer.cs` (M), `QuickFiler/Viewers/ItemViewer.cs` (M), `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (M), `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs` (A), `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (M), `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (M), `QuickFiler.Test/QuickFiler.Test.csproj` (M); the feature folder (A); eleven `.claude/agent-memory` files (M/A).
- **Plan:** `plan.2026-09-12T13-23.md`, status `Executed`, 64/64 tasks checked, Decisions Record D1-D11 read in full. Branch COST selected at P4-T1 on the P1-T11 verdict.
- **Baseline figures (same session, executor artifacts):** serial `QuickFiler.Test` 1394/1394; analyzer and nullable Rebuilds `0 Warning(s)`/`0 Error(s)`; ViewerSetup.cs 190/210 = 0.904762; Initialization.cs 249/262 = 0.950382; file sizes 200/400/467/497/498/278/353.
- **Post-change figures:** serial 1400/1400 (six tests added); both Rebuilds `0`/`0`; csharpier check exit 0; ViewerSetup.cs 193/213 = 0.906103; Initialization.cs unchanged; file sizes 212/406/478/497/498/304/396/312.
- **Evidence discovery order applied:** `evidence/issue-updates/` (1 file), `evidence/regression-testing/` (7), `evidence/other/` (14), `evidence/qa-gates/` (17), `evidence/baseline/` (15); no `remediation-baseline/`; no epic. Fail-before search: a failing-run artifact exists (`ac2-fail-before-three-runs`), so no exception dossier was required; SearchScope `evidence/regression-testing/`, SearchPatterns `ac2-fail-before*.md`, `fail-before-exception.*.md`, SearchResult the former found, the latter none (not needed).

## Acceptance Criteria Inventory

Five checkbox criteria in `spec.md`, all `- [x]` on disk (checked by the executor at P6-T10 through P6-T14). AC3 carries two components tracked as AC3A (blocking) and AC3B (supporting).

| ID | Criterion (abbreviated) | Spec lines | On-disk state |
|---|---|---|---|
| AC1 | Mechanism identified by measurement, not inference: artifact under `evidence/baseline/` with (i) observable declared in advance, (ii) measured value with command and load condition, (iii) rejected hypothesis named with the rejecting observation, (iv) per-test durations; FAILS on static-only reasoning, on the removed identifiers, or on agreement with both; a no-expiry run "is a recorded negative result, not a pass" | 413-425 | `[x]` |
| AC2 | Deterministic regression test (file + method): (i) fails 3x pre-change and passes 3x post-change, same machine and session, transcribed under `evidence/regression-testing/`; (ii) grep for banned constructs empty and transcribed; (iii) single `[Timeout]` only; (iv) structural assertion | 427-436 | `[x]` |
| AC3 | (a) BLOCKING: the AC2 member completes with the synchronous `IUiDispatcher` double and no `WinFormsPumpHost`, constructing zero concrete `ItemViewer` instances, asserted structurally in one run; (b) SUPPORTING: at least 62 consecutive clean targeted runs with N, p = (20/21)^N, scope statement and the base-rate interval caveat | 438-456 | `[x]` |
| AC4 | Coverage of the two controller partials retained or improved: (i) fresh pre/post same session same command, cross-session figures not used; (ii) denominator stated, `InitializeWebViewAsync` exclusion recorded; (iii) post >= pre for both; (iv) denominator delta accounted; (v) every section 7 disposition-table test exists and passes | 458-476 | `[x]` |
| AC5 | #511 and #571 reconciled: (i) artifact under `evidence/issue-updates/` quoting the premise correction and confirming its mechanics; (ii) one comment per issue replacing the #592 pointer with #743; (iii) the gate hypothesis marked superseded citing C1; URLs recorded; no claim the refutation was in error | 478-488 | `[x]` |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence the verdict rests on | Summary |
|---|---|---|---|
| AC1 | **PARTIAL** | `evidence/baseline/ac1-observable-declaration.2026-09-12T16-30.md`; `evidence/baseline/ac1-serial-measurement.2026-09-12T17-00.md`; `evidence/baseline/ac1-parallel-measurement.2026-09-12T17-00.md`; `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md`; `evidence/regression-testing/p4-branch-confirmation.2026-09-12T18-30.md`; spec lines 160-193, 413-425, 531 | Components (i), (ii) and (iv) PASS. Component (iii) and the no-expiry clause do not: both instrumented runs recorded `timeout=0`, H-LEAK is by definition a cascade following an expiry, so the serial `contended=0` reading is predetermined and cannot reject H-LEAK; the spec states a no-expiry run is "a recorded negative result, not a pass" and "Escalate rather than infer". The artifact records the negative result plainly but still checks the box. |
| AC2 | **PASS** (recorded deviation) | `evidence/regression-testing/ac2-fail-before-three-runs.2026-09-12T17-30.md`; `evidence/regression-testing/ac2-pass-after-three-runs.2026-09-12T18-00.md`; `evidence/regression-testing/ac2-determinism-audit.2026-09-12T18-00.md`; `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs` | Named test: `QfcItemController_SeamMarshallingTests.ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups`. (i) 3 of 3 fail (`InvalidCastException`), 3 of 3 pass, same machine and session; deviation: arrangement order corrected between the two run sets, failure mechanism shown pre-await and order-independent. (ii) grep empty, transcribed. (iii) five `[Timeout(` attributes, one per test. (iv) structural assertions. |
| AC3 | **PASS** | (a) `evidence/regression-testing/ac3a-deterministic-efficacy.2026-09-12T18-00.md` (P3-T6 run 1); (b) `evidence/regression-testing/ac3b-consecutive-runs.2026-09-12T19-00.md` | (a) Test 1 and test 3 passed in one run; `WinFormsPumpHost` occurs zero times in the seam file (verified by reading); the controller was built with the synchronous double. (b) `RUNS=62 FAILURES=0`, p = (20/21)^62 = 0.048558, targeted scope named as the seam class in the serial regime, the "targeted scope only" sentence and the base-rate interval caveat both present. |
| AC4 | **PASS** | `evidence/baseline/phase0-coverage-prechange.2026-09-12T16-30.md`; `evidence/qa-gates/ac4-coverage-comparison.2026-09-12T19-30.md`; `evidence/qa-gates/final-serial-test-run.2026-09-12T19-30.md` | (i) both measurements 2026-09-13, same command and extraction, cross-session figures explicitly not used; (ii) denominators 210 and 262 stated, `InitializeWebViewAsync` exclusion at line 47 recorded; (iii) 0.906103 >= 0.904762 and 0.950382 = 0.950382; (iv) +3 valid / +3 covered accounted line by line to the null-tolerant marshal; (v) every disposition-table test cross-checked by name against the P6-T5 per-test rows, all Passed. |
| AC5 | **PASS** (artifact-based) | `evidence/issue-updates/issue-511-and-571-reconciliation.2026-09-12T19-00.md` | (i) premise correction quoted, mechanics re-verified with the Designer `EndInit` line correction (6165/6166); (ii) `PostedAs: comment`, URLs `.../issues/511#issuecomment-5652002368` and `.../issues/571#issuecomment-5652002536`; (iii) gate hypothesis marked superseded citing C1; no claim the refutation was in error. The live GitHub state of the two comments was not fetched in this review (no `gh`); the verdict rests on the mirror artifact. |

### AC1 — detailed evaluation

What the evidence establishes:

- (i) PASS. The observable ("any acquisition of the one-permit `TransactionGate` finds the permit held with no live holder"), its operationalisation as three counters and a balance test, the decision-rule table and the load condition were declared at P0-T11 (2026-09-13T02-36) before P1-T6/P1-T7 added the instrumentation; the declaration records zero `Interlocked.Increment` occurrences at the time of writing.
- (ii) PASS. Serial regime `acquisitions=11 releases=10 contended=0` (R4 excluded by filter, 1394 tests, `timeout=0`); parallel regime `19/18/14` (1395 tests, `timeout=0`); exact commands and load conditions recorded; confirmed unchanged in kind at P4-T3 (`11/10/0`, `19/18/14`).
- (iv) PASS. Six `ThroughThePumpHost` durations per regime (largest serial 124.5 ms; largest parallel 6,460.4 ms, a 58x elongation under class-level parallelism on an idle machine) with the arithmetic against the 60,000 ms bound and the recorded 6x-26x load multiplier.
- FAIL triggers not met: the artifact does not reason only from static reading; it names neither removed identifier outside the mandated quotation; it does not report agreement with both hypotheses.

What the evidence does not establish:

- (iii) PARTIAL. The artifact names the rejected hypothesis (H-LEAK) and gives an observation (`contended=0`, balance difference 1). But H-LEAK is defined in spec section 4.2 as a leak that occurs when MSTest stops observing a timed-out `async` test, so that its `finally` never releases the permit. In a run with zero expiries no test was abandoned, so no leak could occur whichever hypothesis is true; the serial observable therefore had only one value it could take, and observing that value is consistent with both hypotheses. A further structural point (code review, N-3): under H-LEAK the serial-regime signature would be the balance test blocking on `WaitAsync` and expiring under its own `[Timeout]` with no `GATECOUNTERS` line printed, so rows 2 and 3 of the decision-rule table cannot be observed as printed counter values; only row 1 can. The verdict's phrase "REJECTED by direct observation" overstates what was observed.
- No-expiry clause. Spec AC1's final sentence: "If the instrumented run produces no expiry at all, that is a recorded negative result, not a pass." The risk table (spec line 531): "A recorded negative result is an honest outcome, not a pass. Escalate rather than infer." The artifact records the negative result plainly (section "Expiry statement") and then infers. The plan's P1-T11 task text treated the negative result as something to state rather than as a bar to passing; the spec is the authority over the plan.

What can be said in H-COST's favour on this evidence: H-LEAK cannot be an originating mechanism (it amplifies an initial expiry into a cluster), so the originating mechanism of any expiry is elapsed fixture cost by elimination within the spec's own two-hypothesis frame; the parallel-regime measurement shows that cost reaching 6,460 ms with no external load, and 6,460 ms times the upper recorded multiplier exceeds the bound. That is sufficiency evidence for H-COST as originator. It leaves the cascade question (spec unknown U2) open and untested, which is what the artifact should say.

Verdict: PARTIAL. Resolution paths, cheapest first: (a) amend section (iii) of the verdict artifact to the wording above, record U2 as still open, and obtain the maintainer's explicit ratification that the negative result is accepted for AC1, transcribed into `issue.md` (a gitignored orchestrator-state note is not sufficient); or (b) an instrumented reproduction that produces an expiry, which the spec anticipates may be infeasible. The instrumentation code needs no change under either path. Per the check-off protocol, AC1 should be `- [ ]` until one path completes; this review did not edit `spec.md` (orchestrator directive).

### AC2 — detailed evaluation, including the same-test question

- The named test is cited as file path plus method name in the executor's acceptance-status artifact and in the plan's AC-MAPPING; the method exists at `SeamMarshallingTests.cs` lines 180-221.
- (i) Six outcomes transcribed: P2-T9 runs 1-3 on the defect-preserving intermediate (`bce810495`): `total=5 passed=2 failed=3` each, tests 1 and 2 `InvalidCastException`, test 5 `NullReferenceException`; P3-T6 runs 1-3 on the fixed tree (`cc236c8d2`): `5/5` each. Same machine, same session (2026-09-13T03-04 and T03-28).
- Deviation: between the two run sets the executor moved the ambient `SynchronizationContext` installation to after the WinForms control construction, because a `Control` constructor replaces an ambient context of exact type `SynchronizationContext` and the original order hung (first pass-after attempt: tests 1 and 2 `timed out after 60000ms`, recorded in `ac2-pass-after-three-runs`). Why the fail-before outcome is unaffected: the recorded exception type is the proof. An `InvalidCastException` naming `Castle.Proxies.IItemViewerProxy` can only come from the `(ItemViewer)itemViewer` cast P2-T3 placed in the argument expression of the first `CreateAsync` call, which C# evaluates before the call and therefore before the member's first `await`; the returned `Task` is faulted synchronously. Under the old arrangement, reaching an `await` would have produced a timeout (as it did on the fixed tree), not an exception. Test 5's `NullReferenceException` likewise arises from `_itemViewer.UiDispatcher.InvokeAsync` on a mock that returns null for the sealed `Dispatcher` type, before any await. The arrangement change only affects behaviour after the first await. Residual: the corrected file was never literally run against the intermediate, and the statement that the correction was confined to arrangement order could not be checked by diff in this review; a literal re-run is recommended, not required.
- (ii) PASS: `Select-String` over the seam file for `Thread\.Sleep|Task\.Delay|Stopwatch|DateTime\.Now|DateTime\.UtcNow|Environment\.TickCount|\bwhile\b` returned an empty match list, transcribed verbatim; confirmed by this review's own read of the file.
- (iii) PASS: `[Timeout(` count 5, one per test, all `SeamTimeoutMs`; no other time-valued construct.
- (iv) PASS: assertions are counts, non-emptiness, Moq interaction verifications and a reflection parameter-type check; none is an elapsed duration.
- Relation to AC1's mechanism: with H-COST operative, spec section 7.1 row 2 defines the regression test's form as "the member completes with a synchronous dispatcher and constructs no real viewer"; the named test is of that form.

### AC3 — notes

- (a) The structural zero-construction assertion `NotBeAssignableTo<ItemViewer>` is tautological for an interface Moq proxy (code review N-5); the effective proof that no concrete viewer is required is that the `await` completed on the fixed tree and faulted on the intermediate. Component (a) still PASSES as specified, because the spec asks for a structural assertion in one run and the run exists. Strengthening the assertion is recommended.
- (b) The targeted scope is the seam test class, which by construction never builds a real viewer, so the 62-run streak demonstrates that class's stability rather than the pump-hosted tests'. The spec defines the scope that way and labels the component supporting; PASS as specified.

### AC4 — notes

- The post-change coverage run recorded three environmental failures in `QfcInitEmailQueueZeroBatchTests` (Deedle `TypeInitializationException`); they are outside both subject files and the AC4 accounting is line-level for the subject file, so the verdict stands. Root `lines-covered` fell by 11 between the runs, plausibly for the same reason; not a changed-line regression.
- Branch rate was not transcribed (code review N-8); AC4 is phrased over line rate only.
- Component (v) was cross-checked by name: every test named in spec section 7.1 and 7.2 (five seam tests, the balance test, six gate tests, the Part3 initialization class including the five `ThroughThePumpHost` tests, the two seam-factory `Create*` tests, `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups`, eight breadcrumb-host tests, fourteen contract tests) appears with outcome `Passed` in `final-serial-test-run`.

### AC5 — notes

- The mirror artifact satisfies every content requirement of the evidence-and-timestamp skill; its filename combines two issues into one file rather than `issue-<N>.<timestamp>.md` (observation).
- The comment text is measured and does not describe the earlier refutation as wrong; it corrects the Designer line citation (6166-6167 to 6165-6166) and re-verifies the mechanics against the current tree.
- Live state not fetched: this review had no `gh`; the two comment URLs carry sequential comment ids (5652002368, 5652002536) consistent with two posts in the same minute.

## Orchestrator-Requested Assessments (summary; full text in the code review)

1. AC2(i) same-test requirement: PASS with recorded deviation (reasoning above).
2. `user-story.md` present in a `full-bug` folder: Observation-level policy deviation; non-blocking; no AC-source ambiguity.
3. Evidence hygiene: one host-path hit, `evidence/other/orchestrator-citation-verification.2026-09-12T13-50.md:6`, Blocking; zero hits in the seven Write Set files; projections-only convention honoured (570/570 tracked raw artifacts, 0 untracked).
4. Out-of-scope defects: listed below as follow-ups.
5. Null-tolerance branch: unreachable in production (every production construction path assigns `_uiDispatcher` before `AssignControlsAsync` can run; only the protected parameterless constructor used by the test harness leaves it null); comment adequate on reachability, second clause unclear (non-blocking wording fix).

## Out-of-Scope Follow-ups (not findings against this item)

1. Meziantou.Analyzer HintPath skew: `UtilitiesCS/UtilitiesCS.csproj` line 1308 and `VBFunctions/VBFunctions.csproj` line 58 reference `3.0.203`; lines 3/1300 and 3/73 of the same files and both `packages.config` files reference `3.0.235` (Grep-confirmed). A cold restore fails with CS0006 until `3.0.203` is installed manually. Pre-dates the branch. Promote to an issue.
2. `QfcInitEmailQueueZeroBatchTests` intermittent `TypeInitializationException` (`Deedle.Reflection`, `netstandard 2.1.0.0`) under class-level parallel runs; passes serially and on re-run; two occurrences in this run. Promote to an issue.
3. Test-name drift: `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher` (ViewerSetupTests 309-344) now exercises the null-tolerance branch; rename and simplify in a follow-up (file is outside this item's Write Set and at 498 lines; the change is net-negative).
4. Policy-level: the maintainer's projections-only decision (#671) versus the feature-review artifact-path rule for `artifacts/csharp/coverage.xml`; reconcile so future reviews do not record a procedural coverage FAIL by construction.
5. Evidence-timestamp convention (12-hour offset between executor artifacts and orchestrator receipts).

## Remediation-Required Findings

The launching directive states "Write no other file", so `remediation-inputs.2026-09-13T15-30.md` was not written. The findings that would populate it are recorded here so the orchestrator can author or request that artifact.

| # | Finding | Severity | Location | Required action | Artifact that carries the detail |
|---|---|---|---|---|---|
| R-1 | Absolute user-profile path with the account name in a branch-added evidence file | Blocking | `evidence/other/orchestrator-citation-verification.2026-09-12T13-50.md` line 6 | Substitute `<repo-root>/.claude/worktrees/agent-a190dd2fffe21a25d`; squash-merge the branch; re-run the branch-scoped hygiene sweep including `.claude/agent-memory` | `policy-audit.2026-09-13T15-30.md` section 8 item 1; `code-review.2026-09-13T15-30.md` finding 1 |
| R-2 | AC1 verdict overstates the observable; spec no-expiry clause forecloses PASS | Blocking for acceptance | `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md` section (iii); `spec.md` AC1 checkbox | Amend the verdict wording (H-COST as the only originating mechanism; U2 open); maintainer ratification transcribed into `issue.md`; uncheck AC1 until then | this document, AC1; `code-review.2026-09-13T15-30.md` finding 2 |
| R-3 | `.claude/agent-memory` host-path attribution unresolved | Potentially Blocking | eleven memory files on the branch | `git diff origin/main...HEAD -- .claude/agent-memory` filtered for the account name or a `C:` drive path with a `Users` segment; sanitize any added occurrence | `policy-audit.2026-09-13T15-30.md` section 8 item 7 |

Non-blocking items (N-1 through N-8 in the code review) may be bundled with R-1's commit or deferred to the follow-ups above.

## Acceptance Criteria Check-off

- On-disk state: all five criteria are `- [x]` in `spec.md` (executor check-offs P6-T10 to P6-T14; checkbox characters only were changed, verified by reading lines 413, 427, 438, 458, 478).
- Reviewer action: none. The launching directive assigns the uncheck to the orchestrator; this review did not edit `spec.md`.
- Required correction: AC1 must be returned to `- [ ]` until remediation R-2 completes (check-off protocol rule 4: leave unmet items unchecked and document the gap). AC2, AC3, AC4 and AC5 remain checked.
- Newly checked-off items by this review: none (all PASS items were already checked).

## Summary

- **Verdict: NOT ACCEPTED as delivered — 4 of 5 criteria PASS, AC1 PARTIAL; 2 Blocking findings, neither in production code.**
- The production change is correct and complete for its stated purpose: the member can be driven through `IItemViewer` with the synchronous dispatcher double and no pump host; the pump-hosted tests are unchanged; coverage of the two named partials is retained or improved; the toolchain is clean on one pass.
- The two blockers are (R-1) a one-token hygiene substitution plus squash-merge, and (R-2) a documentation amendment plus an explicit maintainer ratification of the recorded negative result, after which AC1 can be re-checked. R-3 is a verification the orchestrator can run in one command.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-02-quickfiler-itemviewer-ui-marshalling-seam-743/spec.md` (`## Acceptance Criteria`)
- Total AC items: 5
- Checked off (delivered): 5 on disk; 4 verified PASS by this review (AC2, AC3, AC4, AC5)
- Remaining (unchecked): 0 on disk; 1 to be unchecked by the orchestrator (AC1, evaluated PARTIAL)
- Items remaining: `**AC1 — Mechanism identified by measurement, not inference.**` (spec lines 413-425) pending remediation R-2
