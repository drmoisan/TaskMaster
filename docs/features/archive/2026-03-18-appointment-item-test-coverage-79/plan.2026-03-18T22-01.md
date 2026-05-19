# 2026-03-18-appointment-item-test-coverage-79 - Plan

- **Issue:** #79
- **Parent (optional):** none
- **Owner:** drmoisan
- **Requirements Source:** `docs/features/active/2026-03-18-appointment-item-test-coverage-79/issue.md`
- **Last Updated:** 2026-03-19T00-15
- **Status:** Draft
- **Version:** 1.1
- **Work Mode:** `minor-audit`

## Required References

- [`.github/copilot-instructions.md`](../../../../.github/copilot-instructions.md)
- [`.github/instructions/general-code-change.instructions.md`](../../../../.github/instructions/general-code-change.instructions.md)
- [`.github/instructions/general-unit-test.instructions.md`](../../../../.github/instructions/general-unit-test.instructions.md)
- [`.github/instructions/csharp-code-change.instructions.md`](../../../../.github/instructions/csharp-code-change.instructions.md)
- [`.github/instructions/csharp-unit-test.instructions.md`](../../../../.github/instructions/csharp-unit-test.instructions.md)

**This plan uses `issue.md` as the sole requirements source for the approved `minor-audit` path and does not require `spec.md`, `user-story.md`, or `research.md`.**

## Overview

Increase deterministic MSTest coverage for `UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs` to at least 80% by extending the existing appointment-item unit tests under `UtilitiesCS.Test/OutlookObjects/AppointmentItem`.
The small-path implementation is expected to stay within the existing `MeetingItemHelperTests.cs` file, while preserving the existing `UtilitiesCS.Test.csproj` compile registration and validating the full C# QA loop with coverage evidence.

---

## Affected Files

| File | Planned Work |
|---|---|
| `UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs` | Coverage target; production edits are out of scope unless a minimal testability seam becomes strictly necessary during execution |
| `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` | Expand deterministic MSTest scenarios to cover uncovered appointment-item behaviors |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Verify the existing `MeetingItemHelperTests.cs` compile include remains present |

---

## Requirements Traceability

| Requirement Source | Coverage in Plan |
|---|---|
| `docs/features/active/2026-03-18-appointment-item-test-coverage-79/issue.md` | Entire plan |

---

### Phase 0 — Context & Inputs (Policy Reads + Baseline Capture)

- [x] [P0-T1] Read the five mandatory policy files in the required order and record the Phase 0 policy-read artifact.
  - **Acceptance:** Save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/phase0-instructions-read-2026-03-18T22-01.md` with these fields populated from the completed read:
    - `Timestamp:`
    - `Policy Order: .github/copilot-instructions.md -> .github/instructions/general-code-change.instructions.md -> .github/instructions/general-unit-test.instructions.md -> .github/instructions/csharp-code-change.instructions.md -> .github/instructions/csharp-unit-test.instructions.md`
    - `Files Read:`
    - `Output Summary: 5/5 required policy files read; no conflicts detected`

- [x] [P0-T2] Read `issue.md` and this approved plan file to confirm `issue.md` remains the sole requirements source and `plan.2026-03-18T22-01.md` remains the execution plan of record for Issue #79.
  - **Acceptance:** Save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/phase0-requirements-and-plan-read-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Sources: docs/features/active/2026-03-18-appointment-item-test-coverage-79/issue.md; docs/features/active/2026-03-18-appointment-item-test-coverage-79/plan.2026-03-18T22-01.md`
    - `Output Summary: issue.md confirmed as sole requirements source; approved plan file confirmed as execution plan of record for Issue #79`

