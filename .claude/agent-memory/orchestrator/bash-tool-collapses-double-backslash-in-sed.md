---
name: bash-tool-collapses-double-backslash-in-sed
description: The Bash tool collapses \\ before sed or grep sees it, so a backslash pattern silently matches nothing; use . as the separator wildcard, and never trust a zero-match sweep that has no passing control case
metadata:
  type: project
---

**`\\` never reaches sed.** `sed 's|c:\\users\\danmoisan|X|g'` matches NOTHING against the literal text
`c:\users\danmoisan`, while `sed 's|users.danmoisan|X|g'` matches it fine. The `.` wildcard matches the
backslash but `\\` does not, so the tool layer collapses the escape before sed parses the regex.
`sed -i` then exits 0 having changed nothing — a silent no-op, not an error.

**How to apply:** to match a Windows path in sed through this tool, write each separator as `.` and let
the surrounding literal carry the specificity, e.g.
`s|c:.users.danmoisan.repos.taskmaster..claude.worktrees.agent-XXXX|REDACTED-WORKTREE-ROOT|g`
(note `..claude` for `\.claude` — one `.` for the separator, one for the dot). ALWAYS re-grep for a
match count afterwards. On feature 488 the first "successful" pass appeared to do the work and only the
verification grep exposed that nothing had changed.

**It bites `grep` identically, and there the failure is a false ALL-CLEAR.** On 2026-08-29 a
host-identity sweep `grep -rniE "c:\\\\users|danmoisan|..." <feature-folder>` reported zero matches and
looked like a clean bill of health. Running the SAME pattern against a file known to contain
`C:\Users\DanMoisan\...` also reported zero. The pattern was never matching anything; the sweep proved
nothing. Re-run byte-level in Python (`re.compile(rb"[Cc]:\\\\[Uu]sers")` over `open(p,"rb").read()`),
the control matched and the feature folder was genuinely clean — but that was luck, not verification.

**The general rule: a negative result is only evidence if the same command produces a positive on a
known-positive input.** Pair every "no matches, therefore clean" sweep with a control case in the same
invocation and print both. This costs one extra line and is the only thing standing between a mangled
pattern and shipping unredacted host paths while reporting success. It applies to any absence claim,
not just redaction: zero occurrences of a banned API, zero `Skipping target` lines, zero unformatted
files.

**Related guard behaviour.** In an isolated agent worktree the tool also refuses whole commands it
cannot verify stay inside the worktree — "this command is too complex to verify". This fires on
`cmd && cmd` chains that assign a variable and redirect, and on any heredoc whose body merely *mentions*
git. Split into plain sequential commands, or author the file with the Write tool.

**Why:** both surfaced while sanitizing 19 TRX evidence files, where a silent no-op would have shipped
unredacted host paths while reporting success.

See [[bash-tool-rejects-complex-commands-in-isolated-worktree]], [[bash-tool-mangles-msbuild-switches]],
and [[angle-bracket-redaction-breaks-trx-xml]].
