---
name: taskrun-triage-931
description: Issue #931 (#905+#906) triage — only 2 of 22 Task.Run sites in QuickFiler.Test are thread-identity dependent; DispatchValue never reads _ownerThreadId; IFileInfo.OpenRead returns FileStream so MemoryStream cannot replace the .sln fixture
metadata:
  type: project
---

Of 22 `Task.Run` call sites in QuickFiler.Test, only `BreadcrumbPopupBoundaryCoverageTests.cs:58` (owner-only dispatcher, blocking GetResult) and `ItemViewerBreadcrumbThreadAffinityTests.cs:332` (null-owner escape) depend on the work item running off the owner thread. The other six issue-cited candidates exercise `BreadcrumbUiDispatcher` guards that decide by ambient `SynchronizationContext` reference identity or by the `_executingDispatcher` thread-static, never by thread id.

**Why:** `IsCurrentBoundary()` compares thread ids only when `_context == null`, and only `Dispatch(Action)` calls it; `DispatchValue<T>` faults for every non-callback caller when `_context == null` regardless of thread. So `BreadcrumbUiThreadDispatchTests.cs:301` (`ProductionCaptureWithoutUiContext_FailsFast`) passes on any thread; the #900 handoff (`p5-t14-follow-up-handoff...md` Entry 1, second citation) mis-attributed it to the owner-thread check. Also: `IFileInfo.OpenRead()/Open()/Create()/OpenWrite()` return the concrete `FileStream`, so the #906 replacement must be a test-owned `FileStream` (read-only, `FileShare.ReadWrite` open of the test's own assembly, the file's existing sentinel pattern) or a seam-based identity assertion; `MemoryStream` fits only the StreamReader/StreamWriter members. The wrapper seam already exists (`internal FileInfoWrapper(IFileInfo)` + IVT to UtilitiesCS.Test).

**How to apply:** When a future issue cites a `Task.Run` "other thread" site, read the guard first: classify AFFECTED only if the guard's decision reads `Environment.CurrentManagedThreadId`, `Thread` identity, or `CheckAccess()`. For broken-guard demonstrations at these two sites, no test seam exists (sealed dispatcher, private guard); use a temporary reverted production edit (M1) plus the #900 P3-T3 test-only inline mutation (M2). Related: [[taskrun-getresult-inlines-900]].
