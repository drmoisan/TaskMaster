# 2026-09-09-createcancellationtoken-has-no-production-caller (Spec)

- **Issue:** #839
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-bug. This file is the sole authoritative acceptance-criteria source for this item. The companion user-story.md is narrative context only and carries no checkboxes.
- **Research:** research/2026-09-12T18-05-createcancellationtoken-init-path-research.md in this feature folder. Every line number cited below was re-derived from the current tree by that research and re-checked against the tree on 2026-09-12 while writing this spec.

> Formatting rule for this document (do not "fix" it): a downstream extractor harvests backticked, whitespace-free tokens from this file to compute the item's blast radius, and it has no notion of polarity. Repository paths therefore appear in backticks only in the `## Write Set` section and in the evidence artifact paths in Test Strategy and Acceptance Criteria (all of which fall under the Write Set's feature-folder glob). Every other file reference in this document, including files the diff deliberately does not touch and File.cs line citations, is written as plain prose without backticks. Backticked C# identifiers such as `Init()` are member names, not paths.

## Context

`QfcHomeController` has two initialization paths. The asynchronous path (`LaunchAsync` -> `InitAsync`) creates a `CancellationTokenSource` at QfcHomeController.cs line 54 and assigns both `_token` and `_tokenSource` at lines 116-117 before any loader runs. The synchronous path (public constructor at lines 29-33, then `Init()` at lines 86-106, then `Run()` at lines 245-269) never assigns either field. The factory that would do so, `internal void CreateCancellationToken()` at lines 467-471, exists in the file but has zero production callers (research, Numeric Derivation Evidence: six family members repository-wide, of which the only `QfcHomeController` invocation is in a test).

The result on the synchronous path is that `Init()` hands `default(CancellationToken)` to the datamodel loader (line 88) and to the queue loader (line 94), and hands a null `CancellationTokenSource` to the form-controller loader (lines 102-103). `QfcFormController` stores the null source (QfcFormController.cs line 39) and its three `LoadItems` / `LoadItemsAsync` overloads early-return on `_tokenSource is null` (QfcFormController.Actions.cs lines 38, 75 and 131). `Run()` calls `LoadItems` at QfcHomeController.cs line 263, so on this path no item is loaded, no exception is thrown and no log line is written.

Environment: Windows 11, Outlook VSTO host, C# targeting .NET Framework 4.8.1, QuickFiler project.

Severity as filed: High. See the reachability finding below for how that severity should be read.

### Reachability finding (stated plainly)

The synchronous `Init()` path has no live entry point in the shipped product. Verified by the research (Section 2) with an exhaustive repository search across every file type outside docs/ and artifacts/:

- The only production caller of `Init()` on a QuickFiler home controller is `RibbonController.LoadQuickFiler()` (TaskMaster/Ribbon/RibbonController.cs lines 97-110).
- `LoadQuickFiler()` has zero callers anywhere in the repository. The only other hits on that name are the interface member and implementation of `AddInUtilities.LaunchQuickFiler`, whose body calls `LoadQuickFilerAsync()`, the asynchronous entry.
- Both ribbon buttons (RibbonExplorer.xml lines 214 and 222) route through `RibbonViewer` to `LoadQuickFilerAsync()` and `LoadQuickFilerHighConfidenceAsync()`, both of which call `QfcHomeController.LaunchAsync`, which creates the token source correctly.
- No string-based or reflection-based dispatch names `Init`, `LoadQuickFiler` or `CreateCancellationToken`. `RibbonController` holds the controller as `IFilerHomeController`, which does not declare `Init()`.

The defect is therefore a latent breach of a public initializer's contract, not a currently user-visible failure. It is nevertheless a real defect: `Init()` is public, it is declared on the public interface `IQfcHomeController` (IQfcHomeController.cs line 12), it is covered by an existing test (`Init_InitializesCorrectly`), and it produces a controller whose item loading can never run. This spec neither overstates the impact to justify the work nor understates it.

The research could not consult git history (Bash was unavailable in that session), so it recorded the date of the last call site as unknown. That gap has since been closed by direct measurement against this worktree and the resolved finding is recorded in Rollout & Follow-up: the last call site was removed in commit 9f34ea06d, dated 2024-09-27, from TaskMaster/Ribbon/RibbonViewer.cs. No statement in this spec rests on the unknown any longer.

