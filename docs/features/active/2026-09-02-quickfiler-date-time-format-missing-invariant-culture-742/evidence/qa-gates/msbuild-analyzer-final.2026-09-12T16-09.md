# Final QA Gate 2 — .NET Analyzers (issue #742, [P5-T2])

Timestamp: 2026-09-14T02-22

Command: `pwsh -NoProfile -Command '$out = msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1; $exit = $LASTEXITCODE; $out | Select-String -Pattern "^\s*\d+ Error\(s\)\s*$" | ForEach-Object { $_.Line.Trim() }; Write-Output "EXITCODE=$exit"'`

EXIT_CODE: 0

Output Summary: the transcribed summary line, trimmed, is exactly `0 Error(s)`, and `EXITCODE` is 0.
The `Select-String` pattern is anchored to the whole trimmed line (`^\s*\d+ Error\(s\)\s*$`), so a
`10 Error(s)` line could not satisfy this condition.

Acceptance: satisfied.

`/t:Rebuild` was used rather than `/t:Build`, per `CLAUDE.md` and `.claude/rules/csharp.md`: MSBuild's
incremental up-to-date check does not invalidate on a command-line `/p:` change, so a warm
`/t:Build` returns exit 0 with `CoreCompile` skipped and runs no analyzers.

This run followed [P5-T1]. CSharpier rewrote no tracked file between the two, so no restart of the
Phase 5 sequence was required.

The `packages/Meziantou.Analyzer.3.0.203` provisioning recorded in
`../baseline/msbuild-analyzer.2026-09-12T16-09.md` remains in place for this run. It is a gitignored
build input and changes no tracked file.
