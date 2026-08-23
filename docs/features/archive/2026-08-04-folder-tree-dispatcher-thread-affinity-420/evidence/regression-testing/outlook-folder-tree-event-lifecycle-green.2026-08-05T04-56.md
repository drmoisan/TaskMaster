# P5-T35 and P5-T36 green evidence

Timestamp: 2026-08-05T04:56:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~OutlookFolderTreeServiceTraversalCancellationTests"`

EXIT_CODE: 0

Output Summary: All seven focused traversal lifecycle tests passed. Retained notifications and in-flight scheduled-refresh faults are suppressed after disposal; copied snapshot callbacks run outside `_gate` and stop before later subscribers after disposal; caller cancellation preserves the caller token; and M3 cleanup remains exact-once, ordered, and outside the gate.
