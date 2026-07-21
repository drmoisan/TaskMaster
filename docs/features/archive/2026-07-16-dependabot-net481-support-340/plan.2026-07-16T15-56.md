# dependabot-net481-support - Plan

- **Issue:** #340
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-16T15-56
- **Status:** Draft
- **Version:** 0.2

## Required References

- `CLAUDE.md` (repo-root standing instructions; policy compliance order)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `spec.md` (this feature folder) — AC-1 through AC-12, Appendix A
- `user-story.md` (this feature folder)
- `research/2026-07-16T16-10-dependabot-net481-support-research.md` (this feature folder)
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

**All work must comply with these policies; do not duplicate their content here.**

## Scope Note (config-only feature)

This feature adds exactly two artifacts: `.github/dependabot.yml` (new file) and a new
`## Dependency updates (Dependabot)` section in `README.md`. No `.cs`, `.csproj`, or test
file is touched. Per `spec.md`'s Definition of Done, the CSharpier/analyzer/nullable/
`vstest.console.exe` toolchain in `CLAUDE.md` does **not** apply to this change; the final
QC phase below substitutes a YAML-validity check, the AC-4 pre-merge enumeration check, and
an AC-10 diff review in place of that toolchain.

AC-5 (literal 16-directory fallback) and AC-11 (manual post-merge GitHub Insights check) are
intentionally **not** executed by this plan:
- AC-5 is a pre-decided contingency triggered only by a future AC-11 result; it remains a
  documented fallback in `spec.md` Appendix A, not an active task here.
- AC-11 has been resolved by the orchestrator as `scope_change`: it is a deferred, manual,
  post-merge check and is not a blocking Definition-of-Done item in this plan. The README
  documentation task (Phase 5) still records the runbook-note text pointing a future
  maintainer to that manual check.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture & Compliance Reads

