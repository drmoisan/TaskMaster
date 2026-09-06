# uithread-dispatcher-null-race-progresstrackerasync (Spec)

- **Issue:** #584
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-03
- **Status:** Merged (PR #778, merge commit 1c3b210c, 2026-09-04) (amended in plan revision round 15: write set and AC4 extended to a sixth file;
  amended in plan revision round 16: AC5 returned to unchecked pending the sixth file's token-filter
  artifact; amended in plan revision round 17 (preflight round 17 non-blocking findings N1-N4
  applied), of which finding N4 is the only one touching this file: AC5's Evidence line now states the
  diff's added-line figure as the artifact records it)
- **Version:** 0.5

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
  dereference downstream (`ProgressTrackerAsync.InitializeAsync()`, or any of the other reads among
  the 49 live reads across 25 production files, measured against the `pre-782-base` tag under issue
  #782, with 64 textual occurrences across 30 files of which 15 are comments, XML documentation,
  commented-out code, or the exception message literal, spread across `UtilitiesCS`, `QuickFiler`,
  and `TaskMaster` and reading `UiThread.Dispatcher` without a guard) throws an unattributed
  `NullReferenceException`.
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
  - `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — the one reflective consumer of the
    `Dispatcher` PROPERTY in the repository. Its `[TestInitialize]`/`[TestCleanup]` snapshot is
    retargeted from the public property to the private `_dispatcher` backing field, matching the
    idiom the four other reflective consumers already use. Added to scope on 2026-09-03 after the
    full `QuickFiler.Test` run failed 8 of 1312 on this class; see Root Cause Analysis.
  - `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` — a reflective consumer of the private
    `_dispatcher` backing field, formatted and re-verified by the same toolchain pass.
  - `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` — a reflective consumer of the same
    backing field, formatted and re-verified by the same toolchain pass.
  - `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — a reflective consumer of the same
    backing field, formatted and re-verified by the same toolchain pass.
- Out of scope / non-goals:
  - UtilitiesCS/Threading/ProgressTrackerAsync.cs — verified not to require a change (see Root
    Cause Analysis). Named as a fix site to verify in the assignment, not assumed to need an edit.
  - The injectable-seam conversion replacing the 49 live reads across 25 production files, measured
    against the `pre-782-base` tag under issue #782, with 64 textual occurrences across 30 files of
    which 15 are comments, XML documentation, commented-out code, or the exception message literal,
    with the existing `IUiDispatcher` seam. Already identified and
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
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`

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

See this document's `## Write Set` section, which is the single authoritative enumeration. This
section deliberately carries no independent list: a third enumeration alongside the in-scope list
and the Write Set is what allowed the three to disagree.

#### Functions/classes/CLI commands impacted

- `UtilitiesCS.UiThread.Dispatcher` (property getter and backing field only).

#### Data flow and validation changes

- The `Dispatcher` getter now validates its own backing field before returning, at the single
  source, instead of relying on each of the 49 live reads across 25 production files, measured
  against the `pre-782-base` tag under issue #782, with 64 textual occurrences across 30 files of
  which 15 are comments, XML documentation, commented-out code, or the exception message literal,
  to guard independently (or on none of them doing so).

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
- Toolchain commands to run (format → lint → type-check → test): per CLAUDE.md C# Toolchain --
  `dotnet tool run csharpier format .` (verify with `check .`), then
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`,
  then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`,
  then `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`.
- Manual validation steps (if required): none; the deterministic test supersedes manual repro.

## Acceptance Criteria

- [x] AC1: `UiThread.Dispatcher` throws a named `InvalidOperationException` (not a bare
      `NullReferenceException`) when read before `UiThread.Initialize()` has populated its backing
      field, verified by a deterministic regression test in
      `UtilitiesCS.Test/Threading/UiThread_Tests.cs` that does not rely on timing, sleeps, retries,
      or full-suite execution order.
      Evidence: `evidence/regression-testing/p1-t4-expect-fail.md` (`Failed: 1` against the unfixed
      accessor, with the verbatim message "Expected a &lt;System.InvalidOperationException&gt; to be
      thrown, but no exception was thrown.") and
      `evidence/regression-testing/p3-t2-regression-green.md` (`Passed: 2` against the fixed
      accessor).
- [x] AC2: The `null!` null-forgiving suppression on `UiThread`'s `_dispatcher` backing field is
      removed; the field's declared type becomes nullable (`Dispatcher?`) so the nullable analyser
      can verify the getter's guard.
      Evidence: `evidence/qa-gates/p2-t2-nullforgiving-removed.md` (zero `null!` matches in
      `UtilitiesCS/Threading/UiThread.cs`, and `private static Dispatcher? _dispatcher;` present on
      line 149) and `evidence/qa-gates/p4-t4-nullable-build.md` (`0 Error(s)` under
      `/p:TreatWarningsAsErrors=true`, so the compiler confirms the guard narrows the field and no
      `CS8603` is raised).
- [x] AC3: UtilitiesCS/Threading/ProgressTrackerAsync.cs is left unmodified unless the
      implementation phase discovers a concrete reason a change there is required; if left
      unmodified, the plan records the verification that the fix in `UiThread.cs` alone converts the
      downstream failure from an unattributed `NullReferenceException` to a self-diagnosing
      `InvalidOperationException` raised at the `UiDispatcher = UiThread.Dispatcher;` line.
      Evidence: `evidence/other/p3-t4-progresstrackerasync-unmodified.md`, which records the empty
      `--cached` name-status diff for that path against BASE, the empty porcelain status for that
      path, the single `git grep` hit at line 33, and the verification paragraph explaining why the
      fix in `UiThread.cs` alone converts the consumer's failure mode.
- [x] AC4: No regression in `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
      UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs,
      UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs,
      `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`, or
      `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` (all pass, unmodified assertions).
      The last of those five is modified by this change, and "unmodified assertions" is the binding
      constraint on how: the file's `[TestInitialize]`/`[TestCleanup]` snapshot is retargeted from the
      public `Dispatcher` property to the private `_dispatcher` backing field, and no assertion, no
      test method, and no mock setup in it is added, removed, or altered.
      Evidence: `evidence/qa-gates/p1-t5-donotparallelize.md` (the change to
      `IdleAsyncQueue_Tests.cs` and `ProgressTrackerAsync_Tests.cs` is attribute-only and alters no
      assertion), `evidence/regression-testing/p3-t3-at-risk-tests.md` (41 tests executed, all five
      named at-risk tests present and passing, no failure outside the empty `BASELINE_FAILURE_SET`),
      `evidence/regression-testing/p3-t6-quickfiler-wpfuidispatcher.md` (`Failed: 0`),
      `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md` (the change to
      `EmailMoveMonitorTests.cs` retargets one reflection lookup, alters no assertion, and leaves the
      `[TestMethod]` count at 8), `evidence/regression-testing/p4-t6-first-pass-failure.md` (the
      fail-before: 1304 of 1312, with all 8 of that class's tests failing), and
      `evidence/qa-gates/p4-t6-quickfiler-tests.md` (the pass-after: 1312 of 1312).
      Amendment note (2026-09-03, plan revision round 15): this criterion previously named four files
      and carried a scope note recording an open regression in a fifth. That regression was in
      `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, found by P4-T6 and attributed to the
      `UiThread.cs` fix by an executed counterfactual. The file is now named in this criterion and
      repaired by plan task P2-T4, and this criterion is returned to unchecked until the pass-after
      evidence exists. Leaving it out was not tenable: this change now modifies that file, so a
      no-regression criterion that did not name it would leave the repair with no acceptance
      criterion binding it, and the previous scope note would have remained literally false once the
      repair landed.
- [x] AC5: No retry, sleep, or timing tolerance is introduced anywhere in the diff.
      Evidence: `evidence/qa-gates/p3-t5-no-timing-tokens.md`, recording zero matching added lines in
      the BASE-anchored diff (5626 bytes, 94 lines beginning with `+` including the five `+++` file
      headers, across the five owned files; the
      case-insensitive seven-token filter printed nothing and exited 1); and
      `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md`, recording zero matching lines
      in the BASE-anchored diff over the sixth owned file
      `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` (2570 bytes; the identical
      case-insensitive seven-token filter, applied to the whole diff rather than only its added
      lines, printed nothing and exited 1). The two together make "anywhere in the diff" true of the
      whole diff: P3-T5's pathspec `UtilitiesCS UtilitiesCS.Test` covers five of the six owned files
      and P2-T4's covers the sixth.
      Amendment note (2026-09-03, plan revision round 16): this criterion is returned to unchecked in
      this round. It had been checked on the strength of
      `evidence/qa-gates/p3-t5-no-timing-tokens.md` alone, and that artifact's diff pathspec is
      `UtilitiesCS UtilitiesCS.Test`, which reaches five of the six owned files. Plan revision round 15
      widened the write set to a sixth file, `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`,
      which that pathspec does not reach, so this criterion's "anywhere in the diff" wording was not
      yet evidenced across the whole diff at the time it stood checked. The criterion text is unchanged
      and its scope is not narrowed. The evidence chain closes without widening that pathspec: plan
      task P2-T4 carries the identical case-insensitive seven-token filter over the sixth file and
      writes `evidence/qa-gates/p2-t4-emailmovemonitor-reflection-target.md`, and plan task P5-T5
      re-checks this criterion citing both artifacts once that second artifact exists.
- [x] AC6: Full C# toolchain (csharpier -> analyzer msbuild -> nullable msbuild -> vstest with
      coverage) passes in order in a single final pass, with per-step evidence artifacts recorded.
      Evidence: `evidence/qa-gates/p4-t1-format.md` (`EXIT_CODE: 0`, `Formatted 6 files`, identical
      before-and-after unscoped porcelain), `evidence/qa-gates/p4-t2-format-check.md`
      (`EXIT_CODE: 0`, `Checked 1576 files`, empty reported set),
      `evidence/qa-gates/p4-t3-analyzer-build.md` (`EXIT_CODE: 0`, `0 Warning(s)`, `0 Error(s)`),
      `evidence/qa-gates/p4-t4-nullable-build.md` (`EXIT_CODE: 0`, `0 Error(s)`),
      `evidence/qa-gates/p4-t5-utilitiescs-tests.md` (`EXIT_CODE: 0`, 4787 of 4787 passed),
      `evidence/qa-gates/p4-t6-quickfiler-tests.md` (`EXIT_CODE: 0`, 1312 of 1312 passed), and
      `evidence/qa-gates/p4-t8-loop-closure.md` (all seven steps listed in order, both Phase 4 passes
      recorded chronologically, no step after P4-T1 rewrote a tracked file, so the second pass is a
      single clean pass). `evidence/qa-gates/p4-t7-coverage-delta.md` is deliberately not cited here;
      it is AC7's evidence.
- [x] AC7: Repository-wide line coverage does not regress relative to the recorded baseline, and
      coverage on the changed lines meets the `>= 90%` new-code target.
      Evidence: `evidence/baseline/p0-t10-utilitiescs-tests-coverage.md` for the baseline figures and
      `evidence/qa-gates/p4-t7-coverage-delta.md` for the comparison. Baseline `line-rate`
      0.7073317347831605; post-change `line-rate` 0.7073603942281368, an increase of +0.0000286594,
      so no regression against the baseline and the 0.005 tolerance is not consumed. Signed
      `lines-valid` difference: 149761 - 149719 = +42, inside the 0 to 200 comparability band, so no
      `COVERAGE DENOMINATOR MISMATCH` was recorded and the repository-wide comparison stands rather
      than being VOID. Changed-line coverage: 100.0% (8 of 8 coverable added lines in
      `UtilitiesCS/Threading/UiThread.cs` — lines 138, 139, 140, 141, 142, 143, 145, and 146 — each
      with `hits` of 1 or more), which meets the `>= 90%` new-code target. Both `line-rate` figures
      are raw unstripped dotnet-coverage line-rates for the UtilitiesCS.Test process and are not the
      repository first-party figure CLAUDE.md's 80% refers to.

## Risks & Mitigations

- Technical or operational risks: a call site not covered by this research relies on
  `UiThread.Dispatcher` silently returning `null` and would now observe an
  `InvalidOperationException` instead. Mitigation: the repo-wide grep enumerated in Root Cause
  Analysis is the complete set of production reads of `UiThread.Dispatcher` spelled as the qualified
  member expression (`git grep -n "UiThread.Dispatcher\b"`), and each was checked against its nearest
  test coverage; none depends on a silent-null outcome distinct from a generic-exception outcome.
- Census limitation recorded on 2026-09-03, after it materialised as a real regression: that grep
  matches only the literal text `UiThread.Dispatcher`, so it cannot match a REFLECTIVE read, which
  never spells the qualified expression at all. One such read existed —
  `typeof(UiThread).GetProperty("Dispatcher", ...)` in
  `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — and it failed 8 of that class's 8 tests
  when the guarded getter shipped, because `PropertyInfo.GetValue` propagates an exception a throwing
  getter raises. The complementary census the plan DID run, `git grep -F '"_dispatcher"'`, covers
  reflective reads of the private FIELD and correctly found three files; no equivalent census was run
  for the property NAME. Plan task P0-T14 now runs that census across all nine test assemblies and
  repository-wide across `.cs` files, and records its full output. Its re-derived result is that
  exactly one reflective property read exists in the repository, in the file above, and that every
  other occurrence of the literal `"Dispatcher"` is an XML `<see cref="Dispatcher"/>` documentation
  cross-reference. No production file reads `UiThread.Dispatcher` reflectively.
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
