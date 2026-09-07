# QA Gate — .NET analyzers, post-merge final pass (P7-T3 re-run)

Timestamp: 2026-08-27T23-31

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary: **0 Error(s)**, 5 Warning(s). The 5 warnings are pre-existing and sibling-owned; none
originates in a file this feature writes.

## Non-vacuity proof

`/t:Rebuild` is used, never `/t:Build`. The build genuinely compiled:

| Instrument | Count | Meaning |
| --- | --- | --- |
| `Skipping target "CoreCompile"` | **0** | no project skipped compilation, so the analyzer gate was live on every project |
| `csc.exe` | 36 | the compiler was actually invoked |

A zero `Skipping target "CoreCompile"` count is the assertion that matters: a warm `/t:Build` returns
exit 0 with `CoreCompile` skipped on every project and runs no analyzers at all.
