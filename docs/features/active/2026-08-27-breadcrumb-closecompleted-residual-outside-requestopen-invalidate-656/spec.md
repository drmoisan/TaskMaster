# 2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate (Spec)

- **Issue:** #656
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T20-10
- **Status:** Draft
- **Version:** 0.1

## Context
`BreadcrumbDropDownOpenCoordinator._closeCompleted` stays `true` when the drop-down host is reopened by
a path that reaches neither `RequestOpen` nor `Invalidate`, so a subsequent close is wrongly suppressed.
This is the known residual of the SR-4 two-flag close fix shipped for #462 under #501, recorded against
the host paths owned by feature #488.

Environment:
- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` harness

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: it requires a reopen path that bypasses both entry points, which the currently exercised UI
flows do not take. It is a latent correctness gap rather than an observed user-facing failure.


## Repro & Evidence
Steps to Reproduce:
1. Open the breadcrumb drop-down host and close it through `CloseCore`, so `_closeCompleted` becomes `true`.
2. Reopen the host through a path that reaches neither `RequestOpen` nor `Invalidate`.
3. Request a close.

Expected:
The close request reaches `_host.Close`, because the host is genuinely open again.

Actual:
The coordinator still treats the host as already closed and suppresses the close. `_closeCompleted` was
never cleared, because it is cleared only on the `RequestOpen` and `Invalidate` paths.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: no runtime log; the residual is established by source inspection of the flag-clearing paths.


## Scope & Non-Goals

### Classification: latent-correctness hardening, not an observed failure

The reopen-path enumeration recorded in
`docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656/research/2026-08-31T20-15-closecompleted-residual-reopen-path-enumeration.md`
establishes that **no production reopen path bypassing both `RequestOpen` and `Invalidate` exists in the
tree today**. The trace, re-verified against the working tree for this spec:

1. `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:191` — `public bool IsOpen => OpenState;`.
2. `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:244` — `internal bool OpenState { get; set; }`.
3. The only production assignment of `OpenState = true` anywhere in `QuickFiler/` is
   `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:268`, inside `ShowCurrentSurface`. Every other
   production assignment sets it to `false`
   (`BreadcrumbDropDownHost.cs:334`, `:402`, `:434`, `:460`).
4. `ShowCurrentSurface` is reached only from `BreadcrumbDropDownOpenLifetime.OpenAsync`
   (`BreadcrumbDropDownOpenLifetime.cs:44`).
5. `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:88` —
   `return _openLifetime.OpenAsync(anchorScreenBounds, workingArea, desiredSize, takeFocus);` is the only
   production caller of that lifetime entry point.
6. The only production call sites of any `OpenAsync` on the host are
   `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:258-259`, inside `OpenCoreAsync`, reached
   only from `RequestOpen` at `:115` — and `RequestOpen` clears `_closeCompleted` at `:114` immediately
   before.
7. `BreadcrumbPopupUiOperations.ShowOwnedPopup` (`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:101`)
   is referenced in production only at `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:74`, where it is
   supplied as the show delegate to the same open path. It is not an independent reopen entry point.
8. `IBreadcrumbDropDownHost` (`QuickFiler/Viewers/IBreadcrumbDropDownHost.cs:19`) has exactly one
   production implementation, `BreadcrumbDropDownHost`
   (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:22`).

The defect is nonetheless real and reachable: `BreadcrumbDropDownOpenCoordinator` is written against the
`IBreadcrumbDropDownHost` seam (`BreadcrumbDropDownOpenCoordinator.cs:18`, `:53`), and any substituted
implementation may report `IsOpen == true` without the coordinator's `RequestOpen` having run. The
existing suite already drives exactly that state at
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:349`
(`harness.Host.SetOpen(true)`). The issue records severity Medium and latent, and this spec does not
raise that assessment. The work is correctness hardening of a seam contract, not a user-facing fix.

### In scope
- Narrowing the completed-close suppression in `CloseCore`
  (`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:308-342`) so a close is not suppressed while
  the host reports open.
- One new deterministic regression test covering the residual scenario.
- Updating the `_closeCompleted` XML documentation
  (`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:38-46`) and the `CloseCore` guard-order
  documentation (`:302-307`) to describe the new guard.

### Out of scope / non-goals
- Any change to `BreadcrumbDropDownHost`, `BreadcrumbDropDownHost.Open.cs`,
  `BreadcrumbDropDownOpenLifetime`, `BreadcrumbItemViewerLifecycleCoordinator`, or
  `ItemViewer.Breadcrumb.cs`. The issue text attributes the residual to feature #488's host paths; the
  enumeration above shows the residual is closable inside the coordinator alone, so no host-surface file
  is opened.
- Introducing a new production seam. `[assembly: InternalsVisibleTo("QuickFiler.Test")]` already exists
  at `QuickFiler/Properties/AssemblyInfo.cs:5`, and the test host already exposes the required bypass
  (`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:407`).
- Editing any of the four standing guards listed under **Test Strategy**.
- Revisiting the #462 two-flag design, the `_closeInFlight` semantics, or the `_generation` counter.
- Reopening SR-4 in `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md`. That record
  stays as written; this spec reconciles with it rather than amending it.

### Explicitly excluded systems, integrations, or datasets
- No Outlook interop, WebView2, WinForms, or native popup code is touched.
- No project, build, or package file: no `.csproj`, `.props`, `.targets`, or `packages.config` edit.
- No solution-wide analyzer, nullable, or coverage configuration change.

### Hard scope boundary (concurrency)
This item runs concurrently with other items in a parallel run. A wider footprint costs run concurrency.
The authorized footprint is:

| Kind | File | Note |
|---|---|---|
| Production | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | the only production file that may change |
| Test | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` | new test appended here |

