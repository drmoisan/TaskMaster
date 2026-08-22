# Test File 500-Line Cap — Measurement and SPLIT PERFORMED (Issue #449, [P6-T14])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command: `grep -c '' <each test file>` (not `wc -l`, which under-reports by one for a file with no
terminating newline)
EXIT_CODE: 0

## Measurement that triggered the split

After [P6-T12] added the last of the Phase 6 tests and CSharpier formatted the file:

| File | Measured lines | At or above 500? |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` | **569** | **YES** |

569 is at or above 500, so [P6-T14]'s split condition fired and **the split was PERFORMED**.

## Post-split measurements — both files under the cap

| File | Measured lines | Under 500? |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` | **387** | yes |
| `QuickFiler.Test/Controllers/QfcExplorerController.ConversationViewTests.cs` | **205** | yes |

Both counts are taken AFTER the final CSharpier pass, which is the only sound point to measure: the
formatter is the authority on line count and a pre-format measurement can under-report.

Every test file in the diff measures under 500 lines.

## What was moved, and how

The conversation-view tests from [P6-T5] through [P6-T10] were moved into the second file:

- `ExplConvView_ToggleOn_WhenFlagSet_AppliesRememberedView`
- `ExplConvView_ToggleOn_WhenFlagClear_DoesNothing`
- `ExplConvView_ToggleOff_WhenConversationsNotGrouped_DoesNothing`
- `ExplConvView_ToggleOff_WhenSiblingViewMissing_CopiesAndSavesTemporaryView`
- `GetSiblingView_WhenNamedViewPresent_ReturnsIt`
- `GetSiblingView_WhenNamedViewAbsent_ReturnsNull`

The second file is a `partial class` continuation, following the repository's existing split-test
convention demonstrated by `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`
and its `.Part2.cs` sibling. Two properties of that convention were followed exactly:

1. **`[TestClass]` stays on the BASE file only.** The attribute is `AllowMultiple = false`, so
   repeating it on a second part of the same partial type is a CS0579 duplicate-attribute error. The
   continuation file declares `public partial class QfcExplorerControllerTests` with no attribute.
2. **The fixture is shared, not duplicated.** The continuation file reuses the base file's
   `[TestInitialize] Setup`, `CreateController`, and `ArrangeViewsIndexer` helpers. Duplicating a
   40-line mock fixture into a second class would have violated the General Code Change Policy's
   "Avoid copy-paste" requirement; the partial-class form avoids it entirely.

The base file's declaration was changed from `public class` to `public partial class` — a one-word
change, and the only edit to the retained tests.

## Second `<Compile Include>` line — AC-12 and AC-16 reconciliation

A second compile entry was appended in the same partitioned `Controllers` item group, adjacent to the
`QfcDatamodelLivenessTests` entry, in CRLF, without touching the `Form1` region:

```diff
     <Compile Include="Controllers\QfcDatamodelLivenessTests.cs" />
+    <Compile Include="Controllers\QfcExplorerController.ConversationViewTests.cs" />
+    <Compile Include="Controllers\QfcExplorerControllerTests.cs" />
     <Compile Include="Controllers\QfcInitEmailQueueZeroBatchTests.cs" />
```

**RECONCILIATION NOTE (carried into the [P7-T27] and [P7-T31] check-off notes):**

- **AC-12's "exactly one appended line" is SUPERSEDED by two appended lines.**
- **AC-16's project-file figure of 485 is SUPERSEDED by 486.**
  `QuickFiler.Test/QuickFiler.Test.csproj` measures **486** lines (true count via `grep -c ''`;
  `wc -l` reports 485 because the file has no terminating newline). The progression is
  484 pre-change -> 485 after [P1-T2] -> **486** after this second append.
- **Both entries sit in the `Controllers` item group adjacent to the `QfcDatamodelLivenessTests`
  entry**, at lines 120 and 121, which is 40 lines clear of the `Form1` compile region.
- **The `Form1` regions at `:161-166` and `:180-182` remain UNTOUCHED.** The full project-file diff is
  a single hunk at lines 117-123 containing exactly two added lines and no other change, verified in
  `../qa-gates/ac12-csproj-diff.2026-08-22T09-16.md`. Those regions are owned exclusively by sibling
  child #491.

The cause of the supersession is benign and mechanical: the plan's AC-12/AC-16 figures were written on
the assumption that one test file would suffice, and [P6-T14] is the plan's own provision for the case
where it does not. Honouring the 500-line cap required the second file, and the second file requires a
second compile entry.

## Note on the [P6-T15] test filter

Because the split produced a `partial class` rather than a second distinct class, both files
contribute test methods to the SAME fully-qualified type name,
`QuickFiler.Controllers.Tests.QfcExplorerControllerTests`. The [P6-T15] filter therefore needs only
that single name and does not require the `|` join that [P6-T15] anticipated for a two-class split.
`vstest.console.exe` still rejects the word `OR`; no disjunction was needed here.

## Output Summary

`QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` measured **569** lines after [P6-T12],
which is at or above 500, so **the split WAS performed**. The conversation-view tests from [P6-T5]
through [P6-T10] moved into
`QuickFiler.Test/Controllers/QfcExplorerController.ConversationViewTests.cs` as a `partial class`
continuation with `[TestClass]` retained on the base file only. Post-split, post-format measurements:
base **387** lines, continuation **205** lines — every test file in the diff is under 500. A second
`<Compile Include>` line was appended in the same `Controllers` item group adjacent to the
`QfcDatamodelLivenessTests` entry in CRLF; the `Form1` regions are untouched. **AC-12's "exactly one
appended line" is superseded by two, and AC-16's project-file figure of 485 is superseded by 486.**
