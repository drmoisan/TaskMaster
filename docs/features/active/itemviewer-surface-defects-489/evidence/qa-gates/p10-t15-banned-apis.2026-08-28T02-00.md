# P10-T15 — No banned timing API in the four test files this plan creates

Timestamp: 2026-08-28T02-00
Command: (git grep -n -E "Thread\.Sleep|Task\.Delay|DateTime\.Now" -- QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs | Measure-Object).Count
EXIT_CODE: 0

## Result

```
Count=0
DollarQuestion=True
ErrorCount=0
ResidualLASTEXITCODE=1
```

RecordedCount: **0**. Acceptance met.

## Files scanned

| File | Banned-API matches |
|---|---|
| `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` | 0 |

The two `PartN` files are included beyond the two files spec AC58 names. That is a strictly wider
scan than the criterion requires and cannot weaken it: a wider scan returning zero implies the
narrower scan returns zero.

All four are tracked — P10-T1 confirms each appears in
`git diff --name-only <BASELINE_SHA> -- QuickFiler.Test/` — so `git grep` searches them. An untracked
file would be invisible to `git grep`, which is why the intent-to-add gate precedes this one.

## The zero is a real observation, not a dead pattern

A zero-match gate is only meaningful if the pattern can match. A control probe running the same
regular expression over the whole of `QuickFiler.Test/` returns matches in multiple files, including
`Controllers/KaCharTests.cs` (1), `Controllers/KaKeyTests.cs` (1),
`Controllers/QfcCollectionControllerDefects468Tests.cs` (2), `Controllers/QfcDatamodelTests.cs` (1) and
`Helper Classes/MailItemInfoTests.cs` (1). The alternation therefore matches live text in this
repository, and the zero recorded above is a property of the four scanned files rather than of the
pattern.

## How `EXIT_CODE: 0` was determined — and the plan-convention correction

The plan's § Execution conventions states that gates wrapping a `git grep` in
`(… | Measure-Object).Count` "need no such declaration because the wrapper makes the pipeline's own
exit code `0`". **That claim is false on a zero-match result**, and this gate is the second place it
shows, after P9-T8.

`git grep` exits `1` natively when it finds nothing. The `Measure-Object` wrapper changes the value of
the PowerShell expression but does not reset `$LASTEXITCODE`, which continues to carry the native exit
code of the last external program run. The measurement above shows `Count=0` alongside
`ResidualLASTEXITCODE=1`.

Success is therefore judged the way P4-T7 and P9-T8 judge it:

| Signal | Observed | Meaning |
|---|---|---|
| `$?` | `True` | The PowerShell statement completed successfully |
| `$Error.Count` under `$ErrorActionPreference = 'Stop'` | `0` | No terminating or non-terminating error was raised |
| `$LASTEXITCODE` | `1` | Residual native exit code of `git grep` on a zero-match result — **not** a failure signal for this gate |

`EXIT_CODE: 0` is recorded on the `$?` and `$Error.Count` basis, with the residual `$LASTEXITCODE = 1`
documented explicitly rather than written as a bare `0`. `$Error` was cleared and `$LASTEXITCODE` reset
to `0` immediately before the command, so the `1` is attributable to this command alone and cannot
have been inherited. The discrepancy is recorded as finding **E3** in `FEATURE/spec.md` § Out-of-Scope
Findings.

## Determinism policy

`Thread.Sleep`, `Task.Delay` and real wall-clock reads such as `DateTime.Now` are prohibited in test
code by `.claude/rules/general-unit-test.md` § Determinism Infrastructure. None of the four files this
feature created uses any of them. The theme-marshalling test achieves determinism instead through the
synchronous `Mock<IUiDispatcher>` supplied by `QfcItemControllerTestSupport.BuildSyncDispatcher()`
(P10-T8), which runs every posted delegate inline rather than waiting on a real clock or a message
pump.

Output Summary: **Zero** banned timing-API occurrences across all four test files this plan creates.
`(git grep -n -E "Thread\.Sleep|Task\.Delay|DateTime\.Now" -- <four files> | Measure-Object).Count`
records `0`. The pattern is live — the same regular expression matches in at least five other
`QuickFiler.Test/` files — so the zero is a property of the scanned files, not a dead pattern.
`EXIT_CODE: 0` is recorded on the basis of `$? = True` and `$Error.Count = 0` under
`$ErrorActionPreference = 'Stop'`; the residual `$LASTEXITCODE = 1` is the native `git grep`
zero-match exit code and is documented explicitly, the same correction P9-T8 records and finding E3
carries.
