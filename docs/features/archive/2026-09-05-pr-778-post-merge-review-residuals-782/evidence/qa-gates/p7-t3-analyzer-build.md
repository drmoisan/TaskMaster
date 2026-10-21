# QA Gate — Final Toolchain Pass, Step 3: Analyzer Build (P7-T3)

Timestamp: 2026-09-05T23-06

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

`/t:Rebuild` is used rather than `/t:Build`. MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project and runs no analyzers; the gate would then be unable to fail.

EXIT_CODE: 0

Output Summary:

```text
    0 Warning(s)
    0 Error(s)
```

## Project build-output line count

```text
PROJECT_BUILD_OUTPUT_LINES=18
```

The count is taken over lines of the arrow form `<ProjectName> -> <path>\bin\Debug\<Assembly>`.

| Quantity | Value | Source |
|---|---|---|
| Baseline project count | 18 | `BASELINE_PROJECT_COUNT:` line of `evidence/baseline/p0-t4-analyzer-build.md` |
| Observed count | 18 | the run above |

The expected value is located in the baseline artifact by its `BASELINE_PROJECT_COUNT:` token rather
than by a line number, because P0-T4 rewrites that artifact in place under SD23 and any line number
would be a citation into a superseded revision.

18 is also the number of projects `TaskMaster.sln` declares. This delivery adds no project and
removes none, so the count is expected to be identical to the baseline rather than merely close to
it, and it is.
