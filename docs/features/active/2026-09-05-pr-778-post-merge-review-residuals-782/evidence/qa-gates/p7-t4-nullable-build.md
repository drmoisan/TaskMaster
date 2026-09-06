# QA Gate — Final Toolchain Pass, Step 4: Nullable Build (P7-T4)

Timestamp: 2026-09-05T23-06

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:n '/flp:LogFile=coverage\782-p7-nullable.log;Verbosity=normal'
```

The `/flp:` switch is written in single quotes. PowerShell treats `;` as a statement separator, so
the bare form is truncated at the first semicolon and no log file is produced.

`/p:Nullable=enable` is deliberately not added. No project in this repository carries a `<Nullable>`
element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that
conscripts every file which has never adopted the pragma. CI omits it deliberately, and this command
is character-for-character CI's.

EXIT_CODE: 0

Output Summary:

```text
    0 Warning(s)
    0 Error(s)
```

## Gated figure — `CoreCompileInputs.cache` deletion lines

```text
CORECOMPILEINPUTS_CACHE_LINES=18
```

| Quantity | Value | Source |
|---|---|---|
| Baseline deletion-line count | 18 | `BASELINE_CORECOMPILE_DELETION_COUNT:` line of `evidence/baseline/p0-t5-nullable-build.md` |
| Observed deletion-line count | 18 | the run above |

**This is the only figure this task gates.** The 18 deletion lines are one per project cleaned,
`TaskMaster.sln` declares 18 projects, and this delivery adds no project and removes none, so the
figure is stable by construction.

## Observations, not gated

```text
CORECOMPILE_TOKEN_LINES=75
FLP_TOTAL_LINES=12176
```

| Quantity | Baseline | Observed | Difference |
|---|---|---|---|
| Total `CoreCompile` token lines | 84 | 75 | -9 |
| Total log lines | 11658 | 12176 | +518 |

Both are recorded as observations and neither is a failure, per SD19 and the measurement SD23
confirmed.

The reason the aggregate is not gated: under `/m` the file logger re-emits a node-prefixed target
header each time it switches node context, so the header count depends on how the parallel nodes
interleave rather than on how many times the target ran. That is established by measurement rather
than by mechanism alone — the same solution recorded 63 header lines in an 81-line total on the
superseded base and 52 header lines in an 84-line total at the re-anchored base, with no project
added or removed between the two runs. This run's 75 is a third value from the same
non-deterministic component. An equality gate on the aggregate would therefore fail on an unchanged
tree, for a reason unrelated to this delivery.

The `BASELINE_CORECOMPILE_COUNT:` figure of 84 is quoted above as the comparison the task asks for;
the difference of -9 is recorded and is not a failure.