`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` is **455 lines**, measured in
this worktree. A regression test of the shape described below is roughly 40 lines including its XML
documentation, which would place Part2 within a few lines of the 500-line file limit in
`.claude/rules/general-code-change.md`. `BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` already exists
at **173 lines**, is the same `public sealed partial class BreadcrumbDropDownOpenCoordinatorTests`
(`Part3.cs:21`), and shares the `CoordinatorHarness` and `ControlledHost` fixtures declared in the
primary partial. The new test therefore goes in **Part3**, and no new file is created.


## Root Cause Analysis
#462 was fixed by replacing the single `_closePending` flag with two flags, `_closeInFlight` and
`_closeCompleted`. `_closeCompleted` is cleared on `RequestOpen` and `Invalidate` only.

The two-flag form was chosen deliberately. The naive alternative, clearing the close flag on the
successful-close path, makes two existing must-pass tests fail by letting a second `CloseCore` reach
`_host.Close`: `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` and
`SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`. Both encode the repeated-close
suppression contract. The two-flag form passes all three must-pass tests with no test edit, so it was
shipped and this residual recorded rather than traded for a regression.

This belongs to feature #488, not #501: the reopen paths that bypass `RequestOpen` and `Invalidate`
live in the ItemViewer breadcrumb lifecycle host surface. #501 was not permitted to write
`BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbDropDownHost.cs` or `ItemViewer.Breadcrumb.cs`.


## Proposed Fix

### Design summary (what changes where):

`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`, method `CloseCore` (declared at `:308`), is
the only production change. Two edits:

1. Capture the host open state into a local **before** entering the critical section, i.e. before
   `lock (_sync)` at `:310`.
2. Narrow the completed-close suppression at `:316` from `if (_closeCompleted) return true;` to a form
   that suppresses only when the host is also not open.

The intended shape:

```csharp
private bool CloseCore(BreadcrumbDropDownCloseReason reason)
{
    // The host read is hoisted out of the critical section deliberately: see SR-4 reconciliation.
    bool hostOpen = _host.IsOpen;
    lock (_sync)
    {
        if (_released)
            return false;
        if (_closeInFlight)
            return true;
        if (_closeCompleted && !hostOpen)
            return true;
        _closeInFlight = true;
    }
    // ... unchanged from :320 onward
}
```

No other statement in the method changes. The guard order (released, in-flight, completed) is preserved,
as is the `finally` that clears `_closeInFlight` (`:325-329`) and the success block that increments
`_generation` and sets `_closeCompleted` (`:330-338`).

### SR-4 reconciliation (load-bearing)

`docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:426-431` records:

> **SR-4 — DECIDED: minimal two-flag form (research §6.1 option D), without the `&& !_host.IsOpen`
> refinement.**
> *Rationale:* the refinement `if (_closeCompleted && !_host.IsOpen) return true;` would read
> `_host.IsOpen` under `_sync` — the very lock-ordering hazard that #462's potential document flags
> and that #500 exists to remove. Adding it here would create a new instance of the class of defect
> this feature is closing.

`:1062` records the residual as "shipped as designed".

