# ci-nullable-check-skipped-vendored-projects (Potential Bug)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

The CI `quality-gates` job's nullable/`TreatWarningsAsErrors` build pass never actually nullable-checks the vendored projects `SVGControl` and `UtilitiesSwordfish.NET.General`. The first build pass compiles them, and the second pass then finds them up-to-date and skips `CoreCompile`, so `Nullable=enable` flow analysis is silently not enforced on those projects in CI.

## Environment

- OS/version: Windows (CI `windows-latest`; reproduced locally on VS 18 Community MSBuild)
- Python version: n/a
- Command/flags used: the two sequential `msbuild TaskMaster.sln /t:Build` invocations in `.github/workflows/ci.yml` — pass 1 `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, pass 2 `/p:Nullable=enable /p:TreatWarningsAsErrors=true`
- Data source or fixture: `TaskMaster.sln`

## Steps to Reproduce

1. From a clean state, run pass 1: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — succeeds (real compile).
2. Immediately run pass 2: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` — succeeds with 0/0 because MSBuild's timestamp-based incremental up-to-date check skips `CoreCompile` for the already-built projects.
3. Now run a single from-scratch pass carrying all four properties against a cleaned solution — `Build FAILED, 84 Error(s)` (34 in `SVGControl.csproj`, 50 in `UtilitiesSwordfish.NET.General.csproj`), all base-compiler nullable-flow diagnostics (`CS8618`, `CS8625`, `CS8600`–`CS8604`, `CS8619`).

## Expected Behavior

`Nullable=enable`/`TreatWarningsAsErrors=true` enforcement should apply consistently to every project the pass is intended to cover, or the intended coverage scope should be explicit and documented.

## Actual Behavior

The second pass is a no-op recompile for projects already built by the first pass, so nullable-flow analysis is effectively not enforced for `SVGControl` and `UtilitiesSwordfish.NET.General` in CI. 84 latent nullable defects exist in those projects, undetected by the current pipeline.

## Logs / Screenshots

- [x] Evidence captured under issue #267
- Snippet: see `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/csharp-consolidated-build-final.2026-07-07T20-45.md` (records the failing consolidated run and full root-cause analysis).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Note: the affected projects are vendored/third-party. Per `.claude/rules/csharp.md`, analyzers are wired to first-party projects only and vendored projects are excluded, so the correct remedy may be to explicitly scope the nullable gate rather than remediate vendored code. Triage required.

## Suspected Cause / Notes

MSBuild incremental up-to-date short-circuit: property changes between two sequential `/t:Build` invocations do not force a recompile of already-built projects. The two-pass CI structure therefore masks the second pass's enforcement on any project the first pass already built. Discovered during issue #267 (ci-quality-gates-speedup) when consolidating the two passes surfaced the defects.

## Proposed Fix / Validation Ideas

- [ ] Decide intended nullable-enforcement scope: first-party only vs. whole solution.
- [ ] If whole-solution: either remediate the 84 nullable defects or add explicit, documented `[ExcludeFromCodeCoverage]`-equivalent nullable scoping / project-level `<Nullable>` settings for the vendored projects.
- [ ] If first-party only: make the CI nullable pass target only first-party projects so enforcement is real and visible, not accidental.
- [ ] Add a check that the nullable pass performs a real compile (not an incremental skip) for its intended targets.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
