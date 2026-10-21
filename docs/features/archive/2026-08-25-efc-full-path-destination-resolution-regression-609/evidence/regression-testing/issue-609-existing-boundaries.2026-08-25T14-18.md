Timestamp: 2026-08-25T14-18
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue609_|FullyQualifiedName~Issue439|FullyQualifiedName~EmailFilerConfig_Tests" /InIsolation
EXIT_CODE: 0
Output Summary: 27 of 27 filtered compatibility tests passed. Passing coverage includes direct, ancestor, and immediate-child relative filing targets; archive-root boundary behavior; existing Issue439 row/lineage behavior; the Issue609 archive-relative suggestion projection; and the `@` mailbox single-prefix `EmailFilerConfig.ResolvePaths` regression. The VSTest executable directory was added to the process PATH before invoking the recorded command.
