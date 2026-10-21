# P0-T7 — Analyzer Package Version Agreement

Timestamp: 2026-09-01T13-31

Command:
```
grep -n "Analyzer Include" QuickFiler.Test/QuickFiler.Test.csproj
```
plus a full read of `QuickFiler.Test/packages.config`, pairing each `id="..."` line with the
`version="..."` line that follows it inside the same `<package>` element. A line search over
`packages.config` was deliberately not used to obtain versions: that file is CSharpier-formatted and
wraps every multi-attribute `<package>` element one attribute per line, so an `id=` line carries no
`version=` value.

EXIT_CODE: 0

Output Summary:

All six analyzer packages agree between the project file and the package manifest. No skew was
observed, so no `ANALYZER_VERSION_SKEW:` line is recorded and execution continues.

## Version strings embedded in `Analyzer Include` paths (`QuickFiler.Test/QuickFiler.Test.csproj`)

- `:474` — `..\packages\MSTest.Analyzers.4.3.3\analyzers\dotnet\cs\MSTest.Analyzers.CodeFixes.dll` — version `4.3.3`
- `:475` — `..\packages\MSTest.Analyzers.4.3.3\analyzers\dotnet\cs\MSTest.Analyzers.dll` — version `4.3.3`
- `:476` — `..\packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll` — version `10.33.0.1635`
- `:503` — `..\packages\Meziantou.Analyzer.3.0.194\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll` — version `3.0.194`
- `:504` — `..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll` — version `5.0.0`
- `:505` — `..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll` — version `5.0.0`
- `:506` — `..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll` — version `5.0.0`
- `:507` — `..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll` — version `5.0.0`
- `:508` — `..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll` — version `2.1.0`
- `:509` — `..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll` — version `5.6.0`
- `:510` — `..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll` — version `5.6.0`

## Paired `id` and `version` values from `QuickFiler.Test/packages.config`

Read by pairing each `id="..."` line with the `version="..."` line inside the same `<package>`
element, using the wrapped element at `:113-118` as the reference shape.

| Package | `id` line | `version` line | `packages.config` version | Project-file version | Agree |
|---|---|---|---|---|---|
| `AsyncFixer` | `:3` (single line) | `:3` (single line) | `2.1.0` | `2.1.0` | yes |
| `Meziantou.Analyzer` | `:12` | `:13` | `3.0.194` | `3.0.194` | yes |
| `Microsoft.CodeAnalysis.BannedApiAnalyzers` | `:21` | `:22` | `5.6.0` | `5.6.0` | yes |
| `MSTest.Analyzers` | `:114` | `:115` | `4.3.3` | `4.3.3` | yes |
| `Roslynator.Analyzers` | `:140` | `:141` | `5.0.0` | `5.0.0` | yes |
| `SonarAnalyzer.CSharp` | `:146` | `:147` | `10.33.0.1635` | `10.33.0.1635` | yes |

Each of the six packages agrees between the two files. The plan's cited element ranges hold against
this tree: `Meziantou.Analyzer` at `:11-16`, `Microsoft.CodeAnalysis.BannedApiAnalyzers` at `:20-25`,
`MSTest.Analyzers` at `:113-118`, `Roslynator.Analyzers` at `:139-144`, `SonarAnalyzer.CSharp` at
`:145-150`, and `AsyncFixer` on the single line `:3`.

## Supplementary observation (not part of this task's acceptance)

Each of the six `Analyzer Include` paths was additionally checked for existence on disk after the
P0-T6 restore. All six resolved. This is recorded because an unresolvable analyzer path produces a
CS0006 cascade in the P0-T11 analyzer rebuild that reads as a red baseline gate without being one.
