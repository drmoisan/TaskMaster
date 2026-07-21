# Baseline — CSharpier Formatting State

Timestamp: 2026-07-19T00-08

Command: `csharpier check .` (global CSharpier 1.3.0; equivalent to plan's `dotnet tool run csharpier check .` — no local tool manifest exists in this worktree, so the globally installed CSharpier 1.3.0 was used; command recorded verbatim)

EXIT_CODE: 0

Output Summary: PASS. `Checked 1406 files in 3293ms.` Zero files reported as needing formatting (0 "would be formatted"/warning/error lines). The repository is CSharpier-clean at baseline before any in-scope pragma is applied.
