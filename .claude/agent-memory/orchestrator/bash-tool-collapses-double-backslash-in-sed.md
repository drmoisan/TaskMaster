---
name: bash-tool-collapses-double-backslash-in-sed
description: The Bash tool collapses \\ before sed sees it, so a literal backslash cannot be matched with \\ in a sed pattern; use . as the separator wildcard and always verify with a match count, because a bad pattern is a silent no-op
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

**Related guard behaviour.** In an isolated agent worktree the tool also refuses whole commands it
cannot verify stay inside the worktree — "this command is too complex to verify". This fires on
`cmd && cmd` chains that assign a variable and redirect, and on any heredoc whose body merely *mentions*
git. Split into plain sequential commands, or author the file with the Write tool.

**Why:** both surfaced while sanitizing 19 TRX evidence files, where a silent no-op would have shipped
unredacted host paths while reporting success.

See [[bash-tool-rejects-complex-commands-in-isolated-worktree]], [[bash-tool-mangles-msbuild-switches]],
and [[angle-bracket-redaction-breaks-trx-xml]].
