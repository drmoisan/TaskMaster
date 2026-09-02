# P2-T6 — Post-change coverage figures, remediation cycle 1

Timestamp: 2026-09-02T01-36

Report read: `coverage/coverage.cobertura.xml`, written by the P2-T5 run.

## Path taken on each side of the comparison

| Side | Task | Path taken |
|---|---|---|
| Baseline | P0-T9 | P0-T8 printed `Done. Coverage artifact:`, so the report was already post-processed; **D1, D2, D3 read `coverage/coverage.cobertura.xml` directly. D4 was not run.** |
| Post-change | P2-T6 | P2-T5 printed `Done. Coverage artifact:`, so the report was already post-processed; **D1, D2, D3, D6 read `coverage/coverage.cobertura.xml` directly. D4 was not run.** |

**Both sides used the same path.** The clause requiring a statement about differing paths does
not apply; there is no need to argue that two different post-processing routes produce the same
denominator, because only one route was used. Comparing an unfiltered report against a
post-processed one is prohibited in either direction, and no unfiltered report was read on
either side.

D1, D2, D3 and D6 were issued inside **one** `pwsh` session, so `$doc` and the helpers
dot-sourced from `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` were assigned before
D2, D3 and D6 read them.

## Derivation D1 — package-set proof of post-processing

Observed package-name list, verbatim, sorted:

```
QuickFiler,SVGControl,Tags,TaskMaster,TaskTree,TaskVisualization,ToDoModel,UtilitiesCS,VBFunctions
```

| Proof condition | Result |
|---|---|
| subset of the nine-name allowlist | PASS — equals the allowlist |
| contains `QuickFiler` | PASS |
| contains no `log4net` entry | PASS |

## Derivation D2 — post-change figures

Raw D2 output:

```
0.853967|55086|64506|0.794522|13170|16576
```

| Attribute | Value | As percentage |
|---|---|---|
| `line-rate` | 0.853967 | **85.40%** |
| `lines-covered` | 55086 | — |
| `lines-valid` | 64506 | — |
| `branch-rate` | 0.794522 | **79.45%** |
| `branches-covered` | 13170 | — |
| `branches-valid` | 16576 | — |

The denominator is non-empty (`lines-valid` = 64506), so no figure above rests on an empty
denominator. Line coverage 85.40% clears the 80% floor in `CLAUDE.md` and the 85% floor in
`.claude/rules/general-unit-test.md`; branch coverage 79.45% clears the 75% floor in
`.claude/rules/quality-tiers.md`.

## Non-vacuity control

`@($doc.SelectNodes('//class[@filename]')).Count` = **561**, identical to the baseline.

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | observed package-name list recorded verbatim | PASS |
| 2 | it is a subset of the nine-name allowlist | PASS — equal to it |
| 3 | it contains `QuickFiler` and no `log4net` entry | PASS |
| 4 | D2 recorded as six numeric values, line-rate and branch-rate also as percentages to two decimal places | PASS |
| 5 | states which path each side used, and (where they differ) that both call `ConvertTo-KoverageCoberturaXml` with the same allowlist and separator | PASS — both sides used the same path, stated above; the differing-path sub-clause does not apply |
