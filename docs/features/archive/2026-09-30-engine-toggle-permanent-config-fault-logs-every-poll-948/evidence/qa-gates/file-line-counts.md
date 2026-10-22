# File Line Counts (P3-T3)

Timestamp: 2026-10-02T00-10
Command: CMD-LINECOUNT
EXIT_CODE: 0
Output Summary: production file 496 lines and RepeatFaultSuppression partial 290 lines, each strictly under 500; the production count exceeds MERGE-BASE-LINES 476 (equals 476 plus EXPECTED-DELTA 20); every existing partial equals its P0-T7 anchor count; PARTIAL-COUNT 6 equals ANCHOR-PARTIAL-COUNT 5 plus 1.

Pass: 1

```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 496
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 470
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs = 290
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 215
PARTIAL-COUNT: 6
```

| File | Lines | Anchor (P0-T7) | Check |
|---|---|---|---|
| EngineToggleStateCoordinator.cs | 496 | 476 | < 500 and > 476 |
| EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs | 290 | absent | < 500 |
| EngineToggleStateCoordinatorTests.cs (primary fixture) | 470 | 470 | equal |
| EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs | 77 | 77 | equal |
| EngineToggleStateCoordinatorTests.PrimeRegistration.cs | 175 | 175 | equal |
| EngineToggleStateCoordinatorTests.Race.cs | 277 | 277 | equal |
| EngineToggleStateCoordinatorTests.ThrowingSink.cs | 215 | 215 | equal |

This is the authoritative AC-P size audit.
