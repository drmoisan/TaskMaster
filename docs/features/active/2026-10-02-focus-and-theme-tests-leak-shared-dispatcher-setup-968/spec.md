# 2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup (Spec)

- **Issue:** #968
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-02T07-30
- **Status:** Approved for planning (amended 1.1)
- **Version:** 1.1
- **Amendment 1.1 (2026-10-02, orchestrator correction applied by the planner):** the determinism invariant (decision 4) is strengthened from "every invocation occurs while its caller holds a transaction" to "every pin is acquired and released while its caller holds a transaction, so the pin lifetime nests inside the gate hold"; the second-caller transaction test (R4) loses the baseline pin that #950 added, because that pin was released after the gate and installed a transaction value over a pinned parked value; the census acceptance criterion classifies each invocation as acquired and released inside a held transaction with no install between acquisition and release; the acceptance criteria carry `ACn:` labels for mechanical check-off; the no-production-change criterion names the inherited committed set; the evidence list admits the plan's per-task artifacts. No acceptance criterion is weakened.
- **Work Mode:** full-bug (this file is the sole acceptance-criteria source; no `user-story.md` exists for this item)
- **Research record:** docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md (section numbers cited below refer to that document)

## Context
Tests in `QuickFiler.Test` `QfcItemController_FocusAndThemeTests` set up the shared UI-thread dispatcher (through `UiThreadDispatcherFixture.EnsureDispatcher()`) and do not release that setup. Under the parallel test regime, another test class that releases or resets the shared dispatcher can leave a theme test running against a null dispatcher. The #950 preparation found the exposure: its transaction-test fix releases a dispatcher pin at the end of the test, and the same exposure already exists through two other tests in that file.

The research record (sections 0, 1 and 3) narrows the root cause and corrects one premise of the issue:

- The fixture pin is not reference counted. `UiThreadDispatcherFixture.EnsureDispatcher()` (fixture file lines 122-138) seeds the parked dispatcher only when the static is null and returns a scope that remembers only whether that single call installed it. A second concurrent caller receives a no-op scope. Whichever caller installed the value nulls it on `Dispose` (lines 269-272, `CompareExchange(_installed, null)`) regardless of how many other callers still hold a scope. This is the shared state the issue asks to "find and own".
- The two theme tests never read the shared static. `SetThemeDark(bool)` / `SetThemeLight(bool)` call `Theme.SetQfcTheme(async: true)`, which dispatches through the theme's injected `_uiDispatcher`; the tests build that theme with `QfcItemControllerTestSupport.BuildColorTheme`, which injects a `Mock<IUiDispatcher>` whose `InvokeAsync` returns a completed task. The `EnsureUiThreadDispatcher()` calls at focus-and-theme test file lines 452 and 468 are dead arrangement whose only effect is an unpinned write into the shared static (the W1 writer that #950 identified).

