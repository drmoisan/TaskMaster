# [P1-T6] Analyzer build with the falsification mutation in place

Timestamp: 2026-09-06T01-39

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

This is the same command [P0-T8] recorded as the baseline, run with the [P1-T5] mutation in place at
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:65`.

EXIT_CODE: 0

Output Summary: the build succeeded, so the mutated tree is executable and the falsification
observation in [P1-T7] can be taken against a real run rather than derived. The final summary lines,
verbatim:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## What is and is not asserted here

Only the exit code is asserted by this task. A failing build would mean the falsification could not
be demonstrated at all, which is why the task exists; a passing build establishes only that the
mutated assembly was produced and can be executed by the next task.

The mutated line is longer than CSharpier's 100-column print width. That is not a build defect and
raises no analyzer diagnostic, as the zero warning count above shows. No formatter is run while the
mutation is in place, so the over-width line is not rewritten and the [P1-T8] revert restores the
line exactly.
