# P4-T11 — Changed-file set gate (AC1, AC8)

Timestamp: 2026-09-01T20-20
Command: `git diff --name-status 988d35a8f8eb7436cc46a9f6424db917ed93807a HEAD -- QuickFiler QuickFiler.Test`, `git status --porcelain -- QuickFiler QuickFiler.Test`, and `git diff --numstat 988d35a8f8eb7436cc46a9f6424db917ed93807a HEAD -- QuickFiler.Test/QuickFiler.Test.csproj`
EXIT_CODE: 0

## Base-ref substitution — this is the gate that required it

The plan's stated commands name `2b85134b42872e405602e6064e02dc9cda6c319b`. That SHA was `origin/main` at plan-authoring time and has been superseded; `origin/main` advanced to `988d35a8f8eb7436cc46a9f6424db917ed93807a` carrying eight sibling deliveries, and that commit was merged into this branch before execution began. The re-anchored SHA is the merge base of this branch with `origin/main`, established in `evidence/baseline/p0-t7-base-ref.md`.

This task is the one where the substitution is decisive rather than merely tidy. Measured directly, against the superseded SHA:

    git diff --name-status 2b85134b... HEAD -- QuickFiler QuickFiler.Test
    → 22 paths

    git diff --numstat 2b85134b... HEAD -- QuickFiler.Test/QuickFiler.Test.csproj
    → 1	0	QuickFiler.Test/QuickFiler.Test.csproj

**Both clauses of this gate would fail against the plan-pinned SHA, for reasons that have nothing to do with this change.** The first clause demands exactly five paths and would see 22. The second demands that `QuickFiler.Test/QuickFiler.Test.csproj` print nothing, and would see one added line — a line added by a sibling delivery on `origin/main`, not by this work. Executing the plan literally would therefore have produced a false failure and, worse, would have made AC1's "`QuickFiler.Test.csproj` is unchanged" clause unverifiable.

## Result 1 — the name-status listing is exactly the five expected paths

    M	QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs
    M	QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs
    M	QuickFiler/Controllers/QfcItemController.Initialization.cs
    A	QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs
    M	QuickFiler/QuickFiler.csproj

Five paths, and no others. Each matches the plan's enumeration in both path and status:

| Path | Expected status | Observed |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` | added | A |
| `QuickFiler/QuickFiler.csproj` | modified | M |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | modified | M |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | modified | M |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | modified | M |

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is **absent** from the listing, which is what AC8 requires and what P2-T4 verified from the other direction.

## Result 2 — the porcelain listing prints nothing

    git status --porcelain -- QuickFiler QuickFiler.Test
    (no output)

Nothing staged, nothing modified, nothing untracked in either source directory. The tree is clean, so the name-status listing above describes the complete change set rather than a partial view of it.

The porcelain span is required alongside the diff, not redundant with it. A ref-anchored name-listing diff enumerates tracked changes only and is structurally blind to an untracked file, so a newly created file would be invisible to it; conversely, porcelain status goes empty once a change is committed. Here the created file is visible to the diff because P1-T2 staged it and P3-T15 committed it, and the empty porcelain confirms nothing further is pending. Each command alone would be silent in a state where the other reports.

## Result 3 — `QuickFiler.Test/QuickFiler.Test.csproj` is unchanged

    git diff --numstat 988d35a8... HEAD -- QuickFiler.Test/QuickFiler.Test.csproj
    (no output)

The test project file has zero changed lines, as AC1 requires. This holds because the four new tests landed in two files that already carry `<Compile Include>` entries: `Part3.cs` and the primary partial `QfcItemController.InitializationTests.cs`. No new test file was created, so no project-file edit was needed.

By contrast `QuickFiler/QuickFiler.csproj` **is** modified, by exactly one added line and zero deleted lines (measured in P1-T2), because `QuickFiler.csproj` enumerates the `QfcItemController` partials explicitly with no wildcard and the new production partial required an entry.

## The empty-output clauses are discriminating

Two of the three results above are empty output, which verifies nothing unless the same command produces output in the failing case. Both were demonstrated to do so:

- The identical `git diff --numstat` form against `QuickFiler.Test/QuickFiler.Test.csproj` returns a populated row `1	0	QuickFiler.Test/QuickFiler.Test.csproj` when evaluated against the superseded base, so the command, the pathspec and the working directory are all correct and the empty result is a genuine observation of no change.
- The same `--numstat` form returned `3	3	...` for `QfcItemController.Initialization.cs` in P2-T4 and P2-T5, against the same base used here.
