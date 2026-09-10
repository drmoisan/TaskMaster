# Final QA loop, toolchain step 2 — analyzer build (issue #826, [P7-T2])

Timestamp: 2026-09-09T19-52

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:LogFile=coverage/826-raw/p7-t2-analyzers.log;Verbosity=detailed"
```

resolved through `vswhere` and run as one `pwsh -NoProfile -Command` block carrying the plan's C2
preamble branch guard. `/v:q` was added to the console channel only; the detailed-verbosity file logger
the figures are read from is unaffected.

EXIT_CODE: 0

## Gate figures read from `coverage/826-raw/p7-t2-analyzers.log`

| Figure | Observed | Required |
|---|---|---|
| ` 0 Error(s)` (`-SimpleMatch`, leading space load-bearing) | 1 | at least 1 |
| `Skipping target "CoreCompile"` (`-Pattern`) | 0 | 0 |
| `Task "Csc"` (`-Pattern`) | 18 | at least 1 |
| `RS0030` (`-SimpleMatch`) | 0 | recorded, not asserted |

The exit code is asserted together with the verbatim summary token ` 0 Error(s)` rather than against the
absence of the substring `error`, because a successful msbuild prints that substring inside switch names
and summary text. The leading space in the token is load-bearing: `0 Error(s)` without it is a substring
of `10 Error(s)`, so a build reporting 10 errors would satisfy an at-least-1 gate on the unspaced form.

The `Task "Csc"` count of 18 is what makes the zero `Skipping target "CoreCompile"` non-vacuous.

## The RS0030 count of 0 is expected and is why AC10 is satisfied elsewhere

At `suggestion` severity RS0030 is an info-level diagnostic and does not reach the msbuild log at any
verbosity this command produces. A count of 0 here is therefore equally consistent with the ban working
and with it being inert, which is precisely why AC10 is satisfied through the SARIF channel certified in
[P4-T1] and observed in [P4-T2], and not from this log. That channel recorded 45 RS0030 diagnostics
across the three relevant projects, including all 15 sites under test, with three live controls firing.

Recording the 0 rather than asserting it is the plan's instruction, and it is the honest reading: this
log carries no evidence about RS0030 in either direction.

Output Summary: the solution rebuilds clean under `EnableNETAnalyzers` and `EnforceCodeStyleInBuild` and
exits 0, with one ` 0 Error(s)` summary line, zero skipped `CoreCompile` targets and 18 compiler
invocations. Toolchain step 2 passes.
