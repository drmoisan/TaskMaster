Timestamp: 2026-07-06T11-30
Command: msbuild TaskMaster.Test\TaskMaster.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
Output Summary:
- PASS: Project build completed after issue #242 production change.
- MSBuild warning count: 46; error count: 0.
- Issue #242 production constant present: True.
- Test DLL rebuild after production source timestamp: yes.
- TaskMaster.Test\bin\Debug\TaskMaster.Test.dll LastWriteTime=2026-07-06T11:30:29 Length=251904.

Output Tail:
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\AppGlobals\EngineInitTimingProbe.cs(57,57): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\TaskMaster.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\AppGlobals\EngineInitTimingProbe.cs(55,61): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\TaskMaster.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\AppGlobals\ApplicationGlobals.cs(243,57): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\TaskMaster.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\AppGlobals\NonBlockingDelay.cs(47,18): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\TaskMaster.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\AppGlobals\AppItemEngines.cs(57,34): warning CS0618: 'AsyncEnumerable.SelectAwait<TSource, TResult>(IAsyncEnumerable<TSource>, Func<TSource, ValueTask<TResult>>)' is obsolete: 'Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwait functionality now exists as overloads of Select. You will need to modify your callback to take an additional CancellationToken argument.' [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\TaskMaster.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\AppGlobals\AppEvents.cs(270,47): warning CS0618: 'AsyncEnumerable.WhereAwait<TSource>(IAsyncEnumerable<TSource>, Func<TSource, ValueTask<bool>>)' is obsolete: 'Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwait functionality now exists as overloads of Where. You will need to modify your callback to take an additional CancellationToken argument.' [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\TaskMaster.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\AppGlobals\AppEvents.cs(302,27): warning CS0618: 'AsyncEnumerable.ForEachAwaitAsync<TSource>(IAsyncEnumerable<TSource>, Func<TSource, Task>, CancellationToken)' is obsolete: 'Use the language support for async foreach instead.' [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\TaskMaster.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\Ribbon\RibbonController.Intelligence.cs(398,23): warning CS0618: 'AsyncEnumerable.ForEachAwaitAsync<TSource>(IAsyncEnumerable<TSource>, Func<TSource, Task>, CancellationToken)' is obsolete: 'Use the language support for async foreach instead.' [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\TaskMaster.csproj]


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj" (Build target) (1) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\OutlookObjects\Store\StoresWrapperTests.cs(390,27): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\TestableApplicationGlobals.cs(21,39): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs(227,43): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs(231,30): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\TestableApplicationGlobals.cs(25,26): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\AppToDoObjectsTests.cs(47,31): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\AppToDoObjectsTests.cs(48,33): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\EngineInitTimingProbeTests.cs(91,73): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\EngineInitTimingProbeTests.cs(136,80): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]

    46 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.12
