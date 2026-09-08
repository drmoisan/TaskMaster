# Phase 6 — Read-Only CSharpier Gate (P6-T2)

Timestamp: 2026-09-08T08-30

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Run from the worktree root. This read-only check is the formatter gate per D4, not the exit code of `csharpier format`, which is 0 whether or not it rewrote files.

Output Summary:

The tool's own summary line, transcribed verbatim:

```
Checked 1613 files in 6504ms.
```

The tool reported no file as needing formatting, and the exit code is 0. That satisfies the AC7 clause requiring `dotnet tool run csharpier check .` to report zero files needing formatting.

The P0-T8 baseline recorded `PRE-EXISTING FORMAT DRIFT: NONE`, so the conditional fallback branch of this task does not apply: this gate is satisfied on its primary condition, a clean exit code of 0, and no pre-existing drift had to be excused.

CSharpier is invoked through `dotnet tool run` so the manifest-pinned version 1.2.6 is used, matching `.github/workflows/_format-check.yml`. The file count of 1613 is 2 higher than the 1611 recorded by the P0-T8 baseline, which is accounted for exactly by the two files this plan creates: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs` and `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorArchiveRootDegradationTests.cs`.
