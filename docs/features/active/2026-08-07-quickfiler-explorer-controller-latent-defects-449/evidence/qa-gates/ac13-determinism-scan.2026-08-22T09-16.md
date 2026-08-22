# AC-13 — Determinism Prohibition Scan (Issue #449, [P7-T11])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
git grep --untracked -n -E "Thread.Sleep|Task.Delay|MessageBox.Show|Path.GetTempPath|new Form|Application.Run" \
  -- QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs \
     QuickFiler.Test/Controllers/QfcExplorerController.ConversationViewTests.cs
```
EXIT_CODE: 1
Output: (empty — no output)

`git grep` returns exit code 1 when there is no match.

## Result

**ZERO matching lines across BOTH test files.**

The scan covers both test files added by this change, including the second file
`QuickFiler.Test/Controllers/QfcExplorerController.ConversationViewTests.cs` created by the [P6-T14]
size split. `--untracked` is essential here: both files are new and not yet committed at the time of
this scan, so without that flag `git grep` would have searched nothing and returned a vacuous
zero-match result.

| Prohibited construct | Matches |
| --- | --- |
| `Thread.Sleep` | 0 |
| `Task.Delay` | 0 |
| `MessageBox.Show` | 0 |
| `Path.GetTempPath` | 0 |
| `new Form` | 0 |
| `Application.Run` | 0 |

## Supplementary scan

The repository additionally bans `DateTime.Now`, `DateTime.UtcNow`, and `Random.Shared` via
`BannedSymbols.txt`, and prohibits temporary files in tests. A second scan covered those plus the
`GetTempFileName` temporary-file API:

Command:
```
git grep --untracked -n -E "DateTime.Now|DateTime.UtcNow|Random.Shared|GetTempFileName|Thread.Sleep" \
  -- QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs \
     QuickFiler.Test/Controllers/QfcExplorerController.ConversationViewTests.cs
```
EXIT_CODE: 1
Output: (empty — no match)

**Zero matches.** No wall-clock read, no seeded-or-unseeded randomness, and no temporary-file API
appears in either file.

## How the tests achieve determinism without any of these constructs

- **No modal dialog is ever displayed.** Every test that reaches the not-in-view branch of
  `OpenQFItem` replaces the `NotInViewDialogInvoker` seam with a plain lambda returning a fixed
  `DialogResult` before acting. The production `MessageBox.Show` default at
  `QfcExplorerController.cs:63` is never invoked, so no test blocks on user input and no message pump
  is required. This is precisely what the [P5-T3] seam exists to make possible.
- **No live `Form` and no message pump.** The tests construct no WinForms control. All COM and UI
  boundaries are Moq mocks.
- **No waiting.** The `async` tests `await` the returned `Task` directly rather than polling or
  sleeping. `OpenQFItem` internally uses `Task.Run`, but the test awaits the whole operation, so
  completion is observed by the awaiter rather than by elapsed time. No test carries a timing
  tolerance or a retry.
- **No shared mutable state between tests.** A fresh `MockRepository` and a fresh mock graph are built
  in `[TestInitialize] Setup` for every test, so the tests are order-independent and can run in
  parallel. The runs were executed with `Workers: 24, Scope: ClassLevel` parallelisation and passed.
- **No temporary files, no filesystem, no network, no external process.**

## Corroborating run-to-run evidence

The static scan above establishes that no prohibited construct is present. The complementary
observational evidence is
`step5-second-consecutive-run.2026-08-22T09-16.md`, which records two consecutive full-suite runs with
identical executed counts (6,452), identical passed counts (6,452), and **byte-identical pass sets**
compared with `diff`, with an empty failing set in both. Both halves of the AC-13 evidence therefore
agree.

## Output Summary

`git grep --untracked -n -E "Thread.Sleep|Task.Delay|MessageBox.Show|Path.GetTempPath|new Form|Application.Run"`
over **both** test files added by this change returns **zero matching lines** (EXIT_CODE 1, empty
output), and a supplementary scan for `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, and
`GetTempFileName` likewise returns zero. `--untracked` was used so the two new, uncommitted files were
genuinely in scope. Determinism is achieved structurally — the dialog seam is substituted in every test
that reaches it, all boundaries are mocked, and async work is awaited rather than waited on.
