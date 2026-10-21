# [P3-T5] Phase 3 format and builds

Timestamp: 2026-09-08T01-56

Command: `git status --porcelain --untracked-files=all` (before-image)

Command: `dotnet tool run csharpier format .`

Command: `git status --porcelain --untracked-files=all` (after-image)

Command: `dotnet tool run csharpier check .`

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

## Output Summary

Before-image:

```
 M UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs
 M UtilitiesCS/Threading/UiThread.cs
```

Formatter output:

```
Formatted 1611 files in 3882ms.
```

After-image:

```
 M UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs
 M UtilitiesCS/Threading/UiThread.cs
```

The two images are identical. Both listed paths are Phase 3 Write Set files. The formatter did rewrite one region inside `UtilitiesCS/Threading/UiThread.cs`, collapsing the wrapped `_uiThreadId` guard of the new predicate onto a single line, which porcelain cannot show because the file was already listed as modified; the read-only check below is what establishes that no drift remains.

Read-only check:

```
Checked 1611 files in 6180ms.
```

Analyzer build:

```
    0 Warning(s)
    0 Error(s)
```

Nullable build:

```
    0 Warning(s)
    0 Error(s)
```

Both builds used `/t:Rebuild`, and neither passed `/p:Nullable=enable`. `UtilitiesCS/Threading/UiThread.cs` carries `#nullable enable` at line 1, so its nullable-flow diagnostics were promoted to errors by the second build and none was produced by the three fixes.

## Second pass, run after the [P3-T6] test correction

PASS_2_TIMESTAMP: 2026-09-08T02-06

[P3-T6] found that two of the Phase 2 regression tests carried a defective ambient-apartment premise, and re-authoring them changed `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`. A file change requires the toolchain loop to restart, so the whole sequence above was re-run against the corrected tree. The two production-file restorations that the re-measurement performed were both reverted before this pass, and the pass therefore observes the Phase 3 tree.

PASS_2_BEFORE_IMAGE:

```
 M UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs
 M UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs
 M UtilitiesCS/Threading/UiThread.cs
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p3-t5-format-and-builds.md
```

PASS_2_FORMATTER_OUTPUT: `Formatted 1611 files in 3009ms.`

PASS_2_AFTER_IMAGE:

```
 M UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs
 M UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs
 M UtilitiesCS/Threading/UiThread.cs
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p3-t5-format-and-builds.md
```

The two images are identical. The one untracked path is this artifact, written by the first pass.

PASS_2_FORMAT_CHECK: `Checked 1611 files in 6348ms.`, exit 0.

PASS_2_ANALYZER_BUILD: exit 0, `    0 Warning(s)` and `    0 Error(s)`.

PASS_2_NULLABLE_BUILD: exit 0, `    0 Warning(s)` and `    0 Error(s)`.

PASS_2_EXIT_CODE: 0

This second pass is the state [P3-T6] was run against.
