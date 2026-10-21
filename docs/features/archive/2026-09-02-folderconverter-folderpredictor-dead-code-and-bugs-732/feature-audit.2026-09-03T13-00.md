# Feature Audit — folderconverter-folderpredictor-dead-code-and-bugs (#732)

- Timestamp: 2026-09-03T13-00
- Work Mode: `full-bug` (issue.md line 12) — AC source: `spec.md` only, per acceptance-criteria-tracking
- AC location: `spec.md` `## Acceptance Criteria`, lines 184-193 (10 checkbox items)

## Acceptance Criteria Evaluation

| # | Criterion (abridged) | Verdict | Evidence |
|---|---|---|---|
| AC1 | `UtilitiesCS/EmailIntelligence/FolderConverter.cs` no longer exists; no `.csproj` references it | **PASS** | File absent from `git diff --name-status` (shown as `D`) and from the current worktree tree. Grep of every `*.csproj` for `EmailIntelligence\FolderConverter.cs` / `EmailIntelligence/FolderConverter.cs`: zero matches (independently re-run; matches `p3-t1`/`p3-t4`). |
| AC2 | `UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs` no longer exists; `UtilitiesCS.Test.csproj` has no compile-include for it | **PASS** | File deleted (`D` in diff). Grep of `UtilitiesCS.Test.csproj` for `OutlookExtensions\FolderConverter_Tests.cs`: zero matches. |
| AC3 | Live `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` and its public members unchanged, its test suite still passing | **PASS** | `git diff --name-only origin/main..HEAD -- UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs`: empty. `p3-t6-*.md`: 22/22 `FolderConverterTests` pass. |
| AC4 | `FolderPredictor.cs:691` uses `||` (not `|`) and guards `parentBranchPath.Length` before indexing `[0]` | **PASS** | Direct read of the file at HEAD confirms the exact text (see code-review.md diff excerpt): `olAncestor.EndsWith("\\", StringComparison.Ordinal) \|\| (parentBranchPath.Length > 0 && parentBranchPath[0] == '\\')`. |
| AC5 | New regression test invokes `CreateFolder` with empty `parentBranchPath`, asserts no `IndexOutOfRangeException`; written RED-first, confirmed failing before the fix, passing after | **PASS** | `CreateFolder_WhenParentBranchPathIsEmpty_DoesNotThrowIndexOutOfRangeException` present in `FolderPredictorTests.cs`. RED: `p1-t4-*.md` (EXIT_CODE 1, Failed:1, `IndexOutOfRangeException` at line 691, captured against pre-fix code). GREEN: `p2-t4-*.md` (EXIT_CODE 0, Passed:1, same filter, captured against post-fix code). |
| AC6 | Three pre-existing `CreateFolder` tests continue passing unmodified | **PASS** | Diff shows only one new test method inserted; no existing test body altered. `p2-t5-*.md`: 3/3 pass (`CreateFolder_WhenParentBranchStartsWithSeparator_UsesCombinedPathWithoutDoubleSlash`, `InjectedDirectory_CreateFolder_WhenPromptSuppliesName_CreatesFolderAndDirectoryPath`, `CreateFolder_WhenAncestorIsNull_UsesArchiveRootAndCreatesFolder`). |
| AC7 | `AppFileSystemFolderPaths.cs`'s `MatchBestSpecialFolder` and its XML doc comment unchanged; its dedicated test file unchanged and passing | **PASS** | `git diff --name-only origin/main..HEAD -- TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs`: empty (independently re-run; matches `p5-t7`). `p5-t8-*.md`: 9/9 tests pass. Doc-comment text independently re-read at HEAD, still states ordinal `string.Contains` substring semantics (`p4-t2`). |
| AC8 | No production caller of `MatchBestSpecialFolder` introduced/discovered | **PASS** | Repo-wide grep for `MatchBestSpecialFolder` outside test/doc paths matches only the method's own definition/delegation in `AppFileSystemFolderPaths.cs` and the interface declaration in `IFileSystemFolderPaths.cs`. Additional test-double stubs of the interface member in unrelated test-support files are not production callers of the actual implementation (see code-review.md). |
| AC9 | Full C# toolchain (CSharpier, analyzers, nullable, MSTest+coverage) passes clean in a single pass | **PASS** | See policy-audit.md Toolchain table: all four stages EXIT_CODE 0 in the recorded final pass (`p5-t1` through `p5-t5`), with genuine forced-rebuild proof (absence of `Skipping target "CoreCompile"`) for the analyzer and nullable stages. |
| AC10 | No file under `UtilitiesCS/Threading/**` or `UtilitiesCS/To Depricate/FileIO2.cs` modified | **PASS** | Independently confirmed via full `git diff --name-status origin/main..HEAD`: no path begins with `UtilitiesCS/Threading/` and no path equals `UtilitiesCS/To Depricate/FileIO2.cs`. Matches `p5-t6-*.md`. |

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/spec.md`
- Total AC items: 10
- Checked off (delivered): 10 (all `- [x]` in spec.md at HEAD, independently confirmed by direct read; no reviewer check-off action was needed)
- Remaining (unchecked): 0
- Items remaining: none

## Scope-Boundary Verification (independent, not delegated to the executor's own claim)

- `git diff --name-status origin/main..HEAD` lists exactly 4 source files (2 deletions, 2 modifications), all inside spec.md's Write Set, plus the feature-folder documentation/evidence tree. No file outside the Write Set was modified.
- No file under `UtilitiesCS/Threading/**` appears in the diff.
- No file at `UtilitiesCS/To Depricate/FileIO2.cs` appears in the diff.
- The live `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`, its test suite, `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`, and `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` are all confirmed byte-for-byte unchanged from origin/main.
- The dead caller `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` (cited in spec.md as out of scope) is likewise absent from the diff.

## Baseline/Final Test-Count Reconciliation (independent)

- Baseline (`p0-t10`): "Total tests: 4785 Passed: 4785" — Cobertura root `line-rate="0.7072916597091885"` (independently re-parsed from `evidence/baseline/coverage-baseline.cobertura.xml`, exact match).
- Final (`p5-t5`): "Total tests: 4786 Passed: 4786" — Cobertura root `line-rate="0.7073783191750814"` (independently re-parsed from `evidence/qa-gates/coverage-final.cobertura.xml`, exact match).
- Net: +1 test (the new regression test), 0 failures at either point, coverage line-rate improved by +0.0086 points (no regression). See policy-audit.md for the full coverage-tier breakdown, including the two non-blocking findings.

## Overall Verdict

**PASS. 0 blocking findings.** All 10 acceptance criteria in spec.md are independently verified true against the actual diff, the actual build/test evidence, and the actual committed Cobertura XML data (not merely the executor's prose summaries). Two non-blocking findings are carried in code-review.md (a stale line-budget evidence claim, and the repo's known pre-existing sub-floor coverage condition); neither affects any AC verdict above and neither requires a remediation cycle.
