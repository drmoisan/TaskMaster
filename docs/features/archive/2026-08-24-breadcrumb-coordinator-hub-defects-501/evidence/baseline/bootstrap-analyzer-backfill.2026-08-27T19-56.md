# Bootstrap — Analyzer Version Back-Fill (P0-T8)

Timestamp: 2026-08-27T19-56

## Skew confirmed before the fix

`packages.config` restore (P0-T7) produced:

```
packages/Meziantou.Analyzer.3.0.174/
packages/Roslynator.Analyzers.4.16.1/
```

The `<Analyzer Include>` items name different versions. Verified at
`QuickFiler/QuickFiler.csproj:585-589`:

```
<Analyzer Include="..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
<Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
<Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll" />
<Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll" />
<Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll" />
```

A missing `<Analyzer Include>` path is `error CS0006`, not a warning, so without this back-fill every
`EXIT_CODE: 0` build acceptance in this plan would be unreachable.

## Route taken

The plan authorizes either `nuget install ... -OutputDirectory packages` or copying the two folders
from the primary checkout. **Route taken: the copy route**, because the primary checkout already held
both folders, which is the cheaper option the Phase 0 bootstrap note records as available.

Command: `cp -r <primary-checkout>/packages/Meziantou.Analyzer.3.0.156 WS/packages/Meziantou.Analyzer.3.0.156`
EXIT_CODE: 0

Command: `cp -r <primary-checkout>/packages/Roslynator.Analyzers.4.16.0 WS/packages/Roslynator.Analyzers.4.16.0`
EXIT_CODE: 0

Nothing was deleted, moved, or overwritten. Both operations are purely additive: neither target path
existed before the copy, and the restored `3.0.174` and `4.16.1` folders remain in place untouched.

## Post-condition verification

Command: `ls -l packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll`
EXIT_CODE: 0
Output Summary: both files exist.

| Path (relative to `WS`) | Size (bytes) |
| --- | ---: |
| `packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll` | 2749952 |
| `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll` | 382464 |

Acceptance: both named DLL paths exist. PASS.
