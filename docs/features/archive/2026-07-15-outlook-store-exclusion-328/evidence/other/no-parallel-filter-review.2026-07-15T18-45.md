# No-Parallel-Filter Review (Issue #328, P2-T9)

Timestamp: 2026-07-15T18-45
Reviewer: atomic-executor
Scope: confirm every store-enumeration bypass site routes store inclusion through the single
shared predicate `StoresWrapper.ShouldIncludeStore` / `StoreFilterAttribution.Decide`, with no
reimplemented include/exclude logic.

## Method

Grepped the three enumeration source files for residual site-local filtering
(`olExchangePublicFolder`, `ExchangeStoreType`, ad-hoc `.Where` predicates) and for the
`ShouldIncludeStore` call at each site.

## Sites reviewed

1. `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs` — `GetToDoList(...)` (line 180)
   `.Where(store => storesWrapper is null || storesWrapper.ShouldIncludeStore(store))`.
   The prior site-local `store.ExchangeStoreType != olExchangePublicFolder` filter was removed and
   replaced by the shared predicate.
2. `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs` — `GetToDoListAsync(...)` (line 227)
   `?.Where(store => storesWrapper is null || storesWrapper.ShouldIncludeStore(store))`.
3. `ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs` — `GetAsyncEnumerableOfToDoItemsInView(...)`
   (line 71) `?.Where(store => storesWrapper is null || storesWrapper.ShouldIncludeStore(store))`.
   (Relocated from `ToDoEvents.cs` by P2-T10; live path threaded with `IApplicationGlobals` by P2-T2.)
4. `ToDoModel/Data Model/Project/ProjectData.cs` — `Rebuild(Outlook.Application, StoresWrapper)`
   (line 270) `.Where(s => storesWrapper is null || storesWrapper.ShouldIncludeStore(s))` before
   `GetDfToDo(store)`.

## Deleted surfaces

The two previously-dead `ToDoEvents` methods named in the issue
(`GetListOfToDoItemsInView`, `GetToDoItemsInView`) were deleted in P2-T3 after a repo-wide
zero-caller verification, so there is no longer a surface to route (spec AC6 sub-clause superseded by
the user-approved scope expansion).

## Result

PASS. Every bypass site calls the shared predicate with the identical null-safe form
(`storesWrapper is null || storesWrapper.ShouldIncludeStore(...)`, fail-open per AC7). No
reimplemented public-folder / name / path / disabled / StoreID include-exclude logic exists outside
`StoresWrapper.ShouldIncludeStore` and `StoreFilterAttribution.Decide`.
