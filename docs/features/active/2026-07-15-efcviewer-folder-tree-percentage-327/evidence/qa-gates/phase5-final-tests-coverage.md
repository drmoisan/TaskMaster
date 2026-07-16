# Phase 5 Final QA — Tests + Coverage (P5-T4)

Timestamp: 2026-07-16T02-32

Command: dotnet-coverage collect --settings cov.settings.xml --output phase5.cobertura.xml --output-format cobertura -- "vstest.console.exe" UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:cov.runsettings

EXIT_CODE: 0

Output Summary:
- Total tests 4762, Passed 4762, Failed 0.
- Repository LINE coverage: 77.5388% (lines-covered 109553 / lines-valid 141288).
- Repository BRANCH coverage: 53.1184% (branches-covered 13099 / branches-valid 24660).

Per-file coverage of the five new UtilitiesCS/OutlookObjects/Folder/ modules:
- UtilitiesCS.FolderSuggestionNode: line 100%, branch 100%.
- UtilitiesCS.FolderSuggestionTree: line 98.45%, branch 96.43% (2 uncovered defensive lines: the empty-path guard in LeafSegment and the null-row guard in IsBanner; never reached because BuildFromRows only feeds non-empty presented rows).
- UtilitiesCS.PercentageFormatter: line 100%, branch 100%.
- UtilitiesCS.FolderProbabilityAdapter: line 100%, branch 100%.
- UtilitiesCS.IFolderProbabilitySource: interface-only, no executable lines (legitimately excluded from measurement per general-unit-test.md).

All code-bearing new modules exceed the >= 90% line and >= 90% branch target for new code.
