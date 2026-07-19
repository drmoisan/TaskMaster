# utilitiescs-nullable-email-parsing — Spec

- **Issue:** #370
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18
- **Status:** Draft
- **Version:** 0.1

## Overview

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` (a genuine recompile
rather than a silently-skipped incremental build), cannot be enforced against new code until
the pre-existing nullable-reference-type debt (CS86xx diagnostics) is remediated under a
per-file `#nullable enable` opt-in architecture. This feature is the Wave-1 child that
remediates the `EmailIntelligence` parsing/sorting cluster only:
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/` (14 files),
`UtilitiesCS/EmailIntelligence/SubjectMap/` (7 files, one — `SubjectMapMetrics.Designer.cs` —
excluded as Designer-generated), and `UtilitiesCS/EmailIntelligence/Ctf/` (4 files). Total
remediation-target count confirmed by research: 24 of 25 `.cs` files in the cluster.

These files consume the shared extension-method annotations delivered by the Wave-0
`utilitiescs-nullable-extensions` child (issue #363), whose nullability annotations are the
cross-module contracts this cluster relies on for correct null-flow analysis. This cluster's
atomic plan must not begin until Wave-0's verify-only file (`NullExtensions.cs`), Batch B
(`StringExtensions.cs`), and Batch C (`IEnumerableExtensions.cs`) have merged — see
"Upstream Dependency Mapping" below.

## Behavior

Each remediated `.cs` file in the cluster receives a per-file `#nullable enable` pragma and is
brought to zero CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`. Remediation
applies nullable annotations (`?`), generic constraints (`where T : notnull`), unconstrained
`T?` returns and `out` parameters, null-flow corrections, and null-forgiving operators (`!`)
only where justified. Existing null guards already present in the files remain as-is.

This is null-annotation and null-safety remediation only. There are NO behavior changes to the
parsing/sorting logic, no refactors, no API redesign, and no feature work. Public method
signatures remain behavior-compatible: an existing caller that compiles today continues to
compile and behaves identically. Non-remediated files elsewhere in the repository remain
non-opted-in and must not be cross-blocked by this change.

Some of the 24 targets may already be free of CS86xx diagnostics once the pragma is added
(for example small DTO-style files with already-consistent null handling); these become
verify-only under the pragma rather than requiring substantive annotation edits, but every one
of the 24 still requires the `#nullable enable` pragma to be added and a clean rebuild
confirmed.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none. This is a library-internal source change with no
  runtime inputs.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced.
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: public method signatures of the remediated
  types remain behavior-compatible. The observable change is limited to nullability
  annotations, which are additive contract metadata rather than a source- or binary-breaking
  behavior change. One tuple-typed return (`EmailFiler.TryMoveMailItemHelperAsync`'s
  `(MailItem Original, MailItem Moved)`) has its `Moved` element annotated `MailItem?` without
  changing the tuple shape or the deconstruction call sites in
  `EmailFiler.ProcessMailHelperAsync` / `TryMoveMailItemForProcessingAsync`.

## API / CLI Surface

There is no CLI surface and no new API. This is a library-internal change. The relevant
"API surface" is the set of nullability annotations applied to the public and internal members
of the 24 remediation-target files.

- Example invocations with expected outputs (concise): not applicable; no command or CLI flag
  is added. No `/p:Nullable=enable` global flag is introduced into any verification command
  (see Toolchain Note).
