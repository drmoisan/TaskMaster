---
name: bash-heredoc-collapses-doubled-backslashes
description: The Bash tool collapses `\\` to `\` even inside a quoted heredoc, silently breaking .NET/Python regex patterns written to script files — it turned `"\\bin\\Debug\\"` into a word-boundary assertion and forced a full Phase 5 toolchain restart.
metadata:
  type: project
---

Writing a script file from the Bash tool with `cat > f <<'EOF'` **collapses doubled
backslashes to single ones**, despite the quoted heredoc delimiter that is supposed to
suppress all processing. Single backslashes survive; only `\\` is affected.

**Why:** measured on 2026-08-26 while executing Phase 5 of the #446 plan. The plan's
assembly-discovery prelude filters on `$_.FullName -match "\\bin\\Debug\\"`. What actually
reached the `.ps1` file was `"\bin\Debug\"`, verified with `grep ... | cat -A`. Under .NET
regex semantics `\b` is a word-boundary assertion and `\D` a non-digit class, so the filter
matched none of 9 real `bin\Debug` assemblies:

```
RAW=18          # Get-ChildItem -Recurse -Filter *.Test.dll -File
AFTER_BIN=0     # after the collapsed "\bin\Debug\" filter
```

`vstest.console.exe` then ran with an empty source list, printed
`No test source files were specified.` and exited 1. Because the Phase 5 preamble restarts
the loop from step 1 on any failed step, this cost a full re-run of format, format-check,
analyzer rebuild and type-check rebuild — even though nothing in the repository was wrong.

The same trap hits Python: `rb'C:\\Users\\...'` in a heredoc arrives as `rb'C:\Users\...'`
and `re.subn` dies with `PatternError: bad escape \U`. A first, non-raw attempt was worse —
it *silently* mis-scrubbed a TRX, because `'...\repos\...'` turned `\r` into a carriage
return so the repo-root replacement matched 0 times while the user-profile rule fired 6,529
times. That produced a plausible-looking but wrong redaction that only a residual
case-insensitive `danmoisan|megalodon` count exposed.

**How to apply:** never put `\\` in a heredoc body. Build separators programmatically
instead, which is semantically identical and immune:

- PowerShell: `$pat = [regex]::Escape('\bin\Debug\')` (single-quoted, single backslashes)
- Python: `B = bytes([92])` then concatenate, and always pair with `re.escape(...)`

Verify with `grep -n <token> <file> | cat -A` before trusting any script file that contains
a backslash. When Bash cannot express the content safely, fall back to the Write tool — a
long artifact with backticks and apostrophes also aborted a heredoc with
`unexpected EOF while looking for matching '`, and Write handled it first try.

Related: [[project_pwsh_command_quoting_from_bash]], [[project_pwsh_git_gh_cli_gotchas]],
[[_shared_no_absolute_host_paths]].
