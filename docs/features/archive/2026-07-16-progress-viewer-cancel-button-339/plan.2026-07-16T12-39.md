# progress-viewer-cancel-button (Plan)

- **Issue:** #339
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/339
- **Owner:** drmoisan
- **Last Updated:** 2026-07-16T12-39
- **Status:** Executor-ready pending mandatory preflight and plan validation
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Requirements Source:** `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/issue.md`, limited to the checkbox items under its explicit `## Acceptance Criteria` section
- **Feature Folder:** `docs/features/active/2026-07-16-progress-viewer-cancel-button-339`
- **Implementation Scope:** `UtilitiesCS/Threading/ProgressViewer.cs` and `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`

## Requirements and Scope Boundary

This three-phase minimal-audit plan uses `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/issue.md` as the sole requirements and acceptance-criteria source. `spec.md`, `user-story.md`, and `research.md` are neither required nor permitted as substitute requirements for this minor-audit plan. If `spec.md` or `user-story.md` exists unexpectedly in the feature folder, or if the explicit `## Acceptance Criteria` section is missing, execution must record a remediation-required result and stop before implementation.

The verified defect is localized: `ProgressTracker.Initialize()` and `ProgressTrackerAsync.InitializeAsync()` assign the loading operation's `CancellationTokenSource` through `ProgressViewer.CancelSource`; that setter stores the source but does not enable `ButtonCancel`. `SetCancellationTokenSource(...)` performs both operations, and the existing `CancelPath_WhenInvoked_CancelsTokenSource` test verifies cancellation through that alternate setup path. The planned regression test must exercise the `CancelSource` property path and the actual enabled button click against the same source.

No manual bootstrap, user-performed validation, screenshot collection, or other manual step is authorized. Any unavailable command, missing numeric coverage value, failed threshold, unexpected scope expansion, or incomplete evidence artifact produces an automated blocked or remediation-required result, never PASS.

All evidence must be written under the explicitly named `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/`, `evidence/issue-updates/`, or `evidence/other/` subfolder of `docs/features/active/2026-07-16-progress-viewer-cancel-button-339`. Every command-specific Markdown evidence artifact below must contain `Timestamp:`, the exact `Command:`, numeric `EXIT_CODE:`, and `Output Summary:`. The fixed filename timestamp `2026-07-16T12-39` identifies this plan's evidence batch; each artifact's `Timestamp:` field must record its actual execution time in `yyyy-MM-ddTHH-mm` format. Only the successfully merged and repository-postprocessed Cobertura baseline/final XML is authoritative; per-assembly raw files are temporary inputs and the stale partial XML from the timed-out aggregate command is never a baseline.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read `AGENTS.md` in full as the standing repository instruction source.
  - Acceptance: the read covers repository tone, generated-file authority, and policy precedence; evidence is consolidated in P0-T5.

- [x] [P0-T2] Read the cross-language code-change policy in `AGENTS.md` after the standing instructions.
  - Acceptance: the read covers the bugfix test-first sequence, scope limits, and mandatory format-lint-type-test loop; evidence is consolidated in P0-T5.

- [x] [P0-T3] Read the cross-language unit-test policy in `AGENTS.md` after the code-change policy.
  - Acceptance: the read covers deterministic MSTest expectations, the prohibition on temporary files, repository coverage `>= 80%`, new/changed-code coverage `>= 90%`, and no changed-line regression; evidence is consolidated in P0-T5.

- [x] [P0-T4] Read `.agents/skills/csharp/SKILL.md` after the cross-language policies.
  - Acceptance: the read covers CSharpier, analyzer build, nullable/TreatWarningsAsErrors build, MSTest/Moq/FluentAssertions conventions, and the required toolchain order; evidence is consolidated in P0-T5.

- [x] [P0-T5] Record the completed policy reads in `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/phase0-instructions-read.md`.
  - Acceptance: the artifact contains `Timestamp:`, `Policy Order:`, and an explicit ordered list of `AGENTS.md` standing instructions, `AGENTS.md` cross-language code-change policy, `AGENTS.md` cross-language unit-test policy, and `.agents/skills/csharp/SKILL.md`; it also confirms that no policy file was modified.

- [x] [P0-T6] Verify the minor-audit requirements boundary and current branch baseline without changing files.
  - Command: `pwsh -NoProfile -Command '& { $feature = "docs/features/active/2026-07-16-progress-viewer-cancel-button-339"; $issue = Join-Path $feature "issue.md"; if (-not (Test-Path $issue)) { exit 1 }; $text = Get-Content $issue -Raw; if ($text -notmatch "(?m)^- Work Mode: minor-audit$" -or $text -notmatch "(?m)^## Acceptance Criteria$" -or (Test-Path (Join-Path $feature "spec.md")) -or (Test-Path (Join-Path $feature "user-story.md"))) { exit 1 }; git rev-parse --abbrev-ref HEAD; git rev-parse HEAD }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/minor-audit-boundary.2026-07-16T12-39.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records the branch, full HEAD SHA, `minor-audit`, the explicit AC section with three checkbox items, and confirmed absence of `spec.md` and `user-story.md`. Any mismatch stops execution before P1-T1.

- [x] [P0-T7] Run the baseline C# formatting command.
  - Command: `dotnet tool run csharpier format .`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/csharpier-baseline.2026-07-16T12-39.md`.
  - Acceptance: retain the existing failed-attempt and SDK-bootstrap sections in the artifact, append the corrected command retry and its result, and update the artifact's authoritative command and exit-code summary to `dotnet tool run csharpier format .` with `EXIT_CODE: 0`. The artifact reports whether files changed. Any formatter change outside the approved two-file implementation scope is a scope defect and stops execution before Phase 1.

- [x] [P0-T8] Run the baseline .NET analyzer build.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/analyzer-baseline.2026-07-16T12-39.md`.
  - Acceptance: `Output Summary:` records warning and error counts so Phase 2 can enforce zero new analyzer findings.

- [x] [P0-T9] Run the baseline compiler and nullable-analysis build.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/nullable-baseline.2026-07-16T12-39.md`.
  - Acceptance: `Output Summary:` records warning and error counts so Phase 2 can enforce zero new compiler or nullable diagnostics.

