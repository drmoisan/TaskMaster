# Baseline — CSharpier Format Check (P0-T11)

Timestamp: 2026-08-27T19-58

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Output Summary: `Checked 1540 files in 5273ms.` The tool reported **0** files as needing formatting:
CSharpier `check` emits one `Error <path>` line plus a diff for every unformatted file and exits
non-zero when the count is greater than zero. Exit code 0 with no `Error` line and no `Warning` line
means the whole tree is already formatted at `BASELINE_SHA`.

Files reported as needing formatting: 0.

Because that count is zero, no offending-path list is required. This also establishes that the Phase 7
repository-wide `csharpier format .` pass has no pre-existing churn to introduce: any file it rewrites
in Phase 7 will be a file this feature itself changed, which is what the Phase 9 scope-lock gate
(P9-T5) requires, since that gate admits only this feature's own `.cs` and `.csproj` paths.
