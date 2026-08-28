---
name: blocked-bash-command-silently-drops-chained-checkoff
description: The dangerous-pattern guard aborts the WHOLE compound Bash command before anything runs, so an &&-chained plan check-off is lost with no error naming it; audit the checkbox row after any blocked command
metadata:
  type: project
---

When the Bash tool refuses a command (`Blocked dangerous command pattern detected: ...`),
**nothing in that command line runs** — including everything chained ahead of the offending
segment with `&&`.

**Why:** the guard is a pre-execution filter on the whole command string, not a per-segment
one. A line shaped `pwsh checkoff.ps1 -TaskId P7-T4 && cat > runner.ps1 <<'EOF' … Remove-Item -Recurse -Force … EOF`
is rejected as a unit. The only output is the block message, which names the *pattern*, not the
work that was skipped. Observed 2026-08-27 (child 489, Batch D): `[P7-T4]` stayed `- [ ]` in the
plan for the rest of the phase, and the loss was only caught at the end by re-reading the
checkbox row — the artifact existed and the task was genuinely complete, so nothing else
signalled it.

**How to apply:** Never chain a plan check-off, an evidence write, or any other state mutation
onto the same Bash line as the command it verifies. Run the check-off as its own call. After any
blocked command, re-run `grep -oE '^- \[[ x]\] \[P#-T[0-9]+\]' <plan>` for the affected phase and
reconcile against the artifacts on disk before continuing. Prefer the running-count-of-`[x]`
output that the check-off helper prints: a count that advances by 1 per task makes a dropped
check-off visible immediately instead of at the end of the batch.
