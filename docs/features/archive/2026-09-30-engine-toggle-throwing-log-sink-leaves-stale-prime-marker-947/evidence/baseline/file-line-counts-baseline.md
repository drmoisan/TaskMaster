# Baseline: File Hashes and Line Counts (P0-T15)

Timestamp: 2026-10-01T17-43
Task: P0-T15
Command: CMD-HASH (SHA-256 of the production file, the ThrowingSink partial and the PrimeFaultOrdering partial); CMD-LINECOUNT (six coordinator source files)
EXIT_CODE: 0

Output Summary:
- BASE-HASH-PROD: B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
- BASE-HASH-PFO: AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB
- ThrowingSink partial: ABSENT in both commands
- LINES: production 442, main fixture 470, Race 277, PrimeFaultOrdering 77, PrimeRegistration 175 (each equals its BASE-LINES-* value from P0-T4)
- Result: P0-T15 hash and line-count clauses hold.

## CMD-HASH

```
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = ABSENT
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB
```

## CMD-LINECOUNT

```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 442
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 470
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = ABSENT
```
