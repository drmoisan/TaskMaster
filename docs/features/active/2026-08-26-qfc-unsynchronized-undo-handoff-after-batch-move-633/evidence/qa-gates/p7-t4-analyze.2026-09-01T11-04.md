# Final analyzer gate (P7-T4)

Timestamp: 2026-09-01T11-04
Task: [P7-T4]
Working directory: WORKTREE

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/qa-gates/p7-t4-analyze.msbuild.txt;verbosity=normal"
```

EXIT_CODE: 0

File log: `FEATURE/evidence/qa-gates/p7-t4-analyze.msbuild.txt` (11905 lines).

## Verbatim summary lines

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

## Vacuity check

Count of occurrences of the literal `Skipping target "CoreCompile"` in
`FEATURE/evidence/qa-gates/p7-t4-analyze.msbuild.txt`: **0**.

Count of `CS`, `CA`, `IDE`, `SA`, `MA`, `RCS`, or `S`-prefixed compiler or analyzer diagnostic lines: 0.

Output Summary: The analyzer gate passes on the final, formatted tree. The full-solution rebuild exited
0 with 0 errors and 5 warnings, and the warning count is identical to the P0-T8 baseline — the same five
pre-existing, code-less System.Reactive `packages.config` warnings, one per `packages.config` project
that references System.Reactive. This change introduced no analyzer or compiler diagnostic of any kind.

Zero `Skipping target "CoreCompile"` occurrences is what makes the result meaningful. `/t:Rebuild` forced
a genuine compile of every project, so the analyzers actually ran over the changed files. A warm
`/t:Build` would have exited 0 with `CoreCompile` skipped everywhere, because MSBuild's up-to-date check
does not invalidate on a command-line `/p:` change, and the gate would have been vacuous.

Error counts are taken from MSBuild's own `N Error(s)` summary line rather than from a raw
`Select-String 'error CS'` over the log, which double-counts: each diagnostic prints once inline and
again in the summary block.

This artifact is one of the four that the AC19 check-off in P8-T23 depends on.
