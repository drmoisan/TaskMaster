# ribbon-engine-toggle-defects (Spec)

- **Issue:** #735
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** full-bug (this file is the sole acceptance-criteria source; no user story is authored for this item)
- **Last Updated:** 2026-09-02
- **Status:** Ready for planning
- **Version:** 1.0
- **Primary source:** the research record at docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/research/2026-09-02T09-15-ribbon-engine-toggle-defects-research.md, whose file and line citations were re-verified against the working tree while authoring this spec.

## Context

Issue #735 consolidates three code-review defects that all live in the Explorer ribbon subsystem under the directory TaskMaster/Ribbon, with their tests under TaskMaster.Test/Ribbon. They were grouped into one work item because they share a small file set and would otherwise consume three separate orchestration cycles for a combined production delta of roughly forty lines.

- **Finding 1 (source #504) — dead XML-to-handler bindings.** The Explorer CustomUI document declares five callback names that resolve to no public instance method on the RibbonViewer type. Four are a `_Clicked` versus `_Click` suffix mismatch on the Item Sort Settings check boxes; the fifth, `BtnMigrateIDs_Click`, has no implementation anywhere in the solution. Office CustomUI resolves callbacks by name at invocation time, so the add-in compiles and loads cleanly and the affected controls silently do nothing when clicked.
- **Finding 2 (source #524) — unguarded optional-dependency dereference.** The method `ClearSpamManagerAsync` on the Intelligence partial of the ribbon controller dereferences the globals chain through the auto-file manager and the engines facade with no guard. Each link is genuinely null during the window between ribbon construction and the completion of add-in initialization, so a click on Clear Spam Manager in that window raises an unhandled `NullReferenceException` from a UI event handler.
- **Finding 3 (source #525) — toggle-state last-writer race.** Both writers on the engine toggle state coordinator — the user-initiated toggle path and the lazy prime path — read the engine's real activation state and then write the shared pressed-state cache unconditionally. A prime whose observation began *before* a toggle can complete *after* it and overwrite the toggle's result with stale data, leaving the ribbon check box displaying the opposite of the engine's actual state.

Severity for the bundle is **High**, taken from its most severe member (Finding 2, an unhandled-exception crash path reachable from a routine UI action). Findings 1 and 3 are Medium individually.

Environment: Windows 11 Pro; C#/.NET Framework 4.8.1 WinForms VSTO Outlook add-in; Office Ribbon CustomUI 2009 schema. All three findings are static code-review findings rather than field reports, so there is no runtime repro log; each was re-verified by direct file read.

## Scope & Non-Goals

### In scope

1. Renaming four `onAction` attribute values in the Explorer CustomUI document from the `_Clicked` spelling to the `_Click` spelling, and deleting the one button element whose callback has no implementation.
2. Two new reflection-based regression tests pinning XML-to-code callback resolution and check-box callback arity.
3. A new host-neutral, fully unit-tested `SpamManagerResetGate` class, and the deferral of the engine-touching body of `ClearSpamManagerAsync` into a lambda passed to that gate.
4. A monotonic sequence ticket plus compare-and-apply cache write on the engine toggle state coordinator, replacing both unconditional writes.
5. **CR-2 (in scope, justified):** repairing prime completion so a *canceled* prime is treated as a failure — the in-flight marker is cleared and the failure is logged. This lives in the direct completion partner of the method the race fix already rewrites, is a five-line restructure in the same region, and shares the same re-prime invariant. The originating issue #525 records it as worth fixing together.
6. **CR-3 (in scope, justified):** one new test covering the existing `InvalidOperationException` guard on the toggle path when the engines accessor returns null. This is zero production change and closes the only otherwise-uncovered lines in a class whose coverage this change already moves.
7. Compile-item registration for the three new source files in the two legacy non-SDK project files. These are the only two files touched outside the ribbon directories, and the edit is unavoidable rather than discretionary: a file absent from the item group is not compiled.

### Out of scope / non-goals

- **The eight QuickFiler-settings unguarded-globals sites** on the Intelligence partial (the move-entire-conversation, save-attachments, save-pictures and save-email-copy query and toggle members, plus the high-confidence mode and threshold members). Finding 1 makes four of these reachable through a second entry point, but it does not open a new crash window: the sibling pressed-state callbacks for the same four controls already dereference the same chain unguarded and already fire when the menu is opened in the pre-initialization window. These belong to issue #524's site table, not to #735's finding 2, and are to be promoted as a separate issue.
- **The orphaned handler `BuildFolderClassifier_Click`** on the viewer type, which is public and correctly signatured but referenced by no `onAction` anywhere in the document — the inverse of Finding 1. Harmless. The enumeration test added here deliberately asserts only the XML-to-code direction, because that is the direction that produces silent user-facing breakage. Promote separately if the reverse assertion is wanted.
- **The three `NotImplementedException`-throwing bound handlers** (`TestSpamVerbose`, `SpamMetrics`, `SpamInvestigateErrors`) on the Intelligence partial, each bound to a live ribbon button. Their names resolve correctly, so they are outside Finding 1, but they are user-reachable unhandled exceptions. Promote separately.
- Implementing a MigrateToDoIDs behavior. No design document, plan, spec, or potential-feature entry anywhere in the repository proposes such a command; implementing an unspecified data migration is a feature, not a bugfix.
- Reordering the Clear Spam Manager confirmation dialog relative to the readiness check. Showing the not-ready notice before the confirmation prompt would be marginally better UX, but it reorders user-visible behavior on the already-working path for no defect-driven reason. Recorded as a rejected alternative.
- Widening the charter of the engine-callback shape fixture to host the new check-box arity test. That fixture's documented charter is the #505/#506/#518 engine toggle callbacks; the new tests go in the XML-consistency fixture instead, at the cost of one duplicated string constant.
- Any change to TaskMaster/AppGlobals/AppOlObjects.cs or TaskMaster/AppGlobals/NonBlockingDelay.cs. Those files belong to a different concurrent work item in the same parallel run and must not be touched by this change; they were not opened during research. The application globals and auto-file-objects sources, the globals interfaces, and the classifier manager source were read for investigation only and are likewise not modified.

## Root Cause Analysis

**Finding 1 — naming drift across an untyped boundary.** Office CustomUI binds callbacks by string name, resolved reflectively at invocation time, so nothing in the compiler or the loader can detect a mismatch. The `_Click` suffix is the repository-wide convention: every callback method on the viewer type uses it, and the three check boxes that already work — dark mode, SpamBayes enabled, Triage enabled — all bind to `_Click`. The XML side is the outlier, which is why the fix edits the document rather than adding four duplicate public methods to a COM-visible type. The `BtnMigrateIDs` button is a different failure of the same class: a binding was authored against an implementation that was never written, and a repo-wide case-insensitive search for any Migrate-IDs identifier returns only that one XML line plus documentation. An archived plan from a prior investigation already recorded this asymmetry as known and deferred it rather than fixing it.

**Finding 2 — no guard on an optional dependency.** The globals object is assigned only by the controller's `SetGlobals` method, and the ribbon is constructed earlier in add-in startup. Below it, the auto-file objects property returns a backing field that stays null until the basic load runs, the classifier manager is an auto-property assigned only inside the parallel and sequential load paths, and the engines facade has a private setter populated in the same load window. Three links, each independently null in a real window. The controller already concedes this elsewhere: its own engines accessor is written with a null-conditional. `ClearSpamManagerAsync` was simply never given the corresponding treatment, and it cannot be routed through the existing engine-gated command runner because that gate's predicate is inbox-engine readiness, not manager availability — the Clear Spam command is deliberately not a member of the engine command catalog and correctly declares no enabled-state callback.

**Finding 3 — unconditional last write over an asynchronous observation.** Freshness of a cached pressed-state value is determined by when its underlying activation read *began*, not by when its write lands. Both writers ignore that: each awaits the activation read and then assigns into the cache with no comparison against what is already there. Because the prime is started lazily from a cache miss during a ribbon paint and the toggle is started from a user click, the two can overlap trivially, and completion order does not track observation order. The prime marker compounds it: it is never cleared on success, and the completion handler tests only the task's exception, which is null for a canceled task — so a cancellation leaves the marker registered, blocks any re-prime for the session, leaves the cache unset, and logs nothing.

## Proposed Fix

### Finding 1 — repair the bindings, delete the dead one

Edit the Explorer CustomUI document only; no production C# changes.

- Rename four `onAction` values from the `_Clicked` to the `_Click` spelling on the four Item Sort Settings check boxes (move entire conversation, save attachments, save email copy, save pictures). Each already declares a correctly-resolving pressed-state callback, and each already has a `_Click` twin on the viewer type carrying the exact Office check-box signature `void (Office.IRibbonControl, bool)`.
- Delete the entire button element whose id is `BtnMigrateIDs`. Removal rather than implementation is justified above; nothing in the working tree contradicts removal. If stronger evidence is wanted, the atomic plan may run a history search for the introducing commit, but the fix does not depend on it.
- CSharpier formats this document (it is excluded from formatting only for project, props and targets files, not for XML), so the edit must be followed by a format run and any reflow accepted.

### Finding 2 — extract the decision into a testable gate

New production type `SpamManagerResetGate`, `internal sealed`, in namespace `TaskMaster`, placed alongside its siblings in the ribbon directory. It follows the canonical seam shape established by the file EngineReadinessGate.cs and the deferred-invocation shape established by the file EngineGatedCommandRunner.cs:

- Constructor takes `Func<IAppAutoFileObjects> autoFileAccessor`, `Func<IAppItemEngines> enginesAccessor`, and `Action<string> notifyNotReady`; each is validated with the `?? throw new ArgumentNullException(nameof(x))` form used by both precedents.
- Sole public-surface method `internal Task RunAsync(Func<ManagerAsyncLazy, IAppItemEngines, Task> reset)`. Contract: a null `reset` throws `ArgumentNullException` **before** any accessor is invoked; then the accessors are evaluated and the manager resolved through a null-conditional; if either the manager or the engines facade is null, the not-ready message is emitted exactly once, `reset` is never invoked, and a completed task is returned; otherwise `reset(manager, engines)` is returned directly, with no await and no catch block, so a fault from the deferred work propagates unchanged. This preserves the "suppresses invocation, never errors" invariant the sibling runner documents.
- One private static message builder producing a current-culture-formatted not-ready notice. It must not name a control id, because unlike the sibling runner this gate serves exactly one command.
- Usings limited to `System`, `System.Globalization`, `System.Threading.Tasks` and `UtilitiesCS`. No `Microsoft.Office` using, no `System.Windows.Forms` using, no logger field, no COM.
- **No `ExcludeFromCodeCoverage` attribute**, plus an XML-doc paragraph recording that the omission is deliberate, mirroring the equivalent paragraph on EngineReadinessGate.cs.

The controller's Intelligence partial gains a private backing field and a lazily built property inside the existing Spam Manager region, constructing the gate with `() => Globals?.AF!`, `() => Globals?.Engines!`, and the existing private not-ready notifier on the sibling EngineCommands partial. The null-forgiving operators match the established precedent and carry the same explanatory comment: a null result is a supported input that the gate treats as "not ready".

`ClearSpamManagerAsync` itself changes only in its body after the confirmation dialog: the confirmation is inverted to an early return, and the four engine-touching statements move verbatim into an async lambda passed to the gate, with the resolved manager and engines used in place of the globals chain. The synchronization-context preamble and the confirmation dialog stay exactly where they are. No ad-hoc inline null guard is added anywhere — that approach was explicitly disrecommended by the maintainer on #518 and would place the guard permanently inside the coverage-exempt region.

The classifier manager type lives in the `UtilitiesCS` namespace, which the Intelligence partial already imports, and the test project already references that project, so no new project reference is required on either side.

### Finding 3 — monotonic ticket plus compare-and-apply

On the engine toggle state coordinator:

- Add a `private long _stateSequence` read and written only through `Interlocked`, and a private nested `sealed class PressedState` holding an `Active` flag and a `Sequence` ticket. A reference type is required so that the concurrent dictionary's conditional update compares by reference identity — that is exactly the compare-and-swap semantics needed. A value tuple would degrade the comparison to structural equality, weakening the guard to "the value looked the same".
- Retype the pressed-state cache from `ConcurrentDictionary<string, bool>` to `ConcurrentDictionary<string, PressedState>` with ordinal comparison.
- Add `NextSequence()` (an interlocked increment) and `TryApplyState(engineName, active, sequence)`, an explicit compare-and-swap loop that stores an observation only when no newer observation is already cached for that key, and returns whether the write was applied. An explicit loop is used rather than an add-or-update factory because such a factory may run more than once under contention, which makes "did my write land?" non-obvious to a reader. The loop terminates: each iteration either returns or observes a strictly newer stored ticket.
- Both writers take a ticket immediately **before** invoking the activation read — on the toggle path, after the engine toggle completes, because that is the moment the observation window opens — then apply through the compare-and-apply helper and invalidate the control only when the write was applied. Update-before-invalidate ordering is preserved, so the existing ordering test continues to pass. Conditional invalidation is correct: if a write was rejected, a newer writer already stored its value and already invalidated. Unconditional invalidation would also be harmless; conditional is chosen because it keeps "invalidate if and only if the displayed value changed" as a single readable invariant and issues fewer marshalled COM calls.
- The pressed-state reader is updated to unwrap the cached observation. It keeps its `bool` return type and still never awaits, blocks, or throws.
- **CR-2:** the prime completion handler is restructured to return early only when the task ran to completion; otherwise it clears the in-flight marker and logs, synthesizing a `TaskCanceledException` when there is no exception to unwrap. The existing faulted-path behavior — including base-exception unwrapping, which an existing test asserts by reference — is preserved exactly, and the existing prime-failed message text reads correctly for a cancellation, so no new message builder is needed.

Rejected for Finding 3: an add-only write on the prime path (closes prime-versus-toggle only, and wrongly refuses a legitimate second prime after a cleared first); a lock or semaphore serializing writes (it would have to be held across an await on a configuration load, which is precisely the STA-blocking hazard the type's own header comment forbids); and extracting the versioned cache into its own class (cleaner in isolation but adds a production file, a test file and two project entries for roughly forty lines with exactly one consumer).

**Contingency:** if the coordinator source exceeds the 500-line ceiling after formatting, perform that extraction rather than trimming documentation.

## Testability and Coverage Disposition

This section is stated explicitly per repository policy: untestable COM-bound ribbon code must have its decidable logic extracted into a testable seam, with the untestable remainder called out rather than silently claimed as covered.

- **Finding 1 — fully unit-testable, no live Outlook.** The tests read the embedded CustomUI resource and reflect over the viewer type's metadata. No viewer instance is constructed and no method is invoked, so that type's type-level coverage exemption is irrelevant and no COM object is touched. Because the test project carries no Office primary interop assembly reference, parameter types are compared by full type name against the literal string for the ribbon-control interface, following two existing precedents in the same fixture and in the engine-callback shape fixture.
- **Finding 2 — split.** The new gate class is **fully unit-testable** and **must not** carry an `ExcludeFromCodeCoverage` attribute; both of its dependency types are interfaces that mock cleanly, and the one concrete type on its boundary is proven test-constructible by existing repository code whose constructor performs an async-lazy assignment without executing the factory — no disk, no COM. The roughly ten lines remaining inside `ClearSpamManagerAsync` are **not unit-testable**: they show a message box, install a WinForms synchronization context, and call classifier creation and serialization paths that touch disk. Those lines stay inside the ribbon controller's **pre-existing, already-ratified type-level coverage exemption** — this change adds no new exemption attribute anywhere and widens no existing one. They are therefore validated by the documented manual-verification step below **instead of** a coverage claim, and no coverage credit is asserted for them.
- **Finding 3 — fully unit-testable, no live Outlook.** The coordinator is host-neutral and stays non-exempt; every interleaving is driven deterministically by held task completion sources.

No test in this change may sleep, poll, read the wall clock, touch the filesystem, create a temporary file, or start a message pump, matching the existing fixture's stated discipline and repository unit-test policy.

## Test Strategy

**Finding 1** — two methods added to the existing XML-consistency fixture under a new region for this issue, plus one private constant hoisting the ribbon-control type-name literal that already appears inline in that file:

- `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod` — collects callback attribute values by the rule "local name is `onAction`, `onChange` or `onLoad`, or begins with `get`", which is exact for the 2009 CustomUI schema and future-proof against a newly introduced getter. It enumerates descendant *element* nodes only, so the eight commented-out occurrences are excluded structurally with no regex, and it includes the root element's load callback. It asserts each distinct value matches some public instance method name on the viewer type, failing with the full list of unresolved names so a single run reports all of them.
- `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters` — for every check-box action callback, resolves the method and asserts the signature `void (ribbon control, bool)`, comparing the first parameter by full type name. This pins the exact shape whose silent mis-binding produced the defect.

Both fail against the pre-fix tree — the first on five names, the second on four unresolvable names — and pass after the XML edits.

**Finding 2** — nine methods in a new MSTest fixture (MSTest, Moq, FluentAssertions): the three constructor null-argument cases asserting the offending parameter name; a null-reset case using strict mocks that would fail if any accessor were probed; three not-ready cases (auto-file accessor returns null; manager unset; engines accessor returns null) each asserting exactly one notification and that the reset delegate was never invoked; a success case asserting both lambda arguments are the same instances that were resolved and that no notification was emitted; and a faulting-reset case confirming the fault propagates unchanged with no notification. This set reaches every branch and must meet the new-module coverage target of at least 90%.

**Finding 3** — the existing coordinator fixture gains only the `partial` keyword on its class declaration (a one-word edit, following the established two-file partial pattern already used by the controller fixture in the same directory), so the new file can reuse its private nested harness and error record with no duplication. The new partial file adds:

- `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult` — the #525 reproduction. Fails before the fix.
- `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult` — toggle versus toggle. Fails before the fix.
- `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce` — guards against over-suppression by the new conditional invalidation.
- `ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine` — CR-3.
- `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` — CR-2. Fails before the fix, where nothing is logged.
- `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked` — companion assertion that the cache stays unset; may be folded into the previous method at the planner's discretion.

The existing harness needs no modification.

**Manual validation (required, Finding 2 residual only).** Launch Outlook with add-in user-interface errors shown; click Clear Spam Manager before add-in initialization completes and confirm the prompt; observe the not-ready notice instead of a `NullReferenceException`. Then repeat after initialization completes and confirm the reset still runs end to end. Record the outcome in the change description.

**Toolchain.** Run in order and restart from step 1 on any failure or auto-fix: CSharpier format (required after the XML edit) then check; the analyzer build with rebuild; the nullable build with rebuild; then the test run with code coverage enabled.

## Write Set

- `TaskMaster/Ribbon/RibbonExplorer.xml`
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs`
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`
- `TaskMaster/Ribbon/SpamManagerResetGate.cs`
- `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`
- `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs`
- `TaskMaster/TaskMaster.csproj`
- `TaskMaster.Test/TaskMaster.Test.csproj`

## Acceptance Criteria

### Finding 1 — dead XML-to-handler bindings

- [x] The Explorer CustomUI document declared **five** callback names that resolve to no public instance method on the viewer type before this change; after this change the count of such unresolved names is **zero**. Both the pre-fix and post-fix counts are demonstrated by the new enumeration test, which reports every unresolved name in a single failure message.
- [x] Exactly **four** action-callback attribute values are renamed from the `_Clicked` spelling to the `_Click` spelling, on the move-entire-conversation, save-attachments, save-email-copy and save-pictures check boxes in the Item Sort Settings menu. No method on the viewer type is added, renamed, or removed to satisfy these bindings.
- [x] Exactly **one** element is deleted: the button whose id is `BtnMigrateIDs`. No other element, attribute, or attribute value in the document is changed beyond the four renames and any CSharpier reflow.
- [x] The rename-versus-removal partition is exactly four renames plus one removal, totalling the five defective names: a name is renamed when a correctly signatured method with the intended spelling already exists, and removed when no implementation exists anywhere in the solution.
- [x] A test named `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod` exists and passes. It enumerates descendant element nodes only (so commented-out occurrences are excluded structurally), includes the root element's load callback, and treats an attribute as a callback if its local name is `onAction`, `onChange` or `onLoad`, or begins with `get`.
- [x] A test named `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters` exists and passes. It asserts every check-box action callback resolves to a method returning void and taking the ribbon-control interface followed by a bool, comparing the first parameter by full type name because the test project has no Office interop reference.
- [x] Both new tests are demonstrated to fail against the pre-fix tree — the first reporting five unresolved names, the second reporting four unresolvable check-box callbacks — before the production edit lands.

### Finding 2 — unguarded globals dereference in Clear Spam Manager

- [x] A new `internal sealed class SpamManagerResetGate` exists in namespace `TaskMaster` alongside its ribbon siblings, taking an auto-file-objects accessor, an engines accessor and a not-ready notification delegate, and throwing `ArgumentNullException` naming the offending parameter for each of the three.
- [x] The gate's `RunAsync` throws `ArgumentNullException` for a null reset delegate **before** invoking any accessor; emits the not-ready notification exactly once and returns a completed task without invoking the reset delegate when either the resolved manager or the resolved engines facade is null; and otherwise returns the reset invocation directly, with no await and no catch, so a fault propagates unchanged.
- [x] The gate carries **no** `ExcludeFromCodeCoverage` attribute, no `Microsoft.Office` using, no `System.Windows.Forms` using and no logger field, and its XML documentation records that the absence of the coverage attribute is deliberate.
- [x] `ClearSpamManagerAsync` retains its synchronization-context preamble and its confirmation dialog unchanged and in their existing order, and routes only its engine-touching statements through the gate's deferred lambda. No inline ad-hoc null guard is introduced.
- [x] All nine tests in the new gate fixture pass: three constructor null-argument cases, one null-reset case using strict accessors, three not-ready cases (null auto-file objects, unset manager, null engines), one success case asserting the resolved manager and engines are passed through by identity with no notification, and one faulting-reset case asserting the fault propagates with no notification.
- [x] Line coverage for the new gate class is at least 90%, meeting the new-module rule.
- [x] **No new `ExcludeFromCodeCoverage` attribute is introduced anywhere in the diff**, and no existing exemption is widened. The residual lines inside `ClearSpamManagerAsync` remain inside the ribbon controller's pre-existing type-level exemption, and the change description asserts no coverage credit for them.
- [ ] The change description records the manual verification: the not-ready notice is observed instead of a `NullReferenceException` when Clear Spam Manager is confirmed before initialization completes, and the reset still runs end to end when repeated after initialization completes.

### Finding 3 — toggle-state last-writer race

- [x] The pressed-state cache is a concurrent dictionary of a private nested reference type carrying an activation flag and a monotonic sequence ticket, keyed ordinally; the sequence source is read and written only through interlocked operations.
- [x] Both writers capture a ticket immediately before invoking the activation read — on the toggle path, after the engine toggle completes — and store through a compare-and-apply helper that applies a write only when no newer observation is already cached for that key, invalidating the control only when the write was applied.
- [x] The pressed-state reader keeps its `bool` return type and still never awaits, blocks, or throws; the existing update-before-invalidate ordering test continues to pass unmodified.
- [x] Prime completion treats any outcome other than ran-to-completion as a failure: the in-flight marker is cleared and the failure is logged, with a synthesized cancellation exception when there is no exception to unwrap. The existing faulted-path behavior, including base-exception unwrapping asserted by reference in an existing test, is preserved.
- [x] All six new tests in the new coordinator race file pass, and the three that reproduce defects — the prime-after-toggle race, the toggle-versus-toggle race, and the canceled-prime logging case — are demonstrated to fail against the pre-fix tree.
- [x] The existing coordinator test class declaration changes by exactly one added `partial` keyword, with no other edit to that file, so the new file reuses the existing private harness with no duplication.

### Cross-cutting

- [x] All three new source files are registered as compile items in their respective legacy non-SDK project files, and the solution builds.
- [x] Every file created or modified by this change is under the 500-line ceiling after formatting; line counts are verified for the coordinator source, the Intelligence partial, the XML-consistency fixture, and the new coordinator race file.
- [x] The full toolchain passes in order in a single pass with no failures and no auto-fixes: format, analyzers, nullable type-check, and tests with coverage.
- [x] No behavior outside the three findings changes; in particular the eight QuickFiler-settings members, the orphaned folder-classifier handler, and the three not-implemented bound handlers are left untouched and are recorded here as separate follow-ups.

## Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Repairing the four bindings makes four previously dead callbacks live, and they route to members that dereference the globals chain unguarded. | Certain | Low | Not a new crash window: the sibling pressed-state callbacks for the same four controls already dereference the same chain unguarded and already fire when the menu is opened in the pre-initialization window. The fix adds a second entry point into an already-reachable surface. Recorded and promoted as a separate follow-up rather than expanded into this issue. |
| Deleting a ribbon element removes a user-visible button. | Certain | Low | The button has never done anything; clicking it has always been a no-op because no implementation exists. No documentation, plan, or feature entry references a MigrateToDoIDs behavior. |
| The coordinator source may exceed the 500-line ceiling after formatting. | Medium | Medium | Verify line count after formatting. If exceeded, extract the versioned cache into its own class rather than trimming documentation. |
| Conditional invalidation could suppress a needed control refresh. | Low | Medium | A dedicated test asserts exactly one invalidation on the uncontended path; a rejected write means another writer already stored a newer value and already invalidated. |
| The compare-and-swap loop could spin under contention. | Low | Low | Each iteration either returns or observes a strictly newer stored ticket, so the loop terminates; the contended window is a few instructions wide. |
| The residual Clear Spam Manager lines cannot be covered by automated tests. | Certain | Low | Decidable logic is extracted into the fully tested gate; the residual lines are explicitly declared uncovered under the pre-existing exemption and validated by the documented manual step. No coverage credit is claimed for them. |
| CSharpier reflows the CustomUI document beyond the intended edits. | Medium | Low | The document is not formatter-excluded, so a format run is mandatory after the edit and its reflow is accepted; the review diff is checked to confirm no semantic attribute change beyond the four renames and one deletion. |

## Rollout & Follow-up

- No feature flag, configuration key, migration, or telemetry change. The user-visible surface changes only in that four check boxes begin working, one dead button disappears, and one command shows a not-ready notice instead of crashing.
- Follow-up issues to promote separately, as detailed in Scope & Non-Goals: the eight QuickFiler-settings unguarded-globals sites (referencing #524), the orphaned folder-classifier handler, and the three not-implemented bound handlers.
- Links: issue #735 at https://github.com/drmoisan/TaskMaster/issues/735; source issues #504, #524, #525.
