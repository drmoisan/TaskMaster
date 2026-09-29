# coverage-runner-scoped-threshold-and-format (Issue #928)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/coverage-runner-scoped-threshold-and-format/ (Issue #928)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #928
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/928
- Last Updated: 2026-09-28
- Work Mode: minor-audit

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

## Acceptance Criteria

Derived by the preparation orchestrator on 2026-09-28 from Expected Behavior above and the binding operator constraints for run bugs-2026-09-28. Selected approach: automatic skipping of the document-level threshold assertions when the run is scoped, with a logged warning. A run is scoped when the full path of the resolved search root differs from the full path of the repository root (ordinal, case-insensitive, trailing separators ignored); an omitted `-SearchRoot`, `-SearchRoot .`, and `-SearchRoot .\` are all unscoped. No new command-line switch is added, so no caller can opt out of the unscoped gate.

- [x] AC1: A scoped run whose test collection succeeds exits without error even when its post-processed Cobertura document is below 80% line and below 75% branch, and it emits exactly one warning stating that the coverage threshold assertions were skipped because the run was scoped to a search root other than the repository root. Demonstrated by a Pester test using an in-memory below-floor Cobertura fixture and mocked collection.
- [x] AC2: A scoped run whose test collection fails (non-zero collector exit code) still terminates with an error. Demonstrated by a Pester test with mocked collection returning a non-zero exit code.
- [x] AC3: An unscoped run (omitted `-SearchRoot`, or `-SearchRoot .`) whose post-processed Cobertura document is below the floor still throws the existing threshold message: one Pester test for a below-80% line rate and one for a below-75% branch rate with an at-or-above-80% line rate. The CI coverage workflow file is not modified by this change, and the 80% and 75% literals in the threshold assertions are unchanged.
- [x] AC4: The scoped-run behavior and the definition of a scoped run are documented in comment-based help in the coverage script.
- [x] AC5: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and `scripts/vscode/Invoke-MSTest.ps1` are formatter-clean: a PoshQC format run over `scripts/vscode` after the change leaves both files byte-identical.
- [ ] AC6: PoshQC analyze reports no findings on any changed PowerShell file, the Pester suite passes, and the Pester line-coverage figure over the CI Pester population (the scripts/dependencies and scripts/vscode folders, as measured by the Pester workflow) remains at or above 80% and does not fall below its recorded baseline, with every changed production line covered. (Amended 2026-09-28 by the preparation orchestrator: the scripts/vscode folder alone measured below 80% in earlier committed evidence, so the floor is stated against the population the CI gate actually enforces.)
- [x] AC7: All committed evidence follows the CLAUDE.md Committed Test Evidence Format section (projections and summaries only, no raw test-result or raw coverage collector document) and contains no absolute host path, developer account name, or host name; the placeholders `<repo-root>`, `<user-profile>`, `<user>` and `<host>` are used instead.

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