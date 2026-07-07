# Final CSharpier Run — Issue #251 (Iteration 1)

Timestamp: 2026-07-06T23-58

Command: dotnet tool run csharpier format .

EXIT_CODE: 0

Output Summary: `Formatted 1276 files in 1502ms.` CSharpier reformatted `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` (a single line-wrap change on the `mockHomeController.SetupGet(...)` line). Per the Phase 2 restart rule, this run changed a file, so the loop is restarted from P2-T1. See `csharpier-final-iteration2.2026-07-06T23-08.md` for the clean rerun.