Environment:
- OS/version: Windows 11 (local) and windows-latest (CI)
- Python version: n/a (C# / MSTest, Workers=0, Scope=ClassLevel)
- Command/flags used: standard MSTest coverage route (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, runsettings `scripts/vscode/TaskMaster.cli.runsettings`)
- Data source or fixture: `UiThreadDispatcherFixture`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce (as filed):
1. Run `QuickFiler.Test` in parallel.
2. Have a class that resets the shared dispatcher run concurrently with `QfcItemController_FocusAndThemeTests`.
3. A theme test can observe a null dispatcher. This is intermittent.

Deterministic reproduction (replaces step 3; research section 5, test 1): on a single thread, under a `UiThreadDispatcherFixture` transaction that installed a null baseline, take two ensure pins, dispose the first, and read `UiThreadDispatcherFixture.Current`. On the unmodified fixture the field is already null after the first release: the first pin installed the parked dispatcher, the second pin was a no-op scope, and the first pin's `Dispose` ran `CompareExchange(parked, null)`. No scheduling is involved, so the failure is reproducible on every run.

Expected:
Each test class acquires and releases the shared dispatcher through a scoped, reference-counted pin, so no class can null it while another still depends on it.

Actual:
The theme tests depend on dispatcher state they do not own or pin, and the fixture nulls the shared static on the first installer's release regardless of other live holders.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: #950 preparation report; its plan carries the stop marker `THEME TEST NULL-DISPATCHER EXPOSURE OBSERVED`.
- Fail-before evidence for this item is produced at plan execution time by running the regression test against the unmodified fixture (see Test Strategy); the expected FluentAssertions failure text is of the form `Expected afterFirstRelease to refer to Dispatcher { ... Name = "UiThreadDispatcherFixture.ParkedDispatcher" } because a holder that did not take the last pin must not lose the dispatcher, but found <null>.`


## Scope & Non-Goals
- In scope:
  - Reference-count the ensure pin inside `UiThreadDispatcherFixture` (orchestrator decision 1; research Approach A).
  - Delete the two dead `EnsureUiThreadDispatcher()` calls in `QfcItemController_FocusAndThemeTests` (`SetThemeDark_FromNormal_SelectsDarkNormalTheme`, `SetThemeLight_FromNormal_SelectsLightNormalTheme`) and reword their arrange comment.
  - Add the new test class `QfcItemController_UiThreadDispatcherPinCountTests` (one fail-before regression test plus three specification tests) and its `<Compile Include>` entry.
  - Related defects in the touched files, brought into scope under the maintainer's related-defect remediation directive of 2026-10-02 (research section 6): fixture comment drift (D1), wrapper doc drift (D2), the second-caller transaction test's obsolete doc invariant (D3), the removal of that test's baseline pin, which was acquired inside its gate hold but released after it (R4 restructure, amendment 1.1), that test's missing `try/finally` around its first transaction (D4), the duplicated private `BuildExecutingViewer` helper (D5), the theme-test arrange comment (D6), and file-size accounting for every touched file (D7).
  - A call-site census proving the gated-caller invariant (Proposed Fix, "Boundaries and invariants").
  - Baseline and final toolchain and coverage evidence as Markdown projections.
- Out of scope / non-goals:
  - Any production code change. Every change is under `QuickFiler.Test`.
  - `QfcItemControllerTestSupport.EnsureSynchronizationContext` (D8, research section 6): it installs a plain `SynchronizationContext` on the MSTest worker thread and never restores it. It is observed and intentionally unchanged because it touches the per-thread ambient context, not the shared dispatcher static; it is documented as deliberate and relied on by handler tests; and changing it would alter the premise of unrelated tests (#950 recorded the ambient context as an execution-time observation only).
  - Other test assemblies' own mechanisms over the same static (UtilitiesCS.Test `UiThreadDispatcherScope`, `UiThreadStateScope`): they never reference the QuickFiler fixture and run in their own AppDomain under the MSTest adapter (cited from #950 research, not re-verified).
  - Making `EnsureDispatcher` take the transaction gate: rejected by the fixture's design note (callers without `[Timeout]` would block without bound) and not re-litigated.
  - Serialising the suite: `[DoNotParallelize]`, `Workers=1`, retries, `Thread.Sleep`, `Task.Delay`, timeout increases and temporary files are prohibited.
- Explicitly excluded systems, integrations, or datasets: none beyond the above; no Outlook, COM, or filesystem dependency is involved.

## Root Cause Analysis
This is the same family as #950 and #882: raced static test-fixture state. It must not be fixed with `[DoNotParallelize]`, Workers=1 or retries. Find and own the shared state. Sequence it after #950 merges, because #950 introduces the pin.

Precise mechanism (research sections 1.1 and 2.2):

1. `EnsureDispatcher()` keeps no count of live pins. Its scope records only `_installed` (the dispatcher this one call wrote, or null).
2. The first pin on a null field installs the parked dispatcher; every later pin while the field is non-null installs nothing and is a no-op on dispose.
3. When the first pin's scope is disposed, `CompareExchange(_installed, null)` nulls the field even though other scopes are still live. Any holder that depended on the non-null value now reads null.
4. The theme tests contribute to this by discarding their scope (unpinned write W1, never reverted) while never reading the static themselves. The holders that do read the static and can be hurt are the fixture tests (second-caller transaction test R4 pins a baseline inside its gate but releases that pin only after its gate is released and the waiter has completed, so the pin outlives the gate hold, and it installs a transaction value over the pinned parked value; this item removes that pin. Single-pin tests R2 and R3 null the field on release when the baseline was null).

Family note: the W-numbered writers (W1 unpinned ensure, W2 ensure-scope dispose, W3/W4 transaction install/restore, W5 `UiThread.Initialize`, W6 `UiThread.ResetForTesting`) follow #950's numbering and are listed in research section 1.3.


## Proposed Fix

### Design summary (what changes where):

Ratified orchestrator decisions and their rationale:

1. **Fix design: Approach A, reference-count the pin in the fixture, delete the dead theme-test calls.** `UiThreadDispatcherFixture` gains two private statics guarded by `FieldLock`: a pin counter and an install-ownership flag. `EnsureDispatcher()` increments the counter under `FieldLock`; if the field is null it writes the parked dispatcher and sets the ownership flag. `EnsureScope.Dispose()` (idempotent through `_disposed`) decrements under `FieldLock`; when the count reaches zero, the ownership flag is set, and the field still references the parked instance, it writes null and clears the flag. The decrement and the conditional null write are performed inline in the same `lock (FieldLock)` block (not through the re-locking `CompareExchange` helper) so they form one straight-line critical section, consistent with the `FieldLock` contract in the class doc. The two `EnsureUiThreadDispatcher()` calls in the theme tests are deleted. Rationale: the fix owns the shared state at its single mutation point (the fixture's stated design goal); it protects every present and future pin holder, not only the theme tests; it adds no MSTest lifecycle attributes (none exist in `QuickFiler.Test` today); it needs no file split; and the regression test is single-threaded.
   - Approach B (class-level `[ClassInitialize]`/`[ClassCleanup]` pin in the theme test class, fixture unchanged) is rejected: the theme tests do not read the static, so the pin protects nothing in that class and merely relocates the unpinned writes to class start and class end; without counting the class-end disposal still nulls the field for every other unpinned holder; the file is at 497 lines so the addition forces a split; and the `ClassCleanupBehavior` default for MSTest 4.4.1 was not verified and would have to be pinned explicitly.
   - Approach C (per-test `using` around `EnsureUiThreadDispatcher()` in the two theme tests, fixture unchanged) is rejected: each test's disposal is still a W2 write that can null the field under a concurrent unpinned holder, it adds lines to a file at the limit, and it pins state the tests do not use.
2. **Validation mechanism replaced, not weakened.** The issue proposes "show the theme test fails without the fix". Research section 3 traces the theme path: `new FocusController()` (empty protected constructor) -> `SetField(_themes, BuildAllThemes())` (one `Theme` from `BuildColorTheme`, whose `_uiDispatcher` is a `Mock<IUiDispatcher>` returning `Task.CompletedTask`) -> `SetThemeDark(async: true)` -> `Theme.SetQfcTheme(true)` -> `_uiDispatcher.InvokeAsync(...)` -> assertion on `_activeTheme`. `UiThread.Dispatcher` is never evaluated on this path, so a null or foreign value in the static cannot make the theme test fail, and the issue's validation idea is unsatisfiable as worded. The replacement is a fixture-level regression test (research section 5, test 1) that observes the missing reference counting directly and deterministically without concurrency. The theme tests are made independent of the static by deleting the dead calls. This spec records the replacement explicitly so a reviewer does not read it as a dropped criterion.
3. **Regression test location.** New file `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs` plus its `<Compile Include>` item in `QuickFiler.Test/QuickFiler.Test.csproj`. The existing fixture test file is at 470 lines and D4 adds to it, so the new tests cannot go there without breaching the five-hundred-line limit. The file carries test 1 (the fail-before regression) and tests 2 to 4 (specification tests that pass before and after; each is labelled as such in its doc comment). Test 3 starts a running dispatcher through `QfcItemControllerTestSupport.StartRunningDispatcher` and must shut it down through `QfcItemControllerTestSupport.ShutdownDispatcher` in a `finally`.
4. **Determinism invariant (requirement, strengthened by amendment 1.1).** After the fix, every pin taken through `EnsureUiThreadDispatcher()` / `EnsureDispatcher()` in the repository (other than the forwarder itself) is both acquired and released while its caller holds a `UiThreadDispatcherFixture` transaction (that is, while the caller owns the `TransactionGate` permit): the pin lifetime nests inside the gate hold, and no `Install` call lies between a pin's acquisition and its release. Because the gate admits one holder at a time, the pin count is serialised and is zero whenever a transaction is acquired. That is what makes the regression test's final "last release nulls" assertion deterministic under parallel execution: no other class can hold a pin while the test's transaction is live. Nesting is required, not merely acquisition inside the gate: before this amendment R4 took its pin after acquiring `transactionA` but released it only at the end of its `using` block, after `transactionA.Dispose()` had released the gate and the waiter's `transactionB` had completed, so a pin-count test whose transaction was acquired in that window would have observed a count of one and failed its final assertion nondeterministically. The R4 restructure (option (a), see the R4 bullet under "Functions/classes/CLI commands impacted") removes that pin. The plan must include a census (call-site enumeration with two independent search strategies and a member-set comparison, mirroring research section 2.1) executed against the post-change tree, classifying each invocation as acquired and released inside a held transaction and by the absence of an interleaved `Install`, and recorded as evidence.
5. **Related defects in scope** (research section 6): D1 to D7 are delivered by this item; D8 is observed and unchanged (see Scope). D4 overlaps item 5 of open issue #972 (the 950 review residuals, CR-5 of the #950 code review). #968 delivers it; the coordinator must reconcile #972 so the fix is not duplicated. The #972 promotion entry was not found under `docs/features/potential` in this worktree, so the overlap is recorded here from the orchestrator's directive and the research's CR-5 citation.
6. **Constraints.** Tests stay parallel (runsettings `Workers=0`, `Scope=ClassLevel`). Prohibited: `[DoNotParallelize]` additions, `Workers=1`, retries, `Thread.Sleep`, `Task.Delay`, temporary files, timeout increases. MSTest + Moq + FluentAssertions. No production code changes. Toolchain in CLAUDE.md C# order with numeric baseline and final coverage recorded as projections.

### Boundaries and invariants to preserve:

Contract of the counted pin, in one sentence: the fixture writes null into the shared static only when the last live pin releases, only if the fixture itself installed the parked instance, and only if the field still holds that instance.

Trace of two pins through the new code (the accept path and the two guard paths):

- Pin A on a null field: count 0 -> 1, field null -> parked, ownership flag false -> true. Scope A returned.
- Pin B while the field holds parked: count 1 -> 2, field untouched, flag untouched. Scope B returned.
- Dispose A: count 2 -> 1; count is not zero, so the field is untouched. (Before the fix this step nulls the field; this is the regression test's middle assertion.)
- Dispose B: count 1 -> 0; flag is true and the field still references parked, so the field is written null and the flag cleared. (The regression test's final assertion.)
- Guard, foreign value: if a transaction installed a live dispatcher before any pin, the first pin finds a non-null field, installs nothing and leaves the flag false; at count zero nothing is written. The existing fixture test `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` (R1) and new test 3 pin this.
- Guard, field changed under the pin: if the count reaches zero with the flag true but the field no longer references parked (a transaction installed over it), nothing is written; the flag stays true so the next pin cycle can revert the parked value once it is back. Residual recorded in the class doc: when a transaction restores parked after the last pin released, parked remains installed with zero pins. This is the pre-fix leak shape (a non-null parked dispatcher) and is benign. Before this item, R4 reached this branch (it installed `liveA` over the pinned parked value); after the R4 restructure no test in the repository installs over a pinned value, which the census records per invocation (no `Install` call between a pin's acquisition and its release).
- Guard, idempotent dispose: a second `Dispose` on the same scope performs no decrement (existing test R3 `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent`).
- Discarded scope: the count never returns to zero, so the parked dispatcher stays installed for the process lifetime. This is the same end state as today's discard, so no caller regresses, but the doc comments must say "a discarded scope pins for the process lifetime" rather than "leaks exactly as the pre-fix helper did".

Other invariants preserved:
- Lock ordering stays `TransactionGate` then `FieldLock`; `EnsureDispatcher` still never acquires `TransactionGate`; `FieldLock` regions remain straight-line with no wait, thread creation or await (the parked dispatcher is still obtained before the lock is taken).
- `UiThreadDispatcherTransaction` (`Install`, `Dispose`, `Exchange`, `CompareExchange`) and the three monotonic gate counters are unchanged.
- `EmailMoveMonitorTests` (`[DoNotParallelize]`, read-only snapshot of `Current`) is unaffected: counting never writes outside pin acquire/release.
- Existing fixture tests R1 to R6 and the #743 / #882 counter tests pass without changes to their assertions or `because` texts. R4 loses its baseline pin and gains a `try/finally`; its two assertions and `because` texts are unchanged.

### Dependencies or blocked work:

- Depends on #950 having merged (it introduced the pin and the fixture tests this item extends). The research confirms the pin exists on `origin/main` 94287369.
- Overlap with #972 item 5 (D4) must be reconciled by the coordinator after this item merges.

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:

Write set (the backticked paths in this list constitute the complete change footprint; everything else cited in this document is read-only context):

- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (fix: pin counter, ownership flag, counted dispose; D1 docs)
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (delete two dead calls; D5 delete private `BuildExecutingViewer` and call the shared helper; D6 arrange comment)
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (D2 wrapper doc; D5 shared-helper doc sentence)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (D3 doc rewrite; D4 `try/finally`)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs` (new; regression test and three specification tests)
- `QuickFiler.Test/QuickFiler.Test.csproj` (new `<Compile Include>` item adjacent to the existing fixture-test item)
- Evidence (Markdown projections only, per CLAUDE.md "Committed Test Evidence Format"; the paths below are the named deliverables, and the plan's one-artifact-per-task files under the same `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/` and `evidence/other/` folders are part of the footprint and are enumerated in the plan's Write Set):
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/baseline/toolchain-baseline.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/baseline/coverage-summary.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/baseline/coverage-jacoco-projection.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/regression-testing/fail-before-pin-count.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/regression-testing/pass-after-pin-count.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/regression-testing/concurrent-set-test-summary.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/qa-gates/call-site-census.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/qa-gates/file-line-counts.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/qa-gates/prohibited-constructs-grep.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/qa-gates/toolchain-final.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/qa-gates/coverage-summary.md`
  - `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/qa-gates/coverage-jacoco-projection.md`

Read-only context (not backticked on purpose): UtilitiesCS/Threading/UiThread.cs (static getter throws `InvalidOperationException` when null; `Initialize` and `ResetForTesting` writers), QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs (`SetThemeDark`/`SetThemeLight`), UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs (`SetQfcTheme(bool)` dispatches through `_uiDispatcher`), scripts/vscode/TaskMaster.cli.runsettings, scripts/vscode/Invoke-MSTestWithCoverage.ps1, QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs.

#### Functions/classes/CLI commands impacted:

- `UiThreadDispatcherFixture.EnsureDispatcher()` and the private `EnsureScope` class: counted acquire and release as traced above. Public shape (`internal static IDisposable EnsureDispatcher()`) unchanged.
- `UiThreadDispatcherFixture` class doc: add the counting rule under `FieldLock`, the install-ownership rule, the discard consequence and the transaction-restores-while-pinned residual (D1). `EnsureDispatcher` doc and `EnsureScope` doc rewritten: remove "a discarded scope leaks exactly as the pre-fix helper did" and "A scope that installed nothing carries null and is a no-op".
- `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()`: body unchanged (pure forwarder). Doc rewritten (D2): describe the counted pin; name the fixture tests as the remaining legitimate callers; remove "Needed for members that still delegate to a callee using the static", "Becomes moot once the callee routes through the injectable dispatcher seam" and "leaks exactly as the pre-issue-#493 void helper did".
- `QfcItemControllerTestSupport.BuildExecutingViewer()`: doc sentence "Mirrors the private static BuildExecutingViewer() in QfcItemController.FocusAndThemeTests.cs, which is not reachable from another test file" removed (D5); it becomes the single shared implementation.
- `QfcItemController_FocusAndThemeTests`: private `BuildExecutingViewer()` deleted and every caller switched to the shared helper (D5; both implementations execute `Invoke`/`BeginInvoke` synchronously, so no behaviour change); the two `EnsureUiThreadDispatcher()` calls deleted; the arrange comment of `SetThemeDark_FromNormal_SelectsDarkNormalTheme` reworded to state that the queued theme application is absorbed by the theme's injected `IUiDispatcher` mock, which is why the shared static is irrelevant (D6), with `SetThemeLight_FromNormal_SelectsLightNormalTheme` carrying a matching or referencing comment.
- `QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (R4): the baseline pin that #950 added (`using (IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher())`, opened after `transactionA` acquired the gate) is removed (amendment 1.1, option (a)). Rationale: #950 added the pin solely to fence the gate-free writers of the two theme tests, which this item deletes; after the deletion the census proves that every remaining pin nests inside a held transaction, so while `transactionA` holds the gate no other class can write the field, and every other transaction restores before it releases, which is all R4's two assertions need; the pin as placed outlived its gate hold (released after `transactionA.Dispose()` and after `transactionB` completed) and installed `liveA` over a pinned parked value, the one shape that reaches the flag-true-but-field-changed branch; and removing it, rather than releasing it before `transactionA.Dispose()` (option (b)), leaves that branch unreached by any test and keeps the fixture test file smaller. Option (b) was rejected because it keeps the install-over-pinned-parked shape and leaves the parked dispatcher installed with the ownership flag set after every R4 run. The doc paragraph is rewritten (D3) to replace "no other class may dispose an ensure scope holding the parked dispatcher (W2)" with the counting guarantee and the nesting invariant, keeping the `UiThread.Initialize` (W5) residual; `transactionA` is wrapped in `try/finally` with the explicit in-body `transactionA.Dispose()` retained and the `finally` re-dispose relying on the idempotency proved by `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` (D4). R4's two assertions and their `because` texts are unchanged.
- New `QfcItemController_UiThreadDispatcherPinCountTests` (MSTest + FluentAssertions; Moq is not needed and no unused `using` is added; `[Timeout(60000)]` convention of the sibling fixture test file). Proposed names (digit-free so they can be cited in acceptance criteria):
  1. `EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease` (regression; fails before). Arrange: a transaction from `UiThreadDispatcherFixture.BeginTransactionAsync()` disposed in a `finally` (the shape of the sibling fixture tests R2 and R3); `transaction.Install(null)`; `pinA`, `pinB` from `EnsureUiThreadDispatcher()`; `afterBothPins = Current`. Act: dispose `pinA`, read `afterFirstRelease`; dispose `pinB`, read `afterLastRelease`. Assert: `afterBothPins` not null; `afterFirstRelease` same as `afterBothPins` (because "a holder that did not take the last pin must not lose the dispatcher"); `afterLastRelease` null (because "the last release reverts the fixture's own seeding").
  2. `EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome` (specification; passes before and after). Same arrange; dispose `pinB` first, then `pinA`; identical assertions.
  3. `EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher` (specification; passes before and after; extends R1). Transaction installs a dispatcher from `StartRunningDispatcher`; two pins taken and released; `Current` still the live dispatcher. The live dispatcher is shut down through `ShutdownDispatcher` in a `finally`.
  4. `EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores` (specification; passes before and after). After test 1's sequence within the same transaction, a single fresh pin installs and its release restores null (proves the ownership flag was cleared).
- CLI commands: none added or changed.

#### Data flow and validation changes:

None at the production level. Test-side: the fixture's internal state gains two fields; all reads and writes of them occur inside `lock (FieldLock)`.

#### Error handling and logging updates:

None. The fixture does not log. The D4 `try/finally` changes failure propagation only: a throw between gate acquisition and the explicit dispose now releases the gate immediately instead of holding it until the 120 s bound or until the #743 counter test surfaces it.

#### Rollback/feature-flag considerations (if applicable):

Not applicable; test-only change. Rollback is a revert of the branch.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `UiThreadDispatcherFixture.EnsureDispatcher()` -> `IDisposable` (unchanged signature). Post-condition: the shared static is non-null; the pin count is one greater than before.
- `IDisposable.Dispose()` on the returned scope: first call decrements the pin count and conditionally writes null as traced above; later calls are no-ops.

#### Required configuration keys and defaults:

None. The CLI runsettings (`Workers=0`, `Scope=ClassLevel`) are unchanged.

#### Backward-compatibility expectations:

All existing callers compile and behave as before from their own point of view: a scope that installed nothing still nulls nothing on dispose; a single pin on a null baseline still installs and still reverts on dispose; a discarded scope still leaves the parked dispatcher installed.

#### Performance constraints (latency/throughput/memory):

Not applicable. The additional work per pin is one integer increment or decrement and one boolean read under a lock that is already taken.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - `QuickFiler.Test` is a legacy (non-SDK) project targeting .NET Framework 4.8.1 with explicit `<Compile Include>` items; a new test file is invisible to the build until its item is added.
  - The MSTest adapter runs each test assembly in its own AppDomain, so the `UtilitiesCS.Test` mechanisms over the same static cannot interleave with this fixture (cited from #950 research; not re-verified).
  - `SetupAssemblyInitializer` does not write the shared static, so a class run alone starts from a null baseline (relevant to the fail-before run).
- Constraints (budget, performance, compatibility):
  - Every touched or added C# file stays at or under the five-hundred-line limit measured as total physical lines. Pre-change counts (research D7, content lines): focus-and-theme tests 497, fixture tests 470, test support 440, fixture 342. Expected direction: focus-and-theme tests shrink by roughly 15 lines (two calls and the 18-line private helper with its blank line removed, three comment lines added); fixture tests grow by 2 (the removed four-line pin header and its closing brace are replaced by a two-line `try` header and a five-line `finally` block) with a zero-net D3 doc replacement; fixture grows by roughly 32.
  - MSTest + Moq + FluentAssertions only (packages.config: MSTest 4.4.1, Moq 4.21.0, FluentAssertions 8.11.0). No new dependency.
  - Prohibited constructs as listed in Scope.
  - `*.csproj` is excluded from CSharpier by `.csharpierignore`; the `<Compile Include>` edit is hand-authored and must match the existing item style.
- External dependencies (services, libraries, releases): none.

## Data / API / Config Impact
- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy
Seeded from issue, with disposition:

- Issue idea "Have the theme tests acquire and release the #950 dispatcher pin in class initialize and cleanup": superseded by orchestrator decision 1. The theme tests do not read the static, so pinning in that class protects nothing; the dead calls are deleted and the fixture is made sound for every holder instead (Approach B rejection, above).
- Issue idea "Write a deterministic regression test that releases a competing pin mid-test, and show the theme test fails without the fix": the first half is delivered as the fixture-level regression test (two pins, first released mid-test). The second half is unsatisfiable as worded (section 3 trace) and is replaced by the fail-before/pass-after run of that regression test (orchestrator decision 2).

- Regression tests to add or update:
  - Add `QfcItemController_UiThreadDispatcherPinCountTests` (four tests as specified above). Test 1 is the regression gate; tests 2 to 4 are specification tests and say so in their doc comments.
  - Negative control (must isolate the fixture change): compile the new test file in with the fixture at its pre-fix state and run test 1 by fully qualified name (`QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease`) with `/Settings:scripts/vscode/TaskMaster.cli.runsettings`; capture the failure message as a test-result summary in `evidence/regression-testing/fail-before-pin-count.md`. Then apply the fixture change only and re-run; capture `Passed` in `evidence/regression-testing/pass-after-pin-count.md`. The only difference between the two runs is the fixture change.
  - Existing fixture tests R1 to R6 and the #743 / #882 counter tests: run and pass with no changes to their assertions.
  - Theme tests `SetThemeDark_FromNormal_SelectsDarkNormalTheme` and `SetThemeLight_FromNormal_SelectsLightNormalTheme`: run alone and in the concurrent set after the calls are deleted.
- Unit tests (MSTest) for the fixed behavior and boundaries:
  - Counted release (test 1), order independence (test 2), foreign-value protection at count zero (test 3), ownership-flag reset (test 4), idempotent dispose (existing R3), no-install when non-null (existing R1), single pin on null baseline (existing R2).
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - Count reaches zero while a transaction has replaced the field: nothing is written (covered by the design trace; test 3 covers the flag-false branch; the flag-true-but-field-changed branch is documented as a residual and is not separately tested because constructing it requires a transaction to install over a pinned parked value; before this item R4 did exactly that, and after the R4 restructure no test in the repository does, which the census records per invocation).
  - Discarded scope: documented, not tested (it would leave a permanent pin in the shared fixture for the rest of the run).
- Error handling and logging verification:
  - D4: a throw inside the second-caller test's body now releases the gate through `finally`; verified by code review of the diff (no injected fault is added, because injecting one would require a seam in the fixture that does not exist and is not in scope).
- Coverage impact and targets for changed lines/modules:
  - All changes are in a test assembly, which the coverage route excludes from instrumentation. First-party line and branch figures are expected to be unchanged within run-to-run noise. Baseline figures are captured before any edit on this branch (`evidence/baseline/coverage-summary.md`, `evidence/baseline/coverage-jacoco-projection.md`) and final figures after the last edit (`evidence/qa-gates/coverage-summary.md`, `evidence/qa-gates/coverage-jacoco-projection.md`). The acceptance criterion is "not lower than baseline"; the repository floors (line at least 80 percent, branch at least 75 percent on the testable denominator) continue to apply and are not re-derived here.
- Toolchain commands to run (format -> lint -> type-check -> test), in CLAUDE.md order, as one uninterrupted pass after the last edit:
  1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (inner `vstest.console.exe` with `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`; fixed results directory `coverage\test-results` and trx name `mstest-coverage-run.trx`)
  Non-vacuity: the two rebuild logs must contain no `Skipping target "CoreCompile"` line. Commands, exit codes and timestamps are recorded in `evidence/baseline/toolchain-baseline.md` (before edits) and `evidence/qa-gates/toolchain-final.md` (final pass).
- Supporting observations recorded as evidence:
  - Call-site census (`evidence/qa-gates/call-site-census.md`): primary strategy content grep `EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)` over all `*.cs`; cross-check strategy count-mode grep of the bare identifiers `EnsureUiThreadDispatcher|EnsureDispatcher` with per-line classification; member sets compared; every remaining invocation classified as nested (acquired and released inside a `BeginTransactionAsync` holder, with no `Install` call between acquisition and release) or as the forwarder. Expected post-change result: four invocations in the existing files (the forwarder plus three in the fixture tests R1 to R3) and nine in the new pin-count test class, each test-side invocation acquired and released inside a held transaction; zero in the focus-and-theme test file and zero in the second-caller transaction test.
  - File line counts (`evidence/qa-gates/file-line-counts.md`): total physical lines of each file in the write set after the change.
  - Prohibited-constructs grep (`evidence/qa-gates/prohibited-constructs-grep.md`): grep of the branch diff for `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Workers`, `Timeout(`, `Path.GetTempFileName`, `Path.GetTempPath` with the expected result (no additions; `Timeout(` only as the sibling file's existing constant convention in the new test class).
  - Concurrent set (`evidence/regression-testing/concurrent-set-test-summary.md`): one vstest invocation under the CLI runsettings running `QfcItemController_UiThreadDispatcherFixtureTests`, `QfcItemController_UiThreadDispatcherPinCountTests` and `QfcItemController_FocusAndThemeTests` together; all tests `Passed`. This is a supporting observation (MSTest cannot be made to interleave classes on demand), not the regression gate.
- Manual validation steps (if required): none.


## Acceptance Criteria
- [ ] AC1: Counted pin, non-last release: with two ensure pins held on a null baseline inside a fixture transaction, disposing the first pin leaves the shared dispatcher field holding the parked dispatcher; proved by the fail-before regression test `EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease` in `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs` passing after the fix.
- [ ] AC2: Counted pin, last release reverts only the fixture's own seeding: disposing the final live pin writes null back only because the fixture itself installed the parked dispatcher and the field still holds it; proved by the final assertion of the fail-before regression test and by the specification test `EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome` passing.
- [ ] AC3: A foreign transaction value is never nulled by pin release: with a transaction holding a live running dispatcher, taking and releasing all pins leaves that live dispatcher in place; proved by the specification test `EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher` passing, with its running dispatcher shut down through `QfcItemControllerTestSupport.ShutdownDispatcher` in a `finally`, and by the existing test `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` still passing.
- [ ] AC4: Ownership flag is cleared on the last release: after a full two-pin cycle inside the same transaction, a fresh single pin on the null baseline installs the parked dispatcher and its release restores null; proved by the specification test `EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores` passing.
- [ ] AC5: Fail-before and pass-after evidence isolates the fixture change: the regression test is observed failing against the unmodified fixture with the pin-count test file compiled in, the captured FluentAssertions message names the first-release assertion and reports `found <null>`, and the same test is observed passing after the fixture change with no other difference between the two runs; both runs are recorded as test-result summaries (no raw trx) in this feature's regression-testing evidence folder.
- [ ] AC6: The pin-count test class labels its tests: the regression test's doc comment states that it fails before the fix, each of the three specification tests' doc comments states that it passes before and after the fix, and the class doc states why the regression lives at the fixture level (the theme path dispatches through the theme's injected dispatcher mock and never reads the shared static).
- [ ] AC7: The dead theme-test calls are removed: a content grep of `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` for `EnsureUiThreadDispatcher` returns zero hits, and `SetThemeDark_FromNormal_SelectsDarkNormalTheme` and `SetThemeLight_FromNormal_SelectsLightNormalTheme` both pass.
- [ ] AC8: Gated-caller census invariant holds: a census recorded in this feature's qa-gates evidence folder enumerates every invocation of `EnsureUiThreadDispatcher` and `EnsureDispatcher` across all C# files in the repository using two independently constructed search strategies whose member sets are compared and agree, and classifies every invocation other than the forwarder inside `QfcItemControllerTestSupport` as acquired and released inside a held `UiThreadDispatcherFixture` transaction (the scope's acquisition and its disposal both lie within the caller's transaction hold) with no `Install` call between the acquisition and the release.
- [ ] AC9: The pin counter and install-ownership flag are private statics of `UiThreadDispatcherFixture`, every read and write of them occurs inside a `lock (FieldLock)` block, and the last-release null write is performed inline in the same critical section as the decrement rather than through the re-locking compare-exchange helper; verified by reading the fixture diff.
- [ ] AC10: All existing fixture tests pass unchanged in behaviour: every test in `QfcItemController_UiThreadDispatcherFixtureTests` passes after the fix, and the diff of that file touches only the second-caller transaction test's doc comment, the removal of its baseline pin and its transaction disposal structure, leaving every assertion and `because` text unchanged.
- [ ] AC11: Fixture documentation describes the counted pin: the class doc, the `EnsureDispatcher` doc and the scope class doc in `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` describe pin counting under the field lock, install ownership, the discard consequence (a discarded scope pins for the process lifetime) and the transaction-restores-while-pinned residual; a grep of that file for `leaks exactly` and for `installed nothing carries` returns zero hits.
- [ ] AC12: Wrapper documentation describes the counted pin: the doc comment of `EnsureUiThreadDispatcher` in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` describes the counted pin and names the fixture tests as the remaining legitimate callers; a grep of that file for `Becomes moot`, for `leaks exactly` and for `still delegate to a callee` returns zero hits.
- [ ] AC13: The second-caller transaction test's doc no longer asserts the obsolete invariant: the doc comment of `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` states the counting guarantee and keeps the `UiThread.Initialize` residual; a grep of `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` for `no other class may dispose` returns zero hits.
- [ ] AC14: The second-caller transaction test releases its gate on any throw: in `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` the first transaction is disposed in a `finally` block in addition to the explicit in-body dispose, relying on the idempotency proved by `Transaction_DisposedTwice_DoesNotOverReleaseTheGate`; verified by reading the diff and by the test passing.
- [ ] AC15: The duplicated viewer helper is removed: a grep of `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` for `private static Mock<IItemViewer> BuildExecutingViewer` returns zero hits, every former caller in that class uses `QfcItemControllerTestSupport.BuildExecutingViewer`, the shared helper's doc in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` no longer says it mirrors a private copy that is unreachable from another test file, and every test in `QfcItemController_FocusAndThemeTests` passes.
- [ ] AC16: The theme-test arrange comment is corrected: the arrange comment of `SetThemeDark_FromNormal_SelectsDarkNormalTheme` states that the queued theme application is absorbed by the theme's injected `IUiDispatcher` mock, which is why the shared static is irrelevant, and `SetThemeLight_FromNormal_SelectsLightNormalTheme` carries a matching comment or a reference to it; verified by reading the diff.
- [ ] AC17: `EnsureSynchronizationContext` in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` is unchanged; the diff of that file contains no hunk touching it.
- [ ] AC18: File-size limit: every touched or added C# file in the write set is at or under the five-hundred-line limit measured as total physical lines, and the post-change counts are recorded in this feature's qa-gates evidence folder.
- [ ] AC19: No prohibited constructs: the branch diff adds no `DoNotParallelize` attribute, no `Thread.Sleep`, no `Task.Delay`, no retry loop, no temporary file, and no timeout increase, and the CLI runsettings file is unchanged; verified by the recorded grep of the diff in this feature's qa-gates evidence folder.
- [ ] AC20: No production code change: the diff against the merge base, after excluding the paths already committed on the branch before the plan's first task (recorded at Phase 0 as the inherited committed set), lists only paths under `QuickFiler.Test/` and this feature's documentation folder.
- [ ] AC21: The new test file is built and discovered: `QuickFiler.Test/QuickFiler.Test.csproj` carries a `Compile Include` item for `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`, and all four pin-count tests appear as passed in the coverage route's test-result summary.
- [ ] AC22: Full toolchain pass: csharpier check, the analyzer rebuild, the warnings-as-errors rebuild and the MSTest coverage route complete in that order with every step passing in one uninterrupted pass after the last edit, the two rebuild logs contain no skipped compile target, and the commands with exit codes are recorded in this feature's qa-gates evidence folder.
- [ ] AC23: Coverage not reduced: first-party line and branch coverage after the change are each not lower than the baseline captured before any edit on this branch, with both figures recorded as the one-line summary plus the package-level JaCoCo projection in this feature's baseline and qa-gates evidence folders.
- [ ] AC24: Parallel run of the three classes together passes: one vstest invocation under the CLI runsettings (workers zero, class-level scope) running `QfcItemController_UiThreadDispatcherFixtureTests`, `QfcItemController_UiThreadDispatcherPinCountTests` and `QfcItemController_FocusAndThemeTests` reports every test as passed, recorded as a test-result summary in this feature's regression-testing evidence folder.

## Risks & Mitigations
- Technical or operational risks:
  - Counting changes a shared fixture used by four test classes. Mitigation: existing fixture tests R1 to R3 are the semantic guard for "installed nothing" and "single pin on null baseline"; they run unchanged and must pass.
  - A discarded scope now holds a pin for the process lifetime. Mitigation: the census proves every caller disposes its scope inside a gated transaction; the doc states the consequence.
  - The flag-true-but-field-changed branch is not directly tested. Mitigation: after the R4 restructure no test in the repository installs a transaction value over a pinned parked value (the census records, for every invocation, that no `Install` call lies between the pin's acquisition and its release), so the branch is reached by no test; it is documented as a benign residual.
  - D4 duplicates work filed under #972 item 5. Mitigation: the coordinator reconciles #972 after merge; this spec records the overlap.
  - `*.csproj` is outside CSharpier; a malformed `<Compile Include>` item silently drops the new tests. Mitigation: the discovery acceptance criterion requires the four pin-count tests to appear in the test-result summary.
- Mitigations and rollbacks: revert the branch; no data or configuration is affected.

## Rollout & Follow-up
- Release/rollout steps: merge through the standard PR route after the final toolchain pass; no deployment step.
- Post-fix monitoring or clean-up tasks:
  - Coordinator: reconcile #972 item 5 (the second-caller transaction test `try/finally`) as delivered by #968.
  - None otherwise; no new issue is required (research section 8 found no new unrelated defects).
- Links: issue #968 (https://github.com/drmoisan/TaskMaster/issues/968); related #950, #882, #743, #493, #972; research record docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md.
