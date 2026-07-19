---
name: timedout-mstest-leaves-detached-runner
description: A timed-out Invoke-MSTestWithCoverage bash call leaves a detached pwsh runner alive that keeps respawning testhosts; a second run then contends over shared user.config and fails/hangs
metadata:
  type: project
---

When a Bash-tool call running `scripts/vscode/Invoke-MSTestWithCoverage.ps1` hits its timeout, the harness kills the bash shell but NOT the detached grandchildren: the `pwsh` process running the script, plus its `dotnet-coverage`/`vstest.console`/`testhost` pipeline, keep running. Re-running tests then produces TWO concurrent pipelines that contend.

Signature of the problem:
- `Get-Process vstest.console,testhost,dotnet-coverage` shows two pipelines with StartTimes ~minutes apart when you only launched one.
- A spurious test failure in a totally unrelated assembly (observed: `TaskTree.Test.TaskTreeControllerMoveLogicTests.MoveObjectsToSibling_RootTarget_RemovesFromRootsAndReseeds`) throwing `System.Configuration.ConfigurationErrorsException: The configuration file has been changed by another program` on `...user.config` (via `ToDoModel.IDList.GetNextToDoID` -> `ApplicationSettingsBase.Save()`). Two concurrent testhosts write the same per-user `user.config` -> this error.
- Coverage XML shows anomalously low branch-rate (~0.61 vs ~0.76 normal) because a contended run is partial.
- The full suite (5702 tests, ~37s alone) appears to "hang" for minutes.

**Why:** MSTest/settings writers share one machine-global `user.config`; two live pipelines race it.

**How to apply:** Before EVERY test run (and always after a test-run timeout), kill BOTH the pipeline binaries AND the lingering pwsh runner, then verify zero before launching:
```
Get-CimInstance Win32_Process -Filter "Name='pwsh.exe'" | ? { $_.CommandLine -match 'Invoke-MSTest' } | % { Stop-Process -Id $_.ProcessId -Force }
Get-Process vstest.console,testhost,testhost.x86,dotnet-coverage -EA SilentlyContinue | Stop-Process -Force
```
Then re-check `Get-Process vstest.console,testhost,dotnet-coverage` == 0 AND the `Invoke-MSTest` pwsh count == 0 before running. A plain `Stop-Process` on the binaries alone is NOT enough — the surviving pwsh runner respawns testhosts. Give the clean run a generous timeout (>= 8 min) so it never times out and re-stacks. Note: leftover idle `MSBuild.exe` `nodemode` reuse-pool workers are harmless (near-zero CPU) and are NOT the cause — do not confuse them with a concurrent build. See [[project_concurrent_executor_same_worktree]].
