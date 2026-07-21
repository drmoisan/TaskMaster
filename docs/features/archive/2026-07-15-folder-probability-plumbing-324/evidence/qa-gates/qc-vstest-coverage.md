# QC — Tests + Code Coverage (Post-Change)

Timestamp: 2026-07-16T03-32

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage
Actual invocation (readable Cobertura via dotnet-coverage 18.5.2 wrapping VS18 vstest.console.exe):
  dotnet-coverage collect --output <cobertura.xml> --output-format cobertura -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation

EXIT_CODE: 0 (non-instrumented authoritative run) ; 1 (coverage-instrumented run — pre-existing Deedle flakiness only)

Output Summary:
Test counts (coverage-instrumented):
- Total 4231, Passed 4214, Failed 17.
- The +18 tests vs baseline are the new Folder tests (FolderScoreTests 7, FolderScorerRegressionTests 7, FolderRowTests 4); ALL 18 pass.
- The 17 failures are the identical pre-existing Deedle/DataFrame/ETL flakes seen at baseline under coverage instrumentation (e.g. FromDefaultFolder_*). NONE are FolderScore/FolderScorer/FolderRow/FolderPredictor tests. They pass with 0 failures when instrumentation is off.

Post-change coverage headline values (readable Cobertura):
- Repository/merged line-rate: 0.5942 (59.42%); branch-rate: 0.3037 (30.37%). (Baseline 59.35% / 30.28% — no regression; slight increase.)
- FolderScore (new file): line 100%, branch 100%.
- FolderRow (new file): line 100%, branch 100%.
- FolderScorer (primary class): line 97.85% (baseline 97.75%), branch 94.20% (baseline 94.20%).
- FolderPredictor (primary class): line 88.86% (baseline 86.71%), branch 88.28% (baseline 86.27%).

Per new-member line coverage:
- FolderScorer.OrderedScores: 100% (1/1)
- FolderScorer.ToScoredArray(): 100% (1/1)
- FolderScorer.ToScoredArray(int): 100% (1/1)
- FolderScorer.BuildScoredArray: 100% (11/11), branch 100% (empty / zero-guard / topN paths)
- FolderPredictor.FolderRowArray (get): 100% (12/12)
- FolderPredictor.FindFolderRows: 95.7% (22/23), branch 100%
- FolderPredictor.AddMatchRows: 100% (13/13), branch 87.5%
- FolderPredictor.AddSuggestionRows: 100% (9/9)
- FolderPredictor.AddRecentRows: 100% (16/16)

Verdict: all new members >= 90% line; both touched classes improved vs baseline (no reduction on changed lines). No production file excluded from measurement.
