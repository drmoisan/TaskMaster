# utilitiescs-nullable-outlook-mailitem-item — Spec

- **Issue:** #371
- **Parent:** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-20
- **Status:** Draft
- **Version:** 0.1
- **Work Mode:** full-feature

## Overview

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` so it performs a genuine
recompile rather than a silently-skipped incremental build, now surfaces pre-existing
nullable-reference-type (CS86xx) diagnostics that were previously masked. The Outlook
item-adapter cluster under `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,
Table}/` (30 `.cs` files: MailItem 12, Item 9, Conversation 2, Attachment 2, Table 5) carries
such pre-existing nullable debt. This is the Wave-1 child that remediates that cluster only,
using the epic's per-file `#nullable enable` opt-in architecture, and it directly consumes the
already-annotated cross-module contracts produced by the Wave-0 children `utilitiescs-nullable-
extensions` (#363) and `utilitiescs-nullable-helperclasses` (#364).

This is annotation and null-safety work only. There are no behavior changes, no refactors, no
API redesign, and no feature work.

## Behavior

Each remediated `.cs` file receives a per-file `#nullable enable` pragma and is brought to zero
CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`. Remediation applies nullable
annotations (`?`), unconstrained `T?` returns and `out` parameters, null-flow corrections, and
null-forgiving operators (`!`) only where justified. Existing null guards already present in the
files remain as-is; new runtime guards are not introduced solely to satisfy the nullable gate.

Non-remediated files (everything outside this cluster, and any oblivious file this feature does
not touch) remain non-opted-in and are not cross-blocked by this work, per the epic's per-file
pragma architecture.

Three partial-class groups in this cluster must be opted in together, as one unit, because their
members share cross-file field state or cross-file private-method call graphs (see Constraints &
Risks and Implementation Strategy):

- `MailItemHelper` (5 files: `MailItemHelper.cs`, `MailItemHelper.Html.cs`,
  `MailItemHelper.Loading.cs`, `MailItemHelper.Properties.cs`, `MailItemHelper.Serialization.cs`).
- `ConvHelper` (2 files: `ConversationHelper.cs`, `ConversationHelper.Formatting.cs`).
- `OlTableExtensions` (4 files: `OlTableExtensions.cs`, `OlTableExtensions.Etl.cs`,
  `OlTableExtensions.RowTransforms.cs`, `OlTableExtensions.TableAccess.cs`).

Public method signatures remain behavior-compatible: an existing caller that compiles today
continues to compile and behaves identically. Annotation choices reflect the true null behavior
of each method so that the resulting signatures are safe contracts for the downstream consumers
identified in Section 4 of the research (QuickFiler, TaskVisualization, TaskMaster, Tags,
ToDoModel).

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none. This is a library-internal source change with no
  runtime inputs. The relevant "input" is the 30 in-scope `.cs` files themselves.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced.
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: public method signatures of remediated
  members remain behavior-compatible. The observable change is limited to nullability
  annotations, which are additive contract metadata rather than a source- or binary-breaking
  behavior change.

## API / CLI Surface

There is no CLI surface and no new API. This is a library-internal change. The relevant "API
surface" is the set of nullability annotations applied to the public members of `MailItemHelper`,
`OutlookItemFlaggable`/`OutlookItem`/`OutlookItemTry`/`OutlookItemTryGet`/
`OutlookItemFlaggableTry`/`OutlookItemExtensions`, `ConvHelper`, `OlTableExtensions`,
`AttachmentHelper`/`AttachmentSerializable`, `ItemInfo`, `EmailDetails`/`EmailDetailsWrapper`,
and the smaller leaf files (`CidImageResolver`, `MailResolution`, `MailItemExtensions`,
`OlItemPseudoInterface`, `OlItemSummary`, `OlToDoTable`).

- Example invocations with expected outputs (concise): not applicable; no command or CLI flag is
  added. No `/p:Nullable=enable` global flag is introduced into any verification command (see
  Toolchain Note).
- Contracts and validation rules:
  - Public member signatures remain behavior-compatible; only nullability annotations change (for
    example, `MailItemHelper.Sender`/`.FolderInfo`/`.AttachmentsInfo`/`.Globals` — the four
    lazy-backed properties whose getters already return `null` via `?.Value` with no `??`
    fallback — become explicitly nullable-typed public members: `IApplicationGlobals?`,
    `IFolderWrapper?`, `IRecipientInfo?`, `IAttachment[]?`).
  - Annotation choices reflect the member's actual null behavior. Because `MailItemHelper`,
    `OutlookItemFlaggable*`, `OlTableExtensions`, `ConvHelper`, and `AttachmentHelper` are
    confirmed external contracts consumed by QuickFiler, TaskVisualization, TaskMaster, Tags, and
    ToDoModel (research Section 4), an incorrect annotation could propagate a false null-state
    assumption to those callers even though those callers remain nullable-oblivious themselves.
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
    `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) from
    `System.Diagnostics.CodeAnalysis` are not available or polyfilled on net481 and must not be
    used or added (see Constraints & Risks).
  - Unconstrained-generic `TryGet<T>`/`TryCall<T>` returns in the `OutlookItem`/`OutlookItemTry`/
    `OutlookItemTryGet`/`OutlookItemExtensions` family require an explicit `T?` (unconstrained)
    return-type decision at each site; this is a deliberate contract choice, not a mechanical fix,
    analogous to the #364 `Initializer.GetOrLoad` decision this cluster consumes.