- [x] [P0-T10] Run deterministic full-scope coverage as eight bounded, isolated per-assembly MSTest collections; sum their current-run TRX counters; merge their raw reports; and publish one postprocessed baseline Cobertura artifact.
  - Command:

    ```powershell
    pwsh -NoProfile -ExecutionPolicy Bypass -Command '& {
      Set-StrictMode -Version Latest
      $ErrorActionPreference = "Stop"
      $repoRoot = (Resolve-Path ".").Path
      $evidenceRoot = Join-Path $repoRoot "docs\features\active\2026-07-16-progress-viewer-cancel-button-339\evidence\baseline"
      $outputPath = Join-Path $evidenceRoot "csharp-coverage-baseline.2026-07-16T12-39.cobertura.xml"
      $stagingOutputPath = "$outputPath.tmp"
      $scratchPath = [System.IO.Path]::GetFullPath((Join-Path $evidenceRoot "isolated-coverage-baseline.2026-07-16T12-39"))
      $evidencePrefix = [System.IO.Path]::GetFullPath($evidenceRoot).TrimEnd("\") + "\"
      if (-not $scratchPath.StartsWith($evidencePrefix, [System.StringComparison]::OrdinalIgnoreCase)) { throw "Scratch path escaped the canonical baseline evidence folder." }

      function Invoke-BoundedProcess {
        param(
          [Parameter(Mandatory = $true)][string]$FilePath,
          [Parameter(Mandatory = $true)][string[]]$Arguments,
          [Parameter(Mandatory = $true)][string]$StandardOutputPath,
          [Parameter(Mandatory = $true)][string]$StandardErrorPath,
          [Parameter(Mandatory = $true)][int]$TimeoutMilliseconds
        )

        $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = $FilePath
        $startInfo.WorkingDirectory = $repoRoot
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        foreach ($argument in $Arguments) { [void]$startInfo.ArgumentList.Add($argument) }

        $process = [System.Diagnostics.Process]::new()
        $process.StartInfo = $startInfo
        try {
          [void]$process.Start()
          $standardOutputTask = $process.StandardOutput.ReadToEndAsync()
          $standardErrorTask = $process.StandardError.ReadToEndAsync()
          $completed = $process.WaitForExit($TimeoutMilliseconds)
          if (-not $completed) { $process.Kill($true); $process.WaitForExit() }
          $standardOutput = $standardOutputTask.GetAwaiter().GetResult()
          $standardError = $standardErrorTask.GetAwaiter().GetResult()
          Set-Content -LiteralPath $StandardOutputPath -Value $standardOutput -Encoding utf8 -NoNewline
          Set-Content -LiteralPath $StandardErrorPath -Value $standardError -Encoding utf8 -NoNewline
          $exitCode = $process.ExitCode
        }
        finally {
          $process.Dispose()
        }

        if (-not $completed) { throw "Process timed out after $TimeoutMilliseconds ms: $FilePath" }
        if ($exitCode -ne 0) { throw "Process exited with code ${exitCode}: $FilePath" }
      }

      if (Test-Path -LiteralPath $outputPath) { Remove-Item -LiteralPath $outputPath -Force }
      if (Test-Path -LiteralPath $stagingOutputPath) { Remove-Item -LiteralPath $stagingOutputPath -Force }
      if (Test-Path -LiteralPath $scratchPath) { Remove-Item -LiteralPath $scratchPath -Recurse -Force }
      [void](New-Item -ItemType Directory -Path $scratchPath -Force)

      try {
        $vswherePath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        if (-not (Test-Path -LiteralPath $vswherePath)) { throw "vswhere.exe was not found." }
        $vstestPath = & $vswherePath -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
        if (-not $vstestPath) { throw "vstest.console.exe was not found through vswhere.exe." }
        $dotnetCoveragePath = (Get-Command "dotnet-coverage" -ErrorAction Stop).Source
        $runSettingsPath = (Resolve-Path "scripts/vscode/TaskMaster.cli.runsettings").Path
        $coverageConfigPath = (Resolve-Path "coverage.config").Path
        $helperPath = (Resolve-Path "scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1").Path
        $assemblies = @(Get-ChildItem -Path $repoRoot -Recurse -File -Filter "*.Test.dll" | Where-Object { $_.FullName -match "\\bin\\Debug\\" -and $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\ref\\" } | Sort-Object -Property FullName -Unique)
        if ($assemblies.Count -ne 8) { throw "Expected 8 Debug test assemblies but discovered $($assemblies.Count)." }

        $rawCoveragePaths = [System.Collections.Generic.List[string]]::new()
        $summedTotal = 0
        $summedPassed = 0
        $summedFailed = 0
        $summedSkipped = 0

        foreach ($assembly in $assemblies) {
          $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($assembly.Name)
          $rawCoveragePath = Join-Path $scratchPath "$assemblyName.raw.cobertura.xml"
          $trxPath = Join-Path $scratchPath "$assemblyName.trx"
          $collectArguments = @(
            "collect", "--output", $rawCoveragePath, "--output-format", "cobertura", "--settings", $coverageConfigPath, "--",
            $vstestPath, $assembly.FullName, "/Settings:$runSettingsPath", "/InIsolation", "/TestCaseFilter:TestCategory!=LiveOutlook",
            "/ResultsDirectory:$scratchPath", "/Logger:trx;LogFileName=$assemblyName.trx"
          )
          Invoke-BoundedProcess -FilePath $dotnetCoveragePath -Arguments $collectArguments -StandardOutputPath (Join-Path $scratchPath "$assemblyName.collect.stdout.log") -StandardErrorPath (Join-Path $scratchPath "$assemblyName.collect.stderr.log") -TimeoutMilliseconds 600000
          if (-not (Test-Path -LiteralPath $rawCoveragePath) -or -not (Test-Path -LiteralPath $trxPath)) { throw "Coverage or TRX output is missing for $assemblyName." }

          [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
          $counters = $trx.TestRun.ResultSummary.Counters
          if (-not $counters) { throw "TRX counters are missing for $assemblyName." }
          $assemblyTotal = [int]$counters.total
          $assemblyPassed = [int]$counters.passed
          $assemblyFailed = [int]$counters.failed
          $assemblySkipped = [int]$counters.notExecuted
          $assemblyExecuted = [int]$counters.executed
          if ($assemblyFailed -ne 0 -or $assemblySkipped -ne 0 -or $assemblyExecuted -ne $assemblyPassed) { throw "The isolated test run did not fully pass for $assemblyName." }
          $summedTotal += $assemblyTotal
          $summedPassed += $assemblyPassed
          $summedFailed += $assemblyFailed
          $summedSkipped += $assemblySkipped
          $rawCoveragePaths.Add($rawCoveragePath)
          Write-Output "ASSEMBLY_RESULT=$assemblyName;TOTAL=$assemblyTotal;PASSED=$assemblyPassed;FAILED=$assemblyFailed;SKIPPED=$assemblySkipped"
        }

        if ($summedTotal -ne 5467 -or $summedPassed -ne 5467 -or $summedFailed -ne 0 -or $summedSkipped -ne 0) { throw "Unexpected pre-change summed test totals." }
        $mergedRawPath = Join-Path $scratchPath "merged.raw.cobertura.xml"
        $mergeArguments = @("merge", "--output", $mergedRawPath, "--output-format", "cobertura") + $rawCoveragePaths.ToArray()
        Invoke-BoundedProcess -FilePath $dotnetCoveragePath -Arguments $mergeArguments -StandardOutputPath (Join-Path $scratchPath "merge.stdout.log") -StandardErrorPath (Join-Path $scratchPath "merge.stderr.log") -TimeoutMilliseconds 600000
        if (-not (Test-Path -LiteralPath $mergedRawPath)) { throw "Merged raw Cobertura output is missing." }

        . $helperPath
        $projectNames = @(Get-KoverageProjectAllowlist -RepoRoot $repoRoot)
        $processedXml = ConvertTo-KoverageCoberturaXml -XmlContent (Get-Content -LiteralPath $mergedRawPath -Raw -Encoding utf8) -RepoRoot $repoRoot -ProjectNames $projectNames
        Set-Content -LiteralPath $stagingOutputPath -Value $processedXml -Encoding utf8 -NoNewline
        [xml]$coverage = Get-Content -LiteralPath $stagingOutputPath -Raw
        $repositoryCoverage = [math]::Round([double]$coverage.coverage."line-rate" * 100, 2)
        $targetPath = "UtilitiesCS\Threading\ProgressViewer.cs"
        $targetClass = $coverage.SelectNodes("//class") | Where-Object { $_.filename.Replace("/", "\") -eq $targetPath } | Select-Object -First 1
        if (-not $targetClass) { throw "ProgressViewer.cs is absent from the merged coverage artifact." }
        $targetCoverage = [math]::Round([double]$targetClass."line-rate" * 100, 2)
        if ($repositoryCoverage -lt 80) { throw "Repository coverage $repositoryCoverage% is below 80%." }

        Remove-Item -LiteralPath $scratchPath -Recurse -Force
        Move-Item -LiteralPath $stagingOutputPath -Destination $outputPath -Force
        Write-Output "ASSEMBLY_COUNT=$($assemblies.Count)"
        Write-Output "SUMMED_CURRENT_RUN_TOTAL=$summedTotal"
        Write-Output "SUMMED_CURRENT_RUN_PASSED=$summedPassed"
        Write-Output "SUMMED_CURRENT_RUN_FAILED=$summedFailed"
        Write-Output "SUMMED_CURRENT_RUN_SKIPPED=$summedSkipped"
        Write-Output "REPOSITORY_LINE_COVERAGE=$repositoryCoverage%"
        Write-Output "PROGRESSVIEWER_LINE_COVERAGE=$targetCoverage%"
      }
      catch {
        if (Test-Path -LiteralPath $outputPath) { Remove-Item -LiteralPath $outputPath -Force }
        if (Test-Path -LiteralPath $stagingOutputPath) { Remove-Item -LiteralPath $stagingOutputPath -Force }
        Write-Error -ErrorAction Continue $_
        exit 1
      }
    }'
    ```

  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/vstest-coverage-baseline.2026-07-16T12-39.md`; authoritative merged postprocessed coverage: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/csharp-coverage-baseline.2026-07-16T12-39.cobertura.xml`.
  - Acceptance: retain the two aggregate-timeout attempts and bounded diagnostic references in the Markdown artifact, delete the stale partial XML before the revised command starts, and append the isolated-collection result. `EXIT_CODE: 0`; exactly eight named assembly rows sum to `5,467` total, `5,467` passed, `0` failed, and `0` skipped from the current coverage run; the temporary per-assembly reports and TRX files are merged then removed; exactly one authoritative merged postprocessed baseline XML is atomically published; `Output Summary:` records numeric first-party repository line coverage `>= 80%` and numeric `UtilitiesCS/Threading/ProgressViewer.cs` line coverage. A missing assembly/output/value, per-assembly timeout/failure, total mismatch, merge/postprocess failure, or threshold failure records remediation-required state and stops before Phase 1. The stale 11.90% aggregate partial XML is not baseline evidence.

