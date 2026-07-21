# utilitiescs-nullable-svgcontrol (Issue #368)

- Date captured: 2026-07-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/ (Issue #368)

- Issue: #368
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/368
- Last Updated: 2026-07-18
- Work Mode: full-feature
- Epic: utilitiescs-nullable-remediation
- Integration branch: epic/utilitiescs-nullable-remediation-integration
- Wave: 0 (no dependencies)
- Complexity band: C2 (file-local null-safety in an independent WinForms control project)

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `/t:Rebuild`) can be genuinely enforced
only after the pre-existing nullable-reference-type debt (CS86xx diagnostics) is remediated
under a per-file `#nullable enable` opt-in architecture. This feature is the Wave-0 child
that remediates the `SVGControl/` project. `SVGControl/` is a separate `net481` WinForms
control project (WinForms controls, an SVG parser, and type converters). It has NO
`ProjectReference` to `UtilitiesCS`, so it is functionally independent of the other
`UtilitiesCS/` clusters. It is in scope because the current solution-level nullable gate
covers it too, and it must be opted in on the same per-file basis so the repaired gate can
be enforced solution-wide. This is null-annotation and null-safety remediation only, with no
behavior changes.

## Proposed Behavior

Remediate pre-existing nullable-reference-type debt (CS86xx diagnostics) across the
hand-authored `.cs` files in `SVGControl/`: the WinForms controls (`ButtonSVG.cs`,
`PictureBoxSVG.cs`, `ToggleSwitch.cs`), the SVG parser/renderer/selector
(`SVGParser.cs`, `SvgRenderer.cs`, `SvgImageSelector.cs`, `ISvgResource.cs`), and the type
converters/editors (`SvgOptionsConverter.cs`, `SvgOptionsConverter2.cs`,
`SvgResourceConverter.cs`, `DropDownEditor.cs`, `SVGFileNameEditor.cs`). Add a
`#nullable enable` pragma to each remediated file and bring that file to zero CS86xx
diagnostics under the pragma, applying nullable annotations (`?`), null guards, null-forgiving
operators (only where justified), and null-flow corrections. Keep public signatures
behavior-compatible.

## Architecture (confirmed by the maintainer — do not deviate)

- Per-file `#nullable enable` opt-in. Add a `#nullable enable` pragma to each remediated file
  and bring that file to ZERO CS86xx diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `SVGControl.csproj` has no
  `<Nullable>` element and must keep none. Enforcement is per-file pragma only.
- Annotation and null-safety ONLY: nullable annotations (`?`), null guards, null-forgiving
  operators only where justified, and null-flow corrections. NO behavior changes, NO refactors,
  NO API redesign, NO feature work.
- `SVGControl/` is an independent project with no `ProjectReference` to `UtilitiesCS`; its
  annotations are not consumed as cross-module contracts by other epic children. Annotate to
  reflect actual null behavior; keep public signatures behavior-compatible.
- WinForms Designer-generated files (`*.Designer.cs`) and generated `Properties/Resources.Designer.cs`
  are typically excluded from coverage per repo policy. Remediate hand-authored control code and
  keep Designer files consistent; do not opt Designer files in unless required to keep the pragma
  build clean and only with mechanical, behavior-preserving edits.
- Three files (`PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs`) already carry
  `#nullable enable` (vendored BCL-internal helpers). They are verify-only: confirm they still
  compile clean under the pragma gate; do not re-edit unless a diagnostic is emitted.

## Acceptance Criteria

- [x] AC1: Every hand-authored `.cs` file in `SVGControl/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [x] AC2: No project-level `<Nullable>` element is introduced into `SVGControl.csproj`, and no
  `<Nullable>` element is introduced at the solution level.
- [x] AC3: No behavior change; existing tests still pass.
- [x] AC4: No coverage regression on changed lines.
- [x] AC5: Public signatures of the remediated control, parser, and converter types remain
  behavior-compatible; nullability annotations reflect actual null behavior.
- [x] AC6: WinForms `*.Designer.cs` and generated `Properties/Resources.Designer.cs` files remain
  consistent with the pragma build; any edit to them is mechanical and behavior-preserving.

## Constraints & Risks

- Follow the repo C# toolchain in CLAUDE.md order: `csharpier` -> `msbuild` analyzers/codestyle
  -> `msbuild` nullable (`TreatWarningsAsErrors`) -> `vstest` with coverage.
- MSTest + Moq + FluentAssertions for any test work.
- No coverage regression on changed lines. Do not add temp files in tests.
- Target framework `net481`, `LangVersion` `latest`. Nullable post-condition attributes from
  `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
  `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) may not be
  polyfilled on this target; verify availability before use and prefer plain `?`, guard clauses,
  and justified `!` to reach zero CS86xx.
- This is a non-SDK-style legacy `.csproj` with explicit `<Compile Include>` items. Do not
  convert the project format; only source files change.
- Prefer annotation plus justified `!` over new runtime guard statements. New `if (x is null) throw`
  statements are executable lines that would require new test coverage (AC4 pressure) and could
  constitute a behavior change (AC3). Existing guards stay as-is.
- WinForms Designer files are auto-generated; avoid semantic edits. If the pragma build requires a
  change in a Designer file, keep it mechanical and behavior-preserving.

## Test Conditions to Consider

- [ ] Existing test suite covering `SVGControl/` (if any) continues to pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline.
- [ ] The pragma-driven nullable gate (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug
  /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`) produces zero CS86xx diagnostics for the
  remediated files, without passing `/p:Nullable=enable` globally.

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [x] Create `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/` folder from the template
