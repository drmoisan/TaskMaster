# utilitiescs-nullable-svgcontrol — Spec

- **Issue:** #368
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-10
- **Status:** Draft
- **Version:** 0.1

## Overview

What need or gap does this idea address?

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` so it performs a
genuine recompile rather than a silently-skipped incremental build, cannot be enforced
against new code until the pre-existing nullable-reference-type debt (CS86xx diagnostics)
is remediated under a per-file `#nullable enable` opt-in architecture. This feature is the
Wave-0 child that remediates the `SVGControl/` directory tree only.

`SVGControl/` is a separate `net481` WinForms control project (WinForms controls, an SVG
parser/renderer, and PropertyGrid type converters/editors). It has NO `ProjectReference` to
`UtilitiesCS`, so it is functionally independent of every other cluster in the epic; its
annotations are not consumed as cross-module contracts by other epic children. It is in scope
solely because the current solution-level nullable gate covers `SVGControl.csproj` too, and it
must be opted in on the same per-file basis so the repaired gate can be enforced solution-wide.

Scope is `SVGControl/` recursively: 20 `.cs` files total, of which 12 are hand-authored
remediation targets, 3 (`PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs`) already
carry `#nullable enable` and are verify-only (vendored BCL-internal helpers), and 5 are
WinForms `*.Designer.cs` / generated files that are not opted in. This work is null-annotation
and null-safety remediation only; it introduces no behavior changes.

## Behavior

What should the feature do at a high level?

Each of the 12 hand-authored files receives a per-file `#nullable enable` pragma and is
brought to zero CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`. Remediation
applies nullable annotations (`?`), null guards, null-flow corrections (including flow-narrowed
locals such as retyping a null-literal-initialized local to its nullable form so existing
reassignment clears it before use), and null-forgiving operators (`!`) only where justified.
Existing null guards already present in the files remain as-is; no new runtime guard clauses
are introduced.

The 3 verify-only files (`PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs`) are
confirmed, not edited: they already carry `#nullable enable` and must continue to compile
clean under the pragma gate. The 5 Designer/generated files (`ButtonSVG.Designer.cs`,
`PictureBoxSVG.Designer.cs`, `ToggleSwitch.Designer.cs`, `Properties/Resources.Designer.cs`,
`Properties/AssemblyInfo.cs`) are not opted in; because the C# nullable context is a per-file
directive, opting in a hand-authored partial-class file does not force its Designer
counterpart to be checked. They are edited only if the pragma build requires a mechanical,
behavior-preserving change, which research found not to be the case for any of the five.

The work is annotation and null-safety only. There are no behavior changes, no refactors, no
API redesign, and no feature work. Public method signatures remain behavior-compatible: an
existing caller that compiles today continues to compile and behaves identically. The one
exception requiring a deliberate documented decision rather than a mechanical fix is
`SvgImageSelector.ImagePath` (see Constraints & Risks).

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none. This is a library-internal source change with no
  runtime inputs.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced.
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: public method signatures of the remediated
  control, parser, and converter types remain behavior-compatible. The observable change is
  limited to nullability annotations, which are additive contract metadata rather than a
  source- or binary-breaking behavior change. Because `SVGControl/` has no in-solution
  consumer via `ProjectReference` from `UtilitiesCS`, these annotations do not become
  cross-module contracts for any other epic child.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.

There is no CLI surface and no new API. This is a library-internal change. The relevant
"API surface" is the set of nullability annotations applied to the public members of
`ButtonSVG`, `PictureBoxSVG`, `ToggleSwitch`, `SvgImageSelector`, `SvgRenderer`, `SVGParser`,
`ISvgResource`/`SvgResource`, `SvgResourceConverter`, `DropDownEditor`,
`SvgOptionsConverter`/`SvgOptionsConverter1`, and `SvgFileNameEditor`.

- Example invocations with expected outputs (concise): not applicable; no command or CLI flag
  is added. No `/p:Nullable=enable` global flag is introduced into any verification command
  (see Toolchain Note).
- Contracts and validation rules:
  - Public signatures remain behavior-compatible; only nullability annotations change (for
    example, `SvgRenderer.Render()` and its `Document`/backing-field type become `Bitmap?` /
    `SvgDocument?` because the existing implementation already returns `null` on documented
    paths; `ButtonSVG.ObjectToByteArray(Object obj)` becomes `object? obj` because the existing
    body already guards `if (obj != null)`).
  - Annotation choices reflect each member's actual, already-observed null behavior. Because
    `SVGControl/` has no `ProjectReference` to `UtilitiesCS` and is not consumed by any other
    epic cluster, these annotations do not propagate as cross-module contracts to other
    Wave-0/Wave-1 children — the correctness bar is internal consistency and behavior
    preservation, not downstream contract fidelity.
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
    `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) are not
    available or polyfilled for this project and must not be used or added (see Constraints &
    Risks).

## Data & State

Data flow, storage, or state changes introduced by this feature.

- Data transformations and invariants: none changed. This is annotation-only; no runtime data
  flow, transform, or invariant is altered.
- Caching or persistence details: none.
- Migration or backfill requirements (if any): none. In particular, no project-level
  `<Nullable>` element is introduced into `SVGControl.csproj`, and no `<Nullable>` element is
  introduced at the solution level; the project has no `<Nullable>` element today and must
  keep none. Enforcement is per-file pragma only.

## Constraints & Risks

List notable constraints (performance, compatibility, scope) or risks.

- Target framework `net481`, `LangVersion` `latest`. `SVGControl.csproj` has NO
  `ProjectReference` to `UtilitiesCS` and NO indirect path to inherit a polyfill from
  `UtilitiesCS/Extensions/CompilerServicesExtensions.cs`. Nullable post-condition attributes
  from `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) are absent and unpolyfilled here, independently of the same finding for
  other epic clusters. Zero CS86xx is reachable without them, using only plain `?`,
  flow-narrowed locals, and justified `!` — already proven feasible in this same project by the
  already-clean `RelativePath.cs` / `ValueStringBuilder.cs` verify-only files.
