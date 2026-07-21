# utilitiescs-nullable-email-classifier (Issue #372)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/ (Issue #372)
- Epic: utilitiescs-nullable-remediation
- Integration branch: epic/utilitiescs-nullable-remediation-integration
- Wave: 1 (depends on Wave-0 Extensions contracts, issue #363)
- Complexity band: C3 (classifier-engine T1 modules; cross-module contract change)

- Issue: #372
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/372
- Last Updated: 2026-07-19
- Work Mode: full-feature

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild` so it performs a
genuine recompile rather than a silently-skipped incremental build) can be enforced against
new code only after the pre-existing nullable-reference-type debt (CS86xx diagnostics) is
remediated under a per-file `#nullable enable` opt-in architecture. This feature is the
Wave-1 child that remediates the `UtilitiesCS/EmailIntelligence/` classifier cluster:
`Bayesian`, `ClassifierGroups`, and `Flags`.

These modules are classified T1 (Critical) per `.claude/rules/quality-tiers.md`: they are
classifier engines (SpamBayes, Triage) whose behavior bugs can cause silent model drift or
misclassification. This work is null-annotation and null-safety remediation ONLY. It makes
no change to classifier scoring, model logic, corpus math, or any behavior. It consumes the
nullability annotation contracts published by the Wave-0 Extensions child (issue #363), whose
shared extension methods (for example `CastNullSafe`, `IEnumerableExtensions`,
`DictionaryExtensions`, `NullExtensions`) are called throughout these classifier files.

## Proposed Behavior

Remediate pre-existing nullable-reference-type debt (CS86xx diagnostics) across the
`UtilitiesCS/EmailIntelligence/Bayesian`, `.../ClassifierGroups`, and `.../Flags` directory
trees. Add a `#nullable enable` pragma to each remediated file and bring that file to zero
CS86xx diagnostics under the pragma with `TreatWarningsAsErrors`, applying nullable
annotations (`?`), generic constraints (`where T : notnull`), unconstrained `T?` returns and
`out` parameters, null-flow corrections, and null-forgiving operators (`!`) only where
justified. Reflect actual null behavior in the annotations while keeping public signatures
behavior-compatible. Existing null guards remain as-is.

## Architecture (confirmed by the maintainer — do not deviate)

- Per-file `#nullable enable` opt-in. Add a `#nullable enable` pragma to each remediated file
  and bring that file to ZERO CS86xx diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none. Enforcement is per-file pragma only. Non-remediated
  files remain null-oblivious and must not be cross-blocked.
- Annotation and null-safety ONLY: no behavior changes, no refactors, no API redesign, no
  feature work. NO change to classifier scoring, model logic, or corpus/probability math.
- These are T1 modules. Preserve all existing golden/property/characterization tests unchanged;
  do not alter any scoring path.
- Consumes the Wave-0 Extensions annotation contracts (issue #363). Where a classifier file
  calls an extension method annotated by #363, honor that method's published nullability
  contract rather than re-deriving it.

## Acceptance Criteria (early draft)

- [ ] AC1: Every in-scope `.cs` file under `UtilitiesCS/EmailIntelligence/Bayesian`,
  `.../ClassifierGroups`, and `.../Flags` that emits CS86xx carries `#nullable enable` and
  compiles with zero nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors`.
- [ ] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [ ] AC3: No behavior change; no change to any classifier scoring or model path; existing
  tests (including golden/property tests) still pass unchanged.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of remediated members remain behavior-compatible; nullability
  annotations reflect actual null behavior and honor the upstream #363 extension contracts.

## Constraints & Risks

- Follow the repo C# toolchain in CLAUDE.md order: `csharpier` -> `msbuild` analyzers/codestyle
  -> `msbuild` nullable verification via the per-file pragma gate (`/t:Rebuild`,
  `TreatWarningsAsErrors=true`, WITHOUT `/p:Nullable=enable` globally) -> `vstest` with coverage.
- MSTest + Moq + FluentAssertions for any test work; no temp files in tests.
- Designer-generated files (`*.Designer.cs`), interface-only files, dead `Obsolete/` code, and
  already-clean files are candidates for exclusion; the exact remediation set is determined by
  research measuring which files emit CS86xx under the pragma.
- Nullable post-condition attributes from `System.Diagnostics.CodeAnalysis` are NOT available
  or polyfilled on net481 and must not be used or added.
- These are T1 classifier engines; an incorrect annotation on a scoring/corpus path could
  propagate a false null-state assumption. Annotate to the true null behavior; do not alter math.

## Test Conditions to Consider

- [ ] Existing `UtilitiesCS.Test` EmailIntelligence suite (including any golden/property tests)
  continues to pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline.
- [ ] The pragma-driven nullable gate produces zero CS86xx diagnostics for the remediated files,
  without passing `/p:Nullable=enable` globally.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/` folder from the template
