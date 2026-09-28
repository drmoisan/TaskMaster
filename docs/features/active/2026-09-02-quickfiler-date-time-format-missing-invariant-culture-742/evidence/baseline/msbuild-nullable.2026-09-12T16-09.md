# Baseline MSBuild Nullable / Warnings-As-Errors State (issue #742, [P0-T6])

Timestamp: 2026-09-14T02-01

Command: `pwsh -NoProfile -Command '$out = msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1; $exit = $LASTEXITCODE; $out | Select-String -Pattern "^\s*\d+ Error\(s\)\s*$" | ForEach-Object { $_.Line.Trim() }; Write-Output "EXITCODE=$exit"'`

EXIT_CODE: 0

Output Summary: transcribed summary line `0 Error(s)`, exit code `0`. `/p:Nullable=enable` was not
added, per `CLAUDE.md` and `.claude/rules/csharp.md`; nullable participation in this repository is
per-file opt-in via `#nullable enable`.

Acceptance: none stated by the task; this is a baseline capture only.

Note: this run followed the `packages/Meziantou.Analyzer.3.0.203` provisioning recorded in
`msbuild-analyzer.2026-09-12T16-09.md`. Without that provisioning this gate fails from the same
pre-existing CS0006 HintPath skew rather than from any nullable diagnostic, because both gates use
`/t:Rebuild` and compile the same projects.
