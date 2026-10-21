# Phase 2 — Nullable Rebuild Gate (AC10)

Timestamp: 2026-09-13T15-33
Task: [P2-T4]

Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=TestResults/msbuild/p2-t4-nullable.txt;Verbosity=detailed"
EXIT_CODE: 0

WarningCount: 0
ErrorCount: 0

Output Summary: the rebuild printed `Build succeeded.` followed by `0 Warning(s)` and `0 Error(s)`, and
exited 0 after 18.62 seconds. Under this gate `TreatWarningsAsErrors` promotes a warning to an error, so
the zero error count is the operative signal: no file that has opted into nullable analysis through the
per-file pragma produced a CS86xx diagnostic. The detailed file log carries the two csc command lines
that establish compilation actually ran.

## Build Succeeded Line, Quoted

```
Build succeeded.
```

## Count Derivation — Start-Anchored, Not A Bare Substring

The counts are read by the same whole-line anchored match P2-T3 uses: start of line, optional
whitespace, digits, a space, the word Warning or Error, the parenthesised s, end of line. Both the
detailed file log and the console transcript report `0 Warning(s)` and `0 Error(s)`.

## No Nullable Property, Per D3

The command line above contains no `/p:Nullable=enable` and no Nullable property in any other form. It
is character-for-character the form the CI nullable workflow runs. Supplying that property would
conscript every file that has never adopted the per-file pragma; no project in this repository carries
a Nullable element and there is no Directory.Build.props, so the property is a solution-wide opt-in
that produced roughly 195 errors in the UtilitiesCS project when it was last measured. Omitting it
loses no enforcement over any file that has opted in.

## Non-Vacuity Observation — Compilation Ran

- Lines carrying the literal `/out:obj\Debug\UtilitiesCS.dll`: 2
- Lines carrying the literal `/out:obj\Debug\UtilitiesCS.Test.dll`: 2

Each was counted by its own separate search rather than by one combined pattern, so a total of four
cannot be a four-and-zero split misread as two and two. Both counts are at least one, so both projects
really compiled under this gate rather than having CoreCompile skipped as up to date. Per D2 that
observation, and not the exit code, is what discharges the requirement.

## Log Retention

Per D10 the detailed file log stays at the git-ignored path `TestResults/msbuild/p2-t4-nullable.txt`
and is not committed.

Outlook was verified not running before this rebuild. The build lock was acquired immediately before
the invocation and released immediately after it returned.
