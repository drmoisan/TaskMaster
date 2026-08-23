# Repeat Run 1 of 3 — `UtilitiesCS.Test` full parallel run

Timestamp: 2026-08-08T16-58

Task: [P2-T7]

AC served: AC7 (at least three consecutive full parallel runs, identical and fully green for
`WpfDispatcherYieldTests`).

## vstest.console.exe resolution

`vswhere.exe` at `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe` resolved:

```
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
```

`vstest.console.exe` is not on PATH, so this resolution is required.

## Command

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

EXIT_CODE: 0

The assembly path is named explicitly and is workspace-root-relative, so discovery globbing is
bypassed entirely and no stale sibling-worktree assembly can be picked up.

Parallelization is the assembly's own `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
(`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`), unmodified. `Workers = 0` means "use the
processor count", so test classes genuinely run concurrently — which is the condition under which
the original defect manifested.

## Result

```
Test Run Successful.
Total tests: 4667
     Passed: 4667
```

Total 4667 / Passed 4667 / Failed 0 / Skipped 0.

## Per-test outcome for every `WpfDispatcherYieldTests` method

| # | Method | Outcome | Duration |
|---|---|---|---|
| 1 | `YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield` | Passed | 3 ms |
| 2 | `YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback` | Passed | 13 ms |
| 3 | `YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher` | Passed | 12 ms |
| 4 | `YieldAsync_WithoutDispatcher_RemainsStrict` | Passed | 1 ms |

All four passed. The formerly order-dependent
`YieldAsync_WithoutDispatcher_RemainsStrict` passed in 1 ms.

Failure scan of the log for `^\s+Failed ` returned no lines.

Output Summary: PASS, EXIT_CODE 0. `UtilitiesCS.Test` run under its own class-level parallelization
(`Workers = 0`) with `/InIsolation`: Total 4667, Passed 4667, Failed 0. All four
`WpfDispatcherYieldTests` methods passed, including `YieldAsync_WithoutDispatcher_RemainsStrict`.
Run 1 of the three required for AC7.