A precise reading matters here. `RequestOpen` **already** reads `_host.IsOpen` under `_sync`, at
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:112`
(`if (_closeInFlight && _host.IsOpen) return ClosedTask;`). SR-4's rationale is therefore not the claim
that no such read exists in the class. It is the narrower and still-correct claim that a feature whose
purpose is to remove instances of a hazard class must not **add a new instance** of that hazard class.
That distinction is the whole of the reconciliation:

- SR-4 declined a specific code shape — an `_host.IsOpen` read placed *inside* `_sync` — on the ground
  that it enlarges the set of foreign calls made while the coordinator's lock is held. `IsOpen` is an
  interface member (`QuickFiler/Viewers/IBreadcrumbDropDownHost.cs:22`); the coordinator holds a
  `IBreadcrumbDropDownHost` (`BreadcrumbDropDownOpenCoordinator.cs:18`), not the concrete class, so any
  substituted implementation could take its own lock or re-enter the coordinator from inside `_sync`.
- The remedy in this spec does **not** place the read inside `_sync`. The read happens before the lock is
  acquired and only a `bool` local crosses into the critical section. The count of foreign calls made
  while `_sync` is held is unchanged by this fix.
- SR-4 is therefore neither wrong nor overridden. Its stated objection does not apply to the hoisted
  form, which was not among the shapes SR-4 evaluated.

The coordinator already reads host state outside `_sync` on this same posted-work path:
`BreadcrumbDropDownOpenCoordinator.cs:193` reads `_host.IsOpen` inside the `Reset()` continuation with no
lock held. The hoisted read is consistent with that existing pattern.

### Boundaries and invariants to preserve:

- **I-462.1** — `_closeInFlight` is true only while `_host.Close(reason)` executes and is cleared in a
  `finally` (`:325-329`). Unchanged.
- **I-462.3** — a repeated close of an already-closed host is suppressed. Preserved: when the host is not
  open, `hostOpen` is `false`, the added conjunct is `true`, and the guard behaves exactly as on HEAD.
- **Lock discipline** — no new call to any `IBreadcrumbDropDownHost` member is made while `_sync` is
  held. This is the invariant SR-4 protects.
- **Guard order** — released, then in-flight, then completed. Unchanged.
- **Generation semantics** — `_generation` is incremented by a successful close (`:334`) and by
  `Invalidate` (`:350`), never by `RequestOpen`. Unchanged; the fix does not consult `_generation`.
- **Closing while the host reports not open remains permitted.** The coordinator must still be able to
  reach `_host.Close` when `_host.IsOpen == false`; see
  `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` under **Test Strategy**. The fix
  preserves this because `!hostOpen` is a *conjunct added to an existing suppression*, never a
  suppression on its own.

### Dependencies or blocked work:

- None. The change is self-contained in one production file plus one test file.
- Environment bootstrap only (see **Assumptions, Constraints, Dependencies**).
- Does not depend on, and must not wait for, feature #488 or #501.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` — the sole production file.
- `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` — one added test method.

#### Functions/classes/CLI commands impacted:
- `BreadcrumbDropDownOpenCoordinator.CloseCore(BreadcrumbDropDownCloseReason)` — the two edits above.
- The `_closeCompleted` field XML documentation (`:38-46`) and the `CloseCore` summary (`:302-307`) — text
  updated to state that suppression now additionally requires the host to report not open, and to record
  why the host read is hoisted.
- No public API, no CLI command, and no interface member changes. `CloseCore` is `private`.

#### Data flow and validation changes:
- One additional read of `_host.IsOpen` per `CloseCore` invocation, taken before the lock. No writes.
- The read is unconditional, so it also occurs when the coordinator is already released. This is accepted:
  `IsOpen` is a side-effect-free state read on the sole production implementation
  (`BreadcrumbDropDownHost.cs:191` delegating to the auto-property at `:244`), it does not throw after
  disposal, and no test in `QuickFiler.Test` uses a strict `Mock<IBreadcrumbDropDownHost>` or counts
  `IsOpen` reads. The alternative — an early `IsReleased()` check before the read — would add a second
  lock acquisition to every close for no behavioral gain, and is rejected on simplicity grounds.

#### Error handling and logging updates:
- None. The coordinator has no logger, and the change introduces no new failure mode. Existing exception
  routing through `BreadcrumbUiDispatcher` is untouched.

#### Rollback/feature-flag considerations (if applicable):
- No feature flag. The change is a two-line edit to one private method; rollback is a revert of the
  single production file.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- `CloseCore(BreadcrumbDropDownCloseReason reason) -> bool`. Signature unchanged.
- Return-value contract, restated with the fix applied:
  - `false` when the coordinator is released.
  - `true` without calling `_host.Close` when a close is in flight.
  - `true` without calling `_host.Close` when a close has already completed **and** the host reports not
    open.
  - Otherwise `_host.Close(reason)` is called; `true` if the host accepted it, in which case
    `_generation` is incremented and `_closeCompleted` is set; `false` if the host rejected it, in which
    case an `Uncommitted` close with the selector still open cancels the selector (`:339-340`).

#### Required configuration keys and defaults:
- None.

#### Backward-compatibility expectations:
- The observable behavior changes in exactly one state: `_closeCompleted == true` **and**
  `_host.IsOpen == true`. On HEAD that state suppresses the close; after the fix it reaches
  `_host.Close`. No production path can currently produce that state (see **Scope & Non-Goals**), so no
  shipped behavior changes.
- Every other state is bit-identical to HEAD, because `!hostOpen` is `true` whenever the host is not
  open and the guard then evaluates exactly as before.

#### Performance constraints (latency/throughput/memory):
- One additional property read per close request. No allocation, no additional lock acquisition, no I/O.
  No measurable latency or memory impact; no performance budget applies.

