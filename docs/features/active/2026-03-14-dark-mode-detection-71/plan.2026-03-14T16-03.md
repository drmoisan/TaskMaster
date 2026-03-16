# 2026-03-15-dark-mode-detection - Plan

- **Issue:** #71
- **Parent (optional):** none
- **Owner:** drmoisan
- **Requirements Source:** `docs\features\active\2026-03-14-dark-mode-detection-71\issue.md`
- **Last Updated:** 2026-03-14T16-03
- **Status:** Draft
- **Version:** 1.1
- **Work Mode:** `minor-audit`

## Required References

- [`.github/copilot-instructions.md`](../../../../.github/copilot-instructions.md)
- [`.github/instructions/general-code-change.instructions.md`](../../../../.github/instructions/general-code-change.instructions.md)
- [`.github/instructions/general-unit-test.instructions.md`](../../../../.github/instructions/general-unit-test.instructions.md)
- [`.github/instructions/csharp-code-change.instructions.md`](../../../../.github/instructions/csharp-code-change.instructions.md)
- [`.github/instructions/csharp-unit-test.instructions.md`](../../../../.github/instructions/csharp-unit-test.instructions.md)

**All work must comply with these policies; do not duplicate their content here.**

## Overview

Implement Windows-registry-based Dark/Light Mode detection for the TaskMaster Outlook add-in.
The change introduces `SystemThemeDetector` to read
`HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme`
and use that result to initialize `AppOlObjects._darkMode` instead of the persisted settings value.
Deterministic MSTest unit tests cover the dark-theme, light-theme, and detection-unavailable fallback paths without depending on live machine registry state.

This plan is intentionally structured as a `minor-audit` plan with exactly three phases:
baseline capture, constrained implementation plus targeted verification evidence, and a final QC loop with reduced-audit end-state evidence.

---

## Affected Files

| File | Change Type |
|---|---|
| `UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs` | New production class |
| `UtilitiesCS/UtilitiesCS.csproj` | Modified to add `HelperClasses\ThemeHelpers\SystemThemeDetector.cs` compile entry |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | Modified to update the `_darkMode` field initializer |
| `UtilitiesCS.Test/ThemeHelpers/SystemThemeDetectorTests.cs` | New MSTest class |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Modified to add `ThemeHelpers\SystemThemeDetectorTests.cs` compile entry |

---

## Requirements Traceability

| Requirement Source | Coverage in Plan |
|---|---|
| `docs\features\active\2026-03-14-dark-mode-detection-71\issue.md` | Entire plan |

---

### Phase 0 — Context & Inputs (Policy Reads + Baseline Capture)

- [x] [P0-T1] Read the five mandatory policy files in the required order:
  1. `.github/copilot-instructions.md`
  2. `.github/instructions/general-code-change.instructions.md`
  3. `.github/instructions/general-unit-test.instructions.md`
  4. `.github/instructions/csharp-code-change.instructions.md`
  5. `.github/instructions/csharp-unit-test.instructions.md`
  - **Acceptance:** All five files have been read; no conflicting instructions detected. Save evidence artifact `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/phase0-instructions-read-2026-03-14T16-03.md` with fields:
    - `Timestamp: <ISO-8601 at time of read>`
    - `Policy Order: [list-of-5-files-in-sequence]`
    - `Output Summary: 5/5 policy files read — no conflicts detected`

- [x] [P0-T2] Read the existing repository change plan in `change-plan.md` before any code changes and deterministically confirm that no cross-reference update is required for Issue #71.
  - **Acceptance:** `change-plan.md` has been reviewed and evidence artifact `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/phase0-change-plan-read-2026-03-14T16-03.md` exists with fields:
    - `Timestamp: <ISO-8601>`
    - `Source: change-plan.md`
    - `Output Summary: change-plan.md reviewed; current objective noted; no Issue #71 cross-reference update required; Issue #71 feature plan remains the execution plan of record`
  - Evidence missing (status_updater, 2026-03-15T20-11): `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/phase0-change-plan-read-2026-03-14T16-03.md` was not found.

