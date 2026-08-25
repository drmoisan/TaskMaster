# Issue #439 Remediation Plan

## Objective and hard constraints

Remediate the three review findings from `remediation-inputs.2026-08-24T22-25.md` in the isolated worktree. The authoritative remediation source supplements the full-bug requirements in `spec.md` and does not weaken them. Preserve the public `BreadcrumbBridgeRouter` contract, the verified absence of `[ExcludeFromCodeCoverage]` in `QuickFiler/Controllers/EfcFormController.cs`, and all Issue #439 behavior.

No new or modified test may instantiate a WinForms or WebView2 window, control, or handle; call `Show`, `ShowDialog`, or `Application.Run`; start a UI message pump; use Outlook COM; or use temporary files, filesystem, network, or external processes. Tests must use only narrow seams and deterministic Moq/fake collaborators. No task stages, commits, pushes, publishes, or creates/edits a pull request.

All evidence paths below are canonical feature evidence paths. Any unavailable required measurement, failing static headless audit, infeasible controller-coverage design, failed command, or failed comparison is `REMEDIATION_REQUIRED`; it must stop execution before the next implementation task and must not be replaced by a human step, a skipped command, a coverage exemption, or a live GUI/COM test.

### Phase 0 — Policy, baseline, and feasibility gates

- [x] [P0-T1] Read `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-24T17-27-23\AGENTS.md` (standing instructions, cross-language code-change policy, and cross-language unit-test policy), then `.agents\skills\csharp\SKILL.md`, then `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\issue.md`, then `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\spec.md`, then `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\remediation-inputs.2026-08-24T22-25.md`; run `git rev-parse HEAD`, `git branch --show-current`, and `git status --short`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\remediation-baseline\phase0-instructions-read.2026-08-24T22-25.md` contains `Timestamp:`, `Policy Order:`, `Files Read:`, `Work Mode: full-bug`, `Acceptance Criteria Source: spec.md`, baseline commit, branch, worktree status, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`.
- [x] [P0-T2] Count physical lines with `(Get-Content -LiteralPath '<path>').Count` for `QuickFiler\Controllers\BreadcrumbBridgeRouter.cs`, `QuickFiler\Controllers\EfcFormController.cs`, and `QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\remediation-baseline\issue-439-file-size-baseline.2026-08-24T22-25.md` contains `Timestamp:`, `Command:` for each count command, `EXIT_CODE: 0`, `Output Summary:`, the three numeric counts, and the two over-500 findings.
- [x] [P0-T3] Run `dotnet tool run csharpier format .` from `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-24T17-27-23`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\remediation-baseline\csharpier.2026-08-24T22-25.md` contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; a formatter change remains unstaged in the working copy and restarts Phase 0 from P0-T1.
- [x] [P0-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\remediation-baseline\csharp-analyzers.2026-08-24T22-25.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and the numeric analyzer diagnostic baseline.
- [x] [P0-T5] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\remediation-baseline\csharp-nullable.2026-08-24T22-25.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, and numeric compiler and nullable diagnostic baselines.
- [x] [P0-T6] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/remediation-baseline/issue-439-remediation-baseline.normalized.cobertura.xml`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\remediation-baseline\csharp-coverage.2026-08-24T22-25.md` contains required command fields, identifies the wrapper-produced normalized XML as the comparison input, numeric repository coverage, test count, normalization invariant results, and numeric line coverage for `EfcFormController.cs` (81/721 = 11.234397% unless the current source differs), `BreadcrumbBridgeRouter.cs`, and every existing Issue #439 changed production file.
- [ ] [P0-T7] Perform a baseline-only static audit of existing Issue #439 tests with `$auditPaths = @('QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs', 'QuickFiler.Test\Controllers\EfcFormControllerTests.cs') + @(Get-ChildItem -LiteralPath 'UtilitiesCS.Test\OutlookObjects\Folder' -Filter 'Breadcrumb*Tests.cs' -File | Sort-Object FullName | ForEach-Object { $_.FullName }); rg -n -i 'new\s+(System\.Windows\.Forms\.|Microsoft\.Web\.WebView2)|CreateControl|CreateHandle|ShowDialog|\.Show\(|Application\.Run|DoEvents|Outlook\.|Marshal\.GetActiveObject|System\.IO|File\.|Directory\.|HttpClient|WebClient|Process\.Start|Temporary' -- $auditPaths`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\remediation-baseline\issue-439-headless-static-audit.2026-08-24T22-25.md` contains command fields, every match location, a disposition for every match, and `HEADLESS_AUDIT: PASS` only when the existing relevant tests contain no executable prohibited API use.
- [ ] [P0-T8] Produce an automated feasibility decision for the controller coverage defect by mapping each currently uncovered `EfcFormController.cs` sequence point from P0-T6 to one of: headless testable existing seam, candidate narrow injected seam, or WinForms/WebView2/COM-only dependency; calculate whether a cohesive extraction plus forwarding adapters can leave an instrumented `EfcFormController.cs` with at least 80% directly headless-coverable sequence points without changing a public API or moving unrelated EfcViewer behavior; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\remediation-baseline\efc-form-controller-feasibility.2026-08-24T22-25.md` contains command fields, the sequence-point inventory, proposed file/method ownership, projected numerator/denominator/percentage, headless-test design, and exactly one result: `FEASIBILITY: PROCEED` or `REMEDIATION_REQUIRED: EFC_FORM_CONTROLLER_HEADLESS_80_PERCENT_INFEASIBLE`. On the remediation-required result, stop before P1-T1 and report the result without a human step.

### Phase 1 — Router production-file reduction

- [ ] [P1-T1] When P0-T8 is `FEASIBILITY: PROCEED`, split `QuickFiler\Controllers\BreadcrumbBridgeRouter.cs` into partial-class source files `QuickFiler\Controllers\BreadcrumbBridgeRouter.cs`, `QuickFiler\Controllers\BreadcrumbBridgeRouter.Binding.cs`, and `QuickFiler\Controllers\BreadcrumbBridgeRouter.Navigation.cs`, keeping the constructor, public overloads, public properties, events, and observable behavior on the same public `BreadcrumbBridgeRouter` type; place hierarchy-path conversion, chain retrieval, and key attachment with binding, and inbound dispatch, arrow/segment/child activation, selection, host delivery, and row lookup with navigation; acceptance: every modified/added router production file is `<=500` physical lines, `BreadcrumbBridgeRouter` has no public signature or event change, and archive-root conversion, typed inbound validation, fallback, active-segment provider-key use, event propagation, and queued host delivery remain in their original behavioral owner paths.
- [ ] [P1-T2] Update `QuickFiler\QuickFiler.csproj` with explicit `Compile` items for `Controllers\BreadcrumbBridgeRouter.Binding.cs` and `Controllers\BreadcrumbBridgeRouter.Navigation.cs`; acceptance: `msbuild QuickFiler\QuickFiler.csproj /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` exits 0 and each split router source file is included exactly once.
- [ ] [P1-T3] Add or update only headless MSTest coverage in `QuickFiler.Test\Controllers\BreadcrumbBridgeRouterTests.cs` and existing Issue #439 test coverage for the router seams moved in P1-T1; acceptance: strict Moq/fake provider and web-host tests cover archive-relative and already-rooted conversion, null/empty/exception/cancellation fallback, invalid typed-message state preservation, active ancestor expansion, child selection, and `SelectedFolderPathChanged` propagation without constructing a form, control, handle, WebView2 object, COM object, filesystem resource, network client, or process.

### Phase 2 — Issue #439 test-file reduction

- [ ] [P2-T1] Split `QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs` into `QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs`, `QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439NavigationTests.cs`, and `QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439TestSupport.cs`, placing only shared pure test builders in the support file and preserving the seven existing Issue #439 test scenarios and assertions across MSTest classes; acceptance: every modified/added Issue #439 test file is `<=500` physical lines, all seven test method names remain discoverable, and every test retains a clear Arrange-Act-Assert structure.
- [ ] [P2-T2] Update `QuickFiler.Test\QuickFiler.Test.csproj` with one explicit `Compile` item for each new split Issue #439 test source file; acceptance: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` exits 0 and `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /ListTests` lists all seven Issue #439 test names exactly once.
- [ ] [P2-T3] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests|FullyQualifiedName~EfcFormControllerTests" /InIsolation`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\regression-testing\issue-439-remediation-router-and-file-split.2026-08-24T22-25.md` contains command fields, exactly `17/17` passing audited Issue #439-related tests, and a declaration cross-referencing the passing P0-T7 headless static audit.

