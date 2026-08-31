Timestamp: 2026-08-31T11:02:14-04:00

## Baseline coverage

Source: `evidence/baseline/p0-t16-coverage-headline.md`.

- line-rate=0.853428; line coverage=85.3428%
- branch-rate=0.793049; branch coverage=79.3049%
- lines-covered=54808; lines-valid=64221
- branches-covered=13052; branches-valid=16458

## Post-change coverage

Source: `evidence/qa-gates/p7-t6-coverage-headline.md`.

- line-rate=0.853327; line coverage=85.3327%
- branch-rate=0.793089; branch coverage=79.3089%
- lines-covered=54822; lines-valid=64245
- branches-covered=13059; branches-valid=16466

## Changed and new-code coverage

Source: `evidence/qa-gates/p7-t7-changed-line-coverage.md`.

- Added-line and zero-hit intersections are empty for all three changed production files.
- Every emitted Cobertura sequence point within `ToFilingStemOrVerbatim` has non-zero hits.
- The helper conditional has `branch="True"` and `condition-coverage="100% (6/6)"`, demonstrating both outcomes.

Coverage-floor authority: the General Unit Test Policy in `AGENTS.md` and `.agents/skills/csharp/SKILL.md` require repository-wide line coverage at or above 80 percent, new modules, classes and methods to target at least 90 percent coverage, and no coverage regression on changed lines. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487` enforces the same repository-wide 80-percent figure.

Result: post-change line coverage is 85.3327%, which is at or above 80%. The changed-line uncovered intersection is empty, and the new helper's emitted sequence points are all covered. `BASELINE BELOW FLOOR` does not apply.