- [x] [P0-T3] Verify the active feature folder remains minor-audit only by confirming `issue.md` and the approved plan file are present and that `spec.md`, `user-story.md`, and `research.md` are absent.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$feature = 'docs/features/active/2026-03-18-appointment-item-test-coverage-79'; $files = Get-ChildItem $feature -File | Select-Object -ExpandProperty Name | Sort-Object; $blocked = @('spec.md','user-story.md','research.md') | Where-Object { Test-Path (Join-Path $feature $_) }; Write-Output ('Files=' + ($files -join ',')); Write-Output ('BlockedFiles=' + ($(if ($blocked.Count -gt 0) { $blocked -join ',' } else { 'none' }))) ; if ($blocked.Count -gt 0) { exit 1 }"`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/phase0-feature-folder-check-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$feature = 'docs/features/active/2026-03-18-appointment-item-test-coverage-79'; $files = Get-ChildItem $feature -File | Select-Object -ExpandProperty Name | Sort-Object; $blocked = @('spec.md','user-story.md','research.md') | Where-Object { Test-Path (Join-Path $feature $_) }; Write-Output ('Files=' + ($files -join ',')); Write-Output ('BlockedFiles=' + ($(if ($blocked.Count -gt 0) { $blocked -join ',' } else { 'none' }))) ; if ($blocked.Count -gt 0) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary: Files=issue.md,plan.2026-03-18T22-01.md; BlockedFiles=none`

- [x] [P0-T4] Run the repository restore command before any implementation work to confirm the solution is in a restorable baseline state.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU'`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-restore-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU'`
    - `EXIT_CODE: 0`
    - `Output Summary: Restore succeeded for TaskMaster.sln`

- [x] [P0-T5] Run the C# formatter on the appointment-item source and test files to capture the baseline formatting state.
  - **Acceptance:** Run
    `dotnet tool run csharpier UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-format-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: dotnet tool run csharpier UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs`
    - `EXIT_CODE: 0`
    - `Output Summary: CSharpier completed for MeetingItemHelper source and tests`

- [x] [P0-T6] Run the analyzer-enabled MSBuild command to capture the baseline C# lint state.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-build-analyzer-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Analyzer build succeeded with 0 errors`

- [x] [P0-T7] Run the nullable-enforced MSBuild command to capture the baseline C# type-check state.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-build-nullable-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Nullable build succeeded with 0 errors`

- [x] [P0-T8] Run the coverage-enabled MSTest command to capture baseline test and repository coverage results.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-test-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary: <passed-count> passed, <failed-count> failed, Repo Line Coverage: <numeric-percent>%`

- [x] [P0-T9] Extract baseline line coverage for `UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs` from `coverage/coverage.cobertura.xml`.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "[xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq 'UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs' } | Select-Object -First 1; if (-not $class) { exit 1 }; $fileCoverage = [math]::Round([double]$class.'line-rate' * 100, 2); Write-Output \"MeetingItemHelper Line Coverage: $fileCoverage%\""`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-appointment-item-coverage-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "[xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq 'UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs' } | Select-Object -First 1; if (-not $class) { exit 1 }; $fileCoverage = [math]::Round([double]$class.'line-rate' * 100, 2); Write-Output \"MeetingItemHelper Line Coverage: $fileCoverage%\""`
    - `EXIT_CODE: 0`
    - `Output Summary: MeetingItemHelper Line Coverage: <numeric-percent>%`

---

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] Record the constrained small-path implementation handoff before modifying appointment-item tests.
  - **Acceptance:** Save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/other/minor-audit-implementation-handoff-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Handoff Target: constrained-small-path-implementation`
    - `Requirements Source: docs/features/active/2026-03-18-appointment-item-test-coverage-79/issue.md`
    - `Output Summary: Small-path implementation handoff recorded for MeetingItemHelper coverage work`

- [x] [P1-T2] Add the deterministic MSTest scenario `CompressPlainText_WithNullInput_ReturnsEndMarkerOnly`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` contains a test method named `CompressPlainText_WithNullInput_ReturnsEndMarkerOnly`.

- [x] [P1-T3] Add the deterministic MSTest scenario `CompressPlainText_WithShowStrippedLinks_ReplacesLinksWithPlaceholder`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` contains a test method named `CompressPlainText_WithShowStrippedLinks_ReplacesLinksWithPlaceholder`.

