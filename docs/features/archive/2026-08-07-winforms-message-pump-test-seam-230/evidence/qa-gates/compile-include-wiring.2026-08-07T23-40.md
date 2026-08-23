# P7-T8 — csproj Wiring and Test-Discovery Verification (D9)

Issue: #230
Task: [P7-T8]

`QuickFiler.Test.csproj` is a legacy non-SDK `packages.config` project: a `.cs`
file with no explicit `<Compile Include>` entry silently does not compile, so its
tests never run while filtered runs still report green (D9). This task proves the
wiring took effect using two independent, wiring-sensitive checks.

## Check 1 — Every feature-added `.cs` file under `QuickFiler.Test/` is wired

- Timestamp: 2026-08-07T23-40
- Command: enumerate added files from `git status --porcelain -uall`, then test each
  against the literal `<Compile Include="...">` needle in
  `QuickFiler.Test/QuickFiler.Test.csproj`
- EXIT_CODE: 0
- Output Summary: `ADDED_COUNT=4`, **`UNWIRED_COUNT=0`**.

| Feature-added file | Wired | csproj entry |
|---|---|---|
| `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` | True | `<Compile Include="TestSupport\WinFormsPumpHost.cs" />` |
| `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` | True | `<Compile Include="TestSupport\WinFormsPumpHostTests.cs" />` |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | True | `<Compile Include="Controllers\QfcItemController.InitializationTests.Part2.cs" />` |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | True | `<Compile Include="Controllers\QfcItemController.InitializationTests.Part3.cs" />` |

The only other feature change to the csproj is these four `<Compile Include>`
entries (`git diff --stat` shows 4 insertions across two hunks); no other csproj
element was touched.

## Check 2 — Static `[TestMethod]` enumeration versus `/ListTests` discovery

### 2a. Static enumeration (read from source, so it cannot shrink when a file is unwired)

- Timestamp: 2026-08-07T23-40
- Command: parse every `[TestMethod]`-annotated method name from the two
  feature-added test files, plus every test-method name introduced by this feature
  into the three modified test files (taken from `git diff -U0` added lines)
- EXIT_CODE: 0
- Output Summary: **`STATIC_TESTMETHOD_COUNT=21`.**

```
AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread
BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread
Constructor_WhenHostStarts_CapturesWinFormsContextOnADistinctThread
CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing
CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController
Dispose_CalledTwice_IsANoOp
InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults
InitializeBool_ThroughThePumpHost_CompletesAndInitializesState
InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme
InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates
InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState
InvokeAsync_WhenWorkThrows_FaultsTheAwaitedTaskWithTheOriginalException
InvokeAsyncAction_WhenPosted_RunsOnThePumpThread
InvokeAsyncFactory_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue
PostingMembers_AfterStop_FaultWithObjectDisposedException
ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups
RunAsyncResult_WhenPosted_RunsOnThePumpThreadAndReturnsTheValue
RunAsyncResult_WhenWorkFaults_SurfacesTheOriginalUnwrappedException
RunAsyncVoid_WhenPosted_StartsAndResumesOnThePumpThread
RunAsyncVoid_WhenWorkFaults_SurfacesTheOriginalUnwrappedException
StopAsync_WhenThePumpLoopRecordedAnException_RethrowsIt
```

Per-source-file breakdown: `WinFormsPumpHostTests.cs` 13,
`QfcItemController.InitializationTests.Part3.cs` 5,
`QfcItemController.SeamFactoryTests.cs` 2,
`QfcItemController.ViewerSetupTests.cs` 1.

### 2b. Rebuild

- Timestamp: 2026-08-07T23-40
- Command: `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Rebuild -p:Configuration=Debug -p:Platform="AnyCPU" -v:m`
- EXIT_CODE: 0
- Output Summary: Full rebuild (not incremental) succeeded, so the discovered
  assembly reflects the current csproj `<Compile>` set exactly.

### 2c. Discovery

- Timestamp: 2026-08-07T23-40
- Command:
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /ListTests
  ```
- EXIT_CODE: 0
- Output Summary: **`DISCOVERED_TOTAL=867`** test methods discovered in
  `QuickFiler.Test.dll`.

### 2d. Comparison

- `STATIC_COUNT=21`
- `MISSING_FROM_DISCOVERY=0`
- Result: *"All statically-enumerated feature test names are present in /ListTests
  output."*

## Why both checks are wiring-sensitive

- The static `[TestMethod]` enumeration is read from **source**, so it does not
  shrink if a file is unwired — the expected set stays at 21 regardless.
- `/ListTests` reflects only what actually **compiled into the assembly**, and it
  was produced from a full `-t:Rebuild`.
- A name present in source but absent from discovery is precisely the D9 silent-
  failure condition. Zero names are missing, so no feature-added `.cs` file is
  silently excluded from the build.

Neither check depends on an executed-test count, so it is unaffected by the
overlapping filters used in the Phase 1-6 runs.