## Repro & Evidence

Steps to reproduce (code-level; there is no end-user route today):

1. Construct `QfcHomeController` through the public constructor (QfcHomeController.cs lines 29-33) and call `Init()`.
2. Observe that `CreateCancellationToken()` is never invoked on that path, so `TokenSource` is null and `Token` is `default(CancellationToken)` with `CanBeCanceled == false`.
3. Observe the form-controller loader receiving the null source at lines 102-103, and the guards at QfcFormController.Actions.cs lines 38, 75 and 131 returning early when `Run()` reaches `LoadItems` at line 263.

Expected: after `Init()` returns, `TokenSource` is non-null, `Token == TokenSource.Token`, `Token.CanBeCanceled` is true, and every loader on the synchronous path received that same source and token, so `LoadItems` proceeds past its guard.

Actual: `LoadItems` and `LoadItemsAsync` return without loading anything. The failure is a no-op with no log line and no exception.

Logs / screenshots: none. The defect is the absence of any signal.

Evidence of asymmetry: `EfcHomeController` declares an identically-bodied `CreateCancellationToken()` (EfcHomeController.cs lines 399-403) and calls it on every construction path (lines 62, 126 and 162). The Qfc controller declares the same method and calls it nowhere. Verified by the research (Section 1.3).

## Scope & Non-Goals

In scope:

- Insert one call to the existing `CreateCancellationToken()` factory as the first statement of `QfcHomeController.Init()`.
- Remove one commented-out dead line from the same file so it stays within the 500-line ceiling.
- Add one deterministic MSTest regression test to the existing `QfcHomeControllerTests` class.
- Record evidence projections under this feature folder.
- File a follow-up for removal of the dead synchronous entry path (remedy d below).

Out of scope / non-goals (paths deliberately unbackticked; the diff does not touch them):

- QuickFiler/Controllers/QfcFormController.Actions.cs: the early-return guards stay as they are (remedy c rejected).
- QuickFiler/Controllers/QfcFormController.cs: no fail-fast added at the consumer constructor (remedy e not adopted; see Rollout & Follow-up).
- QuickFiler/Controllers/QfcHomeController.Metrics.cs, QuickFiler/Controllers/QfcCollectionController.cs and QuickFiler/Controllers/QfcItemController.cs (plus its Initialization partial): owned by concurrent sibling items. The research verified no finding requires any of them: the Metrics partial's only token reference is a comment at line 194; the collection controller stores whatever source it is given at line 42; the item controller reads `TokenSource` in its Initialization partial at line 386 and needs no change once the source is non-null.
- QuickFiler/Controllers/EfcHomeController.cs: the precedent, unchanged.
- TaskMaster/Ribbon/RibbonController.cs: the dead `LoadQuickFiler()` stays until the follow-up removes it (remedy d deferred).
- QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs: the existing test caller of `CreateCancellationToken()` at line 124 builds the controller without `Init()` and still needs its explicit call; unchanged.
- QuickFiler/QuickFiler.csproj and QuickFiler.Test/QuickFiler.Test.csproj: both are legacy non-SDK projects with explicit compile item lists, but the regression test goes into an existing test file and no new source file is created, so neither project file is edited.
- Any interface change, any nullable directive, any csproj change.

## Root Cause Analysis

Surfaced during preparation of feature 821 in the review-residuals-2026-09-08 epic as observation O-4 and deliberately left out of that feature's blast radius.

The early-return guards in QfcFormController.Actions.cs are individually defensible: each also covers `_globals`, `_formViewer`, `_parent` and `_states`, every one of which has a legitimate null path (before `Init()` and after `Cleanup()`). The defect is that nothing on the synchronous path establishes the invariant those guards protect.

### Invariant the fix establishes

After `QfcHomeController.Init()` returns, `_tokenSource` is a live `CancellationTokenSource` owned by the controller, `_token` is that source's `Token`, and every loader invoked by `Init()` observed that same source and token, so the synchronous path is structurally identical to `InitAsync` (lines 116-117) and to `EfcHomeController` (line 62).

### Trace of one value from accept point to silent absorption

