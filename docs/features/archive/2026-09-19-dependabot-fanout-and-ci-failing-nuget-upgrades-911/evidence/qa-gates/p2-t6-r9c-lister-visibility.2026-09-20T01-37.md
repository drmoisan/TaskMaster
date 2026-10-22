# R9c — Enumerated-Directory Count in the Default Manifest Lister

- Timestamp: 2026-09-20T08-52-07
- Task: [P2-T6]
- Finding: R9c, decision D4
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/dependencies/DependabotConfig.Tests.ps1`,
  `<FILTER>` = `*R9c- records the enumerated directory count*`
- EXIT_CODE: 0

## The Production Change

`$script:DefaultFileLister` in `scripts/dependencies/Repair-PackageManifestConsistency.ps1` now
binds its result to `$file` and emits a verbose record before returning it:

```powershell
    # Discovery reaches the root and its immediate subdirectories only, so a project nested
    # deeper is skipped. This record does not prevent that; it makes the shortfall observable
    # in the run log, which is decision D4 for finding R9c. Widening the walk would change
    # which manifests the production pass discovers and no test covers that change.
    Write-Verbose ('Manifest discovery: enumerated directories {0}, returned files {1}' -f ($directory.Count + 1), $file.Count)
    return $file
```

The directory count is `$directory.Count + 1`, the pruned subdirectory list plus the root itself,
which is exactly the set the enumeration walks.

## Counts Line, Verbatim

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=12 NotRun=11
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `EXIT_CODE` | 0 | **0** | PASS |

## The Test Was Observed Red Before the Production Edit

The assertion is a **text assertion over production source** and is described as such in the test
body: it observes what the file says, not what a run of it does.

It was run **before** the production edit and failed:

```
PESTER Passed=0 Failed=1 Skipped=0 Executed=1 Total=12 NotRun=11
FAILMSG: Expected like wildcard '*Write-Verbose*' to match '$script:DefaultFileLister = {
```

That red observation is what makes it a real gate rather than a restatement of an edit already
made. It was then run again after the edit and passed.

The test isolates the lister block by finding the `$script:DefaultFileLister = {` assignment and
the first closing brace in column one, and asserts both boundaries were found **before** asserting
over the block. Those two guards are what stop an unfound or undelimited block from satisfying the
two `-BeLike` assertions vacuously.

## Decision D4, Stated Plainly

The review offered two discharges: a recursive walk with the existing prune list, or a verbose
enumerated-directory count.

**The verbose count was chosen. It does not prevent a nested project from being skipped.** It
makes the shortfall observable in the run log, which is what the review asked for as its second
option.

The recursive walk was rejected because it changes **which manifests the production pass
discovers** — a behaviour change with no covering test, in a cycle whose purpose is to close a
review. All 18 current manifests sit at depth one, so the risk it would address is latent rather
than active.

The residual is therefore unchanged and is stated rather than implied: a project nested two levels
deep is still skipped by manifest discovery. What changes is that the run log now carries the
directory and file counts, so the shortfall is visible to anyone who runs the pass with
`-Verbose`.

## File Size

`scripts/dependencies/Repair-PackageManifestConsistency.ps1` measures **470** lines, against its
[P0-T5] baseline of 498 and the 500-line cap.

## Output Summary

One test added and one production edit made. The test was red before the edit and green after it,
`Executed=1 Passed=1`, exit 0. The discharge makes the one-level-deep discovery shortfall visible
and does not remove it.
