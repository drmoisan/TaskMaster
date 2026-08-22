---
name: concurrent-dotnet-coverage-deadlock-and-doccomment-retention-gate
description: Two #445 execution hazards - a sibling worktree's dotnet-coverage collect deadlocks yours (diagnose by CPU delta, kill only your own chain), and an XML doc comment quoting a code literal breaks a retention count gate
metadata:
  type: project
---

**1. A concurrent `dotnet-coverage collect` from a sibling agent worktree deadlocks yours.**

Symptom: `dotnet-coverage collect ... -- vstest.console.exe @assemblies` produces no output file and never returns. The `testhost.exe` accrued 0.02 s of CPU across a 60-second sample (32.046875 to 32.0625) — a hang, not slow progress.

Diagnose, do not guess:
```powershell
# progress test: sample CPU twice, 60 s apart. Delta ~0 means deadlocked.
# ownership test: read the command line, which carries the absolute worktree path.
Get-CimInstance Win32_Process | Where-Object { $_.Name -eq 'testhost.exe' -or $_.Name -eq 'vstest.console.exe' } |
  ForEach-Object { "PID=$($_.ProcessId) PPID=$($_.ParentProcessId)"; $_.CommandLine }
```
`Get-CimInstance Win32_Process -Filter "Name='testhost.exe'"` throws `Invalid query` from bash; use `Where-Object` instead.

On #445 the contending workload was a full nine-assembly instrumented run from `agent-a28821f6e56934fc7` (issue #491). Two simultaneous full-solution instrumentation sessions on one machine is the cause.

Remedy: kill **only your own** chain — `dotnet-coverage` -> `vstest.console` -> `testhost` plus the parent `pwsh` runner (walk `ParentProcessId`; killing the runner matters, see [[project_timedout_mstest_leaves_detached_runner]]). Leave the sibling's processes running. Wait for the machine to clear, then re-run the **unaltered** command. It then completed in normal time with 6441/6441 passing.

**Never** respond by adding a sleep/retry/timeout to a test or by changing the command. The hang is machine contention, not a regression, and the plan forbids stabilising a test with timing hacks.

**2. An XML doc comment that quotes a code literal breaks a retention count gate.**

A plan can pin an out-of-scope expression with a retention gate, e.g. `Key.Substring(other.Length - 1, 1)` must stay at count 1 to prove a deferred defect was not "helpfully" fixed. Writing an XML doc comment that quotes that expression as `<c>Key.Substring(other.Length - 1, 1)</c>` raises the count to **2** and fails the gate — even though no code changed.

Fix by describing the expression instead of reproducing it ("branch 1's substring offset expression"). Re-run the count to confirm it returns to baseline.

**Why:** `git grep -F` is text-based and cannot distinguish a code occurrence from a comment occurrence. The gate is doing its job; the doc comment is the defect.

**How to apply:** After writing any comment or doc block on a file that carries retention gates, re-run every count gate for that file before checking the task off. Prefer prose descriptions over verbatim literals in comments on gated files. Related: [[project_multipattern_gate_shared_qualifier_detachment]], [[project_sibling_worktree_shared_tooling_hazard]].
