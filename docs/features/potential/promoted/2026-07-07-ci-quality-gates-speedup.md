# ci-quality-gates-speedup (Potential)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-07-ci-quality-gates-speedup-267/ (Issue #267)

## Problem / Why

The `quality-gates` job in `.github/workflows/ci.yml` runs on `windows-latest` and is the dominant CI cost. Two avoidable inefficiencies drive its wall-clock time:

1. **No dependency caching.** `nuget restore` re-downloads every package for all 17 `packages.config` projects on each run, and `dotnet tool restore` reinstalls CSharpier each run.
2. **Two full-solution builds.** The job runs `msbuild /t:Build` twice — once with analyzers/code-style enforcement and once with `Nullable=enable /p:TreatWarningsAsErrors=true`. Because the second invocation changes msbuild properties, msbuild invalidates incremental state and recompiles the entire solution a second time. The builds are also single-process (no `/m`).

## Proposed Behavior

Reduce CI wall-clock time without weakening any gate:

1. Cache the `packages/` NuGet folder keyed on the hash of all `packages.config` files.
2. Cache the CSharpier tool restore (dotnet tool NuGet cache) keyed on `dotnet-tools.json`.
3. Add `/m` (parallel build) to the msbuild invocation(s).
4. Retain the two build passes (each with `/m`). Consolidation into a single pass was investigated during execution and dropped as not behavior-neutral (see the active folder's issue.md Scope Decision).

## Acceptance Criteria (early draft)

- [ ] AC1: `.github/workflows/ci.yml` restores NuGet packages from a cache keyed on `**/packages.config` and falls back to a restore on cache miss.
- [ ] AC2: `.github/workflows/ci.yml` caches the CSharpier tool restore keyed on `dotnet-tools.json`.
- [ ] AC3: The msbuild invocation(s) pass `/m` for parallel project builds.
- [ ] AC4: The analyzer/code-style enforcement and the nullable `TreatWarningsAsErrors` enforcement are both preserved (consolidated into one build pass or retained as two), with no reduction in enforced diagnostics.
- [ ] AC5: `actionlint` passes on the modified workflow.
- [ ] AC6: A green CI run against the branch head is produced (the `modified-workflow-needs-green-run` gate) before merge.

## Constraints & Risks

- **Build-consolidation risk.** Merging the two builds means any analyzer diagnostic not held at `suggestion` in `.editorconfig` would now fail the build under `TreatWarningsAsErrors=true`. This must be verified locally before relying on it.
- **Workflow-change gate.** A workflow-file change triggers `modified-workflow-needs-green-run` and must not be committed outside the remediation/CI-green loop.
- No C# source changes; the change is confined to CI configuration.

## Test Conditions to Consider

- [ ] Cache hit and cache miss paths both restore a buildable `packages/` tree.
- [ ] The build passes enforce analyzer, code-style, and nullable/warnings-as-errors diagnostics equivalently to the prior two-build sequence.
- [ ] `actionlint` static validation of the modified YAML.
- [ ] Green end-to-end CI run against the branch head.

## Next Step

- [x] Promote to GitHub issue (#267)
- [x] Create `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/` folder from the template
