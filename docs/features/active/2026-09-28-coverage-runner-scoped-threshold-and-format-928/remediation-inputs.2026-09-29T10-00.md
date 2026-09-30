# Remediation Inputs: coverage-runner-scoped-threshold-and-format (Issue #928)

- Timestamp label: 2026-09-29T10-00
- Cycle: 1 (opened by the reduced audit at this timestamp)
- Source audit artifacts (same folder, same timestamp): policy-audit.2026-09-29T10-00.md (G-1), code-review.2026-09-29T10-00.md (CR-1, CR-2), feature-audit.2026-09-29T10-00.md (AC6 PARTIAL, blocking)
- Blocking count entering this cycle: 1
- Layout note: this file uses the flat `remediation-inputs.<timestamp>.md` form in the feature folder root, because the SubagentStop hook's artifact-path regex requires that form; the `remediation/<ts>/` folder layout in the remediation-handoff skill is not used in this repository.

## Remediation-required findings

### R-1 (Blocking): AC6 changed-line and baseline clauses unmet by the agreed measurement route

Observed: entry-point changed line 408 (`Write-Warning` in the scoped arm of `Invoke-MSTestWithCoverageMain`) reads 0 hits in the Route C full-population Pester 5.6.1 breakpoint run; population 94.46% (1620/1715) against baseline 94.49% (1613/1707); entry point 115/129 = 89.15% against 113/126 = 89.68%. The line is executed by It 10 of tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1, which passes.

Cause (measured by the executor, evaluated independently by the reviewer): every suite that invokes the entry point imports it through `Parser::ParseFile(...).GetScriptBlock()` and dot-sources its own compiled copy; Pester 5.6.1 line breakpoints bind to the first copy in which the function executes (`Invoke-MSTest.RunSettings.Tests.ps1`, which sorts first and never takes the scoped arm). Lines reached only by a later suite's copy record no hit. Part files dot-sourced by path from inside the entry point are served from PowerShell's per-path compiled-script cache and are therefore likely credited from any suite (the new part file reads 5/5); this premise is to be confirmed in step 0 below.

Required outcome: `CHANGED-LINES-UNCOVERED: none` for every production file listed by `git diff --numstat 177b6d78e -- scripts/vscode`, `FINAL_POPULATION_LINE_PERCENT` at or above `BASELINE_POPULATION_LINE_PERCENT` by the same Route C command, the new part file at or above 90%, analyzer `ok: true` on every changed file, the Pester suite passing, and the plan's P2-T4 loop closure recorded with `LOOP-CLOSED: yes`; then AC6 checked off per the acceptance-criteria-tracking skill.

Recommended fix (option 3 of the P2-T13 handoff, refined):

