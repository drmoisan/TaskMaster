# winformspumphost-suite-determinism (Spec)

- **Issue:** #511 (primary) — https://github.com/drmoisan/TaskMaster/issues/511
- **Secondary Issue:** #571 — https://github.com/drmoisan/TaskMaster/issues/571
- **Parent (optional):** epic `quickfiler-suite-determinism-foundation` (child 1 of 4, wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-21T18-40
- **Status:** Approved
- **Version:** 1.0
- **Work Mode:** `full-bug`
- **Branch:** `bug/winformspumphost-suite-determinism-511`
- **Integration Branch:** `epic/quickfiler-suite-determinism-foundation-integration`

> Acceptance-criteria authority. Work Mode is `full-bug`, so per the `acceptance-criteria-tracking`
> skill this file is the **sole** authoritative acceptance-criteria source for this feature. No
> `user-story.md` exists for this feature and none is to be created. The atomic plan, execution, and
> feature audit are all measured against the `## Acceptance Criteria` section below.

> Evidence-location invariant. Every evidence artifact this feature produces goes under
> `docs/features/active/winformspumphost-suite-determinism-511/evidence/<kind>/`. No `artifacts/`
> sub-path other than `artifacts/orchestration/` may hold evidence.

## Context

- **Summary of the bug and its impact.** Two open defects describe one underlying condition in the
  `QuickFiler.Test` suite. `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` starts a real WinForms
  message pump on a dedicated STA thread by calling `Application.Run(new ApplicationContext())` at
  `:325-326` and never adds a form or a control, so no native window handle is ever created on the
  pump thread. The pump harness constructs a real `QuickFiler.ItemViewer` (a `UserControl`) on that
  thread and never parents it or forces handle creation. Two pump-hosted initialization tests reach
  `Control.Invoke` through `QfcItemController.InvokeBeginInvoke` and fail with
  `InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the window
  handle has been created` (#571). Separately, the pump-hosted suite has been reported load-flaky:
  six full-suite attempts were required to obtain one clean baseline under sustained high CPU load
  during issue #438 work on 2026-08-08 (#511). Requirements sources:
  `docs/features/potential/promoted/2026-08-08-winformspumphost-tests-load-flaky-visible-window.md`
  and `docs/features/potential/promoted/2026-08-15-qfc-item-controller-init-tests-flaky-window-handle.md`.
  Primary evidence source:
  `docs/features/active/winformspumphost-suite-determinism-511/research/winformspumphost-suite-determinism.2026-08-21T18-20.md`.
- **Observed environment(s).** Windows 11 Pro 10.0.26200; .NET Framework 4.8.1; MSTest executed via
  `vstest.console.exe` (VS18 test platform) across nine `*.Test.dll` assemblies with
  `/EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`. #511 was observed
  through `./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`.
- **Customer impact and severity.** No production defect. The affected parties are reviewers and
  autonomous agents: a suite that fails on some runs and passes on others trains both to re-run
  rather than investigate, and it can fail an otherwise-green protected check at random. #511 is
  recorded High; #571 is recorded Medium.
- **First observed date and version(s) impacted.** #511 observed 2026-08-08 during issue #438
  orchestration. #571 observed 2026-08-15 (run 1 passed both named tests, run 2 failed both, run 3
  passed both). Both conditions are pre-existing on `main`; `WinFormsPumpHost` was introduced by
  issue #230, which is closed.

## Repro & Evidence

- **Steps to reproduce (#571).** Build the solution in Debug, then run the full nine-assembly suite
  with `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation
  /TestCaseFilter:"TestCategory!=LiveOutlook"` and repeat. The two named tests fail on some runs and
  pass on others. Running only
  `/TestCaseFilter:"FullyQualifiedName~QfcItemController_InitializationTests"` passed 9 of 9 on
  every recorded attempt.
- **Steps to reproduce (#511, load-flakiness half).** Drive the machine to sustained high CPU
  utilization (observed at approximately 96%), run the full suite with coverage, and repeat. Six
  attempts were required for one clean baseline on 2026-08-08.
- **Expected vs actual behavior.** Expected: identical inputs and environment produce identical
  results, per `.claude/rules/general-unit-test.md`. Actual: the two named tests fail
  intermittently, and the pump-hosted suite as a whole degrades under CPU contention.
- **Logs/screenshots/error snippets.** The #571 stack trace, extracted from the TRX of the failing
  2026-08-15 run:

  ```
  System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a
  control until the window handle has been created.
     at System.Windows.Forms.Control.MarshaledInvoke(...)
     at System.Windows.Forms.Control.Invoke(Delegate method, Object[] args)
     at QuickFiler.ItemViewer.QuickFiler.IItemViewer.Invoke(Delegate method)
     at QuickFiler.Controllers.QfcItemController.InvokeBeginInvoke(Boolean async, Action action)
        in QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:line 256
     at QuickFiler.Controllers.QfcItemController.ToggleTips(Boolean async, ToggleState desiredState)
        in QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:line 204
  ```

  No failure log was retained for #511; the observation is recorded in the issue #438 execution
  report. A fresh capture under induced load accompanies this fix (see `## Test Strategy`).
- **Frequency / determinism.** #571 is intermittent and correlates with full-suite execution rather
  than class-isolated execution. #511's load-flakiness is intermittent and correlates with CPU
  contention. #511's visible-window observation is a single recorded event with no retained log; it
  is re-attributed rather than reproduced (see `## Root Cause Analysis` and
  `## Rollout & Follow-up`).

## Scope & Non-Goals

### In scope

- Deterministic creation of the `ItemViewer` window handle on the pump thread inside the shared
  pump harness, so that `Control.Invoke` has an existing handle before any act.
- #571 in full: both named intermittent failures.
- #511's **load-flakiness half**: removing the handle race removes one whole class of the load-
  induced failures reported in #511.
- Regression tests for the new fixture invariant, placed in
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`.
- An empirical pre-fix and post-fix determinism record captured as evidence.

### Out of scope / non-goals

- **#511's visible-window half is out of scope and is re-attributed.** The evidence does not
  support the causal claim in #511's Actual Behavior bullet that the visible window is produced by
  `WinFormsPumpHost`. See `## Root Cause Analysis`. This half is recorded under
  `## Rollout & Follow-up` as requiring its own issue against
  `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`. It is deliberately **not** an acceptance
  criterion of this feature, so that the feature audit does not score it as unmet.
- **#511's literal proposed remedy is rejected**: replacing the real message pump with an injectable
  synchronization-context or dispatcher seam. Rationale in `## Root Cause Analysis` and
  `## Proposed Fix`.
- **No production file changes.** `QfcItemController.InvokeBeginInvoke` keeps its current shape in
  this feature. Making it consult `InvokeRequired`/`IsHandleCreated` is a production behaviour
  change and belongs to its own issue.
- **No `IItemViewer` member additions.** The `IItemViewer` UI-thread seam consolidation is issue
  #489, assigned by `epic.md` Non-Goals to the third epic's ItemViewer child.
- **No `QuickFiler.Test/QuickFiler.Test.csproj` edit**, therefore no new test file.
- **No `.claude/**` edit.** Rule files are the policy this fix is measured against, not edit
  targets.
- **The MSTest `[Timeout]` / `UiThreadDispatcherGate` cascade is not fixed here.** The research
  identifies a second, independent load amplifier: MSTest's `[Timeout]` on a `Task`-returning test
  records a failure without aborting the continuation, so a timed-out pump test has not yet run its
  `finally` and therefore has not released the process-wide `UiThreadDispatcherGate` semaphore or
  reverted `UtilitiesCS.UiThread._dispatcher`. `[DoNotParallelize]` on
  `QfcItemController_InitializationTests` and `QfcItemController_SeamFactoryTests` is a candidate
  mitigation with no timing content, but it is not part of the minimal fix and is recorded as a
  follow-up candidate under `## Rollout & Follow-up`.
- **Residual CPU-contention sensitivity is stated, not claimed fixed.** Running real message pumps
  under approximately 96% load remains inherently slower; retaining the pump-hosted coverage is a
  deliberate trade.

### Explicitly excluded systems, integrations, or datasets

- `UtilitiesCS.Test` (all files), including `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`.
- `QuickFiler.Test/Form1.cs` and its designer and resource entries — owned by sibling child #491.
- The appended `Controllers` compile entry in `QuickFiler.Test/QuickFiler.Test.csproj` — owned by
  sibling child #449.
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` — 497 lines, three lines of
  headroom against the 500-line cap; nothing is added to it.
- `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` — 482 lines, 18 lines of headroom; the chosen
  remedy does not touch it.

## Root Cause Analysis

### Confirmed root cause of #571

`QfcItemController.InvokeBeginInvoke` (`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:248`)
reaches `_itemViewer.Invoke(action)` at `:256`. `Control.Invoke` throws
`InvalidOperationException` unless a control in the target's parent chain has a created native
handle. `WinFormsPumpHost.RunPumpThread` calls `Application.Run(new ApplicationContext())` at
`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:325-326` and never adds a form or control, so no
handle is created on the pump thread; the harness constructs the real `ItemViewer` on that thread
and never parents it. `_itemViewer` is `IItemViewer` (`QuickFiler/Controllers/QfcItemController.cs:51`),
bound to that real viewer, and `IsHandleCreated` is `false` for the whole test.

Only two of the six pump-hosted tests reach that call:

- `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` — `Initialize(bool async: false)`
  (`Initialization.cs:168`) → `Initialization.cs:185` `ToggleTips(async: false, ...)` →
  `FocusAndTheme.cs:204` → `:256`.
- `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` — the private nine-arg
  `Initialize` (`Initialization.cs:138`) funnels into the same path at `Initialization.cs:161`.

The mechanism that exonerates the other four is `Control.InvokeRequired`, which searches the parent
chain for a created handle and returns `false` when none is found. The asynchronous paths do not
call `InvokeBeginInvoke` at all — they marshal through the injected `IUiDispatcher` — and the one
sibling that marshals synchronously, `Theme.SetQfcTheme(false)`, is guarded by
`_lblItemNumber.InvokeRequired` at `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:433`.
`ViewerSetup.cs:361` uses the same guard inside `AssignControls`. `InvokeBeginInvoke` is the sole
unguarded marshaller. Note that `Control.BeginInvoke` throws identically on a handle-less control,
so the `async == true` branch is not inherently safe; it is simply never taken by the failing tests.

### Signals/evidence supporting it

- The #571 TRX stack trace names `FocusAndTheme.cs:256` and `:204` exactly.
- Static trace of all six pump-hosted tests, recorded as a per-test table in the research artifact
  (Q2.2), matches the observed failure set exactly: rows 3 and 4 reach `Control.Invoke`, rows 1, 2,
  5, and 6 do not.
- `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` contains no `new Form`, no `.Show()`, and no
  `.ShowDialog()`.

### Unresolved question — the intermittency mechanism is not explained

Static reading predicts that both named tests fail on **every** run, because `Control.Invoke`
throws unconditionally without a handle and the research found no handle-creating call anywhere in
the `ResolveControlGroups` → `SetupThemes` → `PopulateControls` path (`ResolveControlGroups` walks
only `Control.Controls`; `QfcTipsDetails` contains no `Handle`/`CreateControl`/`CreateGraphics`
reference; `QfcThemeHelper.SetupThemes` only captures control references; `AssignControls` sets
cached properties). #571 nevertheless records the tests passing on some runs. Two explanations
remain open:

1. Some third-party path creates the `ItemViewer`'s handle non-deterministically. The prime suspect
   is the `Microsoft.Web.WebView2.WinForms.WebView2` control's `ISupportInitialize.EndInit` or
   implicit-initialization logic (`QuickFiler/Viewers/ItemViewer.Designer.cs:46,49`, wrapped in
   `BeginInit`/`EndInit` at `:89-90` and `:6166-6167`). That code is not present in this repository
   and could not be read.
2. The recorded observation attributes a different failure mode to these two test names.

**This question is deliberately left open and must not be closed by assertion.** The chosen remedy
is correct under either explanation, because forcing the handle removes the dependency in the
passing direction whichever holds. `## Test Strategy` requires that the pre-fix failure behaviour
be established **empirically**, by repeated runs with the observed `IsHandleCreated` value recorded,
rather than asserted from static reading.

### #511's visible-window symptom is re-attributed, not fixed

The visible window is **not attributable to anything in this feature's blast radius**:

- `Application.Run(ApplicationContext)` shows a window only through `context.MainForm`, and the
  parameterless `ApplicationContext` constructor leaves `MainForm` null.
- `WinFormsPumpHost` constructs no `Form`, `UserControl`, or `Control` at all.
- `QuickFiler.Test/Form1.cs` has **zero construction sites**; it appears only in its own
  declaration, its designer, the project-file entries, and a stale `.csproj.bak`.
- The `ItemViewer`'s two WebView2 children never obtain a handle, because nothing in the
  initialization path creates the parent's handle, and the harness injects a
  `Mock<IWebViewCoreInitializer>` whose members throw `WebViewSentinelException`.
- The two windows that *are* created on the pump thread are a WPF message-only dispatcher window
  and the WinForms parking window. Neither is a desktop window and neither is shown.

A repository-wide search across every test project yields exactly one enabled call that shows a
real top-level `Form`: `viewer.Show()` at `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73`,
on a `ProgressViewer` that derives from `Form` (`UtilitiesCS/Threading/ProgressViewer.cs:16`) inside
an `[STATestClass]`. That is a different assembly, outside this epic, and the re-attribution is a
code reading rather than a reproduced observation. Consequently **#511's visible-window half is out
of scope here and must be filed as its own issue**; this feature does not claim a fix it cannot
make.

### Why #511's literal remedy is rejected

Replacing the real pump with an injectable synchronization-context seam would re-exempt coverage
that issue #230 deliberately de-exempted, which epic hard constraint 3 forbids. The affected
justifications, enumerated precisely:

| Block | `QfcItemController.Initialization.cs` line | Member | Named coverage evidence |
| --- | --- | --- | --- |
| A | 135 | private nine-arg `Initialize` (`:138`) | `InitializeNineArgOverload_ThroughThePumpHost_*` — one of the two failing tests |
| B | 164 | `Initialize(bool async)` (`:168`) | `InitializeBool_ThroughThePumpHost_*` — the other failing test |
| C | 196 | `InitializeAsync()` (`:202`) | `InitializeAsync_ThroughThePumpHost_*` |
| D | 259 | `InitializeGraphicsAsync()` (`:263`) | `InitializeGraphicsAsync_ThroughThePumpHost_*` |
| E | 291 | `InitializeSequentialAsync()` (`:295`) | `InitializeSequentialAsync_ThroughThePumpHost_*` |
| F | 403 | `CreateAsync(...)` (`:409`) | `CreateAsync_WithFaultingWebViewSeam_*` |
| G | 447 | `CreateSequentialAsync(...)` (`:451`) | `CreateSequentialAsync_WithInjectedSeams_*` |

That is **seven** de-exemption blocks, not the five the epic manifest cited; `:135` was omitted and
is the target of one of the two failing tests. There is **one further de-exemption** at
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:254` (member `ResolveControlGroupsAsync`
at `:258`). Correction to the epic's citation: `ViewerSetup.cs:30` with its
`[ExcludeFromCodeCoverage]` attribute at `:41` is a **retained** exemption block, not a
de-exemption; the epic cited `:31`. The seven de-exemption line numbers above are exact against the
current worktree, so the "re-derive every line number" instruction resolved to a completeness
correction rather than a drift correction.

Every one of the eight pump-hosted consumer tests is the named coverage evidence for at least one
de-exempted production member. Deleting, `[Ignore]`-ing, or reclassifying any of them out of the
unit suite invalidates the corresponding comment and re-opens the exemption question. Two further
arguments against the literal remedy: the seam it proposes **already exists** (`IItemViewer`
re-declares `InvokeRequired`/`Invoke`/`BeginInvoke` at `QuickFiler/Viewers/IItemViewer.cs:135-137`
for mockability, and `IUiDispatcher` is held at `QfcItemController.cs:66`; both are already
exercised pump-free in `QfcItemController.FocusAndThemeTests.cs:99-115`), and it would not fix the
thing it was filed for, because the visible window is not the pump's.

### Affected components/modules

- Test support and harness (changed): `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`,
  `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`,
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`.
- Production (read, **not** changed): `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`,
  `QfcItemController.Initialization.cs`, `QfcItemController.ViewerSetup.cs`,
  `QuickFiler/Viewers/ItemViewer.cs`, `QuickFiler/Viewers/IItemViewer.cs`,
  `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`.

## Proposed Fix

### Design summary (what changes where):

**Retain the real message pump; make the test fixture deterministic; re-scope #511's visible-window
claim.** This is the recorded reconciliation decision for the tension the epic assigns this child to
settle, and it is not re-opened by the plan.

Force invisible window-handle creation for the `ItemViewer` **on the pump thread**, inside the
shared harness, by reading `viewer.Handle`. Two sites:

1. `BuildPumpHarnessCoreAsync` in
   `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, immediately after
   the viewer is constructed on the pump thread, inside the same `host.InvokeAsync` body (or a
   second `InvokeAsync` on the same host).
2. The equivalent point in the standalone arrange block of
   `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` near `:426`-`:435`, which
   builds its own viewer and does **not** go through `BuildPumpHarnessAsync`. Forcing the handle in
   both places keeps the two sites symmetric; leaving only one would be a latent trap even though
   that consumer does not currently reach `Control.Invoke`.

**Prefer reading `.Handle` over calling `CreateControl()`.** Reading `Control.Handle` forces
creation of that control's handle only and is non-recursive. `Control.CreateControl()` is
`Visible`-gated and **recurses into every visible child control**, which on `ItemViewer` would drag
both `Microsoft.Web.WebView2.WinForms.WebView2` controls into handle creation — exactly the
third-party surface the research flags as unverified. `.Handle` is therefore strictly the narrower
instrument. For a parentless child control WinForms parks the new HWND on the thread's hidden
parking window, which is never shown, so nothing becomes visible.

### This is not a prohibited timing hack

`.claude/rules/csharp.md` "Prohibited Behaviors" bans "adding sleeps, retries, or timing hacks to
mask flaky behavior". Reading `Control.Handle` is none of those, and the distinguishing test is
whether the race still exists after the change:

- A sleep, a retry, or a timing tolerance leaves the race in place and only lowers the probability
  of observing the failure. The failure remains reachable.
- Reading `viewer.Handle` on the pump thread before the act **eliminates the precondition of the
  failure**. `IsHandleCreated` is then `true` unconditionally, for the whole lifetime of the
  fixture, on every machine, at every load level. There is no residual window in which the test can
  fail for this reason, so there is nothing left to mask.

It is also not a wall-clock wait, not probabilistic, and not order-dependent — the three properties
the "Determinism Infrastructure" section of `.claude/rules/general-unit-test.md` constrains. The
in-repo precedent is maintainer-ratified: `Tags.Test/TagControllerRendering.StaTests.cs` does
exactly this, with the comment `// Act: force invisible handle creation, then invoke the real draw
path.` followed by `var handle = checkBox.Handle;` and a later
`checkBox.IsHandleCreated.Should().BeTrue();`, and its class documentation records that the test
never shows a window and uses no message pump, timer, or sleep. A second precedent is
`UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs:233` (`_ = browser.Handle;`).

This reading is stated on the record because #571's own "Suspected Cause / Notes" asserts the
opposite — that a handle-forcing call would violate the prohibition. That sentence is the single
point where the promoted record and the epic disagree; the epic's reading governs, and the argument
above is the one the epic asks this spec to supply.

### Boundaries and invariants to preserve:

1. **No `.csproj` edit.** `QuickFiler.Test/QuickFiler.Test.csproj` carries 116 explicit
   `<Compile Include>` entries and **zero wildcard includes**, so no new test file can be compiled.
   Sibling #491 owns the `Form1` region; sibling #449 owns one appended `Controllers` entry. All
   regression tests therefore go in a file that already carries an entry, specifically
   `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` (290 lines, 210 of
   headroom).
2. **Preserve the cross-class serialization.**
   `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:51` defines
   `UiThreadDispatcherGate`, a `SemaphoreSlim(1, 1)`, and `SwapUiThreadDispatcher` at `:139` mutates
   the process-wide static `UtilitiesCS.UiThread._dispatcher` by reflection.
   `QfcItemController_SeamFactoryTests` (`SeamFactoryTests.cs:29`) is a **separate** `[TestClass]`
   that acquires the same gate by calling the `internal static BuildPumpHarnessAsync` at `:313` and
   `:384`. `BuildPumpHarnessCoreAsync` is therefore the single choke point for both consumer
   classes — which is exactly why the fix belongs there — and any change must preserve the gate or
   the two classes deadlock under class-level parallelization.
3. **Change no production file.** The fix is confined to test-support and harness code, so
   production behaviour is unchanged by construction.
4. **Do not add a member to `IItemViewer`.** A handle-guard remedy would require `IsHandleCreated`
   on the interface, which re-declares only `InvokeRequired`/`Invoke`/`BeginInvoke` at
   `QuickFiler/Viewers/IItemViewer.cs:135-137` (the epic's `:95-100` citation is drifted by +40).
   The `IItemViewer` seam consolidation is issue #489, assigned to a later epic.
5. **No `.claude/**` edit.**
6. **Preserve all 21 pump-host call sites.** 13 self-tests in
   `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` (at `:32`, `:59`, `:88`, `:115`, `:153`,
   `:183`, `:218`, `:270`, `:302`, `:334`, `:367`, `:395`, `:416`) plus 8 consumer tests. The
   research read all 13 self-tests: none asserts handle absence and none asserts on any pump-host
   internal beyond its public surface, so the harness-level remedy has a blast radius of zero on
   them.
7. **The 500-line cap** applies to every touched file.

### Dependencies or blocked work:

- None inbound. This child sits in wave 0 with an empty dependency graph.
- Outbound: two follow-up issues are identified under `## Rollout & Follow-up`; neither blocks this
  fix.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| File | Current lines | Change | Cap headroom after |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 409 | force `viewer.Handle` on the pump thread in `BuildPumpHarnessCoreAsync`, with an explanatory comment | ~85 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 467 | force `viewer.Handle` on the pump thread in the standalone arrange block near `:426`-`:435` | ~30 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 290 | add the regression tests | ~105 |

No other file is modified. Files deliberately **not** touched, with their headroom recorded because
the cap pressure is real: `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` at 443 lines (57 of
headroom), `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` at 497 lines (3 of
headroom), and `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` at 482 lines (18 of headroom).

#### Functions/classes/CLI commands impacted:

- `QfcItemController_InitializationTests.BuildPumpHarnessCoreAsync` (harness; changed).
- `QfcItemController_ViewerSetupTests.ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups`
  arrange block (changed).
- `QfcItemController_InitializationTests` (new regression tests added).
- `QfcItemController_SeamFactoryTests` (unchanged; consumes the changed harness through
  `BuildPumpHarnessAsync`).
- `WinFormsPumpHost` and `WinFormsPumpHostTests` (unchanged).

#### Data flow and validation changes:

None. No data, no serialization format, no configuration key changes. The only behavioural delta is
that the fixture's `ItemViewer` has a created native window handle from the moment the harness
returns.

#### Error handling and logging updates:

None. No logging pattern changes. The failure mode being removed is an exception thrown by the
framework, not a logged condition.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. Rollback is a revert of the three test files; no production surface is affected, so
a revert cannot regress runtime behaviour.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

No public interface changes. No member is added to `IItemViewer` or to `WinFormsPumpHost`. The
harness's returned `PumpHarness` shape is unchanged; only the state of the viewer it exposes
changes.

#### Required configuration keys and defaults:

None.

#### Backward-compatibility expectations:

Full. The production assembly is byte-compatible because no production file changes.

#### Performance constraints (latency/throughput/memory):

Handle creation for one `UserControl` is a single native window creation and is not a measurable
cost against a 60-second harness bound. The pump-hosted timeout constants are unchanged:
`PumpTimeoutMs = 60000` at `QfcItemController.InitializationTests.cs:38`,
`ViewerSetupTests.cs:34`, and `SeamFactoryTests.cs:293`; `TimeoutMs = 30000` at
`WinFormsPumpHostTests.cs:24`. No timeout value is raised, because raising a timeout is a timing
tolerance and is prohibited.

## Assumptions, Constraints, Dependencies

### Assumptions (environment, data, access)

- Reading `Control.Handle` forces creation of that control's handle and is non-recursive, while
  `Control.CreateControl()` is recursive — documented `System.Windows.Forms.Control` behaviour, high
  confidence, not executed in this repository. The minimality consequence (WebView2 children remain
  handle-less) is asserted by a named regression test rather than assumed.
- `Control.InvokeRequired` returns `false` when no control in the parent chain has a created handle
  — documented behaviour, and the mechanism that explains why four of the six pump-hosted tests do
  not fail.
- Nine `*.Test.dll` assemblies constitute the full suite, and they are discoverable from the build
  output with `\.claude\` excluded.

### Constraints (budget, performance, compatibility)

- No `.csproj` edit; no new test file (constraint 1 above).
- No production file change (constraint 3 above).
- No `IItemViewer` member addition (constraint 4 above).
- No `.claude/**` edit (constraint 5 above).
- The `UiThreadDispatcherGate` serialization must survive (constraint 2 above).
- Every touched file stays under 500 lines.
- MSTest + Moq + FluentAssertions only. No temporary files. No sleeps, retries, or timing
  tolerances.
- Evidence goes only under
  `docs/features/active/winformspumphost-suite-determinism-511/evidence/<kind>/`.
- **No Python toolchain exists in this repository** — there is no `scripts/dev_tools/` and no Poetry
  manifest — so no Python command appears anywhere in this spec or in the plan derived from it. A
  skill step naming one is unrunnable by absence and must be reported as such.

### External dependencies (services, libraries, releases)

- `Microsoft.Web.WebView2.WinForms` — third-party, read-only here. Its implicit-initialization
  behaviour is the prime suspect for the unresolved intermittency question and is probed by a named
  regression test rather than by reading its source, which is not present in this repository.
- No new package reference is added.

### Known side effect the plan must anticipate

Forcing the `ItemViewer`'s handle flips currently-`false` `InvokeRequired` guards to `true` whenever
they are evaluated off the pump thread. Two are on paths under test:

- `Theme.cs:433` `_lblItemNumber.InvokeRequired`, evaluated during `InitializeGraphicsAsync`'s
  `SetThemeDark(async: false)`, which resumes on a thread-pool thread after `await Task.Run(...)`.
  It will now marshal to the pump thread via `Theme.cs:435` instead of running inline.
- `ViewerSetup.cs:361` `_itemViewer.InvokeRequired` in `AssignControls`, reached from
  `PopulateControlsAsync` → `AssignControlsAsync`.

Both should succeed, because a live pump is precisely what the fixture supplies, and both become
more production-faithful. This is nevertheless a genuine behaviour change in the tests and is the
most likely source of a surprise during execution, which is why "the other four pump-hosted tests
still pass" is an explicit acceptance criterion rather than an assumption.

## Data / API / Config Impact

- **User-facing or API changes:** none. No production file changes, so no public or internal
  production API is affected.
- **Data or migration considerations:** none.
- **Logging/telemetry updates (if any):** none.
- **Compatibility notes (CLI flags, config schemas, versioning):** no change to `coverage.config`,
  `Directory.Build.targets`, `quality-tiers.yml`, any `*.csproj`, or any workflow file. The
  `vstest.console.exe` invocation is unchanged from the repository standard, including the mandatory
  `/InIsolation`.

## Test Strategy

### Empirical pre-fix baseline (required, must not be replaced by static reasoning)

Before the fix, establish the failure behaviour of the two named tests **by repeated execution**:

1. Run `/TestCaseFilter:"FullyQualifiedName~QfcItemController_InitializationTests"` ten times and
   record per-run pass/fail for each named test.
2. Run the full nine-assembly suite ten times and record per-run pass/fail for each named test.
3. In the same runs, record the observed `IsHandleCreated` value for the harness viewer, so the
   unresolved intermittency question in `## Root Cause Analysis` is answered by observation. If the
   value is `false` while the test passes, the static reading of `Control.Invoke` is wrong; if it is
   sometimes `true`, something third-party creates the handle and that something is named before the
   fix is described as minimal.
4. Record the result under
   `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/`.

The pre-fix behaviour is **not** to be asserted from static reading. The chosen remedy does not
change with the answer; only the explanation recorded in the spec does.

### Regression tests to add or update

All in `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` (210 lines of
headroom), because no new file can be compiled without a `.csproj` edit:

1. `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` — asserts the harness viewer's
   `IsHandleCreated` is `true` and that, queried on the pump thread, `InvokeRequired` is `false`.
   This is the failing-test-first artifact required by the Bugfix Workflow: it fails
   deterministically before the fix and passes after, which is a better regression than depending on
   the intermittent end-to-end symptom.
2. `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` — asserts both WebView2 children remain
   handle-less after the fix, pinning the minimality property of `.Handle` over `CreateControl()`
   and simultaneously probing open question 2.

The negative case — `Control.Invoke` throwing without a handle — is deliberately **not** added,
because it would assert framework behaviour rather than repository behaviour.

### Unit tests for the fixed behaviour and boundaries

MSTest `[TestClass]`/`[TestMethod]`, Moq for the seam doubles already used by the harness, and
FluentAssertions for every new assertion. Scenario coverage for the new fixture invariant:
positive (handle created on the pump thread), boundary (`InvokeRequired` is `false` on the pump
thread), and minimality (children remain handle-less).

### Edge cases and negative scenarios

- The other four pump-hosted consumer tests must still pass after the `InvokeRequired` guards flip
  (see "Known side effect").
- `QfcItemController_SeamFactoryTests` must still pass in the same run as
  `QfcItemController_InitializationTests`, proving the `UiThreadDispatcherGate` serialization
  survived.
- All 13 `WinFormsPumpHostTests` self-tests must still pass, including the post-`StopAsync`
  `ObjectDisposedException` and `Dispose`-idempotence cases.

### Error handling and logging verification

Not applicable: no error-handling or logging code changes. The verification is the absence of the
`InvalidOperationException` in the TRX output.

### Coverage impact and targets for changed lines/modules

`QuickFiler/Viewers/ItemViewer.cs` carries a whole-type `[ExcludeFromCodeCoverage]` at `:20`, so the
fixture change moves no coverage into or out of the denominator. `QfcItemController` coverage must
not regress; the seven `Initialization.cs` de-exemption blocks plus the `ViewerSetup.cs:254`
de-exemption are the checklist. Coverage is captured with `/EnableCodeCoverage` and the report is
stored under `evidence/qa-gates/`, with the pre-fix figure under `evidence/baseline/`.

### Toolchain commands to run (format → lint → type-check → test)

Run in this exact order and restart from the first step if any step fails or changes files:

1. `dotnet tool restore`
2. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

Binding details:

- Always `/t:Rebuild`, never `/t:Build`. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
  project and runs no analyzers.
- Never add `/p:Nullable=enable`. No project carries a `<Nullable>` element and there is no
  `Directory.Build.props`, so the property conscripts files that never opted in and diverges from
  `.github/workflows/ci.yml`.
- `/InIsolation` is mandatory. Without it each assembly's `app.config` binding redirects are
  ignored and roughly 1,695 phantom failures appear with empty messages, surfacing as a Moq
  `TypeInitializationException` via `System.Threading.Tasks.Extensions`. A run missing the flag
  shows a fabricated mass regression that must not be "fixed".
- Exclude `\.claude\` from recursive `*.Test.dll` discovery so stale agent-worktree builds are not
  loaded.

### Manual validation steps

Watch one full-suite run and record whether a top-level window appears. A window that does appear is
evidence for the `ProgressViewer_Tests` re-attribution and belongs in the follow-up issue, not in
this feature's acceptance.

## Acceptance Criteria

- [x] `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`
      (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:175`) passes in
      every one of ten consecutive full nine-assembly runs, with the ten TRX results stored under
      `evidence/regression-testing/`.
- [x] `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`
      (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:131`) passes in
      every one of those same ten consecutive full nine-assembly runs.
- [ ] The ten consecutive full nine-assembly runs are executed under induced CPU load and are all
      green, using `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation
      /TestCaseFilter:"TestCategory!=LiveOutlook"`, with the evidence stored under
      `evidence/regression-testing/`. (Ten under induced load is chosen over #571's "at least 5"
      because it is the epic's stated leading indicator, it targets #511's load-induced cascade
      directly, and it satisfies #571's threshold a fortiori.)
- [x] An empirical pre-fix baseline artifact exists under `evidence/regression-testing/` recording,
      per run across ten runs, the pass/fail outcome of both named tests and the observed harness
      viewer `IsHandleCreated` value, establishing the pre-fix failure behaviour by execution rather
      than by static reading.
- [x] `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` exists in
      `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`, asserts the
      harness viewer's `IsHandleCreated` is `true` before the act, and passes.
- [ ] `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` exists in
      `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`, asserts both
      WebView2 children remain handle-less, and passes.
- [x] `git diff` reports zero hunks in both
      `QuickFiler/Controllers/QfcItemController.Initialization.cs` and
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, and file inspection confirms all
      seven `#230` de-exemption comment blocks in `Initialization.cs` (lines 135, 164, 196, 259,
      291, 403, 447), the `#230` de-exemption block at `ViewerSetup.cs:254`, and the retained
      `[ExcludeFromCodeCoverage]` block at `ViewerSetup.cs:30-41` are present and unmodified.
- [ ] All 21 pump-host call sites pass in the final run: the 13 self-tests in
      `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` and the 8 consumer tests (5 in
      `QfcItemController.InitializationTests.Part3.cs`, 2 in `QfcItemController.SeamFactoryTests.cs`,
      1 in `QfcItemController.ViewerSetupTests.cs`).
- [x] `git diff --name-only` against the merge base lists exactly three code files, all under
      `QuickFiler.Test/` (`Controllers/QfcItemController.InitializationTests.Part2.cs`,
      `Controllers/QfcItemController.ViewerSetupTests.cs`,
      `Controllers/QfcItemController.InitializationTests.Part3.cs`), and lists no file under
      `QuickFiler/`, no `*.csproj`, and no path under `.claude/` other than `.claude/agent-memory/`,
      which epic hard constraint 1 lists as safe to edit and which is agent bookkeeping rather than
      part of the fix.
- [x] `QfcItemController_SeamFactoryTests` and `QfcItemController_InitializationTests` both pass in
      the same run, and file inspection confirms `UiThreadDispatcherGate`
      (`QfcItemController.InitializationTests.Part2.cs:51`) and `SwapUiThreadDispatcher` (`:139`)
      retain their acquire-and-release structure.
- [x] Every changed file is under 500 lines after the change:
      `QfcItemController.InitializationTests.Part2.cs` (was 409),
      `QfcItemController.ViewerSetupTests.cs` (was 467), and
      `QfcItemController.InitializationTests.Part3.cs` (was 290).
- [x] `git diff` introduces no occurrence of `Thread.Sleep`, `Task.Delay`, `SpinWait`, a retry loop,
      or a raised timeout constant, and every existing timeout constant retains its current value
      (`PumpTimeoutMs = 60000`, `TimeoutMs = 30000`).
- [ ] The five-step toolchain in `## Test Strategy` completes green in a single final pass, coverage
      is captured under `evidence/qa-gates/`, and measured `QuickFiler` line coverage is greater
      than or equal to the pre-fix baseline recorded under `evidence/baseline/`.
- [ ] `## Rollout & Follow-up` records #511's visible-window half as out of scope with its
      re-attribution to `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`, and names the filed
      follow-up issue number for it, so the feature audit does not score that half as an unmet
      criterion of this feature.

## Risks & Mitigations

### Technical or operational risks

1. **The flipped `InvokeRequired` guards change behaviour on the four currently-passing pump-hosted
   tests.** After the handle exists, `Theme.cs:433` and `ViewerSetup.cs:361` marshal instead of
   running inline. This is the most likely source of an execution surprise.
   *Mitigation:* all 21 pump-host call sites are an explicit acceptance criterion, and the fix runs
   inside the section already serialized by `UiThreadDispatcherGate` against a live pump.
2. **The intermittency mechanism is unexplained.** If a third-party path is creating the handle
   non-deterministically, the fix is still correct but the "minimal" claim is weaker than stated.
   *Mitigation:* the empirical pre-fix baseline and
   `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` are both acceptance criteria; the spec
   records the question as open rather than resolving it by assertion.
3. **The `[Timeout]` / `UiThreadDispatcherGate` cascade is not fixed.** A load-induced overrun can
   still convert into several correlated failures, because MSTest records the timeout without
   aborting the continuation, so the gate release in `PumpHarness.Restore` has not yet run.
   *Mitigation:* recorded as a residual and as a follow-up candidate (`[DoNotParallelize]`), not
   claimed fixed. If the ten-run green requirement cannot be met, this is the first suspect.
4. **Residual CPU-contention sensitivity.** Retaining the pump-hosted coverage retains real message
   pumps under load. *Mitigation:* stated as an accepted trade, not silently claimed away. The
   alternative buys a pump-free suite at the cost of nine coverage justifications and a rewrite this
   child is neither scoped nor permitted to perform.
5. **Cap pressure on adjacent files.** `WinFormsPumpHost.cs` (18 lines of headroom) and
   `FocusAndThemeTests.cs` (3 lines of headroom) cannot absorb additions.
   *Mitigation:* the chosen remedy touches neither.
6. **A later reviewer may prefer the production guard on `InvokeBeginInvoke`.**
   *Mitigation:* the argument against including it here is recorded in `## Scope & Non-Goals`, and
   a follow-up issue is identified below rather than the change being folded in.

### Mitigations and rollbacks

Rollback is a revert of the three test files. No production surface is affected, so a revert cannot
regress runtime behaviour. There is no feature flag and none is warranted.

## Rollout & Follow-up

### Release/rollout steps

1. Land on `bug/winformspumphost-suite-determinism-511`, pull request into
   `epic/quickfiler-suite-determinism-foundation-integration`.
2. Confirm the wave transition from `git worktree list --porcelain`, `git branch`, and
   `gh pr view --json state,mergedAt,headRefOid`. Do not rely on any `PreToolUse` hook: every hook
   in this repository currently reads `$toolInput.command` while the payload nests the value at
   `$toolInput.tool_input.command`, so the epic wave barrier and merge gate are inert.
3. Attach the pre-fix baseline, the ten-run determinism record, and the coverage report from
   `evidence/` to the pull request context.

### Post-fix monitoring or clean-up tasks

- Watch the next several full-suite runs for any recurrence of the `InvalidOperationException` from
  `FocusAndTheme.cs:256`. A recurrence means the handle is being lost or the fixture is being
  bypassed.
- Watch for `[Timeout]`-attributed failures in `QfcItemController_InitializationTests` or
  `QfcItemController_SeamFactoryTests`, which indicate the unmitigated gate cascade rather than the
  handle race.

### Required follow-up issues

1. **#511's visible-window half — out of scope here, re-attributed, needs its own issue.** The
   evidence does not support attributing the visible window to `WinFormsPumpHost` or to anything in
   this feature's blast radius. The only enabled test in the nine-assembly corpus that shows a real
   top-level window is `viewer.Show()` at `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73`,
   on a `ProgressViewer : Form` (`UtilitiesCS/Threading/ProgressViewer.cs:16`) inside an
   `[STATestClass]`. A one-line remedy exists — the file already has a headless construction helper
   `CreateHeadlessViewer` at `ProgressViewer_Tests.cs:33-34` — but it is in `UtilitiesCS.Test`,
   outside this child's file set and outside this epic. **File this as its own issue and record the
   number here.** Note two constraints on how it is filed: the re-attribution is a code reading, not
   a reproduced observation, so someone should watch a full-suite run and confirm the window is the
   `ProgressViewer` before the issue asserts causation; and `epic.md` forbids any child of this epic
   from writing under `docs/features/potential/**`, so the follow-up is filed directly as a GitHub
   issue rather than by creating a potential entry.
2. **The `InvokeBeginInvoke` production asymmetry.**
   `QfcItemController.InvokeBeginInvoke` (`FocusAndTheme.cs:248`) is the only unguarded marshaller
   in the class; `Theme.cs:433` and `ViewerSetup.cs:361` establish the repository's
   `InvokeRequired`-guard pattern. Adding the guard is attractive on the merits but is a production
   behaviour change (on a handle-less control it would silently run UI mutation on the calling
   thread instead of throwing), it would make the pump-hosted `Initialize(bool)` test pass without
   exercising a real `Control.Invoke` — a coverage regression in substance — and its natural test
   home `FocusAndThemeTests.cs` has three lines of headroom. File as its own issue.
3. **The MSTest `[Timeout]` / `UiThreadDispatcherGate` cascade.** Candidate mitigation:
   `[DoNotParallelize]` on `QfcItemController_InitializationTests` and
   `QfcItemController_SeamFactoryTests`. It has no timing content, but its availability in this
   MSTest version and its interaction with the gate were not verified. File as its own issue if the
   ten-run determinism requirement exposes it.

### Links

- Primary issue #511: https://github.com/drmoisan/TaskMaster/issues/511
- Secondary issue #571: https://github.com/drmoisan/TaskMaster/issues/571
- Consolidated issue record: `docs/features/active/winformspumphost-suite-determinism-511/issue.md`
- Research artifact:
  `docs/features/active/winformspumphost-suite-determinism-511/research/winformspumphost-suite-determinism.2026-08-21T18-20.md`
- Epic: `docs/features/epics/quickfiler-suite-determinism-foundation/epic.md`
- Requirements sources:
  `docs/features/potential/promoted/2026-08-08-winformspumphost-tests-load-flaky-visible-window.md`,
  `docs/features/potential/promoted/2026-08-15-qfc-item-controller-init-tests-flaky-window-handle.md`
