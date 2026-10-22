# Baseline — dotnet tool restore Bootstrap (Issue #656)

Timestamp: 2026-09-01T14-37
Task: [P0-T5]

Command:
```
dotnet tool restore
```
(run from the worktree root)

EXIT_CODE: 0

Results:

- `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`
- `Restore was successful.`
- The manifest is `dotnet-tools.json` at the worktree root and pins `csharpier` to `1.2.6`. Every
  formatting command in this plan is issued through `dotnet tool run` so the manifest-pinned version
  is the one that runs, matching the CI format step.

Output Summary: Bootstrap succeeded. CSharpier 1.2.6 restored from the local tool manifest. This is
a bootstrap step, not a toolchain gate.
