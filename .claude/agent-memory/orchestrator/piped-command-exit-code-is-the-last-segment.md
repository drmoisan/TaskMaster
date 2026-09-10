---
name: piped-command-exit-code-is-the-last-segment
description: In `cmd | head; echo $?` the reported status is head's, not cmd's — I nearly wrote a false memory about gh pr checks changing behaviour
metadata:
  type: feedback
---

In a piped Bash call, `$?` reports the status of the LAST segment, not the command you care about. `gh pr checks <N> | head -20; echo "EXIT=$?"` printed `EXIT=0` on a PR with zero checks, which contradicted the recorded sibling observation that `gh pr checks` exits 1 in that state. The contradiction was my own measurement artifact: `head` succeeded, so `$?` was head's 0 and `gh`'s real exit code was discarded by the pipe.

**Why:** I was one step from writing a memory asserting that `gh pr checks` returns 0 on a no-checks PR and that the earlier sibling record was wrong. That would have poisoned the CI-gate reasoning for every future epic child, because the whole point of the no-checks case is that you must NOT read it as green — and an exit code of 0 is exactly the signal that would tempt a future run to do so.

**How to apply:** When a command's exit code is load-bearing, run it unpiped and capture the status directly, or use `PIPESTATUS`. More generally: when a fresh measurement contradicts an existing memory, suspect the measurement before overwriting the memory — check whether the harness (a pipe, a wrapper, a different cwd) mangled it. See [[my-own-negative-claims-need-a-scoped-search]] for the same failure shape in the other direction, and [[grep-count-wrapper-does-not-clear-lastexitcode]] for a related exit-code leak.

Relevant here because the epic-child CI gate is decided on exactly this signal: [[project_epic_child_prs_no_ci]].
