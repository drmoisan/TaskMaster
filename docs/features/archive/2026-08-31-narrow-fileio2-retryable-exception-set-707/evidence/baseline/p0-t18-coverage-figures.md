Timestamp: 2026-09-03T12-40

Source: evidence/baseline/p0-t17-utilitiescs-coverage.md's coverage/coverage.cobertura.xml (raw dotnet-coverage cobertura output; does NOT carry a `<sources>` element).

DERIVATION_BRANCH: raw dotnet-coverage cobertura output lacks `<sources>`, so the governing derivation dot-sourced scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 and applied ConvertTo-KoverageCoberturaXml (with the Get-KoverageProjectAllowlist first-party, non-`.Test`-suffixed project allowlist: QuickFiler, SVGControl, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel, UtilitiesCS, VBFunctions), producing a transformed document that does carry `<sources>`; its root `coverage` attributes are read below.

BASELINE_LINE_RATE: 0.602252
BASELINE_LINES_COVERED: 38938
BASELINE_LINES_VALID: 64654
BASELINE_BRANCH_RATE: 0.557373
BASELINE_BRANCHES_COVERED: 9268
BASELINE_BRANCHES_VALID: 16628

Output Summary: First-party (non-test) repository denominator after allowlist filtering and class-by-filename merge: line-rate 60.2% (38938/64654), branch-rate 55.7% (9268/16628). This is a whole-first-party-package figure, not scoped solely to UtilitiesCS, because dotnet-coverage instruments the entire UtilitiesCS.Test host process and the ConvertTo-KoverageCoberturaXml allowlist keeps every first-party production package, matching CLAUDE.md's repository-wide coverage method. The identical derivation is applied again at P5-T6 so the before/after comparison is self-consistent regardless of denominator breadth.
