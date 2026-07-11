# Baseline — Analyzer Build (full solution)

Timestamp: 2026-07-11T11-42
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (run from FEATURE_WORKTREE)
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 76 Warning(s). Warnings are pre-existing and unrelated to ScoDictionary (CS8632 nullable-context annotations in TaskMaster.Test, MSTEST0032 always-true assertion in QuickFiler.Test). No warnings originate from UtilitiesCS or UtilitiesCS.Test ScoDictionary code.

Precondition note: The initial baseline analyzer build (before package restore) failed with 36 errors from missing NuGet packages (System.ValueTuple targets, analyzer DLLs) and a pre-existing SVGControl SvgDocument/log4net reference gap — a fresh-worktree restore state, not a code defect. Ran `./scripts/vscode/Invoke-Restore.ps1` (EXIT 0, 169 packages restored), after which the build succeeds as recorded above.
