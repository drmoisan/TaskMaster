# Phase 0 — Baseline line counts of the eleven write-set .cs files

Timestamp: 2026-09-07T01-01
Task: [P0-T11]
Issue: #798

## Counting method

Each count is the number of lines returned by reading the file's lines, which is the number of
content lines and does not count a phantom final empty line after a trailing newline. This method
reproduces the values the plan pins, so the plan's counts and the counts recorded here are on the
same basis. P7-T4, P7-T8 and P8-T9 re-run this audit with the same method.

## Existing files, five in total

- `UtilitiesCS/Extensions/DfDeedle.cs`=410
- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`=154
- `QuickFiler/Controllers/QfcDatamodel.cs`=483
- `TaskMaster/Ribbon/RibbonViewer.cs`=388
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`=882

## Comparison against the values the plan pins

| Path | Plan pins | Observed | Match |
|---|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs` | 410 | 410 | yes |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 154 | 154 | yes |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 483 | 483 | yes |
| `TaskMaster/Ribbon/RibbonViewer.cs` | 388 | 388 | yes |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 882 | 882 | yes |

All five observed counts equal the pinned values, so the worktree is at the base commit and the
plan's line citations are valid against this tree. No re-verification of the plan is required.

## Files this change creates, six in total

- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`=ABSENT
- `TaskMaster/Ribbon/RibbonCommandBoundary.cs`=ABSENT
- `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`=ABSENT
- `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs`=ABSENT
- `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs`=ABSENT
- `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs`=ABSENT

## Downstream thresholds these counts fix

- P1-T2 requires `UtilitiesCS/Extensions/DfDeedle.cs` to fall strictly below 410 once the four
  column methods are moved out.
- P1-T11 requires `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` to fall strictly below 882
  once the reflection helper and its two call sites are removed.
- P7-T4 and P8-T9 hold every write-set file other than
  `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` at or below the 500-line cap.
  `QuickFiler/Controllers/QfcDatamodel.cs` starts at 483, which leaves 17 lines of headroom; that
  file is edited only by P5-T2, which converts two `throw e;` statements to `throw;` and adds no
  line.

Output Summary: All five existing write-set `.cs` files were counted and every count matches the
value the plan pins: 410, 154, 483, 388 and 882 respectively. The six files this change creates are
recorded as ABSENT. The worktree is confirmed to be at the base commit.
