# quickfiler-qfc-form-explorer-controller-coverage — Spec

- **Issue:** #435
- **Parent (optional):** epic #136 (`quickfiler-per-file-coverage`), child F6, wave 1, band C3
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08
- **Status:** Draft
- **Version:** 0.2

## Overview

Child F6 of epic #136 owns the `QfcFormController` partial-class family plus `QfcExplorerController`
and their interface declarations — 10 compiled files, approximately 1,611 lines in
`QuickFiler/QuickFiler.csproj`. Today this cluster does not meet the per-file 80% line-coverage floor
mandated by issue #136:

- `QuickFiler/Controllers/QfcExplorerController.cs` (323 lines) carries `[ExcludeFromCodeCoverage]`
  at line 20 and has no tests at all. Per the epic's Shared Design section 1, that attribute is
  treated as unratified: the `CLAUDE.md` COM/VSTO exemption qualifier "without an injectable seam" is
  a live obligation, not standing permission, so the attribute must be removed and the file covered
  through seam extraction unless F1's ledger ratifies a specific irreducible remainder.
- The four `QfcFormController.*` partials (196 + 232 + 399 + 302 lines) have partial coverage from
  `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` and `QfcFormControllerSeamTests.cs`, but
  the event-handler, action, and setup/disposal paths cross the form/viewer boundary and are the
  least reachable. Several existing tests execute production lines while asserting nothing, so the
  measured line rate overstates real assurance.
- Actual current per-file coverage for each of the ten files is unmeasured on this branch. The epic
  mandates numeric per-file evidence rather than aggregate assembly coverage. The only prior figures
  available (`line-rate` 0.755556 for `QfcFormController.cs`, 0.70684 for
  `QfcFormController.SetupDisposal.cs`) come from a committed Cobertura artifact belonging to the
  in-flight #424 branch and are explicitly not this child's baseline.

Two distinct compiled files are both named `IQfcFormController.cs` — one under `Controllers/`, one
under `Interfaces/`. The determination is recorded in this spec: the `Controllers/` file is the
authoritative, implemented interface; the `Interfaces/` file is compiled dead code with zero
implementers and zero compiled consumers. Its deletion is recommended but routed to F16, not F6.

## Behavior

Raise every file in the F6 set that F1's ledger classifies as `testable` to at least 80% line
coverage, verified with F1's per-file coverage harness and recorded as numeric evidence, without
changing observable QuickFiler behavior. Where a path is unreachable from a deterministic unit test,
introduce a seam (interface seam first, then injectable delegate, then adapter) rather than exempting
the file. Extend no viewer abstraction: `IQfcFormViewer` is already seam-complete for this work.

## Per-File Disposition

F1's ledger at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` is the ratifying
authority for the classification in this table. **F1's outputs do not exist on disk yet — it is being
prepared concurrently in wave 0. That absence is expected and is not a gap or a blocker.** The
classifications below record this child's evidence-backed expectation; if the ledger differs, the
ledger wins and the plan must cite the ledger row rather than this table.

| # | File (repo-relative) | Lines | `[ExcludeFromCodeCoverage]` today | Classification | Target |
| --- | --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcFormController.cs` | 196 | No | testable | >= 80% line coverage |
| 2 | `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | 232 | No | testable | >= 80% line coverage |
| 3 | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 399 | No | testable | >= 80% line coverage |
| 4 | `QuickFiler/Controllers/QfcFormController.Actions.cs` | 302 | No | testable | >= 80% line coverage |
| 5 | `QuickFiler/Controllers/QfcExplorerController.cs` | 323 | **Yes — class scope, line 20** | testable | attribute removed **and** >= 80% line coverage |
| 6 | `QuickFiler/Controllers/IQfcFormController.cs` | 43 | No | no executable content — not a coverage target | classification recorded in F1's ledger; no percentage applies |
| 7 | `QuickFiler/Interfaces/IQfcFormController.cs` | 25 | No | no executable content — not a coverage target | classification recorded in F1's ledger; no percentage applies |
| 8 | `QuickFiler/Interfaces/IQfcExplorerController.cs` | 15 | No | no executable content — not a coverage target | classification recorded in F1's ledger; no percentage applies |
| 9 | `QuickFiler/Interfaces/IQfcFormViewer.cs` | 51 | No | no executable content — not a coverage target | classification recorded in F1's ledger; no percentage applies |
| 10 | `QuickFiler/Interfaces/IFilerFormController.cs` | 25 | No | no executable content — not a coverage target | classification recorded in F1's ledger; no percentage applies |

Notes on the table:

- Files 6–10 are pure declaration files: `using` directives, one namespace, one interface, and member
  signatures with no bodies. .NET Framework 4.8 has no default interface members, and none is present
  regardless. They emit no IL sequence points, so their line-coverage figure is undefined rather than
  zero. `.claude/rules/general-unit-test.md` § "Coverage Requirements" names "C# interface-only
  files" as exactly this case. The correct disposition is `no executable content`, recorded in F1's
  ledger — **not** an entry in `coverage.config`, which is the wrong mechanism and is prohibited for
  production paths.
- No reflection-based test may be authored to assert that a declaration file declares a given member
  or derives from a given base. Such a test executes only test-assembly code, adds zero lines to the
  production numerator, duplicates what the compiler enforces (CS0535), and satisfies no clause of
  the coverage policy.
