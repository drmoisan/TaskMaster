---
name: project-644-ac16-referral-revision-seams
description: Issue #644 post-execution plan revision — converting a check-off task into a referral task when an AC's named instrument prints no figure and the substitute cannot resolve the comparison
metadata:
  type: project
---

Issue #644 (`qfc-unregister-navigation-count-mismatch-orphan`) halted at 56/58 tasks because AC-16
("repository coverage figure from the AC-15 step-4 run >= the AC-0 baseline") could not be satisfied
on its own terms. The plan was revised in place to stop demanding the check-off.

**Why:** two independent grounds, both measured. (1) The instrument the AC names —
`vstest.console.exe … /EnableCodeCoverage` — emits a binary `.coverage` file and prints no
percentage at all, so the plan's repository-wide Cobertura post-processing was a *substitute*
instrument, not the named one. (2) The substitute's run-to-run noise exceeded the delta it was asked
to adjudicate: baseline 54800/64221 (85.3303%), run E 54793/64221 (85.3194%), run F 54811/64221
(85.3475%) on a byte-identical tree with `git status --porcelain` unchanged between E and F. The two
runs straddle the baseline; measured noise ~0.028 points against a 0.01-point shortfall.
`lines-valid` invariant at 64221 across all three runs proves no production file changed instrumented
size, and the sole changed production file carries `[ExcludeFromCodeCoverage]` and appears in 0 of
558 `<class>` entries.

**How to apply.** Three reusable seams from this revision:

- **A named-instrument mismatch is a separate ground from a noise-floor argument, and stronger.**
  Before authoring a coverage AC, confirm the command the AC names actually *prints* the number the
  AC compares. See [[stale-build-output-is-not-evidence-of-existence]] and the contract's
  "observe a command's success-case output before asserting over that output" rule.
- **Converting a check-off task into a referral task must not become a no-op gate.** The replacement
  acceptance pins the *unchecked* state: `- [ ] **AC-16` count exactly 1 and `- [x] **AC-16` count
  exactly 0, both `-SimpleMatch`. Those clauses fail if any executor checks the AC off, which is the
  live failure mode. Same shift applied to the reconciliation task: 1 unchecked / 17 checked, not
  0 / 18. See [[acceptance-edits-must-be-false-before-true-after]].
- **Correct a stale evidence artifact forward, never rewrite it.** The `[P4-T6]` artifact's closing
  sentence still reads "AC-16 is checked off under this adjudication". The revision required the new
  referral artifact to state that sentence is superseded, and explicitly forbade editing the recorded
  run. Rewriting a recorded run to match a later decision destroys the audit trail.

**Selecting the favourable run is rejected as a basis.** Run F was the passing number; the
authorization rests on the measured indecidability, not on F's value. An executor free to choose the
run it is judged against cannot fail — the same defect class
`.claude/rules/plan-acceptance-gates.md` exists to report.

Override name recorded in `artifacts/orchestration/orchestrator-state.json`:
`p4_t6_comparison_clause_undecidable_at_measured_noise_floor`. Its own terms require AC-16 to be
surfaced to feature-review and forbid presenting it as a clean pass.
