# P2-T8 — GREEN for issue #486: the five previously-red tests plus the four pins

Timestamp: 2026-08-28T00-30
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p2-t8.trx" "/TestCaseFilter:FullyQualifiedName~ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler|FullyQualifiedName~ItemViewer_DeclaresNoMenuItemCheckedChangedMembers|FullyQualifiedName~ItemViewer_DeclaresNoMoveOptionsMenuClickHandler|FullyQualifiedName~WireIntentEvents_SubscribesToPicturesChanged|FullyQualifiedName~PicturesChanged_WhenRaised_RefreshesOptionsPictures|FullyQualifiedName~Checked_WhenSetTrue_AssignsCheckedCheckBoxImage|FullyQualifiedName~Checked_WhenSetFalse_AssignsNullImage|FullyQualifiedName~Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce|FullyQualifiedName~ToolStripMenuItemCb_IsNotDerivedFromControl" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\regression-testing
EXIT_CODE: 0
ExpectedExitCode: 0

Passed: 9
Failed: 0
Skipped: 0

## Acceptance

`EXIT_CODE: 0`, 9 passed, 0 failed, 0 skipped. `Test Run Successful.` `Total tests: 9 / Passed: 9`.
TRX artifact: `evidence/regression-testing/p2-t8.trx`.

```
WireIntentEvents_SubscribesToPicturesChanged                 = Passed [239 ms]   (was RED at P1-T9)
PicturesChanged_WhenRaised_RefreshesOptionsPictures          = Passed [12 ms]    (was RED at P1-T9)
ItemViewer_DeclaresNoMenuItemCheckedChangedMembers           = Passed [3 ms]     (was RED at P1-T8)
ItemViewer_DeclaresNoMoveOptionsMenuClickHandler             = Passed [< 1 ms]   (was RED at P1-T8)
ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler   = Passed [< 1 ms]   (was RED at P1-T7)
Checked_WhenSetTrue_AssignsCheckedCheckBoxImage              = Passed [65 ms]    (pin, was green at P1-T6)
Checked_WhenSetFalse_AssignsNullImage                        = Passed [< 1 ms]   (pin, was green at P1-T6)
Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce  = Passed [1 ms]     (pin, was green at P1-T6)
ToolStripMenuItemCb_IsNotDerivedFromControl                  = Passed [< 1 ms]   (pin, was green at P1-T6)
```

## What the red-to-green transition establishes

All five previously-failing tests now pass, and none of the four pins regressed. Read together with
the P1-T6 through P1-T9 artifacts, this is a complete fail-before / pass-after record for every
issue #486 defect:

- **D1** — `ItemViewerExpanded.MenuItem_CheckedChanged` is gone in both overloads (P2-T2), and the
  four constructor calls that invoked it are gone (P2-T1). The image the `ToolStripMenuItemCb`
  setter assigns is no longer cleared by a second handler.
- **D2** — the three dead members on `ItemViewer` are gone (P2-T4): both
  `MenuItem_CheckedChanged` overloads and the empty-bodied `MoveOptionsMenu_Click`.
- **D3** — `WireIntentEvents` now subscribes `PicturesChanged` exactly once (P2-T6) and the new
  `CbxPictures_CheckedChanged` handler (P2-T5) reads the toggled value back into `_optionsPictures`,
  which is what `QfcItemController.MailActions.cs:127` consumes when filing.

The four pins passing **unchanged** before and after is the specific evidence that the deletions
removed redundancy rather than behaviour: the `Checked` setter at
`QuickFiler/Viewers/ToolStripMenuItemCb.cs:32-50` still assigns the checked image, still clears it to
`null`, and still raises the shadowed `CheckedChanged` exactly once.

## Artifact hygiene

`p2-t8.trx` was sanitised in place: 18 occurrences of the worktree root replaced with `<repo-root>`
(case-insensitively), 12 of the machine name with `<host>`, and 3 of the account name with `<user>`.
A case-insensitive search of the committed TRX for either identifier returns 0.

Output Summary: Issue #486 is **green**. All nine tests pass at `EXIT_CODE: 0` — 9 passed, 0 failed,
0 skipped — covering the five that were red at P1-T7, P1-T8 and P1-T9 and the four
`ToolStripMenuItemCb` pins that were already green at P1-T6. The pins passing identically before and
after the deletions is the evidence that the removed `MenuItem_CheckedChanged` machinery was
redundant with the setter rather than load-bearing, and the two `PicturesChanged` tests turning
green is the evidence that the seventeenth intent subscription now reaches the controller's cached
save-pictures option.
