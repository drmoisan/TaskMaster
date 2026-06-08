# Code Review: Outlook Startup UI Lockup Follow-up (#148)

**Review Date:** 2026-05-07  
**Reviewer:** GitHub Copilot  
**Feature Folder:** `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148`  
**Feature Folder Selection Rule:** Explicit user-provided active feature folder for issue `#148`  
**Base Branch:** `development`  
**Head Branch:** `bug/outlook-startup-ui-lockup-followup-148` working tree  
**Review Type:** Initial post-implementation feature review

---

## Executive Summary

This review covers the working-tree implementation associated with issue `#148`. The refreshed PR context shows the requested base ref `development` and the head ref resolving to the same commit SHA, so the review had to use the canonical feature evidence plus the current unstaged working tree rather than a committed branch range.

The implementation direction is technically coherent: the branch adds timing instrumentation, introduces explicit selection and snapshot boundaries, and tries to confine Outlook COM access to UI-thread-owned stages. The problem is not that the core idea is unsound. The problem is that the current branch still fails the saved coverage gate, still lacks the required manual Outlook validation, includes additional unstaged scope drift not reflected in the feature end-state artifact, and uses several implementation-text tests that are weaker than the behavior-driven regression coverage expected for a bug fix of this size.

**What changed:**
- Startup timing instrumentation and batching logic in `TaskMaster/AppGlobals/AppEvents.cs`
- Selection-staging and model-boundary refactors in `QuickFiler/Controllers/EfcHomeController.cs` and `QuickFiler/Controllers/EfcDataModel.cs`
- Resolver/dataframe/table/mail-item snapshot-boundary refactors in `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, and `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`
- New or updated MSTest homes for the eight primary areas, plus additional unrelated test and `.csproj` churn still present in the working tree

**Top 3 risks:**
1. The saved coverage verdict remains failing, so the branch still does not satisfy the plan’s gating quality bar.
2. The actual working tree contains undeclared test and project-file churn outside the primary scope, which makes merge readiness ambiguous.
3. Several new regressions check source text rather than runtime behavior, so they may preserve structure without adequately protecting the bug fix.

**PR readiness recommendation:** **Blocked** — the branch should not move to PR or validator flow until coverage, scope reconciliation, and review findings below are addressed.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/csharp-coverage-summary.2026-05-07T21-19-46-04-00.md` | Coverage Policy Evaluation | The saved Phase 6 coverage gate fails: repository coverage is `21.82%` and changed/new-code coverage is `46.07%`. | Add deterministic behavioral coverage for the eight changed primary production files until the saved summary reaches `Coverage Conclusion: PASS`. | The plan explicitly gates manual Outlook validation and validator readiness on passing coverage. | `evidence/qa-gates/csharp-coverage-summary.2026-05-07T21-19-46-04-00.md`, `evidence/qa-gates/full-bug-end-state.2026-05-07T21-20-23-04-00.md` |
| Blocker | `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/outlook-manual-validation.2026-05-07T21-19-59-04-00.md` | Manual validation gate | The key user-visible acceptance criterion remains unverified because manual Outlook validation was not performed. | Re-run coverage first; once coverage passes, complete the required Outlook repro validation and update the end-state artifact. | The bug’s central promise is improved responsiveness during the live repro path; that cannot be claimed while the validation step remains blocked. | `evidence/qa-gates/outlook-manual-validation.2026-05-07T21-19-59-04-00.md`, `spec.md` acceptance criteria |
| Major | `artifacts/pr_context.appendix.txt` | Current working tree status | The working tree contains additional unstaged scope drift, including multiple `.csproj` edits and unrelated QuickFiler test changes, beyond the changed-file list declared in the feature end-state artifact. | Remove the unrelated changes from this branch or explicitly revise scope artifacts to include them before any new QA pass. | The branch cannot be reviewed deterministically when the actual diff and the declared scope diverge. | `artifacts/pr_context.appendix.txt`; current git status section; `evidence/qa-gates/full-bug-end-state.2026-05-07T21-20-23-04-00.md` |
| Major | `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs` | File structure | Five changed production files exceed the repository’s 500-line limit. | Extract focused helpers or split responsibilities so the touched files comply with the repository structure rule. | Oversized files materially increase review difficulty and make future regressions harder to isolate. | Current review line-count command: `ConversationResolver.cs|594`, `DfDeedle.cs|597`, `ConversationHelper.cs|632`, `MailItemHelper.cs|1096`, `OlTableExtensions.cs|1235` |
| Major | `TaskMaster.Test/AppGlobals/AppEventsTests.cs`, `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, `QuickFiler.Test/Controllers/EfcDataModelTests.cs`, `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`, `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelper_ExtendedTests.cs`, `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`, `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | New regression tests | Many issue-specific tests assert source text or method-body strings rather than observable runtime behavior. | Replace or supplement these tests with seam-based behavioral tests that exercise snapshot handoff, batching, cancellation, and publication cadence through mocks or data fixtures. | Source-text assertions can pass while behavior regresses, and they do not adequately explain the current `46.07%` changed-code coverage result. | Inspected test files; `evidence/qa-gates/targeted-regression.2026-05-07T21-19-36-04-00.md`; `evidence/qa-gates/csharp-coverage-summary.2026-05-07T21-19-46-04-00.md` |
| Minor | `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/baseline/csharp-mstest-coverage.2026-05-07T20-09-30-04-00.md` | Baseline capture | The saved baseline coverage artifact recorded `0.00` coverage because no test assemblies were found, which weakens the later no-regression comparison. | Capture a meaningful baseline from a built branch state before relying on delta-based coverage reasoning in future re-runs. | The saved summary can claim no regression only because the baseline is effectively empty. | `evidence/baseline/csharp-mstest-coverage.2026-05-07T20-09-30-04-00.md`; `evidence/qa-gates/csharp-coverage-summary.2026-05-07T21-19-46-04-00.md` |

