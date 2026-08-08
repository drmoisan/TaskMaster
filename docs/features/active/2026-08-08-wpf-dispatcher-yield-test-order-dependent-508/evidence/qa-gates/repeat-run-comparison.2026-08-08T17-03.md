# Repeat-Run Comparison — Three Consecutive Full Parallel Runs

Timestamp: 2026-08-08T17-03

Task: [P2-T10]

AC served: AC1, AC7.

Sources compared:

- `<FEATURE>/evidence/qa-gates/repeat-run-1.2026-08-08T16-58.md`
- `<FEATURE>/evidence/qa-gates/repeat-run-2.2026-08-08T17-00.md`
- `<FEATURE>/evidence/qa-gates/repeat-run-3.2026-08-08T17-02.md`

## Command identity

All three runs executed the identical command with no intervening rebuild, edit, or configuration
change:

```
<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
         /Settings:scripts\vscode\TaskMaster.cli.runsettings
         /InIsolation
         /TestCaseFilter:"TestCategory!=LiveOutlook"
```

Parallelization is the assembly's own unmodified
`[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
(`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`). `Workers = 0` means "use the processor
count", so classes run concurrently and thread assignment is free to differ between runs.

## Assembly-level counts — IDENTICAL across all three runs

| Run | EXIT_CODE | Total | Passed | Failed | Skipped |
|---|---|---|---|---|---|
| 1 | 0 | 4667 | 4667 | 0 | 0 |
| 2 | 0 | 4667 | 4667 | 0 | 0 |
| 3 | 0 | 4667 | 4667 | 0 | 0 |

GATE PASS: total/passed/failed counts are identical across all three runs. Zero divergence.

## `WpfDispatcherYieldTests` per-method outcomes — ALL PASSED IN ALL THREE RUNS

| Method | Run 1 | Run 2 | Run 3 |
|---|---|---|---|
| `YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield` | Passed (3 ms) | Passed (2 ms) | Passed (6 ms) |
| `YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback` | Passed (13 ms) | Passed (13 ms) | Passed (8 ms) |
| `YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher` | Passed (12 ms) | Passed (21 ms) | Passed (33 ms) |
| `YieldAsync_WithoutDispatcher_RemainsStrict` | Passed (1 ms) | Passed (1 ms) | Passed (1 ms) |

GATE PASS: all four methods passed in all three runs — 12 of 12 observations green.

## Why the duration variance strengthens rather than weakens the result

Per-test durations differ across runs (for example
`YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher` at 12 / 21 / 33 ms). Under
`Workers = 0, Scope = ClassLevel` this reflects genuinely different scheduling and thread assignment
between runs. The scheduling changed; the outcomes did not. That is the specific property AC1
requires: the result no longer depends on which pooled thread the test lands on or on execution
order.

Before the fix, that same variance was decisive — `<FEATURE>/issue.md:50-54` records two consecutive
baseline runs at merge-base `003c5715` with `Failed: 2` and `Failed: 1`, the latter naming
`YieldAsync_WithoutDispatcher_RemainsStrict`.

## Sufficiency

`<FEATURE>/issue.md:87` states "The defect is intermittent, so a single green run does not
demonstrate a fix." Three consecutive fully-green runs with identical counts are recorded here, and
the four in-scope tests were additionally green in the pass-4 full-suite run
(`tests-coverage.2026-08-08T16-55.md`, 6295/6295) and in the two earlier failed full-suite passes,
where the only failures were the unrelated pre-existing `QuickFiler.Test` pair. That is six
independent observations of the four tests, all green.

## Integrity

No `[Ignore]`, `[DoNotParallelize]`, retry, sleep, or per-test filter was introduced to obtain these
results; the only `/TestCaseFilter` is `TestCategory!=LiveOutlook`, which is prescribed by the plan
task itself and excludes live-Outlook integration tests, not any test in scope. The assembly path is
named explicitly, so no stale sibling-worktree assembly can be discovered.

Output Summary: GATE PASS. Three consecutive runs of the identical command produced identical
assembly counts (Total 4667 / Passed 4667 / Failed 0, EXIT_CODE 0 in every run) and all four
`WpfDispatcherYieldTests` methods passed in all three runs (12/12 green observations). Per-test
durations varied across runs, confirming genuinely different scheduling under class-level
parallelization while outcomes stayed constant. No divergence of any kind; AC1 and AC7 are
satisfied.
