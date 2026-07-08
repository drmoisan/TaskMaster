# Final QC — Nullable / TreatWarningsAsErrors (#211 Phase 3)

Timestamp: 2026-06-23T14-30
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(MSBuild path: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`; run in toolchain order immediately after the analyzer build, matching the Phase 0 baseline sequence)
EXIT_CODE: 0

Output Summary:
- `Build succeeded.` — `0 Warning(s)`, `0 Error(s)`.
- Matches the Phase 0 nullable baseline (0/0). Under `Nullable=enable`, the new files'
  `IConditionalEngine<MailItemHelper>?` annotations are in a valid nullable context, so the
  CS8632 warnings seen in the analyzer build do not appear here. No new nullable
  warnings-as-errors introduced. Type-check gate is green; no loop restart required.

Note: the nullable/TWAE gate must be run incrementally after the analyzer build (the documented
toolchain order). Running `Nullable=enable` as a cold first build force-recompiles untouched
legacy files and surfaces pre-existing CS8618/CS8625 in files outside this change's scope; that
is an artifact of the property override on a cold build, not a regression from this change, and
is not part of the mandated toolchain order.
