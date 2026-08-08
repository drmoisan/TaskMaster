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
