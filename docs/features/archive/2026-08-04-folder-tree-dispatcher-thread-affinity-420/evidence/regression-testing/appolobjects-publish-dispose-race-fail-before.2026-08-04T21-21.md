Timestamp: 2026-08-04T21-21
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:DefineConstants=REMEDIATION_P1_T12 /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 1
Output Summary: The deliberate publication-before-completion regression failed to compile with CS0117 because AppOlObjects does not expose the planned instance-scoped publication/disposal interleaving seam. No production-state race, reflection, timing, live UI/Outlook host, or global hook was used.

## Required assertion after P2-T11 and P2-T12

`FolderTreeService_PublishThenDispose_DoesNotReturnDisposedServiceToWaiter` requires the real public getter protocol to pause after candidate publication and before initialization completion, dispose, release the waiter, and prove the waiter does not receive a service that has already been disposed.

## Compiler result

- Target assembly: `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
- Diagnostic: `CS0117: AppOlObjects does not contain VerifyPublishedFolderTreeServiceIsNotReturnedAfterDisposeAsync`.
- Result: expected red compile failure before the P2-T8 publication-completion interleaving seam exists.
