# Plan — outlook-com-sta-materialization (Issue #128)

DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED

- **Issue:** #128
- **Owner:** drmoisan
- **Last Updated:** 2026-04-13T23-19
- **Status:** Completed; ready for reduced-audit review
- **Version:** 1.0
- **Work Mode:** `minor-audit`
- **Requirements Source:** `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/issue.md`
- **Plan Path:** `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-04-13-outlook-com-sta-materialization-128\plan.2026-04-13T22-47.md`

## Overview

Deliver the required small-path C# bug fix for Outlook COM STA materialization by keeping the implementation constrained to the confirmed `EmailDataMiner`, `MailItemHelper`, and `RecipientStatic` paths, using only the explicit acceptance-criteria checkboxes under `issue.md`, and finishing with the repository-approved C# QC loop plus reduced-audit handoff evidence. Do not require or reference `spec.md`, `user-story.md`, or `research.md` for this workflow.

- Feature folder: `docs/features/active/2026-04-13-outlook-com-sta-materialization-128`
- Sole acceptance-criteria source section: `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/issue.md` → `## Acceptance Criteria`
- Non-authoritative files for this plan: `spec.md`, `user-story.md`, `research.md`
- Estimated small-path production scope: `UtilitiesCS/EmailIntelligence/Bayesian/EmailDataMiner.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`
- Likely targeted test homes: `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs`, `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs`, `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticSenderResolverTests.cs`
- Preflight status for this planning session: `PREFLIGHT: ALL CLEAR`; the plan is approved for execution starting with Phase 0.

## Acceptance Criteria Source Snapshot

Use only the checkbox items under `## Acceptance Criteria` in `issue.md`:

- `EmailDataMiner.ToIItemInfo` no longer offloads `MailItemHelper.FromMailItemAsync` to `Task.Run`, so Outlook COM-backed sender/recipient materialization remains on the caller's Outlook STA thread.
- `RecipientStatic.GetSenderName` no longer throws when Exchange Address Book lookup fails; it falls back safely to mail-item sender data without unguarded `sender.Name` access.
- Recipient helper fallbacks use the same defensive pattern for Exchange-backed lookup failures so background tokenization paths degrade safely instead of crashing.
- Regression tests cover the sender/recipient fallback behavior and the helper materialization path implicated by this crash.
- The required C# QA loop passes in order: format, analyzer build, nullable/type-safe build, and MSTest with coverage.

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read the required policy files in repository order plus `issue.md`, then write a policy-read artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/` using the stem `phase0-instructions-read.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/phase0-instructions-read.*.md` and contains `Timestamp:`, `Policy Order:`, `Files Read:`, and `Requirements Source: issue.md only` for `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, and `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/issue.md`.

- [x] [P0-T2] Review `change-plan.md` and write a change-plan review artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/other/` using the stem `change-plan-review.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/other/change-plan-review.*.md` and states that `change-plan.md` was reviewed, records that the repository-wide migration work does not replace this bug-specific minor-audit workflow, and confirms that `issue.md` remains the sole requirements source for this plan.

- [x] [P0-T3] Confirm the minor-audit inputs and feature-folder scope in an artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/other/` using the stem `minor-audit-inputs.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/other/minor-audit-inputs.*.md` and records `Work Mode: minor-audit`, confirms that `issue.md` contains an explicit `## Acceptance Criteria` section, copies the five acceptance-criteria checkboxes verbatim, records this exact plan path, records `SearchScope: docs/features/active/2026-04-13-outlook-com-sta-materialization-128/`, records `SearchPatterns: spec.md, user-story.md, research.md`, and records `SearchResult: none`.

- [x] [P0-T4] Run `dotnet tool run csharpier format .` from the repository root and write a baseline formatter artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/` using the stem `csharp-format.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/csharp-format.*.md` and contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` from the repository root and write a baseline analyzer-build artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/` using the stem `csharp-analyzers-build.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/csharp-analyzers-build.*.md` and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` from the repository root and write a baseline nullable-build artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/` using the stem `csharp-nullable-build.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/csharp-nullable-build.*.md` and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\outlook-com-sta-materialization-128-baseline.cobertura.xml` from the repository root and write a baseline coverage artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/` using the stem `csharp-mstest-coverage.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/csharp-mstest-coverage.*.md` and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\outlook-com-sta-materialization-128-baseline.cobertura.xml`, `EXIT_CODE:`, and `Output Summary:` including numeric total, passed, failed, and skipped test counts, numeric overall line coverage from `coverage\outlook-com-sta-materialization-128-baseline.cobertura.xml`, and the coverage artifact path.

### Phase 1 — Constrained Small-Path Implementation Placeholder

Phase 1 is a constrained handoff placeholder only. Do not expand this phase into additional planning phases or non-minimal workflow documents.

