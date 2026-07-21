# Post-Edit Census Confirmation (Cycle 2, Issue #292)

Timestamp: 2026-07-09T17-45

## Search command

`grep -rn -B1 "public class (StoresWrapperEnumerationScopeTests|StoresWrapperTests|AppOlObjectsCoverageTests|AppOlObjectsAttributionContextTests|AppOlObjectsTests)" TaskMaster.Test --include=*.cs`

## Matched class list (marking state after P1-T2)

| Class | `[DoNotParallelize]` present |
|---|---|
| `StoresWrapperEnumerationScopeTests` (StoresWrapperEnumerationScopeTests.cs L25 attr / L26 class) | Yes |
| `StoresWrapperTests` (StoresWrapperTests.cs L20 attr / L21 class) | Yes |
| `AppOlObjectsCoverageTests` (AppOlObjectsCoverageTests.cs L20 attr / L21 class) | Yes |
| `AppOlObjectsAttributionContextTests` (AppOlObjectsAttributionContextTests.cs L21 attr / L22 class) | Yes (already marked pre-cycle) |
| `AppOlObjectsTests` (AppOlObjectsTests.cs L21 class) | No — excluded, not-a-writer (rewire mocked via `DelayedRewireStoresWrapper`; no real `CurrentStoreContext` scope open, no null-baseline read) |

## Statement

All four confirmed `CurrentStoreContext` scope-opener/null-baseline-reader classes in `TaskMaster.Test` now
carry a class-level `[DoNotParallelize]`. No remaining unmarked scope-opener/reader class exists in
`TaskMaster.Test`: the broad-search substring matches (`ContinuationProbeSequenceTests`,
`ApplicationGlobalsStartupTimingTests`, `TestableApplicationGlobals`) are `StoreWrapper*` method-name false
positives and open no scope (recorded in census-determination.2026-07-09T17-45.md), and `AppOlObjectsTests`
mocks the rewire path. The fix is confined to three class-level attribute additions.
