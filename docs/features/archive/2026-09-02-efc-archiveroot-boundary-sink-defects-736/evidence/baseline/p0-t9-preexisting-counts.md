# P0-T9 — Pre-change occurrence facts that later gates are evaluated against

Timestamp: 2026-09-03T23-36

Command: `Select-String -SimpleMatch` and `[regex]::Matches` over the files named below, run from the
worktree root.

EXIT_CODE: 0

All measurements are in `QuickFiler/Controllers/EfcFormController.cs` unless stated otherwise.

## Archive-root reads through the `_globals.Ol` chain

Every active read of `ArchiveRootPath` through `_globals.Ol`, by line number:

**{556, 566, 863, 873, 1014}** — exactly the set this task's acceptance requires. Their text:

- 556 `_globals.Ol.ArchiveRootPath,`
- 566 `_globals.Ol.ArchiveRootPath,`
- 863 `_globals.Ol.ArchiveRootPath,`
- 873 `_globals.Ol.ArchiveRootPath,`
- 1014 `await _router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token);`

## `catch (` line numbers

**{151, 481, 498, 516, 578, 593, 973, 1016, 1020, 1163}** — a set of **10**.

This corrects the research artifact's section 1.3, which stated 9 and omitted the clause at line
1163. Every count gate downstream uses 10. Their text:

- 151 `catch (System.Exception sinkException)`
- 481 `catch (System.Exception ex)`
- 498 `catch (System.Exception ex)`
- 516 `catch (System.Exception ex)`
- 578 `catch (System.Exception ex)`
- 593 `catch (System.Exception ex)`
- 973 `catch (System.Exception ex)`
- 1016 `catch (OperationCanceledException)`
- 1020 `catch (System.Exception ex)`
- 1163 `catch (System.Exception ex)`

## Token counts in `QuickFiler/Controllers/EfcFormController.cs`

| Token | Pre-change occurrence count |
|---|---|
| `TryReportBoundaryFault` | **7** (one declaration at line 138 plus six invocations) |
| `.Dispose()` | **2** |
| `MessageBox` | **3** |
| `System.Windows.Forms` | 5 |
| `System.Windows.Forms.Application` | 0 |

The last two rows are recorded because P4-T1 asserts on the longer token and expects exactly one
occurrence of it after its edit, which is only meaningful against the pre-change zero.

## `KbdExecuteAsync` declarations

**{921, 927}** — exactly the two-declaration set this task's acceptance requires:

- 921 `async public Task KbdExecuteAsync(Func<Task> action)`
- 927 `public async Task KbdExecuteAsync(System.Action action)`

The other 16 lines carrying the token are call sites in the character-action maps (650, 651, 655,
657, 658, 662, 710, 715, 720, 725, 730, 736) and five commented-out lines (680-684), none of which is
a declaration.

## `<Compile Include=` counts in the three Write Set project files

| Project file | Pre-change `<Compile Include=` count |
|---|---|
| `TaskMaster/TaskMaster.csproj` | **53** |
| `QuickFiler.Test/QuickFiler.Test.csproj` | **161** |
| `TaskMaster.Test/TaskMaster.Test.csproj` | **57** |

These three counts were re-derived from `git show HEAD:<path>` after a first measurement pass
produced 51 / 158 / 54. That first pass read the three project files through
`[System.IO.File]::ReadAllText` with a **relative** path. The .NET current directory is not the
PowerShell location, so those three relative paths resolved against the session's own checkout rather
than against this worktree, and the counts they returned describe a different tree. Every other
measurement in this artifact was taken through `Select-String -LiteralPath` or
`Get-Content -LiteralPath`, both of which resolve through the PowerShell provider and therefore read
this worktree; each of those was re-measured against this worktree after the error was found and each
returned the value already recorded here, unchanged. The corrected figures above are the ones P1-T2,
P1-T6, and P2-T3 are evaluated against.

## `[TestMethod]` plus `[DataTestMethod]` attribute counts

| Test file | `[TestMethod]` | `[DataTestMethod]` | Total |
|---|---|---|---|
| `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` | 11 | 0 | **11** |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 15 | 1 | **16** |
| TaskMaster.Test's AppOlObjectsArchiveRootValidationTests.cs | 6 | 0 | **6** |

The acceptance-pinned values hold: 11 for the data-model archive-root class and 6 for the validation
class. The 16 recorded for `EfcFormControllerTests.cs` is the figure P2-T1 compares against.

## Citation drift

None. Every acceptance-pinned value above matches the plan's stated expectation, including the
corrected `catch (` set of 10.

Output Summary: archive-root read set {556, 566, 863, 873, 1014}; `catch (` set of 10 at
{151, 481, 498, 516, 578, 593, 973, 1016, 1020, 1163}; `TryReportBoundaryFault` 7; `.Dispose()` 2;
`MessageBox` 3; `System.Windows.Forms.Application` 0; `KbdExecuteAsync` declarations {921, 927};
`<Compile Include=` counts 53 / 161 / 57; test-attribute counts 11 / 16 / 6. No citation drift.
