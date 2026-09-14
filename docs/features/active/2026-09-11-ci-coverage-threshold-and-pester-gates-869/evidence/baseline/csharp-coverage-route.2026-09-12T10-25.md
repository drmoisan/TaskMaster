# Phase 0 — Baseline C# coverage route (P0-T7)

Timestamp: 2026-09-14T18-02

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -Command '<worktree prologue>; & "<repo-root>/scripts/vscode/Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug'`
EXIT_CODE: 0

The invocation was made through `-Command` with the worktree prologue and the call operator on an absolute script path, per part 3 of the plan's working-directory rule. `-SearchRoot .` was left unchanged, because line 306 of that script joins the supplied search root onto the script-derived repository root and `Join-Path` does not collapse an absolute second argument. The captured output was additionally teed to `coverage/p0t7-transcript.txt`, a path under the gitignored coverage directory, so that the printed lines quoted below could be read back verbatim rather than from a truncated tool buffer. Teeing the output does not change the invocation or its exit code.

## Output Summary

The single printed line beginning `First-party coverage:`, reproduced verbatim:

```
First-party coverage: lines 56346/65616 (85.87%), branches 13624/17022 (80.04%)
```

Other route output of record:

```
Discovered 9 test assemblies.
Post-processing coverage XML for Koverage compatibility...
Coverage projection: <repo-root>\coverage\coverage.cobertura.jacoco.xml
Test-result summary: <repo-root>\coverage\test-results\mstest-coverage-run.summary.txt
Done. Coverage artifact: <repo-root>\coverage\coverage.cobertura.xml
```

The test-result summary the route wrote reports: total 7293, executed 7293, passed 7293, failed 0; skipped 0; error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0; failed tests: none.

Because the exit code is 0, the `FirstPartyLinePrinted:` field is not required by the task's conditional wording; it is recorded anyway for completeness.

FirstPartyLinePrinted: true

## Document-root attributes

Read from `coverage/coverage.cobertura.xml`, the document the run wrote at line 384 of the entry point before any assertion could throw:

- `line-rate` = `0.858723`
- `branch-rate` = `0.800376`
- `branches-valid` = `17022`

Recorded additionally, because they are the operands the merged evidence-projection item's reconciliation compares against: `lines-covered` = `56346`, `lines-valid` = `65616`, `branches-covered` = `13624`.

## Two denominators, recorded separately and never conflated

AssertedDenominator: all surviving packages — root `line-rate` = `0.858723`.

PrintedDenominator: allowlist — the line percentage carried by the printed `First-party coverage:` text = `85.87`.

These are two different denominators. The figure the CI gate is judged on is the root attribute set only.

Mechanism, re-derived in this pass against the named positions. `ConvertTo-KoverageCoberturaXml` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` removes, at its lines 431 to 435, every package child whose `name` attribute is non-empty and absent from the allowlist; it then calls `Get-CoberturaCoverageSummary` at line 455 and writes that summary onto the document root at lines 456 to 461 as `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered` and `branches-valid`. `Get-CoberturaCoverageSummary`, at line 103 of the same file, iterates every surviving package child at its line 121 and applies no name filter of its own, so the root attributes carry the all-surviving-packages denominator. `Assert-CoberturaLineCoverageThreshold`, at lines 32 and 33 of `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, selects `/coverage` and reads its `line-rate`, so the asserted figure is that root figure. The separately printed `First-party coverage:` line comes from `Get-CoberturaFirstPartyCoverageSummary` in `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`, which at its line 72 skips every package whose `name` is not contained in the allowlist, so it carries the allowlist denominator. Both read the same post-processed string and both therefore see the closure-exemption removal applied at line 441. The two sets coincide except for a package element carrying no `name` attribute or an empty one, which survives the removal at line 432 and is excluded by the filter at line 72; neither call site passes an explicit `-ProjectNames`, so both use the same `Get-KoverageProjectAllowlist` default.

## Agreement verdict, under the plan's mechanical definition and no other

Operand 1, the root `line-rate`: `0.858723`. This is a six-decimal fraction emitted as a string, built at line 130 of `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` as `[string]([math]::Round($coveredLines / $totalLines, 6))`.

Operand 2, the two-decimal percentage inside the printed `First-party coverage:` line: `85.87`. This is built at line 90 of `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` as `([double](100 * $coveredLines / $totalLines)).ToString('0.00', $invariant)`, assigned into the line text at line 117 of that same file, and rendered into the returned string at its line 120.

Formatted comparison value: `0.858723` multiplied by 100 is `85.8723`, formatted `0.00` under invariant culture is `85.87`.

Verdict: AGREE. `85.87` is string-equal to `85.87`.

Recorded as a separate observation, not part of the defined predicate: the branch figures correspond in the same way. The root `branch-rate` `0.800376` multiplied by 100 and formatted `0.00` is `80.04`, which is the branch percentage inside the same printed line. The counts also match exactly: `13624/17022` printed against `branches-covered` 13624 and `branches-valid` 17022.

## Halt branches, each evaluated

1. Below-floor coverage. Requires a non-zero exit code, `FirstPartyLinePrinted:` false, and the token `is below the required 80` in the thrown text. The exit code is 0 and no text was thrown. **Did not fire.**
2. Projection or reconciliation failure. Requires a non-zero exit code. The exit code is 0. **Did not fire.**
3. Unattributable failure. Requires a non-zero exit code. The exit code is 0. **Did not fire.**
4. Timeout. vstest terminated normally; the run completed and the route printed its `Done.` line. **Did not fire.** No `EXIT_CODE: 124` and no `TimeoutObserved` condition arose.
5. Unusable branch figures. The document carries a `branch-rate` attribute; `branches-valid` is `17022`, which is not `"0"`; and the parsed `branch-rate` of `0.800376` is not below `0.75`. **Did not fire.** The unchanged tree therefore sits above the 75 floor this delivery introduces, so the zero exit code P10-T8 requires is reachable within this delivery's scope.

The branch figures this task read are genuinely recomputed rather than inherited: lines 457 and 461 of the helpers file set `branch-rate` and `branches-valid` from the same recomputed summary, so the fifth branch tested real values.

## Document-retention check, recorded and cleared

The evidence-projection item added `Test-RawCoverageDocumentRetained`, which deletes the document at the resolved output path unless that path's parent directory is exactly the repository root joined with `coverage`, compared by equality rather than containment. That discard call sits at line 426, nested inside the `if ($runSummary)` block that opens at line 416, both re-derived in this pass. This task's invocation supplied no `-CoverageOutput`, so the entry point used its declared default `coverage\coverage.cobertura.xml`, stated at line 9 of the parameter block and repeated at line 282 of the main function's parameter block, and resolved it at line 343 as the repository root joined with that relative path. The parent directory is therefore exactly the repository root joined with `coverage`, the predicate returned true, and the document was retained. The three attribute reads above were performed against the retained document, which confirms the clear result rather than only predicting it. The retention would also have held on every failing path this task could have taken, because a throw at line 386 or at lines 397 to 399 never reaches line 416.

## Baseline exit-code note

This baseline task does not demand a zero exit code, because the branch assertion does not exist yet and only the existing line assertion runs. The observed zero exit code means the existing line assertion passed on the unchanged tree, which is consistent with the recorded root `line-rate` of `0.858723` sitting above the 80 floor.
