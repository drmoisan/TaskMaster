# P7-T23 — Acceptance-criteria completion record (item 871)

Timestamp: 2026-09-13T17-19
Task: P7-T23
Scope: one row per acceptance criterion of the `## Acceptance Criteria` section of
docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md,
carrying the evidence pointer each of P7-T1 through P7-T22 recorded.

## Evidence pointer notation

Each pointer is written as `<evidence-kind>/<file name>` and resolves against
docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/.
The three permitted kinds are baseline, qa-gates and regression-testing. A pointer written as
"P#-T# acceptance" names a plan task whose acceptance condition is a direct in-tree check with no
artifact of its own; those tasks are recorded as checked in the plan file and are named here because
the P7 task text names them.

## Rows

| AC | Recorded by | Evidence pointers |
|---|---|---|
| AC1 | P7-T1 | qa-gates/p2-t1-s1.2026-09-12T10-25.md; regression-testing/p4-t4-seam-contracts.2026-09-12T10-25.md; qa-gates/p2-t9-tests.2026-09-12T10-25.md |
| AC2 | P7-T2 | P2-T3 acceptance; P2-T5 acceptance, recorded in qa-gates/p2-t5-s2.2026-09-12T10-25.md; regression-testing/p4-t18-dispatcher-shapes.2026-09-12T10-25.md; qa-gates/p6-t5-diff-review.2026-09-12T10-25.md |
| AC3 | P7-T3 | P3-T1 acceptance; regression-testing/p4-t6-viewer-factory-identity.2026-09-12T10-25.md; regression-testing/p4-t17-addasync-body.2026-09-12T10-25.md |
| AC4 | P7-T4 | P3-T2 acceptance; regression-testing/p4-t4-seam-contracts.2026-09-12T10-25.md; regression-testing/p4-t17-addasync-body.2026-09-12T10-25.md |
| AC5 | P7-T5 | P3-T3 acceptance; P3-T5 acceptance, recorded in qa-gates/p3-t5-itemgroup-callsite.2026-09-12T10-25.md; the `PostFormatStatement:` line at line 59 of qa-gates/p3-t8-format.2026-09-12T10-25.md; regression-testing/p4-t4-seam-contracts.2026-09-12T10-25.md; regression-testing/p4-t16-index-mapping.2026-09-12T10-25.md |
| AC6 | P7-T6 | P3-T4 acceptance; P3-T6 acceptance; regression-testing/p4-t19-background-template.2026-09-12T10-25.md |
| AC7 | P7-T7 | regression-testing/p4-t4-seam-contracts.2026-09-12T10-25.md; regression-testing/p4-t5-headless-construction.2026-09-12T10-25.md |
| AC8 | P7-T8 | qa-gates/p5-t6-line-counts-final.2026-09-12T10-25.md, carrying the seven post-format measured counts 269, 200, 329, 108, 35, 425 and 343. No predicted figure is cited. |
| AC9 | P7-T9 | P1-T3 acceptance; P2-T4 acceptance; P4-T2 acceptance; qa-gates/p4-t3-analyze.2026-09-12T10-25.md as the positive verification of the three production manifest entries; regression-testing/p4-t4-seam-contracts.2026-09-12T10-25.md as the positive verification of the two test-project entries; regression-testing/p4-t5-headless-construction.2026-09-12T10-25.md as the positive reference from a test to a type declared in each new production file |
| AC10 | P7-T10 | regression-testing/p4-t7-guards.2026-09-12T10-25.md |
| AC11 | P7-T11 | regression-testing/p4-t8-success-path.2026-09-12T10-25.md |
| AC12 | P7-T12 | regression-testing/p4-t10-catch-paths.2026-09-12T10-25.md |
| AC13 | P7-T13 | regression-testing/p4-t9-counter-bookkeeping.2026-09-12T10-25.md; regression-testing/p4-t8-success-path.2026-09-12T10-25.md; regression-testing/p4-t10-catch-paths.2026-09-12T10-25.md |
| AC14 | P7-T14 | regression-testing/p4-t11-collection-changed.2026-09-12T10-25.md |
| AC15 | P7-T15 | regression-testing/p4-t12-move-monitor.2026-09-12T10-25.md |
| AC16 | P7-T16 | regression-testing/p4-t13-digits.2026-09-12T10-25.md; regression-testing/p4-t14-carrier.2026-09-12T10-25.md; regression-testing/p4-t15-controller-passthrough.2026-09-12T10-25.md |
| AC17 | P7-T17 | regression-testing/p4-t17-addasync-body.2026-09-12T10-25.md |
| AC18 | P7-T18 | qa-gates/p6-t5-diff-review.2026-09-12T10-25.md, PASS on all eight properties; qa-gates/p5-t8-loop-result.2026-09-12T10-25.md, loop pass 1 completed with the formatter rewriting no file |
| AC19 | P7-T19 | baseline/p0-t12-coverage-baseline.2026-09-12T10-25.md, the baseline Cobertura interpretation; qa-gates/p5-t5-coverage-postchange.2026-09-12T10-25.md, the post-change Cobertura interpretation; qa-gates/p6-t1-coverage-file-rates.2026-09-12T10-25.md; qa-gates/p6-t2-new-code-coverage.2026-09-12T10-25.md; qa-gates/p6-t4-repo-wide-projection.2026-09-12T10-25.md. The two Cobertura documents are named by file name in the AC19 note below. |
| AC20 | P7-T20 | regression-testing/residual-uncovered-regions.2026-09-12T10-25.md, the P6-T3 residual record, covering all five required regions plus a completeness statement |
| AC21 | P7-T21 | qa-gates/p3-t7-out-of-scope-untouched.2026-09-12T10-25.md, verdict: no hunk touches the increment, the `try` or the decrement; qa-gates/p6-t7-followup-link.2026-09-12T10-25.md, whose `LinkTarget:` resolves to an existing file; plus the explicit no-codification statement in the AC21 note below |
| AC22 | P7-T22 | qa-gates/p6-t6-scope-lock.2026-09-12T10-25.md, zero scope-lock failures and `FormatterRepairedPreExistingDrift: NONE` at its line 193; qa-gates/p5-t7-tests-final.2026-09-12T10-25.md, exit 0 at `total=1423 executed=1423 passed=1423 failed=0`. Normal branch taken; see the AC22 note below. |

