# Issue #839 Research: `CreateCancellationToken()` has no production caller on the synchronous `Init()` path

- Issue: #839 (severity High, kind bug)
- Feature folder: docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839
- Timestamp: 2026-09-12T18-05
- Method: read-only analysis with Read, Grep and Glob against the item worktree. No build, format or test was run. Bash (and therefore git history) was unavailable in this session, so no claim below rests on commit history.
- Every line number cited was re-derived from the current tree in this session, not copied from the delegation prompt.

## 1. Current State Analysis

### 1.1 The controller's two initialization paths

QuickFiler/Controllers/QfcHomeController.cs (exactly 500 lines; line 500 is the closing brace, so the file sits at the repository's 500-line ceiling) has two ways to reach a running session:

| Path | Entry | Token source established? | Where |
|---|---|---|---|
| Asynchronous | `LaunchAsync` (lines 35-84) constructs `new CancellationTokenSource()` at line 54 and passes it to `InitAsync` (lines 108-150), which assigns `_token` and `_tokenSource` at lines 116-117 before any loader runs. | Yes | lines 54, 116-117 |
| Synchronous | public ctor (lines 29-33) then `Init()` (lines 86-106) then `Run()` (lines 245-269). | No. Nothing on this path assigns `_tokenSource` or `_token`. | lines 86-106 |

`Init()` reads the unassigned fields at three sites, in this order:

- line 88: `QfcDataModelLoader(Globals, this.Token)` receives `default(CancellationToken)` (never cancellable).
- line 94: `QfcQueueLoader(this.Token, this, Globals)` receives the same default token.
- lines 102-103: `QfcFormControllerLoader(..., this._tokenSource, this._token)` receives a null source.

The factory that would establish the invariant, `internal void CreateCancellationToken()` at lines 467-471, assigns both fields (lines 469-470) and is never called from production code (Section 6 derives the count).

### 1.2 Downstream effect of the null source

QuickFiler/Controllers/QfcFormController.cs stores the source at line 39 (`_tokenSource = tokenSource;`) and exposes it at lines 187-191. QuickFiler/Controllers/QfcFormController.Actions.cs early-returns when `_tokenSource is null` in three overloads:

- line 38 inside `LoadItems(IList<MailItem>)` (lines 31-61). `Run()` calls this at QfcHomeController.cs line 263, so on the synchronous path the guard fires and no item is loaded, with no log line and no exception.
- line 75 inside `LoadItemsAsync(IList<MailItem>, ProgressTracker)` (lines 68-106).
- line 131 inside `LoadItemsAsync(IList<QfcPreScoredItem>, ProgressTracker)` (lines 121-165).

A second, quieter symptom on the same path: the Cancel teardown at QuickFiler/Controllers/QfcFormController.EventHandlers.cs line 133 runs `_parent?.TokenSource?.Cancel()`, which is a silent no-op when the parent source is null, and the datamodel and queue received a token with `CanBeCanceled == false` (Section 1.1), so cancellation could never propagate to them even if the guards were bypassed.

### 1.3 The EFC counterpart (maintainer's asymmetry evidence)

QuickFiler/Controllers/EfcHomeController.cs declares its own `CreateCancellationToken()` at lines 399-403 with an identical body and calls it on every construction path: line 62 (the internal constructor, before `DataModelFactory` receives `this.TokenSource` at line 69), line 126 (`CreateAsync`), and line 162 (`LoadFinderAsync`). The Qfc controller declares the same method and calls it nowhere. Verified: this is an asymmetry between two otherwise parallel controllers, not a case of one controller simply lacking the member.

### 1.4 Disposal ownership already in place

`QfcHomeController.Cleanup()` (lines 371-410) disposes and nulls the source at lines 389-390 (`_tokenSource?.Dispose(); _tokenSource = null;`). `Cleanup` is handed to the form controller as `parentCleanup` at line 100 (synchronous) and line 142 (asynchronous). `QfcFormController.Cleanup()` in QuickFiler/Controllers/QfcFormController.SetupDisposal.cs (lines 213-273) invokes that callback under `finally` at lines 269-271. So a source created on the synchronous path is disposed by the same code that already disposes the asynchronous path's source. Three existing tests pin this: QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs lines 79-126 (disposal), 136-156 (field nulled after cleanup, issue #810 AC3), 171-190 (exactly-once release).

### 1.5 Nullable participation

The file carries no `#nullable` directive. The 28 QuickFiler files that do opt in (all Breadcrumb* / WebView2* / EfcSelectionGuard.cs, each at line 1) are unrelated to this controller. Nullable enforcement in this repository is per-file opt-in; CI's `/p:TreatWarningsAsErrors=true` build promotes CS86xx diagnostics only in files that opted in.

## 2. R1: Reachability of the synchronous path

### 2.1 Exhaustive caller search for `LoadQuickFiler`

Search: regex `LoadQuickFiler\b|LaunchQuickFiler\b`, content mode, entire repository excluding only docs/ and artifacts/ (which hold prose and coverage evidence, not shipped code). Every file type was included: `.cs`, `.xml`, `.resx`, `.Designer.cs`, `.config`, `.csproj`, `.json`, `.ps1`.

Hits outside docs/ and artifacts/, in full:

1. TaskMaster/Ribbon/RibbonController.cs line 97: the declaration `internal void LoadQuickFiler()`.
2. TaskMaster/AddInUtilities.cs line 15: interface member `void LaunchQuickFiler();`.
3. TaskMaster/AddInUtilities.cs line 44: `public void LaunchQuickFiler()`, whose body at line 48 is `_ = _ribbonController.LoadQuickFilerAsync();` (the asynchronous entry).

There is no call site of `LoadQuickFiler()` anywhere. The prompt's fact 8 is confirmed.

### 2.2 Ribbon surface

TaskMaster/Ribbon/RibbonExplorer.xml wires two QuickFiler buttons: `onAction="QuickFiler_Click"` (line 214) and `onAction="QuickFilerHighConfidence_Click"` (line 222). TaskMaster/Ribbon/RibbonViewer.cs implements them at lines 151-157 (`QuickFiler_Click` calls `_controller.LoadQuickFilerAsync()` at line 155) and lines 159-165 (`QuickFilerHighConfidence_Click` calls `_controller.LoadQuickFilerHighConfidenceAsync()` at line 163). Both RibbonController methods (lines 112-125 and 133-146) go through `QfcHomeController.LaunchAsync` at lines 118 and 139. No ribbon callback reaches the synchronous method.

### 2.3 Other routes to `Init()`

- Reflection or callback-name strings: regex `"Init"|nameof\(Init\)|"LoadQuickFiler"|"CreateCancellationToken"` across the repository outside docs/ and artifacts/ matched only unrelated `UiThread.Init` / `StoreWrapper.Init` prose in UtilitiesCS (UtilitiesCS/Threading/UiThread.cs lines 241 and 248; UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs line 162; UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs line 156). No string-based dispatch names any member of this family.
- Direct `.Init()` calls across all `.cs` files: the only calls on a QuickFiler home controller are TaskMaster/Ribbon/RibbonController.cs line 107 (inside the uncalled `LoadQuickFiler`) and QuickFiler.Test/Controllers/QfcHomeControllerTests.cs line 148 (a test). QfcHomeController.cs line 226 is `QfcFormController.Init()`, a different method.
- Projects outside QuickFiler and QuickFiler.Test that reference `QfcHomeController`: a files-with-matches search excluding docs/, artifacts/, QuickFiler/ and QuickFiler.Test/ returned exactly one production source file, TaskMaster/Ribbon/RibbonController.cs (the remaining hits are agent-memory markdown, one Pester test that greps assembly names, and a UtilitiesCS.Test comment at UtilitiesCS.Test/Threading/UiThread_Tests.cs line 395 that mentions a QuickFiler test class). TaskMaster/ThisAddIn.cs contains no QuickFiler reference.
- `Init()` is declared on the public interface `IQfcHomeController` (QuickFiler/Controllers/IQfcHomeController.cs line 12). RibbonController holds the instance as `IFilerHomeController` (RibbonController.cs line 42), whose interface (QuickFiler/Interfaces/IFilerHomeController.cs) does not declare `Init()`, so no interface-typed caller can reach it either.

### 2.4 Verdict

The synchronous `Init()` -> `Run()` path is not reachable from any shipped entry point. Its only production caller, `RibbonController.LoadQuickFiler()`, has zero callers; the ribbon XML, the ribbon viewer, the COM-exposed `AddInUtilities.LaunchQuickFiler`, and every other assembly in the solution reach QuickFiler exclusively through `LaunchAsync`, which is sound. The defect is real (the public `Init()` contract on a public interface produces a controller that silently loads nothing) but it is latent, not live. Git history could not be consulted this session to establish when the synchronous caller was removed.

## 3. R2: Candidate Remedies

### 3.1 (a) Call the existing factory from the synchronous path -- RECOMMENDED

- Invariant established: `_tokenSource` and `_token` are non-null and linked before any loader on the synchronous path observes them; the synchronous path becomes structurally identical to `InitAsync` (which assigns both fields at lines 116-117 as its first two statements) and to the EFC controller (line 62).
- Enforcement point: use time, at the start of `Init()`. The call must precede line 88, not merely line 102, or the datamodel (line 88) and queue (line 94) keep receiving a never-cancellable default token (Section 1.2).
- Cost: one statement in `QuickFiler/Controllers/QfcHomeController.cs`. Because that file is at exactly 500 lines, the same diff must remove one line elsewhere in the file to stay within the repository ceiling. Line 465 (`//public QfcFormViewer FormViewer { get => _formViewer; }`) is a commented-out declaration with no reader; line 41 is a commented-out debug log. Either is a safe deletion; CSharpier does not remove comments, so this must be done by hand in the diff.
- What it does not cover: it does not make the guards in QfcFormController.Actions.cs fail loudly if some future path again forgets the call; the guards remain silent no-ops. It also does not remove the dead synchronous entry point (Section 3.4).
- Alignment: matches the EFC precedent exactly, keeps the loader seams unchanged, requires no interface change, no csproj change, and no nullable directive.

### 3.2 (b) Make the token source non-nullable, enforced at construction -- REJECTED

- Invariant: `_tokenSource` non-null from constructor exit.
- Enforcement point: construction time (both constructors, lines 27 and 29-33).
- Costs and conflicts found by reading:
  1. `LaunchAsync` creates its own source at line 54 and `InitAsync` overwrites the field at line 117. A constructor-created source would be orphaned undisposed on every asynchronous launch unless `InitAsync` first disposes it, which adds lines to a file already at the 500-line ceiling and introduces a second source lifetime that `ProgressTracker` (bound at line 56 to the LaunchAsync source) does not track.
  2. `Cleanup()` nulls the field at line 390, and QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs lines 136-156 (issue #810 AC3) requires that nulling so a later `Cancel()` cannot reach a disposed source. A non-nullable field contradicts a ratified acceptance criterion; the field is nullable by design after cleanup.
  3. Twenty-five test construction sites (Section 4.2) would each allocate a real `CancellationTokenSource` that no test disposes, and QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs lines 262-272 injects a `Mock<CancellationTokenSource>` by reflection, which still works but makes the constructor-created source dead weight.
- Does (b) require a nullable directive? Not for the runtime behaviour: assigning in both constructors is a discipline the compiler cannot check without `#nullable enable`. If the directive were added to obtain compiler enforcement, reading the file yields approximately 37 diagnostics that `/p:TreatWarningsAsErrors=true` would promote to errors:
  - CS8618 (non-nullable member left unassigned at constructor exit): 13 members declared in this file without initializers (`_explorerController` 421, `_formController` 428, `_keyboardHandler` 434, `_datamodel` 441, `_uiScheduler` 450, `_stopWatchMoved` 456, `_stopWatch` 457, `_formViewer` 463, `_tokenSource` 473, `_uiSyncContext` 492, `Globals` 152, `QfcQueue` 153, `ParentCleanup` 154). The private ctor (27) leaves all 13 unassigned; the public ctor (29-33) assigns two. 13 + 11 = 24.
  - CS8625 (null literal to non-nullable): line 38 default parameter, lines 390-396 (seven field/property nullings), line 406. 9.
  - CS8600 (null to non-nullable local): line 80, line 289. 2.
  - CS8603 (possible null return): line 83. 1.
  - CS8604 (possible null argument): line 322, because flow analysis does not correlate the two separate `if (highConfidenceModeEnabled)` checks at 290 and 317. 1.
  - Total approximately 37. Members declared in the Metrics and Iteration partials (for example `TimeProvider` at QuickFiler/Controllers/QfcHomeController.Metrics.cs line 19) are in an oblivious context and would not add CS8618. This estimate is from reading only; it cannot be confirmed without a build.
- Verdict: the invariant (b) claims to enforce is already false by design after `Cleanup()`, and the asynchronous path would have to be restructured to avoid a leak. Rejected.

### 3.3 (c) Remove the early returns -- REJECTED

- Invariant: none. It removes a guard without establishing the thing the guard protects.
- The guards at Actions.cs lines 33-40, 70-77 and 126-133 also cover `listObjects`/`preScored`, `_globals`, `_formViewer`, `_parent` and `_states`. Each has a legitimate null path: `_states` is assigned only by `Init()` -> `CaptureItemSettings()` at QuickFiler/Controllers/QfcFormController.SetupDisposal.cs line 37, so a `LoadItems` call before `Init()` would dereference null; and `Cleanup()` nulls `_globals`, `_formViewer` and `_parent` at SetupDisposal.cs lines 252-257, so a late `LoadItems` after teardown relies on these guards.
- Removing only the `_tokenSource` clause moves the failure rather than fixing it: `QfcCollectionController` stores the null source at QuickFiler/Controllers/QfcCollectionController.cs line 42 without checking it, and `QfcItemController` reads `_homeController.TokenSource` at QuickFiler/Controllers/QfcItemController.Initialization.cs line 386 and hands it to `ConversationResolver` at lines 393-399. The user-visible result would change from "nothing loads" to a null-reference exception deep in item construction, with the root cause still unaddressed.
- Verdict: not defensible. Rejected.

### 3.4 (d) Delete the dead synchronous path (fourth candidate, found during R1) -- DEFER TO FOLLOW-UP

Given the R1 verdict, the most thorough remedy is removal: `RibbonController.LoadQuickFiler()` (TaskMaster/Ribbon/RibbonController.cs lines 97-110), `QfcHomeController.Init()` (lines 86-106), `IQfcHomeController.Init()` (QuickFiler/Controllers/IQfcHomeController.cs line 12), `CreateCancellationToken()` (lines 467-471 once it has no test caller), and the test `Init_InitializesCorrectly` (QuickFiler.Test/Controllers/QfcHomeControllerTests.cs lines 112-163). `Run()` must stay: it is declared on `IFilerHomeController` (line 15) and `EfcHomeController.Run()` is live at RibbonController.cs lines 236, 246 and 256. This spans two projects, removes a member from a public interface, and is feature work under the bugfix workflow's "do not widen scope" rule. It should be filed as a separate issue.

### 3.5 (e) Fail fast at the consumer's constructor (fifth candidate) -- OPTIONAL HARDENING, NOT IN THE RECOMMENDED DIFF

The issue's expected behaviour accepts "the missing token source is reported as an error at construction time". A `tokenSource.ThrowIfNull()` (the extension EfcHomeController already uses at line 61) at QuickFiler/Controllers/QfcFormController.cs line 39 would make any future recurrence loud. All nine test construction sites of `QfcFormController` pass a real `_tokenSource` (QfcFormControllerDeactivateTests.cs 61-70, QfcFormControllerTests.cs 77-86 and 120-129, QfcFormControllerSeamTests.cs 66-75, QfcFormControllerCleanupTests.cs 62-71 and 412-421, QfcFormControllerCancelTeardownTests.cs 93-102, QfcFormControllerUndoHandoffTests.cs 150-159), so no existing test would break. It is not required to fix #839 and is left out of the recommended blast radius so the planner can adopt or defer it explicitly.

### 3.6 Does the answer depend on the R1 verdict?

Only in emphasis. If the path were live, (a) would be mandatory and urgent. Because the path is dead, (a) is still the correct minimal fix: it restores the public `Init()` contract at one line of cost and mirrors the EFC precedent, while (d) is the eventual clean-up. (b) and (c) are rejected under either verdict for the reasons above.

### 3.7 Rejected alternatives summary

(b) contradicts issue #810 AC3 and leaks on the asynchronous path; (c) removes guards that protect pre-Init and post-Cleanup calls and relocates the failure into item construction; (d) is correct but out of bugfix scope; (e) is compatible hardening, optional.

## 4. R3: Caller and Construction-Site Enumeration

### 4.1 Sites that remedy (a) changes

| File | Line(s) | Change |
|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | 86-88 | Insert `CreateCancellationToken();` as the first statement of `Init()`, before the datamodel loader at line 88. |
| `QuickFiler/Controllers/QfcHomeController.cs` | 465 (or 41) | Delete one dead comment line so the file stays at or under 500 lines. |
| `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` | after 163 | Add the regression test (Section 5). File is 275 lines. |

Sites remedy (a) leaves unchanged but that a reviewer must know about:

- QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs line 124 (`controller.CreateCancellationToken();`): the only existing caller. It builds the controller via the public ctor (line 123) and never calls `Init()`, so it still needs the explicit call. Unchanged. Line 388 (`controller.TokenSource.Cancel()`) depends on it.
- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs lines 112-163 (`Init_InitializesCorrectly`): the only existing test that calls `Init()` (line 148). After (a) it will allocate a real source it does not dispose; the new test can call `Cleanup()` at the end, or the planner may add that call here too.

### 4.2 Every construction site of `QfcHomeController`

Production: QuickFiler/Controllers/QfcHomeController.cs line 50 (`LaunchAsync`, private ctor); TaskMaster/Ribbon/RibbonController.cs lines 104-107 (public ctor, inside uncalled `LoadQuickFiler`).

Tests (all public ctor): QfcHomeControllerPropertyTests.cs lines 59, 83, 108, 135, 159, 176, 196, 210, 234, 258, 282, 307, 324; QfcHomeControllerTests.cs lines 51, 93, 116; QfcHomeControllerMetricsTests.cs line 123; QfcHomeControllerRunAsyncTests.cs line 61; QfcHomeControllerIterationTests.cs line 51; QfcHomeControllerIssue218Tests.cs line 39; QfcHomeControllerCleanupTests.cs lines 52, 89, 140, 175. Total: 2 production + 23 test = 25 construction sites. Remedy (a) changes none of them, because it acts in `Init()`, not the constructor; this is the main reason (a) is cheaper than (b).

### 4.3 Tests that touch the token fields directly

QfcHomeControllerPropertyTests.cs lines 262-275 (`_tokenSource` via reflection, `Mock<CancellationTokenSource>`) and 286-300 (`_token` via reflection); QfcHomeControllerIterationTests.cs line 468 (`_token` via reflection); QfcHomeControllerCleanupTests.cs lines 95 and 144 (`_tokenSource` via reflection); QfcHomeControllerTests.cs lines 181-225 (`InitAsync` with a caller-supplied source). None calls `Init()`, so none is invalidated by (a).

## 5. R4: Test Seams

- Test project: QuickFiler.Test. QuickFiler.Test/QuickFiler.Test.csproj references MSTest.TestFramework 4.4.0.0 (line 361), Moq 4.20.72.0 (line 370) and FluentAssertions 8.10.0.0 (line 248). Confirmed MSTest + Moq + FluentAssertions.
- Visibility: QfcHomeController.cs line 15 grants `InternalsVisibleTo("QuickFiler.Test")`, so the internal loader properties and `CreateCancellationToken` are reachable.
- Existing arrangement to reuse: `Init_InitializesCorrectly` (QuickFiler.Test/Controllers/QfcHomeControllerTests.cs lines 112-163) already replaces all five synchronous loaders (`QfcDataModelLoader` 122, `QfcExplorerControllerLoader` 125, `QfcKeyboardHandlerLoader` 129, `QfcQueueLoader` 133, `QfcFormControllerLoader` 136-145) with lambdas returning loose Moq objects, calls `Init()` at 148, and asserts only the returned components. It does not observe the token arguments, which is why the defect has never been caught.
- How the regression test observes the defect without Outlook: assign a `QfcFormControllerLoader` lambda that captures its `tokenSource` and `token` parameters into locals, plus `QfcDataModelLoader` and `QfcQueueLoader` lambdas that capture their `CancellationToken` parameter; call `Init()`; then assert with FluentAssertions that the captured source is not null, that the captured token equals `capturedSource.Token`, that `_controller.TokenSource` is the same instance, and that the datamodel and queue tokens equal that same token with `CanBeCanceled == true`. The last assertion is what pins the ordering rule in Section 3.1: a fix that inserts the call after line 88 passes the null check but fails `CanBeCanceled`. Pre-fix, the first assertion fails (`tokenSource` is null); post-fix all pass. This is deterministic, uses no timers, no filesystem and no COM.
- Known pre-existing debt the new test inherits: `Init()` at line 90 constructs a real `QfcFormViewer` (a `Form`, QuickFiler/Viewers/QfcFormViewer.cs lines 18-26, constructor runs `InitializeComponent()` and captures the ambient synchronization context). The existing `Init_InitializesCorrectly` already does this in the same test class and the structural guard QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs (lines 17-36) checks only that no `Form`-derived type is compiled into the test assembly, not that none is constructed. The regression test therefore introduces no new category of test behaviour, but the planner should record the inherited live-form construction as an explicit exception in the change description rather than silently.
- Disposal in the test: `_controller.Cleanup()` at the end disposes the created source (lines 389-390); with the loose mocks in this class, `Cleanup()` touches `_formViewer?.Worker` on the real viewer, `_datamodel?.Cleanup()` on a loose `Mock<IQfcDatamodel>`, and invokes the `Mock<System.Action>` parent cleanup, none of which throws.

## Numeric Derivation Evidence

The numeric claim is the size of the family of `CreateCancellationToken` declarations and call sites in compiled C# source across the repository, and the derived sub-count of production call sites on the Qfc controller (zero). The count is the finding: one Qfc declaration, one Efc declaration, three Efc production calls, one Qfc test call, zero Qfc production calls.

- Complete Family: CreateCancellationToken
- Exhaustive Search Scope: the entire repository working tree, every file type (C# source, ribbon XML, resx, config, csproj, markdown, Cobertura coverage evidence), with no path excluded at search time; results were then classified by file kind under the inclusion and exclusion rules below
- Inclusion Rules: a hit counts as a family member when it is in a compiled `.cs` file under a project directory and is either the method declaration or an invocation of that method; declaration and invocation are counted as separate members; test-project invocations are included
- Exclusion Rules: hits in markdown prose (issue, spec, epic, research, agent-memory files), Cobertura `<method name="CreateCancellationToken">` elements under docs/**/evidence/, and the differently-named framework member `TimeProviderTaskExtensions.CreateCancellationTokenSource` (matched by the primary regex's trailing word boundary only when followed by a non-word character, so it did not enter the primary set; it is excluded here explicitly for the record) are not family members
- Primary Search Strategy or Query Expression: ripgrep content search for the regex CreateCancellationToken\b over every file type in the entire repository with no glob filter, then retain only hits whose path ends in .cs, then read each retained line to classify it as declaration or invocation
- Cross-check Search Strategy or Query Expression: ripgrep content search for the regex CreateCancellationToken\s*\( restricted by ripgrep's C# file-type filter across the entire repository, followed by a full read of QuickFiler/Controllers/QfcHomeController.cs, QuickFiler/Controllers/EfcHomeController.cs lines 40-180 and 380-440, and QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs lines 95-135 to confirm each hit's kind and to look for any invocation the regex could have missed (for example one split across lines)
- Primary Member Set: QuickFiler/Controllers/QfcHomeController.cs:467 (declaration), QuickFiler/Controllers/EfcHomeController.cs:399 (declaration), QuickFiler/Controllers/EfcHomeController.cs:62 (invocation), QuickFiler/Controllers/EfcHomeController.cs:126 (invocation), QuickFiler/Controllers/EfcHomeController.cs:162 (invocation), QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:124 (invocation)
- Cross-check Member Set: QuickFiler/Controllers/QfcHomeController.cs:467 (declaration), QuickFiler/Controllers/EfcHomeController.cs:399 (declaration), QuickFiler/Controllers/EfcHomeController.cs:62 (invocation), QuickFiler/Controllers/EfcHomeController.cs:126 (invocation), QuickFiler/Controllers/EfcHomeController.cs:162 (invocation), QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:124 (invocation)
- Primary Count: 6
- Cross-check Count: 6
- Member-set Comparison: the two member sets are identical after normalizing path separators and case; both contain the same six file:line members, and in both the subset of production invocations on QfcHomeController is empty (0), which is the defect

Derived sub-counts (both strategies agree): declarations 2; production invocations 3, all on EfcHomeController; test invocations 1, on QfcHomeController; production invocations on QfcHomeController 0.

## 7. R5: Disposal and Ownership under remedy (a)

- Creator: `Init()` via `CreateCancellationToken()` (lines 467-471).
- Owner: the `QfcHomeController` instance, through `_tokenSource` (line 473), exactly as on the asynchronous path after line 117.
- Disposer: `QfcHomeController.Cleanup()` lines 389-390, already present, already tested (QfcHomeControllerCleanupTests.cs lines 79-126 and 136-156), and already reached from the form controller's teardown via the `parentCleanup` callback passed at line 100 and invoked at QfcFormController.SetupDisposal.cs lines 269-271 under `finally`. Cancel teardown `ActionCancelAsync` (QfcFormController.EventHandlers.cs lines 126 onward) cancels the parent source at line 133 before cleanup.
- No new disposal code is needed and no undisposed source is introduced. The only new undisposed allocation is inside tests that call `Init()` without `Cleanup()`, addressed in Section 5.

## 8. R6: Contention

Files a diff must touch:

- `QuickFiler/Controllers/QfcHomeController.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`
- `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/spec.md`
- `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/issue.md`

The diff does not touch QuickFiler/Controllers/QfcHomeController.Metrics.cs (its only token reference is a comment at line 194; `TimeProvider` at line 19 is unaffected), QuickFiler/Controllers/QfcCollectionController.cs (it stores whatever source it is given at line 42 and needs no change once the source is non-null), or QuickFiler/Controllers/QfcItemController.cs (the `TokenSource` read is in the Initialization partial at line 386 and needs no change). No finding requires any of those three files. The diff also does not touch QuickFiler/Controllers/QfcFormController.Actions.cs (remedy (c) rejected), QuickFiler/Controllers/QfcFormController.cs (remedy (e) not adopted), QuickFiler/Controllers/EfcHomeController.cs, TaskMaster/Ribbon/RibbonController.cs (remedy (d) deferred), QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs, or either project file.

## 9. R7: Project Files

- QuickFiler/QuickFiler.csproj is a legacy (non-SDK) project: `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` at line 13, `<LangVersion>preview</LangVersion>` at line 14, and an explicit `<Compile Include>` list spanning lines 288-474 (QfcHomeController.cs at line 330; the Metrics and Iteration partials at 331-332).
- QuickFiler.Test/QuickFiler.Test.csproj is likewise legacy: `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` at line 18, explicit `<Compile Include>` list at lines 58-229 (QfcHomeControllerTests.cs at line 183), `<None Include="packages.config" />` at line 233, and direct `<Reference Include>` entries for FluentAssertions (248), MSTest.TestFramework (361) and Moq (370).
- Consequence: any new `.cs` file requires a matching `<Compile Include>` entry in the owning csproj or it is silently not compiled. The recommended diff adds no new file (the test goes into the existing 275-line QfcHomeControllerTests.cs), so neither csproj is edited. If the planner instead chooses a new test file, QuickFiler.Test/QuickFiler.Test.csproj must gain one line in the 58-229 block and enters the blast radius.

## 10. Behavior Semantics

- Success: after `Init()` returns, `TokenSource` is non-null, `Token == TokenSource.Token`, `Token.CanBeCanceled` is true, and the datamodel, queue and form controller loaders all received that same token/source. `Run()` -> `LoadItems` then proceeds past the guard at Actions.cs line 38.
- Failure (pre-fix): `TokenSource` null, `Token` default, guards fire, no items, no signal.
- Ordering rule: source creation is the first statement of `Init()`, before line 88.
- Edge cases: calling `Init()` twice creates a second source and orphans the first; the asynchronous path has the same property (a second `InitAsync` overwrites at 117) and no caller does either, so this is recorded, not remediated. `Cleanup()` after `Init()` disposes and nulls the source (lines 389-390), and a later `ActionCancelAsync` is a guarded no-op at EventHandlers.cs lines 128 and 133, consistent with issue #810 AC3.

## 11. Requirements Mapping to the Spec's Acceptance Criteria

| Spec AC (spec.md lines 110-117) | Design element |
|---|---|
| Repro steps produce the expected behaviour | `Init()` calls `CreateCancellationToken()` first; `LoadItems` no longer early-returns on the synchronous path |
| Regression test added and passing | new `[TestMethod]` in `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` per Section 5 (name suggestion: `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt`) |
| Edge cases handled | ordering assertion via `CanBeCanceled`; disposal via existing `Cleanup()` |
| No unintended behaviour change outside scope | asynchronous path untouched; 25 construction sites untouched; interfaces untouched |
| Logs/telemetry | none required; the fix removes a silent no-op rather than adding a log |
| Full toolchain pass | format, analyzers, nullable (file remains oblivious, so no new CS86xx), vstest |
| Docs updated | spec.md and issue.md in this feature folder; file a follow-up issue for remedy (d) and, optionally, (e) |

## 12. Testing Implications

- One deterministic regression test as specified in Section 5, in the existing test class, MSTest + Moq + FluentAssertions, no timers, no filesystem, no COM.
- Existing `Init_InitializesCorrectly` remains valid and should gain a trailing `Cleanup()` only if the planner wants the source disposed there too; not required for correctness.
- Coverage: lines 467-471 of QfcHomeController.cs are already covered by the Metrics test; the new call inside `Init()` is covered by both `Init()` tests. No coverage decrease on changed lines.
- Not testable in unit scope and explicitly out of scope: the ribbon route, because `LoadQuickFiler()` has no caller (Section 2) and `RibbonController` is `[ExcludeFromCodeCoverage]` (RibbonController.cs line 36).
