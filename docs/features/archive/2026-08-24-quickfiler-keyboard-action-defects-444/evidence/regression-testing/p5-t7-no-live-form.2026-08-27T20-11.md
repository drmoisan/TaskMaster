# [P5-T7] Assembly-hygiene guard test

Timestamp: 2026-08-27T20-11
Command: `& $vstest .\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~NoLiveFormInTestAssemblyTests"`
EXIT_CODE: 0
Output Summary: `Test Run Successful.` `Total tests: 1`, `Passed: 1`, `Failed: 0`. The single test
`ExecutingAssembly_ContainsNoFormDerivedType` passed in 41 ms.

## Result

```
A total of 1 test files matched the specified pattern.
  Passed ExecutingAssembly_ContainsNoFormDerivedType [41 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.1970 Seconds
```

| Measure | Value |
| --- | --- |
| Total | 1 |
| Passed | 1 |
| Failed | **0** |
| Skipped | 0 |

`/InIsolation` is used, matching CI's invocation form: without it the Moq assemblies in this test
project can fail to load in the shared test host and produce empty-message sub-millisecond
failures that look like a regression but are an assembly-load fault.

## What this establishes

`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` reflects over the executing test assembly and
fails if any type derives from `System.Windows.Forms.Form`. Its passing means this feature added no
form-derived type to the test assembly. That matters because the three regression tests this feature
adds all exercise WinForms-adjacent controller code: the `QfcItemController` interleaving test
arranges a `TlpCellStates` with empty snapshot lists so `ApplyState` is a no-op and no
`TableLayoutPanel` or `Label` is constructed, and the collection-controller tests build their
subject through `FormatterServices.GetUninitializedObject` rather than through a real form. Neither
route introduces a `Form` subclass, and this test is the mechanical confirmation.

The filter uses `FullyQualifiedName~NoLiveFormInTestAssemblyTests`, which selects by class-name
substring. It matched one test, so the filter is not silently empty — a filter that matches zero
tests would also exit 0 and would prove nothing.

## Acceptance

- The run reports `Failed: 0` — met.
- The passed count is at least `1` — met (1).
