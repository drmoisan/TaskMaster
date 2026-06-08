# csharp-analyzer-stack-hardening — Spec

- **Issue:** #181
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-08T12-12
- **Status:** Draft
- **Version:** 0.1

## Overview

The .claude governance sync (issue #178, PR #179) intentionally deferred adopting the
reference repo's hardened C# analyzer-stack + central-config + clock-seam mechanism because
it risks breaking this repo's legacy VSTO/.NET Framework build. This repo's 19 `.csproj`
files are all legacy NON-SDK-style; 16 use `packages.config`; CI restores via
`nuget restore`. The hardened mechanism must be adapted so it builds cleanly with zero new
build/CI failures and without violating the repo's retained policy (MSTest/Moq, 80/90
coverage, msbuild + vstest).


## Behavior

Adopt the C# analyzer-stack and determinism hardening as a repository mechanism, adapted to
this repo's toolchain:

- Add analyzer-only package references (Meziantou.Analyzer, SonarAnalyzer.CSharp,
  Roslynator.Analyzers, AsyncFixer, SecurityCodeScan.VS2019, BannedApiAnalyzers) to
  first-party projects only (exclude vendored SVGControl, UtilitiesSwordfish.NET.General).
- Activate BannedApiAnalyzers + BannedSymbols.txt for the 5 banned time/random symbols.
- Add TimeProvider/FakeTimeProvider seam guidance to rules/csharp.md.
- Add analyzer severities, file-scoped-namespace preference, and naming rules to
  .editorconfig/.globalconfig scoped so the existing codebase does not produce
  build-breaking errors.


## Inputs / Outputs

- Inputs (CLI flags, files, env vars)
- Outputs (artifacts, logs, telemetry)
- Config keys and defaults:
- Versioning or backward-compatibility constraints:

## API / CLI Surface

List commands, flags, request/response shapes, and examples.
- Example invocations with expected outputs (concise):
- Contracts and validation rules:

## Data & State

Data flow, storage, or state changes introduced by this feature.
- Data transformations and invariants:
- Caching or persistence details:
- Migration or backfill requirements (if any):

## Constraints & Risks

- CPM (Directory.Packages.props) is INCOMPATIBLE with packages.config; must not be introduced.
- Mixing PackageReference + packages.config in one project is unsupported; prefer existing style.
- New strict analyzers WILL emit warnings across legacy code; the nullable
  `/p:TreatWarningsAsErrors=true` CI step will break if severities are promoted to errors.
  Introduce analyzers at warning/suggestion severity or scope to new/touched files.
- Directory.Build.props at repo root is imported by ALL projects including vendored ones;
  scope strict analyzers to first-party projects only.
- CI restores via `nuget restore` (not `dotnet restore`); package additions must restore with it.


## Implementation Strategy

- Implementation scope (what changes, not sequencing):
- New classes/functions/commands to add or update:
- Dependency changes (new/removed packages) and rationale:
- Logging/telemetry additions and locations:
- Rollout plan (feature flags, staged deploys, fallback path):

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

## Seeded Test Conditions (from potential)
- [ ] `nuget restore TaskMaster.sln` succeeds with new analyzer packages.
- [ ] Both msbuild stages (analyzers; nullable-as-errors) stay green.
- [ ] vstest MSTest run unaffected.
- [ ] PR GitHub Actions CI green.
