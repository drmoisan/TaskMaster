# 2026-03-19-outlook-folder-wrapper-tests-82 - Plan

- **Issue:** #82
- **Parent (optional):** none
- **Owner:** drmoisan
- **Requirements Source:** `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/issue.md`, `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/user-story.md`, `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/spec.md`, `artifacts/research/20260319-outlook-folder-wrapper-tests-82-research.md`, `change-plan.md`
- **Last Updated:** 2026-03-19
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** `full-feature`

## Required References

- [`.github/copilot-instructions.md`](../../../../.github/copilot-instructions.md)
- [`.github/instructions/general-code-change.instructions.md`](../../../../.github/instructions/general-code-change.instructions.md)
- [`.github/instructions/general-unit-test.instructions.md`](../../../../.github/instructions/general-unit-test.instructions.md)
- [`.github/instructions/csharp-code-change.instructions.md`](../../../../.github/instructions/csharp-code-change.instructions.md)
- [`.github/instructions/csharp-unit-test.instructions.md`](../../../../.github/instructions/csharp-unit-test.instructions.md)

**This plan uses the full-feature document set and remains constrained to deterministic MSTest coverage work for `UtilitiesCS/OutlookObjects/Folder` and `UtilitiesCS.Test/OutlookObjects/Folder`.**

## Overview

Raise every compiled production file under `UtilitiesCS/OutlookObjects/Folder` to at least `80%` line coverage by extending the existing MSTest suite in `UtilitiesCS.Test/OutlookObjects/Folder`, adding one new `MAPIMethods` test file if needed, and introducing only the smallest non-public seam(s) if static UI or filesystem calls block deterministic coverage.
The implementation sequence is intentionally risk-tiered: lock in the near-threshold and already-mockable files first, push the pure/mock-heavy scorer and predictor branches next, then use a conditional seam phase only if the tests-only pass still leaves `FolderPredictor.cs` or `FolderConverter.cs` below the required threshold.

---

## Coverage Targets

Baseline percentages come from `coverage/coverage.cobertura.xml` as recorded in `artifacts/research/20260319-outlook-folder-wrapper-tests-82-research.md`.

| Production File | Baseline Line Coverage | Target | Primary Test Home | Risk Tier |
|---|---:|---:|---|---|
| `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | 42.32% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` | High |
| `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs` | 72.00% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs` | Medium |
| `UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs` | 100.00% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderNavigatorTests.cs` | Verify-only |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 15.11% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | High |
| `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` | 17.21% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs` | High |
| `UtilitiesCS/OutlookObjects/Folder/FolderTree.cs` | 29.85% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeTests.cs` | High |
| `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs` | 70.60% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperStateTests.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTraversalTests.cs` | Medium |
| `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs` | 79.55% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparerTests.cs` | Low |
| `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs` | 100.00% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameComparerTests.cs` | Verify-only |
| `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs` | 100.00% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameCountSizeComparerTests.cs` | Verify-only |
| `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs` | 82.42% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNodeComparerTests.cs` | Verify-only |
| `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs` | 92.86% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNodeContentsComparerTests.cs` | Verify-only |
| `UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs` | 0.00% | >= 80% | `UtilitiesCS.Test/OutlookObjects/Folder/MAPIMethodsTests.cs` | Low |

---

## Planned Files To Change

| File | Planned Work |
|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparerTests.cs` | Add deterministic null and parent-name edge-case scenarios to push the comparer above threshold |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs` | Add UNC/root traversal and restore failure scenarios |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperStateTests.cs` | Add wrapper-state and folder-size fallback scenarios |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTraversalTests.cs` | Add successful compare/load and traversal scenarios |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeTests.cs` | Add selection, progress-aware, detangling, and compare overload scenarios |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs` | Add query-builder, object-array, classifier-input, and suggestion scenarios |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | Add search, recents, suggestions, refresh, folder-lookup, and failure-path scenarios |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` | Add argument-guard, `MAPIFolder` overload, and alternative-resolution scenarios |
| `UtilitiesCS.Test/OutlookObjects/Folder/MAPIMethodsTests.cs` | New deterministic reflection/constants coverage file for compiled nested interop declarations |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Register `MAPIMethodsTests.cs` if the file is added |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | Conditional non-public seam only if Phase 3 coverage evidence proves tests-only execution cannot reach 80% |
| `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | Conditional non-public seam only if Phase 4 coverage evidence proves tests-only execution cannot reach 80% |

---

## Minimal Seam Reservation

The default path is tests only. If a seam becomes necessary, keep it non-public and behavior-preserving:

- `FolderPredictor.cs`: isolate static message-box, directory-creation, or UI-thread calls behind one non-public wrapper object or delegate set that preserves the current default production path.
- `FolderConverter.cs`: isolate `MyBox.ShowDialog`, `InputBox.ShowDialog`, or other static prompt calls behind one non-public wrapper object or delegate set that preserves the current default production path.

No seam may widen the public API, add new configuration, or alter the observable default behavior.

---

## Requirements Traceability

| Requirement Source | Coverage in Plan |
|---|---|
| `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/issue.md` | Entire plan; target scope, 80% per-file threshold, and long-path requirement |
| `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/user-story.md` | Phase 1 through Phase 5 scenario coverage and seam constraints |
| `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/spec.md` | File list, validation loop, seam guardrails, and evidence requirements |
| `artifacts/research/20260319-outlook-folder-wrapper-tests-82-research.md` | Baseline percentages, file-risk grouping, test-home mapping, and seam trigger criteria |

