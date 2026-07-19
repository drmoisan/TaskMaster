# Phase 0 — Baseline Tests + Coverage (P0-T5)

Timestamp: 2026-07-18T08-52
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /Settings:<Cobertura-format coverage runsettings (Workers=4, ClassLevel; module excludes for Deedle/FSharp/Castle.Core/FluentAssertions/Moq/MSTest/test assemblies; default ExcludeFromCodeCoverage attribute excludes)>
EXIT_CODE: 0
Output Summary:
- Total tests: 4838; Passed: 4838; Failed: 0; Skipped: 0.
- Coverage report: TestResults\f2f1472c-7b80-4cb8-a0e1-833f353fd44f\DanMoisan_MEGALODON4_2026-07-18.08_45_10.cobertura.xml
- OVERALL (all instrumented modules loaded by the two test hosts): line 58.74% (42623/72557), branch 46.33% (9560/20635). The overall figure is depressed by assemblies loaded but not targeted by these two test projects (log4net 6.08%, TaskMaster 9.04%, Tags/ToDoModel/TaskVisualization 0%, System.Interactive/System.Linq.Async ~3%, SVGControl 16.22%, Mono.Reflection 39.30%).
- Feature-relevant package baselines (the like-for-like comparison basis for P9-T5):
  - UtilitiesCS: line 88.55%, branch 82.22%
  - QuickFiler: line 72.32%, branch 62.32%
