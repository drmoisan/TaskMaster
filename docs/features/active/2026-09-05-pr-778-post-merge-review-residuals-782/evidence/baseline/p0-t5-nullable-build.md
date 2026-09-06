# Baseline — Nullable Build (P0-T5)

Timestamp: 2026-09-05T19-26

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:n '/flp:LogFile=coverage\782-p0-nullable.log;Verbosity=normal'
```

The `/flp:` switch is written in single quotes because PowerShell would otherwise truncate it at the
first semicolon and no log file would be produced. `/p:Nullable=enable` was not added and
`/t:Build` was not substituted.

EXIT_CODE: 0

BASELINE_CORECOMPILE_COUNT: 81

Output Summary:

The summary warning and error lines, verbatim:

```text
    0 Warning(s)
    0 Error(s)
```

Both hard conditions hold.

**Log line count: observed 11990, expected 11903.** A difference in log length alone is not a
failure, per the task text, and the observed value is recorded here beside the expectation.

**`CoreCompile` count: observed 81, expected 51.** The recorded value differs from the tabled
expectation, so the task's record-and-continue escape is invoked and
`BASELINE_CORECOMPILE_COUNT: 81` is recorded above. P7-T4 derives its expected value from that
recorded observation rather than from the tabled 51.

The recorded figure is the quantity the task's `Output Summary:` instruction defines: the number of
lines in the log containing the token `CoreCompile`. P7-T4's `Output Summary:` instruction uses the
same phrase, so the baseline and the final figure are produced by one measurement method and remain
comparable.

The 81 token-bearing lines decompose exactly as follows:

| Form | Count |
|---|---|
| Target-header lines matching `^(\d+>)?CoreCompile:$` after trimming | 63 |
| `Deleting file "...csproj.CoreCompileInputs.cache".` lines | 18 |
| Total lines containing the token `CoreCompile` | 81 |

The 18 cache-deletion lines are one per built project and are deterministic. The 63 target-header
lines are not equally stable: the build runs under `/m`, and the file logger re-emits a node-prefixed
target header each time it switches node context, so the header count depends on how the parallel
nodes interleave rather than on how many times the target ran. That mechanism is the most likely
reason this figure does not reproduce the tabled 51, and it means the aggregate 81 may vary between
otherwise identical runs. If P7-T4's count differs from 81, the 18 deterministic cache-deletion
lines are the stable secondary comparator and are recorded here for that purpose.

Thirty-six lines in the log reference `csc.exe`, which is recorded for information only; no
acceptance condition in this plan reads that figure.
