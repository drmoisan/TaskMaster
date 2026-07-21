Timestamp: 2026-07-20T13-05

Policy Order: CLAUDE.md (all sections) → General Code Change Policy → General Unit Test Policy → C# Code Change Policy → C# Unit Test Policy

Files read (in order):
1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

## CLAUDE.md — Policy Compliance Order (quoted verbatim)

```
## Policy Compliance Order

The four core policies below are embedded directly in this file and apply to every session without requiring explicit skill loads. Apply them in this order:

1. This file (CLAUDE.md) — all sections
2. General Code Change Policy (§ below)
3. General Unit Test Policy (§ below)
4. For C#: C# Code Change Policy (§ below) and C# Unit Test Policy (§ below)
```

## general-code-change.md — Mandatory Toolchain Loop (quoted verbatim)

```
## Mandatory Toolchain Loop

Run the full seven-stage toolchain in this exact order and repeat until all stages pass in a single pass:

1. **Formatting** (e.g., Black, Prettier, CSharpier, Invoke-Formatter)
2. **Linting** (e.g., Ruff, ESLint, PSScriptAnalyzer, .NET analyzers)
3. **Type checking** (e.g., Pyright, TSC, nullable analysis; skip for PowerShell)
4. **Architecture-boundary tests** (e.g., dependency-cruiser, NetArchTest.Rules)
5. **Unit tests** (e.g., Pytest, Vitest, MSTest, Pester) including property-based tests where applicable per `quality-tiers.md`
6. **Contract / schema compatibility checks** (e.g., oasdiff, schema-snapshot diff)
7. **Integration tests**

**Restart from step 1** if any stage fails or auto-fixes any files. Do not stop the loop until all seven stages complete without errors in a single pass.

Mutation testing and golden tests run in pre-merge or nightly pipelines, not the per-commit loop.
```

Note: for this C# minor-audit plan, the concrete toolchain command sequence actually executed is the CUT3/C#7 four-step sequence (format → lint → type-check → test) as specified in CLAUDE.md's "C# Toolchain" section and `.claude/rules/csharp.md`'s "Toolchain" section; the general seven-stage loop's architecture-boundary/contract/integration stages have no C#-specific command defined in this repo's C# policy sections and are not separately invoked by this plan.

## general-unit-test.md — Coverage Requirements (quoted verbatim)

```
## Coverage Requirements

- **Line coverage must remain >= 85% across all tiers (T1–T4).**
- **Branch coverage must remain >= 75% across all tiers (T1–T4).**
- Code changes or refactors must not reduce coverage for the lines that were changed.
- Tier-specific lower coverage thresholds are not used in this repository. See `.claude/rules/quality-tiers.md` for the full tier system.
- Coverage is a supporting metric, not the sole quality gate. Untested critical behavior is not acceptable even if the overall percentage looks good.
- Configure coverage tooling to exclude test files (e.g., `tests/`) so metrics reflect application code, not tests.
- Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement. Examples: Python `Protocol`-only modules consumed only under `TYPE_CHECKING`, TypeScript interface/type-only files, and C# interface-only files. Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold.
```

Note: per the plan's "Coverage Floor Used By This Plan" section, this plan uses issue.md AC-5's explicit >= 90% new/changed-code coverage figure (consistent with CLAUDE.md's C# Unit Test Policy repo-wide 80% / new-code 90% figures) for the new/changed-code coverage gate in this minor-audit scope; the wider repo-floor conflict noted in the plan is out of scope here.

## csharp.md — Toolchain and Testing Standards (quoted verbatim)

```
## Toolchain

1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use `dotnet format`. Command: `dotnet tool run csharpier .` or `csharpier .`
2. **Linting — .NET Analyzers**: C# code must pass Roslyn/.NET analyzer diagnostics. Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type Checking — Nullable Analysis**: Enable nullable reference types and fail on warnings. Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. **Testing — MSTest + Moq + FluentAssertions**: Run tests with: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Run the toolchain in order: format → lint → type-check → test. Restart from step 1 if any step fails or changes files.
```

```
## Testing Standards

- Use **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`) as the test framework.
- Use **Moq** for mocking.
- Prefer **FluentAssertions** for assertions; use MSTest `Assert` only when FluentAssertions is not practical.
- Use `[TestClass]` and `[TestMethod]` attributes.
- Follow Arrange–Act–Assert structure.
- No external dependencies in unit tests.
- Repository-wide line coverage must remain >= 80%.
- Any new module, class, or method must reach >= 90% coverage.
- Coverage regression on changed lines is a blocking finding.
```
