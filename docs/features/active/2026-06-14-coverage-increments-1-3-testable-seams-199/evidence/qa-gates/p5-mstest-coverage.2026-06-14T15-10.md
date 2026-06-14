# P5-T10 — MSTest + Coverage (ToDoModel.Test + TaskMaster.Test)

- Timestamp: 2026-06-14T15-10
- Command: `vstest.console.exe ToDoModel.Test\bin\Debug\ToDoModel.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /EnableCodeCoverage /Settings:scripts\vscode\TaskMaster.cli.runsettings`
- EXIT_CODE: 0

## Output Summary

PASS. Total tests: 185; Passed: 185; Failed: 0; total time 3.90 s. `/InIsolation` is required for
these Moq-based assemblies (otherwise Moq fails to load `System.Threading.Tasks.Extensions 4.2.0.1`).

All 12 Phase 5 tests passed:
- `ProjectEntryDialogBranchesTests` (3): `SetProjectId_MalformedId_ShowsErrorDialogAndReturnsFalse`,
  `CompareTo_EqualOrdinalThenShorterOtherLength_ReturnsNegativeOne`,
  `CompareTo_EqualOrdinalThenLongerOtherLength_ReturnsPositiveOne`.
- `AppFileSystemFolderPathsMatchBestSpecialFolderTests` (9): positive, longest-match,
  case-mismatch, trailing-separator, no-match, null-collection, empty-collection, empty-path,
  null-path-throws.

## Coverage headline (scoped run: ToDoModel.Test + TaskMaster.Test)

The raw `.coverage` was merged to cobertura at the gitignored
`artifacts/csharp/p5-coverage.cobertura.xml` (not stored under feature evidence per evidence
hygiene). Seam-specific covered source lines (post-Phase-5):

| Seam | Source lines | Covered/Total | Note |
|---|---|---|---|
| `AppFileSystemFolderPaths.MatchBestSpecialFolder` (instance + new static helper) | 57-91 | 10/13 | pure static helper body fully covered; 3 uncovered are the delegation/comment lines the unit tests bypass |
| `ProjectEntry.SetProjectId` | 102-139 | 21/26 | malformed-ID + dialog-free branches covered |
| `ProjectEntry.ChangeId` | 141-170 | 0/28 | NOT covered — change-confirmation flag-and-stop (raw un-seamed MessageBox in the ProjectID property setter; see evidence/other/p5-projectentry-changeconfirm-gap) |
| `ProjectEntry.CompareTo(IProjectEntry)` | 182-209 | 22/22 | fully covered incl. length tie-break |

The new test classes themselves are 100% covered
(`ProjectEntryDialogBranchesTests` 80/80, `AppFileSystemFolderPathsMatchBestSpecialFolderTests`
148/148 block-lines).

Note: the cobertura `overall line-rate` (0.127) is not the feature's production-only rate — this
scoped run exercised only two of the test projects against all instrumented production assemblies.
The production-only rate comparison is computed by the first-party denominator method in P5-T11
relative to the prior #199 state.
