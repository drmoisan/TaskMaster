# P7-T24 — Reconciliation of the spec against the evidence on disk (item 871)

Timestamp: 2026-09-13T17-19
Task: P7-T24
EXIT_CODE: 0

This task reconciles two things that can disagree: the checkbox state of the spec's
`## Acceptance Criteria` section, and the existence on disk of every evidence artifact the P7-T23
completion record names. A checkbox whose named evidence is absent is a reconciliation failure and is
un-checked rather than explained.

## Clause 1 — every one of the 22 checkboxes reads checked

CheckboxesTotal: 22
CheckboxesChecked: 22
CheckboxesUnchecked: 0
ReconciliationFailures: NONE

The 22 checkbox lines were re-read from the spec at this task by an anchored search for a criterion
checkbox at the start of a line. They sit at spec lines 541, 550, 567, 574, 582, 589, 597, 605, 619,
630, 634, 638, 643, 647, 653, 658, 664, 671, 682, 694, 704 and 714, and every one of them reads
`- [x]`. The count of criterion checkboxes in the section is 22, which agrees with the plan's
declared AC1-through-AC22 inventory, so no criterion is missing from the section itself.

## The admitted exception is not taken

AdmittedExceptionTaken: no

P7-T24 admits exactly one exception to clause 1: AC22 may read unchecked when, and only when, P7-T22
recorded its escalation branch because the `FormatterRepairedPreExistingDrift:` line in the P6-T6
record is not `NONE`. That line reads `NONE`, at line 193 of
qa-gates/p6-t6-scope-lock.2026-09-12T10-25.md, so P7-T22 took its normal branch and checked AC22 off.
The exception is therefore not available and is not taken, there is no escalated criterion, and the
AC status summary reports 22 of 22 rather than 21 of 22.

## Starting state, recorded because it was not zero

ACStateAtPhase7Start: 15 checked, 7 unchecked, 22 total.

Phase 7 did not begin from an all-unchecked section. Fifteen criteria — AC1, AC3, AC4, AC5, AC6, AC7,
AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16 and AC17 — were already checked when this phase began,
having been checked off during Phase 4 as the verification task for each passed, which is the timing
the acceptance-criteria tracking protocol directs. The seven that were not — AC2, AC8, AC18, AC19,
AC20, AC21 and AC22 — were each verified against the evidence on disk and then checked off by P7-T2,
P7-T8, P7-T18, P7-T19, P7-T20, P7-T21 and P7-T22 respectively. No already-checked criterion was
un-checked and re-checked; the citation work in the completion record was performed for all 22
regardless of the box's prior state.

## Clause 2 — every named evidence artifact exists on disk

NamedArtifactsDistinct: 35
NamedArtifactsFound: 35
NamedArtifactsMissing: NONE

The completion record names 35 distinct evidence artifacts across its 22 rows. Each was checked for
existence at this task by enumerating the Markdown artifacts under the feature folder's evidence
tree and matching file names. All 35 lie under one of the three canonical evidence directories of
this feature folder, which are the baseline, qa-gates and regression-testing directories. None lies
outside that tree and none lies under a repository-level artifacts directory.

| Evidence kind | Artifacts named | Found |
|---|---|---|
| baseline | 1 | 1 |
| qa-gates | 17 | 17 |
| regression-testing | 17 | 17 |

The single baseline artifact is p0-t12-coverage-baseline.2026-09-12T10-25.md. The 17 qa-gates
artifacts are p2-t1-s1, p2-t5-s2, p2-t9-tests, p3-t5-itemgroup-callsite, p3-t7-out-of-scope-untouched,
p3-t8-format, p4-t3-analyze, p5-t5-coverage-postchange, p5-t6-line-counts-final, p5-t7-tests-final,
p5-t8-loop-result, p6-t1-coverage-file-rates, p6-t2-new-code-coverage, p6-t4-repo-wide-projection,
p6-t5-diff-review, p6-t6-scope-lock and p6-t7-followup-link. The 17 regression-testing artifacts are
p4-t4-seam-contracts, p4-t5-headless-construction, p4-t6-viewer-factory-identity, p4-t7-guards,
p4-t8-success-path, p4-t9-counter-bookkeeping, p4-t10-catch-paths, p4-t11-collection-changed,
p4-t12-move-monitor, p4-t13-digits, p4-t14-carrier, p4-t15-controller-passthrough,
p4-t16-index-mapping, p4-t17-addasync-body, p4-t18-dispatcher-shapes, p4-t19-background-template and
residual-uncovered-regions. Each carries the 2026-09-12T10-25 suffix and the Markdown extension.

Pointers of the form "P#-T# acceptance" are not evidence artifacts and are not counted here. They
name plan tasks whose acceptance condition is a direct in-tree check — a declaration count, a
manifest item, a call-site form — with no artifact of its own. Those tasks are recorded as checked in
the plan file, and the builds and test runs that would have failed had any of them not held are
themselves recorded in the artifacts counted above.

## The two Cobertura documents are named, not cited

The AC19 row names two raw Cobertura documents by file name:
coverage-baseline.2026-09-12T10-25.cobertura.xml and
coverage-postchange.2026-09-12T10-25.cobertura.xml. Neither is on disk and neither was ever
committed; both were deleted after P6-T4 consumed them, under the evidence-hygiene rule recorded
under issue 671 that forbids committing a raw `.cobertura.xml` document. Their absence is not a
reconciliation failure and AC19 is not marked down for it, for two reasons. P7-T19 requires that both
be named by file name, which is a naming requirement and is satisfied. And every figure either
document supplied survives in the P0-T12, P5-T5, P6-T1, P6-T2 and P6-T4 Markdown artifacts, all five
of which are counted above and all five of which exist. The on-disk existence check in clause 2 is
run against those Markdown artifacts, which is what carries the evidence.

## One stale sentence recorded rather than corrected

Line 36 of qa-gates/p5-t8-loop-result.2026-09-12T10-25.md states that the P5-T5 Cobertura document
"sits alongside its Markdown interpretation in the same directory". That was true when P5-T8 was
written and committed in the Phase 5 commit, and it stopped being true when the document was deleted
afterwards. No task in Phase 7 is authorised to edit a committed Phase 5 artifact, and rewriting a
recorded observation to match a later state would falsify the audit trail rather than repair it. The
divergence is therefore recorded here and in the AC19 note of the completion record. It affects no
acceptance condition: P5-T8's gate is the loop outcome it records, not the location of a file.

## Reconciliation outcome

ReconciliationOutcome: COMPLETE
EscalatedCriteria: 0

Output Summary: Reconciliation complete with no failures and no escalated criterion. All 22
acceptance-criteria checkboxes in the spec read checked, verified by re-reading the section at this
task. All 35 distinct evidence artifacts the P7-T23 completion record names exist on disk under the
three canonical evidence directories, split 1 baseline, 17 qa-gates and 17 regression-testing. The
admitted AC22 exception is not taken, because the `FormatterRepairedPreExistingDrift:` line reads
`NONE` and P7-T22 took its normal branch. The AC status summary reports 22 of 22. Two recorded
observations carry forward: the two raw Cobertura documents are named rather than cited and were
deleted under the issue-671 evidence-hygiene rule, and one sentence in the committed P5-T8 artifact
is stale as a result and is recorded rather than edited. Acceptance met.
