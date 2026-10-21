# Phase 0 — baseline coverage figures (P0-T9)

Timestamp: 2026-09-01T22-10

Report read: `coverage/coverage.cobertura.xml`. P0-T8 printed the literal `Done. Coverage artifact:`,
which is emitted only after `ConvertTo-KoverageCoberturaXml` post-processing and the on-disk write
both succeed, so the file on disk is the post-processed document. **Derivation D4 was not required
and was not used on the baseline side.**

## Derivation D1 — package-set proof of post-processing

Command:

```powershell
. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
$doc = [xml](Get-Content -LiteralPath 'coverage/coverage.cobertura.xml' -Raw -Encoding UTF8)
$names = @($doc.SelectNodes('//package') | ForEach-Object { $_.GetAttribute('name') } | Sort-Object)
$names -join ','
```

Observed package-name list, verbatim:

```
QuickFiler,SVGControl,Tags,TaskMaster,TaskTree,TaskVisualization,ToDoModel,UtilitiesCS,VBFunctions
```

Package count: 9.

Proof conditions, all three satisfied:

1. **Subset of the allowlist.** The allowlist derived from the nine non-test project files in this
   tree is, sorted:
   `QuickFiler,SVGControl,Tags,TaskMaster,TaskTree,TaskVisualization,ToDoModel,UtilitiesCS,VBFunctions`.
   The observed set is byte-identical to it, and is therefore a subset of it.
2. **Contains `QuickFiler`.** Yes.
3. **Contains no `log4net` entry.** Confirmed: no third-party package name appears at all.

The XPath form was used, not a line search for the text `<package name=`. That naive search returns
zero matches against this document because the element emits `name` after `line-rate`.

## Derivation D2 — root-level figures (BASELINE_COVERAGE)

Command:

```powershell
$c = $doc.SelectSingleNode('/coverage')
'{0}|{1}|{2}|{3}|{4}|{5}' -f $c.GetAttribute('line-rate'), $c.GetAttribute('lines-covered'), $c.GetAttribute('lines-valid'), $c.GetAttribute('branch-rate'), $c.GetAttribute('branches-covered'), $c.GetAttribute('branches-valid')
```

Raw output:

```
0.853973|55001|64406|0.794239|13124|16524
```

### Output Summary — BASELINE_COVERAGE

| Attribute | Value |
|---|---:|
| `line-rate` | 0.853973 |
| `lines-covered` | 55001 |
| `lines-valid` | 64406 |
| `branch-rate` | 0.794239 |
| `branches-covered` | 13124 |
| `branches-valid` | 16524 |

Expressed as percentages to two decimal places:

- **Repository-wide line coverage: 85.40 %**
- **Repository-wide branch coverage: 79.42 %**

No placeholder value appears above; every figure is a measured number read from the post-processed
document.

## Policy-floor reconciliation at baseline

- `CLAUDE.md` floor: line >= 80 %. Observed 85.40 %. **Met.**
- `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` floors: line >= 85 %,
  branch >= 75 %. Observed 85.40 % and 79.42 %. **Both met.**

No pre-existing shortfall against any policy floor exists at baseline. The line figure clears the
85 % floor by 0.40 percentage points, which is a narrow margin: a change that adds uncovered lines
can cross it, so P2-T6 restates both figures and P2-T7 states the difference.

EVIDENCE_SUBSTITUTION: the raw Cobertura report `coverage/coverage.cobertura.xml` measures 194037
lines by Derivation D8 and 10796787 bytes on disk. It is retained untracked under the git-ignored
`coverage/` directory (`.gitignore:144`) and is deliberately **not** committed, because a
full-repository Cobertura document of that size is too large to carry in permanent history. The
committed substitute is the package-level summary at
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/coverage-baseline.jacoco.xml`,
whose `LINE` counter totals reproduce the `lines-covered` and `lines-valid` values recorded above.
