# storewrapper-dialog-imprecise-for-genuine-failure (Spec)

- **Issue:** #287
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T21-55
- **Status:** Approved for planning
- **Version:** 2.0
- **Work Mode:** full-bug

Research backing this spec:
`docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/research/readiness-state-semantics.2026-08-31T21-10.md`.
Every line citation below was re-derived against this branch, which is based on `origin/main` at
`2b85134b42872e405602e6064e02dc9cda6c319b`.

## Context

`StoreWrapperController.Launch()` gates on `readiness.State != StoreLaunchReadinessState.Ready` and
then shows one hardcoded dialog for every non-`Ready` readiness state:

- message: `Store settings are not available yet. Please try again after startup completes.`
- title: `Store Settings Unavailable`

`StoreLaunchReadinessState` declares exactly three members (`Ready`, `ModelUnavailable`,
`StoresUnavailable`), so both non-`Ready` states collapse onto identical copy. The copy asserts
without qualification that retrying after startup will resolve the condition. That assertion is not
true for every case the branch covers, which is the defect.

Environment:

- OS/version: n/a (user-facing copy defect in a WinForms/VSTO shell)
- Command/flags used: n/a
- Data source or fixture: `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`,
  `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs`

Impact / Severity:

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Messaging-accuracy defect only. No functional or data-integrity impact: the gate still prevents the
user from entering a settings dialog bound to an unready model in either case.

## Repro & Evidence

1. `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:120` gates on
   `readiness.State != StoreLaunchReadinessState.Ready`; lines 122-127 call `MyBox.ShowDialog` with
   the single hardcoded message and title, then return without constructing `Viewer`.
2. `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:164` applies the identical gate and
   lines 166-171 show the identical message and title.
3. `StoreLaunchReadinessState` is declared at `StoreWrapperController.cs:19-24` with three members.
4. `StoreLaunchReadinessEvaluator.Evaluate` at
   `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs:22-39` returns
   `ModelUnavailable` when `globals?.Ol?.StoresWrapper` is null and `StoresUnavailable` when
   `model.Stores` is null.

### Exhaustive occurrence inventory

