# Protected Regions Unchanged (P2-T7)

Timestamp: 2026-09-30T13-39
Command: CMD-REGION-COMPARE with LEFT ANCHOR-SHA (b305903e275b8abf58e8e65831c189f517568fe4), RIGHT WORKING and region set PROTECTED; CMD-REGION-COMPARE with LEFT ANCHOR-SHA, RIGHT WORKING and region set EDIT-WINDOWS; git diff --exit-code ANCHOR-SHA -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs TaskMaster/Ribbon/RibbonController.EngineCommands.cs TaskMaster/TaskMaster.csproj; git diff --exit-code ANCHOR-SHA -- TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings
EXIT_CODE: 0
Output Summary:
PROTECTED: HEAD, PRESSED-STATE, PRIMETASKS-DECLARATION, MIDDLE, GETPRIMETASK, APPLYPRIME-AND-COMPLETEPRIME and TAIL each print equal=True.
EDIT-WINDOWS: GATE-AND-TASKS-FIELDS and PRIME-START each print equal=False (positive control: the comparison detects the edits it confines).
No row prints TOKEN-MISSING.
PROTECTED_FILES_DIFF_EXIT=0; RUNSETTINGS_DIFF_EXIT=0.
Verdict: CompletePrime (summary, remarks, body and its _primeTasks.TryRemove(engineName, out _); statement) and ApplyPrimeAsync are identical to the re-anchored origin/main; GetPrimeTask is untouched (D-3); the _primeTasks declaration is unchanged; every P2-T7 acceptance clause holds.

Substitutions recorded: (1) Plan correction C1 (orchestrator-accepted): the SHA-256 hasher was created with New-Object System.Security.Cryptography.SHA256Managed instead of the static factory call in the CMD-REGION-COMPARE text, because the pr-author PreToolUse hook refuses a command containing that factory call; the digest algorithm is the same. (2) Both region sets were evaluated in one pwsh invocation (a loop over the two sets, sharing the same Get-SideLines and Get-Region functions and the same LEFT and RIGHT sides), and the two exit-code lines were printed by string concatenation rather than by an interpolated string; the git commands and computed values are unchanged.

## Details

```
SET PROTECTED
REGION HEAD left=1-57 right=1-57 equal=True
REGION PRESSED-STATE left=62-70 right=62-70 equal=True
REGION PRIMETASKS-DECLARATION left=77-80 right=78-81 equal=True
REGION MIDDLE left=81-236 right=82-237 equal=True
REGION GETPRIMETASK left=237-258 right=238-259 equal=True
REGION APPLYPRIME-AND-COMPLETEPRIME left=307-361 right=329-383 equal=True
REGION TAIL left=362-420 right=384-442 equal=True
SET EDIT-WINDOWS
REGION GATE-AND-TASKS-FIELDS left=58-80 right=58-81 equal=False
REGION PRIME-START left=259-306 right=260-328 equal=False
PROTECTED_FILES_DIFF_EXIT=0
RUNSETTINGS_DIFF_EXIT=0
```

## POST-FORMAT:

P3-T2, pass 1, Timestamp: 2026-09-30T13-44. Command: the P2-T7 commands (CMD-REGION-COMPARE LEFT ANCHOR-SHA RIGHT WORKING for PROTECTED and EDIT-WINDOWS, with plan correction C1: New-Object System.Security.Cryptography.SHA256Managed; the two protected-file git diff --exit-code commands), re-run on the tree after the P3-T1 repository-wide format. EXIT_CODE: 0.

Output Summary: every P2-T7 clause holds on the post-format tree: all seven PROTECTED rows equal=True, both EDIT-WINDOWS rows equal=False, no TOKEN-MISSING, PROTECTED_FILES_DIFF_EXIT=0, RUNSETTINGS_DIFF_EXIT=0. CompletePrime (summary, remarks, body, including its _primeTasks.TryRemove(engineName, out _); statement) and ApplyPrimeAsync are identical to the re-anchored origin/main, GetPrimeTask is untouched, and the _primeTasks declaration is unchanged.

```
SET PROTECTED
REGION HEAD left=1-57 right=1-57 equal=True
REGION PRESSED-STATE left=62-70 right=62-70 equal=True
REGION PRIMETASKS-DECLARATION left=77-80 right=78-81 equal=True
REGION MIDDLE left=81-236 right=82-237 equal=True
REGION GETPRIMETASK left=237-258 right=238-259 equal=True
REGION APPLYPRIME-AND-COMPLETEPRIME left=307-361 right=329-383 equal=True
REGION TAIL left=362-420 right=384-442 equal=True
SET EDIT-WINDOWS
REGION GATE-AND-TASKS-FIELDS left=58-80 right=58-81 equal=False
REGION PRIME-START left=259-306 right=260-328 equal=False
PROTECTED_FILES_DIFF_EXIT=0
RUNSETTINGS_DIFF_EXIT=0
```
