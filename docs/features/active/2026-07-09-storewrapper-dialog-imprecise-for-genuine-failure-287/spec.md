# storewrapper-dialog-imprecise-for-genuine-failure (Spec)

- **Issue:** #287
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T21-40
- **Status:** Approved for planning
- **Version:** 1.0
- **Work Mode:** full-bug

## Context

`StoreWrapperController.Launch()` gates on `readiness.State != StoreLaunchReadinessState.Ready` and
then shows one hardcoded dialog for every non-`Ready` readiness state:

- message: `Store settings are not available yet. Please try again after startup completes.`
- title: `Store Settings Unavailable`

`StoreLaunchReadinessState` declares exactly three members (`Ready`, `ModelUnavailable`,
`StoresUnavailable`), so both non-`Ready` states collapse onto identical copy. The copy asserts that
retrying after startup will resolve the condition. That assertion is not true for every state the
branch covers, which is the defect.

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

Verified against `origin/main` at `2b85134b42872e405602e6064e02dc9cda6c319b`.

1. `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:120` gates on
   `readiness.State != StoreLaunchReadinessState.Ready`; lines 122-127 call `MyBox.ShowDialog` with
   the single hardcoded message and title, then return without constructing `Viewer`.
2. `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:164` applies the identical gate and
   lines 166-171 show the identical message and title.
3. `StoreLaunchReadinessState` is declared at `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:19-24`
   with three members.
4. `StoreLaunchReadinessEvaluator.Evaluate` at
   `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs:22-39` returns
   `ModelUnavailable` when `globals?.Ol?.StoresWrapper` is null and `StoresUnavailable` when
   `model.Stores` is null.

Exhaustive occurrence inventory (derived twice, by a case-sensitive `git grep` over `*.cs` and by a
case-insensitive cross-check; both passes agree):

| Item | Count in `*.cs` | Locations |
| --- | --- | --- |
| Literal `Store settings are not available yet` | 2 | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:167`, `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:123` |
| Literal `Store Settings Unavailable` | 2 | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:168`, `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:124` |
| Production readiness gates reading `readiness.State` | 2 | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:164`, `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:120` |
| Production call sites of the evaluator | 2 | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:163` (direct), `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:111` (via `EvaluateLaunchReadiness`) |
| Test assertions on the dialog copy | 0 | none exist today |

Both message occurrences are production; no test asserts either literal. The absence claim for tests
is recorded under `Negative evidence` below.

Expected:

The copy shown for a given readiness state must be accurate about what the user can do. In
particular, the copy must not assert that retrying after startup will resolve a condition that
retrying cannot resolve.

Actual:

Both non-`Ready` states produce identical copy asserting that retrying after startup will resolve the
condition.

## Correction to the issue's stated premise

The issue text asserts that one non-`Ready` state is transient and the other is a "genuine/permanent
failure" in which `Globals.Ol.StoresWrapper` is "populated but permanently unable to resolve". That
mapping is not what the code does. The corrected mapping, verified against the load pipeline, is:

- **`ModelUnavailable` is the state that carries the permanent case.** It means
  `Globals.Ol.StoresWrapper` is null, that is the model was never assigned. `AppOlObjects.LoadStoresAsync`
  (`TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:35-73`) assigns the model on both of its success
  paths, and its `catch` block at lines 66-72 swallows the exception, logs
  `Failed to load StoresWrapper; store settings will remain unavailable until this is resolved.`, and
  leaves the property null. `LoadStoresAsync` is reached only from `LoadAsync`, which is called only
  from the startup pipeline. The only two production references to `_olObjects.LoadAsync()` are
  `TaskMaster/AppGlobals/ApplicationGlobals.cs:138`, inside `LoadParallelAsync`, and `:414`, the body
  of `LoadOlObjectsPhaseAsync`, whose only production caller is `LoadSequentialAsync` at `:196`. Both
  are one-shot startup paths. There
  is no retry and no re-entry, so once that `catch` fires the state is terminal for the life of the
  Outlook session. `ModelUnavailable` is therefore *ambiguous*: it is the transient state while
  startup is still running and the terminal state after a caught load failure.
