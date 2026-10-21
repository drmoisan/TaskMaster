# Phase 0 — Nullable baseline (P0-T7)

Task: [P0-T7]
Timestamp: 2026-09-13T02-24
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true'` Run from the item worktree root via Set-Location inside one pwsh invocation, with the Command Reference tool resolution prepended; console output redirected to the ignored path `coverage\p0-t7-nullable.log`. `/p:Nullable=enable` was not added. Run while holding the shared machine build lock for item 743.
EXIT_CODE: 0
Output Summary:
- `Build succeeded.`
- `    0 Warning(s)`
- `    0 Error(s)`
- `Time Elapsed 00:00:18.13`
- All nineteen projects in the solution reached `Done Building Project ... (Rebuild target(s))`; no line of the form `error XXnnnn` or `warning XXnnnn` appears anywhere in the log.
- Environment note: this run follows the P0-T6 provisioning of the gitignored `packages\Meziantou.Analyzer.3.0.203` folder recorded in the P0-T6 artifact; without it this command fails identically with the two `CS0006` errors, because `<Analyzer Include>` items are unconditional.
