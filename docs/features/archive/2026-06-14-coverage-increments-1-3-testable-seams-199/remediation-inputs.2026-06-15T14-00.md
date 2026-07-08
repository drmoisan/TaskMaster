# Remediation / Scope-Change Inputs — Issue #199 (2026-06-15T14-00)

- Canonical issue number: 199
- Trigger: **Failed required CI check after PR open.** PR #201, check `Format, build, analyze, and test`,
  run `27550758142`, job `81436048339`. One MSTest failure:
  `UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`.
  Assertion: `Expected callCount to be 0 because action must not run when the UiThread Dispatcher is
  unavailable, but found 1 (difference of 1).` at `IdleAsyncQueue_Tests.cs:219`.
- Active folder: `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/`
- Branch: `refactor/coverage-increments-1-3-199`

## Background

The failing test is **not in PR #201's changeset** (the file diff vs `origin/main` is empty for
`UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`). It is a pre-existing test whose latent
order/parallelism sensitivity was surfaced by PR #201 adding many new tests across multiple
assemblies, which changed execution ordering in the `UtilitiesCS.Test` run.

### Root cause (verified by source inspection)

- `IdleAsyncQueue.OnApplicationIdle` (`UtilitiesCS/Threading/IdleAsyncQueue.cs:60-95`) routes a
  `useUiThread == true` entry through `await await UiThread.Dispatcher.InvokeAsync(...)`.
- The test documents and asserts the precondition that `UiThread.Dispatcher` is **null** in the
  test environment, so `InvokeAsync` throws `NullReferenceException`, the internal `try/catch`
  swallows it, and the queued action never runs (`callCount` stays `0`).
- `UiThread.Dispatcher` (`UtilitiesCS/Threading/UiThread.cs:108-113`) is **process-global, set-once
  static state**. It is populated by `UiThread.Initialize()` (creates a `SyncContextForm` and assigns
  `Dispatcher = _syncContextForm.UiDispatcher`). `Initialize()` runs lazily via the `UiSyncContext`
  and `AutoScaleFactor` getters, or directly via `UiThread.Init()`.
- When any other test in the `UtilitiesCS.Test` assembly causes `UiThread.Initialize()` to run before
  this test, `UiThread.Dispatcher` becomes non-null for the remainder of the run. The Dispatcher branch
  then actually dispatches the action, so `callCount == 1` and the assertion fails. The failure is
  therefore order-dependent: the test passes when it runs before any Dispatcher-initializing test and
  fails otherwise.

### Why this is in scope for PR #201

The check `Format, build, analyze, and test` is a required check that blocks PR #201's merge. A
pre-existing, repo-wide failure that blocks the PR's required check is fixed, not deferred
(consistent with the repository's whole-repo-CI-gate handling). The fix is test-only.

## Directive

Make `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` **deterministic**
by explicitly establishing its documented precondition (`UiThread.Dispatcher == null`) in the
test's Arrange step, independent of test ordering or parallelism.

This is a determinism fix, not an assertion relaxation. The assertion (`callCount == 0`,
queue drained) and the documented behavior under test (Dispatcher-unavailable fault isolation)
**must remain unchanged**. Weakening, relaxing, or deleting the assertion is prohibited.

## Authorized change (test-only)

**Test file:** `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` (1 test file; 0 production files)

Required behavior:

1. Before exercising the Dispatcher-routing branch, force `UiThread.Dispatcher` to `null` via
   reflection on the private static backing field `_dispatcher`
   (`typeof(UiThread).GetField("_dispatcher", BindingFlags.NonPublic | BindingFlags.Static)`),
   so the "Dispatcher unavailable" scenario is guaranteed regardless of whether an earlier test
   initialized it.
2. Capture the prior `_dispatcher` value and **restore it after the test** (e.g., `[TestCleanup]`
   or a `try/finally`) so this test does not contaminate other tests in the assembly. Restoration
   must run whether the test passes or fails.
3. Keep the existing reflection-helper style already used in this file (`ResetStaticState`,
   `GetEntries`, `InvokeOnIdle`). Prefer a small, well-named private helper (for example
   `ForceDispatcherNull()` returning the captured prior value) over inline reflection, with an XML
   doc comment explaining why the reset is required (global static contamination).
4. Do not add sleeps, retries, polling, or timing tolerances. Do not add `[DoNotParallelize]` as a
   substitute for the deterministic reset — `[DoNotParallelize]` controls parallelism only and does
   not address sequential order-dependence, so it does not fix the root cause on its own.

## Constraints

- Change is limited to `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`. If a fix appears to
  require touching production code or any other file, **flag-and-stop** (scope-change rule) and
  return for a new cycle.
- Do not modify production `IdleAsyncQueue.cs` or `UiThread.cs`.
- Test framework: MSTest + Moq + FluentAssertions, AAA pattern, deterministic, no temp files,
  no WinForms message loop.
- File-size limit: keep `IdleAsyncQueue_Tests.cs` under 500 lines.
- Full C# toolchain green in order: csharpier → msbuild analyzers → msbuild nullable
  (`/p:TreatWarningsAsErrors=true`) → MSTest with coverage.
- Do not write full Cobertura coverage XML into the feature evidence folder; write it to
  `artifacts/csharp/` only.

## Acceptance for this cycle

- `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` deterministically
  establishes `UiThread.Dispatcher == null` in Arrange and restores the prior value afterward.
- The test passes regardless of execution order relative to Dispatcher-initializing tests in the
  `UtilitiesCS.Test` assembly (verify by running the full assembly, not the test in isolation).
- No assertion is weakened; the documented Dispatcher-unavailable behavior is still exercised.
- Full C# toolchain green.
- PR #201 required check `Format, build, analyze, and test` passes (green CI run on branch head).
