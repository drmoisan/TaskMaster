# P3-T16 Build and scoped vstest run over the thirteen rewritten C# test classes

Timestamp: 2026-09-29T19-49
Command: the P3-T16 build payload (MSBuild resolved through vswhere, TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU", log coverage/logs/927-p3-build.log); the P3-T16 vstest payload (vstest.console.exe over the four test assemblies with /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation, the thirteen-clause FullyQualifiedName filter joined with a pipe, "/logger:trx;LogFileName=927-fixtures.trx" /ResultsDirectory:coverage\test-results, log coverage/logs/927-p3-vstest.log); the P3-T16 TRX reader
EXIT_CODE: 0
Output Summary:
- Build: BUILD-EXIT=0; the build log carries "Build succeeded." (SUCCEEDED=1) and a whole-line "0 Error(s)" (ZERO-ERRORS=1); warning summary line "0 Warning(s)".
- vstest exit code: 0 (the EXIT_CODE row).
- COUNTERS total=190 executed=190 passed=190 failed=0
- DISTINCT-CLASSES=14; every one of the thirteen class simple names in the filter appears as a substring of at least one CLASS| line. The fourteenth class, TaskMaster.Test.OutlookObjects.Store.StoresWrapperTests, is matched by the StoresWrapperTests substring clause and is recorded, not excluded.
- CLASS| QuickFiler.Test.Controllers.EfcSelectionGuardTests
- CLASS| TaskMaster.Test.AppGlobals.AppAutoFileObjectsFolderPredictorTests
- CLASS| TaskMaster.Test.AppGlobals.AppFileSystemFolderPathsMatchBestSpecialFolderTests
- CLASS| TaskMaster.Test.AppGlobals.AppFileSystemFolderPathsOneDriveResolutionTests
- CLASS| TaskMaster.Test.OutlookObjects.Store.StoresWrapperTests
- CLASS| ToDoModel.Tests.Data_Model.People.PeopleScoDictionaryNewTests
- CLASS| UtilitiesCS.Test.EmailIntelligence.EmailFilerConfig_Tests
- CLASS| UtilitiesCS.Test.EmailIntelligence.LcppnFolderPredictorStore_Tests
- CLASS| UtilitiesCS.Test.NewtonsoftHelpers.FilePathHelperConverterTests
- CLASS| UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemContractTests
- CLASS| UtilitiesCS.Test.OutlookObjects.Folder.FolderConverterIssue614Tests
- CLASS| UtilitiesCS.Test.OutlookObjects.Store.StoreFilterAttributionTests
- CLASS| UtilitiesCS.Test.OutlookObjects.Store.StoresWrapperDisableTests
- CLASS| UtilitiesCS.Test.OutlookObjects.Store.StoresWrapperTests
- The TRX stays under the ignored coverage/test-results directory and is not copied; this artifact records the counters and the class list only.
