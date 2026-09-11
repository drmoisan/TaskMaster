---
name: isolation-worktree-spawn-param-kills-toolchain
description: Spawning a child with isolation "worktree" hard-sandboxes it to a NEW auto-created worktree where pwsh is refused for scoped-tool agents, making the entire C# toolchain unreachable; the refusal is isolation-dependent, not agent-type dependent
metadata:
  type: feedback
---

Never pass `isolation: "worktree"` when delegating an item that already has its own checked-out worktree and needs
the build toolchain. The harness creates a DIFFERENT worktree and hard-sandboxes the session to it. Inside that
sandbox two things break at once:

1. Every `pwsh` invocation is refused for agents holding **scoped** Bash permissions (`orchestrator`,
   `atomic-executor`, `csharp-typed-engineer`). Since `msbuild` and `vstest.console.exe` are resolved through
   `vswhere` *inside* pwsh, and the repo-local .NET SDK bootstrap is itself a pwsh script, all four CLAUDE.md
   toolchain steps become unreachable. `dotnet` exits 155 with "The repo-local .NET SDK is missing."
2. Every git operation aimed at the originally-assigned worktree — by `-C` or by `cd` — is refused by the same
   isolation guard.

**Why:** verified across three sessions on issue #736 (2026-09-03/04). Two consecutive sessions blocked and
delivered nothing. The second one "recovered" by checking the item branch into the sandbox worktree with
`git checkout --ignore-other-worktrees`, which left the same branch checked out in two worktrees simultaneously
and had to be reconciled by hand. The third session was launched with **no** isolation parameter, and an
`Agent(atomic-executor)` probe then passed all six toolchain probes in the originally-assigned worktree.

**The misdiagnosis to not repeat.** The blocked session recorded the finding as *agent-type dependent*: it observed
that `Agent(general-purpose)` (unrestricted tools) could run pwsh while `Agent(atomic-executor)` could not, and
concluded scoped-tool agents can never run pwsh. That generalization is false. The real variable is the sandbox:
outside it, `atomic-executor` runs pwsh fine. Reasoning from "which agent type" instead of "am I sandboxed"
produced a `delegate_contract_incomplete` halt on a run that had no actual defect. Probe the *capability* in the
*target* worktree; do not infer it from the agent's tool-scope.

**How to apply:** delegate item work with no isolation parameter and pass the existing worktree path as an
absolute operand. Before committing to a long delegation, spend one cheap `Agent(atomic-executor)` probe on
`pwsh` + the SDK bootstrap + the vswhere lookup — it is ~1 minute and it is decisive, because this failure mode
otherwise surfaces only after the expensive delegation has already burned its budget. Related but distinct:
[[bash-tool-rejects-complex-commands-in-isolated-worktree]] is about static command-complexity refusal inside a
worktree, which happens whether or not you asked for isolation.