- [x] [P1-T4] Add the deterministic MSTest scenario `CompressPlainText_WithReplyHeaderPreserved_KeepsHeaderAndAppendsEndMarker`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` contains a test method named `CompressPlainText_WithReplyHeaderPreserved_KeepsHeaderAndAppendsEndMarker`.

- [x] [P1-T5] Add the deterministic MSTest scenario `ToggleDark_WhenAlreadyOn_DoesNotDuplicateDarkModeHeader`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` contains a test method named `ToggleDark_WhenAlreadyOn_DoesNotDuplicateDarkModeHeader`.

- [x] [P1-T6] Add the deterministic MSTest scenario `ToggleDark_WhenAlreadyOff_LeavesHtmlUnchanged`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` contains a test method named `ToggleDark_WhenAlreadyOff_LeavesHtmlUnchanged`.

- [x] [P1-T7] Add the deterministic MSTest scenario `SetSender_ShouldPopulateSenderCaches`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` contains a test method named `SetSender_ShouldPopulateSenderCaches`.

- [x] [P1-T8] Add the deterministic MSTest scenario `GetHtml_ShouldInjectEmailHeaderInsideBodyTag`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` contains a test method named `GetHtml_ShouldInjectEmailHeaderInsideBodyTag`.

- [x] [P1-T9] Add the deterministic MSTest scenario `LoadRecipients_ShouldPopulateToAndCcRecipientFields`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` contains a test method named `LoadRecipients_ShouldPopulateToAndCcRecipientFields`.

- [x] [P1-T10] Capture targeted verification evidence that the expanded appointment-item test scenarios are present and that the existing test file remains registered in `UtilitiesCS.Test.csproj`.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$path = 'UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs'; $content = Get-Content $path -Raw; $names = @('CompressPlainText_WithNullInput_ReturnsEndMarkerOnly','CompressPlainText_WithShowStrippedLinks_ReplacesLinksWithPlaceholder','CompressPlainText_WithReplyHeaderPreserved_KeepsHeaderAndAppendsEndMarker','ToggleDark_WhenAlreadyOn_DoesNotDuplicateDarkModeHeader','ToggleDark_WhenAlreadyOff_LeavesHtmlUnchanged','SetSender_ShouldPopulateSenderCaches','GetHtml_ShouldInjectEmailHeaderInsideBodyTag','LoadRecipients_ShouldPopulateToAndCcRecipientFields'); $missing = $names | Where-Object { $content -notmatch [regex]::Escape($_) }; $includeCount = (Select-String -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Pattern 'OutlookObjects\\AppointmentItem\\MeetingItemHelperTests.cs').Count; Write-Output ('MissingScenarioCount=' + $missing.Count); Write-Output ('MeetingItemHelperTestsCompileIncludeCount=' + $includeCount); if ($missing.Count -gt 0 -or $includeCount -ne 1) { exit 1 }"`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/regression-testing/targeted-verification-tests-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$path = 'UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs'; $content = Get-Content $path -Raw; $names = @('CompressPlainText_WithNullInput_ReturnsEndMarkerOnly','CompressPlainText_WithShowStrippedLinks_ReplacesLinksWithPlaceholder','CompressPlainText_WithReplyHeaderPreserved_KeepsHeaderAndAppendsEndMarker','ToggleDark_WhenAlreadyOn_DoesNotDuplicateDarkModeHeader','ToggleDark_WhenAlreadyOff_LeavesHtmlUnchanged','SetSender_ShouldPopulateSenderCaches','GetHtml_ShouldInjectEmailHeaderInsideBodyTag','LoadRecipients_ShouldPopulateToAndCcRecipientFields'); $missing = $names | Where-Object { $content -notmatch [regex]::Escape($_) }; $includeCount = (Select-String -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Pattern 'OutlookObjects\\AppointmentItem\\MeetingItemHelperTests.cs').Count; Write-Output ('MissingScenarioCount=' + $missing.Count); Write-Output ('MeetingItemHelperTestsCompileIncludeCount=' + $includeCount); if ($missing.Count -gt 0 -or $includeCount -ne 1) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary: MissingScenarioCount=0; MeetingItemHelperTestsCompileIncludeCount=1`

