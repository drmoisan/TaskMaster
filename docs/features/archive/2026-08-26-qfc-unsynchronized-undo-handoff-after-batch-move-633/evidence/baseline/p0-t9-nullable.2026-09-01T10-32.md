# Baseline nullable / type-check build (P0-T9)

Timestamp: 2026-09-01T10-32
Task: [P0-T9]
Working directory: WORKTREE

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:logfile=FEATURE/evidence/baseline/p0-t9-nullable.msbuild.txt;verbosity=normal"
```

EXIT_CODE: 0

`/p:Nullable=enable` was not added, and `/t:Build` was not substituted for `/t:Rebuild`. This command is
character-for-character the one `.github/workflows/ci.yml` runs for its nullable step, apart from the
`/fl` file-logger switch this plan requires for the vacuity check.

File log: `FEATURE/evidence/baseline/p0-t9-nullable.msbuild.txt` (12044 lines).

## Verbatim summary lines

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

## Vacuity check

Count of occurrences of the literal `Skipping target "CoreCompile"` in
`FEATURE/evidence/baseline/p0-t9-nullable.msbuild.txt`: **0**.

## Note on the five warnings under warnings-as-errors

The same five pre-existing System.Reactive `packages.config` warnings recorded in the P0-T8 artifact
appear here and are not promoted to errors. That is expected rather than anomalous:
`/p:TreatWarningsAsErrors=true` promotes *compiler* warnings, and these are raised by a NuGet-supplied
MSBuild `.targets` file with no diagnostic code, so the promotion does not reach them. The error count
is 0.

Output Summary: The baseline type-check gate is green. The full-solution rebuild under
`/p:TreatWarningsAsErrors=true` exited 0 with 0 errors, and the file log contains zero
`Skipping target "CoreCompile"` occurrences, so every project was genuinely recompiled and every
nullable-flow diagnostic in every file that has opted into `#nullable enable` was actually evaluated.
No `CS86xx` diagnostic is present. This is the reference state for P7-T5.
