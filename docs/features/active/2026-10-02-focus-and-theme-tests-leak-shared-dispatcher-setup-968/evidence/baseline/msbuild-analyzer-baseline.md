# Baseline: analyzer rebuild (issue #968, task P0-T10)

Timestamp: 2026-10-03T02-45
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null; $log = "coverage\logs\p0-t10.msbuild.log"; if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }; $global:LASTEXITCODE = 0; & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=$log;Verbosity=normal" | Out-Null; Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE); $lines = Get-Content -LiteralPath $log -Encoding UTF8; Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value)); Write-Output ("WARNINGS: " + (($lines | Select-String -Pattern "^\s*(\d+) Warning\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value)); Write-Output ("SKIP_CORECOMPILE_LINES: " + @($lines | Where-Object { $_.Contains("Skipping target ""CoreCompile""") }).Count); Write-Output ("CSC_OUT_QUICKFILER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.dll") }).Count); Write-Output ("CSC_OUT_QUICKFILER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.Test.dll") }).Count); $files = @("QfcItemController.UiThreadDispatcherFixture.cs(", "QfcItemController.FocusAndThemeTests.cs(", "QfcItemController.TestSupport.cs(", "QfcItemController.UiThreadDispatcherFixtureTests.cs(", "QfcItemController.UiThreadDispatcherPinCountTests.cs(", "QuickFiler.Test.csproj(", "SynchronousBackgroundWorker.cs(", "ArmingFakeTimeProvider.cs(", "QfcDatamodelLivenessTests.cs(", "QfcDatamodelTeardownTests.cs(", "QfcInitEmailQueueZeroBatchTests.cs(", "QfcDatamodelTests.cs(", "QfcDatamodel.cs(", "QfcDatamodel.QueueProcessing.cs("); $diag = @($lines | Where-Object { $l = $_; (@($files | Where-Object { $l.Contains($_) }).Count -gt 0) -and ($l -match "(error|warning) [A-Z]+[0-9]+") }); Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + $diag.Count); Write-Output ("WRITESET_DIAGNOSTIC_CODES: " + ((@($diag | ForEach-Object { [regex]::Match($_, "(error|warning) ([A-Z]+[0-9]+)").Groups[2].Value }) | Sort-Object -Unique) -join ",")); Write-Output ("TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll")); Write-Output ("UCS_TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll"))' (CMD-REBUILD with the analyzer GATEARGS and TASKID p0-t10; the executed payload separates statements with newlines, shown here as semicolons)
Canonical command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere against WORKTREE/TaskMaster.sln, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory)
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- WARNINGS: 0
- SKIP_CORECOMPILE_LINES: 0
- CSC_OUT_QUICKFILER: 2
- CSC_OUT_QUICKFILER_TEST: 2
- WRITESET_DIAGNOSTIC_LINES: 0
- WRITESET_DIAGNOSTIC_CODES: (empty)
- TEST_DLL_EXISTS: True
- UCS_TEST_DLL_EXISTS: True
- ANALYZER-BASELINE-WARNINGS: 0
- ANALYZER-BASELINE-WRITESET-LINES: 0
- ANALYZER-BASELINE-WRITESET-CODES: (empty)
