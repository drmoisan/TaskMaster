# 2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root (Spec)

- **Issue:** #752
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-03T00-00
- **Status:** Draft
- **Version:** 0.1

## Context
- Summary: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`'s test-assembly discovery predicate (`Invoke-MSTestWithCoverageMain`, line 301) excludes any candidate assembly whose absolute `FullName` contains a `\.claude\` path segment. The filter was added by item #733 (PR #748) to exclude sibling agent worktrees nested beneath the search root when the script runs from the main checkout. When the checkout the script itself runs FROM is located under `.claude/worktrees/` — the normal case for every parallel-run/epic-run child worktree — every candidate assembly path also contains that segment, so the discovered set is always empty and the script throws a misleading "No test assemblies found ... Build first." error, misattributing the cause to a missing build.
- Observed environment(s): Windows 11 Pro 10.0.26200, PowerShell 7, any agent worktree under `.claude/worktrees/agent-<id>/`.
- Customer impact and severity: affects every C# item executed by the parallel and epic orchestration surfaces when they run the coverage wrapper script in-place; CI is unaffected because CI runner checkouts never contain a `.claude` path segment.
- First observed date and version(s) impacted: introduced 2026-09-02 by item #733 / PR #748; reported 2026-09-03 as issue #752.

## Repro & Evidence
- Steps to reproduce: (1) create an agent worktree under `.claude/worktrees/` and build the solution there so `*.Test.dll` assemblies exist under `bin\Debug\`; (2) from that worktree, run `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1`; (3) observe the script throw `No test assemblies found under '<worktree>' for configuration 'Debug'. Build first.` even though the assemblies exist.
- Expected vs actual behavior: expected — assembly discovery excludes only sibling agent worktrees nested beneath the search root (the original intent of the #733 filter) and finds the test assemblies of the checkout the script is running from, regardless of where that checkout lives on disk. Actual — the discovery set is always empty when the search root itself is under `.claude\worktrees\`.
- Logs/error snippets: `No test assemblies found under '<user-profile>\repos\TaskMaster\.claude\worktrees\agent-...' for configuration 'Debug'. Build first.` (reported by the item #735 child orchestration during parallel run `bugs-2026-09-02`; recorded in `artifacts/orchestration/parallel-orchestrator-state.json` under `environment_regression_from_item_733`).
- Frequency / determinism: always, deterministic — every coverage-wrapper run from a checkout under `.claude/worktrees/` fails identically.

## Scope & Non-Goals
- In scope: the single discovery predicate at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` inside `Invoke-MSTestWithCoverageMain`; new/updated Pester regression coverage for the self-exclusion and nested-sibling-exclusion behaviors under `tests/scripts/vscode/`.
- Out of scope / non-goals:
  - `scripts/vscode/Invoke-MSTest.ps1`'s `Get-MSTestAssemblyPathList` discovery pipeline, which carries no `.claude` clause at all today. Research (§1.4, §2 of `research/research-findings.2026-09-03T00-00.md`) confirms this is a documented, deliberately out-of-scope gap already noted by item #733's own research and is not the same defect (there is no over-broad predicate there to correct). Extending discovery-filter parity to that script is a separate scope decision, not part of this fix.
  - `Invoke-MSTestWithCoverage.Helpers.ps1`'s `Get-KoverageProjectAllowlist` project-file filter, which also has no `.claude` clause and is a distinct, unreported concern outside this issue's scope.
  - Any coverage threshold, CI gate wiring, or Cobertura merge-logic change. This item is a discovery-filter correctness fix only.
- Explicitly excluded systems, integrations, or datasets: none beyond the above; no production TaskMaster C#/PowerShell runtime behavior outside the two named scripts is touched.