- [x] [P0-T3] Run `msbuild TaskMaster.sln /t:Restore /p:RestorePackagesConfig=true /p:Configuration=Debug /p:Platform='Any CPU'` to verify the solution restores cleanly before any code changes.
  - **Acceptance:** Command exits with code `0` and saves `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-restore-2026-03-14T16-03.md` with fields:
    - `Timestamp: <ISO-8601>`
    - `Command: msbuild TaskMaster.sln /t:Restore /p:RestorePackagesConfig=true /p:Configuration=Debug /p:Platform='Any CPU'`
    - `EXIT_CODE: 0`
    - `Output Summary: Restore succeeded with packages.config hydration`
  - Evidence missing (status_updater, 2026-03-15T20-11): `baseline-restore-2026-03-14T16-03.md` exists, but it records `dotnet restore TaskMaster.sln`, not the required `msbuild ... /t:Restore /p:RestorePackagesConfig=true` command.

- [x] [P0-T4] Run `dotnet format TaskMaster.sln` to capture the baseline C# formatter state.
  - **Acceptance:** Command exits with code `0` and saves `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-format-2026-03-14T16-03.md` with fields:
    - `Timestamp: <ISO-8601>`
    - `Command: dotnet format TaskMaster.sln`
    - `EXIT_CODE: 0`
    - `Output Summary: Format complete — 0 files changed`
  - Evidence missing (status_updater, 2026-03-15T20-11): no baseline formatter artifact matching `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-format-2026-03-14T16-03.md` was found.

- [x] [P0-T5] Run MSBuild with .NET analyzers enabled to capture the baseline C# lint/analyzer state.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    and save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-build-analyzer-2026-03-14T16-03.md` with fields:
    - `Timestamp: <ISO-8601>`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Build succeeded — 0 Error(s)`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `baseline-build-analyzer-2026-03-14T16-03.md` artifact was found; `baseline-build-2026-03-14T16-03.md` records a different command.

- [x] [P0-T6] Run MSBuild with nullable analysis treated as errors to capture the baseline C# type-check state.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    and save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-build-nullable-2026-03-14T16-03.md` with fields:
    - `Timestamp: <ISO-8601>`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Build succeeded — 0 Error(s)`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `baseline-build-nullable-2026-03-14T16-03.md` artifact was found.

- [ ] [P0-T7] Run the MSTest baseline command through `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and record a numeric baseline coverage headline value.
  - **Acceptance:** Task completes and saves `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-test-2026-03-14T16-03.md` with fields:
    - `Timestamp: <ISO-8601>`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary: <passed-count> passed, <failed-count> failed, Line Coverage: <baseline-percent>%`
  - Evidence missing (status_updater, 2026-03-15T20-11): `baseline-test-2026-03-14T16-03.md` exists, but it records `Invoke-MSTest.ps1`, has `EXIT_CODE: 1`, and does not include numeric line coverage.

- [ ] [P0-T8] Capture baseline coverage for the pre-existing `AppOlObjects` initializer line that will be replaced and record the deterministic new-file exception for `SystemThemeDetector.cs`.
  - **Acceptance:** Run the exact command
    `pwsh -NoProfile -Command "[xml]$xml = Get-Content 'coverage\coverage.cobertura.xml'; $line = (Select-String -Path 'TaskMaster\AppGlobals\AppOlObjects.cs' -Pattern '_darkMode = Properties.Settings.Default.DarkMode').LineNumber; $classNode = $xml.SelectNodes('//class') | Where-Object { $_.filename -eq 'TaskMaster/AppGlobals/AppOlObjects.cs' } | Select-Object -First 1; if (-not $classNode) { exit 1 }; $lineNode = $classNode.SelectNodes('./lines/line') | Where-Object { [int]$_.number -eq $line } | Select-Object -First 1; if (-not $lineNode) { exit 1 }; $hits = [int]$lineNode.hits; $covered = if ($hits -gt 0) { 'yes' } else { 'no' }; Write-Output \"Baseline Existing Changed Line: TaskMaster/AppGlobals/AppOlObjects.cs:$line; Hits=$hits; Covered=$covered\"; Write-Output \"New File Baseline Exception: UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs\""`
    and save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-changed-line-seed-2026-03-14T16-03.md` with:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary: Baseline Existing Changed Line: TaskMaster/AppGlobals/AppOlObjects.cs:<line>; Hits=<hits>; Covered=yes|no; New File Baseline Exception: UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `baseline-changed-line-seed-2026-03-14T16-03.md` artifact was found.

