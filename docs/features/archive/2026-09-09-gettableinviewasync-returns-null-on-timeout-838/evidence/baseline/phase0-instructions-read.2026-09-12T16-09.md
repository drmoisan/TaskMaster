# Phase 0 — Policy Reads

Timestamp: 2026-09-13T00-48

Policy Order: the reading order mandated by `CLAUDE.md` section "Policy Compliance Order" and by the `policy-compliance-order` skill, applied in this order:

1. `CLAUDE.md` (worktree root) — read in full by P0-T1
2. `.claude/rules/general-code-change.md` — read in full by P0-T2
3. `.claude/rules/general-unit-test.md` — read in full by P0-T3
4. `.claude/rules/csharp.md` — read in full by P0-T4

All four files were read in this session, in that order, from the item worktree.

---

## P0-T1 — CLAUDE.md

The four-command C# toolchain order, quoted from the section "C# Toolchain (run in this exact order)":

1. **Format**: `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)
2. **Analyze**: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type-check**: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. **Test**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

> If any step fails, fix and restart from step 1.

The coverage figures, quoted from section UT2 of the General Unit Test Policy embedded in CLAUDE.md:

> Repository-wide line coverage must remain `>= 80%`.

> Any new modules, classes, or methods added must target `>= 90%` coverage.

> Code changes or refactors must not reduce coverage for the lines that were changed.

---

## P0-T2 — .claude/rules/general-code-change.md

The 500-line file-size limit, quoted from section "File Size Limit":

> - No production code, test code, or reusable script file may exceed **500 lines**.
> - Exceptions: temporary throwaway scripts created and deleted within an agent session; raw text fixtures for language-processing test data; Markdown documentation files.

The mandatory toolchain loop restart rule, quoted from section "Mandatory Toolchain Loop":

> **Restart from step 1** if any stage fails or auto-fixes any files. Do not stop the loop until all seven stages complete without errors in a single pass.

---

## P0-T3 — .claude/rules/general-unit-test.md

The coverage-exclusion policy, quoted from section "Coverage Exclusion Policy":

> No production file may be excluded from coverage measurement. Every production source file is in the denominator of the coverage metric, regardless of whether its lines are reachable in the test environment.

> **Prohibited `exclude` entries:**
> - Any path under `src/` that contains production runtime code, regardless of whether it is auto-generated, host-bound, or difficult to test.

The test-file-location rule, quoted from section "Test File Location":

> Test files must live in a `tests/` directory tree that mirrors the production source structure. The test for `src/foo/bar.ts` belongs at `tests/foo/bar.test.ts`; the test for `scripts/powershell/Foo.ps1` belongs at `tests/scripts/powershell/Foo.Tests.ps1`. Language-specific rules may add further naming conventions (framework suffix, file extension) on top of this universal layout requirement.

> Colocation — placing test files alongside production source files in `src/` or equivalent — is not permitted. An agent that creates or moves a test file into the production source tree has violated this rule.

Applied to this change: the new test file is created under the `UtilitiesCS.Test` project, which is this repository's separate test assembly tree mirroring `UtilitiesCS`. It is not colocated with production source.

---

## P0-T4 — .claude/rules/csharp.md

The framework, mocking and assertion requirements, quoted verbatim from its lines 33 through 35:

> - Use **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`) as the test framework.
> - Use **Moq** for mocking.
> - Prefer **FluentAssertions** for assertions; use MSTest `Assert` only when FluentAssertions is not practical.

---

## Policy conflict noted and resolved by the compliance order

`CLAUDE.md` states a repository-wide line coverage floor of 80 percent and a new-code target of 90 percent. `.claude/rules/general-unit-test.md` states 85 percent line and 75 percent branch. The policy-compliance order places `CLAUDE.md` first, so the CLAUDE.md figures apply to this change. The divergence is pre-existing, is recorded in the feature spec under follow-ups, and is not resolved by this plan.
