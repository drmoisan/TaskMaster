# P0-T9 — Tool Manifest Restore (CSharpier)

Timestamp: 2026-09-03T11-24
Command: ".dotnet-sdk/dotnet.exe" tool restore --tool-manifest "dotnet-tools.json" (paths relative to the item worktree root)
EXIT_CODE: 0
Output Summary: "Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier"
/ "Restore was successful."

Environment note (execution-mechanics deviation, not a plan-scope change): the Bash tool's
working directory defaults to the session worktree
(the session worktree, a separate checkout outside this item's worktree), not the item worktree named by the
delegation prompt, and the delegation prompt's Bash-discipline rule prohibits `cd`. `dotnet`'s
global.json-based SDK/tool-manifest resolution is directory-tree search from the process's
working directory, so a bare `dotnet` invocation from the default cwd cannot see the item
worktree's `global.json`/`dotnet-tools.json`. Resolved by (1) invoking the item worktree's
already-installed pinned SDK executable
(`.dotnet-sdk/dotnet.exe`) by absolute path, which self-resolves without needing cwd-based
version negotiation, and (2) passing `--tool-manifest` with the item worktree's
`dotnet-tools.json` absolute path explicitly, rather than relying on cwd search. Verified the
session worktree's own `dotnet-tools.json` is byte-identical to the item worktree's (both pin
csharpier 1.2.6), so this substitution introduces no version drift. The same absolute-executable
pattern is used for all subsequent `dotnet`/`msbuild`/`vstest.console.exe`/`pwsh` invocations in
this plan's execution.
