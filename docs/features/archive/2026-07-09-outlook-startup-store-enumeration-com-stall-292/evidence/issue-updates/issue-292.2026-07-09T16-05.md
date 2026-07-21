# Issue #292 Update Mirror — Remediation Cycle 1

- Timestamp: 2026-07-09T16-05
- PostedAs: unknown (local mirror only; not posted to GitHub by this executor)

## Remediation summary (intended text)

Blocking CI test-isolation regression (issue #292 / PR #294 head `9ae5c0e3`) — remediated.

Root cause: `CurrentStoreContext` is a process-global `static volatile string _current` (required by the
#260 watchdog cross-thread read; must not become `AsyncLocal`). This PR added enumeration-phase writers
in `StoresWrapper`/`StoreWrapper` that set `_current = "<Stores-enumeration>"` during store materialization.
`UtilitiesCS.Test` runs class-level parallel; the `[DoNotParallelize]` null-baseline readers
(`CurrentStoreContextTests`, `ThreadMonitorTests`) observed the polluted global written by store test
classes still in the parallel bucket, causing 10 deterministic-under-parallelism failures at CI run
29046195330.

Selected approach (A): move every scope-opening `UtilitiesCS.Test` class into the serialized non-parallel
bucket by adding `[DoNotParallelize]`. MSTest runs all `[DoNotParallelize]` classes sequentially and never
concurrently with each other; the readers are already in that bucket, so once every writer joins it no
writer can overlap a reader. Structural mutual-exclusion guarantee, not a timing hack; no production code,
no weakened assertions, no removed enumeration scope, no sleeps/retries.

Files changed (test-only, `[DoNotParallelize]` added to the class): `StoresWrapperTests`,
`StoresWrapperRehookTests`, `StoresWrapperDisableTests`, `StoreWrapperTests`, `StoreWrapperViewerTests`,
`StoreWrapperInitProbeTests`, `StoreWrapperController_Tests` (one partial part), `StoreWrapperControllerTests`.
`StoreDisableServiceTests` evaluated and confirmed N/A (opens no scope).

Verification:
- CI-equivalent full-suite (`/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`):
  5 green passes (5141/5141, Failed 0) plus 6 more green confirmation passes; the 10 formerly-failing tests
  pass in every pass. git-stash baseline reproduced the pristine race (10 failures, schedule-dependent).
- Toolchain: csharpier clean; analyzer build 0 errors; nullable build 0 warnings/0 errors.
- Coverage: repository-wide line-rate 81.80% (unchanged); UtilitiesCS 88.33%. No regression (no production
  code changed).

Evidence: `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/evidence/`
(`regression-testing/green-after-fix.2026-07-09T16-05.md`, `qa-gates/`, `other/`).

## NOTE — newly discovered out-of-scope finding

`TaskMaster.Test` contains a second, pre-existing, intra-assembly instance of the same `CurrentStoreContext`
race (`StoresWrapperEnumerationScopeTests` is an unmarked scope-opening null-baseline reader;
`AppOlObjectsAttributionContextTests` is already `[DoNotParallelize]`). `TaskMaster.Test` has NO
`[assembly: Parallelize]`, so it runs sequentially and is race-free under the CI `/EnableCodeCoverage`
invocation (NOT a CI-gate risk); the flake only appears when the coverage-measurement
`TaskMaster.cli.runsettings` force-imposes `ClassLevel` parallelization. It is OUTSIDE this plan's
`UtilitiesCS.Test` scope and was reported, not fixed, per the execution directive. Recommended follow-up:
mark `StoresWrapperEnumerationScopeTests` (and any `TaskMaster.Test` `CurrentStoreContext` writer)
`[DoNotParallelize]` so the tests are robust if `TaskMaster.Test` is ever parallelized.
See `evidence/other/out-of-scope-finding-taskmaster-test-race.2026-07-09T16-05.md`.
