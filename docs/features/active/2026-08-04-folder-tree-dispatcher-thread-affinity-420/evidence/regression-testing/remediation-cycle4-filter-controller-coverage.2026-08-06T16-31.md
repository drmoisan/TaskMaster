Timestamp: 2026-08-06T16-31
Command: `dotnet tool run csharpier format UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.Coverage.cs`; then `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; then `vstest.console.exe UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests" /InIsolation /Logger:"console;verbosity=normal"`.
EXIT_CODE: 0
Output Summary: Formatting passed. The analyzer build passed with zero errors and six known warnings: five packages.config warnings and the existing duplicated `PercentageFormatterTests.cs` source warning. The focused fixture passed 13/13 deterministic tests. The coverage partial is 258 lines, `FilterOlFoldersController.Lifecycle.cs` is 496 lines, and the partial has exactly one adjacent `Compile` entry in `UtilitiesCS.Test.csproj`.

## Testability seam and targeted coverage

- `ViewerFactory_ShowFailure_DisposesCandidateAndPreservesOriginalException` covers factory-created viewer show failure and candidate disposal while preserving the original exception.
- `ViewerFactory_InvokeRequiredClose_ClosesOnceAndPreservesConstructionFailure` covers the marshaled `CloseViewerAfterInitializationFailure` branch with exact fault identity and one close.
- `ViewerFactory_InvokeFault_IsContainedWithoutReplacingConstructionFailure` covers invoke-fault containment while preserving the construction exception.
- `CompatibilityFactory_DisposeAfterConstruction_DiscardsCandidate` uses the P5-T42 compatibility factory seam to exercise the late disposed branch after a sealed compatibility view is created.
- `CandidateCreated_DisposeBeforeCommit_DiscardsCandidate` uses the P5-T42 candidate-created hook to exercise the disposed branch in `TryCommitFolderTreeView`.
- Existing fixture cases cover queued dispose before initialization and refresh, ArchiveRoot callbacks, tree-view getter disposal, request creation, subscription attachment/detachment, refresh observation, candidate commit, no retained handler, and no post-dispose view mutation.

The seam is justified in `remediation-cycle4-testability-seam.2026-08-06T16-16.md`. No real viewer, message loop, reflection, global mutable hook, global dispatcher mutation, live Outlook, network resource, temporary file, timer, or polling loop was used. Final changed-method coverage remains subject to P5-T46.
