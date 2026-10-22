# [P5-T3] Existing test files are unedited (Issue 638)

Timestamp: 2026-08-29T12-36

Command:

```
git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01 -- QuickFiler.Test TaskMaster.Test
git status --porcelain -uall -- QuickFiler.Test TaskMaster.Test
```

The anchored diff enumerates tracked modifications; the porcelain status is its required
companion because a name-listing diff is blind to files this plan created. Neither alone is
sufficient.

EXIT_CODE: 0

Output Summary:

## `git diff --name-only` output, verbatim

```
QuickFiler.Test/QuickFiler.Test.csproj
```

## `git status --porcelain -uall` output, verbatim

```
 M QuickFiler.Test/QuickFiler.Test.csproj
?? QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs
```

## Assessment

The union of the two outputs is exactly:

- `QuickFiler.Test/QuickFiler.Test.csproj` (modified — the single
  `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` entry added at `:116`
  by [P3-T2])
- `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` (untracked — created by
  [P3-T1])

Neither output names any of the six existing test files the spec protects:

- `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` — absent
- `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs` — absent
- `QuickFiler.Test/Controllers/EfcDataModelTests.cs` — absent
- `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` — absent
- `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` — absent
- `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` — absent

All six are therefore unmodified, and the two sentinel tests among them passed unchanged in
[P5-T2].
