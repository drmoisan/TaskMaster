# P7-T2 — File-Size Criterion (AC-31)

Timestamp: 2026-08-26T10-56

Command: `pwsh -NoProfile -Command '$w = @(<18 written files>); "=== WRITTEN ==="; foreach ($p in $w) { "{0}|{1}" -f $p, (Get-Content -LiteralPath $p).Count }; "=== PREEXISTING VIOLATIONS ==="; $g = @("UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs","QuickFiler/Controllers/EfcFormController.cs","QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs"); foreach ($p in $g) { "{0}|{1}|{2}" -f $p, (Get-Content -LiteralPath $p).Count, (git hash-object $p) }; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**PASS.** Every source file written by this plan measures at or under 500 lines. The three PRE-EXISTING
500-line violations are byte-identical to their `P0-T16` baseline, proving this feature neither worsened
nor repaired any of them.

The written-file set was derived mechanically, not from memory:
`git diff --name-status 61edc19befcf6c4e95b5acd32542f2dcdab41b78 HEAD -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test *.csproj`,
where `61edc19b...` is the `P0-T10` baseline commit. It returned 18 paths: 5 added, 13 modified.

### A. Source files written by this plan — one row per file

| # | File | Lines now | Limit | Result |
|---:|---|---:|---:|---|
| 1 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 310 | 500 | PASS |
| 2 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` (new) | 211 | 500 | PASS |
| 3 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` (new) | 204 | 500 | PASS |
| 4 | `QuickFiler/Resources/FolderBreadcrumb.html` | 490 | 500 | PASS |
| 5 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | 462 | 500 | PASS |
| 6 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` (new) | 368 | 500 | PASS |
| 7 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | 462 | 500 | PASS |
| 8 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` (new) | 261 | 500 | PASS |
| 9 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 248 | 500 | PASS |
| 10 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` (new) | 384 | 500 | PASS |
| 11 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 489 | 500 | PASS |
| 12 | `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 141 | 500 | PASS |
| 13 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` | 395 | 500 | PASS |
| 14 | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | 495 | 500 | PASS |
| 15 | `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | 479 | 500 | PASS |

All fifteen rows are at or under 500. Maximum observed value: **495**.

Three owned files this plan did NOT write are absent from the list above and are unchanged at HEAD:
`QuickFiler/Controllers/KeyboardHandler.cs` (414), `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`
(361, additions forbidden by `P6-T3`), and
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` (379).

### B. Project files edited (build configuration, not source)

The three project files received `Compile Include` entries only. The 500-line limit in
`.claude/rules/general-code-change.md` governs "production code, test code, or reusable script file";
an MSBuild project file is build configuration and is not in that set. They are recorded here for
completeness and to prove the edits were minimal and did not rewrite line endings.

| Project file | Baseline lines (`P0-T10` commit) | Lines now | Delta | `git diff --numstat` |
|---|---:|---:|---:|---|
| `QuickFiler/QuickFiler.csproj` | 595 | 597 | +2 | 3 added / 1 removed |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 477 | 479 | +2 | 3 added / 1 removed |
| `UtilitiesCS/UtilitiesCS.csproj` | 1314 | 1315 | +1 | 2 added / 1 removed |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 971 | 971 | 0 | not edited |

CRLF preservation confirmed: a CR-terminated-line count returns 597 of 597, 479 of 479 and 1315 of 1315
respectively, so every line in each edited project file is still CR-terminated. The per-file numstat
figures (single-digit) prove no whole-file line-ending rewrite occurred; a stream-editor rewrite would
have produced a several-hundred-line diff.

### C. The three PRE-EXISTING 500-line violations — unowned, unwritten, unworsened

The binding gate is equality with the `P0-T16` baseline captured in this same execution. Both the line
count and the `git hash-object` object id match exactly, which is a stronger statement than line-count
equality: the file content is byte-identical.

| File | `P0-T16` baseline lines | Measured now | `P0-T16` baseline object id | Measured object id | Verdict |
|---|---:|---:|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1000 | 1000 | `8e9c834c5b5eac7ae238f3c8969392e310ff0d41` | `8e9c834c5b5eac7ae238f3c8969392e310ff0d41` | UNCHANGED |
| `QuickFiler/Controllers/EfcFormController.cs` | 1084 | 1084 | `836c013ca3667bd4c35a6478c2ae449156df5259` | `836c013ca3667bd4c35a6478c2ae449156df5259` | UNCHANGED |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 694 | 694 | `57af52e2ff05729e537274e8b14a00b0b00b6189` | `57af52e2ff05729e537274e8b14a00b0b00b6189` | UNCHANGED |

`FolderPredictor.cs` is owned but not written (decision D5); the other two are MUST-NOT-WRITE. All three
measured counts equal their `P0-T16` baseline counts, which is the binding condition.

### D. Re-measurement of the plan's advisory "File-Size Constraint" figures, and why they are superseded

The plan's `P7-T2` prose quotes eight advisory figures. Those figures are **pre-change baselines** —
the counts measured at plan-revision time, before Phases 1 through 6 executed — and the plan states
explicitly that they are "advisory context only" and that "the binding gate is equality with the
`P0-T16` baseline captured in this same execution". They agree exactly with `P0-T16` as pre-change
values. They are superseded as post-change values because Phase 1 split one file and Phases 2 through 6
added code to, and created siblings of, several others. The plan's prose table was deliberately NOT
edited to match; the measured reality is recorded here instead, and the 500-line limit — not the table —
is the gate.

| File | Plan advisory figure (pre-change) | `P0-T16` baseline (pre-change) | Measured now (post-change) | Why the post-change value differs |
|---|---:|---:|---:|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 596 (stated "before the `P1-T2` split") | 596 | 310 | `P1-T2` relocated twelve private members to `.Selection.cs`; Phase 6 relocated the arrow handlers to `.Arrows.cs`. |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 485 | 485 | 489 | Phases 3 to 6 added four lines. 11 lines of headroom remain. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 457 | 457 | 248 | Phase 6 split the row-navigation members into `BreadcrumbStateModel.Row.cs` (384 lines). |
| `QuickFiler/Resources/FolderBreadcrumb.html` | 489 | 489 | 490 | `P6-T17` added one line. Cannot be split; 10 lines of headroom remain. |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | 462 | 462 | 462 | Unchanged in size; `P2-T1` split new methods into `.Part2.cs` (368 lines). |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | 435 | 435 | 462 | Phase 6 added methods; the remainder went to `.Selection.cs` (261 lines). |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 361 | 361 | 361 | Not written; `P6-T3` forbids additions. |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 98 | 98 | 141 | Phase 4 added the suffix-match resolution and a logger. |

Three partial siblings created in Phase 6 are not present in the plan's advisory table because they did
not exist when it was written. They are added to the roster here and are measured in section A above:
`QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` (211),
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` (261), and
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` (384). All three are well under the
limit.

**AC-31 disposition: SATISFIED.** No file written by this plan exceeds 500 lines, and the three
pre-existing violations are byte-identical to their baseline.
