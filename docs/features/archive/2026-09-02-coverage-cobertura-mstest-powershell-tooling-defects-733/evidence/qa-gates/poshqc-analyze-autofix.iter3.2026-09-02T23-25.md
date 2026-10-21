# P5-T3 — PoshQC analyze autofix (Final QA Loop, iteration 3, final) — NOT RUN

Timestamp: 2026-09-02T23-25

Status: not run on this iteration, by the task's own branch condition.

## Basis

P5-T2 iteration 3 reports exactly 3 diagnostics across this plan's 14 write-set files, an
identical set to iteration 2 apart from two line-number shifts caused by the remediation refactor:

| Rule | Severity | File | Line | Autofixable in practice |
|---|---|---|---|---|
| PSUseSingularNouns | Warning | scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 137 | no — measured breaking, see below |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 185 | no — `SuggestedCorrections` count 0 |
| PSAvoidUsingWriteHost | Warning | scripts/vscode/Invoke-MSTest.ps1 | 186 | no — `SuggestedCorrections` count 0 |

`PSAvoidUsingWriteHost` emits no suggested correction at all, so no autofix mechanism exists for
it. The remediation moved both call sites into `Invoke-MSTestMain` without altering their text, so
this determination carries over unchanged from iteration 2.

`PSUseSingularNouns` does emit a suggested correction, and the autofix tool was therefore run once,
on iteration 1, rather than reasoned about. Its measured output is recorded in
`poshqc-analyze-autofix.iter1.2026-09-02T23-03.md`: the tool renames each flagged function's
definition only and leaves every call site bound to the old name, producing a non-functional
script. It did this to `Get-CoberturaLineConditionCoverageParts` in
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and to two functions in the out-of-scope
`scripts/vscode/Invoke-VSBuild.ps1`. Both rewrites were reverted and the tree was verified
byte-identical to its pre-autofix state.

That measurement, not an argument from documentation, is why this diagnostic is treated as not
autofixable. The finding is additionally pre-existing and explicitly out of this plan's scope: the
P0-T6 baseline records it as pre-existing, not one of the seven findings, and out of this plan's
scope to change.

The file the remediation edited, `scripts/vscode/Invoke-MSTest.ps1`, carries no
`PSUseSingularNouns` diagnostic, so the remediation introduced no new autofix candidate.

## Output Summary

No autofixable-and-safe diagnostic is present in this plan's write set on iteration 3, so this
task did not run on this iteration and execution proceeded to P5-T4. No file was changed by this
task on this iteration, so the loop does not restart.
