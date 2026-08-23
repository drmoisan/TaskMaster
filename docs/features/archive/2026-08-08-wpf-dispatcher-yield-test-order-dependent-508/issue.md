# wpf-dispatcher-yield-test-order-dependent

- Work Mode: minor-audit
- Issue: #508
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/508
- Promotion Type: bug
- Base Branch: main
- Merge Base: 003c5715055d7d1933db68a742531332756e30b2
- Branch: bug/wpf-dispatcher-yield-test-order-dependent-508
- Last Updated: 2026-08-08

## Problem / Why

`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`
(`WpfDispatcherYieldTests.cs:28-37`) fails intermittently on the full-suite run.

The test asserts that `WpfDispatcherYield.YieldAsync` throws `InvalidOperationException` when no WPF
`Dispatcher` is available. The production resolution is:

```csharp
Dispatcher dispatcher =
    Dispatcher.FromThread(Thread.CurrentThread) ?? UtilitiesCS.UiThread.Dispatcher;
if (dispatcher is null) { throw new InvalidOperationException(...); }
```

Both operands of that `??` are ambient process/thread state that the test never arranges:

1. `Dispatcher.FromThread(Thread.CurrentThread)` is non-null whenever an earlier test that landed on
   the same pooled worker thread touched `Dispatcher.CurrentDispatcher`, which creates and caches a
   dispatcher for the calling thread on first access. At least nine test classes in this assembly do
   exactly that (for example `OutlookFolderTreeServiceConcurrencyTests`, `ProgressTracker_Tests`,
   `ProgressViewer_Tests`, `WpfUiDispatcherTests`).
2. `UiThread.Dispatcher` is process-global, set-once static state that becomes non-null for the
   remainder of the run as soon as any test triggers `UiThread.Initialize()`.

Under `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]` the thread this test
lands on is not determined, so the "without dispatcher" precondition silently evaporates and the
assertion fails.

This violates `.claude/rules/general-unit-test.md` Core Principles 1 (Independence) and 4
(Determinism).

## Impact / Severity

Medium. The suite is not reliably green at baseline, which undermines every downstream quality gate:
an agent or developer cannot distinguish "my change broke a test" from "the suite is flaky". It also
produces spurious CI failures and encourages re-running until green, which is the exact failure mode
the determinism rule exists to prevent.

Evidence of non-determinism (two consecutive baseline runs at merge-base `003c5715`, no intervening
code change):

- Run 1: `Total tests: 6293, Passed: 6291, Failed: 2`
- Run 2: `Failed: 1` — `YieldAsync_WithoutDispatcher_RemainsStrict`

## Implementation Intent

Make the dispatcher-free precondition something the test **arranges** rather than something it
**inherits**. The preferred shape is an injectable seam on `WpfDispatcherYield` for the two
dispatcher lookups, so the absent-dispatcher case can be constructed explicitly:

- Keep the production resolution order (`thread-affinitized dispatcher`, then the process-global
  `UiThread.Dispatcher` fallback) inside the class under test so the test still verifies the ordering.
- Default the seams to the current production behavior so no runtime behavior changes and no call
  site needs updating.
- Rewrite the test to supply seam values directly, covering: thread dispatcher present, thread
  dispatcher absent with fallback present, and both absent (the strict `InvalidOperationException`
  contract).

An alternative shape — running the assertion on a dedicated thread the test itself owns — arranges
only the first operand and still leaves the `UiThread.Dispatcher` global unarranged, so it is not
sufficient on its own.

## Scope Boundary

- In scope: `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` and, if required for
  arrangeability, `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`.
