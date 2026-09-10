# Base anchor (Issue #824, task P0-T2)

Timestamp: 2026-09-09T14-58

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; Write-Output ("BRANCH=" + (git rev-parse --abbrev-ref HEAD)); Write-Output ("HEAD=" + (git rev-parse HEAD)); Write-Output ("BASE_REF=" + (git merge-base HEAD origin/main))'`

EXIT_CODE: 0

Output Summary:

```
BRANCH=bug/ilglobals-loadopcodes-unsynchronised-static-race-824-exec
HEAD=553f874a287261af0dd42e4f9270d31ac475308a
BASE_REF=6f08302a4f0af0061f27856e8a654f819df902aa
```

- `BASE_REF=6f08302a4f0af0061f27856e8a654f819df902aa` — 40 hexadecimal characters.
- `HEAD=553f874a287261af0dd42e4f9270d31ac475308a` is the epic integration tip this worktree was
  fast-forwarded to; it carries the merged work of the sibling children of the epic.
- The branch name carries the `-exec` suffix deliberately: a preparation branch of the same base
  name remains checked out in a locked preparation worktree.

Per plan D12 no shell variable survives between tasks, so every later task re-derives the merge base
inside its own invocation. The value recorded here is the reference those re-derivations are
compared against. A re-derived value that differs from it is reported in the consuming task's
artifact rather than used silently.

Note on the recorded `Command:` line: the executing session's process working directory is a
different worktree from the one under test, so a `Set-Location` prefix is prepended to every plan
command. The prefix is an invocation-channel detail, not a change to the command the plan states.