- [x] [P1-T11] Record constrained small-path implementation completion after the targeted verification artifact is present.
  - **Acceptance:** Save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/other/minor-audit-implementation-complete-2026-03-18T22-01.md` with these exact lines:
    - `Target Test File: UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs`
    - `Targeted Verification Artifact: docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/regression-testing/targeted-verification-tests-2026-03-18T22-01.md`
    - `Scope Status: constrained-small-path-complete`

---

### Phase 2 — Final QC Loop

Run the full C# toolchain loop in order. If any command exits non-zero or any step changes files, fix the reported issue and restart from `P2-T1`.

- [x] [P2-T1] Run the C# formatter on the appointment-item source and test files after implementation changes.
  - **Acceptance:** Run
    `dotnet tool run csharpier UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-format-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: dotnet tool run csharpier UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs`
    - `EXIT_CODE: 0`
    - `Output Summary: CSharpier completed for MeetingItemHelper source and tests`

- [x] [P2-T2] Run the analyzer-enabled MSBuild command.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-build-analyzer-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Analyzer build succeeded with 0 errors`

- [x] [P2-T3] Run the nullable-enforced MSBuild command.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-build-nullable-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Nullable build succeeded with 0 errors`

- [x] [P2-T4] Run the coverage-enabled MSTest command and record the post-change repository coverage headline.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-test-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary: <passed-count> passed, 0 failed, Repo Line Coverage: <numeric-percent>%`

