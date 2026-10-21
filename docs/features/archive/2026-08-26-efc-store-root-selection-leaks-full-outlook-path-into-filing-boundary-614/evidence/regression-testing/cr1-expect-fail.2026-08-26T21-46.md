# CR-1 Fail-Before Proof (P2-T2) — remediation cycle 1, issue #614

This artifact records exactly one gate: the CR-1 `[expect-fail]` run. No other gate is recorded here.

Timestamp: 2026-08-26T21-46

Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests" "/Logger:trx;LogFileName=p2-t2.trx" "/ResultsDirectory:coverage\trx\p2-t2"`

(Preceded by `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, exit code 0 — so the two failures below are ASSERTION failures, not compile failures.)

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

`Test Run Failed.` — `Total tests: 21`, `Passed: 19`, `Failed: 2`, `Skipped: 0`.

**Exactly the two P2-T1 regression tests failed. Every other `EfcSelectionGuardTests` test passed.**

| Test | Result |
| --- | --- |
| `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` | **Failed** (P2-T1 CR-1 regression) |
| `IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted` | **Failed** (P2-T1 CR-1 regression) |
| `IsValidFilingSelection_NullSelection_IsRejected` | Passed |
| `IsValidFilingSelection_EmptySelection_IsRejected` | Passed |
| `IsValidFilingSelection_WhitespaceSelection_IsRejected` | Passed |
| `IsValidFilingSelection_BannerSentinel_IsRejected` | Passed |
| `IsValidFilingSelection_StoreRootedSelection_IsRejected` | Passed |
| `IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected` | Passed |
| `IsValidFilingSelection_DriveRootedSelection_IsRejected` | Passed |
| `IsValidFilingSelection_ValidRelativeStem_IsAccepted` | Passed |
| `IsValidCreationSelection_NullSelection_IsRejected` | Passed |
| `IsValidCreationSelection_EmptySelection_IsRejected` | Passed |
| `IsValidCreationSelection_WhitespaceSelection_IsRejected` | Passed |
| `IsValidCreationSelection_BannerSentinel_IsRejected` | Passed |
| `IsValidCreationSelection_TwoCharacterSelection_IsRejected` | Passed |
| `IsValidCreationSelection_SingleCharacterSelection_IsRejected` | Passed |
| `IsValidCreationSelection_MinimumLengthSelection_IsAccepted` | Passed |
| `IsValidCreationSelection_RootedSelection_IsRejected` | Passed |
| `IsValidCreationSelection_ValidRelativeStem_IsAccepted` | Passed |
| `ResolveArchiveRootOrEmpty_AccessorSucceeds_ReturnsRootAndLogsNothing` | Passed |
| `ResolveArchiveRootOrEmpty_AccessorThrowsInvalidOperation_DegradesToEmpty` | Passed |

## Failure messages (verbatim, redaction-safe)

```
Failed IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted
  Error Message:
   Expected EfcSelectionGuard.IsValidFilingSelection(name, @"\Archive") to be True because
   filing to the archive folder 'HR' must remain possible, but found False.

Failed IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted
  Error Message:
   Expected EfcSelectionGuard.IsValidFilingSelection("A", @"\Archive") to be True because
   filing to the archive folder 'A' must remain possible, but found False.
```

Both failures land in `FluentAssertions.Primitives.BooleanAssertions.BeTrue`, confirming an
assertion failure against a compiled, running predicate. This is the CR-1 defect reproduced: the
filing predicate's minimum-length conjunct rejects a legitimate two-character and one-character
archive folder name.

Raw TRX was written to the gitignored `coverage\trx\p2-t2\` tree, not under `evidence/`.
