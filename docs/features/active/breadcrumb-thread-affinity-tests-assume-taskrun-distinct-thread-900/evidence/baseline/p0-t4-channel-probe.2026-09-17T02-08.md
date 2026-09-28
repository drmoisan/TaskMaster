# P0-T4 Part 1 — Command Channel Probe

Timestamp: 2026-09-17T02-08

Command: rung 1 `pwsh -NoProfile -Command '<probe payload>'`; rung 2 `pwsh -NoProfile -File coverage/plan900-helper.ps1`.

EXIT_CODE: 0

CHANNEL: COMMAND

## Rung results

RUNG-1: OK

The trivial payload `Write-Output "PROBE-OK"` was submitted through
`pwsh -NoProfile -Command`, wrapped in single quotes so the calling shell performed no
interpolation, and the output line `PROBE-OK` was observed. No refusal text was produced.

RUNG-1-REFUSAL: none

RUNG-2: OK

The same probe line was written byte for byte to the single fixed helper path
`coverage/plan900-helper.ps1` and executed with `pwsh -NoProfile -File`. The output line
`PROBE-OK` was observed. The helper path was not refused by a `Write` PreToolUse hook, so the
session-scratchpad alternative the plan declares was not needed. `coverage/` is git-ignored, the
helper is registered in no project file and is asserted by no acceptance condition.

RUNG-2-REFUSAL: none

Rung 2 was probed even though rung 1 succeeded, so that this artifact documents both rungs in this
session. This is the plan's stated behaviour, not a fallback.

## Channel determination

CHANNEL: COMMAND

Rung 1 returned `OK`, which fixes the channel as `COMMAND` by the plan's rule. Every later
command-bearing task uses that channel and records it in its own `CHANNEL:` field. `CHANNEL
UNAVAILABLE` was not reached; this session is not worktree-isolated, which is consistent with both
rungs succeeding, since the isolation guard keys on `pwsh` occupying the command-name position and
would have refused both rungs with the same text.

## Execution-environment wrapper (recorded once, here)

This session's process working directory is a different checkout from the execution worktree, so
every `pwsh` payload in this run is prefixed with a two-statement preamble as its first two
statements:

    Set-Location -LiteralPath "<repo-root>"
    [System.IO.Directory]::SetCurrentDirectory("<repo-root>")

Both statements are required: `Set-Location` does not update .NET's `CurrentDirectory`, so without
the second statement any `[System.IO.*]` call would resolve a relative path against the wrong
checkout. `-File` invocations additionally pass `-WorkingDirectory <repo-root>`, because a `-File`
process otherwise starts in the session root rather than the execution worktree.

The preamble is an execution-environment wrapper supplied by the delegation. It changes no plan
command, no plan path and no acceptance condition; it is how this session satisfies the plan's
standing "Working directory" convention that every command runs with the current directory at the
worktree root and every path is repository-relative. It is recorded once here and is not restated
in later artifacts. `<repo-root>` above stands for the worktree root; the absolute value is not
written into any artifact.

## Output Summary

Both declared rungs were probed and both returned the expected `PROBE-OK` line. The channel is
fixed as `COMMAND` for the remainder of this run. Neither rung produced a refusal, so no refusal
text required host-identifier substitution. This artifact contains no absolute filesystem path, no
account name and no machine name.
