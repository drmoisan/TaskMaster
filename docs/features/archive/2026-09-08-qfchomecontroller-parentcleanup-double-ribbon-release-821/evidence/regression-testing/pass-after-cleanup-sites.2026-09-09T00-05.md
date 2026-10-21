# Phase 6 — Pass-after record for the two cleanup sites

Timestamp: 2026-09-09T13-55
Task: [P6-T6]

Matching fail-before record, cited by path:
`docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/fail-before-cleanup-sites.2026-09-09T00-05.md`

Source of these results: the `[P6-T5]` captured run,
`pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/coverage.cobertura.xml`,
EXIT_CODE 0.

## The three tests, before and after

| # | Test | Fail-before result | Pass-after result |
|---|---|---|---|
| 1 | `QfcHomeControllerCleanupTests.Cleanup_CalledTwice_InvokesParentCleanupOnce` | **Failed** — `Expected invocation on the mock once, but was 2 times: x => x.Invoke()` | **Passed** |
| 2 | `QfcHomeControllerCleanupTests.Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` | **Failed** — `Expected invocation on the mock once, but was 2 times: x => x.Invoke()` | **Passed** |
| 3 | `EfcHomeControllerLifecycleTests.Cleanup_CalledTwice_InvokesParentCleanupOnce` | **Failed** — `Expected probe.ParentCleanupCallCount to be 1 ... but found 2.` | **Passed** |

Result lines located in the `[P6-T5]` captured output, verbatim:

```text
  Passed Cleanup_CalledTwice_InvokesParentCleanupOnce [< 1 ms]
  Passed Cleanup_CalledTwice_InvokesParentCleanupOnce [< 1 ms]
  Passed Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted [7 ms]
```

`Cleanup_CalledTwice_InvokesParentCleanupOnce` appears twice because the name is carried by two
distinct test classes — one in `QfcHomeControllerCleanupTests` and one in
`EfcHomeControllerLifecycleTests`. A `Select-String -SimpleMatch` search of the captured output for
that name returned exactly 2 matches, both prefixed `Passed`, and a search for the third name
returned exactly 1 match, also prefixed `Passed`. The whole run recorded 0 occurrences of `Failed:`
and 0 of `Test Run Failed.`

## What the transition demonstrates

Each of the three tests failed on the same observable before the fix — the parent-cleanup delegate
invoked **2 times** where the invariant permits **1** — and each passes after it. The change between
the two runs is exactly the read-into-local-then-clear idiom applied at
`QuickFiler/Controllers/QfcHomeController.cs` lines 405-407 and
`QuickFiler/Controllers/EfcHomeController.cs` lines 349-351. A fix whose test passed both before and
after would have demonstrated nothing; these three did not.

Output Summary: all three tests that failed in `[P1-T7]` are reported **passed** in the `[P6-T5]` run.
The fail-before record is cited above by full path and lies under
`evidence/regression-testing/` as AC6 requires.
