# P4-T1 — CPU Load Generator Started

Timestamp: 2026-08-22T11-53

Command:
```powershell
$jobCount = [Environment]::ProcessorCount - 1
$loop = { $x = 1; while ($true) { $x = ($x * 1103515245 + 12345) % 2147483647 } }
$jobs = @()
for ($i = 0; $i -lt $jobCount; $i++) { $jobs += Start-Job -ScriptBlock $loop }

Get-Counter '\Processor(_Total)\% Processor Time' -SampleInterval 1 -MaxSamples 5
```

P4-T1, P4-T2 and P4-T3 run inside a **single** `pwsh -NoProfile` session. PowerShell background jobs
are session-scoped, so a job started in one process dies when that process exits; running the three
tasks in one session is what keeps the generator loading the machine across the whole ten-run window
and is what makes P4-T3's `Stop-Job` / `Remove-Job` operate on the jobs P4-T1 started.

EXIT_CODE: 0

Output Summary:

| Measure | Value |
| --- | --- |
| `[Environment]::ProcessorCount` | 24 |
| Required job count (`ProcessorCount - 1`) | 23 |
| **Jobs actually started** | **23** |
| Sampled `\Processor(_Total)\% Processor Time` | `100`, `100`, `100`, `100`, `100` |
| **Mean of the five samples** | **100.00** |
| Required mean | at least 80 |
| MSBuild node-reuse process count at start | 0 |

Acceptance: the recorded job count (23) equals `[Environment]::ProcessorCount - 1` (24 - 1 = 23),
and the mean of the five sampled utilization values (100.00) is at least 80.

Each job runs a pure integer busy loop with no `Start-Sleep`, no wait, and no file I/O. The
generator is test-harness scaffolding executed in the runner session; it introduces no sleep, retry,
or timing tolerance into any test, and it creates no temporary file.

## Recorded interruption and restart

An earlier attempt at this window began at 2026-08-22T10-42 with 23 jobs, a sampled mean of 99.99,
and **17** MSBuild node-reuse processes resident. It completed three runs
(6439 / 6439 passed on each, durations 447.8 s, 861.3 s and 1282.9 s) and was then terminated
mid-run-4 when its host process was stopped by the session's background-task lifecycle. Its 23 load
jobs were children of that process and died with it; a process inventory taken immediately
afterwards confirmed zero surviving load jobs, zero `testhost`, zero `vstest.console` and zero
`MSBuild` processes, so no orphan carried into the restart.

The window was restarted from scratch rather than resumed, because P4-T2's ten runs must all sit
inside one continuous load window for the load figures at the two ends to bracket them. The
`p4-t2` results directory was cleared before the restart, so the three TRX files from the abandoned
attempt are not present and cannot be mistaken for part of the ten. The restarted runner was
launched detached, outside the background-task lifecycle, and survived to completion.

The abandoned attempt is recorded here rather than discarded because the plan forbids obtaining a
result without recording every attempt. Its three runs were green; it produced no failure that this
restart conceals.

## Additional condition recorded: MSBuild node-reuse process count

The plan mandates a CPU load generator. The condition empirically observed to reproduce the #511
failure during Phase 0 was different: the one genuine pre-fix failing run
(6430 / 6437, seven 60,000 ms `PumpTimeoutMs` expiries including both named tests) differed from the
passing runs either side of it only in the presence of **17 idle MSBuild node-reuse processes**.
Clearing them restored 6437 / 6437.

**MSBuild node-reuse process count at load-generator start on the completed window: 0.** The 17
nodes left by the P2-T6 and P3-T3 rebuilds reached their idle timeout and exited during the
abandoned attempt, so the ten completed runs all executed with a node count of 0. The per-run count
is recorded in the P4-T6 consolidated artifact, and the gap between the plan's mandated CPU-load
condition and the empirically observed reproduction condition is closed by the separate
supplementary ten-run record
`supplementary-msbuild-node-contention-ten-runs.2026-08-21T18-10.md`.