---

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] Through `atomic-executor`, delegate P1-T2 through P1-T10 to the configured `csharp-typed-engineer` with this exact plan path and the two-file scope above.
  - Acceptance: the engineer acknowledges `AGENTS.md`, `.agents/skills/csharp/SKILL.md`, `csharp-qa-gate`, `acceptance-criteria-tracking`, and the canonical evidence rules; it must stop and return a scope defect rather than edit a third production or test file.

- [x] [P1-T2] Add the deterministic MSTest regression test `CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick` to `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` before changing production code.
  - Test design: follow the existing STA and `SynchronizationContext` setup; construct a real `ProgressViewer`; resolve the private `ButtonCancel` field through `BindingFlags.NonPublic | BindingFlags.Instance` as a `System.Windows.Forms.Button`; assign a non-null `CancellationTokenSource` through `viewer.CancelSource`; call `viewer.Show()` so the form and button are selectable; assert the resolved button's `Enabled` property is true; call `button.PerformClick()`; assert the captured token from that same source has `IsCancellationRequested == true`; close or dispose the viewer in `finally` if the expected click did not already close it; and restore the prior synchronization context. Use FluentAssertions and no temporary files, external services, sleeps, retries, or live Outlook dependencies.
  - Acceptance: only the test file is modified at this point, the new test covers both enabled-state and same-source cancellation, and the file remains below 500 lines.

