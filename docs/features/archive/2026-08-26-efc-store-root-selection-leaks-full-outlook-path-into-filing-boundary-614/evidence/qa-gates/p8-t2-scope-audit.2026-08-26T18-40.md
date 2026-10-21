# P8-T2 - Scope-isolation audit (#614; AC25 scope half)

Timestamp: 2026-08-26T18-40

Commands, run under `pwsh -NoProfile` as separate statements so PowerShell does not mis-parse the
revision range:

```
$base = git merge-base HEAD origin/main
git status --porcelain
git diff --name-only "$base"
git ls-files --others --exclude-standard
git diff --name-only HEAD
```

EXIT_CODE: 0 (all five statements)

- `git merge-base HEAD origin/main` resolved to `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`;
  EXIT_CODE 0.
- `git status --porcelain`: EXIT_CODE 0.
- `git diff --name-only "$base"`: EXIT_CODE 0. (The `<base>..HEAD` form is deliberately NOT used:
  it would be vacuous for uncommitted work. This form compares the working tree to the merge-base
  and therefore also sees the interim commits made on this branch.)
- `git ls-files --others --exclude-standard`: EXIT_CODE 0.
- `git diff --name-only HEAD`: EXIT_CODE 0.

## Output Summary

**Out-of-scope path count: 1.** Every other changed or added path is in the in-scope set. The one
exception is `QuickFiler.Test/packages.config`; it is analysed in its own section below.

### `git status --porcelain`

```
 M docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/regression-testing/p1-t4-producer-companion-fail-before.2026-08-26T16-05.md
?? docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/change-description.2026-08-26.md
```

### `git ls-files --others --exclude-standard`

```
docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/change-description.2026-08-26.md
```

### `git diff --name-only "$base"` - full path list, classified

Production source (the seven files named in the spec, plus the three new files):

```
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
QuickFiler/Controllers/EfcDataModel.cs
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/EfcSelectionGuard.cs                 (new, in-scope)
TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs
TaskMaster/AppGlobals/AppOlObjects.cs
TaskMaster/AppGlobals/ArchiveRootPathGuard.cs               (new, optional, in-scope)
UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs
UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs    (new, in-scope)
UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs
```

Project files (the five named, plus `TaskMaster/TaskMaster.csproj`, which the plan permits
because the optional `ArchiveRootPathGuard.cs` was created):

```
QuickFiler/QuickFiler.csproj
QuickFiler.Test/QuickFiler.Test.csproj
TaskMaster/TaskMaster.csproj
TaskMaster.Test/TaskMaster.Test.csproj
UtilitiesCS/UtilitiesCS.csproj
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

Test files (new and edited, all named by the plan):

```
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs   (P3-T4 spec correction)
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs   (new)
QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs           (P1-T3 AC18 test)
QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs             (new)
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs                (new)
TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs (new)
TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs (new)
UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs
UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs   (new)
UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs (new)
UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs       (P5-T4 :329 correction)
```

Feature folder (`<FEATURE>/**`, in-scope): 17 paths under
`docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/`
(`issue.md`, `spec.md`, `plan.2026-08-26T09-59.md`, the research artifact, six
`evidence/baseline/` artifacts, and eight `evidence/regression-testing/` artifacts).

Agent memory (`.claude/agent-memory/**`, in-scope): 9 paths across the `atomic-executor`,
`atomic-planner`, `prd-feature` and `task-researcher` folders. These were written by the planning
and research agents earlier on this branch, ahead of the merge-base comparison; see the
`git diff --name-only HEAD` check below.

Pre-plan branch state (the six allowlisted paths, in-scope only as pre-existing branch state):

```
.gitignore
docs/features/potential/promoted/2026-08-26-analyzer-include-paths-skewed-from-packages-config-masked-by-ci-cache.md
docs/features/potential/promoted/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary.md
docs/features/potential/promoted/2026-08-26-folderpredictor-createfolder-non-shortcircuit-or-indexes-empty-path.md
docs/features/potential/promoted/2026-08-26-matchbestspecialfolder-substring-matching-codified-by-tests.md
docs/features/potential/promoted/2026-08-26-orphaned-duplicate-folderconverter-dead-file-with-always-false-guards.md
```

### `git diff --name-only HEAD` - proof that the six allowlisted paths are untouched by this change

```
docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/regression-testing/p1-t4-producer-companion-fail-before.2026-08-26T16-05.md
```

None of the six pre-plan paths appears in this output, which proves this change did not modify any
of them; they are reported by the merge-base diff only because the promotion, research, spec and
chore commits already on this branch added them ahead of the merge-base.

## Explicit non-modification checks (spec AC25)

- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` appears in NO diff output. Verified
  against `git status --porcelain`, `git diff --name-only "$base"`,
  `git ls-files --others --exclude-standard`, and `git diff --name-only HEAD`: zero occurrences in
  all four.
- `UtilitiesCS/EmailIntelligence/FolderConverter.cs` (the uncompiled duplicate) appears in NO diff
  output. Zero occurrences across all four commands. Note that the file that IS modified is the
  compiled `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`, a different path.
- `AppFileSystemFolderPaths.MatchBestSpecialFolder` is not modified: `git diff -U0 --` for
  `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` produces hunks at old lines 14, 20, 150, 197,
  203 and 206-246 only. None intersects the method's `:77-91` range. Its untouched test class
  `AppFileSystemFolderPathsMatchBestSpecialFolderTests` is green (9 passed, P7-T5 artifact).

## The one out-of-scope path: `QuickFiler.Test/packages.config`

**Status: recorded exception, flagged for orchestrator review. Not silently accepted.**

What changed: a single added line pinning `log4net` at version `3.3.2` for `net481` - the same
version every other project in the solution already pins.

Why it was necessary: spec AC2 requires "A unit test asserts that activating a segment not at or
under the archive root leaves `SelectedFolderPath` unchanged **and emits a diagnostic**", and AC3
extends that to the store-root, cross-store and at-or-above-root activations. The router emits its
diagnostic through log4net. Asserting it requires the established repository pattern - a log4net
`MemoryAppender` attached to the target type's logger, as used by
`TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs` - which requires a compile-time reference to
log4net. `QuickFiler.Test` had none. These are non-SDK projects: a `ProjectReference` does not flow
package references to the compiler, so a reference had to be added to `QuickFiler.Test` itself.

Why `packages.config` and not only the `.csproj`: the plan's in-scope list names
`QuickFiler.Test/QuickFiler.Test.csproj`, which carries the `<Reference>` item and its `HintPath`.
Under packages.config-style restore, the `.csproj` `HintPath` and the `packages.config` pin are two
halves of one declaration; adding the reference without the pin would make `QuickFiler.Test`'s
restore depend on another project happening to pin the same package, which is exactly the
`HintPath`-versus-`packages.config` divergence class that produced the pre-existing analyzer-skew
defect tracked as issue #615. The `packages.config` edit is therefore the mechanically necessary
companion of an explicitly allowlisted `.csproj` edit, and is judged a plan-list omission rather
than a scope expansion: no new capability, no new package version, no behaviour change.

Blast radius: `QuickFiler.Test` only. No production assembly is affected. The solution builds with
EXIT_CODE 0 and no new MSB3277 or CS0006 diagnostic.