- No executable code may be added to files 6–10 to manufacture a numerator.
- If F6 adds a new production file (see "Seam Design Summary", item S9), that file is a new module
  and carries the `CLAUDE.md` § UT2 new-module target of >= 90% coverage. F1's ledger should be
  extended with a row for it; F6 supplies the evidence, F1 records the classification.

## Seam Design Summary

Seam hierarchy per `.claude/rules/csharp.md` § "DI Seams" and epic Shared Design §2: **interface seam
> injectable delegate > adapter**. Introduce the smallest seam that makes the path deterministically
testable.

**Orchestrator-verified constraint that governs every choice below:**
`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` is declared at
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11` and compiled via `QuickFiler.csproj:322`.
Moq therefore **can** create dynamic proxies for `internal` QuickFiler interfaces. An `internal`
interface seam is permitted and is preferred over a delegate property wherever an interface expresses
the collaboration more clearly. No seam in this feature may be justified on the grounds that an
internal interface seam is impossible. The atomic planner must evaluate each item below against that
corrected constraint before fixing the design; the shapes listed are the researched options, not a
frozen decision.

Also verified: `[assembly: InternalsVisibleTo("QuickFiler.Test")]` is compiled twice
(`QuickFiler/Properties/AssemblyInfo.cs:5` and `QuickFiler/Controllers/QfcHomeController.cs:18`), so
`internal` seam members on the concrete classes are directly reachable from the test assembly without
reflection and without appearing on any public interface.

### Mandatory seams

These three are hard requirements, not preferences. Without them the affected tests either open a
modal dialog that blocks the run, leak a non-terminating background task into the test host, or
construct a live COM/WinForms-bound collection controller.

| ID | Seam | Sites | Why mandatory |
| --- | --- | --- | --- |
| **S1** | Prompt / dialog seam returning `DialogResult` | `QfcFormController.Actions.cs` `UndoDialog()` lines 225, 238, 248 (three `MessageBox.Show` calls); `QfcExplorerController.cs:168` (one `MessageBox.Show`) | A unit test must never produce a popup requiring human interaction (`CLAUDE.md` § UT4; epic Shared Design §2). Without the seam, any `UndoDialog()` test past its guard blocks the run indefinitely. |
| **S2** | Injectable background-start delegate wrapping `Task.Run(UndoConsumer)` | `QfcFormController.Actions.cs:211` (`_undoConsumerTask ??= Task.Run(UndoConsumer);`) | `UndoConsumer()` never terminates (see "Out of Scope", OOS-1). Any test that starts the real loop leaks a spinning thread for the remainder of the run, violating test isolation and fast execution. Tests must substitute a recorder that captures the delegate and returns a completed task without executing it. |
| **S3** | Injectable `IQfcCollectionController` factory | `QfcFormController.Actions.cs` lines 49, 83, 139 (three `new QfcCollectionController(...)` sites) | `QuickFiler/Controllers/QfcCollectionController.cs` belongs to sibling F11 and must not be edited. Its 8-parameter constructor immediately dereferences `_formViewer.L1v0L2L3v_TableLayout`, `_formViewer.L1v0L2_PanelMain`, `_homeController.KeyboardHandler`, and `_globals.Ol.DarkMode`, and throws on a loose mock. The factory default must reproduce today's construction argument-for-argument. |

### Remaining seams, by file

| ID | File | Seam | Sites | Notes |
| --- | --- | --- | --- | --- |
| S4 | `QfcFormController.EventHandlers.cs` | Existing `UtilitiesCS.Threading.IUiDispatcher` interface seam (tier 1 — no new type) | lines 199, 233 | In-repo precedent: `QfcItemController.Initialization.cs:38,57,380` takes an optional `IUiDispatcher uiDispatcher = null` and defaults it with `new WpfUiDispatcher()`. Removes an existing test-order dependency on `UiThread._dispatcher`. |
| S5 | `QfcFormController.EventHandlers.cs` | Priority-bearing UI dispatch seam for a `Func<Task>` payload | lines 228–231 | `IUiDispatcher` has no `InvokeAsync<TResult>(Func<TResult>, DispatcherPriority)` member, and routing the async lambda through the `Action` overload would convert a discarded `Task` into an `async void` — a behavior change, rejected. Growing `IUiDispatcher` would edit `UtilitiesCS`, outside the epic's denominator. The seam must preserve the existing fire-and-forget discard exactly. |
| S6 | `QfcFormController.EventHandlers.cs` | Notification message seam (`text, caption, buttons, icon`, no return value) | lines 180, 204, 382 | Same popup prohibition as S1. Kept **distinct** from S1: merging them would force the notification sites to consume a `DialogResult` they currently ignore. |
| S7 | `QfcFormController.EventHandlers.cs` | Worker-busy predicate | lines 178, 380 (`_formViewer.Worker?.IsBusy`) | `IQfcFormViewer.Worker` returns the concrete `System.ComponentModel.BackgroundWorker`, whose `IsBusy` is non-virtual and cannot be forced by Moq. Adding `IsWorkerBusy` to `IQfcFormViewer` is rejected — it would require an F15 edit to `QfcFormViewer.cs`. Line 178 currently omits the null-conditional on `_formViewer` that line 380 has; the plan must record explicitly whether adopting the uniform null-safe form is behavior-neutral (it is, because the pre-guard at line 149 already returns on a null viewer). |
| S8 | `QfcFormController.EventHandlers.cs`, `.Actions.cs` | Delay seam replacing `await Task.Delay(...)`; elapsed-time probe replacing the `Stopwatch` read | `.EventHandlers.cs:254`; `.Actions.cs:255-256, 279, 285` | `Task.Delay` is a repo banned symbol and a real wall-clock wait is prohibited in tests. Tests substitute a completed task and drive loop iteration with a scripted `WorkerComplete` / elapsed sequence. `TimeProvider` is an acceptable alternative for the elapsed probe if the planner justifies the timing-source change as behavior-neutral. |
| S9 | `QfcExplorerController.cs` | Extract the file's pure decisions into a host-neutral policy type (temp-view name constant, conversation-upgrade markup strip, remembered-view-name resolution, first-match-wins name scan, two `HasFlag` toggle decisions, folder-path comparison) | lines 79–104, 110–120, 135–137, 150–153, 179 | Recommended destination is a new `QuickFiler/Controllers/QfcConversationViewPolicy.cs` (projected 90–110 lines, 100% pure, no exemption attribute). The fallback — declaring the helpers as `internal static` members inside `QfcExplorerController.cs` — avoids a `QuickFiler.csproj` edit at the cost of host-neutral reuse. Planner decides; both stay under 500 lines. |
| S10 | `QfcExplorerController.cs` | View-resolution seam for `folder.Views[name]` | line 127 | `Outlook.Views` is the only interop type in the F6 set with no in-repo `Mock<T>` precedent. Routing the indexer behind a one-line default removes the last unproven-mockability risk. `Explorer`, `CommandBars`, `View`, `MAPIFolder`, `MailItem`, and `Application` all have working in-repo Moq precedent and get **no** seam. |
| S11 | `QfcExplorerController.cs` | Background-work seam replacing `Task.Run(...)` | lines 154, 158, 159, 180 | Optional but recommended: each call is awaited and therefore already sequential, but the seam removes the thread-pool hop so ordering assertions need no timing. |
| S12 | `QfcFormController.Actions.cs` | Delegate seams for the two static/non-mockable COM materializers: `MailItemHelper.FromMailItemAsync` and `_globals.AF.Manager["Folder"].UnTrain(...)` | lines 46–48, 262–267, 268–272 | `ManagerAsyncLazy` is a concrete type with a non-virtual inherited indexer; an interface seam would require editing `UtilitiesCS`, outside the epic. In-repo precedent for the static-materializer seam is `QfcHighConfidencePreFilter.cs:156-178`. |
| S13 | `QfcFormController.EventHandlers.cs` | Structural extraction of the four `async void` handlers' `try`/`catch` into awaitable `...CoreAsync` methods | `ButtonCancel_Click`, `ButtonOK_Click`, `SpnEmailPerLoad_ValueChanged`, `ButtonSkip_Click` | Public signatures and the `RegisterFormEventHandlers` subscriptions are unchanged; the rethrow-vs-swallow semantics of each handler must be preserved exactly (three rethrow, `SpnEmailPerLoad_ValueChanged` swallows). Without this, the `catch` blocks are unassertable because an `async void` cannot be awaited. |

### Files that need no new production seam

`QfcFormController.cs` and `QfcFormController.SetupDisposal.cs` need **zero** new seams for their own
coverage. Every uncovered line in both is reachable today through the existing `IQfcFormViewer`,
`IApplicationGlobals`/`IOlObjects`, `IQfcQueue`, `IQfcHomeController`, and `IQfcKeyboardHandler`
interfaces, plus reflection-seeded private fields and in-memory, never-shown WinForms controls that
the current suite already constructs. `QfcFormController.cs` is nonetheless the natural declaration
site for the shared seam members S1–S8 and S12 consumed by the other two partials; that is
intra-child sequencing (the declaration task must precede the consuming tasks), not a cross-child
contract.

### Seam constraints the design must respect

- **Seam defaults must reproduce current behavior exactly.** Every default value, lambda body, and
  overload binding must be byte-equivalent in effect to the call it replaces, including the
  fire-and-forget `Task` discard at `.EventHandlers.cs:199` and `:228`.
- **`QfcExplorerController`'s public constructor signature is frozen.** It is bound by two factory
  lambdas outside F6 — `QfcHomeController.cs:175-182` (F7) and
  `EfcHomeControllerDependencyFactories.cs:149-155` (F8). New dependencies must be optional settable
  members, never new required constructor parameters.
- **No member may be added to, removed from, or renamed on any of the five interface files.** They
  are frozen; §"Cross-Child Contract Notes" records why.
- **net48 constraints:** no default interface members, no `init` accessors, no `record` /
  `record struct` (they fail CS0518 in this repository).
- **STA last-resort clause (epic Shared Design §3): not invoked.** No proposed test constructs a
  form, and existing `QuickFiler.Test` classes already construct `TableLayoutPanel`, `Control`, and
  `Control.ControlCollection` successfully in the default apartment. If execution proves a specific
  manipulation apartment-dependent, only those cases move to a dedicated `*.StaTests.cs` file with
  `[STATestClass]`, which ships in MSTest.TestFramework 4.3.3 and needs no new package.

## Cross-Child Contract Notes

### No sibling-owned production file requires an edit

This is stated positively and is the result of the per-file research, not an assumption:

- **`QuickFiler/Viewers/QfcFormViewer.cs` (F15) — no edit required.** `IQfcFormViewer` is already
  seam-complete for F6. Every `_formViewer.` call site across the four partials binds to a member
  already declared on the interface or inherited through `IForm` → `IContainerControl` /
  `IScrollableControl` → `IControl` (`Show`, `Hide`, `Refresh`, `Invoke`, `Controls`, `Size`,
  `ClientSize`, `Handle`, `WindowState`). The Seam B/C/D members added by issue #223 supply the rest.
  `GetScreen()` is an extension method (`UtilitiesCS/Extensions/IControlExtensions.cs:16`), not an
  interface member, and needs no interface change. Seam S7 explicitly avoids adding `IsWorkerBusy` to
  the interface for this reason.
- **`QuickFiler/Controllers/KeyboardHandler.cs` (F3) — no edit required.** It is consumed only
  through `IQfcKeyboardHandler`, reached via `IFilerHomeController.KeyboardHandler`.
- **`QuickFiler/Controllers/QfcCollectionController.cs` (F11) — no edit required.** Every use goes
  through the `IQfcCollectionController` interface; seam S3 exists precisely so the concrete type is
  never constructed in a test.
- **`coverage.config` and shared build property files (F1) — not modified.**
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` writes a *derived* copy of `coverage.config` beside
  the coverage output and deletes it in a `finally` block (lines 79–116, 198–242); the canonical file
  at repo root is read but never written. Running the harness therefore does not breach F1's
  ownership.
