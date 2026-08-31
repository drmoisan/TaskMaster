# P0-T14 — Nullable and Type-Check Baseline Build

Timestamp: 2026-08-31T18-57
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
ExpectedExitCode: 0

BASELINE_NULLABLE_WARNINGS: 5
BASELINE_NULLABLE_ERRORS: 0

Output Summary: MSBuild's final summary, transcribed:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

The build was verified to be a real compilation: the captured log carries 36 `csc.exe` invocations. `/t:Rebuild` was used, not `/t:Build`, for the reason CLAUDE.md section C#1 records.

No `/p:Nullable=enable` was added. Nullable enforcement in this repository is per-file opt-in and `UtilitiesCS/To Depricate/FileIO2.cs` line 1 already carries the `#nullable enable` pragma, so the file under change participates in nullable flow analysis and its `CS86xx` diagnostics are promoted to errors by this command. This is character-for-character the command in `.github/workflows/ci.yml`.

A scan of the log for lines matching a compiler or analyzer diagnostic identifier of the form `(warning|error) <LETTERS><DIGITS>` returned zero matches, so none of the 5 warnings carries a diagnostic ID. They are the same 5 `System.Reactive.PackagesConfigCheck.targets` warnings recorded in P0-T13, which are emitted by a targets file rather than by the compiler and therefore carry no `CS`/`CA` identifier and are not promoted by `TreatWarningsAsErrors`.

Gate consequence: every later nullable gate in this plan (P2-T3, P4-T9, P6-T4) is a non-increase against these two recorded integers — error count at most 0 and warning count at most 5 — never an absolute zero. `BASELINE_NULLABLE_ERRORS:` is 0, so a later run must record 0 errors; `BASELINE_NULLABLE_WARNINGS:` is 5, which is non-zero, so a later artifact recording those carried warnings records `CARRIED_BASELINE_ERRORS:` citing this artifact.
