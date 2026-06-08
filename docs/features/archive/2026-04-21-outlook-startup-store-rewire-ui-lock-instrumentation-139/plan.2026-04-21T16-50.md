# Plan — outlook-startup-store-rewire-ui-lock-instrumentation (Issue #139)

DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED

- **Issue:** #139
- **Owner:** drmoisan
- **Last Updated:** 2026-04-21T16-50
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** `minor-audit`
- **Requirements Source:** `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md`
- **Plan Path:** `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/plan.2026-04-21T16-50.md`

## Overview

Add diagnostic startup timing instrumentation for the Outlook store-rewire UI lock investigation while keeping the small-path bug scope constrained to `StoresWrapper`, `StoreWrapper`, and `FolderMinimalWrapper`. Use only the explicit checkbox items under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md` `## Acceptance Criteria` as implementation requirements, and finish with the mandatory C# QC loop plus reduced minor-audit evidence handoff.

- Feature folder: `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139`
- Sole acceptance-criteria source section: `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md` → `## Acceptance Criteria`
- Non-authoritative files for this plan: `spec.md`, `user-story.md`, and research files are not formal inputs for this `minor-audit` workflow.
- Confirmed small-path production scope: `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`
- Existing tests may be updated only if compile or test stability requires it; keep test scope minimal.
- Preflight status for this planning session: pending validator and executor preflight outside the planner session.

## Acceptance Criteria Source Snapshot

Use only the checkbox items under `## Acceptance Criteria` in `issue.md`:

- `StoresWrapper.RewireOlObjectsAsync()` logs total filtered-store timing, total rewire timing, and per-store loop timing with the `[Startup timing]` prefix.
- `StoreWrapper.Init()` and `StoreWrapper.GetSmtpAddressFromStore()` log per-call elapsed milliseconds for the targeted Outlook COM boundaries identified in the research note.
- `StoreWrapper.Restore()` and `FolderMinimalWrapper.RestoreFromRelativePath()` log timing needed to distinguish folder-restoration delays from store-init delays.
- The diagnostic code compiles cleanly, uses the existing `log4net` infrastructure, and does not change the functional startup behavior beyond additional debug logging.

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read the required policy files in repository order plus `issue.md`, then write a policy-read artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/` using the stem `phase0-instructions-read.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/phase0-instructions-read.*.md` and contains `Timestamp:`, `Policy Order:`, `Files Read:`, and `Requirements Source: issue.md only` for `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, and `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md`.

- [x] [P0-T2] Review `change-plan.md`, then write a change-plan review artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/other/` using the stem `change-plan-review.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/other/change-plan-review.*.md` and states that `change-plan.md` was reviewed, records that the repository-wide migration work does not replace this bug-specific minor-audit workflow, and confirms that `issue.md` remains the sole requirements source for this plan.

- [x] [P0-T3] Confirm the minor-audit inputs from `issue.md`, then write an inputs artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/other/` using the stem `minor-audit-inputs.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/other/minor-audit-inputs.*.md` and records `Work Mode: minor-audit`, confirms that `issue.md` contains an explicit `## Acceptance Criteria` section, copies the four acceptance-criteria checkboxes verbatim, records this exact plan path, records `SearchScope: docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/`, records `SearchPatterns: spec.md, user-story.md`, and records `SearchResult:` showing either `none` or the exact unexpected file paths.

- [x] [P0-T4] Run `csharpier .` from the repository root, then write a baseline formatter artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/` using the stem `csharp-format.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/csharp-format.*.md` and contains `Timestamp:`, `Command: csharpier .`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T5] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` from the repository root, then write a baseline analyzer-build artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/` using the stem `csharp-analyzers-build.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/csharp-analyzers-build.*.md` and contains `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T6] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` from the repository root, then write a baseline nullable-build artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/` using the stem `csharp-nullable-build.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/csharp-nullable-build.*.md` and contains `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T7] Run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` from the repository root, then write a baseline coverage artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/` using the stem `csharp-mstest-coverage.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/baseline/csharp-mstest-coverage.*.md`, contains `Timestamp:`, `Command: vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`, `EXIT_CODE:`, and `Output Summary:` including numeric total, passed, failed, and skipped test counts plus numeric baseline coverage headline values and the saved coverage artifact path.

### Phase 1 — Constrained Small-Path Implementation Placeholder

Phase 1 is a constrained handoff placeholder only. Do not expand this phase into additional planning phases or non-minimal workflow documents.

- [x] [P1-T1] Write the constrained small-path handoff artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/other/` using the stem `constrained-small-path-handoff.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/other/constrained-small-path-handoff.*.md` and lists the in-scope production files `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`, and `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`; records `Production File Count: 3`; states that test updates are allowed only for compile or test stability; states that only `issue.md` `## Acceptance Criteria` governs implementation; and records the stop-and-escalate rule that any required production change outside those three files ends the small-path route.

