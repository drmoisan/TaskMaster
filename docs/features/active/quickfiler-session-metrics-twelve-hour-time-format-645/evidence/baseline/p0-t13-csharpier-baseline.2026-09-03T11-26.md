# P0-T13 — CSharpier Read-Only Baseline (Scoped to Four In-Scope Files)

Timestamp: 2026-09-03T11-26
Command: dotnet tool run csharpier check <four in-scope files, absolute paths>
(invoked via the item worktree's pinned .dotnet-sdk/dotnet.exe by absolute path; the four target
files were also passed as absolute paths, so the file arguments unambiguously resolve to the
item worktree regardless of process working directory)
EXIT_CODE: 0
Output Summary: "Checked 4 files in 1172ms." No unformatted files reported; all four in-scope
files are already CSharpier-clean at baseline (pre-existing drift state: none).