---

### Phase 0 — Context & Inputs (Policy Reads + Baseline Capture)

- [x] [P0-T1] Read the five mandatory policy files in the required order and record the policy-read artifact for this feature.
  - **Acceptance:** Save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `phase0-instructions-read-` and whose contents include:
    - `Timestamp:`
    - `Policy Order: .github/copilot-instructions.md -> .github/instructions/general-code-change.instructions.md -> .github/instructions/general-unit-test.instructions.md -> .github/instructions/csharp-code-change.instructions.md -> .github/instructions/csharp-unit-test.instructions.md`
    - `Files Read:`
    - `Output Summary: 5/5 required policy files read; no conflicts detected`

- [x] [P0-T2] Read `issue.md`, `user-story.md`, `spec.md`, `change-plan.md`, the research artifact, and this approved plan file, then record the full-feature requirements-read artifact.
  - **Acceptance:** Save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `phase0-requirements-and-plan-read-` and whose contents include:
    - `Timestamp:`
    - `Sources: docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/issue.md; docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/user-story.md; docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/spec.md; change-plan.md; artifacts/research/20260319-outlook-folder-wrapper-tests-82-research.md; docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/plan.2026-03-19T09-43.md`
    - `Output Summary: full-feature requirements set, existing change plan reviewed, and plan-of-record confirmed`

- [x] [P0-T3] Verify that the feature folder remains on the `full-feature` path by confirming `issue.md`, `user-story.md`, `spec.md`, and the approved plan file are present.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$feature = 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82'; $required = @('issue.md','user-story.md','spec.md','plan.2026-03-19T09-43.md'); $missing = $required | Where-Object { -not (Test-Path (Join-Path $feature $_)) }; Write-Output ('MissingRequired=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `phase0-feature-folder-check-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$feature = 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82'; $required = @('issue.md','user-story.md','spec.md','plan.2026-03-19T09-43.md'); $missing = $required | Where-Object { -not (Test-Path (Join-Path $feature $_)) }; Write-Output ('MissingRequired=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary: MissingRequired=none`

- [x] [P0-T4] Verify the compiled production scope and the current folder-test compile includes before adding any new test file.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$prod = Select-String -Path 'UtilitiesCS/UtilitiesCS.csproj' -Pattern 'OutlookObjects\\Folder\\|OutlookObjects\\Folder\\MsgToMime\\MAPIMethods.cs' | Select-Object -ExpandProperty Line; $tests = Select-String -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Pattern 'OutlookObjects\\Folder\\' | Select-Object -ExpandProperty Line; Write-Output ('ProdCount=' + $prod.Count); Write-Output ('TestIncludeCount=' + $tests.Count); Write-Output ('HasMapiMethodsTestsInclude=' + [bool]($tests -match 'MAPIMethodsTests.cs')); if ($prod.Count -ne 13) { exit 1 }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `phase0-scope-verification-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$prod = Select-String -Path 'UtilitiesCS/UtilitiesCS.csproj' -Pattern 'OutlookObjects\\Folder\\|OutlookObjects\\Folder\\MsgToMime\\MAPIMethods.cs' | Select-Object -ExpandProperty Line; $tests = Select-String -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Pattern 'OutlookObjects\\Folder\\' | Select-Object -ExpandProperty Line; Write-Output ('ProdCount=' + $prod.Count); Write-Output ('TestIncludeCount=' + $tests.Count); Write-Output ('HasMapiMethodsTestsInclude=' + [bool]($tests -match 'MAPIMethodsTests.cs')); if ($prod.Count -ne 13) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary: ProdCount=13; TestIncludeCount=` followed by the actual numeric count, then `; HasMapiMethodsTestsInclude=` followed by the actual boolean value

- [x] [P0-T5] Run the repository restore command before any implementation work.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU'`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `baseline-restore-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU'`
    - `EXIT_CODE: 0`
    - `Output Summary: Restore succeeded for TaskMaster.sln`

- [x] [P0-T6] Run the baseline C# formatter command using the repo-approved command.
  - **Acceptance:** Run
    `dotnet tool run csharpier .`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `baseline-format-` and whose contents include:
    - `Timestamp:`
    - `Command: dotnet tool run csharpier .`
    - `EXIT_CODE: 0`
    - `Output Summary: CSharpier completed for repo root`

- [x] [P0-T7] Run the analyzer-enabled MSBuild command to capture the baseline C# lint state.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `baseline-build-analyzer-` and whose contents include:
    - `Timestamp:`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Analyzer build succeeded with 0 errors`

