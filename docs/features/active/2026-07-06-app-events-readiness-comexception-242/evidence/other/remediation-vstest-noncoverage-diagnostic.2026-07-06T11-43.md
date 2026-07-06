Timestamp: 2026-07-06T12-01
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
EXIT_CODE: 1
Output Summary:
The diagnostic VSTest command without `/EnableCodeCoverage` still fails due missing `System.Threading.Tasks.Extensions, Version=4.2.0.1`.
Total tests: 199.
Passed: 164.
Failed: 35.
Primary failure signature:
`System.TypeInitializationException: The type initializer for 'Moq.Async.AwaitableFactory' threw an exception. ---> System.IO.FileNotFoundException: Could not load file or assembly 'System.Threading.Tasks.Extensions, Version=4.2.0.1, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies.`
The approved `/EnableCodeCoverage` VSTest command passed in `docs/features/active/2026-07-06-app-events-readiness-comexception-242/evidence/qa-gates/remediation-vstest-coverage.2026-07-06T11-43.md`.
