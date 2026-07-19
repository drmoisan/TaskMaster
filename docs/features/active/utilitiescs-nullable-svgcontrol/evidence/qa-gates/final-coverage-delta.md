# Final QC — Coverage Delta (Baseline vs. Post-Change)

Timestamp: 2026-07-19T04-40

Comparing:
- Baseline: `evidence/baseline/baseline-coverage.cobertura.xml`
- Post-change: `evidence/qa-gates/final-coverage.cobertura.xml`

## Overall (`SVGControl` package)

| Metric | Baseline | Post-Change | Delta |
|---|---|---|---|
| line-rate | 0.266544 (26.65%) | 0.266381 (26.64%) | -0.000163 |
| branch-rate | 0.322807 (32.28%) | 0.322807 (32.28%) | 0 |
| lines-covered | 870 | 870 | 0 |
| lines-valid | 3264 | 3266 | +2 |
| branches-covered | 368 | 368 | 0 |
| branches-valid | 1140 | 1140 | 0 |

The marginal line-rate decrease is arithmetic dilution only: `lines-covered` is unchanged (no
previously-covered line lost coverage), while `lines-valid` grew by 2 — additional
instrumentable-but-never-covered lines introduced by the `#nullable enable` pragma / annotation
edits in the 0%-baseline remediation-target files (first observed at Batch D; see
`evidence/regression-testing/batch-d-tests.md`). Branch-rate and branches-covered/valid are
completely unchanged.

## `RelativePath.cs` — the one file in scope with a real automated baseline

| Metric | Baseline | Post-Change | Delta |
|---|---|---|---|
| line-rate | 0.567529 (56.75%) | 0.567529 (56.75%) | 0 |
| branch-rate | 0.543544 (54.35%) | 0.543544 (54.35%) | 0 |

**No coverage regression on `RelativePath.cs`'s changed lines** — `RelativePath.cs` is verify-only
in this feature (no edits were made to it in any phase); its coverage numbers are byte-identical
between baseline and final (AC4 satisfied for this file).

## The 12 hand-authored remediation-target files

Per the plan's documented coverage posture (confirmed by research and reconfirmed at every batch
gate in Phases 1-5), the automated changed-line coverage baseline for all 12 hand-authored
remediation-target files (`ButtonSVG.cs`, `PictureBoxSVG.cs`, `ToggleSwitch.cs`, `SVGParser.cs`,
`SvgRenderer.cs`, `SvgImageSelector.cs`, `ISvgResource.cs`, `SvgOptionsConverter.cs`,
`SvgOptionsConverter2.cs`, `SvgResourceConverter.cs`, `DropDownEditor.cs`,
`SVGFileNameEditor.cs`) is **0%** — `SVGControl.Test`'s two test classes
(`GetRelativePath_Test.cs`, `RelativePathCoverageTests.cs`) exercise only `RelativePath.cs` and
never instantiate or invoke any of the 12 files. This makes the numeric AC4 gate **vacuous** for
these 12 files specifically: there is no automated baseline percentage to regress from or against.
This is explicitly stated here rather than omitted. Behavior preservation for these 12 files (AC3)
was instead protected by conservative annotation choices (justified `!` over new fallback values
or guard clauses) documented per-batch in `evidence/qa-gates/batch-{a..e}-nullable-gate.md` and, for
the single most consequential judgment call, in
`evidence/other/imagepath-judgment-call-decision.md`.

## Conclusion

No coverage regression on changed lines is confirmed for `RelativePath.cs` (the only file with a
real automated baseline). The overall package-level line-rate movement is fully explained by
non-regressive denominator growth (added uncovered lines in already-0%-baseline files), not by any
previously-covered line becoming uncovered. AC4 is satisfied for `RelativePath.cs`; AC4 is
numerically vacuous (not failing) for the 12 remediation-target files, as documented above and
throughout this plan's execution.
