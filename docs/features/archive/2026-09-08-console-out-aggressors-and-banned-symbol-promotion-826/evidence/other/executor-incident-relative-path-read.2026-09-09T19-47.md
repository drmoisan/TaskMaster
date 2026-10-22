# Executor incident: relative `System.IO.File` reads resolved against the wrong worktree

Timestamp: 2026-09-09T19-47

Recorded rather than absorbed. This documents a tooling mistake the executor made, its measured blast
radius, the repair, and the checks that bound the damage.

## What happened

Several `pwsh -NoProfile -Command` blocks in this run used the .NET API `[System.IO.File]::ReadAllText`
and `[System.IO.File]::ReadAllBytes` with a **repository-relative** path, after calling
`Set-Location -LiteralPath <exec worktree>`.

PowerShell's `Set-Location` changes the PowerShell provider location but does **not** change the .NET
process property `[Environment]::CurrentDirectory`, which is what `System.IO.File` resolves relative
paths against. Measured directly in this worktree:

```
PSLocation           = <exec-worktree>
EnvCurrentDirectory  = <session-worktree>
GetFullPath("docs/.../plan.2026-09-08T23-52.md")
                     = <session-worktree>\docs\...\plan.2026-09-08T23-52.md
```

So those reads came from the **shared session worktree**, while the paired writes used
`Join-Path $Root $f` or an absolute path and correctly targeted the **exec worktree**. Repository-relative
paths are used throughout this artifact set; the two absolute roots above are named here only because the
defect is a property of the difference between them.

PowerShell cmdlets are unaffected: `Get-Content -LiteralPath`, `Set-Content -LiteralPath`,
`Select-String -LiteralPath` and `Get-FileHash -LiteralPath` all resolve through the PowerShell provider
and therefore used the exec worktree throughout.

## Blast radius, measured rather than assumed

Two groups of tasks used the affected pattern.

### Group 1 — the item-1 `.cs` deletions in [P5-T1], [P5-T2], [P5-T3], [P5-T4], [P5-T5], [P5-T7], [P5-T8] and [P5-T9]

**No damage.** Proven by the anchored numstat over all tracked `*.cs` against base
`dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`: every one of the 33 item-1 files reports **0 added lines**,
with removed counts of exactly 1, 2, 6, 7, 10 or 12 as each task intended. Only two rows carry a non-zero
added count, and neither was produced by the affected pattern:

- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs` — 235 added, 0
  removed; created by the Write tool at an absolute path.
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` — 2 added, 2 removed; edited by the
  Edit tool at an absolute path.

A zero added-line count is decisive here. The write stored "the file the read returned, minus the matched
line". If the session worktree's copy had differed from the exec worktree's in any respect, the
difference would appear as added lines in the anchored diff. It does not, for any of the 33 files, so the
two copies were byte-identical at those paths. That is consistent with the epic manifest's statement that
none of the 33 files is touched by any other feature in the epic.

[P5-T6], the two `TreeNode` files, used the Edit tool at absolute paths and was never exposed.

### Group 2 — the [P8-T2] through [P8-T17] plan check-off

**Damaged and repaired.** The read returned the session worktree's copy of this plan file, which carries
no check-offs, and the write stored it to the exec worktree with only the 16 Phase 8 check-offs applied.
That reverted the 43 check-offs recorded for [P0-T1] through [P8-T1].

The damage was detected immediately by an in-band consistency check in the same command: the script
printed a remaining-unchecked count of 47 where 4 was expected. No later task depended on the file in the
interim.

Repair: the 43 reverted check-offs were re-applied with `[System.IO.File]` calls carrying the **absolute**
path, under a per-token guard requiring exactly one occurrence of each `- [ ] [<id>] ` string before
replacing it. The guard would have thrown rather than silently mis-editing.

Verification after repair, against the same base:

| Measure | Value |
|---|---|
| tasks checked | 59 |
| tasks unchecked | 4 (`P8-T18`, `P8-T19`, `P8-T20`, `P8-T21`) |
| anchored numstat for the plan file | 59 added, 59 removed |
| changed lines in the anchored diff | 118 |
| changed lines that are **not** a `- [ ]` / `- [x]` checkbox flip | **0** |

The last row is the one that matters: the entire difference between the plan file and its base version is
checkbox state. No prose, no task text, no task ID and no acceptance condition was altered, and no task
was added or removed.

## Why the file content could not have been corrupted in either direction

The session worktree's copy of this plan file is byte-identical to the exec worktree's base version.
Before the repair, the anchored diff of the clobbered file showed exactly 16 changed line pairs, all of
them Phase 8 checkbox flips, and nothing else. Had the two copies differed in content, that diff would
have carried the difference.

## Corrective practice applied for the remainder of the run

Every subsequent `System.IO.File` call uses an absolute path. Path-bearing operations otherwise use the
PowerShell provider cmdlets or the Read, Write and Edit tools, all of which resolve correctly.

Output Summary: a relative-path .NET read resolved against the shared session worktree instead of the
exec worktree. Measured blast radius: zero effect on any source file, proven by a zero added-line count
across all 33 item-1 files; one documentation file, this feature's plan, had 43 check-offs reverted and
they have been restored, with the anchored diff confirming the file differs from its base only in
checkbox state.
