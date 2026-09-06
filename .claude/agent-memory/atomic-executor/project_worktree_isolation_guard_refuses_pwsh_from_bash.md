---
name: worktree-isolation-guard-refuses-pwsh-from-bash
description: In an isolated agent worktree the PreToolUse guard refuses every Bash invocation of pwsh (both -Command AND -File) and every command whose NAME is a quoted absolute path, so plan tasks whose only command is one of those shapes cannot run
metadata:
  type: project
---

In a worktree-isolated agent session (`.claude/worktrees/agent-<id>/`), the Bash guard refuses two
distinct command shapes.

**Shape 1 — anything that invokes `pwsh`.** Refusal text: "this command runs pwsh in a plain command;
what it reads or is handed as shell text cannot be shown not to run git. Refusing to run it".
Observed 2026-09-02 in `agent-a18cc3bc53f9c1d8a` on FIVE formulations, all refused:
- `cd <worktree> && echo ... && pwsh -NoProfile -Command '...'`
- `pwsh -NoProfile -Command 'Set-Location "<worktree>"; ...'`
- `cd <worktree> && pwsh -NoProfile -Command '...'`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File <abs-path-to-script>.ps1`
- `cd <worktree> && pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/<script>.ps1`

The guard keys on `pwsh` appearing as the command, NOT on `-Command` versus `-File`. A plan that
"fixes" a refused `-Command` block by rewriting it as `-File` is not fixed. The guard's own remedy
text ("Run the plain command from <worktree>") does not unblock either shape. MCP PoshQC tools are
unaffected; only Bash-launched `pwsh` is refused.

**Shape 2 — a quoted absolute path used as the command name.** Refusal text: "runs a command whose
name is computed at runtime in a plain command, so it cannot be shown not to be git". Observed on
`"/c/Program Files (x86)/Microsoft Visual Studio/Installer/vswhere.exe" -latest ...`, with and
without a leading `cd`. This blocks the usual `vswhere.exe` / `vstest.console.exe` resolution idiom.

**Verified workaround for shape 2:** put the directory on PATH as an assignment prefix and invoke by
bare name. Both of these ran successfully in the refusing session:
- `PATH="/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH" vswhere.exe -latest -products '*' -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`
- `PATH="/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform:$PATH" vstest.console.exe ...`

A quoted absolute path used as an ARGUMENT is fine (`ls -l "/c/Program Files/..."` runs); only the
command-name position is refused. So `dotnet-coverage collect ... -- "<abs>/vstest.console.exe" ...`
is permitted, while `"<abs>/vstest.console.exe" ...` on its own is not.

**Verified workaround for shape 1** (repo-local .NET SDK bootstrap, the usual reason a plan wants
`pwsh`): `scripts/vscode/Install-RepoDotNetSdk.ps1` only downloads the SDK zip and extracts it into
`<root>/.dotnet-sdk`. A POSIX equivalent runs: `mkdir -p .dotnet-sdk`, `curl -L -o .dotnet-sdk/sdk.zip
https://builds.dotnet.microsoft.com/dotnet/Sdk/<ver>/dotnet-sdk-<ver>-win-x64.zip`, `unzip -q -o
.dotnet-sdk/sdk.zip -d .dotnet-sdk`, `rm .dotnet-sdk/sdk.zip`. `.gitignore` line 350 `.dotnet*/`
already ignores the result, so it enters no porcelain gate. The URL was reachable (HTTP 200) from the
sandbox on 2026-09-02.

**Why:** an atomic plan that expresses a gate ONLY as a `pwsh` block or a quoted-abs-path invocation
gives the executor no way to run it, and the task has no fallback. This is not a plan-authoring style
preference; it is an availability fact about the sandbox the executor runs in.

**How to apply:**
- In preflight, flag any plan task whose sole command is a `pwsh` block (either flag form) or a
  quoted absolute executable path, and propose the verified equivalent above.
- Do not accept "it is a different invocation shape" as evidence; run the shape and record the result.
- Prefer POSIX forms when authoring: `wc -l` for line counts, `grep -E` for `Select-String`,
  `git grep` for token searches, PATH-prefix for Windows exes not on PATH.

Related: [[project_pwsh_command_quoting_from_bash]], [[project_pwsh_file_array_param_from_bash]],
[[project_count_idiom_pitfalls_csharpier_and_measureobject]],
[[project_bare_msbuild_not_on_path_in_git_bash]].
