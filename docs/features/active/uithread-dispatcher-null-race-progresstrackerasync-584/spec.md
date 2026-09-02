# uithread-dispatcher-null-race-progresstrackerasync (Spec)

- **Issue:** #584
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T09-02
- **Status:** Draft
- **Version:** 0.2

## Context

- `UtilitiesCS.Threading.UiThread.Dispatcher` is a static property backed by a `null!`-initialised
  field with no lazy initialisation and no guard. `ProgressTrackerAsync.InitializeAsync()`
  dereferences the property's return value on the very next statement with no null check. When the
  static is read before `UiThread.Initialize()` has completed, the property silently returns `null`
  and the consumer throws an unattributed `NullReferenceException`.
- Observed environment(s): full-suite MSTest run under
  `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]` in `UtilitiesCS.Test`.
  Observed once during the post-change QC run for issue #449; did not reproduce in isolation or in
  two subsequent clean full-suite runs (0 of 1 isolated, 1 of 3 full-suite, per the issue body).
- Customer impact and severity: no end-user impact has been assessed; the observed failure is in the
  test suite, but the defect (unguarded static null return) is in production code
  (`UtilitiesCS/Threading/UiThread.cs`, and UtilitiesCS/Threading/ProgressTrackerAsync.cs, which was
  verified to need no code change), not test
  scaffolding. Whether a real Outlook/VSTO startup path can read `Dispatcher` before `Init()`
  completes has not been determined by this research; the fix removes the hazard regardless of
  whether that path is reachable in production, per the issue's own item 2/3 recommendation.
  Test-suite impact: a non-deterministic failure erodes trust in the full-suite gate (Core
  Principles 3 Fast Execution / 4 Determinism in the rule file .claude/rules/general-unit-test.md),
  because it cannot be distinguished from a real
  regression without rerunning.
- First observed date and version(s) impacted: observed 2026-08-22 (per the potential document
  filename date) during QC for issue #449; present on origin/main at
  `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` (verified 2026-09-02).

## Repro & Evidence

- Steps to reproduce (with data/flags/inputs): no reliable timing-based repro exists (1 of 3
  full-suite runs; 0 of 1 isolated runs, per the issue body). A deterministic structural repro is
  used instead (see Test Strategy): force `UiThread`'s private `_dispatcher` backing field to `null`
  via reflection (mirroring the existing pattern in
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`'s `ForceDispatcherNull`/`RestoreDispatcher`
  helpers) and assert on `UiThread.Dispatcher`'s accessor contract directly.
- Expected vs actual behavior: expected — a clear, explicit exception naming the missing
  `Initialize()` call. Actual (pre-fix) — the accessor returns `null` silently; the first
  dereference downstream (`ProgressTrackerAsync.InitializeAsync()`, or any of ~40 other call sites
  across `UtilitiesCS`, `QuickFiler`, and `TaskMaster` that read `UiThread.Dispatcher` without a
  guard) throws an unattributed `NullReferenceException`.
- Logs/screenshots/error snippets: `NullReferenceException` at
  UtilitiesCS/Threading/ProgressTrackerAsync.cs, line 35, on an STA thread, 793 ms into the failing run
  versus 191 ms in isolation (per the issue body).
- Frequency / determinism: intermittent, timing-dependent on production code paths (not to be
  stabilised with a sleep/retry/timing tolerance per the assignment's binding scope constraint and
  the rule file .claude/rules/general-unit-test.md).

## Scope & Non-Goals

- In scope:
  - `UtilitiesCS/Threading/UiThread.cs` — the `Dispatcher` accessor's null contract.
  - A new deterministic regression test in `UtilitiesCS.Test/Threading/UiThread_Tests.cs`.
- Out of scope / non-goals:
  - UtilitiesCS/Threading/ProgressTrackerAsync.cs — verified not to require a change (see Root
    Cause Analysis). Named as a fix site to verify in the assignment, not assumed to need an edit.
  - The injectable-seam conversion replacing ~62 remaining direct reads of `UiThread.Dispatcher`
    across ~29 production files with the existing `IUiDispatcher` seam. Already identified and
    explicitly deferred on the issue's own comment thread as a multi-phase, multi-assembly refactor
    with no bounded blast radius.
  - Adding synchronization around `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`'s
    existing reflection-based, unsynchronized mutation of the shared static
    `UiThread._dispatcher` (a `#493`-shaped test-isolation concern, but in a different test
    assembly than `#493` covered). Recorded as a candidate follow-up in Rollout & Follow-up below.
