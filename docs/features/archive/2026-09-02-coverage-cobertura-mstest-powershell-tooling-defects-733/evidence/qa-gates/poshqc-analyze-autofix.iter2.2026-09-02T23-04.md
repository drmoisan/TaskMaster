# P5-T3 — PoshQC analyze autofix (Final QA Loop, iteration 2, final) — NOT RUN

Timestamp: 2026-09-02T23-04

Status: not run on this iteration, by the task's own branch condition.

## Basis

P5-T2 iteration 2 reports exactly 3 diagnostics across this plan's 13 write-set files:

| Rule | Severity | File | Line | Autofixable in practice |
|---|---|---|---|---|
| PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 137 | no — measured breaking, see below |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 145 | no — `SuggestedCorrections` count 0 |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 146 | no — `SuggestedCorrections` count 0 |

`PSAvoidUsingWriteHost` emits no suggested correction at all, so no autofix mechanism exists for
it.

`PSUseSingularNouns` does emit a suggested correction, so on iteration 1 the autofix tool was run
rather than reasoned about, and its actual output was measured. The result is recorded in
`poshqc-analyze-autofix.iter1.2026-09-02T23-03.md`: the tool renames each flagged function's
definition only and leaves every call site bound to the old name, producing a non-functional
script. It did this to `Get-CoberturaLineConditionCoverageParts` in
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (call sites at lines 202 and 322 left
unrenamed) and to two functions in the out-of-scope `scripts/vscode/Invoke-VSBuild.ps1` (call
sites at lines 157 and 158 left unrenamed). Both rewrites were reverted and the tree was verified
byte-identical to its pre-autofix state.

That measurement, not an argument from documentation, is why this diagnostic is treated as not
autofixable. The finding is additionally pre-existing and explicitly out of this plan's scope: the
P0-T6 baseline records it as "pre-existing, is not one of the seven findings, and is out of this
plan's scope to change", and
`docs/features/epics/build-ci-coverage-gate-fidelity/feature-audit.2026-08-15T05-11.md` line 66
records it as an accepted finding from an earlier ratified audit.

## Output Summary

No autofixable-and-safe diagnostic is present in this plan's write set on iteration 2, so this
task did not run on this iteration and execution proceeded to P5-T4. No file was changed by this
task on this iteration, so the loop does not restart. The autofix tool was nonetheless exercised
once, on iteration 1, and its behavior is recorded there rather than asserted here.
