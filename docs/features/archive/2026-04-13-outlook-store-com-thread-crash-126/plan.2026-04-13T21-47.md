# 2026-04-13-outlook-store-com-thread-crash (Plan)

- **Issue:** #126
- **Parent:** none
- **Owner:** drmoisan
- **Last Updated:** 2026-04-14
- **Status:** Approved
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Branch:** `bug/outlook-store-com-thread-crash-126`
- **Requirements Source:** `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/issue.md`

## Overview

Remove `Task.Run` wrappers around Outlook COM access in `AppOlObjects.LoadStoresAsync()`, `StoresWrapper.RewireOlObjectsAsync()`, and `StoresWrapper.CreateAsync()` to eliminate cross-thread COM violations. Add defensive per-store `try/catch` in `LoadInboxes()` so a failing store is logged and skipped rather than crashing the add-in.

## Production Files in Scope

- `TaskMaster/AppGlobals/AppOlObjects.cs`
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read repository policy files in required order and store evidence
  - Read in this exact order:
    1. `.github/copilot-instructions.md`
    2. `.github/instructions/general-code-change.instructions.md`
    3. `.github/instructions/general-unit-test.instructions.md`
    4. `.github/instructions/csharp-code-change.instructions.md`
    5. `.github/instructions/csharp-unit-test.instructions.md`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and explicit list of files read.

- [x] [P0-T2] Review `change-plan.md` at repository root and confirm it does not conflict with this bug fix scope
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/other/change-plan-review.md` confirming review complete and no conflicts.

- [x] [P0-T3] Confirm minor-audit inputs are valid
  - Verify `issue.md` contains `- Work Mode: minor-audit` marker.
  - Verify `issue.md` contains an explicit `## Acceptance Criteria` section with 6 checkboxes.
  - Verify no `spec.md` or `user-story.md` exists in the feature folder.
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/other/minor-audit-inputs.md` confirming all three conditions hold.

- [x] [P0-T4] Run baseline C# format check and store evidence
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/baseline/csharp-format.md` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T5] Run baseline analyzer build and store evidence
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/baseline/csharp-analyzers-build.md` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T6] Run baseline nullable/type-check build and store evidence
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/baseline/csharp-nullable-build.md` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T7] Run baseline MSTest with coverage and store evidence
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/baseline/csharp-mstest-coverage.md` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric baseline coverage values.

---

### Phase 1 — Implementation (Delegated to csharp-typed-engineer)

- [x] [P1-T1] Implement bug fix across production files and verify all acceptance criteria
  - **Production files:**
    - `TaskMaster/AppGlobals/AppOlObjects.cs` — Remove `Task.Run` wrapper around Outlook COM access in `LoadStoresAsync()`. Store deserialization and initialization must execute on the calling thread.
    - `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` — Remove `Task.Run` wrappers in `RewireOlObjectsAsync()` (around `StoreWrapper.Init()` and `Restore()`) and in `CreateAsync()` (around `new StoresWrapper(globals).Init()`). All COM access must stay on the calling thread.
    - `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` (or caller site) — Add per-store `try/catch` in `LoadInboxes()` around `ShouldIncludeStore` enumeration so that a failing store is logged and skipped rather than crashing the add-in.
  - **Acceptance criteria (all 6 from issue.md must be satisfied):**
    1. `AppOlObjects.LoadStoresAsync()` no longer wraps Outlook COM access in `Task.Run`; store deserialization and initialization execute on the calling thread.
    2. `StoresWrapper.RewireOlObjectsAsync()` no longer wraps `StoreWrapper.Init()` or `Restore()` in `Task.Run`; all COM access stays on the calling thread.
    3. `StoresWrapper.CreateAsync()` no longer wraps `new StoresWrapper(globals).Init()` in `Task.Run`.
    4. `LoadInboxes()` wraps per-store enumeration (including `ShouldIncludeStore`) in a `try/catch` so that a failing store is logged and skipped rather than crashing the add-in.
    5. Existing unit tests continue to pass with no regressions.
    6. Full C# toolchain passes (format, analyzers, nullable/type-check, tests).
  - **QA gates:** CSharpier format, analyzer build, nullable/type-check build, and MSTest must all pass before this task is marked complete.
  - Acceptance: All 6 acceptance criteria verified, production files modified, and all 4 QA gate commands pass.

---

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run CSharpier format check and store final evidence
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/qa-gates/csharp-format-final.md` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Command must be executed unconditionally.

- [x] [P2-T2] Run analyzer build and store final evidence
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/qa-gates/csharp-analyzers-build-final.md` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Command must be executed unconditionally.

- [x] [P2-T3] Run nullable/type-check build and store final evidence
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/qa-gates/csharp-nullable-build-final.md` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Command must be executed unconditionally.

- [x] [P2-T4] Run MSTest with coverage and store final evidence
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance: Evidence artifact created at `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/evidence/qa-gates/csharp-mstest-coverage-final.md` containing `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change coverage values. Command must be executed unconditionally.

- [x] [P2-T5] Perform delta verification and confirm acceptance criteria
  - Compare baseline coverage values (from `evidence/baseline/csharp-mstest-coverage.md`) against final coverage values (from `evidence/qa-gates/csharp-mstest-coverage-final.md`).
  - Confirm no coverage regression occurred.
  - Confirm all 6 acceptance criteria from `issue.md` are checked off.
  - Acceptance: Delta verification documented; baseline and final coverage values reported side-by-side; all AC confirmed satisfied; no coverage regression.
