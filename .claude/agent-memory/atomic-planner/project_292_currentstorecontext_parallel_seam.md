---
name: project-292-currentstorecontext-parallel-seam
description: CurrentStoreContext is a process-global static; scope-opening store test classes in UtilitiesCS.Test must be [DoNotParallelize] or they pollute the reader-baseline tests under ClassLevel parallelization
metadata:
  type: project
---

`UtilitiesCS/Threading/CurrentStoreContext.cs` is a deliberately process-global `static volatile string _current` (issue #260/#264 watchdog reads it cross-thread, so it is NOT `AsyncLocal`/`ThreadStatic` — do not propose changing that).

**Why:** `UtilitiesCS.Test` runs `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]` (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`). Any test class that (transitively) opens a `CurrentStoreContext.Begin(...)` scope is a writer of the global. Production scope-open sites: `StoresWrapper.MaterializeFilteredStores` (`<Stores-enumeration>`, added in #292), `StoresWrapper.AddOrRestoreStore`, and `StoreWrapper.Init` (per-store `DisplayName`). Reader tests `CurrentStoreContextTests` and `ThreadMonitorTests` assert `CurrentStoreContext.Current == null` and are `[DoNotParallelize]`, but MSTest runs the non-parallel bucket concurrently with the parallel bucket, so a `[DoNotParallelize]` reader still observes a parallel writer's `_current`. This produced 10 Blocking CI failures on #292.

**How to apply:** When planning any change that adds a `CurrentStoreContext` scope or a new `StoresWrapper`/`StoreWrapper` test class, require `[DoNotParallelize]` on every scope-opening test class so all readers and writers share the single serialized MSTest bucket (mutual exclusion, not a probability reduction). Do NOT weaken reader assertions and do NOT de-`AsyncLocal` the production type. Full de-parallelizing the assembly also works but serializes the largest test assembly; the surgical per-class `[DoNotParallelize]` is preferred. Related shared-static hazard: [[project_manager_asynclazy_shared_seam]].