## Data & State

This feature introduces no data flow, storage, persistence, caching, migration, or backfill
changes. Edits are confined to compile-time nullability annotations and null-flow corrections in
source. Runtime data transformations and invariants are unchanged by design.

- Data transformations and invariants: none changed. This is annotation-only.
- Caching or persistence details: none.
- Migration or backfill requirements (if any): none. No project-level or solution-level
  `<Nullable>` element is introduced into `UtilitiesCS.csproj`; the project has no `<Nullable>`
  element today and must keep none. Enforcement is per-file pragma only.

## Constraints & Risks

- **Pragma-only verification (do NOT use `/p:Nullable=enable`).** Local and CI verification of
  the opted-in files must use `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug
  /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`, relying on each file's own
  `#nullable enable` pragma. It must NOT add `/p:Nullable=enable`, which would enable nullable
  project-wide and surface the whole epic's debt as false failures unrelated to issue #371. The
  rules-vs-convention conflict between this pragma-only convention and any stock global-nullable
  guidance is deferred to the Wave-2 CI capstone child; do not edit `.claude/rules/*` to resolve
  it here.
- **`UtilitiesCS.csproj` keeps no `<Nullable>` element.** No project-level or solution-level
  `<Nullable>` element may be introduced by this feature.
- **Annotation and null-safety ONLY.** No behavior changes, no refactors, no API redesign, no
  feature work. Public signatures stay behavior-compatible; annotations reflect actual runtime
  null behavior so they are safe downstream contracts.
- **Target framework net481, C# 12.** Nullable post-condition attributes from
  `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
  `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) are not
  polyfilled on this target and must not be used or added. Likewise, `init`, positional `record`,
  and `record struct` are not usable on net481 (no `IsExternalInit`; they fail CS0518) and must
  not be introduced as part of this remediation.
