# utilitiescs-nullable-extensions — Spec

- **Issue:** #363
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T21-20
- **Status:** Draft
- **Version:** 0.1

## Overview

What need or gap does this idea address?

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` so it performs a
genuine recompile rather than a silently-skipped incremental build, cannot be enforced
against new code until the pre-existing nullable-reference-type debt (CS86xx diagnostics)
is remediated under a per-file `#nullable enable` opt-in architecture. This feature is the
Wave-0 child that remediates the `UtilitiesCS/Extensions/` directory tree only.

Scope is the Extensions cluster: recursively, 25 `.cs` files, of which 2
(`IAsyncEnumerableExtensions.cs`, `NullExtensions.cs`) already carry `#nullable enable` and
are verify-only, leaving 23 files to remediate. These are shared extension methods consumed
across module boundaries. Their nullability annotations become cross-module contracts that
downstream Wave-1 children (OutlookObjects, EmailIntelligence, Dialogs clusters) consume, so
the annotations must reflect actual null behavior. This work is null-annotation and
null-safety remediation only; it introduces no behavior changes.

## Behavior

What should the feature do at a high level?

Each remediated file receives a per-file `#nullable enable` pragma and is brought to zero
CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`. Remediation applies
nullable annotations (`?`), generic constraints (`where T : notnull`), unconstrained `T?`
returns and `out` parameters, null-flow corrections, and null-forgiving operators (`!`) only
where justified. Existing null guards already present in the files remain as-is.

The work is annotation and null-safety only. There are no behavior changes, no refactors, no
API redesign, and no feature work. Public method signatures remain behavior-compatible: an
existing caller that compiles today continues to compile and behaves identically. The
annotation choices reflect the true null behavior of each method so that the resulting
signatures are safe contracts for downstream consumers.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none. This is a library-internal source change with no
  runtime inputs.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced.
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: public method signatures of the remediated
  extension methods remain behavior-compatible. The observable change is limited to nullability
  annotations, which are additive contract metadata rather than a source- or binary-breaking
  behavior change.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.

There is no CLI surface and no new API. This is a library-internal change. The relevant
"API surface" is the set of nullability annotations applied to the public extension methods
in `UtilitiesCS/Extensions/`.

- Example invocations with expected outputs (concise): not applicable; no command or CLI flag
  is added. No `/p:Nullable=enable` global flag is introduced into any verification command
  (see Toolchain note).
- Contracts and validation rules:
  - Public extension-method signatures remain behavior-compatible; only nullability annotations
    change (for example, an optional `Action<int> onItemCompleted = null` parameter becomes
    `Action<int>? onItemCompleted = null`; an unconstrained `TValue` `out` parameter or return
    becomes `out TValue?` / `TValue?`).
  - Annotation choices reflect the method's actual null behavior. Because these are shared
    extension methods, the annotations become cross-module contracts consumed by Wave-1
    children; an incorrect annotation could propagate a false null-state assumption downstream.
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
    `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) are not
    available or polyfilled on this target and must not be used or added (see Constraints & Risks).

## Data & State

Data flow, storage, or state changes introduced by this feature.

- Data transformations and invariants: none changed. This is annotation-only; no runtime data
  flow, transform, or invariant is altered.
- Caching or persistence details: none.
- Migration or backfill requirements (if any): none. In particular, no project-level `<Nullable>`
  element is introduced into `UtilitiesCS.csproj`; the project has no `<Nullable>` element today
  and must keep none. Enforcement is per-file pragma only.

## Constraints & Risks

List notable constraints (performance, compatibility, scope) or risks.

- Target framework net481, C# 12 (`LangVersion` 12.0). All nullable syntax is available: `?`,
  `!`, unconstrained `T?`, `where T : notnull`, and `is null` / `is not null` flow analysis.
- Nullable post-condition attributes from `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`,
  `[DoesNotReturn]`, `[MemberNotNull]`) are NOT available on this target and are NOT polyfilled
  in the repository. They must not be used or added. Zero CS86xx is reachable without them
  (proven by the already-enabled `NullExtensions.cs`), using plain `?`, `where T : notnull`,
  unconstrained `T?`, guard clauses, and justified `!`. Adding such a polyfill would be new
  production surface and is out of scope.
- `ArrayExtensions.cs` is 544 lines and exceeds the general 500-line file limit. This is a
  pre-existing condition. The work is annotation-only and MUST NOT split the file, since that
  would be a refactor and out of scope. Flag for a future issue; do not fix here.
