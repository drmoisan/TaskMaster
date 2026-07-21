# utilitiescs-nullable-newtonsofthelpers (Issue #367)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/ (Issue #367)
- Epic: utilitiescs-nullable-remediation (child, Wave 0)

- Issue: #367
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/367
- Last Updated: 2026-07-19
- Work Mode: full-feature

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild ... /p:Nullable=enable
/p:TreatWarningsAsErrors=true`) now performs a genuine recompile and surfaces pre-existing
CS86xx nullable-reference-type diagnostics that were previously masked. The
`UtilitiesCS/NewtonsoftHelpers/` directory tree (approximately 14-19 `.cs` files: custom
Newtonsoft.Json `JsonConverter`/`SerializationBinder`/`ITraceWriter` implementations, wrapper
converters, and the SDIL Reader helpers) carries such pre-existing nullable debt. These are
serialization helpers consumed across modules; their nullability annotations become contracts
that downstream persistence and settings-store code consume.

## Proposed Behavior

Remediate the pre-existing nullable-reference-type debt across `UtilitiesCS/NewtonsoftHelpers/`
using a per-file `#nullable enable` opt-in:

- Add a `#nullable enable` pragma to each remediated file and bring that file to zero CS86xx
  diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none.
- Annotation and null-safety only: nullable annotations (`?`), null guards, null-forgiving
  operators only where justified, and null-flow corrections. No behavior changes, no refactors,
  no API redesign, no feature work.
- Keep public signatures behavior-compatible; annotate to reflect the actual null behavior so the
  annotations serve as accurate downstream contracts. `JsonConverter.ReadJson`/`WriteJson`
  overrides carry framework-defined nullability on `existingValue`/`value`/`serializer` that must
  be honored exactly.

## Acceptance Criteria (early draft)

- [ ] Every `.cs` file under `UtilitiesCS/NewtonsoftHelpers/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] No project-level `<Nullable>` element is introduced in `UtilitiesCS.csproj`.
- [ ] No behavior change; existing tests still pass; no coverage regression on changed lines.

## Constraints & Risks

- Use the pragma-only verification build (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without
  `/p:Nullable=enable`) so the whole epic's ~2131 diagnostics are not surfaced as false failures.
- `JsonConverter<T>` / `JsonConverter` override signatures (`ReadJson`, `WriteJson`, `CanConvert`)
  have framework-defined nullable annotations under Newtonsoft.Json; annotate implementations to
  match rather than change them.
- Annotations become cross-module contracts consumed by serialization/persistence callers;
  incorrect annotations could propagate incorrect null assumptions.
- The `.claude/rules/csharp.md` toolchain documents forcing `/p:Nullable=enable` globally, which
  conflicts with the per-file opt-in convention. This conflict is flagged for the maintainer at
  the epic level (capstone child), not resolved here.

## Test Conditions to Consider

- [ ] Existing MSTest suite for UtilitiesCS still passes post-annotation.
- [ ] No coverage regression on changed lines.
- [ ] Nullable gate (`/t:Rebuild /p:TreatWarningsAsErrors=true`) passes for the opted-in files.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/` folder from the template
