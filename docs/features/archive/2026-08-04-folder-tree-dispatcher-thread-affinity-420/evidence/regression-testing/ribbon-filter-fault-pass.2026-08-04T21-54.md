Timestamp: 2026-08-04T21:54:00-04:00
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /Tests:TaskMaster.Test.Ribbon.TryFunctionalityInConstructionTests.RibbonFolderFilterCallback_ObservesOriginalInitializationFaultOnce,TaskMaster.Test.Ribbon.TryFunctionalityInConstructionTests.RibbonFolderFilterCallback_SuccessfulInitializationDoesNotReportFailure
EXIT_CODE: 0
Output Summary: Both P1-T11 regressions passed. The controlled initialization exception reaches the injected log/user-error policy exactly once by object identity; a completed initialization does not invoke that policy. The test uses an instance-scoped delegate seam and no global mutable hook.
