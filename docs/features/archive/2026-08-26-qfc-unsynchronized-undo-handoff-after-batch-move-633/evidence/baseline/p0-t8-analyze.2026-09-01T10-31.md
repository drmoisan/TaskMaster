# Baseline analyzer build (P0-T8)

Timestamp: 2026-09-01T10-31
Task: [P0-T8]
Working directory: WORKTREE

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/baseline/p0-t8-analyze.msbuild.txt;verbosity=normal"
```

EXIT_CODE: 0

File log: `FEATURE/evidence/baseline/p0-t8-analyze.msbuild.txt` (5048 lines). The `.msbuild.txt`
extension is deliberate: `.gitignore:84` is `*.log`, so a `.log` file would never be committed and the
log backing these counts would be absent from the evidence set.

## Verbatim summary lines

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

## Vacuity check

Count of occurrences of the literal `Skipping target "CoreCompile"` in
`FEATURE/evidence/baseline/p0-t8-analyze.msbuild.txt`: **0**.

Zero occurrences is what makes this gate non-vacuous. `/t:Rebuild` forced a real compile of every
project, so the analyzers actually ran; a warm `/t:Build` would have exited 0 with `CoreCompile` skipped
on every project and no analyzer executed.

## Nature of the five warnings

All five are the same pre-existing MSBuild-level warning, not a Roslyn analyzer diagnostic. Each is
raised by `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)` and
reads: "The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference." It fires once per `packages.config` project that references
System.Reactive. No occurrence of a `warning CS`, `warning CA`, or `warning IDE` diagnostic appears in
the log; a search for `:\s+warning\s+[A-Za-z]+[0-9]+` returns zero matches, because this warning carries
no diagnostic code.

Output Summary: The baseline analyzer gate is green. The full-solution rebuild exited 0 with 0 errors
and 5 pre-existing, code-less System.Reactive packages.config warnings, and the file log contains zero
`Skipping target "CoreCompile"` occurrences, confirming every project was genuinely compiled and
analyzed. This is the reference state that the P4-T5, P5-T10, and P7-T4 builds are compared against.
