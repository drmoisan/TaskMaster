# TaskMaster — Claude Instructions

## Project Guidelines

- Repository uses policy skills under `.claude/skills/`, including `csharp-code-change-policy` requiring environment-appropriate C# commands and strict toolchain order.
- C# tests must use **MSTest** as the framework, **Moq** for mocking, and **FluentAssertions** for assertions.
- C# code must pass CSharpier formatting, .NET analyzer diagnostics, nullable checks, and MSTest test coverage.

## Policy Compliance Order

The four core policies below are embedded directly in this file and apply to every session without requiring explicit skill loads. Apply them in this order:

1. This file (CLAUDE.md) — all sections
2. General Code Change Policy (§ below)
3. General Unit Test Policy (§ below)
4. For C#: C# Code Change Policy (§ below) and C# Unit Test Policy (§ below)

---

## General Code Change Policy

**CRITICAL**: When implementing any code, tests, tasks, or scripts, you **must** adhere to these repo policies **without exception**. This includes but is not limited to adding, removing, or changing any code, tasks, scripts, modules, packages, tests or their components.

Read each policy document **thoroughly** before starting work. Implement them **exactly as written**. Do not interpret, modify, or skip any requirements. If you encounter **any** conflicting instructions, halt and notify the user.

Language-specific standards (e.g. for C#) are defined in additional skill files and **layer on top of** this general policy.

**Reading order / authority:** Apply this general policy first, then any language-specific code-change skills, then any unit-test addenda. Operational guidance (e.g., developer tooling, CI docs) sits underneath these policies.

### Before Making Changes

- [ ] Clarify the objective. Begin reasoning from clearly stated assumptions or axioms.
- [ ] Read existing change plans (e.g., `change-plan.md`).
- [ ] Document the plan to make changes. If it is part of an existing change plan, make any relevant updates to the plan before executing.

---

### Bugfix Workflow (all languages, defects only)

Use this workflow only when addressing a bug or defect. Feature work, refactors, and
new capabilities should follow the general planning steps and design principles
rather than this bugfix sequence.

1. **Create a failing regression test first**
   - Add the smallest deterministic test that reproduces the bug using the project's standard test layout.
   - Ensure the test fails before the fix and will pass after; avoid external services or temporary files.

2. **Implement the minimal, targeted fix**
   - Change only what is needed to make the failing test pass; keep boundaries intact and avoid opportunistic refactors.
   - If you uncover deeper design problems, open a new issue instead of widening scope.

3. **Verify locally before review**
   - Re-run the original repro and the new regression test.
   - Run the full toolchain in order (format → lint → type-check → test) using the repo-standard commands or tasks; rerun from the start if any step changes files or fails.

---

### 1. Design Principles

High-level design priorities (applies to all languages):

1. **Simplicity first** — Prefer the simplest design that works and is easy to read. Avoid cleverness and deep indirection.
2. **Reusability** — Factor out logic that is clearly reusable into small methods or pure functions. Avoid copy-paste.
3. **Extensibility** — Design public APIs so they can be extended without breaking callers.
4. **Separation of concerns** — Keep **pure logic** (transforms, calculations, parsing) separate from I/O (disk, network, DB), UI/CLI, and framework-specific glue.

---

### 2. Classes, Functions, and APIs

**Overall rule:** Use **strongly-typed, well-structured classes** to model domain concepts and workflows. Use **functions** (or equivalent) for small, stateless helpers and glue code.

#### 2.1 Prefer classes for domain concepts and workflows

Create a class when at least one is true:
- There is a **clear domain concept** with data + behavior.
- You have **state + invariants** that should travel together.
- You expect **multiple implementations** behind a common interface.
- You are modeling a **multi-step workflow** that shares context.

When you use classes: keep methods small and focused; avoid god objects.

#### 2.2 Use functions for small, pure helpers

Create a standalone function when:
- The operation is **pure, stateless, and simple**.
- It is a **small helper** that doesn't naturally belong on a specific domain class.

#### 2.3 Interfaces and contracts

- Use interfaces / abstract types / protocols when multiple implementations are likely.
- Public methods and functions must have clear, documented contracts (inputs, outputs, invariants).

---

### 3. Error Handling, Logging, and Contracts

1. **Error handling** — Fail **fast and explicitly**. Don't silently ignore errors or broad-catch unless you immediately re-raise or propagate with added context.
2. **Logging** — Use the project's logging pattern instead of ad-hoc print/console output. Log at appropriate levels and include enough context to debug issues.
3. **Contracts / invariants** — Enforce invariants at construction/initialization time. Use assertions only for **internal sanity checks**, not user-facing error handling.

---

### 4. Module & File Structure

1. Keep modules **cohesive** — A module/file should have a clear purpose. Avoid dumping unrelated classes/functions into the same file. Do not exceed 500 lines for any one file.
2. Make the public surface area **small and intentional**.
3. Prefer clear, explicit imports. Avoid circular dependencies.

---

### 5. Naming, Docs, and Comments

1. Names should be descriptive, not cryptic. Abbreviations are okay only when standard and widely understood.
2. Public classes and methods should have a short description covering what it does, important arguments, and what it returns or side effects.
3. Comment **why**, not what. If you use workarounds or non-obvious patterns, add a short comment explaining the reasoning.

---

### 6. Performance, I/O, and Dependencies

1. Prefer clarity first; optimize only where there is a demonstrated need.
2. Isolate I/O (disk, network, APIs) into specific classes or modules. Core domain logic should be testable **without** touching the network or filesystem. **Use of temporary files within tests is strictly prohibited.**
3. Use only the libraries already approved in the project unless specifically told to add more.

---

### 7. How to Interact with Existing Code

1. Where the repo already has a clear style, **match that style**.
2. Avoid breaking public APIs. If a breaking change is necessary, call it out clearly.
3. Treat existing unit tests as **part of the spec**.

---

### 8. After Making Changes

#### 1. Run the full toolchain (no shortcuts)

Run the full toolchain in this exact order and repeat it until everything passes:

1. **Formatting**
2. **Linting**
3. **Type checking**
4. **Testing**

Treat these four steps as one **toolchain pass**. If any step fails or auto-fixes anything, restart from step 1. You **may not stop** this loop while any step is failing.

When you report back, explicitly state which commands you ran and that all four steps passed without errors in the final pass.

#### 2. Summarize key changes and rationale

- Summarize the key changes made and how they relate to the original objective.
- Explain any important design choices and other options you considered but did not implement.

#### 3. Update supporting documents

- Update any supporting documents (e.g., README, design docs, runbooks).
- Update any workplan, change plan, or instructions document to show progress and reflect the new behavior.

#### 4. Provide clear next steps

- Provide clear development next steps (what should happen next, and by whom).
- If development is complete, provide detailed instructions on usage and any operational caveats.

---

## C# Code Change Policy

This policy **extends** the General Code Change Policy above and applies to all **C# source, test, and build-configuration files** (`*.cs`, `*.csproj`, `*.props`, `*.targets`) in this repo.

You must:
- Apply **all** rules in the general code change policy.
- Apply **all** C#-specific rules in this section.
- Apply the unit test policies (General Unit Test Policy and C# Unit Test Policy below) for any work involving tests.

If you encounter any conflicting instructions between these documents, **halt and notify the user.**

---

### C#1. Tooling & Baseline for C#

These are the required tools for C# code in this repo:

1. **Formatting — `csharpier`**
   - All C# source files (`*.cs`) must be formatted with `csharpier`.
   - Do **not** use `dotnet format` — it loads the solution/project model and can mis-handle legacy VSTO / .NET Framework projects by rewriting `.csproj` files.
   - `csharpier` is file-based and does not load the solution or project model, so it cannot rewrite a `.csproj` as a side effect of parsing the build graph. It is **not** restricted to `*.cs`: CSharpier 1.2.6 also accepts and processes `*.xml` and `packages.config`. `*.csproj`, `*.props` and `*.targets` are kept out of the check by `.csharpierignore`, not by any inherent CSharpier behavior.
   - Do not hand-format; if a diff disagrees with `csharpier`, formatter output wins.
   - Run `dotnet tool restore` once per clone or worktree before the first invocation.
   - Approved commands (CSharpier is pinned to 1.2.6 by `dotnet-tools.json`; v1 requires a subcommand, so the bare-path form does not run):
     - Apply formatting: `dotnet tool run csharpier format .`
     - Verify, read-only, CI parity: `dotnet tool run csharpier check .`
   - Always invoke through `dotnet tool run` so the manifest-pinned version is used. Do not invoke a globally installed `csharpier`: a different global version produces diffs that disagree with `.github/workflows/_format-check.yml`, which runs the pinned version after `dotnet tool restore`.

2. **Linting / Static Analysis — .NET analyzers**
   - C# code must pass Roslyn/.NET analyzer diagnostics configured by `.editorconfig`, `.globalconfig`, and project properties.
   - Enforce analyzer diagnostics in build using `EnableNETAnalyzers` and `EnforceCodeStyleInBuild`.
   - Prefer fixing diagnostics over suppressing them.
   - Approved commands (PowerShell):
     - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   - Use `/t:Rebuild`, not `/t:Build`. Analyzer diagnostics are produced during compilation, and MSBuild's incremental up-to-date check compares timestamps without invalidating on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers. `.github/workflows/_build-analyzers.yml` uses `/t:Build /m` for its analyzer step because a runner checkout is always cold; a local working tree is not.

3. **Type Checking — C# compiler + nullable analysis**
   - Treat C# compiler diagnostics and nullable-flow warnings as first-class type-safety checks.
   - Nullable enforcement in this repository is **per-file opt-in**: a file participates in nullable analysis when it carries a `#nullable enable` directive, and `/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to build errors.
   - Avoid introducing nullable warnings; fix the root null-state issue instead.
   - Approved commands (PowerShell):
     - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
   - This is character-for-character the command in `.github/workflows/_build-nullable.yml` (step "Build with nullable warnings treated as errors"). Two properties of it are load-bearing and must not be "restored":
     - **Do not add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts every file which has never adopted the pragma. Forcing it produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero errors without it, and CI omits it deliberately. Removing it loses no enforcement over any file that has opted in.
     - **Do not use `/t:Build`.** MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every project: the gate cannot fail.

> **Testing tools and behavior are defined in the unit test policies.** Do not define test behavior here; instead, obey the General Unit Test Policy and C# Unit Test Policy below.

---

### C#2. C# Design & Type-Safety Principles

1. **Strong contracts and explicit APIs** — Public methods, constructors, and properties must express clear contracts. Use explicit types at public boundaries; use `var` only when the type is obvious.
2. **Null-safety by default** — Keep nullable reference types enabled. Model optional values explicitly with nullable annotations and guard clauses.
3. **Prefer composition and focused types** — Keep classes cohesive and scoped to one core responsibility. Favor composition over inheritance unless polymorphism is a clear requirement.
4. **Asynchrony and resource safety** — Use `async`/`await` for I/O-bound operations. Prefer `using`/`await using` for disposable resources.

---

### C#3. Classes, Methods, and APIs (C#-Specific Guidance)

#### C#3.1 Classes for domain concepts and workflows

Use classes/records when modeling domain concepts with state + behavior, protecting invariants, providing multiple implementations behind interfaces, or orchestrating multi-step workflows.

When using classes/records: keep methods small and focused; avoid god objects; prefer immutable records/value objects for data-centric models.

#### C#3.2 Methods and local functions for focused logic

Use methods/local functions when implementing narrow, deterministic behavior or encapsulating reusable, stateless transformations. Name methods by behavior. Keep branching shallow. Extract helper methods instead of deeply nested conditionals.

#### C#3.3 Interfaces and contracts

- Use interfaces when multiple implementations are expected.
- Keep public APIs stable and avoid unnecessary breaking changes.
- Document non-obvious side effects and failure modes.

---

### C#4. Error Handling, Logging, and Contracts (C#)

1. **Exceptions** — Fail fast with explicit exceptions when invariants are violated. Avoid catching broad `Exception` unless at a clear boundary and with added context.
2. **Logging** — Use the repository/project logging pattern, not ad-hoc console output in production code. Log actionable context at appropriate levels.
3. **Contracts / invariants** — Validate constructor and method preconditions. Use `Debug.Assert` only for internal invariants, not user-facing validation.

---

### C#5. Module & File Structure (C#)

1. Keep files focused on one responsibility area. Keep file size under the repo limit in the General Code Change Policy.
2. Keep public surface area intentional and minimal. Prefer `internal` for non-public APIs.
3. Prefer explicit `using` directives at file scope. Avoid circular dependencies.

---

### C#6. Naming, Docs, and Comments (C#)

1. `PascalCase` for types and public members. `camelCase` for local variables and private fields/parameters. Use descriptive names over abbreviations.
2. Public APIs should include XML documentation comments when behavior or contract is non-obvious.
3. Comment **why**, not what. Keep comments synchronized with behavior.

---

### C#7. Dependencies and Analyzer Configuration (C#)

- Prefer built-in .NET SDK analyzers and configuration through `.editorconfig` / `.globalconfig`.
- Use project-level properties (`EnableNETAnalyzers`, `AnalysisLevel`, `AnalysisMode`, `EnforceCodeStyleInBuild`) rather than ad-hoc per-command behavior where possible.
- Avoid adding external dependencies unless unavoidable and approved by the project direction.
- If suppression is unavoidable, keep it as narrow as possible and document the rationale in-code.

---

## General Unit Test Policy

This policy applies to **all unit tests** in this repository, regardless of language or framework.

Every new or modified unit test must adhere to these guidelines.

---

### UT1. Core Principles

- **Independence** — Tests must be able to run in any order without impacting each other.
- **Isolation** — Each unit test should target a single function, method, or unit of behavior so failures clearly identify the faulty unit.
- **Fast Execution** — Tests must be fast enough to support frequent runs and rapid feedback loops.
- **Determinism** — Given the same inputs and environment, tests must produce the same results. Avoid flakiness.
- **Readability and Maintainability** — Test names, structure, and assertions should be clear and easy to understand.

---

### UT2. Coverage and Scenarios

- **Comprehensive Coverage (within reason)**
  - Aim to exercise critical paths and important edge conditions.
  - Configure coverage tooling to exclude test files (e.g., `tests/`), so metrics reflect the application code, not the tests themselves.
  - Repository-wide line coverage must remain `>= 80%`.
  - **COM/VSTO/WinForms coverage exemption (testable denominator).** The 80% floor applies to the **testable denominator** — production-only first-party code, after excluding:
    - (a) VSTO add-in lifecycle classes (entry points, ribbon event handlers, COM utility registration) that cannot be unit-tested without a live Outlook process;
    - (b) WinForms form-derived classes and Designer-generated code;
    - (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`, `ToDoModel`, and `Tags` that directly depend on `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder` without an injectable seam.

    These classes are formally exempted from the 80% floor. Exemption is applied via `[ExcludeFromCodeCoverage]` attributes in source code (reviewable in PRs) or via `coverage.config` assembly-level excludes for near-wholly-untestable assemblies. **Authority**: This exemption must be ratified by the project maintainer and is tracked in `feature/csharp-coverage-uplift`. Testable seams within otherwise-COM-bound assemblies (e.g., `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, path/settings helpers) are explicitly NOT exempt and must meet the `>= 80%` floor.
  - Any new modules, classes, or methods added must target `>= 90%` coverage.
  - Code changes or refactors must not reduce coverage for the lines that were changed.
  - Coverage is a supporting metric, not the sole quality gate; untested critical behavior is not acceptable even if the overall percentage looks good.

- **Scenario Completeness**
  For each unit or behavior, tests should cover:
  - Positive flows with valid inputs.
  - Negative flows for invalid or missing inputs.
  - Edge cases and boundary conditions.
  - Error-handling behavior.
  - Concurrency behavior when relevant.
  - State transitions for stateful components.

---

### UT3. Test Structure and Diagnostics

- **Clear Failure Messages** — Assertions should produce clear, actionable failure messages that make it easy to see what went wrong.
- **Arrange–Act–Assert pattern** — Organize tests into: Arrange (set up inputs, environment, and dependencies), Act (execute the behavior under test), Assert (verify outcomes via assertions).
- **Document Intent** — Each test must clearly communicate its purpose: use descriptive test names, and/or include a short docstring or comment summarizing the scenario and expected outcome.

---

### UT4. External Dependencies and Environment

- **Avoid External Dependencies** — Unit tests must not depend on external services such as databases, networks, remote APIs, or external processes.
- **Use Mocks / Stubs as Needed** — When code interacts with external systems or heavy resources, use mocks, stubs, or fakes to isolate the unit under test.
- **Environment Stability** — Tests must not rely on mutable global state or external configuration that can change between runs. **Creation and use of temporary files on the local filesystem is expressly prohibited** unless explicitly authorized as an exception.
  - Currently approved exceptions: none.

---

### UT5. Policy Audit

Before submitting any change that includes unit tests:

- Review each new or modified test against this policy.
- Confirm that:
  - It is independent, isolated, fast, and deterministic.
  - It is readable and clearly documents its intent.
  - It covers relevant positive, negative, edge, and error scenarios.
  - It does not rely on external dependencies without proper mocking/stubbing.

If any test cannot comply with these rules for a good reason, **call out the exception explicitly** in the change description.

---

## C# Unit Test Policy

This policy **extends** the General Unit Test Policy above and applies to all C# unit tests in this repo.

You must follow **both**:
- The General Unit Test Policy, and
- The C#-specific rules below.

If there is any conflict between these documents, halt and notify the user.

---

### CUT1. Framework Selection

- **Testing framework** — Use **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`) for C# unit tests in this repository. Do not introduce xUnit or NUnit into existing test projects.

---

### CUT2. C#-Specific Libraries and Conventions

- **Mocking library** — Use **Moq** for mocks/stubs in C# unit tests.
- **Assertion library** — Prefer **FluentAssertions** for new and updated assertions. Use MSTest `Assert` APIs only when FluentAssertions is not practical for a specific assertion shape.
- **MSTest style** — Use `[TestClass]`, `[TestMethod]`, and other MSTest attributes from `Microsoft.VisualStudio.TestTools.UnitTesting`.

---

### CUT3. C# Toolchain Command Selection

For C# work, use these concrete commands for the general policy toolchain loop:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

The loop behavior (restart rules, must-pass requirements, and audit expectations) is defined by the General Code Change Policy above.

---

## Tone Policy

- Use a strictly professional, factual, and neutral tone in all user-facing responses.
- Do not use jokes, humor, metaphors, playful analogies, emojis, GIFs, banter, or conversational filler.
- Avoid motivational hype or theatrical phrasing.
- If wording sounds informal or playful, rewrite it in neutral business language.

## C# Toolchain (run in this exact order)

1. **Format**: `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)
2. **Analyze**: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type-check**: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. **Test**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

If any step fails, fix and restart from step 1.

## Key Skills Reference

### Background skills (always read explicitly when invoked)

- `policy-compliance-order` — mandatory policy reading order and hard constraints → `.claude/skills/policy-compliance-order/SKILL.md`
- `atomic-plan-contract` — atomic plan format, Phase 0, and final QA loop rules → `.claude/skills/atomic-plan-contract/SKILL.md`
- `acceptance-criteria-tracking` — AC check-off protocol and status summary → `.claude/skills/acceptance-criteria-tracking/SKILL.md`
- `evidence-and-timestamp-conventions` — ISO-8601 timestamps and evidence artifact locations → `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `feature-promotion-lifecycle` — promotion workflow from potential entry to active feature folder → `.claude/skills/feature-promotion-lifecycle/SKILL.md`
- `csharp-change-budget-router` — budget-first routing for C# work → `.claude/skills/csharp-change-budget-router/SKILL.md`
- `csharp-orchestration-state-machine` — checkpoint and resume protocol for C# orchestration → `.claude/skills/csharp-orchestration-state-machine/SKILL.md`
- `pr-context-artifacts` — PR context artifact locations → `.claude/skills/pr-context-artifacts/SKILL.md`
- `pr-base-branch-merge-base` — deterministic base branch resolution → `.claude/skills/pr-base-branch-merge-base/SKILL.md`
- `policy-audit-template-usage` — policy audit artifact creation rules → `.claude/skills/policy-audit-template-usage/SKILL.md`
- `remediation-handoff-atomic-planner` — remediation trigger and atomic_planner handoff → `.claude/skills/remediation-handoff-atomic-planner/SKILL.md`
- `skill-canonical-location-audit` — canonical location duplication audit → `.claude/skills/skill-canonical-location-audit/SKILL.md`

### Agent persona skills (invoke explicitly)

- `/orchestrator` — language-agnostic end-to-end feature/bug delivery orchestration
- `/csharp-orchestrator` — C#-specific end-to-end orchestration
- `/atomic-planner` — generate phased atomic implementation plans
- `/atomic-executor` — execute atomic plans verbatim with strict task-by-task verification
- `/csharp-atomic-planner` — C#-specific atomic planning
- `/csharp-atomic-executor` — C#-specific atomic execution with csharpier/msbuild/vstest gates
- `/csharp-typed-engineer` — design and implement testable C# code with MSTest coverage
- `/feature-reviewer` — review feature branches; produce policy/code/feature audit artifacts
- `/task-researcher` — deep research into implementation approaches; writes to `artifacts/research/`
- `/make-skill-template` — scaffold new Claude skill files

### User-invocable commands

- `/orchestrate-csharp-work` — run end-to-end C# workflow via csharp-orchestrator
- `/generate-atomic-plan` — generate and validate an atomic implementation plan
- `/review-feature` — review a feature branch and produce audit artifacts
- `pr-author` — write a GitHub PR body file plus a SHA-256 provenance receipt from PR context artifacts (required before `gh pr create`/`gh pr edit --body-file`; enforced by `enforce-pr-author-skill.ps1`) → `.claude/skills/pr-author/SKILL.md`
