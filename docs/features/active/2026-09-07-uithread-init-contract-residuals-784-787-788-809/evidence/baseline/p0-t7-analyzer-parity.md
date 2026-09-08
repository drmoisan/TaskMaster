# [P0-T7] Analyzer version parity between csproj and packages.config

Timestamp: 2026-09-08T00-23

Command: for each of `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj`, load the project XML, extract every `<Analyzer Include="..\packages\<id>.<version>\...">` path, parse the id and version out of the `packages\` path segment, and compare against the `id`/`version` pair of the matching `<package>` element in that project's sibling `packages.config`.

EXIT_CODE: 0

ANALYZER_PARITY_MISMATCH_COUNT: 0

Output Summary: 31 `<Analyzer Include>` items across the three projects. Every item's csproj path version equals the `packages.config` version for the same package id. No divergence.

| Project | Analyzer id | csproj version | packages.config version | Result |
|---|---|---|---|---|
| `UtilitiesCS/UtilitiesCS.csproj` | Meziantou.Analyzer | 3.0.203 | 3.0.203 | MATCH |
| `UtilitiesCS/UtilitiesCS.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `UtilitiesCS/UtilitiesCS.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `UtilitiesCS/UtilitiesCS.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `UtilitiesCS/UtilitiesCS.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `UtilitiesCS/UtilitiesCS.csproj` | AsyncFixer | 2.1.0 | 2.1.0 | MATCH |
| `UtilitiesCS/UtilitiesCS.csproj` | Microsoft.CodeAnalysis.BannedApiAnalyzers | 5.6.0 | 5.6.0 | MATCH |
| `UtilitiesCS/UtilitiesCS.csproj` | Microsoft.CodeAnalysis.BannedApiAnalyzers | 5.6.0 | 5.6.0 | MATCH |
| `UtilitiesCS/UtilitiesCS.csproj` | SonarAnalyzer.CSharp | 10.33.0.1635 | 10.33.0.1635 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | MSTest.Analyzers | 4.4.0 | 4.4.0 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | MSTest.Analyzers | 4.4.0 | 4.4.0 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | SonarAnalyzer.CSharp | 10.33.0.1635 | 10.33.0.1635 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Meziantou.Analyzer | 3.0.203 | 3.0.203 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | AsyncFixer | 2.1.0 | 2.1.0 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Microsoft.CodeAnalysis.BannedApiAnalyzers | 5.6.0 | 5.6.0 | MATCH |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | Microsoft.CodeAnalysis.BannedApiAnalyzers | 5.6.0 | 5.6.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | MSTest.Analyzers | 4.4.0 | 4.4.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | MSTest.Analyzers | 4.4.0 | 4.4.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | SonarAnalyzer.CSharp | 10.33.0.1635 | 10.33.0.1635 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Meziantou.Analyzer | 3.0.203 | 3.0.203 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Roslynator.Analyzers | 5.0.0 | 5.0.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | AsyncFixer | 2.1.0 | 2.1.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Microsoft.CodeAnalysis.BannedApiAnalyzers | 5.6.0 | 5.6.0 | MATCH |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Microsoft.CodeAnalysis.BannedApiAnalyzers | 5.6.0 | 5.6.0 | MATCH |

Repeated rows for the same id are separate `<Analyzer Include>` items pointing at different analyzer DLLs of the same package (for example the `roslyn4.7` and code-fix assemblies of Roslynator).

This is a verification, not an assertion of any particular version: a Dependabot bump can move both sides together, and only a divergence between the two sides is a defect.
