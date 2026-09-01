# CSharpier format (P7-T2)

Timestamp: 2026-09-01T11-03
Task: [P7-T2]
Working directory: WORKTREE

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0

Verbatim summary line printed by the command:

```
Formatted 1566 files in 4565ms.
```

That number is **not** used as a rewritten-file count. In CSharpier 1.2.6 the value on the `Formatted N
files` line is the number of files *processed*, not the number rewritten, so it reports 1566 on a run
that changed nothing just as on a run that changed several files. The rewritten set is derived from the
porcelain set difference instead.

## Set difference

Pre-run porcelain entry count (from P7-T1): 29.
Post-run porcelain entry count: 29.

Set difference, meaning entries present after the run and absent before it: **0**.

No path appears in the difference, so **the set difference contains no path outside the six in-scope
files**. No `git checkout -- <path>` restoration was required, and none was performed.

The post-run porcelain output is identical to the pre-run output recorded verbatim in the P7-T1
artifact, except for the one new untracked entry that is the P7-T1 artifact itself, which was written
between the two captures and is not a formatter rewrite.

## What the set difference can and cannot detect

The set difference detects a rewrite only of a path that was **clean** before the run. Five of the six
in-scope files were already dirty at this point, so a rewrite of any of them does not appear in the
difference. That is intended rather than a gap: rewrites inside the authorized blast radius are
permitted, and only an out-of-scope rewrite would falsify AC16. The sixth in-scope file,
`QuickFiler.Test/QuickFiler.Test.csproj`, is excluded from CSharpier by `.csharpierignore` and was
already committed.

CSharpier did reformat within the blast radius. It rewrapped several lines this executor had written
past the default 100-column print width in
`QuickFiler.Test/Controllers/FilerQueueTests.cs` and
`QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` — for example splitting
`drain.IsCompleted.Should()` onto its own line and breaking a `field.Should().NotBeNull(...)` chain.
Formatter output wins over hand formatting, so those rewrites are accepted rather than reverted.

Two facts confirm the rewrites did not disturb a gate. First, the P0-T7 baseline recorded zero
unformatted files across the whole repository, so no pre-existing drift could be confused with a rewrite
caused by this change. Second, the P2-T3 and P5-T9 `using (` requirement survives: the three
`BeginTransactionAsync` acquisitions were each written as one physical line with no
`.ConfigureAwait(false)` continuation, and at 92 characters with indentation they sit under the 100-column
width, so CSharpier left them intact. P7-T3 re-verifies convergence.