- Out of scope: `TaskMaster/Ribbon/**` (concurrent work on issues #503 and #507), unrelated tests,
  and any broader refactor of `UiThread` static state.

## Dependencies / Risks

- `WpfDispatcherYield` is currently marked `[ExcludeFromCodeCoverage]`. If the class becomes
  genuinely unit-testable, the attribute should be reconsidered rather than left in place by inertia.
- `UiThread.Dispatcher` is process-global. Any mitigation that mutates it via reflection would need
  serialization and is a second-choice approach compared to a constructor seam.
- The defect is intermittent, so a single green run does not demonstrate a fix.

## Verification Steps

1. Build the solution in Debug.
2. Run the `UtilitiesCS.Test` assembly with class-level parallelization enabled, repeatedly (at least
   three full runs), and confirm an identical pass/fail count on every run.
3. Confirm the new test fails when the seam is removed / the precondition is un-arranged
   (fail-before evidence).
4. Run the full C# toolchain in order: `csharpier .` -> analyzer msbuild -> nullable msbuild ->
   `vstest.console.exe ... /EnableCodeCoverage`.

When globbing for `*.Test.dll`, exclude any discovered assembly that resolves outside the active
workspace root; stale agent-worktree builds otherwise get picked up and produce bogus
`AssemblyInitialize` signature failures. Note that a naive "path contains `\.claude\`" substring test
is unsatisfiable when the workspace root is itself an agent worktree under
`.claude/worktrees/`; the correct assertion is a workspace-root prefix test plus the absence of a
nested `\.claude\worktrees\` segment after that prefix. See the plan's `## MSTest Discovery Caveat`.

Git diff/status gates in this feature must be scoped to source paths
(`-- '*.cs' '*.csproj' '*.sln'`). `.claude/agent-memory/**` is tracked and already modified at the
branch head, and its text contains tokens such as `DoNotParallelize`, so an unscoped diff produces
both unsatisfiable "lists exactly" assertions and false-positive prohibited-fix grep hits.

## Prohibited Fixes

The following are explicitly **not** acceptable resolutions, per `.claude/rules/csharp.md`
("Prohibited Behaviors") and `.claude/rules/general-unit-test.md`:

- Disabling parallelization (`[DoNotParallelize]`) as the mechanism of the fix.
- Adding a retry, sleep, or other timing hack.
- `[Ignore]`-ing or deleting the test.
- Weakening the assertion to a condition that holds regardless of the precondition.
- Creating temporary files in tests.

## Acceptance Criteria

- [x] AC1: `YieldAsync_WithoutDispatcher_RemainsStrict` (or its deterministic replacement) arranges
      its own dispatcher-free precondition explicitly; the test result no longer depends on which
      pooled thread it runs on, on test execution order, or on whether `UiThread.Initialize()` ran
      earlier in the process.
- [x] AC2: The strict contract is preserved and not weakened: `WpfDispatcherYield.YieldAsync` still
      throws `InvalidOperationException` when no dispatcher is resolvable, and the test still asserts
      exactly that.
- [x] AC3: Test coverage pins all three resolution branches of `YieldAsync`: thread-affinitized
      dispatcher present, thread dispatcher absent with `UiThread.Dispatcher` fallback present, and
      both absent (throws).
- [x] AC4: Any production change to `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` is
      minimal, is justified in the PR body, and preserves the existing runtime resolution order and
      exception contract for all existing call sites (no call-site changes required).
- [x] AC5: The fix uses none of the approaches listed under "Prohibited Fixes" above.
- [x] AC6: Fail-before evidence is recorded showing the defect reproduces (or a schema-valid
      fail-before exception dossier explaining why a failing run is not reproducible on demand).
- [x] AC7: At least three consecutive full parallel runs of the `UtilitiesCS.Test` assembly are
      recorded as evidence, all with an identical and fully green result for
      `WpfDispatcherYieldTests`.
- [x] AC8: The full C# toolchain (csharpier -> analyzer msbuild -> nullable msbuild -> vstest with
      coverage) passes in order in a single final pass, with per-step evidence artifacts recorded.
- [x] AC9: Repository-wide line coverage does not regress relative to the recorded baseline, and
      coverage on changed lines does not decrease.

## Evidence Checklist

- [x] baseline
- [x] targeted verification
- [x] end-state

## Source

Promoted during the #503 work from
`docs/features/potential/promoted/2026-08-08-wpf-dispatcher-yield-test-order-dependent.md`
(present on `bug/ribbon-engine-readiness-guard-503`, not yet on `main`).
