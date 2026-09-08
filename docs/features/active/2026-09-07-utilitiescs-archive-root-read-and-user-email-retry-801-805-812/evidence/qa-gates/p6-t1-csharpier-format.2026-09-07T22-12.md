# Phase 6 — Scoped CSharpier Format (P6-T1)

Timestamp: 2026-09-08T08-29

Command: `dotnet tool run csharpier format UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorArchiveRootDegradationTests.cs UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`

EXIT_CODE: 0

The exit code is deliberately not the observation for this task: `csharpier format` exits 0 whether or not it rewrote a file. The observation is the before-and-after tree state recorded below. The write is scoped to this plan's own paths by explicit path arguments, so a repository-wide rewrite cannot enlarge the branch diff.

Output Summary:

The tool's own summary line, transcribed verbatim:

```
Formatted 8 files in 6835ms.
```

`git status --porcelain --untracked-files=all` taken immediately BEFORE the command, 1 line:

```
 M docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/plan.2026-09-07T22-12.md
```

`git status --porcelain --untracked-files=all` taken immediately AFTER the command, 1 line:

```
 M docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/plan.2026-09-07T22-12.md
```

The two are identical, so the formatter rewrote no tracked file and this phase is not restarted. The single entry in both is the plan file, which carries the P5-T11 check-off written after the P5-T11 commit; it is not a `*.cs` path and is not one of the eight arguments given to the formatter. The `Formatted 8 files` wording reports the number of files the tool processed, not the number it changed, which is why the tree comparison rather than that line is the discriminating observation.

Context: the eight Write Set paths were already formatter-clean when this task ran, because the same command was applied to them as a micro-action after the Phase 4 edits and the Phase 5 edits that followed were comment-only and stayed within the 100-column print width. That is why the before and after states agree rather than differing by a formatting commit.
