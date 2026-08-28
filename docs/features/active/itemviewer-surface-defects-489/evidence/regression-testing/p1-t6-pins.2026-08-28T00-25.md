# P1-T6 — ToolStripMenuItemCb pins (must pass before the fix)

Timestamp: 2026-08-28T00-25
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p1-t6.trx" "/TestCaseFilter:FullyQualifiedName~Checked_WhenSetTrue_AssignsCheckedCheckBoxImage|FullyQualifiedName~Checked_WhenSetFalse_AssignsNullImage|FullyQualifiedName~Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce|FullyQualifiedName~ToolStripMenuItemCb_IsNotDerivedFromControl" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\regression-testing
EXIT_CODE: 0
ExpectedExitCode: 0

Passed: 4
Failed: 0
Skipped: 0

## Acceptance

`EXIT_CODE: 0`, 4 passed, 0 failed, 0 skipped. `Test Run Successful.` `Total tests: 4 / Passed: 4`
in 1.25 seconds. TRX artifact: `evidence/regression-testing/p1-t6.trx`.

Per-test outcome read from the run:

```
Checked_WhenSetTrue_AssignsCheckedCheckBoxImage              = Passed [109 ms]
Checked_WhenSetFalse_AssignsNullImage                        = Passed [< 1 ms]
Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce  = Passed [1 ms]
ToolStripMenuItemCb_IsNotDerivedFromControl                  = Passed [< 1 ms]
```

These four pin the already-correct setter behaviour at `QuickFiler/Viewers/ToolStripMenuItemCb.cs:32-50`
**before** the Phase 2 deletions, which is the point of running them now: the `Checked` setter
already assigns `Properties.Resources.CheckBoxChecked` on `true` and `null` on `false` and raises
the shadowed `CheckedChanged` exactly once, so the four `MenuItem_CheckedChanged` calls and the two
overloads Phase 2 deletes are redundant rather than load-bearing. P2-T8 re-runs all four after the
deletions; any change in these results would mean the deletions removed live behaviour.

The fourth test records that `ToolStripMenuItemCb` derives from `ToolStripMenuItem` and `Component`
but **not** from `Control`, which is why constructing it directly needs no window handle and does
not trip the structural guard at `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:16`.

## Artifact hygiene

`p1-t6.trx` was sanitised in place: 8 occurrences of the worktree root replaced with `<repo-root>`
(case-insensitively, because vstest writes `storage=` in all-lower-case), 7 of the machine name with
`<host>`, and 3 of the account name with `<user>`. A case-insensitive search of the committed TRX
for either identifier returns 0. No `/EnableCodeCoverage` was passed, so no host-named `.coverage`
attachment directory was produced.

Output Summary: All four `ToolStripMenuItemCb` pins **pass before the fix**, at `EXIT_CODE: 0` with
4 passed / 0 failed / 0 skipped in 1.25 seconds. They establish that the `Checked` setter already
assigns the checked check-box image, already clears it to `null`, and already raises the shadowed
`CheckedChanged` exactly once, so the redundant `MenuItem_CheckedChanged` machinery Phase 2 deletes
carries no behaviour of its own. `p1-t6.trx` is written to the canonical regression-testing evidence
directory and sanitised of every account, machine and absolute-path identifier.
