# Fixture partial line counts (P2-T9)

Timestamp: 2026-10-03T09-31
Command: CMD-LINECOUNT (Get-ChildItem over TaskMaster.Test\Ribbon filtered to EngineToggleStateCoordinatorTests*.cs, Get-Content line counts)
EXIT_CODE: 0
Output Summary: 7 partials, all at most 500 lines; SinkGuard 210 (from 169); primary fixture 481 (unchanged).

LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 481
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs = 290
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs = 210
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 215
TEST-PARTIALS: 7
SINKGUARD-LINES-FINAL: 210 (equals SINKGUARD-LINES-AFTER: 210 of P1-T4)
