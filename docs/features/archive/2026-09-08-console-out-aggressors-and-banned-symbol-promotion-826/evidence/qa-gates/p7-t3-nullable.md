# Final QA loop, toolchain step 3 — nullable build (issue #826, [P7-T3])

Timestamp: 2026-09-09T19-54

This is the AC3 gate.

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage/826-raw/p7-t3-nullable.log;Verbosity=detailed"
```

resolved through `vswhere` and run as one `pwsh -NoProfile -Command` block carrying the plan's C2
preamble branch guard. `/v:q` was added to the console channel only.

`/p:Nullable=enable` is **not** added. It is absent from the CI command in
`.github/workflows/_build-nullable.yml`, no project in this repository carries a `<Nullable>` element,
and adding it would conscript every file that never adopted the pragma.

EXIT_CODE: 0

## Gate figures read from `coverage/826-raw/p7-t3-nullable.log`

| Figure | Observed | Required |
|---|---|---|
| ` 0 Error(s)` (`-SimpleMatch`, leading space load-bearing) | 1 | at least 1 |
| `CS0169` (`-SimpleMatch`) | 0 | 0 |
| `CS0414` (`-SimpleMatch`) | 0 | 0 |
| `Skipping target "CoreCompile"` (`-Pattern`) | 0 | 0 |
| `Task "Csc"` (`-Pattern`) | 18 | at least 1 |

The `Task "Csc"` count of 18 is what makes the three zero counts non-vacuous: a log recording no
compilation at all would satisfy them equally well, and a log recording 18 compiler invocations, one per
project in the solution, cannot.

`/t:Rebuild` rather than `/t:Build` is what makes the gate capable of failing at all. MSBuild's
up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` would return
exit 0 with `CoreCompile` skipped on every project and no compiler diagnostic produced; the zero
`Skipping target "CoreCompile"` count independently rules that state out.

## AC3 discharge

AC3 requires the CS0169 / CS0414 hazard in the two `TreeNode` files to be proven discharged by the
type-check step rather than by inspection. That is what this artifact records: after [P5-T6] deleted the
`DebugTextWriter` field, its assignment, the `Console.SetOut(tw);` call and the orphaned commented-out
`[ClassInitialize]` block from both files, the nullable gate exits 0 with zero `CS0169` and zero `CS0414`
occurrences. The companion inspection evidence, the whole-word `tw` count of 0 in both files, is in
`<FEATURE>/evidence/qa-gates/p5-t6-treenode-sweep.md`.

The same command at the [P0-T8] baseline also reported both counts at 0 with the same `Task "Csc"` count
of 18, so this run demonstrates that the item-1 sweep introduced neither diagnostic rather than that
neither was ever reachable.

Output Summary: the solution rebuilds clean under `TreatWarningsAsErrors` and exits 0, with one
` 0 Error(s)` summary line, zero CS0169, zero CS0414, zero skipped `CoreCompile` targets and 18 compiler
invocations. Toolchain step 3 passes and AC3 is satisfied.
