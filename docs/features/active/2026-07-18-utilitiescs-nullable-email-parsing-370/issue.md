# utilitiescs-nullable-email-parsing (Issue #370)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/ (Issue #370)
- Parent: Epic `utilitiescs-nullable-remediation` (Wave 1)
- Depends on: `utilitiescs-nullable-extensions` (issue #363)

- Issue: #370
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/370
- Last Updated: 2026-07-19
- Work Mode: full-feature

## Problem / Why

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` (a genuine recompile
rather than a silently-skipped incremental build), cannot be enforced against new code until
the pre-existing nullable-reference-type debt (CS86xx diagnostics) is remediated under a
per-file `#nullable enable` opt-in architecture. This feature is the Wave-1 child that
remediates the `EmailIntelligence` parsing/sorting cluster only:
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/`, `UtilitiesCS/EmailIntelligence/SubjectMap/`,
and `UtilitiesCS/EmailIntelligence/Ctf/` (~18 files that emit CS86xx out of 25 `.cs` files).

These files consume the shared extension-method annotations delivered by the Wave-0
`utilitiescs-nullable-extensions` child (issue #363), whose nullability annotations are the
cross-module contracts this cluster relies on for correct null-flow analysis.

## Proposed Behavior

Each remediated `.cs` file in the cluster receives a per-file `#nullable enable` pragma and is
brought to zero CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`. Remediation
applies nullable annotations (`?`), generic constraints (`where T : notnull`), unconstrained
`T?` returns and `out` parameters, null-flow corrections, and null-forgiving operators (`!`)
only where justified. Existing null guards already present in the files remain as-is.

This is null-annotation and null-safety remediation only. There are NO behavior changes to the
parsing/sorting logic, no refactors, no API redesign, and no feature work. Public method
signatures remain behavior-compatible. Non-remediated files elsewhere in the repository remain
non-opted-in and must not be cross-blocked by this change.

## Acceptance Criteria (early draft)

- [x] AC1: Every `.cs` file in the cluster (`EmailParsingSorting/`, `SubjectMap/`, `Ctf/`) that
  emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the
  per-file pragma with `TreatWarningsAsErrors`.
- [x] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [x] AC3: No behavior change to parsing/sorting logic; existing tests still pass.
- [x] AC4: No coverage regression on changed lines.
- [x] AC5: Public signatures of the remediated types remain behavior-compatible; nullability
  annotations reflect actual null behavior and are consistent with the upstream
  `utilitiescs-nullable-extensions` annotation contracts they consume.
- [x] AC6: Non-remediated files remain non-opted-in and are not cross-blocked; the change is
  independently mergeable under the per-file pragma architecture.

## Constraints & Risks

- Target framework net481, C# 12 (`LangVersion` 12.0). All nullable syntax is available: `?`,
  `!`, unconstrained `T?`, `where T : notnull`, `is null` / `is not null` flow analysis.
- Nullable post-condition attributes from `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`,
  `[DoesNotReturn]`, `[MemberNotNull]`) are NOT available or polyfilled on this target and must
  not be used or added (same constraint as the upstream extensions child).
- `record` / `record struct` / `init` accessors fail CS0518 on net481 (no `IsExternalInit`);
  do not convert existing plain structs to records.
- Prefer annotation plus justified `!` over new runtime guard statements. New
  `if (x is null) throw` statements are executable lines that would require new test coverage
  (AC4 pressure) and could constitute a behavior change (AC3). Existing guards stay as-is.
- Designer-generated files (e.g. `SubjectMapMetrics.Designer.cs`) are generated code and are
  not remediation targets.
- Do NOT pass `/p:Nullable=enable` globally for verification; use the per-file pragma gate
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true`.

## Test Conditions to Consider

- [ ] Existing `UtilitiesCS.Test` suite covering EmailIntelligence parsing/sorting continues to
  pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline.
- [ ] The pragma-driven nullable gate produces zero CS86xx diagnostics for the remediated files
  without passing `/p:Nullable=enable` globally.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/` folder from the template
