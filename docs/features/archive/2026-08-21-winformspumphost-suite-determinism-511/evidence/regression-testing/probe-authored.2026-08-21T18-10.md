# Phase 1 — Regression Probe Authored (P1-T1, `[expect-fail]`)

Timestamp: 2026-08-22T09-55

Command:

```
# Edit, then the mandatory formatting step, then verification
dotnet tool run csharpier check .
dotnet tool run csharpier format .
dotnet tool run csharpier check .
grep -n "InitializeBool_ThroughThePumpHost_CompletesAndInitializesState|InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates|BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread" QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs
pwsh -NoProfile -Command "@(Get-Content -LiteralPath 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs').Count"
git diff --stat -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs
```

EXIT_CODE: 0

ExpectedExitCode: 0

Note on the expectation field: P1-T1 is an **authoring** task, not a test-execution task. Its own
exit code is the toolchain's, which must be 0. The `[expect-fail]` tag concerns the authored test's
*runtime* outcome, which is measured and recorded by P1-T3 and P1-T4; those artifacts carry
`ExpectedExitCode: 1`.

Output Summary:

## What was authored

`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` was appended to
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`, after the last existing
method in the partial class. The diff is **49 insertions, 0 deletions** — purely additive, no existing
line altered.

Post-format text of the added method:

```csharp
        /// <summary>
        /// #511/#571 regression probe: the shared pump harness must hand back an
        /// <c>ItemViewer</c> whose window handle already exists, created on the pump thread.
        /// Every pump-hosted test in this class marshals work through the viewer, and
        /// <c>Control.Invoke</c> throws on a handle-less control, so a harness that returns a
        /// viewer with no handle makes those tests fail. This probe reports the harness viewer's
        /// handle state directly, so a run in which the end-to-end tests happen to pass still
        /// records whether the handle was present.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: false).ConfigureAwait(false);

                // Act — read the marshalling predicate on the pump thread that owns the viewer.
                bool invokeRequiredOnPumpThread = await host.InvokeAsync(() =>
                        harness.Viewer.InvokeRequired
                    )
                    .ConfigureAwait(false);

                // Assert — the handle exists, so Control.Invoke cannot throw for want of one.
                harness
                    .Viewer.IsHandleCreated.Should()
                    .BeTrue(
                        because: "the harness must create the viewer's window handle on the pump thread"
                    );
                invokeRequiredOnPumpThread
                    .Should()
                    .BeFalse(
                        because: "the pump thread owns the viewer's handle, so no marshalling is required there"
                    );
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }
```

## Acceptance conditions, each verified

| Condition | Result |
| --- | --- |
| Method exists in that file with exactly that name | **Met** — declared at line 301 |
| Uses `[TestMethod]` | **Met** — with `[Timeout(PumpTimeoutMs)]` |
| `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` still declared at line 131 | **Met** |
| `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` still declared at line 175 | **Met** |
| File line count less than 500 | **Met** — 339 lines (161 of headroom) |

Both spec-cited line numbers were re-derived from the file rather than taken on trust, both before and
**after** the CSharpier format step, and both are unchanged at 131 and 175. Appending after the last
method rather than inserting between existing methods is what preserves them.

## Shape conformance

- Follows the Arrange-Act-Assert shape of the existing tests in the file.
- Constructs `WinFormsPumpHost`, calls `BuildPumpHarnessAsync(host, darkMode: false)`, restores in
  `finally` via `harness.Restore()`, and awaits `host.StopAsync()` — the same fixture protocol as
  every other test in this partial class. This preserves the `UiThreadDispatcherGate`
  acquire-and-release structure that Binding Constraint 4 protects.
- Asserts with FluentAssertions: `harness.Viewer.IsHandleCreated.Should().BeTrue(...)` and
  `invokeRequiredOnPumpThread.Should().BeFalse(...)`, where the second value comes from
  `await host.InvokeAsync(() => harness.Viewer.InvokeRequired)`.
- Both assertions carry a `because:` reason, so a failure message states the expected invariant.

## Prohibited constructs — none present

A scan of the diff for `Sleep`, `Delay`, `SpinWait`, `Retry`, and `retry` returned only the single
line `+        [Timeout(PumpTimeoutMs)]`. There is **no sleep, no retry, no `SpinWait`, and no timing
tolerance**. `PumpTimeoutMs` is the pre-existing `internal const int PumpTimeoutMs = 60000` declared at
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs:38`; its value was **not**
changed, consistent with Binding Constraint 6.

No temporary file is created by the test. No production file under `QuickFiler/` was touched. No
`.csproj` was touched: `QfcItemController.InitializationTests.Part3.cs` already carries a
`<Compile Include>` entry, which is why it is the only permitted home for new tests.

## Toolchain step 1 (formatting)

`csharpier check .` initially reported this one file as `Was not formatted` — CSharpier prefers the
lambda placed as `InvokeAsync(() =>` with the body on the following line. `csharpier format .` was
applied and the subsequent `csharpier check .` reported `Checked 1517 files in 6284ms.` with zero
unformatted files. `git status --porcelain` confirms the format step modified **only** this one source
file; no other tracked file changed, consistent with the P0-T12 baseline of zero unformatted files.

## Why this test is authored in Phase 1 rather than Phase 3

Recorded so a reviewer does not read it as phase drift. Two reasons, both from the plan:

1. The repository Bugfix Workflow requires a failing regression test **before** the fix.
2. It is the only instrument that reports the harness viewer's `IsHandleCreated` value on a run where
   the two end-to-end tests happen to pass — which the P0-T15 baseline already showed can occur.

Phase 3 authors the second named test and verifies both.