- **`StoresUnavailable` is transient, and is close to unreachable in production.** It means the model
  object exists but `Stores` is null. `StoresWrapper.Init()`
  (`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:37`) always assigns `Stores`, and
  `StoresWrapper.RewireOlObjectsAsync` (declared at
  `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:85`, with `this.Stores ??= [];` at `:87`)
  executes `this.Stores ??= [];` as its first statement before any `await`. Both the fresh-build path
  and the deserialize-then-rewire path therefore leave `Stores` non-null. The state survives only in
  the window between the model assignment at
  `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:47` and the first statement of the rewire, and
  it is defended by the `#240` guard rather than commonly observed.
- The `StoreWrapperController` XML doc at lines 97-103 and the evaluator XML doc at
  `StoreLaunchReadinessEvaluator.cs:15-21` both describe every non-`Ready` condition as transient.
  That description is incomplete for `ModelUnavailable`, and it is the reason the original copy was
  written as an unconditional "try again" instruction.

**Consequence for scope.** The readiness enum cannot distinguish "startup still running" from
"startup finished and the load failed", because both are `ModelUnavailable`. A copy-only change
keyed on the enum therefore cannot deliver the issue's literal Expected Behavior of naming a
permanent-failure case. The achievable and correct objective, adopted by this spec, is narrower and
is what actually fixes the reported harm: **state-specific copy that does not assert a remedy the
state cannot guarantee**, and that names the log as the place the terminal cause is recorded.

Making permanence directly knowable would require a load-failed signal set by the `catch` at
`AppOlObjects.StoreLoading.cs:66`. That is out of scope here: it crosses from `UtilitiesCS` into the
`TaskMaster` project, changes `IOlObjects`, and sits against the deliberate `#240` decision to use
live-state inspection rather than a startup signal. It is recorded under Rollout & Follow-up.

## Scope & Non-Goals

- In scope:
  - A new pure, unit-testable helper in `UtilitiesCS/OutlookObjects/Store/` that maps a
    `StoreLaunchReadinessState` to the message and title to display.
  - Replacing the hardcoded literals at **both** readiness gates with a call to that helper.
  - MSTest coverage of the helper for every enum member and for an undefined cast value.
  - MSTest coverage asserting the copy actually shown by each `Launch()` through the existing
    `MyBox.DialogInvoker` seam.
  - Correcting the XML doc comments that misstate the permanence of `ModelUnavailable` and the
    now-stale `DisabledStoresController.Launch` remark that it "shows the same warning as the
    single-store editor".
- Out of scope / non-goals:
  - Introducing a load-failed flag, a startup-complete event, or any new state-detection mechanism.
  - Adding a member to `StoreLaunchReadinessState`.
  - Any change to `StoreLaunchReadinessEvaluator.Evaluate`'s return values or to the readiness gate
    condition itself.
  - Any change to `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` or to `StoresWrapper.cs`.
  - Removing either `[ExcludeFromCodeCoverage]` attribute from the two `Launch()` shells.
  - Adding a second user-facing notification path.
- Explicitly excluded: live Outlook/COM dependencies in tests; temporary files in tests; any test
  that shows a modal dialog or starts a message pump.

### Scope decision: both call sites, one shared helper

The fix covers **both** call sites. Rationale:

1. The two gates are byte-identical today (message, title, buttons, icon, and the early return).
   Changing one produces a user-visible divergence between two dialogs that are documented as
   deliberately identical.
2. `StoreLaunchReadinessEvaluator` was introduced precisely so, in its own words, "the readiness
   behavior is defined once" (`StoreLaunchReadinessEvaluator.cs:8-12`). Message selection is the
   presentation half of that same readiness decision and belongs in the same shared, testable place.
3. `DisabledStoresController.Launch`'s XML summary states it "shows the same warning as the
   single-store editor" (`DisabledStoresController.cs:154-159`). Fixing one site would silently
   falsify that comment.
4. The incremental cost is one additional call-site edit and one additional test; the incremental
   risk of leaving the second site is a guaranteed follow-up defect.

## Root Cause Analysis

