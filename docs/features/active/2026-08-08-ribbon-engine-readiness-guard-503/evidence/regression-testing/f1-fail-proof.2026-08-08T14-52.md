# F1 — Recorded Fail-Proof: the Corrected Assertion Fails on the Mutated Resource (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T7] **[expect-fail]**
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation /Settings:TaskMaster.runsettings /TestCaseFilter:'FullyQualifiedName~TaskMaster.Test.Ribbon.RibbonExplorerXmlTests'"`
EXIT_CODE: **1**

A **non-zero** exit code is the expected and required outcome of this task, and of this task only. It is the executable half of the F1 proof: it demonstrates that the corrected assertion genuinely fails on the condition it names, rather than merely appearing to assert it.

## Proof that the assembly under test carried the mutation

Cross-reference: `evidence/regression-testing/f1-mutated-assembly.2026-08-08T14-52.md` records `EMBEDDED_GETENABLED_COUNT=7` read directly out of `TaskMaster.Test\bin\Debug\TaskMaster.dll`, with `EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T19:04:58.9399552Z` advancing past the pre-mutation value `2026-08-08T18:57:19.8234515Z`. The failure recorded below therefore cannot be attributed to a stale assembly; the resource the test read was the mutated one.

Control: `evidence/regression-testing/f1-green-before-mutation.2026-08-08T14-52.md` records the identical command passing 8/8 against the same corrected test with the **unmutated** resource. The failure is attributable to the mutation, not to the P1-T1 edit.

## Output Summary

```text
Test Run Failed.
Total tests: 8
     Passed: 6
     Failed: 2
```

| Metric | Value |
|---|---|
| Total | 8 |
| Passed | 6 |
| **Failed** | **2** |

### The AC5 test — reported Failed

`RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` is reported **Failed**. Verbatim failure message, isolated by re-running the single test under `/TestCaseFilter:'FullyQualifiedName=...'`:

```text
  Failed RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback [166 ms]
  Error Message:
   Expected getEnabled not to be <null> because control 'TrainSpam' is engine-backed and must declare a getEnabled callback.
  Stack Trace:
     at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message) in /_/Src/FluentAssertions/Execution/LateBoundTestFramework.cs:line 22
   at FluentAssertions.Execution.AssertionChain.FailWith(Func`1 getFailureReason) in /_/Src/FluentAssertions/Execution/AssertionChain.cs:line 277
   at FluentAssertions.Primitives.ReferenceTypeAssertions`2.NotBeNull(String because, Object[] becauseArgs) in /_/Src/FluentAssertions/Primitives/ReferenceTypeAssertions.cs:line 71
   at TaskMaster.Test.Ribbon.RibbonExplorerXmlTests.RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback() in C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs:line 202
```

The stack frame names `ReferenceTypeAssertions.NotBeNull` at `RibbonExplorerXmlTests.cs:line 202` — the exact assertion introduced by P1-T1. The failure message carries the `because` reason naming the specific control (`TrainSpam`), so the diagnostic identifies which of the eight controls regressed.

This is decisive. **Before** the P1-T1 change, the identical mutation would have produced a silent pass: `Attribute("getEnabled")` returns `null`, the `?.` short-circuits the entire chain including `.Should()`, and no assertion executes. The test is now non-vacuous on condition 1 of the three required conditions, and conditions 2 and 3 (wrong value, empty value) both route through `getEnabled!.Value.Should().Be(...)`, which is reached unconditionally once the attribute is present.

### Other test that failed as a consequence of the same mutation

`RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` (the AC6 sibling set-equality test) also failed:

```text
  Failed RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls [66 ms]
  Error Message:
   Expected declaringIds to contain exactly 8 items in any order because only the engine-backed controls may be disabled by the readiness callback, but it misses {"TrainSpam"}
```

This failure is **expected and correct**. That test was already non-vacuous before this cycle — it is the criterion that kept AC5 genuinely enforced while the AC5 test itself was vacuous, as recorded in `code-review.2026-08-08T14-15.md`. Its failure here confirms the mutation reached the assembly through a second independent route.

### Tests that passed

`RibbonExplorerXml_IsWellFormedXml`, `RibbonExplorerXml_MenusContainOnlyMenuLegalControls`, `RibbonExplorerXml_TaskMasterGroupsLiveUnderTaskmasterTab`, `RibbonExplorerXml_TabMailCarriesNoCustomGroup`, `RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled`, and `RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer` all passed. None of them asserts the presence of the attribute, so none is sensitive to this mutation. That is the correct blast radius.

## Restoration

The mutation is restored by P1-T8 (`f1-mutation-restored.2026-08-08T14-52.md`). The permanent tree retains no part of it.

Binary outcome satisfied: `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` is reported **Failed**, with a non-zero exit code and a verbatim failure message naming the `NotBeNull` assertion at line 202.
