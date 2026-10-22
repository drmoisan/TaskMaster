# File Line Counts and Registrations (P1-T26, AC6)

Timestamp: 2026-10-03T08-07
Task: P1-T26
Command: CMD-LINECOUNT; CMD-HASH; Grep tool count of `Ribbon\x5CEngineToggleStateCoordinator\.` over TaskMaster/TaskMaster.csproj; Grep tool count of `Ribbon\x5CEngineToggleStateCoordinatorTests` over TaskMaster.Test/TaskMaster.Test.csproj
EXIT_CODE: 0

Output Summary:
- PRODUCTION-FILES: 3; production LINES 302 (main), 197 (Prime), 86 (Messages); every value at most 450 (plan expectation about 307, 196 and 86 was an observation only).
- TEST-PARTIALS: 7; test LINES 481 (primary), 77, 175, 277 (Race), 290, 169 (SinkGuard), 215; every value at most 500.
- TaskMaster/TaskMaster.csproj: 3 coordinator compile entries (lines 466, 467, 468), one per production LINES row.
- TaskMaster.Test/TaskMaster.Test.csproj: 7 fixture entries (lines 352, 359 to 364), one per test LINES row.
- Verdict: PASS (no FILE SIZE CEILING EXCEEDED).

CMD-LINECOUNT output:
```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 302
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs = 86
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.Prime.cs = 197
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 481
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs = 290
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs = 169
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 215
PRODUCTION-FILES: 3
TEST-PARTIALS: 7
```

Csproj registrations (Grep tool, matching lines):
```
TaskMaster/TaskMaster.csproj:466:    <Compile Include="Ribbon\EngineToggleStateCoordinator.cs" />
TaskMaster/TaskMaster.csproj:467:    <Compile Include="Ribbon\EngineToggleStateCoordinator.Messages.cs" />
TaskMaster/TaskMaster.csproj:468:    <Compile Include="Ribbon\EngineToggleStateCoordinator.Prime.cs" />
TaskMaster.Test/TaskMaster.Test.csproj:352:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.cs" />
TaskMaster.Test/TaskMaster.Test.csproj:359:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.Race.cs" />
TaskMaster.Test/TaskMaster.Test.csproj:360:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs" />
TaskMaster.Test/TaskMaster.Test.csproj:361:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs" />
TaskMaster.Test/TaskMaster.Test.csproj:362:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs" />
TaskMaster.Test/TaskMaster.Test.csproj:363:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs" />
TaskMaster.Test/TaskMaster.Test.csproj:364:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs" />
```

PHASE1-HASHES:
```
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 8836AEFB84FA685EAD4EFB993C3A0E1492EB21606A06CF0B2FE1497E872E4EF4
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.Prime.cs = 50DC74A866D6310184EA89C2773875994D9C73EEB3DF11B63244A80B80AC52CD
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs = 1E52B69D775038726117DEB0A63AEAC301A13E2FA5B9C96E6B00CC63046159AC
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 3E905F281D4493F289783EDE8BC9C98127547349CDDBB52E5D6DC3D692604211
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs = D172DA1C8E2596198AC4D69EDFD10B584505BC24FBBBBC5604C03F21B63B0F9D
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 7B372E1FED2F271E3A67CE14AA0CAF3A1B5E6E4C08A703E6F96C54C57776595C
```
