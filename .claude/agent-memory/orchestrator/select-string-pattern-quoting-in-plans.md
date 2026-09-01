---
name: select-string-pattern-quoting-in-plans
description: Two quoting traps that silently neuter Select-String assertions in plans — escaped pipe is a LITERAL pipe, and \\ / \" die in transit to pwsh; spell them as \x5C and \x22
metadata:
  type: project
---

Both were measured on 2026-09-01 during issue #663 preparation, and both produce
a gate that reads correctly and verifies nothing.

**1. `\|` is a literal pipe, not alternation.** `Select-String -Pattern` takes a
.NET regex, where a backslash before a pipe escapes it. Measured against a
two-line fixture:

- `FromHandle\|new KeyEventArgs` returned **0** matches.
- `FromHandle|new KeyEventArgs` returned **2**.

So an acceptance condition asserting "returns zero matches" over the escaped
spelling passes whatever the executor wrote, and one asserting a positive count
can never pass. The escaped form is the one an author reaches for out of shell
habit, and it is always wrong here.

Complication: a bare `|` inside a GitHub-flavoured markdown TABLE cell
terminates the cell, even inside a code span. Do not solve that by escaping the
pipe. Move the pattern into a fenced block outside the table and reference it by
a label such as VC-1.

**2. `\\` and `\"` do not survive the trip to `pwsh`.** In a plan task that runs
`pwsh -NoProfile -Command '... -Pattern "..."'`:

- a doubled backslash de-doubles when the command text reaches the native `pwsh`
  executable, leaving an unbalanced group (`Too many )'s`);
- `\"` is not an escape sequence inside a PowerShell double-quoted string, so the
  string terminates at that quote and the rest is parsed as a command
  (`The module 'Meziantou' could not be loaded`).

Both raise TERMINATING errors, so the task fails outright rather than passing
vacuously — noisy, but it still costs a preflight round.

Fix: spell the backslash `\x5C` and the double quote `\x22`. Those hex escapes
are interpreted by the .NET regex engine and are inert to every intervening
quoting layer. Keep `\.` as-is; escaping a literal dot is correct and unaffected.

**How to apply:** before a plan ships, execute every `Select-String` pattern it
asserts over, verbatim, and confirm both that it runs and that it matches real
data on the side you expect. Reading the pattern is not enough for either trap.
Related: [[bash-tool-collapses-double-backslash-in-sed]],
[[pwsh-double-quoted-command-refused-in-worktree]].
