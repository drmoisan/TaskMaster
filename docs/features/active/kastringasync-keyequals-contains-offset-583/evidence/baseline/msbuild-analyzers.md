# MSBuild Analyzer Rebuild (Baseline)

- Timestamp: 2026-09-13T00-30
- Command: MSBuild.exe (VS18) TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
  "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
  /flp:logfile=<detailed-file-log>;verbosity=detailed
- EXIT_CODE: 0

## Verdict line

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Log-line counts (from the detailed file logger)

- "Skipping target ... CoreCompile" occurrences: 0
- "CoreCompile:" occurrences: 130 (proves the rebuild was not vacuous)
- Total warning count: 0 (console verdict "0 Warning(s)"; no analyzer/compiler warning lines
  present in the detailed log for this run — the five System.Reactive
  PackagesConfigCheck.targets target-invocation trace lines present in the log are target-graph
  trace lines, not warning diagnostics, and none is followed by a warning-severity message text)

## Output Summary

Exit code 0; verdict "Build succeeded."; 0 "Skipping target CoreCompile" occurrences; 130
"CoreCompile:" occurrences (non-vacuous rebuild); recorded warning count is 0, which is the
ceiling P5-T3 compares against. This differs numerically from the archived precedent's 5
System.Reactive packages.config warnings at
docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/msbuild-analyzers.2026-08-22T09-21.md,
but is consistent in kind (0 skip / >=9 CoreCompile / 0 errors); the numeric warning count is
recorded as observed rather than assumed to match the archived figure.
