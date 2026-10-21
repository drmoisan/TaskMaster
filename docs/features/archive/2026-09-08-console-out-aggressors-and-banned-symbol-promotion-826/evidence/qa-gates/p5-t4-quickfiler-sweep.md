# Item-1 sweep: six `QuickFiler.Test/Controllers` files (issue #826, [P5-T4])

Timestamp: 2026-09-09T19-30

Command: each initializer body was read in full before anything was deleted, then a
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard removed exactly the single
install statement from each file, asserting a per-file match count of 1 before writing. The touched paths
were then formatted with `csharpier format`, gated with `-SimpleMatch` counts and an anchored
`git diff --numstat`, and the sibling-owned file was checked with:

```
git diff --name-only $Base -- "QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs"
git status --porcelain --untracked-files=all -- "QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs"
```

EXIT_CODE: 0 (csharpier `format` reported `Formatted 6 files in 3901ms.` and exited 0)

## Naming trap observed

Matching was on the full filename. `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` is
sibling-owned by another child of this epic, is not in the 33-file population, and was **not opened or
edited** by this task. Four other files whose names begin `QfcHomeController` are in the population, as
are two `QfcFormController` files; all six are listed explicitly below.

## Risk 5 discharge — initializer bodies read before deleting

All six `[TestInitialize] public void Setup()` methods do other work, so only the single install line was
removed in each:

| File | Statements the initializer retains |
|---|---|
| `QfcHomeControllerTests.cs` | `_mockRepository` construction, `Create<IApplicationGlobals>()`, a `SetupGet(x => x.AF.CancelToken)` chain and `Create<Outlook.Application>()` |
| `QfcHomeControllerRunAsyncTests.cs` | same shape |
| `QfcHomeControllerPropertyTests.cs` | same shape |
| `QfcHomeControllerIterationTests.cs` | same shape |
| `QfcFormControllerTests.cs` | `_mockGlobals` and `_mockAF` constructions plus a `SetupSet`/`Callback`/`Verifiable` chain |
| `QfcFormControllerSeamTests.cs` | `_mockGlobals`, `_mockAF`, a `Setup(g => g.AF)` chain and four further mock constructions |

None of these six is in the AC4 delete-the-method group.

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `Console.SetOut(` across the six paths | 0 | 0 |
| `DebugTextWriter` across the six paths | 0 | 0 |

## Sibling-owned file — both spans empty

```
git diff --name-only $Base -- "QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs"
(empty)

git status --porcelain --untracked-files=all -- "QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs"
(empty)
```

The anchored diff being empty proves the tracked sibling-owned file is neither modified nor deleted
relative to the base anchor. The porcelain companion catches the one remaining state the diff misses, a
delete-then-recreate-as-untracked, and it is empty too.

## Anchored numstat

```
0	1	QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs
0	1	QuickFiler.Test/Controllers/QfcFormControllerTests.cs
0	1	QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs
0	1	QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs
0	1	QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs
0	1	QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
```

Exactly one removed line and zero added lines per file, measured after the csharpier pass, and
`QfcHomeControllerCleanupTests.cs` is absent from the list.

Output Summary: all six install statements removed with their initializer methods and every other
statement intact, `Console.SetOut(` and `DebugTextWriter` both 0 across the six paths, and the
sibling-owned `QfcHomeControllerCleanupTests.cs` provably untouched in both the anchored diff and the
porcelain span.
