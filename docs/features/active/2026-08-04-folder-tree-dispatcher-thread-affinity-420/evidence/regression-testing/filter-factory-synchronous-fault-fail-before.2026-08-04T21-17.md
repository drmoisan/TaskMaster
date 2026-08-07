Timestamp: 2026-08-04T21-17
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:CreateAsync_SynchronousFolderTreeServiceFault_ClosesFactoryViewerAndPreservesOriginalException,InjectedViewerConstructor_SynchronousFolderTreeServiceFault_ClosesViewerAndPreservesOriginalException
EXIT_CODE: 1
Output Summary: 2 deterministic fake-viewer tests failed as expected. Both preserve the original controlled InvalidOperationException, but each reports viewer.CloseCount 0 instead of 1 after synchronous FolderTreeService composition fails. No real viewer, message loop, reflection, timing, polling, or global mutable hook was used.

## Failing regressions

- `CreateAsync_SynchronousFolderTreeServiceFault_ClosesFactoryViewerAndPreservesOriginalException`: public `CreateAsync(IApplicationGlobals, Func<IFilterOlFoldersViewer>?)` receives a fake factory and exposes the original exception, but the created viewer is not closed.
- `InjectedViewerConstructor_SynchronousFolderTreeServiceFault_ClosesViewerAndPreservesOriginalException`: the injected-viewer construction path exposes the original exception, but the viewer is not closed.

## Common-path source evidence

`FilterOlFoldersController(IApplicationGlobals)` delegates to `FilterOlFoldersController(IApplicationGlobals, IFilterOlFoldersViewer)` after `CreateAndShowViewer()`. `CreateAsync` also constructs through that injected-viewer constructor. Consequently, P3-T6 must establish one cleanup helper at that construction boundary so the legacy constructor and factory path share the same synchronous-failure cleanup contract.
