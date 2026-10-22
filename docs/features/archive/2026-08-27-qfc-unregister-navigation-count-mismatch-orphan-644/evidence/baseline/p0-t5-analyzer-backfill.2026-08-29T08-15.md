# Baseline — Analyzer package version back-fill ([P0-T5])

- Issue: #644
- Task: `[P0-T5]`
- Timestamp: 2026-08-29T08-15

## Why this task is required

All 16 first-party `.csproj` files carry unconditional `<Analyzer Include>` items naming
`..\packages\Meziantou.Analyzer.3.0.156\...` and `..\packages\Roslynator.Analyzers.4.16.0\...`,
while `packages.config` pins Meziantou `3.0.174` and Roslynator `4.16.1`. `[P0-T4]`'s restore
therefore installs the pinned versions and not the versions the `<Analyzer Include>` items name.
A missing analyzer assembly is `error CS0006`, not a warning, so this back-fill must succeed
before any msbuild task in this plan can pass.

## Commands

Command: `nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages`
EXIT_CODE: 0

Output tail (host paths redacted):

```
Resolving actions to install package 'Meziantou.Analyzer.3.0.156'
Retrieving package 'Meziantou.Analyzer 3.0.156' from '<user-profile>\.nuget\packages\'.
Successfully installed 'Meziantou.Analyzer 3.0.156' to <repo-root>\packages
```

Command: `nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages`
EXIT_CODE: 0

Output tail (host paths redacted):

```
Resolving actions to install package 'Roslynator.Analyzers.4.16.0'
Retrieving package 'Roslynator.Analyzers 4.16.0' from '<user-profile>\.nuget\packages\'.
Successfully installed 'Roslynator.Analyzers 4.16.0' to <repo-root>\packages
```

## Acceptance verification

Command: `Test-Path` on each of the two analyzer assembly paths the task names.

```
packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll  -> True
packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll -> True
```

Output Summary: Both `nuget install` invocations exited 0, so the `REMEDIATION-REQUIRED`
reporting branch this task authorizes was **not** taken. Both analyzer assemblies the
`<Analyzer Include>` items name now exist on disk. The `<Analyzer Include>` versus
`packages.config` version skew is a known pre-existing condition recorded in the plan and is
worked around locally by this task; it is deliberately not fixed here and no issue is filed for
it by this plan.
