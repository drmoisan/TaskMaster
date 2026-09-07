# P7-T8 — [expect-fail] compile-time RED for issue #490 D3, the FocusSubject signature

Timestamp: 2026-08-28T01-19
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 1
ExpectedExitCode: 1

## Acceptance — the build exits non-zero on the Returns(false) call

`Build FAILED.` with `3 Warning(s)` and `1 Error(s)`. The single error is the intended one:

```
QuickFiler.Test\Controllers\QfcItemController.MailActionsTests.Part2.cs(119,49): error CS1061:
'ISetup<IItemViewer>' does not contain a definition for 'Returns' and no accessible extension
method 'Returns' accepting a first argument of type 'ISetup<IItemViewer>' could be found (are you
missing a using directive or an assembly reference?)
```

Column 49 on that line is the `.Returns(false)` call in
`viewer.Setup(v => v.FocusSubject()).Returns(false);`. The diagnostic names the non-generic
`ISetup<IItemViewer>` overload, which is the overload Moq selects for a **void-returning** member;
the generic `ISetup<IItemViewer, bool>` that carries `Returns(bool)` is only produced once
`FocusSubject` returns `bool`. The compiler error is therefore a direct statement that
`IItemViewer.FocusSubject()` still returns `void`, which is exactly the defect #490 D3 records.

The three warnings are the pre-existing `System.Reactive` `packages.config` advisory, unchanged from
P7-T4.

This is the fail-before evidence for #490 D3's signature change. The test-assembly build stays broken
until P8-T3; the interface declaration changes at P8-T1 and the implementation at P8-T2.

## Platform spelling

PlanCommandAsWritten: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
PlanCommandAsWrittenExitCode: 1

The plan's printed command also exits `1`, but for the wrong reason and **without compiling
anything**: it fails in 0.08 seconds at
`Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is
not set for project 'QuickFiler.Test.csproj' … Platform='Any CPU'`, reporting `0 Warning(s)` and
`1 Error(s)` with no `CS` diagnostic at all. This is the fifth occurrence of the carry-forward
defect corrected at P1-T5, P3-T2, P5-T4 and P7-T4. It matters more here than at any earlier
occurrence: an `[expect-fail]` gate whose acceptance is a non-zero exit would be **satisfied
vacuously** by the configuration error, and the artifact would record a passing RED for a build that
never invoked the compiler. The corrected `/p:Platform=AnyCPU` form is the one that produces the
`CS1061` above, and the acceptance is discharged by that diagnostic, not by the exit code alone.

Output Summary: **Compile-time RED confirmed.** The corrected command exits `1` with
`Build FAILED.`, `3 Warning(s)`, `1 Error(s)`, and the sole error is
`error CS1061: 'ISetup<IItemViewer>' does not contain a definition for 'Returns'` at
`QfcItemController.MailActionsTests.Part2.cs(119,49)` — the `Returns(false)` call on the
`void`-returning `FocusSubject` member. The plan's printed command exits `1` too but compiles
nothing, so it cannot discharge this gate; both exit codes are recorded.
