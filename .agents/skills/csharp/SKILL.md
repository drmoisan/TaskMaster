---
name: csharp
paths:
  - "**/*.cs"
  - "**/*.csproj"
description: C#-specific toolchain and coding standards.
---

# C# Code Standards

Legacy C# variant resource for Codex push-down.

This rule file summarizes the C#-specific policies for this repository.

## Toolchain

1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use `dotnet format`. Run `dotnet tool restore` first when the manifest tool has not been restored. Apply formatting with `dotnet tool run csharpier format .` and verify read-only with `dotnet tool run csharpier check .`. Always invoke through `dotnet tool run` so the manifest-pinned CSharpier version is used.
2. **Linting — .NET Analyzers**: C# code must pass Roslyn/.NET analyzer diagnostics. Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. `/t:Rebuild` is intentional for a warm local worktree: `/t:Build` can skip `CoreCompile` through MSBuild incrementality and exit 0 without running analyzers. CI may retain `/t:Build` on a cold checkout.
3. **Type Checking — Nullable Analysis**: Compiler and nullable-flow diagnostics must pass with warnings as errors. Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`. `/t:Rebuild` is required locally so compiler and nullable-flow diagnostics actually run. Projects opt into nullable per file with `#nullable enable`; do not pass `/p:Nullable=enable`, which opts every unannotated file in at once.
4. **Testing — MSTest + Moq + FluentAssertions**: Run tests with: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Run the toolchain in order: format → lint → type-check → test. Restart from step 1 if any step fails or changes files.

## Coding Standards

- **Naming**: `PascalCase` for types and public members. `camelCase` for locals and private fields/parameters.
- **Null safety**: Keep nullable reference types enabled. Model optional values with nullable annotations and guard clauses.
- **Composition over inheritance**: Keep classes cohesive and scoped to one responsibility. Favor composition unless polymorphism is a clear requirement.
- **Async/await**: Use `async`/`await` for I/O-bound operations. Prefer `using`/`await using` for disposable resources.
- **Exceptions**: Fail fast with explicit exceptions. Avoid broad `catch (Exception)` unless at a defined boundary with added context.
- **Public surface**: Keep public API surface intentional and minimal. Prefer `internal` for non-public APIs.
- **XML docs**: Public APIs should include XML documentation comments when behavior or contract is non-obvious.

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

## Deterministic Test Rules

Unit tests must not depend on network, mutable machine PATH or profile state, implicit working-directory assumptions, or external services. Use seam-based mocking for all external boundaries (processes, HTTP, filesystem, clocks). Tests must produce identical results in the IDE test runner and in CLI runs so local and CI behavior agree.

## DI Seams

Introduce the smallest seam that enables reliable unit testing. Apply in this order of preference:

1. **Interface seam (preferred)** — extract boundary calls into narrow purpose-specific interfaces (for example, `IProcessRunner`, `IFileSystem`, `IClock`). Keep interfaces minimal.
2. **Injectable delegate seam** — use a narrow `Func<>`/`Action<>` delegate for a single call path when a full interface is excessive. Default behavior must remain safe and deterministic.
3. **Adapter seam for static or third-party APIs** — wrap the static or third-party call behind a small adapter so tests can mock the adapter with Moq.

## Prohibited Behaviors

- Broad refactors across unrelated projects or files.
- Introducing heavy generic abstraction frameworks without need.
- Creating analyzer debt and deferring cleanup.
- Weakening assertions or relaxing test expectations to make tests pass.
- Adding sleeps, retries, or timing hacks to mask flaky behavior.
- Reporting success without running the required toolchain.
