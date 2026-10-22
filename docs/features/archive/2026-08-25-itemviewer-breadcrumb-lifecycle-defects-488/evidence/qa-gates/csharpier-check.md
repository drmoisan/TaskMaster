# Final QC Stage 1 Verification — CSharpier Check ([P8-T2])

Timestamp: 2026-08-28T06-20

Command: `dotnet tool run csharpier check .`, run from the worktree root. Read-only; verifies and
reports and rewrites nothing.
EXIT_CODE: 0

## Complete output

```
Checked 1554 files in 4461ms.
```

## Result

**`EXIT_CODE: 0`.** The first branch of this task's acceptance is satisfied directly: the command
reports **no formatting differences** anywhere in the repository, so the fallback branch — comparing a
non-empty reported set against the `[P0-T9]` baseline set — is not needed.

## Comparison against the [P0-T9] baseline

| | Baseline `[P0-T9]` | Now `[P8-T2]` |
| --- | --- | --- |
| Exit code | 0 | **0** |
| Files checked | 1553 | **1554** |
| Unformatted-file set | **(empty)** | **(empty)** |

The reported unformatted set is **exactly the baseline set**, both being empty, and it therefore
**contains none of the seven owned files**.

The checked-file count rose by exactly **one**, from 1553 to 1554. That is
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`, the single new file this
feature adds. No file was removed from CSharpier's scope.

## Why the empty baseline made this the strongest available form of the gate

`[P0-T9]` recorded an empty baseline unformatted set. A non-empty baseline would have let this gate pass
while some file remained unformatted, provided the set matched. Because the baseline was empty, the
comparison reduces to requiring that the post-change run **also** report an empty set — that is, that
none of the seven files this feature writes was left unformatted and no other file was disturbed. That
is what happened.

This also keeps the criterion `[P9-T6]` flips reachable in its plain form. The authorized exception in
`[P9-T15]` — leaving the formatting criterion unchecked when a pre-existing unformatted set exists — is
**not** triggered, because no such set exists at either end.

A count of files processed is deliberately not treated as the result signal. `Checked 1554 files` is the
tool's throughput line and prints on a clean and a dirty run alike; the signal is the exit code together
with the absence of any per-file unformatted report in the output.

Output Summary: EXIT_CODE 0. **1554 files checked, zero unformatted.** The reported set is empty,
identical to the `[P0-T9]` baseline empty set, and contains none of the seven owned files. The +1 file
count against the baseline is the one new test file.
