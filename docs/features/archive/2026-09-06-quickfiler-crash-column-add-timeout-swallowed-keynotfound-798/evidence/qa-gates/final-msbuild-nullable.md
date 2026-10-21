# P8-T3 — Final QC toolchain step 3 of 4: type check (nullable analysis)

Timestamp: 2026-09-07T05-43
Toolchain pass: 1

Host-specific absolute paths are redacted to a `<worktree>` or `<vs-install>` token.

## Command

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

Executed with the working directory set to `<worktree>`, using the vswhere-resolved MSBuild at
`<vs-install>\MSBuild\Current\Bin\MSBuild.exe`, with a normal-verbosity file logger writing to
`<worktree>\coverage\msbuild-nullable-final.log` so the summary block could be observed. The logging
switch changes neither what is compiled nor which diagnostics are produced. The `coverage` directory
is gitignored and the log carries unredacted absolute paths, so the log is not a retained artifact.

EXIT_CODE: 0

## Summary lines quoted verbatim

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Time Elapsed 00:00:17.37.

Compared against the P0-T7 nullable baseline, which also recorded `0 Error(s)` and a warning count of
0, there is no change.

## `/p:Nullable=enable` was not supplied

This is deliberate and matches CI. The command above is the one in the repository's nullable CI
workflow, character for character apart from the added file logger. Two properties are load-bearing
and were not "restored":

- **No `/p:Nullable=enable`.** Nullable enforcement in this repository is per-file opt-in: a file
  participates when it carries a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true`
  then promotes that file's `CS86xx` diagnostics to errors. No project carries a `<Nullable>` element
  and there is no repository-root build property file, so supplying the property solution-wide would
  conscript every file that has never adopted the pragma. Both files this change adds carry the
  pragma, so both are inside the enforced set without it.
- **`/t:Rebuild`, not `/t:Build`.** MSBuild's up-to-date check does not invalidate on a command-line
  `/p:` change, so a warm `/t:Build` would return exit 0 having skipped `CoreCompile` on every
  project, and the gate could not fail.

The rebuild is confirmed to have compiled rather than skipped: the file log contains 74
`CoreCompile:` target executions across 19 `Done Building Project ... .csproj` lines.

This artifact asserts the exit code together with the summary line `0 Error(s)`, never the absence of
the substring `error`.

Output Summary: The solution-wide nullable rebuild exited 0 with the summary line `0 Error(s)` and a
warning count of 0, unchanged from the P0-T7 baseline. `/p:Nullable=enable` was not supplied, matching
CI. All nineteen projects rebuilt. No step rewrote a file, so the toolchain loop proceeds to P8-T4
without a restart.
