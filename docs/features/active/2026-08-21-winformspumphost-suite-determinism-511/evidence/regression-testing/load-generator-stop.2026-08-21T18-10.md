# P4-T3 — CPU Load Generator Stopped

Timestamp: 2026-08-22T14-24

Command:
```powershell
Get-Counter '\Processor(_Total)\% Processor Time' -SampleInterval 1 -MaxSamples 5
$jobs | Stop-Job
$jobs | Remove-Job -Force
@(Get-Job).Count
```

The five samples were taken **immediately before** `Stop-Job`, in the same session that started the
jobs in P4-T1, inside the `finally` block that follows the tenth run.

EXIT_CODE: 0

Output Summary:

| Measure | Value |
| --- | --- |
| Pre-stop `\Processor(_Total)\% Processor Time` samples | `99.94`, `100`, `100`, `100`, `100` |
| **Mean pre-stop utilization** | **99.99** |
| Required mean | at least 80 |
| **Post-stop job count** | **0** |
| MSBuild node-reuse process count at stop | 0 |

Acceptance: the recorded mean pre-stop utilization (99.99) is at least 80 and was sampled
immediately before the load generator was stopped; the recorded post-stop job count is exactly 0.

## Bracketing, not continuous measurement

The P4-T1 and P4-T3 samples bracket the ten-run window; they are not a continuous measurement across
it. What they establish is that the generator was still loading the machine at both ends:

| Point | Timestamp | Mean utilization |
| --- | --- | --- |
| P4-T1, before run 1 | 2026-08-22T11-53 | 100.00 |
| P4-T3, after run 10 | 2026-08-22T14-24 | 99.99 |

Corroborating evidence from inside the window: the ten runs took between 399.7 s and 1471.9 s of
wall-clock time each, against a measured unloaded baseline of 55.4 s to 70.0 s for the same
nine-assembly command in P1-T4. Every run inside the window was therefore between 6x and 26x slower
than unloaded, which is consistent with sustained contention across the whole window rather than
only at its ends.
