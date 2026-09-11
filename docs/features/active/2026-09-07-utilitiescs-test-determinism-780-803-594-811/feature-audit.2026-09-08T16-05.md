# Feature Audit (Remediation Cycle 1 Exit Reaudit) — utilitiescs-test-determinism (Issue #811)

- Date: 2026-09-08T16-05
- Branch: `bug/utilitiescs-test-determinism-780-803-594-811`
- Head: `41005c6ba889fd645b1aa4058ba43168a6ff12e4`
- Base: `origin/main` @ `e6fc0e79be93e72bb5007fcd4f4314675470b073`
- Work mode: `full-bug` → **`spec.md` is the sole acceptance-criteria source**; no `user-story.md`
  exists and none is required.
- Cycle-entry audit: `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/feature-audit.2026-09-08T11-30.md`

This is a reaudit. Per-criterion implementation evidence established at cycle entry is referenced
rather than restated. The acceptance-criteria states below were read directly from `spec.md`
§ `## Acceptance Criteria` (lines 291-296) in this session, not taken from the delegation prompt.

## R-1 Disposition

**CLOSED.**

The cycle-entry blocking finding required a durable follow-up artifact for the `ILGlobals`
unsynchronised-static race, on the stated ground that the defect "currently exists only as prose
inside `<FEATURE>/evidence/`, which is archived at merge."

`docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md` (111 lines)
now exists on the branch and carries every required content element. All twelve required elements
were re-verified against the post-change source in this session; the verification table is in
`policy-audit.2026-09-08T16-05.md` § F-1. A repository search for `ILGlobals`, `SDIL`, and
`MethodBodyReader` under `docs/features/potential/` returned zero matches at cycle entry and returns
exactly one file now.

The GitHub-issue half of R-1's acceptance was deliberately withheld and is accepted as non-gating.
The full adjudication is in `policy-audit.2026-09-08T16-05.md` § F-1a. In short: the blocking
rationale concerned durability past merge, which a tracked `docs/features/potential/` entry fully
satisfies; issue filing is an operator action that this agent is instructed not to perform and that
the promotion route gates; and the unchecked promotion checkbox is the repository's prevailing state
for such entries, including ones already promoted, so it cannot bear a merge gate.

## Acceptance Criteria Evaluation

Source: `spec.md` § `## Acceptance Criteria`, read directly at head `41005c6b`.

| AC | Criterion (abbreviated) | State in `spec.md` | Verdict | Evidence |
|---|---|---|---|---|
| AC1 | `TryAddValuesAsync` no longer cancels on a fixed wall-clock window; test passes deterministically under 24-worker parallel coverage | `[x]` | **PASS** | `DictionaryExtensions.cs` linked source and `CancelAfter(500)` deleted; `evidence/regression-testing/p4-t4-ac1-pass-after.md`; `evidence/other/p4-t5-ac1-checkoff-note.md` |
| AC2 | `DfDeedle_COM_Tests` no longer mutates observable process-wide static seams; `DfDeedle.cs:186` guards the null snapshot with a descriptive failure | `[x]` | **PASS** | `TableEtlInvoker`/`StoreTableEtlInvoker` replaced by optional delegate parameters; null guard added ahead of `LogDfTiming`; RED-first proof at `evidence/regression-testing/p2-t4-ac2-fail-before.md` and `p3-t2-ac2-pass-after.md` |
| AC3 | The `Console.Out` races are removed by eliminating the shared-console dependency | `[x]` | **PASS** | `TextWriter` seam added to the four production members; four victim tests converted to their own `StringWriter`; `[DoNotParallelize]` removed; `NLogTraceWriter_Test` save/restore removed; `evidence/regression-testing/p5-t10-ac3-pass-after.md` |
| AC4 | Ten consecutive full nine-assembly `/InIsolation` runs with zero failures, recorded as evidence | `[ ]` | **NOT MET — correctly unchecked** | `evidence/regression-testing/p8-t6-ac4-ten-run.md`: 9 of 10 runs clean. Run 7 failed on `GetBodyCode_ReturnsConcatenatedInstructions` via the `ILGlobals` race, which is a defect outside this item's fix scope and is now documented for separate work |
| AC5 | No test is stabilized by a sleep, a retry, or a timing tolerance | `[x]` | **PASS** | Explicit diff search recorded at `evidence/qa-gates/p7-t10-ac5-timing-hack-search.md`; the pre-existing `Returns(120)` tolerance at `OlTableExtensions_Tests.cs:960-963` retired and replaced by the fake clock |

**AC4 is confirmed still unchecked in `spec.md` at line 295.** It was not checked off by this
reviewer and must not be, since its literal condition (ten of ten clean) is unmet. AC1, AC2, AC3,
and AC5 are confirmed checked at lines 292, 293, 294, and 296 respectively. This matches the state
the delegation prompt described, verified independently rather than assumed.

### On AC4's disposition

AC4 is the only unmet criterion, and it is unmet for a reason that is documented, understood, and
attributable to a defect this change did not introduce and deliberately did not fix. Three
properties of how it was handled are worth recording, because each is a place where the outcome
could have been quietly manipulated:

1. The failing run was **not** re-run to green and re-recorded. All ten runs are reported.
2. The criterion was **not** checked off with a footnote explaining the exception.
3. The underlying race was **not** opportunistically fixed on this branch, which would have
   breached the CLAUDE.md Bugfix Workflow minimal-fix requirement and shipped an unproven
   concurrency change with no RED-first test.

The handling is correct. AC4 should be re-evaluated after the `ILGlobals` race is fixed under its
own issue.

## Baseline Comparison

The feature's stated objective is to stop the required `mstest-coverage` check failing on unrelated
pull requests, by removing three specific nondeterminism sources. Against baseline:

- The `TaskCanceledException` source (#780) is removed at the root: the 500 ms window is deleted,
  not widened.
- The `NullReferenceException` source (#803 / #594 item 1) is removed and the production diagnostic
  is improved: an unattributed `NullReferenceException` becomes an `InvalidOperationException`
  naming the folder.
- The `Console.Out` races (#594 items 2-3) are eliminated at the seam rather than suppressed by
  `[DoNotParallelize]`, and the stopgap attribute is removed, so the gate now exercises the seam.

A fourth, previously unattributed source (`ILGlobals`) was discovered by the ten-run gate itself.
That is the gate working as designed. It is out of scope for this item and is now durably recorded.

## Blocking Findings

**0.**

## Verdict

**PASS.** Remediation cycle 1 exit criteria are satisfied. R-1 is CLOSED, no new blocking finding
exists, and the acceptance-criteria states in `spec.md` are correct as recorded.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/spec.md`
- Total AC items: 5
- Checked off (delivered): 4
- Remaining (unchecked): 1
- Items remaining: AC4 — "A full nine-assembly `/InIsolation` run with `TestCategory!=LiveOutlook`
  reports zero failures on ten consecutive runs, recorded as evidence." Correctly left unchecked:
  9 of 10 runs clean, the single failure attributable to the `ILGlobals` race now documented at
  `docs/features/potential/2026-09-08-ilglobals-loadopcodes-unsynchronised-static-race.md` for
  separate resolution.