### Race analysis for the hoisted read

The host state can change between the unlocked read and the lock acquisition. Both directions must be
stated rather than assumed benign.

**Direction 1 — read `true`, host closes before the lock is taken.** The added conjunct evaluates
`!hostOpen == false`, so the completed-close suppression does not fire and `_host.Close(reason)` is
invoked on a host that is now closed. This is a defined, safe call: `BreadcrumbDropDownHost.Close`
(`BreadcrumbDropDownHost.cs:247-257`) returns `false` when `_disposed`, and when `OpenState` is `false`
it returns `_openLifetime.TryCancelPendingOpen(...)` rather than performing a close. `closed` is then
`false`, `_closeCompleted` is left unchanged, and the `Uncommitted` fallback at `:339-340` may cancel a
still-open selector. That fallback is the coordinator's existing and correct response to a host that
declined a close, so the outcome is a redundant call with a correct result, not a corrupted state.

**Direction 2 — read `false`, host opens before the lock is taken.** The conjunct evaluates `true`, the
close is suppressed, and the residual persists for that interleaving. This is exactly HEAD's behavior,
so it is a narrowed residual rather than a regression.

**Why the window is not reachable in production.** Every production invocation of `CloseCore` runs on the
`BreadcrumbPopupUiOperations` queue: `SetDroppedDown` calls it inside `_operations.PostAsync`
(`:167`), `HandleSelectorOpenStateChanged` calls it inside `_operations.PostAsync` (`:182`), and
`FinishOpenCore` — the only other caller (`:277`) — is itself invoked inside `_operations.RunAsync`
(`:223`). The production mutations of `OpenState` occur in WinForms event handling
(`BreadcrumbDropDownHost.cs:426-437`) and in work scheduled through the host's own UI operations
(`BreadcrumbDropDownOpenLifetime.cs:268`, `BreadcrumbDropDownHost.cs:397-411`). The read and the
mutations are therefore serialized on the UI thread in production, and the interleaving above requires a
host implementation that mutates open state from another thread.

**Conclusion.** The race is tolerable in both directions: one direction produces a redundant but
correctly-handled call, the other reproduces current behavior. This is a strictly better position than
HEAD, which suppresses unconditionally. The alternative that removes the race entirely — reading
`_host.IsOpen` inside `_sync` — is the shape SR-4 declined and is not adopted.

### Option ranking

