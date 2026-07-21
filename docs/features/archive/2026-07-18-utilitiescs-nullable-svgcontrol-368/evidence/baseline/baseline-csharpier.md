# Baseline CSharpier Formatting State

Timestamp: 2026-07-19T00-10

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Output Summary: `Checked 1406 files in 5378ms.` No files require formatting changes; repository is
already CSharpier-clean at baseline. (First invocation returned exit 1 because the repo-local
.NET SDK and NuGet packages had not yet been bootstrapped in this fresh worktree; after running
`scripts/vscode/Install-RepoDotNetSdk.ps1` and `scripts/vscode/Invoke-Restore.ps1`, the command
succeeded cleanly on the next run — this bootstrap step is environment setup, not a
formatting finding.)