- **`QuickFiler/Controllers/EfcFormController.cs` (F9) — no edit required**, because
  `IFilerFormController` is frozen (below).
- **`QuickFiler/Controllers/FilerQueue.cs` (F2) — no edit required.** It has a public parameterless
  constructor and `Consumer` defaults to `Task.CompletedTask`, so a real instance is a safe stub.
- **`UtilitiesCS` — no edit required.** It is outside the epic's file assignments entirely.

### Interface freeze rationale

| Interface | Implementers | Compiled consumers outside F6 | Decision |
| --- | --- | --- | --- |
| `Interfaces/IQfcFormViewer.cs` | 1 — `QfcFormViewer.cs` (F15, off-limits) | F3, F7, F11 plus 15 mock sites | Frozen. Any added member forces an F15 edit on net48. |
| `Interfaces/IFilerFormController.cs` | 2 — `QfcFormController` (F6) and `EfcFormController` (F9, 1,086 lines, off-limits) | F7, F8, F11, F15 | Frozen — the most change-hostile file in the set. Any new controller-side member belongs on `Controllers/IQfcFormController.cs`, which has exactly one F6-owned implementer. |
| `Interfaces/IQfcExplorerController.cs` | 1 — `QfcExplorerController` (F6) | F7, F8, F9, F10 plus 7 fixtures, two with `MockBehavior.Strict` | Frozen. Widest consumer blast radius in the set. |
| `Controllers/IQfcFormController.cs` | 1 — `QfcFormController` (F6) | `QfcHomeController` (F7) plus 7 F7 fixtures | Treated as frozen; F6's seam work is inbound and does not need it grown. If growth proves unavoidable, an additive member is source-compatible for F7 (its two references are type references, not member invocations); a removal or rename is not. |
| `Interfaces/IQfcFormController.cs` | **0** | **0** | Compiled dead code. Deletion recommended, routed to F16 (below). |

