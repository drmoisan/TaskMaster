# [P0-T10] Baseline analyzer build

Timestamp: 2026-09-08T00-31

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

`$msbuild` was resolved with the vswhere form the plan's standing conventions state, and resolved to the Visual Studio 18 Community MSBuild. Only the executable token is resolved by path; the argument list is character-for-character the list `CLAUDE.md` states.

EXIT_CODE: 0

BASELINE_PROJECT_COUNT: 18

Output Summary: the two trailing summary lines, verbatim:

```
    0 Warning(s)
    0 Error(s)
```

`BASELINE_PROJECT_COUNT` is the count of build-output lines of the arrow form `<ProjectName> -> <path>\bin\Debug\<Assembly>` in the normal-verbosity file log.

`/t:Rebuild` was used rather than `/t:Build`. MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers; a `/t:Build` result would not be admissible evidence for this gate.
