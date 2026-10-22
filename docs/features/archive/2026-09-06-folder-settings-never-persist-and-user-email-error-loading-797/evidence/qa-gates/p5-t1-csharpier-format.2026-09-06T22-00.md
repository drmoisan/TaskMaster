# P5-T1 — CSharpier Format (Issue #797)

Timestamp: 2026-09-07T09-57

Commands, run from the repository root of this worktree:

```powershell
git status --porcelain --untracked-files=all -- '*.cs' | Out-File -Encoding utf8 coverage/plan797-format-before.txt
dotnet tool run csharpier format .
git status --porcelain --untracked-files=all -- '*.cs' | Out-File -Encoding utf8 coverage/plan797-format-after.txt
```

EXIT_CODE: 0

## Summary count the formatter printed

`Formatted 1605 files in 3488ms.`

Per rule R5 that line reports the number of files processed, not the number rewritten, so it does not
by itself distinguish a clean run from a repairing one. The observations below supply that
discrimination.

## Set difference between the after and the before listings

The difference is empty: both listings contain the same thirteen entries.

```text
 M TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs
 M TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs
 M TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs
 M UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs
 M UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs
 M UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs
 M UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
 A UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
 M UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
 M UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs
?? UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
?? UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs
?? UtilitiesCS/Interfaces/IGlobals/IJunkFolderSelectionSink.cs
```

The Phase 0 artifact recorded `PRE-EXISTING-FORMAT-DRIFT: NONE`, so the acceptance requires every path
in the difference to be a Write Set path. The difference is empty, so that condition holds. Every one
of the thirteen paths listed above is nevertheless a Write Set path, and no path outside the Write Set
appears in either listing. No path enumerated as pre-existing drift needed reverting, because none was
recorded.

## Direct rewrite observation, beyond the porcelain listing

A porcelain listing cannot report a rewrite of a file that is already marked modified: such a file
stays marked modified whether or not the formatter touched it. This step therefore also hashed each of
the thirteen Write Set C# files immediately before and immediately after the formatter ran, with
SHA-256, and compared the two hashes.

```text
FORMAT-REWRITTEN-COUNT=0
FORMAT-EXIT=0
```

Zero Write Set files were rewritten by this run, which is the direct evidence that this formatter pass
is a clean pass rather than a repairing one.

## Loop restart

An earlier execution of this step, before the pass recorded here, did rewrite files. Under the Phase 5
loop rule that outcome restarts the loop at P5-T1, and the pass recorded above is the restarted pass.
The restart is recorded in the P5-T10 clean-pass artifact.

Output Summary: The formatter exits 0, processes 1605 files, rewrites zero Write Set files, and leaves
the C#-scoped porcelain listing unchanged. This is the clean formatter pass.
