# CR-2 Fail-Before Proof (P3-T2) — remediation cycle 1, issue #614

This artifact records exactly one gate: the CR-2 `[expect-fail]` run. No other gate is recorded here.

Timestamp: 2026-08-26T21-56

Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests" "/Logger:trx;LogFileName=p3-t2.trx" "/ResultsDirectory:coverage\trx\p3-t2"`

(Preceded by `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, exit code 0 — so the two failures below are ASSERTION failures, not compile failures.)

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

`Test Run Failed.` — `Total tests: 23`, `Passed: 21`, `Failed: 2`, `Skipped: 0`.

**Exactly the two P3-T1 regression tests failed. Every other `EfcSelectionGuardTests` test passed**,
including both P2-T1 CR-1 regressions, which were still green.

| Test | Result |
| --- | --- |
| `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` | **Failed** (P3-T1 CR-2 regression) |
| `IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted` | **Failed** (P3-T1 CR-2 regression) |
| all 21 other `EfcSelectionGuardTests` tests | Passed |

## Failure messages (verbatim, redaction-safe)

```
Failed IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted
  Error Message:
   Expected EfcSelectionGuard.IsValidFilingSelection(@"\aRcHiVe\Clients\North", @"\Archive")
   to be True because a rooted target under the archive root resolves and is selectable,
   but found False.

Failed IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted
  Error Message:
   Expected EfcSelectionGuard.IsValidFilingSelection(@"\Archive", @"\Archive") to be True
   because the archive root resolves against itself, but found False.
```

This is the CR-2 defect reproduced: the filing predicate rejects rootedness as such, so a value that
`BreadcrumbBridgeRouter.SelectRow` deliberately admits verbatim (the case-insensitive under-root
value pinned by `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`) is
selectable but unfilable.

Raw TRX was written to the gitignored `coverage\trx\p3-t2\` tree, not under `evidence/`.
