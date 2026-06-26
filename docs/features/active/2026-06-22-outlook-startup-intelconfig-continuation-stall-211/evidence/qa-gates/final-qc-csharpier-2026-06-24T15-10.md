# Final QC — CSharpier (issue #211)

Timestamp: 2026-06-24T15-10

Command: `dotnet tool run csharpier .` (verified via `dotnet tool run csharpier check .`; per repo
convention `check` is used repo-wide so that `format` does not reformat `*.csproj` project files,
which the repo keeps in Visual Studio format. The new/modified `.cs` files were already brought to
CSharpier format during their respective phases.)

EXIT_CODE: 0

Output Summary:
- Result: PASS. `Checked 1100 files in 3212ms.` No formatting violations; no files changed.
- File count rose from 1095 (baseline) to 1100, reflecting the five new files
  (`SpamBayes.Conditions.cs`, `SpamBayes.Actions.cs`, `SpamBayes.Classify.cs`,
  `SpamInitTimingProbe.cs`, `SpamInitTimingProbeTests.cs`).