1. Accept point: the public constructor (QfcHomeController.cs lines 29-33) assigns `Globals` and `ParentCleanup` and nothing else. It does not validate or create a token source, and by design it must not (see rejected remedy b).
2. Propagation: `Init()` line 88 passes `this.Token` (default, never cancellable) to `QfcDataModelLoader`; line 94 passes the same default token to `QfcQueueLoader`; lines 102-103 pass `this._tokenSource` (null) and `this._token` to `QfcFormControllerLoader`.
3. Storage: `QfcFormController` stores the null source at QfcFormController.cs line 39 without checking it.
4. Absorption: `Run()` (line 263) calls `LoadItems`; the guard at QfcFormController.Actions.cs line 38 sees `_tokenSource is null` and returns. There is no log statement and no exception on that branch, which is why the failure is silent. A second, quieter absorption: the cancel teardown at QfcFormController.EventHandlers.cs line 133 runs `_parent?.TokenSource?.Cancel()`, a no-op when the parent source is null, and the datamodel and queue hold a token whose `CanBeCanceled` is false, so cancellation could never propagate to them even if the guard were bypassed.

The fix acts at step 2, before any propagation, by making the first statement of `Init()` establish the invariant. It does not change steps 3 or 4.

### Inverse constraint (what must not be widened or removed)

