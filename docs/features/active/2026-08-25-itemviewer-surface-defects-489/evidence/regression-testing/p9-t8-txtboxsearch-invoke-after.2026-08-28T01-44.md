# P9-T8 — Post-change occurrence count of `TxtboxSearch.Invoke`

Timestamp: 2026-08-28T01-44
Command: (git grep -F -n "TxtboxSearch.Invoke" -- QuickFiler/Viewers/ | Measure-Object).Count
EXIT_CODE: 0

## Result

```
Count=0
DollarQuestion=True
ErrorCount=0
ResidualLASTEXITCODE=1
```

RecordedCount: **0**
P9-T1 pre-change count: **1** (`QuickFiler/Viewers/ItemViewer.FolderSearch.cs:79`)

Acceptance met: the recorded count is `0` against the `1` recorded in P9-T1, and `EXIT_CODE: 0`.

## How `EXIT_CODE: 0` was determined — and a correction to the plan

The plan's § Execution conventions states, under **Expected exit codes**, that gates wrapping a
`git grep` in `(... | Measure-Object).Count` "need no such declaration because the wrapper makes the
pipeline's own exit code `0`". **That claim is false on a zero-match result and it is false here.**

`git grep` exits `1` natively when it finds nothing. Wrapping the pipeline in
`(... | Measure-Object).Count` changes the value of the PowerShell **expression**, but it does not
reset `$LASTEXITCODE`, which continues to carry the native exit code of the last external program
run. The measurement above proves it directly: `Count=0` while `ResidualLASTEXITCODE=1`.

This artifact therefore judges success the way P4-T7 did, and not from `$LASTEXITCODE`:

| Signal | Observed | Meaning |
|---|---|---|
| `$?` | `True` | The PowerShell statement completed successfully |
| `$Error.Count` under `$ErrorActionPreference = 'Stop'` | `0` | No terminating or non-terminating error was raised |
| `$LASTEXITCODE` | `1` | Residual native exit code of `git grep` on a zero-match result — **not** a failure signal for this gate |

`EXIT_CODE: 0` above is recorded on the `$?` and `$Error.Count` basis, with the residual
`$LASTEXITCODE = 1` documented explicitly rather than silently normalised. `$Error` was cleared
immediately before the command and `$LASTEXITCODE` was reset to `0`, so the `1` observed afterwards
is attributable to this command alone and cannot have been inherited.

The same correction applies to P10-T15, which uses the identical idiom and likewise expects a
zero-match result. It does **not** affect P0-T16, P0-T17 or P4-T7 as executed: P0-T16 and P0-T17
returned non-zero match counts, so `git grep` exited `0` natively there, and P4-T7 already judged
success on this basis.

This discrepancy between the plan's stated convention and the observed behaviour is recorded as a
finding for the Phase 10 out-of-scope record. No plan task was added and no plan text was edited.

## What this proves

This is the pass-after half of the fail-before / pass-after pair for the AC31 zero-match assertion.
The gate is falsifiable: the literal stood at exactly `1` before P9-T2 ran (P9-T1,
`FEATURE/evidence/regression-testing/p9-t1-txtboxsearch-invoke-before.2026-08-28T01-37.md`), so had
P9-T2 not changed `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`, this task would record `1` and
fail. `FocusSearch()` is now the bare forward `public void FocusSearch() => TxtboxSearch.Focus();`
and no `Control.Invoke` call remains anywhere under `QuickFiler/Viewers/` for that control.

Output Summary: `(git grep -F -n "TxtboxSearch.Invoke" -- QuickFiler/Viewers/ | Measure-Object).Count`
records **0**, against the **1** recorded by P9-T1, so the AC31 zero-match assertion holds and the
fail-before / pass-after pair is complete. `EXIT_CODE: 0` is recorded on the basis of `$? = True` and
`$Error.Count = 0` under `$ErrorActionPreference = 'Stop'`; the residual `$LASTEXITCODE = 1` is the
native `git grep` zero-match exit code and is documented explicitly, correcting the plan's claim that
the `Measure-Object` wrapper forces the pipeline exit code to `0`.
