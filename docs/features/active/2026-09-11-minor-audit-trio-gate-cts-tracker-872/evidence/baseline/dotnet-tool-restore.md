# Phase 0 — dotnet tool restore

Timestamp: 2026-09-13T14-50
Task: [P0-T3]

Command: dotnet tool restore
EXIT_CODE: 0

CSharpierVersion: 1.2.6

The version above is read from the tools manifest at the repository root, file name dotnet-tools.json,
whose csharpier entry declares version 1.2.6 with rollForward false. It is not inferred from the
restore output alone.

Output Summary: the run printed the restored-tool line
`Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` and the line
`Restore was successful.`, and exited 0. Both lines are quoted here because the exit code alone is not
the observation: the command exits 0 whether or not it installed anything, so the restored-tool line
and the success line are the falsifiable signal that the manifest-pinned tool is present. The version
in the restored-tool line agrees with the manifest version recorded above.

## Re-Run Note, Per D15

This artifact overwrites a superseded capture taken before the main branch carrying the fix for issue
#877 was merged into this branch. The merge changed no tool manifest entry, and the re-measured
version is unchanged at 1.2.6. The restore is re-run rather than assumed because the merge changed the
tracked tree and a restore is cheap to re-verify.

## Invocation Note

The command was issued inside a single pwsh invocation whose first statement sets the location to this
worktree root, so the manifest resolved is this worktree's manifest and not that of any sibling
worktree. The build lock was held across this single command and released immediately afterwards.