The readiness gate is a single `if` over an inequality (`!= Ready`). It discards the specific state
before the message is chosen, so all non-`Ready` states share one message. The message text was
written in `#240` for the transient case only (see
`docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/plan.2026-07-06T06-41.md:46`, which
specifies the exact literal) and `#262` explicitly deferred correcting it
(`docs/features/archive/2026-07-07-folder-settings-store-model-null-262/spec.md`, Scope & Non-Goals:
"Changing the `StoreWrapperController` 'not available yet' dialog copy for the genuine-failure case
(imprecise but not required by any AC; documented follow-up only)").

The message text is also embedded inline inside a `[ExcludeFromCodeCoverage]` WinForms shell, so no
test can reach it without exercising the shell, and no test does today. That is why the imprecision
survived two features.

## Proposed Fix

### Design summary (what changes where)

Extract state-to-copy selection into a pure static helper and leave only wiring in the two shells.

| File | Change |
| --- | --- |
| `UtilitiesCS/OutlookObjects/Store/StoreLaunchUnavailableMessage.cs` | New. Pure `internal static` class mapping `StoreLaunchReadinessState` to a message/title pair. No WinForms, no `MyBox`, no COM, no `[ExcludeFromCodeCoverage]`. |
| `UtilitiesCS/UtilitiesCS.csproj` | New `<Compile Include>` item for the file above. |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | `Launch()` calls the helper and passes the result to `MyBox.ShowDialog`. XML doc at lines 97-103 corrected. |
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs` | `Launch()` calls the helper and passes the result to `MyBox.ShowDialog`. XML summary corrected. |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreLaunchUnavailableMessageTests.cs` | New. Helper unit tests. |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | New `<Compile Include>` item for the file above. |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` | Extend the two existing `Launch` tests to assert the copy captured through `MyBox.DialogInvoker`. |
| `UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs` | Add `Launch` readiness-copy tests. |

`UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` are non-SDK `packages.config` projects: every
source file is listed explicitly (see `UtilitiesCS/UtilitiesCS.csproj:744` for
`StoreLaunchReadinessEvaluator.cs`). A new `.cs` file that is not added to the project file is not
compiled and the change silently does nothing. Both `<Compile Include>` additions are mandatory.

### Boundaries and invariants to preserve

- The gate condition stays `readiness.State != StoreLaunchReadinessState.Ready`. No state is newly
  admitted to or excluded from the dialog path.
- Both `Launch()` methods keep `[ExcludeFromCodeCoverage]` and keep returning without constructing a
  viewer when not ready.
- `StoreLaunchReadinessEvaluator.Evaluate` is not modified; its five existing tests in
  `StoreWrapperController_Tests.Launch.cs` (lines 110, 127, 146, 168, 189) must continue to pass
  unmodified.
- `MessageBoxButtons.OK` and `MessageBoxIcon.Warning` are unchanged at both sites.
- No production file is added to any coverage exclusion, per `.claude/rules/general-unit-test.md`.

### Technical specifications (interfaces/contracts)

The helper exposes one method. Suggested shape, to be finalized by the plan:

- `internal static StoreLaunchUnavailableCopy For(StoreLaunchReadinessState state)`
- returns a small `readonly struct` (not a `record struct`: `net48` has no `IsExternalInit`) carrying
  `Message` and `Title`.

Contract:

- `ModelUnavailable` returns the model-unavailable copy.
- `StoresUnavailable` returns the still-loading copy.
- `Ready` throws `ArgumentOutOfRangeException`. There is no "unavailable" copy for a ready model, and
  reaching the helper with `Ready` is a caller defect that must fail fast per the General Code Change
  Policy.
- Any other value, reachable only by a cast to an undefined enum member, returns the
  `ModelUnavailable` copy. This is the conservative choice: it never tells the user to retry a
  condition the code cannot characterize.

### Proposed copy

Written to `.claude/rules/tonality.md`: neutral, factual, no hyperbole, no dramatized urgency, no
metaphor.

**`ModelUnavailable`**

- Title: `Store Settings Unavailable`
- Message: `Store settings are not available. The store list may still be loading if Outlook has just started. If this message continues to appear after startup has finished, the store list failed to load and the application log records the reason.`

**`StoresUnavailable`**

- Title: `Store Settings Loading`
- Message: `Store settings are still loading. Please try again in a moment.`

Alternatives considered and not adopted:

- Keeping the original wording for `ModelUnavailable` and changing only `StoresUnavailable`. Rejected
  because `ModelUnavailable` is the state that carries the terminal case, so the inaccurate retry
  instruction would survive on the state that needs it removed.
- Naming a support path or a specific log file in the copy. Rejected: no support path is defined
  anywhere in the repository, and naming one would be an unverifiable claim.
- Wording the `ModelUnavailable` case as an outright failure ("Store settings could not be loaded").
  Rejected because the state is genuinely ambiguous during startup, and that wording would be wrong
  for a user who clicked one second after Outlook launched.

### Error handling and logging updates

No new logging. The helper is pure and performs no I/O. The existing
`AppOlObjects.LoadStoresAsync` error log already records the terminal cause the new copy points at.

## Assumptions, Constraints, Dependencies

- Assumptions: `MyBox.ShowDialog(string, string, MessageBoxButtons, MessageBoxIcon)` continues to set
  `MyBoxViewer.Text` from the title and `MyBoxViewer.TextMessage.Text` from the message
  (the `MessageBoxButtons`/`MessageBoxIcon` entry point at `UtilitiesCS/Dialogs/MyBox.cs:129-139`
  delegates to the overload at `:112-127`, which sets `viewer.Text` and `viewer.TextMessage.Text`
  before invoking `DialogInvoker`), which is what makes the end-to-end assertion
  possible.
- Constraints: `net48`; MSTest + Moq + FluentAssertions only; no temporary files; no live Outlook;
  no test may show a modal dialog. The `MyBox.DialogInvoker` seam is `AsyncLocal`-backed and must be
  saved and restored in a `finally` block, matching the existing pattern at
  `StoreWrapperController_Tests.Launch.cs:34-55` (and again at `:75-96`) and
  `DisabledStoresControllerTests.cs:240-261`.
- Dependencies: none added.

## Data / API / Config Impact

- User-facing changes: two dialog messages and one dialog title change. No API, data, migration, or
  configuration change.
- Compatibility: the helper is `internal`, matching `StoreLaunchReadinessEvaluator`.
  `UtilitiesCS/Properties/AssemblyInfo.cs:19` grants `InternalsVisibleTo("UtilitiesCS.Test")`, which
  is what lets the existing `StoreLaunchReadinessState` tests compile. `UtilitiesCS` does not grant
  `InternalsVisibleTo("TaskMaster")`, so the helper must not be referenced from the `TaskMaster`
  project.

## Test Strategy

- Regression tests to add:
  - `StoreLaunchUnavailableMessageTests.cs`: one test per enum member (3), one for the undefined cast
    value, and one asserting the two returned messages differ.
  - `StoreWrapperController_Tests.Launch.cs`: extend the two existing `Launch` tests to capture the
    invoked `MyBoxViewer` and assert its `TextMessage.Text` and `Text` equal the expected copy for
    `ModelUnavailable` and `StoresUnavailable` respectively.
  - `DisabledStoresControllerTests.cs`: the equivalent two tests for the second call site, also
    asserting `Viewer` remains null.
- Negative and edge scenarios: undefined enum value; `Ready` passed to the helper (expects
  `ArgumentOutOfRangeException`); `Globals` null and `Globals.Ol` null, both of which already resolve
  to `ModelUnavailable` and must keep showing the `ModelUnavailable` copy.
- Determinism: no clock, no randomness, no sleep, no filesystem. The `AsyncLocal` seam is restored in
  `finally`.
- Coverage: the new helper is a new module and must reach `>= 90%`; it has no unreachable branch, so
  the target is full coverage of its four outcomes. Repository-wide line coverage must not regress.
- Toolchain, in order: `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`;
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
  /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; the same with
  `/p:TreatWarningsAsErrors=true`; then the repo test wrapper
  `scripts/vscode/Invoke-MSTest.ps1`, which supplies `/InIsolation` and
  `/TestCaseFilter:TestCategory!=LiveOutlook`, or `scripts/vscode/Invoke-MSTestWithCoverage.ps1` for
  the coverage capture.

## Negative evidence

- Claim: no test asserts either dialog literal today.
  - `SearchScope:` the whole tracked tree of the branch, restricted to `*.cs`.
  - `SearchPatterns:` `Store settings are not available yet`, `Store Settings Unavailable`,
    case-sensitive and case-insensitive.
  - `SearchResult:` 2 hits each, both under `UtilitiesCS/OutlookObjects/Store/`; zero under any
    `*.Test/` path. Known-positive control: the same search over the whole tree (not restricted to
    `*.cs`) returns additional hits in `docs/features/archive/...`, confirming the search mechanism
    is not silently returning nothing.

## Acceptance Criteria

- [ ] **AC1** A new file `UtilitiesCS/OutlookObjects/Store/StoreLaunchUnavailableMessage.cs` exists,
      declares an `internal static` type that maps `StoreLaunchReadinessState` to a message and a
      title, references neither `System.Windows.Forms` nor `MyBox`, and carries no
      `[ExcludeFromCodeCoverage]` attribute.
- [ ] **AC2** `UtilitiesCS/UtilitiesCS.csproj` contains exactly one `<Compile Include>` item naming
      `OutlookObjects\StoreLaunchUnavailableMessage.cs`'s path, and the file compiles into
      `UtilitiesCS.dll`.
- [ ] **AC3** The helper returns a different message string for `ModelUnavailable` than for
      `StoresUnavailable`, asserted by a named MSTest test.
- [ ] **AC4** The helper throws `ArgumentOutOfRangeException` when passed
      `StoreLaunchReadinessState.Ready`, asserted by a named MSTest test.
- [ ] **AC5** The helper returns the `ModelUnavailable` copy when passed an undefined enum value
      produced by a cast, asserted by a named MSTest test.
- [ ] **AC6** Neither `StoreWrapperController.Launch()` nor `DisabledStoresController.Launch()`
      contains a dialog message or title literal; both obtain both strings from the helper. After the
      change, a case-sensitive search for `Store settings are not available yet` over `*.cs` returns
      0 matches, down from the 2 recorded in this spec's inventory table.
- [ ] **AC7** Both `Launch()` methods still carry `[ExcludeFromCodeCoverage]`, still gate on
      `readiness.State != StoreLaunchReadinessState.Ready`, and still return without constructing a
      viewer when not ready.
- [ ] **AC8** A new MSTest test file for the helper exists under
      `UtilitiesCS.Test/OutlookObjects/Store/`, is registered by a `<Compile Include>` item in
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and uses MSTest attributes with FluentAssertions.
- [ ] **AC9** Tests assert the exact message and title observed through the `MyBox.DialogInvoker`
      seam when `StoreWrapperController.Launch()` runs with `ModelUnavailable` and with
      `StoresUnavailable`, and assert the two observed messages differ.
- [ ] **AC10** Equivalent tests exist for `DisabledStoresController.Launch()` covering both non-ready
      states, and assert `Viewer` remains null in both.
- [ ] **AC11** The XML summary on `DisabledStoresController.Launch` no longer states that it shows
      the same warning as the single-store editor.
- [ ] **AC12** The XML doc on `StoreWrapperController` that describes the readiness states records
      that `ModelUnavailable` is also the terminal state after a caught failure in
      `AppOlObjects.LoadStoresAsync`, and cites that location.
- [ ] **AC13** All five existing readiness tests in
      `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` still pass, and
      the two `EvaluateLaunchReadiness` return values are unchanged.
- [ ] **AC14** The full C# toolchain passes in order with zero errors in a single final pass:
      CSharpier check clean, analyzer build clean, nullable build clean, and the MSTest run green.
- [ ] **AC15** No production source file is added to a coverage exclusion by this change, and the new
      helper file is present in the coverage denominator.
- [ ] **AC16** No file exceeds 500 lines after the change, and no production or test file outside the
      table in "Design summary" is modified.

## Risks & Mitigations

- Risk: a new `.cs` file omitted from the non-SDK project file compiles nothing and the change
  appears to work locally in an editor while doing nothing at run time. Mitigation: AC2 and AC8 assert
  the `<Compile Include>` items explicitly.
- Risk: the end-to-end `Launch()` assertions construct a real `MyBoxViewer`. Mitigation: this is the
  established pattern already used by the two existing `Launch` tests and by
  `DisabledStoresControllerTests.cs`; the `DialogInvoker` stub returns without showing a modal
  dialog, and the seam is restored in `finally`.
- Risk: the aggregate multi-assembly `vstest` run intermittently aborts with a test-host crash
  unrelated to this change. Mitigation: re-run per assembly with `/InIsolation` before treating an
  abort as a failure.

## Rollout & Follow-up

- No release, migration, or monitoring step is required.
- **Follow-up, out of scope here.** `ModelUnavailable` conflates "startup still running" with
  "startup finished and the store load failed", because
  `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72` swallows the load exception and leaves
  `StoresWrapper` null with no retry path. Distinguishing the two would require a load-failed signal
  read by `StoreLaunchReadinessEvaluator`, which crosses the `UtilitiesCS`/`TaskMaster` boundary and
  runs against the `#240` decision to inspect live state rather than a startup signal. This should be
  captured as its own potential entry and issue rather than widened into this change.
- Links: issue #287; `#240` (`docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/`);
  `#262` (`docs/features/archive/2026-07-07-folder-settings-store-model-null-262/`); epic `#260`.
