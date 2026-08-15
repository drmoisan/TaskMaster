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
