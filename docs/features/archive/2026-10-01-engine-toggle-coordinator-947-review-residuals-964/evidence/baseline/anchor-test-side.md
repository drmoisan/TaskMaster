# Test-Side Anchor (P0-T5)

Timestamp: 2026-10-03T07-35
Task: P0-T5
Command: CMD-LINECOUNT; Grep tool -n over TaskMaster.Test/TaskMaster.Test.csproj for `EngineToggleStateCoordinatorTests` and over TaskMaster/TaskMaster.csproj for `EngineToggleStateCoordinator`; Grep tool counts over TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests*.cs for `\[TestMethod\]`, `\[DataTestMethod\]`, `\[DataRow\(`, `\[TestClass\]`, `OnNotify`; Grep tool over TaskMaster.Test for each NEW-NAMES-964 name
EXIT_CODE: 0

Output Summary:
- PRODUCTION-FILES: 1; TEST-PARTIALS: 6.
- LINES: production 496; primary 470, PrimeFaultOrdering 77, PrimeRegistration 175, Race 277, RepeatFaultSuppression 290, ThrowingSink 215.
- Test csproj fixture entries: 6 (lines 352, 359, 360, 361, 362, 363); production csproj coordinator entry: 1 (line 466).
- [TestMethod] 36; [DataTestMethod] 1; [DataRow( 3; [TestClass] 1.
- EXPECTED-CASES: 39
- OnNotify count: 0. NEW-NAMES-964 hits: 0 for every name.
- No EngineToggleStateCoordinatorTests.SinkGuard.cs, EngineToggleStateCoordinator.Prime.cs or EngineToggleStateCoordinator.Messages.cs exists (CMD-LINECOUNT enumeration lists none).
- Verdict: PASS (no TEST ANCHOR MOVED).

Details:

CMD-LINECOUNT output:
```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 496
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 470
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs = 290
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 215
PRODUCTION-FILES: 1
TEST-PARTIALS: 6
```

TaskMaster.Test/TaskMaster.Test.csproj (Grep -n):
```
352:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.cs" />
359:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.Race.cs" />
360:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs" />
361:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs" />
362:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs" />
363:    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs" />
```

TaskMaster/TaskMaster.csproj (Grep -n):
```
466:    <Compile Include="Ribbon\EngineToggleStateCoordinator.cs" />
```

[TestMethod] per file: ThrowingSink 4, RepeatFaultSuppression 7, Race 6, PrimeFaultOrdering 1, PrimeRegistration 3, primary 15 (total 36). [DataTestMethod], [DataRow( (3) and [TestClass] all in the primary file.

NEW-NAMES-964 search over TaskMaster.Test (one alternation pattern of the four names): 0 matches.
