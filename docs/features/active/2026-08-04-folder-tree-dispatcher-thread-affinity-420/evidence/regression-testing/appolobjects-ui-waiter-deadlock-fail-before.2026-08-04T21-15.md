Timestamp: 2026-08-04T21-15
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:DefineConstants='TRACE;DEBUG;REMEDIATION_P1_T8' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 1
Output Summary: The TaskMaster.Test compilation failed with CS0117 because AppOlObjects does not contain VerifyCapturedStaCanRunQueuedFolderTreeServiceCompositionAsync. The deliberate red test has no runtime execution, timeout, timer, polling loop, or blocked STA thread.

## Missing seam

`AppOlObjects.VerifyCapturedStaCanRunQueuedFolderTreeServiceCompositionAsync(Dispatcher)` is the narrow protected/internal, instance-scoped composition-ownership seam required by P2-T8. The `TestableAppOlObjects` subclass exposes it only to the test class. The deliberate red test is guarded by `REMEDIATION_P1_T8` so the remaining planned red tests can compile and execute before P2-T8 supplies the seam.

## Expected assertion after P2-T8 and P2-T10

`FolderTreeService_WorkerFirstComposition_AllowsCapturedStaQueuedWork` requires the seam to return `true` only when the captured STA executes queued folder-tree composition without synchronously waiting for worker-owned initialization.

## Assembly and compiler result

- Target assembly: `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
- Source: `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`
- Diagnostic: `CS0117: 'AppOlObjects' does not contain a definition for 'VerifyCapturedStaCanRunQueuedFolderTreeServiceCompositionAsync'`
- Result: expected compile failure before the P2-T8 composition-ownership seam exists.
