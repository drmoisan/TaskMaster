# Constrained-Implementation Brief (P1-T1)

Timestamp: 2026-09-01T15-51

This is a brief, not a delegation. The executor has no agent-invocation tool,
and P1-T2 through P1-T9 already specify every edit completely, so the edits are
performed directly by this executor and no delegation is required.

## The four in-scope files

| File | Change |
|---|---|
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | rename the constant at `:15` to `BannerRejectionPrefix` keeping the value `"==="`; update the two `StartsWith` call sites at `:49` and `:75`; rewrite the constant's XML doc |
| `QuickFiler/Controllers/EfcFormController.cs` | replace the `SelectedFolder` comment at `:318-320`; comment only, no code change |
| `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` | delete the `BannerPrefix` declaration at `:16`; qualify the single reader at `:197` |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | add one `[TestMethod]` |

## The two files asserted unmodified

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` (AC5b). Feature
  #498's acceptance criteria assert this file is unmodified, so this work may
  only add a reader to its existing public constant.
- `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (AC7).

## Prohibitions

- **No `.csproj` edit anywhere.** `QuickFiler.Test/QuickFiler.Test.csproj:63`
  already carries `<Compile Include="Controllers\EfcSelectionGuardTests.cs" />`.
- **No new file is created by this plan.** The new test method is added to an
  existing file.
- Everything under `docs/features/active/efc-controller-surface-defects-464/` is
  a closed-feature audit record and is read-only.
- `artifacts/orchestration/orchestrator-state.json` carries a local
  skip-worktree flag. Do not run `git update-index` for any reason.

## The Directional Constraint (reproduced from the plan)

The issue's Expected Behavior reads as though the three-character and
four-character banner prefixes should be unified upward to four characters.
**That reading is wrong and would be a behavioural regression. This plan
prohibits it.**

The mechanism, verified against the current tree:

- `QuickFiler/Controllers/EfcSelectionGuard.cs:15` declares
  `private const string BannerPrefix = "===";` — three characters.
- `QuickFiler/Controllers/EfcFormController.cs:1146` classifies banner rows
  through `UtilitiesCS.OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix`,
  which is `"===="` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`).
  `IsBannerRow("===")` is therefore false.
- `EfcSelectionGuard.IsValidCreationSelection` (`EfcSelectionGuard.cs:66-77`)
  tests `value.Length >= MinimumCreationLength` where `MinimumCreationLength = 3`
  (`:22`). For the input `"==="` that comparison is `3 >= 3`, which is true, so
  the length rule rejects nothing.
- The three-character prefix at `:49` and `:75` is therefore the **only**
  mechanism rejecting a three-equals row at either EFC classification site.

Widening the guard constant to the producers' four-character value makes
`IsValidFilingSelection("===")` and `IsSelectableFolder("===")` both return true
and fails `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:463`
(`creationPath.Should().BeFalse(...)`). The sibling assertion on `:462`, which
reads like the consistency guard, still passes under that edit, so the
relaxation would not be caught by the assertion a reader would expect to catch
it.

**Mandated direction: the guard's constant keeps the value `"==="` exactly.**
Only its name and its XML documentation change. Any edit that alters that value,
or that widens either `StartsWith` comparison, is out of scope and must be
reverted.