- Explicitly excluded systems, integrations, or datasets: the Claude runtime tree at .claude/**, the
  Codex mirror tree at .codex/**, the dot-agents tree at .agents/**, and the two published files
  config/blast-radius.json and config/orchestration-routing.json (published from upstream
  drm-copilot with zero templating; edits here are silently overwritten).

## Write Set

Every file this plan's diff creates, modifies, or deletes:

- `UtilitiesCS/Threading/UiThread.cs`
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs`
- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
- `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`

## Root Cause Analysis

- Confirmed root cause: `UtilitiesCS/Threading/UiThread.cs:135-140` declares
  `public static Dispatcher Dispatcher { get => _dispatcher; private set => _dispatcher = value; }`
  backed by `private static Dispatcher _dispatcher = null!;`. The getter returns the field
  unconditionally with no null guard and no lazy initialisation, unlike the two other lazy-init
  properties in the same class (`UiSyncContext`, `AutoScaleFactor`), which call `Init()` on demand
  when their backing field is still `null`. The `null!` null-forgiving operator suppresses exactly
  the nullable-flow diagnostic (`CS86xx`) that would otherwise flag the hazard at every call site.
- Signals/evidence supporting it: verified directly against origin/main at
  `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` on 2026-09-02 by reading
  `UtilitiesCS/Threading/UiThread.cs:1-163` and UtilitiesCS/Threading/ProgressTrackerAsync.cs, lines
  1-104, in full. The existing repository idiom for the same hazard already exists in
  UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs, lines 57-66, which explicitly comments: "
  `UiThread.Dispatcher` is set-once state populated by `UiThread.Init()` and is null outside a live
  host, so that null state is surfaced as `InvalidOperationException` to preserve the strict
  contract callers relied on." That file already treats `UiThread.Dispatcher`'s return value as
  potentially null despite its non-nullable compile-time type, confirming the type signature
  currently misrepresents the real contract.
- Affected components/modules: `UtilitiesCS/Threading/UiThread.cs` (the accessor); consumers reading
  `UiThread.Dispatcher` without a guard include
  UtilitiesCS/Threading/ProgressTrackerAsync.cs, UtilitiesCS/Threading/ProgressTracker.cs,
  UtilitiesCS/Threading/ProgressTrackerPane.cs, UtilitiesCS/Threading/IdleActionQueue.cs,
  UtilitiesCS/Threading/IdleAsyncQueue.cs, files under UtilitiesCS/HelperClasses/ThemeHelpers/,
  UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs, several files under
  QuickFiler/Controllers/,
  and TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs, files under TaskMaster/AppGlobals/,
  TaskMaster/ThisAddIn.cs. None of these require code changes for this fix: they already either
  (a) run inside a broad `catch (Exception ex)` (for example
  `IdleAsyncQueue.OnApplicationIdle`, verified against `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`'s
  `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`, which asserts only that
  no exception escapes and does not assert on the concrete exception type), or (b) currently fail
  with an unhandled `NullReferenceException` when the static is null and will instead fail with an
  unhandled `InvalidOperationException` of the same unhandled-ness after this fix — a strictly
  clearer failure, not a new one.

## Proposed Fix

### Design summary (what changes where)

Change `UiThread.Dispatcher`'s getter in `UtilitiesCS/Threading/UiThread.cs` to fail fast with an
explicit `InvalidOperationException` naming the missing `Initialize()` call, instead of silently
returning `null`. Change the backing field's declared type from `Dispatcher` (suppressed via
`null!`) to `Dispatcher?` and remove the null-forgiving operator, so the nullable analyser can once
again reason correctly about the field. This mirrors the existing `InvalidOperationException`
contract already established for the same hazard in
UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs.

### Boundaries and invariants to preserve

- `UiThread.Dispatcher`'s public type remains `Dispatcher` (non-nullable): callers keep receiving a
  guaranteed non-null value or an exception, never `null`, so no existing call site's null-handling
  assumptions change.
- `UiThread.Init()` / `UiThread.Initialize()` remain untouched: the `ThreadSafeSingleShotGuard`-based
  one-time initialisation semantics are out of scope for this fix.
- No retry, sleep, or timing tolerance is introduced anywhere in production or test code.

### Dependencies or blocked work

None. The fix is self-contained to `UiThread.cs`'s `Dispatcher` accessor.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

- `UtilitiesCS/Threading/UiThread.cs`
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs` (new regression test class)

#### Functions/classes/CLI commands impacted

- `UtilitiesCS.UiThread.Dispatcher` (property getter and backing field only).

#### Data flow and validation changes

- The `Dispatcher` getter now validates its own backing field before returning, at the single
  source, instead of relying on each of the ~40 call sites (or none) to guard independently.

#### Error handling and logging updates

- The new failure is a named `InvalidOperationException` with a message stating the missing
  `Initialize()` call, matching the message shape already used in
  UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:64-66. No new logging is added; existing catch sites (for example
  `IdleAsyncQueue.OnApplicationIdle`) already log caught exceptions generically.

#### Rollback/feature-flag considerations (if applicable)

None; this is a narrow accessor-contract change with no configuration surface.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- `UiThread.Dispatcher` getter: no inputs; returns `System.Windows.Threading.Dispatcher` (never
  `null`) or throws `InvalidOperationException`.

#### Required configuration keys and defaults

None.

#### Backward-compatibility expectations

- Every existing call site that reads `UiThread.Dispatcher` and is not otherwise guarded already has
  undefined/crashing behavior when the static is unset (`NullReferenceException`). This fix replaces
  that crash with a named, self-diagnosing `InvalidOperationException` at the same call site,
  verified compatible with every currently-passing test that touches this path (see Root Cause
  Analysis).

#### Performance constraints (latency/throughput/memory)

None; the change adds a single null check to an existing property getter.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): the fix does not require determining whether a real
  Outlook/VSTO startup path can read `Dispatcher` before `Init()` completes (issue "Proposed
  direction" item 1); it removes the silent-null hazard regardless of reachability.
- Constraints (budget, performance, compatibility): 1-3 production files, per the assignment's
  small-path budget; no edits to the Claude runtime tree at .claude/**, the Codex mirror tree at
  .codex/**, the dot-agents tree at .agents/**, or the two published files
  config/blast-radius.json and config/orchestration-routing.json.
- External dependencies (services, libraries, releases): none.

## Data / API / Config Impact

- User-facing or API changes: none (internal static utility).
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy

- Regression tests to add or update: a new deterministic test in
  `UtilitiesCS.Test/Threading/UiThread_Tests.cs` that uses reflection to force the private
  `_dispatcher` backing field to `null` (its pre-`Initialize()` state), asserts that
  `UiThread.Dispatcher` throws `InvalidOperationException` naming the missing `Initialize()` call,
  and restores the prior field value in a `finally` block so the shared static is not left
  corrupted for other tests in the assembly (mirroring the existing capture/restore pattern in
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`).
- Unit tests for the fixed behavior and boundaries: the new test covers both the null case (throws)
  and, if a repository-approved seam allows arranging it deterministically, the non-null case
  (returns the same instance stored in the field) without touching WPF message-pump machinery.
- Edge cases and negative scenarios: verified the fix does not change the outcome of
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`'s
  `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` (broad `catch (Exception
  ex)`, no type assertion) and UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs's
  `YieldAsync_WithoutDispatcher_RemainsStrict` and
  `OutlookFolderTreeServiceConcurrencyTests.GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher`
  (neither asserts on `UiThread.Dispatcher`'s exception message text; `WpfDispatcherYieldTests`'s
  four tests supply explicit injected provider delegates and never call the real
  `UiThread.Dispatcher` property at all).
- Error handling and logging verification: the new test asserts the exception type and that it
  names the missing `Initialize()` call.
- Coverage impact and targets for changed lines/modules: the new getter branch is new code and must
  meet the repository's `>= 90%` new-code coverage target; the change is small enough that the
  single new test should reach 100% of the new lines.
- Toolchain commands to run (format → lint → type-check → test): per `CLAUDE.md` C# Toolchain --
  `dotnet tool run csharpier format .` (verify with `check .`), then
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`,
  then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`,
  then `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`.
- Manual validation steps (if required): none; the deterministic test supersedes manual repro.

## Acceptance Criteria

- [ ] AC1: `UiThread.Dispatcher` throws a named `InvalidOperationException` (not a bare
      `NullReferenceException`) when read before `UiThread.Initialize()` has populated its backing
      field, verified by a deterministic regression test in
      `UtilitiesCS.Test/Threading/UiThread_Tests.cs` that does not rely on timing, sleeps, retries,
      or full-suite execution order.
- [ ] AC2: The `null!` null-forgiving suppression on `UiThread`'s `_dispatcher` backing field is
      removed; the field's declared type becomes nullable (`Dispatcher?`) so the nullable analyser
      can verify the getter's guard.
- [ ] AC3: UtilitiesCS/Threading/ProgressTrackerAsync.cs is left unmodified unless the
      implementation phase discovers a concrete reason a change there is required; if left
      unmodified, the plan records the verification that the fix in `UiThread.cs` alone converts the
      downstream failure from an unattributed `NullReferenceException` to a self-diagnosing
      `InvalidOperationException` raised at the `UiDispatcher = UiThread.Dispatcher;` line.
- [ ] AC4: No regression in `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
      UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs,
      UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs, or
      `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` (all pass, unmodified assertions).
- [ ] AC5: No retry, sleep, or timing tolerance is introduced anywhere in the diff.
- [ ] AC6: Full C# toolchain (csharpier -> analyzer msbuild -> nullable msbuild -> vstest with
      coverage) passes in order in a single final pass, with per-step evidence artifacts recorded.
- [ ] AC7: Repository-wide line coverage does not regress relative to the recorded baseline, and
      coverage on the changed lines meets the `>= 90%` new-code target.

## Risks & Mitigations

- Technical or operational risks: a call site not covered by this research relies on
  `UiThread.Dispatcher` silently returning `null` and would now observe an
  `InvalidOperationException` instead. Mitigation: the repo-wide grep enumerated in Root Cause
  Analysis is the complete set of production reads of `UiThread.Dispatcher`
  (`git grep -n "UiThread.Dispatcher\b"`), and each was checked against its nearest test coverage;
  none depends on a silent-null outcome distinct from a generic-exception outcome.
- Mitigations and rollbacks: the change is a single accessor; reverting is a one-file revert with no
  migration or data considerations.

## Rollout & Follow-up

- Release/rollout steps: standard PR review and merge; no feature flag or staged rollout needed.
- Post-fix monitoring or clean-up tasks (candidate follow-ups, not part of this fix):
  1. Add synchronization (or an injectable seam, per the already-partially-adopted `IUiDispatcher`)
     around `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`'s reflection-based mutation of
     `UiThread._dispatcher`, mirroring the fixture-level fix `#493` applied in `QuickFiler.Test`.
  2. The broader injectable-seam conversion of the ~62 remaining direct `UiThread.Dispatcher` reads
     across ~29 production files, already flagged as out of scope on the issue's own comment thread.
- Links: issue #584; related #493 (test-side isolation on the same static, already resolved);
  related #508 (archived; established the `InvalidOperationException` contract this fix reuses).
