# Baseline nullable build (issue #826, [P0-T8], toolchain step 3)

Timestamp: 2026-09-09T19-05

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage/826-raw/p0-t8-nullable.log;Verbosity=detailed"
```

resolved through `vswhere` and run as one `pwsh -NoProfile -Command` block carrying the plan's C2
preamble branch guard. `/p:Nullable=enable` is not present and is never added: it is absent from the CI
command in `.github/workflows/_build-nullable.yml`, no project carries a `<Nullable>` element, and
adding it would conscript every file that never adopted the pragma. `/v:q` was added to the console
channel only; the detailed-verbosity file logger the gate figures are read from is unaffected.

EXIT_CODE: 0

## Gate figures read from `coverage/826-raw/p0-t8-nullable.log`

| Figure | Observed | Required |
|---|---|---|
| ` 0 Error(s)` (`-SimpleMatch`, leading space load-bearing) | 1 | at least 1 |
| `CS0169` (`-SimpleMatch`) | 0 | 0 |
| `CS0414` (`-SimpleMatch`) | 0 | 0 |
| `Skipping target "CoreCompile"` (`-Pattern`) | 0 | 0 |
| `Task "Csc"` (`-Pattern`) | 18 | at least 1 |

The `Task "Csc"` count of 18 is what makes the three zero counts non-vacuous. A log recording no
compilation at all would satisfy the CS0169, CS0414 and Skipping-target zeros equally well; a log
recording 18 compiler invocations cannot.

Output Summary: the nullable gate passes clean at the base anchor with `TreatWarningsAsErrors` in force,
and neither CS0169 nor CS0414 is present. This is the pre-change reference the AC3 gate in [P7-T3]
compares against, and it establishes that any CS0169 or CS0414 appearing later is introduced by this
feature's `TreeNode` edits rather than pre-existing.

---

Timestamp correction, recorded rather than absorbed: the `Timestamp:` value in this artifact was initially written as an extrapolated value rather than read from the clock. At 2026-09-09T19-10 the executor read the real clock, observed the discrepancy, and replaced the value with this artifact's own observed filesystem write time. The corrected value is an observation; the superseded value was not.
