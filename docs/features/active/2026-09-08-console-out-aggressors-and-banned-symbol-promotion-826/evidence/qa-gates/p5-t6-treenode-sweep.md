# Item-1 sweep: the two `TreeNode` files, all four elements together (issue #826, [P5-T6])

Timestamp: 2026-09-09T19-33

This is spec Risk 1, the highest-likelihood way a careless implementation of item 1 breaks the nullable
gate.

Command: both files were read in full first, then the four elements were deleted together with the Edit
tool. The paths were then formatted with `csharpier format` and gated:

```
& $dotnet tool run csharpier format "ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs" "ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs"
@(Select-String -LiteralPath <path> -CaseSensitive -Pattern "\btw\b").Count
@(Select-String -LiteralPath <path> -CaseSensitive -SimpleMatch "DebugTextWriter").Count
@(Select-String -LiteralPath <path> -CaseSensitive -SimpleMatch "Console.SetOut(").Count
@(Select-String -LiteralPath <path> -CaseSensitive -SimpleMatch "TestInitialize").Count
git diff --numstat $Base -- <both quoted paths>
```

The directory name `ToDoModel.Test/Data Model/` contains a space, so every path was double-quoted.

EXIT_CODE: 0 (csharpier `format` reported `Formatted 2 files in 1860ms.` and exited 0)

## The four elements deleted together, and why

Deleting only the call would leave the field assigned but never read, which is **CS0414**. Deleting the
call and the assignment but keeping the field would leave it never used, which is **CS0169**. Both are
compiler warnings, so the `.editorconfig` `suggestion` ceiling does not apply to them, and
`/p:TreatWarningsAsErrors=true` promotes them to build errors in toolchain step 3. All four elements
therefore go together.

The block removed from each file, identical in both apart from indentation context:

```csharp
        private DebugTextWriter tw;

        //[ClassInitialize]
        //public void ClassInitialize()
        //{
        //    tw = new DebugTextWriter();
        //    Console.SetOut(tw);
        //}
```

plus, inside the `[TestInitialize]` method body:

```csharp
            tw = new DebugTextWriter();
            Console.SetOut(tw);
```

## What was deliberately kept

Both files retain their `[TestInitialize]` method and its
`this.mockRepository = new MockRepository(MockBehavior.Strict);` statement. These two files are in the
delete-one-line group, not in the AC4 delete-the-method group, which is why the `TestInitialize` count
must stay at its census value rather than fall to zero.

## Gate figures

| File | `\btw\b` | `DebugTextWriter` | `Console.SetOut(` | `TestInitialize` | Census `TestInitialize` |
|---|---|---|---|---|---|
| `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` | 0 | 0 | 0 | 2 | 2 |
| `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` | 0 | 0 | 0 | 2 | 2 |

All four required values hold for both files: whole-word `tw` 0, `DebugTextWriter` 0, `Console.SetOut(`
0, and `TestInitialize` unchanged at 2.

## Anchored numstat

```
0	10	ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs
0	10	ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs
```

Ten removed lines and zero added lines per file: the field declaration, the blank line that followed it,
the six-line commented-out `[ClassInitialize]` block, and the two statements inside `[TestInitialize]`.

The type-check proof that the CS0169 / CS0414 hazard is actually discharged is not this task's gate; it
is the interim nullable build in [P5-T11] and the AC3 gate in [P7-T3], both of which assert exit 0 with
zero `CS0169` and zero `CS0414` occurrences in a log that also records a non-zero `Task "Csc"` count.

Output Summary: in both `TreeNode` files the `DebugTextWriter` field, its assignment, the
`Console.SetOut(tw);` call and the orphaned commented-out `[ClassInitialize]` block are all removed
together, the whole-word `tw` count is 0, and the `[TestInitialize]` method with its `MockRepository`
construction is retained with the count unchanged from the census at 2.