### Notes that do exist

- **CROSS-CHILD CONTRACT NOTE (F1) — ledger key.** F1's per-file ledger must be keyed by
  **repo-relative path, not base name**, because two distinct compiled files share the base name
  `IQfcFormController.cs` (`QuickFiler/Controllers/IQfcFormController.cs` at `QuickFiler.csproj:303`
  and `QuickFiler/Interfaces/IQfcFormController.cs` at `:363`). The same collision exists for
  `IQfcHomeController.cs`, which is present in both folders although only the `Controllers/` copy is
  compiled. F6 owns both `IQfcFormController.cs` rows and is the first child to hit the collision.
- **CROSS-CHILD CONTRACT NOTE (F1) — new-file ledger row.** If seam S9 lands as a new production
  file, F1's 121-file enumeration predates it; the ledger should gain a row classifying it
  `testable` with the >= 90% new-module target.
- **CROSS-CHILD CONTRACT NOTE (F16) — dead-interface deletion.** Deleting
  `QuickFiler/Interfaces/IQfcFormController.cs` and its `QuickFiler.csproj:363` entry is recommended
  on dead-code-hygiene grounds and to remove a latent CS0104 ambiguity trap, but it is **routed to
  F16, not executed by F6**: it reduces the epic's compiled-file denominator from 121 to 120 (a
  quantity F1 defines and F16 verifies), it edits a shared build input during a fourteen-way parallel
  wave, and it moves F6's coverage evidence by exactly zero lines. If it is not done, the ledger row
  should read `no executable content — unreferenced, deletion recommended`.
