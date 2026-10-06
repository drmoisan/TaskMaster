# Pin-count test file census (issue #968, tasks P1-T1, P1-T2 and P1-T3)

Timestamp: 2026-10-03T02-56
Command: pwsh -NoProfile -Command '<CMD-EOL payload>' with FILE = `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs` (PC), the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted; followed by CMD-TOKEN-COUNT on PC and on PROJ (separate payloads) and two git calls
Canonical command: CMD-EOL on PC; CMD-TOKEN-COUNT on PC and PROJ; git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test/QuickFiler.Test.csproj; git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (every payload)
- P1-T1 CMD-EOL: BARE_LF: 0; CRLF_COUNT: 248; LINES: 248 (CRLF_COUNT equals LINES; at most 500 and at least 200)
- P1-T2: exactly one project-file line contains `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs`; it reads `    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs" />` (four leading spaces) and the line before it is the fixture-tests item `    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />`
- PC tokens (task order): 10, 1, 3, 1, 1, 4, 4, 1, 3, 1, 4, 1, 2, 2, 0, 0, 0, 1, 1 (all as expected)
- PROJ tokens: `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs` 1, `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` 1
- git diff --numstat HEAD -- QuickFiler.Test/QuickFiler.Test.csproj (exit 0): `1	0	QuickFiler.Test/QuickFiler.Test.csproj`
- git status --porcelain -- QuickFiler QuickFiler.Test (exit 0):

PHASE1-PORCELAIN:
```
 M QuickFiler.Test/QuickFiler.Test.csproj
?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs
```
