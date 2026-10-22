# Phase 1 — Fail-before capture for the two cleanup sites (AC6)

Timestamp: 2026-09-09T12-57
Task: [P1-T7] [expect-fail]

Command:

```text
pwsh -NoProfile -Command '$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~Cleanup_CalledTwice_InvokesParentCleanupOnce|FullyQualifiedName~Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted"'
```

EXIT_CODE: 1
ExpectedExitCode: 1

`/Logger:trx` was deliberately **not** passed. `.trx` is not gitignored in this repository and
carries the host user name and absolute paths in two casings, and this artifact is committed.

All three tests live in `QuickFiler.Test.dll`, so one assembly suffices.

## The three failing tests

### 1. `QfcHomeControllerCleanupTests.Cleanup_CalledTwice_InvokesParentCleanupOnce`

Result: **Failed** [4 ms]. Assertion failure message, verbatim:

```text
Test method QuickFiler.Controllers.Tests.QfcHomeControllerCleanupTests.Cleanup_CalledTwice_InvokesParentCleanupOnce threw exception:
Moq.MockException: the ribbon release callback must fire at most once per controller instance
Expected invocation on the mock once, but was 2 times: x => x.Invoke()

Performed invocations:

   Mock<Action:2> (x):

      Action.Invoke()
      Action.Invoke()
```

Failing source location: `QuickFiler.Test\Controllers\QfcHomeControllerCleanupTests.cs:line 185`.

### 2. `QfcHomeControllerCleanupTests.Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted`

Result: **Failed** [249 ms]. Assertion failure message, verbatim:

```text
Test method QuickFiler.Controllers.Tests.QfcHomeControllerCleanupTests.Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted threw exception:
Moq.MockException: 
Expected invocation on the mock once, but was 2 times: x => x.Invoke()

Performed invocations:

   Mock<Action:1> (x):

      Action.Invoke()
      Action.Invoke()
```

Failing source location: `QuickFiler.Test\Controllers\QfcHomeControllerCleanupTests.cs:line 119`.
This is the `parentCleanup.Verify(x => x.Invoke(), Times.Once);` assertion whose own text was not
altered; only the position of the second `controller.Cleanup()` call changed, which is exactly what
made this previously-blind assertion able to observe the defect.

### 3. `EfcHomeControllerLifecycleTests.Cleanup_CalledTwice_InvokesParentCleanupOnce`

Result: **Failed** [258 ms]. Assertion failure message, verbatim:

```text
Expected probe.ParentCleanupCallCount to be 1 because the ribbon release callback must fire at most once per controller instance, but found 2.
```

Failing source location: `QuickFiler.Test\Controllers\EfcHomeControllerLifecycleTests.cs:line 189`.

## Run summary

```text
Total tests: 3
     Failed: 3
Test Run Failed.
 Total time: 1.3473 Seconds
```

The output contains the literal `Test Run Failed.`

Output Summary: all three tests the plan names failed against the pre-fix production code, and each
failed on the same observable — the parent-cleanup delegate was invoked **2 times** where the
invariant permits **1**. None of the three passed, so the guard being added is the guard the defect
requires. Exit code 1 equals the declared `ExpectedExitCode`, so this gate normalizes to pass. The
matching pass-after record is written by `[P6-T6]` to
`docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/pass-after-cleanup-sites.2026-09-09T00-05.md`.

Note on the captured console output: vstest also emitted a Fluent Assertions licensing notice under
`Standard Output Messages` and `Debug Trace` for one test. It is unrelated to the assertions above
and is not reproduced here.
