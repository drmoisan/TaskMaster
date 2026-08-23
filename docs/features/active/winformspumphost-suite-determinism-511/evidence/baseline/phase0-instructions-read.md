# Phase 0 — Policy Instructions Read

Timestamp: 2026-08-22T09-12

Policy Order:

1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and coding standards)

Files read, in the order read:

- `CLAUDE.md` — read end to end (P0-T1)
- `.claude/rules/general-code-change.md` — read end to end (P0-T2)
- `.claude/rules/general-unit-test.md` — read end to end (P0-T3)
- `.claude/rules/csharp.md` — read end to end, 96 lines (P0-T4)

All four paths are relative to the worktree root
`<repo-root>\.claude\worktrees\agent-ad37a256a0fb60243`.

---

## P0-T1 — `CLAUDE.md`

The four numbered policy sections `CLAUDE.md` embeds directly, each of which applies to every
session without requiring an explicit skill load:

1. General Code Change Policy
2. General Unit Test Policy
3. C# Code Change Policy
4. C# Unit Test Policy

Quoted Policy Compliance Order list, verbatim from `CLAUDE.md`:

> ## Policy Compliance Order
>
> The four core policies below are embedded directly in this file and apply to every session without requiring explicit skill loads. Apply them in this order:
>
> 1. This file (CLAUDE.md) — all sections
> 2. General Code Change Policy (§ below)
> 3. General Unit Test Policy (§ below)
> 4. For C#: C# Code Change Policy (§ below) and C# Unit Test Policy (§ below)

---

## P0-T2 — `.claude/rules/general-code-change.md`

Quoted 500-line file-size limit, verbatim:

> ## File Size Limit
>
> - No production code, test code, or reusable script file may exceed **500 lines**.
> - Exceptions: temporary throwaway scripts created and deleted within an agent session; raw text fixtures for language-processing test data; Markdown documentation files.

Quoted mandatory toolchain loop, verbatim:

> ## Mandatory Toolchain Loop
>
> Run the full seven-stage toolchain in this exact order and repeat until all stages pass in a single pass:
>
> 1. **Formatting** (e.g., Black, Prettier, CSharpier, Invoke-Formatter)
> 2. **Linting** (e.g., Ruff, ESLint, PSScriptAnalyzer, .NET analyzers)
> 3. **Type checking** (e.g., Pyright, TSC, nullable analysis; skip for PowerShell)
> 4. **Architecture-boundary tests** (e.g., dependency-cruiser, NetArchTest.Rules)
> 5. **Unit tests** (e.g., Pytest, Jest, MSTest, Pester) including property-based tests where applicable per `quality-tiers.md`
> 6. **Contract / schema compatibility checks** (e.g., oasdiff, schema-snapshot diff)
> 7. **Integration tests**
>
> **Restart from step 1** if any stage fails or auto-fixes any files. Do not stop the loop until all seven stages complete without errors in a single pass.
>
> Mutation testing and golden tests run in pre-merge or nightly pipelines, not the per-commit loop.

Relevance to this child: Binding Constraint 5 of the plan applies the 500-line cap to each of the
three touched test files. Pre-change counts are recorded in P0-T7.

---

## P0-T3 — `.claude/rules/general-unit-test.md`

Quoted coverage thresholds, verbatim:

> ## Coverage Requirements
>
> - **Line coverage must remain >= 85% across all tiers (T1–T4).**
> - **Branch coverage must remain >= 75% across all tiers (T1–T4) for languages whose coverage tooling measures branch coverage.** PowerShell (Pester) and bash (kcov) are the exceptions: neither tool measures branch coverage in any output format, so only the line threshold applies to them and there is no branch-coverage gate. This is a threshold exemption only; PowerShell and bash production files remain in the coverage denominator under the Coverage Exclusion Policy below.
> - Code changes or refactors must not reduce coverage for the lines that were changed.
> - Tier-specific lower coverage thresholds are not used in this repository. See `.claude/rules/quality-tiers.md` for the full tier system.
> - Coverage is a supporting metric, not the sole quality gate. Untested critical behavior is not acceptable even if the overall percentage looks good.
> - Configure coverage tooling to exclude test files (e.g., `tests/`) so metrics reflect application code, not tests.
> - Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement. Examples: Python `Protocol`-only modules consumed only under `TYPE_CHECKING`, TypeScript interface/type-only files, and C# interface-only files. Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold.

