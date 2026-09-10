# Item-1 sweep: two `UtilitiesCS.Test/Threading` files (issue #826, [P5-T3])

Timestamp: 2026-09-09T19-28

Command: both initializer bodies were read in full before anything was deleted, then a
`pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard removed exactly the single
install statement from each file, asserting a per-file match count of 1 before writing. The touched paths
were then formatted with `csharpier format` and gated with `-SimpleMatch` counts and an anchored
`git diff --numstat`.

EXIT_CODE: 0 (csharpier `format` reported `Formatted 2 files in 1521ms.` and exited 0)

## Risk 5 discharge — initializer bodies read before deleting

Both initializers do other work, so only the single install line was removed in each. Each retains
`this.mockRepository = new MockRepository(MockBehavior.Loose);`, a
`this.mockRepository.Create<IFileSystemFolderPaths>()` assignment, a commented `SetupGet` line that is
not the method's only body, a `Create<IApplicationGlobals>()` assignment, `SetupAllProperties()` and a
`SetupGet(x => x.FS)` chain.

Neither file is in the AC4 delete-the-method group.

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| `Console.SetOut(` across the two paths | 0 | 0 |
| `DebugTextWriter` across the two paths | 0 | 0 |

## Anchored numstat

```
0	1	UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs
0	1	UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs
```

Exactly one removed line and zero added lines per file, measured after the csharpier pass.

## Encoding preservation

Both files carry a UTF-8 byte-order mark, and both were written back with a `UTF8Encoding` constructed
from that observed flag, so neither lost it.

Output Summary: both install statements removed with their initializer methods and every other statement
intact. `Console.SetOut(` and `DebugTextWriter` counts are both 0 across the two paths.
