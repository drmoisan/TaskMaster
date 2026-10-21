# P5-T9 — Scope Gate (Issue #797)

Timestamp: 2026-09-07T10-12

```powershell
$BaseSha = (Select-String -Path 'docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-base-sha.2026-09-06T22-00.md' -Pattern '^BASE-SHA: ([0-9a-f]{40})$').Matches[0].Groups[1].Value
git add --all -- '*.cs' '*.csproj'
git diff --name-status $BaseSha -- '*.cs' '*.csproj'
git status --porcelain --untracked-files=all -- '*.cs' '*.csproj'
```

EXIT_CODE: 0

## ANCHORED-DIFF-LISTING

```text
M	TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs
M	TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs
M	TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs
M	UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs
M	UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs
A	UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
M	UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs
A	UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs
M	UtilitiesCS.Test/UtilitiesCS.Test.csproj
A	UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs
M	UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
A	UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
M	UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
M	UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs
M	UtilitiesCS/UtilitiesCS.csproj
```

## PORCELAIN-LISTING

```text
M  TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs
M  TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs
M  TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs
M  UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs
M  UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs
A  UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
M  UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs
A  UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs
M  UtilitiesCS.Test/UtilitiesCS.Test.csproj
A  UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs
M  UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
A  UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
M  UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
M  UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs
M  UtilitiesCS/UtilitiesCS.csproj
```

The porcelain listing is taken after the staging command, so it overlaps the anchored diff listing
rather than complementing it: every path appears in both. The two mechanisms are complementary in
general and each alone is wrong in one state — an anchored diff enumerates tracked changes only and
would be blind to a newly created file, while a porcelain listing goes empty once the change is
committed — so both are taken and their union is the working set.

## Working set, and the subtraction

The union of the two listings is the fifteen paths above.

The Phase 0 scope-baseline sets are subtracted. `SCOPE-BASELINE-COMMITTED:` held five feature-folder
and promotion documents, and `SCOPE-BASELINE-WORKTREE:` held five agent-memory residuals, this plan
file and the Phase 0 evidence artifacts. Neither set contains any `.cs` or `.csproj` path, so the
subtraction removes nothing and the remaining working set is the same fifteen paths.

Every remaining path is a member of the Write Set. Mapping them:

- Production, modified: the store-loading partial, the junk-folders partial, the serializer, the store
  wrapper, and the store wrapper controller — five paths, all claimed.
- Production, created: the junk-folder sink interface and the controller display partial — two paths,
  both claimed.
- Tests, modified: the button-and-populate partial, the store controller tests, the store wrapper
  tests, and the application-globals coverage tests — four paths, all claimed.
- Tests, created: the serializer guard tests and the controller display test partial — two paths, both
  claimed.
- Project compile-entry carriers, modified: the UtilitiesCS project file and the UtilitiesCS test
  project file — two paths, both claimed.

The test is a subset test, not an equality test. TaskMaster.Test/TaskMaster.Test.csproj is claimed in
the Write Set but ends the change unmodified, because the AC1 tests were appended to the
already-registered AppOlObjectsCoverageTests.cs rather than placed in a new file. Its absence from the
listings therefore does not fail this gate. TaskMaster/TaskMaster.csproj needed no compile-entry change
either, because both files receiving the AC1 and AC5 production edits are already registered in it.

## Explicit scope constraints, confirmed separately

- Paths under the dot-claude, dot-codex or dot-agents trees in the working set: zero.
- Paths under the config directory in the working set: zero.
- GitHub workflow files in the working set: zero.
- Files at the repository root in the working set: zero. In particular the solution file and both
  repository-root build property files are untouched.
- Files with an extension of resx, config, props or targets in the working set: zero.

## Why the pathspec restricts the gate

The pathspec restricts both commands to source and project files because this change also writes
feature-folder documents and evidence artifacts by design: the specification and the issue document
for acceptance-criteria check-off, this plan file for task check-off, and the timestamp-named evidence
artifacts. Those are excluded from the Write Set by spec.md's stated convention and are enumerated in
the plan's own prose. Including them here would report them as scope violations when they are
deliberate. The session helper and the coverage outputs do not appear in either listing because the
coverage directory is git-ignored.

Output Summary: The working set is fifteen source and project paths, every one of them a member of the
Write Set, with no path under the dot-claude, dot-codex, dot-agents or config trees, no GitHub
workflow file, and no repository-root file.
