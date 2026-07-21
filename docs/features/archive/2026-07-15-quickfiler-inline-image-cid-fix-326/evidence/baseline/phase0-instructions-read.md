# Phase 0 — Policy Read Evidence

- **Timestamp:** 2026-07-15T23-27
- **Feature:** quickfiler-inline-image-cid-fix (#326)
- **Workspace root used for this execution:** `C:/Users/DanMoisan/repos/TaskMaster-wt/quickfiler-inline-image-cid-fix-326`
  (the plan's embedded workspace-root reference to `.../TaskMaster/.claude/worktrees/agent-a1e77dc4a849cd790`
  is a stale planning-worktree path from an earlier preparation run and was not used; per the resuming
  agent's explicit instruction, the equivalent files were read at the current worktree root instead.)

## Policy Order

1. `CLAUDE.md` (this file, at the current workspace root)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

## Files Read

1. `C:/Users/DanMoisan/repos/TaskMaster-wt/quickfiler-inline-image-cid-fix-326/CLAUDE.md`
2. `C:/Users/DanMoisan/repos/TaskMaster-wt/quickfiler-inline-image-cid-fix-326/.claude/rules/general-code-change.md`
3. `C:/Users/DanMoisan/repos/TaskMaster-wt/quickfiler-inline-image-cid-fix-326/.claude/rules/general-unit-test.md`
4. `C:/Users/DanMoisan/repos/TaskMaster-wt/quickfiler-inline-image-cid-fix-326/.claude/rules/csharp.md`

## Quoted Sections

### CLAUDE.md — Policy Compliance Order (P0-T1)

> The four core policies below are embedded directly in this file and apply to every session without
> requiring explicit skill loads. Apply them in this order:
>
> 1. This file (CLAUDE.md) — all sections
> 2. General Code Change Policy (§ below)
> 3. General Unit Test Policy (§ below)
> 4. For C#: C# Code Change Policy (§ below) and C# Unit Test Policy (§ below)

### `.claude/rules/general-code-change.md` — Mandatory Toolchain Loop (P0-T2)

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
>
> Mutation testing and golden tests run in pre-merge or nightly pipelines, not the per-commit loop.

(Note: this feature's plan (`plan.2026-07-15T16-53.md`) scopes its Phase 4 final QA loop to the
four-step C# toolchain — CSharpier, analyzers, nullable build, vstest — per the C# Unit Test Policy's
`CUT3` command selection and CLAUDE.md's "C# Toolchain (run in this exact order)" section; this
executor follows the plan's own Phase 4 task list exactly, which is consistent with the plan-approved
scope for this bugfix.)

### `.claude/rules/general-unit-test.md` — Coverage Requirements (P0-T3)

> - **Line coverage must remain >= 85% across all tiers (T1–T4).**
> - **Branch coverage must remain >= 75% across all tiers (T1–T4).**
> - Code changes or refactors must not reduce coverage for the lines that were changed.
> - Tier-specific lower coverage thresholds are not used in this repository. See `.claude/rules/quality-tiers.md` for the full tier system.
> - Coverage is a supporting metric, not the sole quality gate. Untested critical behavior is not acceptable even if the overall percentage looks good.
> - Configure coverage tooling to exclude test files (e.g., `tests/`) so metrics reflect application code, not tests.
> - Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement. ... This is a clarification only; it does not lower any coverage threshold.

(Note: CLAUDE.md's embedded C# Unit Test Policy separately states an 80%/90% COM/VSTO-exemption
regime tracked under `feature/csharp-coverage-uplift`. Both documents were read per policy order;
this executor applies the plan's own Phase 4/P4-T5 acceptance criteria, which cite both the
>=85%/>=75% repo-wide rule and the >=90% new-module target from CLAUDE.md.)

### `.claude/rules/csharp.md` — Toolchain (P0-T4)

> 1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use `dotnet format`. Command: `dotnet tool run csharpier .` or `csharpier .`
> 2. **Linting — .NET Analyzers**: C# code must pass Roslyn/.NET analyzer diagnostics. Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
> 3. **Type Checking — Nullable Analysis**: Enable nullable reference types and fail on warnings. Command: `msbuild <solution>.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
> 4. **Testing — MSTest + Moq + FluentAssertions**: Run tests with: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
>
> Run the toolchain in order: format → lint → type-check → test. Restart from step 1 if any step fails or changes files.
