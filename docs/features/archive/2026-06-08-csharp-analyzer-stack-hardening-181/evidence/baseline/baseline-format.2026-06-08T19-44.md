# Baseline — Fail-Before Formatting (Cycle 3)

Timestamp: 2026-06-08T19-44

Command: dotnet tool restore; dotnet tool run csharpier check .

EXIT_CODE: 1

Output Summary:
- CSharpier 1.2.6 (restored via dotnet tool restore).
- Repo-wide `csharpier check .` reports EXACTLY ONE unformatted file:
  - `.\ToDoModel.Test\Data Model\ToDo\ToDoItemTests.cs` — "Was not formatted."
- Reported location: Around Line 111. Expected indentation is 8 spaces for the
  commented `//[TestCategory("ProductionBugSuspected")]` line; actual is 7 spaces,
  misaligned with the adjacent `[TestMethod]` and `//[Ignore(...)]` lines.
- Files checked: 1057. Unformatted: 1. No other formatting violations exist.
- This is the pre-fix failing-gate proof for the formatting acceptance criterion;
  it pairs with the pass-after check in P1-T2 / P2-T1.