- [x] [P1-T3] Build the solution after adding the regression test and before changing production code.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/regression-testing/regression-build-before.2026-07-16T12-39.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` confirms the new test compiled while the production setter remained unchanged.

- [x] [P1-T4] [expect-fail] Run only the new regression test against the unchanged `CancelSource` setter.
  - Command: `pwsh -NoProfile -Command '& { $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; if (-not (Test-Path $vswhere)) { exit 1 }; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; if (-not $vstest) { exit 1 }; $runSettings = (Resolve-Path "scripts/vscode/TaskMaster.cli.runsettings").Path; & $vstest "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" "/Settings:$runSettings" "/InIsolation" "/TestCaseFilter:FullyQualifiedName~CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick"; exit $LASTEXITCODE }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/regression-testing/fail-before-339.2026-07-16T12-39.md`.
  - Acceptance: the artifact contains a non-zero numeric `EXIT_CODE:` and `Output Summary:` identifies the new test's disabled-button assertion as the expected failure. A missing test, discovery failure, build failure, unrelated exception, or passing result is not valid fail-before evidence and requires remediation before P1-T5.

- [x] [P1-T5] Implement the minimal targeted production fix in `UtilitiesCS/Threading/ProgressViewer.cs`.
  - Change: expand only the `CancelSource` setter so it assigns `_cancelSource = value` and immediately sets `ButtonCancel.Enabled = value != null`. Preserve `SetCancellationTokenSource(...)`, `CancelButton_Click(...)`, tracker call sites, public signatures, and all unrelated code.
  - Acceptance: the loading path's non-null source enables the button, assigning null does not leave the control enabled, no new dependency or public API is introduced, and the production file remains below 500 lines.

- [x] [P1-T6] Rebuild the solution after the production fix.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/regression-testing/regression-build-after.2026-07-16T12-39.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records zero build errors.

- [x] [P1-T7] Re-run the new regression test and confirm pass-after behavior.
  - Command: `pwsh -NoProfile -Command '& { $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; if (-not (Test-Path $vswhere)) { exit 1 }; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; if (-not $vstest) { exit 1 }; $runSettings = (Resolve-Path "scripts/vscode/TaskMaster.cli.runsettings").Path; & $vstest "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" "/Settings:$runSettings" "/InIsolation" "/TestCaseFilter:FullyQualifiedName~CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick"; exit $LASTEXITCODE }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/regression-testing/pass-after-339.2026-07-16T12-39.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records one passed and zero failed tests and confirms the same source assigned through `CancelSource` was canceled by `PerformClick()`.

- [x] [P1-T8] Verify the implementation diff is confined to the approved source and test files.
  - Command: `pwsh -NoProfile -Command '& { $approved = @("UtilitiesCS/Threading/ProgressViewer.cs", "UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs"); $changed = @(git status --short --untracked-files=all -- "*.cs" | ForEach-Object { $_.Substring(3).Replace("\", "/") }); $changed | ForEach-Object { Write-Output $_ }; $unexpected = @($changed | Where-Object { $_ -notin $approved }); $missing = @($approved | Where-Object { $_ -notin $changed }); if ($unexpected.Count -gt 0 -or $missing.Count -gt 0) { exit 1 } }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/regression-testing/implementation-scope-339.2026-07-16T12-39.md`.
  - Acceptance: `Output Summary:` names exactly `UtilitiesCS/Threading/ProgressViewer.cs` and `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`, confirms the production diff is limited to the property setter, and confirms no third implementation file was changed. Any concrete need for a third file is a scope defect returned to the orchestrator before further edits.

- [x] [P1-T9] Check off the first issue #339 acceptance criterion after P1-T7 verifies the enabled loading-state behavior.
  - File: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/issue.md`.
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/issue-updates/ac1-enabled-state-339.2026-07-16T12-39.md`.
  - Acceptance: change only the first AC checkbox from `[ ]` to `[x]`, preserve its text, and map it to P1-T4 and P1-T7 evidence.

- [x] [P1-T10] Check off the second issue #339 acceptance criterion after P1-T7 verifies same-source cancellation.
  - File: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/issue.md`.
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/issue-updates/ac2-cancellation-339.2026-07-16T12-39.md`.
  - Acceptance: change only the second AC checkbox from `[ ]` to `[x]`, preserve its text, and map it to the pass-after assertion proving cancellation of the same configured source; leave the third AC unchecked pending Phase 2.

---

### Phase 2 — Final QC Loop

P2-T1 through P2-T4 are unconditional and must run in the stated order. No task permits `SKIPPED`. If any command fails or P2-T1 changes a C# file, fix the issue within approved scope and restart the final QC loop from P2-T1. Completion requires all four commands to pass without errors in one final pass.

The completed P0-T10 baseline remains authoritative and must not be rerun after the production and regression-test changes: `5,467` total, `5,467` passed, `0` failed, `0` skipped, `83.44%` first-party repository line coverage, and `100%` `UtilitiesCS/Threading/ProgressViewer.cs` line coverage. That baseline used the original CLI runsettings with `Workers=0` and must not be described retroactively as single-worker. P2-T4 changes only MSTest scheduling to `Workers=1` with `Scope=ClassLevel`; test-assembly selection, `coverage.config` instrumentation, `/InIsolation`, the `TestCategory!=LiveOutlook` filter, TRX validation, raw-report merge, first-party postprocessing, and atomic final publication remain identical, so P2-T5 may compare the existing baseline and revised final artifact without recapturing a post-change baseline.

- [x] [P2-T1] Run final C# formatting.
  - Command: `dotnet tool run csharpier format .`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/csharpier-final.2026-07-16T12-39.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` states whether files changed. If files changed, restart at P2-T1 after preserving the evidence.

- [x] [P2-T2] Run the final .NET analyzer build.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/analyzer-final.2026-07-16T12-39.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records warning/error counts and proves zero new analyzer findings relative to P0-T8.

- [x] [P2-T3] Run the final compiler and nullable-analysis build.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/nullable-final.2026-07-16T12-39.md`.
  - Acceptance: `EXIT_CODE: 0`; `Output Summary:` records warning/error counts and proves zero new compiler or nullable diagnostics relative to P0-T9.