- `DfDeedle.cs` and `DfDeedle.FrameUtilities.cs` are the same `partial class` and must be
  remediated in the same batch so shared members are annotated together.
- `DfDeedle.EmailRecord` must remain a plain `private struct`. Do not convert it to a `record` or
  `record struct` (`init` / positional `record` / `record struct` fail CS0518 on net481, which
  lacks `IsExternalInit`). Its `= default` reference-type field initializers become `= default!`
  or are typed non-nullable and initialized in the constructor.
- Prefer annotation plus justified `!` over new runtime guard statements. New `if (x is null) throw`
  statements are executable lines that would require new test coverage (AC4 pressure) and could
  constitute a behavior change (AC3). Existing guards stay as-is.
- Annotations on shared extension methods are cross-module contracts; incorrect annotations could
  propagate false null-state assumptions to downstream Wave-1 children.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to each of
  the 23 files that require remediation and bring each to zero CS86xx under the pragma; verify the
  2 already-enabled files (`IAsyncEnumerableExtensions.cs`, `NullExtensions.cs`) still compile
  clean with no edits expected. No project or solution file changes.
- New classes/functions/commands to add or update: none. No new types, methods, commands, or
  files are added; only nullability annotations on existing members change.
- Batch grouping (from research; leaf-first, contract-core before its dataframe consumers):
  - Batch A — trivial / confirm-clean leaves: `ExtToChar.cs`, `CompilerServicesExtensions.cs`,
    `DrawingExtensions.cs`, `QueueExtensions.cs`, `IControlExtensions.cs`, `ExceptionExtensions.cs`.
  - Batch B — string / serialization / image-stream utilities: `StringExtensions.cs`,
    `JsonExtensions.cs`, `JsonSerializerExtensions.cs`, `ImageExtensions.cs`, `StreamExtensions.cs`,
    `LazyExtension.cs`.
  - Batch C — core generic collection contracts (careful review; must precede Batch E):
    `IEnumerableExtensions.cs`, `ArrayExtensions.cs`, `IListExtensions.cs`, `DictionaryExtensions.cs`.
  - Batch D — reflection / metadata / WinForms: `EnumExtensions.cs`, `TraceExtensions.cs`,
    `WinFormsExtensions.cs`.
  - Batch E — dataframe + async serialization (consumes Batch C): `AsyncSerialization.cs`,
    `DfMLNet.cs`, `DfDeedle.cs`, `DfDeedle.FrameUtilities.cs`.
  - Ordering constraint: Batch C must precede Batch E because `DfMLNet`/`DfDeedle` consume
    `CastNullSafe`, `ToStringArray`, `SliceColumn`, and `To2D` from Batch C; annotating the core
    contracts first prevents re-touching the dataframe files.
  - Verify-only (not a batch): `IAsyncEnumerableExtensions.cs`, `NullExtensions.cs`.
  - The full task-by-task sequencing belongs to the atomic plan, not this spec.
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable. Each remediated
  file is independently mergeable because non-opted-in files remain null-oblivious and are not
  cross-blocking under the per-file pragma architecture.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

Acceptance criteria (from `issue.md`, mapped here for traceability):

- [ ] AC1: Every `.cs` file under `UtilitiesCS/Extensions/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [ ] AC3: No behavior change; existing tests still pass.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of the remediated extension methods remain behavior-compatible;
  nullability annotations reflect actual null behavior so they are safe contracts for downstream
  epic consumers.

## Seeded Test Conditions (from potential)
- [ ] Existing `UtilitiesCS.Test/Extensions/` suite continues to pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline (prefer annotation and justified
  `!` over new runtime guards to avoid introducing uncovered executable lines).
- [ ] The pragma-driven nullable gate (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug
  /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`) produces zero CS86xx diagnostics for the
  remediated files, without passing `/p:Nullable=enable` globally.

## Toolchain Note

Run the repo C# toolchain in CLAUDE.md order:

1. `csharpier .` (adding a pragma line and `?` annotations reformats; run before each build).
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
   /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style).
3. Nullable verification via the per-file pragma gate:
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`. Under `TreatWarningsAsErrors`, any CS86xx in a pragma-enabled
   file becomes an error while non-opted files stay silent.
4. `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage`.

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag forces
nullable project-wide and surfaces the full pre-existing debt across the solution, drowning this
child's signal. That global-flag-versus-per-file-pragma mismatch is the rules-versus-convention
conflict the epic flags for the maintainer and defers to the Wave-2 CI capstone child; resolving
it is out of scope here.