| Option | Verdict | Reason |
|---|---|---|
| **Hoisted host read + `if (_closeCompleted && !hostOpen)`** | **Recommended** | Closes the residual, edits no test, adds no foreign call under `_sync`, and does not add a new instance of the hazard class SR-4 declined. Its only cost is the bounded race analyzed above. |
| SR-4 refinement: `if (_closeCompleted && !_host.IsOpen)` inside `_sync` | Rejected | Removes the residual and passes every existing test, but places an `IBreadcrumbDropDownHost` call inside the critical section. Declined by SR-4 (`docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:426-431`) on lock-hazard grounds; that ground still applies. |
| Option A — clear `_closeCompleted` on the successful-close path | Rejected | Lets a second `CloseCore` reach `_host.Close`, breaking `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`, `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`, and `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce` (whose XML documentation at `Part2.cs:366-373` names it as the standing guard against exactly this option). A remedy requiring a test edit is a regression trade and is out of scope. |
| Option B — gate `CloseCore` on `!_host.IsOpen` alone | Rejected | `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` (`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:301-318`) proves that closing while `_host.IsOpen == false` is required behavior, so a bare `!_host.IsOpen` gate suppresses a required close. It also re-introduces the under-`_sync` read unless hoisted. |
| Option C — track `_closedAtGeneration` | Rejected | `_generation` is incremented by the successful close itself (`:334`) and by `Invalidate` (`:350`), and never by `RequestOpen`. A generation stamp would suppress the close of a genuinely new open unless additional reset state were added, which is a larger change than the residual warrants. |
| Option D — two flags with distinct meanings | Shipped, insufficient | This is HEAD (`:36`, `:46`). It is correct for every reachable production state and is the source of the residual. Retained; the fix narrows its suppression rather than replacing it. |
| Route the bypassing reopen paths through `RequestOpen` or `Invalidate` (the issue's first two proposed-fix bullets) | Not applicable | The enumeration in **Scope & Non-Goals** shows there is no such production path to route. Acting on those bullets would require editing host-surface files that this spec places out of scope, with no defect to fix at those sites. |


## Assumptions, Constraints, Dependencies

### Assumptions (environment, data, access):
- The residual is closable inside `BreadcrumbDropDownOpenCoordinator` alone. Supported by the reopen-path
  enumeration above; the issue's attribution to feature #488's host paths is superseded by that finding
  and the reason is recorded here rather than by editing `issue.md`.
- `IBreadcrumbDropDownHost.IsOpen` is a side-effect-free state read for every implementation the
  coordinator will be given. True of the sole production implementation
  (`BreadcrumbDropDownHost.cs:191`, `:244`) and of every test double in `QuickFiler.Test`.
- The existing `ControlledHost` fixture is sufficient for the regression test; no new production seam is
  required. `[assembly: InternalsVisibleTo("QuickFiler.Test")]` exists at
  `QuickFiler/Properties/AssemblyInfo.cs:5`.

### Constraints (budget, performance, compatibility):
- Target framework .NET Framework 4.8; C# language features must remain compatible with the existing
  project settings. No `init` accessors and no `record` types (no `IsExternalInit` on net48).
- File-size limit of 500 lines per `.claude/rules/general-code-change.md`. After the change,
  `BreadcrumbDropDownOpenCoordinator.cs` (378 lines on HEAD) and
  `BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` (173 lines on HEAD) must both remain under 500.
- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:1` carries `#nullable enable`, so the file
  participates in nullable analysis and `CS86xx` diagnostics are promoted to errors by the type-check
  gate. The added local is a non-nullable `bool` and introduces no null state.
- Tests must be MSTest with FluentAssertions per `CLAUDE.md` (Moq is available but not needed here; the
  hand-written `ControlledHost` fixture is used).
- Deterministic tests only: no timers, no `Thread.Sleep`/`Task.Delay`, no second thread, no temporary
  files, no Outlook or WebView2.
- Parallel-run concurrency constraint: the footprint table in **Scope & Non-Goals** is a hard boundary.

### External dependencies (services, libraries, releases):
- No new NuGet package, no package version change, no `packages.config` edit.

### Environment preconditions (bootstrap required before any gate can run)
This worktree contains **no `.dotnet-sdk` directory and no `packages/` directory**, verified by a glob of
both paths returning no files. Both must be bootstrapped before any `msbuild` or test command can run:

1. `scripts/vscode/Install-RepoDotNetSdk.ps1` — provisions the repo-local SDK.
2. A NuGet restore — `scripts/vscode/Invoke-Restore.ps1` populates `packages/`.

Running `msbuild` before these complete fails for environment reasons and must not be recorded as a gate
failure.


## Data / API / Config Impact
- **User-facing or API changes:** none. `CloseCore` is `private`; `IBreadcrumbDropDownHost` and every
  `internal` member of `BreadcrumbDropDownOpenCoordinator` keep their current signatures. No user-visible
  behavior changes on any currently reachable production path.
- **Data or migration considerations:** none. The coordinator holds no persisted state; the change adds a
  method-local `bool`.
- **Logging/telemetry updates (if any):** none. The class has no logger and the change adds no diagnostic
  surface.
- **Compatibility notes (CLI flags, config schemas, versioning):** none. No CLI flag, no configuration
  key, no schema, and no assembly version change. No `.csproj`, `.props`, `.targets`, or
  `packages.config` edit.


## Test Strategy

This is a C# item. The framework is **MSTest**, assertions use **FluentAssertions**, and **Moq** is
available for mocking, per `CLAUDE.md`. The template's "pytest" wording does not apply and is replaced
here.

### Standing guards that must remain unedited

Four tests encode the repeated-close suppression contract that the #462 two-flag design was chosen to
satisfy. Any remedy requiring an edit to any of them is a regression trade and is rejected; the rejection
reason for each is recorded in the option table above.

1. `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:263` —
   `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`. Two consecutive
   `SetDroppedDown(false)` drives; asserts
   `harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.Uncommitted)` — a single close
   reason (`:278`). The host open is still pending at that point (`pending.SetResult(false)` at `:274`),
   so `IsOpen` is `false` during both drives.
   *Under the recommended remedy:* `hostOpen` is `false` on the second drive, the conjunct is `true`, and
   the second close stays suppressed. Unchanged.
2. `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:121` —
   `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`. Asserts
   `harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.ExplicitCommit)` (`:139`)
   after two `HandleSelectorOpenStateChanged` drives following a successful open then close.
   *Under the recommended remedy:* the host accepted the first close and `ControlledHost.Close` sets
   `IsOpen = false` (`BreadcrumbDropDownOpenCoordinatorTests.cs:436-437`), so `hostOpen` is `false` on
   the second drive and it stays suppressed. Unchanged.
3. `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:333` —
   `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync`. Asserts
   `harness.Host.Requests.Should().HaveCount(2)` (`:358-360`). It reopens the host via
   `harness.Host.SetOpen(true)` (`:349`) — the same bypass seam the new regression test uses.
   *Under the recommended remedy:* this test exercises `RequestOpen`, not `CloseCore`, and is unaffected.
4. `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:375` —
   `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce`. Its XML documentation
   (`Part2.cs:366-373`) states it is "the standing guard that rules out research section 6.1 option A
   (clearing the flag on the successful-close path)". Asserts a single `Uncommitted` close reason
   (`:391-396`).
   *Under the recommended remedy:* no reopen occurs, the host is closed, `hostOpen` is `false`, and the
   close reaches the host exactly once. Unchanged.

Additionally, `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:302` —
`PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` — proves that closing while
`_host.IsOpen == false` is *required* behavior, which is why a bare `!_host.IsOpen` gate (option B) is
wrong. Under the recommended remedy `_closeCompleted` is `false` on that first close, so the added
conjunct cannot suppress it. This test must not regress and must not be edited.

### Regression test to add

**File:** `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` (append to the
existing `public sealed partial class BreadcrumbDropDownOpenCoordinatorTests`; no `[TestClass]` attribute
is repeated, per the note at `Part3.cs:11-14`).

**Name:** `CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain`

**Scenario (Arrange–Act–Assert):**
- *Arrange:* construct `CoordinatorHarness`; `harness.Host.Enqueue(Task.FromResult(true))`; drive
  `RequestOpen()` and `DrainUntil` to completion; drive `SetDroppedDown(false)` and `DrainAll` so the host
  accepts a close and `_closeCompleted` becomes `true`; assert `harness.Host.IsOpen` is `false`.
- *Act:* `harness.Host.SetOpen(true)` — the reopen that bypasses both `RequestOpen` and `Invalidate`
  (`BreadcrumbDropDownOpenCoordinatorTests.cs:407`); set `harness.SelectorOpen = true`; drive a second
  `SetDroppedDown(false)` and `DrainAll`.
- *Assert:* `harness.Host.CloseReasons.Should().Equal(new[] { BreadcrumbDropDownCloseReason.Uncommitted, BreadcrumbDropDownCloseReason.Uncommitted })`
  — the close after a bypassing reopen must reach `_host.Close` a second time.
- *Determinism:* single thread, explicit drain of the capturing synchronization context, no timers, no
  sleeps, no temporary files.

**Red-to-green requirement:** the test must be demonstrated failing on HEAD (`CloseReasons` holds one
element, because `_closeCompleted` suppresses the second close) and passing after the fix. A test that is
green before the production change does not verify the fix and does not satisfy the acceptance criteria.

### Edge cases and negative scenarios
- Repeated close with no reopen — covered by standing guard 4; must stay suppressed.
- Close while the host reports not open and `_closeCompleted` is `false` — covered by
  `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen`; must still reach `_host.Close`.
- Close after `Release()` — covered by
  `SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched`
  (`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:192`); the released guard
  must still return before any host state changes.
- Host rejects the close (`CloseResult = false`) — covered by
  `PendingToggleClose_RejectedHostPerformsOneFallbackCancellation`
  (`BreadcrumbDropDownOpenCoordinatorTests.cs:283`) and
  `ResetReleaseAndCloseResults_PreserveRetryAndBlockReleasedWork`
  (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:143`); the selector-cancel fallback must be unchanged.
- Integration-level single-close assertions — `SetFolderDroppedDownFalse_RequestsOneUncommittedCloseAndRollback`
  (`QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:89`) and
  `InitializationFailure_CancelsSessionWithoutDuplicateClose` (`:264`) both use a host mock whose `Close`
  sets `_hostOpen = false` on success (`:353-360`), so `hostOpen` is `false` at any repeated close and
  their `Times.Once()` verifications are unaffected.

### Error handling and logging verification
- Not applicable. The change adds no exception path and no log statement. The existing
  `BreadcrumbUiDispatcher` error-sink behavior is exercised by
  `RequestOpen_RollbackOperationThrows_CompletesFalseWithoutSurfacingSecondary`
  (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:302`) and must not regress.

### Coverage impact and targets for changed lines/modules
- The changed lines are the hoisted read and the widened guard in `CloseCore`. Both are executed by the
  new regression test and by every existing close test, so changed-line coverage is 100%.
- Both outcomes of the new conjunct are covered: `!hostOpen == true` (suppression retained) by standing
  guards 1, 2 and 4; `!hostOpen == false` (suppression released) by the new regression test.
- Coverage for `QuickFiler` must not decrease relative to the pre-change run.

### Toolchain commands to run (format -> lint/analyze -> type-check -> test)

Run in this exact order; if any step fails or modifies a file, fix and restart from step 1.

1. **Format:** `dotnet tool run csharpier format .` — verify with `dotnet tool run csharpier check .`.
   Always invoke through `dotnet tool run` so the manifest-pinned version is used.
2. **Analyze:**
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type-check:**
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. **Test with coverage:** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which supplies `/InIsolation`
   and `/TestCaseFilter:TestCategory!=LiveOutlook`.

Mandatory command-shape rules:
- **Do not add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and
  there is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts files
  that never adopted the pragma. It can never pass and CI omits it deliberately.
- **Use `/t:Rebuild`, never `/t:Build`.** MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
  project and the gate cannot fail.
- **Do not call `vstest.console.exe` directly.** A bare invocation omits the
  `TestCategory!=LiveOutlook` filter and would launch a real Outlook process. Use the wrapper script.
- Complete the environment bootstrap in **Assumptions, Constraints, Dependencies** before step 2.

### Manual validation steps (if required)
None. The residual is not reachable through the shipped UI (see **Scope & Non-Goals**), so there is no
manual gesture that exercises it. Verification is entirely by automated test and source inspection.


## Acceptance Criteria

- [x] AC-1 — `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` reads `_host.IsOpen` into a local
      declared **before** the `lock (_sync)` that opens `CloseCore`, and the completed-close guard inside
      that lock is `if (_closeCompleted && !<that local>) return true;`. Checkable by reading the changed
      lines of `CloseCore` in the diff.
- [x] AC-2 — SR-4 reconciliation: no statement added or modified inside any `lock (_sync)` block of
      `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` invokes a member of
      `_host`/`IBreadcrumbDropDownHost`. Checkable by reading every `lock (_sync)` body in the changed
      file and confirming the only pre-existing such call remains the one at `RequestOpen`
      (`if (_closeInFlight && _host.IsOpen) return ClosedTask;`), with no new one added.
- [x] AC-3 — A new test named `CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain` exists
      in `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`, drives a successful
      open, a successful close, `harness.Host.SetOpen(true)`, and a second close, and asserts that
      `harness.Host.CloseReasons` equals two `BreadcrumbDropDownCloseReason.Uncommitted` entries.
      Checkable by reading the test body.
- [x] AC-4 — `CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain` is demonstrated **failing
      before** the production edit and **passing after** it, with both run outputs recorded in the
      feature evidence folder under `evidence/qa-gates/`. Checkable by comparing the two recorded
      `Invoke-MSTestWithCoverage.ps1` outputs for that test name.
- [x] AC-5 — `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`
      (`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:263`) passes and its file is
      unchanged in the diff. Checkable by the test run output and by `git diff --stat` for that file.
- [x] AC-6 — `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`
      (`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:121`) passes and its
      assertion text is unchanged. Checkable by the test run output and by `git diff` for that file
      showing no change to the test.
- [x] AC-7 — `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync`
      (`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:333`) passes and its
      assertion text is unchanged. Checkable by the test run output and by `git diff` for that file.
- [x] AC-8 — `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce`
      (`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:375`) passes and its
      assertion text is unchanged. Checkable by the test run output and by `git diff` for that file.
- [x] AC-9 — `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen`
      (`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:302`) passes, confirming that a
      close while the host reports not open still reaches `_host.Close`. Checkable by the test run
      output.
- [x] AC-10 — Production footprint: `git diff --name-only <merge-base>...HEAD` lists no file under
      `QuickFiler/` other than `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`. Checkable by
      running that command and reading its output.
- [x] AC-11 — Build-configuration footprint: the same `git diff --name-only` output contains no path
      matching `*.csproj`, `*.props`, `*.targets`, or `packages.config`. Checkable by running that
      command and reading its output.
- [x] AC-12 — Test footprint: the same `git diff --name-only` output lists no file under
      `QuickFiler.Test/` other than
      `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`. Checkable by running
      that command and reading its output.
- [x] AC-13 — File-size limit: `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` and
      `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` each contain fewer than
      500 lines after the change. Checkable by a line count of each file.
- [x] AC-14 — Format gate: `dotnet tool run csharpier check .` exits 0 and reports no file requiring
      formatting. Checkable by the command's exit code and output.
- [x] AC-15 — Analyzer gate:
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      completes with `0 Error(s)` and introduces no new warning attributed to
      `BreadcrumbDropDownOpenCoordinator.cs`. Checkable by the msbuild summary and a warning grep of the
      log for that file name.
- [x] AC-16 — Analyzer-gate non-vacuity: the analyzer-gate log contains no
      `Skipping target "CoreCompile"` line for `QuickFiler` or `QuickFiler.Test`, proving the changed
      files were actually compiled. Checkable by grepping the captured msbuild log.
- [x] AC-17 — Type-check gate:
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      completes with `0 Error(s)`. The command must not include `/p:Nullable=enable` and must use
      `/t:Rebuild`. Checkable by the msbuild summary and by the recorded command text.
- [x] AC-18 — Test gate: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` completes with zero failed tests
      for `QuickFiler.Test`, and its recorded invocation shows `/InIsolation` and
      `/TestCaseFilter:TestCategory!=LiveOutlook` in effect. Checkable by the run summary and the
      recorded command line.
- [x] AC-19 — The `_closeCompleted` field documentation
      (`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:38-46`) and the `CloseCore` summary
      (`:302-307`) state that completed-close suppression now additionally requires the host to report
      not open, and record why the host read is taken outside `_sync`. Checkable by reading those comment
      blocks in the diff.
- [x] AC-20 — No new production seam: the diff adds no new `internal` or `public` member to
      `BreadcrumbDropDownOpenCoordinator` and no member to `IBreadcrumbDropDownHost`. Checkable by
      reading the diff of `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` and confirming
      `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` is absent from the changed-file list.


## Risks & Mitigations

### Technical or operational risks
- **R-1 — The hoisted read observes stale host state.** Analyzed in **Race analysis for the hoisted
  read**. One direction yields a redundant `_host.Close` on an already-closed host, which
  `BreadcrumbDropDownHost.Close` (`:247-257`) handles by returning `false` without closing; the other
  direction reproduces HEAD behavior. *Mitigation:* the analysis is recorded in the spec and in the code
  comment required by AC-19, so a future reader does not re-derive it; production invocations are
  serialized on the UI operations queue, which closes the window in practice.
- **R-2 — An unnoticed test asserts a single close in a state where the host still reports open.** Such a
  test would fail after the change. *Mitigation:* the close-count assertions across `QuickFiler.Test`
  were inspected; every fake and mock host clears its open state when `Close` succeeds
  (`BreadcrumbDropDownOpenCoordinatorTests.cs:436-437`,
  `BreadcrumbDropDownIntegrationTests.cs:353-360`,
  `BreadcrumbSubfolderActivationTests.cs:322-329`,
  `BreadcrumbSelectorOpenRetryTests.cs:345-349`), and a loose `Mock<IBreadcrumbDropDownHost>` returns
  `false` for `IsOpen` by default. The full-suite run required by AC-18 is the authoritative check.
- **R-3 — The unconditional read touches the host after `Release()`.** *Mitigation:* `IsOpen` is a
  side-effect-free auto-property read on the sole production implementation
  (`BreadcrumbDropDownHost.cs:191`, `:244`) and does not throw after disposal; no test uses a strict host
  mock or counts `IsOpen` reads. `SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched`
  (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:192`) covers the released path and is part of the
  AC-18 run.
