# csproj Registration (P1-T2)

Timestamp: 2026-09-30T13-31
Command: pwsh -NoProfile -Command (line positions of the Race, PrimeFaultOrdering and PrimeRegistration compile entries in TaskMaster.Test\TaskMaster.Test.csproj; exact-entry count; git diff --numstat ANCHOR-SHA -- TaskMaster.Test/TaskMaster.Test.csproj)
EXIT_CODE: 0
Output Summary: The compile entry for Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs was inserted on line 361, immediately after the PrimeFaultOrdering entry (line 360). NEW_COUNT=1; NEW_ENTRY_EXACT=1; RACE_LINE=359 and PFO_LINE=360 equal the P0-T8 values; anchored numstat reports 1 insertion, 0 deletions.

## Observed

- RACE_LINE=359 (RACE-ENTRY-LINE from P0-T8: 359)
- PFO_LINE=360 (PFO-ENTRY-LINE from P0-T8: 360)
- NEW_LINE=361 (PFO_LINE plus 1)
- NEW_COUNT=1
- NEW_ENTRY_EXACT=1
- Numstat against ANCHOR-SHA b305903e275b8abf58e8e65831c189f517568fe4: `1	0	TaskMaster.Test/TaskMaster.Test.csproj`

The project file is excluded from the formatter by .csharpierignore, so no format pass follows this edit.
