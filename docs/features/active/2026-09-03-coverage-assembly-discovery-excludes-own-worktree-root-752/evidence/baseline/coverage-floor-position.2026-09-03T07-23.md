# Coverage Floor Position ([P0-T11])

Timestamp: 2026-09-03T11-56

Command: comparison of the `BASELINE LINE COVERAGE PERCENT` value recorded in `evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.md` against the 85 percent line floor stated in `.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md`.

EXIT_CODE: 0

BASELINE LINE COVERAGE PERCENT: 78.3042394014963

FLOOR: 85

BASELINE AT OR ABOVE FLOOR: false

Output Summary: The pre-change baseline over `scripts/vscode`, measured by the `[P0-T10]` run before this item changed anything, is 78.3042394014963 percent, which is below the 85 percent line floor. The shortfall predates this item: it is a property of the coverage the existing `tests/scripts/vscode` suite provides over `scripts/vscode` on the unmodified tree, and this item is a single-line defect fix that neither caused it nor can close it within its scope. This plan's blocking coverage condition is therefore post-change greater than or equal to baseline, verified in `evidence/qa-gates/coverage-delta.2026-09-03T07-23.md`, rather than an absolute floor claim. No other threshold figure is restated, waived, or substituted here; the 85 percent figure remains the repository's stated floor and the 75 percent branch threshold remains inapplicable to PowerShell because Pester measures no branch coverage.
