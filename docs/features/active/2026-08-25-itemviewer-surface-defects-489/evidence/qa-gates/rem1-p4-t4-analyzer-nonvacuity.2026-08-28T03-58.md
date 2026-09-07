# P4-T4 — Analyzer non-vacuity proof (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T03-58
Task: [P4-T4]
LoopIteration: 1
Command: Select-String -SimpleMatch over docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates\rem1-p4-t3-analyzer-build.2026-08-28T03-57.msbuild.txt for the CoreCompile skip literal and for (Rebuild target)
EXIT_CODE: 0

## Why this task exists

A warm local worktree can make an analyzer gate vacuous. MSBuild's incremental up-to-date check
compares timestamps and does **not** invalidate on a command-line `/p:` change, so a `/t:Build` over a
warm tree returns exit `0` having skipped `CoreCompile` on every project — no compilation, therefore no
analyzers, therefore a gate that cannot fail. `/t:Rebuild` is used precisely to defeat that, and this
task measures whether it worked rather than assuming it.

## Measured over the P4-T3 log

| Signal | Count |
|---|---:|
| Occurrences of the literal `Skipping target "CoreCompile"` | **0** |
| `(Rebuild target)` entries | 10 |
| `csc.exe /noconfig` compiler invocations | 18 |
| Total log lines scanned | 11775 |

**Zero** skip occurrences. Ten `(Rebuild target)` entries confirm the Rebuild target was entered, and
eighteen real C# compiler invocations confirm that compilation — and therefore analysis — actually
ran. The gate is not vacuous.

### How the verdict was judged

Per convention 6, the verdict is taken from the **reported count**, not from an exit code. A search
that finds nothing exits non-zero in most search tools, and piping such a command into a counting
stage does not clear the last-exit-code variable, so a zero-match result can leave a stale `1` behind.
The measurement above was performed in PowerShell under `$ErrorActionPreference = 'Stop'` with the
match counts materialised as array lengths; that block reported `$?` = `True` and `$Error.Count` = `0`,
and the skip count it produced is `0`. `EXIT_CODE: 0` above is the exit code of that PowerShell
measurement block, which completed without error — it is not a search tool's zero-match code, and no
bare zero is being claimed without this explanation.

### Corroboration across the cycle

The same two signals were measured on every build in this remediation, and they agree:

| Build | Skip literal | `(Rebuild target)` | `csc.exe` invocations |
|---|---:|---:|---:|
| P1-T2 (pre-fix compile) | 0 | 10 | 18 |
| P2-T2 (post-fix compile) | 0 | 10 | 18 |
| P4-T3 (analyzer gate) | **0** | 10 | 18 |

## Acceptance

| P4-T4 condition | Result |
|---|---|
| Exactly 0 occurrences of `Skipping target "CoreCompile"`, judged from the reported count | **Yes** — reported count 0 |

Output Summary: The analyzer gate is provably non-vacuous. The P4-T3 log contains **0** occurrences of
the literal `Skipping target "CoreCompile"` across 11775 lines, alongside 10 `(Rebuild target)` entries
and 18 `csc.exe /noconfig` compiler invocations, so `CoreCompile` ran on every project and the
analyzers ran with it. The verdict is taken from the reported match count, not from a search tool's
exit code, and the PowerShell measurement block that produced it reported `$?` = `True` and
`$Error.Count` = `0` under `$ErrorActionPreference = 'Stop'`. The same figures were observed on the
P1-T2 and P2-T2 builds, so the result is consistent across the whole cycle.
