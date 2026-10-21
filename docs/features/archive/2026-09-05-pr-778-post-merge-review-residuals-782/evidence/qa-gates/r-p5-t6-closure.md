# [P5-T6] Remediation closure — issue #782, findings R3 and R4

Timestamp: 2026-09-06T02-01

Command:

```text
No command is run by this task. It records the state of the remediation as a whole, reading each
task's own artifact.
```

EXIT_CODE: 0

Output Summary: all 53 tasks of `remediation-plan.2026-09-06T00-15.md` executed in order. The first
commit is recorded below. Three tasks are marked `PENDING AT WRITE TIME` for the reason stated
beneath the table.

## The [P5-T4] commit

- **SHA:** `b91dd859b85434ac66c2ae817d7daebf3b0d3342`
- **Subject:** `fix(782): correct the message-pinning claim and the baseline coverage input record`
- **Files changed:** 38, with 2633 insertions and 38 deletions.
- Both required trailers are present:
  `Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>` and
  `Claude-Session: https://claude.ai/code/session_011ucgeqsVLVSVbmJfkDzcBs`.

## Task table

All artifact paths are relative to
`docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/`.

| Task | Artifact | State |
|---|---|---|
| [P0-T1] | `evidence/remediation-baseline/r-p0-t1-instructions-read.md` | PASS |
| [P0-T2] | `evidence/remediation-baseline/r-p0-t2-claim-inventory.md` | PASS |
| [P0-T3] | `evidence/remediation-baseline/r-p0-t3-assertion-sites.md` | PASS |
| [P0-T4] | `evidence/remediation-baseline/r-p0-t4-pre782-message.md` | PASS |
| [P0-T5] | `evidence/remediation-baseline/r-p0-t5-retained-cobertura-reaggregation.md` | PASS |
| [P0-T6] | `evidence/remediation-baseline/r-p0-t6-retained-document-provenance.md` | PASS |
| [P0-T7] | `evidence/remediation-baseline/r-p0-t7-csharpier-check.md` | PASS |
| [P0-T8] | `evidence/remediation-baseline/r-p0-t8-analyzer-build.md` | PASS |
| [P0-T9] | `evidence/remediation-baseline/r-p0-t9-nullable-build.md` | PASS |
| [P0-T10] | `evidence/remediation-baseline/r-p0-t10-tests-coverage.md` | PASS |
| [P0-T11] | `evidence/remediation-baseline/r-p0-t11-anchor.md` | PASS |
| [P0-T12] | `evidence/remediation-baseline/r-p0-t12-dotclaude-baseline.md` | PASS |
| [P1-T1] | source edit, verified in `evidence/qa-gates/r-p1-t10-assertion-token-gate.md` | PASS |
| [P1-T2] | source edit, verified in `evidence/qa-gates/r-p1-t10-assertion-token-gate.md` | PASS |
| [P1-T3] | `evidence/qa-gates/r-p1-t3-analyzer-build.md` | PASS |
| [P1-T4] | `evidence/qa-gates/r-p1-t4-assertion-tests.md` | PASS |
| [P1-T5] | `evidence/regression-testing/r-p1-t5-mutation-applied.md` | PASS |
| [P1-T6] | `evidence/regression-testing/r-p1-t6-mutation-build.md` | PASS |
| [P1-T7] | `evidence/regression-testing/r-p1-t7-fail-before.md` | PASS (expect-fail; EXIT_CODE 1 equals `ExpectedExitCode: 1`) |
| [P1-T8] | `evidence/regression-testing/r-p1-t8-mutation-reverted.md` | PASS |
| [P1-T9] | `evidence/regression-testing/r-p1-t9-pass-after.md` | PASS |
| [P1-T10] | `evidence/qa-gates/r-p1-t10-assertion-token-gate.md` | PASS |
| [P2-T1] | `spec.md` AC10, gated by `evidence/qa-gates/r-p2-t4-spec-claim-gate.md` | PASS |
| [P2-T2] | `spec.md` AC11, gated by `evidence/qa-gates/r-p2-t8-spec-wildcard-gate.md` | PASS |
| [P2-T3] | `spec.md` Behavioral Contract bullet, gated by `evidence/qa-gates/r-p2-t4-spec-claim-gate.md` | PASS |
| [P2-T4] | `evidence/qa-gates/r-p2-t4-spec-claim-gate.md` | PASS |
| [P2-T5] | `evidence/other/code-review.2026-09-05T23-00.md` entry (b) | PASS |
| [P2-T6] | `evidence/other/ac-status-summary.2026-09-05T23-15.md` AC10 row | PASS |
| [P2-T7] | `evidence/other/ac-status-summary.2026-09-05T23-15.md` AC11 row | PASS |
| [P2-T8] | `evidence/qa-gates/r-p2-t8-spec-wildcard-gate.md` | PASS |
| [P3-T1] | `evidence/baseline/p0-t7-coverage.md` amendment header | PASS |
| [P3-T2] | `evidence/baseline/p0-t7-coverage.md` input-document note | PASS |
| [P3-T3] | `evidence/baseline/p0-t7-coverage.md` two-collections section | PASS |
| [P3-T4] | `evidence/baseline/p0-t7-coverage.md` orphaned-base statement | PASS |
| [P3-T5] | `evidence/baseline/p0-t7-coverage.md` test-run section | PASS |
| [P3-T6] | `evidence/baseline/p0-t7-coverage.md` reproduction section | PASS |
| [P3-T7] | `evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md` | PASS |
| [P4-T1] | `evidence/qa-gates/r-p4-t1-format.md` | PASS |
| [P4-T2] | `evidence/qa-gates/r-p4-t2-format-check.md` | PASS |
| [P4-T3] | `evidence/qa-gates/r-p4-t3-analyzer-build.md` | PASS |
| [P4-T4] | `evidence/qa-gates/r-p4-t4-nullable-build.md` | PASS |
| [P4-T5] | `evidence/qa-gates/r-p4-t5-tests-coverage.md` | PASS |
| [P4-T6] | `evidence/qa-gates/r-p4-t6-coverage-comparison.md` | PASS |
| [P4-T7] | `evidence/qa-gates/r-p4-t7-loop-closure.md` | PASS |
| [P5-T1] | `evidence/qa-gates/r-p5-t1-dotclaude-untouched.md` | PASS |
| [P5-T2] | staging, recorded in `evidence/qa-gates/r-p5-t3-staged-set.md` | PASS |
| [P5-T3] | `evidence/qa-gates/r-p5-t3-staged-set.md` | PASS |
| [P5-T4] | commit `b91dd859` | PASS |
| [P5-T5] | `evidence/qa-gates/r-p5-t5-post-commit-verification.md` | PASS |
| [P5-T6] | `evidence/qa-gates/r-p5-t6-closure.md` (this file) | PASS |
| [P5-T7] | second commit of the three post-commit artifacts | PENDING AT WRITE TIME |
| [P5-T8] | no artifact by design; reported in the executor's return | PENDING AT WRITE TIME |
| [P5-T9] | plan-completion commit | PENDING AT WRITE TIME |

