# Feature Audit — quickfiler-keyboard-action-defects (Issue #444, closes #472, #482)

- Artifact: `feature-audit.2026-08-27T20-34.md`
- Branch: `bug/quickfiler-keyboard-action-defects-444` @ `833423ba`
- Diff base: `4f238289` (merge-base with `origin/epic/quickfiler-bug-family-integration`)
- Work mode: `full-bug` — `spec.md` is the sole acceptance-criteria source
- `user-story.md`: NONE (intentionally absent for `full-bug`; not a gap)
- Plan: `plan.2026-08-24T20-33.md` — 167/167 tasks checked (verified by grep: 167 `[x]`, 0 `[ ]`)

## Verification Method

Each criterion was evaluated against the branch diff (`4f238289..HEAD`), the working tree, the
committed evidence dossier, and where applicable an independent reviewer re-run (grep/diff/XML
parse/CSharpier/`gh`). Baseline comparisons use `git show 4f238289:<path>`.

## Acceptance Criteria Evaluation

Legend: PASS = delivered and verified; DEFERRED = deliberately deferred to the integration PR body
per plan tasks [P5-T25]/[P5-T26]/[P5-T27], with one evidence artifact each; not a defect.

### Issue #444 (11 criteria)

| # | Criterion (abbreviated) | Verdict | Evidence / reviewer verification |
| --- | --- | --- | --- |
| 444-01 | `WireUpKeyboardHandler` zero hits (inherited from #468, verify only) | PASS | `evidence/baseline/p0-t12-upstream-468-verification.2026-08-27T09-45.md` recorded zero hits at Phase 0. Reviewer re-run at head: the identifier exists nowhere as a code member; the single textual hit is a doc-comment prose mention in the new test file (policy-audit OB-4). Substantive condition holds. |
| 444-02 | Duplicate registration recorded as satisfied upstream by #468, reported as inherited | PASS | Citation present in `## Repro & Evidence` of spec.md; this audit reports it as inherited, not delivered. |
| 444-03 | `Keys.Down` decision = `SelectNextItem()` recorded in `## Proposed Fix` with five citations | PASS | Spec section `### #444 — product decision` present with citations. |
| 444-04 | Enumerable ctor throws `ArgumentException` with `already exists` on duplicate (SourceId, StoredKeyEquals-equal Key) | PASS | Diff adds the guard; test `EnumerableConstructor_WhenSeedContainsDuplicateSourceAndStoredKey_ThrowsArgumentException` asserts `.WithMessage("*already exists*")`. |
| 444-05 | Guard compares via `StoredKeyEquals`, not `KeyEquals` | PASS | Diff shows `StoredKeyEquals(_list[i].Key, _list[j].Key)`; pinned by `EnumerableConstructor_WhenStoredKeysDifferButKeyEqualsOverlaps_DoesNotThrow` ("10"/"1" seed). |
| 444-06 | Pre-existing characterization test still passes unmodified | PASS | `Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate` unmodified in diff; suite 6713/6713 green. |
| 444-07 | Null seed still throws `ArgumentNullException`, not NRE | PASS | Materialization precedes the scan; `EnumerableConstructor_WhenListIsNull_ThrowsArgumentNullException`. |
| 444-08 | Duplicate-free seed and same-Key-different-SourceId seed both construct | PASS | Two named negative tests in `KbdActionsRemainingBranchesTests.cs`. |
| 444-09 | `logger.Error` immediately before throw, matching existing pattern | PASS | Diff shows `logger.Error(message);` then `throw`; matches the `FindIndex` pattern in the same file. |
| 444-10 | `RegisterAsyncKeyActions` registers exactly one Down (bound `SelectNextItemAsync`) and one Up | PASS | Decision-pin test in `QfcCollectionControllerNavigationDigitsTests.cs`; recorded pass-after-only, as the spec anticipates. |
| 444-11 | Duplicate-guard regression test observed red-then-green with both runs recorded | PASS | `evidence/qa-gates/p1-t3-444-red.2026-08-27T09-45.md` and `p1-t6-444-green.2026-08-27T09-45.md`. |

### Issue #472 (10 criteria)

| # | Criterion (abbreviated) | Verdict | Evidence / reviewer verification |
| --- | --- | --- | --- |
| 472-01 | New `private int _registeredDigits`, assigned in `RegisterNavigation` from the captured value | PASS | Verified in the diff. |
| 472-02 | `UnregisterNavigation` uses `_registeredDigits`; zero `Digits` reads in its body | PASS | Reviewer re-ran the source search: 0 occurrences of `Digits` in the method body. |
| 472-03 | Format written as `_registeredDigits == 2 ? "00" : ""` (uninitialized-object safety) | PASS | Verbatim in the diff, with why-comment; four pre-existing navigation tests pass (suite green, file untouched). |
| 472-04 | Register at 10 / unregister at 9 leaves only the unreachable `"10"` entry | PASS | `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys` asserts `remaining == ["10"]` exactly. |
| 472-05 | Width-fidelity test carries XML doc comment attributing the residual `"10"` to the promoted count-mismatch defect | PASS | Doc comment present, names the downstream-notes item and the out-of-scope status. |
| 472-06 | Mirror-direction test (register at 9, grow to 10, unregister) passes | PASS | `UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys`. |
| 472-07 | Four pre-existing navigation tests pass unmodified; `[TestMethod]` count of `QfcCollectionControllerTests.cs` unchanged | PASS | Path absent from the branch diff, so both counts are trivially identical; suite green. |
| 472-08 | `IQfcCollectionController.cs` not modified | PASS | Absent from `git diff --name-only 4f238289..HEAD`. |
| 472-09 | #472 regression test observed red-then-green, both recorded | PASS | `evidence/qa-gates/p2-t3-472-red.2026-08-27T09-45.md` and `p2-t7-472-green.2026-08-27T09-45.md`. |
| 472-10 | Count-mismatch defect promoted to potential entry AND GitHub issue AND issue number recorded in PR body | DEFERRED | Promotion and issue creation are complete: `docs/features/potential/promoted/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan.md` + issue #644, reviewer-verified OPEN via `gh issue view 644` (commit `12256da4`). The PR-body clause cannot be satisfied before the integration PR exists. Deferral recorded in `evidence/issue-updates/p5-t25-ac472-10-deferred.2026-08-27T20-16.md`. Left unchecked, correctly. |

### Issue #482 (12 criteria)

| # | Criterion (abbreviated) | Verdict | Evidence / reviewer verification |
| --- | --- | --- | --- |
| 482-01 | `SyncExpandedRegistrations(bool)` exists, no coverage exclusion, sole caller of the four register/unregister methods | PASS | Reviewer grep: the four method names occur in that file only inside `SyncExpandedRegistrations` (lines 188-193). No `[ExcludeFromCodeCoverage]` on the new member. |
| 482-02 | Both `ToggleState` overloads delegate to `SyncExpandedRegistrations(_expanded)` after `_expanded` is written | PASS | Verified in the diff for both overloads. |
| 482-03 | Both overloads retain accessibility, `virtual`, parameters, return type, coverage attribute | PASS | Diff touches only their bodies; signatures and attributes unchanged. |
| 482-04 | Async-On → sync-Off → async-On completes without `ArgumentException` | PASS | Named interleaving test; suite green. |
| 482-05 | Expanded state: both registries hold exactly one 'B' and one 'D' for EntryId | PASS | `BothRegistriesShouldHold(..., 1)` in the interleaving test. |
| 482-06 | Collapsed state: both registries hold zero 'B'/'D' entries | PASS | Named collapse-direction test. |
| 482-07 | Two consecutive On calls do not throw | PASS | Named idempotence test. |
| 482-08 | `SyncExpandedRegistrations` exercised directly for true and false; >= 90% line coverage as new member | PASS | Named `InvokeNonPublic` test; reviewer independently read the `<method>` node from the raw Cobertura final document: line-rate 1, branch-rate 1. |
| 482-09 | #482 test constructs no `System.Threading.Timer`; `UnRead` false established explicitly; no wall-clock wait | PASS | Harness pins `UnRead` false by guard assertion with documented rationale (setter writes through to `Item.Save()`); reviewer grep: no `Thread.Sleep`/`Task.Delay`/timer construction in the test file (code-review CR-3). |
| 482-10 | #482 regression test observed red-then-green (third step `ArgumentException`), both recorded | PASS | `evidence/qa-gates/p3-t3-482-red.2026-08-27T09-45.md` and `p3-t8-482-green.2026-08-27T09-45.md`. |
| 482-11 | Deliberate behavior widening stated in the PR body | DEFERRED | The widening is stated in the spec; the PR body does not exist yet. Deferral recorded in `evidence/issue-updates/p5-t26-ac482-11-deferred.2026-08-27T20-16.md`. Left unchecked, correctly. |
| 482-12 | #482 filed-trigger/severity correction stated in this spec AND repeated in the PR body | DEFERRED | The correction is present in the spec (unreachable `QfcCollectionController.cs:1439` trigger; live trigger Right → Down → Right; symptom is a dead key, caught at `KeyboardHandler.cs:141-147`). The PR-body half is pending. Deferral recorded in `evidence/issue-updates/p5-t27-ac482-12-deferred.2026-08-27T20-17.md`. Left unchecked, correctly. |

### Upstream contract and scope discipline (11 criteria)

| # | Criterion (abbreviated) | Verdict | Evidence / reviewer verification |
| --- | --- | --- | --- |
| SC-01 | Upstream contract tables match delivered code exactly | PASS | Diff adds only: private ctor guard logic in `KbdActions.cs` (no signature change), private `SyncExpandedRegistrations` in `Navigation.cs`; no member outside the tables changed. |
| SC-02 | `KeyboardHandler.cs` not modified | PASS | Absent from diff name list. |
| SC-03 | `IQfcCollectionController.cs` not modified | PASS | Absent from diff name list. |
| SC-04 | None of the nine other `QfcItemController` partials modified | PASS | Reviewer grep of the diff name list: no match. |
| SC-05 | Production-file list is a subset of the three permitted paths | PASS | Exactly the three paths (numstat). |
| SC-06 | `KbdActions.Remove` retains `bool` return and silent `false`; no `TryRemove` member | PASS | `Remove` untouched by the diff; `TryRemove` absent from all source (grep hits only vendored binaries). |
| SC-07 | No public-API change | PASS | New members are one private field and one private method; the ctor overload pre-existed. |
| SC-08 | #484's timer-factory seam declined in spec; no seam in the diff | PASS | Spec section present; no timer-factory in the diff. |
| SC-09 | Phase 0 re-derived anchors; no post-#468 line number transcribed into the plan | PASS | `evidence/baseline/p0-t13/p0-t15` anchor artifacts; plan anchors are member-name based ([P5-T8] verification). |
| SC-10 | `NoLiveFormInTestAssemblyTests` passes; no Form-derived type added | PASS | Suite green; `evidence/regression-testing/p5-t7-no-live-form.2026-08-27T20-11.md`. |
| SC-11 | New test file registered by a single `<Compile Include>` line in the reserved slot; nothing else changed in the `.csproj` | PASS | Reviewer read the +1/-0 diff hunk directly; insertion is between the two named neighbours. |

### File-size, toolchain, and coverage (13 criteria)

| # | Criterion (abbreviated) | Verdict | Evidence / reviewer verification |
| --- | --- | --- | --- |
| QA-01 | No added file > 500 lines; changed pre-existing files at/below cap or no larger than baseline | PASS | Policy-audit section 5 table; `QfcCollectionController.cs` excess is pre-existing, size unchanged, remediation forbidden to this feature (plan D-P6). |
| QA-02 | `QfcCollectionControllerTests.cs` unchanged (line and `[TestMethod]` counts) | PASS | Path absent from diff. |
| QA-03 | CSharpier check zero unformatted in final pass | PASS | Evidence EXIT_CODE 0 over 1541 files; independent reviewer re-check of the 7 changed files, exit 0. |
| QA-04 | Analyzer Rebuild zero errors, no new warnings | PASS | `p4-t4-analyzers` EXIT_CODE 0; the 5 warnings are the pre-existing System.Reactive diagnostic. |
| QA-05 | Nullable/TreatWarningsAsErrors Rebuild zero errors, no `/p:Nullable=enable` | PASS | `p4-t5-typecheck` EXIT_CODE 0; command recorded verbatim without the forbidden property. |
| QA-06 | vstest with coverage/isolation/filter, zero failures, `\.claude\` exclusion | PASS | 6713/6713; TRX independently parsed by the reviewer. |
| QA-07 | All four steps passed in a single final pass; commands stated in completion report | PASS | `p4-t12-clean-pass` and `qa-gates/p5-t10-completion-report.2026-08-27T20-14.md`. |
| QA-08 | `SyncExpandedRegistrations` >= 90% line coverage | PASS | 100% line — reviewer re-read the raw Cobertura `<method>` node. |
| QA-09 | New guard branch covered on throwing and non-throwing paths | PASS | Lines 61 and 65 both hits=1 — reviewer re-read from raw Cobertura. |
| QA-10 | Phase 0 coverage baseline captured; final shows no regression (line, branch, changed lines) | PASS | Baseline 85.04/79.12 vs final 85.13/79.21 (deltas +0.09/+0.09); both changed-file rates rose; reviewer re-derived all four repo-wide figures from the raw XML documents. |
| QA-11 | 80-vs-85 coverage-policy conflict recorded as pre-existing and unresolved | PASS | Recorded in `p4-t11` and the completion report; policy-audit OB-2. |
| QA-12 | No plan acceptance claims coverage attributable to `QfcCollectionController.cs` (excluded class) | PASS | `p4-t11` carries the explicit no-attribution statement; XPath returns no node for that filename in either document. |
| QA-13 | All evidence under canonical `evidence/<kind>/`; working tree clean at completion | PASS | 85/85 files canonical, zero under `artifacts/`; reviewer re-ran `git status --porcelain` — empty. |

## Deferred Items — Disposition

The three DEFERRED criteria (472-10, 482-11, 482-12) share one blocker: each requires a statement in
the integration PR body, which does not exist at review time. The plan recorded them as explicit
deferrals at [P5-T25], [P5-T26], [P5-T27] with one evidence artifact each. They are evaluated as
deferred-pending-PR-body, not as failures, and this review does not check them off; the orchestrator
satisfies and checks them when the PR body is authored. Substantive prerequisites already met:
issue #644 exists and is OPEN (reviewer-verified), the widening and the trigger/severity correction
are both stated in the spec.

## Baseline-Relative Verification

- Deletion invariant: `git diff --numstat 4f238289..HEAD | awk '$1==0 && $2>0'` — zero rows
  (reviewer re-run). No file loses content the base gained.
- Fail-before states for all three defects captured at Phase 0 against the base; pass-after states
  captured post-fix; the suite grew from 6686 to 6713 passed tests (+27) with zero failures.
- Repo-wide coverage moved up on both axes relative to the Phase 0 baseline (+0.09 line, +0.09
  branch, identical command and denominator).

### Acceptance Criteria Status

- Source: `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md` (sole source; work mode `full-bug`)
- Total AC items: 57
- Checked off (delivered): 54
- Remaining (unchecked): 3
- Items remaining: 472-10 (PR-body clause of the #644 promotion), 482-11 (behavior widening stated
  in PR body), 482-12 (trigger/severity correction repeated in PR body) — all three
  deferred-pending-PR-body by design; no newly checked-off items were added by this review (all 54
  PASS items were already checked, correctly).

## Verdict

GO from this reviewer's standpoint: 0 Blocking findings, 54/57 acceptance criteria PASS, 3/57
deferred by design to the integration PR body. Outstanding for the orchestrator: author the PR body
with the three required statements, then check off 472-10, 482-11, 482-12; perform the final
merge-up onto the epic tip (10 sibling commits landed after this branch's last merge-up).
