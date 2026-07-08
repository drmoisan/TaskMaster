# P5-T12 — Production-Change Boundary / Invariant Check

- Timestamp: 2026-06-14T15-10
- Command: `git merge-base origin/main HEAD` then `git diff --name-only <merge-base> HEAD` plus `git status --short` (working tree Phase 5 changes), and `git diff` content inspection of the production files
- EXIT_CODE: 0

## Merge base

`d436a06f10240361ef4470d9477e31396b572db4`

## Phase 5 production-file changes (working tree)

Exactly two production files changed, both authorized:

1. `UtilitiesCS/Properties/AssemblyInfo.cs` — single added line:
   `[assembly: InternalsVisibleTo("ToDoModel.Test")]`. No `MyBox` member visibility or behavior change.
2. `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` — `MatchBestSpecialFolder` pure-helper
   extraction: added `using System.Collections.Generic;`; the instance method now delegates
   (`return MatchBestSpecialFolder(SpecialFolders, path);`) to a new
   `internal static string MatchBestSpecialFolder(IReadOnlyDictionary<string,string>, string)`
   whose body is byte-for-byte the original matching logic (only the local reference renamed from
   `SpecialFolders` to the `specialFolders` parameter). No runtime behavior change.

`UtilitiesCS/Dialogs/MyBox.cs` was NOT modified (the existing settable internal `DialogInvoker`
seam sufficed; no third seam added).

## Test/config/docs changes (allowed)

- Test code: `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs` (new),
  `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` (new).
- Test-project Compile includes: `ToDoModel.Test/ToDoModel.Test.csproj`,
  `TaskMaster.Test/TaskMaster.Test.csproj` (added the two new test files; test-project config, not production).
- Docs/spec/plan/evidence under the feature folder.

## Forbidden-change checks

- `[ExcludeFromCodeCoverage]` added/removed in any production diff: NONE (grep of
  `git diff -- TaskMaster/ UtilitiesCS/ ToDoModel/ QuickFiler/` returned no match).
- `coverage.config` changed: NO.
- `TaskMaster.runsettings` changed: NO.
- Pipeline scripts (`scripts/vscode/*`, Koverage) changed: NO.

## Outcome

PASS: Phase 5 production changes are limited to the two maintainer-authorized seams
(`UtilitiesCS/Properties/AssemblyInfo.cs` attribute and `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`
pure-helper extraction). No `MyBox.cs` change, no `[ExcludeFromCodeCoverage]` change, no
coverage/config/pipeline change. No flag-and-stop boundary violation.
