Timestamp: 2026-07-20T18-20

## Branch-gap analysis for `QfcItemController.FolderHandling.cs`

Class-level branch coverage (P0-T8 re-baseline): 73.81% (branch-rate 0.7380952380952381, complexity
42). To reach >= 75%, at least one additional previously-uncovered branch condition must be
exercised (closing one branch raises the rate to approximately 76.19%, comfortably above the 75%
floor — computed as (covered+1)/total assuming a small integer denominator consistent with the
observed rate).

### Candidate uncovered/partially-covered lines (from P0-T8's per-line Cobertura data)

| Line | Method | hits | branch | condition-coverage |
|---|---|---|---|---|
| 36 | `LoadFolderHandler` | 1 | True | 50% (4/8) |
| 49 | `LoadFolderHandler` | 1 | True | 50% (4/8) |
| **139** | **`PopulateFolderComboBox`** | **1** | **True** | **50% (1/2)** |
| 140-142 | `PopulateFolderComboBox` | 0 | False | (unreached lines inside the `InvokeRequired` true-branch) |
| 164 | `AssignFolderComboBox` | 1 | True | 50% (1/2) |
| 165-167 | `AssignFolderComboBox` | 0 | False | (unreached lines inside the `InvokeRequired` true-branch) |
| 170 | `AssignFolderComboBox` | 1 | True | 83.33% (5/6) |

### Selected target: line 139, `PopulateFolderComboBox`

```csharp
public void PopulateFolderComboBox(object varList = null)
{
    LoadFolderHandler(varList);

    if (_itemViewer.InvokeRequired)         // line 139 — TRUE branch never exercised (0 hits on 140-142)
    {
        _itemViewer.Invoke(() => AssignFolderComboBox());   // lines 140-142
    }
    else
    {
        AssignFolderComboBox();             // line 145 — exercised by the existing
    }                                       // PopulateFolderComboBox_WhenFactorySucceeds_... test
}
```

### Rationale

- **Smallest, most self-contained addition**: `PopulateFolderComboBox` is a short, already-public,
  already-tested method (one existing test,
  `PopulateFolderComboBox_WhenFactorySucceeds_LoadsHandlerAndAssignsComboFromViewer`, already covers
  the `InvokeRequired == false` / `else` branch at line 145). Only the `InvokeRequired == true`
  branch (lines 140-142) is unexercised. No new production code path is implied — this simply
  exercises an existing, pre-#392, previously-untested branch.
- **No production behavior change**: the fix requires zero edits to
  `QfcItemController.FolderHandling.cs`; only a new test is added.
- **Established pattern already in the same test project**: the identical
  `InvokeRequired == true` / `viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once())`
  pattern is already used in `QfcItemController.ViewerSetupTests.cs`,
  `QfcItemController.ConversationTests.cs`, `QfcItemController.FocusAndThemeTests.cs`,
  `QfcItemController.SeamCoreTests.cs`, and `QfcItemController.NavigationTests.cs` — this is a
  well-established, low-risk seam-verification idiom in this codebase, not a novel test technique.
- **Alternative considered and rejected**: line 164 in `AssignFolderComboBox` has the identical
  `InvokeRequired == true` / recursive-`Invoke`-then-`return` shape and would equally close the gap.
  `PopulateFolderComboBox` was chosen because it is the simpler of the two call sites (a single
  guard clause with no downstream state to additionally assert on), minimizing the size and risk of
  the new test. Only one new test is authored in P1-T3 (the plan authorizes "1-2"); a second test
  for line 164 is not needed to clear the 75% floor and is not added, per the plan's "smallest ...
  addition" instruction.

### Expected effect

Exercising line 139's true branch (lines 140-142) is expected to raise the class-level branch-rate
from 73.81% to approximately 76.19% (one additional branch-condition covered out of the same
denominator), which clears the >= 75% floor with headroom. This will be confirmed numerically by
P2-T4/P2-T5/P2-T6 after the new test is authored (P1-T3) and passes (P1-T4).
