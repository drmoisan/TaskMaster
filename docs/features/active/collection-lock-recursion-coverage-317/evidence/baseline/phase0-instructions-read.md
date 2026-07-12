# Phase 0 — Policy Read Evidence (#317)

Timestamp: 2026-07-11T19-40

## Policy Order

1. `CLAUDE.md` (feature worktree root)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

## Files Read (feature worktree `C:/Users/DanMoisan/repos/TaskMaster-wt/collection-lock-recursion-coverage-317`)

1. `C:/Users/DanMoisan/repos/TaskMaster-wt/collection-lock-recursion-coverage-317/CLAUDE.md`
2. `C:/Users/DanMoisan/repos/TaskMaster-wt/collection-lock-recursion-coverage-317/.claude/rules/general-code-change.md`
3. `C:/Users/DanMoisan/repos/TaskMaster-wt/collection-lock-recursion-coverage-317/.claude/rules/general-unit-test.md`
4. `C:/Users/DanMoisan/repos/TaskMaster-wt/collection-lock-recursion-coverage-317/.claude/rules/csharp.md`

## Quoted Sections

### CLAUDE.md — Policy Compliance Order (verbatim)

> The four core policies below are embedded directly in this file and apply to every session without requiring explicit skill loads. Apply them in this order:
>
> 1. This file (CLAUDE.md) — all sections
> 2. General Code Change Policy (§ below)
> 3. General Unit Test Policy (§ below)
> 4. For C#: C# Code Change Policy (§ below) and C# Unit Test Policy (§ below)

### general-code-change.md — Mandatory Toolchain Loop (verbatim)

> Run the full seven-stage toolchain in this exact order and repeat until all stages pass in a single pass:
>
> 1. **Formatting** (e.g., Black, Prettier, CSharpier, Invoke-Formatter)
> 2. **Linting** (e.g., Ruff, ESLint, PSScriptAnalyzer, .NET analyzers)
> 3. **Type checking** (e.g., Pyright, TSC, nullable analysis; skip for PowerShell)
> 4. **Architecture-boundary tests** (e.g., dependency-cruiser, NetArchTest.Rules)
> 5. **Unit tests** (e.g., Pytest, Vitest, MSTest, Pester) including property-based tests where applicable per `quality-tiers.md`
> 6. **Contract / schema compatibility checks** (e.g., oasdiff, schema-snapshot diff)
> 7. **Integration tests**
>
> **Restart from step 1** if any stage fails or auto-fixes any files. Do not stop the loop until all seven stages complete without errors in a single pass.

Note: this repo's `.claude/rules/csharp.md` (C#-specific) narrows the applicable per-commit loop to the
four-stage format→lint→type-check→test sequence for C# work, per CLAUDE.md's own "C# Toolchain" section
and the plan's Phase 3 scope (this is a test-only restoration; no architecture-boundary/contract/integration
stages apply to this change).

### general-unit-test.md — Coverage Requirements (verbatim)

> - **Line coverage must remain >= 85% across all tiers (T1–T4).**
> - **Branch coverage must remain >= 75% across all tiers (T1–T4).**
> - Code changes or refactors must not reduce coverage for the lines that were changed.
> - Tier-specific lower coverage thresholds are not used in this repository. See `.claude/rules/quality-tiers.md` for the full tier system.
> - Coverage is a supporting metric, not the sole quality gate. Untested critical behavior is not acceptable even if the overall percentage looks good.
> - Configure coverage tooling to exclude test files (e.g., `tests/`) so metrics reflect application code, not tests.
> - Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement. Examples: Python `Protocol`-only modules consumed only under `TYPE_CHECKING`, TypeScript interface/type-only files, and C# interface-only files. Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold.

Note: CLAUDE.md's embedded "General Unit Test Policy" section states an 80% repo-wide floor with a
COM/VSTO/WinForms testable-denominator exemption; `.claude/rules/general-unit-test.md` states an 85%/75%
uniform floor across tiers. Per CLAUDE.md's own Policy Compliance Order, CLAUDE.md is read first (position
1) and `.claude/rules/*` files layer on top per position 2/3. This plan's coverage gate (P3-T6, AC-5) is a
no-regression-on-changed-lines check against this test-only, zero-production-file change, so the two
floor values do not create a conflicting instruction for this specific task; both formulations agree that
changed-line coverage must not regress and that new test-file lines are exercised by execution. No halt is
required for this plan's scope.

### csharp.md — Toolchain (verbatim)

> 1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use `dotnet format`. Command: `dotnet tool run csharpier .` or `csharpier .`
> 2. **Linting — .NET Analyzers**: C# code must pass Roslyn/.NET analyzer diagnostics. Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
> 3. **Type Checking — Nullable Analysis**: Enable nullable reference types and fail on warnings. Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
> 4. **Testing — MSTest + Moq + FluentAssertions**: Run tests with: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
>
> Run the toolchain in order: format → lint → type-check → test. Restart from step 1 if any step fails or changes files.
