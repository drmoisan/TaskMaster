# C# Full-Suite Coverage Measurement (Corrected) — Issue #248

Timestamp: 2026-07-06T21:26:43-04:00
Canonical issue number: 248

## Purpose

Corrects the repository-wide C# coverage measurement recorded in prior evidence
(`csharp-vstest-coverage-remediation-final.2026-07-06T19-09.md`, 20.21%) and in
`coverage-floor-disposition.2026-07-06T19-09.md` (`BLOCKED_BY_REPOSITORY_WIDE_COVERAGE_DEBT`).
Those figures were produced by running coverage against a single test assembly
(`QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`). vstest instruments the whole solution
regardless of which tests execute, so a single-assembly run leaves every other project's
production code at zero hits and understates repo-wide coverage. `.github/workflows/ci.yml`
runs all `*.Test.dll` together; this evidence reproduces that full-suite measurement.

## Commands

```bash
export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*'
msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:"Platform=Any CPU"
vstest.console.exe \
  QuickFiler.Test/bin/Debug/QuickFiler.Test.dll \
  Tags.Test/bin/Debug/Tags.Test.dll \
  TaskMaster.Test/bin/Debug/TaskMaster.Test.dll \
  TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll \
  ToDoModel.Test/bin/Debug/ToDoModel.Test.dll \
  UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll \
  VBFunctions.Test/bin/Debug/VBFunctions.Test.dll \
  /EnableCodeCoverage /InIsolation
dotnet-coverage merge <run>.coverage --output artifacts/cov248-full.cobertura.xml --output-format cobertura
```

## Results

- Total tests: 4989. Passed: 4989. Failed: 0. Total time: 54.06 s.
- Raw cobertura line-rate (all instrumented modules incl. test + third-party): 69.31% (123,366 / 177,993 lines-covered/lines-valid reported in the cobertura header; 69.16% recomputed from `<line>` elements).
- Production-only (excluding `*.Test` packages): 49.70% (104,212 / 209,686) — still includes bundled third-party libraries and is not the policy denominator.
- **First-party production only (policy denominator): 81.19% (87,827 / 108,179 lines) — ABOVE the 80% floor.**

Excluded from the first-party production denominator: `*.Test`/`*.Tests` packages and bundled
third-party libraries (FSharp.Core, Deedle, System.Linq.Async, System.Interactive, log4net,
FluentAssertions, Mono.Reflection, Swordfish.NET.*).

### First-party production breakdown

| Assembly | Covered/Valid | Line % |
|----------|---------------|--------|
| UtilitiesCS | 70,610/79,967 | 88.30% |
| QuickFiler | 10,574/14,640 | 72.23% |
| TaskMaster | 2,960/4,635 | 63.86% |
| ToDoModel | 2,046/3,801 | 53.83% |
| SVGControl | 563/3,444 | 16.35% |
| Tags | 1,040/1,542 | 67.44% |
| TaskVisualization (instrumented portion) | 26/142 | 18.31% |
| VBFunctions | 8/8 | 100.00% |
| **Total** | **87,827/108,179** | **81.19%** |

Note: the 81.19% figure meets the floor before applying any COM/VSTO/WinForms coverage
exemptions permitted by `CLAUDE.md`; those exemptions would only raise the testable-denominator
figure further.

## Disposition

COV-1 (`BLOCKED_BY_REPOSITORY_WIDE_COVERAGE_DEBT`) is withdrawn. It was a measurement error,
not a genuine coverage shortfall. Repository-wide first-party production C# line coverage is
81.19%, above the 80% policy floor. No coverage policy exception is required.