- [x] [P0-T8] Run the nullable-enforced MSBuild command to capture the baseline C# type-check state.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `baseline-build-nullable-` and whose contents include:
    - `Timestamp:`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Nullable build succeeded with 0 errors`

- [x] [P0-T9] Run the coverage-enabled MSTest command to capture the baseline test and repository coverage headline.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `baseline-test-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary:` containing exactly these three lines with actual numeric values:
      - `Passed=<count>`
      - `Failed=<count>`
      - `Repo Line Coverage: <percent>%`

- [x] [P0-T10] Extract baseline per-file line coverage for all 13 compiled folder targets from `coverage/coverage.cobertura.xml`.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$targets = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs','UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs','UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs','UtilitiesCS/OutlookObjects/Folder/FolderTree.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs','UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs'); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; foreach ($target in $targets) { $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $target } | Select-Object -First 1; if (-not $class) { Write-Output ('Missing=' + $target); exit 1 }; $rate = [math]::Round([double]$class.'line-rate' * 100, 2); Write-Output ($target + '=' + $rate + '%') }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/` whose filename starts with `baseline-folder-coverage-matrix-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$targets = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs','UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs','UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs','UtilitiesCS/OutlookObjects/Folder/FolderTree.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs','UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs'); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; foreach ($target in $targets) { $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $target } | Select-Object -First 1; if (-not $class) { Write-Output ('Missing=' + $target); exit 1 }; $rate = [math]::Round([double]$class.'line-rate' * 100, 2); Write-Output ($target + '=' + $rate + '%') }"`
    - `EXIT_CODE: 0`
    - `Output Summary:` followed by 13 lines, one for each exact target file, where each line uses the exact file path, an equals sign, and that file's numeric line-coverage percentage

---

### Phase 1 — Near-Threshold and Existing-Seam Coverage Uplift

- [x] [P1-T1] Add comparer edge-case tests to `FolderWrapperNameAndParentNameComparerTests.cs` for null wrappers and parent-name comparisons.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparerTests.cs` contains MSTest methods whose names include `Null` and `ParentName`, and both names appear in a new regression-testing artifact for this phase.

- [x] [P1-T2] Add restore and traversal tests to `FolderMinimalWrapperTests.cs` for UNC-root walking and restore failure branches.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs` contains MSTest methods whose names include `Unc` and `Restore`, and both names appear in a new regression-testing artifact for this phase.

- [x] [P1-T3] Add folder-size fallback and serialized-state coverage to `FolderWrapperStateTests.cs`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperStateTests.cs` contains MSTest methods whose names include `FolderSize` and `State`, and both names appear in a new regression-testing artifact for this phase.

- [x] [P1-T4] Add successful compare-item and load-item coverage to `FolderWrapperTraversalTests.cs`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTraversalTests.cs` contains MSTest methods whose names include both `Compare` and `Load`, and the added names appear in a new regression-testing artifact for this phase.

- [x] [P1-T5] Add selection-constructor and progress-aware coverage to `FolderTreeTests.cs`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeTests.cs` contains MSTest methods whose names include both `Selection` and `Progress`, and the added names appear in a new regression-testing artifact for this phase.

- [x] [P1-T6] Capture a Phase 1 targeted verification artifact that proves the new test methods from `P1-T1` through `P1-T5` are present in the expected test files.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$checks = @(@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparerTests.cs','Null','ParentName'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs','Unc','Restore'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperStateTests.cs','FolderSize','State'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTraversalTests.cs','Compare','Load'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeTests.cs','Selection','Progress')); foreach ($check in $checks) { $path = $check[0]; $content = Get-Content $path -Raw; $tokens = $check[1..($check.Length - 1)]; $missing = $tokens | Where-Object { $content -notmatch [regex]::Escape($_) }; Write-Output ($path + ':Missing=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 } }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/regression-testing/` whose filename starts with `phase1-targeted-verification-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$checks = @(@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparerTests.cs','Null','ParentName'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs','Unc','Restore'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperStateTests.cs','FolderSize','State'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTraversalTests.cs','Compare','Load'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeTests.cs','Selection','Progress')); foreach ($check in $checks) { $path = $check[0]; $content = Get-Content $path -Raw; $tokens = $check[1..($check.Length - 1)]; $missing = $tokens | Where-Object { $content -notmatch [regex]::Escape($_) }; Write-Output ($path + ':Missing=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 } }"`
    - `EXIT_CODE: 0`
    - `Output Summary:` containing five lines, one per checked test file, each ending with `Missing=none`

- [x] [P1-T7] Run a coverage-enabled MSTest checkpoint and record whether the Phase 1 production files meet or exceed 80% after the low- and medium-risk test additions.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/regression-testing/` whose filename starts with `phase1-coverage-checkpoint-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary:` with numeric lines for `FolderWrapperNameAndParentNameComparer.cs`, `FolderMinimalWrapper.cs`, `FolderWrapper .cs`, and `FolderTree.cs`, each using the exact filename, an equals sign, and the numeric percentage

---

### Phase 2 — Scorer and Predictor Tests-Only Expansion

- [x] [P2-T1] Add deterministic `FolderScorer` coverage for query-builder and object-array branches in `FolderScorerTests.cs`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs` contains MSTest method names including both `Query` and `Array`, and those names appear in a new Phase 2 regression-testing artifact.

- [x] [P2-T2] Add deterministic `FolderPredictor` coverage for the `FolderArray` branch in `FolderPredictorTests.cs`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` contains MSTest method names including `FolderArray`, and that name appears in a new Phase 2 regression-testing artifact.

- [x] [P2-T3] Add deterministic `FolderPredictor` coverage for recents and suggestions branches in `FolderPredictorTests.cs`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` contains MSTest method names including both `Recents` and `Suggestions`, and those names appear in a new Phase 2 regression-testing artifact.

