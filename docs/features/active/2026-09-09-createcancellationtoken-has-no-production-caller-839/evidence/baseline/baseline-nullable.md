# Baseline: nullable warnings-as-errors solution rebuild — issue #839

Timestamp: 2026-09-13T02-55
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/839-baseline-nullable.detailed.log"; $code = $LASTEXITCODE; "MSBUILD_EXIT=$code"; exit $code'
Command: pwsh -NoProfile -Command '$f = "coverage/839-baseline-nullable.detailed.log"; $q = [char]34; $csc = "Task " + $q + "Csc" + $q; $a = @(Select-String -Path $f -SimpleMatch -CaseSensitive $csc).Count; $b = @(Select-String -Path $f -Pattern "^\s+0 Error\(s\)\s*$").Count; $cm = @(Select-String -Path $f -Pattern "^\s+\d+ Error\(s\)\s*$"); $c = if ($cm.Count -gt 0) { $cm[-1].Line.Trim() } else { "none" }; $d = @(Select-String -Path $f -Pattern ": error CS86\d\d:").Count; "CSC_TASK_LINES=$a"; "ZERO_ERROR_SUMMARY_LINES=$b"; "ERROR_SUMMARY_LINE=$c"; "CS86_ERROR_LINES=$d"'
EXIT_CODE: 0

Output Summary:
- MSBUILD_EXIT=0. All 18 projects built.
- CSC_TASK_LINES=18, at least 1, so compilation ran on every project and the gate was not skipped by MSBuild incrementality.
- ZERO_ERROR_SUMMARY_LINES=1
- ERROR_SUMMARY_LINE=0 Error(s)
- CS86_ERROR_LINES=0
- Gate result: EXIT_CODE 0, CSC_TASK_LINES at least 1, ZERO_ERROR_SUMMARY_LINES equal to 1, CS86_ERROR_LINES equal to 0. The Decision D14 halt condition for a red nullable baseline is not met.
- The solution-wide Nullable property was NOT passed, in accordance with policy rank 1 and rank 6. Nullable enforcement stays per-file opt-in, so only files carrying a #nullable directive contribute CS86xx diagnostics.
- This rebuild also produced QuickFiler.Test/bin/Debug/QuickFiler.Test.dll, the assembly [P0-T18] runs.
- The summary command was restructured exactly as recorded in the [P0-T15] artifact, for the same nested-quote reason, and measures the same four values with the same patterns.
- Outlook was verified CLOSED immediately before this rebuild. The rebuild ran while this item held the shared machine build lock, which was released immediately after it returned.
