---
name: project-loop-iteration-vs-single-artifact-glob-and-hash-anchor
description: Preflight defect class - a "glob must match exactly one artifact" convention contradicts repair/restart loops, and a hash-anchor gate keyed on "the formatter recorded REWRITTEN 0" breaks when the loop ran twice; also core.autocrlf IS readable from a linked worktree
metadata:
  type: project
---

Found in #931 round-4 preflight (2026-09-28).

1. A plan convention "later tasks locate `<task-id>-<name>.*.md` with a glob that must match exactly one file" silently conflicts with any loop that re-runs a task (P2-T9 "re-run P2-T7 through P2-T9, record each iteration"; Phase 4 "restart from P4-T1"). Each iteration writes a new timestamped artifact, so later comparisons ("its P2-T7 post-format hash") become ambiguous. Ask for an `ITERATION:` field and a "read the highest iteration" rule.

2. A gate "hash equals the pre-loop anchor when the formatter task recorded REWRITTEN: 0" is unsatisfiable after a two-iteration format loop: the final iteration always records 0, but the file was rewritten in iteration 1. Key the condition on "every iteration recorded 0 / loop started once", else compare against the final iteration's after-hash.

3. `git config --show-origin --get-all core.autocrlf` works from inside a linked worktree (on this workstation it comes from the Git for Windows system gitconfig, value true). A plan claiming it "cannot be re-read from inside the worktree" states a false fact. Existing tracked .cs/.csproj files here read `i/lf w/crlf attr/text=auto` in `git ls-files --eol`.

**How to apply:** on any plan with a loop or restart clause, check every cross-task reference to a looped task's artifact and every equality gate against a pre-loop anchor.
