# coverage-runner-searchroot-threshold-false-fail (Issue #891)

- Date captured: 2026-09-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/coverage-runner-searchroot-threshold-false-fail/ (Issue #891)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #891
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/891
- Last Updated: 2026-09-14
## Summary

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` exits non-zero for any `-SearchRoot`-scoped
invocation, even when every test in the scoped assembly passes, because its post-run gate
compares a repository-wide document-level Cobertura `line-rate` against a hard-coded 80%
threshold that a single-assembly run can never reach.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Command/flags used: `pwsh -NoProfile -Command './scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"; Write-Output "EXITCODE=$LASTEXITCODE"'`
- Data source or fixture: repository `TaskMaster`, branch `bug/quickfiler-date-time-format-missing-invariant-culture-742` (also reproduces on the unfixed base tree, i.e. it is not caused by any in-flight change)

## Steps to Reproduce

1. From the repo root, run `./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`.
2. Observe the vstest run itself completes with all `QuickFiler.Test` tests passing (confirmed via the runner's own trx: 1429/1429 passed on the unfixed tree, 1434/1434 passed after an unrelated fix landed).
3. Observe `$LASTEXITCODE` is `1` regardless, and the console shows an exception thrown from `Assert-CoberturaLineCoverageThreshold` in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` line 54: `Cobertura line coverage 24.3028% is below the required 80% threshold.`

## Expected Behavior

A `-SearchRoot`-scoped coverage run's exit code should reflect the pass/fail state of the tests it actually executed (or the runner should measure coverage over the scoped assembly's own instrumented lines, or expose a way to skip/adjust the threshold for a deliberately narrow run). Callers that need "did my scoped test run pass" cannot get that signal from the exit code today.

## Actual Behavior

`Assert-CoberturaLineCoverageThreshold` reads the `line-rate` attribute of the Cobertura document's root `/coverage` element, which spans every instrumented assembly in the solution regardless of `-SearchRoot` (61,894–61,898 valid lines observed), not just the assembly the scoped run executed. A `-SearchRoot`-scoped run only exercises and covers a fraction of that repository-wide denominator (15,040–15,047 lines observed, ~24.3%), so the hard-coded `if ($percentage -lt 80)` check at line 52 of `Invoke-MSTestWithCoverage.Threshold.ps1` always throws for a scoped run, independent of test outcome. `Invoke-MSTestWithCoverage.ps1` exposes only `-SearchRoot`, `-Configuration`, `-CoverageOutput`, and `-NoExecute` — no parameter lowers, disables, or rescales the threshold for scoped invocations.

Discovered while executing the approved atomic plan for issue #742: the plan's task `[P5-T4]` (and, before it, baseline task `[P0-T8]`) both intentionally scope `-SearchRoot QuickFiler.Test` to avoid a known unrelated hang in `UtilitiesCS.Test`'s shell-icon test classes on this machine, and both runs hit this false-fail exit code even though the scoped test suite was 100% green in both cases. The task's literal acceptance criterion ("EXITCODE is 0") could not be satisfied, and the corresponding item in `spec.md`'s Acceptance Criteria list (the criterion mapped to `vstest.console.exe ... reports zero failures`) was left unchecked as a result, even though that criterion is substantively true (1434/1434 passed).

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:
  ```
  Exception: ...\scripts\vscode\Invoke-MSTestWithCoverage.Threshold.ps1:54
  Cobertura line coverage 24.3093% is below the required 80% threshold.
  ```
  Full evidence transcripts: `docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/evidence/baseline/vstest-coverage-baseline.2026-09-12T16-09.md` and `.../evidence/qa-gates/vstest-coverage-final.2026-09-12T16-09.md`.

## Impact / Severity

- [ ] Blocker
- [x] Medium
- [ ] Low

Not a blocker because the unscoped, whole-repository invocation (without `-SearchRoot`) is unaffected — the document-level denominator then matches what is actually executed. It becomes a real obstacle any time a plan or workflow deliberately scopes a run (as this issue's plan did, to route around the separate `UtilitiesCS.Test` shell-icon hang), because the scoped run's exit code is then unusable as a pass/fail signal and every such plan must fall back to reading the trx directly.

## Suspected Cause / Notes

- `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, function `Assert-CoberturaLineCoverageThreshold` (line 3; threshold check at line 52; hard-coded literal `80`).
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — the `-SearchRoot` parameter scopes test *execution* (via `Get-ChildItem -Recurse -Filter '*.Test.dll'` under the resolved search root) but does not scope, or inform, the coverage *threshold* comparison, which is computed from the full Cobertura document produced by the coverage collector.
- Reproduces identically on the unfixed base tree (pre-existing, not introduced by issue #742's change).

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: add a Pester/unit test for `Assert-CoberturaLineCoverageThreshold` (or its caller) covering a `-SearchRoot`-scoped invocation against a synthetic multi-assembly Cobertura document, asserting the threshold comparison uses a denominator scoped to the executed assembly rather than the whole document.
- [x] Integration scenario to retest: re-run `Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test` (or any other single-project scope) after the fix and confirm exit code 0 when all scoped tests pass, and a genuine scoped-coverage-regression still throws.
- Candidate approaches: (a) filter the Cobertura document to only the packages/classes under the scoped search root before evaluating the threshold; (b) skip the threshold check entirely when `-SearchRoot` narrows execution below the full test suite and rely on the trx pass/fail signal instead; (c) add an explicit opt-out parameter (e.g. `-SkipCoverageThreshold`) for scoped diagnostic runs, documented as such.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
