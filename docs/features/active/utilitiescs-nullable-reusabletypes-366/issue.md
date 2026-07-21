# utilitiescs-nullable-reusabletypes (Issue #366)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-nullable-reusabletypes/ (Issue #366)

- Issue: #366
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/366
- Last Updated: 2026-07-18
- Work Mode: full-feature
- Epic: utilitiescs-nullable-remediation
- Integration branch: epic/utilitiescs-nullable-remediation-integration
- Wave: 0 (no dependencies)
- Complexity band: C3 (cross-module contract change)
- Cluster: `UtilitiesCS/ReusableTypeClasses/` (including `TimedActions/` and `NewSmartSerializable/`)

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `/t:Rebuild`) can be genuinely enforced only
after the pre-existing nullable-reference-type debt (CS86xx diagnostics) is remediated under a
per-file `#nullable enable` opt-in architecture. This feature is the Wave-0 child that remediates
the `UtilitiesCS/ReusableTypeClasses/` directory tree (including `TimedActions/` and
`NewSmartSerializable/`). These are reusable base and value types (collections, serialization
bases, matrices, timed-action helpers) consumed across module boundaries; their nullability
annotations become contracts that downstream epic features (OutlookObjects, EmailIntelligence,
Dialogs clusters) consume. This is null-annotation and null-safety remediation only, with no
behavior changes.

## Proposed Behavior

Remediate pre-existing nullable-reference-type debt (CS86xx diagnostics) across the
`UtilitiesCS/ReusableTypeClasses/` directory tree (recursive). Add a `#nullable enable` pragma to
each remediated file and bring that file to zero CS86xx diagnostics under the pragma, applying
nullable annotations (`?`), null guards, null-forgiving operators (only where justified), and
null-flow corrections. Reflect actual null behavior in the annotations while keeping public
signatures behavior-compatible. Files that emit no CS86xx diagnostics and are not required for a
clean opted-in build remain non-opted-in and must not be cross-blocked.

## Architecture (confirmed by the maintainer — do not deviate)

- Per-file `#nullable enable` opt-in. Add a `#nullable enable` pragma to each remediated file and
  bring that file to ZERO CS86xx diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no `<Nullable>`
  element and must keep none. Enforcement is per-file pragma only.
- Annotation and null-safety ONLY: nullable annotations (`?`), null guards, null-forgiving
  operators only where justified, and null-flow corrections. NO behavior changes, NO refactors, NO
  API redesign, NO feature work.
- These are reusable base/value types consumed across module boundaries. Their nullability
  annotations become contracts that downstream epic features consume. Annotate to reflect actual
  null behavior; keep public signatures behavior-compatible.
- Designer-generated and WinForms-derived files (e.g. `ConfigViewer.Designer.cs`) follow the repo
  COM/VSTO/WinForms posture; research confirms whether they are in scope for the opt-in.

## Acceptance Criteria

- [x] AC1: Every `.cs` file under `UtilitiesCS/ReusableTypeClasses/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [x] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [x] AC3: No behavior change; existing tests still pass.
- [x] AC4: No coverage regression on changed lines.
- [x] AC5: Public signatures of the remediated reusable types remain behavior-compatible;
  nullability annotations reflect actual null behavior so they are safe contracts for downstream
  epic consumers.
- [x] AC6: Non-opted-in files elsewhere in the repository are not cross-blocked by this change.

## Constraints & Risks

- Follow the repo C# toolchain in CLAUDE.md order: `csharpier` -> `msbuild` analyzers/codestyle
  -> `msbuild` nullable (`TreatWarningsAsErrors`) -> `vstest` with coverage.
- MSTest + Moq + FluentAssertions for any test work.
- No coverage regression on changed lines. Do not add temp files in tests.
- Annotations on shared reusable types are cross-module contracts; incorrect annotations could
  propagate false null-state assumptions to downstream Wave-1 children.
- Serialization types (`NewSmartSerializable/`, `SerializableNew/`, `Serializable/`) and generic
  collection bases require care so nullable annotations on generic parameters remain accurate.

## Test Conditions to Consider

- [ ] Existing UtilitiesCS test suite continues to pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline.
- [ ] Nullable build (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`) produces zero CS86xx
  diagnostics for remediated files.

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [x] Create `docs/features/active/utilitiescs-nullable-reusabletypes/` folder from the template