- `record` / `init` / `record struct` trap: `SVGControl.csproj` combines `TargetFrameworkVersion
  v4.8.1` with `LangVersion latest`. Net481 has no `IsExternalInit` polyfill anywhere in the
  solution, so `init` accessors, positional `record`, and `record struct` all fail CS0518. This
  remediation is annotation-only and introduces none of these constructs; reject any drive-by
  "convert to record" temptation — for example, `ISvgResource.cs`'s `SvgResource` class (a plain
  mutable class with settable auto-properties) must stay a class, not become a `record`.
- `SVGControl.csproj` is a non-SDK-style legacy project (`ToolsVersion="15.0"`, explicit
  `<Compile Include>` items, no SDK-style `<Nullable>` element). Do not convert the project
  format; only source-file edits are in scope.
- WinForms Designer/generated-file handling: none of the five Designer/generated files
  (`ButtonSVG.Designer.cs`, `PictureBoxSVG.Designer.cs`, `ToggleSwitch.Designer.cs`,
  `Properties/Resources.Designer.cs`, `Properties/AssemblyInfo.cs`) require opt-in to keep the
  pragma build clean, because the nullable context is per-file and none contains
  nullable-sensitive code. Do not opt them in unless a specific diagnostic later requires it,
  and if so keep the edit mechanical and behavior-preserving (per AC6).
- Prefer annotation plus justified `!` over new runtime guard statements. New `if (x is null)
  throw` statements are executable lines that would require new test coverage (AC4 pressure)
  and could constitute a behavior change (AC3). Existing guards stay as-is.
- `SvgImageSelector.ImagePath` judgment call: the property's `set` accessor body is entirely
  commented out (a functional no-op today), so `_relativeImagePath` is never assigned on any
  live code path, yet the `get` accessor's fallback `return _relativeImagePath;` is a real
  CS8603 (possible null return from a non-nullable `string`-typed property) under the pragma.
  This must be resolved behavior-preservingly, not mechanically: the chosen resolution is a
  null-forgiving `_relativeImagePath!` with an in-code comment noting that the setter is
  currently a no-op, because a `?? "(none)"` fallback would change the returned value from
  `null` to a literal string when this path is hit — an observable behavior change that AC3
  does not permit absent an explicit maintainer decision to accept it. The atomic plan and its
  execution must record this specific decision at the point it is applied, not treat it as a
  routine annotation.
- Dead-code and naming quirks present in the file set (`SvgOptionsConverter.cs` defines the
  unreferenced class `SvgOptionsConverter1`; `SVGParser.cs` has zero in-project consumers) do
  not exempt those files from AC1: both are hand-authored, compiled files and must still reach
  zero CS86xx under the pragma. Do not rename, delete, or otherwise refactor them; that would
  exceed annotation-only scope.
- `RelativePath.cs` (1678 lines) already exceeds the repository's 500-line file-size limit.
  This is a pre-existing condition, is verify-only, and out of scope to split here (splitting
  would be a refactor).

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to each
  of the 12 hand-authored files and bring each to zero CS86xx under the pragma; verify the 3
  already-enabled files (`PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs`) still
  compile clean with no edits expected; confirm the 5 Designer/generated files require no
  pragma opt-in. No project or solution file changes.
- New classes/functions/commands to add or update: none. No new types, methods, commands, or
  files are added; only nullability annotations on existing members change.
