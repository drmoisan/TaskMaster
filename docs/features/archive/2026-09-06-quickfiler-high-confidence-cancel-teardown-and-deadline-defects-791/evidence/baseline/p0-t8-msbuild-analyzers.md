# [P0-T8] Analyzer-build baseline

Timestamp: 2026-09-06T14-29

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

BASELINE-ANALYZER-WARNINGS: 0
BASELINE-ANALYZER-ERRORS: 0

Output Summary: The command is character-for-character the CLAUDE.md analyzer gate. `/t:Rebuild` is
used rather than `/t:Build`, because MSBuild's incremental up-to-date check does not invalidate on a
command-line `/p:` change and a warm `/t:Build` would skip `CoreCompile` on every project and run no
analyzers.

MSBuild reported:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.95
```

Seventeen projects were rebuilt, ending with `UtilitiesCS.Test` and then the solution node. The
tree is analyzer-clean at `BASE-SHA`: this baseline is zero warnings and zero errors, so [P3-T3]
has no pre-existing diagnostic set to discount and any diagnostic it reports is attributable to
this change.
