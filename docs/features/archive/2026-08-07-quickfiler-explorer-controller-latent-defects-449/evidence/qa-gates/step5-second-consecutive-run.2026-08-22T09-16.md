# Final QC Step 5 — Second Consecutive Full-Suite Run, AC-13 Determinism (Issue #449, [P7-T7])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command (identical to [P7-T6], only the output path differs):
```
dotnet-coverage collect `
  --output <WORKTREE>\coverage\postchange-p7t7-second.cobertura.xml `
  --output-format cobertura `
  --settings coverage.config `
  -- vstest.console.exe <9 discovered assemblies> `
     /Settings:scripts\vscode\TaskMaster.cli.runsettings `
     /InIsolation `
     /TestCaseFilter:TestCategory!=LiveOutlook
```
EXIT_CODE: 0

The assembly-discovery logic, the settings file, `/InIsolation`, and the test-case filter are all
byte-identical to [P7-T6]. Nothing in the tree changed between the two runs.

## Counts — identical across the two runs

| Metric | Run 1 ([P7-T6]) | Run 2 ([P7-T7]) | Identical? |
| --- | --- | --- | --- |
| Executed | 6452 | 6452 | **yes** |
| Passed | 6452 | 6452 | **yes** |
| Failed | 0 | 0 | **yes (empty in both)** |
| Skipped | 0 | 0 | **yes** |

Run 2 output:
```
Test Run Successful.
Total tests: 6452
     Passed: 6452
```

## Pass SETS are byte-identical, not merely the counts

Equal counts would not by themselves prove determinism: two runs could pass 6,452 tests each while
disagreeing about WHICH tests passed. The pass sets were therefore compared element by element.

Command:
```
grep -o '^  Passed [A-Za-z0-9_]*' p7t6-vstest-rerun.log | sed 's/^  Passed //' | sort > run1.txt
grep -o '^  Passed [A-Za-z0-9_]*' p7t7-second.log        | sed 's/^  Passed //' | sort > run2.txt
diff run1.txt run2.txt
```
EXIT_CODE: 0
Output:
```
run1 lines: 6452  run2 lines: 6452
=== DIFF ===
(no differences)
```

`diff` reports **no differences**. Both sorted pass sets contain 6,452 entries and are identical, so
every individual test that passed in run 1 also passed in run 2, and no test moved between the passing
and failing sets.

**The set of failing tests is EMPTY in both runs.**

## AC-13 determinism evidence

This is the run-to-run half of the AC-13 determinism evidence. The other half is the static
prohibition scan in `ac13-determinism-scan.2026-08-22T09-16.md`, which confirms the tests added by
this change contain no `Thread.Sleep`, `Task.Delay`, `MessageBox.Show`, `Path.GetTempPath`,
`new Form`, or `Application.Run`.

Two consecutive full-suite runs producing identical executed counts, identical passed counts, and
byte-identical pass sets is the strongest available observational evidence that the suite is
deterministic on this tree.

### Disclosure regarding an earlier flake

A first attempt at [P7-T6] recorded one failure in an unrelated `UtilitiesCS` WPF-`Dispatcher` STA
test (`ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`),
which then passed in isolation and passed in both of the two consecutive runs compared here. It is
fully disclosed in `step5-vstest-coverage.2026-08-22T09-16.md`. It is recorded as a latent determinism
defect in a project this change does not touch. No test was modified, retried, or given a timing
tolerance; the two runs compared above are unmodified consecutive executions of the same command.

## Output Summary

The second consecutive full-suite run returned **EXIT_CODE 0** with **6,452 executed and 6,452
passed**, exactly matching run 1. The two sorted pass sets were compared with `diff` and are
**byte-identical** (6,452 entries each, no differences), and the failing-test set is **empty in both
runs**. This satisfies the AC-13 determinism requirement for run-to-run stability.
