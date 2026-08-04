# Selection and Lifecycle Failure-Before Gate

Timestamp: 2026-07-21T23-04Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderBreadcrumbRouterSelectionConcurrencyTests|FullyQualifiedName~BreadcrumbCoordinatorLifecycleTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests" /Logger:"console;Verbosity=normal"`

EXIT_CODE: 1

Output Summary: Expected-failure gate accepted. VSTest resolved through `vswhere`, discovered all 14 filtered tests across both assemblies, and completed in 4.5843 seconds. Thirteen tests failed through intended assertions and the direct-selection control passed. No build, discovery, VSTest-resolution, display, infrastructure, environmental, or unrelated test failure occurred.

- Resolved VSTest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
- Total: 14; passed controls: 1; intended failures: 13.

## Post-start selection loss

- `UpgradeStarted_ClosedMoveToDuplicateRow_RemainsSelectedAfterReplacement` failed because replacement restored index `0` instead of the exact moved duplicate row at index `1`.
- `UpgradeStarted_OpenPendingMoveToDuplicateRow_CommitsExactMovedRow` failed because the moved pending row could not be committed after replacement.
- `UpgradeStarted_ActivationOfDuplicateRow_CommitsExactActivatedRow` failed because exact duplicate-row activation was rejected.
- `UpgradeStarted_DirectItemSelectionOfAnotherPath_SurvivesReplacement` passed as the non-duplicate control.

## Stale coordinator post, callback, and lifetime invalidation

- `OverlappingUpgrades_CurrentCompletionPostsOnceAndStaleCompletionPostsNothing` established one current render post, then failed because the stale completion added a second render post.
- `Clear_InvalidatesLateSuccessfulUpgradeBeforeAnyPostOrCallback` failed because a late success posted after clear.
- `ViewerResetThenReuse_InvalidatesLateFailureWithoutDuplicatingCurrentState` failed because output remained after the stale completion boundary.
- `Dispose_InvalidatesLateSuccessAndUnsubscribesBeforePostOrCallback` and `Dispose_InvalidatesLateFailureWithoutPostCallbackOrErrorMutation` failed because the coordinator has no disposable lifetime boundary to invalidate and unsubscribe outstanding work.

## Pending-open close and late show/focus suppression

- `CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent` failed because late factory completion showed the popup once after close.
- `CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus` failed because late readiness completion showed the popup once after close.
- `CloseCanceledFactory_AllowsOneFreshReopenWithoutLateMutation` failed because the stale pending open completed as open instead of canceled.
- `ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce` failed because toggle did not close the pending host.
- `AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce` failed because automatic selector close did not close the pending host.

The first unaccepted diagnostic attempt exposed a test-only unpumped WinForms synchronization context. The test harness was corrected, and P1-T12 through P1-T14 were rerun before this accepted gate. This artifact records only the valid, deterministic failure-before run.
