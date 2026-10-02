---
name: engine-toggle-fault-suppression-948
description: "#948 research (2026-10-01): EngineTogglePressedStateCache is never cleared so 'per episode' == 'per key per lifetime'; first prime fault can be a transient NRE (Engines assigned before AF.Manager); ResetConfigAsyncLazy has no post-ctor production caller; MSTest scripts have no filter parameter"
metadata:
  type: project
---

Research for issue #948 (permanent configuration fault logged on every cache-miss `getPressed` poll)
completed 2026-10-01. Research file:
`docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/research/2026-10-01T07-20-engine-toggle-permanent-config-fault-logs-every-poll-research.md`

**Why:** several facts about the ribbon toggle coordinator and its test tooling are non-obvious and
will recur for #947 (throwing logError sink) and any later coordinator change.

**How to apply:** when touching `EngineToggleStateCoordinator`, its four-plus test partials, or
designing suppression/back-off policies anywhere a cache is populated once and never cleared.

- **`EngineTogglePressedStateCache` has no remove/clear** (`NextSequence`, `TryGetActive`,
  `TryApplyState` only; `_pressedState` is `readonly`). Once a key is cached, `StartPrimeIfNeeded`
  is never reached again for it. Therefore "suppress until a success resets it" is UNOBSERVABLE
  and untestable; a per-key suppression collapses to per-key-per-lifetime. Do not propose a
  reset-on-success line; it cannot be pinned by a test.
- **A transient first fault is structurally possible:** `ApplicationGlobals.cs:120` assigns
  `Engines` in the ctor, `AppAutoFileObjects.Manager` is assigned only in the load paths
  (`:68`, `:86`), so `EngineActiveAsync` can NRE before the permanent config fault appears. That is
  why #948 recommends keying suppression on `(engineKey, failure.GetType())` rather than key alone.
- **`AsyncLazy<T>` caches the faulted `Task` object** (`Lazy<Task<T>>` over `Task.Run(factory)`);
  `ResetConfigAsyncLazy()` has exactly two production call sites, both inside `ManagerAsyncLazy`
  (ctor `:41`, null-guard `:329`) — no production recovery path exists for a faulted config load,
  and `IAppItemEngines` exposes nothing that reaches it. "Recovery via explicit reset" policies
  have no caller.
- **`Invoke-MSTestWithCoverage.ps1` / `Invoke-MSTest.ps1` have NO test-filter parameter**; the
  inner vstest args are fixed (`/Settings`, `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`,
  explicit `/ResultsDirectory` + `/Logger:trx;LogFileName=`). `-SearchRoot TaskMaster.Test` gives a
  scoped run that skips the 80/75 thresholds. Per-class runs must invoke `vstest.console.exe`
  directly with `/TestCaseFilter:FullyQualifiedName~...` (exact command shape in
  `.../944/evidence/baseline/coordinator-tests-baseline.md:4`). No shell-icon exclusion exists in
  repo scripts; the known shell-icon hang is UtilitiesCS.Test only.
- **Fixture facts (2026-10-01):** four partials, 26 methods / 28 cases; main fixture 470 lines
  (do not grow it); all prime-failure message assertions are `Contain(SpamEngine)`, so the
  message text can change safely. `.Race.cs:195-202` remark says a re-prime "logs a second error"
  — false once suppression lands.
- **Session note:** the Bash tool can be disabled for a researcher session; verify commit claims
  from file content (e.g. the `issue #944` comment in the production file) instead of `git log`.
- The #947 promoted record was NOT present in the #948 worktree; describe #947 from the #944
  spec (`:278`, `:287`) and research (§8 item 1).

Related: [[ribbon-engine-toggle-defects-735]], [[ribbon-toggle-state-guards-505]], [[net481-timeprovider-available]]
