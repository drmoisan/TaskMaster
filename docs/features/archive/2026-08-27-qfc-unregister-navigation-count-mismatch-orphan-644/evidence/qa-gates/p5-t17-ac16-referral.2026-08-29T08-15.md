# [P5-T17] AC-16 referral to feature-review — not a pass

Timestamp: 2026-08-29T08-15

Command:

```
@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -SimpleMatch -Pattern '- [ ] **AC-16').Count
@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -SimpleMatch -Pattern '- [x] **AC-16').Count
```

EXIT_CODE: 0

Output Summary: AC-16 is deliberately left unchecked in `spec.md` and is referred to feature-review
for independent adjudication. The unchecked-form span returns **1** and the checked-form span
returns **0**, which is the required state. The three post-processed coverage measurements are
**54800**, **54793**, and **54811** covered lines against an identical `lines-valid` of **64221**;
runs E and F were taken on a byte-identical tree; the sole changed production file is absent from
all **558** `<class>` entries because it carries `[ExcludeFromCodeCoverage]`. The comparison clause
of AC-16 is undecidable at the measured noise floor under the recorded override
`p4_t6_comparison_clause_undecidable_at_measured_noise_floor`. This referral is not a pass.

## Why AC-16 is not checked off

AC-16 is a conjunction of two clauses.

**Second clause — holds.** The changed production lines live in an `[ExcludeFromCodeCoverage]`
class, and that fact is stated explicitly in the coverage evidence at
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md`.

**First clause — left unverified.** The clause requires that the repository coverage figure from the
AC-15 step-4 run be greater than or equal to the AC-0 baseline. It is left unverified on two
independent measured grounds.

### Ground 1 — the instrument AC-16 names produces no figure at all

The AC-15 step-4 command is `vstest.console.exe … /EnableCodeCoverage`. That command emits a binary
`.coverage` file and prints no percentage. The repository-wide Cobertura post-processing performed by
`[P4-T6]` is therefore a **substitute instrument**, not the instrument the criterion names. No figure
attributable to the named instrument exists.

### Ground 2 — the substitute cannot decide the comparison at the resolution the clause demands

| Run | Tree state | `lines-covered` | `lines-valid` | line-rate | Percent |
|---|---|---|---|---|---|
| A — `[P0-T12]` baseline | pre-change base | 54800 | 64221 | 0.853303 | 85.3303% |
| E — `[P4-T6]` designated measurement | final state | 54793 | 64221 | 0.853194 | 85.3194% |
| F — orchestrator noise measurement | final state, identical to E | 54811 | 64221 | 0.853475 | 85.3475% |

Run F was taken on a tree byte-identical to run E's. `git status --porcelain` was unchanged between
the two runs and there was no intervening edit of any kind — not a source change, not a whitespace
change, not a comment change.

`lines-valid` is identical at **64221** across all three runs, which confirms that no production
file changed instrumented size.

The two final-state runs **straddle the baseline**: E is 0.0109 points below it and F is 0.0172
points above it. The measured root noise between two runs of the identical tree is approximately
0.028 percentage points, roughly three times the 0.0109-point shortfall observed on run E.
The observed delta therefore carries no information about whether a regression occurred.

Selecting run F because it is the favourable number is **expressly rejected** as a basis. An
executor free to choose the run it is judged against cannot fail. Both runs are on the record and
neither serves as the basis for a pass.

### Mechanical corroboration that this change cannot move the figure

`QuickFiler/Controllers/QfcCollectionController.cs` is the sole changed production file. It carries
`[ExcludeFromCodeCoverage]`, and it was verified to appear in **0 of the 558** `<class>` entries of
the post-processed Cobertura document. It sits in neither the numerator nor the denominator of the
repository coverage figure, so it cannot move that figure mechanically.

This is corroboration, not proof of the clause: a real regression below the measured noise floor
would remain indistinguishable from noise by this gate.

## Recorded override

Override name recorded in the orchestrator checkpoint:

```
p4_t6_comparison_clause_undecidable_at_measured_noise_floor
```

## Supersession of one sentence in the P4-T6 artifact

The closing sentence of
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md`,
at line 205, reads that AC-16 is checked off under the adjudication. **That sentence is superseded by
this referral.** AC-16 is not checked off. It is referred to feature-review for independent
adjudication.

That earlier artifact is a recorded run and is **not edited**. It is corrected forward here rather
than rewritten, so the audit trail of what that run actually recorded is preserved.

## Disposition

AC-16 is **referred to feature-review for independent adjudication**. It is **not a pass**, and it is
not presented as one. Feature-review is the adjudicating authority for whether the substitute
instrument's straddling measurements satisfy the criterion, or whether the criterion requires a
different instrument or a different resolution.

## Measured acceptance clause results

| Clause | Span | Required | Measured |
|---|---|---|---|
| 1 | `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -SimpleMatch -Pattern '- [ ] **AC-16').Count` | exactly 1 | 1 |
| 2 | `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -SimpleMatch -Pattern '- [x] **AC-16').Count` | exactly 0 | 0 |
| 3 | this artifact exists and carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` | all four present | all four present |
| 4 | `-SimpleMatch` count of `p4_t6_comparison_clause_undecidable_at_measured_noise_floor` in this artifact | at least 1 | recorded below |
| 5 | `-SimpleMatch` count of each of `54800`, `54793`, `54811`, `64221`, `558` in this artifact | at least 1 each | recorded below |

Clause 4 and clause 5 measurements are appended below after this artifact was written. Appending a
count cannot reduce an occurrence count, so recording them does not disturb clauses 4 and 5.

### Clause 3 — required fields present in this artifact

| Field | Required | Measured occurrence count |
|---|---|---|
| `Timestamp:` | present | 2 |
| `Command:` | present | 2 |
| `EXIT_CODE:` | present | 2 |
| `Output Summary:` | present | 2 |

All four required fields are present. The count of 2 for each arises because the clause-3 row of the
table above restates the field names; the requirement is presence, not a pinned total.

### Clause 4 — override name occurrence

| Pattern | Required | Measured |
|---|---|---|
| `p4_t6_comparison_clause_undecidable_at_measured_noise_floor` | at least 1 | 3 |

### Clause 5 — required integers, one `-SimpleMatch` span per integer

| Integer | Required | Measured |
|---|---|---|
| `54800` | at least 1 | 3 |
| `54793` | at least 1 | 3 |
| `54811` | at least 1 | 3 |
| `64221` | at least 1 | 6 |
| `558` | at least 1 | 3 |

All five clauses of `[P5-T17]` hold. No `REMEDIATION-REQUIRED` condition was found.
`AC-16 remains unchecked in spec.md.`