- [x] [P0-T9] Record a coverage-remediation-required artifact and stop before Phase 1 if either `P0-T7` or `P0-T8` cannot produce the required numeric baseline coverage evidence.
  - **Acceptance:** If either prerequisite task cannot be completed, save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/other/coverage-remediation-required-2026-03-14T16-03.md` with these exact lines:
    - `Coverage Baseline Status: remediation-required`
    - `Missing Artifact: baseline-test-2026-03-14T16-03.md|baseline-changed-line-seed-2026-03-14T16-03.md`
    - `Next Step: remediate coverage tooling or baseline evidence before Phase 1`
    - `Reduced Audit Status: BLOCKED`
  - **Execution rule:** Do not start `P1-T1` unless `P0-T7` and `P0-T8` both completed successfully; execute `P0-T9` instead when baseline coverage evidence is unavailable.
  - Evidence missing (status_updater, 2026-03-15T20-11): no `coverage-remediation-required-2026-03-14T16-03.md` artifact was found even though `P0-T7`/`P0-T8` evidence is incomplete.

---

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] Create the file `UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs` with the approved production implementation.
  - **Acceptance:** File exists at `UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs`, and running
    `Select-String -Path "UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs" -Pattern "TryGetIsSystemDarkMode"`
    returns at least one match.
  - Evidence (status_updater, 2026-03-15T20-11): `UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs` exists in the current diff and contains `TryGetIsSystemDarkMode`.

- [x] [P1-T2] Add `SystemThemeDetector.cs` to `UtilitiesCS/UtilitiesCS.csproj` immediately after the existing `ThemeControlGroup.cs` compile entry.
  - **Acceptance:** Running
    `Select-String -Path "UtilitiesCS\UtilitiesCS.csproj" -Pattern "HelperClasses\\ThemeHelpers\\SystemThemeDetector.cs"`
    returns exactly one result, and the entry appears directly after `ThemeControlGroup.cs`.
  - Evidence (status_updater, 2026-03-15T20-11): current diff shows a single compile include added immediately after `ThemeControlGroup.cs` in `UtilitiesCS/UtilitiesCS.csproj`.

- [x] [P1-T3] Replace the `_darkMode` field initializer in `TaskMaster/AppGlobals/AppOlObjects.cs` with `SystemThemeDetector.IsSystemDarkMode()`.
  - **Acceptance:** Running
    `Select-String -Path "TaskMaster\AppGlobals\AppOlObjects.cs" -Pattern "SystemThemeDetector.IsSystemDarkMode"`
    returns exactly one match, and `Properties.Settings.Default.DarkMode` remains referenced only inside the `DarkMode` setter.
  - Evidence (status_updater, 2026-03-15T20-11): current diff shows `_darkMode` changed from `Properties.Settings.Default.DarkMode` to `SystemThemeDetector.IsSystemDarkMode()` in `TaskMaster/AppGlobals/AppOlObjects.cs`.

- [ ] [P1-T4] Create `UtilitiesCS.Test/ThemeHelpers/SystemThemeDetectorTests.cs` with three deterministic MSTest methods covering dark-theme, light-theme, and detection-unavailable fallback scenarios.
  - **Acceptance:** File exists at `UtilitiesCS.Test/ThemeHelpers/SystemThemeDetectorTests.cs`, and running
    `pwsh -NoProfile -Command "$content = Get-Content 'UtilitiesCS.Test\ThemeHelpers\SystemThemeDetectorTests.cs' -Raw; $count = (Select-String -Path 'UtilitiesCS.Test\ThemeHelpers\SystemThemeDetectorTests.cs' -Pattern '\[TestMethod\]').Count; $dark = [regex]::IsMatch($content, 'IsSystemDarkMode_ReturnsTrue_WhenRegistryReportsDarkTheme'); $light = [regex]::IsMatch($content, 'IsSystemDarkMode_ReturnsFalse_WhenRegistryReportsLightTheme'); $fallback = [regex]::IsMatch($content, 'IsSystemDarkMode_FallsBackToSavedTheme_WhenThemeDetectionUnavailable'); Write-Output \"TestMethodCount=$count; HasDarkScenario=$dark; HasLightScenario=$light; HasFallbackScenario=$fallback\""`
    returns `TestMethodCount=3; HasDarkScenario=True; HasLightScenario=True; HasFallbackScenario=True`.
  - Evidence missing (status_updater, 2026-03-15T20-11): `UtilitiesCS.Test/ThemeHelpers/SystemThemeDetectorTests.cs` exists, but it currently contains 2 machine-dependent tests (`IsSystemDarkMode_ReturnsBoolean`, `TryGetIsSystemDarkMode_ReturnsTrue_WhenRegistryReadable`) rather than 3 deterministic dark/light/fallback scenarios.

- [x] [P1-T5] Add `SystemThemeDetectorTests.cs` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` immediately after the existing `SysImageListHelperTests.cs` compile entry.
  - **Acceptance:** Running
    `Select-String -Path "UtilitiesCS.Test\UtilitiesCS.Test.csproj" -Pattern "ThemeHelpers\\SystemThemeDetectorTests.cs"`
    returns exactly one result.
  - Evidence (status_updater, 2026-03-15T20-11): current diff shows `ThemeHelpers\SystemThemeDetectorTests.cs` added once to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` immediately after `SysImageListHelperTests.cs`.

- [ ] [P1-T6] Delegate constrained small-path implementation verification to the implementation executor.
  - **Acceptance:** Artifact `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/other/minor-audit-implementation-handoff-2026-03-14T16-03.md` exists with:
    - `Timestamp: <ISO-8601>`
    - `Handoff Target: constrained-small-path-implementation`
    - `Requirements Source: docs\features\active\2026-03-14-dark-mode-detection-71\issue.md`
    - `Output Summary: Small-path implementation handoff recorded`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `minor-audit-implementation-handoff-2026-03-14T16-03.md` artifact was found.

- [ ] [P1-T7] Capture targeted production-wiring verification evidence in `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/regression-testing/targeted-verification-production-2026-03-14T16-03.md`.
  - **Acceptance:** Run the exact command
    `pwsh -NoProfile -Command "$a=(Select-String -Path 'TaskMaster\AppGlobals\AppOlObjects.cs' -Pattern 'SystemThemeDetector.IsSystemDarkMode').Count; $b=(Select-String -Path 'UtilitiesCS\UtilitiesCS.csproj' -Pattern 'HelperClasses\\ThemeHelpers\\SystemThemeDetector.cs').Count; Write-Output \"AppOlObjectsMatches=$a; UtilitiesCSProjectMatches=$b\""`
    and save an artifact containing:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary: AppOlObjectsMatches=1; UtilitiesCSProjectMatches=1`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `targeted-verification-production-2026-03-14T16-03.md` artifact was found.

