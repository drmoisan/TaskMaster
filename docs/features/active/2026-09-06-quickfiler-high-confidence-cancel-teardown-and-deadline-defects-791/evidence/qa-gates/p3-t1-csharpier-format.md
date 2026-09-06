# [P3-T1] CSharpier format

Timestamp: 2026-09-06T15-02

Command: `dotnet tool run csharpier format .`, with `DOTNET_ROOT` bound to the repository-local
`.dotnet-sdk` directory, and with the tree observed before and after by
`git status --porcelain --untracked-files=all` and `git diff --stat $BaseSha`.

`$BaseSha` was bound inside the same command block by the R10 line that reads
`BASE-SHA` out of the [P0-T2] artifact. It resolved to
`51b557dfe35702090fec778febfd4049e0e0fed4`.

`format` rewrites tracked source and still exits 0 after rewriting, so the exit code alone cannot
distinguish a clean run from a repairing one. The distinguishing observation is the pair of
before/after comparisons recorded below.

## Pass 1 — repairing

Verbatim printed line:

```
Formatted 1587 files in 4731ms.
```

EXIT_CODE: 0
PATH_SETS_IDENTICAL: True
DIFFSTAT_IDENTICAL: False

The path set was unchanged (43 entries before and after: the formatter created and deleted no file),
but the anchored diffstat moved from `18 files changed, 1705 insertions(+), 161 deletions(-)` to
`18 files changed, 1721 insertions(+), 161 deletions(-)`. A 16-line insertion delta with no change
to the file set is the signature of a repairing run: CSharpier rewrapped code this plan had written
in a shape the pinned 1.2.6 formatter does not produce.

Because this step changed files, the toolchain loop was restarted from step 1 rather than continued,
as the General Code Change Policy requires.

## Pass 2 — clean

Verbatim printed line:

```
Formatted 1587 files in 2093ms.
```

EXIT_CODE: 0
PATH_SETS_IDENTICAL: True
DIFFSTAT_IDENTICAL: True

Both derived comparison lines are `True`, so this invocation rewrote nothing: the path set is
identical (43 entries) and the anchored diffstat is byte-identical
(`18 files changed, 1721 insertions(+), 161 deletions(-)`) before and after. This is the clean pass
that opens the uninterrupted toolchain pass [P3-T6] records.

## The 1587 figure

`BASELINE-CSHARPIER-CHECKED-FILES` from [P0-T7] is 1583. The delta of 4 is exactly the four new
`.cs` files this plan creates: `QfcStreamingDequeueConfidenceGateTests.Part4.cs`,
`QfcFormControllerCancelTeardownTests.cs`, `QfcHomeControllerCleanupTests.cs` and
`QfcDatamodelTeardownTests.cs`. [P3-T2] records the same delta from the read-only `check`.

## Line-count effect of the format

The pass-1 rewrite changed line counts, which is why the ceiling audit is taken after the format
rather than before it. The measurements [P3-T9] records are taken from the tree as it stands after
pass 2. No `.cs` file in the plan's edited or created set exceeded 500 lines at any point after the
format; the largest is `QfcStreamingDequeueConfidenceGateTests.Part2.cs` at 498.
