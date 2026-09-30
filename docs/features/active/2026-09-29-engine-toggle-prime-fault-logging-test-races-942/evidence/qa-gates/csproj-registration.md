# Test project registration of the new partial (issue 942)

Timestamp: 2026-09-30T07-36
Task: P1-T3
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $p = Get-Content -LiteralPath "TaskMaster.Test\TaskMaster.Test.csproj" -Encoding UTF8; ... "RACE_LINE=$race NEW_LINE=$new NEW_COUNT=..."; git diff --numstat 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- TaskMaster.Test/TaskMaster.Test.csproj'
EXIT_CODE: 0

Output Summary:
- Inserted `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs" />` immediately after the Race entry, with the same four-space indentation.
- RACE_LINE=359 NEW_LINE=360 NEW_COUNT=1
- NEW_LINE equals RACE_LINE plus 1.
- Anchored numstat: `1	0	TaskMaster.Test/TaskMaster.Test.csproj` (1 insertion, 0 deletions).
- Line endings after the edit: 423 CRLF of 423 LF (consistent CRLF).
- The project file is outside the formatter (.csharpierignore), so no format pass follows this edit.
