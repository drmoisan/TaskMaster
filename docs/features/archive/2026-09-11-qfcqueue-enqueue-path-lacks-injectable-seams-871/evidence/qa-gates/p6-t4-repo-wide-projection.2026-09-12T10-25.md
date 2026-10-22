# P6-T4 — Projected repository-wide line coverage

Timestamp: 2026-09-13T17-00
Command: read of the coverage root element of the reference document, then arithmetic over the QuickFiler package figures P0-T12 and P5-T5 recorded
EXIT_CODE: 0

## Why a projection rather than a measurement

A repository-wide measurement cannot be taken on this host. A whole-solution local run pulls in four
shell-icon test classes in another assembly that stall the test runner on this machine; that is an
environmental property of this host rather than a regression, and continuous integration covers those
classes. Every coverage run in this item is therefore scoped to the single QuickFiler test assembly, and
the repository-wide figure is projected from the most recent committed repository-wide Cobertura document
in the tree.

## Reference document

Path: docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/coverage-postchange.cobertura.xml

That is the post-change coverage artifact under the evidence qa-gates directory of the 2026-09-08
etl-deadline-mechanics follow-ups feature folder for item 825, a file whose name begins with the words
coverage postchange.

ReferenceLinesCovered: 56029
ReferenceLinesValid: 65402
ReferenceLineRate: 0.856686

All three were re-derived from the coverage root element of that document in this pass, not copied from
the plan. The document spans nine packages — QuickFiler, UtilitiesCS, TaskVisualization, SVGControl,
ToDoModel, Tags, TaskMaster, TaskTree and VBFunctions — against the six that a QuickFiler-scoped run
produces, which is why it is usable as a repository-wide reference at all.

## QuickFiler package figures from the two scoped runs

Both figures in each row come from `Get-CoberturaPackageLineSummary` applied to the `package` element
whose `name` attribute is QuickFiler. That helper does emit `LinesCovered` and `LinesValid` under exactly
those names, so no property-name mapping is required at package level; the mapping that P0-T12 recorded
applies to the class-level helper only.

| Run | LinesCovered | LinesValid | LineRate |
|---|---|---|---|
| P0-T12 baseline | 10215 | 12603 | 0.810521 |
| P5-T5 post-change | 10314 | 12626 | 0.816886 |

CoveredDelta: 99
ValidDelta: 23

The change added 23 executable lines to the QuickFiler assembly and covered 99 more than before. The
covered delta exceeds the valid delta because the new tests cover statements that already existed and
were previously unreached, not only the statements this change introduced.

## Projected repository-wide rate

```
ProjectedLinesCovered = 56029 + 99    = 56128
ProjectedLinesValid   = 65402 + 23    = 65425
ProjectedLineRate     = 56128 / 65425 = 0.857898
```

ProjectedLinesCovered: 56128
ProjectedLinesValid: 65425
ProjectedLineRate: 0.857898

## Gate clause 1 — the QuickFiler package delta rate, which is the discriminating clause

The covered delta divided by the valid delta must be at least 0.80.

```
99 / 23 = 4.304348
4.304348 >= 0.80   TRUE
```

Clause1DeltaRate: 4.304348
Clause1Verdict: PASS

The valid delta is positive, so the alternative branch of this clause — the one that applies when the
valid delta is zero or negative and requires only that the covered delta be zero or greater — is not
entered and no rate substitution is made.

A delta rate above 1 is arithmetically ordinary here and is not an error: the numerator counts every
newly covered line in the assembly while the denominator counts only the net growth in executable lines.
A change that adds 23 lines and covers 22 of them while also reaching 77 previously unreached existing
lines produces exactly this shape. The clause is nonetheless the discriminating one, because a change
that added executable lines without covering them would drive this quotient below 0.80 while leaving the
projected repository-wide rate essentially unmoved.

## Gate clause 2 — the projected repository-wide rate

The projected rate must be at least 0.80, which is the repository-wide floor stated in the standing
instructions file.

```
0.857898 >= 0.80   TRUE
```

Clause2ProjectedRate: 0.857898
Clause2Verdict: PASS

This clause is dominated by the reference document and cannot on its own discriminate the quality of this
change. 56029 divided by 65402 is already 0.856686, and no delta this change can produce moves that
quotient below 0.80: the whole QuickFiler package contributes 12626 valid lines against a repository
denominator of 65425, so even a hypothetical change that added 23 wholly uncovered lines would move the
projection to 0.856385. The second clause therefore records the floor, while the first clause is what can
fail.

The projection moves the rate upward by 0.001212, from 0.856686 to 0.857898.

## Overall verdict

P6T4Verdict: PASS

Both clauses pass.

## Coverage-floor divergence, recorded as a factual note and not as a gate

The floors this plan uses are 80 percent repository-wide and 90 percent for new code, as stated in the
standing instructions file at the repository root. A competing figure of 85 percent line coverage and 75
percent branch coverage appears in the general unit-test rule file and in the quality-tiers rule file
under the repository rules directory.

The standing instructions file states its own policy compliance order and places itself first in that
order; it does not name either of those two rule files. This plan therefore applies the 80 and 90 figures
and records the divergence without resolving it. Recording it is not adopting it: no gate in this item is
evaluated against 85 or 75. For information only, the projected rate of 0.857898 would also clear an 85
percent line floor, and the branch figures this change produced are recorded per file in the P5-T5
artifact.

No tier-classification criterion is written anywhere in this plan, because the tier manifest that the
quality-tiers rule file refers to does not exist at the repository root and no pipeline stage validates
one. A step asking to confirm a project's tier would be unsatisfiable.

## Deletion of the two raw Cobertura documents

This task is the last consumer of the two raw Cobertura documents this item produced. Repository
evidence-hygiene policy recorded under issue 671 forbids committing a raw `.cobertura.xml`, and both were
written and left untracked so that every acceptance condition reading them was satisfiable. Both are
deleted immediately after this artifact is written:

- docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
- docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/coverage-postchange.2026-09-12T10-25.cobertura.xml

RawCoberturaDocumentsDeleted: 2

No task after this one reads either document. P6-T5 and later read no Cobertura document at all, and
P7-T19 names both by file name without opening them. The deletion is required rather than optional,
because P7-T25 asserts that the porcelain status contains no line naming a path under the three canonical
evidence directories, and an untracked raw document under one of them would violate that assertion.

No document named `artifacts/csharp/coverage.xml` was created by this item at any point. A repository hook
activates an 85 percent floor only when that path exists, repository-wide raw coverage measured on this
host is far below it because of the single-assembly denominator, and creating the file would manufacture
a failure that reflects nothing about this change.

Output Summary: Clause 1 PASS — the QuickFiler package delta rate is 99 covered over 23 valid, or
4.304348, against a floor of 0.80; the valid delta is positive so the zero-or-negative branch is not
entered. Clause 2 PASS — the projected repository-wide rate is 56128 over 65425, or 0.857898, against the
0.80 floor, up 0.001212 from the reference 0.856686 that was re-derived from the item 825 document this
pass. The second clause is recorded as dominated by the reference document and unable to discriminate.
The competing 85 and 75 figures in the two rule files are recorded factually and not adopted. Both raw
Cobertura documents were deleted after this task. Acceptance met.
