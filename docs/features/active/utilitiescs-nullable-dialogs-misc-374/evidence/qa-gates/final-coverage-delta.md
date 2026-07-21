# Final QC — Changed-Line Coverage Delta (AC4)

- Timestamp: 2026-07-19T12-45
- Task: [P7-T5]
- Inputs: `evidence/baseline/baseline-coverage.cobertura.xml` (baseline) vs `evidence/qa-gates/final-coverage.cobertura.xml` (post-change)

## Repository-wide coverage

- Baseline: line 83.80% (line-rate 0.838032), branch 76.35% (branch-rate 0.763485).
- Post-change: line 83.82% (line-rate 0.838187), branch 76.38% (branch-rate 0.763759).
- Delta: +0.02 pp line, +0.03 pp branch. No repository-wide regression.

## Per-file coverage for the 14 remediated files (covered/total executable lines)

| File | Baseline cov/tot | Final cov/tot | Delta% |
|---|---|---|---|
| DelegateButtonTemplate.cs | 4/4 | 4/4 | 0.00 |
| FolderNotFoundViewer.cs | 22/22 | 22/22 | 0.00 |
| MyBoxViewer.cs | 81/89 | 81/89 | 0.00 |
| InputBoxViewer.cs | 22/28 | 22/28 | 0.00 |
| ActionButton.cs | 118/123 | 118/123 | 0.00 |
| DelegateButton.cs | 108/122 | 108/122 | 0.00 |
| FunctionButton.cs | 253/265 | 253/265 | 0.00 |
| InputBox.cs | 23/24 | 23/24 | 0.00 |
| NotImplementedDialog.cs | 26/26 | 26/26 | 0.00 |
| MyBox.cs | 207/232 | 207/232 | 0.00 |
| MyBoxModeless.cs | 33/33 | 33/33 | 0.00 |
| YesNoToAll.cs | 61/61 | 61/61 | 0.00 |
| ExtraDeclarations.cs | 0/0 | 0/0 | 0.00 |
| AssemblyInfo.cs | 0/0 | 0/0 | 0.00 |
| **TOTAL (14 files)** | **958/1029 (93.10%)** | **958/1029 (93.10%)** | **0.00** |

## Changed-line coverage assessment (AC4)

REGRESSIONS: NONE. Every remediated file's covered/total line count is identical between baseline and
post-change (per-file delta 0.00%). The cluster line coverage is 93.10% in both runs. This is the
expected result of annotation-only remediation: every change is a non-executable `#nullable enable`
pragma directive, a `?` nullability annotation on a type/parameter/return, or a runtime-neutral `!`
null-forgiving operator on an existing expression. No new executable statement or branch was
introduced, so no changed line lost coverage. AC4 (no coverage regression on changed lines) is
satisfied.

Method: per-file coverage computed by aggregating distinct `<line number hits>` entries across all
`<class>` elements whose filename basename matches each target (Designer siblings excluded), from
both Cobertura documents (`scratchpad/covdelta374.py`).
