# P0-T12 — Coverage-threshold reconciliation

Timestamp: 2026-09-03T08-29

Command:
```text
(documentary task; the reconciliation is derived from the policy files read in P0-T1 and from the
baseline coverage figures recorded in P0-T10)
```

EXIT_CODE: 0

## Output Summary

### Conflicting sources

| Rank | Source | Line coverage | Branch coverage | New-code coverage |
|---|---|---|---|---|
| 1 | `CLAUDE.md` | `>= 80%` repository-wide | not specified | `>= 90%` for any new module, class, or method |
| 3 | `.claude/rules/general-unit-test.md` | `>= 85%` | `>= 75%` | not specified |
| 4 | `.claude/rules/quality-tiers.md` | `>= 85%` (uniform T1-T4) | `>= 75%` (uniform T1-T4) | not specified |

**`CLAUDE.md` is the rank-1 authority** under `policy-compliance-order`, which places `CLAUDE.md`
first and the `.claude/rules/` files after it. Its figures therefore supersede the rank-3 and rank-4
figures for this plan. The figures this plan enforces are consequently `>= 80%` repository line
coverage and `>= 90%` new-code coverage.

The `>= 85%` line and `>= 75%` branch figures in the two rule files are recorded here rather than
silently discarded. The divergence is pre-existing, is repository-wide, and is not resolved or
narrowed by this bug fix.

### What this plan actually gates on

The enforced repository-level gate is **no regression relative to the P0-T10 baseline**, applied in
P4-T7 clause (d) with a 0.005 tolerance. The absolute `>= 80%` figure is recorded as an observation
rather than gated, because the baseline is measured rather than assumed and a floor asserted against
an unmeasured baseline could be unsatisfiable for reasons this change did not cause.

### Baseline line-rate quoted from P0-T10

`line-rate` = **0.7073317347831605**

Supporting values from the same root `<coverage>` element: `lines-covered` = 105901, `lines-valid`
= 149719.

This figure is the raw unstripped `dotnet-coverage` line rate for the `UtilitiesCS.Test` process. It
is **not** comparable to CLAUDE.md's `>= 80%` repository figure, which refers to the repository's
first-party testable denominator after third-party stripping and after the COM/VSTO/WinForms
exemption CLAUDE.md ratifies. Because the baseline figure (0.7073, i.e. 70.73%) already sits below
the 80% policy floor on this unstripped basis, a post-change figure below that floor is recorded by
P4-T7 as `PRE-EXISTING FLOOR SHORTFALL` and is not attributed to this change.

## Acceptance

The artifact names `CLAUDE.md` explicitly as the superseding rank-1 authority and quotes the concrete
baseline `line-rate` value `0.7073317347831605` recorded in P0-T10.
