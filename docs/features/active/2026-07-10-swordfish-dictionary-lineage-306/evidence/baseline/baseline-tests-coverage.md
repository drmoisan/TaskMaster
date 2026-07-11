# Phase 0 — MSTest + Coverage Baseline (P0-T5)

Timestamp: 2026-07-11T03-22

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage

EXIT_CODE: 1

Output Summary:

Test counts (baseline):
- Total tests: 4519
- Passed: 4518
- Failed: 1
- Total time: 49.80 seconds

Single baseline failure (pre-existing, out of F1 scope):
- `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` — `System.Threading.Tasks.TaskCanceledException: A task was canceled.` at 25 s (thrown from `UtilitiesCS\Extensions\DictionaryExtensions.cs:line 177`).
- This is a known timing/timeout flake that surfaces under coverage instrumentation combined with default full-suite parallelism (24 workers): the async operation exceeds its internal cancellation window when the host is under coverage-instrumented load. The test targets `DictionaryExtensions.TryAddValuesAsync`, which is not a `ScoDictionary` / `ScoDictionaryNew` consumer and is not in the F1 scope-lock. The failure is unrelated to the lineage migration this feature performs. It is recorded here as the authoritative baseline state so the Phase 9 (P9-T4/P9-T5) no-regression comparison uses an apples-to-apples reference.

Coverage (baseline, from the emitted `.coverage` attachment converted to Cobertura via `dotnet-coverage merge ... -f cobertura`):
- Coverage attachment: `TestResults\9c3ba962-2070-4c0b-bbd1-6c3390bbeb47\DanMoisan_MEGALODON4_2026-07-10.23_16_06.coverage`
- Repo-wide line coverage (merged, includes all instrumented assemblies including vendored packages): 63.18% (lines-covered 107,113 / lines-valid 169,538).
- Per scope-lock production file (line coverage; interface-only files map no executable regions):
  - `TaskMaster\AppGlobals\AppToDoObjects.cs`: 200/315 = 63.5%
  - `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`: 112/139 = 80.6%
  - `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs`: 360/396 = 90.9%
  - `UtilitiesCS\OutlookObjects\MailItem\EmailDetails.cs`: 115/139 = 82.7%
  - `UtilitiesCS\OutlookObjects\MailItem\EmailDetailsWrapper.cs`: 12/12 = 100.0%
  - `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`: 36/66 = 54.5%
  - `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`: interface-only, no executable lines mapped
  - `UtilitiesCS\Interfaces\IToDo\ISubjectMapEncoder.cs`: interface-only, no executable lines mapped
  - `UtilitiesCS\Interfaces\IOutlookObjects\IEmailDetailsWrapper.cs`: interface-only, no executable lines mapped

Environment notes:
- vstest.console.exe: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
- `MSYS_NO_PATHCONV=1` used to prevent git-bash path conversion of the `/EnableCodeCoverage` switch and relative DLL paths.
- Coverage numeric extraction: `dotnet-coverage.exe merge <attachment>.coverage -o <out>.cobertura.xml -f cobertura` (v18.5.2.0), then per-`<class>` `<line hits>` aggregation per filename. The binary `.coverage` format is not human-readable offline; Cobertura conversion is the numeric-coverage path used here and will be repeated identically at P9-T4/P9-T5 for a consistent comparison.
