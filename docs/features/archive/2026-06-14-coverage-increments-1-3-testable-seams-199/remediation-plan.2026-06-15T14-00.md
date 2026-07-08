# Remediation Plan — Issue #199 / PR #201 (2026-06-15T14-00)

## Cycle Summary

- Canonical issue: 199. PR: #201. Branch: `refactor/coverage-increments-1-3-199`.
- Trigger: post-PR-open failure of required check `Format, build, analyze, and test`
  (run `27550758142`, job `81436048339`). One MSTest failure:
  `UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`,
  assertion at `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:219`.
- Root cause: `UiThread.Dispatcher` (`UtilitiesCS/Threading/UiThread.cs:108-113`) is process-global,
  set-once static state. When an earlier test in the `UtilitiesCS.Test` assembly triggers
  `UiThread.Initialize()`, `Dispatcher` becomes non-null, the `useUiThread=true` branch dispatches the
  action, and `callCount` is `1` instead of the asserted `0`. The failure is order-dependent.
- Fix: TEST-ONLY, exactly one file: `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`. Force
  `UiThread.Dispatcher` null via reflection on private static field `_dispatcher` in Arrange, capture
  the prior value, and restore it after the test (runs on pass or fail). No production change, no
  assertion weakening, no `[DoNotParallelize]`-only substitute, no sleeps/retries/timing tolerances.

## Scope Guard (Mandatory)

If any task appears to require modifying production code (including
`UtilitiesCS/Threading/IdleAsyncQueue.cs` or `UtilitiesCS/Threading/UiThread.cs`) or any file other
than `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, the executor MUST flag-and-stop, record the
scope-change trigger, and return for a new remediation cycle. Do NOT widen scope.

## Evidence Location Invariant

All evidence summary artifacts in this plan are written to canonical `<FEATURE>/evidence/<kind>/`
paths, where `<FEATURE>` is
`docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199`. The raw Cobertura
coverage XML is written to `artifacts/csharp/` only (per the cycle directive); the coverage evidence
summary that records numeric headline values is written to the canonical
`<FEATURE>/evidence/<kind>/` path. Any non-canonical evidence path supplied downstream is rejected and
replaced with the canonical path, recorded as
`EVIDENCE_LOCATION_OVERRIDE_REJECTED: <supplied> replaced with <canonical>`.

---

### Phase 0 — Policy Read and Baseline Capture

- [x] [P0-T1] Read policy files in required order and record the read in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/remediation-baseline/phase0-instructions-read.2026-06-15T14-00.md`.
  Acceptance: artifact exists with `Timestamp:`, `Policy Order:`, and an explicit list of files read:
  `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
  `.claude/rules/csharp.md`, and the cycle directive
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/remediation-inputs.2026-06-15T14-00.md`.
- [x] [P0-T2] Capture baseline csharpier state by running `dotnet tool run csharpier .` and record the
  result in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/remediation-baseline/csharpier-baseline.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` stating
  whether any files were reformatted.
- [x] [P0-T3] Capture baseline analyzer state by running
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  and record the result in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/remediation-baseline/analyzers-baseline.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with the
  build result and warning/error counts.
- [x] [P0-T4] Capture baseline nullable/type-check state by running
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  and record the result in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/remediation-baseline/nullable-baseline.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with the
  build result and warning/error counts.
- [x] [P0-T5] Capture the baseline failing-test signal by running the full `UtilitiesCS.Test`
  assembly with coverage:
  `vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage` and record the result in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/remediation-baseline/mstest-baseline.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
  total/passed/failed counts, the baseline coverage headline percent for the testable denominator, and
  explicit confirmation that
  `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` is among the failures under
  the current execution order (or, if it passes under this run's order, a note recording that the
  failure is order-dependent per the verified root cause).

### Phase 1 — Deterministic Dispatcher-Null Test Fix

- [x] [P1-T1] In `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, add a private static helper
  `ForceDispatcherNull()` in the `#region Helpers` block that captures the current value of the
  private static `UiThread._dispatcher` field
  (`typeof(UiThread).GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)`), sets it
  to `null`, and returns the captured prior value, with an XML doc comment explaining the reset is
  required to eliminate process-global static contamination of `UiThread.Dispatcher`.
  Acceptance: the helper compiles, returns the captured prior `Dispatcher` value, leaves
  `UiThread.Dispatcher` null after invocation, and uses the existing reflection-helper style in the
  file. Scope guard: this edit touches only `IdleAsyncQueue_Tests.cs`.
