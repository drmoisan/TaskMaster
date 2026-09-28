# Phase 0 — Policy Reads (issue #816)

Timestamp: 2026-09-13T22-58

Policy Order: the four documents below were read in full, in this exact order, per the
repository policy-compliance order.

1. `CLAUDE.md` (worktree root)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

---

## P0-T1 — CLAUDE.md

### The four C# toolchain commands, quoted from the heading `## C# Toolchain (run in this exact order)`

1. **Format**: `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`; always via `dotnet tool run`, never a global install)
2. **Analyze**: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type-check**: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. **Test**: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /ResultsDirectory:coverage\test-results /Logger:trx;LogFileName=mstest-run.trx`

The same section closes with: "If any step fails, fix and restart from step 1."

### The repository-wide line-coverage floor, quoted from the General Unit Test Policy section

> Repository-wide line coverage must remain `>= 80%`.

The same section adds:

> Any new modules, classes, or methods added must target `>= 90%` coverage.
> Code changes or refactors must not reduce coverage for the lines that were changed.

---

## P0-T2 — .claude/rules/general-code-change.md

### The 500-line file-size limit, quoted

> - No production code, test code, or reusable script file may exceed **500 lines**.
> - Exceptions: temporary throwaway scripts created and deleted within an agent session; raw text fixtures for language-processing test data; Markdown documentation files.

### The mandatory toolchain-loop restart rule, quoted

> **Restart from step 1** if any stage fails or auto-fixes any files. Do not stop the loop until all seven stages complete without errors in a single pass.

---

## P0-T3 — .claude/rules/general-unit-test.md

### The figures stated there, quoted

> - **Line coverage must remain >= 85% across all tiers (T1–T4).**
> - **Branch coverage must remain >= 75% across all tiers (T1–T4) for languages whose coverage tooling measures branch coverage.**

### The governing ruling for this delivery

The specification document in this feature folder rules, under `## Assumptions, Constraints,
Dependencies`, that CLAUDE.md's figures govern:

> Coverage policy: CLAUDE.md governs, which is 80% repository-wide line coverage and 90% for
> newly added members. The 85% line and 75% branch figures and the T1-T4 tier system that appear in
> the rule files under the .claude directory are reference-repository leakage: the quality-tiers.yml
> file those rules name as their source of truth does not exist in this repository. CLAUDE.md's
> figures are the ones used throughout this specification, per the policy reading order.

Reason, verified during this Phase 0 read: `.claude/rules/general-code-change.md` states that
"Every project must be classified in `quality-tiers.yml` at repo root", and
`.claude/rules/quality-tiers.md` names that same file as its source of truth. A glob for
`quality-tiers.yml` over the worktree root returned no file. The 85/75 figures therefore rest on a
source-of-truth document that is absent from this repository, while CLAUDE.md's 80 percent and
90 percent figures are stated directly in the always-loaded instruction file that heads the policy
reading order. This delivery uses 80 percent as the line floor and 90 percent for newly added
members, and applies no separate branch-coverage gate.

---

## P0-T4 — .claude/rules/csharp.md

This file adds the following C#-specific obligations beyond what CLAUDE.md states.

- **Deterministic test rules.** Unit tests must not depend on network, mutable machine PATH or
  profile state, implicit working-directory assumptions, or external services; results must be
  identical in the IDE runner and in CLI runs.
- **DI seam preference order.** Interface seam first, then injectable delegate seam, then adapter
  seam for static or third-party APIs. (Not exercised by this delivery: no new seam is added.)
- **Time seam guidance.** New or touched time-dependent code should take `System.TimeProvider` by
  constructor injection rather than calling the clock directly. Guidance only. (Not exercised: this
  delivery touches no time-dependent code.)
- **Analyzer stack.** A fixed set of five static-analysis packages wired into first-party projects
  by explicit `<Analyzer Include>` items plus `packages.config` development dependencies, with
  banned symbols enforced from a repo-root `BannedSymbols.txt` at `severity = suggestion`.
- **Severity-first ordering invariant.** New analyzer rule severities are set to `suggestion` in
  `.editorconfig` before any `<Analyzer Include>` item is wired in, because the type-check step runs
  with `/p:TreatWarningsAsErrors=true`.
- **SecurityCodeScan.VS2019 is deferred**, not silently omitted, and no CS8032 suppression is
  introduced.
- **Prohibited behaviours** enumerated there: broad refactors across unrelated projects, heavy
  generic abstraction without need, analyzer debt, weakening assertions to make tests pass, adding
  sleeps or retries to mask flakiness, and reporting success without running the toolchain.
- It restates, rather than adds, the 80 percent repository-wide line floor and the 90 percent floor
  for any new module, class or method, and states that coverage regression on changed lines is a
  blocking finding.