Recorded threshold divergence, without reconciliation: `CLAUDE.md` and `.claude/rules/csharp.md`
both state a repository-wide line-coverage floor of `>= 80%` and a `>= 90%` floor for new modules,
classes, and methods, while this rule file states `>= 85%` line and `>= 75%` branch uniformly across
tiers. Both are recorded because this child changes no production file and, per the plan's coverage
note, `QuickFiler/Viewers/ItemViewer.cs` carries a whole-type `[ExcludeFromCodeCoverage]`, so the
operative requirement for this child is **no regression** in `QfcItemController` coverage rather
than clearing any absolute floor.

Quoted Determinism Infrastructure banned-API list, verbatim:

> ## Determinism Infrastructure
>
> All test code must be deterministic. The following infrastructure requirements apply uniformly:
>
> - **Controllable clock** — use a `Clock` interface (TypeScript) or `TimeProvider` (.NET) injected into code under test. Do not read wall-clock time directly in production code under test.
> - **Seeded RNG** — randomness must be supplied via a seedable interface; on test failure the seed must be printed so the failure is reproducible.
> - **Banned APIs in test code** — `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits, and `Date.now()` outside the clock interface are prohibited in tests.
> - **Virtual scheduler / fake timers / `FakeTimeProvider`** — async tests must use the framework's fake-timer facility (`jest.useFakeTimers()` for Jest, `FakeTimeProvider` for .NET) to advance simulated time deterministically.

Relevance to this child: the banned-API list plus Binding Constraint 6 forbid any sleep, retry,
`SpinWait`, timing tolerance, or raised timeout constant in the fix. `PumpTimeoutMs = 60000` and
`TimeoutMs = 30000` retain their current values.

---

## P0-T4 — `.claude/rules/csharp.md`

Quoted four toolchain commands, verbatim:

> ## Toolchain
>
> 1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use `dotnet format`. Run `dotnet tool restore` first when the manifest tool has not been restored. Apply formatting with `dotnet tool run csharpier format .` and verify read-only with `dotnet tool run csharpier check .`. Always invoke through `dotnet tool run` so the manifest-pinned CSharpier version is used.
> 2. **Linting — .NET Analyzers**: C# code must pass Roslyn/.NET analyzer diagnostics. Command: `msbuild <solution>.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. `/t:Rebuild` is intentional for a warm local worktree: `/t:Build` can skip `CoreCompile` through MSBuild incrementality and exit 0 without running analyzers. CI may retain `/t:Build` on a cold checkout.
> 3. **Type Checking — Nullable Analysis**: Compiler and nullable-flow diagnostics must pass with warnings as errors. Command: `msbuild <solution>.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`. `/t:Rebuild` is required locally so compiler and nullable-flow diagnostics actually run. Projects opt into nullable per file with `#nullable enable`; do not pass `/p:Nullable=enable`, which opts every unannotated file in at once.
> 4. **Testing — MSTest + Moq + FluentAssertions**: Run tests with: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
>
> Run the toolchain in order: format → lint → type-check → test. Restart from step 1 if any step fails or changes files.

Quoted six "Prohibited Behaviors" bullets, verbatim:

> ## Prohibited Behaviors
>
> - Broad refactors across unrelated projects or files.
> - Introducing heavy generic abstraction frameworks without need.
> - Creating analyzer debt and deferring cleanup.
> - Weakening assertions or relaxing test expectations to make tests pass.
> - Adding sleeps, retries, or timing hacks to mask flaky behavior.
> - Reporting success without running the required toolchain.

All six bullets are present in the file and all six are quoted above.