- **COM/VSTO coverage exemption.** All 30 in-scope files except `CidImageResolver.cs` reference
  `Microsoft.Office.Interop.Outlook` types directly or (in the case of `MailItemHelper.Html.cs`)
  inherit COM-boundedness through the shared `MailItemHelper` partial class's `_item` field.
  COM-bound Outlook interop adapter/event-handler classes without an injectable seam are
  coverage-exempt per repo policy: annotate for null-safety only, and do NOT force new tests
  around COM-bound code (e.g., `OutlookItem.GetPropertyValue<T>`'s late-bound `InvokeMember` path,
  `AttachmentSerializable.GetBytes`'s `File.ReadAllBytes`/`SaveAsFile` COM calls). Existing
  injectable seams — `EmailDetailsWrapper`/`IEmailDetailsWrapper` over the static `EmailDetails`
  extension methods, and the `OutlookItemTry`/`OutlookItemTryGet`/`OutlookItemFlaggableTry`
  try/catch-swallowing decorators over `IOutlookItem`/`IOutlookItemFlaggable` — must be preserved
  exactly as-is; no new seam is added and no existing seam is removed. `CidImageResolver.cs` is
  not COM-bound (confirmed: no `Microsoft.Office.Interop.Outlook` reference) and is held to normal
  (non-exempt) coverage expectations, consistent with its dedicated `CidImageResolverTests.cs`.
- **Partial-class groups must be opted in together.** `MailItemHelper.*` (5 files), `ConvHelper`
  (`ConversationHelper.*`, 2 files), and `OlTableExtensions.*` (4 files) each share cross-file
  field state or cross-file private-method call graphs; opting in only some files of a group would
  produce inconsistent CS8618/definite-assignment diagnostics in whichever files are enabled
  first. Each group must reach zero CS86xx as one unit.
- **Pre-existing conditions, flagged not fixed:**
  - `OutlookItem.cs` is 503 lines, exceeding the repo 500-line file-size limit. This is
    pre-existing; annotation-only work (a pragma line plus `?`/`!` annotations) will push the file
    further over 500, not under it. Do not split the file — that would be a refactor and is out of
    scope. Flag for a future issue.
  - `dynamic item = itemObj;` in `OlToDoTable.EnsureItemValues` is invisible to nullable-flow
    analysis; the compiler cannot verify null-safety through the `dynamic` member-access call
    sites (`item.PropertyAccessor`, `item.EntryID`, `item.Save()`). Converting `dynamic` to a
    typed access pattern would be a behavior-risk refactor and is out of scope. Flag only.
  - `MailItemHelper.Html.cs` contains a pre-existing interior `#nullable enable`/`#nullable
    disable` region (lines 107-144) wrapping only the `_emailHeader` field/property, inconsistent
    with the epic's whole-file-pragma convention. Normalizing this to a whole-file
    `#nullable enable` (removing the interior `#nullable disable`) is in-scope remediation work
    for this file, not a flag-only item, because the file must still reach the group's
    whole-file-pragma convention.
  - `CaptureEmailAddressesModule2.cs` and `ItemComparer.cs` are commented-out dead files with no
    live code. Remediation is a no-op pragma addition; zero live diagnostics are possible.
- **Cross-module contract sensitivity.** `MailItemHelper`, `OutlookItemFlaggable*`,
  `OlTableExtensions`, `ConvHelper`, and `AttachmentHelper` public members are confirmed external
  contracts consumed by QuickFiler, TaskVisualization, TaskMaster, Tags, and ToDoModel (research
  Section 4). Nullable choices on these types' public surface are visible to that consuming code
  even though the consumers themselves remain nullable-oblivious and are not cross-blocked.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to each of
  the 30 in-scope files and bring each to zero CS86xx under the pragma; normalize
  `MailItemHelper.Html.cs`'s interior `#nullable enable`/`disable` region to a whole-file pragma;
  make the deliberate `T?`/nullable-property contract decisions flagged above (the four
  `MailItemHelper` properties; the `OutlookItem`-family `TryGet<T>`/`TryCall<T>` unconstrained
  generics). No project or solution file changes.
- New classes/functions/commands to add or update: none. No new types, methods, commands, or
  files are added; only nullability annotations on existing members change.
- **Consumed upstream contracts (from #363 and #364; must land or be already-landed before this
  cluster can be verified CS86xx-clean against them):**
  - `Initializer.GetOrLoad` (#364) — called by `MailItemHelper.Loading.cs.ResolveMail`
    (`Initializer.GetOrLoad(ref _item, () => (MailItem)olNs.GetItemFromID(EntryId, StoreId),
    strict, _entryId, _storeId)`). The #364 `ref T`/`default(T)` contract decision flows directly
    into `ResolveMail`'s return type and its callers' null-checks.
  - `FilePathHelper` (#364) — `AttachmentHelper.cs` constructs and holds `FilePathHelper`
    instances (`_filePathHelperSave`, `_filePathHelperSaveAlt`); `AttachmentHelper.FilePathSave`/
    `.FolderPathSave` forward directly to `FilePathHelperSave.FilePath`/`.FolderPath` and must
    inherit the non-nullable `""`-default contract as-is, without adding a conflicting nullable
    annotation.
  - `PrettyPrint.PrettyText` (#364, batch 8, last/highest-contract-sensitivity) — called from
    `ConversationHelper.cs`/`OlTableExtensions.*` (`Debug.WriteLine(df.PrettyText())`). This
    cluster cannot be fully verified CS86xx-clean until all eight #364 batches, including the
    last one, are merged upstream.
  - `LazyExtension`'s `.ToLazy()`/`.ToLazyValue()`/`.ToLazyTry()` (#363, Batch B) — used
    extensively across `MailItemHelper.cs`/`.Loading.cs`/`.Properties.cs`/`.Serialization.cs`.
  - `IEnumerableExtensions.ForEach` (#363, Batch C) — consumed by
    `ConversationHelper.Formatting.cs` (`ConversationColumnSchemas.ForEach(schema =>
    table.Columns.Add(schema))`).
  - `ArrayExtensions.ToStringArray`/`SliceRow`/`To2D` (#363, Batch C) — consumed by
    `OlTableExtensions.TableAccess.cs.EnumerateTable` (`ToStringArray()`, `SliceRow(i)`) and
    `OlTableExtensions.Etl.cs.EtlByRow`/`EtlByRowAsync` (`To2D()`). This is a second, previously
    undocumented consumer of #363 Batch C with the identical ordering dependency the #363 spec
    already names for `DfMLNet`/`DfDeedle`: Batch C must land before this cluster's
    `OlTableExtensions` batch.
  - None of the above requires new runtime guards in this cluster: the upstream contracts, once
    landed, are read-only extension-method call sites. Where an upstream signature becomes
    nullable (e.g., `ToStringArray()` returning `string?[,]`), the correction is a compatible
    nullable local/parameter type in this cluster's own signatures where the value flows to a
    public member, not a new guard.
- **Batch grouping (from research; scope, not fine-grained sequencing — task-by-task sequencing
  belongs to the atomic plan):**
  - Batch A — trivial / dead-code confirm-clean: `CaptureEmailAddressesModule2.cs`,
    `ItemComparer.cs`.
  - Batch B — pure/host-neutral leaf: `CidImageResolver.cs`.
  - Batch C — small COM-bound leaves, no partial-class entanglement, no upstream dependency:
    `MailResolution.cs`, `MailItemExtensions.cs`, `OlItemPseudoInterface.cs`,
    `OlItemSummary.cs`, `OlToDoTable.cs`.
  - Batch D — `OutlookItem` reflection-wrapper family (reviewed together for consistent
    `TryGet<T>`/`default(T)`/`out T` unconstrained-generic annotation choices): `OutlookItem.cs`,
    `OutlookItemExtensions.cs`, `OutlookItemFlaggable.cs`, `OutlookItemTry.cs`,
    `OutlookItemTryGet.cs`, `OutlookItemFlaggableTry.cs`.
  - Batch E — Attachment cluster: `AttachmentSerializable.cs` then `AttachmentHelper.cs`.
    Depends on #364's `FilePathHelper` contract.
  - Batch F — `ItemInfo.cs`, `EmailDetails.cs`, `EmailDetailsWrapper.cs`.
  - Batch G — `MailItemHelper` partial-class group (5 files, highest-contract-sensitivity in this
    cluster; must stay intact as one batch). Depends on Batches D/E/F plus #364
    `Initializer.GetOrLoad` and #363 `LazyExtension`.
  - Batch H — `ConvHelper` partial-class group (2 files). Depends on #363 Batch C
    (`IEnumerableExtensions.ForEach`), #364 Batch 8 (`PrettyPrint.PrettyText`), and scheduling
    after Batch G.
  - Batch I — `OlTableExtensions` partial-class group (4 files, largest and most
    cross-file-coupled group in this cluster). Depends on #363 Batch C
    (`ToStringArray`/`To2D`) and on Batch H (`using static UtilitiesCS.ConvHelper;`).
  - Ordering constraint rationale: A/B/C (trivial+leaf) precede D (OutlookItem family), which
    precedes E (Attachment, needs #364), which precedes F (ItemInfo/EmailDetails), which precedes
    G (MailItemHelper, needs D/E/F plus #363/#364), which precedes H (ConvHelper, needs #363/#364
    and overlaps G's Outlook-type domain), which precedes I (OlTableExtensions, needs #363 Batch C
    and H via the `using static ConvHelper` compile-time dependency). This dependency-graph order
    was chosen over a directory-listing order specifically to avoid re-touching already-annotated
    files (see research Section 6, "Rejected alternatives").
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable. Each remediated
  file is independently mergeable because non-opted-in files remain null-oblivious and are not
  cross-blocking under the per-file pragma architecture; the batches above are additive scope
  groupings, and the atomic plan owns the concrete task-by-task execution order within them.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format -> lint -> type-check -> test)

Acceptance criteria (from `issue.md`, mapped here for traceability):

- [ ] AC1: Every `.cs` file under
  `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}` that emits CS86xx
  carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file
  pragma with `/p:TreatWarningsAsErrors=true`.
- [ ] AC2: No project-level or solution-level `<Nullable>` element is introduced;
  `UtilitiesCS.csproj` retains none.
- [ ] AC3: No behavior change; existing MSTest tests for UtilitiesCS still pass.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of remediated members remain behavior-compatible; nullability
  annotations reflect actual null behavior and correctly consume the upstream #363/#364
  contracts.
- [ ] AC6: Outlook Interop event-handler classes that directly depend on
  `Microsoft.Office.Interop.Outlook` types without an injectable seam are annotated for
  null-safety but respect the repo COM/VSTO coverage exemption (no new tests forced around
  COM-bound code).

## Seeded Test Conditions (from potential)

- [ ] Existing `UtilitiesCS.Test/OutlookObjects/` suite continues to pass with no behavior
  change (both current-layout and legacy-named duplicate test files identified in research
  Section 8 must stay green).
- [ ] Changed-line coverage does not regress relative to baseline (prefer annotation and
  justified `!` over new runtime guards to avoid introducing uncovered executable lines).
- [ ] The pragma-driven nullable gate
  (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true`) produces zero CS86xx for the remediated files, without
  `/p:Nullable=enable` globally.
- [ ] No new tests are forced around COM-bound members lacking an injectable seam (e.g.,
  `OutlookItem.GetPropertyValue<T>`'s `InvokeMember` path); `CidImageResolver.cs` remains held to
  normal, non-exempt coverage expectations.

## Toolchain Note

Run the repo C# toolchain in CLAUDE.md order:

1. `csharpier .` (adding a pragma line and `?`/`!` annotations reformats; run before each build).
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
   /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style).
3. Nullable verification via the per-file pragma gate:
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`. Under `TreatWarningsAsErrors`, any CS86xx in a pragma-enabled
   file becomes an error while non-opted files stay silent.
4. `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage`.

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag forces
nullable project-wide and surfaces the full pre-existing epic debt across the solution, drowning
this child's signal. That global-flag-versus-per-file-pragma mismatch is the rules-versus-
convention conflict deferred to the Wave-2 CI capstone child; resolving it, and any edit to
`.claude/rules/*`, is out of scope here.
