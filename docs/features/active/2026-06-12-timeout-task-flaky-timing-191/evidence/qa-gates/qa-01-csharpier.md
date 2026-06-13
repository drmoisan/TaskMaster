# QA-01 — CSharpier Formatting

Timestamp: 2026-06-13T00-35

Command: dotnet tool run csharpier format .  (then dotnet tool run csharpier check on the two changed .cs files)
EXIT_CODE: 0

Output Summary:
- `csharpier format .` formatted 1060 files in 1119 ms, EXIT 0.
- The two in-scope changed source files pass `csharpier check` with no changes required:
  `csharpier check UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` -> "Checked 2 files in 479ms.", EXIT 0.
- The two intended `.cs` edits are csharpier-clean; no `.cs` reformatting was produced by the change.

Finding (recorded, out-of-scope side effect handled): The installed CSharpier is v1, which now formats XML project files (`*.csproj`) by default. `csharpier format .` reformatted 8 project files (QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskMaster, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test .csproj). This conflicts with the repository's documented intent — `.prettierignore` excludes `*.csproj`/`*.props`/`*.targets` ("Leave *.csproj / *.props / *.targets to VS") and CLAUDE.md C#1 states csharpier "formats only `*.cs` without touching project files" (a v0 assumption). Those project-file changes are outside this test-only task's scope (max 2 test files, 0 production files) and were reverted via `git checkout -- <csproj paths>`. Only the two intended test `.cs` files remain modified. The format gate is satisfied for the in-scope files via `csharpier check`.

Note for follow-up (not actioned here to stay in scope): the repo's `.csharpierignore` does not list `*.csproj`/`*.props`/`*.targets`; adding them would prevent CSharpier v1 from reformatting project files. This is a separate change outside this bug's scope.