## Root Cause Analysis
- Confirmed root cause: the `-notmatch '\\\.claude\\'` clause at line 301 tests the candidate's absolute `FullName`. `Get-ChildItem -Path $resolvedSearchRoot -Recurse` guarantees every candidate path is prefixed by `$resolvedSearchRoot`, so when `$resolvedSearchRoot` itself contains a `.claude\worktrees\agent-<id>\` segment, that segment is present in every candidate's absolute path and the exclusion always fires, regardless of where the assembly sits relative to the search root.
- Signals/evidence supporting it: independently re-derived against the current worktree — see `research/research-findings.2026-09-03T00-00.md` §1.1–§1.3. The sibling clauses in the same `Where-Object` (`\bin\<Configuration>\`, `\obj\`, `\ref\`) are unaffected because they match segments that only ever appear inside a project's own build-output tree, never as a prefix contributed by the search root itself.
- Affected components/modules: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (`Invoke-MSTestWithCoverageMain`) only. Repo-wide search (research §2) found no sibling predicate of the same class anywhere else in the repository; the fix is confined to this one line.

## Proposed Fix

### Design summary (what changes where):
Replace the absolute-path match at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` with a match against the candidate path computed **relative to** `$resolvedSearchRoot` (already in scope at line 272), using `[System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)`. This is an in-place predicate edit (research Approach A) — no extraction into a separate function is required for correctness or testability, since `Invoke-MSTestWithCoverageMain` is already a directly callable, fully mockable function and the existing Pester harness already exercises this exact discovery block end-to-end.

### Boundaries and invariants to preserve:
- Preserve the existing `\bin\<Configuration>\`, `\obj\`, `\ref\` clauses unchanged; only the `.claude` clause's match target changes from `$_.FullName` to the `GetRelativePath`-derived relative path.
- Preserve the existing `@(...)`-wrapped assignment shape at lines 296/303 (array-safety under `Set-StrictMode`); do not remove it.
- Preserve the existing regression test `'excludes assemblies discovered under a .claude worktree segment'` (`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:416-442`) unmodified — it documents the "exclude a nested sibling worktree beneath a non-`.claude` search root" behavior, which must continue to pass unchanged under the relative-path fix.
- Do not touch `scripts/vscode/Invoke-MSTest.ps1` or `Invoke-MSTestWithCoverage.Helpers.ps1` — research confirmed neither carries the same defect (see Scope & Non-Goals).

### Dependencies or blocked work:
None. This is a single-line predicate fix with additive test coverage; no other open item blocks or is blocked by this change.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — `Invoke-MSTestWithCoverageMain`'s assembly-discovery `Where-Object` predicate (line 301).
- A new Pester test file, `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`, mirroring the existing `Invoke-MSTest.AssemblyDiscovery.Tests.ps1` naming convention. `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` is already at 488 of the repository's 500-line file cap (research §3.4), so the new regression cases land in a new sibling file rather than growing that one further.

#### Functions/classes/CLI commands impacted:
`Invoke-MSTestWithCoverageMain` (predicate logic only; function signature unchanged).

#### Data flow and validation changes:
None. The shape of `$testAssemblies` (an array of absolute path strings) is unchanged; only which paths are admitted to it changes.

#### Error handling and logging updates:
None required. The existing "No test assemblies found ... Build first." error at line 306 is unchanged in wording; it will simply no longer fire for the previously-misclassified self-exclusion case.

#### Rollback/feature-flag considerations (if applicable):
Not applicable. Internal developer-tooling script with no runtime feature-flag surface; rollback is a standard git revert if needed.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
No new interface. `Invoke-MSTestWithCoverageMain`'s parameters and return/side-effect behavior (coverage collection invocation) are unchanged; only the internal filter target changes.

#### Required configuration keys and defaults:
None introduced.

#### Backward-compatibility expectations:
Strictly widens the discovered-assembly set to include the previously wrongly-excluded self-root case while preserving the previously-excluded nested-sibling-worktree case. No consumer of `$testAssemblies` observes a shape change.

#### Performance constraints (latency/throughput/memory):
Not applicable; `GetRelativePath` is a pure, deterministic, O(1)-per-call static method with no I/O.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): PowerShell 7+ is the execution runtime (per `.claude/rules/powershell.md`), so `[System.IO.Path]::GetRelativePath` is available and normalizes both arguments via `GetFullPath` internally, tolerating `$resolvedSearchRoot`'s existing trailing `\.` shape (research §1.3) without needing to strip it first.
- Constraints (budget, performance, compatibility): confined to `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and `tests/scripts/vscode/`; no coverage threshold, CI gate, or file outside these two trees may be touched (per Scope & Non-Goals).
- External dependencies (services, libraries, releases): none.

## Data / API / Config Impact
- User-facing or API changes: none; internal developer-tooling script only.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy
- Regression tests to add: a new `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`, using the same dot-source/AST-parse/mock pattern already established in `Invoke-MSTest.RunSettings.Tests.ps1` (`BeforeEach` mocking `Resolve-Path`, `Test-Path`, `Resolve-RunSettingsPath`, `Invoke-VsWhereExe`, `Get-Command`, `Get-ChildItem`, `Invoke-DotnetCoverageCollection`, `Get-Content`, `ConvertTo-KoverageCoberturaXml`, `Set-Content`), exercising `Invoke-MSTestWithCoverageMain` directly with in-memory `[pscustomobject]` fixtures for `Get-ChildItem`. At minimum:
  - a case where `Resolve-Path` is mocked to return a `.claude\worktrees\agent-<id>\` search root and `Get-ChildItem` returns an assembly directly beneath it — the assembly must be included (self-exclusion fix, new case).
  - a case preserving the existing nested-sibling-worktree exclusion under a non-`.claude` search root, added here as an additional case for regression symmetry alongside the untouched original at `Invoke-MSTest.RunSettings.Tests.ps1:416-442`.
- Unit tests (pytest): not applicable — this repository has no Python toolchain for this area; all tests are Pester v5.x under `tests/scripts/vscode/`.
- Edge cases and negative scenarios: a search root under `.claude\worktrees\agent-N\` that itself contains a further-nested sibling worktree beneath it — the nested one must still be excluded while root-level assemblies are retained (flagged by research §7 as the most rigorous edge case; include if practicable without exceeding the new file's own line budget).
- Error handling and logging verification: none required — no new error path is introduced.
- Coverage impact and targets for changed lines/modules: PowerShell coverage carries a line-only floor (no branch-coverage gate for Pester, per `.claude/rules/quality-tiers.md`); the changed line must remain covered by both the new self-exclusion case and the preserved nested-sibling case.
- Toolchain commands to run (format → lint → type-check → test): PoshQC format, then PoshQC analyze, then Pester test via the repository's MCP PoshQC tools, restarting from format on any failure or file change. No type-check stage applies to PowerShell.
- Manual validation steps (if required): none beyond the automated Pester regression suite.

## Acceptance Criteria
- [x] 1. `scripts/vscode/Invoke-MSTestWithCoverage.ps1`'s assembly-discovery predicate excludes a candidate path based on that candidate's path relative to `$resolvedSearchRoot`, not the candidate's absolute `FullName`.
- [x] 2. Running the coverage wrapper from a checkout whose search root is located under `.claude\worktrees\agent-<id>\` discovers the test assemblies built directly beneath that root, and does not throw "No test assemblies found ... Build first." when assemblies exist there.
- [x] 3. A sibling agent worktree nested beneath the search root (a `.claude\worktrees\...` segment appearing after `$resolvedSearchRoot` in the candidate path) remains excluded from discovery, preserving the existing regression test `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:416-442` unmodified and passing.
- [x] 4. A new Pester regression test file covers both the self-exclusion fix (AC2) and the continued nested-sibling exclusion (AC3), using in-memory fixtures only (no temporary files), consistent with the repository's unit-test policy.
- [x] 5. No other file in the repository is found to carry the same absolute-path-vs-`.claude` discovery defect within the scope of this fix; the sibling-defect search performed during research is treated as authoritative unless the planner identifies a citation-backed gap during preflight.
- [x] 6. Full PowerShell toolchain (PoshQC format → PoshQC analyze → Pester test) passes in a single clean pass after the change, with no unrelated file modified.

## Risks & Mitigations
- Technical or operational risks: `$resolvedSearchRoot`'s un-normalized trailing `\.` shape (when `-SearchRoot` defaults to `.`) could theoretically interact unexpectedly with `GetRelativePath` if the runtime were not PowerShell 7+/.NET; mitigated because `.claude/rules/powershell.md` already mandates PowerShell 7+ for this repository, and `GetRelativePath` is confirmed to normalize both arguments internally (research §1.3).
- Mitigations and rollbacks: single-line production change with additive tests only; revertible via a standard git revert with no data or migration risk.

## Rollout & Follow-up
- Release/rollout steps: standard PR merge to main after the full PowerShell toolchain passes; no staged rollout or feature flag required for this developer-tooling script change.
- Post-fix monitoring or clean-up tasks: none required. Discovery-filter parity for `Invoke-MSTest.ps1` (no `.claude` clause at all today) remains a separate, explicitly out-of-scope follow-up if ever needed.
- Links: issue #752 (this item). Prior related item: issue #733 / PR #748 (introduced the original `.claude` exclusion filter this item corrects).

## Write Set
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` (new)
- `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md`
