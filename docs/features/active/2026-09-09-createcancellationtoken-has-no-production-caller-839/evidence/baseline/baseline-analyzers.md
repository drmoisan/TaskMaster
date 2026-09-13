# Baseline: analyzer-enabled solution rebuild — issue #839

Timestamp: 2026-09-13T02-51
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/839-baseline-analyzers.detailed.log"; $code = $LASTEXITCODE; "MSBUILD_EXIT=$code"; exit $code'
Command: pwsh -NoProfile -Command '$f = "coverage/839-baseline-analyzers.detailed.log"; $q = [char]34; $csc = "Task " + $q + "Csc" + $q; $a = @(Select-String -Path $f -SimpleMatch -CaseSensitive $csc).Count; $b = @(Select-String -Path $f -Pattern "^\s+0 Error\(s\)\s*$").Count; $cm = @(Select-String -Path $f -Pattern "^\s+\d+ Error\(s\)\s*$"); $c = if ($cm.Count -gt 0) { $cm[-1].Line.Trim() } else { "none" }; $d = @(Select-String -Path $f -Pattern ": error CS86\d\d:").Count; "CSC_TASK_LINES=$a"; "ZERO_ERROR_SUMMARY_LINES=$b"; "ERROR_SUMMARY_LINE=$c"; "CS86_ERROR_LINES=$d"'
EXIT_CODE: 0

Output Summary:
- MSBUILD_EXIT=0. All 18 projects in the solution built, ending with UtilitiesCS.Test.
- CSC_TASK_LINES=18, which is at least 1, so compilation genuinely ran on every project rather than being skipped by MSBuild incrementality. The Rebuild target was used, never Build, exactly as policy rank 6 requires.
- ZERO_ERROR_SUMMARY_LINES=1
- ERROR_SUMMARY_LINE=0 Error(s)
- CS86_ERROR_LINES=0
- Gate result: EXIT_CODE 0, CSC_TASK_LINES at least 1, ZERO_ERROR_SUMMARY_LINES equal to 1. The Decision D14 halt condition for a red analyzer baseline is not met.

## Pre-existing repository defect encountered and resolved by environment provisioning (not a code change)

The FIRST invocation of this command in this fresh worktree exited 1 with ten `error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found` diagnostics, reported against VBFunctions.csproj and UtilitiesCS.csproj, with the remaining projects failing transitively.

Cause, measured rather than assumed:
- 15 first-party project files carry an `<Analyzer Include>` item naming Meziantou.Analyzer version 3.0.203, while packages.config resolves Meziantou.Analyzer to version 3.0.235. A clean-worktree restore therefore installs only 3.0.235 and the 3.0.203 folder the project files name is absent. Enumerating every analyzer package folder referenced by any project file showed exactly one missing: Meziantou.Analyzer.3.0.203. The other six (AsyncFixer 2.1.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0, MSTest.Analyzers 4.4.0, Roslynator.Analyzers 5.0.0, SonarAnalyzer.CSharp 10.34.0.3385, and Meziantou.Analyzer 3.0.235 which some project files name) were all present.
- Confirmed pre-existing and not introduced by this run: `git status --porcelain -- "*.csproj" "*/packages.config"` printed nothing, so no project file or packages.config is locally modified on this branch.
- Blast radius is the whole solution, not only the analyzer gate, because `<Analyzer Include>` is unconditional. The nullable gate and every test assembly would have been unreachable as well.

Resolution applied: the missing version was installed into the gitignored packages directory with `nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -NonInteractive`, which exited 0. Verification after the install: `git status --porcelain --untracked-files=all -- packages "*.csproj"` printed nothing, so no tracked file changed, nothing entered the diff, and the Write Set was not widened. This is environment bootstrap of the same kind as the SDK, tool and NuGet restore tasks that precede it in Phase 0; it is not a repository code change and it is not adopted as a remedy by any task in this plan.

Because the red first attempt was caused by an incomplete bootstrap rather than by a repository source defect, it was not carried forward as this plan's baseline. Carrying a CS0006 baseline forward would have degraded every later exit-0 gate into a gate that cannot fail. The artifact above records the green run that measures the tree this plan actually changes, and the red first attempt is recorded here in full for audit.

The durable fix is upstream and out of scope for this item: the 15 `<Analyzer Include>` version strings must be updated in the same commit that bumps packages.config. This is enumerated as an out-of-scope pre-existing defect in the executor's final report rather than fixed here.

## Supporting measurements

Command: git status --porcelain -- "*.csproj" "*/packages.config"
Output: no lines.

Command: pwsh -NoProfile -Command '$files = Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object FullName; $lines = Select-String -Path $files -SimpleMatch "Analyzer Include=" | ForEach-Object { $_.Line }; $ids = $lines | ForEach-Object { $parts = $_.Split([char]92); $k = [array]::IndexOf($parts, "packages"); if ($k -ge 0) { $parts[$k+1] } } | Sort-Object -Unique; foreach ($f in $ids) { "$f EXISTS=$(Test-Path -LiteralPath (Join-Path ''packages'' $f))" }'
Output:
AsyncFixer.2.1.0 EXISTS=True
Meziantou.Analyzer.3.0.203 EXISTS=False
Meziantou.Analyzer.3.0.235 EXISTS=True
Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0 EXISTS=True
MSTest.Analyzers.4.4.0 EXISTS=True
Roslynator.Analyzers.5.0.0 EXISTS=True
SonarAnalyzer.CSharp.10.34.0.3385 EXISTS=True

Command: nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages -NonInteractive
Output: NUGET_EXIT=0; PROVISIONED=True.

## Note on the summary command form

The plan's CMD-MSBUILD-SUMMARY label nests an escaped double-quoted literal inside a subexpression inside an expandable string. That nesting does not survive this executor's shell boundary and produced a parameter-binding error on every input line rather than a count. The four measurements were therefore taken with the same four patterns, the same case sensitivity and the same simple-match/regex modes, restructured so each result is assigned to a variable before interpolation and so the log is streamed by path instead of being loaded into memory. The measured values are the values CMD-MSBUILD-SUMMARY is written to produce.

Outlook was verified CLOSED immediately before this rebuild. The rebuild ran while this item held the shared machine build lock, which was released immediately after it returned.
