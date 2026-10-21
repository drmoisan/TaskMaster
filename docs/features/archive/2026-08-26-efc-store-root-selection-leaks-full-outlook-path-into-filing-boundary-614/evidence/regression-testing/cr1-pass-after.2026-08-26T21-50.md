# CR-1 Pass-After Proof (P2-T4) — remediation cycle 1, issue #614

Timestamp: 2026-08-26T21-50

Command (1 of 2):
`& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`

Command (2 of 2):
`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests" "/Logger:trx;LogFileName=p2-t4.trx" "/ResultsDirectory:coverage\trx\p2-t4"`

EXIT_CODE: 0 (both commands)

## Output Summary

`Test Run Successful.` — `Total tests: 21`, `Passed: 21`, `Failed: 0`, `Skipped: 0`. The runner
printed 21 `Passed` lines, so every test in the class passed.

The same 21-test set that produced 2 failures at P2-T2 now produces 0. The only change between the
two runs is P2-T3, which removed the minimum-length conjunct from `IsValidFilingSelection` and left
it in `IsValidCreationSelection`.

### Explicitly named results

| Test | Result | Significance |
| --- | --- | --- |
| `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` | **Passed** | CR-1 fixed: filing to `HR`, `IT`, `PR`, `QA`, `Q1` now succeeds. |
| `IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted` | **Passed** | CR-1 fixed: filing to a one-character folder now succeeds. |
| `IsValidCreationSelection_TwoCharacterSelection_IsRejected` | **Passed** | The creation path still rejects `"AB"` — the length rule was moved, not deleted. |

### Regression protection still intact after the CR-1 fix

| Test | Result |
| --- | --- |
| `IsValidFilingSelection_StoreRootedSelection_IsRejected` | Passed (D1/D9 store-root rejection intact) |
| `IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected` | Passed |
| `IsValidFilingSelection_DriveRootedSelection_IsRejected` | Passed |
| `IsValidFilingSelection_BannerSentinel_IsRejected` | Passed |
| `IsValidFilingSelection_NullSelection_IsRejected` | Passed |
| `IsValidFilingSelection_EmptySelection_IsRejected` | Passed |
| `IsValidFilingSelection_WhitespaceSelection_IsRejected` | Passed |
| `IsValidFilingSelection_ValidRelativeStem_IsAccepted` | Passed |
| `IsValidCreationSelection_SingleCharacterSelection_IsRejected` | Passed |
| `IsValidCreationSelection_MinimumLengthSelection_IsAccepted` | Passed |
| `IsValidCreationSelection_RootedSelection_IsRejected` | Passed |
| `IsValidCreationSelection_NullSelection_IsRejected` | Passed |
| `IsValidCreationSelection_EmptySelection_IsRejected` | Passed |
| `IsValidCreationSelection_WhitespaceSelection_IsRejected` | Passed |
| `IsValidCreationSelection_BannerSentinel_IsRejected` | Passed |
| `IsValidCreationSelection_ValidRelativeStem_IsAccepted` | Passed |
| `ResolveArchiveRootOrEmpty_AccessorSucceeds_ReturnsRootAndLogsNothing` | Passed |
| `ResolveArchiveRootOrEmpty_AccessorThrowsInvalidOperation_DegradesToEmpty` | Passed |

Raw TRX was written to the gitignored `coverage\trx\p2-t4\` tree, not under `evidence/`.
