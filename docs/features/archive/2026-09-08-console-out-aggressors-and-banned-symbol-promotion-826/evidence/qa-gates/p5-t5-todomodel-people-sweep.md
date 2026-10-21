# Item-1 sweep: `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` (issue #826, [P5-T5])

Timestamp: 2026-09-09T19-31

Command: the initializer body was read in full before anything was deleted, then a
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard removed exactly the single
install statement, asserting a match count of 1 before writing. The path was then formatted with
`csharpier format` and gated with `-SimpleMatch` counts and an anchored `git diff --numstat`.

The directory name `ToDoModel.Test/Data Model/` contains a space, so the path was double-quoted in every
invocation, including the `csharpier format` argument and the `git diff` pathspec.

EXIT_CODE: 0 (csharpier `format` reported `Formatted 1 files in 1107ms.` and exited 0)

## Risk 5 discharge — initializer body read before deleting

The `[TestInitialize] public void Setup()` method does other work, so only the single install line was
removed. It retains `_mockGlobals = new Mock<IApplicationGlobals>();`, a
`new Mock<IFileSystemFolderPaths>()` construction and a `specialFolders` dictionary literal. This file is
not in the AC4 delete-the-method group.

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `Console.SetOut(` | 0 | 0 |
| `DebugTextWriter` | 0 | 0 |

## Anchored numstat

```
0	1	ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs
```

Exactly one removed line and zero added lines, measured after the csharpier pass.

## Encoding preservation

The file carries a UTF-8 byte-order mark and was written back with a `UTF8Encoding` constructed from that
observed flag, so it did not lose one.

Output Summary: the install statement is removed, the initializer method and every other statement are
intact, and both `Console.SetOut(` and `DebugTextWriter` counts are 0 for this path.