0. Diagnostic gate before any edit: run the CMD-PESTER-DIRECT shape with `Run.Path` = `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` then the new test file, and `CodeCoverage.Path` = `scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`; record whether every part-file line is credited (`ci` greater than 0) when the new file sorts second. If yes, proceed. If no, stop and take R-1 fallback (b) below.
1. In `scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`, add one function (approved verb, singular noun; suggested name `Assert-CoberturaCoverageThresholdForRun`) with parameters `-CoberturaXml`, `-RepoRoot`, `-ResolvedSearchRoot` (all `[Parameter(Mandatory = $true)] [string]`), `[CmdletBinding()]`, comment-based help. Body: the current entry-point conditional verbatim: `if (Test-CoverageRunIsScoped -RepoRoot $RepoRoot -ResolvedSearchRoot $ResolvedSearchRoot) { Write-Warning (<the existing two-literal message using $ResolvedSearchRoot and $RepoRoot>) return }` then `Assert-CoberturaLineCoverageThreshold -CoberturaXml $CoberturaXml` and `Assert-CoberturaBranchCoverageThreshold -CoberturaXml $CoberturaXml` in that order (D3 preserved: the two statements keep their text and order). Update the part-file header comment (lines 3 to 6) to describe the file's two functions; drop the change-budget sentence.
2. In `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, replace lines 404 to 414 with a two-line comment (why the gate lives in the part file: it is path-loaded so every suite credits it, and the entry point stays thin) and one call `Assert-CoberturaCoverageThresholdForRun -CoberturaXml $processedXmlContent -RepoRoot $repoRoot -ResolvedSearchRoot $resolvedSearchRoot`. Update the `.DESCRIPTION` sentence if its wording refers to the entry point performing the skip. Keep the dot-source at line 313 and the comment at 311 to 312 (or reword 311 to 312 to drop the budget rationale).
3. In `tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1`, add one Describe with three It cases calling the new function directly with the existing fixtures: scoped (`Should -Invoke Write-Warning -Times 1 -Exactly`, no throw), unscoped below the line floor (line message), unscoped at line floor and below the branch floor (branch message). Keep every existing It unchanged; It 9 to 13 continue to prove AC1 to AC3 through the entry point. Optionally fold in CR-2 (`[ValidateNotNullOrEmpty()]` and an `IsPathRooted` guard on `Test-CoverageRunIsScoped`, with two negative cases).
4. Re-run the Phase 2 loop (format, analyze, test with coverage) per the plan's P2-T1 to P2-T4 with iteration 2 artifacts; record `CHANGED-LINES-UNCOVERED` for both production files from the `-U0` hunk headers; re-derive the plan's file ceilings (the part file will exceed the plan's 60-line ceiling from P1-T4 and P2-T5; the planner revises that ceiling to a value at or under 120 in the revision delta). Then P2-T12 outcome (i) checks off AC6 and P2-T13 is refreshed.

Verification commands (as recorded in the plan; unchanged): CMD-FORMAT, CMD-ANALYZE on the three changed files, CMD-TEST over the four folders, CMD-PESTER-DIRECT with `$o` set to `coverage/p2-t3-pester-coverage.iter2.jacoco.xml`, `git diff --numstat 177b6d78e -- scripts/vscode tests/scripts/vscode`, `git diff -U0 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1 scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`.

Fallbacks, in order, only if step 0 fails:

- (a) None by code within the Write Set. Do not edit sibling test files (out of scope, and `Invoke-MSTest.RunSettings.Tests.ps1` is at 499 lines) and do not change the local measurement route to `UseBreakpoints = $false` (it would diverge from `.github/workflows/_pester.yml` and invalidate the recorded baseline).
- (b) Maintainer-ratified measurement exception for the specific uncredited line(s), citing the P2-T3 diagnostics, transcribed into issue.md as a dated note directly beneath AC6 (the AC text itself unchanged). A ratification recorded only in gitignored orchestrator state does not satisfy this. With the note in place, AC6 may be checked off by the orchestrator, and the reduced audit is re-run.

### Promotion candidates (not part of this cycle; route through the potential-feature lifecycle)

- P-1: Pester breakpoint coverage under-credits entry-point lines reached only by later-sorting suites (four entry-point suites import separate `ParseFile` copies). Options: switch the suites to path dot-sourcing (safe under the entry point's `InvocationName` guard), or evaluate `CodeCoverage.UseBreakpoints = $false` in `_pester.yml` (workflow change; needs a green run and a re-baselined figure).
- P-2: tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 lines 41, 42, 104, 124 embed the developer account name in literal fixture paths; replace with a neutral drive-letter fixture root.
- P-3: script-level comment-based help for scripts/vscode/Invoke-MSTestWithCoverage.ps1 so `Get-Help` on the script surfaces the scoped-run behavior (CR-4).

## Do-not-do list

- Do not edit `.github/workflows/_pester.yml` or `_mstest-coverage.yml` in this cycle.
- Do not edit `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, `Invoke-MSTestWithCoverage.Helpers.ps1`, any other part file, or any existing test file.
- Do not change the 80 and 75 literals, their messages, or the order of the two assertion statements.
- Do not add a command-line switch or any other opt-out from the unscoped gate.
- Do not add analyzer suppressions, `[ExcludeFromCodeCoverage]`-style exclusions, or coverage `exclude` entries.
- Do not weaken or delete any of the 14 existing It cases; do not add sleeps or retries.
- Do not measure the post-change figure by a different route than the baseline; if the route changes, re-measure the baseline by the same route in the same session.
- Do not commit any raw JaCoCo, JUnit or trx document; keep evidence as derived figures under `<FEATURE>/evidence/<kind>/` with `<repo-root>` placeholders and no host identifiers.
- Do not amend the AC6 text; only a maintainer may add a ratification note beneath it.

## Pointers

- Executor evidence establishing the finding: evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md, evidence/qa-gates/p2-t4-loop-closure.2026-09-29T09-25.md, evidence/regression-testing/p2-t12-ac6-check-off.2026-09-29T09-27.md, evidence/other/p2-t13-reduced-audit-handoff.2026-09-29T09-27.md.
- Baseline: evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md.
- Plan to revise: plan.2026-09-28T19-45.md (Production Specification, Test Specification, D3 wording, P1-T4 and P2-T5 part-file ceiling, P2-T3 iteration 2, P2-T4, P2-T12, P2-T13).
