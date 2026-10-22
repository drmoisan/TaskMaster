# [P0-T8] Baseline — analyzer build

Timestamp: 2026-09-06T01-33

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Run from the worktree root. `/t:Rebuild` is used rather than `/t:Build`: MSBuild's up-to-date check
does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 having
skipped `CoreCompile` on every project and having run no analyzers.

EXIT_CODE: 0

Output Summary: the build succeeded with no analyzer diagnostics. The final summary lines, verbatim:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

BASELINE-ANALYZER-WARNINGS: 0
BASELINE-ANALYZER-ERRORS: 0

## Consumers

[P1-T3] re-runs this command after the two test-file assertion edits and requires the same three
figures. [P1-T6] re-runs it with the temporary falsification mutation in place, where only the exit
code is asserted. [P4-T3] re-runs it as the lint step of the final toolchain pass and requires the
same three figures again.
