# QA Gate: Edits Confined to Windows E1 to E6; Protected Files Unchanged (P1-T10)

Timestamp: 2026-10-01T17-55
Task: P1-T10
Command: CMD-WINDOWS (git show 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85:TaskMaster/Ribbon/EngineToggleStateCoordinator.cs; git diff -U0 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85 -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs); git diff --exit-code 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85 -- <seven protected files>; git diff --exit-code 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85 -- TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings
EXIT_CODE: 0

Output Summary:
- The six WINDOW lines equal BASE-WINDOW-E1: to BASE-WINDOW-E6: from P0-T3 (154-155, 246-248, 295-301, 351-382, 167-168, 182-185).
- WINDOW-E1-LINE2-IS-CATCH-CLAUSE: True; WINDOW-E4-STARTS-AT-SUMMARY: True; WINDOW-E6-LINE3-IS-TOGGLE-SINK-CALL: True
- HUNK-COUNT: 10 (at least 6); every HUNK line names a window.
- HUNKS-OUTSIDE-WINDOWS: 0
- WINDOWS-TOUCHED: E1,E2,E3,E4,E5,E6 (positive control: each edit landed)
- PROTECTED_FILES_DIFF_EXIT=0 (EngineToggleStateCoordinatorTests.cs, .Race.cs, .PrimeFaultOrdering.cs, .PrimeRegistration.cs, RibbonController.EngineCommands.cs, RibbonCommandBoundary.cs, TaskMaster.csproj)
- RUNSETTINGS_DIFF_EXIT=0
- Result: P1-T10 acceptance holds; no EDIT OUTSIDE WINDOW.

## CMD-WINDOWS output

```
WINDOW E1 = 154-155
WINDOW E2 = 246-248
WINDOW E3 = 295-301
WINDOW E4 = 351-382
WINDOW E5 = 167-168
WINDOW E6 = 182-185
WINDOW-E1-LINE2-IS-CATCH-CLAUSE: True
WINDOW-E4-STARTS-AT-SUMMARY: True
WINDOW-E6-LINE3-IS-TOGGLE-SINK-CALL: True
HUNK-COUNT: 10
HUNK @@ -154,2 +154,2 @@ namespace TaskMaster base=154-155 window=E1
HUNK @@ -167,2 +167,5 @@ namespace TaskMaster base=167-168 window=E5
HUNK @@ -184 +187,8 @@ namespace TaskMaster base=184-184 window=E6
HUNK @@ -247,2 +257,3 @@ namespace TaskMaster base=247-248 window=E2
HUNK @@ -295,2 +306,3 @@ namespace TaskMaster base=295-296 window=E3
HUNK @@ -301 +313,2 @@ namespace TaskMaster base=301-301 window=E3
HUNK @@ -355 +368,2 @@ namespace TaskMaster base=355-355 window=E4
HUNK @@ -357,0 +372 @@ namespace TaskMaster base=357-357 window=E4
HUNK @@ -364,0 +380,10 @@ namespace TaskMaster base=364-364 window=E4
HUNK @@ -378,3 +403,12 @@ namespace TaskMaster base=378-380 window=E4
HUNKS-OUTSIDE-WINDOWS: 0
WINDOWS-TOUCHED: E1,E2,E3,E4,E5,E6
```

## Protected-file diffs

```
PROTECTED_FILES_DIFF_EXIT=0
RUNSETTINGS_DIFF_EXIT=0
```

Note: git's minimal diff splits several edits into narrower hunks than the windows (for example E6 shows as one replaced base line 184 plus inserted lines, because the clause header and braces are retained text); every hunk's base range still lies inside its window.
