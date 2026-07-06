---
name: testresults-coverage-xml-cross-module-check
description: TestResults/<baseline|final>-coverage.xml from a single-project vstest run also instruments other loaded first-party modules, letting a reviewer spot-check whether a "repository line coverage" claim scoped to one assembly is misleading
metadata:
  type: project
---

On issue #240 review (2026-07-06), the executor's `dotnet-coverage merge ... -o TestResults/final-coverage.xml` (run against only `UtilitiesCS.Test.dll`) turned out to also contain full module-level coverage entries for other first-party/vendored assemblies transitively loaded during that run: `TaskMaster.dll` (8.58% line), `Tags.dll` (0.00%), `ToDoModel.dll` (0.00%), `QuickFiler.dll` (0.00%), `SVGControl.dll` (15.15%), `Swordfish.NET.General.dll` (45.86%), alongside the intended `UtilitiesCS.dll` (85.88%). Grepping `module id="[A-F0-9]+" name="X.dll"` in that XML gives an instant per-module summary line without opening the 30MB file. This confirmed [[csharp-repowide-coverage-below-80]]'s finding independently, from a completely different coverage run.

**Caveat:** the near-0% modules almost certainly reflect that their own dedicated `*.Test` projects were not executed in this run (only `UtilitiesCS.Test.dll` ran), not their true tested state — do not report those as certified per-module percentages, only as corroborating evidence that no single-project run yields a valid repo-wide figure.

**Why:** this let me independently verify (without rerunning coverage generation, per the skill's "do not rerun" rule) that a feature's self-reported "repository line coverage" was actually single-assembly-scoped, using an artifact the executor had already produced but not highlighted.

**How to apply:** when a C# feature's evidence reports a "repository"/"repo-wide" coverage percentage, check for an uncommitted `TestResults/*.xml` (dotnet-coverage native format) generated during that session before accepting the claim. Grep for `<module id=... name="..." ... line_coverage="...">` lines to see every module the run actually instrumented, not just the one the evidence highlights. See [[csharp-coverage-artifact-is-cobertura]] and [[csharp-repowide-coverage-below-80]] for the canonical-artifact/format side of this gap.
