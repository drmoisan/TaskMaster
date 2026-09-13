# Phase 2 — Analyzer Rebuild Gate (AC9)

Timestamp: 2026-09-13T15-31
Task: [P2-T3]

Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=TestResults/msbuild/p2-t3-analyzers.txt;Verbosity=detailed"
EXIT_CODE: 0

WarningCount: 0
ErrorCount: 0

Output Summary: the rebuild printed `Build succeeded.` followed by the summary lines `0 Warning(s)`
and `0 Error(s)`, and exited 0 after 17.81 seconds. Both counts are zero, so the AC9 error-count
demand is met and the warning count does not exceed the P0-T6 baseline. The detailed file log carries
the two csc command lines that establish compilation actually ran.

## Build Succeeded Line, Quoted

```
Build succeeded.
```

That line stands at line 75955 of `TestResults/msbuild/p2-t3-analyzers.txt`, immediately above the two
summary count lines at lines 75956 and 75957.

## Count Derivation — Start-Anchored, Not A Bare Substring

The two counts are read by a whole-line match of the form start-of-line, optional whitespace, one or
more digits, a space, the word Warning or Error, and the parenthesised s, anchored at end of line. The
match returned exactly two lines from the detailed log:

```
75956:    0 Warning(s)
75957:    0 Error(s)
```

A bare substring search for a zero-valued count would also match a ten-valued one, which is why the
whole-line anchored form is used. The console transcript printed the same two lines, so the two
independent readings of the counts agree.

## Non-Vacuity Observation — Compilation Ran

A warm `/t:Build` returns exit 0 with CoreCompile skipped as up to date on every project, so the exit
code alone cannot discharge AC9's requirement that compilation actually ran. Per D2 this gate uses
`/t:Rebuild`, and the observation that establishes real compilation is the presence of the csc command
lines MSBuild echoes under each project's CoreCompile heading. Searching the detailed file log:

- Lines carrying the literal `/out:obj\Debug\UtilitiesCS.dll`: 2
- Lines carrying the literal `/out:obj\Debug\UtilitiesCS.Test.dll`: 2

Each count is at least one, so both projects compiled. The figures were produced by the Grep tool
against the log file with the backslashes escaped for the regex engine, not by a shell command with an
embedded quote, so no quoting mangling can have reduced a real count to zero. Both projects also
appear in the console transcript building through CoreCompile and copying their freshly linked
assemblies to their output directories.

## Log Retention

Per D10 the detailed file log stays at the git-ignored path
`TestResults/msbuild/p2-t3-analyzers.txt` and is not committed, because an MSBuild log carries
absolute host paths. Only the values transcribed above enter the tracked tree.

Outlook was verified not running before this rebuild: a process query for the Outlook image name
returned a count of 0. The build lock was acquired immediately before the invocation and released
immediately after it returned.
