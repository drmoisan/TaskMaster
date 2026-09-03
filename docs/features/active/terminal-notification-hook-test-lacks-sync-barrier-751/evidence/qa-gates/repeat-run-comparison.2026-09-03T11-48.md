# P3-T3 — Repeat-Run Comparison (Issue #751)

Timestamp: 2026-09-03T14-38

This artifact is the AC5 green-after evidence and is simultaneously the repeat-run stress record that
fail-before route 2 pairs with the P1-T2 dossier. It follows the shape of the precedent at
`docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/repeat-run-comparison.2026-08-08T17-03.md`.

## Source artifacts

1. `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/qa-gates/repeat-run-1.2026-09-03T11-48.md`
2. `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/qa-gates/repeat-run-2.2026-09-03T11-48.md`
3. `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/qa-gates/repeat-run-3.2026-09-03T11-48.md`
4. `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/qa-gates/repeat-run-4.2026-09-03T11-48.md`
5. `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/qa-gates/repeat-run-5.2026-09-03T11-48.md`

## Command-identity statement

All five runs executed the **identical** command line, differing only in the run index that appears in the
`LogFileName` and `ResultsDirectory` switches, which exist to keep each run's evidence separate:

```
& $vstest 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=P3-T2-run<n>.trx" "/TestCaseFilter:TestCategory!=LiveOutlook" "/ResultsDirectory:coverage\trx\P3-T2-run<n>"
```

There was **no** intervening rebuild, edit, or configuration change between the five runs. All five ran
against the same `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` produced by the P2-T5 rebuild, which is the
build that carries the three-line fix. The runs were launched consecutively between 14:36:42 and 14:37:35
local time.

## Per-run table

| Run | EXIT_CODE | Total | Passed | Failed | Skipped | Failed-name set | subset of BASELINE_FAILURE_SET |
|---|---|---|---|---|---|---|---|
| 1 | 0 | 408 | 408 | 0 | 0 | (empty) | **yes** |
| 2 | 0 | 408 | 408 | 0 | 0 | (empty) | **yes** |
| 3 | 0 | 408 | 408 | 0 | 0 | (empty) | **yes** |
| 4 | 0 | 408 | 408 | 0 | 0 | (empty) | **yes** |
| 5 | 0 | 408 | 408 | 0 | 0 | (empty) | **yes** |

## Per-run outcome of the target test

`TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`:

| Run | Outcome | Duration |
|---|---|---|
| 1 | **Passed** | 00:00:00.0023498 |
| 2 | **Passed** | 00:00:00.0022553 |
| 3 | **Passed** | 00:00:00.0023537 |
| 4 | **Passed** | 00:00:00.0022332 |
| 5 | **Passed** | 00:00:00.0017741 |

## BASELINE_FAILURE_SET, printed once for reference

The `TaskMaster.Test.dll` members of `BASELINE_FAILURE_SET`, as recorded by P0-T14:

```
(empty)
```

P0-T14 recorded `BASELINE_FAILURE_SET: none` across all nine test assemblies (6984 total, 6984 passed, 0
failed). Because the baseline set is empty, each run's "subset of BASELINE_FAILURE_SET" column is satisfied
only by an empty failed-name set, which is what all five runs recorded.

## Comparison against the pre-change three-run series (P0-T15)

| | Pre-change (P0-T15) | Post-change (P3-T2) |
|---|---|---|
| Runs | 3 | 5 |
| Command shape | identical CI-shaped invocation | identical CI-shaped invocation |
| Total per run | 408 | 408 |
| Failed per run | 0 | 0 |
| Target test outcome | Passed, Passed, Passed | Passed, Passed, Passed, Passed, Passed |
| Target test duration range | 0.0016355 - 0.0019062 s | 0.0017741 - 0.0023537 s |

**What this comparison does and does not show.** The pre-change series was green on all three runs, so the
two series do not differ in observed outcome. That is expected and is exactly why fail-before route 2 was
selected: research §2.4 establishes that interleaving (b) passes unconditionally and the interleaving-(a)
race window is sub-microsecond, so the pre-change tree is not reliably red. The post-change series therefore
does **not** demonstrate a red-to-green transition, and this artifact does not claim one.

What the post-change series does establish is that the repaired test passes on every run of the series under
the CI-shaped invocation, with no failure and no re-run required, which is what AC5 requires. The mechanical
argument that the race is closed is separate and rests on the barrier itself: the added
`await run.Terminal` establishes a happens-before edge from the fixture's increment at
`AppOlObjectsFolderTreeServiceLifecycleTests.cs:200` — via the `Interlocked.Exchange` full fence at `:201`
and the `TrySetResult` release at `:202` — to the assertion that reads the counter. The counter read is now a
`Volatile.Read` of a field written by `Interlocked.Increment`. The small upward shift in the target test's
duration is consistent with the assertion now awaiting the terminal signal rather than reading an
unsynchronised field.

## Integrity statement

No `[Ignore]`, no `[DoNotParallelize]`, no retry, no sleep, and no per-test filter beyond the CI-prescribed
`TestCategory!=LiveOutlook` was introduced to obtain this result.

- No attribute was added to any test or class. The only source changes on this branch are the three lines
  committed by P2-T6, mechanically bounded by P4-T10.
- No run was retried to obtain a different outcome. All five runs completed on their first attempt; the
  relaunch provision of the Long-running commands convention was never invoked, and no completed run was
  discarded or re-run.
- No banned determinism API was added anywhere in the branch diff; P4-T9 is the mechanical gate for that.
- The `/TestCaseFilter:TestCategory!=LiveOutlook` filter is the filter CI itself uses
  (`.github/workflows/_mstest-coverage.yml:99`); it was not narrowed to select or deselect the target test.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| The artifact exists | this file | PASS |
| Its per-run table has five rows | 5 rows | PASS |
| Every row records `subset of BASELINE_FAILURE_SET: yes` | all 5 rows record **yes** | PASS |
| Every row of the second table records `Passed` for the named test | all 5 rows record **Passed** | PASS |
| The integrity statement is present | present above | PASS |
