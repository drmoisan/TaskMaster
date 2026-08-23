# [P0-T5] Analyzer Build Baseline — Baseline Evidence

- **Issue:** #424
- **Task:** [P0-T5]
- **Toolchain step:** 2 of 4 (lint / .NET analyzers)

Timestamp: 2026-08-06T22-21

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

- **Errors: 0.**
- **Warnings: 5** on the steady-state incremental pass; **7 distinct warning instances** observed on the preceding cold pass that compiled every project.

### Warning inventory (all pre-existing, none first-party analyzer diagnostics)

| Count | Diagnostic | Projects | Assessment |
|---|---|---|---|
| 5 | `warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.` (from `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`) | `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`, `QuickFiler`, `TaskMaster` | Pre-existing package/build-configuration warning, unrelated to issue #424. Not touched by this plan. |
| 1 (cold pass only) | `CSC : warning CS2002: Source file '...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` | `UtilitiesCS.Test` | Pre-existing duplicate `<Compile>` include in `UtilitiesCS.Test.csproj`. Out of scope for issue #424; recorded, not fixed. |

### Analyzer stack status

Zero diagnostics from the five configured first-party analyzers (Meziantou.Analyzer, SonarAnalyzer.CSharp, Roslynator.Analyzers, AsyncFixer, Microsoft.CodeAnalysis.BannedApiAnalyzers). Per `.claude/rules/csharp.md`, new analyzer rule severities are pinned at `suggestion`, so they surface as messages rather than warnings and do not appear in the warning count.

### Build products confirmed

All solution projects built successfully, including the two directly relevant to this plan:
- `QuickFiler -> QuickFiler\bin\Debug\QuickFiler.dll`
- `QuickFiler.Test -> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`

**Baseline conclusion:** the analyzer gate passes at exit code 0 with zero errors and only pre-existing, out-of-scope warnings. Any new warning or error appearing in `[P6-T2]` is attributable to changes made by this plan.
