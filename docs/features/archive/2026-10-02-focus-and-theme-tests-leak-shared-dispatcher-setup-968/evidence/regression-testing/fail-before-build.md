# Fail-before build (issue #968, task P1-T4)

Timestamp: 2026-10-03T02-56
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null; $log = "coverage\logs\p1-t4.msbuild.log"; if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }; $before = (Get-Item -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" -ErrorAction SilentlyContinue).LastWriteTimeUtc; $beforeProd = (Get-Item -LiteralPath "QuickFiler\bin\Debug\QuickFiler.dll" -ErrorAction SilentlyContinue).LastWriteTimeUtc; $global:LASTEXITCODE = 0; & $msbuild TaskMaster.sln /t:Build /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" "/flp:LogFile=$log;Verbosity=normal" | Out-Null; Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE); $lines = Get-Content -LiteralPath $log -Encoding UTF8; Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value)); $after = (Get-Item -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll").LastWriteTimeUtc; $afterProd = (Get-Item -LiteralPath "QuickFiler\bin\Debug\QuickFiler.dll").LastWriteTimeUtc; Write-Output ("TEST_DLL_ADVANCED: " + ($null -eq $before -or $after -gt $before)); Write-Output ("PROD_DLL_ADVANCED: " + ($null -eq $beforeProd -or $afterProd -gt $beforeProd)); Write-Output ("CSC_OUT_QUICKFILER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.dll") }).Count); Write-Output ("CSC_OUT_QUICKFILER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.Test.dll") }).Count)' (CMD-BUILD, TASKID p1-t4; newline separators shown as semicolons)
Canonical command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" (resolved through vswhere against WORKTREE/TaskMaster.sln)
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- TEST_DLL_ADVANCED: True
- PROD_DLL_ADVANCED: False (recorded; no production file changed in this task)
- CSC_OUT_QUICKFILER: 0 (recorded)
- CSC_OUT_QUICKFILER_TEST: 2
- The new pin-count test file compiles against the unmodified fixture.
