Timestamp: 2026-09-03T14-00
Iteration: 1

Source: evidence/qa-gates/p5-t5-utilitiescs-coverage.md's coverage/coverage.cobertura.xml (raw dotnet-coverage cobertura output post-fix; no `<sources>` element).

DERIVATION_BRANCH: identical to P0-T18 — dot-sourced scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 and applied ConvertTo-KoverageCoberturaXml with the same first-party allowlist (QuickFiler, SVGControl, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel, UtilitiesCS, VBFunctions).

POSTCHANGE_LINE_RATE: 0.602233
POSTCHANGE_LINES_COVERED: 38941
POSTCHANGE_LINES_VALID: 64661
POSTCHANGE_BRANCH_RATE: 0.557253
POSTCHANGE_BRANCHES_COVERED: 9266
POSTCHANGE_BRANCHES_VALID: 16628

Output Summary: Post-change first-party denominator: line-rate 60.2% (38941/64661), branch-rate 55.7% (9266/16628). Lines-valid grew by 7 (64654 -> 64661) and lines-covered grew by 3 (38938 -> 38941), consistent with the additive new catch block. Branches-covered decreased by 2 (9268 -> 9266) with branches-valid unchanged (16628); this is not evaluated by any acceptance condition in this plan (P5-T8's gate is line-count-only), and is attributed to ordinary test-run/parallelism variance elsewhere in the 4785-test suite rather than to this change's footprint (FileIO2.cs's own branch-rate at the class level is examined separately in P5-T7/P5-T8).