### Phase 3 — EfcFormController headless coverage remediation

- [ ] [P3-T1] When P0-T8 is `FEASIBILITY: PROCEED`, extract only the cohesion groups identified by its sequence-point inventory from `QuickFiler\Controllers\EfcFormController.cs` into the explicitly named, narrow controller collaborators recorded in `evidence\remediation-baseline\efc-form-controller-feasibility.2026-08-24T22-25.md`; retain the legacy public controller methods as forwarding adapters and retain `BindBreadcrumbRowsAsync` as the internal Issue #439 binding seam; acceptance: the resulting `EfcFormController.cs` is instrumented, contains no `ExcludeFromCodeCoverage` attribute or `System.Diagnostics.CodeAnalysis` using directive, preserves all pre-existing public signatures, and its remaining sequence points are exactly the P0-T8 projected headless-testable set.
- [ ] [P3-T2] Update `QuickFiler\QuickFiler.csproj` with one explicit `Compile` item for every collaborator extracted by P3-T1; acceptance: `msbuild QuickFiler\QuickFiler.csproj /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` exits 0 and each extracted source file is included exactly once.
- [ ] [P3-T3] Add MSTest classes under `QuickFiler.Test\Controllers\` only for the collaborators extracted in P3-T1 and for the preserved internal binding seam, then update `QuickFiler.Test\QuickFiler.Test.csproj` with one explicit `Compile` item for every added test source; acceptance: each test uses only strict mocks/fakes for controller dependencies, covers positive, negative, edge, and error branches in the P0-T8 inventory, contains no prohibited GUI, WebView2, COM, filesystem, network, temporary-file, or process use, and each added test source is included exactly once; after updating the project, `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` exits 0 before P3-T4 runs.
- [ ] [P3-T4] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~EfcFormController" /InIsolation`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\regression-testing\efc-form-controller-headless-coverage.2026-08-24T22-25.md` contains command fields, all selected tests pass, and the evidence explicitly verifies the P3-T3 tests did not instantiate or invoke a prohibited boundary.

### Phase 4 — Full C# quality gate and coverage comparison

- [ ] [P4-T1] Run `dotnet tool run csharpier format .`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\qa-gates\remediation-csharpier.2026-08-24T22-25.md` contains command fields and a no-change result. Any formatter change restarts this phase at P4-T1 after the changed source is brought within plan scope.
- [ ] [P4-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\qa-gates\remediation-csharp-analyzers.2026-08-24T22-25.md` contains command fields, final numeric diagnostic count, P0-T4 baseline count, and zero new findings. A failure restarts this phase at P4-T1 after an in-scope correction.
- [ ] [P4-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\qa-gates\remediation-csharp-nullable.2026-08-24T22-25.md` contains command fields, final compiler/nullable counts, P0-T5 baseline counts, and zero new diagnostics. A failure restarts this phase at P4-T1 after an in-scope correction.
- [ ] [P4-T4] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests|FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~EfcFormController" /InIsolation`, then derive the test-source list from the union of `git diff --name-only <P0-T1-baseline-commit> -- '*.cs'` and `git ls-files --others --exclude-standard -- '*.cs'`, retaining only changed or untracked paths under test projects, including staged, unstaged, and untracked remediation worktree changes, before running `rg -n -i 'new\s+(System\.Windows\.Forms\.|Microsoft\.Web\.WebView2)|CreateControl|CreateHandle|ShowDialog|\.Show\(|Application\.Run|DoEvents|Outlook\.|Marshal\.GetActiveObject|System\.IO|File\.|Directory\.|HttpClient|WebClient|Process\.Start|Temporary'` against exactly that derived test-file list; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\regression-testing\issue-439-remediation-final-headless.2026-08-24T22-25.md` contains command fields, all selected tests pass, the explicit baseline-commit-to-worktree range, a nonempty baseline-to-worktree-derived test-source list that includes untracked C# test files, every match location, a disposition for every match, and `HEADLESS_AUDIT: PASS` only when no executable prohibited API is present in every added/modified Issue #439 test.
- [ ] [P4-T5] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-remediation-final.normalized.cobertura.xml`; acceptance: `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\qa-gates\remediation-csharp-coverage.2026-08-24T22-25.md` contains required command fields, identifies the wrapper-produced normalized XML as the comparison input, test count, numeric repository coverage `>=80%`, all normalization invariants, and numeric per-file coverage for every changed/new production file.
- [ ] [P4-T6] Run the exact read-only comparison script below from the worktree; it loads only the wrapper-produced normalized XML files from P0-T6 and P4-T5, derives changed production and test paths from the P0-T1 baseline commit through the worktree (including staged and unstaged remediation changes), and writes only `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\qa-gates\issue-439-remediation-coverage-comparison.2026-08-24T22-25.md`.

  ```powershell
  @'
  $ErrorActionPreference = 'Stop'
  . .\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1
  $feature = 'docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439'
  $baselineRecord = Join-Path $feature 'evidence/remediation-baseline/phase0-instructions-read.2026-08-24T22-25.md'
  $baselineCommit = ([regex]::Match((Get-Content -Raw -LiteralPath $baselineRecord), '(?m)^Baseline Commit:\s*`?([0-9a-f]+)`?\s*$')).Groups[1].Value
  if ([string]::IsNullOrWhiteSpace($baselineCommit)) { throw 'Missing P0-T1 baseline commit.' }
  $baselineXmlPath = Join-Path $feature 'evidence/remediation-baseline/issue-439-remediation-baseline.normalized.cobertura.xml'
  $finalXmlPath = Join-Path $feature 'evidence/qa-gates/issue-439-remediation-final.normalized.cobertura.xml'
  $outputPath = Join-Path $feature 'evidence/qa-gates/issue-439-remediation-coverage-comparison.2026-08-24T22-25.md'
  [xml]$baselineXml = Get-Content -Raw -LiteralPath $baselineXmlPath
  [xml]$finalXml = Get-Content -Raw -LiteralPath $finalXmlPath
  function Assert-Normalized([xml]$document) {
      $sources = @($document.SelectNodes('/coverage/sources/source') | ForEach-Object { $_.InnerText })
      if ($sources.Count -ne 1 -or $sources[0] -ne '.') { throw 'Invalid normalized sources element.' }
      $classes = @($document.SelectNodes('//class'))
      if ($classes.Count -eq 0) { throw 'Normalized XML contains no classes.' }
      $names = @{}
      foreach ($class in $classes) {
          $name = $class.GetAttribute('filename').Replace('\\', '/')
          if ([string]::IsNullOrWhiteSpace($name) -or $name -match '^[A-Za-z]:|^/') { throw "Non-relative class filename: $name" }
          if ($names.ContainsKey($name)) { throw "Unmerged class filename: $name" }
          $names[$name] = $true
      }
      foreach ($package in @($document.SelectNodes('//package'))) {
          if ($package.GetAttribute('name') -match 'log4net|Mono.Reflection|Microsoft.IO.RecyclableMemoryStream|System.Interactive|System.Linq.Async') { throw "Forbidden package: $($package.GetAttribute('name'))" }
      }
  }
  function Get-FileSummary([xml]$document, [string]$file) {
      $classes = @($document.SelectNodes('//class') | Where-Object { $_.GetAttribute('filename').Replace('\\', '/') -eq $file })
      if ($classes.Count -ne 1) { throw "Missing or non-merged numeric coverage for $file" }
      return Get-CoberturaClassLineSummary -ClassNode $classes[0]
  }
  function Get-ChangedLineMap([string]$range) {
      $map = @{}; $current = $null
      foreach ($line in @(git diff --no-color --unified=0 $range -- '*.cs')) {
          if ($line -match '^\+\+\+ b/(.+)$') { $current = $Matches[1]; if (-not $map.ContainsKey($current)) { $map[$current] = @{} }; continue }
          if ($current -and $line -match '^@@ -\d+(?:,\d+)? \+(\d+)(?:,(\d+))? @@') {
              $start = [int]$Matches[1]; $count = if ($Matches[2]) { [int]$Matches[2] } else { 1 }
              foreach ($number in $start..($start + $count - 1)) { if ($count -gt 0) { $map[$current][$number] = $true } }
          }
      }
      return $map
  }
  Assert-Normalized $baselineXml; Assert-Normalized $finalXml
  $range = $baselineCommit
  $trackedChangedCs = @(git diff --name-only $baselineCommit -- '*.cs')
  $untrackedChangedCs = @(git ls-files --others --exclude-standard -- '*.cs')
  $changedCs = @($trackedChangedCs + $untrackedChangedCs | Sort-Object -Unique)
  $productionFiles = @($changedCs | Where-Object { $_ -notmatch '(^|/)[^/]*\.Test(s)?/' })
  $testFiles = @($changedCs | Where-Object { $_ -match '(^|/)[^/]*\.Test(s)?/' })
  if ($productionFiles.Count -eq 0) { throw 'No changed or untracked production C# files in remediation comparison range.' }
  if ($productionFiles -notcontains 'QuickFiler/Controllers/EfcFormController.cs') { throw 'EfcFormController.cs is absent from the remediation comparison range.' }
  $baselineSummary = Get-CoberturaCoverageSummary -XmlDocument $baselineXml
  $finalSummary = Get-CoberturaCoverageSummary -XmlDocument $finalXml
  $changedLines = Get-ChangedLineMap $baselineCommit
  $failures = [System.Collections.Generic.List[string]]::new()
  $rows = [System.Collections.Generic.List[string]]::new()
  $changedCovered = 0; $changedTotal = 0
  foreach ($file in $productionFiles) {
      $finalFile = Get-FileSummary $finalXml $file
      $finalRate = if ($finalFile.TotalLines -gt 0) { 100.0 * $finalFile.CoveredLines / $finalFile.TotalLines } else { $null }
      if ($null -eq $finalRate) { $failures.Add("Missing final numeric coverage for $file"); continue }
      $baselineClass = @($baselineXml.SelectNodes('//class') | Where-Object { $_.GetAttribute('filename').Replace('\\', '/') -eq $file })
      if ($baselineClass.Count -eq 1) {
          $baselineFile = Get-CoberturaClassLineSummary -ClassNode $baselineClass[0]
          $baselineRate = if ($baselineFile.TotalLines -gt 0) { 100.0 * $baselineFile.CoveredLines / $baselineFile.TotalLines } else { $null }
          if ($null -eq $baselineRate -or $finalRate -lt $baselineRate) { $failures.Add("Coverage regression for $file") }
          $rows.Add(('{0}: baseline {1:N6}% ({2}/{3}); final {4:N6}% ({5}/{6})' -f $file,$baselineRate,$baselineFile.CoveredLines,$baselineFile.TotalLines,$finalRate,$finalFile.CoveredLines,$finalFile.TotalLines))
      } else {
          if ($finalRate -lt 90) { $failures.Add("New production file below 90%: $file") }
          $rows.Add(('{0}: new-file final {1:N6}% ({2}/{3})' -f $file,$finalRate,$finalFile.CoveredLines,$finalFile.TotalLines))
      }
      if ($file -eq 'QuickFiler/Controllers/EfcFormController.cs' -and $finalRate -lt 80) { $failures.Add('EfcFormController.cs is below 80%.') }
      if ($changedLines.ContainsKey($file)) {
          foreach ($number in $changedLines[$file].Keys) {
              if ($finalFile.LineMap.ContainsKey([int]$number)) {
                  $changedTotal++
                  if ($finalFile.LineMap[[int]$number].Hits -gt 0) { $changedCovered++ }
              }
          }
      } elseif ($untrackedChangedCs -contains $file) {
          foreach ($line in $finalFile.LineMap.Values) {
              $changedTotal++
              if ($line.Hits -gt 0) { $changedCovered++ }
          }
      }
  }
  foreach ($file in @($productionFiles + $testFiles)) { if ((Get-Content -LiteralPath $file).Count -gt 500) { $failures.Add("File exceeds 500 physical lines: $file") } }
  $changedRate = if ($changedTotal -gt 0) { 100.0 * $changedCovered / $changedTotal } else { $null }
  if ($null -eq $changedRate -or $changedRate -lt 90) { $failures.Add('Changed/new instrumentable production coverage is below 90%.') }
  $baselineRepositoryRate = 100.0 * [double]$baselineSummary.LinesCovered / [double]$baselineSummary.LinesValid
  $repositoryRate = 100.0 * [double]$finalSummary.LinesCovered / [double]$finalSummary.LinesValid
  if ($repositoryRate -lt 80) { $failures.Add('Repository coverage is below 80%.') }
  $exclusion = @(rg -n -i 'ExcludeFromCodeCoverage|System.Diagnostics.CodeAnalysis' QuickFiler/Controllers/EfcFormController.cs)
  if ($LASTEXITCODE -eq 0) { $failures.Add('EfcFormController coverage exclusion remains present.') }
  $lines = @('Timestamp: ' + (Get-Date -Format 'yyyy-MM-ddTHH-mm'), 'Command: exact P4-T6 inline PowerShell normalized-coverage comparison', 'EXIT_CODE: ' + $(if ($failures.Count -eq 0) { '0' } else { '1' }), 'Output Summary: normalized baseline/final coverage, file-size, changed-line, and exclusion comparison.', 'Baseline Commit: ' + $baselineCommit, 'Range: ' + $baselineCommit + '..WORKTREE', 'Baseline SHA256: ' + (Get-FileHash -Algorithm SHA256 -LiteralPath $baselineXmlPath).Hash, 'Final SHA256: ' + (Get-FileHash -Algorithm SHA256 -LiteralPath $finalXmlPath).Hash, ('Repository baseline/final/delta: {0:N6}% ({1}/{2}) / {3:N6}% ({4}/{5}) / {6:N6} percentage points' -f $baselineRepositoryRate,$baselineSummary.LinesCovered,$baselineSummary.LinesValid,$repositoryRate,$finalSummary.LinesCovered,$finalSummary.LinesValid,($repositoryRate - $baselineRepositoryRate)), ('Changed/new instrumentable production: {0}/{1} = {2:N6}%' -f $changedCovered,$changedTotal,$changedRate), 'Normalized invariants: PASS', 'Changed production files: ' + ($productionFiles -join ', '), 'Changed test files: ' + ($testFiles -join ', ')) + $rows + @('Efc exclusion search: ' + $(if ($LASTEXITCODE -eq 1) { 'no match' } else { 'match' })) + $failures
  Set-Content -LiteralPath $outputPath -Value $lines
  if ($failures.Count -gt 0) { exit 1 }
  '@ | pwsh -NoProfile -Command -
  ```

  Acceptance: the artifact contains command fields, SHA-256 hashes, normalized-input invariants, the P0-T1 baseline-commit-to-worktree range including staged, unstaged, and untracked remediation changes, numeric repository baseline/final/delta values, per-file no-regression results for every pre-existing changed production file, every new-production-file `>=90%` result, changed/new instrumentable production coverage `>=90%`, physical line counts `<=500` for every changed/added production and test file, numeric `EfcFormController.cs` coverage `>=80%`, and a no-match Efc exclusion result. The command exits nonzero for any missing numeric value, invariant failure, regression, size breach, threshold failure, or excluded controller; that result is `REMEDIATION_REQUIRED` and restarts at the applicable implementation task.
- [ ] [P4-T7] Produce `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\evidence\qa-gates\issue-439-remediation-qa-loop.2026-08-24T22-25.md` only after P4-T1 through P4-T6 pass in one uninterrupted loop; acceptance: it enumerates the final formatter, analyzer, nullable, focused headless regression, coverage-wrapper, and normalized-comparison command artifacts with their exit codes and confirms no command was skipped.

### Phase 5 — Review handoff

- [ ] [P5-T1] After the parent orchestration has automatically created the post-P4 stage commit, without executor staging or committing, run the repository feature-review workflow against that committed comparison range; acceptance: validated timestamped `policy-audit`, `code-review`, and `feature-audit` artifacts in `docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\` report `REVIEW_STATUS: PASS`, explicitly confirm the file-size limits, numeric controller coverage, absent exclusion, and headless-test constraints, or generate a new automated remediation input/plan pair without a publish step. An unavailable automated stage-commit handoff records `REMEDIATION_REQUIRED: REVIEW_COMPARISON_COMMIT_UNAVAILABLE` without requesting human action.
