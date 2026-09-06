---
name: bash-cwd-resets-use-env-dash-c
description: Agent-thread Bash resets cwd between calls so a standalone `cd` does not persist; `env -C <dir> <cmd>` runs a plan's relative-path command from the worktree root with no `cd` and no chaining
metadata:
  type: project
---

In an agent thread the Bash tool resets its working directory to the session root between calls. A
standalone `cd <worktree>` followed by `pwd` in the next call reports the session root again, so the
"working directory persists between calls" note in the tool description does not hold here.

Use `env -C "<ABSOLUTE WORKTREE PATH>" <command...>` as a uniform prefix instead. It is a single
unchained command, uses no `cd`, and satisfies a delegation's "never use `cd`" discipline literally
while still running the command from the worktree root.

**Why:** Atomic plans are written with relative operands (`TaskMaster.sln`,
`UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`, `> TestResults/x.diff`) and say "run from the
worktree root". Rewriting them all to absolute paths would violate execute-verbatim AND break
acceptance clauses that assert the recorded command line. `dotnet` is the sharpest case: it resolves
`global.json` from cwd, so a bare `dotnet --version` from the wrong worktree exits 155 with
`The repo-local .NET SDK is missing` even when the item worktree has a working `.dotnet-sdk/`.

**How to apply:** Prefix every plan command block with `env -C <worktree> `, place it BEFORE any
`MSYS_NO_PATHCONV=1` or `PATH=` assignment, and record the prefix in the evidence artifact's
`Command:` field. Well-written plans already word command-line gates as "contains
`msbuild.exe TaskMaster.sln`" rather than "begins with", which accommodates the prefix. Combine with
[[project_pwsh_command_quoting_from_bash]] and
[[project_msys_slash_switch_conversion_rule]].