- [x] [P1-T2] In `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`, add a private static helper
  `RestoreDispatcher(object priorValue)` that writes the supplied prior value back into the private
  static `UiThread._dispatcher` field via reflection, with an XML doc comment noting it reverses
  `ForceDispatcherNull()` so the test does not contaminate other tests in the assembly.
  Acceptance: the helper compiles and restores the previously captured value to
  `UiThread.Dispatcher`. Scope guard: this edit touches only `IdleAsyncQueue_Tests.cs`.
- [x] [P1-T3] In `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`, call
  `ForceDispatcherNull()` in the Arrange step (after `ResetStaticState()`), capturing the returned
  prior value into a local, so the "Dispatcher unavailable" precondition is guaranteed regardless of
  execution order or parallelism.
  Acceptance: the Arrange step establishes `UiThread.Dispatcher == null` deterministically before the
  Act step; the existing AAA structure and comments are preserved. Scope guard: only
  `IdleAsyncQueue_Tests.cs` is modified.
- [x] [P1-T4] In the same test, wrap the Act and Assert sections so the captured prior `_dispatcher`
  value is restored via `RestoreDispatcher(...)` in a `finally` block (or an equivalent
  always-runs mechanism), ensuring restoration runs whether the test passes or fails.
  Acceptance: restoration is in a `finally` (or equivalent) path that executes on both pass and fail;
  no other test reads a contaminated `Dispatcher` after this test. Scope guard: only
  `IdleAsyncQueue_Tests.cs` is modified.
- [x] [P1-T5] Confirm the existing assertions are unchanged: `actDelegate.Should().NotThrow(...)`, the
  queue-drained assertion `GetEntries().Count.Should().Be(0, ...)`, and `callCount.Should().Be(0, ...)`
  at the documented line remain exactly as before, with no weakening, relaxation, deletion, sleeps,
  retries, polling, timing tolerances, or `[DoNotParallelize]`-only substitution introduced.
  Acceptance: a diff review of the test confirms the three assertions and their reason strings are
  unchanged and no prohibited construct was added. Scope guard: only `IdleAsyncQueue_Tests.cs` is
  modified.
- [x] [P1-T6] Confirm `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` remains under 500 lines and
  no production file (`UtilitiesCS/Threading/IdleAsyncQueue.cs`, `UtilitiesCS/Threading/UiThread.cs`,
  or any other file) was modified.
  Acceptance: the file line count is `< 500`; `git status`/diff shows exactly one changed file,
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`. If any other file shows a change, flag-and-stop
  per the Scope Guard.

### Phase 2 — Final QA Loop and CI Verification

- [x] [P2-T1] Run formatting: `dotnet tool run csharpier .`. Record the result in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/remediation-final-csharpier.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`. If
  formatting changes any file, restart the loop at P2-T1.
- [x] [P2-T2] Run linting/analyzers:
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Record the result in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/remediation-final-analyzers.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with a
  clean analyzer result. If the build fails or files change, restart at P2-T1.
- [x] [P2-T3] Run type-check/nullable:
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  Record the result in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/remediation-final-nullable.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with no
  nullable warnings-as-errors. If the build fails or files change, restart at P2-T1.
- [x] [P2-T4] Run the FULL `UtilitiesCS.Test` assembly with coverage (not the single test in
  isolation): `vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage`, directing the
  raw Cobertura XML to `artifacts/csharp/` only. Record the coverage-bearing result summary in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/remediation-final-mstest-coverage.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with
  total/passed/failed counts (0 failed), the post-change coverage headline percent for the testable
  denominator, and explicit confirmation that the full assembly passed. The raw Cobertura XML path is
  recorded as `artifacts/csharp/...`. If any step changed files, restart at P2-T1.
- [x] [P2-T5] Verify order-independence of the fixed test by confirming
  `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` passes within the full
  `UtilitiesCS.Test` assembly run from P2-T4 (the run that exercises the same execution-ordering that
  surfaced the failure). Record the determinism confirmation in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/remediation-determinism-check.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, the test's pass result within the full-assembly run, and
  a statement that the deterministic `_dispatcher`-null Arrange/restore removes the order dependence.
- [x] [P2-T6] Confirm and record coverage no-regression versus the Phase 0 baseline in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/remediation-coverage-delta.2026-06-15T14-00.md`.
  Acceptance: artifact reports baseline coverage (from P0-T5), post-change coverage (from P2-T4), and a
  statement that changed-line coverage did not regress. Because the change is test-only, production
  coverage must not decrease.
- [x] [P2-T7] Confirm PR #201 required check `Format, build, analyze, and test` reports green against
  the branch head after the fix is pushed, and mirror the result in
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/remediation-ci-check.2026-06-15T14-00.md`.
  Acceptance: artifact contains `Timestamp:`, the CI run/job reference, and the check conclusion. If
  the check is still red, record the failure detail and return for a further remediation cycle rather
  than reporting PASS.
