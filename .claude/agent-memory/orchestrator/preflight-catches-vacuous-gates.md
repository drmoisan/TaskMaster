---
name: preflight-catches-vacuous-gates
description: Always route plans through atomic-executor preflight before execution; it reliably finds gates that pass vacuously (empty diffs, unsatisfiable acceptance, uncompilable assertions) that the MCP plan validator cannot see.
metadata:
  type: feedback
---

Never treat an `ok: true` from `mcp__drm-copilot__validate_orchestration_artifacts` as sufficient to begin execution. Run `atomic-executor` with `DIRECTIVE: PREFLIGHT VALIDATION ONLY` and iterate until `PREFLIGHT: ALL CLEAR`.

**Why:** the MCP validator checks structure only — phase headings, task ID sequencing, evidence paths. It cannot tell whether a task's binary outcome is *achievable*. On issue #503 two preflight rounds (one on the implementation plan, one on the remediation plan) caught six defects the validator passed clean, and every one would have produced a falsely-green execution:

- **Vacuous diff gates.** `HEAD` equalled the merge-base and the plan had no commit task, so every `git diff --numstat <MERGE_BASE>..HEAD` gate returned empty and "passed" while verifying nothing. Three separate acceptance criteria were affected, including the AC15 zero-line-diff check.
- **Unsatisfiable worktree gate.** A task required empty `git status --porcelain` at a point where earlier tasks in the same phase had just written evidence files.
- **Unsatisfiable build acceptance.** A task required "the solution builds" while forwarding to a method the *next* task creates — guaranteed CS1061.
- **Uncompilable assertion.** A planned `typeof(Microsoft.Office.Core.IRibbonControl)` in `TaskMaster.Test`, which carries no Office Core PIA `<Reference>`; legacy non-SDK `ProjectReference` does not flow it to `csc`. Fixed by comparing `ParameterType.FullName` as a string.
- **Contradictory acceptance.** An AC demanding "zero failed tests" against a plan that correctly allowed a known pre-existing flake, leaving the executor no honest branch.
- **Unscoped scope-lock gate.** A whole-branch diff necessarily contains every earlier-cycle path, so "no path outside the scope lock" could never hold. Scope it with `git show --numstat --format= HEAD` or a path-scoped `-- <paths>` form.

**How to apply:** give preflight the *measured* environment facts (tool paths, baseline exit codes, known-flaky tests) so it can check the plan against reality rather than reason abstractly — the best findings came from it actually running the commands. Preflight cannot fix the plan; re-delegate the deltas to `atomic-planner` in place at the same `plan-path`, re-run the MCP validator, then re-preflight. Two rounds is normal, not a smell.

**Converging the loop (verified 2026-08-10, #457 — 29 corrections over 3 iterations).** Three levers stopped it from running forever:

1. **Apply the non-blocking observations too, in the same round.** Deferring them just re-surfaces them as findings next iteration. Round 1 was 8 blocking + 8 non-blocking; applying all 16 at once meant round 2 found only genuinely new defects.
2. **Tell each iteration what the previous one found and fixed**, and instruct it to *re-measure* the load-bearing facts rather than accept the planner's restatement. Round 2 falsified two of the planner's own recorded figures (a "112 differing lines" formatter claim that was actually 5, and a restore branch that named one of three formatter-dirty files) — the planner had faithfully transcribed a measurement nobody re-ran.
3. **State a proportionality calibration in the final iterations**: "this plan has absorbed N corrections; withhold clearance only for defects that would cause the executor to fail, produce false evidence, or leave a spec AC undischarged — not for stylistic preferences." Without it, an adversarial preflight keeps finding wordsmithing. The clearing iteration returned five non-blocking observations and explicitly marked them as requiring no revision.

Also worth a sweep instruction: when a defect is a *class* (a bad quoting form, an AC checked off against `[expect-fail]`-only evidence), tell the planner to sweep the whole file for that class, not just the cited task. Both classes had additional unreported instances.


**An orchestrator-authored AC delta can be unsatisfiable in the OPPOSITE direction (verified 2026-08-25, epic child #444).** Preflight can only revise the plan; a defective acceptance criterion in `spec.md` is the orchestrator's to fix, and that fix is itself an unreviewed gate. Iteration 1 found AC-QA-01 asserting "no file changed or added by this feature exceeds 500 lines" against a 2,349-line pre-existing controller — impossible to PASS. I rewrote clause 2 as "no pre-existing file changed by this feature grows beyond its Phase 0 baseline", which iteration 2 correctly rejected: that forbids adding a single line to any pre-existing file, and five of the plan's six target files grow by construction. The working form is a DISJUNCTION — "either at or below 500 lines OR no larger than its Phase 0 baseline" — under which the growing files pass on the first disjunct and the oversized one on the second.

**How to apply:** before committing any AC text you author, name the concrete file that satisfies each clause and the concrete executor behaviour that would violate it. A criterion needs BOTH a reachable pass and a reachable fail; iteration 3 was explicitly asked to check the rewrite had not become vacuously true, and it verified three distinct failure paths. Hand the delta back through the next preflight iteration rather than treating your own edit as settled — three iterations here cost far less than an epic child executing against an AC it can never discharge.

Two corollaries seen on the same run: a spec fix must be PROPAGATED to the plan (correcting a non-canonical `evidence/coverage/` path in `spec.md` left the plan still ordering the executor to write an `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record for an override that no longer existed), and a gate rewritten by an earlier preflight iteration is a NEW gate no one has reviewed — iteration 3 found two of iteration 2's own restatements had gone stale against the amended spec.
