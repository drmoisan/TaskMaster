# `utilitiescs-nullable-outlook-mailitem-item` — User Story

- Issue: #371
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T22-20
- Epic: utilitiescs-nullable-remediation (child, Wave 1)
- Work Mode: full-feature

## Story Statement

- As the repository maintainer, I want the `UtilitiesCS/OutlookObjects/{MailItem,Item,
  Conversation,Attachment,Table}/` nullable-reference-type debt remediated under a per-file
  `#nullable enable` opt-in, so that the repaired CI nullable gate can be genuinely enforced on
  these files without cross-blocking non-opted-in files elsewhere in the epic.
- As a downstream consumer of this cluster's public contracts (QuickFiler, TaskVisualization,
  TaskMaster, Tags, and ToDoModel), I want `MailItemHelper`, `OutlookItemFlaggable*`,
  `OlTableExtensions`, `ConvHelper`, and `AttachmentHelper` annotated to reflect their actual null
  behavior, so that I consume accurate nullability contracts and do not inherit false null
  assumptions, even while my own files remain nullable-oblivious.

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild`) now performs a genuine
recompile and surfaces pre-existing CS86xx nullable-reference-type diagnostics that were
previously masked. The Outlook item-adapter cluster under `UtilitiesCS/OutlookObjects/` —
`MailItem/`, `Item/`, `Conversation/`, `Attachment/`, and `Table/` (30 `.cs` files) — carries such
pre-existing nullable debt. This is the Wave-1 child that remediates that cluster only,
consuming the already-annotated cross-module contracts produced by the Wave-0 Extensions (#363)
and HelperClasses (#364) children.

A global force-enable of nullable would make no epic child independently mergeable until the
entire epic's debt were fixed at once. The per-file opt-in lets this child be remediated and
merged on its own while non-opted-in files, in this cluster and elsewhere, stay oblivious and
non-cross-blocking. This is annotation and null-safety work only: no behavior changes, no
refactors, no API redesign.

## Personas & Scenarios

- Persona: Repository maintainer (drmoisan)
  - Who: owner of the nullable-remediation epic and the CI nullable gate.
  - Cares about: a genuinely enforceable nullable gate that does not permanently block future
    PRs; a per-file opt-in architecture that keeps each epic child independently mergeable;
    upstream #363/#364 contracts being consumed correctly rather than re-decided.
  - Constraints: annotation and null-safety only — no behavior changes, no refactors, no API
    redesign; no project- or solution-level `<Nullable>` element; no editing of
    `.claude/rules/*`; the three partial-class groups (`MailItemHelper`, `ConvHelper`,
    `OlTableExtensions`) must each be opted in as one unit.
  - Goals and frustrations: wants the Outlook item-adapter debt cleared under the confirmed
    architecture, and wants pre-existing conditions (the `OutlookItem.cs` 500-line breach, the
    `dynamic item` hazard in `OlToDoTable.cs`, the non-standard interior pragma in
    `MailItemHelper.Html.cs`, the two dead files) surfaced accurately rather than silently
    "fixed" with a refactor.

- Persona: Downstream Wave-1 and cross-project consumer
  - Who: QuickFiler, TaskVisualization, TaskMaster, Tags, and ToDoModel code that calls
    `MailItemHelper`, `OutlookItemFlaggable*`, `OlTableExtensions`, `ConvHelper`, and
    `AttachmentHelper` public members, and the Wave-2 CI capstone child that will eventually
    reconcile the rules-vs-convention conflict.
  - Cares about: consuming this cluster's public members with nullability annotations that match
    actual runtime behavior, so their own compilation and null-flow analysis (present or future)
    are correct even while remaining nullable-oblivious today.
  - Constraints: must not have to re-derive or work around inaccurate annotations; must not see
    new behavior or new forced test obligations around COM-bound members they depend on.
  - Goals and frustrations: an incorrect annotation on `MailItemHelper.Sender`/`.FolderInfo`/
    `.AttachmentsInfo`/`.Globals` or on an `OutlookItem`-family `TryGet<T>` site would propagate a
    false null-state assumption into every one of these consuming projects.

- Scenario: Remediating and verifying an Outlook item-adapter batch
  - Who is acting: the executor delivering issue #371, batch by batch (research Section 6: A/B/C
    trivial+leaf, D OutlookItem family, E Attachment, F ItemInfo/EmailDetails, G MailItemHelper,
    H ConvHelper, I OlTableExtensions).
  - Trigger: the repaired nullable gate now surfaces pre-existing CS86xx in this cluster, and the
    upstream #363/#364 contracts this cluster consumes (`Initializer.GetOrLoad`,
    `FilePathHelper`, `PrettyPrint.PrettyText`, `LazyExtension`, `IEnumerableExtensions.ForEach`,
    `ArrayExtensions.ToStringArray`/`To2D`) are available to build against.
  - Steps: opt each batch's files in with `#nullable enable` (keeping the three partial-class
    groups intact as single units); apply annotation/null-safety edits; build with the
    pragma-only command (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without
    `/p:Nullable=enable`) to drive the opted-in files to zero CS86xx; run the batch's
    `UtilitiesCS.Test/OutlookObjects/` tests (including legacy-named duplicate test files) and
    require them green and behavior-identical.
  - Obstacles/decisions: normalize `MailItemHelper.Html.cs`'s interior `#nullable enable`/
    `disable` region to a whole-file pragma; decide the four `MailItemHelper` lazy-property
    nullable contracts (`Sender`, `FolderInfo`, `AttachmentsInfo`, `Globals`) without adding new
    `??` guards; decide each `OutlookItem`-family `TryGet<T>`/`TryCall<T>` unconstrained-generic
    return; flag (do not split) `OutlookItem.cs`'s 503-line breach; flag (do not fix) the
    `dynamic item` hazard in `OlToDoTable.cs`; annotate COM-bound classes for null-safety without
    forcing new tests around them; do not add `/p:Nullable=enable` to the verification command.
  - Expected outcome: every in-scope file that emitted CS86xx is opted-in and clean under the
    pragma-only gate, with no behavior change and no coverage regression on changed lines, the
    upstream contracts correctly consumed, and all flagged pre-existing conditions documented for
    the maintainer.

## Acceptance Criteria

- [x] AC1: Every `.cs` file under
  `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}` that emits CS86xx
  carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file
  pragma with `/p:TreatWarningsAsErrors=true`.
- [x] AC2: No project-level or solution-level `<Nullable>` element is introduced;
  `UtilitiesCS.csproj` retains none.
- [x] AC3: No behavior change; existing MSTest tests for UtilitiesCS still pass.
- [x] AC4: No coverage regression on changed lines.
- [x] AC5: Public signatures of remediated members remain behavior-compatible; nullability
  annotations reflect actual null behavior and correctly consume the upstream #363/#364
  contracts.
- [x] AC6: Outlook Interop event-handler classes that directly depend on
  `Microsoft.Office.Interop.Outlook` types without an injectable seam are annotated for
  null-safety but respect the repo COM/VSTO coverage exemption (no new tests forced around
  COM-bound code).

## Non-Goals

- No behavior changes, refactors, API redesign, or feature work of any kind. Nullable annotation
  and null-safety only.
- No project-level or solution-level `<Nullable>` element as an enforcement mechanism; no
  `/p:Nullable=enable` in this feature's verification command.
- No editing of `.claude/rules/*` to resolve the rules-vs-convention conflict (flagged at the
  epic level, deferred to the Wave-2 CI capstone child).
- No splitting of `OutlookItem.cs` to meet the 500-line limit (pre-existing condition, flagged
  not fixed).
- No conversion of `dynamic item` in `OlToDoTable.EnsureItemValues` to a typed access pattern
  (would be a behavior-risk refactor; flagged not fixed).
- No new tests forced around COM-bound members lacking an injectable seam; no changes to the
  existing `EmailDetailsWrapper`/`IEmailDetailsWrapper`,
  `OutlookItemTry`/`OutlookItemTryGet`/`OutlookItemFlaggableTry` seams beyond annotation.
- No changes to files outside `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,
  Table}/`, including the out-of-scope `IOutlookItem`/`IOutlookItemFlaggable` interfaces under
  `UtilitiesCS/Interfaces/IOutlookObjects/`.
