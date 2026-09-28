---
name: pwsh-double-quoted-backslash-defeats-msbuild-nonvacuity-grep
description: A needle written as "/out:obj\\Debug\\" in a PowerShell double-quoted string keeps BOTH backslashes and matches 0 lines, making an msbuild non-vacuity gate read a false zero
metadata:
  type: project
---

PowerShell double-quoted strings do **not** treat backslash as an escape character (the escape
char is the backtick). So `"/out:obj\\Debug\\"` is the literal `/out:obj\\Debug\\` — two
backslashes each — and `Select-String -SimpleMatch` against an msbuild log finds **0** matches
even when the token is present dozens of times.

**Why:** this is the exact shape of a gate that reports a wrong answer for a reason unrelated to
the code. The msbuild non-vacuity check (`at least 18 lines containing /out:obj\Debug\`, gate
rule 7) exists to prove `CoreCompile` actually ran. A false zero reads as "the build skipped
every compile" on a build that in fact compiled 18 assemblies — and the natural next move is to
go hunting for a warm-build problem that does not exist.

**How to apply:** build the needle from `[char]92` rather than typing it:

```powershell
$bs = [char]92
$needle = "/out:obj" + $bs + "Debug" + $bs
@(Select-String -Path "coverage/analyzers.msbuild.log" -Pattern $needle -SimpleMatch).Count
```

Note this is the *opposite* direction from [[project_bash_heredoc_collapses_doubled_backslashes]]
and [[project_tool_layer_collapses_double_backslash_in_file_content]], where a layer **removes**
one backslash. Here nothing removes it, so doubling it is what breaks the match. When a Windows
path token has to reach PowerShell, check which layers are in play before choosing the spelling,
and confirm the count is non-zero on a run you know should match.

Confirmed 2026-09-20 on issue #911 remediation cycle 1, tasks P0-T10 and P0-T11.
