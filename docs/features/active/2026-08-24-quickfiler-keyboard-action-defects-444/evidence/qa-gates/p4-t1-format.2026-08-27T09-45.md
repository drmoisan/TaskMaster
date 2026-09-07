# [P4-T1] Toolchain step 1 — formatting (mutating pass over owned paths only)

Timestamp: 2026-08-27T09-45
Command: `dotnet tool run csharpier format QuickFiler\Controllers\KbdActions.cs QuickFiler\Controllers\QfcCollectionController.cs QuickFiler\Controllers\QfcItemController.Navigation.cs QuickFiler.Test\Controllers\KbdActionsTests.cs QuickFiler.Test\Controllers\KbdActionsRemainingBranchesTests.cs QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs QuickFiler.Test\Controllers\QfcItemController.NavigationTests.cs`
EXIT_CODE: 0

The seven owned paths are named explicitly. **A bare dot is never used for the mutating pass**, so no
file outside this feature's ownership can be rewritten. Three sibling children are executing
concurrently in their own worktrees; a directory-scoped or bare-dot format pass would risk rewriting
files this plan separately asserts are unmodified.

## Output (verbatim)

```
Formatted 7 files in 2888ms.
```

`Formatted N files` reports the number of files **processed**, not the number rewritten. The SHA-256
comparison below is what establishes the rewritten count.

## SHA-256 before and after (14 values)

| # | Path | Before | After | Changed |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/KbdActions.cs` | `078cb6121dc542f9dda4c7840a87963f486cc83cb4884a15018a1f2a12d84cd7` | `078cb6121dc542f9dda4c7840a87963f486cc83cb4884a15018a1f2a12d84cd7` | no |
| 2 | `QuickFiler/Controllers/QfcCollectionController.cs` | `cebe711cb917625045b9456de3905b23654ceed743270fe6f01d70970173aedd` | `cebe711cb917625045b9456de3905b23654ceed743270fe6f01d70970173aedd` | no |
| 3 | `QuickFiler/Controllers/QfcItemController.Navigation.cs` | `8a36b51893b8fcd4721977eaa39d89eeb746cc7ed24bcdeea6a0e6c1ddf49c3c` | `8a36b51893b8fcd4721977eaa39d89eeb746cc7ed24bcdeea6a0e6c1ddf49c3c` | no |
| 4 | `QuickFiler.Test/Controllers/KbdActionsTests.cs` | `b1e200e8b38f5ee95990f1257572a2522b5acc33d621baca0ff1a8312b28d289` | `b1e200e8b38f5ee95990f1257572a2522b5acc33d621baca0ff1a8312b28d289` | no |
| 5 | `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | `dd7acde95dd08e59f3edb525dc1af97d389f4cfee1bbc9fc6a3f184419932697` | `dd7acde95dd08e59f3edb525dc1af97d389f4cfee1bbc9fc6a3f184419932697` | no |
| 6 | `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | `2dfe71bca77fdc0e3d3b88daa522c4503ccf2240d791c2ad3a545a3c3f3b649b` | `2dfe71bca77fdc0e3d3b88daa522c4503ccf2240d791c2ad3a545a3c3f3b649b` | no |
| 7 | `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` | `0ab5ef51bd3e8b087e8b219dca819f87a91c2d943b5534e8f9f6795badf5962c` | `0ab5ef51bd3e8b087e8b219dca819f87a91c2d943b5534e8f9f6795badf5962c` | no |

```
REWRITTEN_FILE_COUNT = 0
```

Every before/after pair is identical, so this pass rewrote **zero** files. All seven were already
formatted by the per-phase passes at `[P1-T19]`, `[P2-T13]`, and `[P3-T16]`. That is the condition
`[P4-T12]` requires for a clean single final pass: no step of the final loop auto-fixed a file, so the
loop does not restart.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS.
- The artifact records fourteen SHA-256 values and an explicit rewritten-file count derived from
  comparing them. PASS.

Output Summary: format exit 0 over the seven explicitly named owned paths; 14 SHA-256 values recorded;
rewritten-file count 0, so the final pass is clean and no restart is triggered.
