# Baseline — Nullable Build (P0-T5, re-recorded under SD23)

SUPERSEDED BASELINE RE-RECORDED: SD23

RE-ANCHORED BASE: 736c2cf2

Timestamp: 2026-09-05T21-57

## Why the earlier figures are superseded

An external actor rebased the feature branch from `a007f72e` onto `origin/main` at `77c6d314`
during execution. Every prior commit received a new SHA. The base commit the superseded record was
taken at, `b95a5252`, is orphaned and is no longer an ancestor of HEAD, so the figures it carried
describe a tree that is no longer this branch's baseline.

## Measurement method and measuring party

This gate was measured by the **orchestrator, not the executor**, at the re-anchored base commit
`736c2cf2`, by the temporary-restore method: the orchestrator restored the six Write Set source
files Phase 1 has changed so far — `UtilitiesCS/Threading/UiThread.cs`,
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`,
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — to their `pre-782-base` content with
`git checkout pre-782-base -- <those six paths>`, ran the four gates, restored those files to HEAD
in a `finally` block, and left the worktree clean and at HEAD afterwards.

The executor did **not** re-run the nullable build for this task, and this artifact does not present
the figures as an executor run.

Command (the orchestrator's command):

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:n '/flp:LogFile=coverage\782-p0-nullable.log;Verbosity=normal'
```

The `/flp:` switch is written in single quotes because PowerShell would otherwise truncate it at the
first semicolon and no log file would be produced. `/p:Nullable=enable` was not added and
`/t:Build` was not substituted.

EXIT_CODE: 0

That is the exit code the **orchestrator** observed, not an exit code the executor observed.

BASELINE_CORECOMPILE_COUNT: 84

BASELINE_CORECOMPILE_DELETION_COUNT: 18

Output Summary:

The summary warning and error lines, verbatim, as the orchestrator observed them:

```text
    0 Warning(s)
    0 Error(s)
```

Both hard conditions hold.

**Log line count: 11658.** The superseded run recorded 11990. A difference in log length alone is
not a failure.

**`CoreCompile` token-line count: 84**, an observation and not a gated figure.

**`CoreCompileInputs.cache` deletion-line count: 18**, one per project. This is the figure P7-T4
gates.

### Decomposition of the re-measured 84

| Form | Count |
|---|---|
| Node-prefixed target-header lines | 52 |
| Unprefixed `CoreCompile:` line | 1 |
| `Deleting file "...csproj.CoreCompileInputs.cache".` lines | 18 |
| Further node-interleaved repeats | 13 |
| Total lines containing the token `CoreCompile` | 84 |

### Decomposition of the superseded 81

| Form | Count |
|---|---|
| Node-prefixed target-header lines | 63 |
| `Deleting file "...csproj.CoreCompileInputs.cache".` lines | 18 |
| Total lines containing the token `CoreCompile` | 81 |

### Why the header-derived total is not gated (SD19, confirmed by measurement)

The header component moved from **63 to 52** across the two runs on a tree whose project set did not
change: no project was added and none removed between `b95a5252` and `736c2cf2`. That is the direct
confirmation of SD19's premise. The build runs under `/m`, and the file logger re-emits a
node-prefixed target header each time it switches node context, so the header count depends on how
the parallel nodes interleave rather than on how many times the target ran. An equality gate on the
aggregate would therefore fail on an unchanged tree, for a reason unrelated to this delivery.

The 18 cache-deletion lines were identical in both runs, one per project, and are the stable figure.
`TaskMaster.sln` declares 18 projects; this delivery adds none and removes none.

### Second independent non-vacuity observation

The re-measured log carries **36 `csc.exe` lines**, two per project across 18 projects. That is a
second independent signal that the compiler actually ran on every project, alongside the 18
deletion lines, so the recorded `0 Warning(s)` / `0 Error(s)` result is not the vacuous outcome of a
skipped `CoreCompile`.

P7-T4 derives its gated expectation from the `BASELINE_CORECOMPILE_DELETION_COUNT:` line above and
records the total beside the `BASELINE_CORECOMPILE_COUNT:` line as an observation; a difference
between the two totals is recorded and is not a failure.