- Contracts and validation rules:
  - Public method/property signatures remain behavior-compatible; only nullability annotations
    change (for example, `EmailDataMiner.Serialization`'s `Deserialize<T>` /
    `DeserializeFromFolder<T>` / `DeserializeAsync<T>` overloads, which currently return
    `default(T)` on a missing lookup or file, become unconstrained `T?` returns; `Folder`-typed
    getters that already explicitly return `null` on failure, such as
    `EmailFilerConfig.TryResolveDestinationFolder()` and `MovedMailInfo.MailItem`, become
    `Folder?` / `MailItem?`).
  - Annotation choices reflect each method's actual null behavior, consistent with the
    upstream `utilitiescs-nullable-extensions` annotation contracts this cluster consumes
    (`NullExtensions.ThrowIfNull<T>`'s non-null-asserted return, `StringExtensions.IsNullOrEmpty`,
    `IEnumerableExtensions.Transpose`).
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
    `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) are not
    available or polyfilled on this target and must not be used or added (see Constraints & Risks).

## Data & State

- Data transformations and invariants: none changed. This is annotation-only; no runtime data
  flow, transform, or invariant is altered. Existing null guards (e.g.
  `EmailFilerConfig.IsDeleteRelevant`'s `currentFolder.ThrowIfNull()`,
  `SubjectMapEncoder.RebuildEncoding()`'s `NullReferenceException` guard) remain unchanged.
- Caching or persistence details: none.
- Migration or backfill requirements (if any): none. No project-level `<Nullable>` element is
  introduced into `UtilitiesCS.csproj`; the project has no `<Nullable>` element today and must
  keep none. Enforcement is per-file pragma only.

## Constraints & Risks

- Target framework net481, C# 12 (`LangVersion` 12.0). All nullable syntax is available: `?`,
  `!`, unconstrained `T?`, `where T : notnull`, `is null` / `is not null` flow analysis.
- Nullable post-condition attributes from `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`,
  `[DoesNotReturn]`, `[MemberNotNull]`) are NOT available or polyfilled on this target and must
  not be used or added (same constraint as the upstream extensions child).
- `record` / `record struct` / `init` accessors fail CS0518 on net481 (no `IsExternalInit`);
  do not convert existing plain structs to records. The one struct in this cluster,
  `FolderStruct` (`EmailDataMiner.Transform.cs`, lines 17-28), uses a C# 12 primary constructor
  and must remain a plain `internal struct` with primary-constructor syntax — primary
  constructors on non-record types do not require `IsExternalInit` and carry no CS0518 risk.
  `SpamBayesOptions` in `EmailTokenizer.cs` is a plain `struct` with only `const` fields and
  carries no nullable-annotation risk.
- Prefer annotation plus justified `!` over new runtime guard statements. New
  `if (x is null) throw` statements are executable lines that would require new test coverage
  (AC4 pressure) and could constitute a behavior change (AC3). Existing guards stay as-is.
- Designer-generated files (`SubjectMapMetrics.Designer.cs`) are generated code and are
  not remediation targets; only its partial-class sibling `SubjectMapMetrics.cs` is remediated.
- Do NOT pass `/p:Nullable=enable` globally for verification; use the per-file pragma gate
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true`.
- Files exceeding the 500-line general file-size limit are pre-existing conditions and must
  not be split during this annotation-only remediation (splitting would be an out-of-scope
  refactor): `SortEmail.cs` (~1407 lines, almost entirely `[ExcludeFromCodeCoverage]`),
  `EmailTokenizer.cs` (~729 lines), `SubjectMapEntry.cs` (~657 lines). Flag these for a future
  refactor issue; do not fix here. `EmailDataMiner.FolderExtraction.cs` (~483 lines) is under
  the limit but flagged as the largest of the four `EmailDataMiner` partial files.
  `ArrayExtensions.cs` from the upstream Wave-0 spec is the analogous precedent for this
  "flag, do not split" constraint.
- COM/Outlook interop types (`MailItem`, `Folder`/`MAPIFolder`, `Explorer`, `Attachment`) are
  referenced throughout most of `EmailParsingSorting/` and cannot be constructed in isolation
  for a runtime check, but the per-file pragma architecture means only a compile-time
  `msbuild /t:Rebuild` is required — no live Outlook process is needed to remediate or verify
  annotations.
