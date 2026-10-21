# [P6-T2] Changed-line coverage over the three production Write Set files

Timestamp: 2026-09-08T03-03

Command: `git add -N UtilitiesCS/Threading/IUiCaptureSource.cs`; then `git diff pre-809-base -- UtilitiesCS/Threading/UiThread.cs UtilitiesCS/Threading/SyncContextForm.cs UtilitiesCS/Threading/IUiCaptureSource.cs`; together with `git status --porcelain --untracked-files=all -- UtilitiesCS/Threading/`.

EXIT_CODE: 0

The diff is anchored to `pre-809-base` rather than to `HEAD`, so it enumerates every line this delivery added to these three files regardless of which phase commit carried it. The `git add -N` span makes a newly created interface file visible to the diff; it was a no-op here because the phase-boundary commits had already tracked the file, and the companion `git status --porcelain --untracked-files=all -- UtilitiesCS/Threading/` returned no lines, confirming that no file under that directory is untracked or uncommitted. The anchored diff reported `154 insertions(+), 6 deletions(-)` across the three files.

Added lines were mapped to their post-change line numbers from the hunk headers, then looked up with the pinned per-file lookup against `coverage\809-p5-final.cobertura.xml`. A changed line number is counted once and is treated as covered when any matching element carries `hits` greater than zero. A line number that matches no element is not executable and is excluded from both numerator and denominator.

## Per-file table

| File | Added lines | Executable | Covered | Uncovered | Percentage |
|---|---|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 103 | 48 | 46 | 2 | 95.83 |
| `UtilitiesCS/Threading/SyncContextForm.cs` | 1 | 0 | 0 | 0 | no executable added line |
| `UtilitiesCS/Threading/IUiCaptureSource.cs` | 50 | 0 | 0 | 0 | no executable line; interface declaration only |
| **Total** | **154** | **48** | **46** | **2** | **95.83** |

CHANGED_LINE_COVERAGE= 95.83

That figure is at least `90.00`.

UNCOVERED_ENUMERATION_COUNT= 2

- `UtilitiesCS/Threading/UiThread.cs:177` — `{`
- `UtilitiesCS/Threading/UiThread.cs:178` — `return true;`

Both are the body of the `ReferenceEquals(_context, _uiSyncContext)` clause of the replaced predicate, whose condition sits at line 176.

## Per-member table

This is the mechanical form of the AC6 clause requiring each newly added member at 90% or better. Every new member of this delivery consists entirely of added lines in these three files, so the added-line set is exactly the new-member set plus the replaced `IsCompleted` body.

| Member | Lines | Executable | Covered | Uncovered | Percentage |
|---|---|---|---|---|---|
| `IUiCaptureSource` (whole file) | 1 through 50 | 0 | 0 | 0 | declaration only, no executable line, recorded as such |
| `UiThread.Init` apartment precondition and `InitLock` region | 26 through 67 | 10 | 10 | 0 | 100.00 |
| `UiThread.SyncContextFormFactory` | 104 through 109 | 1 | 1 | 0 | 100.00 |
| `UiThread.DefaultSyncContextFormFactory` | 111 | 1 | 1 | 0 | 100.00 |
| `UiThread.ResetForTesting` | 113 through 136 | 14 | 14 | 0 | 100.00 |
| `SynchronizationContextAwaiter.IsCompleted` (replaced body) | 155 through 190 | 20 | 18 | 2 | 90.00 |
| `UiThread.NonStaInitMessagePrefix` | 229 through 232 | 0 | 0 | 0 | constant, no executable line, recorded as such |
| `UiThread.NonStaInitMessage` | 233 through 234 | 1 | 1 | 0 | 100.00 |

Every row whose executable count is greater than zero shows a percentage of at least `90.00`.

One further added executable line falls outside every member range above and is included in the file and total figures: `UtilitiesCS/Threading/UiThread.cs:72`, `_syncContextForm = SyncContextFormFactory();`, which is the rewritten call site inside the pre-existing `Initialize()` method rather than a new member. It is covered.

The two uncovered lines belong to the `IsCompleted` row, which is therefore recorded here with the reason they are uncovered and the test that leaves them so. The clause at 176 through 178 is reached only when the caller stands on the owning UI thread, the ambient context is non-null and is not the captured context, and the captured context is the persistent `_uiSyncContext`. `SynchronizationContextAwaiter_Tests.IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` is the case that installs `_uiThreadId` for the owning host thread, but it captures a `DispatcherSynchronizationContext`, so it falls through to the dispatcher clause at 184 through 188 rather than returning at 178. No case in this delivery installs `_uiSyncContext` and then awaits that same instance from the owning thread while a different context is ambient. The row still meets the 90% floor exactly.

## No changed line can lose coverage

Every changed line in these three files is either an added line, which is measured above, or a deleted line. A deleted line has no coverage to lose, because it no longer exists in the post-change file and is therefore absent from both the numerator and the denominator of any post-change measurement. There is no third category: `git diff` classifies every changed line as one or the other. The no-regression obligation over changed lines is therefore discharged by the added-line measurement alone, and the file-level figures in `p6-t1-uithread-file-coverage.md` corroborate it, rising from 76.83% to 96.03%.
