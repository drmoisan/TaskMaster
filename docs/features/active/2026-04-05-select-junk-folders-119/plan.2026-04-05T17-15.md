# 2026-04-05-select-junk-folders - Plan

- **Issue:** #119
- **Owner:** drmoisan
- **Branch:** `feature/select-junk-folders-119`
- **Work Mode:** `minor-audit`
- **Requirements Source:** `docs/features/active/2026-04-05-select-junk-folders-119/issue.md` only
- **Last Updated:** 2026-04-05T17-15
- **Status:** Draft

## Directives

- `DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED`
- `DIRECTIVE: PREFLIGHT VALIDATION ONLY AFTER PLAN UPDATE UNTIL ALL CLEAR`

## Overview

Create a small-scope C# change that adds a discoverable UI entry point for selecting the Outlook folders used for confirmed junk and potential junk, persists those selections through the existing `AppOlObjects` settings keys, refreshes cached folder references after Save, and preserves current state on Cancel. Keep implementation limited to the existing UI entry point plus the minimum persistence and cache-refresh work needed to satisfy the issue acceptance criteria.

## Policy Inputs

- `.github/copilot-instructions.md`
- `.github/instructions/general-code-change.instructions.md`
- `.github/instructions/general-unit-test.instructions.md`
- `.github/instructions/csharp-code-change.instructions.md`
- `.github/instructions/csharp-unit-test.instructions.md`

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read the policy files listed in `## Policy Inputs` plus `docs/features/active/2026-04-05-select-junk-folders-119/issue.md`, then save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/phase0-instructions-read.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Policy Order:`, `Files Read:`, and `Requirements Source: issue.md only`.
- [x] [P0-T2] Verify that `docs/features/active/2026-04-05-select-junk-folders-119/` does not contain `spec.md`, `user-story.md`, or `research.md`, then save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/phase0-feature-folder-scope.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `SearchScope: docs/features/active/2026-04-05-select-junk-folders-119/`, `SearchPatterns: spec.md, user-story.md, research.md`, and `SearchResult: none`.
- [x] [P0-T3] Run `dotnet tool run csharpier format .` from the repository root and save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/p0-t3-csharpier.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` from the repository root and save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/p0-t4-analyzers.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` from the repository root and save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/p0-t5-nullable.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\select-junk-folders-baseline.cobertura.xml` from the repository root and save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/baseline/p0-t6-mstest-coverage.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\select-junk-folders-baseline.cobertura.xml`, `EXIT_CODE:`, and `Output Summary:` including numeric total, passed, failed, and skipped test counts, numeric overall line coverage from `coverage/select-junk-folders-baseline.cobertura.xml`, and the coverage artifact path.

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] Record the selected small-path production files, selected MSTest files, and the existing UI launch chain for this feature in `docs/features/active/2026-04-05-select-junk-folders-119/evidence/other/p1-t1-scope.md`.
  - Acceptance: The artifact exists, names the selected launch chain, identifies whether the implementation extends `RibbonViewer.FolderSettings_Click` -> `RibbonController.FolderStoresSettings()` -> `StoreWrapperController.Launch()` or a narrower existing hook, states `Production File Count:` with a value from 1 through 3, states `Test File Count:` with a value from 1 through 3, includes `Production Files CSV:` with a comma-separated repo-relative list of the touched production `.cs` files, includes `Test Files CSV:` with a comma-separated repo-relative list of the touched MSTest `.cs` files, and states `Public API Changes: none` unless a specific existing API adjustment is explicitly listed.
- [x] [P1-T2] Update the selected existing UI launch chain from `p1-t1-scope.md` so the user can intentionally open the junk-folder selector without waiting for the current folder-resolution failure path.
  - Acceptance: The changed UI path is traceable from the launch chain named in `p1-t1-scope.md` to the junk-folder selection flow, and the implementation stays within the file-count limits recorded in `p1-t1-scope.md`.
- [x] [P1-T3] Update `StoreWrapperController.PopulateWithCurrent()` or the narrower selected UI-population method from `p1-t1-scope.md` so the current confirmed-junk and potential-junk selections are visible when the selector opens.
  - Acceptance: Opening the selector shows the current `Junk Email` and `Junk Potential` selections drawn from the existing settings-backed state.
- [x] [P1-T4] Update `StoreWrapperController.SaveChanges()` or the selected save method from `p1-t1-scope.md` so Save persists `OlJunkCertain` and `JunkPotential` through the existing relative-path storage path.
  - Acceptance: Save writes both junk-folder settings through the existing persistence path, stores relative paths, and does not introduce a parallel persistence mechanism.
- [x] [P1-T5] Refresh the cached `AppOlObjects` junk-folder references after Save through the selected save pipeline from `p1-t1-scope.md`.
  - Acceptance: Subsequent junk or potential-junk resolution uses the updated selections without requiring the existing reactive recovery prompt first.
- [x] [P1-T6] Update `StoreWrapperController.ButtonCancel_Click()` or the selected cancel method from `p1-t1-scope.md` so Cancel leaves the stored settings and active folder selections unchanged.
  - Acceptance: Cancel returns without changing `OlJunkCertain`, `JunkPotential`, or the active cached folder selections.
- [x] [P1-T7] Update `AppOlObjects.LoadJunkCertain()`, `AppOlObjects.LoadJunkPotential()`, or the selected unresolved-folder helper from `p1-t1-scope.md` so the user gets a clear re-selection path without an invalid overwrite.
  - Acceptance: The unresolved-folder path keeps the stored value unchanged until the user chooses a valid replacement and exposes a clear route back to folder selection.
- [x] [P1-T8] Add an MSTest for `StoreWrapperController.PopulateWithCurrent()` or the selected UI-population method showing the current `Junk Email` and `Junk Potential` selections in the selected test file from `p1-t1-scope.md`.
  - Acceptance: The new test fails if the selector opens without the current junk-folder selections populated and passes once the implementation is complete.
- [x] [P1-T9] Add an MSTest for `StoreWrapperController.SaveChanges()` or the selected save method persisting both settings keys and refreshing active junk-folder state in the selected test file from `p1-t1-scope.md`.
  - Acceptance: The new test fails if Save does not persist both settings or does not refresh the active junk-folder state and passes once the implementation is complete.
- [x] [P1-T10] Add an MSTest for `StoreWrapperController.ButtonCancel_Click()` or the selected cancel method leaving stored settings and active folder state unchanged in the selected test file from `p1-t1-scope.md`.
  - Acceptance: The new test fails if Cancel changes persisted settings or active folder state and passes once the implementation is complete.
- [x] [P1-T11] Add an MSTest for `AppOlObjects.LoadJunkCertain()`, `AppOlObjects.LoadJunkPotential()`, or the selected unresolved-folder helper keeping the stored value until a valid replacement is chosen in the selected test file from `p1-t1-scope.md`.
  - Acceptance: The new test fails if an invalid or unresolved folder overwrites the stored value and passes once the implementation is complete.

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run `dotnet tool run csharpier format .`, restart Phase 2 from `P2-T1` if the formatter changes files or exits non-zero, and save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t1-csharpier.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final pass.
- [x] [P2-T2] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`, restart Phase 2 from `P2-T1` if the command exits non-zero, and save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t2-analyzers.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final pass.
- [x] [P2-T3] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`, restart Phase 2 from `P2-T1` if the command exits non-zero, and save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t3-nullable.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`, `EXIT_CODE: 0`, and `Output Summary:` from the clean final pass.
- [x] [P2-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\select-junk-folders-final.cobertura.xml`, restart Phase 2 from `P2-T1` if the command exits non-zero, and save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t4-mstest-coverage.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\select-junk-folders-final.cobertura.xml`, `EXIT_CODE: 0`, and `Output Summary:` including numeric total, passed, failed, and skipped test counts, numeric overall line coverage from `coverage/select-junk-folders-final.cobertura.xml`, and the coverage artifact path.
- [x] [P2-T5] Compare `coverage/select-junk-folders-baseline.cobertura.xml` and `coverage/select-junk-folders-final.cobertura.xml` for the touched production files listed in `docs/features/active/2026-04-05-select-junk-folders-119/evidence/other/p1-t1-scope.md`, then save `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t5-coverage-delta.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: pwsh -NoProfile -Command "$scope = Get-Content 'docs/features/active/2026-04-05-select-junk-folders-119/evidence/other/p1-t1-scope.md' -Raw; $csv = ([regex]::Match($scope, 'Production Files CSV:\s*(.+)')).Groups[1].Value; if ([string]::IsNullOrWhiteSpace($csv)) { exit 1 }; $files = $csv.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ }; [xml]$baseline = Get-Content 'coverage/select-junk-folders-baseline.cobertura.xml'; [xml]$final = Get-Content 'coverage/select-junk-folders-final.cobertura.xml'; $baselineRepo = [math]::Round([double]$baseline.coverage.'line-rate' * 100, 2); $finalRepo = [math]::Round([double]$final.coverage.'line-rate' * 100, 2); Write-Output \"Baseline Repo Line Coverage: $baselineRepo%\"; Write-Output \"Final Repo Line Coverage: $finalRepo%\"; foreach ($file in $files) { $baselineClass = $baseline.SelectNodes('//class') | Where-Object { $_.filename -eq $file } | Select-Object -First 1; $finalClass = $final.SelectNodes('//class') | Where-Object { $_.filename -eq $file } | Select-Object -First 1; if (-not $finalClass) { exit 1 }; $baselineRate = if ($baselineClass) { [math]::Round([double]$baselineClass.'line-rate' * 100, 2) } else { -1 }; $finalRate = [math]::Round([double]$finalClass.'line-rate' * 100, 2); if ($baselineRate -lt 0) { $threshold = if ($finalRate -ge 90) { 'PASS' } else { 'FAIL' }; Write-Output \"FILE $file | BASELINE NEW | FINAL $finalRate% | New File Coverage Threshold: $threshold\" } else { $threshold = if ($finalRate -ge $baselineRate) { 'PASS' } else { 'FAIL' }; Write-Output \"FILE $file | BASELINE $baselineRate% | FINAL $finalRate% | Existing File No-Regression: $threshold\" } }; Write-Output \"Repo Coverage Threshold: $(if ($finalRepo -ge 80) { 'PASS' } else { 'FAIL' })\""`, `EXIT_CODE: 0`, and `Output Summary:` including numeric baseline overall line coverage, numeric final overall line coverage, each touched production file with numeric baseline and final line coverage, a `Repo Coverage Threshold:` result, a `New File Coverage Threshold:` result for any new production file, and an `Existing File No-Regression:` result for every previously existing touched production file.
- [x] [P2-T6] Record the minor-audit end-state summary in `docs/features/active/2026-04-05-select-junk-folders-119/evidence/qa-gates/p2-t6-end-state.md`.
  - Acceptance: The artifact exists and lists the final touched production files, the final touched test files, the paths to every Phase 0 and Phase 2 artifact, and an `Acceptance Criteria Coverage:` section mapping each checkbox in `issue.md` to the implementing code or test evidence.

## Test Plan

- Confirm the selected UI entry point displays the current confirmed-junk and potential-junk folder selections drawn from the existing settings state.
- Confirm Save updates `OlJunkCertain` and `JunkPotential` independently, uses the current relative-path storage model, and refreshes cached `AppOlObjects` folder references before the next junk-routing operation.
- Confirm Cancel leaves the stored settings and active folder selections unchanged.
- Confirm an unresolved saved folder presents a clear re-selection path and does not overwrite the stored value until the user picks a valid folder.
- Confirm the final MSTest coverage run records numeric overall coverage and numeric per-file coverage for the touched production files.
