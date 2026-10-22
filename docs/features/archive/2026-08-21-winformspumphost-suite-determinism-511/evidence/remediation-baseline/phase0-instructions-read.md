# Phase 0 — Instructions Read (remediation cycle 1)

Timestamp: 2026-08-23T18-59

Policy Order: `CLAUDE.md` -> `.claude/rules/general-code-change.md` -> `.claude/rules/general-unit-test.md` -> `.claude/rules/csharp.md` -> `docs/features/active/winformspumphost-suite-determinism-511/remediation-inputs.2026-08-23T20-57.md`

## Files read in full

| # | Path | Lines | Task |
| --- | --- | --- | --- |
| 1 | `CLAUDE.md` | 447 | P0-T1 |
| 2 | `.claude/rules/general-code-change.md` | 80 | P0-T2 |
| 3 | `.claude/rules/general-unit-test.md` | 105 | P0-T3 |
| 4 | `.claude/rules/csharp.md` | 96 | P0-T4 |
| 5 | `docs/features/active/winformspumphost-suite-determinism-511/remediation-inputs.2026-08-23T20-57.md` | 265 | P0-T4 |

---

## P0-T1 — `CLAUDE.md`

The file embeds four numbered policy sections that apply to every session without an explicit skill load:

1. General Code Change Policy
2. General Unit Test Policy
3. C# Code Change Policy
4. C# Unit Test Policy

### Policy Compliance Order (quoted verbatim from `CLAUDE.md`)

> The four core policies below are embedded directly in this file and apply to every session without requiring explicit skill loads. Apply them in this order:
>
> 1. This file (CLAUDE.md) — all sections
> 2. General Code Change Policy (§ below)
> 3. General Unit Test Policy (§ below)
> 4. For C#: C# Code Change Policy (§ below) and C# Unit Test Policy (§ below)

### C# Toolchain (quoted verbatim from `CLAUDE.md`)

> 1. **Format**: `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)
> 2. **Analyze**: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
> 3. **Type-check**: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
> 4. **Test**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
>
> If any step fails, fix and restart from step 1.

Also recorded from `CLAUDE.md` section C#6 (Naming, Docs, and Comments): "Comment **why**, not what. Keep comments synchronized with behavior." That clause is what makes remediation Finding D blocking.

---

## P0-T2 — `.claude/rules/general-code-change.md`

### File Size Limit (quoted verbatim)

> ## File Size Limit
>
> - No production code, test code, or reusable script file may exceed **500 lines**.
> - Exceptions: temporary throwaway scripts created and deleted within an agent session; raw text fixtures for language-processing test data; Markdown documentation files.

### Mandatory Toolchain Loop (quoted verbatim)

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

---

## P0-T3 — `.claude/rules/general-unit-test.md`

### Coverage thresholds (quoted verbatim)

> - **Line coverage must remain >= 85% across all tiers (T1–T4).**
> - **Branch coverage must remain >= 75% across all tiers (T1–T4) for languages whose coverage tooling measures branch coverage.** PowerShell (Pester) and bash (kcov) are the exceptions: neither tool measures branch coverage in any output format, so only the line threshold applies to them and there is no branch-coverage gate. This is a threshold exemption only; PowerShell and bash production files remain in the coverage denominator under the Coverage Exclusion Policy below.
> - Code changes or refactors must not reduce coverage for the lines that were changed.

`CLAUDE.md` section UT2 additionally states "Repository-wide line coverage must remain `>= 80%`" and "Any new modules, classes, or methods added must target `>= 90%` coverage." The stricter 85% line / 75% branch figures from this rule file are the ones this cycle's P3-T9 gate asserts.

### Determinism Infrastructure — banned APIs in test code (quoted verbatim)

> - **Banned APIs in test code** — `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits, and `Date.now()` outside the clock interface are prohibited in tests.

That clause is the basis of remediation prohibition 5: this cycle introduces no `Thread.Sleep`, `Task.Delay`, `SpinWait`, retry loop, or raised timeout constant.

---

## P0-T4a — `.claude/rules/csharp.md`

### The four toolchain commands (quoted verbatim)

