# Build After QfcHomeController Extraction (Cycle 2) — Issue #218

Timestamp: 2026-06-28T15-34

Command (toolchain order, step 2 then step 3):
1. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (genuine full recompile of QuickFiler)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` (P2-T4 prescribed command)

Invoked via `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` with dash-style switches.

EXIT_CODE: 0

Output Summary:
- Analyzer build (genuine recompile, 6.01s): Build succeeded, 0 Error(s), 47 Warning(s). The 47 warnings are the same pre-existing CS8632 (nullable-annotation-context) warnings in `TaskMaster.Test`, unrelated to the extraction.
- Nullable build (P2-T4, toolchain order immediately after analyzer build, 1.25s): Build succeeded, 0 Error(s), 0 Warning(s). EXIT_CODE 0.
- The QfcHomeController extraction compiles cleanly and preserves the `IQfcHomeController` public surface. The analyzer build genuinely recompiled the entire QuickFiler assembly and resolved every reference to the moved metrics methods (`QfcHomeController.Metrics.cs`) and iteration methods (`QfcHomeController.Iteration.cs`) across the partials. Cross-partial references (e.g., `NonBlockingProducer` -> `TimedConsumerAsync`/`_metrics` fields kept in the main file; `Iterate2` -> `IterateQueueAsync`) resolve correctly under partial-class semantics.

Note: the same pre-existing whole-assembly nullable-debt condition documented in `build-after-qfcdatamodel-cycle2-218.md` applies (a standalone forced nullable recompile surfaces ~495 pre-existing errors across untouched QuickFiler/EmailFiler files). It is out of scope for issue #218 and unaffected by this verbatim extraction. The prescribed toolchain order (analyzer build then nullable build) yields EXIT_CODE 0, matching cycle 1's gate behavior.
