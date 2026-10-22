# GetRelativePath Anchored-Pattern Probe ([P1-T3])

Timestamp: 2026-09-03T12-01

Command: `pwsh -NoProfile -Command '$bs = [char]92; $old = $bs + $bs + $bs + ".claude" + $bs + $bs; $new = "(^|" + $bs + $bs + ")" + $bs + ".claude" + $bs + $bs; "OLD_PATTERN=" + $old; "NEW_PATTERN=" + $new; $pairs = @(@("C:\repo\.claude\worktrees\agent-7\.","C:\repo\.claude\worktrees\agent-7\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll"),@("C:\repo\.","C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll"),@("C:\repo\.claude\worktrees\agent-7\.","C:\repo\.claude\worktrees\agent-7\.claude\worktrees\agent-9\Nested.Test\bin\Debug\Nested.Test.dll")); foreach ($p in $pairs) { $rel = [System.IO.Path]::GetRelativePath($p[0], $p[1]); "REL=" + $rel; "OLD_REGEX_MATCH=" + ($rel -match $old); "NEW_REGEX_MATCH=" + ($rel -match $new) }; exit 0'`

EXIT_CODE: 0

## Emitted lines, verbatim (all eleven)

```
OLD_PATTERN=\\\.claude\\
NEW_PATTERN=(^|\\)\.claude\\
REL=QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
OLD_REGEX_MATCH=False
NEW_REGEX_MATCH=False
REL=.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
OLD_REGEX_MATCH=False
NEW_REGEX_MATCH=True
REL=.claude\worktrees\agent-9\Nested.Test\bin\Debug\Nested.Test.dll
OLD_REGEX_MATCH=False
NEW_REGEX_MATCH=True
```

Output Summary: Both patterns arrived intact. `OLD_PATTERN=` is character-identical to the pattern this plan quotes, and `NEW_PATTERN=` is character-identical to the anchored replacement literal, so no layer between the plan and `pwsh` collapsed a doubled backslash and every `_MATCH=` value below them is trustworthy. The three measured relative paths confirm the planner finding: `GetRelativePath` returns a descendant path with no leading separator, so the unanchored `\\\.claude\\` pattern matches none of the three relative paths (three `OLD_REGEX_MATCH=False`), which would retain the nested sibling worktree in case 2 and break the preserved original regression test. The anchored `(^|\\)\.claude\\` pattern excludes cases 2 and 3, whose relative paths begin with `.claude`, and retains case 1, whose relative path does not. This observation is what the anchored replacement literal in `[P2-T1]` rests on.
