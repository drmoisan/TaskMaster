# QA Gate: Compile Entry for the New Partial (P1-T4)

Timestamp: 2026-10-01T17-50
Task: P1-T4
Command: git diff --numstat 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85 -- TaskMaster.Test/TaskMaster.Test.csproj
EXIT_CODE: 0

Output Summary:
- Inserted line: `    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs" />` immediately after the PrimeRegistration entry.
- PR_LINE=361 (equals PR-ENTRY-LINE: 361 from P0-T4) NEW_LINE=362 (PR_LINE plus 1) NEW_COUNT=1
- NEW_ENTRY_EXACT=1
- NEW_ENTRY_INDENT_MATCHES=True
- Anchored numstat: `1	0	TaskMaster.Test/TaskMaster.Test.csproj` (1 insertion, 0 deletions); exit 0.
- Line endings preserved: 425 CRLF terminators, 425 LF characters (no bare LF introduced).
- Porcelain span prints exactly the two expected lines (below).
- The project file is outside the formatter (.csharpierignore line 12), so no format pass follows this edit.
- Result: P1-T4 acceptance holds.

## Payload output

```
PR_LINE=361 NEW_LINE=362 NEW_COUNT=1
NEW_ENTRY_EXACT=1
NEW_ENTRY_INDENT_MATCHES=True
1	0	TaskMaster.Test/TaskMaster.Test.csproj
```

## Porcelain

Command: git status --porcelain --untracked-files=all -- TaskMaster TaskMaster.Test

```
 M TaskMaster.Test/TaskMaster.Test.csproj
?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs
```
