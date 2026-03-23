# Phase 0 — Policy Instructions Read Evidence

Timestamp: 2026-03-19T22:35
Policy Order: Required reading order per `policy-compliance-order` skill

## Files Read (in order)

1. `.github/copilot-instructions.md` — Project guidelines, MSTest + Moq + FluentAssertions convention
2. `.github/instructions/general-code-change.instructions.md` — Baseline code change rules, toolchain loop (format → lint → type-check → test), bugfix workflow, design principles
3. `.github/instructions/general-unit-test.instructions.md` — Core test principles (independence, isolation, determinism), >=80% coverage floor, AAA pattern, no external deps, no temp files
4. `.github/instructions/csharp-code-change.instructions.md` — C# tooling: csharpier for formatting, msbuild with EnableNETAnalyzers for linting, msbuild with Nullable=enable/TreatWarningsAsErrors for type checking; design/type-safety/naming conventions
5. `.github/instructions/csharp-unit-test.instructions.md` — MSTest framework, Moq for mocks, FluentAssertions for assertions, C# toolchain commands (csharpier, msbuild analyzer, msbuild nullable, vstest.console.exe)

## Key Constraints Noted

- No `dotnet format` — use `csharpier` only
- Tests must be deterministic, isolated, no temp files
- All new test files must be registered in `UtilitiesCS.Test.csproj` via `<Compile Include>`
- Toolchain loop: csharpier → msbuild analyzers → msbuild nullable → vstest with coverage
- Repo uses VS MSBuild (not dotnet CLI) for building
- Legacy packages.config projects need `/t:Restore /p:RestorePackagesConfig=true`
