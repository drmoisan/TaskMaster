# R2 Line 248 — The Issue #902 Rejection Handler

- Timestamp: 2026-09-20T08-40-10
- Task: [P1-T4]
- Finding: R2. **This is the priority case of R2.**
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`,
  `<FILTER>` = `*R2- warns and records no repair*`
- EXIT_CODE: 0

## Test Added

`It 'R2- warns and records no repair when no asset folder the target framework can consume ships the file'`

Drives `Get-HintPathRepair` over:

- a project text carrying one stale `<HintPath>` bound to `Contoso.Widgets.1.0.0`;
- a version map declaring `Contoso.Widgets` at `2.0.0`, so the identifier matches the restore
  folder at a different version;
- a seam whose `TestPath` returns `$false` for both the current and the candidate hint path, and
  `$true` for every absolute probe, so the library directory and the required file inside the one
  offered asset folder are both found;
- a seam whose `ListAssetFolder` offers only `@('netstandard2.1')`.

The discrimination between the two `TestPath` classes is `$Path.Contains('..')`: the current and
candidate hint paths are composed relative to the project directory and carry `..`, while the
library-directory and required-file probes are composed from the packages directory and do not.
The rejection is therefore the **compatibility gate's decision**, not a missing file.

## Counts Line, Verbatim

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=9 NotRun=8
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `Total` (context) | — | 9 | — |
| `NotRun` (context) | — | 8 | — |

## The Captured Warning, Verbatim

Reproduced by an independent direct invocation of the same function with the same fixture, so the
exact text is on the record rather than only the substring the assertion matches:

```
Cannot resolve Contoso.Widgets.dll from Contoso.Widgets.2.0.0; no asset folder the target framework can consume ships it.
```

That direct invocation also recorded `REPAIRCOUNT=0` and `WARNCOUNT=1`.

The test asserts three things: the repair set is empty, the warning set is **non-empty** — which is
what stops the text assertion from passing vacuously over an empty collection — and the joined
warning text contains the exact fragment
`no asset folder the target framework can consume ships it`.

## What This Exercises

**Line 248**, the `Write-Warning` in `Get-HintPathRepair` that fires when
`Resolve-PackageAssetFolder` returns empty for the corrected target folder.

This is the handler for the exact condition issue **#902** introduced. AC7 asserts only what the
shared selector **returns** for an unconsumable asset set; nothing asserted what the script does
when it receives that answer. This test closes that gap.

**How it fails.** Three distinct defects fail this assertion:

1. the script binds an unconsumable asset folder — the repair set would be non-empty;
2. the script emits no warning — the warning set would be empty and the non-empty guard would fire;
3. the script produces a repair record silently — the count assertion would fire.

The first is the regression issue #902 was: an ordered preference array that ranked an
unconsumable framework last and therefore still selected it when nothing else was offered.

## Output Summary

One test added, one executed, one passed, exit 0. Line 248 is now reached, the warning text is
recorded verbatim, and the no-repair and non-empty-warning properties are both asserted.
