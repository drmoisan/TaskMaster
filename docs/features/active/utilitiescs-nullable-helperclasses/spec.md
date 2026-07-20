# utilitiescs-nullable-helperclasses — Spec

- **Issue:** #364
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (child, Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T21-45
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Overview

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild ... /p:Nullable=enable
/p:TreatWarningsAsErrors=true`) now performs a genuine recompile and surfaces pre-existing
CS86xx nullable-reference-type diagnostics that were previously masked. The
`UtilitiesCS/HelperClasses/` directory tree (43 `.cs` files, including the FileSystem,
ThemeHelpers, Logging, ToolTips, Windows Forms, CloningFunctions, BinaryFlags, WipUnfinished
sub-clusters and the root helper classes) carries such pre-existing nullable debt. These are
shared helpers consumed across module boundaries; their nullability annotations become contracts
that downstream epic features (Wave-1 clusters: outlook-folder-store, outlook-mailitem-item,
dialogs-misc, and the broader OutlookObjects, EmailIntelligence, and Dialogs work) consume.

This feature remediates that debt for the `HelperClasses/` tree only, using a per-file
`#nullable enable` opt-in. It is annotation and null-safety work exclusively. It introduces no
behavior change and no refactor.

## Behavior

Remediate the pre-existing nullable-reference-type debt across `UtilitiesCS/HelperClasses/`
using a per-file `#nullable enable` opt-in. The following are maintainer-mandated hard
constraints, not options; no alternative architecture is to be proposed or adopted:

- Add a `#nullable enable` pragma to each remediated file and bring that file to zero CS86xx
  diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none. No project-level or solution-level `<Nullable>`
  element may be introduced by this feature.
- Annotation and null-safety ONLY: nullable annotations (`?`), null guards, null-forgiving
  operators (`!`) only where justified, and null-flow corrections. No behavior changes, no
  refactors, no API redesign, no feature work.
- Keep public signatures behavior-compatible; annotate to reflect the actual runtime null
  behavior so the annotations serve as accurate downstream contracts.

Files that are not opted-in remain in an oblivious nullable context and are not cross-blocking.
This is the mechanism that lets each epic child merge independently without requiring the entire
epic (~2131 diagnostics across ~234 files) to be fixed first.

## Inputs / Outputs

- Inputs (files): the 43 `.cs` files under `UtilitiesCS/HelperClasses/` (recursive). The
  interfaces implemented by the FileSystem cluster (`IFileInfo`, `IDirectoryInfo`,
  `IFileSystemInfo`) live under `UtilitiesCS/Interfaces/IHelperClasses/` and are OUT of scope;
  they receive no pragma and remain oblivious.
- Outputs (source changes): a `#nullable enable` pragma plus annotation/null-safety edits on each
  in-scope file that emits CS86xx; no new files, no removed files, no project-file edits.
- Config keys and defaults: none introduced. `UtilitiesCS.csproj` remains without a `<Nullable>`
  element.
- Versioning or backward-compatibility constraints: public member signatures must remain
  behavior-compatible. Nullability annotations added to public members become cross-module
  contracts consumed by Wave-1 dependents; they must reflect actual null behavior rather than
  change it.

## API / CLI Surface

This feature exposes no new commands or CLI. The "surface" is the set of nullability annotations
applied to public members of the shared helpers. These annotations ARE the contract consumed
outside `HelperClasses/`.

Top cross-module-contract files (annotate deliberately; preserve current runtime behavior):

- **FilePathHelper** (`UtilitiesCS` root namespace) — widely consumed; has a Newtonsoft
  converter. String properties split into two contract groups: `FilePath`/`FolderPath`/`FileName`
  default to `""` (non-null); `FileStemSeed`/`FileStemSuffix`/`FileStem`/`FileExtension` are
  null-by-design sentinels (nullable). Getting this split correct is the crux of the file.
- **Initializer** (`UtilitiesCS`) — `SetAndSave`/`GetOrLoad`/`Load` generic helpers used across
  modules; `ref T`/`default(T)` returns require deliberate `T?` / `[return: MaybeNull]`
  decisions that ripple to every caller.
- **FileSystem wrapper/adapter set** — `DirectoryInfoWrapper`, `FileInfoWrapper`,
  `FileSystemInfoWrapper`, `PhysicalDirectoryInfoAdapter`, `PhysicalFileInfoAdapter`,
  `MyFileSystemInfo`. Because the implemented interfaces are out of scope (oblivious), annotate
  implementations to match current behavior using `!` at the `Parent`/`Root`/`Directory`/
  `DirectoryName` boundaries (see Constraints & Risks).
- **TraceUtility** extension methods (`GetMyMethodNames`, `GetMyTraceString`, `GetCallerMethod`,
  `GetAssembly`) — consumed by ReflectionHelper and FilePathHelper; several return-nullable
  decisions.
- **PrettyPrint** (`PrettyPrinters`, `UtilitiesCS`) — `ToFormattedText`/`PrettyText`/`ToMarkdown`
  formatting extensions used broadly.

Narrower public surface (self-documenting nullability): ReflectionHelper, ParamArray,
ShellUtilitiesStatic.GetFileIcon (XML doc already declares nullable). Internal-only, low
contract sensitivity: PhysicalFileInfoAdapter and PhysicalDirectoryInfoAdapter (`internal
sealed`, constructed via public wrappers), ComStreamWrapper, DebugTextWriter, and the WinForms
visual helpers.

Contracts and validation rules: annotations must express the null behavior that already occurs
at runtime. Where a member currently throws on a null input (adapter root boundaries), the
behavior-preserving annotation is non-nullable with `!` at the throwing call site, not a nullable
contract change.

## Data & State

This feature introduces no data flow, storage, persistence, caching, migration, or backfill
changes. Edits are confined to compile-time nullability annotations and null-flow corrections in
source. Runtime data transformations and invariants are unchanged by design; the "no behavior
change" constraint means observable state transitions before and after remediation are identical.

## Constraints & Risks

The following mechanics flags are carried verbatim in substance from the research findings and
govern execution:

1. **Pragma-only verification command (do NOT use `/p:Nullable=enable`).** Local and CI
   verification of the opted-in files must use the pragma-only build
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`, relying on each file's own `#nullable enable` pragma. It must
   NOT add `/p:Nullable=enable`, which would enable nullable project-wide and surface the whole
   epic's ~2131 CS86xx diagnostics across ~234 files as false failures unrelated to issue #364.
   This is a deliberate, documented deviation from the stock CLAUDE.md / `.claude/rules/csharp.md`
   type-check command for this child only; it must NOT be resolved by editing `.claude/rules/*`.
2. **FileSystem adapter root boundaries — `!` is correct, latent throw is FLAGGED not fixed.** At
   `PhysicalDirectoryInfoAdapter.Parent`/`Root` and `PhysicalFileInfoAdapter.Directory` (and the
   `DirectoryInfoWrapper`/`FileInfoWrapper` equivalents), a BCL `DirectoryInfo?`/`FileInfo?`
   (null at the filesystem root) is passed into a `*Wrapper` ctor that throws
   `ArgumentNullException` on null. The wrapped interfaces are out of scope (oblivious), so a
   behavior-preserving `!` (with a short `// why` comment) is the correct annotation. The latent
   "root throws" design question is FLAGGED for a possible future issue, not fixed here.
3. **`DvgForm.Designer.cs` must not be pragma-annotated or hand-edited.** Default handling: leave
   the generated Designer file non-opted-in (oblivious). Because `#nullable enable` is
   lexical/per-file, the Designer file's members produce no CS8618/CS8625 and do not cross-block
   the opted-in hand-written `DvgForm.cs`. The epic listing `DvgForm.Designer.cs` in scope
   conflicts with the "do not touch Designer files" convention; this conflict is FLAGGED to the
   maintainer. Fallback only if the maintainer requires all files opted-in: annotate the generated
   field as `private IContainer? components = null;` (annotation-only, no behavior change) and
   still avoid touching `InitializeComponent`.
4. **`PrettyPrint.cs` (677 lines) exceeds the repo 500-line limit — PRE-EXISTING.** This is a
   pre-existing condition. Annotation-only work adds a pragma and annotations and cannot bring the
   file under 500 without a refactor, which is outside annotation-only scope. Do NOT split it;
   FLAG it as a known pre-existing policy exception, do not fix it. (`FilePathHelper.cs` at 494
   lines is near the limit; if the added pragma plus annotations pushes it over 500, that is an
   annotation-driven breach to flag rather than trigger a refactor.)
5. **Rules-vs-convention conflict (flagged at epic level, not resolved here).**
   `.claude/rules/csharp.md` documents forcing `/p:Nullable=enable` globally, which conflicts with
   the per-file opt-in convention. This is flagged at the epic level (capstone child); it is not
   resolved in this feature and no `.claude/rules/*` file is edited.

Additional constraints and risks:

- Follow the repo C# toolchain order (csharpier -> msbuild analyzers/codestyle -> msbuild
  type-check -> vstest with coverage). For this child the type-check stage uses the pragma-only
  form in item (1), not the stock `/p:Nullable=enable` form. Any test work uses MSTest + Moq +
  FluentAssertions.