- [ ] [P1-T8] Capture targeted test-registration verification evidence in `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/regression-testing/targeted-verification-tests-2026-03-14T16-03.md`.
  - **Acceptance:** Run the exact command
    `pwsh -NoProfile -Command "$a=(Select-String -Path 'UtilitiesCS.Test\UtilitiesCS.Test.csproj' -Pattern 'ThemeHelpers\\SystemThemeDetectorTests.cs').Count; $content = Get-Content 'UtilitiesCS.Test\ThemeHelpers\SystemThemeDetectorTests.cs' -Raw; $b=(Select-String -Path 'UtilitiesCS.Test\ThemeHelpers\SystemThemeDetectorTests.cs' -Pattern '\[TestMethod\]').Count; $c=[regex]::IsMatch($content,'IsSystemDarkMode_ReturnsTrue_WhenRegistryReportsDarkTheme'); $d=[regex]::IsMatch($content,'IsSystemDarkMode_ReturnsFalse_WhenRegistryReportsLightTheme'); $e=[regex]::IsMatch($content,'IsSystemDarkMode_FallsBackToSavedTheme_WhenThemeDetectionUnavailable'); Write-Output \"TestProjectMatches=$a; TestMethodCount=$b; HasDarkScenario=$c; HasLightScenario=$d; HasFallbackScenario=$e\""`
    and save an artifact containing:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary: TestProjectMatches=1; TestMethodCount=3; HasDarkScenario=True; HasLightScenario=True; HasFallbackScenario=True`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `targeted-verification-tests-2026-03-14T16-03.md` artifact was found.

- [ ] [P1-T9] Record constrained small-path implementation completion in `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/other/minor-audit-implementation-complete-2026-03-14T16-03.md`.
  - **Acceptance:** The artifact exists and contains these exact lines:
    - `Touched Files Count: 5`
    - `Production Verification Artifact: docs/features/active/2026-03-14-dark-mode-detection-71/evidence/regression-testing/targeted-verification-production-2026-03-14T16-03.md`
    - `Test Verification Artifact: docs/features/active/2026-03-14-dark-mode-detection-71/evidence/regression-testing/targeted-verification-tests-2026-03-14T16-03.md`
    - `Scope Status: constrained-small-path-complete`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `minor-audit-implementation-complete-2026-03-14T16-03.md` artifact was found.

---

### Phase 2 — Final QC Loop

Run the full C# toolchain loop. If any step exits non-zero or modifies files, fix the reported issues and restart the loop from `P2-T1`.

- [ ] [P2-T1] Run `dotnet format TaskMaster.sln`.
  - **Acceptance:** Command exits `0`. Save evidence artifact `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-format-2026-03-14T16-03.md` with fields:
    - `Timestamp: <ISO-8601>`
    - `Command: dotnet format TaskMaster.sln`
    - `EXIT_CODE: 0`
    - `Output Summary: Format complete — 0 files changed`
  - If formatting changes any files, restart the QA loop from `P2-T1` after the changes are reviewed.
  - Evidence missing (status_updater, 2026-03-15T20-11): `final-qa-format-2026-03-14T16-03.md` exists, but it records `EXIT_CODE: 1`, so the QA format gate is not complete.

- [x] [P2-T2] Run MSBuild with .NET analyzers enabled.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    and save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-analyzer-2026-03-14T16-03.md` with:
    - `Timestamp: <ISO-8601>`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Build succeeded — 0 Error(s)`
  - Evidence (status_updater, 2026-03-15T20-11): `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-analyzer-2026-03-14T16-03.md` records the required command with `EXIT_CODE: 0`.

- [x] [P2-T3] Run MSBuild with nullable analysis treated as errors.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    and save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-nullable-2026-03-14T16-03.md` with:
    - `Timestamp: <ISO-8601>`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Build succeeded — 0 Error(s)`
  - Evidence (status_updater, 2026-03-15T20-11): `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-nullable-2026-03-14T16-03.md` records the required command with `EXIT_CODE: 0`.

- [ ] [P2-T4] Run the MSTest workspace command and record the post-change coverage headline values.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-test-2026-03-14T16-03.md` with:
    - `Timestamp: <ISO-8601>`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary: <passed-count> passed (including IsSystemDarkMode_ReturnsTrue_WhenRegistryReportsDarkTheme, IsSystemDarkMode_ReturnsFalse_WhenRegistryReportsLightTheme, and IsSystemDarkMode_FallsBackToSavedTheme_WhenThemeDetectionUnavailable), 0 failed, Line Coverage: <final-percent>%`
  - Evidence missing (status_updater, 2026-03-15T20-11): `final-qa-test-2026-03-14T16-03.md` exists, but it records `Invoke-MSTest.ps1`, has `EXIT_CODE: 1`, and does not include the required coverage headline.

- [ ] [P2-T5] Run an explicit changed-code coverage calculation command against `coverage\coverage.cobertura.xml` and the current diff for `UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs` and `TaskMaster/AppGlobals/AppOlObjects.cs`, compare final coverage for the changed `AppOlObjects` lines against the `P0-T8` baseline seed, record the deterministic new-file exception for `SystemThemeDetector.cs`, fail the task if repo coverage drops below 80%, if new/changed code coverage is below 90%, or if existing changed-line coverage regresses, then save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-coverage-delta-2026-03-14T16-03.md`.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$baselineLineCoverage = [double](([regex]::Match((Get-Content 'docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-test-2026-03-14T16-03.md' -Raw), 'Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); $baselineSeed = Get-Content 'docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-changed-line-seed-2026-03-14T16-03.md' -Raw; $baselineExistingCovered = ([regex]::Match($baselineSeed, 'Covered=(yes|no)')).Groups[1].Value; [xml]$xml = Get-Content 'coverage\coverage.cobertura.xml'; $files = @('UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs','TaskMaster/AppGlobals/AppOlObjects.cs'); $newFile = 'UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs'; $diff = git diff --unified=0 -- $files; $ranges = @{}; $oldRanges = @{}; $current = $null; foreach ($line in $diff) { if ($line -match '^\+\+\+ b/(.+)$') { $current = $Matches[1]; if (-not $ranges.ContainsKey($current)) { $ranges[$current] = New-Object System.Collections.ArrayList }; if (-not $oldRanges.ContainsKey($current)) { $oldRanges[$current] = New-Object System.Collections.ArrayList } } elseif ($line -match '^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@' -and $current) { $oldStart = [int]$Matches[1]; $oldCount = if ($Matches[2]) { [int]$Matches[2] } else { 1 }; $newStart = [int]$Matches[3]; $newCount = if ($Matches[4]) { [int]$Matches[4] } else { 1 }; if ($oldCount -gt 0) { [void]$oldRanges[$current].Add([pscustomobject]@{ Start = $oldStart; End = ($oldStart + $oldCount - 1) }) }; if ($newCount -gt 0) { [void]$ranges[$current].Add([pscustomobject]@{ Start = $newStart; End = ($newStart + $newCount - 1) }) } } }; $classNodes = $xml.SelectNodes('//class'); $newFileNode = $classNodes | Where-Object { $_.filename -eq $newFile } | Select-Object -First 1; if ($newFileNode -and (-not $ranges.ContainsKey($newFile) -or $ranges[$newFile].Count -eq 0)) { $ranges[$newFile] = New-Object System.Collections.ArrayList; $lineNodes = $newFileNode.SelectNodes('./lines/line'); if ($lineNodes.Count -gt 0) { $minLine = (($lineNodes | ForEach-Object { [int]$_.number }) | Measure-Object -Minimum).Minimum; $maxLine = (($lineNodes | ForEach-Object { [int]$_.number }) | Measure-Object -Maximum).Maximum; [void]$ranges[$newFile].Add([pscustomobject]@{ Start = $minLine; End = $maxLine }) } }; $classNodes = $xml.SelectNodes('//class'); $changedCovered = 0; $changedTotal = 0; foreach ($file in $files) { if (-not $ranges.ContainsKey($file)) { continue }; $classNode = $classNodes | Where-Object { $_.filename -eq $file } | Select-Object -First 1; if (-not $classNode) { continue }; $hitsByLine = @{}; foreach ($lineNode in $classNode.SelectNodes('./lines/line')) { $hitsByLine[[int]$lineNode.number] = [int]$lineNode.hits }; foreach ($range in $ranges[$file]) { for ($lineNumber = $range.Start; $lineNumber -le $range.End; $lineNumber++) { $changedTotal++; if ($hitsByLine.ContainsKey($lineNumber) -and $hitsByLine[$lineNumber] -gt 0) { $changedCovered++ } } } }; $appNode = $classNodes | Where-Object { $_.filename -eq 'TaskMaster/AppGlobals/AppOlObjects.cs' } | Select-Object -First 1; $appHits = @{}; foreach ($lineNode in $appNode.SelectNodes('./lines/line')) { $appHits[[int]$lineNode.number] = [int]$lineNode.hits }; $existingChangedCovered = 0; $existingChangedTotal = 0; foreach ($range in $ranges['TaskMaster/AppGlobals/AppOlObjects.cs']) { for ($lineNumber = $range.Start; $lineNumber -le $range.End; $lineNumber++) { $existingChangedTotal++; if ($appHits.ContainsKey($lineNumber) -and $appHits[$lineNumber] -gt 0) { $existingChangedCovered++ } } }; $existingChangedCoverage = if ($existingChangedTotal -gt 0) { [math]::Round(($existingChangedCovered / $existingChangedTotal) * 100, 2) } else { 100 }; $existingChangedNoRegression = if (($baselineExistingCovered -eq 'yes' -and $existingChangedCoverage -eq 100) -or ($baselineExistingCovered -eq 'no')) { 'yes' } else { 'no' }; $finalLineCoverage = [math]::Round(([double]$xml.coverage.'line-rate') * 100, 2); $changedCoverage = if ($changedTotal -gt 0) { [math]::Round(($changedCovered / $changedTotal) * 100, 2) } else { 100 }; $repoThresholdMet = if ($finalLineCoverage -ge 80) { 'yes' } else { 'no' }; $changedCoverageThresholdMet = if ($changedCoverage -ge 90) { 'yes' } else { 'no' }; Write-Output \"Baseline Line Coverage: $baselineLineCoverage%\"; Write-Output \"Final Line Coverage: $finalLineCoverage%\"; Write-Output \"Changed Production Lines Covered: $changedCovered/$changedTotal\"; Write-Output \"New/Changed Code Coverage: $changedCoverage%\"; Write-Output \"Baseline Existing Changed Line Coverage: $baselineExistingCovered\"; Write-Output \"Existing Changed Lines Covered: $existingChangedCovered/$existingChangedTotal\"; Write-Output \"Existing Changed-Line No-Regression: $existingChangedNoRegression\"; Write-Output \"New File Baseline Exception: UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs\"; Write-Output \"Repo Threshold Met: $repoThresholdMet\"; Write-Output \"New/Changed Code >= 90%: $changedCoverageThresholdMet\"; if ($repoThresholdMet -ne 'yes' -or $changedCoverageThresholdMet -ne 'yes' -or $existingChangedNoRegression -ne 'yes') { exit 1 }"`
    and save `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-coverage-delta-2026-03-14T16-03.md` with:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary: Baseline Line Coverage: <baseline-percent>%, Final Line Coverage: <final-percent>%, Changed Production Lines Covered: <covered-lines>/<total-lines>, New/Changed Code Coverage: <changed-code-percent>%, Baseline Existing Changed Line Coverage: yes|no, Existing Changed Lines Covered: <covered-lines>/<total-lines>, Existing Changed-Line No-Regression: yes, New File Baseline Exception: UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs, Repo Threshold Met: yes, New/Changed Code >= 90%: yes`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `final-qa-coverage-delta-2026-03-14T16-03.md` artifact was found.

- [ ] [P2-T6] Confirm the five per-command QA artifacts exist and each records `EXIT_CODE: 0`.
  - **Acceptance:** All five files exist:
    - `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-format-2026-03-14T16-03.md`
    - `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-analyzer-2026-03-14T16-03.md`
    - `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-nullable-2026-03-14T16-03.md`
    - `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-test-2026-03-14T16-03.md`
    - `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-coverage-delta-2026-03-14T16-03.md`
  - Running `pwsh -NoProfile -Command "$files = Get-ChildItem 'docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/*2026-03-14T16-03.md'; $count = $files.Count; $exitCodeMatches = (Select-String -Path $files.FullName -Pattern '^EXIT_CODE: 0$').Count; Write-Output \"QaArtifactCount=$count; ExitCodeZeroCount=$exitCodeMatches\""` returns `QaArtifactCount=5; ExitCodeZeroCount=5`.
  - Evidence missing (status_updater, 2026-03-15T20-11): only 4 matching QA artifacts are present, and the existing format/test artifacts record non-zero exit codes.

- [ ] [P2-T7] Delegate reduced-audit review using the completed QA artifacts.
  - **Acceptance:** Artifact `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/other/minor-audit-reduced-audit-handoff-2026-03-14T16-03.md` exists with:
    - `Timestamp: <ISO-8601>`
    - `Handoff Target: reduced-audit-review`
    - `QA Artifact Set: docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-format-2026-03-14T16-03.md; docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-analyzer-2026-03-14T16-03.md; docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-build-nullable-2026-03-14T16-03.md; docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-test-2026-03-14T16-03.md; docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-coverage-delta-2026-03-14T16-03.md`
    - `Output Summary: Reduced-audit handoff recorded`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `minor-audit-reduced-audit-handoff-2026-03-14T16-03.md` artifact was found.

- [ ] [P2-T8] Record reduced-audit end-state evidence in `docs/features/active/2026-03-14-dark-mode-detection-71/evidence/other/minor-audit-end-state-2026-03-14T16-03.md`.
  - **Acceptance:** The artifact exists and contains these exact lines:
    - `Baseline Coverage Artifact: docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-test-2026-03-14T16-03.md`
    - `Baseline Changed-Line Artifact: docs/features/active/2026-03-14-dark-mode-detection-71/evidence/baseline/baseline-changed-line-seed-2026-03-14T16-03.md`
    - `Final Coverage Artifact: docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-test-2026-03-14T16-03.md`
    - `Coverage Delta Artifact: docs/features/active/2026-03-14-dark-mode-detection-71/evidence/qa-gates/final-qa-coverage-delta-2026-03-14T16-03.md`
    - `Baseline Line Coverage: <baseline-percent>%`
    - `Final Line Coverage: <final-percent>%`
    - `New/Changed Code Coverage: <changed-code-percent>%`
    - `New File Baseline Exception: UtilitiesCS/HelperClasses/ThemeHelpers/SystemThemeDetector.cs`
    - `New/Changed Code >= 90%: yes`
    - `Repo Threshold Met: yes`
    - `Existing Changed-Line No-Regression: yes`
    - `Reduced Audit Status: READY`
  - Evidence missing (status_updater, 2026-03-15T20-11): no `minor-audit-end-state-2026-03-14T16-03.md` artifact was found.

---

## Plan Self-Check

| Gate | Status |
|---|---|
| Canonical phase headings (`### Phase N — <Title>`) | ✅ |
| Task IDs (`[P#-T#]`) sequential per phase | ✅ |
| Exactly 3 phases for `minor-audit` | ✅ |
| Zero forbidden placeholder tokens | ✅ |
| One outcome per atomic task | ✅ |
| Machine-verifiable acceptance criteria | ✅ |
| Phase 0 includes policy-read evidence plus baseline C# format → analyzer → nullable → test artifacts | ✅ |
| Phase 1 includes explicit small-path implementation handoff + targeted verification evidence tasks | ✅ |
| Phase 2 includes format → analyzer → nullable → test QA loop | ✅ |
| Baseline and final test artifacts require numeric coverage values | ✅ |
| Phase 2 includes explicit reduced-audit handoff + end-state evidence task | ✅ |

---

## Preflight Validation

**DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED**

**DIRECTIVE: PREFLIGHT VALIDATION ONLY**

Plan format and executability self-check:

- All phases use canonical headings.
- The plan contains exactly three phases, matching the `minor-audit` contract.
- Phase 0 includes policy reads plus baseline C# format, analyzer, nullable, and coverage-enabled test evidence tasks.
- Phase 1 includes an explicit constrained small-path implementation handoff plus targeted verification evidence tasks.
- Phase 2 includes the full C# QA loop, an explicit reduced-audit handoff, and reduced-audit end-state evidence.
- The baseline and final MSTest artifacts require numeric coverage values in `Output Summary`.
- Later QA tasks have been reset so execution can proceed in strict order from the first unchecked task.
- Required references list the exact C# policy files instead of placeholder text.
- No forbidden placeholder tokens remain.
