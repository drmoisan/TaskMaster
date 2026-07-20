---
name: sibling-worktree-shared-tooling-hazard
description: A concurrent agent in a SIBLING worktree corrupts your test runs and temp logs via shared global tooling, even though source is isolated
metadata:
  type: project
---

Two agents in DIFFERENT worktrees (e.g. `.claude/worktrees/agent-XXXX` and
`TaskMaster-wt/<other-feature>`) share machine-global resources even though their source trees are
git-isolated. This is distinct from [[project_concurrent_executor_same_worktree]] (same-worktree
file corruption).

**Why:** Observed on #374 (dialogs-misc) while a sibling agent ran #365 (outlook-folder-store)
concurrently. Three failure modes, all environmental (not code defects):
1. **Shared `/tmp`** — MSYS `/tmp` maps to a machine-wide temp dir. Both agents naturally pick the
   same log filenames (e.g. `/tmp/baseline-tests.log`), so one clobbers the other; you may read the
   OTHER worktree's output and see its paths/assemblies. Symptom: your log references a foreign
   worktree path.
2. **Shared vstest/dotnet-coverage/testhost** — `Invoke-MSTestWithCoverage.ps1` uses global exes.
   Under concurrent 24-worker (`<Workers>0</Workers>` = auto) coverage runs from both agents, the
   testhost crashes mid-suite ("Test host process crashed", exit -1/1/127, partial pass counts like
   522/927/5701 with 0 failures). Nondeterministic.

**How to apply:**
- Write ALL temp logs to the session scratchpad dir (from the system reminder), NEVER `/tmp`.
- Before every coverage/test run, poll for a quiet machine and only run when zero
  `vstest|dotnet-coverage|testhost` processes exist; retry the whole run if it crashes with 0 test
  failures. A poll-loop (up to ~40×15s) then run, wrapped in an up-to-5-cycle retry, reliably
  catches a quiet window; a clean run is Total 5702 / Passed 5702 / 0 failed.
- Trust the bash `$?` exit code (yours), not log-derived counts, when logs may be clobbered. Build
  gate conclusions on the scoped-build EXIT code (0 = zero CS86xx), which is immune to log
  contamination.
- Do NOT kill testhost/vstest/dotnet-coverage processes — you cannot tell yours from the sibling
  agent's, and killing theirs sabotages them. Just wait.
- Never stage the sibling/orchestrator's `.claude/agent-memory/orchestrator/*` files (they change
  under you during the run); leave them untracked/modified and commit only your feature paths.
