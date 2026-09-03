# Marked [DoNotParallelize] classes still pass (P5-T6)

Timestamp: 2026-09-02T23-31

Command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~DASLFilterParserTests|FullyQualifiedName~StackGeek_Tests"`

EXIT_CODE: 0

PassedCount: 14

FailedCount: 0

Output Summary:

- Preceding rebuild: `& $msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU` returned exit code 0 with `5 Warning(s)` and `0 Error(s)`. The five warnings are the pre-existing `System.Reactive` 7.0.0 `PackagesConfigCheck` warnings also recorded by P4-T5.
- `Test Run Successful.` `Total tests: 14` / `Passed: 14` / no failures.
- The filter uses `|` rather than `OR`, which is the operator `vstest.console.exe` accepts in a
  `/TestCaseFilter:` expression.
- `[DoNotParallelize]` is additive and order-independent: both classes still pass with their test
  bodies, assertions, and test-method names unchanged, as the P5-T3 anchored diff shows.
- The orphan duplicate `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` was deleted by
  P5-T5 before this run, and the rebuild above confirms its absence breaks nothing: it was never
  in the `<Compile Include>` list (P5-T4).