- **CROSS-CHILD CONTRACT NOTE (F7, F15) — no action required.** Recorded so those children can
  confirm the analysis rather than discover it at fan-in: `QfcHomeController.cs:208` and `:415` bind
  to the `Controllers` variant of `IQfcFormController` and are unaffected by the recommended
  deletion; `Viewers/QfcFormViewerExpanded.cs` and `Viewers/QfcFormViewerDark.cs` are not compiled,
  are not in F15's assigned set, and would become unambiguous rather than broken.

## Shared-Build-File Risk

`QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` are legacy non-SDK
projects with explicit `<Compile Include>` item lists — files are not globbed. Every new production
file (seam S9) and every new test file therefore requires its own `<Compile Include>` line.

Neither project file is assigned to any child by the epic's Feature File Assignments, yet **every one
of the fourteen wave-1 siblings that adds a file must edit both**. This is recorded here as the most
likely source of merge conflicts on
`epic/quickfiler-per-file-coverage-integration`.

Mitigation, binding on this child:

- F6 confines its edits to **adding its own `<Compile Include>` lines**. It removes no existing
  entry, reorders no existing block, and changes no project property, target, reference, or
  `packages.config` entry.
- New entries are inserted in alphabetical position within the existing `Controllers\` block so
  conflicts are line-adjacent and mechanically resolvable.
- Any conflict at fan-in is handled by this child's own R1–R5 remediation loop per the
  `epic-orchestrate` skill.
- Neither file is `coverage.config` nor a shared build **property** file, so the F1 ownership
  constraint is not breached by these additions.

## Anti-Pattern Guard — Denominator Shifting Is Prohibited

Issue #223's own coverage evidence
(`docs/features/archive/2026-06-28-qfc-form-viewer-testability-223/evidence/regression-testing/coverage-delta.2026-06-28T20-52.md:10`)
records that `QfcFormController` coverage rose from 39.24% to 51.86% partly because "the denominator
decreased from 767 to 700 because Seam D moved the ~58-line `new TlpCellStates(...)` construction
block out of the controller and into the `[ExcludeFromCodeCoverage]` Form (`CaptureTlpCellStates`)."

That manoeuvre was legitimate at the time. Under this epic's Shared Design §1 it is no longer
available: the attribute on `QfcFormViewer.cs:17` is itself treated as unratified, and F15 is
obliged to cover that file. Moving lines into it would raise F6's per-file number by shrinking its
denominator while transferring untested lines onto a sibling that did not agree to absorb them.
`.claude/rules/general-unit-test.md` requires the opposite direction: extract logic **out of**
host-bound files into host-neutral testable modules.

**Any change whose net effect is relocating executable lines from a file in the F6 set into a file
carrying `[ExcludeFromCodeCoverage]` — `QfcFormViewer.cs` or any other — is rejected at plan review.**
The 51.86% figure in that archived artifact is a historical measurement from 2026-06-28 and is not
F6's baseline; the baseline must come from F1's harness on this branch.

## Inputs / Outputs

This is an internal testability and coverage feature. It has no runtime inputs, outputs, configuration
keys, or user-visible artifacts. Its only deliverables are production seams that preserve existing
behavior, new unit tests, and coverage evidence files under `<FEATURE>/evidence/qa-gates/`.

## API / CLI Surface

None. No command, flag, request shape, response shape, or public API is added, removed, or changed.
The `QfcExplorerController` constructor signature and all five interface member sets are explicitly
frozen; new seam members are `internal` on `internal` classes and add nothing to the public surface.

## Data & State

No data flow, persistence, caching, schema, or migration is introduced or altered. The only state
concerns are the existing in-memory controller lifecycle invariants that the new tests pin:
double-`Cleanup()`, dispose-before-setup, repeated `Init()`, register/unregister round-trip, and the
`Cleanup()` ordering invariant that unsubscription (line 215) runs before `_formViewer` is nulled
(line 219). `QfcFormController.Cleanup()` is **already idempotent** — a second call does not throw,
verified line by line — so the correct action is to pin the invariant with regression tests, not to
add a guard.

## Constraints & Risks

- `QfcExplorerController` depends on the Outlook `Explorer`/`View`/`Views`/`MAPIFolder`/`MailItem`
  interop surface; the form controller's event handlers and disposal paths cross the form/viewer
  boundary.
- `Viewers/QfcFormViewer.cs` (the concrete `IQfcFormViewer` implementation) belongs to sibling F15
  and must not be edited. Per the research it needs no edit; the design is required to keep it that
  way.
- `Controllers/KeyboardHandler.cs` belongs to sibling F3 and must not be edited; this controller
  consumes it only through `IQfcKeyboardHandler`.
- `Controllers/QfcCollectionController.cs` belongs to sibling F11 and must not be edited; seam S3
  exists so the concrete type is never constructed in a test.
- `coverage.config` and shared build property files belong to F1 and must not be modified here.
- Setup and disposal carry state-transition invariants (double-dispose, dispose-before-setup,
  repeated setup, register/unregister ordering).
- Depends on F1 (`quickfiler-coverage-ledger`, wave 0) for the per-file measurement harness and the
  ratified classification ledger at
  `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. F1's outputs are not on disk
  yet; that is expected and is not a gap.