- Two duplicate-named test-file pairs were identified by research (e.g. `EmailFiler_Tests.cs`
  exists in two directories; `EmailTokenizer(Tests|_Tests).cs`, `CommonWords_Test(s).cs`,
  `CtfMap(Tests|_Tests).cs`, `CtfIncidenceList(Tests|_Tests).cs`, `MinedMailInfo*Tests.cs` are
  each duplicated). This is not necessarily a build problem (MSTest requires unique
  fully-qualified class names, not unique file names) but the atomic plan must capture a clean
  baseline test run before editing to avoid attributing a pre-existing ambiguity to this
  feature's changes.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to each
  of the 24 remediation-target files and bring each to zero CS86xx under the pragma. No new
  types, methods, commands, or files are added; only nullability annotations on existing
  members change. No project or solution file changes.
- New classes/functions/commands to add or update: none.
- Batch grouping (from research; leaf-first, dependency-ordered, annotation-scope only):
  - **Batch A — trivial leaves** (DTOs / obsolete / small interfaces, no partial-class
    entanglement): `IEmailTokenizer.cs`, `TesseractOcrTextExtractor.cs`, `CtfMapEntry.cs`,
    `CtfIncidence.cs`, `MinedMailInfo.cs`, `MovedMailInfo.cs`.
  - **Batch B — CTF map and subject-map leaf collections** (depend only on Batch A's
    `CtfMapEntry`): `CtfMap.cs`, `CtfIncidenceList.cs`, `CommonWords.cs`.
  - **Batch C — SubjectMap encoding chain** (internally ordered: `SubjectMapEncoder` before
    `SubjectMapEntry` before the `SubjectMapSco` partial pair, since `SubjectMapEntry` consumes
    `ISubjectMapEncoder` and `SubjectMapSco` consumes `SubjectMapEntry`):
    `SubjectMapEncoder.cs`, `SubjectMapEntry.cs`, then the combined partial pair
    `SubjectMapSco.cs` + `SubjectMapSco.Orchestration.cs` (**must be remediated in one batch**
    — see partial-class batching rule below), then `SubjectMapMetrics.cs` (consumes
    `SubjectMapSco.SummaryMetric`).
  - **Batch D — email filing/config core** (depends on Wave-0's `NullExtensions.cs` /
    `StringExtensions.cs` already being remediated): `EmailFilerConfig.cs`, then `EmailFiler.cs`
    (constructs/consumes `EmailFilerConfig`).
  - **Batch E — image/OCR/tokenization chain** (depends on Batch A's
    `TesseractOcrTextExtractor.cs` and Wave-0's `IEnumerableExtensions.Transpose` /
    `StringExtensions.IsNullOrEmpty`): `ImageStripper.cs`, then `EmailTokenizer.cs` (constructs
    `new ImageStripper()` in `setup()`).
  - **Batch F — `EmailDataMiner` partial-class group** (single combined batch — see
    partial-class batching rule below): `EmailDataMiner.cs` + `EmailDataMiner.FolderExtraction.cs`
    + `EmailDataMiner.Serialization.cs` + `EmailDataMiner.Transform.cs` remediated together.
  - **Batch G — static sorting orchestrators** (depend on Batch D/F types):
    `AutoFile.cs`, `SortEmail.cs`.
  - Full task-by-task sequencing within each batch belongs to the atomic plan, not this spec.
- **Partial-class batching rules (must remediate together in one batch):**
  1. `EmailDataMiner` (4 files, namespace `UtilitiesCS.EmailIntelligence.Bayesian` — note this
     namespace differs from the `EmailParsingSorting` folder name, a pre-existing
     folder/namespace mismatch with no bearing on annotation work): `EmailDataMiner.cs`,
     `EmailDataMiner.FolderExtraction.cs`, `EmailDataMiner.Serialization.cs`,
     `EmailDataMiner.Transform.cs`. The shared private fields `_globals` and `_sw` (both
     declared in `EmailDataMiner.cs`) are consumed across all four files' methods. Annotating
     them in isolation without checking usage across all four files would risk an inconsistent
     nullable contract for the partial type.
  2. `SubjectMapSco` (2 files, namespace `UtilitiesCS`): `SubjectMapSco.cs` and
     `SubjectMapSco.Orchestration.cs`. The class's public/internal surface (`Add`, `Find`,
     `Serialize`) is exercised across both files' methods (e.g. `RepopulateSubjectMapEntries` in
     `.Orchestration.cs` calls `this.Add(...)` defined in the primary file), so both files must
     be remediated in the same commit/PR to keep the partial type's nullable contract coherent.
  - `SubjectMapMetrics.cs` / `SubjectMapMetrics.Designer.cs` is also technically a partial-class
    pair, but the Designer file is excluded from remediation entirely (generated code, no
    `#nullable` state to reconcile), so no batching constraint arises from that pair.
