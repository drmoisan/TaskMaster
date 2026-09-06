---
name: powershell-batch-budget-caps-plan-authored-helpers
description: A plan that mandates authoring .ps1 helper files is capped at 3 distinct non-test .ps1 paths by a PreToolUse hook; mandate ONE fixed gitignored helper path, because repeat writes to a recorded path are free and a gitignored path is invisible to porcelain scope gates
metadata:
  type: feedback
---

When a plan rule requires snippets to be authored into a `.ps1` file and invoked with `pwsh -File`, name **one fixed helper path** that is (a) reused and rewritten in place for every snippet and (b) inside a gitignored directory. In TaskMaster that path is `coverage/plan731-helpers.ps1` — `coverage/*` is gitignored at `.gitignore:144`. Never let the plan imply one helper per task.

**Why:** two independent mechanisms punish multiple helper paths, and both were verified against the live tree during #731 round 8.

1. `.claude/settings.json:144` registers `.claude/hooks/enforce-powershell-batch-budget.ps1` under the `"matcher": "Write|Edit"` PreToolUse block (`:128`). The hook's default production cap is **3** distinct non-test `.ps1` paths (`$ProdCap = 3` at `:316`); it **denies** the write creating a fourth (`:293`). A plan needing helpers at six or seven sites exhausts the cap mid-Phase-0 and the executor is hard-blocked. The three remediations the hook's own deny message offers — raise `CLAUDE_POWERSHELL_BUDGET_PROD`, delete the state file, split the batch — are governance bypasses a plan must not authorise.
2. A helper at a path `.gitignore` does not cover shows up in `git status --porcelain`, falls outside a Phase 5 scope gate's frozen exclusion union, and makes that gate **unsatisfiable in a state a restart loop cannot clear** — a restart re-runs the same capture against the same file.

**How to apply:**
- One fixed path, rewritten in place. Repeat writes to an already-recorded path are allowed **unconditionally** (`:289`), so the single path costs exactly one of the three slots forever.
- Put it under a gitignored directory so no porcelain/diff gate ever sees it.
- Provide a fallback for a spent cap: the agent session scratchpad lies outside the worktree root, so the hook's containment filter **discards** it without spending a slot (`:279-281`). Never write that absolute path into the plan or any evidence artifact — it contains a machine account name.
- State explicitly that the helper is not a deliverable: registered in no project file, asserted by no acceptance condition, present in no gate. Otherwise a reviewer reads it as an undeclared plan output.
- Spell the helper path with **forward slashes**. PowerShell accepts them, and a path with no backslash cannot be corrupted by the doubled-backslash collapse that the authoring rule exists to prevent in the first place.
- The extension filter is `.ps1|.psm1|.psd1` (`:273`); the test/prod split is `(^|/)tests/.*\.ps1$` or `\.Tests\.ps1$` (`:284`), each with its own cap of 3.

Related: [[project-731-r6-coverage-runner-bypass-seams]], [[gitignore-does-not-untrack-indexed-paths]], [[agent-memory-is-tracked-scope-git-gates]].
