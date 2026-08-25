---
name: project-468-preflight-revision-seams
description: "#468 QfcCollectionController preflight R1: red-test-before-seam inverts full-suite gates; [expect-fail] belongs on run tasks; epic children merge-base against the integration branch; pre-commit diff gates need the working-tree form + non-zero count"
metadata:
  type: project
---

Four seams from the #468 (qfc-collection-controller-defects) preflight revision pass, all generalizable:

1. **Red-test-before-seam inversion.** If a deliberately-red fail-before test file gets a `Compile Include` before the behaviour-preserving seam lands, every full-suite "failed count of exactly 0" gate between the include and the fix is unsatisfiable (the red test is compiled into the assembly). Order every seam phase: seam edit -> seam full-suite (passed count identical to prior phase) -> seam-only commit -> red test file -> csproj include -> `[expect-fail]` scoped run. Phases 11/13 of the #468 plan had this right; Phase 10 was the lone inversion.
   **Why:** blocked finding B2 — P10-T5 could never pass with the red STA test compiled in from P10-T2 onward.
   **How to apply:** when a phase mixes a seam and a fail-before test, sweep for any full-suite `failed == 0` gate that runs while the red test is compiled and unfixed.

2. **`[expect-fail]` tags the run, not the file creation.** A file-creation task with ordinary pass acceptance (file exists, under 500 lines, one `[TestMethod]`) cannot fail and must not carry `[expect-fail]`. Tag only the paired run task (artifact + `ExpectedExitCode: 1`), and have the creation task say "the failing run and its evidence artifact are the paired [expect-fail] run task P#-T#".

3. **Epic-child `<MERGE_BASE>` resolves against the integration branch.** `git merge-base HEAD origin/main` is latently wrong for a child of an epic integration branch — it agrees with `origin/epic/<slug>-integration` only until a sibling merges first. Resolve against the integration branch; record the origin/main value for reference and note whether they agree. See [[diff-gates-need-a-commit-task]].

4. **Pre-commit diff gates use the working-tree form.** A `git diff <MERGE_BASE>..HEAD -- <file>` gate placed before the phase's commit task is vacuously empty. Use `git diff <MERGE_BASE> -- <file>` (no `..HEAD`) and require the artifact to record a non-zero total changed-line count so an empty diff cannot pass.

Also confirmed in this pass: deferred orchestrator-owned ACs (PR body, closing merge, promotions) get DEFERRED-TO-ORCHESTRATOR rows in the reconciliation artifact with checkboxes left unchecked — see [[terminal-phase-planner-traps]]; committed TRX hygiene (LogFileName= + path/computerName scrubbing) per the shared no-absolute-host-paths rule.
