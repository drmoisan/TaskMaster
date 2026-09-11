---
name: system-io-relative-paths-escape-the-worktree
description: In a Bash-launched pwsh, System.IO members and native exes resolve relative paths against Environment.CurrentDirectory, not Set-Location, so a relative path silently reads or writes ANOTHER worktree
metadata:
  type: feedback
---

`Set-Location` moves only PowerShell's *provider* location. `System.IO` members and any
native executable resolve a relative path against `Environment.CurrentDirectory`, which a
`pwsh` launched from the Bash tool inherits from the launching process — the **session
root**, not the agent worktree. So a relative path in `[System.IO.File]::WriteAllText`,
`[System.IO.Directory]::Exists/Delete`, or an argument to `gh` silently targets a
different worktree.

**Why:** this bit three times in one run of issue 812 and is invisible on inspection.

1. An executor wrote a `<Compile Include>` line into the *parent session worktree's*
   `UtilitiesCS.Test.csproj` via `WriteAllText` with a relative path. It was caught only
   because the item worktree showed no diff, then reverted.
2. A plan step I authored used `[System.IO.Directory]::Exists("coverage/...")` to clear a
   stale results tree. Preflight proved by probe that `Environment.CurrentDirectory` sat
   in a different worktree while the provider location was correct: the test would have
   returned `False`, left the stale tree, and blended two coverage collections — and a
   delete that *did* find something there would have removed another worktree's directory.
3. `gh pr create --body-file artifacts/pr_body_<N>.md` resolved the body against the
   session root for the same reason.

The failure is silent and bidirectional: it skips the work you intended, or it performs
the work on someone else's tree.

**How to apply:** prefer absolute paths for every `System.IO` call and every native-exe
argument. When a relative path is unavoidable — a hook that demands the canonical
repo-relative spelling — set BOTH inside the same command string:

```
Set-Location '<worktree>'; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); <command>
```

Verify with `[System.IO.Directory]::GetCurrentDirectory()` before trusting any
relative-path result. Related: [[bash-tool-eats-dollar-vars-and-drops-git-output-flag]],
[[child-orchestrator-pr-hook-reads-session-root]].