---

## Implementation Audit

### C# implementation audit

#### What changed well

- The implementation consistently pushes toward an explicit `UI-thread snapshot -> background transform -> final publication` shape across startup and first-selection paths.
- `AppEvents.cs` now distinguishes startup timing envelopes, batching checkpoints, and completion timing instead of leaving startup processing as a single opaque segment.
- `EfcHomeController.cs` introduces `HandleSelectionChangedAsync(...)` and an explicit selection snapshot boundary before model creation, which is the correct architectural direction for the first-selection path.
- `ConversationResolver.cs`, `DfDeedle.cs`, `ConversationHelper.cs`, `MailItemHelper.cs`, and `OlTableExtensions.cs` all make the intended snapshot boundaries more explicit than before.

#### Type safety and API notes

- The Phase 6 nullable build passes with `0` warnings and `0` errors, so the branch does not appear to introduce new nullability problems.
- The new production changes do not appear to widen the public API significantly.
- The current issue is not API clarity; it is insufficient behavioral validation of the new boundaries and unresolved scope reconciliation in the working tree.

#### Error handling and logging

- Timing/logging coverage is materially improved and now supports attribution of startup segments versus first-selection segments.
- The branch continues to respect the rule that Outlook COM access must stay on the Outlook-owned thread, at least by design and according to the static evidence captured in the feature folder.
- The review cannot yet certify the end-to-end user-visible outcome because the manual Outlook validation step is still blocked by the coverage gate.

---

## Test Quality Audit

The test story is mixed. The branch has broad issue-specific regression intent, and the saved focused red/green evidence shows a disciplined regression-first workflow. The weakness is that too many of the newly added tests lock down source structure instead of runtime behavior, while overall changed-code coverage still stops at `46.07%`.

### Reviewed test and QA artifacts

- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/regression-testing/p2-t1-appevents-startup-timing.2026-05-07T20-25-50-04-00.md` — verifies the red state for AppEvents startup-timing instrumentation.
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/regression-testing/p5-t1-appevents-green.2026-05-07T21-11-12-04-00.md` — verifies the green state for the two AppEvents regressions.
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/regression-testing/p5-t2-controller-model-green.2026-05-07T21-11-55-04-00.md` — verifies green QuickFiler stage-boundary regressions.
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/regression-testing/p5-t3-utilities-green.2026-05-07T21-12-31-04-00.md` — verifies green Utilities stage-boundary regressions.
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/csharp-mstest-coverage.2026-05-07T21-19-13-04-00.md` — records the full coverage-enabled run with `385` total tests, `383` passing, and per-file coverage values.
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/csharp-coverage-summary.2026-05-07T21-19-46-04-00.md` — records the failing coverage verdict.
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/outlook-manual-validation.2026-05-07T21-19-59-04-00.md` — records that manual validation was not performed because coverage remained failing.

### Quality assessment prompts

- **Determinism:** Good. The red/green artifact set shows deterministic, repeatable targeted regressions.
- **Isolation:** Mixed. Test names are focused, but source-text assertions are weaker than behavioral seam tests.
- **Speed:** Good. Focused regressions and the final suite both completed without instability.
- **Diagnostics:** Good for structural failures, weaker for runtime behavior because the new tests do not always exercise live logic transitions.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | No secret material appeared in the reviewed implementation or evidence files. |
| No unsafe subprocess or command construction | ✅ PASS | No issue-specific production diff introduced command construction or subprocess behavior. |
| Input validation at boundaries | ⚠️ PARTIAL | The staged-boundary design is clearer, but low behavioral coverage means boundary validation is not yet strongly demonstrated. |
| Error handling remains explicit | ✅ PASS | No broad catch-all anti-pattern was evident in the reviewed diffs. |
| Configuration / path handling is safe | ⚠️ PARTIAL | The working tree currently includes many `.csproj` changes outside the declared primary scope and those need reconciliation. |

---

## Research Log

No external web research was required for this review. The review relied on:
- the active feature folder artifacts,
- refreshed `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`,
- the current working-tree diff summary, and
- direct inspection of the newly added regression test files.

---

## Verdict

This branch is **not ready** for normal PR flow. The implementation direction is reasonable, but the current evidence supports a blocked outcome rather than a conditional approval. The saved QA state still records a failing coverage conclusion, the critical live-responsiveness acceptance criterion remains unverified, and the actual working tree contains additional undeclared changes that should be removed or explicitly promoted into scope before another final QA pass.

The correct next step is remediation rather than merge review. The remediation should focus on closing the coverage gap with behavioral tests, reconciling actual branch scope to the declared feature scope, and rerunning the manual Outlook validation only after the coverage summary records `Coverage Conclusion: PASS`.
