# Remediation QA Gate — Scoped CSharpier Format

Timestamp: 2026-08-23T19-12

Command:
```
dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs
```
(run from the worktree root; the mutating pass is deliberately scoped to the three touched files —
a repo-wide `format .` would rewrite unrelated files and break the three-file scope lock)

EXIT_CODE: 0

Output Summary:

Console output: `Formatted 3 files in 1774ms.`

That console line is CSharpier 1.x's **processed**-file count, not a rewrite count: it reads 3
whatever the formatter did, so a restart rule keyed on it would never terminate. The rewritten-file
count below is therefore derived from SHA-256 hashes captured immediately before and immediately
after the `format` invocation.

| File | SHA-256 before | SHA-256 after | Rewritten |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | `5002CA0E5BEDF06708F020F16E654AB4490576BE025F1DEB83048BA9CC14A31A` | `5002CA0E5BEDF06708F020F16E654AB4490576BE025F1DEB83048BA9CC14A31A` | no |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | `2A65D8472AF261AE63F3A7A87D08AB3BDEB3B7434D9201132C64FB5F1B6863FA` | `2A65D8472AF261AE63F3A7A87D08AB3BDEB3B7434D9201132C64FB5F1B6863FA` | no |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | `9BCABD8824031B25DEE81880CD9E226F29AEF82CFDB05D0CE9F38B987E2EC7AC` | `9BCABD8824031B25DEE81880CD9E226F29AEF82CFDB05D0CE9F38B987E2EC7AC` | no |

**Hash-derived rewritten-file count: 0.**

The Phase 1 comment corrections were authored at the same wrap width and indentation CSharpier
produces, so the formatter changed nothing. Because the rewritten-file count is 0, no loop restart
from P3-T1 is triggered by this task.
