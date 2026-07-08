# ci-quality-gates-speedup (Issue #267)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ci-quality-gates-speedup/ (Issue #267)

- Issue: #267
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/267
- Last Updated: 2026-07-08
- Work Mode: minor-audit

## Problem / Why

The `quality-gates` job in `.github/workflows/ci.yml` runs on `windows-latest` and is the dominant CI cost. Two avoidable inefficiencies drive its wall-clock time:

1. **No dependency caching.** `nuget restore` re-downloads every package for all 17 `packages.config` projects on each run, and `dotnet tool restore` reinstalls CSharpier each run.
2. **Two full-solution builds.** The job runs `msbuild /t:Build` twice — once with analyzers/code-style enforcement and once with `Nullable=enable /p:TreatWarningsAsErrors=true`. Because the second invocation changes msbuild properties, msbuild invalidates incremental state and recompiles the entire solution a second time. The builds are also single-process (no `/m`).

## Proposed Behavior

Reduce CI wall-clock time without weakening any gate:

1. Cache the `packages/` NuGet folder keyed on the hash of all `packages.config` files.
2. Cache the CSharpier tool restore (dotnet tool NuGet cache) keyed on `dotnet-tools.json`.
3. Add `/m` (parallel build) to the msbuild invocation(s).
4. Preserve both build gates as **two separate `msbuild` passes** (analyzers/code-style, then nullable/`TreatWarningsAsErrors`), adding `/m` to each. See the Scope Decision note below: consolidation into a single pass was investigated and dropped because it is not behavior-neutral.

## Scope Decision (2026-07-07)

The original item 4 proposed consolidating the two build passes into one. Local verification during execution proved this is **not behavior-neutral**: the current two-pass sequence silently skips nullable-flow analysis on the vendored projects `SVGControl` and `UtilitiesSwordfish.NET.General` (MSBuild incremental up-to-date short-circuit in the second pass). A single consolidated pass surfaces 84 pre-existing nullable defects in those vendored projects and fails the build. Per user decision (Option A), consolidation is dropped; the two passes are retained (each with `/m`) so behavior is unchanged. The discovered CI nullable-check gap is captured separately at `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`. AC4 remains satisfied by the "retained as two, with no reduction in enforced diagnostics" branch.

## Acceptance Criteria

- [x] AC1: `.github/workflows/ci.yml` restores NuGet packages from a cache keyed on `**/packages.config` and falls back to a restore on cache miss.
- [x] AC2: `.github/workflows/ci.yml` caches the CSharpier tool restore keyed on `dotnet-tools.json`.
- [x] AC3: The msbuild invocation(s) pass `/m` for parallel project builds.
- [x] AC4: The analyzer/code-style enforcement and the nullable `TreatWarningsAsErrors` enforcement are both preserved (consolidated into one build pass or retained as two), with no reduction in enforced diagnostics.
- [x] AC5: `actionlint` passes on the modified workflow.
- [x] AC6: A green CI run against the branch head is produced (the `modified-workflow-needs-green-run` gate) before merge. Satisfied by PR #271 CI run 28912404849 against head `aaa2ae4e` — required checks `Format, build, analyze, and test` and `actionlint` both SUCCESS. Note: this checkoff commit advances the branch head; a follow-up CI run against the new head confirms the green state for the merge commit.

## Constraints & Risks

- **Build-consolidation risk.** Merging the two builds means any analyzer diagnostic not held at `suggestion` in `.editorconfig` would now fail the build under `TreatWarningsAsErrors=true`. Repository policy (`.claude/rules/csharp.md`) states analyzer severities are held at `suggestion` specifically so the nullable/warnings-as-errors build does not break; this must be verified locally by running the consolidated command before relying on it.
- **Workflow-change gate.** Per repository policy, a workflow-file change triggers `modified-workflow-needs-green-run` and must not be committed outside the remediation/CI-green loop.
- No C# source changes; the change is confined to CI configuration.

## Test Conditions to Consider

- [ ] Cache hit and cache miss paths both restore a buildable `packages/` tree.
- [ ] Consolidated build enforces analyzer, code-style, and nullable/warnings-as-errors diagnostics equivalently to the prior two-build sequence.
- [ ] `actionlint` static validation of the modified YAML.
- [ ] Green end-to-end CI run against the branch head.

## Next Step

- [x] Promote to GitHub issue (#267)
- [x] Create `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/` folder from the template
- [ ] Generate atomic plan, execute, review, and pass the CI-green gate