- Annotations become cross-module contracts; incorrect annotations could propagate incorrect null
  assumptions to Wave-1 dependents (outlook-folder-store, outlook-mailitem-item, dialogs-misc).
- Contract decisions on unconstrained-generic returns (Initializer `GetOrLoad`/`Load` returning
  `default(T)`; ObjectCopier `Clone<T>`) are deliberate contract choices (`T?` /
  `[return: MaybeNull]`), not mechanical fixes, and change the annotated public contract consumed
  downstream.
- Preserve the `PhysicalFileInfoAdapter` injectable-delegate seam exactly (the `_appendText`/
  `_openByMode`/`_openByModeAndAccess`/`_openWrite` fields, both constructors, and the `?? throw`
  guards). This seam exists to keep `PhysicalFileSystemAdapters_Tests` deterministic on shared
  files; perturbing it risks reintroducing known shared-file flakiness.
- `Theme.cs` and `Theme.Rendering.cs` are two files of one partial `Theme` type and must be
  opted-in together to avoid inconsistent field-null-state analysis across the two parts.

## Implementation Strategy

- Implementation scope: add `#nullable enable` to each in-scope `HelperClasses/` file that emits
  CS86xx and apply annotation/null-safety edits to reach zero CS86xx per file under the
  pragma-only build. No new classes, functions, or commands; no dependency changes; no
  logging/telemetry additions; no project-file edits.
