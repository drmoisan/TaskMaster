Timestamp: 2026-08-31T10:49:04-04:00
Command (build): `pwsh -NoProfile -Command '<resolved MSBuild> TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true'`
Build EXIT_CODE: 0
Build output: `(Rebuild target(s))` observed.
Command (tests): `pwsh -NoProfile -Command '<resolved vstest command> /TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests&TestCategory!=LiveOutlook /Logger:trx /ResultsDirectory:coverage\\testresults\\p5-t6'`
EXIT_CODE: 0
Output Summary: 10 tests passed; 0 failed. `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively` and `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` both passed.