Derived twice by two independent passes (the orchestrator's case-sensitive `git grep` over `*.cs`
plus a case-insensitive cross-check, and the research agent's separate repo-wide pass). Both passes
agree on every production figure.

| Item | Count in `*.cs` | Locations |
| --- | --- | --- |
| Literal `Store settings are not available yet` | 2 production, 0 test | `DisabledStoresController.cs:167`, `StoreWrapperController.cs:123` |
| Literal `Store Settings Unavailable` | 2 production, 0 test | `DisabledStoresController.cs:168`, `StoreWrapperController.cs:124` |
| Production readiness gates reading `readiness.State` | 2 | `DisabledStoresController.cs:164`, `StoreWrapperController.cs:120` |
| Production readiness evaluations | 2 | `DisabledStoresController.cs:163` (calls the evaluator directly), `StoreWrapperController.cs:111` (via `EvaluateLaunchReadiness`, called from `:119`) |
| `StoreLaunchReadinessState` references in `*.cs` | 18 | 14 member references (`.Ready` 6, `.ModelUnavailable` 5, `.StoresUnavailable` 3) plus 4 bare type-name uses at `StoreWrapperController.cs:19`, `:34`, `:44`, `:50` |
| Test assertions on the dialog copy | 0 | none exist today |

The two independent counts of `StoreLaunchReadinessState` reconcile exactly with no residue:
14 member references + 4 bare type-name uses = 18 total lines.

Expected:

The copy shown for a given readiness state must be accurate about what the user can do. It must not
assert without qualification that retrying will resolve a condition that retrying may not resolve.

Actual:

Both non-`Ready` states produce identical copy asserting that retrying after startup will resolve the
condition.

## Correction to the issue's stated premise

The issue text asserts that one non-`Ready` state is transient and the other is a "genuine/permanent
failure" in which `Globals.Ol.StoresWrapper` is "populated but permanently unable to resolve". A
populated model with an unresolvable store list is `StoresUnavailable`. **The issue has the two
states inverted.** The corrected mapping, verified independently by the orchestrator and by the
research agent:

- **`ModelUnavailable` is the state that carries the permanent case.** It means
  `Globals.Ol.StoresWrapper` is null, that is the model was never assigned.
  `AppOlObjects.LoadStoresAsync` (`TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:35-73`) assigns
  the model on both of its success paths (`:47` and `:64`), and its `catch (Exception)` at `:66-72`
  swallows the exception, logs
  `Failed to load StoresWrapper; store settings will remain unavailable until this is resolved.`, and
  leaves the property null. There is no retry, no fallback assignment inside the catch, and no
  re-entry point. `LoadStoresAsync` is reached only from `LoadAsync`; the only two production
  references to `_olObjects.LoadAsync()` are `TaskMaster/AppGlobals/ApplicationGlobals.cs:138`
  (inside `LoadParallelAsync`) and `:414` (the body of `LoadOlObjectsPhaseAsync`, whose only
  production caller is `LoadSequentialAsync` at `:196`). Both are one-shot startup paths, entered
  once per session from `TaskMaster/ThisAddIn.cs:76`. Once that catch fires the state is terminal for
  the remainder of the Outlook session.
- **This permanence is already codified in a passing regression test.**
  `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs:146`,
  `LoadStoresAsync_WhenDeserializeThrows_AbsorbsExceptionAndLeavesStoresWrapperNull`, is commented
  "Path 3 - genuine failure" and asserts `sut.StoresWrapper.Should().BeNull()` together with
  "there is no fresh-build retry after a mid-deserialize exception."
- **`StoresUnavailable` is transient, and no path was found that makes it permanent.**
  `StoresWrapper.Init()` (`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:37`) always assigns
  `Stores` at `:49`, and `StoresWrapper.RewireOlObjectsAsync` (declared at `:85`) executes
  `this.Stores ??= [];` at `:87` as its first statement, before any stopwatch, COM call, or `await`.
  That statement is reached on both the awaited `AwaitStoreRewireAsync` path and the
  fire-and-forget `[OnDeserialized]` path, so even a COM failure during store enumeration still
  leaves `Stores` non-null. The residual window is the interval between the property write at
  `AppOlObjects.StoreLoading.cs:47` and `StoresWrapper.cs:87`, closed by an awaited call in the same
  method with no user action.
- **`ModelUnavailable` is therefore ambiguous, and the enum does not record which case applies.** It
  is the transient state while the startup continuation has not yet run `LoadStoresAsync`, and the
  terminal state after that method completed through its catch block.
- The `StoreWrapperController` XML doc at `:97-103` and the evaluator XML doc at
  `StoreLaunchReadinessEvaluator.cs:15-21` both describe every non-`Ready` condition as transient.
  Neither records the permanent `ModelUnavailable` cause that `#262` later introduced. That omission
  is why the original copy was written as an unconditional retry instruction, and correcting it is in
  scope.

**Consequence for scope.** The permanent/transient split does not map onto the enum. It maps onto the
*cause* of one enum value, which the enum does not carry. A copy change keyed on the enum therefore
cannot tell a user "retrying will not help" without asserting something the state does not know, so
the issue's literal Expected Behavior is not reachable within a copy fix.

The achievable and correct objective, adopted by this spec, is: **state-specific copy that is
accurate for every case the state covers, that does not promise a remedy the state cannot guarantee,
and that names where the terminal cause is recorded.** Making permanence directly knowable would
require a fourth state set inside the `LoadStoresAsync` catch; that is recorded under Rollout &
Follow-up and is out of scope here.

## Scope & Non-Goals

- In scope:
  - Two pure, unit-testable message-selection methods added to the existing
    `StoreLaunchReadinessEvaluator`, which both controllers already depend on.
  - Replacing the hardcoded literals at **both** readiness gates with calls to those methods.
  - MSTest coverage of the methods for every enum member and for an undefined cast value.
  - MSTest coverage asserting the copy actually shown by each `Launch()` through the existing
    `MyBox.DialogInvoker` seam.
  - Correcting the XML doc comments that omit the permanent `ModelUnavailable` cause, and the
    now-stale `DisabledStoresController.Launch` remark that it "shows the same warning as the
    single-store editor".
- Out of scope / non-goals:
  - Introducing a load-failed flag, a startup-complete event, a fourth enum member, or any new
    state-detection mechanism.
  - Any change to `StoreLaunchReadinessEvaluator.Evaluate`'s return values or to the gate condition.
  - Any change to `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs`, `StoresWrapper.cs`,
    `ThisAddIn.cs`, or `ApplicationGlobals.cs`.
  - Any change to any `.csproj`, `.props`, `.targets`, or `packages.config` file.
  - Adding any new source or test file.
  - Removing either `[ExcludeFromCodeCoverage]` attribute from the two `Launch()` shells.
  - Moving user-facing strings into a `.resx`. The research confirmed every user-facing string in
    `UtilitiesCS` is a source literal and the three non-Designer `.resx` files hold no UI copy;
    a resource table would be unmatched convention.
- Explicitly excluded: live Outlook/COM dependencies in tests; temporary files in tests; any test
  that shows a modal dialog, creates a window handle, or starts a message pump.

### Scope decision: both call sites, one shared helper

The fix covers **both** call sites. Rationale:

1. The two gates are byte-identical today (message, title, buttons, icon, and the early return).
   Changing one produces a user-visible divergence between two dialogs that were deliberately made
   identical by `#265`.
2. `StoreLaunchReadinessEvaluator` was introduced so, in its own words, "the readiness behavior is
   defined once" (`StoreLaunchReadinessEvaluator.cs:8-12`). Message selection is the presentation
   half of that same readiness decision and belongs in the same shared, non-exempt, testable type.
3. `DisabledStoresController.Launch`'s XML summary states it "shows the same warning as the
   single-store editor" (`DisabledStoresController.cs:154-158`). Fixing one site would silently
   falsify that comment.
4. The incremental cost is one additional call-site edit and one additional test; the incremental
   risk of leaving the second site is a guaranteed follow-up defect.

## Root Cause Analysis

The readiness gate is a single `if` over an inequality (`!= Ready`). It discards the specific state
before the message is chosen, so all non-`Ready` states share one message. The message text was
written in `#240` for the transient case only; the exact literal is specified in
`docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/plan.2026-07-06T06-41.md:46`. `#262`
then introduced the permanent `ModelUnavailable` cause and explicitly deferred correcting the copy
(`docs/features/archive/2026-07-07-folder-settings-store-model-null-262/spec.md`, Scope & Non-Goals:
"Changing the `StoreWrapperController` 'not available yet' dialog copy for the genuine-failure case
(imprecise but not required by any AC; documented follow-up only)").

The message text is also embedded inline inside a `[ExcludeFromCodeCoverage]` WinForms shell, so it
sits outside the coverage denominator and no test asserts it. That is why the imprecision survived
two features.

## Proposed Fix

### Design summary (what changes where)

Move state-to-copy selection into the existing shared, non-exempt evaluator and leave only wiring in
the two shells. No new file and no project-file edit is required.

| File | Current lines | Change |
| --- | --- | --- |
| `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs` | 41 | Add two pure `internal static` message-selection methods and extend the type's XML doc to record the permanent `ModelUnavailable` cause. |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 478 | `Launch()` obtains both strings from the evaluator instead of inline literals. |
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs` | 180 | Same wiring change; XML summary corrected. |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` | 234 | Add a region of unit tests for the two new methods; extend the two existing `Launch` tests to assert the captured copy. |
| `UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs` | 291 | Add `Launch` readiness-copy tests for the second call site. |

Placement rationale. `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` are non-SDK
`packages.config`-era projects that list every source file explicitly (see `UtilitiesCS.csproj:744`
for `StoreLaunchReadinessEvaluator.cs`), so a new `.cs` file that is not registered compiles into
nothing and the change silently does nothing. `StoreWrapperController.cs` is also 478 lines against
the 500-line cap, leaving 22 lines of headroom. Using the existing 41-line evaluator file
avoids both hazards, keeps the readiness decision and the copy that describes it in one place, and
keeps the diff to five already-registered files.

### Boundaries and invariants to preserve

- The gate condition stays `readiness.State != StoreLaunchReadinessState.Ready`. No state is newly
  admitted to or excluded from the dialog path.
- `StoreLaunchReadinessEvaluator.Evaluate` is not modified. Its five existing tests in
  `StoreWrapperController_Tests.Launch.cs` (at `:110`, `:127`, `:146`, `:168`, `:189`) must continue
  to pass unmodified.
- Both `Launch()` methods keep `[ExcludeFromCodeCoverage]` and keep returning without constructing a
  viewer when not ready.
- `MessageBoxButtons.OK` and `MessageBoxIcon.Warning` are unchanged at both sites.
- No production file is added to any coverage exclusion, per `.claude/rules/general-unit-test.md`.
- `StoreWrapperController.cs` must remain under 500 lines.

### Technical specifications (interfaces/contracts)

Two pure static methods on `StoreLaunchReadinessEvaluator`, following the repository's established
`Build<X>Message` convention for pure copy builders (`UtilitiesCS/Dialogs/MyBoxModeless.cs:123`,
`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:342-370`):

- `internal static string BuildUnavailableMessage(StoreLaunchReadinessState state)`
- `internal static string BuildUnavailableTitle(StoreLaunchReadinessState state)`

Two `string`-returning methods are preferred over a single tuple- or struct-returning method because
the repository precedent returns `string`, and because `net48` with `LangVersion 12.0` has no
`IsExternalInit`, so `record` and `record struct` are unavailable and a hand-written struct would add
a type for no gain.

Contract for both methods:

- `ModelUnavailable` returns the model-unavailable copy.
- `StoresUnavailable` returns the still-loading copy.
- `Ready` throws `ArgumentOutOfRangeException`. There is no "unavailable" copy for a ready model, and
  reaching these methods with `Ready` is a caller defect that must fail fast per the General Code
  Change Policy.
- Any other value, reachable only by a cast to an undefined enum member, returns the
  `ModelUnavailable` copy. This is the conservative choice: it never asserts a retry will succeed for
  a condition the code cannot characterize.

### Selected copy

Selected from the tone-constrained option set in the research artifact (Case T option T2 and Case M
option M3). Written to `.claude/rules/tonality.md`: neutral, factual, no hyperbole, no dramatized
urgency, no metaphor. Brevity has a real effect here: assigning a longer message fires
`MyBoxViewer.GrowTextbox()` (`UtilitiesCS/Dialogs/MyBoxViewer.cs:97-127`), which resizes the form.

**`ModelUnavailable`**

- Title: `Store Settings Unavailable`
- Message: `Store settings are not available. Retry once startup has completed; if the message persists, the store settings failed to load and the application log records the cause.`

**`StoresUnavailable`**

- Title: `Store Settings Loading`
- Message: `The store list has not finished loading. Please try again shortly.`

The `ModelUnavailable` wording bounds the retry advice instead of removing it, which is correct
because the state genuinely is transient during the startup race and genuinely is terminal after a
caught load failure. Pointing at the application log is a supported claim, not a guess: the terminal
path always writes an `Error`-level line with the exception attached at
`AppOlObjects.StoreLoading.cs:68-71`.

Alternatives considered and not adopted:

- Wording `ModelUnavailable` as an outright failure ("Store settings could not be loaded"). Rejected:
  it would be wrong for a user who clicked during the startup race.
- Asserting unconditionally that retrying will not help. Rejected: the state does not support that
  claim, so it would replace one inaccurate assertion with another.
- Naming a support path or a specific log file. Rejected: no support path is defined anywhere in the
  repository, and naming one would be unverifiable.
- A single rewritten message for all non-`Ready` states (research Case S). Rejected: it discards the
  accurate "try again shortly" guidance that `StoresUnavailable` legitimately warrants.
- Keeping the original title for `StoresUnavailable`. Rejected: a distinct title makes the distinct
  condition visible without lengthening the message.

Open item carried forward, not blocking. The issue's own checklist asks to "confirm with the
maintainer what the genuine-failure copy should say (e.g. pointing at logs or a support path)". Both
selected strings assume the application log is acceptable guidance, which the code supports. If the
maintainer prefers a different support path, only the two literals and their assertions change.

### Error handling and logging updates

No new logging. Both methods are pure and perform no I/O. The existing `AppOlObjects.LoadStoresAsync`
error log already records the terminal cause the new copy points at.

## Assumptions, Constraints, Dependencies

- Verified assumption: `MyBox.ShowDialog(string, string, MessageBoxButtons, MessageBoxIcon)`
  (`UtilitiesCS/Dialogs/MyBox.cs:129-139`) constructs a `MyBoxViewer` and delegates to the overload
  at `:112-127`, which sets `viewer.Text` from the title at `:121` and `viewer.TextMessage.Text` from
  the message at `:122`, both before `DialogInvoker(viewer)` at `:125`. A capturing invoker therefore
  observes the final values. `TextMessage` is declared at
  `UtilitiesCS/Dialogs/MyBoxViewer.Designer.cs:175`.
- Constraints: `net48` (`TargetFrameworkVersion v4.8.1`), `LangVersion 12.0`; MSTest + Moq +
  FluentAssertions only; no temporary files; no live Outlook; no test may show a modal dialog.
- The `MyBox.DialogInvoker` seam is `AsyncLocal`-backed (`MyBox.cs:30-45`) and must be saved and
  restored in a `finally` block, matching the existing pattern at
  `StoreWrapperController_Tests.Launch.cs:34-55` (and again at `:75-96`) and
  `DisabledStoresControllerTests.cs:240-261`.
- Apartment state: no attribute change is needed. `StoreWrapperController_Tests` is
  `[TestClass]` + `[DoNotParallelize]` (`StoreWrapperController_Tests.cs:13-14`), not
  `[STATestClass]`, and already invokes `Launch()` through this exact path twice.
  `DisabledStoresControllerTests` is `[TestClass]` and already swaps the same seam.
- Compatibility: the methods are `internal`, matching `StoreLaunchReadinessEvaluator`.
  `UtilitiesCS/Properties/AssemblyInfo.cs:19` grants `InternalsVisibleTo("UtilitiesCS.Test")`.
  `UtilitiesCS` does not grant `InternalsVisibleTo("TaskMaster")`, so neither method may be
  referenced from the `TaskMaster` project.
- Dependencies: none added.

## Data / API / Config Impact

- User-facing changes: two dialog messages and one dialog title change, applied identically at both
  call sites. No API, data, migration, or configuration change.

## Test Strategy

- Unit tests for the two new methods, added as a new region in
  `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs`: one per enum member
  (3), one for an undefined cast value, one asserting the two messages differ, and one asserting
  `Ready` throws. These are the primary coverage vehicle, because the methods are not
  coverage-exempt.
- End-to-end wiring assertions: extend the two existing `Launch` tests in the same file to capture
  the invoked `MyBoxViewer` and assert `TextMessage.Text` and `Text` for `ModelUnavailable` and
  `StoresUnavailable` respectively. Their existing arrange blocks at `:28-32` and `:69-73` already
  produce the two states.
- Second call site: add the equivalent two tests to
  `UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs`, asserting the same copy
  for the same state and that `Viewer` remains null. These tests must not construct
  `DisabledStoresViewer`; the `Ready` path is out of scope.
- Moq caveat carried from the `#240` research: on the `IOlObjects` mock set only `Ol` and
  `StoresWrapper`; do not force a setup on the `Task`-returning `LoadAsync`.
- Determinism: no clock, no randomness, no `Thread.Sleep`, no `Task.Delay`, no filesystem. The
  `AsyncLocal` seam is restored in `finally`.
- Coverage: the two new methods are new code and must reach `>= 90%`; they have four outcomes and no
  unreachable branch. Repository-wide line coverage must not regress.
- Baseline obligation: the research agent had no shell and could not confirm the existing tests are
  green on this branch; its green claim rests on the `#240` pass-after artifact and is documentary.
  Phase 0 must run `StoreWrapperController_Tests` and `DisabledStoresControllerTests` and record the
  observed result before any edit.
- Toolchain, in order, restarting from step 1 on any failure or file change:
  1. `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` for the coverage capture, or
     `scripts/vscode/Invoke-MSTest.ps1` for a plain run. Both wrappers supply `/InIsolation` and
     `/TestCaseFilter:TestCategory!=LiveOutlook`. Run them from the item worktree; the discovery
     filter matches `*.Test.dll` under `bin\<Configuration>\` with no worktree exclusion, so running
     from a checkout that contains nested agent worktrees would sweep foreign assemblies. Expect 9
     test assemblies.

## Negative evidence

- Claim: no test asserts either dialog literal today.
  - `SearchScope:` the whole tracked tree of this branch.
  - `SearchPatterns:` `Store settings are not available yet` and `Store Settings Unavailable`, each
    case-sensitive and case-insensitive, restricted to `*.cs` and then unrestricted.
  - `SearchResult:` 2 hits each in `*.cs`, both under `UtilitiesCS/OutlookObjects/Store/`; zero under
    any `*.Test/` path. Known-positive control: the same searches unrestricted by extension return
    additional hits under `docs/features/archive/`, confirming the search mechanism is not silently
    returning nothing.
- Claim: no production path leaves `StoresWrapper.Stores` null indefinitely.
  - `SearchScope:` the whole tracked tree.
  - `SearchPatterns:` `Stores\s*=[^=]` and `Stores\s*\?\?=` over `*.cs`; `new StoresWrapper` and
    `override.*Init\(\)` over `*.cs`.
  - `SearchResult:` no production override of `StoresWrapper.Init()` exists; the only production
    `new StoresWrapper(...)` is `AppOlObjects.StoreLoading.cs:33`, which calls `.Init()`.
    Known-positive control: the same `new StoresWrapper` query returns 25 or more test-side
    constructions, confirming the pattern matches.

## Acceptance Criteria

- [x] **AC1** `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs` declares two pure
      `internal static` methods that map a `StoreLaunchReadinessState` to a message string and a
      title string. Neither references `System.Windows.Forms` or `MyBox`, neither performs I/O, and
      neither carries `[ExcludeFromCodeCoverage]`.
- [x] **AC2** No new source or test file is added, and no `.csproj`, `.props`, `.targets`, or
      `packages.config` file is modified by this change.
- [x] **AC3** The message method returns a different string for `ModelUnavailable` than for
      `StoresUnavailable`, asserted by a named MSTest test.
- [x] **AC4** Both methods throw `ArgumentOutOfRangeException` when passed
      `StoreLaunchReadinessState.Ready`, asserted by named MSTest tests.
- [x] **AC5** Both methods return the `ModelUnavailable` copy when passed an undefined enum value
      produced by a cast, asserted by named MSTest tests.
- [x] **AC6** Neither `StoreWrapperController.Launch()` nor `DisabledStoresController.Launch()`
      contains a dialog message or title literal; both obtain both strings from the evaluator. After
      the change, a case-sensitive search for `Store settings are not available yet` over `*.cs`
      returns 0 matches, down from the 2 recorded in the inventory table above.
- [x] **AC7** Both `Launch()` methods still carry `[ExcludeFromCodeCoverage]`, still gate on
      `readiness.State != StoreLaunchReadinessState.Ready`, still pass `MessageBoxButtons.OK` and
      `MessageBoxIcon.Warning`, and still return without constructing a viewer when not ready.
- [x] **AC8** Tests assert the exact message and title observed through the `MyBox.DialogInvoker`
      seam when `StoreWrapperController.Launch()` runs with `ModelUnavailable` and with
      `StoresUnavailable`, and assert the two observed messages differ.
- [x] **AC9** Equivalent tests exist for `DisabledStoresController.Launch()` covering both non-ready
      states, asserting the same copy for the same state and that `Viewer` remains null. No test
      constructs a `DisabledStoresViewer` or a `StoreWrapperViewer`.
- [x] **AC10** The XML summary on `DisabledStoresController.Launch` no longer states that it shows
      the same warning as the single-store editor.
- [x] **AC11** The XML doc on `StoreLaunchReadinessEvaluator` records that `ModelUnavailable` is also
      the terminal state after the caught failure at
      `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72`, and cites that location.
- [x] **AC12** All five existing readiness tests in `StoreWrapperController_Tests.Launch.cs` still
      pass, and `StoreLaunchReadinessEvaluator.Evaluate`'s return values are unchanged.
- [x] **AC13** The full C# toolchain passes in order with zero errors in a single final pass:
      CSharpier check clean, analyzer build clean, nullable build clean, and the MSTest run green
      across all 9 test assemblies with no new failure relative to the Phase 0 baseline.
- [x] **AC14** No production source file is added to a coverage exclusion, and the two new methods
      are present in the coverage denominator with measured coverage at or above 90%.
- [x] **AC15** `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` remains under 500 lines,
      and no other file touched by this change exceeds 500 lines. The post-change line count of every
      file in the design-summary table is recorded as evidence.
- [x] **AC16** No file outside the five listed in the design-summary table is modified.

## Risks & Mitigations

- Risk: `StoreWrapperController.cs` is within 22 lines of the 500-line cap, so an incautious
  doc addition breaks the file-size rule. Mitigation: the permanence documentation goes in the
  evaluator file (41 lines), not the controller; AC15 asserts the post-change count.
- Risk: the end-to-end `Launch()` assertions construct a real `MyBoxViewer`. Mitigation: this is the
  established in-repo pattern, already exercised twice by the existing `Launch` tests and by
  `DisabledStoresControllerTests`; the stubbed invoker returns without showing a modal dialog or
  creating a window handle, `MyBox.ShowDialog` disposes the viewer via `using`, and the seam is
  restored in `finally`.
- Risk: a longer message triggers `GrowTextbox()`, which mutates `Size` and `MinimumSize`.
  Mitigation: the selected strings are short, and the assertion reads `TextMessage.Text` rather than
  any size-derived value.
- Risk: the aggregate multi-assembly `vstest` run intermittently aborts with a test-host crash
  unrelated to this change. Mitigation: re-run per assembly with `/InIsolation` before treating an
  abort as a failure; an abort reports `Total tests: Unknown` and carries no verdict.

## Rollout & Follow-up

- No release, migration, or monitoring step is required.
- **Follow-up, out of scope here.** `ModelUnavailable` conflates "startup has not reached the
  store-load phase" with "the store-load phase completed through its catch block", because
  `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72` swallows the load exception and leaves
  `StoresWrapper` null with no retry path. Removing that ambiguity requires recording the failure
  where it happens and surfacing it as a distinct readiness state, which crosses the
  `UtilitiesCS`/`TaskMaster` boundary and changes the `IOlObjects` and evaluator contracts. The
  repository already models this distinction elsewhere: `TaskMaster/AppGlobals/StoreRehookCoordinator.cs`
  defines `StoreRehookOutcome.TransientTimeout` (`:176`) and `StoreRehookOutcome.PermanentError`
  (`:186`) with distinct copy per outcome. This should be captured as its own potential entry and
  issue rather than widened into this change.
- Links: issue #287; `#240` (`docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/`);
  `#262` (`docs/features/archive/2026-07-07-folder-settings-store-model-null-262/`);
  `#265`; epic `#260`.
