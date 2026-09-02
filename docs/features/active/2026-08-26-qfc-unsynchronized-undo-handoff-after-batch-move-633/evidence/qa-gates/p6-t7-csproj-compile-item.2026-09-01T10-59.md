# Test project compile item (P6-T7)

Timestamp: 2026-09-01T10-59
Task: [P6-T7]
Working directory: WORKTREE

Command: `pwsh -NoProfile -File <scratchpad>/p6sweeps.ps1`, searching
`QuickFiler.Test/QuickFiler.Test.csproj` for the literal token
`Controllers\QfcFormControllerUndoHandoffTests.cs`
EXIT_CODE: 0

Match count: **1**.

Matching line:

```
114: <Compile Include="Controllers\QfcFormControllerUndoHandoffTests.cs" />
```

Output Summary: The test project carries exactly one `<Compile Include>` entry for the new test file.
It sits at line 114, immediately after the existing `Controllers\FilerQueueTests.cs` entry at line 113,
and is written in the same XML element form as its neighbours.

The entry is required rather than conventional: `QuickFiler.Test/QuickFiler.Test.csproj` uses explicit
compile items, so a new `.cs` file under `Controllers/` is not picked up automatically and would compile
into nothing. The functional half of the same claim is recorded by P7-T8, which must find all five of
the new test names in a run of the built assembly; a missing compile item would have shown there as
zero discovered tests rather than as a build error.

The project file grew from 512 lines to 513, an increase of exactly one line, which
`git diff --stat` confirmed as `1 insertion(+)` with no deletion.

This artifact, together with P7-T8, supplies the evidence for the AC17 check-off in P8-T21.
