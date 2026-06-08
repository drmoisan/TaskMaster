---
paths:
  - "**/*.cs"
  - "**/*.csproj"
description: C#-specific toolchain and coding standards.
---

# C# Code Standards

This rule file summarizes the C#-specific policies for this repository.

## Toolchain

1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use `dotnet format`. Command: `dotnet tool run csharpier .` or `csharpier .`
2. **Linting — .NET Analyzers**: C# code must pass Roslyn/.NET analyzer diagnostics. Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type Checking — Nullable Analysis**: Enable nullable reference types and fail on warnings. Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
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

### Time seam (TimeProvider) — guidance only

For new or touched time-dependent code, inject `System.TimeProvider` through the constructor instead of calling the clock directly:

- Production supplies `TimeProvider.System`.
- Tests supply `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` to make time deterministic.
- Do not call `DateTime.Now`, `DateTime.UtcNow`, or `DateTimeOffset.Now` directly in new/touched code; obtain time via the injected `TimeProvider` (for example `GetUtcNow()` / `GetLocalNow()`).

This is guidance only: it introduces no runtime behavior change and does not require rewriting existing call sites. `Microsoft.Bcl.TimeProvider` (the .NET Framework backport of `System.TimeProvider`) is already present in UtilitiesCS, so the seam is available without adding a new production dependency. Legacy call-site migration is follow-up work, not a requirement of adopting this guidance.

## Analyzer Stack (Issue #181)

This repository adopts a fixed set of FIVE static-analysis packages, wired first-party only (the 15 first-party projects; the 4 vendored projects — SVGControl, SVGControl.Test, UtilitiesSwordfish.NET.General, UtilitiesSwordfish.Test — are excluded):

1. **Meziantou.Analyzer**
2. **SonarAnalyzer.CSharp**
3. **Roslynator.Analyzers**
4. **AsyncFixer**
5. **Microsoft.CodeAnalysis.BannedApiAnalyzers**

### Mechanism

- Each first-party project references its analyzers via a `packages.config` `<package ... developmentDependency="true" />` entry plus an explicit `<Analyzer Include="..\packages\<id>.<version>\analyzers\dotnet\cs\<dll>" />` item in the project's analyzer `<ItemGroup>`. This file-based wiring is used because the projects are legacy (non-SDK, `packages.config`) VSTO/.NET Framework projects; no PackageReference, no Central Package Management, and no `dotnet restore` are introduced.
- For this repo's Roslyn 5.6 (VS18) toolchain, use the analyzer DLLs from the Meziantou `roslyn5.0` and Roslynator `roslyn4.7` subfolders.
- **Banned symbols** are enforced by BannedApiAnalyzers using a repo-root `BannedSymbols.txt` referenced by each first-party project as `<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />`. The banned targets are `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, and `Task.Delay`. RS0030 is held at `severity = suggestion` for initial rollout (existing call sites are not build-broken); promotion to `warning` after legacy cleanup is documented follow-up work.

### Severity-first ordering invariant

All new analyzer rule severities are configured in `.editorconfig` at `severity = suggestion` (never `warning`/`error`) BEFORE any `<Analyzer Include>` item is wired into a project. This is required because the type-check toolchain step runs `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`, which promotes any `warning`-severity analyzer diagnostic to a build error. Keeping new analyzer diagnostics at `suggestion` (message level) prevents the analyzer adoption from breaking the protected nullable gate.

### Deferred analyzer — SecurityCodeScan.VS2019

SecurityCodeScan.VS2019 was evaluated and **deferred** (not silently omitted) from this rollout. Version 5.6.7 is incompatible with this repository's Roslyn 5.6 (VS18) analyzer loader: its types fail to initialize (`TypeInitializationException` → `FileNotFoundException` for `YamlDotNet, Version=11.0.0.0`), which the compiler reports as warning **CS8032**. CS8032 is a compiler warning, not an analyzer rule ID, so it cannot be set to `suggestion` via `.editorconfig`; under `/p:TreatWarningsAsErrors=true` it is promoted to an error and breaks the protected nullable build. SecurityCodeScan.VS2019 is therefore dropped from the analyzer set entirely. **No CS8032 suppression** (no `dotnet_diagnostic.CS8032` entry and no `<WarningsNotAsErrors>` containing CS8032) is introduced, and no substitute security analyzer is added. Re-evaluation is follow-up work pending a Roslyn-5.x-compatible security analyzer.

## Prohibited Behaviors

- Broad refactors across unrelated projects or files.
- Introducing heavy generic abstraction frameworks without need.
- Creating analyzer debt and deferring cleanup.
- Weakening assertions or relaxing test expectations to make tests pass.
- Adding sleeps, retries, or timing hacks to mask flaky behavior.
- Reporting success without running the required toolchain.