- **Existing test file `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is 827 lines**, already
  327 lines over the 500-line limit that `.claude/rules/general-code-change.md` applies to test code.
  F6 must **not** append to it and must **not** split it. All new tests go in new files. The split is
  promoted separately (OOS-4).
- **`QfcFormControllerSeamTests.cs:352-374` reads `QfcFormController.Actions.cs` off disk and
  string-matches two exact `LoadItemsAsync` signatures.** Seam work in `Actions.cs` must be
  **body-only**. Renaming either overload, reordering them, adding a parameter (which would make
  CSharpier reflow the signature onto multiple lines), or introducing the identifiers
  `ApplyHighConfidenceFilterAsync` / `RemoveBelowThresholdAsync` into the `MailItem` overload's body
  breaks that test.
- `UtilitiesCS.UiThread` holds process-global statics that `QfcHomeControllerRunAsyncTests.cs:329`
  initializes for the whole test process. The CLI runsettings run classes in parallel
  (`<Workers>0</Workers>`, `<Scope>ClassLevel</Scope>`), so no F6 test may read or mutate `UiThread`
  in either direction. Applying a theme with an empty `ControlGroups` map, and using the synchronous
  `Theme.SetTheme()` path, both keep `UiThread.Dispatcher` untouched.
- Five sites call `SynchronizationContext.SetSynchronizationContext`. Any test reaching one must
  capture `SynchronizationContext.Current` in `[TestInitialize]` and restore it in `[TestCleanup]`,
  or test independence is violated for the whole assembly.
- Assertions must not depend on host display configuration. `SpaceForEmail` reads
  `Screen.PrimaryScreen`, which can be null on a headless or session-0 agent; new tests must compute
  their expected value from the same `Screen.PrimaryScreen?.WorkingArea.Height ?? 0` source or assert
  the deterministic guard-return of `0`.

## Out of Scope

Each item below is recorded, not actioned, in this child. Latent defects are promoted through the
MCP promotion lifecycle into their own GitHub issues so the finding survives the merge of this
feature folder.

| ID | Item | Why out of scope |
| --- | --- | --- |
| OOS-1 | **`UndoConsumer()` never terminates** (`QfcFormController.Actions.cs:258`). `while (!_undoQueue.IsCompleted \|\| exit)` holds the loop open on its own (nothing calls `CompleteAdding`), and once the 10-second `else if` sets `exit` the disjunction is permanently true and no `await` is reached — a busy spin. The post-loop `if (exit) { _undoConsumerTask = null; }` is unreachable in any terminating execution. | Correcting the condition is a **behavior change**, which acceptance criterion AC-7 forbids. Promoted to its own defect issue. F6's obligation is seam S2, which keeps the real loop out of the test host. |
| OOS-2 | **`ExplConvView_Cleanup()` throws `NotImplementedException`** (`QfcExplorerController.cs:61-64`, with a `//PRIORITY: Implement` comment). | Implementing it is new behavior, not coverage. The honest coverage treatment is a negative test asserting the current throw. Promoted to its own issue. |
| OOS-3 | **`OpenQFItem` re-calls `ActiveExplorer()`** at `QfcExplorerController.cs:140` instead of reusing `_activeExplorer` captured at `:35`. If Outlook returned a different `Explorer` instance between the two calls, the folder switch and the subsequent `AutoFile.AreConversationsGrouped(_activeExplorer)` at `:141` would act on different objects. | A latent defect whose fix is a behavior change. Promoted to its own issue; F6's tests are written against the current two-call shape. |
| OOS-4 | **Splitting the 827-line `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`** into per-partial files under the 500-line limit. | The file mixes tests for all four partials, so a correct split is a four-way concurrent edit to one file executed by four F6 plan phases in the same wave — the exact conflict shape the epic's decomposition avoids. The violation is pre-existing and not caused by this child. Promoted to its own issue. |
| OOS-5 | **Deleting the dead `QuickFiler/Interfaces/IQfcFormController.cs`** (zero implementers, zero compiled consumers) and its `QuickFiler.csproj:363` entry. | Routed to **F16**: it changes the compiled-file denominator F1 defines and F16 verifies, it edits a shared build input mid-wave, and it moves F6's evidence by zero lines. |
| OOS-6 | **`QuickFiler/Interfaces/IQfcFormViewer.cs` namespace inconsistency** — the file sits in `Interfaces/` but declares `namespace QuickFiler` while its siblings declare `namespace QuickFiler.Interfaces`. | Correcting it is a breaking change that would require verifying or adding a `using` in every consumer across F3, F7, F11, and F15 during a fully-parallel wave, and delivers no coverage. Standalone follow-up after the epic closes. |
| OOS-7 | **Deleting the dead `#region Email Sorting To Rewrite`** at `QfcExplorerController.cs:183-321` — six `private static` / `internal static` members with zero callers in the compiled tree, each a verbatim copy of code already maintained and tested in `UtilitiesCS/.../SortEmail.cs`, `UtilitiesCS/.../EmailFiler.cs`, and `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`. Two latent defects inside the region (transposed `Path.Combine` arguments; a write into a `null` `ref string[]`) are unreachable while it stays dead. | Promoted to **issue #449**, which sequences the deletion AFTER this child merges so two children do not edit `QfcExplorerController.cs` concurrently. **This deferral has a real cost:** retaining roughly 50–60 uncoverable statements may put AC-1's 80% floor out of reach for this one file. The plan measures the true rate and routes any shortfall to F1's ledger citing #449 rather than adding an exemption attribute or deleting the region. Reversing this sequencing is an epic-level decision. |

Additional observations recorded without action, for the plan author and for F16:

