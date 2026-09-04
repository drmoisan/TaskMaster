---
name: worktree-isolation-blocks-pwsh-not-agent-type
description: An isolation-sandboxed session refuses pwsh for scoped-tool agents, making the C# toolchain unreachable; the discriminator is the SANDBOX, not the agent type, and reading it as agent-type-dependent causes a false blocked halt
metadata:
  type: project
---

If the harness hard-isolates your session to a worktree other than the item's own, the isolation guard
refuses **every** `pwsh` invocation from scoped-tool agents — `-Command` and `-File`, single- or
double-quoted, with or without a `$`. The refusal text is `... this command runs pwsh in a plain command;
what it reads or is handed as shell text cannot be shown not to run git.` It also refuses
`git -C <other-worktree>` and `cd <other-worktree> && git ...`.

**Corrected 2026-09-04. The discriminator is the sandbox, not the agent type.** An earlier revision of
this memory claimed the block was agent-type dependent, on this evidence, all gathered on 2026-09-03
inside a session that had been spawned with an `isolation: "worktree"` parameter:

| Agent | pwsh | git |
|---|---|---|
| `general-purpose` (`Tools: *`) | OK | OK |
| `atomic-executor` (scoped `Bash(pwsh *)`) | REFUSED | OK |
| `orchestrator` (scoped Bash) | REFUSED | OK |

Those observations were real, but the inference from them was wrong, because every row shares an
uncontrolled confound: all three ran inside the sandbox. On 2026-09-04 the same item was re-run from a
session spawned with **no** isolation parameter, and `Agent(atomic-executor)` — the same scoped-tool
agent type the table marks REFUSED — passed all six toolchain probes in the item's own worktree: `pwsh`
ran, the repo-local SDK bootstrapped to 8.0.205, and vswhere resolved MSBuild and vstest.console. The
whole 77-task plan then executed.

The cost of the wrong inference was a full session halted as `delegate_contract_incomplete` on a run
that had no defect in it. Read the table as "sandboxed sessions refuse pwsh to scoped-tool agents", not
as a statement about agent types.

**Why it is fatal when it does apply.** Inside the sandbox there is no non-pwsh fallback:

- `msbuild` — `command not found` from bash (it is resolved through vswhere inside pwsh).
- `vstest.console.exe` — `command not found` from bash.
- `dotnet` — on PATH, but every subcommand exits 155 with `The repo-local .NET SDK is missing. Run
  ./scripts/vscode/Install-RepoDotNetSdk.ps1`, and that bootstrap is itself a pwsh script.

So all four CLAUDE.md toolchain steps are unreachable and `atomic-executor` cannot complete S5. Under
the orchestrate contract a mandatory delegated step that cannot be completed means stop and record
blocked state — do not substitute `general-purpose` (it is not a configured specialist) and do not run
the toolchain yourself (the orchestrator is orchestration-only).

**How to apply.** The prevention is upstream: never spawn the child with `isolation: "worktree"` when
the item already has its own worktree — see [[isolation-worktree-spawn-param-kills-toolchain]]. When you
are the child and are handed a worktree path, check whether the harness actually placed you there
(`git worktree list` plus your own `pwd`). If you are isolated somewhere else, probe `pwsh` with the
agent type that will execute before spending planner cycles, and report the isolation mismatch as the
blocking condition rather than concluding anything about agent types. Do not "recover" by checking the
item branch into the sandbox worktree with `--ignore-other-worktrees`: that leaves one branch checked
out in two worktrees and has to be reconciled by hand.

Related: [[pwsh-double-quoted-command-refused-in-worktree]] records a narrower guard where the
discriminator was `$` complexity; [[bash-tool-rejects-complex-commands-in-isolated-worktree]] and
[[subagent-self-reported-correction-can-be-false]] are the same family.
