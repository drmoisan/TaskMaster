# Phase 0 — Analyzer build baseline

Timestamp: 2026-09-09T13-55

Task: [P0-T9]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

`/t:Rebuild` was used, not `/t:Build` (D4). A normal-verbosity file log was written to the session
scratchpad so the D6 non-vacuity observation could be derived mechanically; the log is a local run
output and is not committed.

EXIT_CODE: 0

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

BASELINE-ANALYZER-WARNINGS: 0
BASELINE-ANALYZER-ERRORS: 0
BASELINE-ANALYZER-PROJECTS-COMPILED: 18
SKIPPED-CORECOMPILE-OCCURRENCES: 0

The projects-compiled count is the number of distinct project names for which the log reports a
`CoreCompile` execution, derived by counting distinct values of the `Compilation request <name>,
PathToTool=` marker the `Csc` task prints inside `CoreCompile`. There were 18 such lines and 18
distinct names, so no project compiled twice and none was skipped. The 18 are QuickFiler,
QuickFiler.Test, SVGControl, SVGControl.Test, Tags, Tags.Test, TaskMaster, TaskMaster.Test,
TaskTree, TaskTree.Test, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test,
UtilitiesCS, UtilitiesCS.Test, VBFunctions and VBFunctions.Test.

The literal `Skipping target "CoreCompile"` occurs zero times in the log, so the rebuild was not
vacuous. Per D5 no assertion is made on the bare string `error`, which a successful msbuild run
prints many times inside csc command lines; the summary counters above are the assertion.

D7 check: the log contains zero occurrences of MSB3061 and zero of MSB3021, so no Outlook or
test-host process was holding the build output.

Output Summary: Solution-wide analyzer rebuild passed at exit 0 with 0 warnings and 0 errors.
18 distinct projects compiled and zero skipped `CoreCompile` targets, so the gate was non-vacuous.
No file-lock warnings.
