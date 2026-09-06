---
name: dotnet-global-json-cwd-search-vs-bash-discipline
description: dotnet's global.json/tool-manifest resolution walks the process cwd, not an invoked exe's location, which breaks under a no-cd Bash-discipline delegation where the Bash tool's default cwd is a different worktree than the item worktree
metadata:
  type: project
---

When a delegation prompt enforces "never use cd, address the item worktree via absolute paths"
(the standard TaskMaster item-worktree Bash-discipline block), a bare `dotnet` invocation for
SDK-pinned commands (`dotnet tool restore`, `dotnet tool run csharpier ...`) can fail with the
item worktree's own `global.json` errorMessage ("The repo-local .NET SDK is missing...") even
though the SDK was correctly installed into that worktree's `.dotnet-sdk/`.

**Why:** the .NET muxer's `global.json` search walks the **process working directory** upward,
not the location of the `dotnet.exe` you invoke. The Bash tool's default cwd is the session
worktree (e.g. `TaskMaster-wt/<session>/`), not the item worktree named in the delegation
(`.claude/worktrees/<item>/`) — confirmed via `pwd`. Since `cd` is prohibited and every other
command must take absolute-path operands, there is no literal way to point `dotnet`'s cwd-based
resolution at the item worktree.

**How to apply:**
- Invoke the item worktree's own pinned SDK executable by absolute path
  (`<item-worktree>/.dotnet-sdk/dotnet.exe ...`) — this self-resolves for simple commands
  (`--version`) without needing cwd-based version negotiation.
- For `dotnet tool restore`, pass `--tool-manifest <item-worktree>/dotnet-tools.json` explicitly
  rather than relying on cwd-based manifest discovery.
- For `dotnet tool run <cmd>` (no `--tool-manifest` flag exists for this subcommand), the manifest
  is resolved by cwd search regardless. If the session worktree is a sibling checkout of the SAME
  repo and its own `dotnet-tools.json` is byte-identical to the item worktree's (verify with
  `diff`/`cat` before relying on this), running from the default cwd resolves the same pinned tool
  version. This is safe because CSharpier operates on the absolute file-path arguments you pass,
  regardless of which cwd's manifest resolved the tool binary. Document the substitution in the
  evidence artifact.
- MSBuild.exe and vstest.console.exe are standalone native executables (resolved via `vswhere`),
  not the `dotnet` muxer — they are unaffected by this issue as long as the solution/project/DLL
  paths passed to them are absolute.
- Record the deviation explicitly in the P0-T8/P0-T9-equivalent evidence artifact rather than
  silently working around it, since it is a real mechanical consequence of the Bash-discipline
  contract, not a plan defect.

See also [[project_planner_and_executor_observe_different_worktrees]] and
[[project_relative_path_in_pwsh_dotnet_io_hits_wrong_worktree]] for the same class of
cwd/worktree-mismatch issue in other tools.