- `QfcFormController`'s constructor throws `NullReferenceException` (not `ArgumentNullException`) for
  a null `appGlobals.AF`, `formViewer`, or `parent`; `QfcExplorerController`'s constructor likewise
  has no argument validation. Adding guards changes the thrown exception type on an already-fatal
  path. F6 pins the current behavior in tests; adding guards is a separate issue.
- `Init()`, `SetupLightDark()`, and `RegisterFormEventHandlers()` are not idempotent — a second
  `Init()` double-subscribes every form intent event. No production caller invokes `Init()` twice, so
  this is latent. F6 documents the current subscription counts rather than adding a guard, which
  would be an observable change.
- Five existing tests in `QfcFormControllerTests.cs` (`RemoveTemplatesAndSetupTlp_ShouldSetupTlp`,
  `SetupLightDark_ShouldSetupThemes`, `RegisterFormEventHandlers_ShouldRegisterHandlers`,
  `UnregisterFormEventHandlers_ShouldUnregisterHandlers`, `Cleanup_ShouldCleanupResources`) have an
  empty Assert section, and `UndoConsumer_ShouldConsumeUndoQueue` is a suppressed tautology. They
  execute production lines while verifying nothing, so the measured line rate overstates assurance.
  F6 supersedes them behaviorally with new tests in new files and does not delete or weaken them.
- `SpaceForEmail_ShouldReturnCorrectValue` asserts `result > 0`, which is host-dependent and would
  fail where `Screen.PrimaryScreen` is null. F6 does not modify it and does not repeat the pattern.
- The fire-and-forget `Task` discard at `.EventHandlers.cs:199` and `:228` is preserved by design,
  not fixed. If the maintainer wants it fixed, that is a separate issue.

## Implementation Strategy

- **Scope of production change.** Seam declarations and call-site substitutions inside the five
  executable files only, plus at most one new pure-logic file (S9). No behavior change. Projected
  post-change sizes, all under 500 lines: `QfcFormController.cs` ~196 → ~226 (seam declarations);
  `.SetupDisposal.cs` 232 unchanged; `.EventHandlers.cs` 399 → ~419; `.Actions.cs` 302 → ~256
  (seam substitutions are net-negative); `QfcExplorerController.cs` 323 → ~330 (seam declarations
  only; the dead region is retained — see below); new policy file ~90–110.
- **Dead-code deletion in `QfcExplorerController.cs` is OUT OF SCOPE for this child.** Lines 183–321
  contain six `private static` / `internal static` members with zero callers anywhere in the compiled
  tree; each is a verbatim copy of code that already lives — and in three cases is already tested — in
  `UtilitiesCS/.../SortEmail.cs`, `UtilitiesCS/.../EmailFiler.cs`, and
  `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`. Deleting them would be behavior-neutral,
  but the deletion is tracked by **issue #449**, which sequences it AFTER this child merges so that two
  children do not edit the same file concurrently. This child therefore deletes no production code.

  **Consequence for AC-1, recorded honestly rather than engineered around.** Retaining the region
  leaves roughly 50–60 uncoverable statements in this file's denominator against roughly 60 live ones,
  so the 80% per-file floor may be unreachable for `QfcExplorerController.cs` in this child. The
  earlier 97–98% projection in `research/QfcExplorerController.cs.md` §6 assumed the deletion. The plan
  covers the one honestly reachable member of the region (`internal static StripTabsCrLf`), measures
  the real rate, and routes any shortfall to F1's ledger citing issue #449. It does not add an
  `[ExcludeFromCodeCoverage]` attribute, delete the region, or reflection-invoke private statics to
  inflate the number. Whether to instead pull #449's deletion into this child is an epic-level
  sequencing decision, not this child's to make.
- **New test files, none shared with an existing fixture.** New tests go in new files under
  `QuickFiler.Test/Controllers/`, each under 500 lines, with a shared non-`[TestClass]` support file
  eliminating the reflection helpers currently duplicated verbatim in two fixtures. The support file
  follows the established `QfcItemController.TestSupport.cs` precedent. Neither
  `QfcFormControllerTests.cs` (827 lines) nor `QfcFormControllerSeamTests.cs` (378 lines, shared
  territory across three partials) is grown or edited.
- **No dependency change.** `QuickFiler.Test.csproj` already references MSTest.TestFramework 4.3.3,
  MSTest.Analyzers 4.3.3, Moq 4.20.72, and FluentAssertions 8.10. No package is added.
- **No logging or telemetry change.** The existing log4net pattern is untouched; no assertion depends
  on it.
- **No rollout mechanics.** There is no feature flag, staged deploy, or fallback path, because there
  is no behavior change to roll out.
- **Sequencing constraint (intra-child).** The seam declarations in `QfcFormController.cs` and the
  shared test-support file are prerequisites for the `.EventHandlers.cs` and `.Actions.cs` work and
  must be scheduled first.
- **Measurement.** Baseline and final per-file coverage are produced with F1's harness over the
  Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, matching the `<class>` element
  whose `filename` is the repo-relative path (separator-insensitive comparison — the Koverage
  post-processing emits forward slashes for some elements). The baseline is captured before any test
  is written and committed under `<FEATURE>/evidence/baseline/`; the final result is committed under
  `<FEATURE>/evidence/qa-gates/`.

## Acceptance Criteria

