# Pre-Existing Failing / Flaky Test Set — Issue #503 (P0-T10)

Timestamp: 2026-08-08T13-12

Source run: P0-T9, `<FEATURE>\evidence\baseline\tests-with-coverage.2026-08-08T13-11.md` (6293 total, 6293 passed, 0 failed, 0 skipped, `Test Run Successful.`)

## Failing tests observed in P0-T9

**None.** The P0-T9 baseline run reported zero failures. The observed pre-existing failure set from that run is therefore the empty set.

## The pre-existing set (recorded)

The recorded pre-existing set for the purposes of the Phase 6 pass rule contains exactly one member:

| Fully-qualified name | File | Tracking issue | Status |
|---|---|---|---|
| `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` | `UtilitiesCS.Test\OutlookObjects\Folder\WpfDispatcherYieldTests.cs` | **#508** | Pre-existing order-dependent flake; OUT OF SCOPE for #503 |

This test is a pre-existing, order-dependent flake under parallel execution. It has been observed failing on some merge-base runs and passing on others with no code change (2 failures on one orchestrator baseline run, 1 on the next, and 0 on the P0-T9 run recorded here). It is already promoted to issue **#508** and is explicitly out of scope for issue #503.

It is included in the recorded pre-existing set even though it passed on the P0-T9 run, because its failure is nondeterministic across runs and a Phase 6 occurrence would otherwise be misread as a regression introduced by #503.

## The final-QC pass rule (verbatim)

> A Phase 6 test run passes when the only failures are members of this recorded set; any test not in this set that fails is a real regression that restarts the Phase 6 loop at P6-T1; issue #508 must not be fixed inside this change.

## Search scope for this determination

- SearchScope: the full P0-T9 vstest console output at `C:\Users\DANMOI~1\AppData\Local\Temp\claude\C--Users-DanMoisan-repos-TaskMaster-wt-2026-08-08T11-55\ef7e1b49-f808-435f-8b58-e04bac54f30b\scratchpad\p0t9.log`
- SearchPatterns: `Failed`, `Skipped`, `Total tests`, `Test Run`
- SearchResult: `Test Run Successful.` / `Total tests: 6293` / `Passed: 6293`; no `Failed:` or `Skipped:` line was emitted.

EXIT_CODE: 0
