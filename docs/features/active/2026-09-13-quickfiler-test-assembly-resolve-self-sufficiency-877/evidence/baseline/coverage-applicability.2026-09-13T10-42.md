# Coverage applicability position — issue #877

Timestamp: 2026-09-13T10-42
Command: no command executed, static scope determination
EXIT_CODE: 0
Output Summary: The write set for this change contains no production source file. Every path in it lives in the test tree, and the coverage tooling is configured to exclude test files from the coverage denominator. No coverage command is run for this item and no coverage percentage is reported, because none is collected.

## Write set

- `TestSupport/TestAssemblyResolver.cs` — new shared test-support source file, linked into both test projects.
- `QuickFiler.Test/QuickFiler.Test.csproj` — test project file, one additive `<Compile>` item.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — test project file, one additive `<Compile>` item.
- `QuickFiler.Test/SetupAssemblyInitializer.cs` — test assembly initializer source file.
- `UtilitiesCS.Test/TestAssemblyInitializer.cs` — test assembly initializer source file.

Zero of the paths above are production source files.

## Governing thresholds

`CLAUDE.md` governs the coverage thresholds for this repository and for this item. It sets C# repository-wide line coverage at `>= 80%`, sets new modules, classes and methods at `>= 90%`, and requires no reduction in coverage for changed lines.

`CLAUDE.md` sets NO branch-coverage threshold. The `85%` line and `75%` branch figures stated in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` are not authoritative for this item.

## Consequence

No coverage command is planned or run for this change, and `artifacts/csharp/coverage.xml` is not produced. A coverage delta run over a write set that contains no production source file cannot produce a figure attributable to this change. This artifact reports no measured coverage percentage and no estimated coverage percentage, because none is collected. The gate for this change is the M3 discriminator together with the three C# toolchain gates in `CLAUDE.md` order.
