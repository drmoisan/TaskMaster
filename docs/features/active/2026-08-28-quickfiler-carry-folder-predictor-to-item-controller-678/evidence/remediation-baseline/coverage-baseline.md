# Baseline — Coverage figures (`R_BASELINE_COVERAGE`)

- Timestamp: 2026-09-02T01-09
- Issue: #678
- Task: [P0-T9]
- Source document: `coverage/coverage.cobertura.xml`

## Path taken

P0-T8 printed the literal `Done. Coverage artifact:`, so the report at
`coverage/coverage.cobertura.xml` is already post-processed. Derivation **D4 was not
required and was not run**. Derivations D1, D2 and D3 were issued inside one `pwsh` session,
so `$doc` and the dot-sourced helpers from
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` were assigned before D2 and D3 read
them.

## Derivation D1 — package-set proof of post-processing

Observed package-name list, verbatim, sorted:

```
QuickFiler,SVGControl,Tags,TaskMaster,TaskTree,TaskVisualization,ToDoModel,UtilitiesCS,VBFunctions
```

Allowlist derived from the nine non-test project files in this tree, sorted:

```
QuickFiler,SVGControl,Tags,TaskMaster,TaskTree,TaskVisualization,ToDoModel,UtilitiesCS,VBFunctions
```

| Proof condition | Result |
|---|---|
| Observed set is a subset of the nine-name allowlist | PASS — the observed set equals the allowlist |
| Observed set contains `QuickFiler` | PASS |
| Observed set contains no `log4net` entry | PASS |

An unfiltered `dotnet-coverage` report carries third-party packages including `log4net`;
their absence together with the exact nine-name match establishes that
`ConvertTo-KoverageCoberturaXml` ran over this document.

The XPath form is the only accepted derivation for this proof: a line search for the text
`<package name=` returns zero matches against this XML, because the element emits `name`
after `line-rate`.

## Derivation D2 — `R_BASELINE_COVERAGE`

Raw D2 output:

```
0.853964|55073|64491|0.794373|13158|16564
```

| Attribute | Value | As percentage |
|---|---|---|
| `line-rate` | 0.853964 | **85.40%** |
| `lines-covered` | 55073 | — |
| `lines-valid` | 64491 | — |
| `branch-rate` | 0.794373 | **79.44%** |
| `branches-covered` | 13158 | — |
| `branches-valid` | 16564 | — |

```
R_BASELINE_COVERAGE = line-rate 0.853964 (85.40%), lines-covered 55073, lines-valid 64491,
                      branch-rate 0.794373 (79.44%), branches-covered 13158,
                      branches-valid 16564
```

These six attributes are written by `ConvertTo-KoverageCoberturaXml` and exist only on a
post-processed document, so D2 is meaningful here precisely because D1 passed. No
placeholder value appears above; every figure is read from the document.

The line rate of 85.40% is above the repository-wide 80% floor in `CLAUDE.md` and above the
85% floor in `.claude/rules/general-unit-test.md`. The branch rate of 79.44% is above the
75% branch floor in `.claude/rules/quality-tiers.md`. The denominator is non-empty
(`lines-valid` = 64491), so no figure above rests on an empty denominator.

## Non-vacuity control

`@($doc.SelectNodes('//class[@filename]')).Count` = **561**.

## Output Summary

Package list equals the nine-name allowlist exactly, contains `QuickFiler`, contains no
`log4net`; the report is post-processed. `R_BASELINE_COVERAGE` = line-rate 0.853964
(85.40%), 55073/64491 lines; branch-rate 0.794373 (79.44%), 13158/16564 branches. Class-node
control 561.
