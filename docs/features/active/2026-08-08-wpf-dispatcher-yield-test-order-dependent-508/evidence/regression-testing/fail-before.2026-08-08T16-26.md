# Fail-Before Evidence — `[expect-fail]` Probe

Timestamp: 2026-08-08T16-26

Task: [P0-T12] `[expect-fail]`

AC served: AC6.

## Result: GENUINE FAILING RUN PRODUCED

A failing run was obtained, so no `fail-before-exception.<ts>.md` dossier is required (see P0-T13).

## Mechanism

The pre-change `WpfDispatcherYield` class has **no seam**, so nothing can be injected into it. The
probe therefore arranges the ambient state instead: it marshals the existing unchanged call
`new WpfDispatcherYield().YieldAsync(CancellationToken.None)` onto a pumping STA thread the test
itself owns. On that thread `Dispatcher.FromThread(Thread.CurrentThread)` is non-null, so the first
operand of the production `??` resolves, `YieldAsync` completes normally, and the unchanged
`ThrowAsync<InvalidOperationException>()` assertion fails.

This is the defect stated positively: the test's outcome is decided by which thread it happens to
run on, not by anything it arranges.

## Probe edit (temporary, in place, reverted by P0-T14)

Edited `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` **in place**. No new
`.cs` file was added, because `UtilitiesCS.Test.csproj` is a legacy non-SDK project with explicit
`<Compile Include>` items (`UtilitiesCS.Test.csproj:334`) and adding a file would require a csproj
edit that P0-T14 and P1-T15 forbid.

The probe form of `YieldAsync_WithoutDispatcher_RemainsStrict`:

```csharp
[TestMethod]
[Timeout(30000)]
public async Task YieldAsync_WithoutDispatcher_RemainsStrict()
{
    using (var host = new ProbeStaDispatcherHost())
    {
        var dispatcherYield = new WpfDispatcherYield();

        Func<Task> act = () =>
            host.Dispatcher.InvokeAsync(
                    () => dispatcherYield.YieldAsync(CancellationToken.None)
                )
                .Task.Unwrap();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
```

The temporary `ProbeStaDispatcherHost` nested helper (modelled on
`FolderTreeSnapshotBuilderYieldTests.cs:118-147`) captures `Dispatcher.CurrentDispatcher` on an STA
thread, signals an `AutoResetEvent`, calls `Dispatcher.Run()` so the dispatcher genuinely pumps, and
shuts down with `BeginInvokeShutdown(DispatcherPriority.Send)` + `Join()`. `IsBackground = true` was
set on the host thread so an un-joined foreground thread could not delay testhost exit if the
timeout fired.

Constraints honored:

- The assertion is unchanged: still `ThrowAsync<InvalidOperationException>()`. Not weakened.
- The call under test is unchanged: still `new WpfDispatcherYield().YieldAsync(CancellationToken.None)`.
- `[Timeout(30000)]` bounds the probe so a composition mistake would surface as a timeout rather
  than a suite hang. It is part of the temporary edit and is removed by the P0-T14 revert.
- No production file was touched.

## Rebuild before running (mandatory — guards against a false pass)

Command: `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`
EXIT_CODE: 0 (6 warnings, 0 errors, 7.24s)

Note: a direct csproj build requires the project-level platform name `AnyCPU`; the solution-level
`Any CPU` spelling fails `_CheckForInvalidConfigurationAndPlatform` with "The BaseOutputPath/OutputPath
property is not set". The first attempt used `Any CPU` and errored; the retry with `AnyCPU`
succeeded. This is a platform-name mapping detail only, not a change of build semantics.

Rebuild proof — `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` last-write time:

| Point in time | LastWriteTime |
|---|---|
| Before the probe rebuild | 2026-08-08T16:18:36.0992567-04:00 |
| After the probe rebuild (and at probe run) | 2026-08-08T16:24:18.7130626-04:00 |

The timestamp advanced, so the executed assembly contains the probe edit. The stale-assembly false
pass is ruled out.

## Probe run

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Tests:YieldAsync_WithoutDispatcher_RemainsStrict`

EXIT_CODE: 1

```
VSTest version 18.8.0 (x64)
A total of 1 test files matched the specified pattern.
Test Parallelization enabled for ...\UtilitiesCS.Test.dll (Workers: 24, Scope: ClassLevel)
  Failed YieldAsync_WithoutDispatcher_RemainsStrict [235 ms]
  Error Message:
   Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.
  Stack Trace:
     at FluentAssertions.Specialized.AsyncFunctionAssertions`2.<ThrowAsync>d__7`1.MoveNext()
   ...
   at UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.<YieldAsync_WithoutDispatcher_RemainsStrict>d__1.MoveNext()
       in ...\UtilitiesCS.Test\OutlookObjects\Folder\WpfDispatcherYieldTests.cs:line 43

Total tests: 1
     Failed: 1
Test Run Failed.
 Total time: 1.2357 Seconds
```

## Why this is the right failure

The failure message is exactly the predicted one: **"Expected a `<System.InvalidOperationException>`
to be thrown, but no exception was thrown."** That is the FluentAssertions "did not throw" failure,
which proves `YieldAsync` ran to completion because a dispatcher was ambiently available on the
executing thread. It is not a compile error, not an infrastructure error, and not a timeout.

The run was bounded: the test itself took 235 ms and the whole run 1.2357 s, far inside the
`[Timeout(30000)]` budget. The hang hazard described in P0-T13 did not materialize, because the
owned dispatcher genuinely pumps via `Dispatcher.Run()`.

Output Summary: FAIL-AS-EXPECTED, EXIT_CODE 1. `YieldAsync_WithoutDispatcher_RemainsStrict`, edited
in place to run the unchanged call on an owned pumping STA thread, failed in 235 ms with
"Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown."
The test assembly was rebuilt first (DLL mtime 16:18:36 -> 16:24:18), so the failure came from the
edited code and not a stale assembly. This is a genuine failing run, so AC6 is satisfied by this
artifact and no exception dossier is needed.
