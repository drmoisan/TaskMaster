# Acceptance Criteria Status Summary — issue #798

Timestamp: 2026-09-07T05-56
Task: [P9-T15]
Work Mode: full-bug
AC source: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`

Under the acceptance-criteria-tracking skill, `full-bug` resolves to spec.md as the sole authoritative
acceptance-criteria source. spec.md carries 14 criteria, AC1 through AC14.

## Status of all 14 criteria

| AC | Status | Evidence artifact |
|---|---|---|
| AC1 | MET | `evidence/regression-testing/p2-ac1-nonoverlap-fail-before.md`, `evidence/regression-testing/p2-ac1-loud-failure-fail-before.md`, `evidence/regression-testing/p3-ac1-ac2-pass-after.md` |
| AC2 | MET | `evidence/regression-testing/p2-ac2-timing-fail-before.md`, `evidence/regression-testing/p3-ac1-ac2-pass-after.md` |
| AC3 | MET | `evidence/regression-testing/p2-ac3-negative-fail-before.md`, `evidence/regression-testing/p2-ac3-message-fail-before.md`, `evidence/regression-testing/p4-ac3-pass-after.md` |
| AC4 | MET | `evidence/regression-testing/p2-ac4-fail-before.md`, `evidence/regression-testing/p5-ac4-pass-after.md` |
| AC5 | MET | `evidence/regression-testing/p2-ac5-boundary-fail-before.md`, `evidence/regression-testing/p2-ac5-shape-fail-before.md`, `evidence/regression-testing/p6-ac5-pass-after.md` |
| AC6 | **PENDING MANUAL** | `evidence/other/ac6-manual-verification-handoff.md` |
| AC7 | MET | `evidence/regression-testing/p3-ac1-ac2-pass-after.md` |
| AC8 | MET | `evidence/qa-gates/p3-banned-symbol-and-overload-scope.md`, `evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md`, `evidence/qa-gates/final-msbuild-analyzers.md` |
| AC9 | MET | `evidence/qa-gates/p7-ac9-fixed-arity.md`, `evidence/regression-testing/p4-ac3-pass-after.md` |
| AC10 | MET | `evidence/qa-gates/p5-ac10-out-of-scope-throws.md` |
| AC11 | MET | `evidence/qa-gates/p6-ac11-handler-inventory.md`, `evidence/regression-testing/p6-ac5-pass-after.md` |
| AC12 | MET | `evidence/qa-gates/p7-ac12-inverse-constraints.md` |
| AC13 | MET | `evidence/qa-gates/p7-ac13-write-set-diff.md`, `evidence/qa-gates/p7-ac13-compile-entries.md`, `evidence/qa-gates/p7-ac13-line-cap.md`, `evidence/baseline/line-cap-preexisting.md`, `evidence/other/followup-promotions.md` |
| AC14 | MET | `evidence/qa-gates/final-toolchain-clean-pass.md`, `evidence/baseline/log4net-capture-probe.md`, `evidence/qa-gates/coverage-delta.md` |

## Summary counts

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md`
- Total AC items: 14
- Checked off (delivered): 13
- Remaining (unchecked): 1
- Items remaining: AC6 — "Launching QuickFiler on the "T&E" folder either succeeds or shows the AC1/AC3 error message (manual verification)."

## AC6 — why it remains unchecked

AC6 requires a live Outlook session against a specific mailbox folder. No automated test in this
repository can supply that: the repository's unit-test policy prohibits dependence on external
processes, and the column-add path this change modifies is reached only through
`Microsoft.Office.Interop.Outlook` against a real `MAPIFolder`.

Its status is recorded here and in `evidence/other/ac6-manual-verification-handoff.md`, and **not**
as a note beside the criterion in spec.md. spec.md's Authority blockquote states that AC1 through AC6
must not be renumbered, reworded, merged, split or weakened, and the acceptance-criteria-tracking
skill permits exactly one edit to a criterion line: changing `- [ ]` to `- [x]`. The AC6 line was
verified byte-identical to its authored text.

The handoff artifact specifies three manual steps with explicit pass and fail conditions: launch on
the "T&E" reproduction folder, launch on Inbox as a regression check, and confirm the per-step
`[Df timing]` column-add lines in the debug log in both cases.

## AC13 — status at the time this summary was written

AC13's substantive conditions were all verified before this summary was written: the write-set diff
resolves to exactly 16 paths, all six `<Compile Include>` entries are present one per new source
file, every write-set `.cs` file is at or below 500 lines except
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, and that file stands at 869, strictly below its
base-commit value of 882 as AC13 requires for a file already over the cap at base.

AC13's final clause additionally requires the pre-existing 500-line-cap violation to be recorded as a
follow-up. That record is `evidence/other/followup-promotions.md`, written by P9-T16.

## Confirmation appended after P9-T16

*(This section was a placeholder when the summary was first written. P9-T16 had not yet run, so no
observation about its artifact could be recorded. The observation follows.)*

Timestamp: 2026-09-07T05-59

Observed after P9-T16 completed: `evidence/other/followup-promotions.md` exists and records three
findings. Its third entry is the pre-existing 500-line-cap violation in
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, carrying the observed post-change line count of
869 against a base-commit value of 882 and a cap of 500. That entry is the record AC13's final clause
requires.

AC13's final conjunct is therefore discharged, and the AC13 checkbox in spec.md was set to `[x]` at
that point. The AC13 row in the table above is confirmed MET.

The same artifact records that the potential-to-issue promotion route was **not** exercised in this
session, because creating a promotion file on this branch would add a seventeenth path and falsify
the AC13 write-set gate. All three findings are to be promoted after this branch merges.

Output Summary: 14 acceptance criteria total. 13 are MET and checked off in spec.md. 1 remains
unchecked: AC6, which is PENDING MANUAL because it requires live-Outlook verification that no
automated test can supply. Every criterion names its evidence artifact.
