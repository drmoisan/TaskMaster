# Post-Merge Step 0: Tool and NuGet Restore

Timestamp: 2026-09-29T19-53
HEAD: 55a50e9226173d39d3c169b0dc90400b430af59b (merge commit 55a50e922, origin/main merged into bug/tests-depend-on-uncontrolled-environment-931)
Command:
- dotnet tool restore (run from <repo-root>)
- pwsh -NoProfile -File scripts\vscode\Invoke-Restore.ps1 (the restore command recorded in evidence/baseline/p0-t5-nuget-restore.2026-09-29T08-56.md; absolute script path resolved at run time through Join-Path (Get-Location).Path; MSBUILDDISABLENODEREUSE set to 1; output teed to the git-ignored coverage\logs\postmerge.restore.log)
EXIT_CODE: 0

Output Summary:
- TOOL_RESTORE_EXIT_CODE: 0 ("Tool 'csharpier' (version '1.2.6') was restored."; "Restore was successful.")
- RESTORE_EXIT_CODE: 0 (restore log tail: "Build succeeded."; "0 Warning(s)"; "0 Error(s)")
- PACKAGE-DIR-COUNT: 174 (P0-T5 recorded 172 before the merge)
- MSTEST_ANALYZERS_441: True (packages\MSTest.Analyzers.4.4.1 exists)
- MEZIANTOU_30290: True (packages\Meziantou.Analyzer.3.0.290 exists)
- Observation: both folders were already present before the restore ran; the restore was run anyway, as instructed.

Acceptance: both exit codes 0 and both analyzer package folders exist. Both hold.
