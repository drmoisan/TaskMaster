# MSBuild Nullable Rebuild (Post-Change, P5-T4)

- Timestamp: 2026-09-13T01-55
- Command: MSBuild.exe (VS18) TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
  "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
  /flp:logfile=<detailed-file-log>;verbosity=detailed
- EXIT_CODE: 0

## Verdict line

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Log-line count

- "Skipping target ... CoreCompile" occurrences: 0
- "CoreCompile:" occurrences: 167 (non-vacuous rebuild)

## Output Summary

Exit code 0; verdict "Build succeeded."; "Skipping target CoreCompile" count of exactly 0.
Matches the recorded shape at
docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/msbuild-nullable.2026-08-22T09-23.md.
No nullable opt-in property was passed.