- [x] [P2-T5] Calculate the post-change appointment-item coverage delta and fail the gate if repository coverage drops below 80%, if `MeetingItemHelper.cs` remains below 80%, if file coverage regresses from baseline, or if changed production lines fall below 90% coverage when any production lines changed.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$baselineRepo = [double](([regex]::Match((Get-Content 'docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-test-2026-03-18T22-01.md' -Raw), 'Repo Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); $baselineFile = [double](([regex]::Match((Get-Content 'docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-appointment-item-coverage-2026-03-18T22-01.md' -Raw), 'MeetingItemHelper Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; $repo = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2); $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq 'UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs' } | Select-Object -First 1; if (-not $class) { exit 1 }; $file = [math]::Round([double]$class.'line-rate' * 100, 2); $diff = git diff --unified=0 -- UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs; $ranges = New-Object System.Collections.ArrayList; foreach ($line in $diff) { if ($line -match '^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@') { $newStart = [int]$Matches[3]; $newCount = if ($Matches[4]) { [int]$Matches[4] } else { 1 }; if ($newCount -gt 0) { [void]$ranges.Add([pscustomobject]@{ Start = $newStart; End = ($newStart + $newCount - 1) }) } } }; $hitsByLine = @{}; foreach ($lineNode in $class.SelectNodes('./lines/line')) { $hitsByLine[[int]$lineNode.number] = [int]$lineNode.hits }; $changedCovered = 0; $changedTotal = 0; foreach ($range in $ranges) { for ($i = $range.Start; $i -le $range.End; $i++) { $changedTotal++; if ($hitsByLine.ContainsKey($i) -and $hitsByLine[$i] -gt 0) { $changedCovered++ } } }; $changedCoverage = if ($changedTotal -eq 0) { 100 } else { [math]::Round(($changedCovered / $changedTotal) * 100, 2) }; $repoThresholdMet = $repo -ge 80; $fileThresholdMet = $file -ge 80; $fileNoRegression = $file -ge $baselineFile; $changedThresholdMet = $changedCoverage -ge 90; Write-Output \"Baseline Repo Line Coverage: $baselineRepo%\"; Write-Output \"Final Repo Line Coverage: $repo%\"; Write-Output \"Baseline MeetingItemHelper Line Coverage: $baselineFile%\"; Write-Output \"Final MeetingItemHelper Line Coverage: $file%\"; Write-Output \"Changed Production Lines Covered: $changedCovered/$changedTotal\"; Write-Output \"Changed Production Coverage: $changedCoverage%\"; Write-Output \"Repo Threshold Met: $repoThresholdMet\"; Write-Output \"MeetingItemHelper Threshold Met: $fileThresholdMet\"; Write-Output \"MeetingItemHelper No Regression: $fileNoRegression\"; Write-Output \"Changed Production Coverage >= 90%: $changedThresholdMet\"; if (-not $repoThresholdMet -or -not $fileThresholdMet -or -not $fileNoRegression -or ($changedTotal -gt 0 -and -not $changedThresholdMet)) { exit 1 }"`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-coverage-delta-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$baselineRepo = [double](([regex]::Match((Get-Content 'docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-test-2026-03-18T22-01.md' -Raw), 'Repo Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); $baselineFile = [double](([regex]::Match((Get-Content 'docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-appointment-item-coverage-2026-03-18T22-01.md' -Raw), 'MeetingItemHelper Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; $repo = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2); $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq 'UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs' } | Select-Object -First 1; if (-not $class) { exit 1 }; $file = [math]::Round([double]$class.'line-rate' * 100, 2); $diff = git diff --unified=0 -- UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs; $ranges = New-Object System.Collections.ArrayList; foreach ($line in $diff) { if ($line -match '^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@') { $newStart = [int]$Matches[3]; $newCount = if ($Matches[4]) { [int]$Matches[4] } else { 1 }; if ($newCount -gt 0) { [void]$ranges.Add([pscustomobject]@{ Start = $newStart; End = ($newStart + $newCount - 1) }) } } }; $hitsByLine = @{}; foreach ($lineNode in $class.SelectNodes('./lines/line')) { $hitsByLine[[int]$lineNode.number] = [int]$lineNode.hits }; $changedCovered = 0; $changedTotal = 0; foreach ($range in $ranges) { for ($i = $range.Start; $i -le $range.End; $i++) { $changedTotal++; if ($hitsByLine.ContainsKey($i) -and $hitsByLine[$i] -gt 0) { $changedCovered++ } } }; $changedCoverage = if ($changedTotal -eq 0) { 100 } else { [math]::Round(($changedCovered / $changedTotal) * 100, 2) }; $repoThresholdMet = $repo -ge 80; $fileThresholdMet = $file -ge 80; $fileNoRegression = $file -ge $baselineFile; $changedThresholdMet = $changedCoverage -ge 90; Write-Output \"Baseline Repo Line Coverage: $baselineRepo%\"; Write-Output \"Final Repo Line Coverage: $repo%\"; Write-Output \"Baseline MeetingItemHelper Line Coverage: $baselineFile%\"; Write-Output \"Final MeetingItemHelper Line Coverage: $file%\"; Write-Output \"Changed Production Lines Covered: $changedCovered/$changedTotal\"; Write-Output \"Changed Production Coverage: $changedCoverage%\"; Write-Output \"Repo Threshold Met: $repoThresholdMet\"; Write-Output \"MeetingItemHelper Threshold Met: $fileThresholdMet\"; Write-Output \"MeetingItemHelper No Regression: $fileNoRegression\"; Write-Output \"Changed Production Coverage >= 90%: $changedThresholdMet\"; if (-not $repoThresholdMet -or -not $fileThresholdMet -or -not $fileNoRegression -or ($changedTotal -gt 0 -and -not $changedThresholdMet)) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary: Baseline Repo Line Coverage: <numeric-percent>%; Final Repo Line Coverage: <numeric-percent>%; Baseline MeetingItemHelper Line Coverage: <numeric-percent>%; Final MeetingItemHelper Line Coverage: <numeric-percent>%; Changed Production Lines Covered: <covered>/<total>; Changed Production Coverage: <numeric-percent>%; Repo Threshold Met: True; MeetingItemHelper Threshold Met: True; MeetingItemHelper No Regression: True; Changed Production Coverage >= 90%: True`

- [x] [P2-T6] Confirm the five QC gate artifacts exist and each records `EXIT_CODE: 0`.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$files = @('docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-format-2026-03-18T22-01.md','docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-build-analyzer-2026-03-18T22-01.md','docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-build-nullable-2026-03-18T22-01.md','docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-test-2026-03-18T22-01.md','docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-coverage-delta-2026-03-18T22-01.md'); $count = ($files | Where-Object { Test-Path $_ }).Count; $matches = (Select-String -Path $files -Pattern '^EXIT_CODE: 0$').Count; Write-Output ('QaArtifactCount=' + $count); Write-Output ('ExitCodeZeroCount=' + $matches); if ($count -ne 5 -or $matches -ne 5) { exit 1 }"`
    and save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-artifact-check-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$files = @('docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-format-2026-03-18T22-01.md','docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-build-analyzer-2026-03-18T22-01.md','docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-build-nullable-2026-03-18T22-01.md','docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-test-2026-03-18T22-01.md','docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-coverage-delta-2026-03-18T22-01.md'); $count = ($files | Where-Object { Test-Path $_ }).Count; $matches = (Select-String -Path $files -Pattern '^EXIT_CODE: 0$').Count; Write-Output ('QaArtifactCount=' + $count); Write-Output ('ExitCodeZeroCount=' + $matches); if ($count -ne 5 -or $matches -ne 5) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary: QaArtifactCount=5; ExitCodeZeroCount=5`

- [x] [P2-T7] Record the reduced-audit handoff after the QC artifact set is complete.
  - **Acceptance:** Save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/other/minor-audit-reduced-audit-handoff-2026-03-18T22-01.md` with:
    - `Timestamp:`
    - `Handoff Target: reduced-audit-review`
    - `QA Artifact Set: docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-format-2026-03-18T22-01.md; docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-build-analyzer-2026-03-18T22-01.md; docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-build-nullable-2026-03-18T22-01.md; docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-test-2026-03-18T22-01.md; docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-coverage-delta-2026-03-18T22-01.md`
    - `Output Summary: Reduced-audit handoff recorded`

- [x] [P2-T8] Record the end-state minor-audit evidence once the QC coverage thresholds and artifact checks have passed.
  - **Acceptance:** Save `docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/other/minor-audit-end-state-2026-03-18T22-01.md` with these exact lines:
    - `Baseline Coverage Artifact: docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-test-2026-03-18T22-01.md`
    - `Baseline AppointmentItem Coverage Artifact: docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/baseline/baseline-appointment-item-coverage-2026-03-18T22-01.md`
    - `Final Coverage Artifact: docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-test-2026-03-18T22-01.md`
    - `Coverage Delta Artifact: docs/features/active/2026-03-18-appointment-item-test-coverage-79/evidence/qa-gates/final-qa-coverage-delta-2026-03-18T22-01.md`
    - `Reduced Audit Status: READY`

---

## Plan Self-Check

| Gate | Status |
|---|---|
| Canonical phase headings (`### Phase N — <Title>`) | ✅ |
| Task IDs (`[P#-T#]`) sequential per phase | ✅ |
| Exactly 3 phases for `minor-audit` | ✅ |
| No Python policy references | ✅ |
| Zero forbidden placeholder tokens from the template | ✅ |
| Machine-verifiable acceptance criteria | ✅ |
| Phase 0 includes policy-read evidence plus baseline restore, format, analyzer, nullable, and coverage artifacts | ✅ |
| Phase 1 includes explicit small-path handoff plus targeted verification evidence tasks | ✅ |
| Phase 2 includes unconditional C# QA loop tasks plus reduced-audit handoff and end-state evidence | ✅ |
| Baseline and final coverage artifacts require numeric coverage values | ✅ |

---

## Preflight Validation

**DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED**

**DIRECTIVE: PREFLIGHT VALIDATION ONLY**

Validated against the `minor-audit` executor contract:

- `issue.md` is the sole requirements source.
- The plan updates the approved file path in place and does not introduce sibling plan files.
- The plan contains exactly three phases: baseline capture, constrained small-path implementation, and final QC loop.
- Required references name the general and C# code-change and unit-test instruction files only.
- Phase 0 includes baseline evidence tasks.
- Phase 1 includes targeted verification evidence tasks.
- Phase 2 includes end-state evidence tasks and unconditional final-QC command tasks.

**PREFLIGHT: ALL CLEAR**
