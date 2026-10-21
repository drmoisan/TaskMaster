# P0-T4 — Analyzer package version agreement

Timestamp: 2026-09-07T14-07
Task: [P0-T4]
Issue: #796
Channel used: A

Command: a PowerShell measurement that, for each of the two project files, parses
that project's `packages.config` as XML into an id-to-version map, extracts every
`<Analyzer Include="...">` path with the regex `Analyzer\s+Include="([^"]+)"`,
derives the package id and version from the `..\packages\<Id>.<Version>\...` folder
segment by longest-id match against the map, compares the two versions, and calls
`Test-Path` on the referenced .dll resolved relative to the project directory.

EXIT_CODE: 0

## Why this is re-measured rather than assumed

A missing analyzer HintPath is `error CS0006`, not a warning, so a skew would fail
the P0-T8 and P0-T9 rebuild gates rather than downgrade them. The skew was resolved
upstream in issue #647; this task re-measures rather than assuming the resolution is
present in this worktree.

## QuickFiler/QuickFiler.csproj

The analyzer `<ItemGroup>` spans lines 591-603. The `<Analyzer Include>` entries
occupy lines 593-600 and 602; line 601 is an `<AdditionalFiles>` entry for
BannedSymbols.txt and line 592 is a comment. This matches the plan's cited range
593-602.

| Package | csproj HintPath version | packages.config version | Verdict | .dll on disk |
|---|---|---|---|---|
| Meziantou.Analyzer | 3.0.203 | 3.0.203 | AGREES | yes |
| Roslynator.Analyzers (Roslynator.CSharp.Analyzers.dll) | 5.0.0 | 5.0.0 | AGREES | yes |
| Roslynator.Analyzers (Roslynator_Analyzers_Roslynator.Common.dll) | 5.0.0 | 5.0.0 | AGREES | yes |
| Roslynator.Analyzers (Roslynator_Analyzers_Roslynator.Core.dll) | 5.0.0 | 5.0.0 | AGREES | yes |
| Roslynator.Analyzers (Roslynator_Analyzers_Roslynator.CSharp.dll) | 5.0.0 | 5.0.0 | AGREES | yes |
| AsyncFixer | 2.1.0 | 2.1.0 | AGREES | yes |
| Microsoft.CodeAnalysis.BannedApiAnalyzers (BannedApiAnalyzers.dll) | 5.6.0 | 5.6.0 | AGREES | yes |
| Microsoft.CodeAnalysis.BannedApiAnalyzers (CSharp.BannedApiAnalyzers.dll) | 5.6.0 | 5.6.0 | AGREES | yes |
| SonarAnalyzer.CSharp | 10.33.0.1635 | 10.33.0.1635 | AGREES | yes |

Nine rows. Every row AGREES and every referenced .dll exists on disk.

## QuickFiler.Test/QuickFiler.Test.csproj

The `<Analyzer Include>` entries occupy lines 491-493 and 516-523.

| Package | csproj HintPath version | packages.config version | Verdict | .dll on disk |
|---|---|---|---|---|
| MSTest.Analyzers (MSTest.Analyzers.CodeFixes.dll) | 4.4.0 | 4.4.0 | AGREES | yes |
| MSTest.Analyzers (MSTest.Analyzers.dll) | 4.4.0 | 4.4.0 | AGREES | yes |
| SonarAnalyzer.CSharp | 10.33.0.1635 | 10.33.0.1635 | AGREES | yes |
| Meziantou.Analyzer | 3.0.203 | 3.0.203 | AGREES | yes |
| Roslynator.Analyzers (Roslynator.CSharp.Analyzers.dll) | 5.0.0 | 5.0.0 | AGREES | yes |
| Roslynator.Analyzers (Roslynator_Analyzers_Roslynator.Common.dll) | 5.0.0 | 5.0.0 | AGREES | yes |
| Roslynator.Analyzers (Roslynator_Analyzers_Roslynator.Core.dll) | 5.0.0 | 5.0.0 | AGREES | yes |
| Roslynator.Analyzers (Roslynator_Analyzers_Roslynator.CSharp.dll) | 5.0.0 | 5.0.0 | AGREES | yes |
| AsyncFixer | 2.1.0 | 2.1.0 | AGREES | yes |
| Microsoft.CodeAnalysis.BannedApiAnalyzers (BannedApiAnalyzers.dll) | 5.6.0 | 5.6.0 | AGREES | yes |
| Microsoft.CodeAnalysis.BannedApiAnalyzers (CSharp.BannedApiAnalyzers.dll) | 5.6.0 | 5.6.0 | AGREES | yes |

Eleven rows. Every row AGREES and every referenced .dll exists on disk.

## Remediation

None applied. No row is SKEWED and no referenced .dll is absent, so the task's
remediation branch (a repeat of the P0-T3 restore, or a corrected HintPath) was not
entered.

Output Summary: 20 analyzer HintPath rows measured across the two project files.
20 of 20 read AGREES; 20 of 20 referenced .dll files are present on disk. No skew.