- Upstream dependency mapping (issue #363 contracts consumed by this cluster):
  - `NullExtensions.cs` (Wave-0 verify-only, already `#nullable enable`): consumed via
    `ThrowIfNull<T>` and the `ThrowIfNullOrEmpty` overloads (`EmailFiler.cs`,
    `EmailFilerConfig.cs`, `EmailDataMiner.Serialization.cs`).
  - `StringExtensions.cs` (Wave-0 Batch B): consumed via `string.IsNullOrEmpty()` extension
    across `EmailDataMiner.Transform.cs`, `EmailDataMiner.Serialization.cs`, `EmailFiler.cs`,
    `EmailTokenizer.cs`, `ImageStripper.cs`, `SortEmail.cs`.
  - `IEnumerableExtensions.cs` (Wave-0 Batch C): consumed via `Transpose<T>` in
    `EmailTokenizer.cs`'s `commonprefix`/`commonsuffix` helpers.
  - `IListExtensions.cs` (Wave-0 Batch C): declares `IsNullOrEmpty(this IList<string> list)`;
    no confirmed call site in this cluster, retained for completeness since it shares Wave-0's
    Batch C with `IEnumerableExtensions.cs`.
  - **Ordering constraint inherited from Wave-0:** this cluster's atomic plan must not begin
    until Wave-0's verify-only file (`NullExtensions.cs`), Batch B, and Batch C have merged,
    because `EmailTokenizer.cs` and most of this cluster's other files compile against those
    files' post-remediation signatures. The epic's stated Wave dependency (`#370` depends on
    `#363`) encodes this at the feature level; this constraint is the confirmed file-level
    reason.
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable. Each remediated
  batch is independently mergeable because non-opted-in files remain null-oblivious and are not
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

- [ ] AC1: Every `.cs` file in the cluster (`EmailParsingSorting/`, `SubjectMap/`, `Ctf/`) that
  emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the
  per-file pragma with `TreatWarningsAsErrors`.
- [ ] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [ ] AC3: No behavior change to parsing/sorting logic; existing tests still pass.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of the remediated types remain behavior-compatible; nullability
  annotations reflect actual null behavior and are consistent with the upstream
  `utilitiescs-nullable-extensions` annotation contracts they consume.
- [ ] AC6: Non-remediated files remain non-opted-in and are not cross-blocked; the change is
  independently mergeable under the per-file pragma architecture.

## Seeded Test Conditions (from potential)

- [ ] Existing `UtilitiesCS.Test` suite covering EmailIntelligence parsing/sorting continues to
  pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline (prefer annotation and
  justified `!` over new runtime guards to avoid introducing uncovered executable lines).
- [ ] The pragma-driven nullable gate produces zero CS86xx diagnostics for the remediated files
  without passing `/p:Nullable=enable` globally.
- [ ] A baseline `vstest.console.exe` run (pass/fail counts and coverage percentage) for
  `UtilitiesCS.Test` is captured before any edit, per the evidence-and-timestamp-conventions
  skill, so any regression during remediation is attributable to an annotation change and not
  a pre-existing duplicate-test-name ambiguity (see Constraints & Risks).
- [ ] After each batch, the same test assembly is rerun and pass/fail counts and per-file
  changed-line coverage are diffed against the baseline — no new failures, no coverage
  regression on the lines touched by that batch.

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

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag
forces nullable project-wide and surfaces the full pre-existing debt across the solution,
drowning this child's signal. This is the same rules-versus-convention conflict the Wave-0
spec and the epic manifest flag for the maintainer and defer to the Wave-2 CI capstone child;
resolving it is out of scope here.