- **R-4 — Scope creep into host-surface files.** The issue text directs the fix at feature #488's host
  paths, which would enlarge the footprint and reduce parallel-run concurrency. *Mitigation:* the
  enumeration in **Scope & Non-Goals** removes the justification for touching those files, and AC-10
  through AC-12 pin the footprint mechanically.
- **R-5 — The regression test is authored green.** A test that passes before the production edit proves
  nothing. *Mitigation:* AC-4 requires recorded before-and-after runs.
- **R-6 — Environment bootstrap is mistaken for a gate failure.** The worktree has no `.dotnet-sdk` and
  no `packages/`. *Mitigation:* the bootstrap steps are recorded as a precondition in **Assumptions,
  Constraints, Dependencies**.

### Mitigations and rollbacks
- Rollback is a revert of `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` and removal of the
  added test method. There is no migration, no persisted state, and no feature flag to unwind.
- Because no production path can currently reach the changed state, a rollback restores behavior that is
  observationally identical to the fixed build on every shipped path.


## Rollout & Follow-up

### Release/rollout steps
1. Complete the environment bootstrap (`scripts/vscode/Install-RepoDotNetSdk.ps1`, then a NuGet restore).
2. Add the regression test and record its failing run under the feature `evidence/qa-gates/` folder.
3. Apply the two-line `CloseCore` edit and the documentation update.
4. Run the four-step toolchain in order until it passes in a single pass; record each gate output under
   the feature `evidence/qa-gates/` folder.
