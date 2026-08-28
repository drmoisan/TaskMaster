# P8-T1 — Toolchain Step 1, Mutating Format Pass (scoped)

Timestamp: 2026-08-26T11-23

Pass number: **3** — the final pass. See `p8-t6-clean-pass.md` for the full pass history.

Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier format QuickFiler/Controllers/BreadcrumbBridgeRouter.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

- Exit code: **0**
- CSharpier console output: `Formatted 16 files in 8131ms.`
- **Files CSharpier REWROTE: 0**

### How the rewrite count was determined

The acceptance condition requires the rewrite count to be determined by SHA-256 comparison, not by
reading CSharpier's own processed-file count. CSharpier's `Formatted 16 files` line is a count of files
PROCESSED, not files changed, and would misreport this gate as 16 rewrites on every pass.

A SHA-256 hash of each of the 16 listed files was taken with `Get-FileHash -Algorithm SHA256`
immediately before the command and again immediately after, and the two lists were compared pairwise.
On this pass **zero** hashes changed: the comparison printed `REWRITTEN_COUNT: 0` with no `REWRITTEN:`
lines. The before/after hash lists were held in the session scratchpad outside the repository and were
not written under `evidence/`.

### Rewrites observed on earlier passes (recorded, per the task text)

| Pass | Files rewritten | Which | Consequence |
|---:|---:|---|---|
| 1 | 0 | none | `P8-T2` to `P8-T5` all clean; the pass was later invalidated by the `P8-T7` coverage finding |
| 2 | **1** | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` | forced an immediate restart from `P8-T1` |
| 3 | 0 | none | terminal pass |

The pass-2 rewrite was of a file this plan DOES write, and it was caused by this plan's own remediation:
three tests were appended to that owned file to close the `BreadcrumbStateModel.Row.cs` changed-line
coverage gap identified by `P8-T7` (89.56% against a 90.00% floor). CSharpier reflowed the appended
code. Because CSharpier is idempotent, pass 3 rewrote zero files and the loop terminated, exactly as the
task text predicts.

No listed file was rewritten that no task in this plan wrote.

### Target-list composition

The plan's `P8-T1` list of 13 files was extended, as the task directs, with every additional new
partial-class sibling created in `P6-T11`, `P6-T15` or `P7-T1`:

| Added path | Created by | Note |
|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | Phase 6 | new partial sibling |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` | Phase 6 | new partial sibling |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` | Phase 6 | new partial sibling |

`P7-T1` created no new sibling (no owned file exceeded 500 lines), so it contributed no path. Total
target list: **16 files**.

The list names individual files and no directory, exactly as the task requires. Consequently no
MUST-NOT-WRITE file was touched: `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`,
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`,
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs` and
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` all remain clean, which
`P7-T4`, `P7-T8`, `P7-T5` and `P7-T6` each verified with a `git status` check, and no sibling epic
child's file entered this feature's diff (`P7-T3`).

`QuickFiler/Controllers/KeyboardHandler.cs` and `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
were excluded because no task in this plan writes them.
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` and
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` were RETAINED per the task text even
though no task as written adds to either; both hashed identical before and after on every pass,
consistent with their being unwritten.
`QuickFiler/Resources/FolderBreadcrumb.html` was excluded because it is not a CSharpier input.

### Loop consequence

Zero files were rewritten on this pass, so this step does not force a further `P8-T6` restart.
