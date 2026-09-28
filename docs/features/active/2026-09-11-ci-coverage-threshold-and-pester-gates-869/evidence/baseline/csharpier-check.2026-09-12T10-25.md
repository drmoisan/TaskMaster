# Phase 0 — Baseline C# formatter state (P0-T17)

Timestamp: 2026-09-14T18-19

Purpose: establish the tree's formatter baseline so that the final formatter check in P10-T7 is judged against this tree rather than against an absolute expectation the delivery does not control.

Command: `pwsh -NoProfile -Command '<worktree prologue>; dotnet tool run csharpier check .'`
EXIT_CODE: 0

The command was issued through `pwsh` because the Bash allowlist grants no bare `dotnet` entry. It was invoked through `dotnet tool run` so that the manifest-pinned CSharpier 1.2.6 restored in P0-T5 is used rather than the globally installed 1.3.0, per the C# code change policy in CLAUDE.md. The worktree prologue is load-bearing here for the same reason it is on `dotnet tool restore`: CSharpier resolves the manifest by searching from the working directory upward, and the `.` argument is likewise resolved against the working directory.

## Output, verbatim

```
Checked 1639 files in 5246ms.
```

## Recorded observations

- Count of files the tool reported as checked: **1639**.
- Full list of files the tool reported as unformatted: **empty**. The tool reported none.

The `check` subcommand is read-only and rewrites no file, so its exit code together with its printed file list is a sufficient observation and no before-and-after tree comparison is required.

The baseline exit code is 0, so the branch of this task that records a non-zero baseline exit code without treating it as a failure does not apply. P10-T7 is judged against the recorded file list above: its own unformatted list must be identical to this one, namely empty, so the delivery must introduce no new formatting difference. The file count is recorded as context; a change in it would follow from a file this delivery adds or removes, and the only such candidates are the two contingency fixture files named by P6-T7, of which `packages.config` is processed by CSharpier and `SyncFixture.Test.csproj` is excluded by the `.csharpierignore` entry for project files.

Output Summary: CSharpier 1.2.6 checked 1639 files and reported no unformatted file, exiting 0. The baseline unformatted list is empty.
