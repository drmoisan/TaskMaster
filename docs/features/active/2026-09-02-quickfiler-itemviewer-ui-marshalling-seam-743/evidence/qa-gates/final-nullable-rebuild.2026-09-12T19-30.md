# Phase 6 — Final QA loop, step 3: nullable Rebuild gate (P6-T4)

Task: [P6-T4]
Toolchain pass: 1

Timestamp: 2026-09-13T03-47
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true'` Run from the item worktree root via Set-Location inside one pwsh invocation, with the Command Reference tool resolution prepended (inner quoting inverted to single quotes; semantics identical); console output redirected to the ignored path `coverage\p6-t4-nullable.log`. `/p:Nullable=enable` was not added; `/t:Rebuild` was used, never `/t:Build`. Run while holding the shared machine build lock for item 743 (acquired 03:46:55, released immediately after the command returned). Outlook was closed.
EXIT_CODE: 0
Output Summary:
- `Build succeeded.`
- `    0 Warning(s)`
- `    0 Error(s)`
- `Time Elapsed 00:00:16.17`
- 20 `Done Building Project` lines in the log; no line of the form `error XXnnnn` or `warning XXnnnn` appears anywhere in the log (regex count 0).
- `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exists after the run (this is the assembly P6-T5 and P6-T6 execute).

Comparison against P0-T7 (`evidence/baseline/phase0-nullable-rebuild.2026-09-12T16-30.md`): the transcribed error line `    0 Error(s)` is character-for-character identical to the P0-T7 baseline line `    0 Error(s)`; the warning line `    0 Warning(s)` is likewise identical.
