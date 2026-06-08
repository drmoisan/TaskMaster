---
name: vstest-binding-redirect-flakiness
description: Local vstest runs of UtilitiesCS.Test fail en masse with a System.Threading.Tasks.Extensions binding-redirect error that is environmental, not a code defect
metadata:
  type: project
---

Running the full `UtilitiesCS.Test` assembly via `vstest.console.exe` in the local
sandbox intermittently fails ~860+ Moq-using tests with:
`System.TypeInitializationException: The type initializer for 'Moq.Async.AwaitableFactory'
threw an exception ---> Could not load file or assembly 'System.Threading.Tasks.Extensions,
Version=4.2.0.1 ...'`.

The binding redirect (`0.0.0.0-4.2.4.0 -> 4.2.4.0`) IS present in
`UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll.config` and the physical
`System.Threading.Tasks.Extensions.dll` (v4.2.4.0) is in the bin dir. The vstest host
does not reliably apply the app.config binding redirect on uninstrumented full-assembly
or filtered runs. Runs WITH `/EnableCodeCoverage` change the load path and the redirect
applies, so coverage runs pass where plain runs fail.

**Why:** Pre-existing host/config interaction in the local environment, independent of
any source change. Present identically on baseline commits.

**How to apply:** When verifying C# test changes locally, do not treat a high
full-assembly failure count as a regression by itself. Compare failed-test-name SETS
between baseline and post-change on the same commit/environment (the binding noise
cancels in the delta), and run the affected classes WITH `/EnableCodeCoverage` to get a
deterministic pass for the tests under repair. Also note `ShellUtilities_Tests`
GetFileIcon tests and repo-root-walk filesystem tests are flaky under heavy parallelism
(Win32 handle exhaustion / contention) and flip between runs.