- Phasing: the research identifies an 8-batch sequence, foundational/low-risk clusters first and
  cross-module/high-contract files last. Batches are subdirectory-cohesive and independently
  reviewable; each opts in its files and reaches zero CS86xx for those files under the pragma-only
  verification. The batches (scope, not fine-grained sequencing) are:
  1. Root pure/simple helpers (GenericBitwise, MergeSortImplementations, ObjectSize, ParamArray,
     SimpleRegex, Tokenizer, SegmentStopWatch).
  2. Logging (DebugTextLogger, DebugTextWriter, VerboseLogger, TraceUtility) — settle
     TraceUtility's extension-method contracts before its consumers.
  3. CloningFunctions + reflection (DeepCompare, ObjectCopier, DispatchUtility, ReflectionHelper).
  4. FileSystem wrappers/adapters (FileSystemInfoWrapper, DirectoryInfoWrapper, FileInfoWrapper,
     PhysicalDirectoryInfoAdapter, PhysicalFileInfoAdapter, MyFileSystemInfo) — reviewed together
     for the shared BCL-null/oblivious-interface `!` decision.
  5. COM/P-Invoke + Form/Designer special cases (ShellUtilities, ShellUtilitiesStatic,
     SysImageListHelper, ComStreamWrapper, DvgForm.cs, DvgForm.Designer.cs handling per the
     Designer rule).
  6. Windows Forms cluster (ControlPosition, ControlResizer, ImageHelper, MouseDownFilter,
     OlvExtension, ScreenHelper, TableLayoutHelper).
  7. ThemeHelpers + ToolTips (SystemThemeDetector, Theme.cs, Theme.Rendering.cs, ThemeControlGroup,
     QfcTipsDetails, TipsController) — keep Theme.cs and Theme.Rendering.cs together.
  8. High-contract finish (Initializer, FilePathHelper, PrettyPrint) — the highest-risk,
     highest-contract-sensitivity files, done last.
- Verification per batch: build with the pragma-only command to capture a per-batch CS86xx
  baseline, then drive to zero; run that batch's corresponding `UtilitiesCS.Test/HelperClasses/`
  tests and require them green and behavior-identical.
- Rollout: no feature flags or staged deploys. Each batch is additive; non-opted-in files remain
  oblivious until remediated.

## Definition of Done

- [x] Every `.cs` file under `UtilitiesCS/HelperClasses/` that emits CS86xx carries a
  `#nullable enable` pragma and compiles with zero nullable (CS86xx) diagnostics under the
  per-file pragma with `/p:TreatWarningsAsErrors=true`.
- [x] No project-level or solution-level `<Nullable>` element is introduced; `UtilitiesCS.csproj`
  retains none.
- [x] Changes are annotation/null-safety only: no behavior change, no API/signature semantics
  change, no refactor beyond nullable annotation.
- [x] All existing MSTest tests for UtilitiesCS still pass; no coverage regression on changed
  lines.
- [x] The full C# toolchain (csharpier -> analyzer/codestyle build -> nullable/
  TreatWarningsAsErrors build -> vstest with coverage) passes on the final pass, using the
  pragma-only type-check command (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without
  `/p:Nullable=enable`) for this child.
- [x] The `PhysicalFileInfoAdapter` injectable-delegate seam is preserved exactly (seam fields,
  both constructors, and `?? throw` guards unchanged).
- [x] FileSystem adapter root-boundary annotations use behavior-preserving `!` (with a `// why`
  comment); the latent root-throws behavior is flagged, not fixed.
- [x] `DvgForm.Designer.cs` handling and the epic-scope conflict are documented; the Designer
  file is not hand-edited (default: left non-opted-in).
- [x] The `PrettyPrint.cs` 500-line pre-existing violation is flagged (not fixed) in the feature
  docs.

## Seeded Test Conditions (from potential)

- [x] Existing MSTest suite for UtilitiesCS still passes post-annotation.
- [x] No coverage regression on changed lines.
- [x] Nullable gate passes for the opted-in files using the pragma-only build
  (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable`).
