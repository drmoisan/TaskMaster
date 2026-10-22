# Remediation Inputs: focus-and-theme-tests-leak-shared-dispatcher-setup (Issue #968, folding Issue #972)

- Timestamp: 2026-10-03T04-00
- Branch: bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968, head 5570b337cfd11e57579228b36bc919f9673b6195
- Source artifacts: policy-audit.2026-10-03T04-00.md, code-review.2026-10-03T04-00.md, feature-audit.2026-10-03T04-00.md (all in docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/)

Review-Verdict: AWAITING_CI

## Derivation

- Blocking findings: 1 (B-1 below).
- Findings of class autonomous: 0, so the verdict is not REMEDIATION_REQUIRED.
- Findings of class external_dependency, policy_hold or human_decision_required: 0, so the verdict is not HALT_NON_REMEDIABLE.
- The remaining blocking finding is of class awaiting_ci, so the verdict is AWAITING_CI.
- No code change, test change or evidence change is required before the pull request is opened.

## Blocking findings

### B-1: AC22 (full toolchain pass) is closed only from this pull request's CI run on the final head

Severity: Blocking
Remediability: awaiting_ci
Remediability-Evidence: the only locally failing step is the verbatim MSTest coverage runner, and it fails for an environmental shell-icon test in UtilitiesCS.Test that reproduces on main on this workstation; the orchestrator ruling recorded under AC22 in spec.md (following the #950 AC17 precedent) designates the pull request's own CI run on the final head as the evidence source, and no local action can produce that run.
File: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md, line 296 (`- [ ] AC22`) with the ruling note at line 297
Rule: spec.md acceptance criterion AC22; CLAUDE.md "After Making Changes" step 1 (full toolchain, step 4 is the MSTest coverage route); acceptance-criteria-tracking skill, "CI-Dependent Criteria"
Evidence: evidence/qa-gates/toolchain-final.md (steps 1 to 4 exit 0 in ITERATION 1, SINGLE-PASS: YES, both rebuilds SKIP_CORECOMPILE_LINES 0, coverage step COVERAGE-ROUTE DIRECT, RUNNER-GREEN NO, 7365/7365 passed); evidence/baseline/stall-probe.md (STALL-PROBE: REPRODUCES; ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension fails with "Win32 handle that was passed to Icon is not valid or is the wrong type"); evidence/qa-gates/coverage-comparison.md (lines 85.35% to 85.36%, branches 79.73% to 79.75%, AC23-STATUS MET); evidence/other/ac-status-summary.md (AC22 NOT MET, environmental)
Required action (orchestrator, at the CI green gate): open the pull request from the final head with a body carrying `Closes #968` and `Closes #972`; after the C# test-and-coverage job reports success, record the CI run ID, the head SHA, the job's pass and fail counts and coverage figures, and the local DIRECT result under AC22 in spec.md; change `- [ ] AC22` to `- [x] AC22` in the item's own worktree; push; re-run the CI green gate so that ci_gate.head_sha equals the final pull request head. If CI fails any test, AC22 is not met and the item returns to remediation.

## Non-blocking findings carried for reference (no remediation owed)

- CR-1: pre-existing commented-out statements remain in QuickFiler/Controllers/QfcDatamodel.cs (lines 63, 70, 199, 268, 318, 350); the ratified spec confines the production edit, and the research disposition (addendum F2) keeps the diff reviewable. Remove in the next change to the file.
- CR-2: the rewritten sibling test in QuickFiler.Test/Controllers/QfcDatamodelTests.cs (lines 103-152) registers its awaits under the ambient SynchronizationContext; accepted by the spec (addendum section 6.2). Wrap the dequeue call in the null-context scope if a non-pumping context ever appears on an MSTest thread.
- O-4: canonical C# coverage artifact path artifacts/csharp/coverage.xml absent in the worktree; committed projections and summaries used under the standing ruling (recurring).

## Unrelated defects to file

- None from this item. quality-tiers.yml is absent at the repository root (pre-existing; already promoted by the #956 review).
