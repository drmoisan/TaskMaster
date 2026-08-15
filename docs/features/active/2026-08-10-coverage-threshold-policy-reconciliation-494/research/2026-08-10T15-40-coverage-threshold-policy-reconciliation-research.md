# Coverage Threshold and Exclusion Policy Reconciliation — Research (Issue #494)

- **Issue:** #494
- **Feature folder:** `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/`
- **Epic:** `build-ci-coverage-gate-fidelity` (wave 2; depends on #457, transitively #441/#478)
- **Branch:** `bug/coverage-threshold-policy-reconciliation-494`
- **Governance-document line numbers in this artifact are as of commit `edf3d34c`.**
- **Timestamp:** 2026-08-10T15-40

## Method and Evidence Discipline

Every claim below is labelled **[VERIFIED]** (read directly from the working tree in this session),
**[INFERRED]** (a conclusion drawn from verified facts, stated as such), or
**[UNVERIFIED]** (could not be checked in this session; search scope recorded).

**Tooling limitation, recorded explicitly.** This session had no shell/Bash tool available
(available tools: Read, Grep, Glob, WebFetch, Write, Edit). **`git log` and `git blame` could not be
run.** R2's provenance question is therefore answered from committed documentary evidence and
persisted agent memory rather than from commit history. Every provenance statement below is marked
accordingly, and the spec should re-run `git log -L`/`git blame` on the two named sections before
treating provenance as settled.

Per the instruction, tooling locators are anchored on function/symbol names. Line numbers appear
only for governance documents (`CLAUDE.md`, `AGENTS.md`, `.claude/rules/**`, `.claude/skills/**`,
`.claude/agents/**`, `.agents/**`, `.github/instructions/**`), which features #441/#457/#512 do not
modify.

---

## R1 — Complete Site Inventory

**Scope searched [VERIFIED]:** `CLAUDE.md`, `AGENTS.md`, `.claude/rules/*.md`, `.claude/skills/**`,
`.claude/agents/**`, `.claude/hooks/**`, `.claude/agent-memory/**`, `.agents/**`,
`.github/instructions/**`, `.github/skills/**`, `.github/workflows/**`, `scripts/**`,
`coverage.config`, `**/*.runsettings`, `TaskMaster.sln`.
**Patterns used:** `(?i)(>=\s*`?8[05]|>=\s*`?75|>=\s*`?90|8[05]\.0|75\.0|below 8[05]|below 75|coverage (floor|threshold)|line coverage|branch coverage)`, plus targeted
`(?i)(ExcludeFromCodeCoverage|testable denominator|exclusion policy|no production file|exempt)` and
per-file reads.

The divergence is materially wider than the issue's partial list. It spans **five** normative
surfaces (Claude rules, Claude skills/agents, Codex `.agents/`, Copilot `.github/instructions/`,
and root `AGENTS.md`), and there are **three** internally self-contradicting files.

### 1a. Normative policy documents

| # | Path | Line(s) | Exact text (abridged where marked) | Camp | Kind |
|---|---|---|---|---|---|
| 1 | `CLAUDE.md` | 296 | "Configure coverage tooling to exclude test files (e.g., `tests/`), so metrics reflect the application code, not the tests themselves." | exclusion | root policy |
| 2 | `CLAUDE.md` | 297 | "Repository-wide line coverage must remain `>= 80%`." | 80/90 | root policy |
| 3 | `CLAUDE.md` | 298-301 | "**COM/VSTO/WinForms coverage exemption (testable denominator).** The 80% floor applies to the **testable denominator** — production-only first-party code, after excluding: (a) VSTO add-in lifecycle classes …; (b) WinForms form-derived classes and Designer-generated code; (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`, `ToDoModel`, and `Tags` …" | exclusion | root policy |
| 4 | `CLAUDE.md` | 303 | "These classes are formally exempted from the 80% floor. Exemption is applied via `[ExcludeFromCodeCoverage]` attributes … or via `coverage.config` assembly-level excludes … **Authority**: This exemption must be ratified by the project maintainer and is tracked in `feature/csharp-coverage-uplift`. Testable seams … are explicitly NOT exempt and must meet the `>= 80%` floor." | exclusion + 80 | root policy |
| 5 | `CLAUDE.md` | 304 | "Any new modules, classes, or methods added must target `>= 90%` coverage." | 80/90 | root policy |
| 6 | `CLAUDE.md` | 305 | "Code changes or refactors must not reduce coverage for the lines that were changed." | neutral | root policy |
| 7 | `.claude/rules/general-unit-test.md` | 23 | "**Line coverage must remain >= 85% across all tiers (T1–T4).**" | 85/75 | rule |
| 8 | `.claude/rules/general-unit-test.md` | 24 | "**Branch coverage must remain >= 75% across all tiers (T1–T4).**" | 85/75 | rule |
| 9 | `.claude/rules/general-unit-test.md` | 26 | "Tier-specific lower coverage thresholds are not used in this repository. See `.claude/rules/quality-tiers.md` for the full tier system." | 85/75 | rule |
| 10 | `.claude/rules/general-unit-test.md` | 29 | "Type-only / interface-only modules … may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold." | exclusion | rule |
| 11 | `.claude/rules/general-unit-test.md` | 31-46 | "## Coverage Exclusion Policy" — full text quoted in R2 | exclusion | rule |
| 12 | `.claude/rules/quality-tiers.md` | 9 | "The tier system source of truth is `docs/ci.research.md` section 1; the file `quality-tiers.yml` at the repository root maps every project to a tier. Adding a project without a tier classification fails CI." | tiers | rule |
| 13 | `.claude/rules/quality-tiers.md` | 20-21 | "`quality-tiers.yml` at repo root maps every project to one tier." / "The CI pipeline's `tier-classification` stage validates that every project entry has a tier…" | tiers | rule |
| 14 | `.claude/rules/quality-tiers.md` | 33-34 | "Line coverage: >= 85%." / "Branch coverage: >= 75%." | 85/75 | rule |
| 15 | `.claude/rules/quality-tiers.md` | 51 | "…line coverage >= 85% and branch coverage >= 75% apply uniformly across T1–T4; tier-specific lower coverage floors are not used in this repository." | 85/75 | rule |
| 16 | `.claude/rules/csharp.md` | 39-41 | "Repository-wide line coverage must remain >= 80%." / "Any new module, class, or method must reach >= 90% coverage." / "Coverage regression on changed lines is a blocking finding." | 80/90 | rule (512-owned **file**, outside 512's stated line range — see R8) |
| 17 | `.claude/rules/powershell.md` | 63-65 | "Line coverage must remain >= 85% across all tiers (T1–T4) per `.claude/rules/quality-tiers.md`." / "Branch coverage must remain >= 75%…" / "Coverage regression on changed lines is a blocking finding." | 85/75 | rule |
| 18 | `.claude/rules/powershell.md` | 57 | "Organize tests to mirror code structure (e.g., `tests/scripts/dev-tools/ScriptName.Tests.ps1`)." | layout | rule (relevant to AC9 — see R10) |
| 19 | `.claude/rules/python.md` | 16 | "New logic must have test coverage >= 90%." | 80/90 | rule |
| 20 | `.claude/rules/python.md` | 88-90 | ">= 80%" repo-wide / ">= 90%" new / changed-line regression blocking | 80/90 | rule |
| 21 | `.claude/rules/typescript.md` | 42-45 | ">= 80%" repo-wide / ">= 90%" new / changed-line regression blocking | 80/90 | rule |
| 22 | `.claude/rules/architecture-boundaries.md` | 10 | "Architecture boundary enforcement is a uniform gate across all tiers (T1–T4)." | tiers | rule (tier dependency, no number) |
| 23 | `.github/instructions/general-unit-test.instructions.md` | 39-40 | ">= 80%" repo-wide / ">= 90%" new | 80/90 | Copilot instruction (named as a protected policy location by `policy-compliance-order`) |
| 24 | `AGENTS.md` | 372-373 | "Repository-wide line coverage must remain `>= 80%`." / "Any new modules, classes, or methods added must target `>= 90%` coverage." | 80/90 | Codex root instructions |

**Note on #24 [VERIFIED]:** `AGENTS.md` § 2 (lines 366-375) is a near-copy of `CLAUDE.md` § UT2 but
**omits the COM/VSTO/WinForms exemption entirely**. So `AGENTS.md` states the 80% floor with no
testable-denominator carve-out — a third distinct position, not simply a mirror.

### 1b. Skills and agent definitions (`.claude/`)

| # | Path | Line(s) | Text | Camp |
|---|---|---|---|---|
| 25 | `.claude/skills/feature-review-workflow/SKILL.md` | 112 | "New code files …: line coverage >= 85% and branch coverage >= 75%. Flag as FAIL otherwise." | 85/75 |
| 26 | `.claude/skills/feature-review-workflow/SKILL.md` | 113 | "Modified files …: line coverage >= 85%, branch coverage >= 75%, and no regression on changed lines…" | 85/75 |
| 27 | `.claude/skills/feature-review-workflow/SKILL.md` | 114 | "Repo-wide per language: line coverage >= 85% and branch coverage >= 75%." | 85/75 |
| 28 | `.claude/skills/csharp-qa-gate/SKILL.md` | 46 | "**New modules, classes, or methods**: coverage >= 90% for each new unit introduced in the batch." | 80/90 (512-owned file) |
| 29 | `.claude/skills/python-qa-gate/SKILL.md` | 46 | identical text | 80/90 |
| 30 | `.claude/skills/powershell-qa-gate/SKILL.md` | 45 | "**New modules, classes, or methods**: line coverage >= 85% and branch coverage >= 75% per the uniform tier rule (`.claude/rules/quality-tiers.md`)." | 85/75 |
| 31 | `.claude/agents/feature-review.md` | 112-114 | "line coverage >= 85%, branch coverage >= 75%" (new / modified / repo-wide) | 85/75 |
| 32 | `.claude/agents/feature-review.md` | 127 | "For each new file: if line coverage is below 90%, flag as FAIL…" | 80/90 |
| 33 | `.claude/agents/feature-review.md` | 128 | "For each modified file: if line coverage has regressed from baseline or is below 80%, flag as FAIL…" | 80/90 |

**Finding — self-contradicting file #1 [VERIFIED].** `.claude/agents/feature-review.md` states both
camps **fourteen lines apart in the same numbered procedure**. Lines 112-114 set 85/75 thresholds;
lines 127-128 instruct the same agent to flag FAIL at 90% (new) and 80% (modified). This is a live
agent definition, not documentation. It is not enumerated in issue.md and must be added to the AC10
disposition list.

### 1c. Enforcement scripts, hooks, and CI

| # | Path | Anchor | Behavior | Camp |
|---|---|---|---|---|
| 34 | `.claude/hooks/validate-feature-review-coverage.ps1` | `.SYNOPSIS` block, line 29-30 | "When repo-wide coverage is below 80 percent for an available artifact, the policy audit must carry a FAIL verdict for that language." | 80 |
| 35 | `.claude/hooks/validate-feature-review-coverage.ps1` | `Test-LanguageCoverageRow`, line 313 | `if ($null -ne $RepoWidePct -and $RepoWidePct -lt 85.0)` | 85 |
| 36 | `.claude/hooks/validate-feature-review-coverage.ps1` | `Test-LanguageCoverageRow`, line 318 | message text "below the 85% line coverage floor" | 85 |
| 37 | `.claude/hooks/validate-feature-review-coverage.ps1` | `Test-LanguageCoverageRow`, line 323 | `$BranchFloor = 75.0` | 75 |
| 38 | `.claude/hooks/validate-feature-review-coverage.ps1` | `Test-LanguageCoverageRow`, line 327 | message text "below the 75% branch coverage floor" | 75 |
| 39 | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `Invoke-DotnetCoverageCollection` | **No threshold comparison anywhere.** Only failure path: `throw "MSTest with coverage failed with exit code $coverageExitCode"` | none |
| 40 | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | all six functions | **No threshold comparison anywhere.** | none |
| 41 | `scripts/temp-extract-coverage.ps1` | inline | `if ($lr -lt 0.80) { $below80 += $obj }` — categorization only, no gate; hard-coded output path pointing at an archived feature folder | 80 (non-normative) |
| 42 | `.github/workflows/ci.yml` | "Run MSTest suite with coverage" step | `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`; failure only on non-zero exit; uploads `TestResults/**/*.coverage` as an artifact. **No threshold, no Cobertura conversion, no gate.** | none |

**Finding — self-contradicting file #2 [VERIFIED].** The hook's documented behavior (80) and its
enforced constants (85/75) disagree inside a single file. This is the AC8 target.

**Finding [VERIFIED].** `scripts/temp-extract-coverage.ps1` is a committed throwaway script whose
default `-OutputPath` targets `docs/features/active/2026-03-19-utilities-coverage-part-three-87/…`,
a feature folder that no longer exists under `active/`. It is a latent-cleanup candidate, not a
threshold site. Recommend a follow-up issue rather than in-scope deletion.

### 1d. Coverage-exclusion configuration (the mechanism side of AC2)

| # | Path | Content | Assessment |
|---|---|---|---|
| 43 | `coverage.config` | `ModulePaths/Exclude`: `.*Deedle.*`, `.*FSharp.*`, `.*Castle\.Core.*`, `.*FluentAssertions.*`, `.*Moq.*`, `.*Microsoft\.Testing.*`, `.*MSTest.*` | **Excludes no production assembly** [VERIFIED] — confirms issue.md item 5 |
| 44 | `TaskMaster.runsettings` | identical `ModulePaths/Exclude` list under a `Code Coverage` `DataCollector` | same |
| 45 | `scripts/vscode/TaskMaster.cli.runsettings` | MSTest parallelization only; **no** coverage data collector (documented in `Resolve-RunSettingsPath`) | not a threshold/exclusion site |
| 46 | `TaskVisualization.Test/coverage.runsettings` | `ModulePath` include of `.*TaskVisualization\.dll$`; `Attributes/Exclude` includes `^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$`, `DebuggerHidden`, `DebuggerNonUserCode`, `GeneratedCode` | attribute-based exclusion is live here |
| 47 | `TaskTree.Test/coverage.tasktree.runsettings` | same shape, plus `CompilerGeneratedAttribute` | attribute-based exclusion is live here |
| 48 | `UtilitiesCS.Test/test.runsettings` | no `CodeCoverage`/`Exclude`/`ModulePath`/`Attribute` elements [VERIFIED by grep] | not a site |

**Consequence [INFERRED, high confidence].** Today the "exclusion" mechanism that actually removes
production lines from the denominator is the `[ExcludeFromCodeCoverage]` **attribute**, honoured by
the Visual Studio Code Coverage collector by default and explicitly listed in two per-project
runsettings. `coverage.config` and `TaskMaster.runsettings` exclude only third-party modules.
So `.claude/rules/general-unit-test.md`'s "Prohibited `exclude` entries" clause, which is written
about `exclude` **entries in a config file**, does not literally reach the mechanism this repository
actually uses. This is a load-bearing observation for AC2 (see R2).

### 1e. `.agents/` bundle — a second live runtime, not a passive mirror

`.agents/README.md:5` [VERIFIED]: "This directory is the canonical Codex runtime surface for
repository-local skills." It is therefore normative for Codex sessions.

| # | Path | Line(s) | Camp | Divergence from the `.claude/` counterpart |
|---|---|---|---|---|
| 49 | `.agents/skills/general-unit-test/SKILL.md` | 29-30, 37-52 | 85/75 + exclusion policy | matches `.claude/rules/general-unit-test.md` (except line 32 cites `.agents/skills/quality-tiers.md`, a path that does not exist — the file is `.agents/skills/quality-tiers/SKILL.md`) |
| 50 | `.agents/skills/quality-tiers/SKILL.md` | 15, 27, 39-40, 49, 57 | 85/75 + tiers | matches; **also duplicates the false `quality-tiers.yml` / `docs/ci.research.md` claim** (line 15); frontmatter carries a duplicated `description:` key at lines 3 and 6 |
| 51 | `.agents/skills/csharp/SKILL.md` | 42-43 | 80/90 | matches `.claude/rules/csharp.md` |
| 52 | `.agents/skills/python/SKILL.md` | 17, 89-90 | 80/90 | matches |
| 53 | `.agents/skills/typescript/SKILL.md` | 43-44 | 80/90 | matches |
| 54 | `.agents/skills/powershell/SKILL.md` | 64-65 | **80/90** | **DIVERGES** — `.claude/rules/powershell.md:63-64` says 85/75 |
| 55 | `.agents/skills/powershell-qa-gate/SKILL.md` | 45 | **>= 90%** | **DIVERGES** — `.claude/skills/powershell-qa-gate/SKILL.md:45` says 85/75 |
| 56 | `.agents/skills/feature-review-workflow/SKILL.md` | 101-103 | **90 / 80 / 80** | **DIVERGES** — `.claude/skills/feature-review-workflow/SKILL.md:112-114` says 85/75 |
| 57 | `.agents/skills/csharp-qa-gate/SKILL.md` | 48 | >= 90% | matches |
| 58 | `.agents/skills/python-qa-gate/SKILL.md` | 46 | >= 90% | matches |

**Finding [VERIFIED].** The `.agents/` bundle is **not a faithful mirror**. Three files (54, 55, 56)
state the opposite camp from their `.claude/` counterparts. issue.md characterises `.agents/` as "a
parallel mirror bundle … [that] restates both camps"; the sharper statement is that `.agents/` is a
**stale snapshot frozen at an earlier point in the drift**, so a Codex-run session and a Claude-run
session applying the same nominal policy today reach different verdicts on PowerShell and on
feature review.

### 1f. Agent memory

`.claude/agent-memory/**` is committed to the repository and is read by agents at session start.
It contains no numeric threshold *policy statement*, but it does contain durable interpretive
guidance that currently substitutes for policy [VERIFIED]:

- `.claude/agent-memory/task-researcher/project_winforms_testability_epic_298.md:17` — "Repo coverage
  policy is 80/90 with ratified COM/VSTO exemption (CLAUDE.md authoritative), not the 85/75 tier
  policy in `.claude/rules/`."
- `.claude/agent-memory/feature-review/project_csharp-repowide-coverage-below-80.md:17` — "…per the
  #178 governance-sync decision CLAUDE.md's 80/90 is authoritative and the 85/75 tiers were not
  adopted."
- `.claude/agent-memory/feature-review/coverage-hook-forces-fail-below-floor-despite-exemption.md` —
  operational guidance on evading/satisfying the hook (quoted in R3).
- `.claude/agent-memory/task-researcher/project_qfc_item_controller_227_r2_denial.md` — a recorded
  **maintainer denial** (2026-07-01) of a blanket `[ExcludeFromCodeCoverage]` exemption boundary,
  directing per-member barrier analysis instead. Directly relevant to AC2 and to Automation
  Feasibility.

**Assessment [INFERRED].** These memory files are the de facto authority rule today. AC3 exists
precisely to replace them with a written rule. The spec should state that once AC3 lands, the
memory entries asserting authority become redundant and should be superseded (they are not policy
and cannot be cited as such).

### 1g. Sites explicitly checked and found to contain no threshold or exclusion rule

`.github/workflows/codex-web-setup-test.yml`; `.claude/hooks/*` other than
`validate-feature-review-coverage.ps1` (`validate-planner-output.ps1`,
`validate-discovery-artifact-gate.ps1`, `enforce-evidence-locations.ps1`,
`enforce-discovery-artifact-gate.ps1` mention the word "coverage" only in artifact-name or
evidence-kind contexts); `scripts/vscode/Invoke-MSTest.ps1`, `Invoke-VSBuild.ps1`,
`Invoke-Restore.ps1`, `Install-RepoDotNetSdk.ps1`, `Sync-PackageReferences.ps1`,
`TestProcessCleanup.ps1`, `scripts/dev-tools/run-actionlint.ps1`;
`.github/skills/**` (only `atomic-plan-contract/SKILL.md:112` "baseline coverage", non-numeric);
`.codex/**` (no numeric coverage thresholds found).

---

## R2 — The Exclusion-Policy Contradiction, Precisely Characterized

### 2a. `CLAUDE.md` § UT2 — full text of the exemption (lines 296-306)

```
  - Configure coverage tooling to exclude test files (e.g., `tests/`), so metrics reflect the
    application code, not the tests themselves.
  - Repository-wide line coverage must remain `>= 80%`.
  - **COM/VSTO/WinForms coverage exemption (testable denominator).** The 80% floor applies to the
    **testable denominator** — production-only first-party code, after excluding:
    - (a) VSTO add-in lifecycle classes (entry points, ribbon event handlers, COM utility
      registration) that cannot be unit-tested without a live Outlook process;
    - (b) WinForms form-derived classes and Designer-generated code;
    - (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`,
      `ToDoModel`, and `Tags` that directly depend on
      `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder` without
      an injectable seam.

    These classes are formally exempted from the 80% floor. Exemption is applied via
    `[ExcludeFromCodeCoverage]` attributes in source code (reviewable in PRs) or via
    `coverage.config` assembly-level excludes for near-wholly-untestable assemblies. **Authority**:
    This exemption must be ratified by the project maintainer and is tracked in
    `feature/csharp-coverage-uplift`. Testable seams within otherwise-COM-bound assemblies (e.g.,
    `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, path/settings helpers) are explicitly NOT
    exempt and must meet the `>= 80%` floor.
  - Any new modules, classes, or methods added must target `>= 90%` coverage.
  - Code changes or refactors must not reduce coverage for the lines that were changed.
  - Coverage is a supporting metric, not the sole quality gate; untested critical behavior is not
    acceptable even if the overall percentage looks good.
```

### 2b. `.claude/rules/general-unit-test.md` — full text of the exclusion policy (lines 31-46)

```
## Coverage Exclusion Policy

No production file may be excluded from coverage measurement. Every production source file is in
the denominator of the coverage metric, regardless of whether its lines are reachable in the test
environment.

The correct response to a file that contains untestable lines is to refactor it — extract all logic
into host-neutral, testable modules and leave only the thinnest possible wiring in the host-bound
entry point. The entry point's uncovered lines then represent a real and visible cost in the
coverage metric, which creates ongoing pressure to keep those files minimal.

**Permitted `exclude` entries** (non-production paths only):
- Build output directories: `dist/**`, `lib/**`, `lib-amd/**`.
- Test files and test infrastructure: `**/*.test.ts`, `tests/**`, `src/test-support/**`.
- Config files that are not production code: `jest.config.cjs`, `eslint.config.mjs`,
  `.dependency-cruiser.cjs`, `webpack.config.js`.
- `node_modules/**`.

**Prohibited `exclude` entries:**
- Any path under `src/` that contains production runtime code, regardless of whether it is
  auto-generated, host-bound, or difficult to test.

**Enforcement:** Feature-review agents must treat any `exclude` entry that matches a production
source path as a **Blocking** finding.
```

### 2c. Where they are genuinely incompatible, and where they only appear to be

**Genuinely incompatible (one substantive conflict, one enforcement conflict):**

1. **Denominator definition.** `CLAUDE.md` defines the metric's denominator as the *testable*
   denominator: three named categories of production code are removed from it before the floor is
   applied. `general-unit-test.md` line 33 states the opposite as an absolute: "Every production
   source file is in the denominator of the coverage metric, regardless of whether its lines are
   reachable in the test environment." These are contradictory definitions of the same quantity.
   They cannot both be true. **This is the real conflict and the one AC2 must resolve.**

2. **Prescribed remedy.** `CLAUDE.md` prescribes *exempting* host-bound code (with maintainer
   ratification). `general-unit-test.md` line 35 prescribes *refactoring* it, and explicitly
   justifies keeping the uncovered lines visible: "The entry point's uncovered lines then represent
   a real and visible cost in the coverage metric, which creates ongoing pressure to keep those
   files minimal." These are opposite incentive designs, not two wordings of one design.

3. **Enforcement instruction (partially conflicting).** Line 46 orders feature-review agents to
   treat a production-path `exclude` entry as **Blocking**. `CLAUDE.md` line 303 authorises exactly
   such entries ("`coverage.config` assembly-level excludes for near-wholly-untestable assemblies").
   A feature-review agent reading both is instructed to block a change the root policy permits.

**Differently worded but reconcilable (do not spend the spec's budget here):**

- Both documents exclude **test files** from measurement (`CLAUDE.md:296` vs
  `general-unit-test.md:28`). Identical intent.
- `general-unit-test.md:29` already carves out type-only/interface-only modules and says so
  explicitly "does not lower any coverage threshold". `CLAUDE.md` is silent on this; silence is not
  conflict.
- `CLAUDE.md:305` and `general-unit-test.md:25` both make changed-line regression a violation.
  Identical.

**A fourth, non-obvious mismatch [INFERRED, high confidence].** The `general-unit-test.md` clause
regulates **`exclude` entries in a coverage configuration file**. The mechanism this repository
actually uses to remove production lines from the C# denominator is the
`[ExcludeFromCodeCoverage]` **attribute**, honoured by default by the VS Code Coverage collector and
named explicitly in `TaskVisualization.Test/coverage.runsettings:19` and
`TaskTree.Test/coverage.tasktree.runsettings:19`. The prohibited-entries clause therefore does not
literally bind the dominant mechanism. A reconciled rule must be written in terms of *which
production lines leave the denominator by any mechanism*, not in terms of `exclude` glob entries, or
it will be evadable by construction.

### 2d. Is `general-unit-test.md`'s exclusion section foreign-authored? — Evidence

**Assessment: yes, with high confidence.** Five independent lines of evidence, all [VERIFIED]
except where noted:

1. **Vocabulary.** Every permitted-exclude example is TypeScript/Node: `dist/**`, `lib-amd/**`,
   `**/*.test.ts`, `src/test-support/**`, `jest.config.cjs`, `eslint.config.mjs`,
   `.dependency-cruiser.cjs`, `webpack.config.js`, `node_modules/**`. The prohibited clause is
   scoped to "Any path under `src/`". **This repository has no `src/` directory, no `package.json`,
   no `node_modules/`, and no `.ts` files anywhere** (Glob for `package.json` → no files; Glob for
   `**/*.ts` → no files). Every named example is inapplicable.

2. **A sibling rule file in the same directory contradicts the codebase outright.**
   `.claude/rules/architecture-boundaries.md:22` states: "New runtime code must not reference
   Outlook desktop automation APIs (`Microsoft.Office.Interop.Outlook`)." TaskMaster is a VSTO
   Outlook add-in built entirely on that API. Line 15 references ".NET (when the backend exists)"
   and line 38 "applies once the backend exists" — describing a No-COM architecture that does not
   exist here.

3. **A rule file in the same directory names the foreign origin explicitly.**
   `.claude/rules/orchestrator-state.md` § "Foreign Schema Warning (do not copy verbatim)": "A
   hardened snapshot from another repository contains a JSON Schema … whose `$id` references a
   foreign origin (`drmoisan.github.io/mix-calculator/`). That schema MUST NOT be copied verbatim
   into this repository…" This is in-repo, committed proof that a foreign governance snapshot was
   imported into `.claude/rules/` and that at least one artifact from it was later caught and
   quarantined.

4. **`.claude/rules/quality-tiers.md` cites a source-of-truth document that does not exist in this
   repository.** Line 9 names `docs/ci.research.md` section 1. Glob `**/ci.research*` → **no files**.
   Grep for `ci\.research` across the whole worktree returns exactly two hits, both of them the
   citation itself (`.claude/rules/quality-tiers.md:9` and `.agents/skills/quality-tiers/SKILL.md:15`).
   The tier examples in that file — "classifier engines (SpamBayes, Triage)", "Graph
   extended-properties adapter", "host-agnostic command bus", `TaskMaster.Domain`,
   `TaskMaster.Application` — name projects that do not exist in `TaskMaster.sln`.

5. **Persisted agent memory records the decision that these were rejected [documentary, not
   git-verified].** `C:\Users\DanMoisan\.claude\projects\C--Users-DanMoisan-repos-TaskMaster\memory\project_claude_governance_sync_178.md`
   (written ~63 days before this session) records: in 2026-06, issue #178 / PR #179 into `main`,
   branch `chore/sync-claude-hardening`, `.claude/` was synced from a hardened reference repo under
   the directive **"keep current policy, adapt mechanism."** It lists as **Kept**: "80% line / 90%
   new-module coverage (line-only, no branch gate)". It lists as **"Deliberately EXCLUDED from the
   reference repo (do not reintroduce without a decision)"**: "85% line / 75% branch coverage, the
   7-stage toolchain, the T1–T4 `quality-tiers.yml` system, `rules/architecture-boundaries.md` (it
   bans COM/VSTO — contradicts this codebase), `rules/benchmark-baselines.md`…" and adds: "If a
   future `.claude` file references `quality-tiers.md` or 85/75, that is reference-repo leakage to
   revert."

**Verification of #5 against current state [VERIFIED].** Every artifact that memory says was
excluded is present in the working tree today: `.claude/rules/quality-tiers.md` (85/75, T1–T4),
`.claude/rules/general-unit-test.md:23-24` (85/75), `.claude/rules/architecture-boundaries.md`,
`.claude/rules/benchmark-baselines.md`, and `.claude/rules/general-code-change.md` § "Mandatory
Toolchain Loop" (the seven-stage loop). **The #178 exclusions were reversed by a later change.**

**Conclusion for the spec [INFERRED, high confidence].** The 85/75 cluster entered this repository
as an unreconciled import from a foreign governance snapshot, **after** a recorded decision at #178
to keep 80/90 and to reject that snapshot's coverage model. Was it ever reconciled against this
repository's C#/VSTO reality? **No** — the `src/`/`node_modules/`/`jest.config.cjs` examples, the
No-COM architecture rules, and the non-existent `docs/ci.research.md` and `quality-tiers.yml` are
all still present verbatim.

**[UNVERIFIED — required follow-up.]** The exact commit and PR that reintroduced 85/75 and
`quality-tiers.md` could not be identified without `git log`/`git blame`. The spec must run, at
minimum:
```
git log --follow --oneline -- .claude/rules/quality-tiers.md
git log --follow --oneline -- .claude/rules/general-unit-test.md
git log -L 31,46:.claude/rules/general-unit-test.md
git log -L 23,24:.claude/rules/general-unit-test.md
git log --oneline -- .claude/rules/architecture-boundaries.md
```
and record the resulting commit SHAs, dates, and PR numbers in the spec's decision record. If the
reintroduction was itself an explicit maintainer decision that supersedes #178, that changes the
answer to R2 materially and must be surfaced before the reconciliation is written.

---

## R3 — What Is Actually Enforced Today

### 3a. The three confirmations requested

**(a) CONFIRMED [VERIFIED].** `scripts/vscode/Invoke-MSTestWithCoverage.ps1` contains no coverage
threshold comparison. Grep for `(?i)(threshold|8[05]\.0|75\.0|-lt 8|-lt 7|-lt 9)` across `scripts/`
returns exactly one line, `Invoke-MSTestWithCoverage.ps1:236`, which is the test-process exit-code
throw. The script's responsibilities are: resolve `vstest.console.exe` via `vswhere`; discover
`*.Test.dll`; derive an effective `coverage.config`; run `dotnet-coverage collect --output-format
cobertura`; post-process via `ConvertTo-KoverageCoberturaXml`. It emits a report; it does not judge
one.

**(b) CONFIRMED, with two corrections [VERIFIED].** `.claude/hooks/validate-feature-review-coverage.ps1`
is the only numeric gate in the repository. Its `.SYNOPSIS` (line 29) documents "below 80 percent";
`Test-LanguageCoverageRow` enforces `85.0` (line 313) and `$BranchFloor = 75.0` (line 323).

  Two corrections to the issue's framing, both material to AC4 and AC8:

  - **The line and branch checks are not symmetric.** The line check (313-321) only requires that
    *the policy-audit text contain a FAIL token* when repo-wide < 85. The branch check (324-329)
    returns `Ok = $false` **unconditionally** when branch < 75 — it never inspects the audit for a
    FAIL token, despite its own message saying "policy-audit must record FAIL on the corresponding
    coverage row." A sub-75 branch figure therefore blocks subagent termination with no available
    disposition. This asymmetry is a defect in its own right and should be enumerated in the spec.

  - **The gate is not a coverage gate; it is an audit-text-consistency gate,** and it is trivially
    evadable. `Get-LanguageRepoCoverage` returns `$null` when the artifact file is absent, and
    `Test-LanguageCoverageRow` skips both numeric branches when the value is `$null`. Committed
    agent memory at `.claude/agent-memory/feature-review/coverage-hook-forces-fail-below-floor-despite-exemption.md`
    records this as an accepted working practice: *"Confirmed #327: instructed NOT to write
    `artifacts/csharp/coverage.xml` (avoids a false 85% FAIL against a pre-existing repo-wide 77.5%
    exemption) … Deliberately not producing coverage.xml is a valid tactic."* **AC4's negative-path
    proof must therefore prove more than "a low number produces non-zero"; it must prove that the
    gate cannot be satisfied by withholding the input.**

**(c) CONFIRMED [VERIFIED].** `.github/workflows/ci.yml` applies no coverage threshold. The "Run
MSTest suite with coverage" step runs `vstest.console.exe … /EnableCodeCoverage /InIsolation
/Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` and throws only on a non-zero exit code.
The next step uploads `TestResults/**/*.trx` and `TestResults/**/*.coverage` as artifacts. There is
no `.coverage` → Cobertura/JaCoCo conversion, no parsing, and no comparison. `.github/workflows/`
contains only `ci.yml` and `codex-web-setup-test.yml` [VERIFIED by Glob].

### 3b. How the hook obtains `RepoWidePct` and `BranchPct` — exact contract

This is the input contract the AC4 negative-path proof must feed.

| Language | Line source (`Get-LanguageRepoCoverage`) | Branch source (`Get-LanguageBranchCoverage`) | Path | Format |
|---|---|---|---|---|
| TypeScript | `Get-LcovRepoCoverage` | `Get-LcovBranchCoverage` | `coverage/lcov.info` | **LCOV** |
| Python | `Get-LcovRepoCoverage` | `Get-LcovBranchCoverage` | `artifacts/python/lcov.info` | **LCOV** |
| PowerShell | `Get-JacocoRepoCoverage` | `Get-JacocoBranchCoverage` | `artifacts/pester/powershell-coverage.xml` | **JaCoCo** |
| C# | `Get-JacocoRepoCoverage` | `Get-JacocoBranchCoverage` | `artifacts/csharp/coverage.xml` | **JaCoCo** |

Parsing details [VERIFIED]:

- LCOV: sums `LF:` / `LH:` prefixed lines for line coverage, `BRF:` / `BRH:` for branch. Returns
  `$null` if the file is missing or the found-total is `<= 0`.
- JaCoCo: `[xml]$doc.SelectNodes('//counter[@type="LINE"]')` and `//counter[@type="BRANCH"]`, summing
  the `missed` and `covered` attributes across **all** matching nodes at every nesting level
  (report / package / class / method). Returns `$null` if the file is missing or no counters exist.

**Critical mismatch [VERIFIED].** `scripts/vscode/Invoke-MSTestWithCoverage.ps1` emits **Cobertura**
to `coverage/coverage.cobertura.xml`. The hook reads **JaCoCo** from `artifacts/csharp/coverage.xml`.
**No committed script or workflow produces `artifacts/csharp/coverage.xml`.** The only record of how
it has ever been produced is
`docs/features/archive/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/coverage-conversion-392.2026-07-20T14-50.md`,
which states the producer was a *"Python conversion script (inline, scratchpad-only, **not committed
to the repo**)"*. The same artifact records that the multi-level counter summation is
ratio-preserving ("a constant multiplicative factor cancels in the covered/total ratio") and that
the hook's own XPath logic was re-run against the generated file and reproduced identical figures.

**Consequence for AC4 [INFERRED, high confidence].** The single numeric gate in the repository
consumes an artifact with **no committed producer**, in a format **no committed tool emits**. Any
reconciled enforcement must either (i) add a committed Cobertura→JaCoCo converter and wire it into
the toolchain, or (ii) teach the gate to read the Cobertura the repository actually produces. Option
(ii) is materially simpler and removes a whole class of format drift; it is the recommendation in R10.

### 3c. Every place a threshold constant must change for a reconciled number to take effect

Ordered by necessity.

**Must change (enforcement):**
1. `.claude/hooks/validate-feature-review-coverage.ps1` — `Test-LanguageCoverageRow`: the `85.0`
   literal, the `$BranchFloor = 75.0` literal, both message strings, and the `.SYNOPSIS` prose
   (AC8). Recommend extracting the two numbers to named script-scope constants
   (e.g. `$script:LineCoverageFloor`, `$script:BranchCoverageFloor`) with a single defining comment,
   so a future divergence is a one-line edit and is greppable.
2. **New gate logic** (AC4). No existing constant can be edited into a gate — one must be authored.
   See R10 for the recommended shape and location.

**Must change (normative documents in this feature's scope):**
3. `CLAUDE.md` § UT2 lines 297, 298, 303, 304.
4. `.claude/rules/general-unit-test.md` lines 23, 24, and the § "Coverage Exclusion Policy" block
   (31-46).
5. `.claude/rules/quality-tiers.md` lines 9, 20-21, 33-34, 51.

**Must be dispositioned under AC10 (outside this feature's edit scope, still stating numbers):**
6. `AGENTS.md:372-373`; `.github/instructions/general-unit-test.instructions.md:39-40`;
   `.claude/rules/python.md:16,88-89`; `.claude/rules/typescript.md:42-43`;
   `.claude/rules/powershell.md:63-64`; `.claude/skills/python-qa-gate/SKILL.md:46`;
   `.claude/skills/powershell-qa-gate/SKILL.md:45`; **`.claude/agents/feature-review.md:112-114,127-128`**
   (not in issue.md's list — add it); `.claude/skills/feature-review-workflow/SKILL.md:112-114`;
   the nine `.agents/skills/**` sites at 49-58 above; and the 512-owned
   `.claude/rules/csharp.md:39-40` and `.claude/skills/csharp-qa-gate/SKILL.md:46`.

**Not a threshold site, no change needed:** `coverage.config`, all `*.runsettings`,
`.github/workflows/ci.yml`, `scripts/vscode/Invoke-MSTestWithCoverage*.ps1`.

---

## R4 — The #424 / #230 Precedent, Reconstructed

### 4a. Origin — #424 plan Decisions Record item 13 (verbatim)

`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/plan.2026-08-06T21-17.md:41`:

> **13. Repository-wide coverage floor is reported, not blocking, for this change (orchestrator
> rationale, v1.2).** The recorded baseline is repo line-rate 70.19% and branch-rate 58.30% —
> already below the 80% floor before any change in this branch. The repository-wide 80% floor in
> `CLAUDE.md` / `.claude/rules/csharp.md` applies to the testable denominator after the ratified
> COM/VSTO/WinForms/Outlook-Interop exemptions in `CLAUDE.md` § UT2; the raw uninstrumented
> repo-wide figure is not that denominator and is pre-existing debt at the merge-base. A bug fix is
> not the vehicle for retiring it. The change-scoped gates — no coverage regression on changed
> lines, and >= 90% on new/changed modules and methods — remain fully blocking in [P6-T5].

Also in the same plan, item 10 (line 38): *"Out of scope (record only, do not fix): … any change
under `.claude/rules/**` or policy documents."*

Where the precedent was applied in #424:
`evidence/qa-gates/coverage-delta.2026-08-07T00-48.md` § C ("REPORTED, NON-BLOCKING — repository-wide
rates") and § D ("Documented threshold conflict"), which states:

> Both figure sets are recorded above so a reviewer can apply either. Against the **85/75** set, the
> post-change repository figures are 85.65% line (>= 85) and 79.00% branch (>= 75) — both satisfied,
> subject to the same denominator caveat. Against the **80/90** set applied by this plan, the
> blocking change-scoped gates in sections A and B all pass. No policy document was modified.

### 4b. Application by analogy — #230 plan decisions D5 and D12 (verbatim)

`docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/plan.2026-08-07T20-36.md:31` (D5):

> **D5 — Coverage gates.** Per the #424 precedent, the raw repo-wide coverage figure (~70% line at
> prior baseline) is pre-existing merge-base debt and is reported non-blocking. Blocking gates for
> this feature: (a) final full-suite line-rate >= baseline line-rate from the Phase 0 Cobertura
> capture, reported both raw and denominator-adjusted — removing 8 exemptions moves previously-
> uninstrumented members into the denominator, so the raw comparison alone is not
> denominator-stable and must not be misread as a regression; (b) changed lines in
> `QfcItemController.Initialization.cs` (factory seam parameters) covered >= 90%; (c) numeric
> per-member line coverage reported for each of the 8 de-exempted members, each > 0%. … Placeholders
> such as `UNVERIFIED` are invalid; missing numbers force a remediation-required outcome, never PASS.

Same file, line 47 (D12):

> **D12 — Coverage-threshold document conflict.** CLAUDE.md (80% repo / 90% new) and
> `.claude/rules/general-unit-test.md` (85% line / 75% branch uniform) conflict; per the
> #424-ratified handling, this plan gates on no-regression vs. the captured baseline plus the 90%
> changed-line bar (D5) and reports the raw figures without hard-gating on either document's
> repo-wide number.

The precedent's execution artifact is
`docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/qa-gates/coverage-delta.2026-08-08T00-15.md`,
which records all three gates PASS and states: *"Per D5/D12, the absolute repo-wide figure is
reported, not hard-gated; it is nonetheless above the `.claude/rules/general-unit-test.md` uniform
floors of 85% line and 75% branch, and above the CLAUDE.md 80% repo floor."*

### 4c. Assessment: is the precedent internally coherent?

**Partly. Three defects, one of which is disqualifying as written.**

1. **Its factual premise was false at the time it was written [VERIFIED].** Item 13 asserts "The
   recorded baseline is repo line-rate 70.19% and branch-rate 58.30% — already below the 80% floor."
   The same feature's own final measurement, taken two hours later with the same command form, was
   **85.65% line / 79.00% branch** (`evidence/qa-gates/final-qc-tests.2026-08-07T00-45.md`), and its
   policy-audit line 86 records: *"C# repo-wide coverage at HEAD … 85.65 percent line
   (94937/110849), 79.00 percent branch (22001/27848) — **PASS** against both the CLAUDE.md 80
   percent floor and the general-unit-test 85/75 floors."* The feature's own evidence
   (`coverage-delta.2026-08-07T00-48.md:65`) calls the difference "a measurement artifact" and
   attributes it to denominator instability, not to the change. **The precedent's justification —
   "the floor is unreachable, so report it" — rested on a measurement that the same feature
   disproved within the same run.**

2. **It is a per-change disposition, not a threshold rule.** Read literally, it says nothing about
   what the repository-wide floor *is*; it says only that a bug fix will not be blocked by it. That
   is a legitimate change-scoping principle. It is not, and cannot be, an answer to AC1.

3. **It conflates "not applicable to this change" with "not enforced".** Because no tooling
   enforces any repo-wide number (R3), "reported, non-blocking" is the *de facto* universal state,
   not a considered exception granted to two features.

### 4d. Compatibility with each camp

The precedent's **change-scoped** half is compatible with **both** camps and with either
reconciliation outcome:
- "no regression on changed lines" appears verbatim in `CLAUDE.md:305`, `.claude/rules/csharp.md:41`,
  `.claude/rules/general-unit-test.md:25`, `.claude/rules/powershell.md:65`,
  `.claude/rules/python.md:90`, `.claude/rules/typescript.md:45`, and `.claude/rules/quality-tiers.md:35`.
- the "90% changed-line / new-unit bar" is the 80/90 camp's own new-code number
  (`CLAUDE.md:304`). The 85/75 camp states **no** new-code number at all. So ratifying the 90%
  changed-line bar is a strict addition to the 85/75 camp and a restatement of the 80/90 camp.

The precedent's **repo-wide** half ("raw repo-wide figures non-blocking") is compatible with neither
camp as written: both camps state the repo-wide floor as a "must remain" requirement.

### 4e. Recommendation to the spec on AC5

**Split the precedent and dispose of the two halves differently.** The spec should:

- **Ratify** the change-scoped half as the written rule, in the authoritative document, with
  explicit numbers: (i) no coverage regression on changed lines, blocking; (ii) new or changed units
  meet the new-code bar, blocking. Both already appear in every document; ratifying them costs
  nothing and closes AC5's first branch.
- **Explicitly supersede** the "raw repo-wide figures non-blocking" half, in those words, and
  replace it with a written rule that says what the repo-wide figure *is measured against* (the
  reconciled denominator from AC2) and *what happens when it falls below* (blocking, or blocking
  with a named disposition path — see the branch-check asymmetry in R3b).
- Record the 70.19%-vs-85.65% discrepancy as the stated reason the precedent cannot be carried
  forward unmodified. This is the strongest available justification and it is entirely in-repo.

---

## R5 — Feasibility of Each Candidate Threshold, Measured

**No test suite was run in this session, per instruction.** All figures below are read from
committed evidence.

### 5a. Most recent committed repository-wide C# measurement

| Rank | Date | Line | Branch | lines-covered / lines-valid | Evidence path |
|---|---|---|---|---|---|
| Most recent | **2026-08-08T00-15** (post-#230) | **85.8333%** | **79.2226%** | 95,293 / 111,021 | `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/qa-gates/coverage-final.cobertura.xml` (root `<coverage>`), tabulated in `evidence/qa-gates/coverage-delta.2026-08-08T00-15.md` |
| — | 2026-08-07T21-52 (#230 baseline) | 85.6453% | 79.0039% | 94,937 / 110,849 | `.../230/evidence/baseline/coverage-baseline.cobertura.xml` + `baseline-test-coverage.2026-08-07T21-52.md` |
| — | 2026-08-07T00-45 (#424 final) | 85.6453% | 79.0039% | 94,937 / 110,849 | `.../424/evidence/qa-gates/coverage-final.cobertura.xml` |
| — | 2026-08-06T22-31 (#424 baseline) | **70.1927%** | **58.2976%** | 56,124 / 79,957 | `.../424/evidence/baseline/test-coverage-baseline.2026-08-06T22-31.md` |
| — | 2026-08-04 (#418) | 85.3844% | 78.5521% | 93,484 / 109,486 | `.claude/agent-memory/feature-review/project_csharp-repowide-coverage-below-80.md` (#418 data point) |

**These figures are computed under the defective arithmetic [VERIFIED].** The root `<coverage>`
attributes in every one of these artifacts are **overwritten** by
`ConvertTo-KoverageCoberturaXml`, which calls `Get-CoberturaCoverageSummary` and then does
`$xml.coverage.SetAttribute('line-rate', …)`, `SetAttribute('lines-valid', …)`, and so on for all
six attributes. `Get-CoberturaCoverageSummary` selects over `.//lines/line` — the descendant axis —
which is exactly the #441 double-count. The epic's own statement that
`lines-valid="110849"` equals the raw `<line number=` count is consistent with this. **None of the
five figures above is the number the #494 decision will be made against.**

### 5b. A denominator-instability finding that is larger than #441/#457 and is not in the epic's scope

**[VERIFIED, and this is the single most important finding for how the spec frames the decision.]**

Two full-suite runs of the *same command form* on *essentially the same tree*, roughly 26 hours
apart, produced denominators that differ by **38.6%**:

| Run | Command | lines-valid | line-rate |
|---|---|---|---|
| #424 P0-T7, 2026-08-06T22-31 | `Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput …` | 79,957 | 70.19% |
| #424 P6-T4, 2026-08-07T00-45 | same script | 110,849 | 85.65% |

The #424 evidence itself records the diagnosis
(`evidence/qa-gates/coverage-delta.2026-08-07T00-48.md:65`):

> **Interpretation caveat — the two figures are not like-for-like.** The denominator grew from
> 79,957 to 110,849 valid lines (+38.6%), which this plan's ~600 added lines cannot explain. This is
> the known `dotnet-coverage` denominator instability for this repository: which assemblies get
> instrumented, and therefore how much uninstrumented vendored code lands in the denominator, varies
> between full-suite runs. The apparent +15.5-point line-rate improvement is therefore **not** a
> claim this change made; it is a measurement artifact.

Corroborating spread across the historical record for the *same nominal quantity*
(`.claude/agent-memory/feature-review/project_csharp-repowide-coverage-below-80.md`): ~58.9%
(raw all-DLL merge), 65.73% (two-assembly scope), 69.33% (#253 raw), ~79.4% (Koverage, 2026-07-06),
85.38% (#418, 2026-08-04), 88.82% (#269 by-name exclusion), 91.22% (#253 by-name exclusion).

**Assessment [INFERRED, high confidence].** #441/#478 fix *how* lines are counted and #457 fixes
*which* lines enter the denominator from `[ExcludeFromCodeCoverage]`. **Neither fixes non-deterministic
assembly instrumentation between runs.** A repository-wide numeric floor is only enforceable if the
measurement is reproducible; a metric with a ±15-point run-to-run spread cannot support one.

**Recommendation for the spec.** Add a re-measurement acceptance condition to AC7's task: the
re-measurement must be run **at least twice** (ideally three times) after #441/#457 land, and the
spread recorded. If the spread exceeds a small tolerance (suggest 1.0 percentage point), the spec
must either (i) file a new blocking issue for measurement determinism before ratifying any
repo-wide number, or (ii) ratify the change-scoped gates only and record the repo-wide figure as a
tracked trend with an explicit written statement that it is not enforceable until determinism is
established. Option (ii) is the honest reading of the evidence and is *also* what R4 shows agents
have been doing informally — the difference is that AC5 would make it a written rule with a named
exit condition rather than an improvisation.

### 5c. Committed per-assembly / per-project breakdown

**Yes, one exists [VERIFIED].** The committed Cobertura files carry `<package>` elements with their
own `line-rate` / `branch-rate`. Most recent (post-#230,
`docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/qa-gates/coverage-final.cobertura.xml`):

| Package | Line | Branch | XML line |
|---|---:|---:|---:|
| VBFunctions | 100.00% | 100.00% | 187080 |
| TaskTree | 95.48% | 92.16% | 186079 |
| Tags | 92.69% | 91.58% | 175334 |
| TaskVisualization | 89.84% | 83.25% | 158162 |
| UtilitiesCS | 89.54% | 83.41% | 28101 |
| QuickFiler | 80.82% | 74.65% | 7 |
| TaskMaster | 70.97% | 65.18% | 177948 |
| ToDoModel | 57.31% | 48.82% | 169365 |
| SVGControl | 47.30% | 47.02% | 163148 |

**These per-package figures are more trustworthy than the root figures [VERIFIED by code reading].**
`ConvertTo-KoverageCoberturaXml` rewrites only the **root** `<coverage>` attributes;
`Merge-CoberturaClassesByFilename` recomputes only **class-level** `line-rate`/`branch-rate`. No code
path recomputes `<package>` attributes, so they remain `dotnet-coverage`'s own output and are not
subject to the #441 descendant-axis double count. (They are still subject to #457 and to the
instrumentation instability in 5b.)

**Consequence for the decision [INFERRED, high confidence].** Nine production packages exist. Under
any uniform repo-wide floor of 80% or above, **three of nine fail at package scope today**
(TaskMaster 70.97%, ToDoModel 57.31%, SVGControl 47.30%), and QuickFiler at 80.82% line / 74.65%
branch fails an 85/75 bar. Under 85/75, **five of nine fail on line and five of nine fail on
branch**. The spec must state which of these it is choosing to declare failing on day one, and with
what remediation path — this is a required part of "justify the number".

Note also `.claude/agent-memory/feature-review/project_csharp-repowide-coverage-below-80.md` (#418
data point): SVGControl at 47% "is dominated by wholly-untested files (`DropDownEditor` 0/99,
`SVGParser` 0/122, `ToggleSwitch` 0/62+0/23, `SvgFileNameEditor` 0/104, three converters 0/48, 0/48,
0/26), which makes any `SVGControl`-touching change unable to meet the 85% modified-file floor at
file scope no matter how well the change itself is tested." A related open item exists at
`docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`.

### 5d. Cross-language posture

| Language | Production code present? | Coverage evidence | Assessment |
|---|---|---|---|
| **C#** | Yes — 9 production + 9 test projects in `TaskMaster.sln` | Extensive (5a, 5c) | The only language with real measurement |
| **PowerShell** | Yes — 10 `.ps1` under `scripts/`, 33 `.ps1` under `.claude/hooks/` | `tests/scripts/vscode/` holds **4** Pester files covering **3** of the 10 `scripts/` files (`Install-RepoDotNetSdk`, `Invoke-MSTestWithCoverage.Helpers`, `Invoke-VSBuild`, plus `Invoke-MSTest.RunSettings`). **No test exists for any of the 33 hooks.** No committed `artifacts/pester/powershell-coverage.xml`. One historical data point in memory: 72.73% raw (#283). | Well below any candidate floor; the hooks are entirely untested |
| **Python** | **None.** Glob `**/*.py` returns 2 files, both inside `docs/features/archive/2026-07-18-…-354/` (an archived feature's own scripts/tests). No `pyproject.toml`, no `poetry.lock`, no `requirements*.txt`, no `setup.py`. | none | `.claude/rules/python.md` is **wholly inapplicable** to this repository |
| **TypeScript** | **None.** Glob `**/*.ts` returns **no files**. No `package.json`, no `node_modules/`, no `src/`. | none | `.claude/rules/typescript.md` is **wholly inapplicable** |

**Consequence for AC1/AC10 [INFERRED, high confidence].** The "cross-language reconciled policy"
framing in the epic is partly hypothetical: two of the four languages have no code here. The
`.claude/rules/python.md` and `.claude/rules/typescript.md` threshold statements can be dispositioned
under AC10 as **non-normative-by-absence** at essentially zero cost, which is a cheaper and more
honest disposition than aligning numbers in documents that govern nothing. The PowerShell posture is
the one that actually matters and it is the weakest: **the only numeric gate in the repository
(`validate-feature-review-coverage.ps1`) has no test of its own**, which AC4 and AC9 together will fix.

---

## R6 — The `quality-tiers.yml` / `tier-classification` Claim

### 6a. Re-confirmation of absence

| Claim in `.claude/rules/quality-tiers.md` | Reality | Method |
|---|---|---|
| line 9, 20: "`quality-tiers.yml` at repo root maps every project to one tier" | **Absent.** | Glob `quality-tiers.y*ml` → no files |
| line 21: "The CI pipeline's `tier-classification` stage validates that every project entry has a tier…" | **Absent.** | Glob `.github/workflows/*` → only `ci.yml`, `codex-web-setup-test.yml`; grep for `tier` in `ci.yml` → no matches |
| line 9: "The tier system source of truth is `docs/ci.research.md` section 1" | **The document does not exist anywhere in the repository.** | Glob `**/ci.research*` → no files. Grep `ci\.research` across the worktree → exactly 2 hits, both the citation itself (`.claude/rules/quality-tiers.md:9`, `.agents/skills/quality-tiers/SKILL.md:15`) |

All three [VERIFIED]. The issue's item 4 is confirmed and **extended**: the claimed *source of
truth* for the tier system is also missing, not just the mapping file and the CI stage. The same
three false claims are duplicated at `.agents/skills/quality-tiers/SKILL.md:15,26-27`.

### 6b. What authoring it would require

**Project enumeration from `TaskMaster.sln` [VERIFIED].** 18 project entries plus one solution
folder ("Solution Items", GUID `{2150E333-…}`, not a project).

Production (9): `Tags`, `ToDoModel`, `TaskVisualization`, `UtilitiesCS`, `QuickFiler`, `TaskTree`,
`TaskMaster`, `SVGControl`, `VBFunctions`.
Test (9): `ToDoModel.Test`, `UtilitiesCS.Test`, `QuickFiler.Test`, `TaskVisualization.Test`,
`Tags.Test`, `TaskTree.Test`, `SVGControl.Test`, `VBFunctions.Test`, `TaskMaster.Test`.

(Incidental correction to a stale prior note: `SVGControl.Test` **is** present in the solution at
`TaskMaster.sln:42`.)

**Does any tier assignment already exist anywhere? No [VERIFIED].** Grep for `\bT[1-4]\b` /
`tier` across the repository finds tier *references* only inside `.claude/rules/quality-tiers.md`,
`.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`,
`.claude/rules/architecture-boundaries.md`, `.claude/skills/**`, and the `.agents/` mirrors. No file
assigns a tier to any TaskMaster project.

**The tier examples in the document do not match this repository [VERIFIED].** `quality-tiers.md`
lines 13-16 name, as T1: "classifier engines (SpamBayes, Triage), ToDo ID allocator and hierarchy
operations, Graph extended-properties adapter, auth/token handling, host-agnostic command bus"; as
T2: "`TaskMaster.Domain`, `TaskMaster.Application`, mail-item DTOs, settings store abstraction"; as
T3: "Outlook task pane UI, Office.js wrappers, Microsoft Graph SDK wrappers". The header on line 13
literally says "Examples (No-COM architecture)". `TaskMaster.Domain` and `TaskMaster.Application` do
not exist in `TaskMaster.sln`. There is no Graph adapter, no Office.js, no task pane. Only "Triage"
maps to real code (in `UtilitiesCS`). This is the same foreign-import signature documented in R2.

**What a `tier-classification` CI stage would have to validate.** Per the document: (i) every entry
in `quality-tiers.yml` carries a tier in `{T1,T2,T3,T4}`; (ii) the set of projects in the solution is
a subset of the classified set (no unclassified project); (iii) fail the build otherwise. In
addition, if the tier system is to have any effect, the stage would need to feed the tier-dependent
gate matrix (lines 39-47), which requires four capabilities this repository **does not have**
[VERIFIED]: `dependency-cruiser` / `NetArchTest.Rules` (architecture violations),
`fast-check` / `hypothesis` (property test density), a mutation-testing runner (mutation score
>= 75% for T1), and a golden-corpus harness.

### 6c. Cost comparison for the AC6 decision

| Option | Work required | Ongoing cost | Value delivered |
|---|---|---|---|
| **A — Author it** | Create `quality-tiers.yml` with 18 entries; author `docs/ci.research.md` § 1 or redirect the citation; add a `tier-classification` job to `ci.yml`; author a validator script; author its Pester test at `tests/scripts/…`; rewrite the tier *examples* in `quality-tiers.md` to name real projects | Every new project must be classified or CI fails; the file must be kept in sync with `TaskMaster.sln` | **Near zero for coverage.** `quality-tiers.md:25` and `:51` both state line and branch coverage thresholds are **uniform across T1–T4**. The classification changes no coverage gate. Its only live consumers would be four gates the repository cannot run (6b). |
| **B — Remove the claim** | Delete or rewrite `quality-tiers.md` lines 9, 20-21 (and the tier examples at 13-16 that name non-existent projects); mirror in `.agents/skills/quality-tiers/SKILL.md:15,26-27` | None | Removes three false assertions from an always-loaded rule file; removes a citation to a non-existent source of truth |

**Recommendation [INFERRED, high confidence]: Option B, with a narrow carve-out.** The document's
own text defeats the case for Option A: it states the coverage thresholds are uniform across tiers,
so the classification cannot affect the metric this feature is reconciling. Authoring a mapping file
plus a CI stage plus a validator plus its test, in order to feed gates that have no tooling, is
cost with no gate-fidelity return — and this epic exists specifically to stop gates from claiming
things they do not do.

The carve-out: `.claude/rules/architecture-boundaries.md:10` and `.claude/rules/powershell.md:63-64`
both reference "all tiers (T1–T4)". If the tier vocabulary is removed wholesale, those references
dangle. The minimal coherent edit is to remove the **false claims** (the `quality-tiers.yml` file,
the `tier-classification` CI stage, the `docs/ci.research.md` source of truth, and the non-existent
project examples) while retaining the T1–T4 vocabulary as a descriptive taxonomy with no asserted
enforcement mechanism — or, if the maintainer prefers, to remove the tier system entirely and clean
up the three dangling references in the same change. **Both are defensible; this is a scope choice
the spec should put to the maintainer as part of the single question in Automation Feasibility.**

---

## R7 — Authority-Rule Mechanisms Available

### 7a. Mechanisms that exist in this repository today

**1. Prose precedence lists (two of them, and they do not agree in structure) [VERIFIED].**

`CLAUDE.md:9-16` — "Policy Compliance Order":
> The four core policies below are embedded directly in this file and apply to every session…
> 1. This file (CLAUDE.md) — all sections
> 2. General Code Change Policy (§ below)
> 3. General Unit Test Policy (§ below)
> 4. For C#: C# Code Change Policy (§ below) and C# Unit Test Policy (§ below)

**This list ranks only `CLAUDE.md`'s own embedded sections.** It never mentions `.claude/rules/`.
It therefore does **not**, on its own text, place `CLAUDE.md` above
`.claude/rules/general-unit-test.md`. The issue's statement that "CLAUDE.md's own Policy Compliance
Order places itself first, which would make 80/90 authoritative" is a reasonable reading but is not
what the document says.

`.claude/skills/policy-compliance-order/SKILL.md:17-28` — "Required Policy Reading Order (Baseline)":
> Claude Code auto-loads rules via path-scoped frontmatter in `.claude/rules/`. **This ordering
> documents precedence when policies conflict:**
> 1) `CLAUDE.md` (standing instructions, always loaded)
> 2) `.claude/rules/general-code-change.md`
> 3) `.claude/rules/general-unit-test.md`
> 4) Language- or domain-specific rules…

**This is the only document in the repository that actually states precedence between `CLAUDE.md`
and `.claude/rules/`.** It puts `CLAUDE.md` first. It is a *skill*, not a rule, and it is not
auto-loaded — `CLAUDE.md`'s "Key Skills Reference" lists it under "Background skills (always read
explicitly when invoked)". So today the authority rule is stated in a document that only loads on
demand.

**2. Path-scoped frontmatter [VERIFIED].** Every `.claude/rules/*.md` carries YAML frontmatter with
`paths:` and `description:`. Observed values: `general-unit-test.md` and `quality-tiers.md` use
`paths: ["**"]`; `csharp.md` uses `["**/*.cs", "**/*.csproj"]`; `architecture-boundaries.md` uses
`["**/*.ts", "**/*.cs"]`. This scopes *when* a rule loads. It does **not** express precedence
between two rules that both match. It is a necessary but insufficient mechanism for AC3.

**3. `CLAUDE.md`'s halt directive [VERIFIED].** `CLAUDE.md:24` — "If you encounter **any**
conflicting instructions, halt and notify the user." Repeated at `CLAUDE.md:177` and `:359`. This is
the current conflict-resolution mechanism, and R4 demonstrates it is not followed.

**4. Precedent carried in committed agent memory.** See R1f. Effective in practice, invisible to
reviewers, and explicitly the thing #494 exists to eliminate.

### 7b. Is "cite, do not restate" already used, and is it enforceable?

**It is used — but always in the degenerate form "cite *and* restate", which is the drift vector
itself [VERIFIED].** Every citation instance found:

- `.claude/rules/powershell.md:63` — "Line coverage must remain **>= 85%** across all tiers (T1–T4)
  **per `.claude/rules/quality-tiers.md`**." Cites and restates.
- `.claude/skills/powershell-qa-gate/SKILL.md:45` — "line coverage **>= 85%** and branch coverage
  **>= 75%** **per the uniform tier rule (`.claude/rules/quality-tiers.md`)**." Cites and restates.
- `.claude/skills/feature-review-workflow/SKILL.md:111` — "Coverage thresholds (**uniform tier rule
  per quality-tiers.md**):" followed by three restated numeric lines (112-114).
- `.claude/rules/general-unit-test.md:26` — cites `quality-tiers.md` for the tier system, after
  restating 85/75 at lines 23-24.

**No instance was found anywhere in the repository of a document citing a coverage authority
*without* restating the number.** Search scope: `.claude/rules/`, `.claude/skills/`,
`.claude/agents/`, `.agents/`, `AGENTS.md`, `CLAUDE.md`, `.github/instructions/`.

**Enforceability [INFERRED, high confidence]. Yes, and cheaply.** The convention is mechanically
checkable by a single regex over the governance surface: *no file other than the named authority may
contain a coverage-threshold numeric literal*. A PowerShell validator plus a Pester test is roughly
60-80 lines and reuses the exact pattern already proven at
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (dot-source a pure function, feed
it an inline here-string, assert). Recommended shape:

- Authority: **`CLAUDE.md` § UT2** — it is the only always-loaded document, it is first in both
  precedence lists, and the exemption text that AC2 must reconcile already lives there.
- Non-authoritative documents state the rule by reference only:
  *"Coverage thresholds are defined in `CLAUDE.md` § UT2. This document does not restate them."*
- Mechanical guard: a check that greps the governance surface for coverage-threshold numerals and
  fails on any hit outside the authority file and the enforcement script's named constants. This is
  a natural second Pester test alongside the AC4 negative-path test, and it makes AC3 *provable*
  rather than aspirational.
- The enforcement script's constants are the one permitted duplicate; keep them named and add a
  comment pointing at the authority, so the pair is greppable.

**One structural risk to record.** `.claude/rules/general-unit-test.md` carries `paths: ["**"]` and
is auto-loaded on every file; `CLAUDE.md` is also always loaded. Both will always be in context
together. If the authority is `CLAUDE.md` and `general-unit-test.md` cites it, that is coherent. If
the authority were placed in a *path-scoped* rule instead, sessions touching non-matching files
would see the citation but not the authority. **The authority must live in an always-loaded
document.** `CLAUDE.md` and `.claude/rules/general-unit-test.md` (`paths: ["**"]`) are the only two
qualifying candidates.

---

## R8 — The 512 Boundary

### 8a. Exact line ranges in `CLAUDE.md` (as of `edf3d34c`)

**512-owned (C# toolchain command blocks) — three disjoint regions [VERIFIED]:**

| Region | Lines | Content |
|---|---|---|
| A | **181-208** | `### C#1. Tooling & Baseline for C#`. The three numbered tool blocks are 185-192 (csharpier — issue #509), 194-199 (analyzers), 201-206 (nullable — issues #512/#492/#522). Line 208 is the "Testing tools … are defined in the unit test policies" pointer. |
| B | **377-386** | `### CUT3. C# Toolchain Command Selection`. The four numbered commands are **381-384**; 386 is the loop-behavior pointer. |
| C | **397-402** | `## C# Toolchain (run in this exact order)` — the duplicate appendix. The four commands are **399-402**. |

**494-owned (§ UT2 coverage block) [VERIFIED]:**

| Region | Lines | Content |
|---|---|---|
| D | **292-306** | `### UT2. Coverage and Scenarios` heading (292) through the last coverage bullet (306). The threshold and exemption statements are 296, 297, 298-303, 304, 305. Lines 308-315 are the "Scenario Completeness" sub-block, which #494 need not touch. |

**Disjointness [VERIFIED].** D ends at 306. B begins at 377. The nearest approach between any
494-owned and any 512-owned line is **71 lines** (306 → 377). Regions A (181-208) and D (292-306)
are separated by 84 lines. **The two features' `CLAUDE.md` edits are provably disjoint and will
merge cleanly**, which confirms the epic's decision not to place an ordering edge between them.

**Discrepancy to flag [VERIFIED].** The epic charter (`epic.md:124-126`) states 512 edits
"`CLAUDE.md` lines 185-206 and 381-401". The range `381-401` **encloses `## Tone Policy` (390-395)**,
which belongs to neither feature. The epic's stated range is over-broad by 6 lines. Recommended
disposition: the 512 plan should cite regions **B (377-386)** and **C (397-402)** separately rather
than the merged span, and explicitly exclude 388-396. This is a note for the epic orchestrator, not
a blocker for #494.

### 8b. The genuine overlap — 512-owned *files* containing 494-relevant *content*

| Site | Content | In 512's stated edit range? | Risk |
|---|---|---|---|
| `.claude/rules/csharp.md:39-40` | ">= 80%" repo-wide, ">= 90%" new | **No.** Epic scopes 512 to csharp.md lines **14-16** (toolchain commands) and **83** (severity-first ordering). Lines 39-40 sit in `## Testing Standards`, 23 lines below 512's upper bound and 43 above its lower. | **Low file-level, high semantic.** No line conflict. But if #494 reconciles to anything other than 80/90, `.claude/rules/csharp.md:39-40` is left stating a superseded number in an auto-loaded, path-scoped (`**/*.cs`) rule that fires on every C# change. |
| `.claude/skills/csharp-qa-gate/SKILL.md:46` | ">= 90% for each new unit" | **No.** Epic scopes 512 to line **32** (the nullable command). Line 46 is in `## Delta Requirements`. | Same shape. Also note lines 44-45 state per-file and overall coverage deltas with **no numbers**, so only line 46 is a numeric site. |

**Recommended disposition [INFERRED, high confidence].** Do **not** widen #494's edit scope into
512-owned files — the issue forbids it and the epic's Execution Authorization is scoped per-issue.
Instead:

1. **AC10 disposition, recorded in `spec.md`:** enumerate `.claude/rules/csharp.md:39-41` and
   `.claude/skills/csharp-qa-gate/SKILL.md:46` as **deferred to a named follow-up issue**, filed
   through the MCP promotion lifecycle so it survives the merge (prose in a feature folder does not).
2. **Make the deferral safe under AC3.** If AC3's authority rule states that `CLAUDE.md` § UT2 is
   authoritative and that all other documents are non-normative on coverage numbers, then a
   superseded literal in `csharp.md` is *incorrect* but not *authoritative*, so the window between
   #494 landing and the follow-up landing is a documentation-freshness gap rather than a live policy
   conflict. **This is the strongest argument for choosing `CLAUDE.md` § UT2 as the authority
   (R7).** With a different authority choice, the same deferral leaves a genuine contradiction open.
3. **Sequencing note for the epic orchestrator.** If 512 merges first, its diff to `csharp.md`
   touches lines 14-16 and 83 only; a later #494-driven edit to 39-41 by the follow-up would not
   conflict. Order is immaterial. No wave edge is needed.

---

## R9 — Dependency-Aware Re-Measurement Design (AC7)

### 9a. Prepared plans for 441 / 457 — status

**None exist [VERIFIED].** Glob `docs/features/active/*/spec.md` returns eight folders:
`…-400`, `…-420`, `…-424`, `…-438`, `…-230`, `…-503`, `…-505`, and `…-494`. There is **no**
`cobertura-coverage-arithmetic-441`, no `excludefromcodecoverage-nested-lambdas-457`, no
`csharp-toolchain-gate-fidelity-512`, and no `utilitiescs-test-cs2002-…-394` folder. The #494 folder
itself contains only `issue.md`, an unfilled `spec.md` template, and `plan.2026-08-10T14-10.md`.

**Consequence.** The re-measurement task cannot be aligned to 441/457's actual output shape at
research time. The plan must state that the executor **re-reads** `Get-CoberturaCoverageSummary`,
`Merge-CoberturaClassesByFilename`, and `ConvertTo-KoverageCoberturaXml` at execution time and
records their then-current signatures in the baseline evidence artifact before running anything.
Anchor on those three symbol names, never on line numbers.

### 9b. The corrected invocation — verified against current scripts

**Step 1 — Build (must be `/t:Rebuild`, not `/t:Build`) [VERIFIED].** `.github/workflows/ci.yml`
lines 106-115 document the reason in-line:

> Use `/t:Rebuild` (not `/t:Build`) so this step always performs a genuine full recompile. …
> MSBuild's incremental up-to-date check does not invalidate on this command-line property change
> alone, so a plain `/t:Build` would silently skip recompilation and never enforce this gate.

CI itself uses `/t:Rebuild` for the nullable step and `/t:Build` for the analyzer step. For a
coverage re-measurement the relevant hazard is stale `*.Test.dll` binaries, so the re-measurement
must be preceded by a full rebuild:
```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
```
Do **not** add `/p:Nullable=enable`: `epic.md:116-119` records issue #522 — that switch is
deliberately absent from CI and produces ~200-414 errors red on a clean `main`. #494's own
`issue.md` Out of Scope confirms it is not a gate for this feature.

**Step 2 — Coverage run [VERIFIED against the script].**
```
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot . -Configuration Debug `
    -CoverageOutput docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/coverage-remeasure-<n>.cobertura.xml
```
What this actually does, read from `Invoke-MSTestWithCoverageMain`:
`$repoRoot = Resolve-Path($ScriptRoot\..\..)`; discovery = `Get-ChildItem -Recurse -Filter '*.Test.dll'`
filtered to `\\bin\\$Configuration\\` and excluding `\\obj\\` and `\\ref\\`; then
`Invoke-DotnetCoverageCollection`, which builds
`dotnet-coverage collect --output <path> --output-format cobertura --settings <derived config> -- <vstest> <assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`;
then `ConvertTo-KoverageCoberturaXml` post-processing.

**Emitted artifact:** a single Cobertura XML at the `-CoverageOutput` path (~10 MB; the #230 baseline
is 10,398,171 bytes). It carries root `<coverage>` attributes (post-#441 these become the corrected
figures) **and** nine `<package>` elements with untouched `line-rate`/`branch-rate` — which is the
per-assembly breakdown AC7 needs (R5c).

### 9c. The three operational hazards — verified status

| Hazard | Verified status | Correct handling |
|---|---|---|
| **`/InIsolation` per-assembly on host crash** | The script **already passes `/InIsolation`** unconditionally (`Get-DotnetCoverageArgumentList`) [VERIFIED]. Host crashes are nonetheless documented across the repo record: `…-418/evidence/qa-gates/toolchain-clean-pass.2026-08-04T14-36.md` ("aborted with `Test host process crashed` after 1266 passing tests"); `…-400/evidence/qa-gates/final-pass-integrity.2026-07-21T20-40.md`; `archive/…-374/evidence/regression-testing/batch-d-tests.md` (attributed to "concurrent sibling-worktree agent" runs) | `/InIsolation` is already on. The residual crash cause in the record is **concurrent test runs from sibling worktrees**, not isolation mode. The plan should require that no other agent worktree is executing tests during the re-measurement, and should record `Total tests:` explicitly so a `Total tests: Unknown` outcome is detected rather than silently accepted. |
| **Exclude `\.claude\` worktree paths from `*.Test.dll` discovery** | **The script does NOT implement this filter** [VERIFIED — `Invoke-MSTestWithCoverageMain` filters only on `\bin\<Config>\`, `\obj\`, `\ref\`]. It is a **plan-level** control: #424 plan Decisions item 9, evidenced at `…-424/evidence/baseline/test-coverage-baseline.2026-08-06T22-31.md:26-40` ("**9 assemblies discovered; `CLAUDE_PATH_COUNT = 0`**"). | The naive substring test is **wrong when running inside a worktree**, and this is documented at `docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/issue.md:101-104` and its plan § "MSTest Discovery Caveat": the workspace root is itself under `\.claude\worktrees\`. **Correct assertion:** every discovered path begins with the workspace-root prefix, **and** no discovered path contains a `\.claude\worktrees\` segment *after* that prefix. **[VERIFIED for this worktree]** Glob `.claude/worktrees/**` inside this worktree returns no files, so discovery is naturally scoped here — but the plan must assert it rather than assume it, and must record the assembly list and count in the evidence artifact as #424 did. |
| **MSBuild `/t:Build` skips `CoreCompile`** | **CONFIRMED** from `ci.yml:106-112` [VERIFIED]. | Use `/t:Rebuild` (9b). |

**A fourth hazard, found in this session and not in the prompt's list [VERIFIED].**
`Get-KoverageProjectAllowlist` builds the package allowlist by
`Get-ChildItem -Path $RepoRoot -Recurse -File -Include '*.csproj','*.vbproj','*.fsproj'`, filtering
only `\bin\`, `\obj\`, `\packages\`. It does **not** exclude `\.claude\worktrees\`. When run from the
**main** repository checkout (where ~20 agent worktrees live), it recurses into every one of them.
Because it collects assembly *names* into a case-insensitive `HashSet`, the practical effect is
benign (a union of identical names), but the scan is O(worktrees) and the behavior is undocumented.
Record it as an observation; if the re-measurement is run from the main checkout rather than a
worktree, note the elapsed-time impact in the evidence artifact. This is #441-adjacent tooling — do
**not** fix it in #494.

### 9d. Recommended re-measurement task shape for the plan

1. **[P0]** Record the then-current signatures of `Get-CoberturaCoverageSummary`,
   `Merge-CoberturaClassesByFilename`, `ConvertTo-KoverageCoberturaXml`, and
   `Get-KoverageProjectAllowlist` (symbol-anchored, post-#441/#457), plus the resolved
   `dotnet-coverage` and `vstest.console.exe` versions. Evidence: `evidence/baseline/`.
2. **[P0]** `msbuild TaskMaster.sln /t:Rebuild …` — full recompile. Record `EXIT_CODE` and
   warning/error counts.
3. **[P0]** Assert test-assembly discovery: record the full discovered list, the count, and the
   worktree-prefix assertion from 9c. Fail the task if any path carries a nested
   `\.claude\worktrees\` segment after the workspace-root prefix.
4. **[P0]** Run the coverage command from 9b **three times**, writing
   `coverage-remeasure-1|2|3.cobertura.xml` under `evidence/baseline/`. Record for each: root
   `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered`, `branches-valid`,
   `Total tests`, and all nine `<package>` `line-rate`/`branch-rate` values.
5. **[P0]** Compute and record the **spread** across the three runs, per R5b. This artifact is the
   direct input to the AC7 decision and to the determinism question; it belongs in
   `evidence/baseline/` with a name such as `coverage-remeasure-spread.<ts>.md`.
6. The number written into the governance documents must cite this artifact by path.

Note the epic NFR (`epic.md:18-19`): "No coverage threshold may be lowered to accommodate a corrected
denominator without an explicit, recorded decision." The three-run spread artifact is what makes
that decision recordable rather than assertable.

---

## R10 — Negative-Path Proof Design (AC4)

### 10a. Constraints in force

- No temporary files, anywhere, ever (`CLAUDE.md:331` "expressly prohibited … Currently approved
  exceptions: none"; `.claude/rules/general-unit-test.md:73`;
  `.claude/rules/general-code-change.md` § I/O Boundaries).
- Deterministic; independent; no external services or processes.
- Pester v5.x; `*.Tests.ps1`; located in a `tests/` tree mirroring the production source
  (`.claude/rules/general-unit-test.md:78`, `.claude/rules/powershell.md:57`).
- The gate must fail on a below-threshold input **and** must not be satisfiable by withholding the
  input (R3b).

### 10b. Correction to AC9's stated path

AC9 says "Any Pester tests added live at `tests/scripts/powershell/<Name>.Tests.ps1`". That literal
path derives from the *example* in `.claude/rules/general-unit-test.md:78`
("the test for `scripts/powershell/Foo.ps1` belongs at `tests/scripts/powershell/Foo.Tests.ps1`").
The rule is a **mirroring** rule, and `.claude/rules/powershell.md:57` gives a different example
(`tests/scripts/dev-tools/ScriptName.Tests.ps1`) for the same rule.

**This repository has no `scripts/powershell/` directory [VERIFIED].** It has `scripts/vscode/`
(7 files) and `scripts/dev-tools/` (1 file). And `tests/scripts/vscode/` **already exists** with four
Pester files [VERIFIED] — so the tree is established and the issue's "`tests/scripts/powershell/`
does not exist" observation, while literally true, is not the operative fact.

**Recommendation.** The correct location is determined by where the gate script lives:
- gate at `scripts/vscode/<Name>.ps1` → test at `tests/scripts/vscode/<Name>.Tests.ps1` (joins the
  existing tree; **preferred**)
- gate at `scripts/dev-tools/<Name>.ps1` → test at `tests/scripts/dev-tools/<Name>.Tests.ps1`
- test for the existing hook `.claude/hooks/validate-feature-review-coverage.ps1` → the mirror would
  be `tests/.claude/hooks/validate-feature-review-coverage.Tests.ps1`, which is awkward but is what
  the mirroring rule dictates.

The spec should **restate AC9 in mirroring terms** rather than as a fixed literal path, and record
the restatement as a deliberate correction.

### 10c. Options evaluated

| Option | Determinism | No temp files | Layout | Proves AC4? | Assessment |
|---|---|---|---|---|---|
| **1 — Inline here-string XML fixture fed to a pure gate function** | Yes | Yes | Yes | Yes | **Recommended.** Exactly the pattern already proven at `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, which builds Cobertura XML in a `@'…'@` here-string and passes it to `ConvertTo-KoverageCoberturaXml`. Zero new infrastructure. |
| **2 — Committed fixture XML file under the feature's evidence tree** | Yes | Yes (a committed fixture is not a temp file) | Fixture placement is unregulated | Yes | Viable, but adds a file whose *purpose* is unclear to a reviewer, and evidence trees are for evidence, not test inputs. Prefer option 1; use only if a fixture is too large to inline. |
| **3 — Injectable-delegate seam in the gate (a `-CoverageReader` scriptblock parameter)** | Yes | Yes | Yes | Partly | Useful **in addition** to option 1 for testing the *file-absent* path (R3b) without touching the filesystem. Recommend as a complement, not a substitute. |
| **4 — Run the real C# suite and observe a real regression** | No (R5b: ±15-point run-to-run spread) | Yes | n/a | No | **Reject.** Non-deterministic, ~35 s to many minutes, and would make the acceptance evidence hostage to the very instability documented in R5b. |
| **5 — Mutate a source file to create a real regression, then revert** | No | Yes | n/a | No | **Reject.** Mutates the working tree; not repeatable in CI; not a unit test. |

### 10d. Recommended design

**Shape.** Author the gate as a **pure function** that takes coverage figures (or a coverage-XML
string) plus floors and returns a structured verdict, and a **thin wrapper** that reads the artifact
and calls it. Only the wrapper touches the filesystem. This mirrors the existing separation between
`Invoke-MSTestWithCoverage.Helpers.ps1` (pure) and `Invoke-MSTestWithCoverage.ps1` (I/O), which is
already exercised by the existing Pester file via `. $helperScriptPath` from a `BeforeAll` block.

**Input format decision.** Feed the gate the **Cobertura** the repository actually produces
(`coverage/coverage.cobertura.xml` from `Invoke-MSTestWithCoverage.ps1`), not the JaCoCo the hook
currently expects. Rationale: no committed producer exists for `artifacts/csharp/coverage.xml`
(R3b), so keeping the JaCoCo contract means AC4 depends on an uncommitted scratchpad converter.
Reading Cobertura removes a whole format-drift class. If the existing hook's JaCoCo path must be
retained for PowerShell/TS/Python, keep it — but the C# path should read the artifact the C#
toolchain emits.

**Test cases (each an `It`, each with an inline here-string fixture):**
1. Line rate above the floor and branch rate above the floor → verdict pass, exit 0.
2. Line rate **one basis point below** the floor → verdict fail, non-zero. *(boundary)*
3. Line rate exactly **at** the floor → pass. *(boundary; pins `>=` vs `>`)*
4. Branch rate below the floor, line rate above → verdict fail, non-zero.
5. **Artifact absent / unreadable → verdict fail, non-zero.** This is the case that closes the
   R3b evasion; use the option-3 injectable reader so no filesystem access is needed.
6. Malformed XML → verdict fail with a distinguishable reason (fail closed, never fail open).
7. **Authority-consistency test (AC3):** assert that the gate's floor constants equal the numbers
   stated in the authoritative document. Implement by parsing the authority document's threshold
   lines from a string the test supplies, or by a repo-scan helper that returns every
   coverage-threshold literal outside the authority; assert the set is empty except for the gate's
   own named constants. This makes AC3 mechanically provable.

**Evidence capture (AC4).** Run the Pester file and capture the transcript to
`docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/regression-testing/gate-negative-path.<ts>.md`
with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. The artifact must show, by name, the
below-threshold case producing the non-zero result — that specific line is the acceptance evidence
AC4 asks for.

**Canonical evidence locations for this feature** (per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, enforced by
`.claude/hooks/enforce-evidence-locations.ps1`):
`…/494/evidence/baseline/`, `…/494/evidence/qa-gates/`, `…/494/evidence/regression-testing/`.
No `artifacts/**` evidence path is permitted.

---

## Automation Feasibility

### Items that require no human interaction

| Work item | Automatable? | Note |
|---|---|---|
| R1 site inventory → AC10 enumeration | Yes | Mechanical |
| AC8 — make the hook internally consistent | Yes | Two constants plus prose, once the numbers are decided |
| AC4 — author the gate and its negative-path Pester test | Yes | Pattern already proven in-repo (R10) |
| AC9 — test location and determinism | Yes | With the AC9 restatement in R10b |
| AC7 — re-measure after #441/#457 | Yes | R9d; long-running but unattended |
| AC3 — authority rule and "cite, do not restate" | Yes, once the authority document is chosen | The *choice* is the human-gated part |
| AC6 — resolve `quality-tiers.yml` | **Decision-gated** | Author-vs-remove is a scope choice; execution of either is mechanical |
| AC1/AC2/AC5 — the numbers and the exclusion policy | **Decision-gated** | See below |

### The maintainer-ratification question

**`CLAUDE.md:303` states, verbatim: "**Authority**: This exemption must be ratified by the project
maintainer and is tracked in `feature/csharp-coverage-uplift`."** [VERIFIED]

This is not incidental. There is a **recorded precedent of the maintainer exercising exactly this
authority and refusing**: `.claude/agent-memory/task-researcher/project_qfc_item_controller_227_r2_denial.md`
records that on 2026-07-01 the maintainer **denied** ratification of a 103-member
`[ExcludeFromCodeCoverage]` boundary on issue #227 and directed seam redesign instead, with the
stated reason that a blanket per-method/per-partial exemption defeats the purpose of the testability
work. That memory explicitly generalises: *"This is a general precedent for any future
`[ExcludeFromCodeCoverage]` boundary submitted for ratification."*

There is also a recorded precedent of the maintainer setting the threshold policy directly:
the #178 governance-sync decision (R2, item 5) kept 80/90 and explicitly rejected 85/75 under the
directive "keep current policy, adapt mechanism."

**Assessment.** Two of this feature's decisions are maintainer decisions, not agent decisions:

1. **The governing numbers.** Changing them from 80/90 would reverse a recorded maintainer decision
   (#178). Keeping 80/90 and deleting 85/75 would reverse whatever later change reintroduced 85/75
   — a change this session could not identify without `git log` (R2, [UNVERIFIED]). An agent cannot
   adjudicate between two prior maintainer-adjacent decisions on its own authority. Additionally,
   R5c shows that **three of nine production assemblies fail an 80% bar and five of nine fail an 85%
   bar today**; whichever number is chosen, the maintainer is choosing which assemblies are declared
   failing on day one.

2. **Whether the COM/VSTO/WinForms exemption survives.** `CLAUDE.md` reserves this to the maintainer
   in its own text, and the #227 denial shows the reservation is live. AC2 requires reconciling that
   exemption against "no production file may be excluded" — which is, precisely, a
   ratify-or-revoke decision on the exemption.

The remaining acceptance criteria (AC3, AC4, AC6, AC7, AC8, AC9, AC10) are fully automatable once
those two answers exist.

### The minimal, precisely-scoped maintainer question

One question, four parts, each answerable in a sentence. Everything else follows mechanically.

> **Coverage policy reconciliation (issue #494) — four decisions needed.**
>
> The repository states two incompatible coverage policies. `CLAUDE.md` § UT2 says **>= 80% line
> repo-wide, >= 90% for new units**, applied to a *testable denominator* that excludes VSTO
> lifecycle, WinForms/Designer, and Outlook-Interop classes. `.claude/rules/general-unit-test.md`
> and `.claude/rules/quality-tiers.md` say **>= 85% line, >= 75% branch**, and forbid excluding any
> production file. Agent memory records that at issue #178 you kept 80/90 and explicitly rejected
> 85/75 as reference-repo leakage; the 85/75 documents are nonetheless present today, and the
> commit that reintroduced them has not been identified. No tooling enforces any repo-wide number,
> so neither policy has ever bound anything.
>
> **1. Numbers.** Which governs — 80 line / 90 new (no branch gate), or 85 line / 75 branch? Or a
> third set? Current measured state, under arithmetic that features #441/#457 will correct:
> repo-wide 85.8% line / 79.2% branch; per assembly — VBFunctions 100, TaskTree 95.5, Tags 92.7,
> TaskVisualization 89.8, UtilitiesCS 89.5, QuickFiler 80.8, TaskMaster 71.0, ToDoModel 57.3,
> SVGControl 47.3. **At 80%: three of nine assemblies fail. At 85%: five of nine fail.**
>
> **2. Exemption.** Does the COM/VSTO/WinForms testable-denominator exemption in `CLAUDE.md` § UT2
> survive as written, survive in narrowed form, or is it revoked in favour of the
> refactor-don't-exclude rule? (Related: on #227 you denied a blanket exemption boundary and
> directed seam redesign instead.)
>
> **3. Authority.** Confirm `CLAUDE.md` § UT2 as the single authoritative source for coverage
> thresholds, with every other document citing it and stating no number of its own. If you prefer a
> different authority document, name it.
>
> **4. Tier system.** `.claude/rules/quality-tiers.md` asserts a `quality-tiers.yml`, a
> `tier-classification` CI stage, and a `docs/ci.research.md` source of truth. **None of the three
> exists**, and the document's own text says coverage thresholds are uniform across tiers, so the
> classification would change no coverage gate. Author them, or delete the claims?

**Measurement caveat that should accompany the question.** Two runs of the same coverage command 26
hours apart produced denominators differing by 38.6% (79,957 vs 110,849 valid lines; 70.19% vs
85.65% line rate), and the #424 evidence attributes this to non-deterministic assembly
instrumentation — a defect that neither #441 nor #457 addresses. **Any repo-wide number ratified now
may not reproduce.** Recommend the maintainer's answer to part 1 distinguish the *change-scoped*
gates (changed-line no-regression, new-unit bar — reproducible and enforceable today) from the
*repo-wide* floor (not yet reproducible), and that the repo-wide floor be ratified as enforceable
only after the three-run spread from R9d is under a stated tolerance.

**Bottom line on autonomy.** This feature **cannot execute fully autonomously**. The four-part
question above is the minimal human interaction. Once answered, every remaining acceptance criterion
is mechanical. If a fully autonomous fallback is required, the only defensible one is: reconcile to
the numbers already recorded as the maintainer's decision (**80/90 with the exemption retained**,
per #178 and per `CLAUDE.md`'s own precedence position), name `CLAUDE.md` § UT2 authoritative, delete
the tier-file claims, and file the exemption-revocation question as a separate issue. That path
changes no maintainer decision and reverses only the unattributed drift — but it should be taken
only if the maintainer is unavailable, and the spec must record that it was taken for that reason.

---

## Findings the Spec Should Not Miss

1. **`.claude/agents/feature-review.md` contradicts itself internally** (85/75 at 112-114, 90/80 at
   127-128). It is a live agent definition and is absent from issue.md's inventory. Add to AC10.
2. **The `.agents/` bundle is a stale snapshot, not a mirror.** Three files state the opposite camp
   from their `.claude/` counterparts. It is the canonical Codex runtime surface per its own README.
3. **The single numeric gate is evadable by withholding its input**, and committed agent memory
   records this as accepted practice. AC4 must prove the artifact-absent path fails closed.
4. **The gate's C# input artifact has no committed producer** and is in a format no committed tool
   emits. Recommend re-pointing the C# path at Cobertura.
5. **The hook's branch check blocks unconditionally** while its line check only requires a FAIL
   token — an asymmetry not covered by AC8's wording. Add it.
6. **The measurement has a ±15-point run-to-run spread** that #441/#457 do not fix. This is the
   finding most likely to change how the spec frames the decision: it may make a repo-wide numeric
   floor unratifiable in this feature regardless of which camp wins.
7. **`docs/ci.research.md` does not exist**, so `quality-tiers.md` cites a missing source of truth
   in addition to a missing mapping file and a missing CI stage.
8. **The #424 precedent's stated premise was disproved by its own feature's later measurement**
   (70.19% vs 85.65%). This is the cleanest available justification for superseding rather than
   ratifying its repo-wide half.
9. **AC9's literal `tests/scripts/powershell/` path misreads a mirroring rule**; `tests/scripts/vscode/`
   already exists and is the natural home.
10. **The epic's stated 512 range `CLAUDE.md:381-401` over-reaches into the Tone Policy** (390-395).
    Flag to the epic orchestrator; not a #494 blocker.
