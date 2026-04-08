# Plan — outlook-recipient-com-cross-thread-crash (Issue #124)

DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED

## Overview

Deliver the small-path C# bug fix for the Outlook recipient COM cross-thread crash by keeping scope constrained to the confirmed helper and recipient paths, adding regression coverage for the two failing behaviors described in `issue.md`, and finishing with the required C# QA loop plus reduced audit handoff evidence.

- Feature folder: `docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124`
- Plan path: `docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124/plan.2026-04-08T00-00.md`
- Work mode: `minor-audit`
- Sole requirements source: `docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124/issue.md`
- Acceptance-criteria source section: `## Acceptance Criteria`
- Non-authoritative files: `spec.md` none, `user-story.md` none, `research.md` none
- Confirmed small-path scope: `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`, plus targeted tests under `UtilitiesCS.Test/OutlookObjects/Recipient/` and `UtilitiesCS.Test/OutlookObjects/MailItem/`
- Preflight status for this planning session: pending required executor/validator validation outside the planner session

## Acceptance Criteria Source Snapshot

Use only the checkbox items under `## Acceptance Criteria` in `issue.md`:

- `MailItemHelper` no longer relies on background `Task.Run` evaluation of Outlook COM-backed lazy sender/recipient properties during the `ProcessMailItemAsync` tokenization path.
- Exchange recipient-name resolution no longer throws an unhandled COM exception when directory property access fails; it falls back to safe recipient data.
- Regression tests cover the recipient fallback behavior and the helper/tokenization path that previously crossed thread-affinity boundaries.
- The C# QA loop passes in the required order: format, analyzer build, nullable/type-safe build, and MSTest with coverage.

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read the required policy files in repository order and write `evidence/baseline/phase0-instructions-read.YYYY-MM-DDTHH-mm.md`.
  - Preconditions: The feature folder exists and the target plan file is present at `docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124/plan.2026-04-08T00-00.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Policy Order:`, and the exact read list for `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, and `.github/instructions/csharp-unit-test.instructions.md`.

- [x] [P0-T2] Review `change-plan.md` and write `evidence/other/change-plan-review.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and states that `change-plan.md` was reviewed, records any bug-workflow constraints that apply, and confirms that `issue.md` remains the sole requirements source for this minor-audit plan.

- [x] [P0-T3] Confirm the minor-audit inputs from `issue.md` and this plan file in `evidence/other/minor-audit-inputs.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and records `Work Mode: minor-audit`, confirms that `issue.md` contains an explicit `## Acceptance Criteria` section, lists the four acceptance-criteria checkboxes verbatim, records this exact plan path, and states that `spec.md`, `user-story.md`, and `research.md` are not required inputs for this workflow.

- [x] [P0-T4] Run `dotnet tool run csharpier format .` and write `evidence/baseline/csharp-format.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE:`, and `Output Summary:` with the baseline formatter result.

- [x] [P0-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` and write `evidence/baseline/csharp-analyzers-build.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:` with the analyzer-build baseline result.

- [x] [P0-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` and write `evidence/baseline/csharp-nullable-build.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:` with the nullable/type-safe baseline result.

- [x] [P0-T7] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and write `evidence/baseline/csharp-mstest-coverage.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:` with numeric coverage headline values from the baseline coverage run.

### Phase 1 — Constrained Small-Path Implementation Placeholder

Phase 1 execution note: This phase is a constrained small-path handoff placeholder only. Do not expand it into concrete code-change, test-authoring, or focused-regression execution steps inside this plan revision.

- [x] [P1-T1] Write the small-path handoff note to `evidence/other/constrained-small-path-handoff.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and lists the in-scope production files `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs` and `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`, the targeted test files `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticTests.cs` and one chosen file under `UtilitiesCS.Test/OutlookObjects/MailItem/`, states that `docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124/issue.md` `## Acceptance Criteria` is the sole implementation requirements source, and records the stop-and-escalate rule that any required production change outside those files ends the small-path route.

- [x] [P1-T2] Delegate constrained small-path implementation to the small-path implementation engineer using the scope locked by [P1-T1].
  - Acceptance: The delegated handoff explicitly requires the downstream implementation to stay within the scoped production and test files from [P1-T1], satisfy only the `issue.md` `## Acceptance Criteria` items captured in Phase 0, preserve this exact plan path as the controlling small-path plan, and return control to Phase 2 for the unconditional C# QA loop without adding concrete implementation subtasks to this plan.

### Phase 2 — Final QC Loop

Execute `P2-T1` through `P2-T4` in order. If any step changes files or exits non-zero, fix the issue and restart at `P2-T1` until one clean pass is recorded. Complete `P2-T5` through `P2-T7` only after a clean pass across all four command tasks.

- [x] [P2-T1] Run `dotnet tool run csharpier format .` and write `evidence/qa-gates/csharp-format.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` for the clean final formatter pass.

- [x] [P2-T2] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` and write `evidence/qa-gates/csharp-analyzers-build.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact command, `EXIT_CODE: 0`, and `Output Summary:` for the clean final analyzer-build pass.

- [x] [P2-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` and write `evidence/qa-gates/csharp-nullable-build.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact command, `EXIT_CODE: 0`, and `Output Summary:` for the clean final nullable/type-safe build pass.

- [x] [P2-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and write `evidence/qa-gates/csharp-mstest-coverage.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact command, `EXIT_CODE: 0`, and `Output Summary:` with numeric post-change coverage headline values from the final coverage run.

- [x] [P2-T5] Run the focused MSTest regression verification for the two in-scope bug behaviors and write `evidence/qa-gates/targeted-regression.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact focused test command, `EXIT_CODE: 0`, and `Output Summary:` identifying the recipient fallback regression test and the MailItem helper/tokenization regression test that passed.

- [x] [P2-T6] Compare the baseline and final coverage evidence in `evidence/qa-gates/csharp-coverage-summary.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and records the numeric baseline coverage headline from `evidence/baseline/csharp-mstest-coverage.*.md`, the numeric final coverage headline from `evidence/qa-gates/csharp-mstest-coverage.*.md`, the computed delta, and the pass or fail coverage conclusion for this bug workflow.

- [x] [P2-T7] Write the reduced audit handoff to `evidence/qa-gates/reduced-audit-handoff.YYYY-MM-DDTHH-mm.md`.
  - Acceptance: The artifact exists and lists the final changed files, the Phase 0 baseline artifacts, the targeted regression artifact from `evidence/qa-gates/targeted-regression.*.md`, the Phase 2 QA artifacts, the acceptance-criteria-to-evidence mapping, and the reduced-audit next step for small-path review or remediation.
