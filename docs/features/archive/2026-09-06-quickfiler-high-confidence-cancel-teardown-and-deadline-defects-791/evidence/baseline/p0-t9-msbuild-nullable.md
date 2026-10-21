# [P0-T9] Nullable-build baseline

Timestamp: 2026-09-06T14-32

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

BASELINE-NULLABLE-WARNINGS: 0
BASELINE-NULLABLE-ERRORS: 0

Output Summary: The command is character-for-character the CLAUDE.md nullable gate and the command
in `.github/workflows/_build-nullable.yml`. `/p:Nullable=enable` was not added, because no project
in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the
property would be a solution-wide opt-in that conscripts every file which has never adopted the
`#nullable enable` pragma. `/t:Build` was not substituted, because MSBuild's up-to-date check does
not invalidate on a command-line `/p:` change and a warm `/t:Build` would return exit 0 having
skipped `CoreCompile` on every project.

MSBuild reported:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.95
```

Nullable enforcement in this repository is per-file opt-in: a file participates when it carries
`#nullable enable`, and `/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to
build errors. The baseline is clean, so [P3-T4] has no pre-existing diagnostic set to discount.
