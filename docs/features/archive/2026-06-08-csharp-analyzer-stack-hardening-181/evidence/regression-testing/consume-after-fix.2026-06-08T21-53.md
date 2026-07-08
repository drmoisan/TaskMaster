# Consume After Fix — Finding C test 4 (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Tests:Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress`
(VS18 vstest.console.exe; MSYS_NO_PATHCONV=1; run three consecutive times to confirm determinism.)

EXIT_CODE: 0

Output Summary:
- Run 1: Passed [275 ms]. Run 2: Passed [274 ms]. Run 3: Passed [266 ms]. Total 1, Passed 1, Failed 0 each run.
- `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` PASSES deterministically across 3 consecutive runs. With the per-item `progress.Report` seam, a 3-element sequence yields the eager `0` report plus one report per consumed item (4 reports total), so `consumed == {1,2,3}`, `tracker.Reports.Count >= 2`, and at least one report `JobName.StartsWith("Consuming ")` hold independent of wall-clock timing.
