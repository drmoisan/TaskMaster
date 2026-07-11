# P9-T4 — Final MSTest + Coverage Gate

Timestamp: 2026-07-11T04-14

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage

EXIT_CODE: 0

Output Summary:

Test counts (final, fully green):
- Total tests: 4524
- Passed: 4524
- Failed: 0
- Total time: 28.85 seconds

Delta vs baseline (P0-T5): baseline was 4519 total / 4518 passed / 1 failed. The +5 tests are the new
`ScoDictionaryNew_OnDiskCompatibility_Tests` (P6). The single baseline failure
(`TryAddValuesAsync_UpdatesExistingValue`, a nondeterministic ~22 s timeout under coverage
instrumentation + default 24-worker parallelism, unrelated to F1) passed in this final run. It was
independently confirmed to pass in isolation (38 ms), and it also passed in the first of the
post-change full-suite runs, confirming it is a pre-existing timing flake rather than an F1
regression.

New F1 tests (all passed):
- DictRemap_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens
- FilteredFolderScraping_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens
- FolderRemap_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens
- SubjectMapEncoder_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens
- DefaultWritePath_ForAllPersistedTypes_NeverEmitsGlobalsWrapperTokens

Coverage (final, from the emitted `.coverage` attachment converted to Cobertura via
`dotnet-coverage merge ... -f cobertura`):
- Coverage attachment: `TestResults\34ff8786-9831-4ea6-8705-3399a584534b\DanMoisan_MEGALODON4_2026-07-10.23_56_51.coverage`
- Repo-wide line coverage (merged, includes all instrumented assemblies including vendored packages): 63.18% (lines-covered 107,189 / lines-valid 169,669).
- Per scope-lock production file (line coverage; interface-only files map no executable regions):
  - `TaskMaster\AppGlobals\AppToDoObjects.cs`: 200/315 = 63.5%
  - `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`: 113/140 = 80.7%
  - `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs`: 365/401 = 91.0%
  - `UtilitiesCS\OutlookObjects\MailItem\EmailDetails.cs`: 115/139 = 82.7%
  - `UtilitiesCS\OutlookObjects\MailItem\EmailDetailsWrapper.cs`: 12/12 = 100.0%
  - `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`: 36/66 = 54.5%
  - `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`: interface-only, no executable lines mapped
  - `UtilitiesCS\Interfaces\IToDo\ISubjectMapEncoder.cs`: interface-only, no executable lines mapped
  - `UtilitiesCS\Interfaces\IOutlookObjects\IEmailDetailsWrapper.cs`: interface-only, no executable lines mapped

Environment notes:
- vstest.console.exe: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
- `MSYS_NO_PATHCONV=1` used for the `/EnableCodeCoverage` switch and relative DLL paths.
- Numeric coverage extraction: `dotnet-coverage.exe merge <attachment>.coverage -o <out>.cobertura.xml -f cobertura` (v18.5.2.0), then per-`<class>` `<line hits>` aggregation by filename — the identical method used for the P0-T5 baseline, giving a consistent comparison.
