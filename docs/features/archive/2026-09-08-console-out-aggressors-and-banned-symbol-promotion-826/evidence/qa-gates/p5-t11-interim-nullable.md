# Interim nullable build after the item-1 sweep (issue #826, [P5-T11])

Timestamp: 2026-09-09T19-43

This is an interim diagnostic build that discharges the CS0169 / CS0414 hazard of spec Risk 1 as early as
possible. It is not a toolchain-loop pass; the loop itself runs in Phase 7.

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage/826-raw/p5-t11-nullable.log;Verbosity=detailed"
```

resolved through `vswhere` and run as one `pwsh -NoProfile -Command` block carrying the plan's C2
preamble branch guard. `/p:Nullable=enable` is not present. `/v:q` was added to the console channel only;
the detailed-verbosity file logger the figures are read from is unaffected.

EXIT_CODE: 0

## Gate figures read from `coverage/826-raw/p5-t11-nullable.log`

| Figure | Observed | Required |
|---|---|---|
| `CS0169` (`-SimpleMatch`) | 0 | 0 |
| `CS0414` (`-SimpleMatch`) | 0 | 0 |
| ` 0 Error(s)` (`-SimpleMatch`, leading space load-bearing) | 1 | at least 1 |
| `Skipping target "CoreCompile"` (`-Pattern`) | 0 | 0 |
| `Task "Csc"` (`-Pattern`) | 18 | at least 1 |

The `Task "Csc"` count of 18 is what makes the two zero CS-code counts non-vacuous. A log recording no
compilation would satisfy them equally well; a log recording 18 compiler invocations, one per project in
the solution, cannot.

`/t:Rebuild` rather than `/t:Build` is what makes the run non-vacuous in the other direction: MSBuild's
up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` would have
returned exit 0 with `CoreCompile` skipped on every project, which the zero
`Skipping target "CoreCompile"` count independently rules out here.

## What this proves about the `TreeNode` edits

At the [P0-T8] baseline the same command reported CS0169 count 0 and CS0414 count 0 with the same
`Task "Csc"` count of 18. After [P5-T6] deleted the `DebugTextWriter` field, its assignment, the
`Console.SetOut(tw);` call and the orphaned commented-out `[ClassInitialize]` block from both `TreeNode`
files, the counts are still 0. Had the field been left behind, it would now be never used and CS0169
would fire; had only the call been deleted, the field would be assigned but never read and CS0414 would
fire. Both are compiler warnings that `/p:TreatWarningsAsErrors=true` promotes to build errors, so either
mistake would have produced a non-zero exit code here rather than a silent pass.

Output Summary: the solution rebuilds clean under `TreatWarningsAsErrors` after the whole 33-file item-1
sweep, with zero CS0169 and zero CS0414 occurrences in a log that records 18 compiler invocations and no
skipped `CoreCompile` target. Spec Risk 1 is discharged.
