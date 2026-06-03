# Policy Read Confirmation — Issue #171

- Timestamp: 2026-06-02
- Task: [P0-T1]

## Policy sources read

| File | Read |
|------|------|
| `CLAUDE.md` | Yes |
| `.claude/rules/general-code-change.md` | Yes |
| `.claude/rules/general-unit-test.md` | Yes |
| `.claude/rules/csharp.md` | Yes |
| `.claude/rules/tonality.md` | Yes |

## Confirmed policy constraints

- Test framework: **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`). Do not introduce xUnit/NUnit.
- Mocking: **Moq**.
- Assertions: **FluentAssertions** preferred; MSTest `Assert` only when FluentAssertions is impractical.
- Formatting: **CSharpier** (`dotnet tool run csharpier .`). **Do not** use `dotnet format`.
- Toolchain order (restart from step 1 on any failure or file rewrite):
  1. Format — `dotnet tool run csharpier .`
  2. Analyzers — `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. Nullable type-check — `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. Test + coverage — `vstest.console.exe <assemblies> /EnableCodeCoverage`
- No temporary files in tests; no live Outlook COM in tests.
- New scoring/filter logic goes in new file `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`; oversized controllers must not be made materially worse.
- Evidence written only under the feature folder `evidence/<kind>/` paths.
