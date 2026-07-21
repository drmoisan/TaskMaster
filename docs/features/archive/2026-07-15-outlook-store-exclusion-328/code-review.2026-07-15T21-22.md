# Code Review — outlook-store-exclusion (Issue #328)

- Timestamp: 2026-07-15T21-22
- Reviewer: feature-review
- Base branch: `main` @ `26905c4b737b7fb20cf4e05b92d44fdefb18894e`
- Head: `c0414696f91f03b1ca8e5b33f6920473c9178da8`
- Language in scope: C# (23 changed `.cs` files; 15 production, 8 test)

## Executive Summary

The change is well-structured and closely follows the spec. Store-access decisions remain centralized
in `StoresWrapper`: the new `ExcludedStoreIds` exact-match rule is added as the first, authoritative
branch across `Decide`, the instance `ShouldIncludeStore`, and the static `StoreIsIncluded`, and the
four `Session.Stores` bypass sites are routed through the shared predicate via a uniform null-safe
`.Where(s => storesWrapper is null || storesWrapper.ShouldIncludeStore(s))` seam. COM `StoreID` reads
are consistently guarded and fail open. The UI toggle is implemented as a thin WinForms forwarder with
the mutation/persistence logic folded into the existing `AnyChanges`/`SaveChanges` path, using the
captured `StoreWrapper.StoreId` field so the controller has no live-COM dependency and is unit-testable.
Backward compatibility is preserved (additive `[JsonProperty]` members, defaults on legacy JSON).

No blocking code-quality defects were found. Findings are Low severity or informational: a deviation
from the stated non-goal on the two dead `ToDoEvents` methods (deleted rather than threaded), a
concrete-type cast to reach the filtered `Rebuild` overload, and the acknowledged (spec-accepted)
filter-predicate duplication. Coverage findings are recorded in the policy audit, not repeated here as
code defects.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | ToDoModel/Data Model/ToDo/ToDoEvents.cs | Removed `GetListOfToDoItemsInView`, `GetToDoItemsInView` | The two issue-named dead methods were deleted, not threaded with the filter. Spec §2.2 and user-story Non-Goals stated deletion was out of scope / "a separate issue" and that the feature would thread them for consistency; AC6 literally says they are "threaded with the same filter." | Confirm the deletion is intended (it is stronger: it removes the bypass entirely). Record the scope deviation in the plan/issue so the user-story non-goal and AC6 wording stay consistent with the delivered behavior. | The deletion eliminates the bypass more completely than threading and the methods had no callers; the only concern is the documented-intent mismatch. | git diff ToDoEvents.cs (-128 lines); file-size-check evidence notes "P2-T3 deletion of the two dead methods". |
| Low | TaskMaster/Ribbon/TryFunctionalityInConstruction.cs | `TryRebuildProjInfo` | `((ProjectData)AppGlobals.TD.ProjInfo).Rebuild(...)` casts the `IProjectData` interface to the concrete type to reach the filtered overload. | Acceptable for this scope; if `IProjectData` is later extended, add the filtered `Rebuild` to the interface and drop the cast. | The cast is documented with a why-comment and interface extension was explicitly out of scope; the risk is a hidden invalid-cast if a non-`ProjectData` implementation is ever injected. | git diff TryFunctionalityInConstruction.cs. |
| Info | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | `ShouldIncludeStore` vs `Decide` vs `StoreIsIncluded` | The StoreID exclusion branch is implemented three times (inline instance predicate, `Decide`, and the static overload) rather than sharing one path. | No action for this feature. | Spec §2.2 explicitly accepts this duplication as out of scope; all three were updated identically and are covered by tests, so divergence risk is bounded but present for future edits. | git diff StoresWrapper.cs, StoresWrapper.Filtering.cs, StoreFilterAttribution.cs. |
| Info | UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | `Init` StoreID capture | Branch coverage of `StoreWrapper` is 64.81% (below the 75% floor), unchanged in kind from its pre-existing 65.38% baseline. | Track as a coverage item (see policy audit / remediation inputs); not a code-correctness defect. | The new try/catch capture is covered on both arms by `StoreWrapperTests`; the class-level branch percentage is pulled down by pre-existing uncovered branches elsewhere in the file. | policy-audit.2026-07-15T21-22.md Section 5.2; coverage-delta evidence. |

## Positive Observations

- Consistent fail-open semantics for unreadable `StoreID` at every surface (filter predicate and UI),
  matching the existing `FilePath` guard and `AppOlObjects.LoadInboxes` precedent.
- Uniform null-safe filter seam at all four bypass sites; no site-local filtering logic was introduced,
  satisfying the centralization constraint.
- Additive, backward-compatible serialization; XML doc comments explain intent (why, not what) and cite
  the issue numbers.
- Deterministic tests using MSTest + Moq + FluentAssertions with positive, negative, edge, and
  fail-open/fail-safe scenarios; no banned timing/temp-file APIs.
- File splits into `*.Filtering.cs` partials keep parent files under the 500-line limit without changing
  behavior.