**[P5-T7], [P5-T8], and [P5-T9] have not yet run when this artifact is written**, because this
artifact is one of the three files [P5-T7] commits. Their rows therefore record
`PENDING AT WRITE TIME` rather than a pass or fail state. Every other row records a state.

## The R3 decision and its reasoning

`spec.md` AC10 and `evidence/other/code-review.2026-09-05T23-00.md` entry (b) claimed the removal of
the `WpfDispatcherYield` message tail was pinned by the C20 `WithMessage` assertion. Both assertions
were the wildcard `"*UiThread.Init()*"`, and the pre-782 message likewise contains `UiThread.Init()`,
so the wildcard matched both messages and the claim was false as written.

Of the two available options — make the assertion exact, or shrink the claim — this remediation took
the first: **make the acceptance criterion true rather than smaller.** Both assertions now read
`WithMessage(UiThread.DispatcherNotInitializedMessage)`, which FluentAssertions compares against the
entire message because the constant's value contains neither of its two wildcard characters. The cost
was two assertion lines and no production change.

The corrected prose states exactly what that form establishes and no more:

- a caller-specific tail appended at the `WpfDispatcherYield` throw site fails the assertion in
  `WpfDispatcherYieldTests.cs`, and one appended at the `UiThread.Dispatcher` throw site fails the
  assertion in `UiThread_Tests.cs`;
