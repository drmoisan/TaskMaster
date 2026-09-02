---
name: preimplementation-gate-scope
description: Exact allow/deny scope of enforce-orchestration-preimplementation-gate.ps1 — parallel runs need no synthetic orchestrator-state.json, but .json/.js writes are blocked everywhere including the temp scratchpad
metadata:
  type: project
---

`.claude/hooks/enforce-orchestration-preimplementation-gate.ps1` is registered on three
PreToolUse matchers: `Bash`, `Write|Edit`, and `Agent`. Its scope is narrower and stranger
than its deny message suggests.

**Why:** The deny message ("requires artifacts/orchestration/orchestrator-state.json to
contain issue number, feature folder, route metadata, lifecycle readiness, and checkpoint
state") is misleading on a parallel run — it names a checkpoint the parallel surface does
not use and a "checkpoint state" key the hook never actually reads.

**How to apply:**

- **A parallel run needs NO synthetic `orchestrator-state.json`.** An `Agent(orchestrator)`
  prompt carrying `Parallel mode: true` (case-insensitive) is redirected to
  `artifacts/orchestration/parallel-orchestrator-state.json` and must satisfy six conjuncts:
  `route_id == 'parallel'`, non-empty `parallel_slug`, non-empty `parallel_manifest_path`,
  non-empty `items[]`, an `items[]` record matching the prompt's
  `docs/features/active/<basename>` token or issue number, and that record's `merge_status`
  not in `{merged, worktree_removed}`. A validly seeded parallel checkpoint clears it.
  See [[parallel-run-execution-playbook]].
- A prompt-declared `parallel_checkpoint_path:` must equal the canonical value exactly or it
  is a deny; it can never redirect the gate to a different file.
- **Write/Edit is extension-gated, not path-gated.** Blocked extensions:
  `py ps1 psm1 ts tsx js jsx cs json yml yaml`. Everything else — notably `.md` — is allowed
  anywhere. There is no repo-root confinement, so a `.js` or `.json` write into the temp
  scratchpad is blocked too. Exempt regardless of extension: any path under
  `docs/features/active/`, and the seven `artifacts/orchestration/*-state.json` checkpoints.
- **Bash blocks only five command shapes**, one of which is `git add|commit`. `msbuild`,
  `dotnet`, `csharpier`, `vstest`, `gh`, and `node` are not implementation and pass freely.
- **`git add`/`git commit` clears via an operand exemption** when every pathspec operand
  resolves under `docs/features/{epics,parallel,active,potential}/` or
  `artifacts/orchestration/`. Two traps: `git commit -m "..."` with **zero** pathspec
  operands is denied, so append `-- <path>`; and a compound `cd X && git add Y` is denied
  while the bare `git add Y` is allowed. Use `git -C <abs> add <relative-pathspec>` — the
  `-C` value is an option, not an operand, so it does not trip the absolute-path rejection.
- Only five subagent types are gated as implementation: `python-typed-engineer`,
  `powershell-typed-engineer`, `typescript-engineer`, `csharp-typed-engineer`,
  `atomic-executor`. Every other `subagent_type` passes without a checkpoint read.
- There is no environment-variable bypass. `Agent(orchestrator)` carrying BOTH
  `Preparation mode: true.` and `route_id: preparation.` (case-sensitive, trailing periods
  required) is the one true bypass and reads no checkpoint at all.
