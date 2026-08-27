# Pre-Change Facts — remediation cycle 2

Timestamp: 2026-08-26T22-17

Command: `git rev-parse HEAD; git status --porcelain; (Get-Content <path>).Count; case-sensitive ordinal fixed-string count sweep`

EXIT_CODE: 0

Output Summary: HEAD and working-tree scope were captured; all six file counts and all 26 literal
gate counts match the cycle-2 plan's pre-change table.

## 1. Informational HEAD

`01e26f7cd76b0ed6105ab67546a2df37a2b45219`

This is recorded as an informational fact and is not compared to a hard-coded expected SHA.

## 2. Working-tree state

`git status --porcelain` contains only cycle-2 files under the authorized feature folder:

```text
 M docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/phase0-instructions-read.md
 M docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/remediation-plan.2026-08-26T22-12.md
?? docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/analyzer-build.2026-08-26T22-14.md
?? docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/format-check.2026-08-26T22-13.md
?? docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/full-suite-coverage.2026-08-26T22-16.md
?? docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/remediation-baseline/nullable-build.2026-08-26T22-15.md
```

## 3. Baseline line counts

| File | Expected | Measured |
| --- | ---: | ---: |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | 147 | 147 |
| `QuickFiler/Controllers/EfcFormController.cs` | 1079 | 1079 |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | 316 | 316 |
| `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` | 453 | 453 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 596 | 596 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 694 | 694 |

## 4. Case-sensitive literal-gate verification

| Token | Scope | Expected | Measured |
| --- | --- | ---: | ---: |
| `IsValidFilingSelection(selectedFolder, archiveRoot)` | `EfcFormController.cs` | 1 | 1 |
| `IsValidFilingSelection(selectedFolder)` | `EfcFormController.cs` | 0 | 0 |
| `D6-validated` | `EfcFormController.cs` | 1 | 1 |
| `message => logger.Error(message)` | `EfcFormController.cs` | 1 | 1 |
| `archiveRoot` | `EfcFormController.cs` | 2 | 2 |
| `archiveRoot` | `EfcSelectionGuard.cs` | 5 | 5 |
| `IsValidFilingSelection(string? selection)` | `EfcSelectionGuard.cs` | 0 | 0 |
| `TryMakeArchiveRelative` | `EfcSelectionGuard.cs` | 1 | 1 |
| `InvalidOperationException` | `EfcSelectionGuard.cs` | 1 | 1 |
| `ResolveArchiveRootOrEmpty` | tracked `*.cs` | 8 | 8 |
| `RootUnavailableDiagnostic` | tracked `*.cs` | 3 | 3 |
| `MinimumCreationLength` | `EfcSelectionGuard.cs` | 2 | 2 |
| `IsValidCreationSelection` | `EfcSelectionGuard.cs` | 2 | 2 |
| `IsValidCreationSelection(SelectedFolder)` | `EfcFormController.cs` | 1 | 1 |
| `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` | tracked `*.cs` | 1 | 1 |
| `IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted` | tracked `*.cs` | 1 | 1 |
| `IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected` | tracked `*.cs` | 1 | 1 |
| `IsValidFilingSelection_RootedTargetWithUnavailableRoot_IsRejected` | tracked `*.cs` | 1 | 1 |
| `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsRejected` | tracked `*.cs` | 0 | 0 |
| `IsValidFilingSelection_ArchiveRootExactTarget_IsRejected` | tracked `*.cs` | 0 | 0 |
| `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` | tracked `*.cs` | 0 | 0 |
| `GetStem_FolderPathOutsideAncestor_ReturnsInputTrimmedOfLeadingSeparators` | tracked `*.cs` | 0 | 0 |
| `share one predicate` | `spec.md` | 1 | 1 |
| `two scope-specific predicates` | `spec.md` | 0 | 0 |
| `- [x] **AC16` | `spec.md` | 1 | 1 |
| `- [ ] **AC16` | `spec.md` | 0 | 0 |

Verdict: PASS. Every pre-change premise matches the plan table.
