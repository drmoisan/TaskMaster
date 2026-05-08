# Remediation Plan — outlook-startup-ui-lockup-followup (Issue #148)

- **Issue:** #148
- **Branch:** `bug/outlook-startup-ui-lockup-followup-148`
- **Base Branch:** `development`
- **Last Updated:** 2026-05-07T21-30
- **Status:** Drafted from review-triggered remediation inputs
- **Work Mode:** `full-bug`
- **Plan Path:** `c:\Users\DanMoisan\repos\TaskMaster-wt-2026-05-07-13-34\docs\features\active\2026-05-07-outlook-startup-ui-lockup-followup-148\remediation-plan.2026-05-07T21-30.md`
- **Requirements Sources:** `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/spec.md`
- **Supporting Context:** `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/remediation-inputs.2026-05-07T21-30.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/policy-audit.2026-05-07T21-30.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/code-review.2026-05-07T21-30.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/feature-audit.2026-05-07T21-30.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/plan.2026-05-07T19-34.md`, `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`

## Objective

Finish issue `#148` remediation without widening scope beyond the declared startup and first-selection follow-up area by reconciling the actual branch diff to the declared feature scope, replacing brittle source-text regressions with behavioral seam tests, raising changed/new-code coverage to policy threshold, restoring structural compliance for the oversized changed production files, and only then rerunning the final QA loop and manual Outlook validation.

### Phase 0 — Context, Scope, and Baseline Refresh

- [x] [P0-T1] Read the remediation inputs in this exact order: `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/issue.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/spec.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/remediation-inputs.2026-05-07T21-30.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/policy-audit.2026-05-07T21-30.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/code-review.2026-05-07T21-30.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/feature-audit.2026-05-07T21-30.md`, and `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/plan.2026-05-07T19-34.md`, then write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/baseline/remediation-phase0-instructions-read.*.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Policy Order:`, `Files Read:`, `Requirements Sources:`, `Supporting Context:`, and `Work Mode: full-bug`.
- [x] [P0-T2] Run `git status --short` and `git diff --name-status development...HEAD`, compare both outputs to `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/other/implementation-scope.2026-05-07T20-09-49-04-00.md`, and write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/other/remediation-scope-refresh.*.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the two exact commands, `EXIT_CODE:` values, `Declared Scope Files:`, `Additional Working-Tree Files:`, and `Scope Decision:` for every out-of-scope path (`remove`, `promote`, or `already-clean`).
- [x] [P0-T3] Capture fresh baseline command-step artifacts for the current remediation cycle by running the C# formatter, analyzer build, nullable build, and coverage-enabled MSTest commands from the repository root and storing one artifact per step under `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/baseline/`.
  - Acceptance: Four artifacts exist, one per command, and each contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. The baseline coverage artifact must include numeric repository coverage and numeric coverage values for the eight primary production files.

### Phase 1 — Scope Reconciliation and Behavioral Test Hardening

- [x] [P1-T1] Remove or explicitly justify every out-of-scope working-tree file recorded by [P0-T2] before further implementation continues.
  - Acceptance: Running `git status --short` after this task shows only the approved issue `#148` production files, their mapped tests, feature-folder artifacts, and any newly extracted helper files that remain within the approved functional area.
- [x] [P1-T2] Replace the new source-text regression assertions in `TaskMaster.Test/AppGlobals/AppEventsTests.cs`, `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, `QuickFiler.Test/Controllers/EfcDataModelTests.cs`, `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`, `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelper_ExtendedTests.cs`, `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`, and `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` with behavioral seam tests or equivalent runtime-observable assertions.
  - Acceptance: Each listed test home still contains deterministic regression coverage for the intended issue `#148` behavior, but the test evidence no longer depends primarily on raw source-file string matching.
- [x] [P1-T3] Re-run focused red/green regression evidence as needed for any rewritten test homes and store the resulting proof under `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/regression-testing/`.
  - Acceptance: The refreshed evidence shows deterministic red/green coverage for the rewritten regression homes and identifies the exact test names and commands executed.

### Phase 2 — Coverage Closure and Structural Compliance

- [x] [P2-T1] Add or adjust only the minimal production and test changes required to bring changed/new-code coverage for the eight primary production files to `>= 90%`.
  - Acceptance: The refreshed coverage-enabled MSTest run records `New/Changed-Code Coverage >= 90%`, and no new out-of-scope production path is introduced.
- [x] [P2-T2] Reduce the line count of the oversized changed production files by extracting focused helpers or narrowing the branch delta within the same approved functional areas.
  - Acceptance: `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, and `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs` are no longer over `500` lines in the final remediated branch state, or the corresponding oversized changes have been reverted/split from this branch.
- [x] [P2-T3] Write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/other/post-remediation-structure-check.*.md` after [P2-T2].
  - Acceptance: The artifact exists and records `Timestamp:`, the line-count command used, every changed production file and line count, and `Structure Conclusion: PASS` only when all changed production files satisfy the file-size rule.

### Phase 3 — Final QA Loop and Acceptance Re-Verification

Execute [P3-T1] through [P3-T5] in order. If any step changes files or exits non-zero, fix the issue and restart at [P3-T1].

- [x] [P3-T1] Run `dotnet tool run csharpier format .` from the repository root and write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/remediation-csharp-format.*.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final pass.
- [x] [P3-T2] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` and write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/remediation-csharp-analyzers-build.*.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`.
- [x] [P3-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` and write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/remediation-csharp-nullable-build.*.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`.
- [x] [P3-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/remediation-csharp-mstest-coverage.*.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with numeric total/passed/failed/skipped counts, numeric repository coverage, and numeric coverage for every changed primary production file.
- [x] [P3-T5] Compare the Phase 0 remediation baseline coverage artifact with the final remediation coverage artifact and write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/remediation-csharp-coverage-summary.*.md`.
  - Acceptance: The artifact exists and contains numeric baseline coverage, numeric final coverage, numeric changed/new-code coverage, explicit threshold evaluation, and `Coverage Conclusion: PASS` only when all required coverage gates pass.

### Phase 4 — Manual Outlook Validation and Review Refresh

- [ ] [P4-T1] Perform the manual Outlook startup and first-selection validation documented in `spec.md` after [P3-T5] records `Coverage Conclusion: PASS`, then write `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/remediation-outlook-manual-validation.*.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Operator:`, `Environment:`, `Repro Path:`, `Responsiveness Result: PASS`, `Timing Evidence:`, and `Evidence Source:`.
- [ ] [P4-T2] Refresh the end-state artifact at `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/remediation-full-bug-end-state.*.md`.
  - Acceptance: The artifact exists and maps all eight acceptance criteria, records the refreshed final QA artifacts, and sets `Ready For Validator: true` only when [P4-T1] passes.
- [ ] [P4-T3] Refresh the review set by generating updated `policy-audit`, `code-review`, and `feature-audit` artifacts for the remediated branch state.
  - Acceptance: The refreshed review set post-dates the remediation QA artifacts and records a non-blocked conclusion only if coverage, scope, structural compliance, and manual validation all pass.
