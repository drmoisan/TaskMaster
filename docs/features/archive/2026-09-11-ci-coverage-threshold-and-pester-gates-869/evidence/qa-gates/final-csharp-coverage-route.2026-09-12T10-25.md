# Final C# coverage route (P10-T8)

Timestamp: 2026-09-14T21-22

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -Command '<worktree prologue>; & "<repo-root>/scripts/vscode/Invoke-MSTestWithCoverage.ps1" -SearchRoot . -Configuration Debug'`
EXIT_CODE: 0

`-SearchRoot .` is unchanged, because line 306 of that script joins the supplied search root onto the script-derived repository root and `Join-Path` does not collapse an absolute second argument. The captured output was teed to `coverage/p10t8-transcript.txt`, a path under the gitignored coverage directory, so the printed lines below could be read back verbatim.

## Output Summary

The single printed line beginning `First-party coverage:`, reproduced verbatim:

```
First-party coverage: lines 56348/65616 (85.88%), branches 13626/17022 (80.05%)
```

Other route output of record:

```
Discovered 9 test assemblies.
Coverage projection: <repo-root>\coverage\coverage.cobertura.jacoco.xml
Done. Coverage artifact: <repo-root>\coverage\coverage.cobertura.xml
```

## Both assertions passed

**Reaching a zero exit code proves both the line assertion and the branch assertion returned without throwing.** On the changed tree the assertion site is at lines 386 and 387 of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: the line assertion at 386 and the branch assertion P2-T2 inserted at 387. A throw from either terminates the script and yields a non-zero exit, so a zero exit is the joint pass signal for both. The projection block the evidence-projection item added sits below at lines 395 to 399, so the zero exit additionally covers that region, and the printed `Coverage projection:` line confirms it completed.

## Document-root attributes

Read from `coverage/coverage.cobertura.xml`, the document the run wrote:

- `line-rate` = `0.858754`
- `branch-rate` = `0.800493`
- `branches-valid` = `17022`

Recorded additionally: `branches-covered` = `13626`, `lines-covered` = `56348`, `lines-valid` = `65616`.

## Two denominators, recorded separately and never conflated

AssertedDenominator: all surviving packages — root `line-rate` = `0.858754`, root `branch-rate` = `0.800493`.

PrintedDenominator: allowlist — the line percentage inside the printed `First-party coverage:` text = `85.88`, and its branch percentage = `80.05`.

These are two different figures computed by two different functions over two different package filters. **Both the line gate and the branch gate are judged on the root attributes only.**

The mechanism is the one re-derived in P0-T7 against lines 431 to 435, 455 to 461 and 103 to 127 of `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, line 72 of `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` and lines 32 to 33 of `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`. In brief: the post-processor removes every package whose `name` is non-empty and absent from the allowlist, then rewrites the document-root counters from a summary computed over every surviving package with no name filter of its own, while the printed report applies the allowlist filter itself. The two sets coincide except for a package element carrying no `name` attribute or an empty one.

## Agreement verdict, under the same mechanical definition P0-T7 states and no other

Operand 1, the root `line-rate`: `0.858754`. A six-decimal fraction built at line 130 of `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.

Operand 2, the two-decimal percentage inside the printed `First-party coverage:` line: `85.88`. Built at line 90 of `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1`, assigned into the line text at line 117 of that file and rendered into the returned string at its line 120.

Formatted comparison value: `0.858754` multiplied by 100 is `85.8754`, formatted `0.00` under invariant culture is **`85.88`**.

**Verdict: AGREE.** `85.88` is string-equal to `85.88`.

Recorded as a separate observation, outside the defined predicate: the branch figures correspond in the same way. `0.800493` multiplied by 100 and formatted `0.00` is `80.05`, which is the branch percentage inside the same printed line, and the counts match exactly at `13626/17022` against `branches-covered` 13626 and `branches-valid` 17022.

## Document-retention check, recorded and cleared

The discard call at line 426 of the entry point, nested inside the `if ($runSummary)` block that opens at line 416, deletes the document only when its parent directory is not exactly the repository root joined with `coverage`. This invocation supplied no `-CoverageOutput`, so the entry point used its default `coverage\coverage.cobertura.xml`, declared at line 9 and repeated at line 282, and resolved it at line 343 against the repository root. The parent is therefore exactly that directory, the predicate returned true, and the document was retained. The three attribute reads above were performed against the retained document, which confirms the clear result rather than only predicting it.

## Failure attribution

Not required on this run: the exit code is 0, so none of P0-T7's discrimination branches applies and no attribution was made. The timeout halt branch did not fire either; vstest terminated normally and the route printed its `Done.` line.

## Line-number shift note

P2-T2 inserted its single statement between the line assertion and the report statement, so the branch assertion occupies line 387 and the report statement moved to line 388, with every position below shifting by one. This task's acceptance cites no absolute line number in the shifted region, so the shift changes nothing it asserts. The positions quoted above for the projection block and the discard were re-read against the current file.

Output Summary: the coverage route exited 0 on the changed tree, which proves both the line and branch assertions returned without throwing. Root `line-rate` is 0.858754 and root `branch-rate` is 0.800493 with 17022 valid branches, both above their floors of 0.80 and 0.75. The asserted and printed line figures agree at 85.88 under the plan's mechanical definition.