- [x] [P2-T4] Create and retain the canonical single-worker runsettings artifact, then run deterministic full-scope coverage as eight bounded, isolated per-assembly MSTest collections; sum their current-run TRX counters; merge their raw reports; and publish one postprocessed final Cobertura artifact.
  - Command:

    ```powershell
    pwsh -NoProfile -ExecutionPolicy Bypass -Command '& {
      Set-StrictMode -Version Latest
      $ErrorActionPreference = "Stop"
      $repoRoot = (Resolve-Path ".").Path
      $evidenceRoot = Join-Path $repoRoot "docs\features\active\2026-07-16-progress-viewer-cancel-button-339\evidence\qa-gates"
      $otherEvidenceRoot = Join-Path $repoRoot "docs\features\active\2026-07-16-progress-viewer-cancel-button-339\evidence\other"
      $outputPath = Join-Path $evidenceRoot "csharp-coverage-final.2026-07-16T12-39.cobertura.xml"
      $stagingOutputPath = "$outputPath.tmp"
      $runSettingsPath = [System.IO.Path]::GetFullPath((Join-Path $otherEvidenceRoot "p2-t4-single-worker-classlevel.2026-07-16T15-49.runsettings"))
      $runSettingsStagingPath = "$runSettingsPath.tmp"
      $scratchPath = [System.IO.Path]::GetFullPath((Join-Path $evidenceRoot "isolated-coverage-final.2026-07-16T12-39"))
      $evidencePrefix = [System.IO.Path]::GetFullPath($evidenceRoot).TrimEnd("\") + "\"
      $otherEvidencePrefix = [System.IO.Path]::GetFullPath($otherEvidenceRoot).TrimEnd("\") + "\"
      if (-not $scratchPath.StartsWith($evidencePrefix, [System.StringComparison]::OrdinalIgnoreCase)) { throw "Scratch path escaped the canonical QA-gate evidence folder." }
      if (-not $runSettingsPath.StartsWith($otherEvidencePrefix, [System.StringComparison]::OrdinalIgnoreCase)) { throw "Runsettings path escaped the canonical other-evidence folder." }

      function Invoke-BoundedProcess {
        param(
          [Parameter(Mandatory = $true)][string]$FilePath,
          [Parameter(Mandatory = $true)][string[]]$Arguments,
          [Parameter(Mandatory = $true)][string]$StandardOutputPath,
          [Parameter(Mandatory = $true)][string]$StandardErrorPath,
          [Parameter(Mandatory = $true)][int]$TimeoutMilliseconds
        )

        $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = $FilePath
        $startInfo.WorkingDirectory = $repoRoot
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        foreach ($argument in $Arguments) { [void]$startInfo.ArgumentList.Add($argument) }

        $process = [System.Diagnostics.Process]::new()
        $process.StartInfo = $startInfo
        try {
          [void]$process.Start()
          $standardOutputTask = $process.StandardOutput.ReadToEndAsync()
          $standardErrorTask = $process.StandardError.ReadToEndAsync()
          $completed = $process.WaitForExit($TimeoutMilliseconds)
          if (-not $completed) { $process.Kill($true); $process.WaitForExit() }
          $standardOutput = $standardOutputTask.GetAwaiter().GetResult()
          $standardError = $standardErrorTask.GetAwaiter().GetResult()
          Set-Content -LiteralPath $StandardOutputPath -Value $standardOutput -Encoding utf8 -NoNewline
          Set-Content -LiteralPath $StandardErrorPath -Value $standardError -Encoding utf8 -NoNewline
          $exitCode = $process.ExitCode
        }
        finally {
          $process.Dispose()
        }

        if (-not $completed) { throw "Process timed out after $TimeoutMilliseconds ms: $FilePath" }
        if ($exitCode -ne 0) { throw "Process exited with code ${exitCode}: $FilePath" }
      }

      if (Test-Path -LiteralPath $outputPath) { Remove-Item -LiteralPath $outputPath -Force }
      if (Test-Path -LiteralPath $stagingOutputPath) { Remove-Item -LiteralPath $stagingOutputPath -Force }
      if (Test-Path -LiteralPath $scratchPath) { Remove-Item -LiteralPath $scratchPath -Recurse -Force }
      [void](New-Item -ItemType Directory -Path $scratchPath -Force)
      [void](New-Item -ItemType Directory -Path $otherEvidenceRoot -Force)

      try {
        $sharedRunSettingsPath = (Resolve-Path "scripts/vscode/TaskMaster.cli.runsettings").Path
        $sharedRunSettingsHash = (Get-FileHash -LiteralPath $sharedRunSettingsPath -Algorithm SHA256).Hash
        $runSettingsContent = @(
          "<?xml version=`"1.0`" encoding=`"utf-8`"?>",
          "<RunSettings>",
          "  <MSTest>",
          "    <Parallelize>",
          "      <Workers>1</Workers>",
          "      <Scope>ClassLevel</Scope>",
          "    </Parallelize>",
          "  </MSTest>",
          "</RunSettings>"
        ) -join [System.Environment]::NewLine
        if (Test-Path -LiteralPath $runSettingsStagingPath) { Remove-Item -LiteralPath $runSettingsStagingPath -Force }
        Set-Content -LiteralPath $runSettingsStagingPath -Value $runSettingsContent -Encoding utf8 -NoNewline
        [xml]$persistentRunSettings = Get-Content -LiteralPath $runSettingsStagingPath -Raw
        if ([int]$persistentRunSettings.RunSettings.MSTest.Parallelize.Workers -ne 1 -or [string]$persistentRunSettings.RunSettings.MSTest.Parallelize.Scope -ne "ClassLevel") { throw "Persistent runsettings did not validate as Workers=1 and Scope=ClassLevel." }
        Move-Item -LiteralPath $runSettingsStagingPath -Destination $runSettingsPath -Force

        $vswherePath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        if (-not (Test-Path -LiteralPath $vswherePath)) { throw "vswhere.exe was not found." }
        $vstestPath = & $vswherePath -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
        if (-not $vstestPath) { throw "vstest.console.exe was not found through vswhere.exe." }
        $dotnetCoveragePath = (Get-Command "dotnet-coverage" -ErrorAction Stop).Source
        $coverageConfigPath = (Resolve-Path "coverage.config").Path
        $helperPath = (Resolve-Path "scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1").Path
        $assemblies = @(Get-ChildItem -Path $repoRoot -Recurse -File -Filter "*.Test.dll" | Where-Object { $_.FullName -match "\\bin\\Debug\\" -and $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\ref\\" } | Sort-Object -Property FullName -Unique)
        if ($assemblies.Count -ne 8) { throw "Expected 8 Debug test assemblies but discovered $($assemblies.Count)." }

        $rawCoveragePaths = [System.Collections.Generic.List[string]]::new()
        $summedTotal = 0
        $summedPassed = 0
        $summedFailed = 0
        $summedSkipped = 0

        foreach ($assembly in $assemblies) {
          $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($assembly.Name)
          $rawCoveragePath = Join-Path $scratchPath "$assemblyName.raw.cobertura.xml"
          $trxPath = Join-Path $scratchPath "$assemblyName.trx"
          $collectArguments = @(
            "collect", "--output", $rawCoveragePath, "--output-format", "cobertura", "--settings", $coverageConfigPath, "--",
            $vstestPath, $assembly.FullName, "/Settings:$runSettingsPath", "/InIsolation", "/TestCaseFilter:TestCategory!=LiveOutlook",
            "/ResultsDirectory:$scratchPath", "/Logger:trx;LogFileName=$assemblyName.trx"
          )
          Invoke-BoundedProcess -FilePath $dotnetCoveragePath -Arguments $collectArguments -StandardOutputPath (Join-Path $scratchPath "$assemblyName.collect.stdout.log") -StandardErrorPath (Join-Path $scratchPath "$assemblyName.collect.stderr.log") -TimeoutMilliseconds 600000
          if (-not (Test-Path -LiteralPath $rawCoveragePath) -or -not (Test-Path -LiteralPath $trxPath)) { throw "Coverage or TRX output is missing for $assemblyName." }

          [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
          $counters = $trx.TestRun.ResultSummary.Counters
          if (-not $counters) { throw "TRX counters are missing for $assemblyName." }
          $assemblyTotal = [int]$counters.total
          $assemblyPassed = [int]$counters.passed
          $assemblyFailed = [int]$counters.failed
          $assemblySkipped = [int]$counters.notExecuted
          $assemblyExecuted = [int]$counters.executed
          if ($assemblyFailed -ne 0 -or $assemblySkipped -ne 0 -or $assemblyExecuted -ne $assemblyPassed) { throw "The isolated test run did not fully pass for $assemblyName." }
          $summedTotal += $assemblyTotal
          $summedPassed += $assemblyPassed
          $summedFailed += $assemblyFailed
          $summedSkipped += $assemblySkipped
          $rawCoveragePaths.Add($rawCoveragePath)
          Write-Output "ASSEMBLY_RESULT=$assemblyName;TOTAL=$assemblyTotal;PASSED=$assemblyPassed;FAILED=$assemblyFailed;SKIPPED=$assemblySkipped"
        }

        if ($summedTotal -ne 5468 -or $summedPassed -ne 5468 -or $summedFailed -ne 0 -or $summedSkipped -ne 0) { throw "Unexpected post-change summed test totals." }
        $mergedRawPath = Join-Path $scratchPath "merged.raw.cobertura.xml"
        $mergeArguments = @("merge", "--output", $mergedRawPath, "--output-format", "cobertura") + $rawCoveragePaths.ToArray()
        Invoke-BoundedProcess -FilePath $dotnetCoveragePath -Arguments $mergeArguments -StandardOutputPath (Join-Path $scratchPath "merge.stdout.log") -StandardErrorPath (Join-Path $scratchPath "merge.stderr.log") -TimeoutMilliseconds 600000
        if (-not (Test-Path -LiteralPath $mergedRawPath)) { throw "Merged raw Cobertura output is missing." }

        . $helperPath
        $projectNames = @(Get-KoverageProjectAllowlist -RepoRoot $repoRoot)
        $processedXml = ConvertTo-KoverageCoberturaXml -XmlContent (Get-Content -LiteralPath $mergedRawPath -Raw -Encoding utf8) -RepoRoot $repoRoot -ProjectNames $projectNames
        Set-Content -LiteralPath $stagingOutputPath -Value $processedXml -Encoding utf8 -NoNewline
        [xml]$coverage = Get-Content -LiteralPath $stagingOutputPath -Raw
        $repositoryCoverage = [math]::Round([double]$coverage.coverage."line-rate" * 100, 2)
        $targetPath = "UtilitiesCS\Threading\ProgressViewer.cs"
        $targetClass = $coverage.SelectNodes("//class") | Where-Object { $_.filename.Replace("/", "\") -eq $targetPath } | Select-Object -First 1
        if (-not $targetClass) { throw "ProgressViewer.cs is absent from the merged coverage artifact." }
        $targetCoverage = [math]::Round([double]$targetClass."line-rate" * 100, 2)
        if ($repositoryCoverage -lt 80) { throw "Repository coverage $repositoryCoverage% is below 80%." }
        if ((Get-FileHash -LiteralPath $sharedRunSettingsPath -Algorithm SHA256).Hash -ne $sharedRunSettingsHash) { throw "Shared TaskMaster.cli.runsettings changed during P2-T4." }

        Remove-Item -LiteralPath $scratchPath -Recurse -Force
        Move-Item -LiteralPath $stagingOutputPath -Destination $outputPath -Force
        Write-Output "RUNSETTINGS_PATH=$runSettingsPath"
        Write-Output "MSTEST_WORKERS=1"
        Write-Output "MSTEST_SCOPE=ClassLevel"
        Write-Output "ASSEMBLY_COUNT=$($assemblies.Count)"
        Write-Output "SUMMED_CURRENT_RUN_TOTAL=$summedTotal"
        Write-Output "SUMMED_CURRENT_RUN_PASSED=$summedPassed"
        Write-Output "SUMMED_CURRENT_RUN_FAILED=$summedFailed"
        Write-Output "SUMMED_CURRENT_RUN_SKIPPED=$summedSkipped"
        Write-Output "REPOSITORY_LINE_COVERAGE=$repositoryCoverage%"
        Write-Output "PROGRESSVIEWER_LINE_COVERAGE=$targetCoverage%"
      }
      catch {
        if (Test-Path -LiteralPath $outputPath) { Remove-Item -LiteralPath $outputPath -Force }
        if (Test-Path -LiteralPath $stagingOutputPath) { Remove-Item -LiteralPath $stagingOutputPath -Force }
        if (Test-Path -LiteralPath $runSettingsStagingPath) { Remove-Item -LiteralPath $runSettingsStagingPath -Force }
        Write-Error -ErrorAction Continue $_
        exit 1
      }
    }'
    ```

  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/vstest-coverage-final.2026-07-16T12-39.md`; authoritative merged postprocessed coverage: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/csharp-coverage-final.2026-07-16T12-39.cobertura.xml`; retained scheduler configuration: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/other/p2-t4-single-worker-classlevel.2026-07-16T15-49.runsettings`; remediation source: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/p2-t4-plan-revision-request.2026-07-16T15-49.md`.
  - Acceptance: retain the two `QuickFiler.Test` timeout attempts and the corrected in-scope `UtilitiesCS.Test` harness failure in the Markdown evidence, then append the exact revised command and result. The persistent runsettings artifact exists under canonical `evidence/other`, contains exactly `Workers=1` and `Scope=ClassLevel`, is used by all eight VSTest invocations, and remains after scratch cleanup; `scripts/vscode/TaskMaster.cli.runsettings` retains its original SHA-256 hash and is not edited. `EXIT_CODE: 0`; exactly eight named assembly rows sum to `5,468` total, `5,468` passed, `0` failed, and `0` skipped from the current final coverage run; every assembly retains `/InIsolation`, `TestCategory!=LiveOutlook`, `coverage.config`, TRX validation, and the `600,000` ms process bound; temporary per-assembly reports and TRX files are merged then removed; exactly one authoritative merged postprocessed final XML is atomically published; `Output Summary:` records the runsettings path, `MSTEST_WORKERS=1`, `MSTEST_SCOPE=ClassLevel`, and numeric first-party repository line coverage `>= 80%` plus numeric `UtilitiesCS/Threading/ProgressViewer.cs` line coverage. A missing assembly/output/value, per-assembly timeout/failure, total mismatch, merge/postprocess failure, shared-runsettings mutation, or threshold failure restarts the final QC loop from P2-T1 after correction. No aggregate partial, per-assembly raw, or scratch XML is final evidence.

- [x] [P2-T5] Calculate and enforce repository, target-file, and changed-production-line coverage deltas from the authoritative scheduling-comparable baseline and final Cobertura XML files.
  - Command: `pwsh -NoProfile -Command '& { [xml]$baseline = Get-Content "docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/csharp-coverage-baseline.2026-07-16T12-39.cobertura.xml"; [xml]$final = Get-Content "docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/csharp-coverage-final.2026-07-16T12-39.cobertura.xml"; $target = "UtilitiesCS\Threading\ProgressViewer.cs"; $baselineClass = $baseline.SelectNodes("//class") | Where-Object { $_.filename.Replace("/", "\") -eq $target } | Select-Object -First 1; $finalClass = $final.SelectNodes("//class") | Where-Object { $_.filename.Replace("/", "\") -eq $target } | Select-Object -First 1; if (-not $baselineClass -or -not $finalClass) { exit 1 }; $baselineRepo = [math]::Round([double]$baseline.coverage."line-rate" * 100, 2); $finalRepo = [math]::Round([double]$final.coverage."line-rate" * 100, 2); $baselineFile = [math]::Round([double]$baselineClass."line-rate" * 100, 2); $finalFile = [math]::Round([double]$finalClass."line-rate" * 100, 2); $hits = @{}; foreach ($line in $finalClass.SelectNodes("./lines/line")) { $hits[[int]$line.number] = [int]$line.hits }; $ranges = [System.Collections.ArrayList]::new(); foreach ($line in (git diff --unified=0 -- UtilitiesCS/Threading/ProgressViewer.cs)) { if ($line -match "^@@ -\d+(?:,\d+)? \+(\d+)(?:,(\d+))? @@") { $start = [int]$Matches[1]; $count = if ($Matches[2]) { [int]$Matches[2] } else { 1 }; if ($count -gt 0) { [void]$ranges.Add([pscustomobject]@{ Start = $start; End = $start + $count - 1 }) } } }; $changedCovered = 0; $changedInstrumented = 0; foreach ($range in $ranges) { for ($number = $range.Start; $number -le $range.End; $number++) { if ($hits.ContainsKey($number)) { $changedInstrumented++; if ($hits[$number] -gt 0) { $changedCovered++ } } } }; if ($changedInstrumented -eq 0) { exit 1 }; $changedCoverage = [math]::Round(($changedCovered / $changedInstrumented) * 100, 2); Write-Output "Baseline Repository Line Coverage: $baselineRepo%"; Write-Output "Final Repository Line Coverage: $finalRepo%"; Write-Output "Baseline ProgressViewer Line Coverage: $baselineFile%"; Write-Output "Final ProgressViewer Line Coverage: $finalFile%"; Write-Output "Changed Instrumented Lines Covered: $changedCovered/$changedInstrumented"; Write-Output "Changed Production Line Coverage: $changedCoverage%"; if ($finalRepo -lt 80 -or $finalRepo -lt $baselineRepo -or $finalFile -lt $baselineFile -or $changedCoverage -lt 90) { exit 1 } }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/coverage-delta-339.2026-07-16T12-39.md`.
  - Acceptance: the baseline input remains the completed P0-T10 artifact with `5,467` passing tests, `83.44%` repository coverage, and `100%` `ProgressViewer.cs` coverage; do not recapture it from changed code or label it single-worker. The final input is the successful P2-T4 artifact produced with the retained `Workers=1`, `Scope=ClassLevel` runsettings. These artifacts are scheduling-comparable because test-assembly selection, `coverage.config` instrumentation, `/InIsolation`, the `TestCategory!=LiveOutlook` filter, TRX validation, raw-report merge, and first-party postprocessing are identical; only MSTest worker scheduling differs. Inputs are exclusively the atomically published merged postprocessed XML artifacts, never an aggregate partial, per-assembly raw, staging, or scratch XML. `EXIT_CODE: 0`; `Output Summary:` records all six numeric values emitted by the command and confirms final repository coverage `>= 80%`, repository coverage did not regress from `83.44%`, `ProgressViewer.cs` coverage did not regress from `100%`, and changed production line coverage is `>= 90%`. Missing values or a failed threshold is remediation-required and cannot be reported as PASS.

- [x] [P2-T6] Compare the preserved P0-T10 and single-worker P2-T4 current-run TRX totals and record the zero-regression result.
  - Command: `pwsh -NoProfile -Command '& { $baselineText = Get-Content "docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/vstest-coverage-baseline.2026-07-16T12-39.md" -Raw; $finalText = Get-Content "docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/vstest-coverage-final.2026-07-16T12-39.md" -Raw; function Read-CurrentRunValue { param([string]$Text, [string]$Name); $match = [regex]::Match($Text, "(?m)^" + [regex]::Escape($Name) + "\s*=\s*(\d+)\s*$"); if (-not $match.Success) { throw "Missing $Name in coverage evidence." }; return [int]$match.Groups[1].Value }; $baselineTotal = Read-CurrentRunValue $baselineText "SUMMED_CURRENT_RUN_TOTAL"; $baselinePassed = Read-CurrentRunValue $baselineText "SUMMED_CURRENT_RUN_PASSED"; $baselineFailed = Read-CurrentRunValue $baselineText "SUMMED_CURRENT_RUN_FAILED"; $baselineSkipped = Read-CurrentRunValue $baselineText "SUMMED_CURRENT_RUN_SKIPPED"; $finalTotal = Read-CurrentRunValue $finalText "SUMMED_CURRENT_RUN_TOTAL"; $finalPassed = Read-CurrentRunValue $finalText "SUMMED_CURRENT_RUN_PASSED"; $finalFailed = Read-CurrentRunValue $finalText "SUMMED_CURRENT_RUN_FAILED"; $finalSkipped = Read-CurrentRunValue $finalText "SUMMED_CURRENT_RUN_SKIPPED"; Write-Output "BASELINE_TOTAL=$baselineTotal"; Write-Output "BASELINE_PASSED=$baselinePassed"; Write-Output "BASELINE_FAILED=$baselineFailed"; Write-Output "BASELINE_SKIPPED=$baselineSkipped"; Write-Output "FINAL_TOTAL=$finalTotal"; Write-Output "FINAL_PASSED=$finalPassed"; Write-Output "FINAL_FAILED=$finalFailed"; Write-Output "FINAL_SKIPPED=$finalSkipped"; if ($baselineTotal -ne 5467 -or $baselinePassed -ne 5467 -or $baselineFailed -ne 0 -or $baselineSkipped -ne 0 -or $finalTotal -ne 5468 -or $finalPassed -ne 5468 -or $finalFailed -ne 0 -or $finalSkipped -ne 0 -or $finalTotal -ne ($baselineTotal + 1) -or $finalPassed -ne ($baselinePassed + 1)) { exit 1 } }'`
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/test-delta-339.2026-07-16T12-39.md`.
  - Acceptance: the command reads only the current-run summed TRX labels from the preserved P0-T10 evidence and revised single-worker P2-T4 evidence, not historical aggregate-attempt summaries. The scheduling-only difference does not change the eight selected assemblies, test filter, or expected test universe. The artifact contains `Timestamp:`, the exact command, `EXIT_CODE: 0`, and `Output Summary:` showing baseline `5,467/5,467/0/0`, final `5,468/5,468/0/0`, zero new failing or skipped tests, and exactly one additional passing test confirming inclusion of the new regression test.

- [x] [P2-T7] Check off the third issue #339 acceptance criterion only after P2-T1 through P2-T6 pass in one final loop.
  - File: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/issue.md`.
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/issue-updates/ac3-toolchain-339.2026-07-16T12-39.md`.
  - Acceptance: change only the third AC checkbox from `[ ]` to `[x]`, preserve its text, and map it to fail-before, pass-after, final toolchain, and coverage-delta evidence.

- [x] [P2-T8] Record final minor-audit readiness and acceptance-criteria status.
  - Evidence: `docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/qa-gates/minor-audit-readiness-339.2026-07-16T12-39.md`.
  - Acceptance: the artifact confirms all required Phase 0, regression-testing, QA-gate, and issue-update evidence exists with complete schema; every planned command has a numeric exit code; the final C# toolchain passed in one ordered pass; the completed P0-T10 baseline remains `5,467/5,467/0/0`, `83.44%` repository coverage, and `100%` `ProgressViewer.cs` coverage without a post-change recapture or false single-worker label; P2-T4 retained its canonical `Workers=1`, `Scope=ClassLevel` runsettings under `evidence/other`, left `scripts/vscode/TaskMaster.cli.runsettings` unchanged, and produced `5,468/5,468/0/0` plus exactly one authoritative merged and postprocessed full-scope final coverage artifact; the scheduling-only comparability rationale is recorded; no aggregate partial or temporary raw coverage file was accepted; no third implementation file was added; only the approved two implementation files changed; `issue.md` is the sole AC source; all three ACs are checked; and the AC status summary reports source, total `3`, checked `3`, remaining `0`, and no remaining item text.

- [x] [P2-T9] Return the automated small-path QC result and evidence paths to the orchestrator for the configured post-implementation reduced `feature-review` handoff.
  - Acceptance: return `SMALL_PATH_QC: PASS` only when P2-T8 passes; otherwise return `SMALL_PATH_QC: REMEDIATION_REQUIRED`. The orchestrator must record the executor receipt, perform the required pre-review commit workflow, and delegate the repository's reduced minor-audit review without requesting manual validation.

---

## Acceptance Criteria Coverage Map

- AC1, non-null `CancelSource` assignment enables Cancel immediately in tracker loading state: P1-T2, P1-T4, P1-T5, P1-T7, and P1-T9.
- AC2, selecting Cancel cancels the same configured source: P1-T2, P1-T4, P1-T7, and P1-T10.
- AC3, deterministic fail-before/pass-after and clean ordered C# toolchain with coverage: P1-T3, P1-T4, P1-T6, P1-T7, P2-T1 through P2-T7.