RowCount: 22
RowsAbsent: NONE
RowsDuplicated: NONE

The table carries one body row per acceptance criterion, labelled AC1 through AC22 in ascending
order. Every criterion of the spec's `## Acceptance Criteria` section has a row, no criterion appears
twice, and the row labels form a contiguous run from AC1 to AC22 with no gap.

## Notes

### AC22 — which branch of P7-T22 was taken, and why

BranchTaken: normal. AC22 is checked off.

P7-T22 carries two branches and the escalation branch fires only when the
`FormatterRepairedPreExistingDrift:` line in the P6-T6 record is not `NONE`. That line reads `NONE`,
at line 193 of qa-gates/p6-t6-scope-lock.2026-09-12T10-25.md, where it is reproduced verbatim from
the P5-T1 artifact. The chain behind that value was checked rather than assumed: P0-T8 recorded
`PreExistingDriftFiles: NONE` and `DriftInsideWriteSet: NONE`, so the derivation rule P5-T1 applies
produces `NONE`, and P6-T6 records `AgreementCheck: AGREE`. Independently, the two porcelain captures
P5-T1 took immediately before and immediately after the repository-wide format are byte-identical, so
the formatter rewrote nothing at all, inside the Write Set or outside it.

The escalation branch is therefore not entered. No repository-wide format repaired pre-existing drift
in any file outside the Write Set, so the conflict between two repository rules that the escalation
branch exists to report does not arise, and nothing is waived. The consequence for P7-T24 is that its
admitted exception does not apply either: all 22 checkboxes read checked and the AC status summary
reports 22 of 22.

