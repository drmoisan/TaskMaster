# Remediation Policy Read — Issue #171

- **Task:** [P0-T1]
- **Date:** 2026-06-02T10-36
- **Findings covered:** R1, R2, R3

## Policy sources read and confirmed

The following policy sources were read prior to any change:

1. `CLAUDE.md` — project instructions (all sections), including the embedded General Code Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, Tone Policy, and the C# Toolchain section.
2. `.claude/rules/general-code-change.md` — cross-language code change policy summary.
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy summary.
4. `.claude/rules/tonality.md` — required professional tone policy.
5. C# Code Change Policy and C# Unit Test Policy sections within `CLAUDE.md`.

## Confirmed policy points

- **Formatting:** CSharpier (`dotnet tool run csharpier .`) is the required formatter. `dotnet format` is explicitly prohibited because it can rewrite legacy VSTO `.csproj` files. CSharpier formats only `*.cs` files.
- **Tests:** MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`) framework, Moq for mocking, FluentAssertions for assertions.
- **Toolchain order (4 steps, restart from step 1 on any failure or file rewrite):**
  1. Format: `dotnet tool run csharpier .`
  2. Analyzers: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. Nullable type-check: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. Test + coverage: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
- **Canonical coverage artifact path:** `artifacts/csharp/coverage.xml` (permitted orchestration output path; gitignored; must exist for re-audit).
- **Canonical evidence subtree:** all other evidence under `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/<kind>/`.

## Result

All policy sources read and confirmed. No conflicting instructions found. Proceeding under the stated constraints.
