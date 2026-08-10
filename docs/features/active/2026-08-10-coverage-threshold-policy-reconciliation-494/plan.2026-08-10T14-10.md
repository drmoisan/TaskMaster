# Coverage Threshold Policy Reconciliation (Issue #494) — Atomic Implementation Plan

- **Issue:** #494 — https://github.com/drmoisan/TaskMaster/issues/494
- **Epic:** `build-ci-coverage-gate-fidelity` (wave 2; after #441/#478 and #457)
- **Work Mode:** `full-bug` (spec-driven)
- **Feature folder:** `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/`
- **Branch:** `bug/coverage-threshold-policy-reconciliation-494`
- **Workspace root:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-abe56d74550beb67c`
- **Baseline HEAD at planning time:** `edf3d34c` (recorded, not gated — see P0-T2)
- **Plan created:** 2026-08-10T14-10

## Conventions Used By Every Task

- `<FEATURE>` = `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494`
- `<TS>` = the execution-time ISO-8601 stamp `yyyy-MM-ddTHH-mm` captured when the artifact is written.
- Every evidence artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Artifacts that
  record a decision rather than a command record `Timestamp:`, `Inputs:`, `Determination:`.
- Evidence locations are non-overridable: baselines and the coverage re-measurement go to
  `<FEATURE>/evidence/baseline/`, the AC4 proof goes to `<FEATURE>/evidence/regression-testing/`,
  final QC goes to `<FEATURE>/evidence/qa-gates/`, decision records go to `<FEATURE>/evidence/other/`,
  issue mirrors go to `<FEATURE>/evidence/issue-updates/`. No `artifacts/**` evidence path is permitted.
- **Locator discipline (C1).** Features #441/#478/#457 modify `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
  and `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` before this plan runs. All locators in those
  files are symbol names: `Get-CoberturaCoverageSummary`, `Merge-CoberturaClassesByFilename`,
  `Get-CoberturaLineConditionCoverageParts`, `ConvertTo-KoverageCoberturaXml`, `Get-KoverageProjectAllowlist`.
  Governance-document line numbers in this plan are labelled **"as of `edf3d34c`"**; if the quoted anchor text
  is not at that line, re-locate by the quoted text and record the new line number in the task's evidence.
- **Authorized edit path list** (any file outside this list touched by this feature is a Blocking finding):
  `CLAUDE.md` (§ UT2 only), `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`,
  `.claude/rules/general-code-change.md` (the `quality-tiers.yml` sentence only),
  `.agents/skills/quality-tiers/SKILL.md`, `.claude/hooks/validate-feature-review-coverage.ps1`,
  `scripts/vscode/Invoke-CoverageThresholdGate.ps1` (new),
  `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1` (new),
  `tests/.claude/hooks/validate-feature-review-coverage.Tests.ps1` (new),
  `tests/fixtures/coverage/*.xml` (new), and everything under `<FEATURE>/`.
- **Out of bounds (owned by `csharp-toolchain-gate-fidelity-512`):** the `CLAUDE.md` C# toolchain command
  blocks (regions 181-208, 377-386, 397-402 as of `edf3d34c`), `.claude/rules/csharp.md`, and
  `.claude/skills/csharp-qa-gate/SKILL.md`.
- **Not a gate for this feature:** the `/p:Nullable=enable` type-check command (issue #522, defective; fixed by 512).

## Planner Note — One Divergence Between the Two Acceptance-Criteria Sources

`issue.md` AC6 was widened on 2026-08-10T16-10 to cover `.claude/rules/quality-tiers.md:9,20`,
`.claude/rules/general-code-change.md:29`, and `.agents/skills/quality-tiers/SKILL.md:27`. `spec.md`'s own
`## Acceptance Criteria` block still carries the **narrow** AC6 wording, and `spec.md` D6/D7 defer the latter
two sites to follow-up FU-A. Per the planning directive ("AC6 was widened; use the current text"), this plan
executes the **widened** AC6 and includes P5-T4, which reconciles `spec.md`'s AC6 text and D6/D7 dispositions
to match. This is recorded openly rather than resolved silently.

---

### Phase 0 — Baseline Capture, Policy Reads, and Blocking Provenance Gates

- [ ] [P0-T1] Read the policy documents in `policy-compliance-order` order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`) and write `<FEATURE>/evidence/baseline/phase0-instructions-read.<TS>.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read.
  - Acceptance: the artifact exists and lists all six files by path in the stated order.
- [ ] [P0-T2] Record the baseline git state — `git rev-parse HEAD` and `git status --porcelain` — into `<FEATURE>/evidence/baseline/git-state.<TS>.md`.
  - Acceptance: the artifact records the HEAD sha as an observation (no sha is asserted as an expectation elsewhere in this plan) and records the porcelain output verbatim.
- [ ] [P0-T3] Verify the upstream dependencies have landed by grepping `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` for `Get-CoberturaLineConditionCoverageParts` and `Merge-CoberturaClassesByFilename`, and `coverage.config` plus the helpers for the #457 nested-lambda exclusion handling; record the result in `<FEATURE>/evidence/baseline/upstream-dependency-check.<TS>.md`.
  - Acceptance: the artifact states, per symbol, present or absent. **Halt condition:** if `Get-CoberturaLineConditionCoverageParts` is absent, #441/#478 have not landed on this branch; stop and escalate rather than measuring under the defective arithmetic.
- [ ] [P0-T4] Execute the D1 execution-time provenance re-verification gate and write `<FEATURE>/evidence/baseline/d1-provenance-reverification.<TS>.md`, running exactly: `git log --follow --oneline -- .claude/rules/quality-tiers.md`; `git log --follow --oneline -- .claude/rules/general-unit-test.md`; `git log -L 23,24:.claude/rules/general-unit-test.md`; `git log -L 31,46:.claude/rules/general-unit-test.md`; `git log -L 292,306:CLAUDE.md`; `git show --stat 48e46387`; `git show --stat --format="" 48e46387 -- CLAUDE.md`.
  - Acceptance: the artifact records each command with its `EXIT_CODE:` and output, and compares each result against `<FEATURE>/evidence/other/threshold-provenance-verification.2026-08-10T16-10.md` sections E1-E7 (note: `48e46387` is dated **2026-07-05**, not 2026-08-05).
  - **Halt condition (blocking, D1):** if the history shows a commit that touched **both** `CLAUDE.md` and the 85/75 surfaces with a message adjudicating which governs — i.e. the 85/75 reintroduction was an explicit maintainer reconciliation that also adjudicated `CLAUDE.md` — then D1's premise is falsified. Write `HALT: D1 FALSIFIED` into the artifact, stop, and escalate. Do not apply any governance edit.
- [ ] [P0-T5] Verify that `CLAUDE.md` § UT2 is textually disjoint from every 512-owned region, by locating the line span of the anchor `### UT2. Coverage and Scenarios` through the line before `- **Scenario Completeness**`, and the line spans of the three C# toolchain command blocks (regions 181-208, 377-386, 397-402 as of `edf3d34c`, re-located by their quoted headings `2. **Linting / Static Analysis — .NET analyzers**`, `## C# Toolchain (run in this exact order)`, and `## Key Skills Reference`); record all spans in `<FEATURE>/evidence/baseline/claude-md-region-disjointness.<TS>.md`.
  - Acceptance: the artifact states the numeric gap between the § UT2 span and the nearest 512 region and asserts the spans do not overlap. **Halt condition:** if any span overlaps, stop and escalate a merge-coordination conflict with feature 512 rather than editing.
- [ ] [P0-T6] Record the then-current signatures and first five body lines of `Get-CoberturaCoverageSummary`, `Merge-CoberturaClassesByFilename`, `Get-CoberturaLineConditionCoverageParts`, `ConvertTo-KoverageCoberturaXml`, and `Get-KoverageProjectAllowlist` from `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` into `<FEATURE>/evidence/baseline/coverage-function-signatures.<TS>.md`.
  - Acceptance: all five symbols are recorded with their current line numbers; this is the toolchain-drift record required by spec D9.
- [ ] [P0-T7] Capture the pre-change coverage-numeral inventory by grepping the governance surface for `>= 8[05]%|>= 75%|>= 90%|85\.0|75\.0|80\.0` across `CLAUDE.md`, `AGENTS.md`, `.claude/rules/`, `.claude/skills/`, `.claude/agents/`, `.claude/hooks/`, `.github/instructions/`, `.agents/`, and `.codex/`, and write the full hit list to `<FEATURE>/evidence/baseline/coverage-numeral-inventory.<TS>.md`.
  - Acceptance: the artifact records path plus line plus text for every hit and a total count. This is the AC1/AC3 pre-state and the AC10 completeness input.
- [ ] [P0-T8] Record the current state of `.claude/hooks/validate-feature-review-coverage.ps1` — its `.SYNOPSIS` coverage sentence, the `85.0` literal, the `$BranchFloor = 75.0` literal, and the four artifact paths in `Get-LanguageRepoCoverage` and `Get-LanguageBranchCoverage` — into `<FEATURE>/evidence/baseline/hook-current-state.<TS>.md`.
  - Acceptance: all four items are quoted verbatim with their current line numbers, located by quoted text rather than by the line numbers stated in `spec.md`.
- [ ] [P0-T9] Run the baseline PowerShell format check `mcp__drm-copilot__run_poshqc_format` (workspace scope: the repository root) and write `<FEATURE>/evidence/baseline/ps-format.<TS>.md`.
  - Acceptance: artifact records `Command:`, `EXIT_CODE:`, and whether any file was rewritten.
- [ ] [P0-T10] Run the baseline PowerShell lint `mcp__drm-copilot__run_poshqc_analyze` and write `<FEATURE>/evidence/baseline/ps-analyze.<TS>.md`.
  - Acceptance: artifact records `EXIT_CODE:` and the pre-existing diagnostic count by severity.
- [ ] [P0-T11] Run the baseline PowerShell test suite with coverage `mcp__drm-copilot__run_poshqc_test` (config `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`) and write `<FEATURE>/evidence/baseline/ps-pester-coverage.<TS>.md`.
  - Acceptance: `Output Summary:` records numeric baseline PowerShell **line coverage %** and **branch coverage %**, the passed/failed/total test counts, and confirms the four pre-existing files under `tests/scripts/vscode/` all pass. If the MCP tool is unavailable in the execution session, this task is explicitly authorized to fall back to `pwsh -NoProfile -Command "Invoke-Pester -Path tests -CI -CodeCoverage @('scripts/**/*.ps1','.claude/hooks/*.ps1') -CodeCoverageOutputFile artifacts/pester/powershell-coverage.xml -CodeCoverageOutputFileFormat JaCoCo"` and must record which route was used. `EXIT_CODE: SKIPPED` is not a valid outcome.
- [ ] [P0-T12] Establish the coverage re-measurement preconditions and record them in `<FEATURE>/evidence/baseline/test-discovery-precondition.<TS>.md`: (a) no other agent worktree is executing tests (enumerate running `vstest.console`/`testhost`/`dotnet-coverage` processes), and (b) the discovery rule that every `*.Test.dll` path found under `scripts/vscode/Invoke-MSTestWithCoverage.ps1`'s default `-SearchRoot .` must begin with the workspace-root prefix and must contain no `\.claude\worktrees\` segment *after* that prefix.
  - Acceptance: the artifact lists the running-process check output and states the discovery assertion verbatim. **Halt condition:** if a concurrent test host is running, wait and re-check rather than proceeding.

---

### Phase 1 — Corrected-Arithmetic Coverage Re-Measurement

This phase must complete before any task in Phase 2. No figure produced here selects a threshold
(spec D9 reasoning point 3); the figures validate, contextualise, and identify failing assemblies.

- [ ] [P1-T1] Rebuild the solution so the analyzer/compile step actually runs: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`, and write `<FEATURE>/evidence/baseline/csharp-rebuild.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` and `Output Summary:` records warning/error counts. Do **not** add `/p:Nullable=enable`.
- [ ] [P1-T2] Execute coverage re-measurement run 1: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/remeasure-run1.cobertura.xml`, and write `<FEATURE>/evidence/baseline/coverage-remeasurement-run1.<TS>.md`.
  - Acceptance: `Output Summary:` records the discovered assembly **list and count**, the explicit `Total tests:` value (an `Unknown` value fails this task and requires a re-run with per-assembly isolation), and the root `line-rate`, `branch-rate`, `lines-valid`, `lines-covered` from `coverage/remeasure-run1.cobertura.xml`. The P0-T12 discovery assertion is re-checked against the recorded assembly list.
- [ ] [P1-T3] Execute coverage re-measurement run 2 against an unchanged working tree: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/remeasure-run2.cobertura.xml`, and write `<FEATURE>/evidence/baseline/coverage-remeasurement-run2.<TS>.md`.
  - Acceptance: same fields as P1-T2, plus a `git status --porcelain` capture proving no source, test, or project file changed between runs.
- [ ] [P1-T4] Execute coverage re-measurement run 3 against an unchanged working tree: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/remeasure-run3.cobertura.xml`, and write `<FEATURE>/evidence/baseline/coverage-remeasurement-run3.<TS>.md`.
  - Acceptance: same fields as P1-T3.
- [ ] [P1-T5] Compute the reproducibility spread from the three runs and evaluate it against the D5 tolerance, writing `<FEATURE>/evidence/baseline/coverage-remeasurement-spread.<TS>.md`.
  - Acceptance: the artifact records max, min, median and spread for repository-wide **line rate** and for **lines-valid**, then states `TOLERANCE MET` only if line-rate spread `<= 1.0` percentage point **and** lines-valid spread `<= 0.5%` of median lines-valid; otherwise `TOLERANCE NOT MET`, which sets the FU-B trigger consumed by P5-T2.
- [ ] [P1-T6] Produce the per-assembly coverage table from `coverage/remeasure-run3.cobertura.xml` for the nine production projects (`QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, `VBFunctions`) and write `<FEATURE>/evidence/baseline/coverage-per-assembly.<TS>.md`.
  - Acceptance: the artifact lists each project with line rate and branch rate, and explicitly enumerates which projects fall below 80% line and which fall below 75% branch.
- [ ] [P1-T7] Record the D1-versus-measurement reconciliation required by `spec.md` Risk 2 in `<FEATURE>/evidence/other/d1-measurement-reconciliation.<TS>.md`.
  - Acceptance: the artifact states the corrected repository-wide figure, states that D1's numbers are decided on governance provenance and take no measurement as input, and selects exactly one disposition: (a) figure below 80% — D1 stands, D5 reported-and-tracked applies, failing assemblies enumerated from P1-T6; (b) figure comfortably above 85% — D1 stands, observation recorded for maintainer review; (c) D1 gate falsified in P0-T4 — already halted. Under no disposition is a threshold number altered to match a measured figure.

---

### Phase 2 — Governance Document Reconciliation

All tasks in this phase are blocked until Phase 1 is complete (AC7 sequencing). Replacement text is given in
Appendix A, B and C at the end of this plan.

- [ ] [P2-T1] Re-locate the § UT2 block in `CLAUDE.md` by the quoted anchors `### UT2. Coverage and Scenarios` and `- **Scenario Completeness**` and record the current line span plus the verbatim pre-edit text of the `- **Comprehensive Coverage (within reason)**` bullet block in `<FEATURE>/evidence/other/claude-md-ut2-relocation.<TS>.md`.
  - Acceptance: the artifact records the span and states whether it still matches lines 292-306 as of `edf3d34c`.
- [ ] [P2-T2] Insert the coverage-authority declaration and conflict-resolution rule (Appendix A block A1) into `CLAUDE.md` § UT2, immediately after the `### UT2. Coverage and Scenarios` heading and before the `- **Comprehensive Coverage (within reason)**` bullet.
  - Acceptance: `CLAUDE.md` § UT2 contains the sentence "This section is authoritative for coverage policy" and the sentence "An agent encountering such a divergence resolves it by this rule and does **not** halt."
- [ ] [P2-T3] Replace the threshold bullets in `CLAUDE.md` § UT2 with Appendix A block A2, so the section states `>= 80%` repository-wide line coverage and `>= 90%` line coverage for new modules, classes, and methods.
  - Acceptance: the two numerals `>= 80%` and `>= 90%` each appear exactly once in the threshold bullets of `CLAUDE.md` § UT2 and the pre-existing "must not reduce coverage for the lines that were changed" bullet is retained.
- [ ] [P2-T4] Replace the COM/VSTO/WinForms exemption bullet in `CLAUDE.md` § UT2 with the single reconciled denominator-and-exclusion rule, Appendix A block A3.
  - Acceptance: `CLAUDE.md` § UT2 contains the mechanism-independent sentence "Production lines may leave the denominator **only** through one of the three categories above", retains the `[ExcludeFromCodeCoverage]` / `coverage.config` mechanisms, retains the maintainer-ratification authority clause, and retains the explicit "NOT exempt" sentence naming `ToDoLoader`, `IDList`, `KbdActions<>` and path/settings helpers.
- [ ] [P2-T5] Insert the branch-coverage disposition bullet (Appendix A block A4) into `CLAUDE.md` § UT2.
  - Acceptance: `CLAUDE.md` § UT2 states that branch coverage is measured and reported, that no branch floor is adopted, and that this is a recorded decision rather than an omission.
- [ ] [P2-T6] Insert the ratified gate-scope rule (Appendix A block A5) into `CLAUDE.md` § UT2, covering the change-scoped Blocking gates, the reported-and-tracked repository-wide floor, and the numeric reproducibility exit condition.
  - Acceptance: `CLAUDE.md` § UT2 contains the word "ratified", names issues #424 and #230, states `<= 1.0` percentage point and `<= 0.5%` of median `lines-valid`, and states the condition under which the repository-wide floor becomes Blocking.
- [ ] [P2-T7] Verify the untouched-region invariants by diffing `CLAUDE.md` against the P0 baseline and writing `<FEATURE>/evidence/other/claude-md-scope-check.<TS>.md`.
  - Acceptance: the artifact proves the `- **Scenario Completeness**` sub-block and all three 512-owned C# toolchain command blocks are byte-identical to their pre-edit text, and that `git diff -- CLAUDE.md` touches only lines inside the § UT2 span recorded in P2-T1.
- [ ] [P2-T8] Replace the `## Coverage Requirements` section of `.claude/rules/general-unit-test.md` (anchored on the heading and the bullet beginning "**Line coverage must remain >= 85%") with Appendix B block B1, which states no coverage numeral and cites `CLAUDE.md` § UT2.
  - Acceptance: `grep -n "85%\|75%\|80%\|90%" .claude/rules/general-unit-test.md` returns no hit inside the `## Coverage Requirements` section.
- [ ] [P2-T9] Remove the `## Coverage Exclusion Policy` section of `.claude/rules/general-unit-test.md` in full (from the heading through the "**Enforcement:**" sentence) and replace it with Appendix B block B2, a one-paragraph citation of `CLAUDE.md` § UT2.
  - Acceptance: the strings "No production file may be excluded from coverage measurement", `dist/**`, `node_modules/**`, `jest.config.cjs` and `src/test-support/**` no longer appear anywhere in `.claude/rules/general-unit-test.md`.
- [ ] [P2-T10] Verify that the `## Test Categories` tier references in `.claude/rules/general-unit-test.md` (the "tier-dependent obligations per `.claude/rules/quality-tiers.md`" and "required for all tiers (T1–T4)" lines) still cite content that survives Phase 2, and adjust the citation wording if it points at removed content; record the outcome in `<FEATURE>/evidence/other/general-unit-test-tier-refs.<TS>.md`.
  - Acceptance: every `.claude/rules/quality-tiers.md` citation in `.claude/rules/general-unit-test.md` resolves to a section that still exists after P2-T11 through P2-T14.
- [ ] [P2-T11] Remove the false artifact claims from `.claude/rules/quality-tiers.md`: delete the `docs/ci.research.md` source-of-truth citation and the `quality-tiers.yml` mapping-file claim from the opening paragraph (line 9 as of `edf3d34c`), and delete the entire `## Source of Truth` section (lines 18-21 as of `edf3d34c`), replacing the opening paragraph with Appendix C block C1.
  - Acceptance: `grep -n "quality-tiers.yml\|tier-classification\|ci.research" .claude/rules/quality-tiers.md` returns no hits.
- [ ] [P2-T12] Rewrite the four tier example bullets in `.claude/rules/quality-tiers.md` (lines 13-16 as of `edf3d34c`) with Appendix C block C2, naming only projects that exist in `TaskMaster.sln`.
  - Acceptance: the strings `TaskMaster.Domain`, `TaskMaster.Application`, `Office.js`, `Microsoft Graph`, and `No-COM architecture` no longer appear in `.claude/rules/quality-tiers.md`.
- [ ] [P2-T13] Replace the `Line coverage: >= 85%.` and `Branch coverage: >= 75%.` bullets in the `### Uniform across all tiers (T1–T4)` list of `.claude/rules/quality-tiers.md` with Appendix C block C3, a citation that states no numeral.
  - Acceptance: neither `>= 85%` nor `>= 75%` appears in the uniform-gate list of `.claude/rules/quality-tiers.md`.
- [ ] [P2-T14] Replace the `## Rationale (uniform coverage thresholds)` paragraph in `.claude/rules/quality-tiers.md` with Appendix C block C4, removing the `85%`/`75%` numerals while retaining the rationale.
  - Acceptance: `grep -n "85%\|75%" .claude/rules/quality-tiers.md` returns no hits anywhere in the file.
- [ ] [P2-T15] Remove the false `quality-tiers.yml` assertion from `.claude/rules/general-code-change.md` by deleting the sentence "Every project must be classified in `quality-tiers.yml` at repo root." (line 29 as of `edf3d34c`, re-located by quoted text) and retaining the preceding sentence that cites `.claude/rules/quality-tiers.md`.
  - Acceptance: `grep -n "quality-tiers.yml" .claude/rules/general-code-change.md` returns no hits. This satisfies the AC6 widening of 2026-08-10T16-10.
- [ ] [P2-T16] Remove the three false tier claims (`quality-tiers.yml`, `tier-classification`, `docs/ci.research.md`) from `.agents/skills/quality-tiers/SKILL.md`, re-locating each by quoted text, and leave the tier taxonomy itself intact.
  - Acceptance: `grep -n "quality-tiers.yml\|tier-classification\|ci.research" .agents/skills/quality-tiers/SKILL.md` returns no hits.
- [ ] [P2-T17] Sweep for T1-T4 references left dangling by P2-T11 through P2-T16 across `.claude/rules/`, `.claude/skills/`, `.agents/skills/` and `.github/instructions/`, and record an explicit disposition for each in `<FEATURE>/evidence/other/tier-reference-dispositions.<TS>.md`.
  - Acceptance: every hit is assigned exactly one of `no change needed` (taxonomy reference stating no numeral and asserting no missing file), `aligned here` (edited in this phase), or `deferred to FU-A`, and no hit points at content deleted by this phase. `.claude/rules/architecture-boundaries.md` and `.claude/rules/powershell.md` must each appear with a disposition.
- [ ] [P2-T18] Prove single-numeral authority across the three in-scope documents by re-running the P0-T7 grep restricted to `CLAUDE.md`, `.claude/rules/general-unit-test.md`, and `.claude/rules/quality-tiers.md`, and writing `<FEATURE>/evidence/other/authority-single-numeral-proof.<TS>.md`.
  - Acceptance: every coverage-threshold numeral hit in those three files lies inside the `CLAUDE.md` § UT2 span; the other two files return zero hits. This is the AC1 and AC3 mechanical proof.
- [ ] [P2-T19] Audit governance-edit scope by running `git diff --name-only` and comparing the result against the authorized edit path list in this plan's Conventions section, writing `<FEATURE>/evidence/other/governance-edit-scope-audit.<TS>.md`.
  - Acceptance: every changed path is on the authorized list. Any `.claude/rules/` or `.agents/` path not on the list, and any 512-owned path, is a Blocking finding that stops the phase.

---

### Phase 3 — Enforcement Tooling: Threshold Gate, Committed Producer, and Hook Reconciliation

- [ ] [P3-T1] Create `scripts/vscode/Invoke-CoverageThresholdGate.ps1` with `Set-StrictMode -Version Latest`, a `param()` block accepting `-CoveragePath`, `-Format` (`Cobertura` default), and `-ContentReader` (a `[scriptblock]` seam defaulting to a single-argument file reader), plus the pure function `Get-CoverageThresholdPolicy` returning `@{ LineFloorPercent = 80.0; NewCodeFloorPercent = 90.0; BranchGated = $false }` with a comment citing `CLAUDE.md` § UT2 as the authority.
  - Acceptance: the file exists, dot-sources cleanly, and `Get-CoverageThresholdPolicy` is the single place in the file where a threshold numeral is written.
- [ ] [P3-T2] Add the pure parser `Get-CoberturaCoverageRates` to `scripts/vscode/Invoke-CoverageThresholdGate.ps1`, taking `-XmlContent [string]` and returning `@{ Ok; LineRatePercent; BranchRatePercent; LinesValid; LinesCovered; Reason }`, computing the line rate from the root `lines-covered`/`lines-valid` attributes and falling back to the root `line-rate` attribute only when both are absent.
  - Acceptance: the function touches no filesystem path, returns `Ok = $false` with a distinguishable `Reason` for null, empty, non-XML, or zero-`lines-valid` input, and never throws.
- [ ] [P3-T3] Add the pure verdict function `Test-CoverageThresholdVerdict` to `scripts/vscode/Invoke-CoverageThresholdGate.ps1`, taking `-LineRatePercent [Nullable[double]]`, `-BranchRatePercent [Nullable[double]]`, `-ArtifactAvailable [bool]`, `-LineFloorPercent [double]` and returning `@{ Ok; ArtifactAvailable; LineRatePercent; BranchRatePercent; Reason }`.
  - Acceptance: `Ok` is `$false` whenever `ArtifactAvailable` is `$false` or `LineRatePercent` is `$null` (fail closed); `Ok` is `$true` when `LineRatePercent -ge $LineFloorPercent`; `BranchRatePercent` is carried into the result but never influences `Ok` (spec D3).
- [ ] [P3-T4] Add the pure function `Test-CoverageNumeralAuthority` to `scripts/vscode/Invoke-CoverageThresholdGate.ps1`, taking `-DocumentMap [hashtable]` of path-to-text and `-AuthoritySpanText [string]`, and returning the list of coverage-threshold numeral hits found outside the authority span.
  - Acceptance: the function performs no filesystem access, takes all text as parameters, and returns an empty list for a compliant document map.
- [ ] [P3-T5] Add the wrapper `Invoke-CoverageThresholdGate` plus the script exit tail to `scripts/vscode/Invoke-CoverageThresholdGate.ps1`: the wrapper resolves content through `-ContentReader`, calls `Get-CoberturaCoverageRates` then `Test-CoverageThresholdVerdict`, and the tail (guarded by the `$MyInvocation.InvocationName -eq '.'` dot-source check) writes the verdict and calls `exit 1` when `Ok` is `$false`, otherwise `exit 0`.
  - Acceptance: `scripts/vscode/Invoke-CoverageThresholdGate.ps1` is the only file that performs filesystem access for the gate, and the pure functions remain callable without it. Running the script with a `-CoveragePath` that does not exist returns exit code 1.
- [ ] [P3-T6] Add a `.NOTES` block to `scripts/vscode/Invoke-CoverageThresholdGate.ps1` naming the committed producer for each language artifact: C# — `scripts/vscode/Invoke-MSTestWithCoverage.ps1` emitting Cobertura to `coverage/coverage.cobertura.xml`; PowerShell — `mcp__drm-copilot__run_poshqc_test` emitting JaCoCo to `artifacts/pester/powershell-coverage.xml`; and stating that a gate must never depend on an artifact whose only producer is an uncommitted scratchpad tool.
  - Acceptance: the `.NOTES` block names a producer for every artifact path the gate or the hook reads.
- [ ] [P3-T7] Rewrite the coverage sentences in the `.SYNOPSIS`/`.DESCRIPTION` of `.claude/hooks/validate-feature-review-coverage.ps1` so the documented behaviour states the reconciled numbers: repository-wide line floor 80 percent, new-code line floor 90 percent, branch coverage reported and not gated, and coverage artifact absent or malformed treated as a failure.
  - Acceptance: the string "below 80 percent" is replaced by wording that states the same 80 floor the enforced constant uses, and the documentation block mentions no `85` and no `75`.
- [ ] [P3-T8] Dot-source the gate script from `.claude/hooks/validate-feature-review-coverage.ps1` using a `$PSScriptRoot`-relative path to `scripts/vscode/Invoke-CoverageThresholdGate.ps1` resolved through `Join-Path $PSScriptRoot '..\..\scripts\vscode\Invoke-CoverageThresholdGate.ps1'`.
  - Acceptance: the hook resolves the gate script without depending on the process working directory, and dot-sourcing does not execute the gate's exit tail.
- [ ] [P3-T9] Re-point the C# rows of `Get-LanguageRepoCoverage` and `Get-LanguageBranchCoverage` in `.claude/hooks/validate-feature-review-coverage.ps1` from `artifacts/csharp/coverage.xml` (JaCoCo, no committed producer) to `coverage/coverage.cobertura.xml` parsed by `Get-CoberturaCoverageRates`, leaving the TypeScript, Python and PowerShell rows unchanged.
  - Acceptance: `grep -n "artifacts/csharp/coverage.xml" .claude/hooks/validate-feature-review-coverage.ps1` returns no hits, and both C# rows read the Cobertura artifact produced by the committed runner.
- [ ] [P3-T10] Replace the hard-coded `85.0` literal in `Test-LanguageCoverageRow` in `.claude/hooks/validate-feature-review-coverage.ps1` with `(Get-CoverageThresholdPolicy).LineFloorPercent`, and update the associated message string so the reported floor is interpolated from the constant rather than written as a literal.
  - Acceptance: no numeric coverage literal remains in `Test-LanguageCoverageRow`, and the failure message reports the floor value from the constant.
- [ ] [P3-T11] Remove the branch-gate block from `Test-LanguageCoverageRow` in `.claude/hooks/validate-feature-review-coverage.ps1` — the `$BranchFloor = 75.0` assignment, the `$BranchPct -lt $BranchFloor` comparison, and its unconditional `Ok = $false` return — while retaining the `-BranchPct` parameter so branch coverage continues to be measured and reported.
  - Acceptance: `grep -n "BranchFloor\|75.0" .claude/hooks/validate-feature-review-coverage.ps1` returns no hits, and a below-75 branch figure no longer produces a failing row.
- [ ] [P3-T12] Make the artifact-absent path fail closed in `.claude/hooks/validate-feature-review-coverage.ps1` by routing each changed language through `Test-CoverageThresholdVerdict` and returning a failing row with a distinguishable reason when `ArtifactAvailable` is `$false`, replacing the current `$null -ne $RepoWidePct` guard that skips the numeric check entirely.
  - Acceptance: for a language with changed files and no coverage artifact, `Invoke-FeatureReviewCoverageValidation` returns `Ok = $false` with a reason naming the missing artifact path; the below-floor case continues to require a FAIL verdict on a coverage row per spec D5 (reported-and-tracked, not silently blocking).
- [ ] [P3-T13] Prove AC8 internal consistency by grepping `.claude/hooks/validate-feature-review-coverage.ps1` and `scripts/vscode/Invoke-CoverageThresholdGate.ps1` for coverage numerals and writing `<FEATURE>/evidence/other/hook-consistency-proof.<TS>.md`.
  - Acceptance: exactly one line-floor numeral exists across both files, it lives in `Get-CoverageThresholdPolicy`, it equals the number stated in `CLAUDE.md` § UT2, and the hook's documentation block states the same number.
- [ ] [P3-T14] Audit file sizes for `scripts/vscode/Invoke-CoverageThresholdGate.ps1` and `.claude/hooks/validate-feature-review-coverage.ps1` and record the line counts in `<FEATURE>/evidence/other/file-size-audit.<TS>.md`.
  - Acceptance: both files are under the 500-line repository limit. If the hook exceeds 500 lines after the edits, split the coverage-reading functions into the gate script rather than requesting a waiver.

---

### Phase 4 — Pester Tests and the Two-Case AC4 Negative-Path Proof

Tests create no temporary files. Inline here-string fixtures follow the pattern already proven at
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. The single on-disk fixture is committed.

- [ ] [P4-T1] Create the committed below-threshold fixture `tests/fixtures/coverage/below-threshold.cobertura.xml`, a minimal valid Cobertura document whose root attributes give a repository-wide line rate strictly below the D1 floor (for example `lines-valid="1000" lines-covered="700"`).
  - Acceptance: the file is committed, is valid XML, parses through `Get-CoberturaCoverageRates` to a line rate below 80.0, and is referenced by both P4-T11 and the Pester suite.
- [ ] [P4-T2] Create `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1` with a `BeforeAll` that dot-sources `scripts/vscode/Invoke-CoverageThresholdGate.ps1` by a `$PSScriptRoot`-relative path, and the first `It`: a line rate above the floor yields `Ok = $true`.
  - Acceptance: the test passes, uses an inline here-string Cobertura fixture, and touches no filesystem path other than dot-sourcing the script under test.
- [ ] [P4-T3] Add the boundary `It` to `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1` asserting that a line rate one basis point **below** the floor yields `Ok = $false` with a reason naming the floor.
  - Acceptance: the test passes and is the unit-level expression of AC4 Case A.
- [ ] [P4-T4] Add the boundary `It` to `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1` asserting that a line rate **exactly at** the floor yields `Ok = $true`, pinning `>=` against `>`.
  - Acceptance: the test passes and fails if the comparison operator is changed to `-gt`.
- [ ] [P4-T5] Add the `It` to `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1` asserting that an absent artifact — simulated by passing a `-ContentReader` scriptblock that returns `$null`, not by deleting a file — yields `ArtifactAvailable = $false` and `Ok = $false`.
  - Acceptance: the test passes, performs no filesystem access, and is the unit-level expression of AC4 Case B.
- [ ] [P4-T6] Add the `It` to `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1` asserting that malformed XML yields `Ok = $false` with a distinguishable reason and does not throw.
  - Acceptance: the test passes and the reason string is distinct from the absent-artifact reason and from the below-floor reason.
- [ ] [P4-T7] Add the `It` to `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1` asserting that a branch rate below 75 with a line rate above the floor yields `Ok = $true`, pinning spec D3 against silent reintroduction of a branch gate.
  - Acceptance: the test passes and fails if any branch comparison is added to `Test-CoverageThresholdVerdict`.
- [ ] [P4-T8] Add the AC3 authority-consistency `It` to `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1`, calling `Test-CoverageNumeralAuthority` over the committed text of `CLAUDE.md`, `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` and asserting the returned hit list is empty, and separately asserting that `(Get-CoverageThresholdPolicy).LineFloorPercent` equals the numeral stated in `CLAUDE.md` § UT2.
  - Acceptance: the test passes, reads only committed files, writes nothing, and fails if a coverage numeral is reintroduced into either rule file or if the gate constant drifts from the authority document.
- [ ] [P4-T9] Create `tests/.claude/hooks/validate-feature-review-coverage.Tests.ps1` covering the hook's reconciled behaviour: a changed-language row with an absent coverage artifact returns `Ok = $false` (fail closed), and a below-75 branch figure with an above-floor line figure no longer produces a failing row.
  - Acceptance: both tests pass, dot-source the hook by a `$PSScriptRoot`-relative path, use in-memory payload strings, and create no temporary files. This is the executing-path proof required by `spec.md` Risk 6.
- [ ] [P4-T10] Audit determinism and temporary-file prohibition across `tests/scripts/vscode/Invoke-CoverageThresholdGate.Tests.ps1` and `tests/.claude/hooks/validate-feature-review-coverage.Tests.ps1` by grepping for `New-TemporaryFile`, `New-Item`, `Out-File`, `Set-Content`, `$env:TEMP`, `Start-Sleep`, `Get-Date` and `Get-Random`, and write `<FEATURE>/evidence/other/test-determinism-audit.<TS>.md`.
  - Acceptance: zero hits for every pattern; this is the AC9 mechanical proof.
- [ ] [P4-T11] `[expect-fail]` Execute AC4 Case A — below-threshold input — by running `pwsh -NoProfile -File scripts/vscode/Invoke-CoverageThresholdGate.ps1 -CoveragePath tests/fixtures/coverage/below-threshold.cobertura.xml` and capturing the transcript to `<FEATURE>/evidence/regression-testing/ac4-negative-path-case-a.<TS>.md`.
  - Acceptance: `EXIT_CODE: 1` is recorded, the emitted reason names the measured line rate and the 80 floor, and `Output Summary:` labels the record "Case A — below-threshold input".
- [ ] [P4-T12] `[expect-fail]` Execute AC4 Case B — absent artifact — by running `pwsh -NoProfile -File scripts/vscode/Invoke-CoverageThresholdGate.ps1 -CoveragePath coverage/deliberately-absent.cobertura.xml` (a path that is not created and not deleted) and capturing the transcript to `<FEATURE>/evidence/regression-testing/ac4-negative-path-case-b.<TS>.md`.
  - Acceptance: `EXIT_CODE: 1` is recorded, the emitted reason names the missing artifact path, and `Output Summary:` labels the record "Case B — absent artifact". A zero exit code fails this task and means the gate is not fail-closed.
- [ ] [P4-T13] Consolidate the two-case AC4 proof into `<FEATURE>/evidence/regression-testing/ac4-negative-path-proof.<TS>.md`, citing both case artifacts by path.
  - Acceptance: the consolidated artifact records `Timestamp:`, `Command:` for each case, `EXIT_CODE:` for each case, and an `Output Summary:` that names Case A and Case B **individually**. A proof covering only Case A does not satisfy AC4.
- [ ] [P4-T14] Run the full PowerShell test suite via `mcp__drm-copilot__run_poshqc_test` and write `<FEATURE>/evidence/other/pester-suite-post-implementation.<TS>.md`.
  - Acceptance: all tests pass, including the four pre-existing files under `tests/scripts/vscode/` unchanged (Boundaries invariant 7), and `Output Summary:` records passed/failed/total counts plus line and branch coverage percentages.

---

### Phase 5 — Follow-Up Issues, Document Reconciliation, and Acceptance-Criteria Check-Off

- [ ] [P5-T1] File follow-up issue **FU-A** ("Convert all remaining coverage-numeral sites to citations of `CLAUDE.md` § UT2") through the MCP promotion lifecycle and mirror the result to `<FEATURE>/evidence/issue-updates/fu-a.<TS>.md`.
  - Acceptance: the mirror records the created issue number and URL, and the issue body explicitly names `.claude/agents/feature-review.md` (highest priority, self-contradicting), `.claude/rules/powershell.md`, `.claude/skills/powershell-qa-gate/SKILL.md`, `.claude/skills/feature-review-workflow/SKILL.md`, `.github/instructions/general-unit-test.instructions.md`, `AGENTS.md`, the three divergent `.agents/` files, `.codex/hooks/validate-feature-review-coverage.ps1`, and the `.claude/agent-memory/**` entries superseded on landing.
- [ ] [P5-T2] Record the **FU-B** determination from the P1-T5 spread result and, if `TOLERANCE NOT MET`, file FU-B ("Coverage measurement determinism") through the MCP promotion lifecycle; mirror either outcome to `<FEATURE>/evidence/issue-updates/fu-b.<TS>.md`.
  - Acceptance: the mirror records the measured line-rate and lines-valid spreads and states either the created issue number or an explicit `FU-B NOT REQUIRED` determination citing the tolerance result. An unrecorded determination fails this task.
- [ ] [P5-T3] File follow-up issue **FU-C** ("Remove or relocate `scripts/temp-extract-coverage.ps1`") through the MCP promotion lifecycle and mirror the result to `<FEATURE>/evidence/issue-updates/fu-c.<TS>.md`.
  - Acceptance: the mirror records the created issue number and URL; `scripts/temp-extract-coverage.ps1` itself is not modified by this feature.
- [ ] [P5-T4] Reconcile the AC6 divergence between the two acceptance-criteria sources by updating `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md`: replace the narrow AC6 text in the `## Acceptance Criteria` block with the widened `issue.md` wording, and update the D6 carve-out table and the D7 out-of-scope table so `.claude/rules/general-code-change.md:29` and `.agents/skills/quality-tiers/SKILL.md` read `aligned here` rather than `deferred to FU-A`.
  - Acceptance: the AC6 text in `spec.md` matches `issue.md` verbatim, and no row in the `spec.md` out-of-scope table defers a site this plan actually edited.
- [ ] [P5-T5] Append any newly discovered threshold-stating sites from the P0-T7 inventory to the `spec.md` out-of-scope disposition table with an explicit disposition each, in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md`.
  - Acceptance: every hit in `<FEATURE>/evidence/baseline/coverage-numeral-inventory.<TS>.md` appears in the table with exactly one of `aligned here`, `deferred to FU-A`, `non-normative`, or `no change needed`; `.codex/hooks/validate-feature-review-coverage.ps1` appears explicitly.
- [ ] [P5-T6] Check off **AC1** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing `<FEATURE>/evidence/other/authority-single-numeral-proof.<TS>.md`.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, with the evidence pointer recorded inline.
- [ ] [P5-T7] Check off **AC2** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing the P2-T4 and P2-T9 results recorded in `<FEATURE>/evidence/other/claude-md-scope-check.<TS>.md`.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, with the evidence pointer recorded inline.
- [ ] [P5-T8] Check off **AC3** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing `<FEATURE>/evidence/other/authority-single-numeral-proof.<TS>.md` and the P4-T8 authority-consistency test.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, with the evidence pointer recorded inline.
- [ ] [P5-T9] Check off **AC4** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing `<FEATURE>/evidence/regression-testing/ac4-negative-path-proof.<TS>.md`.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, and the cited proof names Case A and Case B individually.
- [ ] [P5-T10] Check off **AC5** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing the ratified gate-scope block written by P2-T6 in `CLAUDE.md`.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, with the evidence pointer recorded inline.
- [ ] [P5-T11] Check off **AC6** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing the P2-T11, P2-T15, P2-T16 greps and `<FEATURE>/evidence/other/tier-reference-dispositions.<TS>.md`.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, and the widened AC6 text is the text being checked off.
- [ ] [P5-T12] Check off **AC7** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing `<FEATURE>/evidence/baseline/coverage-remeasurement-spread.<TS>.md` and `<FEATURE>/evidence/other/d1-measurement-reconciliation.<TS>.md`.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, and the cited evidence pre-dates every Phase 2 governance edit.
- [ ] [P5-T13] Check off **AC8** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing `<FEATURE>/evidence/other/hook-consistency-proof.<TS>.md`.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, with the evidence pointer recorded inline.
- [ ] [P5-T14] Check off **AC9** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing `<FEATURE>/evidence/other/test-determinism-audit.<TS>.md` and recording that the mirrored test paths follow the `spec.md` Test Strategy restatement.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, and the restatement rationale is cited rather than silently deviated from.
- [ ] [P5-T15] Check off **AC10** in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, citing the completed disposition table and the FU-A issue number from `<FEATURE>/evidence/issue-updates/fu-a.<TS>.md`.
  - Acceptance: exactly one acceptance-criterion checkbox is flipped by this task, in both files, and FU-A exists as a real issue rather than as prose.
- [ ] [P5-T16] Write the acceptance-criteria status summary to `<FEATURE>/evidence/issue-updates/ac-status-summary.<TS>.md`, listing AC1-AC10 with status and evidence path.
  - Acceptance: all ten criteria are listed, each with a `[x]` status and a resolvable evidence path.

---

### Phase 6 — Final QA Toolchain Loop and Close-Out

The loop is format then lint then test, restarted from the top if any step fails or rewrites a file.
No task in this phase may complete with `EXIT_CODE: SKIPPED`.

- [ ] [P6-T1] Run the final PowerShell format stage `mcp__drm-copilot__run_poshqc_format` and write `<FEATURE>/evidence/qa-gates/final-ps-format.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`; if any file was rewritten, restart the loop at this task after committing the rewrite.
- [ ] [P6-T2] Run the final PowerShell lint stage `mcp__drm-copilot__run_poshqc_analyze` and write `<FEATURE>/evidence/qa-gates/final-ps-analyze.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0` with no new diagnostics relative to `<FEATURE>/evidence/baseline/ps-analyze.<TS>.md`.
- [ ] [P6-T3] Record the type-check stage disposition in `<FEATURE>/evidence/qa-gates/final-typecheck-disposition.<TS>.md`: PowerShell has no type-check stage per `.claude/rules/powershell.md` step 3, and no C# source file changed in this feature.
  - Acceptance: the artifact records `git diff --name-only` filtered to `*.cs`, `*.csproj`, `*.props`, `*.targets` and shows an empty result; if that result is non-empty, the full C# toolchain (format, analyze, rebuild) must be run and recorded here, excluding `/p:Nullable=enable`.
- [ ] [P6-T4] Run the final PowerShell test stage with coverage `mcp__drm-copilot__run_poshqc_test` and write `<FEATURE>/evidence/qa-gates/final-ps-pester-coverage.<TS>.md`.
  - Acceptance: `EXIT_CODE: 0`, all tests pass, and `Output Summary:` records post-change **line coverage %** and **branch coverage %** plus passed/failed/total counts. The JaCoCo artifact `artifacts/pester/powershell-coverage.xml` exists after the run.
- [ ] [P6-T5] Compute and record the coverage delta and new-code threshold check in `<FEATURE>/evidence/qa-gates/coverage-delta.<TS>.md`, comparing the P0-T11 baseline against the P6-T4 post-change figures and computing coverage for the newly added files `scripts/vscode/Invoke-CoverageThresholdGate.ps1` and the changed lines of `.claude/hooks/validate-feature-review-coverage.ps1`.
  - Acceptance: the artifact reports baseline coverage, post-change coverage, and new/changed-code coverage numerically; new-code line coverage is `>= 90%`; changed lines show no coverage regression. A shortfall is a remediation-required outcome, not a PASS.
- [ ] [P6-T6] Verify the gate's executing path end to end by running `pwsh -NoProfile -File scripts/vscode/Invoke-CoverageThresholdGate.ps1 -CoveragePath coverage/remeasure-run3.cobertura.xml` and recording the result in `<FEATURE>/evidence/qa-gates/gate-executing-path.<TS>.md`.
  - Acceptance: the run produces a verdict against the real re-measured artifact and records `EXIT_CODE:` plus the measured line rate; the artifact also names `.claude/hooks/validate-feature-review-coverage.ps1` as the hook-level executing path proven by P4-T9.
- [ ] [P6-T7] Re-run the AC1/AC3 authority grep across `CLAUDE.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/general-code-change.md`, and `.agents/skills/quality-tiers/SKILL.md` after all edits, and write `<FEATURE>/evidence/qa-gates/final-authority-grep.<TS>.md`.
  - Acceptance: coverage-threshold numerals appear only inside the `CLAUDE.md` § UT2 span and inside `Get-CoverageThresholdPolicy`; `quality-tiers.yml`, `tier-classification`, and `ci.research` return zero hits across all five paths.
- [ ] [P6-T8] Audit file sizes for every file added or modified by this feature and write `<FEATURE>/evidence/qa-gates/final-file-size-audit.<TS>.md`, running the audit **after** the P6-T1 format stage.
  - Acceptance: every `.ps1` file added or modified is under 500 lines; Markdown documentation files are exempt per `.claude/rules/general-code-change.md`.
- [ ] [P6-T9] Commit all source, test, governance and evidence changes on `bug/coverage-threshold-policy-reconciliation-494` with a message referencing issue #494, and record the commit sha in `<FEATURE>/evidence/qa-gates/final-commit.<TS>.md`.
  - Acceptance: `git status --porcelain` is empty after the commit; the evidence artifacts under `<FEATURE>/evidence/` are included in the commit.
- [ ] [P6-T10] Run the final scope-lock diff audit `git diff --name-only <baseline-sha>..HEAD` (using the sha recorded by P0-T2) against the authorized edit path list, writing `<FEATURE>/evidence/qa-gates/final-scope-lock.<TS>.md`, and treat `docs/**` and `.claude/agent-memory/**` as permitted incidental paths.
  - Acceptance: every changed path is on the authorized edit path list or is a permitted incidental path; zero 512-owned paths appear; zero `artifacts/**` evidence paths appear.
- [ ] [P6-T11] Write the informational maintainer note to `<FEATURE>/evidence/other/maintainer-note.<TS>.md` recording that 80/90 was restored, the COM/VSTO/WinForms exemption was retained, branch coverage is reported and not gated, `CLAUDE.md` § UT2 is now the single authority, and the tier claims were removed.
  - Acceptance: the note is explicitly labelled informational and non-blocking per `spec.md` § Automation Feasibility, and states which of the four blocking-escalation conditions were tested and found not to hold.
- [ ] [P6-T12] Reconcile the final acceptance state by verifying every AC1-AC10 checkbox is `[x]` in both `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` and `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/issue.md`, and record the reconciliation in `<FEATURE>/evidence/qa-gates/final-ac-reconciliation.<TS>.md`.
  - Acceptance: twenty checkboxes total (ten per file) are `[x]`; any unchecked box is a remediation-required outcome and the feature is not reported complete.

---

## Appendix A — `CLAUDE.md` § UT2 Replacement Text

The blocks below replace the `- **Comprehensive Coverage (within reason)**` bullet block (lines 294-306 as of
`edf3d34c`). The `- **Scenario Completeness**` sub-block is not touched.

### Block A1 — Authority declaration and conflict-resolution rule (P2-T2)

```
- **Coverage authority (this section governs).**
  - Coverage thresholds, the coverage denominator, and the coverage exclusion/exemption policy are
    defined in this section (`CLAUDE.md` § UT2) and nowhere else. This section is authoritative for
    coverage policy and takes precedence over any other document in this repository, including files
    under `.claude/rules/`, `.claude/skills/`, `.claude/agents/`, `.claude/hooks/`,
    `.github/instructions/`, `.agents/`, `.codex/`, and `AGENTS.md`.
  - Other documents may cite this section; they must not restate a coverage numeral. If another
    document states a coverage numeral that differs from this section, this section governs, the other
    document is defective, and the divergence must be filed as an issue. An agent encountering such a
    divergence resolves it by this rule and does **not** halt.
```

### Block A2 — Thresholds (P2-T3)

```
- **Comprehensive Coverage (within reason)**
  - Aim to exercise critical paths and important edge conditions.
  - Configure coverage tooling to exclude test files (e.g., `tests/`), so metrics reflect the
    application code, not the tests themselves.
  - Repository-wide line coverage must remain `>= 80%`.
  - Any new modules, classes, or methods added must target `>= 90%` line coverage.
  - Code changes or refactors must not reduce coverage for the lines that were changed.
  - Coverage is a supporting metric, not the sole quality gate; untested critical behavior is not
    acceptable even if the overall percentage looks good.
```

### Block A3 — Reconciled denominator and exclusion rule (P2-T4)

```
  - **Coverage denominator and exclusion policy (stated once, here).** Coverage is measured against the
    **testable denominator** — all first-party production code except:
    - (a) VSTO add-in lifecycle classes (entry points, ribbon event handlers, COM utility registration)
      that cannot be unit-tested without a live Outlook process;
    - (b) WinForms form-derived classes and Designer-generated code;
    - (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`,
      `ToDoModel`, and `Tags` that directly depend on `Microsoft.Office.Interop.Outlook.Application`,
      `MailItem`, `Store`, or `MAPIFolder` without an injectable seam.

    Production lines may leave the denominator **only** through one of the three categories above, and
    only by a mechanism recorded in the repository — an `[ExcludeFromCodeCoverage]` attribute visible in
    a pull-request diff, or a `coverage.config` / runsettings exclusion. Removing production lines from
    the denominator by any other means, or for any category not enumerated here, is a **Blocking**
    finding. Testable seams within otherwise-COM-bound assemblies (e.g., `ToDoLoader`, `IDList`
    arithmetic, `KbdActions<>`, path/settings helpers) are explicitly **NOT** exempt and must meet the
    `>= 80%` floor. **Authority**: this exemption is ratified by the project maintainer and is tracked in
    `feature/csharp-coverage-uplift`; widening the enumerated categories requires maintainer ratification.
```

### Block A4 — Branch-coverage disposition (P2-T5)

```
  - **Branch coverage is measured and reported, not gated.** Branch coverage must be reported in
    coverage evidence artifacts and in feature-review policy audits. No branch-coverage floor is
    adopted, and branch coverage must not be used as a blocking gate. This is a recorded decision, not
    an omission.
```

### Block A5 — Ratified gate scope (P2-T6)

```
  - **Gate scope — ratification of the #424 / #230 precedent.** The precedent improvised in issues #424
    and #230 is **ratified** as written policy, split by scope:
    - **Change-scoped gates are Blocking, unconditionally.** (i) No coverage regression on changed
      lines, measured against a baseline captured in the same change. (ii) New or changed modules,
      classes, and methods meet the `>= 90%` line bar.
    - **The repository-wide `>= 80%` floor is measured, reported, and tracked, and becomes Blocking only
      when measurement reproducibility is demonstrated.** Until that condition is met, a
      repository-wide figure below `80%` is a reported finding that must appear in the policy audit and
      is not on its own a Blocking finding.
    - **Reproducibility exit condition.** The repository-wide floor becomes a Blocking gate when, and
      only when, three consecutive full-suite coverage runs against an unchanged working tree, using the
      same command form and toolchain versions, show a repository-wide line-rate spread (maximum minus
      minimum) of `<= 1.0` percentage point **and** a `lines-valid` spread of `<= 0.5%` of the median
      `lines-valid`. Both conditions must hold, and the evidence must be captured with the change that
      flips the gate.
```

## Appendix B — `.claude/rules/general-unit-test.md` Replacement Text

### Block B1 — replaces `## Coverage Requirements` (P2-T8)

```
## Coverage Requirements

Coverage thresholds, the coverage denominator, and the coverage exclusion/exemption policy are defined
in `CLAUDE.md` § UT2 ("Coverage and Scenarios"), which is the single authority for coverage policy in
this repository. This file states no coverage numeral. See `CLAUDE.md` § UT2 for the repository-wide
line floor, the new-code line floor, the branch-coverage disposition, the testable-denominator
definition, the enumerated exemption categories, and the change-scoped versus repository-wide gate scope.

- Code changes or refactors must not reduce coverage for the lines that were changed.
- Coverage is a supporting metric, not the sole quality gate. Untested critical behavior is not
  acceptable even if the overall percentage looks good.
- Configure coverage tooling to exclude test files (e.g., `tests/`) so metrics reflect application code,
  not tests.
- Type-only / interface-only modules with no executable behavior may be omitted from coverage
  measurement. Examples: TypeScript interface/type-only files and C# interface-only files. Such modules
  legitimately report 0% executable coverage. This is a clarification only; it lowers no threshold.
```

### Block B2 — replaces `## Coverage Exclusion Policy` (P2-T9)

```
## Coverage Exclusion Policy

The rule governing which production lines may leave the coverage denominator, and by which recorded
mechanisms, is stated once in `CLAUDE.md` § UT2 and is not restated here. Feature-review agents apply
that rule directly: removing production lines from the denominator outside the categories and mechanisms
`CLAUDE.md` § UT2 enumerates is a **Blocking** finding.
```

## Appendix C — `.claude/rules/quality-tiers.md` Replacement Text

### Block C1 — replaces the opening paragraph and deletes `## Source of Truth` (P2-T11)

```
This rule defines the T1–T4 module rigor taxonomy used to describe module criticality in this
repository. The taxonomy is descriptive. There is no `quality-tiers.yml` mapping file, no
`tier-classification` CI stage, and no `docs/ci.research.md`; nothing in this file asserts that any of
them exists, and none is required for the gates below.
```

### Block C2 — replaces the four tier example bullets (P2-T12)

```
- **T1 — Critical.** Behavior bugs cause silent data loss or misclassification. Examples: the
  classifier and scoring seams in `UtilitiesCS`, ToDo ID allocation and hierarchy arithmetic in
  `ToDoModel`, and settings/serialization helpers whose corruption is silent.
- **T2 — Core.** Bugs cause feature regressions but not data loss. Examples: `ToDoModel`, `Tags`,
  `TaskTree`, and the non-COM helper surface of `UtilitiesCS`.
- **T3 — Adapters & UI.** Glue around APIs the team does not own. Examples: `QuickFiler`,
  `TaskVisualization`, `SVGControl`, and the Outlook Interop wrappers in `TaskMaster`.
- **T4 — Scaffolding.** Examples: VSTO bootstrap and ribbon wiring, `scripts/vscode/` build and test
  tooling, `.claude/hooks/` governance hooks, and generated designer code.
```

### Block C3 — replaces the two coverage bullets in the uniform-gate list (P2-T13)

```
- Line and branch coverage: see `CLAUDE.md` § UT2, the single authority for coverage policy. This file
  states no coverage numeral.
- No regression on changed lines.
```

### Block C4 — replaces `## Rationale (uniform coverage thresholds)` (P2-T14)

```
## Rationale (uniform coverage thresholds)

High test coverage is a fundamental quality-control design choice that enables autonomous agentic
development and trust in the work product. For that reason the coverage thresholds defined in
`CLAUDE.md` § UT2 apply uniformly across T1–T4; tier-specific lower coverage floors are not used in this
repository. The numeric values live in `CLAUDE.md` § UT2 and are not repeated here.
```

## Traceability — Acceptance Criteria to Tasks

| AC | Implementing tasks | Verifying tasks | Check-off |
|---|---|---|---|
| AC1 | P2-T3, P2-T8, P2-T13, P2-T14 | P2-T18, P6-T7 | P5-T6 |
| AC2 | P2-T4, P2-T9 | P2-T7, P6-T7 | P5-T7 |
| AC3 | P2-T2, P2-T8, P2-T13, P3-T4 | P2-T18, P4-T8, P6-T7 | P5-T8 |
| AC4 | P3-T1 through P3-T6, P3-T12, P4-T1 | P4-T3, P4-T5, P4-T11, P4-T12, P4-T13, P6-T6 | P5-T9 |
| AC5 | P2-T6 | P2-T7 | P5-T10 |
| AC6 | P2-T11, P2-T12, P2-T15, P2-T16 | P2-T17, P6-T7 | P5-T11 |
| AC7 | P1-T1 through P1-T7 (all before Phase 2) | P1-T5, P1-T7 | P5-T12 |
| AC8 | P3-T7, P3-T10, P3-T11 | P3-T13, P4-T9 | P5-T13 |
| AC9 | P4-T2 through P4-T9 | P4-T10, P4-T14 | P5-T14 |
| AC10 | P5-T4, P5-T5 | P5-T1, P0-T7 | P5-T15 |
