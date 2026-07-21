# Final QA — Nullable / Type-Check (P7-T3)

Timestamp: 2026-07-09T17-54
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 0 Warning(s). The nullable/TreatWarningsAsErrors gate is
green, run in strict order immediately after the analyzer build (matching repository baseline
behavior for this incremental gate). No new nullable warning-as-error is introduced by the refactor;
the genuine full-recompile TaskTree nullable-diagnostic set net-decreased from the pre-change baseline
(13) because the former MoveObjects `TreeListView`-null-argument diagnostics and the controller's
`GetSelectedTreeNode` try/catch returns were removed by the seam refactor.
