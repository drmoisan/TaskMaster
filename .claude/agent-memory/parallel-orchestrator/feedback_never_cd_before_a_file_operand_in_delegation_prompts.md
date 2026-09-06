---
name: never-cd-before-a-file-operand-in-delegation-prompts
description: A cd prefix makes a file operand statically unresolvable, and the mere existence of any Read() deny rule then forces a user permission prompt — never instruct a child to cd before a command naming a file
metadata:
  type: feedback
---

Never write a delegation prompt that tells a child to prefix bash calls with `cd`. Instruct
absolute paths instead, and route file-to-file copies through the Write tool rather than `cp`.

**Why:** On run `bugs-2026-09-02` (2026-09-02) the operator was interrupted by
`cp on 'artifacts/orchestration/orchestrator-state.json' after a cd would search a directory that
cannot be determined here, and a Read() deny rule is configured; only you can approve running it
anyway.` The operator's reaction was direct: "why do you keep asking me for permission you should
already have?" The prompt was caused by two instructions I had written into every child prompt —
prefix every bash invocation with a `cd` into the item worktree, and sync the per-item checkpoint to
the canonical path with `cp`.

The mechanism is worth stating exactly, because the deny rule that triggers it is unrelated to the
command. `.claude/settings.json` denies only `Read(./.env)`, `Read(./.env.*)`, and
`Read(./secrets/**)`. A `cd` prefix makes the operand's directory statically undeterminable, so the
engine cannot PROVE the target falls outside those three patterns. It cannot allow what it cannot
resolve, so it escalates. The denied paths never had to be involved; their existence alone is
enough once the path is opaque. An absolute operand resolves, is checked against the patterns,
does not match, and is allowed silently.

**How to apply:**

- **Absolute paths, no `cd`, for anything naming a file.** This is the same family as the
  pre-implementation gate trap in [[preimplementation-gate-scope]], where `cd X && git add Y` is
  denied while the bare `git add Y` is allowed. Both are static command analysis defeated by a
  directory change. Generalize it: a `cd` prefix breaks every analyzer that has to reason about a
  path, not just the one gate where it was first observed.
- **Prefer the Write tool over `cp` for checkpoint synchronization.** `Edit(artifacts/**)` is on the
  allow list and the pre-implementation gate exempts the seven
  `artifacts/orchestration/*-state.json` checkpoints regardless of extension, so having the child
  write both the per-item file and the canonical file directly avoids bash permission resolution
  entirely. There is no reason to spend a shell command on a copy.
- **A worktree directive does not require a `cd` directive.** Reusing an existing worktree
  ([[reuse-existing-item-worktrees]]) needs the child to operate on absolute paths under that
  worktree; it does not need, and should not carry, an instruction to `cd` there first. State the
  absolute root and require absolute operands.
- **There is no channel to retract an instruction from a running background child.** `SendMessage`
  is not in the parallel-orchestrator tool set, so a prompt defect discovered mid-flight cannot be
  corrected for children already launched — it can only be fixed for the next batch. That asymmetry
  is a reason to review a delegation prompt for permission-surface defects BEFORE the first launch,
  not after the first complaint.
- Same authoring family as [[keyed-issue-num-in-delegation-prompts]] and
  [[never-backtick-exclusion-paths-in-delegation-prompts]]: the prompt text is an interface to
  mechanical matchers, and a natural phrasing can defeat one.
