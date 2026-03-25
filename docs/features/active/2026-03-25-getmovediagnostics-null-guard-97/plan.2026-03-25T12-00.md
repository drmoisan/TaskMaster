# 2026-03-25-getmovediagnostics-null-guard (Plan)

- **Issue:** #97
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-25T12-00
- **Status:** In Progress
- **Version:** 1.0
- **Work Mode:** minor-audit

Requirements source: `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md`

## Root Cause Summary

`UtilitiesCS.Calendar.GetCalendar("Email Time", ...)` returns `null` when the subfolder does not exist.
`WriteMoveToCalendar` already propagates that null (sets `OlAppointment = null` when `OlEmailCalendar is null`).
However:
- `GetMoveDiagnostics` in `QfcCollectionController.cs` unconditionally dereferences `olAppointment.Body` at line 2115 without a null check.
- `QuickFileMetrics_WRITE` in `QfcHomeController.cs` unconditionally calls `olEmailCalendar.Items.Add()` at line 419 without a null check.

Fix: guard both dereferences with `if (olAppointment is not null)` and `if (olEmailCalendar is not null)` respectively, and add regression tests for both null paths.

---

### Phase 0 — Policy Read + Baseline Capture

- [x] [P0-T1] Read mandatory policy files in policy-compliance order and save a policy-read evidence artifact.
  - Files to read (in order):
    1. `CLAUDE.md`
    2. `.claude/skills/general-code-change-policy/SKILL.md`
    3. `.claude/skills/general-unit-test-policy/SKILL.md`
    4. `.claude/skills/csharp-code-change-policy/SKILL.md`
    5. `.claude/skills/csharp-unit-test-policy/SKILL.md`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/phase0-instructions-read.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Policy Order:` listing all five policy files read in order
    - Explicit list of filenames read

- [x] [P0-T2] Run the formatter to establish a format baseline and save the artifact.
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-format.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: dotnet tool run csharpier format .`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming no files were changed

- [x] [P0-T3] Run the lint/analyzer build to establish a lint baseline and save the artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-lint.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming build succeeded with 0 errors

- [x] [P0-T4] Run the nullable/type-check build to establish a nullable baseline and save the artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-nullable.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming build succeeded with 0 errors

- [x] [P0-T5] Run the targeted regression test filter to establish a test baseline (expect 0 matching tests before implementation) and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~NullAppointment"`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-test-filter.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: <recorded integer>`
    - `Output Summary:` noting that `NullAppointment` tests do not yet exist at baseline (0 tests found is the expected baseline state)

- [x] [P0-T6] Run the full QuickFiler.Test suite with coverage enabled to establish a numeric coverage baseline and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-coverage.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` including the numeric QuickFiler.Test line-coverage percentage reported by vstest (e.g., `Lines covered: XX%`)

---

### Phase 1 — Regression Tests + Implementation Fix

- [x] [P1-T1] [expect-fail] Add regression test `QuickFileMetrics_WRITE_GetCalendarReturnsNull_DoesNotThrow` to `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`. Run it before the fix and confirm it fails.
  - Precondition: Phase 0 all tasks complete; `QfcHomeControllerTests.cs` exists.
  - Test scenario: Mock `Globals.Ol.App.Session` so that `GetDefaultFolder(olFolderCalendar).Folders` is an empty `Folders` collection (causing `GetCalendar` to return null). Mock `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", ...)` to return true with a non-null path. Mock `_formController.Groups.EmailsToMove` to return 1. Call `_controller.QuickFileMetrics_WRITE("test.txt")` and assert no exception is thrown.
  - Acceptance: Test added with `[TestMethod]` attribute; run confirms a `NullReferenceException` is thrown before the fix (test fails as expected, tagged `[expect-fail]`). Evidence artifact saved at `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/regression-testing/fail-before-exception.2026-03-25T00-00.md` with `WhyFailingRunImpossible` explanation OR a failing-run evidence artifact.

- [x] [P1-T2] [expect-fail] Add regression test `GetMoveDiagnostics_NullAppointment_DoesNotThrow` to `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` (or a new `QfcCollectionControllerTests.cs` if the class is not already covered there). Run it before the fix and confirm it fails.
  - Precondition: Phase 0 all tasks complete.
  - Test scenario: Create a `QfcCollectionController` via mock-friendly construction or invoke `GetMoveDiagnostics` via the interface mock with a null `AppointmentItem ref`. If direct instantiation is infeasible (due to UI dependencies), mock `IQfcCollectionController` and write the test against the guard behavior; record why direct construction is not viable.
  - Note: if direct construction of `QfcCollectionController` requires too many UI mocks, add the test to `QfcHomeControllerTests.cs` as a test of `WriteMetricsAsync` passing a null appointment through to `GetMoveDiagnostics` (end-to-end null path test).
  - Acceptance: Test added with `[TestMethod]` attribute; run confirms a `NullReferenceException` is thrown before the fix (test fails as expected). Evidence artifact saved.

- [x] [P1-T3] Fix `GetMoveDiagnostics` in `QuickFiler/Controllers/QfcCollectionController.cs`: wrap the `olAppointment.Body` access block (lines 2115–2124) in `if (olAppointment is not null) { ... }`.
  - Precondition: P1-T1 and P1-T2 added and confirmed to fail.
  - Acceptance: In the loop body, the block `if (string.IsNullOrEmpty(olAppointment.Body)) { ... } else { ... }` is wrapped with `if (olAppointment is not null)`. No other changes to the method body or return type.

- [x] [P1-T4] Fix `QuickFileMetrics_WRITE` in `QuickFiler/Controllers/QfcHomeController.cs`: guard `olEmailCalendar` before calling `olEmailCalendar.Items.Add()`.
  - Precondition: P1-T3 complete.
  - Current code at line ~419: `var olAppointment = (AppointmentItem)olEmailCalendar.Items.Add();`
  - Fix: Introduce a null check before this line. If `olEmailCalendar` is null, set `olAppointment` to null and skip the appointment-setup block. Then pass `olAppointment` (which may be null) to `GetMoveDiagnostics` — which is now guarded by P1-T3.
  - Acceptance: `olEmailCalendar.Items.Add()` is only called when `olEmailCalendar is not null`. The method still calls `GetMoveDiagnostics` and writes to file when MyDocuments is available. The null path for `olEmailCalendar` results in `olAppointment = null` and no exception.

---

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run `dotnet tool run csharpier format .` and confirm no files changed.
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-format.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: dotnet tool run csharpier format .`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming 0 files changed; if files were changed, they were saved and this step was rerun from the format step.

- [x] [P2-T2] Run the lint/analyzer build and confirm 0 errors.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-lint.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` build succeeded with 0 errors, 0 new warnings relative to baseline

- [ ] [P2-T3] Run the nullable/type-check build and confirm 0 errors.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-nullable.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` build succeeded with 0 errors, 0 new nullable warnings

- [ ] [P2-T4] Run the targeted regression test filter and confirm the new tests now pass.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~NullAppointment OR FullyQualifiedName~GetCalendarReturnsNull"`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-regression-tests.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` both new regression tests pass (2 passed, 0 failed)

- [ ] [P2-T5] Run the full QuickFiler.Test suite with coverage enabled and confirm no regressions.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-coverage.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` all previously passing tests still pass; numeric post-change coverage >= baseline coverage from P0-T6; new regression tests included in passing count

- [ ] [P2-T6] Check off satisfied acceptance criteria in `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md` and produce a final AC status summary.
  - Acceptance: Each AC item in `issue.md` that is satisfied by the work is checked off (`[x]`). A summary of AC status is written to this plan (or to a final-state evidence artifact).
