# coverage-runner-scoped-threshold-and-format (Issue #928)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/coverage-runner-scoped-threshold-and-format/ (Issue #928)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #928
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/928
- Last Updated: 2026-09-28
## Summary
Consolidates #891 with sub-item 2 of #914. Both concern the same two scripts, `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and `scripts/vscode/Invoke-MSTest.ps1`, and both must be changed in one place to avoid a merge conflict.

1. **#891:** a `-SearchRoot`-scoped coverage run always exits 1.
   - `Assert-CoberturaLineCoverageThreshold` (`Invoke-MSTestWithCoverage.Threshold.ps1:52`) and the branch assertion compare the document-level Cobertura rate, taken across every instrumented assembly, against the 80% line and 75% branch floors.
   - A single-assembly run measures roughly 24% of that denominator, so it fails even when every test passes.
   - The assertions are applied unconditionally at `Invoke-MSTestWithCoverage.ps1:386-387`.
2. **#914 sub-item 2:** both scripts carry formatting that the repository PowerShell formatter would rewrite.

## Environment
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (PowerShell 7 toolchain scripts)
- Command/flags used: `./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml`
- Data source or fixture: `main` at `177b6d78e`

## Steps to Reproduce
1. Run the command above against an all-green `QuickFiler.Test`.
2. Observe exit code 1 and `Cobertura line coverage 24.3% is below the required 80% threshold.`
3. Run the repository PowerShell formatter (PoshQC format) over `scripts/vscode/`. Observe that it would rewrite the two scripts.

## Expected Behavior
- A scoped run's exit code reflects the outcome of the tests it executed. Coverage is then either measured over the scoped assemblies only, or the threshold assertion is skipped with an explicit, logged reason.
- The unscoped run (`-SearchRoot .`) keeps enforcing 80% line / 75% branch exactly as it does now. CI (`_mstest-coverage.yml:87-96`, added by PR #897) depends on it and must not be weakened.
- Both scripts are formatter-clean.

## Actual Behavior
- Every scoped run fails the threshold gate regardless of test outcome, so plans that scope a run have to fall back to reading the trx.
- The two scripts are not formatter-clean.

## Logs / Screenshots
- [x] Attached minimal logs or snippet
- Snippet: `Invoke-MSTestWithCoverage.Threshold.ps1:54: Cobertura line coverage 24.3093% is below the required 80% threshold.` (from #891)

## Impact / Severity
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes
The threshold gate was designed for whole-solution runs. `-SearchRoot` narrows the executed tests without narrowing the coverage denominator. The formatting drift predates #911, which deliberately left it alone.

## Proposed Fix / Validation Ideas
- [ ] Choose one approach and document it in the script help:
  - an explicit switch such as `-SkipCoverageThreshold`, or
  - automatic skipping whenever `-SearchRoot` is not the repository root, with a logged warning, or
  - computing the rate over the packages of the scoped assemblies only.
- [ ] Add Pester tests under `tests/scripts/vscode/` for:
  - a scoped run with all tests passing, which exits 0
  - a scoped run with a failing test, which exits non-zero
  - the unscoped run below the floor, which still fails
- [ ] Run PoshQC format on both scripts.
- [ ] Line coverage for `scripts/vscode` stays at or above 80% (the `_pester.yml` gate).

## Next Step
- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Consolidates: #891, #914 sub-item 2.