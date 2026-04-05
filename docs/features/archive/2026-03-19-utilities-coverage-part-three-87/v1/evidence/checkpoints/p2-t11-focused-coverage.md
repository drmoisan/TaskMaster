# P2-T11 Focused Coverage Checkpoint

Timestamp: 2026-03-22T17:25:29.8209973-04:00
Task: P2-T11
Command: `dotnet-coverage collect --settings coverage.config --output-format cobertura --output coverage\p2t11-focused-20260322.cobertura.xml -- vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~ObsoleteBayesianClassifier_Tests|FullyQualifiedName~ObsoleteClassifierGroup_Tests" /InIsolation`
EXIT_CODE: 0
Build Command: `scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU'`
BUILD_EXIT_CODE: 0

## Output Summary

- Focused tests run: 29
- Passed: 29
- Failed: 0
- Coverage artifact: `coverage/p2t11-focused-20260322.cobertura.xml`

## Target File Line Rate

- `UtilitiesCS/EmailIntelligence/Bayesian/Obsolete/BayesianClassifier.cs`: 95.18%
- `UtilitiesCS/EmailIntelligence/Bayesian/Obsolete/ClassifierGroup.cs`: 100.00%

## Result

- `P2-T11` acceptance satisfied; both target files are >= 80% line coverage in the focused artifact.
