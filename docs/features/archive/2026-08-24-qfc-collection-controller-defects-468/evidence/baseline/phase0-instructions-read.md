# Phase 0 — Instructions Read (Issue #468)

Timestamp: 2026-08-26T08-25

Task: [P0-T5] (covering the reading performed by [P0-T1] through [P0-T4])

## Policy Order

The four policy files were read end-to-end, in this order, per
`.claude/skills/policy-compliance-order/SKILL.md`:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

## Files Read (explicit list)

Policy documents:

- `CLAUDE.md` (447 lines, read end-to-end)
- `.claude/rules/general-code-change.md` (80 lines, read end-to-end)
- `.claude/rules/general-unit-test.md` (105 lines, read end-to-end)
- `.claude/rules/csharp.md` (96 lines, read end-to-end)

Feature requirement and research documents:

- `docs/features/active/qfc-collection-controller-defects-468/spec.md` (1323 lines; the
  `## Acceptance Criteria` section at `:1155` was read in full and carries AC-1 through AC-29)
- `docs/features/active/qfc-collection-controller-defects-468/issue.md` (58 lines, read end-to-end;
  confirms `- Work Mode: full-bug` and states that its own `## Acceptance Criteria` section is a
  pointer to `spec.md` only)
- `docs/features/active/qfc-collection-controller-defects-468/research/qfc-collection-controller-defects.md`
  (1002 lines, read end-to-end)
- `docs/features/active/qfc-collection-controller-defects-468/research/test-harness-feasibility.md`
  (827 lines, read end-to-end)
- `docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md` (the
  authoritative plan of record)

## [P0-T1] Acceptance — the four policy sections CLAUDE.md embeds

`CLAUDE.md` was read end-to-end. It embeds these four policy sections, which apply to every session
without an explicit skill load:

1. **General Code Change Policy** — `CLAUDE.md:20`
2. **C# Code Change Policy** — `CLAUDE.md:168`
3. **General Unit Test Policy** — `CLAUDE.md:280`
4. **C# Unit Test Policy** — `CLAUDE.md:357`

`CLAUDE.md`'s own `## Policy Compliance Order` section (`:9`) states the application order as:
this file, then General Code Change, then General Unit Test, then (for C#) C# Code Change and
C# Unit Test.

## [P0-T2] Acceptance — `.claude/rules/general-code-change.md`

- **File Size Limit.** "No production code, test code, or reusable script file may exceed **500
  lines**." Exceptions listed are temporary throwaway scripts created and deleted within an agent
  session, raw text fixtures for language-processing test data, and Markdown documentation files.
- **Mandatory toolchain-loop restart rule.** The seven-stage loop is: formatting, linting, type
  checking, architecture-boundary tests, unit tests, contract/schema compatibility checks,
  integration tests. "**Restart from step 1** if any stage fails or auto-fixes any files. Do not
  stop the loop until all seven stages complete without errors in a single pass."

Note for the record: `CLAUDE.md` §8.1 and `.claude/rules/csharp.md` state the operative C# loop as
the four stages format, lint, type-check, test, with the same restart-from-step-1 rule. This plan's
`## Conventions` section binds the executor to those four C# commands.

## [P0-T3] Acceptance — `.claude/rules/general-unit-test.md`

The **Determinism Infrastructure** section's banned-API list for test code is, verbatim:

> **Banned APIs in test code** — `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits,
> and `Date.now()` outside the clock interface are prohibited in tests.

The same section additionally requires a controllable clock (`TimeProvider` for .NET), a seeded RNG
whose seed is printed on failure, and use of the framework fake-timer facility
(`FakeTimeProvider` for .NET) for async tests.

This binds AC-23 and the #473 defect-1 test design, which must sequence exclusively through
`TaskCompletionSource` + `TaskContinuationOptions.ExecuteSynchronously`.

## [P0-T4] Acceptance — the `.claude/rules/csharp.md` rules that bind this change

- **Toolchain.** CSharpier via `dotnet tool run csharpier format .` / `check .` (never a global
  install, never `dotnet format`); analyzers via `msbuild <sln> /t:Rebuild /m
  /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true
  /p:EnforceCodeStyleInBuild=true`; nullable via the same command with
  `/p:TreatWarningsAsErrors=true`; tests via `vstest.console.exe <assemblies> /EnableCodeCoverage`.
  `/t:Rebuild` is required locally because `/t:Build` can skip `CoreCompile` through MSBuild
  incrementality and exit 0 without running analyzers. **Do not pass `/p:Nullable=enable`** — it
  opts every unannotated file in at once; nullable is per-file `#nullable enable`.
- **Testing standards.** MSTest + Moq + FluentAssertions; `[TestClass]`/`[TestMethod]`;
  Arrange-Act-Assert; no external dependencies. Repository-wide line coverage >= 80%; any new
  module/class/method >= 90%; coverage regression on changed lines is a blocking finding.
- **DI seams, in order of preference.** (1) interface seam, (2) injectable `Func<>`/`Action<>`
  delegate seam for a single call path, (3) adapter seam for static or third-party APIs. This binds
  the three AC-20 seams: `DrainBackgroundLoadingTasksAsync` (extract-method),
  `TryGetMoveReadiness` + `_notifyNotReady` (delegate seam, form 2), and `ShrinkByRows` (pure
  static helper).
- **Deterministic test rules.** No network, no mutable machine PATH/profile state, no implicit
  working-directory assumptions, no external services; identical results in the IDE runner and CLI.
- **Coding standards that bind the edits.** `camelCase` locals and private fields; fail fast with
  explicit exceptions; **avoid broad `catch (Exception)`** unless at a defined boundary with added
  context (this is the rule that condemns `TryGetItemGroupByIndex`'s
  `catch (System.Exception)` under #469 defect 3, and the double-catch under #473 defect 2); prefer
  `internal` over `public` for non-public API; XML docs on public APIs whose contract is non-obvious
  (this binds the #469 defect-4 `stackMovedItems` documentation requirement, AC-7).
- **Analyzer stack.** Five packages wired file-based via `packages.config` +
  `<Analyzer Include="..\packages\<id>.<version>\analyzers\dotnet\cs\<dll>" />`, because the
  projects are legacy non-SDK VSTO/.NET Framework projects. `BannedSymbols.txt` at the repo root
  bans `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, and `Task.Delay` at
  `severity = suggestion` (RS0030). New analyzer severities are configured at `suggestion` before
  wiring, because the type-check step runs `/p:TreatWarningsAsErrors=true`.
- **Prohibited behaviors that bind this bugfix branch.** No broad refactors across unrelated files
  (this reinforces AC-21 and AC-25), no weakening of assertions to make tests pass, no sleeps or
  retries to mask flakiness, no reporting success without running the required toolchain.

## Reconciliation note (recorded, not acted on)

`.claude/rules/general-unit-test.md` states line coverage >= 85% and branch coverage >= 75%;
`CLAUDE.md` §UT2 and `.claude/rules/csharp.md` state >= 80% repository-wide and >= 90% for new
code, with a ratified COM/VSTO/WinForms testable-denominator exemption. This plan does not resolve
that divergence and does not need to: per the plan's `### Coverage scope note`,
`QfcCollectionController` carries `[ExcludeFromCodeCoverage]` at `QfcCollectionController.cs:21`
(AC-25 forbids removing it), so no test added by this plan moves any coverage number for that file.
Coverage is captured numerically at baseline (P0-T14) and at final QC, and the delta is reported,
but no acceptance condition in this plan claims a coverage increase attributable to this feature.