- [ ] **AC-1 — Per-file coverage floor.** Every file in the F6 set that F1's ledger classifies as
      `testable` reaches >= 80% line coverage, measured with F1's per-file harness over the Cobertura
      output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. The numeric per-file line rate for
      each such file is recorded under
      `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/evidence/qa-gates/`,
      one row per repo-relative path. Aggregate `QuickFiler.dll` coverage does not satisfy this
      criterion. Any new production file added by this child reaches >= 90% per `CLAUDE.md` § UT2's
      new-module rule. Files the ledger classifies `no executable content` are exempt from the
      percentage; their evidence is the classification itself.
- [ ] **AC-2 — `QfcExplorerController` exemption removed.** `[ExcludeFromCodeCoverage]` no longer
      appears anywhere in `QuickFiler/Controllers/QfcExplorerController.cs` (a repo search over that
      file returns zero occurrences), the file appears in the Cobertura report rather than being
      absent from it, and its measured line rate is >= 80%. This child proposes a residual of zero.
      Any residual exemption requires ratification by an explicit row in F1's ledger at
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` naming the specific
      irreducible remainder; F6 may not ratify an exemption on its own say-so, and no new
      `[ExcludeFromCodeCoverage]` attribute is introduced on any file, existing or new.
- [ ] **AC-3 — File-size compliance.** Every production file in the F6 set, and every production and
      test file this child adds or modifies, is at or under 500 lines, verified by a line count in
      the evidence. The pre-existing 827-line `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`
      is out of scope (OOS-4) and is left unmodified; `QfcFormControllerSeamTests.cs` is likewise
      left unmodified.
- [ ] **AC-4 — Test framework and determinism.** All new tests use MSTest
      (`[TestClass]`/`[TestMethod]`), Moq for boundaries, and FluentAssertions for assertions, in
      Arrange–Act–Assert form with a stated scenario per test. No new test creates a temporary file,
      contacts an external service or process, constructs or shows a WinForms `Form`, produces a
      modal dialog, or uses `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, or
      `Random.Shared`. Any test that sets a `SynchronizationContext` restores the prior value in
      `[TestCleanup]`. No test reads or mutates `UtilitiesCS.UiThread` static state. The full suite
      passes with class-level parallelism enabled.
- [ ] **AC-5 — Scenario completeness per file.** For each file the ledger classifies `testable`, the
      new test set includes at least one case in each of four categories — positive path,
      invalid/missing input, boundary condition, and error handling — and the evidence maps each
      category to the specific test method names that satisfy it for that file. Multi-operand guards
      have one case per operand, and stateful members have an explicit state-transition case.
- [ ] **AC-6 — Toolchain green in final form.** A single final pass of the C# toolchain completes in
      order with no failures and no file rewrites: `csharpier .`; `msbuild TaskMaster.sln /t:Build
      /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true
      /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug
      /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; and the
      coverage-enabled MSTest run. The exact commands and their results are recorded under
      `<FEATURE>/evidence/qa-gates/`.
- [ ] **AC-7 — No observable behavior change.** Verified by all of the following: every seam default
      reproduces the call it replaces, including the fire-and-forget `Task` discard at
      `QfcFormController.EventHandlers.cs:199` and `:228`; the `QfcExplorerController` public
      constructor signature is unchanged, so `QfcHomeController.cs:175-182` (F7) and
      `EfcHomeControllerDependencyFactories.cs:149-155` (F8) compile unmodified; no member is added
      to, removed from, or renamed on any of the five interface files in the F6 set; the two
      `LoadItemsAsync` signature lines in `QfcFormController.Actions.cs` remain single-line and
      unchanged so `QfcFormControllerSeamTests.cs:352-374` still passes; no production code is
      deleted by this child; and every pre-existing test in `QfcFormControllerTests.cs` and
      `QfcFormControllerSeamTests.cs` passes without being edited, weakened, or skipped.

## Definition of Done

Delivery is complete when all seven acceptance criteria above are checked off with the stated
evidence present under
`docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/evidence/`,
this spec and `user-story.md` reflect the delivered design, and any latent defect listed in "Out of
Scope" has been promoted to its own GitHub issue through the MCP promotion lifecycle rather than left
as prose in this folder.

## Test Conditions in Scope

- Unit coverage areas: form-controller construction and initialization, theme load and dark-mode
  propagation, event-handler dispatch and the four `async void` shims, action methods (`LoadItems`,
  the four `LoadItemsAsync` overloads, `UndoDialog`, `UndoConsumer`, maximize/minimize), setup and
  disposal ordering, and explorer-controller conversation-view toggling, folder navigation, and item
  selection.
- State transitions: double-`Cleanup()`, dispose-before-setup, repeated `Init()`,
  register-then-unregister, register-unregister-cleanup, and the `Cleanup()` ordering invariant that
  unsubscription precedes nulling the viewer.
- Error handling: null and invalid dependencies, viewer callbacks that throw, collection-controller
  calls that throw, cancelled tokens, and the `catch` paths in the extracted `...CoreAsync` methods
  including the one handler that swallows rather than rethrows.
- Boundary conditions: guard-threshold row counts, zero and negative spinner values, empty and null
  collections, first-match-wins name resolution, and the `-1` items-per-iteration sentinel.
- No integration, CLI, or API surface is in scope.