- Batch grouping (from research; leaf-first, dependency-ordered):
  - Batch A — trivial independent leaves: `ISvgResource.cs`, `ToggleSwitch.cs`, `SVGParser.cs`,
    `SvgRenderer.cs`. `SvgRenderer.cs` has real null-flow work of its own but zero dependency on
    any other SVGControl hand-authored type, so it belongs with the independent leaves. This
    batch establishes the pragma + csharpier + build loop before the hub is touched.
  - Batch B — `ISvgResource` consumers, pre-hub: `SvgResourceConverter.cs`, `DropDownEditor.cs`.
    Both consume `ISvgResource` (Batch A) only; neither depends on `SvgImageSelector`.
  - Batch C — the hub (isolated on its own for careful review): `SvgImageSelector.cs`. Defines
    the `AutoSize` enum consumed by `SvgRenderer` (Batch A) and by Batch D, and carries the
    single highest-consequence judgment call in the cluster (the `ImagePath` dead-setter
    nuance, above). Isolated as a single-file batch so its review is not diluted by unrelated
    changes, mirroring the precedent of isolating the highest-scrutiny contract file(s) in the
    sibling `utilitiescs-nullable-extensions` cluster.
  - Batch D — `SvgImageSelector` consumers: `SvgOptionsConverter.cs`, `SvgOptionsConverter2.cs`,
    `SVGFileNameEditor.cs`. All three read post-remediation members of `SvgImageSelector`
    (`AboluteImagePath`, `ResourceName`, `AutoSize`) or, for `SVGFileNameEditor.cs`, the
    verify-only `RelativePath.cs`; must follow Batch C.
  - Batch E — WinForms controls, top of the dependency graph: `ButtonSVG.cs`,
    `PictureBoxSVG.cs`. Both host an `SvgImageSelector` field and depend on its
    post-remediation public property types; naturally the last consumers in the chain.
  - Ordering rationale: the grouping is leaf-first by intra-project type dependency
    (`ISvgResource` and `SvgRenderer` have zero in-project dependents to wait on; the hub,
    `SvgImageSelector`, is isolated because it carries the one judgment call in the cluster;
    its consumers and the top-level controls follow in dependency order). This avoids
    re-touching any file after it has already been brought to zero CS86xx.
  - The full task-by-task sequencing within and across these batches belongs to the atomic
    plan, not this spec.
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable. This child is
  independently mergeable because `SVGControl/` has no `ProjectReference` to `UtilitiesCS` and
  no other epic cluster depends on it (`depends_on: []` in the epic manifest); non-opted-in
  files elsewhere in the solution remain null-oblivious and are not cross-blocking under the
  per-file pragma architecture.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

Acceptance criteria (from `issue.md`, mapped here for traceability):

- [ ] AC1: Every hand-authored `.cs` file in `SVGControl/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] AC2: No project-level `<Nullable>` element is introduced into `SVGControl.csproj`, and no
  `<Nullable>` element is introduced at the solution level.
- [ ] AC3: No behavior change; existing tests still pass.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of the remediated control, parser, and converter types remain
  behavior-compatible; nullability annotations reflect actual null behavior.
- [ ] AC6: WinForms `*.Designer.cs` and generated `Properties/Resources.Designer.cs` files
  remain consistent with the pragma build; any edit to them is mechanical and
  behavior-preserving.

## Seeded Test Conditions (from potential)

- [ ] Existing `SVGControl.Test/` suite continues to pass with no behavior change. Note: the
  suite exercises only `RelativePath.cs` (`GetRelativePath_Test.cs`,
  `RelativePathCoverageTests.cs`); it does not exercise any of the 12 hand-authored
  remediation-target files. The automated changed-line coverage baseline for those 12 files is
  effectively 0%, so AC4 is numerically vacuous for this cluster specifically. This does not
  reduce the importance of AC3: with no automated safety net for these files, resolve any
  judgment call (in particular `SvgImageSelector.ImagePath`) conservatively, preferring `!` over
  a new fallback value, because a regression there would not be caught by CI. This spec does
  not require adding new tests; the work is annotation-only. If the implementing plan
  nonetheless adds a characterization test to protect a specific judgment call, it must use
  MSTest + Moq + FluentAssertions per repo convention and must not create or use temp files.
- [ ] Changed-line coverage does not regress relative to baseline for `RelativePath.cs`
  (verify-only; the one file in `SVGControl/` with a real automated test baseline).
- [ ] The pragma-driven nullable gate (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug
  /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`) produces zero CS86xx diagnostics for the
  remediated files, without passing `/p:Nullable=enable` globally.

## Toolchain Note

Run the repo C# toolchain in CLAUDE.md order:

1. `csharpier .` (adding a pragma line and `?`/`!` annotations reformats surrounding code; run
   before each build).
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
   /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style).
3. Nullable verification via the per-file pragma gate:
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`. Under `TreatWarningsAsErrors`, any CS86xx in a pragma-enabled
   `SVGControl/` file becomes a build error while the un-opted-in Designer/generated files and
   any not-yet-remediated hand-authored files elsewhere in the solution stay silent. This is the
   same gate used by PR #361 and applied solution-wide because `SVGControl.csproj` is part of
   `TaskMaster.sln`.
4. `vstest.console.exe <SVGControl.Test assembly path> /EnableCodeCoverage`.

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag
forces nullable project-wide and, applied to `SVGControl.csproj` (which has no `<Nullable>`
element, per AC2), would surface the full pre-existing CS86xx debt across every not-yet-
remediated file in `SVGControl/` — and, if run solution-wide, across all not-yet-remediated
`UtilitiesCS/` files too — instead of isolating this cluster's own per-file signal. This is the
same rules-versus-convention conflict the epic flags for the maintainer and defers to the
Wave-2 CI capstone child; it is not resolved here. Verify with the pragma-driven `/t:Rebuild
/p:TreatWarningsAsErrors=true` gate only.