- [x] [P1-T1] Write the constrained small-path handoff artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/other/` using the stem `constrained-small-path-handoff.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/other/constrained-small-path-handoff.*.md` and lists the in-scope production files `UtilitiesCS/EmailIntelligence/Bayesian/EmailDataMiner.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, and `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`; lists the targeted test homes `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs`, `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs`, and `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticSenderResolverTests.cs`; states `Production File Count: 3`; states `Test File Count: 3`; records that only `issue.md` `## Acceptance Criteria` governs implementation; and records the stop-and-escalate rule that any required production change outside those three files or any required test expansion beyond those three test files ends the small-path route.

- [x] [P1-T2] Delegate the constrained small-path implementation using the scope locked by [P1-T1].
	- Acceptance: The delegated handoff explicitly requires the downstream implementation to keep Outlook COM-backed materialization on the caller's STA thread, apply COM-safe sender and recipient fallback guards, add regression coverage only in the Phase 1 test homes, keep this exact plan path as the controlling plan, and return control to Phase 2 for the unconditional C# QC loop plus reduced-audit handoff without adding `spec.md`, `user-story.md`, or `research.md`.

### Phase 2 — Final QC Loop

Execute [P2-T1] through [P2-T4] in order. If any step changes files or exits non-zero, fix the issue and restart at [P2-T1] until one clean pass is recorded. Complete [P2-T5] through [P2-T7] only after a clean pass across all four command tasks.

- [x] [P2-T1] Run `dotnet tool run csharpier format .` from the repository root and write a final formatter artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/` using the stem `csharp-format.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/csharp-format.*.md` and contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final formatter pass.

- [x] [P2-T2] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` from the repository root and write a final analyzer-build artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/` using the stem `csharp-analyzers-build.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/csharp-analyzers-build.*.md` and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final analyzer-build pass.

- [x] [P2-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` from the repository root and write a final nullable-build artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/` using the stem `csharp-nullable-build.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/csharp-nullable-build.*.md` and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final nullable/type-safe build pass.

- [x] [P2-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\outlook-com-sta-materialization-128-final.cobertura.xml` from the repository root and write a final coverage artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/` using the stem `csharp-mstest-coverage.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/csharp-mstest-coverage.*.md` and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\outlook-com-sta-materialization-128-final.cobertura.xml`, `EXIT_CODE: 0`, and `Output Summary:` including numeric total, passed, failed, and skipped test counts, numeric overall line coverage from `coverage\outlook-com-sta-materialization-128-final.cobertura.xml`, and the coverage artifact path.

- [x] [P2-T5] Write targeted regression verification evidence under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/` using the stem `targeted-regression.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/targeted-regression.*.md`, contains `Timestamp:`, contains `Source Artifact:` pointing to the successful `csharp-mstest-coverage.*.md` artifact from [P2-T4], contains `Verified Test Files:` listing every changed test file from Phase 1, and contains `Verified Test Names:` listing at least one passing regression test for the STA materialization path and at least one passing regression test for the sender/recipient fallback path.

- [x] [P2-T6] Compare the baseline and final coverage results in a coverage-summary artifact under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/` using the stem `csharp-coverage-summary.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/csharp-coverage-summary.*.md`, contains `Timestamp:`, contains `Baseline Coverage Artifact:` pointing to `csharp-mstest-coverage.*.md` under `evidence/baseline/`, contains `Final Coverage Artifact:` pointing to `csharp-mstest-coverage.*.md` under `evidence/qa-gates/`, records numeric baseline overall line coverage, records numeric final overall line coverage, records the computed delta, records numeric `New/Changed-Code Coverage:` or the repository-equivalent changed-lines/new-code coverage metric, records `Coverage Policy Evaluation:` against the repository coverage policy, and records a `Coverage Conclusion:` value of `PASS` only when baseline overall coverage is recorded, final overall coverage is recorded, the no-regression requirement is satisfied, and the new/changed-code coverage requirement is satisfied; otherwise `FAIL`.

- [x] [P2-T7] Write the reduced-audit end-state handoff under `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/` using the stem `reduced-audit-handoff.`.
	- Acceptance: The artifact exists at `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/reduced-audit-handoff.*.md`, contains `Timestamp:`, contains `Changed Files:` listing the final production and test files, contains `Baseline Artifacts:` listing every Phase 0 artifact, contains `Targeted Verification Artifact:` pointing to the [P2-T5] artifact, contains `Final QC Artifacts:` listing [P2-T1] through [P2-T6], contains `Acceptance Criteria Coverage:` mapping each `issue.md` checkbox to implementing code or evidence, and contains `Post-Validation Expectation:` directing reduced-audit review only when all required artifacts are present, all acceptance criteria are met, [P2-T6] reports `Coverage Conclusion: PASS`, and every final QC gate is passing; otherwise it directs remediation planning.
