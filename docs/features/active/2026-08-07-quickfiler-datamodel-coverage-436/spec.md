# quickfiler-datamodel-coverage — Spec

- **Issue:** [#436](https://github.com/drmoisan/TaskMaster/issues/436)
- **Parent epic issue:** [#136](https://github.com/drmoisan/TaskMaster/issues/136) (`quickfiler-per-file-coverage`, child F5, wave 1)
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Upstream dependency:** F1 `quickfiler-coverage-denominator-and-exemption-ledger`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** `full-feature` — `spec.md` and `user-story.md` are the authoritative acceptance-criteria sources.

---

## 1. Overview

Parent epic issue #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj`
to reach at least 80% line coverage or to sit on an explicitly ratified exemption ledger. This child
covers the QuickFiler data-model cluster: the `QfcDatamodel` partial-class family, the `EfcDataModel`
class, and the `IQfcDatamodel` contract.

The cluster is the queue and data backbone behind the QuickFiler filing loop. Two conditions block the
epic goal today:

1. `QuickFiler/Controllers/QfcDatamodel.cs` carries `[ExcludeFromCodeCoverage]` at line 25 (verified on
   disk). Under the epic's ratified policy reconciliation (`epic.md` § Shared Design 1), that attribute
   is unratified until F1's ledger either justifies it against the irreducible-remainder standard or
   marks it for removal. The qualifier "without an injectable seam" in the `CLAUDE.md` § UT2 exemption
   is a live obligation, not a standing permission.
2. The attribute is **type-scoped**, so it currently removes all three `QfcDatamodel` partials from the
   coverage denominator simultaneously. Measured coverage for those three files today is not 0% — the
   files are **absent** from the Cobertura report (§5.1). Genuine gaps were therefore unknown before the
   five per-file research artifacts in `research/`.

The delivered outcome is per-file line coverage at or above 80% for every `testable` file in the
cluster, achieved through injectable seams rather than exemptions, with no behavior change to
observable QuickFiler flows.

**AC2 is answerable YES.** All four production-file research artifacts independently concluded that no
irreducible remainder exists in this cluster. Removing the attribute is safe, and it must be sequenced
as the **last** production task of the feature (§7).

---

## 2. Scope

Five production files, 1,283 lines total. Line counts re-verified against the worktree on 2026-08-08.

| File | Lines (verified) | `[ExcludeFromCodeCoverage]` | Production edit planned |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 496 | **yes**, line 25 (type-scoped) | Delete verified-dead members; add S1–S4 call sites; split lifecycle members into a new partial |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 177 | no (suppressed by the type-scoped attribute) | **None.** No seam, no split, no attribute |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 154 | no (suppressed by the type-scoped attribute) | Two call-site edits for S5/S6; optional severable extraction (§11.2) |
| `QuickFiler/Controllers/EfcDataModel.cs` | 397 | no | Add E1–E5 seams; extract three pure helpers (E6) |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 59 | no | **None.** Zero production edits (§4) |

New production files this feature may add (each requires an explicit `<Compile Include>` entry, §9.1):

| File | Status | Rationale |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.Construction.cs` | **Required** | Single DI surface for S1–S7; also resolves the `QfcDatamodel.cs` size pressure (§10) |
| `QuickFiler/Controllers/QfcEmailFrameShaper.cs` | **Severable** (§11.2) | Host-neutral pure shaping logic; design quality and the epic Non-Goal, not AC2 |
| `QuickFiler/Controllers/EfcDataModel.Seams.cs` | **Contingency only** | Used only if `EfcDataModel.cs` edits exceed ~480 lines (§10) |

Each new production file increases the epic's compiled-file denominator from 121. The F16 capstone must
account for the delta; this spec records it rather than assuming F16 will discover it.

Tests live under `QuickFiler.Test/`, mirroring the production tree.

Per the #136 per-file mandate, research and planning proceed one production file at a time: a separate
research artifact per production file (complete — five artifacts under `research/`), a separate
atomic-plan phase per production file, and **each individual test case as its own atomic task**.

---

## 3. Correction notes

Two statements carried by the promotion-time drafts of `issue.md` are factually wrong. Both were
research-verified and independently re-verified. They are recorded here so that planners and reviewers
do not act on the superseded text.

### 3.1 Correction — `IQfcDatamodel` consumer map (F11 is not a consumer)

An earlier draft named sibling **F11** (`QfcCollectionController`) as an `IQfcDatamodel` consumer.
**That is false.** A grep of `QuickFiler/Controllers/QfcCollectionController.cs` (2,349 lines —
essentially all of F11) for `DataModel|Datamodel|_datamodel` returns **zero matches**. There is no path
from F11 to this interface: F11's other file mentions `SloStack<IMovedMailInfo>`
(`Interfaces/IQfcCollectionController.cs:50`), but that instance is obtained independently at
`QfcFormController.cs:49` from `_globals.AF.MovedMails`, not via `IQfcDatamodel.MovedItems`.

The verified consumer set is **F7 plus two consumers that reach the contract indirectly through
`IQfcHomeController.DataModel`** — which is exactly why a grep for `IQfcDatamodel` does not surface
them. Full map in §4.1. Source: `research/2026-08-08T00-43-iqfcdatamodel.md` §3.3.

### 3.2 Correction — `QfcDatamodel.FrameBuilding.cs` is not WinForms

An earlier draft stated that `QfcDatamodel.FrameBuilding.cs` "interacts with WinForms layout".
**That is false.** All 154 lines were read: the using block (lines 1–7) is `System`, `System.Linq`,
`System.Threading.Tasks`, `Deedle`, `Microsoft.Office.Interop.Outlook`, `QuickFiler.Interfaces`,
`UtilitiesCS`. There is **no `System.Windows.Forms` import and no WinForms type**, fully qualified or
otherwise, anywhere in the file. "Frame" denotes `Deedle.Frame<int, string>`.

Consequences, all load-bearing:

- **The STA last-resort clause (`epic.md` § Shared Design 3) does not apply anywhere in this child.**
  The STA-member count is zero, and **no `*.StaTests.cs` file is introduced.** `QuickFiler.Test` still
  has none, and this feature does not create the first one. `EfcDataModel.cs` likewise constructs no
  WinForms control; its only UI touch is `MessageBox.Show`, removed from the test path by seam E4.
- Reviewers must not look for a WinForms seam in this cluster. The real host boundary is Outlook COM
  reached through two `DfDeedle` static calls (§6.3).
- `QuickFiler/Helper Classes/TlpCellSnapShot.cs` (a genuine `System.Windows.Forms` `TableLayoutPanel`
  helper, owned by **F4**) is unrelated and must not be pulled into F5.

Source: `research/2026-08-08T00-43-qfcdatamodel-framebuilding.md` §0.2.

---

## 4. Cross-child contract analysis

### 4.1 Verified consumer map

| Consumer site | Members used | Owning child |
| --- | --- | --- |
| `QfcHomeController.cs:162,170` (loader delegate return types), `:254`, `:261`, `:284-289`, `:299-304`, `:390`, `:428-429`; `QfcHomeController.Iteration.cs:15,21-24,62-65,66`; `Controllers/IQfcHomeController.cs:11` | `InitEmailQueue`, `InitEmailQueueAsync`, both `DequeueNextItemGroupAsync` overloads, `DequeueNextItemGroup`, `Complete`, `Cleanup`, and the type itself | **F7** |
| `QfcQueue.cs:476-479` — `await _homeController.DataModel.DequeueNextItemGroupAsync(...)`, result dereferenced at `:480` as `items.Count` with no null guard | `DequeueNextItemGroupAsync(int,int)` | **F2** |
| `QfcFormController.EventHandlers.cs:196` — `if (!_parent.DataModel.Complete)` | `Complete` | **F6** |
| — | none | **F11 — not a consumer** (§3.1) |

`QfcHomeController.cs:299-304` is the sole production consumer of the issue-#424 four-argument overload.
`QfcHomeController.cs:163` and `:173` are the two bind sites the seams S3/S4 must preserve verbatim.

### 4.2 No cross-child breaking change exists — recorded as a positive finding

All nine `IQfcDatamodel` members and all seven `SortOptionsEnum` members keep byte-identical shapes.
`QuickFiler/Interfaces/IQfcDatamodel.cs` receives **zero production edits**. Every seam in §6 is an
`internal` instance property, an additive `internal` constructor, or an additive `internal static`
overload on a **concrete** class — none is an interface member, and the interface has no constructor and
no static member, so S3/S4 cannot touch it even in principle. Every consumer call site in §4.1 compiles
and behaves unchanged.

### 4.3 The real basis of the "do not widen the interface" constraint

The constraint does **not** rest on compile breakage. `IQfcDatamodel` has exactly **one** compiled
implementer — `QfcDatamodel` (`QfcDatamodel.cs:26`), which F5 itself owns. (`QuickFiler/Notes/notes_interfaces.cs:26`
declares a stale duplicate but is not in `QuickFiler.csproj`; `EfcDataModel` does not implement the
interface — independently verified.) Adding an interface member **would compile**, with a one-file fix
inside F5's own scope. A planner told only "it would break siblings" would discover the claim is false
and might then proceed.

The prohibition rests instead on a **silent cross-child test hazard**: there are **19 `Mock<IQfcDatamodel>`
sites across six F7-owned test files** (`QfcHomeControllerTests.cs`, `QfcHomeControllerRunAsyncTests.cs`,
`QfcHomeControllerRunAsyncHighConfidenceTests.cs`, `QfcHomeControllerPropertyTests.cs`,
`QfcHomeControllerIterationTests.cs`, `QfcHomeControllerIssue218Tests.cs`). Moq generates the proxy at
runtime, so those files keep compiling — but every mock returns `default` for a new member (`null` for a
reference type, `false` for `bool`, `null` rather than `Task.CompletedTask` for a `Task`). Any F7 test
whose production path reaches the new member fails with an NRE pointing at **F7's** file, not at F5's
change. That is precisely the fan-in failure the epic's disjoint-file-set design exists to avoid.
Secondary grounds: `IQfcDatamodel` is `public` and is re-exposed through `public interface IQfcHomeController`,
so widening it to enable a unit test inverts the seam hierarchy.

### 4.4 Second cross-child contract — `SortOptionsEnum` with F2 (previously undocumented)

`SortOptionsEnum` is declared in F5's `QuickFiler/Interfaces/IQfcDatamodel.cs:12-22`, interpreted in
exactly one place — `EmailSorter.GetSortKey` (`QuickFiler/Controllers/EmailSorter.cs:45-48`, **F2**) — and
consumed from exactly one production site, F5's own `QfcDatamodel.FrameBuilding.cs:114`
(`new EmailSorter(SortOptionsEnum.Default)`), reached unconditionally from both frame-build paths
(`FrameBuilding.cs:24` and `:63`). It is a contract with **F2**, not with F7 or F11.

`Default = 42` decomposes to `2 + 8 + 32` =
`TriageImportantFirst | DateRecentFirst | ConversationUniqueOnly` (verified). `EmailSorter.GetSortKey`
has exactly one predicate, the conjunction at `EmailSorter.cs:45-48`
(`HasFlag(TriageImportantFirst) && HasFlag(DateRecentFirst)`); when it fails, every row's key becomes
`-1` (`EmailSorter.cs:67`) and `SortTriageDate` degenerates into an unconditional reversal of Deedle's
tie order. **Nothing pins this today** — `EmailSorterTests.cs:19` asserts only a tautology, and the tests
at `:62` and `:78` construct `TriageImportantFirst | DateRecentFirst` explicitly instead of using
`Default`. Three characterization tests close that gap (§11.1); they earn zero line-coverage credit and
this spec says so plainly.

**Coordination requirement.** If F2 restructures `GetSortKey`'s flag predicate while raising
`EmailSorter.cs` coverage, F5's frame sort order changes. F5 pins the enum side; F2 owns the predicate
side. **Neither side may change `Default`.**

### 4.5 Hard planner constraints

F5 **MUST NOT**:

- **R1.** Modify, rename, reorder, or remove any of the nine `IQfcDatamodel` members, or change any
  parameter type, parameter name, or return type.
- **R2.** Add a member to `IQfcDatamodel` (grounds in §4.3).
- **R3.** Change `SortOptionsEnum` in any way, specifically not `Default = 42`. Note the trap: `Default`
  is declared **first**, so a member appended after `ConversationUniqueOnly = 32` without an explicit
  initializer receives the implicit value `33`, setting bits 0 and 5 and silently corrupting `HasFlag`
  for every consumer.
- **R4.** Introduce an additive overload using **optional parameters** on any member bound by
  `QfcHomeController.cs:163`/`:173`. Use **distinct arity**, so binding depends on arity rather than on
  an overload-resolution tie-break.
- **R5.** Add `[ExcludeFromCodeCoverage]` to `IQfcDatamodel.cs` at type or member level (§5.3).
- **R6.** Modify any sibling-owned file to accommodate this one — specifically `QfcQueue.cs`,
  `EmailSorter.cs`, `EmailSorterTests.cs`, `QfcStreamingDequeueConfidenceGate.cs`,
  `QfcHighConfidencePreFilter.cs`, `QfcRemainingQueueAdmission.cs` (F2);
  `QfcFormController.EventHandlers.cs` (F6); `QfcHomeController*.cs`, `IQfcHomeController.cs`,
  `IFilerHomeController.cs` and the six `QfcHomeController*Tests.cs` files (F7);
  `QfcCollectionController.cs` (F11); `QuickFiler/Helper Classes/**` (F4); `UtilitiesCS/**`; or
  `coverage.config` and any shared build property file (F1).

---

## 5. Coverage baseline and ledger classification

### 5.1 Baseline — the three `QfcDatamodel` partials are absent, not 0%

The committed full-suite Cobertura report at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
contains **no `<class>` element** for `QuickFiler.Controllers.QfcDatamodel`; the single textual hit is an
unrelated `set_DataModel` signature at report line 21903. Its companion delta note records the same
conclusion for `QfcDatamodel.QueueProcessing.cs`. Verified three times independently.

Read-derived baselines, all **medium confidence** hand counts to be replaced by F1's harness output:

| File | Read-derived baseline | Projected after this feature |
| --- | --- | --- |
| `QfcDatamodel.cs` | ~40 of ~230 executable lines reached; absent from the report | above the floor with margin |
| `QfcDatamodel.QueueProcessing.cs` | ~27 of ~47 (~57%); absent from the report | ~100% (no irreducible remainder) |
| `QfcDatamodel.FrameBuilding.cs` | 6 of ~55 (~11%); absent from the report | ~100%; residual is two null-coalescing branch arms, zero lines |
| `EfcDataModel.cs` | **measured**: `line-rate="0.55618"`, `branch-rate="0.457143"`; hand count 124/250 lines and 18/46 branch arms; 126 lines uncovered | > 90% |
| `IQfcDatamodel.cs` | not measurable (§5.2) | not measurable |

The two `EfcDataModel` figures do not reconcile because the tool's class-level rate uses a denominator
that omits the async state-machine methods present in the `<lines>` union. **Both are far below 80%, so
the gap conclusion is robust either way.** No projected or read-derived figure in this spec may be used
to close an acceptance criterion.

### 5.2 `IQfcDatamodel.cs` needs a third ledger category — `not-measurable (declaration-only)`

The file is **absent** from the Cobertura report, not 0%. Verified structurally, not assumed: there is
no `<class>` element for `QuickFiler.Interfaces.IQfcDatamodel`, none for
`QuickFiler.Interfaces.SortOptionsEnum`, none for **any** QuickFiler interface type, and none for **any**
enum anywhere across 110,849 instrumented lines. The assembly is genuinely instrumented — for example
`QuickFiler.Controllers.EmailSorter` is present with `line-rate="0.9591836734693877"` — so the absence is
a property of the declaration kind, not of the assembly. The instrumenter emits `<class>` elements only
for types with method bodies.

Consequence: any per-file report derived by grouping Cobertura output on `filename` will have **no row**
for this file. It can neither pass nor fail an 80% line-coverage gate. No test can raise a percentage the
tool never emits — a test that reads `SortOptionsEnum.Default` executes IL in the *test* assembly and in
`System.Enum.HasFlag`, never in this file.

**Request to F1.** Record `QuickFiler/Interfaces/IQfcDatamodel.cs` as **`not-measurable (declaration-only)`**
— a distinct **third** category, not a variant of `ratified-exempt`. Basis:
`.claude/rules/general-unit-test.md` § Coverage Requirements, which names "C# interface-only files"
explicitly and whose mechanism is *omission from measurement*, removing nothing from either the numerator
or the denominator.

**Epic-scale consequence, flagged deliberately.** Per `epic.md` § Scope, **~24 of the 121 compiled files
are interface-only declarations**. Collapsing them into `ratified-exempt` would inflate the exemption
ledger with ~24 files that need no maintainer ratification and carry no irreducible-remainder argument,
obscuring the exemptions that do. A harness that seeds its file list from `QuickFiler.csproj` and defaults
missing files to 0% would produce ~24 permanent, unfixable gate failures and would make F16's gate
unclosable. F1 should treat "present in `QuickFiler.csproj` but absent from the Cobertura `filename` set"
as a classification signal to reconcile against the ledger, not as 0%.

### 5.3 `[ExcludeFromCodeCoverage]` must **not** be added to `IQfcDatamodel.cs`

The file references `MailItem` in its signatures (via `using Microsoft.Office.Interop.Outlook;` at line 6,
in signatures at lines 26, 40, 46, 49, 50), which makes a § UT2 COM/VSTO classification tempting. It does
not qualify: it is not a class, it is not an event handler, and it has no behavior to seam. Recording it
under § UT2 would invoke the maintainer-ratification requirement for no reason and would invite an
attribute that excludes nothing. The attribute is prohibited here by R5.

---

## 6. Seam inventory

All seams are additive `internal` members with a null-means-production default, matching the house style
already established at `QfcDatamodel.cs:112` (`TimeProvider`) and `:114-128` (`RemainingEmailLoader`).
Null-coalescing at the call site rather than a property initializer, because every existing datamodel test
constructs via `FormatterServices.GetUninitializedObject`, which bypasses initializers.

**Numbering note.** The research artifacts assigned overlapping labels: the `QfcDatamodel.cs` artifact
called its `NewMailEx` contingency "S5", while the FrameBuilding artifact used "S5"/"S6" for the
data-frame providers, and the `EfcDataModel` artifact used "S1"–"S7" for a disjoint set. This spec
renumbers to a single namespace: **S1–S7** for the `QfcDatamodel` partial family (the `NewMailEx`
contingency becomes **S7**), and **E1–E7** for `EfcDataModel`.

### 6.1 `QfcDatamodel` family

| ID | Seam | Declared in | Consumed at | Purpose |
| --- | --- | --- | --- | --- |
| **S1** | `internal IFolderScoringService ScoringService { get; set; }` — interface seam; the interface already exists at `QfcHighConfidencePreFilter.cs:130`, so no new abstraction | `QfcDatamodel.Construction.cs` | `QfcDatamodel.cs:368` as `ScoringService ?? new FolderScoringService()` | Removes the hard-coded COM-bound `new FolderScoringService()`. Also unblocks `QueueProcessing.cs:119`, where the same method group is the gate's scorer |
| **S2** | `internal Func<string, DialogResult> MessageBoxInvoker { get; set; }` | `QfcDatamodel.Construction.cs` | `QfcDatamodel.cs:309` | Opens the empty-frame branch of the remaining-email loader without a modal dialog. Declared as an **instance** property, not the mutable `static` of the `DfDeedle.MessageBoxInvoker` precedent, so tests stay independent |
| **S3** | Additive `internal QfcDatamodel(IApplicationGlobals, CancellationToken, Func<Explorer, Frame<int,string>> frameBuilder)`; the existing public 2-arg ctor becomes a `: this(..., null)` chain | `QfcDatamodel.Construction.cs` | ctor body; defaults to the `InitDf` method group | Lets a test construct a model without building a Deedle frame. Statement order preserved exactly. Public ctor arity, parameter types, order, and accessibility unchanged, so `QfcHomeController.cs:163` binds identically |
| **S4** | Additive `internal static LoadAsync(..., Func<QfcDatamodel, Explorer, ProgressTracker, Task> dataFrameInitializer)`; the public 4-arg `LoadAsync` retained verbatim as a delegating wrapper | `QfcDatamodel.Construction.cs` | `QfcDatamodel.cs:69-71` | A parameter rather than a property, because `LoadAsync` constructs the model itself. `QfcHomeController.cs:173` binds identically. **Distinct arity, no optional parameter** (R4) |
| **S5** | `internal Func<Explorer, Frame<int,string>> EmailDataInViewProvider { get; set; }` | `QfcDatamodel.Construction.cs` | `QfcDatamodel.FrameBuilding.cs:15` | Bypasses the `DfDeedle` modal-dialog blocker (§6.3) for the synchronous frame build |
| **S6** | `internal Func<Explorer, CancellationToken, CancellationTokenSource, ProgressTracker, Task<Frame<int,string>>> EmailDataInViewAsyncProvider { get; set; }` | `QfcDatamodel.Construction.cs` | `QfcDatamodel.FrameBuilding.cs:82-89` | Same for the asynchronous path. **`.ConfigureAwait(false)` at line 89 must be preserved verbatim** (§8.3) |
| **S7** | `internal Action<Outlook.Application> NewMailSubscriber` / `NewMailUnsubscriber` — **contingency only** | `QfcDatamodel.Construction.cs` | ctor and `Cleanup()` | Adopt **only if** Moq cannot proxy the `[ComEventInterface]` add/remove accessors of `Application.NewMailEx` (unverified without building — **INFERRED**). Adopting it pre-emptively is unnecessary indirection |

`QfcDatamodel.QueueProcessing.cs` needs **no seam of its own**. It is the only file in the cluster that
dereferences **zero** Outlook COM members — `MailItem` appears only as a generic type argument and as a
list element. Its single transitive COM reach is the scorer method group at line 119, which S1 resolves.
A gate-factory seam was considered and rejected: replacing the real `QfcStreamingDequeueConfidenceGate`
in tests would *weaken* rather than pin the ordering invariants, all of which are observable through the
real gate with `FakeTimeProvider` plus S1.

### 6.2 `EfcDataModel`

| ID | Seam | Consumed at | Purpose |
| --- | --- | --- | --- |
| **E1** | `internal IFolderSearchHandler FolderSearchOverride { get; set; }`, with `private IFolderSearchHandler FolderSearchHandler => FolderSearchOverride ?? _folderHelper;` — reuses the existing `UtilitiesCS.IFolderSearchHandler`, which `FolderPredictor` already implements | `FindMatches`, line 381 | Interface seam (rank 1) with **no change to `UtilitiesCS`**. `FolderHelper` stays typed `FolderPredictor`, so `EfcFormController.cs:492,771,891,1037` are unaffected |
| **E2** | `internal Func<IApplicationGlobals, FolderPredictor> FolderPredictorEmptyFactory` and `internal Func<IApplicationGlobals, object, FolderPredictor.InitOptions, Task<FolderPredictor>> FolderPredictorInitializer` | `InitFolderHandlerAsync`, lines 179–212 | Two delegates rather than the three-field `QfcItemController` shape, because `EfcDataModel` always follows construction with `InitAsync`; folding init into the delegate avoids calling the non-mockable `InitAsync` on a test double. All three branches and the `Task.Run(..., Token)` wrapper stay behaviorally identical |
| **E3** | `internal Func<EmailFilerConfig, IList<MailItemHelper>, Task<bool>> SortAsyncAction`; `internal Func<EmailFilerConfig, Task> OpenOlFolderAction`; `internal Func<EmailFilerConfig, Task> OpenFsFolderAction` | `MoveToFolderAsync` 292–293; `OpenOlFolderAsync` 313–314; `OpenFsFolderAsync` 331–332 | Each seam carries the **whole construct-and-invoke step** — see §6.4 |
| **E4** | `internal Action<string> MoveFailureMessageAction { get; set; } = text => MessageBox.Show(text);` — copied verbatim from `EfcHomeController.ExecuteMoves.cs:22-23` | `MoveToFolderAsync(MAPIFolder, …)`, line 358 | Removes the modal dialog from the test path |
| **E5** | `internal Action<MailItem> RefreshSuggestionsAction { get; set; }` | line 392 | `IFolderSearchHandler` does not declare `RefreshSuggestions`, and adding it would edit `UtilitiesCS` (R6) |
| **E6** | Pure-function extraction to `internal static`: `BuildSearchPattern(string)`, `ShouldSaveAttachments(string, bool)`, `StripAncestorPrefix(string, string)` | lines 376–379, 271, 344–348 | Converts branch-heavy inline expressions into directly testable units; the cheapest coverage in the file |
| **E7** | No seam — construction technique only: `FormatterServices.GetUninitializedObject` plus reflection assignment of `_globals`, `_mail`, `_conversationResolver`, `_folderHelper`, `_token` | `FolderHelper`, `MailInfo`, `PackageItems`, `TryGetFirstInSelection` | These members have no structural blocker; the only obstacle was that no test constructed the type without running the COM-touching public constructor |

### 6.3 Verified seam blocker justifying S5/S6

Both `DfDeedle` entry points funnel into `AddQfcColumns` (`UtilitiesCS/Extensions/DfDeedle.cs:296-316`) —
directly for the synchronous path (`DfDeedle.cs:92`) and via `AddQfcColumnsAsync` for the async path
(`DfDeedle.cs:164, 318-343`). Traced end to end with a loose `Mock<MAPIFolder>`:

1. `AddQfcColumns` → `EnsureTriageColumnExists(folder)` (`DfDeedle.cs:298, 345`).
2. → `HasUserDefinedProperty(folder, "Triage")` (`DfDeedle.cs:352, 392`). A loose mock returns `null` for
   `folder.UserDefinedProperties`, so line 394 returns `false`.
3. **`MessageBoxInvoker(...)` shows a real modal Yes/No dialog** (`DfDeedle.cs:357`).
4. Not `Yes` → `return false` (line 365).
5. **A second real modal dialog** (`DfDeedle.cs:300`), then line 307 throws `InvalidOperationException`.

The seam that would neutralize this is `DfDeedle.MessageBoxInvoker`, declared **`internal static`**
(`DfDeedle.cs:54-60`). `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants `InternalsVisibleTo` to
`DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` only — **`QuickFiler.Test` is not
granted access and cannot set it.** Even if it could, step 5 still throws, so the happy path remains
unreachable without a live Outlook folder carrying a `Triage` user-defined property. Two modal dialogs in
a unit test are a unit-test-policy violation, not merely inconvenient.

A rank-1 interface seam (`IEmailDataFrameSource` plus a `DfDeedleEmailDataFrameSource` implementation) was
considered and rejected: it **relocates** the uncovered lines into a new adapter that is itself untestable
and would immediately need its own exemption — a Blocking finding under `epic.md` § Shared Design 1 — and
it adds another compiled file to the epic denominator. The delegate keeps the permanently-untestable
residual to **two null-coalescing fallback arms** with zero additional source lines.

### 6.4 A seam that looks right but is wrong — recorded so it is not re-attempted

Copying `QfcItemController`'s `Func<EmailFilerConfig, EmailFiler>` factory seam
(`QfcItemController.Initialization.cs:389-397`) for `EfcDataModel` **does not work**.
`EmailFiler.SortAsync(IList<MailItemHelper>)`, `OpenOlFolderAsync()`, and `OpenFileSystemFolderAsync()`
are **non-virtual** (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:128, 89, 109`), so
Moq cannot intercept them on a factory-produced instance, and `SortAsync`'s body immediately dereferences
`MailHelpers.FirstOrDefault().FolderInfo.OlFolder` cast to `Folder` (COM). `OpenFileSystemFolderAsync`
additionally calls `Process.Start("explorer.exe", …)`, an external process prohibited by § UT4.

Therefore **seam E3 must carry the whole construct-and-invoke step**, not just construction. Making the
three `EmailFiler` methods `virtual` was also rejected: it modifies a `UtilitiesCS` public behavior surface
for one consumer's tests, and `SortAsync`'s body would still be the thing under test.

### 6.5 Non-structural blocker notes

- `SortEmail.Cleanup_Files()` (`EfcDataModel.cs:294`) remains in the covered path. It mutates static
  `YesNoToAllResponse` fields but is proven non-throwing (`UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:175`)
  and resets rather than accumulates, so test independence holds. Record it in the policy audit as a known
  static touch rather than leaving it silent.
- `Mock<ProgressTracker>` requires explicit configuration: `Increment` and `SpawnChild` return
  `ProgressTracker`, so an unconfigured loose mock returns null and the call site NREs. All four members
  used are `virtual` (`ProgressTracker.cs:109,121,141,218`). A bare `new ProgressTracker(cts)` without
  `Initialize()` NREs inside `Report(double)`, so tests must **mock**, not construct.
- `Mock<MailItem>` must be **loose**. The confidence gate logs `mailItem.Subject` / `mailItem.EntryID`; a
  `MockBehavior.Strict` mail item throws inside the gate and produces a confusing failure.

---

## 7. Phase ordering and the attribute-removal sequence

Per the #136 per-file mandate, one atomic-plan phase per production file, each test case its own atomic
task. The following edges are **binding**, not stylistic:

| Edge | Reason |
| --- | --- |
| **P1 (`QfcDatamodel.cs`) → P2 (`QueueProcessing.cs`)** | 12 of P2's 39 tests require seam S1, which P1 declares in `QfcDatamodel.Construction.cs`. Fallback if the plan must invert the order: a lower-tier delegate seam `internal Func<MailItem, CancellationToken, Task<long>> RemainingItemScorer` declared in `QfcDatamodel.Construction.cs` — a deliberate, documented downgrade from an interface seam |
| **P1 → P3 (`FrameBuilding.cs`)** | S5/S6 are declared in `QfcDatamodel.Construction.cs` to keep one DI surface. P3's tests cannot compile until that file lands. Fallback: declare S5/S6 locally in `FrameBuilding.cs` and have P1 relocate them, at the cost of a move diff |
| **P3 → attribute removal** | The attribute at `QfcDatamodel.cs:25` is type-scoped, so removing it admits all three partials into the denominator in one commit. Removing it before P3's seams land would report a large regression on `FrameBuilding.cs` |
| **Attribute removal is the LAST production task of the feature** | Consequence of the two edges above. It must follow P1, P2 and P3 |
| **P4 (`EfcDataModel.cs`) — independent** | No dependency on P1–P3 in either direction |
| **P5 (`IQfcDatamodel.cs`) — independent** | Zero production edits; its three characterization tests depend on nothing |

Coordination rule for the whole feature: **all DI seam declarations for the `QfcDatamodel` partial family
land in `QfcDatamodel.Construction.cs`**, not in the individual partials, so the type has one DI surface.

---

## 8. Determinism requirements

### 8.1 The clock seam is `TimeProvider` — no new abstraction

A search for `interface IClock` across all `*.cs` returns **no matches**. The repository has no clock
abstraction of its own and none is to be introduced. The established seam is `System.TimeProvider`:

- Production: `Microsoft.Bcl.TimeProvider` **10.0.10** is referenced by `QuickFiler/packages.config:19`,
  so `TimeProvider` and `TimeProviderTaskExtensions.Delay(this TimeProvider, TimeSpan, CancellationToken)`
  resolve under `net481`.
- Test: `Microsoft.Extensions.TimeProvider.Testing` is referenced by `QuickFiler.Test/packages.config:85`,
  supplying `FakeTimeProvider`, already used at `QfcDatamodelTests.cs:107,254,288` and
  `QfcDatamodelLivenessTests.cs:84`.
- The datamodel already exposes the seam: `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;`
  at `QfcDatamodel.cs:112`.

`Thread.Sleep`, `Task.Delay`, and real wall-clock waits are prohibited in tests. Where a `BackgroundWorker`
state transition must be observed, use the existing condition-driven bounded `SpinWait.SpinUntil` helper
(`QfcDatamodelLivenessTests.cs:54-57`), which returns as soon as the predicate holds — it is not a fixed
sleep.

### 8.2 The `TimeProvider` trap — a silent failure mode the plan must design against

`TimeProvider` at `QfcDatamodel.cs:112` is an **auto-property with an initializer**, so its assignment runs
in the instance constructor. The established test construction path
`FormatterServices.GetUninitializedObject(typeof(QfcDatamodel))` (`QfcDatamodelTests.cs:231`) skips
constructors, leaving the backing field **null** — exactly as it leaves `_masterQueue` and `_moveMonitor`
null.

The consequence is **silent**: with a null `TimeProvider`, `QfcStreamingDequeueConfidenceGate`'s
constructor falls back to `TimeProvider.System` (gate line 69) rather than throwing. A high-confidence test
that forgets `model.TimeProvider = fake` therefore runs against the **real wall clock and a real
12-second deadline — and still appears to pass.**

**Mandate.** Every test touching a timing-dependent path assigns `model.TimeProvider` explicitly in Arrange,
and each such test class routes construction through one shared local
`CreateModelWithFakeClock(out FakeTimeProvider fake)` helper so the assignment cannot be forgotten. This
must be restated in each affected atomic task, not just here.

Contrast, recorded because it changes how a failure presents: in `FrameBuilding.cs` the same omission fails
**loudly** — `TimeProvider.Delay` at line 43 is an extension method, so a null provider throws immediately.
`WaitForQueue` (`QueueProcessing.cs:173`) likewise fails loudly. The silent trap is specific to paths that
hand the provider to the confidence gate.

### 8.3 Review rule — `ConfigureAwait(false)` discipline

The synchronous entry point `DequeueNextItemGroup` blocks on the asynchronous gate via
`.GetAwaiter().GetResult()` (`QueueProcessing.cs:138`). It is deadlock-safe today only because the awaited
chain uses `.ConfigureAwait(false)` throughout and MSTest installs no `SynchronizationContext`. A
deterministic unit test for this property would have to install a single-threaded context and block its
only pumping thread — i.e. hang on failure — so **no test is proposed**.

Instead this is a standing review rule: **any change to `ScoreRemainingQueueMailItemAsync`
(`QfcDatamodel.cs:371`), to the gate's await chain, or to `FrameBuilding.cs:50` and `:89` must preserve
`ConfigureAwait(false)` verbatim.** Dropping it deadlocks the Outlook UI thread.

---

## 9. Build and evidence mechanics

### 9.1 Global build constraint — explicit `<Compile Include>` item lists

Both `QuickFiler/QuickFiler.csproj` (verified at `:312-315` and `:361`) and
`QuickFiler.Test/QuickFiler.Test.csproj` (verified at `:90-145`) are legacy non-SDK projects with explicit
item lists. **A new `.cs` file silently will not build without a csproj entry** — no error, no warning, the
file is simply not compiled and its tests never run. Every atomic task that creates a file must add the
matching entry in the same task.

### 9.2 Evidence locations

All evidence artifacts belong under
`docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/evidence/<kind>/` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Per-file coverage results go to
`evidence/qa-gates/`. Writing to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any
other non-canonical path is a policy violation.

### 9.3 Toolchain order

Fixed and non-negotiable: `csharpier .` → analyzer msbuild → nullable msbuild →
`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. Restart from step 1 on any failure or any
file change.

---

## 10. File-size analysis

The 500-line ceiling is a hard constraint (AC3). `QfcDatamodel.cs` at 496 lines has **4 lines of headroom**,
which is the binding case.

**Resolution: ~123 lines of verified-dead code are deleted.** All are verified unreferenced by repo-wide
search:

| Removed | Member | Evidence of deadness |
| --- | --- | --- |
| `log` static readonly field, lines 96–99 (4) | duplicate of `logger` | A `\blog\b` search across `QfcDatamodel*.cs` returns only the declaration |
| `Worker_RunWorkerCompleted`, lines 212–236 (25) | shows `MessageBox` on cancel/error | Only reference is the **commented-out** subscription at line 170 |
| `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)`, lines 378–417 (40) | synchronous predecessor | Only references are `nameof(...)` uses inside its own body and in log strings (lines 333, 339, 405, 411); no call site |
| `LoadRemainingEmailsToQueueAsync(BackgroundWorker, CancellationToken)`, lines 418–466 (49) | obsolete-API predecessor carrying `#pragma warning disable CS0618` | Only other reference is the commented-out line 185. The method-group assignments at lines 40 and 51 bind to the **one-argument** overload, because `RemainingEmailLoader` is `Func<CancellationToken, Task<bool>>` |
| Empty `#region Linked List Locking`, lines 469–473 (5) | no members | — |
| **Total** | | **123** |

Two incidental cleanups fall out: two `MessageBox.Show` call sites disappear, and a `CS0618` suppression is
**removed** rather than added. These deletions are the only permanently-uncoverable lines in the file, and
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy directs refactoring over exclusion. The
decision is recorded here explicitly so review does not read it as scope creep.

Projected sizes:

| File | Now | After | Note |
| --- | --- | --- | --- |
| `QfcDatamodel.cs` | 496 | ~311 | 496 − 123 dead = 373; + ~68 seam lines = ~441; then the lifecycle-and-seams concern moves to the new partial |
| `QfcDatamodel.Construction.cs` | — | ~178 | New. Constructors, `LoadAsync`, `Cleanup`, and all DI seam declarations (S1–S7) |
| `QfcDatamodel.QueueProcessing.cs` | 177 | 177 | No production edit |
| `QfcDatamodel.FrameBuilding.cs` | 154 | ~128 with the §11.2 extraction, ~158 without | Both compliant |
| `QfcEmailFrameShaper.cs` | — | ~50 | New, only if §11.2 is taken |
| `EfcDataModel.cs` | 397 | ~435–450 | E1 ≈ 3, E2 ≈ 10, E3 ≈ 9, E4 ≈ 2, E5 ≈ 2, E6 ≈ +12 net. Contingency if final edits exceed ~480: add `partial` and move seam declarations to `EfcDataModel.Seams.cs` (precedent: `FolderPredictor.IFolderSearchHandler.cs:5-8`) |
| `IQfcDatamodel.cs` | 59 | 59 | No production edit |

Size must be measured after each atomic task in P4, not assumed at the end.

---

## 11. Test surface

### 11.1 Proposed test cases per production file

| Production file | Proposed cases | New test files |
| --- | --- | --- |
| `QfcDatamodel.cs` | 40 | `QfcDatamodelLifecycleTests.cs`, `QfcDatamodelWorkerTests.cs`, `QfcDatamodelInitEmailQueueTests.cs`, `QfcDatamodelRemainingLoadTests.cs`, `QfcDatamodelStateTests.cs` |
| `QfcDatamodel.QueueProcessing.cs` | 39 | `QfcDatamodelDequeueRoutingTests.cs`, `QfcDatamodelUnhookTests.cs`, `QfcDatamodelHighConfidenceDequeueTests.cs`, `QfcDatamodelWaitForQueueTests.cs` |
| `QfcDatamodel.FrameBuilding.cs` | 30 | `QfcEmailFrameShaperSortTests.cs`, `QfcEmailFrameShaperConversationTests.cs`, `QfcDatamodelInitDfTests.cs`, `QfcDatamodelInitDfAsyncTests.cs`, `QfcDatamodelEmailsInViewTests.cs` |
| `EfcDataModel.cs` | 45 | `EfcDataModel.TestSupport.cs` (support only), `EfcDataModelPureLogicTests.cs`, `EfcDataModelFolderHandlingTests.cs`, `EfcDataModelSelectionTests.cs`, `EfcDataModelMoveTests.cs`, plus 2 cases appended to the existing `EfcDataModelTests.cs` (409 lines, ~90 lines of headroom) |
| `IQfcDatamodel.cs` | 3 | `SortOptionsEnumTests.cs` — characterization of `Default = 42` against `EmailSorter`'s predicate. **Earns zero line-coverage credit for `IQfcDatamodel.cs`** (§5.2); justified by `CLAUDE.md` § UT2, "untested critical behavior is not acceptable even if the overall percentage looks good" |
| **Total** | **157** | subject to planner de-duplication |

**Arithmetic correction.** The delegation brief rolled these up as "~117"; its own components
(40 + 39 + 30 + 45 + 3) sum to **157**. The component figures are verified against the five research
artifacts; the 157 total supersedes the 117 rollup.

Several enumerated cases are marked *conditional* or *no line-coverage delta* in the research artifacts
(for example `UnhookDequeuedNodes_NullBatch`, and the `EfcDataModel` guard tests at cases 25, 44, 45).
Those are retained for § UT2 scenario completeness, or dropped if F1's harness shows the lines already
covered — the planner decides per case and records the decision.

Test files must live under `QuickFiler.Test/` mirroring the production tree. For `SortOptionsEnumTests.cs`
this points to `QuickFiler.Test/Interfaces/` per `.claude/rules/general-unit-test.md` § Test File Location;
the existing `MailItemActionsAdapterTests.cs` precedent flattens `Interfaces/` into `Controllers/`. Either
satisfies every other constraint; the rule-conformant location is recommended.

### 11.2 Severable scope — `QfcEmailFrameShaper` extraction

Extracting the pure shaping logic (`SortTriageDate`, `MostRecentByConversation`, and the three-step
filter → dedup → sort pipeline duplicated at `FrameBuilding.cs:18/21/24` and `:56/59/63`) into a new
host-neutral `internal static class QfcEmailFrameShaper` is **recommended** on three grounds:
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy ("extract all logic into host-neutral,
testable modules"); `epic.md` Non-Goals ("prefer host-neutral extraction that a future WebView2/Office.js
port can reuse"); and `CLAUDE.md` Design Principles 2 and 4.

**It is explicitly severable. AC2 does not depend on it.** The attribute-removal conclusion rests on
S5/S6 alone. If the plan's change budget is tight, sever it and retarget the affected test cases at the
public instance methods on an uninitialized model; every test scenario survives the severing unchanged.
Severing also removes a percentage-fragility risk: extraction shrinks `FrameBuilding.cs` to ~35
instrumented lines, where four uncovered lines is 89% and eight is 77%.

If taken, the extraction must preserve the pipeline order **filter → dedup → sort** exactly; one test case
exists specifically to pin that order and is the guardrail for the move.

---

## 12. Promote-to-issue register — do NOT fix in this child

AC7 forbids behavior change. Each item below is promoted to its own GitHub issue through the MCP promotion
lifecycle; prose in a feature folder disappears at merge. Where a test pins the current behavior, that is
characterization, not endorsement.

| # | Defect / observation | Evidence | Handling here |
| --- | --- | --- | --- |
| D1 | **`QfcDatamodel.Cleanup()` is not idempotent.** A second call NREs at line 80 (`_moveMonitor.UnhookAll()` after `_moveMonitor = null`) | `QfcDatamodel.cs:79-84` | No guard added; no test pins the NRE |
| D2 | **Null-vs-empty return asymmetry at `quantity <= 0`.** Normal mode: `TryTakeFirst(n)` returns `null` for `n < 1` (`LockingLinkedList.cs:403-406`), so `UnhookDequeuedNodes` returns `null`; high-confidence mode returns an empty list. `QfcHomeController.Iteration.cs:25` dereferences `listObjects.Count` with **no null guard**, so a zero or negative `ItemsPerIteration` NREs inside `IterateQueueAsync` | `QueueProcessing.cs:147-150`; gate lines 96–99; `Iteration.cs:21-25` | Tests pin the current shape. The fix would sit in F7's file |
| D3 | **Items can leave the master queue still hooked.** Below-threshold candidates discarded by the confidence gate are never unhooked, and a batch node surviving a shrink is returned to the caller unhooked. The stale `BeforeItemMove` callback then runs `_masterQueue.Remove(x)` (`QfcDatamodel.cs:358`) against a queue the item has already left, holding a COM reference for the process lifetime | gate lines 116, 138–141; `QueueProcessing.cs:31, 52, 154` | Two tests pin current behavior |
| D4 | **`EfcDataModel.PackageItems(bool)` has no caller repo-wide.** The only other hits are an unrelated `QfcItemController.PackageItems()` with a different signature and a commented-out line at `EfcHomeController.cs:437` | `EfcDataModel.cs:362-372` | **Covered** rather than deleted, as the conservative choice under AC7. Deletion is recorded as an explicit option: it would remove 7 lines from the denominator and two test cases |
| D5 | **Unreachable nested condition.** `FrameBuilding.cs:39` re-tests `if (!offline)` inside the block already guarded by line 36; its false arm can never execute | `FrameBuilding.cs:36,39` | One permanently-uncoverable **branch** arm; no line-coverage impact |
| D6 | **`throw e;` resets the stack trace** instead of a bare `throw;` | `FrameBuilding.cs:108`, and the same pattern in `QfcDatamodel.cs` (the remaining-email loader) | One issue covering both sites. The test asserts only exception type and message, so it passes before and after a future fix |
| D7 | **XML doc contradicts behavior.** Lines 29–33 claim the method will "save the state and toggle it to offline mode"; it neither saves state (the caller does, line 77) nor sets offline mode directly | `FrameBuilding.cs:29-33` vs `:34-46` | Documentation-defect issue |
| D8 | **Asymmetric error handling around the offline probe.** `_globals.Ol.NamespaceMAPI.Offline` (line 77) is read **outside** the `try` at line 80, so a COM failure there escapes unlogged and without restoring state, while an identical failure inside the fetch is both logged and restored | `FrameBuilding.cs:77, 80, 102-108` | One test pins current behavior so a future fix is deliberate and visible |
| D9 | **Restore failure masks the original exception.** If the restore toggle at line 99 or 104 throws, the original `TaskCanceledException` or fetch exception is lost | `FrameBuilding.cs:96-108` | Low priority; may be bundled with D6 |
| O1 | **Two dead `IQfcDatamodel` members.** `UndoMove()` is an unconditional `throw new NotImplementedException()` with a `//TODO`; `MovedItems` has no production consumer (the only code needing the moved-mail stack reads `_globals.AF.MovedMails` directly at `QfcFormController.cs:49`) | `IQfcDatamodel.cs:47-48`; `QueueProcessing.cs:23-27`; `QfcDatamodel.cs:141-144` | Removing a member from a public interface re-exposed through `IQfcHomeController` is a breaking change (R1). One test *pins* the `UndoMove` throw — the AC7-compliant treatment |
| O2 | **Four dead `SortOptionsEnum` flags.** `TriageIgnore`, `TriageImportantLast`, `DateOldestFirst`, `ConversationUniqueOnly` are read nowhere. `ConversationUniqueOnly` is misleadingly a component of `Default = 42`, implying the conversation de-duplication is configurable when `MostRecentByConversation` runs unconditionally at `FrameBuilding.cs:21` and `:59` | `IQfcDatamodel.cs:16,18,20,21` | R3 forbids changing the enum; whether to delete the flags or wire `ConversationUniqueOnly` to the existing filter is a design decision spanning F5 and F2 |
| O3 | **Misleading identifier in an F2-owned file.** `EmailSorter.GetSortKey` tests `HasFlag(TriageImportantFirst)` at line 46 but indexes `_triageImportantLast` at line 53. **Behavior appears correct** — `SortTriageDate` sorts ascending then reverses, so descending on that table yields "triage important first, date recent first" — but the name inverts the apparent intent, and a future maintainer "fixing" it would invert the production sort order | `EmailSorter.cs:21-35, 46, 53`; `EmailSorterTests.cs:51-54` | Low-priority readability issue against F2's file (R6) |

---

## 13. Cross-child observations (non-defect)

- **Five mislabelled tests.** `QfcDatamodelTests.cs:21-219` — the five `TryQueueRemainingMailItemAsync_*`
  tests — do **not** touch `QfcDatamodel` at all. Each constructs `QfcRemainingQueueAdmission` directly and
  calls `admission.TryQueueAsync(...)`. That file belongs to sibling **F2**. Report to the epic; do **not**
  move them — that would conflict with F2's plan and churn a 317-line file. F2's coverage evidence may
  appear to double-count them.
- **Incidental cross-child coverage.** Four `SortTriageDate` test cases exercise `EmailSorter.GetSortKey` /
  `GetDateKey` in the F2-owned `EmailSorter.cs`. Production-file ownership is not violated; F2's per-file
  evidence may include lines these tests reach.
- **F2 gate coupling.** The high-confidence dequeue test file asserts behavior jointly produced by
  `QueueProcessing.cs` and the F2-owned `QfcStreamingDequeueConfidenceGate.cs`. If F2 changes the gate's
  scan or deadline semantics on the integration branch, those tests fail. Treat such a failure at the
  epic's integration rebase as a **coordination signal**, not as a defect in F5. They are the correct
  assertions for this file's contract, which owns the gate construction and argument marshalling.
- **A `ConversationResolver` test in the wrong file.** `EfcDataModelTests.cs:83` exercises
  `ConversationResolver` and `MailItemHelper` and contributes no `EfcDataModel` line coverage.
  `ConversationResolver.cs` belongs to **F4**. Leave it in place — moving it creates an F4/F5 conflict for
  no coverage gain.
- **Vestigial `ref`.** `TryUnhookOrReplace(ref List<MailItem> nodes, int i)` never reassigns the parameter
  (`QueueProcessing.cs:29-63`). Removing `ref` would change an `internal` signature with no coverage
  benefit. Leave as-is; noted as a cleanup candidate.

---

## 14. Upstream dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`) delivers two artifacts this child consumes:

1. The ratified per-file classification ledger at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.
2. The repeatable per-file line-coverage harness derived from
   `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.

**Neither exists on disk at planning time** — verified 2026-08-08; the epic folder contains only `epic.md`.
That is expected: F1 is prepared concurrently, and at execution time F1 merges to the integration branch
before wave 1 runs.

Consumption rules:

- **F1's harness output is authoritative.** Every projected or read-derived coverage figure in this spec and
  in the research artifacts must be replaced by harness output before any acceptance criterion is checked
  off. Aggregate assembly coverage does not satisfy any criterion; #136 measures success per production file.
- **F1's ledger is authoritative on classification.** This spec asserts `testable` with zero exempt members
  for `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`, and
  `EfcDataModel.cs`, and `not-measurable (declaration-only)` for `IQfcDatamodel.cs`. If the ledger disagrees,
  re-read §5 and §6 at plan time.
- **Escalate rather than comply in two cases.** (a) If the ledger ratifies a type-wide exemption for a file
  this seamable, that is inconsistent with `epic.md` § Shared Design 1 — escalate with the evidence in §6.3.
  (b) If the ledger classifies `IQfcDatamodel.cs` as `testable` with an 80% obligation, AC1 becomes
  permanently unsatisfiable — escalate with the measured evidence in §5.2. The same argument applies to the
  other ~23 declaration-only files.
- This child must not modify `coverage.config` or any shared build property file; those belong to F1.

---

## 15. Non-goals

- No behavior change to end-user QuickFiler flows.
- No fix for any item in the §12 register.
- No edit to any sibling-owned file (R6), to `UtilitiesCS`, or to `coverage.config`.
- No new clock abstraction; `TimeProvider` is the seam.
- No `*.StaTests.cs` file and no STA-apartment test anywhere in this child (§3.2).
- No new production interface where an `internal` member plus the existing
  `InternalsVisibleTo("QuickFiler.Test")` grant (`QuickFiler/Properties/AssemblyInfo.cs:5`) suffices.
- No narrowing of `public` members that this analysis shows have no external consumer; that is churn with no
  coverage or contract benefit.
- No coverage work on `QuickFiler/Legacy/**` or `QuickFiler/Notes/**` (not compiled).

---

## 16. Acceptance Criteria

- [ ] **AC1 — Per-file coverage floor.** Every file in the §2 scope table that F1's ledger classifies as
      `testable` reaches at least 80% line coverage, verified with F1's per-file coverage harness and
      recorded as numeric per-file evidence under
      `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/evidence/qa-gates/`. Projected or
      read-derived figures are not acceptable evidence and must be replaced by harness output before this
      criterion is checked off. `QuickFiler/Interfaces/IQfcDatamodel.cs` is outside this numeric gate only
      if F1's ledger classifies it `not-measurable (declaration-only)` per §5.2; if F1 classifies it
      `testable`, escalate with the measured evidence in §5.2 rather than attempting to comply.
- [ ] **AC2 — Exemption removal.** `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcDatamodel.cs:25`
      is removed, and all three `QfcDatamodel` partials it currently suppresses reach the 80% floor through
      seam extraction rather than exemption — unless F1's ledger ratifies a specific irreducible remainder,
      in which case that remainder is recorded member-level alongside the ledger entry ratifying it. The
      removal is the last production task of the feature (§7). No new `[ExcludeFromCodeCoverage]` is added
      to any file in scope.
- [ ] **AC3 — File size.** No production file in scope exceeds 500 lines after the change, including every
      new production file this feature adds (`QfcDatamodel.Construction.cs`, and `QfcEmailFrameShaper.cs` /
      `EfcDataModel.Seams.cs` if taken).
- [ ] **AC4 — Test conventions and determinism.** All new and modified tests use MSTest, Moq, and
      FluentAssertions with Arrange–Act–Assert, and are independent, isolated, and deterministic: timing is
      driven exclusively by `System.TimeProvider` / `FakeTimeProvider`, with no `Thread.Sleep`, no
      `Task.Delay`, no real wall-clock wait, no temporary file, no external service or process, no live
      form, no modal dialog, and no STA-apartment test.
- [ ] **AC5 — Scenario completeness.** For each file in scope, the delivered tests span the positive path
      plus invalid-input, boundary, and error-handling behavior, and state-transition and ordering behavior
      where the file exhibits such behavior.
- [ ] **AC6 — Toolchain.** The full C# toolchain passes in its final form, in order and with no intervening
      file change: `csharpier .`; the analyzer msbuild; the nullable msbuild; and `vstest.console.exe` with
      `/EnableCodeCoverage`.
- [ ] **AC7 — No behavior change.** No observable QuickFiler flow changes.
      `QuickFiler/Interfaces/IQfcDatamodel.cs` receives zero production edits, `SortOptionsEnum` is unchanged
      including `Default = 42`, all nine `IQfcDatamodel` members keep byte-identical signatures, and every
      consumer call site in §4.1 compiles and behaves unchanged.
- [ ] **AC8 — Defects promoted, not fixed.** Every latent defect and promote-to-issue observation in the §12
      register is promoted to a GitHub issue through the MCP promotion lifecycle rather than fixed in this
      child, and each resulting issue number is recorded in this spec.
