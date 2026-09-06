# Baseline — CSharpier Check (P0-T3, re-recorded under SD23)

SUPERSEDED BASELINE RE-RECORDED: SD23

RE-ANCHORED BASE: 736c2cf2

Timestamp: 2026-09-05T21-55

## Why the earlier figure is superseded

An external actor rebased the feature branch from `a007f72e` onto `origin/main` at `77c6d314`
during execution. Every prior commit received a new SHA. The base commit the superseded record was
taken at, `b95a5252`, is orphaned and is no longer an ancestor of HEAD, so the figure it carried
describes a tree that is no longer this branch's baseline. The main advance added one file to
`QuickFiler.Test`, which is why the checked-file count rose by one.

The superseded figure was `Checked 1580 files`. The re-measured figure is `Checked 1581 files`.

## Measurement method and measuring party

This gate was measured by the **orchestrator, not the executor**, at the re-anchored base commit
`736c2cf2`, by the temporary-restore method: the orchestrator restored the six Write Set source
files Phase 1 has changed so far — `UtilitiesCS/Threading/UiThread.cs`,
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`,
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — to their `pre-782-base` content with
`git checkout pre-782-base -- <those six paths>`, ran the four gates, restored those files to HEAD
in a `finally` block, and left the worktree clean and at HEAD afterwards.

The executor did **not** re-run this gate for this task, and this artifact does not present the
figure as an executor run. A run against the current tree would measure the Phase 1 tree, not the
baseline.

Command (the orchestrator's command, run from the worktree root):

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier check .
```

The `DOTNET_ROOT` / `PATH` preamble is required because `global.json` pins SDK 8.0.205 and the host
SDK cannot satisfy it.

EXIT_CODE: 0

That is the exit code the **orchestrator** observed, not an exit code the executor observed.

Output Summary:

The printed count line, verbatim, as the orchestrator observed it:

```text
Checked 1581 files
```

BASELINE_CHECKED_FILES: 1581

P7-T2 derives its expected value from the `BASELINE_CHECKED_FILES:` line above rather than from any
figure tabled in the plan, so that line is load-bearing and is written as a bare integer with no
surrounding text. The Phase 7 expectation is that recorded value plus exactly two, which is
`Checked 1583 files`, the plus-two being the two files this delivery creates.