- neither assertion detects an edit to the constant's own wording, because an assertion written
  against the constant moves with the constant;
- the one part of that wording a test does hold is the substring `UiThread.Init()`, asserted at
  `WpfDispatcherYieldTests.cs:196`.

The claim is observed, not derived. [P1-T7] appended the removed tail at the `WpfDispatcherYield`
throw site and recorded `YieldAsync_WithoutDispatcher_RemainsStrict` failing with the sibling test
still passing; [P1-T9] recorded both passing once the mutation was reverted.

## The R4 decision and its reasoning

`evidence/baseline/p0-t7-coverage.md` recorded the re-measured first-party figures 112355 and 26500
while naming `coverage\782-p0-baseline.cobertura.xml` as its `--output`. Re-aggregating that document
yields 112359 and 26496 — the two figures the artifact itself labels superseded.

The remediation took the combined option: **record both collections with their own inputs and
figures, keep the re-measured figures authoritative on substance, state that the authoritative
collection's output document is not retained, and supply a reproduction procedure.** The reasoning:

- the re-measured figures were taken at the re-anchored base `736c2cf2`, this branch's actual base,
  so they are correct on substance; promoting the retained document's figures would resurrect a
  measurement of an orphaned tree and would contradict
  `evidence/qa-gates/p7-t7-changed-line-coverage.md`, fixing one inconsistency by creating another;
- re-running the baseline collection was rejected: it would require restoring six files to
  `pre-782-base` content in the delivered worktree and would yield a third measurement rather than a
  confirmation of the second;
- the reconciling observation is that the retained document is the earlier collection's output. Its
  companion log records `Total tests: 6992`, the superseded-base count, against the `6997` the
  re-anchored run recorded. That discriminator is independent of file timestamps.

## R1 and R2 dispositions

R1 is **accepted with no remediation** and R2 is **waived**, following the reviewer's own
recommendations. **No file was changed for either item.** The full record, including the reviewer's
stated qualification that the "would force a FAIL verdict" rationale for SD1 is not a legitimate
reason to omit the artifact, is at
`evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md`.

## Confirmations

- **No production `.cs` file was changed.** The anchored diff in [P5-T5] lists exactly
  `UtilitiesCS.Test/Threading/UiThread_Tests.cs` and
  `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, both test files.
  `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` was mutated temporarily by [P1-T5] and
  reverted by [P1-T8]; it is absent from the diff.
- **No file under `.claude/` was changed**, including agent memory. [P0-T12] and [P5-T1] each record
  zero lines from both a porcelain status and an anchored diff over that path.
- **No file under `artifacts/orchestration/` was changed or staged.**
  `artifacts/orchestration/orchestrator-state.json` returned zero matches over the staged set in
  [P5-T3].
- **Neither the historical plan nor any reviewer artifact was changed.**
  Specifically `plan.2026-09-05T15-47.md`, `user-story.md`, `policy-audit.2026-09-05T23-48.md`,
  `code-review.2026-09-05T23-48.md`, `feature-audit.2026-09-05T23-48.md`,
  `remediation-inputs.2026-09-05T23-48.md`, `evidence/qa-gates/p1-t9-phase1-tests.md`,
  `evidence/baseline/p0-t6-vstest.md`, and `issue.md` are all absent from the [P5-T3] staged set and
  from the [P5-T4] commit.
- **The `pre-782-base` tag is unmoved** at `736c2cf234cdd71b604c908f348b6aa89b256b53`.