5. Check off the acceptance criteria in this file as each is verified.
6. Open the pull request. No staged rollout, no feature flag, and no runtime configuration change is
   required.

### Post-fix monitoring or clean-up tasks
- No telemetry to monitor: the changed state is unreachable from shipped UI, so there is no production
  signal to watch.
- Optional follow-up, not required by this issue: `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md`
  records the SR-4 residual as "shipped as designed" at `:1062` and as a known limitation at `:432-437`.
  Once this fix merges, that record becomes historical. Amending it is out of scope here; if the project
  wants the #501 spec annotated, promote a separate documentation item rather than widening this
  footprint.
- If a future host implementation is introduced that mutates open state off the UI thread, revisit
  **R-1**; the race window analyzed here becomes reachable in that configuration.

### Links
- Issue: https://github.com/drmoisan/TaskMaster/issues/656
- Research artifact:
  `docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656/research/2026-08-31T20-15-closecompleted-residual-reopen-path-enumeration.md`
- Origin: split out of #501 / #462; see `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md`,
  SR-4 (`:426-437`) and the implementation note at `:1062`.
- Prior option space: `docs/features/active/breadcrumb-coordinator-hub-defects-501/research/2026-08-24T09-12-breadcrumb-ordering-invariants-research.md`,
  section 6.1.
- Prior evidence: `docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/qa-gates/closepending-split.2026-08-27T20-53.md`
- PRs: to be added on submission.
