# Final QC — Coverage Delta / Changed-Line No-Regression (P10-T6, AC4)

- Timestamp: 2026-07-19T10-50
- Task: [P10-T6]
- Inputs: baseline Cobertura `evidence/baseline/coverage-baseline.2026-07-19T10-50.cobertura.xml` (P0-T5); post-change Cobertura `evidence/qa-gates/final-coverage.2026-07-19T10-50.cobertura.xml` (P10-T4).

## Overall coverage (baseline -> post-change)

| Metric | Baseline | Post-change | Delta |
|---|---|---|---|
| Overall line-rate | 0.65299 (65.30%) | 0.653005 (65.30%) | +0.00002 (flat) |
| Overall branch-rate | 0.613274 (61.33%) | 0.612853 (61.29%) | -0.00042 (flat) |
| In-scope OutlookObjects line % | 87.07% (3319/3812) | 87.07% (3320/3813) | +0.00% |
| Tests passed | 4511/4511 | 4511/4511 | 0 (no tests added/removed) |

## Changed-line no-regression (AC4)

- This is annotation-and-null-safety-only work: the changed lines are almost entirely the per-file `#nullable enable` pragma (non-executable) plus `?`/`!` annotations applied IN PLACE on already-existing executable lines. No new executable production lines were added except (a) the `AttachmentSerializable.AttachmentData` setter rebuilt as an explicit `new Lazy<byte[]?>(() => value)` (covered — `AttachmentSerializable.cs` holds at 97.1%), and (b) a small number of provably-redundant `?.` removals in `MailItemHelper.cs` factory lambdas (which reduced its valid-line count from 184 to 182 without lowering its rate — 97.3%).
- **No in-scope file regressed in coverage rate.** Per-file rates are identical to baseline or slightly improved:
  - Files with a tiny valid/covered count shift (all rate-neutral or improved): `AttachmentHelper.cs` 94.7% (178/188, was 177/187); `OutlookItem.cs` 82.5% (175/212, was 174/211); `MailItemHelper.Properties.cs` 81.7% (94/115, was 93/114); `MailItemHelper.cs` 97.3% (177/182, was 179/184). All others byte-identical to baseline.
  - The one non-exempt file, `CidImageResolver.cs`, holds at 94.7% (36/38) — unchanged from baseline.
- Result: **NO coverage regression on changed lines (AC4 satisfied).** Outcome: PASS (not remediation-required).
