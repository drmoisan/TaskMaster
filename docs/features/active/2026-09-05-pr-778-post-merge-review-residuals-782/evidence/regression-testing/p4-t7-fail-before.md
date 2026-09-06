# Regression Testing — Fail-Before for the Three New AC7 Tests (P4-T7)

Timestamp: 2026-09-05T22-40

Command:

```powershell
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1

$filter = 'FullyQualifiedName~YieldAsync_ProductionFallbackWithoutDispatcher_ThrowsNamingInit|FullyQualifiedName~InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException|FullyQualifiedName~Initialize_WhenDispatcherNotCaptured_ThrowsInvalidOperationException'

& $vstest `
    'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /Logger:trx `
    /ResultsDirectory:TestResults\782-p4-failbefore `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    "/TestCaseFilter:$filter"
```

The plain build is used deliberately, without `/p:TreatWarningsAsErrors=true`: the temporary edits
raise a nullable-flow warning that is expected and must not fail this build. The build recorded
`1 Warning(s)`, and it is exactly that expected warning:

```text
UtilitiesCS\OutlookObjects\Folder\WpfDispatcherYield.cs(68,19): warning CS8602: Dereference of a possibly null reference. [UtilitiesCS\UtilitiesCS.csproj]
```

The `/Blame:` switch is written in single quotes so PowerShell does not truncate it at the first
semicolon.

The filter selects exactly three tests. The `~` operator is a substring match, and
`InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException` does not contain the
substring `Initialize_WhenDispatcherNotCaptured_ThrowsInvalidOperationException`, so the two C26
clauses do not overlap. The observed `Total tests: 3` confirms this.

EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:

## The two temporary source edits, verbatim

Both edits are required together. Removing only the `UiThread` throw leaves the sibling guard in
`WpfDispatcherYield`, which throws the same exception type with the same shared constant, so the
C21 test would still pass and the demonstration would be vacuous.

### Edit 1 — `UtilitiesCS/Threading/UiThread.cs`

```diff
@@ -154,18 +154,7 @@ namespace UtilitiesCS
         {
             get
             {
-                // Read the non-volatile static exactly once so the guard and the return value
-                // cannot observe different values if another thread completes Init() in between.
-                Dispatcher? captured = _dispatcher;
-                if (captured is null)
-                {
-                    // Initialize() constructs and shows a hidden WinForms SyncContextForm, so it
-                    // has UI-thread affinity. A lazy Init() from an arbitrary reader is therefore
-                    // deliberately avoided here even though the sibling UiSyncContext and
-                    // AutoScaleFactor accessors do self-heal.
-                    throw new InvalidOperationException(DispatcherNotInitializedMessage);
-                }
-                return captured;
+                return _dispatcher!;
             }
             private set => _dispatcher = value;
         }
```

### Edit 2 — `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`

```diff
@@ -60,10 +60,10 @@ namespace UtilitiesCS.OutlookObjects.Folder
             // only injected providers, which are typed Func<Dispatcher?> and exist only in tests.
             Dispatcher? dispatcher =
                 _currentThreadDispatcherProvider() ?? _fallbackDispatcherProvider();
-            if (dispatcher is null)
-            {
-                throw new InvalidOperationException(UiThread.DispatcherNotInitializedMessage);
-            }
+            //if (dispatcher is null)
+            //{
+            //    throw new InvalidOperationException(UiThread.DispatcherNotInitializedMessage);
+            //}
 
             await dispatcher.InvokeAsync(
                 () => { },
```

## Run result

```text
Total tests: 3
     Failed: 3
Test Run Failed.
```

`Passed: 0` — vstest prints no `Passed:` line when the pass count is zero, and `Failed:` equal to
`Total tests:` is the same observation.

These are locally-filtered figures over one assembly, `UtilitiesCS.Test`, under the three-clause
`/TestCaseFilter` above. They are not CI figures.

## The three tests, their outcomes and verbatim failure messages

Absolute host paths in the messages below are replaced with `<worktree>`; nothing else is altered.

### 1. `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_ProductionFallbackWithoutDispatcher_ThrowsNamingInit`

Outcome: **Failed**

```text
Expected type to be System.InvalidOperationException because the production fallback must surface the uncaptured-dispatcher guard, but found System.NullReferenceException.
```

### 2. `UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException`

Outcome: **Failed**

```text
Expected a <System.InvalidOperationException> to be thrown, but found <System.NullReferenceException>:
System.NullReferenceException: Object reference not set to an instance of an object.
   at UtilitiesCS.Threading.ProgressTrackerAsync.<InitializeAsync>d__6.MoveNext() in <worktree>\UtilitiesCS\Threading\ProgressTrackerAsync.cs:line 35
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at FluentAssertions.Specialized.AsyncFunctionAssertions`2.<InvokeWithInterceptionAsync>d__15.MoveNext() in /_/Src/FluentAssertions/Specialized/AsyncFunctionAssertions.cs:line 373.
```

### 3. `UtilitiesCS.Test.ProgressTracker_Tests.Initialize_WhenDispatcherNotCaptured_ThrowsInvalidOperationException`

Outcome: **Failed**

```text
Expected a <System.InvalidOperationException> to be thrown, but found <System.NullReferenceException>:
System.NullReferenceException: Object reference not set to an instance of an object.
   at UtilitiesCS.ProgressTracker.Initialize() in <worktree>\UtilitiesCS\Threading\ProgressTracker.cs:line 35
   at UtilitiesCS.Test.ProgressTracker_Tests.<>c__DisplayClass22_0.<Initialize_WhenDispatcherNotCaptured_ThrowsInvalidOperationException>b__0() in <worktree>\UtilitiesCS.Test\Threading\ProgressTracker_ReportAndViewerTests.cs:line 222
   at FluentAssertions.Specialized.DelegateAssertions`2.InvokeSubjectWithInterception() in /_/Src/FluentAssertions/Specialized/DelegateAssertions.cs:line 173.
```

## Why the failures are attributable to the removed guards

Every one of the three messages names `System.NullReferenceException`, an exception type other than
`InvalidOperationException`, and none reports a harness fault such as a missing type, an
unresolvable assembly, or a timeout. The two stack traces that carry a production frame point at the
exact line each temporary edit exposed: `ProgressTrackerAsync.cs` line 35 and `ProgressTracker.cs`
line 35 are both the `UiDispatcher.Invoke`/`InvokeAsync` call that immediately follows the now
unguarded read, so the null reached the call site rather than being rejected by the accessor. The
C21 message reports the same substitution one level up: with the sibling guard in
`WpfDispatcherYield` commented out as well, the null dispatcher reached `InvokeAsync` there too.

The TRX was written to `TestResults\782-p4-failbefore\` under a filename generated by vstest from
the local account and machine names; that filename is deliberately not reproduced here, and no
absolute host path appears in this artifact.
