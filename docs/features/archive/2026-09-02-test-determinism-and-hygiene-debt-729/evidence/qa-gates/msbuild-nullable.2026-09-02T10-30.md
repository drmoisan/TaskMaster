# Post-change MSBuild nullable rebuild (P6-T4)

Timestamp: 2026-09-02T23-36

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

Tool resolution used the Block K prelude.

EXIT_CODE: 0

## MSBuild summary lines

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.14
```

## Command-shape compliance

`/p:Nullable=enable` was not added, and `/t:Build` was not substituted for `/t:Rebuild`. The
command above is character-for-character the repository's approved nullable gate. Adding
`/p:Nullable=enable` would conscript every file that has never adopted the per-file pragma;
substituting `/t:Build` would let MSBuild's up-to-date check skip `CoreCompile` and return exit 0
without running the gate at all.

## Output Summary

The build log contains zero occurrences of `CS8632`. A pattern search for the whole `CS86xx`
nullable-diagnostic family also returns zero occurrences.

- `CS8632` — 0 occurrences.
- `CS86[0-9][0-9]` — 0 occurrences.

This is the AC3 evidence: the `#nullable enable annotations` / `#nullable restore annotations`
pragma pair scoped around the `ITimer? timer = null;` local in
`TaskMaster/AppGlobals/NonBlockingDelay.cs` is doing its job. The nullable rebuild emits no
CS8632 for that file, so the narrowly-scoped annotations-only context was preserved correctly
when the 2-arg overload's nullable local replaced the prior `Timer? timer = null;` local.

## Non-vacuity check

The captured log contains 55 `CoreCompile:` task executions, so compilation actually occurred and
the gate is not passing vacuously through an incremental skip.

The 5 warnings are the same non-diagnostic-ID `System.Reactive` `PackagesConfigCheck`
`packages.config` messages recorded by P0-T9, P0-T10, and P6-T3. They carry no diagnostic
identifier, which is why `/p:TreatWarningsAsErrors=true` does not promote them to errors and the
build reports `0 Error(s)`.
