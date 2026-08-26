# CR-2 Pass-After and Router-Agreement Proof (P3-T4) — remediation cycle 1, issue #614

Timestamp: 2026-08-26T22-02

Command (1 of 2):
`& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`

Command (2 of 2):
`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests|FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests|FullyQualifiedName~BreadcrumbBridgeRouterIssue614Tests|FullyQualifiedName~BreadcrumbBridgeRouterTests" "/Logger:trx;LogFileName=p3-t4.trx" "/ResultsDirectory:coverage\trx\p3-t4"`

EXIT_CODE: 0 (both commands)

## Output Summary

`Test Run Successful.` — `Total tests: 62`, `Passed: 62`, `Failed: 0`, `Skipped: 0`. Lines matching
`^  Failed `: **0**.

The scope covers the filing guard and all three breadcrumb router test classes, which is the
agreement surface CR-2 names.

### Explicitly named results

| Test | Result | Significance |
| --- | --- | --- |
| `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` | **Passed** | The router still passes the rooted under-root value `\aRcHiVe\Clients\North` through verbatim; the file was NOT edited. |
| `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` | **Passed** | In its P3-T4-corrected form from the delivery plan; unchanged by this cycle. |
| `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` | **Passed** | CR-2 fixed: the guard now admits exactly the value the router admits. |
| `IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted` | **Passed** | Recorded root-exact consequence realised. |
| `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` | **Passed** | CR-1 fix still holds after the CR-2 restructure. |
| `IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted` | **Passed** | CR-1 fix still holds after the CR-2 restructure. |

### D1 / D4 / D9 protection verified NOT weakened

The CR-2 fix narrows over-rejection only. Each of these values is still rejected, and each is proved
by a named passing test in this run:

| Value | Root | Verdict | Test |
| --- | --- | --- | --- |
| `\\mailbox@example.com` (store root) | `\Archive` | rejected | `IsValidFilingSelection_StoreRootedSelection_IsRejected` |
| `\External\Clients` (above root) | `\Archive` | rejected | `IsValidFilingSelection_RootedTargetAboveArchiveRoot_IsRejected` |
| `\\other-mailbox@example.com\Archive\Clients` (cross-store) | `\Archive` | rejected | `IsValidFilingSelection_CrossStoreRootedTarget_IsRejected` |
| `\Archive2\Clients` (separator-boundary near miss) | `\Archive` | rejected | `IsValidFilingSelection_SeparatorBoundaryNearMiss_IsRejected` |
| `\Archive\Clients` | `string.Empty` (degrade path) | rejected | `IsValidFilingSelection_RootedTargetWithUnavailableRoot_IsRejected` |
| `\Archive\Clients` | `null` | rejected | `IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected` |
| `C:\Users\testuser\OneDrive - Contoso` (drive-rooted) | `\Archive` | rejected | `IsValidFilingSelection_DriveRootedSelection_IsRejected` |

Raw TRX was written to the gitignored `coverage\trx\p3-t4\` tree, not under `evidence/`.
