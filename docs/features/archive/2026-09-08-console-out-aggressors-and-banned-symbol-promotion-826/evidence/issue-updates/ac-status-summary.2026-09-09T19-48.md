# Acceptance-criteria status summary (issue #826, [P8-T18])

Timestamp: 2026-09-09T19-48

PostedAs: unknown

This mirror is written for the orchestrator. Nothing is mirrored into `issue.md`: this feature must not
edit `issue.md`, and AC16 asserts its absence from the change footprint.

Work Mode is `full-bug`, so the sole authoritative acceptance-criteria source is
`docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/spec.md`
section `## Acceptance Criteria`. No `user-story.md` exists and none was created.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/spec.md`
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none

## Per-criterion detail

| AC | State | Plan task | Establishing artifact |
|---|---|---|---|
| AC1 — Console writer installs are gone | `- [x]` | P8-T2 | `evidence/qa-gates/p5-t10-sweep-verification.md` |
| AC2 — No residual writer references in the changed test files | `- [x]` | P8-T3 | `evidence/qa-gates/p5-t10-sweep-verification.md` |
| AC3 — CS0169/CS0414 hazard discharged by the type-check step | `- [x]` | P8-T4 | `evidence/qa-gates/p5-t6-treenode-sweep.md`, `evidence/qa-gates/p7-t3-nullable.md` |
| AC4 — Empty initializers deleted with their statement | `- [x]` | P8-T5 | `evidence/qa-gates/p5-t7-empty-initializers.md`, `evidence/qa-gates/p5-t8-obsolete-bayesian.md`, `evidence/qa-gates/p5-t9-comment-only-initializers.md` |
| AC5 — No console output remains in the table-access file | `- [x]` | P8-T6 | `evidence/qa-gates/p6-t1-ac5-ac6.md` |
| AC6 — Both diagnostics routed through the logger, confined to two statements | `- [x]` | P8-T7 | `evidence/qa-gates/p6-t1-ac5-ac6.md` |
| AC7 — Named regression test covers the branch and passes | `- [x]` | P8-T8 | `evidence/other/item2-branch-reachability.2026-09-09T19-11.md`, `evidence/regression-testing/p2-t2-regression-test.md`, `evidence/regression-testing/fail-before-exception.2026-09-09T19-12.md`, `evidence/qa-gates/p7-t4-tests-coverage.md` |
| AC8 — The project file gains exactly one line | `- [x]` | P8-T9 | `evidence/qa-gates/p6-t2-ac8-project-file.md` |
| AC9 — The eight DocID lines are present in the existing format | `- [x]` | P8-T10 | `evidence/qa-gates/p6-t3-ac9-ac12.md` |
| AC10 — DocIDs proven to resolve by positive observation with a control | `- [x]` | P8-T11 | `evidence/qa-gates/p4-t1-rs0030-channel.md`, `evidence/qa-gates/p4-t2-rs0030-observation.md` |
| AC11 — RS0030 severity unchanged, deliberately | `- [x]` | P8-T12 | `evidence/qa-gates/p3-t3-severity-unchanged.md`, `evidence/qa-gates/p4-t3-severity-restored.md` |
| AC12 — The two documented exclusions hold | `- [x]` | P8-T13 | `evidence/qa-gates/p6-t3-ac9-ac12.md` |
| AC13 — Tracking comment no longer points at closed work | `- [x]` | P8-T14 | `evidence/qa-gates/p3-t2-editorconfig-comment.md` |
| AC14 — Full toolchain pass, non-vacuous | `- [x]` | P8-T15 | `evidence/qa-gates/p7-t1-format.md`, `p7-t2-analyzers.md`, `p7-t3-nullable.md`, `p7-t4-tests-coverage.md`, `p7-t5-tests-ci-verbatim.md`, `p7-t7-toolchain-attestation.md` |
| AC15 — Coverage obligations met and recorded | `- [x]` | P8-T16 | `evidence/baseline/baseline-tests-coverage.md`, `evidence/qa-gates/p7-t6-coverage-delta.md` |
| AC16 — No out-of-scope file is touched | `- [x]` | P8-T17 | `evidence/qa-gates/p8-t1-ac16-write-set.md`, `evidence/qa-gates/p8-t20-committed-write-set.md` |

Every state above agrees with the check-off state of the corresponding bullet in `spec.md`: an
independent count of that file reports 16 lines matching `- [x] **AC` and 0 matching `- [ ] **AC`, and
the anchored diff of `spec.md` shows 16 added and 16 removed lines in which the only difference on each
line is the checkbox character. No criterion text was altered, none was added and none was deleted.

## Headline results behind the summary

- Item 1: repository-wide `Console.SetOut(` occurrences fell from 38 across 35 files to **2** across
  exactly `TaskMaster/ThisAddIn.cs` and
  `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs`.
- Item 2: both `Console.WriteLine` diagnostics now leave `GetTableInViewAsync` through the log4net
  `logger` at `Warn` level; the file's `Console.WriteLine` count is 0 and its `Console.` count is 1.
- Item 3: `BannedSymbols.txt` grew from 7 to 15 lines; RS0030 severity is unchanged at `suggestion`.
- Toolchain: all four steps passed in a single uninterrupted final pass with 0 restarts.
- Tests: 7192 of 7192 passed, 0 failed, in both the measured and the CI-verbatim step-4 forms.
- Coverage: root line coverage 86.1329 percent, up from 86.1154 percent, above the 85 percent floor.
  Both changed production lines carry a non-zero post-change hit count.

Output Summary: all 16 acceptance criteria are delivered, verified and checked off in `spec.md`. None
remains outstanding, and none was checked off on a caveat.
