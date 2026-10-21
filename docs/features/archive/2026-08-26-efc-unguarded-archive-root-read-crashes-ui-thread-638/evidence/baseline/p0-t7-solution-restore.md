# [P0-T7] Solution restore and analyzer skew resolution (Issue 638)

Timestamp: 2026-08-29T12-20

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`

EXIT_CODE: 0

Output Summary:

## 1. Solution restore

The restore reported:

```
    Installed:
        172 package(s) to packages.config projects
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

This materialized the previously absent repository-root `packages/` directory.

## 2. Analyzer wiring comparison

`Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'Meziantou.Analyzer.','Roslynator.Analyzers.' | ForEach-Object { $_.Line.Trim() }`
returned seven matches. Two are excluded from the comparison per the task text, because
they are not `<Analyzer Include>` items and name the packages.config version rather than
the analyzer-item version:

- `:3` — `<Import Project="..\packages\Meziantou.Analyzer.3.0.174\build\Meziantou.Analyzer.props" ... />`
- `:490` — `<Error Condition="!Exists('..\packages\Meziantou.Analyzer.3.0.174\build\Meziantou.Analyzer.props')" ... />`

The five `<Analyzer Include>` items considered are:

- `:499` — `..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`
- `:500` — `..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll`
- `:501` — `..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll`
- `:502` — `..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll`
- `:503` — `..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll`

`QuickFiler.Test/packages.config` pins `Meziantou.Analyzer` to `version="3.0.174"` at
`:11-12` and `Roslynator.Analyzers` to `version="4.16.1"` at `:140-141`, so the restore
installed `packages/Meziantou.Analyzer.3.0.174` and `packages/Roslynator.Analyzers.4.16.1`
and neither `<Analyzer Include>` directory existed.

## 3. ANALYZER_SKEW — first check (before remediation)

ANALYZER_SKEW: `Meziantou.Analyzer.3.0.156`, `Roslynator.Analyzers.4.16.0`

Directory existence, first check:

```
packages/Meziantou.Analyzer.3.0.156  : False
packages/Roslynator.Analyzers.4.16.0 : False
packages/Meziantou.Analyzer.3.0.174  : True
packages/Roslynator.Analyzers.4.16.1 : True
```

## 4. Remediation

```
nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages   -> exit 0
nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages  -> exit 0
```

Both reported `Successfully installed ... to <worktree-root>\packages`.

## 5. ANALYZER_SKEW — second check (after remediation)

ANALYZER_SKEW: none

Directory existence, second check:

```
packages/Meziantou.Analyzer.3.0.156  : True
packages/Roslynator.Analyzers.4.16.0 : True
```

Both of the two ids this task inspects (`Meziantou.Analyzer` and `Roslynator.Analyzers`)
now resolve.

## 6. Tracked-file impact

`git status --porcelain -- '*.csproj' '*/packages.config' 'packages'` output, verbatim:

```
```

The output is empty. The remediation touched no tracked file; `.gitignore:191` ignores
`**/[Pp]ackages/*`.
