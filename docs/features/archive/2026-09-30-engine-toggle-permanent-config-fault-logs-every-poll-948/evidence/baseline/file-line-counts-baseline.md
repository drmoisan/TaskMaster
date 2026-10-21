# File Line Counts Baseline (P0-T18)

Timestamp: 2026-10-01T23-33
Command: CMD-LINECOUNT; CMD-HASH
EXIT_CODE: 0
Output Summary: production file 476 lines; five existing partials at their P0-T7 anchor counts; PARTIAL-COUNT 5 equals ANCHOR-PARTIAL-COUNT; ANCHOR-HASH-PROD recorded; the RepeatFaultSuppression partial is ABSENT.

```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 476
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 470
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 215
PARTIAL-COUNT: 5
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs = ABSENT
```

ANCHOR-HASH-PROD: EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF

Every LINES value equals its ANCHOR-LINES-*: value from P0-T7, and PARTIAL-COUNT (5) equals ANCHOR-PARTIAL-COUNT (5).
