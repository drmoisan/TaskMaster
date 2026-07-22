# P5 Line-Limit Split Preservation (consolidates P5-T159 and P5-T165)

Timestamp: 2026-07-22T13:02:21Z

Command: `wc -l <4 split files>; grep -c '\[TestMethod\]' <4 split files>; grep -c "CapturingSynchronizationContext =" QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs`

EXIT_CODE: 0

Output Summary: PASS. No contradiction; atomic replanning not required before P5-T168.

Both split pairs preserve all original test names and assertions with zero test removed or weakened:
- OpenCoordinator: 10 cases preserved (5 primary + 5 Part2). Proof: `p5-opencoordinator-split-ledger.2026-07-22T12-54.md` (P5-T159) and 10/10 pass-after `p5-opencoordinator-split-pass-after.2026-07-22T12-54.md`.
- PopupBoundary: 18 cases preserved (5 primary + 13 Part2). Proof: `p5-popupboundary-split-ledger.2026-07-22T13-00.md` (P5-T165) and 18/18 pass-after `p5-popupboundary-split-pass-after.2026-07-22T13-00.md`.

`CapturingSynchronizationContext` alias/reference: unchanged, present once (count = 1) in `BreadcrumbDropDownOpenCoordinatorTests.cs`.

Line limits: all four files at most 480 lines.
- `BreadcrumbDropDownOpenCoordinatorTests.cs` = 386.
- `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` = 144.
- `BreadcrumbPopupBoundaryCoverageTests.cs` = 361.
- `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` = 220.

Project includes: exactly one `QuickFiler.Test.csproj` `Compile Include` was added per new file (two total) — `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` and `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`.

No other production, test, or configuration file changed relative to the pre-split HEAD/worktree state: the only non-evidence, non-plan working-tree changes are the two corrected split originals, the two new `.Part2.cs` partials, and `QuickFiler.Test.csproj` (see `p5-line-limit-split-scope-inventory.2026-07-22T13-01.md`).
