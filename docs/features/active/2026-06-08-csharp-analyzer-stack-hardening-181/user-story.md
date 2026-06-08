# `csharp-analyzer-stack-hardening` — User Story

- Issue: #181
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-06-08T12-12

## Story Statement

- As a ..., I want ..., so that ...
- As a ..., I want ..., so that ...

## Problem / Why

The .claude governance sync (issue #178, PR #179) intentionally deferred adopting the
reference repo's hardened C# analyzer-stack + central-config + clock-seam mechanism because
it risks breaking this repo's legacy VSTO/.NET Framework build. This repo's 19 `.csproj`
files are all legacy NON-SDK-style; 16 use `packages.config`; CI restores via
`nuget restore`. The hardened mechanism must be adapted so it builds cleanly with zero new
build/CI failures and without violating the repo's retained policy (MSTest/Moq, 80/90
coverage, msbuild + vstest).


## Personas & Scenarios

- Persona: ...
  - who the user is
  - what they care about
  - their constraints
  - their goals and frustrations
  - their context and motivations
- Scenario: ...
  - A concrete, step-by-step narrative that describes how a user accomplishes a goal in a real-world context using the system.
  - who is acting?
  - what triggered the action?
  - what steps do they take?
  - what obstacles or decisions occur?
  - what outcome do they expect?


## Acceptance Criteria

- [ ] AC1: Analyzer packages referenced by first-party projects; restore cleanly via `nuget restore`.
- [ ] AC2: BannedApiAnalyzers + BannedSymbols.txt active; 5 banned symbols flagged in new/touched code.
- [ ] AC3: TimeProvider/FakeTimeProvider seam + guidance added to rules/csharp.md; no runtime behavior changed.
- [ ] AC4: .editorconfig/.globalconfig carries new severities, file-scoped-namespace pref, naming rules, scoped to avoid build-breaking errors.
- [ ] AC5: All four toolchain stages pass locally to the extent the environment allows; nullable TreatWarningsAsErrors step does NOT regress.
- [ ] AC6: PR CI is GREEN, including nullable-as-errors and MSTest-with-coverage steps.
- [ ] AC7: No do_not_change invariant violated; rules/csharp.md updated retaining MSTest/Moq, 80/90 coverage, msbuild+vstest.
- [ ] AC8: Change scoped to C# build-config + rules/csharp.md (+ .editorconfig/.globalconfig + Directory.Build.props if used + per-project analyzer refs). No application logic changes except seam introductions required to compile.


## Non-Goals

Call out what is explicitly excluded from this feature.
