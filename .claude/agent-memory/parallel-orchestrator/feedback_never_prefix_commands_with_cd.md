---
name: never-prefix-commands-with-cd
description: Never write `cd <path> && <cmd>` and never instruct a child to cd per call — a leading cd breaks the Bash allowlist prefix match, so every auto-approved command becomes an approval prompt and orchestration stalls
metadata:
  type: feedback
---

Never begin a Bash command with `cd`. Use `git -C <abs-path>` for git, and absolute path
operands for everything else. Never put a per-call `cd` instruction into a delegation prompt.

**Why:** Operator, 2026-09-02, on run `bugs-2026-09-02`: "stop sending cd cp commands that
require approval. you are intended to run yolo by yourself. do not send stupid instructions
that stop the flow of orchestration." The mechanism is not style. Permission allowlist entries
are command-PREFIX matches (`Bash(git *)`, `Bash(gh *)`, `Bash(bash .claude/lib/bash/...)`).
A command that starts with `cd` does not match any of them, so a compound
`cd <worktree> && git status` is denied the auto-approval that a bare `git -C <worktree> status`
receives. Prefixing every call with `cd` therefore converts an entire orchestration run into a
sequence of approval prompts. On a run with thirteen items and many git calls per item, that is
the difference between unattended execution and a stall on every command.

**How to apply:**

- **Use `git -C <absolute-worktree-path> <subcommand>`.** It matches `Bash(git *)`, it needs no
  cwd, and it is immune to the agent-thread cwd reset. This covers status, log, rev-parse, diff,
  fetch, merge, add, commit, and push.
- **`gh` takes `--repo` and explicit branch/PR operands**, so it never needs a cwd either. Keep
  the command bare so the merge gate reads the right number — see
  [[issue-merge-and-removal-commands-bare]], which is the same "keep the command bare" rule
  arrived at from the gate-parsing side rather than the allowlist side.
- **Do not write a per-call `cd` directive into a child prompt.** State the item worktree as an
  absolute path and tell the child to pass it as an operand (`git -C`, absolute Read/Write/Edit
  paths, absolute solution and project paths for msbuild and dotnet). The child inherits the
  session cwd rather than its worktree, so the path still has to be stated — but as an OPERAND,
  never as a `cd`.
- **Only genuinely cwd-bound tools are the exception**, and they are rare next to git: a
  formatter invoked as `format .` needs its directory. Give those their own single call and
  accept the prompt there rather than paying it on every git call.
- **Same family as [[pr-creation-readiness-gate-and-receipt-mechanics]]**, where `cp -p` was
  named as the receipt-preserving copy. Prefer a tool-native path operand over any shell
  navigation or file-shuffling step whose only purpose is to position a later command.
