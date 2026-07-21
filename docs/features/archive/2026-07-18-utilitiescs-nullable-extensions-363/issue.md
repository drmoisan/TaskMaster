# utilitiescs-nullable-extensions (Issue #363)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/ (Issue #363)

- Issue: #363
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/363
- Last Updated: 2026-07-18
- Work Mode: full-feature
- Epic: utilitiescs-nullable-remediation
- Integration branch: epic/utilitiescs-nullable-remediation-integration
- Wave: 0 (no dependencies)
- Complexity band: C3 (cross-module contract change)

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `/t:Rebuild`) can be genuinely
enforced only after the pre-existing nullable-reference-type debt (CS86xx diagnostics)
is remediated under a per-file `#nullable enable` opt-in architecture. This feature is
the Wave-0 child that remediates the `UtilitiesCS/Extensions/` directory tree. These are
shared extension methods consumed across module boundaries; their nullability annotations
become contracts that downstream epic features (OutlookObjects, EmailIntelligence, Dialogs
clusters) consume. This is null-annotation and null-safety remediation only, with no
behavior changes.

## Proposed Behavior

Remediate pre-existing nullable-reference-type debt (CS86xx diagnostics) across the
`UtilitiesCS/Extensions/` directory tree (recursive; approximately 25 `.cs` files, 2 of
which already carry `#nullable enable`). Add a `#nullable enable` pragma to each remediated
file and bring that file to zero CS86xx diagnostics under the pragma, applying nullable
annotations (`?`), null guards, null-forgiving operators (only where justified), and
null-flow corrections. Reflect actual null behavior in the annotations while keeping public
signatures behavior-compatible.

## Architecture (confirmed by the maintainer — do not deviate)

- Per-file `#nullable enable` opt-in. Add a `#nullable enable` pragma to each remediated
  file and bring that file to ZERO CS86xx diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none. Enforcement is per-file pragma only.
- Annotation and null-safety ONLY: nullable annotations (`?`), null guards, null-forgiving
  operators only where justified, and null-flow corrections. NO behavior changes, NO
  refactors, NO API redesign, NO feature work.
- These are shared extension methods consumed across module boundaries. Their nullability
  annotations become contracts that downstream epic features consume. Annotate to reflect
  actual null behavior; keep public signatures behavior-compatible.

## Acceptance Criteria

- [x] AC1: Every `.cs` file under `UtilitiesCS/Extensions/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma
  with `TreatWarningsAsErrors`.
- [x] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [x] AC3: No behavior change; existing tests still pass.
- [x] AC4: No coverage regression on changed lines.
- [x] AC5: Public signatures of the remediated extension methods remain behavior-compatible;
  nullability annotations reflect actual null behavior so they are safe contracts for
  downstream epic consumers.

## Constraints & Risks

- Follow the repo C# toolchain in CLAUDE.md order: `csharpier` -> `msbuild` analyzers/codestyle
  -> `msbuild` nullable (`TreatWarningsAsErrors`) -> `vstest` with coverage.
- MSTest + Moq + FluentAssertions for any test work.
- No coverage regression on changed lines. Do not add temp files in tests.
- Annotations on shared extension methods are cross-module contracts; incorrect annotations
  could propagate false null-state assumptions to downstream Wave-1 children.

## Test Conditions to Consider

- [ ] Existing UtilitiesCS test suite continues to pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline.
- [ ] Nullable build (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`) produces zero
  CS86xx diagnostics for remediated files.

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [x] Create `docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/` folder from the template