- [x] [P1-T2] Delegate the constrained small-path implementation using the scope locked by [P1-T1].
  - Acceptance: The delegated handoff explicitly requires the downstream implementation to add `[Startup timing]` debug logging only, preserve functional startup behavior, use existing `log4net` infrastructure, keep code changes within the three scoped production files unless compile or test stability requires a minimal test adjustment, keep this exact plan path as the controlling plan, and return control to Phase 2 for the unconditional C# QC loop plus reduced-audit handoff without introducing `spec.md`, `user-story.md`, or additional planning artifacts.

### Phase 2 — Final QC Loop

Execute [P2-T1] through [P2-T4] in order. If any step changes files or exits non-zero, fix the issue and restart at [P2-T1] until one clean pass is recorded. Complete [P2-T5] through [P2-T7] only after a clean pass across all four command tasks.

- [x] [P2-T1] Run `csharpier .` from the repository root, then write a final formatter artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/` using the stem `csharp-format.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/csharp-format.*.md` and contains `Timestamp:`, `Command: csharpier .`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final formatter pass.

- [x] [P2-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` from the repository root, then write a final analyzer-build artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/` using the stem `csharp-analyzers-build.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/csharp-analyzers-build.*.md` and contains `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final analyzer-build pass.

- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` from the repository root, then write a final nullable-build artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/` using the stem `csharp-nullable-build.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/csharp-nullable-build.*.md` and contains `Timestamp:`, `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final nullable/type-safe build pass.

- [x] [P2-T4] Run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` from the repository root, then write a final coverage artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/` using the stem `csharp-mstest-coverage.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/csharp-mstest-coverage.*.md`, contains `Timestamp:`, `Command: vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`, `EXIT_CODE: 0`, and `Output Summary:` including numeric total, passed, failed, and skipped test counts plus numeric post-change coverage headline values and the saved coverage artifact path.

- [x] [P2-T5] Write targeted diagnostic verification evidence under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/` using the stem `targeted-diagnostic-verification.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/targeted-diagnostic-verification.*.md`, contains `Timestamp:`, contains `Source Artifact:` pointing to the successful `csharp-mstest-coverage.*.md` artifact from [P2-T4], contains `Changed Files:` listing every changed production or test file, and contains `Acceptance Criteria Coverage:` mapping each `issue.md` checkbox to the specific instrumentation call site or verification evidence that satisfies it.

- [x] [P2-T6] Compare the baseline and final coverage results in a coverage-summary artifact under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/` using the stem `csharp-coverage-summary.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/csharp-coverage-summary.*.md`, contains `Timestamp:`, contains `Baseline Coverage Artifact:` pointing to `csharp-mstest-coverage.*.md` under `evidence/baseline/`, contains `Final Coverage Artifact:` pointing to `csharp-mstest-coverage.*.md` under `evidence/qa-gates/`, records numeric baseline coverage, records numeric final coverage, records the computed delta, records numeric `New/Changed-Code Coverage:` or the repository-equivalent changed-lines metric, records `Coverage Policy Evaluation:`, and records `Coverage Conclusion:` as `PASS` only when the no-regression and changed-code coverage requirements are satisfied.

- [x] [P2-T7] Write the reduced-audit end-state handoff under `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/` using the stem `reduced-audit-handoff.`.
  - Acceptance: The artifact exists at `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/reduced-audit-handoff.*.md`, contains `Timestamp:`, contains `Changed Files:` listing the final production and test files, contains `Baseline Artifacts:` listing every Phase 0 artifact, contains `Targeted Verification Artifact:` pointing to the [P2-T5] artifact, contains `Final QC Artifacts:` listing [P2-T1] through [P2-T6], contains `Acceptance Criteria Coverage:` mapping each `issue.md` checkbox to implementing code or evidence, and contains `Post-Validation Expectation:` directing reduced-audit review only when all required artifacts are present and all QC gates are passing; otherwise it directs remediation planning.
