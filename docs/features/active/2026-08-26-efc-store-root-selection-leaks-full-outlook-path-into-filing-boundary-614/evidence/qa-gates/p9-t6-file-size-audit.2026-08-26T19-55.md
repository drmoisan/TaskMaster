# P9-T6 - Post-format file-size and scope re-audit (#614; AC25)

Timestamp: 2026-08-26T19-55

This audit was run AFTER the final formatting pass of the clean Phase 9 loop, so every count below
is a post-format count.

EXIT_CODE: 0 (all four re-audit statements)

## Section 1 - line count of every file created or edited by this change

### New files (gate: at or under 500 lines)

| File | Baseline | Post-change | Gate |
| --- | ---: | ---: | --- |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` | n/a (new) | **147** | PASS |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | n/a (new) | **38** | PASS |
| `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` | n/a (new) | **62** | PASS |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs` | n/a (new) | **335** | PASS |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs` | n/a (new) | **335** | PASS |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs` | n/a (new) | **358** | PASS |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | n/a (new) | **87** | PASS |
| `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` | n/a (new) | **123** | PASS |
| `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` | n/a (new) | **133** | PASS |
| `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs` | n/a (new) | **128** | PASS |

### Edited files whose baseline is at or under 500 lines (gate: at or under 500 lines)

| File | Baseline | Post-change | Gate |
| --- | ---: | ---: | --- |
| `QuickFiler/Controllers/EfcDataModel.cs` | 397 | **423** | PASS |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 448 | **467** | PASS |
| `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | 320 | **346** | PASS |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` | 239 | **263** | PASS |
| `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` | 244 | **358** | PASS |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | 435 | **481** | PASS |
| `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` | 337 | **453** | PASS |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs` | 446 | **446** | PASS |

### Pre-existing over-limit files (gate: net non-growth, at or below the recorded baseline)

| File | Baseline | Post-change | Gate |
| --- | ---: | ---: | --- |
| `QuickFiler/Controllers/EfcFormController.cs` | 1084 | **1072** | PASS (-12) |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 596 | **596** | PASS (0, exactly at baseline) |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 694 | **694** | PASS (0, exactly at baseline) |

The third file was added to the net-non-growth handling by execution delta E1, AFTER the
orchestrator's 2026-08-26T10:38:00Z adjudication of the first two, and is applied under the same
ratified reading. It is edited only by P3-T4, which replaces three single-line statements with three
single-line statements and rewrites a two-line comment in place. This is flagged explicitly for
orchestrator review in the AC25 check-off.

Note on `BreadcrumbBridgeRouter.cs`: holding it at exactly its 596-line baseline while adding three
contract guards required extracting the shared six-line selection-commit tail into a private
`CommitSelection` helper and deleting the superseded `ToArchiveRelativePath` method. That is the
same net-non-growth technique the plan's own AC25 interpretation prescribes for `EfcFormController.cs`.

## Section 2 - scope re-audit

Statements, run under `pwsh -NoProfile` as separate statements:

```
$base = git merge-base HEAD origin/main
git status --porcelain
git diff --name-only "$base"
git ls-files --others --exclude-standard
git diff --name-only HEAD
```

The `<base>..HEAD` form was NOT used; it is vacuous for uncommitted work.

- `git merge-base HEAD origin/main` = `c279d40bddacdba00c29a9724d1b5b17f9ebbc90` (unchanged from P8-T2).
- `git diff --name-only "$base"` reports **62 paths**, compared with 62 at P8-T2. The path set is
  **unchanged apart from evidence additions**: the nine `evidence/qa-gates/` artifacts and
  `change-description.2026-08-26.md` are new untracked additions, and two already-in-scope test
  files (`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs`,
  `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs`) were further edited by the
  P9-T5 coverage remediation. No new production, project, or third-party path appeared.
- **Out-of-scope path count: 1**, unchanged from P8-T2: `QuickFiler.Test/packages.config`. Its
  justification as the mechanically necessary companion of an allowlisted `.csproj` edit is
  recorded in full in `p8-t2-scope-audit.2026-08-26T18-40.md` and is flagged for orchestrator
  review.
- The six pre-plan branch paths allowlisted at P8-T2 (`.gitignore` plus the five
  `docs/features/potential/promoted/2026-08-26-*.md` files) appear in the merge-base diff for the
  same pre-existing reason, and **none of them appears in `git diff --name-only HEAD`**, which
  re-confirms this change did not modify any of them.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` and
  `UtilitiesCS/EmailIntelligence/FolderConverter.cs` appear in NO diff output: zero occurrences
  across all four statements.