- The guards at QfcFormController.Actions.cs lines 33-40, 70-77 and 126-133 must stay. Removing them relocates the failure into `QfcCollectionController` (stores the source unchecked at line 42) and `QfcItemController` (reads `_homeController.TokenSource` at Initialization partial line 386 and hands it to `ConversationResolver` at lines 393-399), turning a silent no-op into a null-reference exception deep in item construction.
- `Cleanup()` must continue to dispose and null `_tokenSource` (QfcHomeController.cs lines 389-390). The test `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` in QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs (issue #810 AC3) pins this and must pass unchanged.

## Proposed Fix

### Decision

Remedy (a), stated here as decided: insert `CreateCancellationToken();` as the first statement of `QfcHomeController.Init()`, immediately after the opening brace at QfcHomeController.cs line 87 and before the `QfcDataModelLoader` call at line 88.

The insertion point is load-bearing and is not merely "before the form-controller loader". `Init()` hands `this.Token` to the datamodel loader (line 88) and to the queue loader (line 94) before it reaches the form-controller loader (lines 95-104). An unassigned token field is `default(CancellationToken)`, whose `CanBeCanceled` is false. Inserting the call anywhere after the first statement therefore leaves the datamodel and the queue holding a token that can never be cancelled, a quieter version of the same defect. The regression test pins this ordering by asserting `CanBeCanceled == true` on the tokens the datamodel and queue loaders received.

### Design summary (what changes where)

- QuickFiler/Controllers/QfcHomeController.cs, `Init()` body (lines 86-106): one new first statement, `CreateCancellationToken();`.
- QuickFiler/Controllers/QfcHomeController.cs, one deleted line: either the commented-out debug log inside `LaunchAsync` (currently line 41) or the commented-out `FormViewer` property declaration (currently line 465). Both are dead comments with no reader; CSharpier does not remove comments, so the deletion is done by hand in the diff. Deleting one keeps the file at exactly 500 lines after the insertion.
- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs (currently 275 lines): one new `[TestMethod]`, `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt`, placed after `Init_InitializesCorrectly` (lines 112-163).

### Hard constraint: the 500-line ceiling

The production file is currently exactly 500 lines (line 500 is the closing brace), which is the repository file-size ceiling from the General Code Change Policy. Adding a statement therefore requires removing a line elsewhere in the same file in the same diff. The two candidates above are the only dead lines the research identified; no executable line may be removed to make room.

### Boundaries and invariants to preserve

- The asynchronous path (`LaunchAsync`, `InitAsync`) is untouched.
- The 25 construction sites of `QfcHomeController` (2 production, 23 test; research Section 4.2) are untouched, because the fix acts in `Init()`, not in a constructor.
- No interface member is added, removed or changed.
- The file continues to carry no `#nullable` directive, so the warnings-as-errors nullable build (CLAUDE.md toolchain step 3) gains no new CS86xx obligations from this diff.
- Disposal ownership is already in place: `Cleanup()` disposes and nulls the source at lines 389-390, and `Cleanup` is passed to the form controller as `parentCleanup` at line 100, which `QfcFormController.Cleanup()` invokes under `finally` (QfcFormController.SetupDisposal.cs lines 269-271). No new disposal code is needed.

### Rejected alternatives (recorded so the decision is auditable)

- (b) Make the token source non-nullable and enforce it at construction. Rejected. `Cleanup()` deliberately nulls the field after disposing it (line 390), and the ratified issue #810 AC3 test `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` requires that nulling so a later `Cancel()` cannot reach a disposed source; the field therefore cannot be non-nullable across its whole lifetime. A constructor-created source would also be orphaned undisposed on every asynchronous launch, because `LaunchAsync` creates its own source at line 54 and `InitAsync` overwrites the field at line 117. The file carries no nullable directive, and adding one to obtain compiler enforcement would conscript approximately 37 existing diagnostics (research Section 3.2: 24 CS8618, 9 CS8625, 2 CS8600, 1 CS8603, 1 CS8604, estimated by reading, not confirmed by a build) into a build gate that treats warnings as errors.
- (c) Remove the early-return guards. Rejected. The guards also cover the globals, the form viewer, the parent controller and the states collection, each of which has its own null path: `_states` is assigned only by `Init()` -> `CaptureItemSettings()` (QfcFormController.SetupDisposal.cs line 37), and `Cleanup()` nulls `_globals`, `_formViewer` and `_parent` at SetupDisposal.cs lines 252-257. Removing only the token clause relocates the failure into item-controller construction instead of fixing it (see Inverse constraint).
- (d) Delete the dead synchronous path (`RibbonController.LoadQuickFiler()`, `QfcHomeController.Init()`, `IQfcHomeController.Init()`, `CreateCancellationToken()` once it has no caller, and `Init_InitializesCorrectly`). Deferred to a follow-up. It spans two projects, removes a member from a public interface, and is feature work outside the bugfix workflow's scope rule. `Run()` must stay in any case because `IFilerHomeController` declares it and `EfcHomeController.Run()` is live.
- (e) Fail fast in the `QfcFormController` constructor with `tokenSource.ThrowIfNull()`. Not adopted in this diff. It is compatible hardening (all nine test construction sites of `QfcFormController` already pass a real source) but is not required to fix #839 and would widen the write set into a file the sibling items and the rejected remedy (c) also reference.

### Error handling and logging updates

None. The fix removes a silent no-op by establishing the invariant; it does not add a log line, and the guards' behaviour on a genuinely null source is unchanged.

### Rollback

Reverting the single production statement and the single comment deletion restores the previous behaviour exactly. No feature flag.

## Assumptions, Constraints, Dependencies

- Assumption: the research's line numbers are current as of 2026-09-12 (re-verified while writing this spec). Concurrent sibling items in this parallel run edit other files in QuickFiler/Controllers, not this one, so the numbers should hold until this item merges.
- Constraint: QuickFiler/Controllers/QfcHomeController.cs must not exceed 500 lines after the change.
- Constraint: the regression test must be deterministic and must use no timers, no filesystem and no COM (General Unit Test Policy UT4).
- Constraint: MSTest, Moq and FluentAssertions only (C# Unit Test Policy CUT1, CUT2). QuickFiler.Test already references MSTest.TestFramework 4.4.0.0, Moq 4.20.72.0 and FluentAssertions 8.10.0.0.
- Constraint (maintainer decision on issue 671, effective now): commit projections only. No new raw test-result XML and no new raw coverage XML may be added to the repository. Numeric figures go into the Markdown evidence artifacts and the raw tool output is discarded.
- Dependency: none. This item blocks nothing and is blocked by nothing.

## Data / API / Config Impact

- User-facing or API changes: none. No public or internal signature changes.
- Data or migration considerations: none.
- Logging/telemetry updates: none.
- Compatibility notes: none. `Init()` keeps its signature and return type.

## Test Strategy

### Regression test (must fail before the fix, pass after)

Test project QuickFiler.Test; test class `QuickFiler.Controllers.Tests.QfcHomeControllerTests` in QuickFiler.Test/Controllers/QfcHomeControllerTests.cs. The class already builds the controller through the public constructor in `Setup()` and its `Init_InitializesCorrectly` test (lines 112-163) replaces all five synchronous loaders with lambdas returning loose Moq objects. The new test reuses that arrangement and additionally observes the token arguments, which the existing test never does (that omission is why the defect was never caught).

New test `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt`:

- Arrange: assign a `QfcFormControllerLoader` lambda that captures its `CancellationTokenSource` and `CancellationToken` parameters into locals; assign `QfcDataModelLoader` and `QfcQueueLoader` lambdas that capture their `CancellationToken` parameter (first parameter of the queue loader, second of the datamodel loader); replace the explorer and keyboard loaders as the existing test does.
- Act: call `_controller.Init()`.
- Assert (FluentAssertions): the captured source is not null; the captured form-controller token equals the captured source's `Token`; `_controller.TokenSource` is the same instance as the captured source; the datamodel token and the queue token each equal the captured source's `Token` and each report `CanBeCanceled == true`.
- Teardown: call `_controller.Cleanup()` so the source created by `Init()` is disposed (lines 389-390). With the loose mocks in this class, `Cleanup()` touches `_formViewer?.Worker` on the real viewer, `_datamodel?.Cleanup()` on a loose mock and invokes the `Mock<System.Action>` parent cleanup; none of these throws.

Pre-fix, the not-null assertion fails (the source is null). A fix that inserts the call after line 88 passes the not-null assertion but fails the `CanBeCanceled` assertion on the datamodel token, so the test pins the ordering rule, not just the presence of the call. Post-fix, all assertions pass.

### Declared inherited exception: live form construction inside a unit test

`Init()` at line 90 constructs a real `QfcFormViewer`, a `Form`-derived type whose constructor runs `InitializeComponent()` and captures the ambient synchronization context. The unit-test policy prohibits dependence on heavy host resources, and this test will exercise that construction. This is pre-existing debt: the existing `Init_InitializesCorrectly` in the same class already constructs the same viewer, and the structural guard `NoLiveFormInTestAssemblyTests` checks only that no `Form`-derived type is compiled into the test assembly, not that none is constructed. The new test therefore introduces no new category of test behaviour, but the exception is declared here explicitly rather than silently, per UT5, and must be repeated in the change description. Removing the construction is out of scope; remedy (d) in the follow-up removes the whole synchronous path and with it both tests.

### Existing tests that must keep passing unchanged

- `Init_InitializesCorrectly` (same class). Its five loader replacements and four assertions stay as they are. Adding a trailing `Cleanup()` to dispose the source it will now allocate is permitted but not required.
- `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` and the two neighbouring disposal tests in QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs (lines 79-126, 136-156, 171-190).
- The `QfcHomeControllerMetricsTests` test that calls `CreateCancellationToken()` directly at line 124 and cancels through `TokenSource` at line 388.
- The reflection-based token tests in QfcHomeControllerPropertyTests.cs (lines 262-300), QfcHomeControllerIterationTests.cs (line 468) and the `InitAsync` test in QfcHomeControllerTests.cs (lines 181-225). None calls `Init()`, so none is affected.

### Coverage

Lines 467-471 of QfcHomeController.cs are already covered by the Metrics test; the new statement inside `Init()` is covered by both `Init()` tests. Changed-line coverage must not decrease (UT2). Coverage figures are recorded as numbers in the Markdown projections below; raw Cobertura or TRX output is not committed.

### Evidence projections (Markdown only, feature-relative, fixed filenames so the acceptance criteria can name them; each carries the run timestamp in its Timestamp field per the evidence-and-timestamp-conventions skill)

- Baseline coverage for QuickFiler.Test before the change: `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/coverage-baseline.md`
- Fail-before run of the new test against the pre-fix production file: `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/init-token-source-fail-before.md`
- Pass-after run of the QuickFiler.Test assembly on the post-fix tree: `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/init-token-source-pass-after.md`
- Coverage comparison (before and after, per-file for QfcHomeController.cs and assembly total): `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/coverage-comparison.md`
- Final toolchain pass (format, analyzers, nullable, test; a gate, not an acceptance criterion): `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/toolchain-final-pass.md`

Each projection records `Timestamp:`, `Command:` and `EXIT_CODE:` plus an output summary naming the tests and figures relevant to the criterion it supports.

### Toolchain (gate, not acceptance criterion)

The C# toolchain from CLAUDE.md runs in order (csharpier format and check, msbuild Rebuild with analyzers, msbuild Rebuild with TreatWarningsAsErrors, vstest.console with code coverage), restarting from the first step on any change or failure. Passing it is required to merge and is recorded in the toolchain projection; it is deliberately not listed among the acceptance criteria.

### Not testable in unit scope

The ribbon route cannot be exercised because `LoadQuickFiler()` has no caller and `RibbonController` is excluded from code coverage. The issue's seeded "QuickFiler launched from the ribbon" scenario is not a valid retest for this defect: the ribbon takes the asynchronous path, which was never broken.

## Write Set

- `QuickFiler/Controllers/QfcHomeController.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`
- `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/**`

## Acceptance Criteria

- [x] AC1. In QuickFiler/Controllers/QfcHomeController.cs, the first statement in the body of `QfcHomeController.Init()` is `CreateCancellationToken();` and it precedes the `QfcDataModelLoader` invocation; a reader of the merged method body can confirm this, and no other statement is added to or reordered in `Init()`.
- [x] AC2. QuickFiler/Controllers/QfcHomeController.cs has at most 500 lines after the change (it has exactly 500 before), and the single line removed to make room is one of the two commented-out dead lines identified by the research (the commented debug log inside `LaunchAsync`, line 41 today, or the commented-out `FormViewer` property declaration, line 465 today); no executable line is removed.
- [x] AC3. A new MSTest method `QuickFiler.Controllers.Tests.QfcHomeControllerTests.Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` exists in QuickFiler.Test/Controllers/QfcHomeControllerTests.cs and asserts with FluentAssertions all of the following after calling `Init()`: the `CancellationTokenSource` captured by the `QfcFormControllerLoader` lambda is not null; the token captured by that lambda equals the captured source's `Token`; `_controller.TokenSource` is the same instance as the captured source; the `CancellationToken` captured by the `QfcDataModelLoader` lambda and the one captured by the `QfcQueueLoader` lambda each equal the captured source's `Token` and each have `CanBeCanceled` equal to true.
- [x] AC4. Fail-before evidence exists at `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/init-token-source-fail-before.md` showing `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` run against the pre-fix production file with a non-zero `EXIT_CODE`, an output summary naming that test as failed on the not-null assertion, and `Timestamp:` and `Command:` fields; no raw TRX or coverage XML is committed alongside it.
- [x] AC5. Pass-after evidence exists at `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/regression-testing/init-token-source-pass-after.md` showing the QuickFiler.Test assembly run on the post-fix tree with `EXIT_CODE: 0`, zero failed tests, and an output summary that lists `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` and `Init_InitializesCorrectly` among the passed tests.
- [x] AC6. The same pass-after projection lists `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` (issue #810 AC3) among the passed tests, and QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs is byte-identical to the merge base in the final diff.
- [x] AC7. `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` calls `_controller.Cleanup()` after its assertions, uses no `Thread.Sleep`, `Task.Delay`, timer, filesystem or Outlook interop object, and carries an XML doc comment or leading comment that names issue #839, states the ordering rule it pins (source created before the datamodel loader runs), and declares that `Init()` constructs a real `QfcFormViewer` as pre-existing debt shared with `Init_InitializesCorrectly`.
- [x] AC8. The existing `Init_InitializesCorrectly` test retains its five loader replacements and its four `Assert.AreEqual` assertions unchanged; the only permitted edit to it is the addition of a trailing `_controller.Cleanup()` call.
- [x] AC9. After the change, a content search for `CreateCancellationToken()` invocations (excluding the two declarations) across QuickFiler/ and QuickFiler.Test/ returns exactly five hits: one in QfcHomeController.cs inside `Init()`, three in EfcHomeController.cs (lines 62, 126 and 162 today, unchanged), and one in QfcHomeControllerMetricsTests.cs (line 124 today, unchanged); the research's baseline is zero production invocations on `QfcHomeController` and six family members in total, so the post-change family total is exactly seven.
- [x] AC10. `git diff --name-only` between the merge base and the final commit lists only paths matching the three Write Set entries; in particular the Metrics partial of the home controller, the collection controller, the item controller and its Initialization partial, the form controller and its Actions partial, EfcHomeController.cs, RibbonController.cs, QfcHomeControllerMetricsTests.cs, QuickFiler.csproj and QuickFiler.Test.csproj are absent from that list, and no file with a .xml, .trx or .coverage extension is added anywhere in the repository.
- [x] AC11. Coverage evidence exists at `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/coverage-baseline.md` and `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/coverage-comparison.md`, the comparison records line coverage for QfcHomeController.cs before and after as percentages, the after figure is greater than or equal to the before figure, and the newly inserted statement in `Init()` is reported as covered.
- [x] AC12. The follow-up for remedy (d), removal of the dead synchronous entry path, is enumerated in the Rollout & Follow-up section of this spec by naming all five symbols in scope: `RibbonController.LoadQuickFiler()`, `QfcHomeController.Init()`, `IQfcHomeController.Init()`, `CreateCancellationToken()` and `Init_InitializesCorrectly`. It is deliberately NOT filed from this branch, and no artifact for it appears in this diff. The promotion tooling writes under the potential-features tree, which AC10 excludes from this diff and which this run's staging rules exclude so that a sibling item's queued promotion file is not swept onto this branch; the same constraint led two sibling items in this run to defer their own follow-ups to the run handoff. Filing is therefore the caller's post-run action, not this item's work.

## Risks & Mitigations

- Risk: an implementer inserts the call before the form-controller loader but after the datamodel or queue loader, leaving those two with a never-cancellable token. Mitigation: AC1 fixes the position and AC3's `CanBeCanceled` assertions on the datamodel and queue tokens fail for any later insertion point.
- Risk: the file exceeds 500 lines. Mitigation: AC2; the two dead-comment candidates are identified in advance.
- Risk: the new test allocates a `CancellationTokenSource` that is never disposed. Mitigation: AC7 requires the trailing `Cleanup()`, which reaches lines 389-390.
- Risk: the live `QfcFormViewer` construction inside `Init()` is treated as new policy debt. Mitigation: declared in Test Strategy and pinned by AC7's comment requirement; it is inherited from `Init_InitializesCorrectly`.
- Risk: a concurrent sibling item edits the same partial. Mitigation: the siblings own the Metrics partial, the collection controller and the item controller, none of which this diff touches (AC10); the primary partial is claimed here through the Write Set.
- Risk: a reviewer reads "severity High" as "user-visible today". Mitigation: the reachability finding in Context states plainly that the path is latent.

## Rollout & Follow-up

- Release/rollout steps: none beyond the normal merge. The changed path has no live caller, so there is no runtime rollout risk.
- Follow-up (required by AC12), deferred to the caller as a post-run action and deliberately not filed from this branch: remedy (d), removal of the dead synchronous entry path. Scope is exactly these five symbols: `RibbonController.LoadQuickFiler()`, `QfcHomeController.Init()`, `IQfcHomeController.Init()`, `CreateCancellationToken()` and `Init_InitializesCorrectly`. Justification for removal is the dating above: the path has had no caller since 2024-09-27. Note the ordering constraint, that this follow-up must land after issue 839 merges, because it deletes the very method this item fixes.
- Follow-up (optional, planner may adopt or defer): remedy (e), `tokenSource.ThrowIfNull()` at QfcFormController.cs line 39, so any future path that forgets the factory fails loudly at construction time. If filed, record its reference here: pending.
- Resolved (was a known unknown in the research, which had no git access): the synchronous entry point lost its last call site in commit 9f34ea06d, "Renaming Manager classes and moved to UtilitiesCS", dated 2024-09-27. The call lived in TaskMaster/Ribbon/RibbonViewer.cs. Measured by counting the literal call text in that commit and in its parent: one occurrence before, zero after. The synchronous path has therefore been unreachable for roughly two years, which is why the defect has never been reported from the field and why the severity is latent rather than user-visible.
- Links: issue #839 (https://github.com/drmoisan/TaskMaster/issues/839); research/2026-09-12T18-05-createcancellationtoken-init-path-research.md in this feature folder; issue #810 (AC3, token-source nulling on cleanup); feature 821 in the review-residuals-2026-09-08 epic (origin of observation O-4).
