# P6-T3 — Final Analyzer Build

Timestamp: 2026-08-31T20-20
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
ExpectedExitCode: 0
Iteration: 1

## Recorded integers from MSBuild's final summary

- Warnings: 5
- Errors: 0

## Acceptance evaluation against the recorded baseline

- Recorded error count 0 is less than or equal to `BASELINE_ANALYZER_ERRORS:` 0 from `evidence/baseline/p0-t13-analyzer-build.md`. Holds.
- Recorded warning count 5 is less than or equal to `BASELINE_ANALYZER_WARNINGS:` 5 from the same artifact. Holds.

CARRIED_BASELINE_ERRORS: `evidence/baseline/p0-t13-analyzer-build.md` records `BASELINE_ANALYZER_ERRORS: 0` and `BASELINE_ANALYZER_WARNINGS: 5`. The warning baseline is non-zero, so the carried-blocker form applies to it: the 5 warnings are the `System.Reactive.PackagesConfigCheck.targets` warnings present at branch head, emitted once per affected project by a targets file. They carry no diagnostic identifier and none originates in this change's footprint, so the observed `EXIT_CODE:` is 0.

36 `csc.exe` invocations in the captured log confirm a real compilation rather than a skipped incremental pass. `/t:Rebuild` was used, not `/t:Build`.

Output Summary: The analyzer gate passes on the formatted tree with no increase against either recorded baseline integer.
