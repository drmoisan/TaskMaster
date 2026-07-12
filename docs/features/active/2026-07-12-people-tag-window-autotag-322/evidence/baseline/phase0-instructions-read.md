Timestamp: 2026-07-12T15-57

Policy Order: CLAUDE.md (all sections) -> General Code Change Policy -> General Unit Test Policy -> C# Code Change Policy -> C# Unit Test Policy

Files read (in order):
1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

## 1. CLAUDE.md — Policy Compliance Order (quoted)

> The four core policies below are embedded directly in this file and apply to every session
> without requiring explicit skill loads. Apply them in this order:
>
> 1. This file (CLAUDE.md) — all sections
> 2. General Code Change Policy (§ below)
> 3. General Unit Test Policy (§ below)
> 4. For C#: C# Code Change Policy (§ below) and C# Unit Test Policy (§ below)

CLAUDE.md's own C# Toolchain section (authoritative concrete commands for this repo):

> 1. **Format**: `dotnet tool run csharpier .` (or `csharpier .` if installed globally)
> 2. **Analyze**: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
> 3. **Type-check**: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
> 4. **Test**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
>
> If any step fails, fix and restart from step 1.

## 2. General Code Change Policy — Mandatory Toolchain Loop (quoted)

> Run the full seven-stage toolchain in this exact order and repeat until all stages pass in a
> single pass:
>
> 1. **Formatting** (e.g., Black, Prettier, CSharpier, Invoke-Formatter)
> 2. **Linting** (e.g., Ruff, ESLint, PSScriptAnalyzer, .NET analyzers)
> 3. **Type checking** (e.g., Pyright, TSC, nullable analysis; skip for PowerShell)
> 4. **Architecture-boundary tests** (e.g., dependency-cruiser, NetArchTest.Rules)
> 5. **Unit tests** (e.g., Pytest, Vitest, MSTest, Pester) including property-based tests where
>    applicable per `quality-tiers.md`
> 6. **Contract / schema compatibility checks** (e.g., oasdiff, schema-snapshot diff)
> 7. **Integration tests**
>
> **Restart from step 1** if any stage fails or auto-fixes any files. Do not stop the loop until
> all seven stages complete without errors in a single pass.

Note: this feature's plan (per `atomic-plan-contract` and the C#-specific CLAUDE.md toolchain)
scopes execution to the four concrete C# commands quoted above (format, analyzer build, nullable
build, vstest coverage); no architecture-boundary/.NET NetArchTest project, contract-check, or
separate integration-test stage exists for this repository's C# projects beyond those four steps.

## 3. General Unit Test Policy — Coverage Requirements (quoted)

> - **Comprehensive Coverage (within reason)**
>   - Aim to exercise critical paths and important edge conditions.
>   - Configure coverage tooling to exclude test files (e.g., `tests/`), so metrics reflect the
>     application code, not the tests themselves.
>   - Repository-wide line coverage must remain `>= 80%`.
>   - **COM/VSTO/WinForms coverage exemption (testable denominator).** The 80% floor applies to
>     the **testable denominator** — production-only first-party code, after excluding: (a) VSTO
>     add-in lifecycle classes; (b) WinForms form-derived classes and Designer-generated code;
>     (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`,
>     `ToDoModel`, and `Tags` that directly depend on `Microsoft.Office.Interop.Outlook.Application`,
>     `MailItem`, `Store`, or `MAPIFolder` without an injectable seam.
>   - Any new modules, classes, or methods added must target `>= 90%` coverage.
>   - Code changes or refactors must not reduce coverage for the lines that were changed.

## 4. C# Code Change Policy / C# Unit Test Policy — Toolchain and Testing Standards (quoted from
`.claude/rules/csharp.md`)

> ## Toolchain
>
> 1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use
>    `dotnet format`. Command: `dotnet tool run csharpier .` or `csharpier .`
> 2. **Linting — .NET Analyzers**: C# code must pass Roslyn/.NET analyzer diagnostics. Command:
>    `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
>    /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
> 3. **Type Checking — Nullable Analysis**: Enable nullable reference types and fail on warnings.
>    Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
>    /p:Nullable=enable /p:TreatWarningsAsErrors=true`
> 4. **Testing — MSTest + Moq + FluentAssertions**: Run tests with: `vstest.console.exe
>    <test-assembly-paths> /EnableCodeCoverage`
>
> Run the toolchain in order: format → lint → type-check → test. Restart from step 1 if any step
> fails or changes files.
>
> ## Testing Standards
>
> - Use **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`) as the test framework.
> - Use **Moq** for mocking.
> - Prefer **FluentAssertions** for assertions; use MSTest `Assert` only when FluentAssertions is
>   not practical.
> - Use `[TestClass]` and `[TestMethod]` attributes.
> - Follow Arrange–Act–Assert structure.
> - No external dependencies in unit tests.
> - Repository-wide line coverage must remain >= 80%.
> - Any new module, class, or method must reach >= 90% coverage.
> - Coverage regression on changed lines is a blocking finding.
