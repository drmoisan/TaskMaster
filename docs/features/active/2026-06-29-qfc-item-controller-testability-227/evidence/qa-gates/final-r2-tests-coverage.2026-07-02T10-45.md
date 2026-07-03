# Final QA — Tests + Coverage (P8-T4, AC7)

Timestamp: 2026-07-02T10-45
Command (tests, canonical): vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
Command (numeric coverage): dotnet-coverage collect --output artifacts\csharp\coverage-r2-final.cobertura.xml --output-format cobertura --settings coverage.config -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
EXIT_CODE: 0

## Result

- Total tests: 328
- Passed: 328
- Failed: 0

## Numeric post-change coverage (final tree)

- Repo-wide (Cobertura root): 11163/71036 = **15.71%** (satisfied-with-exception under the #223
  authority-scoped precedent; residual uplift tracked under #197).
- Affected `QfcItemController` non-exempt denominator: 885/1051 = **84.21%** (>= 80% AC5 floor).
- New/extracted seam code (`WireIntentEvents`, `BtnPopOutCore`, `BtnReplyCore`, `BtnReplyAllCore`,
  `BtnForwardCore`, `TxtboxBodyDoubleClickCore`, `HandleWebViewInitializedAsync`): 100% line coverage
  via the dedicated `Seam*Tests`. The new `IUiDispatcher`/`IWebViewCoreInitializer`/`IMailItemActions`
  interfaces have no executable lines; the three `[ExcludeFromCodeCoverage]` adapter shims each carry a
  construction/forwarding smoke test.

Output Summary: 328/328 pass; final affected non-exempt denominator 84.21% (>= 80%); new/extracted code
100%; repo-wide 15.71% (authority-scoped exception). Full C# toolchain (csharpier -> analyzers ->
nullable/TWAE -> MSTest+coverage) passes in order with no regression (AC7).
