# Final QC — Test Run

- **Timestamp:** 2026-03-20T09-56
- **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`
- **EXIT_CODE:** 1
- **Output Summary:** Full-suite MSTest execution aborted. The run again encountered the pre-existing `StackOverflowException`, and in this session also reported an Application Control block while loading `ToDoModel.Test.dll` (`HRESULT 0x800711C7`). Results before abort: 396 passed, total time ~8.38s. Because the run aborted early, this full-suite count is not a reliable regression signal. Supplemental focused verification of the newly added regression tests was captured separately in `focused-triage-regression-tests.md`, where both new tests passed.