- [x] [P2-T4] Capture a Phase 2 targeted verification artifact that proves the new scorer and predictor tests are present in `FolderScorerTests.cs` and `FolderPredictorTests.cs`.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$checks = @(@('UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs','Query','Array'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs','FolderArray','Recents','Suggestions')); foreach ($check in $checks) { $path = $check[0]; $content = Get-Content $path -Raw; $tokens = $check[1..($check.Length - 1)]; $missing = $tokens | Where-Object { $content -notmatch [regex]::Escape($_) }; Write-Output ($path + ':Missing=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 } }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/regression-testing/` whose filename starts with `phase2-targeted-verification-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$checks = @(@('UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs','Query','Array'),@('UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs','FolderArray','Recents','Suggestions')); foreach ($check in $checks) { $path = $check[0]; $content = Get-Content $path -Raw; $tokens = $check[1..($check.Length - 1)]; $missing = $tokens | Where-Object { $content -notmatch [regex]::Escape($_) }; Write-Output ($path + ':Missing=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 } }"`
    - `EXIT_CODE: 0`
    - `Output Summary:` containing one line for `FolderScorerTests.cs` and one for `FolderPredictorTests.cs`, each ending with `Missing=none`

- [x] [P2-T5] Run a coverage-enabled MSTest checkpoint and record whether `FolderScorer.cs` and `FolderPredictor.cs` reach 80% without any production seam.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/regression-testing/` whose filename starts with `phase2-coverage-checkpoint-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary:` with numeric lines for `FolderScorer.cs` and `FolderPredictor.cs`, each using the exact filename, an equals sign, and the numeric percentage

---

### Phase 3 — Converter and MAPIMethods Coverage Expansion

- [x] [P3-T1] Add deterministic `FolderConverter` tests for argument guards and `MAPIFolder` overload paths in `FolderConverterTests.cs`.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` contains MSTest method names including `Argument` and `MAPIFolder`, and those names appear in a new Phase 3 regression-testing artifact.

- [x] [P3-T2] Add deterministic `FolderConverter` tests for alternative-folder or path-resolution branches that are reachable without a new production seam.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` contains MSTest method names including `Alternative`, `Resolve`, or `Path`, and those names appear in a new Phase 3 regression-testing artifact.

- [x] [P3-T3] Create `UtilitiesCS.Test/OutlookObjects/Folder/MAPIMethodsTests.cs` with deterministic reflection or constant assertions that execute the compiled `MAPIMethods.cs` type initializer.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/MAPIMethodsTests.cs` exists and contains MSTest method names including `Guid` and either `Interface` or `Enum`.

- [x] [P3-T4] Register `MAPIMethodsTests.cs` in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
  - **Acceptance:** `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains the exact line `<Compile Include="OutlookObjects\Folder\MAPIMethodsTests.cs" />` exactly once.

- [x] [P3-T5] Capture a Phase 3 targeted verification artifact that proves the new converter and `MAPIMethods` tests are present and compile-registered.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$converter = Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs' -Raw; $mapi = Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/MAPIMethodsTests.cs' -Raw; $includeCount = (Select-String -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Pattern 'OutlookObjects\\Folder\\MAPIMethodsTests.cs').Count; $checks = @(('ConverterArgument=' + [bool]($converter -match 'Argument')),('ConverterMapiFolder=' + [bool]($converter -match 'MAPIFolder')),('ConverterAlternative=' + [bool]($converter -match 'Alternative|Resolve|Path')),('MapiGuid=' + [bool]($mapi -match 'Guid')),('MapiInterface=' + [bool]($mapi -match 'Interface|Enum')),('MapiIncludeCount=' + $includeCount)); $checks | ForEach-Object { Write-Output $_ }; if ($checks -contains 'ConverterArgument=False' -or $checks -contains 'ConverterMapiFolder=False' -or $checks -contains 'ConverterAlternative=False' -or $checks -contains 'MapiGuid=False' -or $checks -contains 'MapiInterface=False' -or $includeCount -ne 1) { exit 1 }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/regression-testing/` whose filename starts with `phase3-targeted-verification-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$converter = Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs' -Raw; $mapi = Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/MAPIMethodsTests.cs' -Raw; $includeCount = (Select-String -Path 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' -Pattern 'OutlookObjects\\Folder\\MAPIMethodsTests.cs').Count; $checks = @(('ConverterArgument=' + [bool]($converter -match 'Argument')),('ConverterMapiFolder=' + [bool]($converter -match 'MAPIFolder')),('ConverterAlternative=' + [bool]($converter -match 'Alternative|Resolve|Path')),('MapiGuid=' + [bool]($mapi -match 'Guid')),('MapiInterface=' + [bool]($mapi -match 'Interface|Enum')),('MapiIncludeCount=' + $includeCount)); $checks | ForEach-Object { Write-Output $_ }; if ($checks -contains 'ConverterArgument=False' -or $checks -contains 'ConverterMapiFolder=False' -or $checks -contains 'ConverterAlternative=False' -or $checks -contains 'MapiGuid=False' -or $checks -contains 'MapiInterface=False' -or $includeCount -ne 1) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary:` containing `ConverterArgument=True`, `ConverterMapiFolder=True`, `ConverterAlternative=True`, `MapiGuid=True`, `MapiInterface=True`, and `MapiIncludeCount=1`

- [x] [P3-T6] Run a coverage-enabled MSTest checkpoint and record whether `FolderConverter.cs` and `MAPIMethods.cs` reach 80% without any new production seam.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/regression-testing/` whose filename starts with `phase3-coverage-checkpoint-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary:` with numeric lines for `FolderConverter.cs` and `MAPIMethods.cs`, each using the exact filename, an equals sign, and the numeric percentage

---

### Phase 4 — Conditional Seam Remediation for Remaining Blockers

Execute this phase only if any Phase 2 or Phase 3 coverage checkpoint shows `FolderPredictor.cs` or `FolderConverter.cs` below `80%`.

- [x] [P4-T1] Record a blocker artifact that identifies the exact remaining uncovered branches in `FolderPredictor.cs` and `FolderConverter.cs` before changing production code.
  - **Precondition:** `phase2-coverage-checkpoint-*` or `phase3-coverage-checkpoint-*` records either file below `80%`.
  - **Acceptance:** Save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/other/` whose filename starts with `phase4-blocking-branches-` and whose contents include:
    - `Timestamp:`
    - `Blocking Files:` with one or both of `FolderPredictor.cs`, `FolderConverter.cs`
    - `Blocking Branches:`
    - `Output Summary:` stating whether the blocker is UI prompt, filesystem, or UI-thread affinity

- [x] [P4-T2] Introduce one non-public behavior-preserving seam in `FolderPredictor.cs` for the blocking static dependency if `FolderPredictor.cs` remains below `80%`.
  - **Precondition:** The Phase 4 blocker artifact names `FolderPredictor.cs`.
  - **Acceptance:** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` contains a new non-public wrapper member whose name includes `Prompt`, `Ui`, `Directory`, or `Factory`, and the file still contains the existing public type name `FolderPredictor`.

- [x] [P4-T3] Add seam-driven deterministic tests to `FolderPredictorTests.cs` that exercise both the default predictor path and the injected seam path.
  - **Precondition:** `P4-T2` is complete.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` contains MSTest method names including `Injected`, `Prompt`, `Ui`, or `Directory`, and those names appear in a new Phase 4 regression-testing artifact.

- [x] [P4-T4] Introduce one non-public behavior-preserving seam in `FolderConverter.cs` for the blocking static dependency if `FolderConverter.cs` remains below `80%`.
  - **Precondition:** The Phase 4 blocker artifact names `FolderConverter.cs`.
  - **Acceptance:** `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` contains a new non-public wrapper member whose name includes `Prompt`, `Dialog`, or `Input`, and the file still contains the existing public type name `FolderConverter`.

- [x] [P4-T5] Add seam-driven deterministic tests to `FolderConverterTests.cs` that exercise both the default converter path and the injected seam path.
  - **Precondition:** `P4-T4` is complete.
  - **Acceptance:** `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` contains MSTest method names including `Injected`, `Prompt`, `Dialog`, or `Input`, and those names appear in a new Phase 4 regression-testing artifact.

- [x] [P4-T6] Capture a Phase 4 targeted verification artifact that proves every conditional seam change is non-public and every seam-driven test is present only when the blocker artifact required it.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$predictor = if (Test-Path 'UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs') { Get-Content 'UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs' -Raw } else { '' }; $predictorTests = if (Test-Path 'UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs') { Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs' -Raw } else { '' }; $converter = if (Test-Path 'UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs') { Get-Content 'UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs' -Raw } else { '' }; $converterTests = if (Test-Path 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs') { Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs' -Raw } else { '' }; Write-Output ('PredictorHasNonPublicSeam=' + [bool]($predictor -match 'internal|protected')); Write-Output ('PredictorHasInjectedTests=' + [bool]($predictorTests -match 'Injected|Prompt|Ui|Directory')); Write-Output ('ConverterHasNonPublicSeam=' + [bool]($converter -match 'internal|protected')); Write-Output ('ConverterHasInjectedTests=' + [bool]($converterTests -match 'Injected|Prompt|Dialog|Input'))"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/regression-testing/` whose filename starts with `phase4-targeted-verification-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$predictor = if (Test-Path 'UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs') { Get-Content 'UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs' -Raw } else { '' }; $predictorTests = if (Test-Path 'UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs') { Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs' -Raw } else { '' }; $converter = if (Test-Path 'UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs') { Get-Content 'UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs' -Raw } else { '' }; $converterTests = if (Test-Path 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs') { Get-Content 'UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs' -Raw } else { '' }; Write-Output ('PredictorHasNonPublicSeam=' + [bool]($predictor -match 'internal|protected')); Write-Output ('PredictorHasInjectedTests=' + [bool]($predictorTests -match 'Injected|Prompt|Ui|Directory')); Write-Output ('ConverterHasNonPublicSeam=' + [bool]($converter -match 'internal|protected')); Write-Output ('ConverterHasInjectedTests=' + [bool]($converterTests -match 'Injected|Prompt|Dialog|Input'))"`
    - `EXIT_CODE: 0`
    - `Output Summary:` containing the four boolean lines written by the command

 - [x] [P4-T7] Run a coverage-enabled MSTest checkpoint and fail this phase if any still-blocked file remains below `80%` after the conditional seam work.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/regression-testing/` whose filename starts with `phase4-coverage-checkpoint-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary:` with numeric lines for every file that triggered `P4-T1`, each using the exact filename, an equals sign, and the numeric percentage, and every recorded percentage must be `>= 80`

---

### Phase 5 — Final QC Loop and Audit Preparation

Run the full C# toolchain loop in order. If any command exits non-zero or any step changes files, fix the reported issue and restart from `P5-T1`.
Because repository-wide baseline coverage is already far below `80%`, this phase treats the repo-wide `>= 80%` rule as an explicit, documented exception for this narrowly scoped folder-coverage feature: final repository coverage must not regress below baseline, every in-scope folder file must still meet `>= 80%`, and any remaining repo-wide shortfall must be called out explicitly in the feature docs and audit artifacts.

- [x] [P5-T1] Run the C# formatter with the repo-approved command after implementation changes.
  - **Acceptance:** Run
    `dotnet tool run csharpier .`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/` whose filename starts with `final-qa-format-` and whose contents include:
    - `Timestamp:`
    - `Command: dotnet tool run csharpier .`
    - `EXIT_CODE: 0`
    - `Output Summary: CSharpier completed for repo root`

- [x] [P5-T2] Run the analyzer-enabled MSBuild command.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/` whose filename starts with `final-qa-build-analyzer-` and whose contents include:
    - `Timestamp:`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Analyzer build succeeded with 0 errors`

- [x] [P5-T3] Run the nullable-enforced MSBuild command.
  - **Acceptance:** Run
    `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/` whose filename starts with `final-qa-build-nullable-` and whose contents include:
    - `Timestamp:`
    - `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    - `EXIT_CODE: 0`
    - `Output Summary: Nullable build succeeded with 0 errors`

- [x] [P5-T4] Run the coverage-enabled MSTest command and record the post-change repository coverage headline.
  - **Acceptance:** Run
    `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/` whose filename starts with `final-qa-test-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
    - `EXIT_CODE: 0`
    - `Output Summary:` containing exactly these three lines with actual numeric values:
      - `Passed=<count>`
      - `Failed=0`
      - `Repo Line Coverage: <percent>%`

- [x] [P5-T5] Calculate the final per-file coverage matrix and fail the gate if repository-wide coverage regresses below baseline, if any in-scope folder file remains below `80%`, or if changed production lines fall below `90%` coverage when applicable.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$baselineRepo = [double](([regex]::Match((Get-Content (Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/baseline-test-*.md' | Sort-Object Name | Select-Object -Last 1).FullName -Raw), 'Repo Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; $repo = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2); $targets = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs','UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs','UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs','UtilitiesCS/OutlookObjects/Folder/FolderTree.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs','UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs'); $under = New-Object System.Collections.Generic.List[string]; foreach ($target in $targets) { $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $target } | Select-Object -First 1; if (-not $class) { Write-Output ('Missing=' + $target); exit 1 }; $rate = [math]::Round([double]$class.'line-rate' * 100, 2); Write-Output ($target + '=' + $rate + '%'); if ($rate -lt 80) { $under.Add($target + '=' + $rate + '%') } }; $changedFiles = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs') | Where-Object { Test-Path $_ -and (git diff --name-only -- $_) }; $changedCoverageMet = $true; foreach ($changedFile in $changedFiles) { $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $changedFile } | Select-Object -First 1; $hitsByLine = @{}; foreach ($lineNode in $class.SelectNodes('./lines/line')) { $hitsByLine[[int]$lineNode.number] = [int]$lineNode.hits }; $diff = git diff --unified=0 -- $changedFile; $changedCovered = 0; $changedTotal = 0; foreach ($line in $diff) { if ($line -match '^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@') { $newStart = [int]$Matches[3]; $newCount = if ($Matches[4]) { [int]$Matches[4] } else { 1 }; if ($newCount -gt 0) { for ($i = $newStart; $i -le ($newStart + $newCount - 1); $i++) { $changedTotal++; if ($hitsByLine.ContainsKey($i) -and $hitsByLine[$i] -gt 0) { $changedCovered++ } } } } }; $changedPercent = if ($changedTotal -eq 0) { 100 } else { [math]::Round(($changedCovered / $changedTotal) * 100, 2) }; Write-Output ($changedFile + ':ChangedCoverage=' + $changedPercent + '%'); if ($changedPercent -lt 90) { $changedCoverageMet = $false } }; $repoDelta = [math]::Round($repo - $baselineRepo, 2); $repoCoverageBelow80 = $repo -lt 80; Write-Output ('BaselineRepo=' + $baselineRepo + '%'); Write-Output ('FinalRepo=' + $repo + '%'); Write-Output ('RepoDelta=' + $repoDelta + '%'); Write-Output ('RepoCoverageBelow80=' + $repoCoverageBelow80); Write-Output ('RepoCoverageExceptionRequired=' + $repoCoverageBelow80); Write-Output ('AnyFileUnder80=' + [bool]($under.Count -gt 0)); Write-Output ('ChangedProductionCoverageMet=' + $changedCoverageMet); if ($repo -lt $baselineRepo -or $under.Count -gt 0 -or -not $changedCoverageMet) { exit 1 }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/` whose filename starts with `final-qa-coverage-delta-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$baselineRepo = [double](([regex]::Match((Get-Content (Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/baseline/baseline-test-*.md' | Sort-Object Name | Select-Object -Last 1).FullName -Raw), 'Repo Line Coverage: ([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value); [xml]$coverage = Get-Content 'coverage/coverage.cobertura.xml'; $repo = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2); $targets = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs','UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs','UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs','UtilitiesCS/OutlookObjects/Folder/FolderTree.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs','UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs','UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs'); $under = New-Object System.Collections.Generic.List[string]; foreach ($target in $targets) { $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $target } | Select-Object -First 1; if (-not $class) { Write-Output ('Missing=' + $target); exit 1 }; $rate = [math]::Round([double]$class.'line-rate' * 100, 2); Write-Output ($target + '=' + $rate + '%'); if ($rate -lt 80) { $under.Add($target + '=' + $rate + '%') } }; $changedFiles = @('UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs','UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs') | Where-Object { Test-Path $_ -and (git diff --name-only -- $_) }; $changedCoverageMet = $true; foreach ($changedFile in $changedFiles) { $class = $coverage.SelectNodes('//class') | Where-Object { $_.filename -eq $changedFile } | Select-Object -First 1; $hitsByLine = @{}; foreach ($lineNode in $class.SelectNodes('./lines/line')) { $hitsByLine[[int]$lineNode.number] = [int]$lineNode.hits }; $diff = git diff --unified=0 -- $changedFile; $changedCovered = 0; $changedTotal = 0; foreach ($line in $diff) { if ($line -match '^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@') { $newStart = [int]$Matches[3]; $newCount = if ($Matches[4]) { [int]$Matches[4] } else { 1 }; if ($newCount -gt 0) { for ($i = $newStart; $i -le ($newStart + $newCount - 1); $i++) { $changedTotal++; if ($hitsByLine.ContainsKey($i) -and $hitsByLine[$i] -gt 0) { $changedCovered++ } } } } }; $changedPercent = if ($changedTotal -eq 0) { 100 } else { [math]::Round(($changedCovered / $changedTotal) * 100, 2) }; Write-Output ($changedFile + ':ChangedCoverage=' + $changedPercent + '%'); if ($changedPercent -lt 90) { $changedCoverageMet = $false } }; $repoDelta = [math]::Round($repo - $baselineRepo, 2); $repoCoverageBelow80 = $repo -lt 80; Write-Output ('BaselineRepo=' + $baselineRepo + '%'); Write-Output ('FinalRepo=' + $repo + '%'); Write-Output ('RepoDelta=' + $repoDelta + '%'); Write-Output ('RepoCoverageBelow80=' + $repoCoverageBelow80); Write-Output ('RepoCoverageExceptionRequired=' + $repoCoverageBelow80); Write-Output ('AnyFileUnder80=' + [bool]($under.Count -gt 0)); Write-Output ('ChangedProductionCoverageMet=' + $changedCoverageMet); if ($repo -lt $baselineRepo -or $under.Count -gt 0 -or -not $changedCoverageMet) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary:` containing 13 per-file coverage lines plus `BaselineRepo=` followed by the actual numeric baseline percentage, `FinalRepo=` followed by the actual numeric final percentage, `RepoDelta=` followed by the numeric delta, `RepoCoverageBelow80=` followed by the actual boolean value, `RepoCoverageExceptionRequired=` followed by the same boolean value, `AnyFileUnder80=False`, and `ChangedProductionCoverageMet=True`

- [x] [P5-T6] Confirm the final QA artifacts exist and each records `EXIT_CODE: 0`.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$files = @((Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-format-*.md' | Sort-Object Name | Select-Object -Last 1).FullName,(Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-build-analyzer-*.md' | Sort-Object Name | Select-Object -Last 1).FullName,(Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-build-nullable-*.md' | Sort-Object Name | Select-Object -Last 1).FullName,(Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-test-*.md' | Sort-Object Name | Select-Object -Last 1).FullName,(Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-coverage-delta-*.md' | Sort-Object Name | Select-Object -Last 1).FullName); $count = ($files | Where-Object { $_ -and (Test-Path $_) }).Count; $matches = (Select-String -Path $files -Pattern '^EXIT_CODE: 0$').Count; Write-Output ('QaArtifactCount=' + $count); Write-Output ('ExitCodeZeroCount=' + $matches); if ($count -ne 5 -or $matches -ne 5) { exit 1 }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/` whose filename starts with `final-qa-artifact-check-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$files = @((Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-format-*.md' | Sort-Object Name | Select-Object -Last 1).FullName,(Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-build-analyzer-*.md' | Sort-Object Name | Select-Object -Last 1).FullName,(Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-build-nullable-*.md' | Sort-Object Name | Select-Object -Last 1).FullName,(Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-test-*.md' | Sort-Object Name | Select-Object -Last 1).FullName,(Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-coverage-delta-*.md' | Sort-Object Name | Select-Object -Last 1).FullName); $count = ($files | Where-Object { $_ -and (Test-Path $_) }).Count; $matches = (Select-String -Path $files -Pattern '^EXIT_CODE: 0$').Count; Write-Output ('QaArtifactCount=' + $count); Write-Output ('ExitCodeZeroCount=' + $matches); if ($count -ne 5 -or $matches -ne 5) { exit 1 }"`
    - `EXIT_CODE: 0`
    - `Output Summary: QaArtifactCount=5; ExitCodeZeroCount=5`

- [x] [P5-T7] Update the active feature docs with the final seam decision, repo-wide coverage exception rationale, and evidence references once the QA gates pass.
  - **Acceptance:** Run
    `pwsh -NoProfile -Command "$coverageArtifact = (Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-coverage-delta-*.md' | Sort-Object Name | Select-Object -Last 1).FullName; $coverageText = Get-Content $coverageArtifact -Raw; $baselineRepo = ([regex]::Match($coverageText, 'BaselineRepo=([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value; $finalRepo = ([regex]::Match($coverageText, 'FinalRepo=([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value; $docs = @('docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/issue.md','docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/user-story.md','docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/spec.md'); foreach ($doc in $docs) { $text = Get-Content $doc -Raw; $checks = [ordered]@{ QaRef = $text.Contains('evidence/qa-gates/'); ThresholdRef = $text.Contains('80%'); ExceptionRef = $text.Contains('Repo-wide coverage exception'); ScopeRef = $text.Contains('outside approved folder scope'); BaselineRef = $text.Contains($baselineRepo + '%'); FinalRef = $text.Contains($finalRepo + '%') }; $summary = ($checks.GetEnumerator() | ForEach-Object { $_.Key + '=' + $_.Value }) -join '; '; Write-Output ($doc + ':' + $summary); if ($checks.Values -contains $false) { exit 1 } }"`
    and save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/` whose filename starts with `final-docs-coverage-exception-check-` and whose contents include:
    - `Timestamp:`
    - `Command: pwsh -NoProfile -Command "$coverageArtifact = (Get-ChildItem 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-coverage-delta-*.md' | Sort-Object Name | Select-Object -Last 1).FullName; $coverageText = Get-Content $coverageArtifact -Raw; $baselineRepo = ([regex]::Match($coverageText, 'BaselineRepo=([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value; $finalRepo = ([regex]::Match($coverageText, 'FinalRepo=([0-9]+(?:\.[0-9]+)?)%')).Groups[1].Value; $docs = @('docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/issue.md','docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/user-story.md','docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/spec.md'); foreach ($doc in $docs) { $text = Get-Content $doc -Raw; $checks = [ordered]@{ QaRef = $text.Contains('evidence/qa-gates/'); ThresholdRef = $text.Contains('80%'); ExceptionRef = $text.Contains('Repo-wide coverage exception'); ScopeRef = $text.Contains('outside approved folder scope'); BaselineRef = $text.Contains($baselineRepo + '%'); FinalRef = $text.Contains($finalRepo + '%') }; $summary = ($checks.GetEnumerator() | ForEach-Object { $_.Key + '=' + $_.Value }) -join '; '; Write-Output ($doc + ':' + $summary); if ($checks.Values -contains $false) { exit 1 } }"`
    - `EXIT_CODE: 0`
    - `Output Summary:` containing one line per file for `issue.md`, `user-story.md`, and `spec.md`, each ending with `QaRef=True; ThresholdRef=True; ExceptionRef=True; ScopeRef=True; BaselineRef=True; FinalRef=True`

- [x] [P5-T8] Record an audit-ready evidence index that lists the latest baseline, regression-testing, other, and QA-gate artifacts for this feature and explicitly cites the repo-wide coverage exception rationale.
  - **Acceptance:** Save a new ISO-8601-stamped markdown artifact under `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/other/` whose filename starts with `audit-evidence-index-` and whose contents include:
    - `Timestamp:`
    - `Baseline Artifacts:`
    - `Regression Testing Artifacts:`
    - `Other Artifacts:`
    - `QA Gate Artifacts:`
    - `Coverage Exception: Repo-wide coverage exception`
    - `Rationale: repository-wide coverage remains below 80% after improving the scoped folder subsystem, and further repo-wide uplift is outside approved folder scope`
    - `Coverage Gate Reference:` followed by the path fragment `evidence/qa-gates/final-qa-coverage-delta-`
    - `Output Summary: audit-ready evidence index prepared with explicit repo-wide coverage exception rationale`

---

## Plan Self-Check

| Gate | Status |
|---|---|
| Canonical phase headings (`### Phase N — descriptive title`) | ✅ |
| Task IDs (`[P#-T#]`) sequential per phase | ✅ |
| Work mode matches `issue.md` (`full-feature`) | ✅ |
| No forbidden placeholder tokens from the template | ✅ |
| Machine-verifiable acceptance criteria | ✅ |
| Phase 0 includes policy-read evidence plus baseline restore, format, analyzer, nullable, and coverage artifacts | ✅ |
| Implementation phases include targeted verification tasks inside the same phase | ✅ |
| Conditional seam work is gated by coverage evidence and remains non-public | ✅ |
| Final QA loop is unconditional and coverage-bearing | ✅ |
| Final QA coverage gate checks all 13 compiled folder files individually | ✅ |

---

## Preflight Validation

DIRECTIVE: PREFLIGHT VALIDATION ONLY

**Required route:** `csharp-atomic-planning -> atomic_planner -> atomic_executor`

This plan has been normalized for executor compatibility and self-checked against the repository’s atomic-plan contract, but it still requires delegated validation-only preflight through the required route before Step 4 can be considered complete.