### AC21 — explicit statement that the counter-leak behaviour is not codified as correct

No test in either `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` or
`QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` asserts the running-jobs counter-leak
behaviour as correct.

The statement was checked against the two test sources at this task rather than inferred from the
plan text. The suite contains exactly one throw statement, `throw _itemGroupFailure;` at line 158 of
the harness part, raised by the recording item-group factory after it records its call. That factory
is invoked from inside the enqueue member's try block, so the finally decrement runs and the counter
returns to 0, which is the behaviour the two catch-path tests assert. No test makes
`BackgroundTlpFactory` throw: all three substitutions of it return a panel, at harness lines 114 and
132 and at test-class line 418. No test makes the move-monitor hook loop throw: the strict mock's
`HookItem` is set up to return normally at test-class line 243 and verified at line 252. Both of
those sites lie outside the try block, which is the property that makes a throw from either one
require the leak to be treated as expected, and neither is made to throw.

Every assertion on the counter asserts the correct post-condition rather than the leak: harness line
282 and test-class line 179 both assert `JobsRunning` is 0 after the call returns, and test-class
line 174 captures the mid-flight value to prove the increment took effect. This is corroborated by
the P4-T10 task text, which confines both catch-path throws to the substituted item-group factory
inside the try block for exactly this reason.

### AC19 — the two Cobertura documents, named by file name

The two raw Cobertura documents this item produced are:

- coverage-baseline.2026-09-12T10-25.cobertura.xml, written by P0-T12 into the baseline evidence
  directory.
- coverage-postchange.2026-09-12T10-25.cobertura.xml, written by P5-T5 into the qa-gates evidence
  directory.

Neither file is on disk now and neither was ever committed. Both were deleted after P6-T4 consumed
them, under the evidence-hygiene rule recorded under issue 671 that forbids committing a raw
`.cobertura.xml` document. The deletion and both file names are recorded at lines 135 and 139-140 of
qa-gates/p6-t4-repo-wide-projection.2026-09-12T10-25.md. Every figure either document supplied
survives in the Markdown artifacts that interpret them, which are the artifacts the AC19 row names
and which P7-T24 checks for on-disk existence: the baseline figures in
baseline/p0-t12-coverage-baseline.2026-09-12T10-25.md, the post-change figures in
qa-gates/p5-t5-coverage-postchange.2026-09-12T10-25.md, and the derived comparisons in the P6-T1,
P6-T2 and P6-T4 artifacts.

One stale sentence is recorded rather than corrected. Line 36 of
qa-gates/p5-t8-loop-result.2026-09-12T10-25.md states that the P5-T5 Cobertura document "sits
alongside its Markdown interpretation in the same directory". That was true when P5-T8 was written
and committed in the Phase 5 commit; the document was deleted afterwards. No task in Phase 7 is
authorised to edit a committed Phase 5 artifact, so the divergence is recorded here and in
qa-gates/p7-t24-reconciliation.2026-09-12T10-25.md instead of being edited away.

## Acceptance criteria status at this record

- Source: docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md
- Total AC items: 22
- Checked off (delivered): 22
- Remaining (unchecked): 0
- Items remaining: none

Output Summary: The completion record carries exactly 22 rows, one per acceptance criterion, none
absent and none duplicated. Fifteen criteria were already checked in the spec when Phase 7 began,
having been checked off by Phase 4 as its verification tasks passed; the seven that were not — AC2,
AC8, AC18, AC19, AC20, AC21 and AC22 — were each verified against the evidence on disk and checked
off by P7-T2, P7-T8 and P7-T18 through P7-T22. P7-T22 took its normal branch because the
`FormatterRepairedPreExistingDrift:` line reads `NONE`, so no criterion is escalated. Every evidence
artifact named above is a Markdown artifact under one of the three canonical evidence directories,
with the sole exception of the two raw Cobertura documents, which are named rather than cited and
whose absence is explained in the AC19 note. Acceptance met.
