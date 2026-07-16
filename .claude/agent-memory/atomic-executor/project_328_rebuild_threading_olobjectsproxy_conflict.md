---
name: project-328-rebuild-threading-olobjectsproxy-conflict
description: Threading StoresWrapper into AppToDoObjects.Rebuild callers (#328 P2-T6) breaks the out-of-scope OlObjectsProxy test double that only stubs get_App
metadata:
  type: project
---

On issue #328 (Outlook store exclusion), plan P2-T6 threads `Parent.Ol.StoresWrapper` into the
`ProjectData.Rebuild(olApp, storesWrapper)` calls at `TaskMaster/AppGlobals/AppToDoObjects.cs`
(`LoadProjInfoAsync` line 121 and `LoadProjInfo` line ~135).

**Why:** This breaks the existing test
`TaskMaster.Test` `LoadProjInfoAsync_RebuildsWhenProjectCountIsZeroAndOutlookApplicationIsAvailable`.
That test uses `OlObjectsProxy` (a hand-rolled `RealProxy` in
`TaskMaster.Test/AppGlobals/AppToDoObjectsTestDoubles.cs`, ~line 122-137) that implements ONLY
`get_App` and throws `NotSupportedException("Member '<name>' is not used by this test proxy.")` for
every other `IOlObjects` member. Because `Parent.Ol.StoresWrapper` is evaluated as a call argument
BEFORE `Rebuild` reaches the `get_Session` access the test asserts, the test gets NotSupportedException
instead of the expected InvalidOperationException. It fails even without coverage instrumentation
(distinct from the ~19 pre-existing Deedle/FSharp coverage-instrumentation flaky failures).

`AppToDoObjectsTestDoubles.cs` was NOT in the #328 plan Scope-Lock, so under a strict scope-lock
delegation the executor cannot fix it and must report a scope conflict.

**How to apply:** When a plan threads a previously-unused `IOlObjects` member into an AppToDoObjects
path, add `AppToDoObjectsTestDoubles.cs` to Scope-Lock. The one-line fix: in `OlObjectsProxy.Invoke`,
return `null` (fail-open) for `call.MethodName == "get_StoresWrapper"` alongside the existing `get_App`
branch — this preserves the test's intent (Rebuild still reaches and throws on `get_Session`). Related:
[[project_iapplicationglobals_member_forces_implementers]] (adding interface members breaks hand-written
stubs); the pattern here is USING an existing member that a minimal proxy never stubbed.
