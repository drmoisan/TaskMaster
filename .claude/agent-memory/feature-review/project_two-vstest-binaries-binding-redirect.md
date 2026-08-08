---
name: two-vstest-binaries-binding-redirect
description: TaskMaster has two vstest.console.exe binaries; the TestWindow one drops the app.config binding redirect and produces spurious Moq assembly-load failures. Use the TestPlatform one with /Settings:TaskMaster.runsettings.
metadata:
  type: project
---

Running a TaskMaster test assembly with `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe` and no `/Settings:` produces mass spurious failures: every test that touches Moq fails with `System.TypeInitializationException` for `Moq.Async.AwaitableFactory` wrapping `FileNotFoundException: Could not load file or assembly 'System.Threading.Tasks.Extensions, Version=4.2.0.1'`. The assembly IS present in the output directory and `app.config` DOES carry a `0.0.0.0-4.2.4.0 -> 4.2.4.0` redirect (on-disk package is 4.6.3); that host simply does not apply it.

The correct invocation, which is what `scripts/vscode/Invoke-MSTestWithCoverage.ps1` resolves via `vswhere`, is:

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' <assembly> /Settings:TaskMaster.runsettings [/TestCaseFilter:...]
```

On #503 the wrong host gave `Total tests: 69, Passed: 43, Failed: 26`; the right host gave `69/69, EXIT 0` on the identical binaries.

**Why:** CLAUDE.md CUT3 prescribes only `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` — it pins neither the binary nor the runsettings file, so a reviewer reaching for the first `vstest.console.exe` on disk will manufacture a 26-test "regression" that does not exist and may wrongly FAIL a green change.

**How to apply:** Always use the `Extensions\TestPlatform` path plus `/Settings:TaskMaster.runsettings` when re-running tests during review. The tell that a failure set is a host artifact rather than a real regression: unrelated pre-existing tests in the same assembly fail with the identical assembly-load exception (on #503, three `FolderTree`/`FolderSnapshot` tests failed alongside the 23 feature tests). This is a different mechanism from [[vstest-argument-order-transitive-dep]] (#418), where ordinal position on the command line decided the outcome — check both before concluding a changed assembly is broken.
