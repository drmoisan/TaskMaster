# Baseline — CSharpier Check (Before Edit) — Fail-Before Evidence

Timestamp: 2026-06-13T01-05
Command: `dotnet csharpier check .`
EXIT_CODE: 1

## Output Summary

`dotnet csharpier check .` failed with exit code 1. Checked 1060 files. Exactly 8
`.csproj` files were reported as "Was not formatted. The file did not end with a
single newline." This is the fail-before evidence for the remediation and matches
the CI failure on PR #190.

### Enumerated failing `.csproj` files (8)

1. `QuickFiler.Test/QuickFiler.Test.csproj`
2. `Tags.Test/Tags.Test.csproj`
3. `TaskMaster/TaskMaster.csproj`
4. `TaskMaster.Test/TaskMaster.Test.csproj`
5. `ToDoModel.Test/ToDoModel.Test.csproj`
6. `TaskVisualization.Test/TaskVisualization.Test.csproj`
7. `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
8. `VBFunctions.Test/VBFunctions.Test.csproj`

### Verbatim tail

```
Error .\QuickFiler.Test\QuickFiler.Test.csproj - Was not formatted.
  The file did not end with a single newline.
Error .\Tags.Test\Tags.Test.csproj - Was not formatted.
  The file did not end with a single newline.
Error .\TaskMaster\TaskMaster.csproj - Was not formatted.
  The file did not end with a single newline.
Error .\TaskMaster.Test\TaskMaster.Test.csproj - Was not formatted.
  The file did not end with a single newline.
Error .\ToDoModel.Test\ToDoModel.Test.csproj - Was not formatted.
  The file did not end with a single newline.
Error .\TaskVisualization.Test\TaskVisualization.Test.csproj - Was not formatted.
  The file did not end with a single newline.
Error .\UtilitiesCS.Test\UtilitiesCS.Test.csproj - Was not formatted.
  The file did not end with a single newline.
Error .\VBFunctions.Test\VBFunctions.Test.csproj - Was not formatted.
  The file did not end with a single newline.
Checked 1060 files in 4108ms.
```
