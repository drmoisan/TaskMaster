# Baseline — Changed .cs Diff Scope (Cycle 3, pre-fix)

Timestamp: 2026-06-08T19-44

Command: git diff --name-only main..HEAD -- "*.cs"

EXIT_CODE: 0

Output Summary:
Pre-fix changed-`.cs` set (feature branch vs main):
- ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs
- ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs
- UtilitiesCS/Extensions/IEnumerableExtensions.cs
- UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs

The file carrying the re-enable edit that introduced the formatting violation is
`ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` (the only file reported unformatted
by repo-wide CSharpier check in P0-T3). This set is the diff-scope baseline; after the
formatting fix the changed-`.cs` set must remain identical (no new file added/removed
by the formatting-only application).
