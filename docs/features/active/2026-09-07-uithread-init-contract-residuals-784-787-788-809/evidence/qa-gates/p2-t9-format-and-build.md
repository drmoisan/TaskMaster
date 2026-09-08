# [P2-T9] Phase 2 format and build

Timestamp: 2026-09-08T01-30

Command: `git status --porcelain --untracked-files=all` (before-image)

Command: `dotnet tool run csharpier format .`

Command: `git status --porcelain --untracked-files=all` (after-image)

Command: `dotnet tool run csharpier check .`

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

## Output Summary

Before-image:

```
 M UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs
 M UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs
 M UtilitiesCS.Test/Threading/UiThread_Tests.cs
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
?? UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs
```

Formatter output:

```
Formatted 1611 files in 3249ms.
```

After-image:

```
 M UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs
 M UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs
 M UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs
 M UtilitiesCS.Test/Threading/UiThread_Tests.cs
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
?? UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs
```

The two images are identical; no path differs between them. Every path in both is a Phase 2 Write Set file. The Phase 1 production paths are absent from both because Phase 1 was committed at `09bb952d`. The processed-file count rose from 1610 to 1611 by the one `.cs` file Phase 2 creates.

Read-only check:

```
Checked 1611 files in 6127ms.
```

Nullable build:

```
    0 Warning(s)
    0 Error(s)
```

## File sizes after the formatter run

Measured with the pinned idiom `(Get-Content -LiteralPath <path>).Count`.

PHASE2_LINES UtilitiesCS.Test/Threading/UiThread_Tests.cs 458
PHASE2_LINES UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs 458

PHASE2_FILES_OVER_495: 0

`UtilitiesCS.Test/Threading/UiThread_Tests.cs` finishes at 458 against the 215-line baseline [P0-T16] recorded, so 243 lines were added against the 275-line budget [P2-T8] states, and the file is 37 lines below the 495-line ceiling. `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` is 458 lines against the 460-line whole-file budget [P2-T1] states. Measuring here rather than only at [P4-T5] is what makes an overrun correctable while the tests are still being written.
