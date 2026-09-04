---
name: worktree-isolation-blocks-pwsh-per-agent-type
description: When the harness isolates a session to a worktree other than the item's own, the isolation guard refuses EVERY pwsh form for scoped-tool agents, which makes the whole C# toolchain unreachable and atomic execution structurally impossible
metadata:
  type: project
---

If the harness isolates your session to a worktree, the worktree-isolation guard refuses **every**
`pwsh` invocation from scoped-tool agents — `-Command` and `-File`, single- or double-quoted, with or
without a `$`. The refusal text is `... this command runs pwsh in a plain command; what it reads or is
handed as shell text cannot be shown not to run git.` It also refuses `git -C <other-worktree>` and
`cd <other-worktree> && git ...`.

**The block is agent-type dependent, and that is the trap.** Verified on issue #736, 2026-09-03, all
three probes in the same worktree with the identical command
`pwsh -NoProfile -Command "Write-Output PROBE"`:

| Agent | pwsh | git |
|---|---|---|
| `general-purpose` (`Tools: *`) | **OK** | OK |
| `atomic-executor` (scoped `Bash(pwsh *)`) | REFUSED | OK |
| `orchestrator` (scoped Bash) | REFUSED | OK |

So a `general-purpose` probe **cannot** be used to clear `pwsh` for `atomic-executor`. I ran exactly
that probe, got `PROBE_OK`, and concluded execution was viable; the atomic-executor preflight agent
then reported pwsh was refused for it. Two agents contradicted each other and I was right to re-probe
with the *same agent type* rather than believe either. Always probe the agent type that will do the work.

**Why this is fatal for C# work, not merely inconvenient.** There is no non-pwsh fallback:

- `msbuild` — `command not found` from bash (it is resolved through vswhere inside pwsh).
- `vstest.console.exe` — `command not found` from bash.
- `dotnet` — on PATH, but every subcommand exits 155 with `The repo-local .NET SDK is missing. Run
  ./scripts/vscode/Install-RepoDotNetSdk.ps1`, and that bootstrap is itself a pwsh script.

So all four CLAUDE.md toolchain steps are unreachable and `atomic-executor` cannot complete S5 however
the plan is written. Under the orchestrate contract a mandatory delegated step that cannot be completed
means **stop and record blocked state** — do not substitute `general-purpose` (it is not a configured
specialist) and do not run the toolchain yourself (the orchestrator is orchestration-only).

**How to apply:** when a parallel/epic parent hands you a worktree path, first check whether the harness
actually placed you there (`git worktree list` plus your own `pwd`). If you are isolated somewhere else,
probe `pwsh` **with the agent type that will execute** before spending planner cycles. Read-only work
still pays off — plan re-anchoring, validator runs, and commits all work fine — so finish and commit
those, then hand back a blocked report naming the one thing the next session needs: a session that is
either not worktree-isolated or is isolated to the item's own worktree.

Related: [[pwsh-double-quoted-command-refused-in-worktree]] records an earlier, narrower form of this
guard where the discriminator was `$` complexity rather than pwsh categorically;
[[bash-tool-rejects-complex-commands-in-isolated-worktree]] and
[[subagent-self-reported-correction-can-be-false]] are the same family.
