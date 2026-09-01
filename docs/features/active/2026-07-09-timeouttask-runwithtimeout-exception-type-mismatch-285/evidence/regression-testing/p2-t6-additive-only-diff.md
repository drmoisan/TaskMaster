# P2-T6 — Additive-Only Diff Proof for the Test Tree

Timestamp: 2026-09-01T08-22

MERGE_BASE, read from `evidence/baseline/p0-t2-branch-and-merge-base.md`:
`2b85134b42872e405602e6064e02dc9cda6c319b`

EXIT_CODE: 0 (all three invocations)

## Invocation 1 — `TimeOutTask_AdditionalTests.cs` must be untouched

Command:

```text
git diff 2b85134b42872e405602e6064e02dc9cda6c319b -- UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs
```

Output, verbatim:

```text
```

**The diff produced no output at all.** The file is byte-identical to the merge base. The at-risk
test at its line 190, `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException`, is therefore
unedited, and so is the file's pre-existing 527-line file-size breach, which the plan's Non-Goals
place out of scope.

## Invocation 2 — `TimeOutTask_OverloadCoverageTests.cs` must be a pure insertion

Command:

```text
git diff 2b85134b42872e405602e6064e02dc9cda6c319b -- UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs
```

Diff header and hunk header, verbatim:

```text
diff --git a/UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs b/UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs
index 5cc469ac..0d43c34d 100644
--- a/UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs
+++ b/UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs
@@ -383,5 +383,45 @@ namespace UtilitiesCS.Test
             // Assert
             await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
         }
```

Deletion-line analysis:

| Measurement | Value |
| --- | --- |
| Total diff lines | 50 |
| Lines beginning with `+` | 41 |
| Lines beginning with a single `-` | **1** |
| Of those, the `--- a/` file header line | 1 |
| **Deletion lines after excluding the `--- a/` header** | **0** |

The single line beginning with `-` is exactly the diff's own `--- a/UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`
file header. Once it is excluded as the plan directs, **zero deletion lines remain**.

The hunk header `@@ -383,5 +383,45 @@` confirms the shape independently: 5 context lines in, 45 lines
out, starting at line 383 — a pure append of 40 new lines immediately before the class-closing brace,
with no line removed or rewritten anywhere in the file. Every pre-existing method, including the
at-risk test at line 106, `RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries`,
is untouched. Its line number is unchanged because the insertion is after it.

## Invocation 3 — untracked additions in the test tree

Command:

```text
git status --porcelain -- UtilitiesCS.Test
```

Output, verbatim:

```text
 M UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs
```

`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` is the **only** entry under
`UtilitiesCS.Test`. This is the companion observation the name-listing diffs cannot supply: an
anchored diff enumerates tracked changes only, so it is blind to a newly created untracked file. The
porcelain output confirms no new test file was created anywhere in that tree, consistent with the
plan's statement that no `.csproj` edit is required because no new source file is added.

Output Summary: `TimeOutTask_AdditionalTests.cs` is byte-identical to the merge base (empty diff).
`TimeOutTask_OverloadCoverageTests.cs` carries a pure insertion of 40 lines at line 383 with zero
deletion lines once the `--- a/` header is excluded. The porcelain status of `UtilitiesCS.Test` lists
that one modified file and nothing else. Neither at-risk test method body was changed.

Acceptance: met. The first diff produced no output at all; the second contains zero lines beginning
with a single `-` once the `--- a/` file header line is excluded; and the porcelain output lists
`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` as the only entry under
`UtilitiesCS.Test`.

## Re-Run Status

The P0-T6 unformatted-file list was recorded as the **empty list**, so neither
`TimeOutTask_AdditionalTests.cs` nor `TimeOutTask_OverloadCoverageTests.cs` appears in it. The
conditional P2-T6 re-run described by P3-T1 is therefore not triggered. No re-run was required and
none was appended.