- [x] [P0-T1] Read `.claude/rules/general-code-change.md` in full and confirm its design-principles/toolchain-loop sections apply to this config-only change (no C# is touched, so only the general policy governs authoring discipline)
  - Acceptance: `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/baseline/phase0-instructions-read.md` created, containing `Timestamp:`, `Policy Order:` (listing this file first), and confirmation the file was read
- [x] [P0-T2] Read `.claude/rules/general-unit-test.md` in full and record in the same evidence artifact that no unit-test policy applies to this feature (no test code is added or modified)
  - Acceptance: `phase0-instructions-read.md` updated with `.claude/rules/general-unit-test.md` added to the "files read" list and the no-test-code note
- [x] [P0-T3] Read `spec.md`, `user-story.md`, and `research/2026-07-16T16-10-dependabot-net481-support-research.md` in full
  - Acceptance: `phase0-instructions-read.md` lists all three documents under "files read" with the exact relative paths
- [x] [P0-T4] Confirm baseline: run `Test-Path .github/dependabot.yml` from the repository root and confirm the result is `False` (file does not yet exist)
  - Acceptance: `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/baseline/pre-change-state.2026-07-16T15-56.md` created containing `Timestamp:`, `Command: Test-Path .github/dependabot.yml`, `EXIT_CODE: 0`, `Output Summary: False (file does not exist)`
- [x] [P0-T5] Confirm baseline: read `README.md` and record that the `## Contents` list currently has no `Dependency updates (Dependabot)` entry, and that the `## Configuration & storage` section (line ~142) is immediately followed by the `## Common issues` section (line ~156) with no section in between
  - Acceptance: `pre-change-state.2026-07-16T15-56.md` updated with a `README.md baseline:` subsection recording the current `## Contents` bullet list verbatim and confirming the `Configuration & storage` → `Common issues` adjacency

### Phase 1 — Author `.github/dependabot.yml` core schema (AC-1, AC-3, AC-9)

- [x] [P1-T1] Create `.github/dependabot.yml` containing `version: 2` and one `updates:` list entry with `package-ecosystem: "nuget"`
  - Acceptance: file `.github/dependabot.yml` exists; its first non-comment line is `version: 2`; the `updates:` list has exactly one entry whose `package-ecosystem` value is `"nuget"` — satisfies AC-1
- [x] [P1-T2] Add `directories: ["/*"]` (or block-style equivalent) as a key of the same `nuget` `updates:` entry created in P1-T1
  - Acceptance: the `nuget` entry in `.github/dependabot.yml` contains a `directories` key whose only value is `/*` — satisfies AC-3
- [x] [P1-T3] Add a `schedule:` key to the same entry with `interval: "weekly"`
  - Acceptance: the `nuget` entry contains `schedule.interval: "weekly"`
- [x] [P1-T4] Add `open-pull-requests-limit: 10` to the same entry
  - Acceptance: the `nuget` entry contains `open-pull-requests-limit: 10` — combined with P1-T3, satisfies AC-9
- [x] [P1-T5] Check off AC-9 in `spec.md` (line beginning `- [ ] AC-9:`) and the corresponding restated bullet in `user-story.md` (the "update cadence is weekly" bullet), changing each to `- [x]`
  - Acceptance: `spec.md` line for AC-9 reads `- [x] AC-9: ...`; `user-story.md`'s weekly-cadence bullet reads `- [x] ...`

### Phase 2 — Author grouping & TFM-compatibility ignore safety net (AC-6, AC-7, AC-8)

- [x] [P2-T1] Add a `groups:` key to the `nuget` entry defining exactly four buckets — `analyzers-dev-deps`, `test-frameworks`, `microsoft-extensions-and-bcl`, `graph-identity-telemetry` — each with a `patterns:` list and `group-by: "dependency-name"`, using the package-family inventory from research §1.1/§6:
  - `analyzers-dev-deps`: `Meziantou.Analyzer`, `SonarAnalyzer.CSharp`, `Roslynator.Analyzers`, `AsyncFixer`, `Microsoft.CodeAnalysis.BannedApiAnalyzers`
  - `test-frameworks`: `MSTest.*`, `Moq`, `FluentAssertions`, `Castle.Core`, `Microsoft.Testing.*`, `Microsoft.TestPlatform.*`
  - `microsoft-extensions-and-bcl`: `Microsoft.Extensions.*`, `Microsoft.Bcl.*`, `System.*`
  - `graph-identity-telemetry`: `Microsoft.Graph*`, `Microsoft.Identity.*`, `Microsoft.IdentityModel.*`, `Azure.*`, `OpenTelemetry*`, `Microsoft.ApplicationInsights`
  - Acceptance: `.github/dependabot.yml`'s `groups` key has exactly four sub-keys matching the four bucket names above, each containing a non-empty `patterns` list and `group-by: "dependency-name"` — satisfies AC-8
- [x] [P2-T2] Add an `ignore:` key to the `nuget` entry with exactly eight entries — one per Microsoft `.NET`-runtime-aligned package family named in `spec.md` AC-6 (`Microsoft.Extensions.*`, `Microsoft.Bcl.*`, `System.Text.Json`, `System.Drawing.Common`, `Microsoft.Graph*`, `Apache.Arrow*`, `Microsoft.Data.Analysis`, `Microsoft.ML*`), each entry containing only `dependency-name` and `update-types: ["version-update:semver-major"]` (no `versions:` key on any entry)
  - Acceptance: `.github/dependabot.yml`'s `ignore` list has exactly 8 entries; each entry's `dependency-name` matches one of the eight listed families with no duplicates or omissions; no entry contains a `versions:` key — satisfies AC-6
- [x] [P2-T3] Read back the `ignore` list added in P2-T2 and confirm no entry's `update-types` contains `version-update:semver-minor` or `version-update:semver-patch`
  - Acceptance: grep of `.github/dependabot.yml` for `semver-minor` and `semver-patch` returns zero matches — satisfies AC-7
- [x] [P2-T4] Check off AC-6, AC-7, and AC-8 in `spec.md` (three `- [ ]` lines changed to `- [x]`) and the corresponding restated bullets in `user-story.md` (the TFM-safety-net bullet and the grouped-PR bullet)
  - Acceptance: `spec.md` lines for AC-6, AC-7, AC-8 each read `- [x] AC-#: ...`; `user-story.md`'s two corresponding bullets read `- [x] ...`

### Phase 3 — Schema structure verification (AC-2)

- [x] [P3-T1] Read the complete `.github/dependabot.yml` file back and confirm the single `nuget` `updates:` entry contains all six required keys (`package-ecosystem`, `directories`, `schedule`, `open-pull-requests-limit`, `groups`, `ignore`), each correctly typed per the Dependabot v2 options reference (string, list, mapping, integer, mapping, list respectively)
  - Acceptance: `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/qa-gates/ac2-schema-structure-review.2026-07-16T15-56.md` created containing `Timestamp:`, a key-by-key checklist of the six required keys with their observed type, and `Output Summary: all 6 required keys present and correctly typed` — satisfies AC-2
- [x] [P3-T2] Check off AC-1, AC-2, and AC-3 in `spec.md` (three `- [ ]` lines changed to `- [x]`)
  - Acceptance: `spec.md` lines for AC-1, AC-2, AC-3 each read `- [x] AC-#: ...`

### Phase 4 — Directory coverage enumeration evidence (AC-4; AC-5/AC-11 left unchecked by design)

- [x] [P4-T1] Run `Get-ChildItem -Path . -Filter packages.config -Recurse | Select-Object FullName` from the repository root and capture the full output
  - Acceptance: `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/qa-gates/ac4-packages-config-enumeration.2026-07-16T15-56.md` created containing `Timestamp:`, `Command: Get-ChildItem -Path . -Filter packages.config -Recurse | Select-Object FullName`, `EXIT_CODE: 0`, and `Output Summary:` listing the full set of returned `FullName` values
- [x] [P4-T2] Confirm the enumeration output from P4-T1 lists exactly 18 `packages.config` files, all at path depth 1 from the repository root, spanning the 16 directories in `spec.md` Appendix A plus `VBFunctions` and `VBFunctions.Test`
  - Acceptance: `ac4-packages-config-enumeration.2026-07-16T15-56.md` updated with a `Depth check:` line confirming each `FullName` has exactly one path segment between the repository root and `packages.config`, and a count line reading `Total: 18` — satisfies AC-4
- [x] [P4-T3] Check off AC-4 in `spec.md` (`- [ ]` changed to `- [x]`) and the corresponding "no directory left unmonitored" bullet in `user-story.md`
  - Acceptance: `spec.md`'s AC-4 line reads `- [x] AC-4: ...`; `user-story.md`'s matching bullet reads `- [x] ...`
- [x] [P4-T4] Confirm AC-5 and AC-11 remain unchecked in `spec.md` by design and record the reason in the plan's Scope Note cross-reference
  - Acceptance: reading `spec.md` confirms the literal strings `- [ ] AC-5:` and `- [ ] AC-11:` are still present (unchanged); `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/other/ac5-ac11-deferred-note.2026-07-16T15-56.md` created stating AC-5 is a pre-decided contingency not triggered by this plan and AC-11 is an orchestrator-resolved `scope_change` deferred to manual post-merge verification

### Phase 5 — README documentation deliverable (AC-12)

- [x] [P5-T1] Add a `* [Dependency updates (Dependabot)](#dependency-updates-dependabot)` entry to `README.md`'s `## Contents` list, inserted immediately after the `* [Configuration & storage](#configuration--storage)` entry and immediately before the `* [Common issues](#common-issues)` entry
  - Acceptance: `README.md`'s `## Contents` list shows the new entry in that exact position
- [x] [P5-T2] Insert a new `## Dependency updates (Dependabot)` section into `README.md` immediately after the `## Configuration & storage` section and immediately before the `## Common issues` section, opening with content point 1: Dependabot's NuGet updater cannot independently bump a transitive/indirect dependency beyond what its referencing primary dependency's own manifest supports (documented default ecosystem behavior per GitHub Docs "About Dependabot security updates"), not a mechanism this repo's config invents
  - Acceptance: `README.md` contains a `## Dependency updates (Dependabot)` heading positioned between the two named sections, with prose covering content point 1
- [x] [P5-T3] Add content point 2 to the same section: the `semver-major`-scoped `ignore` rule set (the eight Microsoft `.NET`-runtime-aligned package families) is this repo's defense against a future framework-support drop, adopted because no currently-published version of any referenced package has dropped net481 support (verified 2026-07-16; research §4); minor/patch updates for the same families remain unaffected
  - Acceptance: the `## Dependency updates (Dependabot)` section in `README.md` contains prose covering content point 2, naming the eight package families and the semver-major scoping
- [x] [P5-T4] Add content point 3 to the same section: the `directories: ["/*"]` decision and its pre-decided fallback (the literal 16-entry list in `spec.md` Appendix A), to be adopted only if post-merge verification shows under-coverage
  - Acceptance: the section contains prose covering content point 3, referencing `spec.md` Appendix A by name
- [x] [P5-T5] Add content point 4 to the same section: a runbook note pointing maintainers to the manual, out-of-toolchain post-merge verification step (spec AC-11) — confirming via the repository's Insights → Dependency graph → Dependabot tab that at least one of the 18 directories is scanned
  - Acceptance: the section contains a runbook note naming the Insights → Dependency graph → Dependabot path and referencing AC-11
- [x] [P5-T6] Check off AC-12 in `spec.md` (`- [ ]` changed to `- [x]`) and the corresponding documentation bullet in `user-story.md`
  - Acceptance: `spec.md`'s AC-12 line reads `- [x] AC-12: ...`; `user-story.md`'s matching bullet reads `- [x] ...`

### Phase 6 — TFM non-modification diff review (AC-10)

- [x] [P6-T1] Run `git status --porcelain` from the repository root and capture the full output; the porcelain output is expected to include pre-existing unrelated entries (agent-memory drift under `.claude/agent-memory/**` and the feature's own `docs/features/active/2026-07-16-dependabot-net481-support-340/` folder) alongside the two files this feature adds/modifies, and P6-T2's check below is scoped to project/build files, not an exact two-path match
  - Acceptance: `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/qa-gates/ac10-diff-review.2026-07-16T15-56.md` created containing `Timestamp:`, `Command: git status --porcelain`, `EXIT_CODE: 0`, and `Output Summary:` listing the changed-path set
- [x] [P6-T2] Confirm the `git status --porcelain` output from P6-T1 shows `.github/dependabot.yml` (untracked/new) and `README.md` (modified) as changed paths, with no `.csproj`, `.cs`, or other project/build file listed
  - Acceptance: `ac10-diff-review.2026-07-16T15-56.md` updated with a `Path check:` line confirming that among the `git status --porcelain` output, the only `.csproj`/`.cs`/other build-project-file changes are none, and that `.github/dependabot.yml` (untracked/new) and `README.md` (modified) are present as changed paths; pre-existing unrelated paths outside this feature's scope (e.g. `.claude/agent-memory/**` and the feature's own `docs/features/active/2026-07-16-dependabot-net481-support-340/` folder) are permitted and must be listed separately as 'pre-existing, out-of-scope' rather than treated as a verification failure
- [x] [P6-T3] Run `git diff --name-only -- "*.csproj"` from the repository root and confirm the command produces no output
  - Acceptance: `ac10-diff-review.2026-07-16T15-56.md` updated with `Command: git diff --name-only -- "*.csproj"`, `EXIT_CODE: 0`, `Output Summary: no output (zero .csproj files changed)` — satisfies AC-10
- [x] [P6-T4] Check off AC-10 in `spec.md` (`- [ ]` changed to `- [x]`) and the corresponding TFM-unchanged bullet in `user-story.md`
  - Acceptance: `spec.md`'s AC-10 line reads `- [x] AC-10: ...`; `user-story.md`'s matching bullet reads `- [x] ...`

### Phase 7 — Final QC: YAML validity and Definition-of-Done reconciliation

- [x] [P7-T1] Run `pip install --quiet pyyaml` from the repository root to guarantee YAML-parsing tooling is available for the validity check in P7-T2
  - Acceptance: `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/qa-gates/yaml-validity.2026-07-16T15-56.md` created containing `Timestamp:`, `Command: pip install --quiet pyyaml`, `EXIT_CODE: 0`, `Output Summary: pyyaml installed/already satisfied`
- [x] [P7-T2] Run `python -c "import yaml; yaml.safe_load(open('.github/dependabot.yml', encoding='utf-8')); print('DEPENDABOT_YAML_VALID')"` from the repository root and confirm it exits 0 and prints `DEPENDABOT_YAML_VALID`
  - Acceptance: `yaml-validity.2026-07-16T15-56.md` updated with `Command: python -c "import yaml; yaml.safe_load(open('.github/dependabot.yml', encoding='utf-8')); print('DEPENDABOT_YAML_VALID')"`, `EXIT_CODE: 0`, `Output Summary: DEPENDABOT_YAML_VALID printed — file parses as valid YAML` — satisfies AC-2's "file parses as valid YAML" clause
- [x] [P7-T3] If P7-T2 fails (non-zero exit or missing `DEPENDABOT_YAML_VALID` output), fix the reported YAML syntax error in `.github/dependabot.yml` and rerun P7-T1 and P7-T2 until both pass in a single pass
  - Acceptance: `yaml-validity.2026-07-16T15-56.md`'s final recorded run for P7-T2 shows `EXIT_CODE: 0` and `DEPENDABOT_YAML_VALID`
- [x] [P7-T4] Check off the applicable `spec.md` Definition of Done items: "Acceptance criteria (AC-1 through AC-12 above) documented and mapped to verification steps", "`.github/dependabot.yml` matches every AC in this spec", "`README.md` documentation section added per AC-12", "Pre-merge enumeration evidence (AC-4) recorded", "Diff review confirms no TFM changes (AC-10)", "Post-merge Dependabot scan verification (AC-11) scheduled/tracked as a runbook item", and "No toolchain pass required beyond YAML validity" — changing each `- [ ]` to `- [x]`
  - Acceptance: all seven listed `spec.md` Definition of Done lines read `- [x] ...`
- [x] [P7-T5] Record a final plan-completion summary noting AC-5 and AC-11 remain intentionally unchecked (contingency and deferred-manual respectively) while AC-1 through AC-4, AC-6 through AC-10, and AC-12 are checked
  - Acceptance: `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/other/plan-completion-summary.2026-07-16T15-56.md` created listing each AC's final checkbox state (checked/unchecked) matching the state left in `spec.md`

## Test Plan

- Unit: not applicable — no C#/production code is added or modified by this feature.
- Integration: not applicable — no runtime code path exists to integration-test; Dependabot's own scheduled scan (AC-11, deferred/manual) is the only integration signal, and it is explicitly out of this plan's execution scope.
- Manual/CLI:
  - `Get-ChildItem -Path . -Filter packages.config -Recurse | Select-Object FullName` (Phase 4, AC-4).
  - `git status --porcelain` and `git diff --name-only -- "*.csproj"` (Phase 6, AC-10).
  - `python -c "import yaml; yaml.safe_load(open('.github/dependabot.yml', encoding='utf-8'))"` (Phase 7, YAML validity).
- Coverage evidence: not applicable — this is a config/documentation-only change with no executable code, so no line/branch coverage baseline or comparison artifact is produced. `spec.md`'s Definition of Done explicitly states no toolchain pass beyond YAML validity applies.

## Evidence Artifact Index

All evidence lives under `docs/features/active/2026-07-16-dependabot-net481-support-340/evidence/`:
- `baseline/phase0-instructions-read.md`
- `baseline/pre-change-state.2026-07-16T15-56.md`
- `qa-gates/ac2-schema-structure-review.2026-07-16T15-56.md`
- `qa-gates/ac4-packages-config-enumeration.2026-07-16T15-56.md`
- `qa-gates/ac10-diff-review.2026-07-16T15-56.md`
- `qa-gates/yaml-validity.2026-07-16T15-56.md`
- `other/ac5-ac11-deferred-note.2026-07-16T15-56.md`
- `other/plan-completion-summary.2026-07-16T15-56.md`

## Open Questions / Notes

- None — `spec.md`'s "Resolved Ambiguities" section closes both residual ambiguities from research (`packages.config` support corroboration path, and `/*` glob-depth decision). No open questions remain for this plan.
