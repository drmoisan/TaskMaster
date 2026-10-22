# P2-T6 — Post-change coverage figures

Timestamp: 2026-09-01T23-17

Report read: `coverage/coverage.cobertura.xml`, written by the P2-T5 run of the final toolchain pass.

## Which path each side used

**Both sides used the same path.** P0-T8 printed the literal `Done. Coverage artifact:` and so did
P2-T5. That line is emitted only after `ConvertTo-KoverageCoberturaXml` post-processing and the
on-disk write both succeed, so both documents are post-processed. **Derivation D4 was not used on
either side.**

Because the two sides used the same path, the clause requiring a reconciliation when they differ is
not engaged. It is recorded anyway that no unfiltered report was compared against a post-processed
one, in either direction: every figure below and every figure in
`evidence/baseline/coverage-baseline.md` was read from a document that had passed through
`ConvertTo-KoverageCoberturaXml` with the same allowlist and the same path separator, so the two
denominators are constructed identically and differ only by the change itself.

## Derivation D1 — package-set proof of post-processing

Observed package-name list, verbatim:

```
QuickFiler,SVGControl,Tags,TaskMaster,TaskTree,TaskVisualization,ToDoModel,UtilitiesCS,VBFunctions
```

Package count: 9. Proof conditions, all three satisfied:

1. **Subset of the nine-name allowlist.** The observed set is byte-identical to it.
2. **Contains `QuickFiler`.** Yes.
3. **Contains no `log4net` entry.** Confirmed; no third-party package name appears at all.

## Derivation D2 — root-level figures

Raw output:

```
0.854119|55083|64491|0.794494|13160|16564
```

| Attribute | Baseline (P0-T9) | Post-change | Delta |
|---|---:|---:|---:|
| `line-rate` | 0.853973 | **0.854119** | +0.000146 |
| `lines-covered` | 55001 | **55083** | +82 |
| `lines-valid` | 64406 | **64491** | +85 |
| `branch-rate` | 0.794239 | **0.794494** | +0.000255 |
| `branches-covered` | 13124 | **13160** | +36 |
| `branches-valid` | 16524 | **16564** | +40 |

Expressed as percentages to two decimal places:

- **Repository-wide line coverage: 85.41 %** (baseline 85.40 %). Carried to four places the figures
  are 85.4119 % post-change against 85.3973 % baseline, a change of **+0.0146 percentage points**.
- **Repository-wide branch coverage: 79.45 %** (baseline 79.42 %), a change of **+0.0255 percentage
  points**.

Both moved slightly **up**. No placeholder value appears above; every figure is a measured number
read from the post-processed document.

### Run-to-run variation, measured and stated

The Phase 2 toolchain loop restarted twice, so the coverage suite ran three times on a passing tree.
The figures above are from the **final** pass, which is the pass of record. Two earlier passes on
nearly identical trees produced `lines-covered` of 55066 and 55075 against the same
`lines-valid` of 64490 and 64491, a spread of 17 covered lines, or about 0.026 percentage points.
The variation sits entirely in the `UtilitiesCS` package (38606 / 38608 / 38614 across the three
passes); **every `QuickFiler` per-file and per-member figure was identical across all three passes**.

This is recorded because it bounds how much of the +0.0146 pp repository-wide movement can be
attributed to the change: the run-to-run spread is of the same order, so the repository-wide figure
supports the claim that coverage did not regress but does not by itself prove a gain. The
change-scoped figures in `coverage-delta.md`, which are stable across passes, carry that argument.

## Policy-floor reconciliation

- `CLAUDE.md` floor: line >= 80 %. Observed 85.40 %. **Met.**
- `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`: line >= 85 %,
  branch >= 75 %. Observed 85.41 % and 79.45 %. **Both met.**

The line figure clears the 85 % floor by 0.41 percentage points, essentially the same narrow margin
as at baseline. The change did not erode it.

EVIDENCE_SUBSTITUTION: the raw Cobertura report `coverage/coverage.cobertura.xml` measures 194268
lines by Derivation D8 and 10810057 bytes on disk. It is retained untracked under the git-ignored
`coverage/` directory (`.gitignore:144`) and is deliberately **not** committed, because a
full-repository Cobertura document of that size is too large to carry in permanent history. The
committed substitute is
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/coverage-post-change.jacoco.xml`,
whose `LINE` counter totals reproduce the `lines-covered` and `lines-valid` values recorded above.
