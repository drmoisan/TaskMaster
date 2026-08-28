---
name: relative-path-in-pwsh-dotnet-io-hits-wrong-worktree
description: PowerShell Set-Location does not move .NET's CurrentDirectory, so [System.IO.File]::ReadAllText('rel/path') in a helper script silently reads or writes the SESSION worktree instead of the target one
metadata:
  type: project
---

In a delegated multi-worktree run, a `pwsh -File helper.ps1` that does
`Set-Location $targetWorktree` and then `[System.IO.File]::ReadAllText('QuickFiler.Test/QuickFiler.Test.csproj')`
reads a **different worktree's** copy of that file.

**Why:** `Set-Location` moves PowerShell's provider location only. `System.IO`
resolves a relative path against `Environment.CurrentDirectory`, which is inherited
from the launching process — the Bash tool's cwd at the moment `pwsh` started. The Bash
tool **resets its cwd between calls**, so a helper script invoked from a call that did not
begin with `cd <target>` lands on the session worktree. Both trees contain the same
relative paths, so `ReadAllText` succeeds and no error is raised.

Observed 2026-08-27 (child 489, Batch D): a csproj insert script threw
`anchor occurrences = 0` for an anchor that `$raw.Contains($anchor)` proved was present.
The anchor was absent because the script had read the sibling worktree's csproj. Had the
anchor happened to exist in both, `WriteAllText` would have **silently edited the wrong
branch** and the target tree's `git status` would have stayed clean.

**How to apply:** In every helper `.ps1`, hard-code absolute paths, or set both locations:

```powershell
Set-Location $root
[System.IO.Directory]::SetCurrentDirectory($root)
```

Do the same before any `& msbuild`/`& vstest` invocation that passes a relative project or
assembly path. A guard that throws when an expected anchor count is wrong (rather than
replacing 0 occurrences and reporting success) is what makes this failure visible at all —
always assert the occurrence count before writing. See [[pwsh-git-gh-cli-gotchas]] and
[[sln-csproj-edit-crlf-preserve]].
