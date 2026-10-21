# Phase 7 — AC12 inverse-constraint audit

Timestamp: 2026-09-07T03-21
Task: [P7-T5]
Issue: #798

Host-specific absolute paths are redacted to a `<worktree>` token. All commands were executed with
the working directory set to `<worktree>`, anchored to base commit c431dc32, against HEAD
4a29d7e79112fcaf359110c16c6933d9819165fa.

## Clause 1 — the QuickFiler home controller is untouched in both the committed and the working state

Two observations are required and both were taken, because each alone is blind in one state: an
anchored name-listing diff cannot report an untracked or unstaged edit, and porcelain status goes
empty once a change is committed.

1. Command: git diff --name-only c431dc32 -- QuickFiler/Controllers/QfcHomeController.cs
   EXIT_CODE: 0
   Output: none (zero output lines).

2. Command: git status --porcelain --untracked-files=all -- QuickFiler/Controllers/QfcHomeController.cs
   EXIT_CODE: 0
   Output: none (zero output lines).

Verdict: PASS. `QuickFiler/Controllers/QfcHomeController.cs` is provably unchanged relative to the
base commit in the committed state, and carries no unstaged or untracked modification in the working
state. The narrow `catch (OperationCanceledException)` at line 71 of that file is therefore neither
widened nor removed by this change. That catch is still present and still narrow, confirmed by a
literal search of the file returning the single line
`71:            catch (OperationCanceledException)`.

## Clause 2 — no modified production file gained a broadened `catch (System.Exception`

The count of the literal `catch (System.Exception` must not be greater than the base-commit count for
each of the four modified production files. Counts were taken at c431dc32 and in the working tree,
and each file was additionally inspected through its own anchored content diff, one command per path
as the task directs. `git grep -c` omits a path whose count is zero, so an absent path in the
working-tree output is recorded below as a count of 0.

| Path | Base count at c431dc32 | Current count | Delta | Verdict |
|---|---|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs` | 1 | 0 | -1 | PASS (decreased) |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 4 | 4 | 0 | PASS (unchanged) |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 1 | 1 | 0 | PASS (unchanged) |
| `TaskMaster/Ribbon/RibbonViewer.cs` | 2 | 2 | 0 | PASS (unchanged) |

No file's count rose. Verdict: PASS.

### Per-file diff inspection

Command: git diff c431dc32 -- UtilitiesCS/Extensions/DfDeedle.cs
EXIT_CODE: 0
Result: 19 insertions, 115 deletions. The insertions are two `ValidateRequiredEmailColumns` call
sites with their explanatory comments and one hoisted `folderName` local. No inserted line contains
`catch (System.Exception`. The single pre-existing occurrence sat inside `EnsureTriageColumnExists`,
which this change relocates wholesale into the new partial
`UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`, so the occurrence left this file by relocation
rather than by removal. The new partial carries exactly 1 occurrence, which is the same guarded
`UserDefinedProperties.Add` failure handler with the same narrow purpose; the total across the two
partials is 1, matching the base-commit total of 1 for the single file. No catch was broadened.

Command: git diff c431dc32 -- QuickFiler/Controllers/QfcDatamodel.cs
EXIT_CODE: 0
Result: two hunks, each replacing `throw e;` with `throw;`. Four changed lines in total, two removed
and two added. No catch clause is added, removed, widened or narrowed; the four existing
`catch (System.Exception` clauses are untouched.

Command: git diff c431dc32 -- QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs
EXIT_CODE: 0
Result: one hunk replacing `throw e;` with `throw;`. No catch clause is added or altered; the one
existing `catch (System.Exception` clause is untouched.

Command: git diff c431dc32 -- TaskMaster/Ribbon/RibbonViewer.cs
EXIT_CODE: 0
Result: one added `_commandBoundary` field, its initialisation in both constructors, three named
handlers rewritten as one-line awaits of `RunAsync`, one added `CreateCommandBoundary` factory and
one added `ReportRibbonCommandFailure` static sink. No inserted line contains
`catch (System.Exception`; the boundary type owns the catching, not this file. The two pre-existing
occurrences are untouched.

## Conclusion

Both AC12 inverse constraints hold. The home controller's narrow cancellation catch is provably
untouched in the committed and the working state, and no modified production file has a greater
count of `catch (System.Exception` than it had at the base commit. No existing catch anywhere was
broadened as an alternative to delivering AC1 or AC3.

Output Summary: AC12 PASS on both clauses. `QuickFiler/Controllers/QfcHomeController.cs` produces
zero output lines from both the anchored name-listing diff and the porcelain status, so it is
untouched in both states, and its narrow `catch (OperationCanceledException)` at line 71 remains.
The `catch (System.Exception` count did not rise in any of the four modified production files:
`UtilitiesCS/Extensions/DfDeedle.cs` fell from 1 to 0 by relocating `EnsureTriageColumnExists` into
the new partial, and `QuickFiler/Controllers/QfcDatamodel.cs` (4),
`QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` (1) and `TaskMaster/Ribbon/RibbonViewer.cs`
(2) are all unchanged. No catch was broadened.