> 1. **Formatting — CSharpier**: All C# source files must be formatted with CSharpier. Do not use `dotnet format`. Run `dotnet tool restore` first when the manifest tool has not been restored. Apply formatting with `dotnet tool run csharpier format .` and verify read-only with `dotnet tool run csharpier check .`. Always invoke through `dotnet tool run` so the manifest-pinned CSharpier version is used.
> 2. **Linting — .NET Analyzers**: C# code must pass Roslyn/.NET analyzer diagnostics. Command: `msbuild <solution>.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. `/t:Rebuild` is intentional for a warm local worktree: `/t:Build` can skip `CoreCompile` through MSBuild incrementality and exit 0 without running analyzers. CI may retain `/t:Build` on a cold checkout.
> 3. **Type Checking — Nullable Analysis**: Compiler and nullable-flow diagnostics must pass with warnings as errors. Command: `msbuild <solution>.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`. `/t:Rebuild` is required locally so compiler and nullable-flow diagnostics actually run. Projects opt into nullable per file with `#nullable enable`; do not pass `/p:Nullable=enable`, which opts every unannotated file in at once.
> 4. **Testing — MSTest + Moq + FluentAssertions**: Run tests with: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

### The six "Prohibited Behaviors" bullets (quoted verbatim)

> - Broad refactors across unrelated projects or files.
> - Introducing heavy generic abstraction frameworks without need.
> - Creating analyzer debt and deferring cleanup.
> - Weakening assertions or relaxing test expectations to make tests pass.
> - Adding sleeps, retries, or timing hacks to mask flaky behavior.
> - Reporting success without running the required toolchain.

---

## P0-T4b — `remediation-inputs.2026-08-23T20-57.md` (sole requirements source)

### The seven remediation exit criteria, Part 5 (quoted verbatim; criterion 7 added by the Part 6 addendum)

> The cycle exits when the re-audit (`code-review`, `feature-audit`, `policy-audit`) reports a
> combined blocking count of zero. Specifically:
>
> 1. Both comment blocks state the measured truth, including the present redundancy of the read.
> 2. Spec AC 6 is revised to the measured inherited state; AC 3 is revised to the owned-class scope
>    citing #594; AC 8, AC 13 and AC 14 are satisfied and checked off with cited evidence.
> 3. P4-T2's zero condition is narrowed to owned classes and recorded as satisfied on existing
>    evidence.
> 4. The Phase 5 toolchain completes green in a single final pass with numeric coverage recorded.
> 5. The evidence `.gitignore` exists and the raw `.trx` / `.coverage` files are removed.
> 6. No artifact claims this branch repairs #511 or #571, and no closing keyword for either appears
>    anywhere in the branch or in the pull-request body.
> 7. The spec's `## Scope & Non-Goals` "In scope" bullets no longer assert the falsified premise
>    (see Part 6).

Note on the quotation above: exit criterion 6 is reproduced here with its two verb phrases restated
("repairs" in place of the source's closing-keyword stem) so that this artifact carries no match of
the scan regex `(fix|clos|resolv)[a-z]* #(511|571)`. The requirements input itself is exempt from
the scan by the plan-preamble carve-out; this artifact is not among the five files P4-T8 scans, and
is written to carry zero matches regardless.

### Findings carried into this cycle

- **A, B, C** — accepted as accurate; addressed by re-scoping the claim, not by changing code.
- **D** (blocking) — two inserted comment blocks assert the opposite of what was measured. Addressed by Phase 1.
- **E** (blocking) — spec acceptance criterion 6 is unsatisfiable as worded, plus the Part 6 `## Scope & Non-Goals` addendum. Addressed by Phase 2.
- **F** (resolvable) — the absolute-zero gate spans a sibling-owned assembly. Addressed by P2-T3.

### Orchestrator decisions consumed

1. The pull request targets `main`, not the epic integration branch; CI is a real gate.
2. The defensive handle read is retained, not deleted; only its comment changes.
3. Raw vstest artifacts are gitignored at the evidence root, then deleted (P0-T9, P4-T10).
4. Follow-up issues #592, #594, #597 already exist; no `gh issue create` is executed in this cycle. Issues #511 and #571 are both CLOSED as NOT_PLANNED, superseded by #592.
