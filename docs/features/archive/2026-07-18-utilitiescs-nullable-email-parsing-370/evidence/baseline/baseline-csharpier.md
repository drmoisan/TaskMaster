# Baseline CSharpier Formatting Check

Timestamp: 2026-07-19T00-20

Command: `dotnet tool run csharpier -- check .`

EXIT_CODE: 0

Output Summary: `Checked 1406 files in 4487ms.` No files require formatting changes. CSharpier
formatting baseline is clean (0 files needing reformat) prior to any edit in this feature.

Note: this run required a one-time environment bootstrap first (repo-local .NET SDK was
missing on this fresh worktree): `pwsh ./scripts/vscode/Install-RepoDotNetSdk.ps1` followed by
`pwsh ./scripts/vscode/Invoke-Restore.ps1` (NuGet restore, 169 packages, build succeeded, 1
pre-existing NU1902 advisory warning on `AngleSharp` 1.4.0, unrelated to this feature).
