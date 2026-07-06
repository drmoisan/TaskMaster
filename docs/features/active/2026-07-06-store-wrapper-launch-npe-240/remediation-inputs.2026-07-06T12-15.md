# Remediation Inputs — store-wrapper-launch-npe (Issue #240)

- Timestamp: 2026-07-06T12-15
- Source artifacts: `policy-audit.2026-07-06T12-15.md`, `code-review.2026-07-06T12-15.md`, `feature-audit.2026-07-06T12-15.md`

## Finding 1 — Blocking: Test file exceeds the 500-line policy limit

- **Status: remediation-required (Blocking)**
- File: `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs`
- Current size: 781 lines. Baseline (merge-base) size: 582 lines — already over the repository's 500-line limit before this issue. This PR's diff adds 199 lines (`git diff --stat` confirms `+199` insertions).
- Policy: `.claude/rules/general-code-change.md` / CLAUDE.md General Code Change Policy §4 — "No production code, test code, or reusable script file may exceed 500 lines." No listed exemption applies to this file.
- Evidence: `policy-audit.2026-07-06T12-15.md` §1.6, §8 item 1; `code-review.2026-07-06T12-15.md` Findings Table row 1; `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/evidence/other/scope-budget-confirmation.md` (executor's own self-disclosure of this deviation, un-remediated).
- Recommended action: split `StoreWrapperController_Tests.cs` into cohesive sub-files (e.g., separate the `Launch(...)`/`EvaluateLaunchReadiness(...)` region added by this issue into its own file, or partition by another existing cohesive boundary already present in the file), so each resulting file is <= 500 lines. This can be done without touching production code or altering any test's behavior/assertions.
- Owner/next step: executor or a follow-up task under this issue's plan; must be resolved before this change is considered fully policy-compliant.

## Finding 2 — Tracked, non-blocking-for-#240: Repo-wide C# coverage artifact absent

- **Status: remediation-required (systemic tracking item; not blocking for issue #240's own merge)**
- No canonical `artifacts/csharp/coverage.xml` exists in this repository session. Per the mandatory Coverage Verification Procedure, this is a FAIL for the "Repo-wide per language" gate for C# (C# has changed files on this branch).
- The feature's own evidence (`evidence/qa-gates/qa-04-test-coverage.md`, `qa-05-coverage-delta.md`) labels 85.88% as "repository (testable-denominator) line coverage," but this is scoped to the single `UtilitiesCS.dll` module. Same-session partial data (`TestResults/final-coverage.xml`, uncommitted) shows other first-party modules (`TaskMaster.dll`, `Tags.dll`, `ToDoModel.dll`, `QuickFiler.dll`) at low-to-zero coverage when loaded under the single-project `UtilitiesCS.Test` run — most likely an artifact of those modules' own dedicated test projects not being executed in this run, not a certified measurement.
- This condition pre-dates issue #240 and is not attributable to either file this PR changed; issue #240's own new/changed-code coverage is independently verified at 100%.
- Evidence: `policy-audit.2026-07-06T12-15.md` §1.2.2, §5, §8 item 2.
- Recommended action: track under the repository's existing `feature/csharp-coverage-uplift` initiative (referenced in CLAUDE.md's General Unit Test Policy UT2 COM/VSTO/WinForms exemption clause) — produce a canonical, multi-project coverage merge (`artifacts/csharp/coverage.xml`, Cobertura or equivalent) covering all first-party C# test projects (`UtilitiesCS.Test`, `TaskMaster.Test`, `QuickFiler.Test`, `Tags.Test`, `ToDoModel.Test`, and any others), scoped to the testable denominator after applying the ratified COM/VSTO/WinForms exclusions, so future reviews can render a defensible repo-wide verdict without ad hoc reconstruction.
- Owner/next step: repository maintainer / CI-infrastructure owner, not this issue's executor.

## Finding 3 — Informational: PR-context summary misclassifies changed C# files

- **Status: informational (process/tooling gap, not a code defect)**
- `artifacts/pr_context.summary.txt`'s "Changed files overview" reports "Core logic changes: 0 files" and files both changed `.cs` files into "Docs/templates/agents/tooling: 21 files" (actual count for that bucket should be 21, with the 2 `.cs` files reported separately as core logic).
- This repeats a previously-documented misclassification pattern for C# changes in this PR-context generator.
- Evidence: `policy-audit.2026-07-06T12-15.md` "PR-Context Artifact Reliability Note"; direct comparison against `git diff --stat`/`git diff --name-status`.
- Recommended action: fix the PR-context summary generator's file-classification logic so `.cs` core-logic files are not bucketed into "docs/templates/agents/tooling." Reviewers should continue to independently verify scope via `git diff` rather than relying on the summary's classification until this is corrected.
- Owner/next step: owner of the PR-context artifact generation tooling.

## Finding 4 — Documentation discrepancy: AC5 check-off vs. review verdict

- **Status: remediation-required (documentation reconciliation)**
- `issue.md` AC5 is checked `[x]`, but this review's verdict for AC5 is PARTIAL (Finding 2 above is the specific cause: the "repository line coverage >= 80%" clause is not substantiated at true repo-wide scope).
- Evidence: `feature-audit.2026-07-06T12-15.md` "Acceptance Criteria Check-off" section.
- Recommended action: maintainer should either (a) narrow AC5's wording to explicitly scope "repository line coverage" to the `UtilitiesCS` project/testable denominator actually measured, or (b) wait for Finding 2's canonical repo-wide artifact before treating AC5 as fully satisfied.
- Owner/next step: maintainer (Dan Moisan) / issue #240 owner.

## Remediation Priority Summary

| Finding | Severity | Blocking for #240 merge? |
|---|---|---|
| 1. Test file 500-line limit | Blocking | Yes |
| 2. Repo-wide C# coverage artifact absent | Systemic / FAIL per procedure | No (pre-existing, tracked separately) |
| 3. PR-context summary misclassification | Informational | No |
| 4. AC5 check-off discrepancy | Documentation | No (but should be reconciled) |
