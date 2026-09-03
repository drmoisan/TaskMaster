# Phase 0 — Instructions Read (Issue #751)

Timestamp: 2026-09-03T14-16

Policy Order: The ordering defined by `.claude/skills/policy-compliance-order/SKILL.md` was applied:

1. `CLAUDE.md` (standing instructions)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (language-specific rules for the `*.cs` files in scope)

## Files read (P0-T1 through P0-T6)

| Task | Path | Read in full |
|---|---|---|
| P0-T1 | `CLAUDE.md` | yes (447 lines) |
| P0-T2 | `.claude/rules/general-code-change.md` | yes (80 lines) |
| P0-T3 | `.claude/rules/general-unit-test.md` | yes (105 lines) |
| P0-T4 | `.claude/rules/csharp.md` | yes (96 lines) |
| P0-T5 | `.claude/rules/quality-tiers.md` | yes (51 lines) |
| P0-T5 | `.claude/rules/tonality.md` | yes (80 lines) |
| P0-T6 | `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/issue.md` | yes (92 lines) |
| P0-T6 | `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/spec.md` | yes (410 lines) |
| P0-T6 | `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md` | yes (495 lines) |

Nine paths total, matching the set named by P0-T1 through P0-T6.

## Acceptance facts recorded from the reads

- **P0-T1.** The four C# toolchain commands from `CLAUDE.md` § "C# Toolchain (run in this exact order)"
  (`CLAUDE.md:403-408`) are, in order:
  1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

  The 500-line file limit is stated in § "4. Module & File Structure" (`CLAUDE.md:106`): "Do not exceed
  500 lines for any one file."

- **P0-T2.** `.claude/rules/general-code-change.md` § "File Size Limit" (`:47-50`): no production code, test
  code, or reusable script file may exceed 500 lines. Exceptions: temporary throwaway scripts created and
  deleted within an agent session; raw text fixtures for language-processing test data; Markdown
  documentation files.

- **P0-T3.** `.claude/rules/general-unit-test.md` § "Determinism Infrastructure" banned-API list (`:104`):
  `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits, and `Date.now()` outside the clock
  interface. § "Coverage Exclusion Policy" permitted `exclude` entries (`:37-41`): build output directories
  (`dist/**`, `lib/**`, `lib-amd/**`); test files and test infrastructure (`**/*.test.ts`, `tests/**`,
  `src/test-support/**`); non-production config files (`jest.config.cjs`, `eslint.config.mjs`,
  `.dependency-cruiser.cjs`, `webpack.config.js`); and `node_modules/**`.

- **P0-T4.** `.claude/rules/csharp.md` mandates **CSharpier** as the formatter (`:14`) and forbids
  **`dotnet format`** (`:14`).

- **P0-T5.** `.claude/rules/quality-tiers.md` uniform gate matrix (`:33`): line coverage `>= 85%` across all
  tiers T1-T4. `.claude/rules/tonality.md` § "Hyperbole — Prohibited" (`:35-43`) prohibits hyperbolic,
  inflated, or sensational language.

- **P0-T6.** The `## Acceptance Criteria` heading of `spec.md` (`:324`) carries exactly ten checkbox items,
  at `spec.md:326`, `:331`, `:333`, `:338`, `:340`, `:344`, `:349`, `:353`, `:355`, and `:359`. They are
  referred to as AC1 through AC10 by that order. `spec.md` is the sole acceptance-criteria source for this
  full-bug plan; `issue.md:12` carries the marker `- Work Mode: full-bug`.

## Note on worktree divergence

The `CLAUDE.md` in this worktree cites `.github/workflows/_format-check.yml`,
`.github/workflows/_build-analyzers.yml`, and `.github/workflows/_build-nullable.yml` as the CI parity
references, consistent with this plan's citations of `.github/workflows/_mstest-coverage.yml`. The facts
required by P0-T1 through P0-T5 were read from this worktree's copies of each file, not from any other
checkout.
