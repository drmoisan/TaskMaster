# Issue #644 — Acceptance Criteria Status Summary

Timestamp: 2026-08-29T08-15

PostedAs: unknown

Command:

```
@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -SimpleMatch -Pattern '- [ ] **AC-').Count
@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -SimpleMatch -Pattern '- [x] **AC-').Count
```

EXIT_CODE: 0

Output Summary: The unchecked-form span returns **1** and the checked-form span returns **17**,
against 18 total acceptance criteria in `spec.md`. The single remaining item is **AC-16**, which is
referred to feature-review for independent adjudication under the recorded override
`p4_t6_comparison_clause_undecidable_at_measured_noise_floor` and is **not presented as a pass**.

## POSTING BLOCKED

This artifact was not posted to GitHub. Reason: the approved plan
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/plan.2026-08-29T07-42.md`
contains no task that posts to the issue or opens a pull request. `[P5-T20]`, the plan's final task,
states that PR authoring and CI monitoring are orchestrator steps outside this plan. Posting this
summary to issue #644 is therefore an orchestrator step, not an executor step, and it was not
performed here.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md`
- Total AC items: 18
- Checked off (delivered): 17
- Remaining (unchecked): 1
- Items remaining: AC-16 (no coverage regression on changed lines)

### Measured reconciliation spans

| Span | Required | Measured |
|---|---|---|
| `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -SimpleMatch -Pattern '- [ ] **AC-').Count` | 1 | 1 |
| `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -SimpleMatch -Pattern '- [x] **AC-').Count` | 17 | 17 |

Both spans read `spec.md` from disk with `Select-String` rather than through `git grep`, so the
assertion holds regardless of whether the file is tracked at the time of the run and regardless of
whether the working-tree edits are committed. `-SimpleMatch` is required because the patterns carry
`[`, `]`, and `*`.

### Delivered criteria (17)

AC-0, AC-1, AC-2, AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, AC-9, AC-10, AC-11, AC-12, AC-13, AC-14,
AC-15, AC-17.

## The remaining item — AC-16

Criterion text, quoted verbatim from `spec.md`:

> **AC-16 (no coverage regression on changed lines).** The repository coverage figure from the
> AC-15 step-4 run is greater than or equal to the AC-0 baseline. Changed production lines live
> in a `[ExcludeFromCodeCoverage]` class and are therefore outside the denominator; that fact is
> stated explicitly in the coverage evidence artifact so the gate is not read as vacuously
> satisfied.

**Disposition: referred to feature-review for independent adjudication. Not a pass.**

Recorded override:

```
p4_t6_comparison_clause_undecidable_at_measured_noise_floor
```

The second clause of AC-16 holds and is recorded in
`evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md`. The first clause is left unverified on
two measured grounds: the instrument AC-16 names emits a binary `.coverage` file and prints no
percentage, so the Cobertura post-processing used is a substitute instrument; and the substitute's
two final-state runs on a byte-identical tree straddle the baseline, giving a measured noise floor of
approximately 0.028 percentage points against a 0.0109-point observed shortfall. The full referral,
with the three measured figures and the mechanical corroboration that the changed file sits outside
the coverage denominator, is recorded in
`evidence/qa-gates/p5-t17-ac16-referral.2026-08-29T08-15.md`.

AC-16 is left unchecked in `spec.md` deliberately. It is not counted as delivered anywhere in this
summary.
