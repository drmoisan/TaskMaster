# Code Review — outlook-store-exclusion (Issue #328)

- Timestamp: 2026-07-16T03-00
- Reviewer: feature-review (remediation re-audit, pass 1)
- Base branch: `main` @ `26905c4b737b7fb20cf4e05b92d44fdefb18894e`
- Head: `1090ae3b2dac3329f9118eb541202513a93cd2b7`
- Language in scope: C# (23 changed `.cs` files; 15 production, 8 test)

## Executive Summary

The production and test C# code is unchanged since the cycle-1 review (`git diff c0414696..1090ae3b`
shows only scoping-doc, evidence, prior-review-artifact, and agent-memory changes). This re-audit
re-verified the code independently by reading the production diffs; the cycle-1 assessment holds.

The change is well-structured and closely follows the spec. Store-access decisions remain centralized
in `StoresWrapper`: the new `ExcludedStoreIds` exact-match rule is added as the first, authoritative
branch across `StoreFilterAttribution.Decide`, the instance `ShouldIncludeStore`, and the static
`StoreIsIncluded`, and the four `Session.Stores` bypass sites are routed through the shared predicate
via a uniform null-safe `.Where(s => storesWrapper is null || storesWrapper.ShouldIncludeStore(s))`
seam. COM `StoreID` reads are consistently guarded and fail open. The UI toggle is a thin WinForms
forwarder with the mutation/persistence logic folded into the existing `AnyChanges`/`SaveChanges` path,
using the captured `StoreWrapper.StoreId` field so the controller has no live-COM dependency and is
unit-testable. Backward compatibility is preserved (additive `[JsonProperty]` members; defaults on
legacy JSON).

No blocking code-quality defects were found. Findings are Low or Info severity: a deviation from the
originally-stated non-goal on the two dead `ToDoEvents` methods (deleted rather than threaded — now
reconciled in the scoping docs under an approved scope change), a concrete-type cast to reach the
filtered `Rebuild` overload, and the spec-accepted filter-predicate duplication. Coverage findings are
recorded in the policy audit, not repeated here as code defects.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | ToDoModel/Data Model/ToDo/ToDoEvents.cs | Removed `GetListOfToDoItemsInView`, `GetToDoItemsInView` | The two issue-named dead methods were deleted (not threaded). The originally-authored spec/user-story described threading them and deferring deletion to a separate issue; AC6 originally read that they are "threaded." | No further action: the scoping docs (spec §2.2, user-story Non-Goals, AC6) and `remediation-inputs` R3 now reconcile this to the maintainer-approved scope-change deletion (`resolved_at: 2026-07-15T23:35:00Z`). | Deletion removes the `Session.Stores` bypass entirely and the methods had no callers, so the substantive requirement is met more completely than threading; the only prior concern (documented-intent mismatch) is now resolved. | git diff ToDoEvents.cs (-128 lines); spec.md §2.2; user-story Non-Goals; remediation-inputs R3. |
| Low | TaskMaster/Ribbon/TryFunctionalityInConstruction.cs | `TryRebuildProjInfo` | `((ProjectData)AppGlobals.TD.ProjInfo).Rebuild(...)` casts the `IProjectData` interface to the concrete type to reach the filtered overload. | Acceptable for this scope; if `IProjectData` is later extended, add the filtered `Rebuild` to the interface and drop the cast. | The cast is documented with a why-comment; interface extension was explicitly out of scope; residual risk is a hidden invalid-cast if a non-`ProjectData` implementation is ever injected. | git diff TryFunctionalityInConstruction.cs. |
| Info | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | `ShouldIncludeStore` vs `Decide` vs `StoreIsIncluded` | The StoreID exclusion branch is implemented three times (inline instance predicate, `Decide`, and the static overload) rather than sharing one path. | No action for this feature. | Spec §2.2 explicitly accepts this duplication as out of the feature's scope; all three were updated identically and are covered by tests, so divergence risk is bounded but present for future edits. | git diff StoresWrapper.cs, StoresWrapper.Filtering.cs, StoreFilterAttribution.cs. |
| Info | UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | `Init` StoreID capture | Branch coverage of `StoreWrapper` is 64.81% (below the 75% floor), unchanged in kind from its pre-existing 65.38% baseline. | Track as a coverage item (policy-audit §5.5); ratified pre-existing exception, not a code-correctness defect. | The new try/catch capture is covered on both arms by `StoreWrapperTests`; the class-level branch percentage is pulled down by pre-existing uncovered branches elsewhere in the file. | policy-audit.2026-07-16T03-00.md §5.5; storewrapper-branch-coverage-disposition.2026-07-16T02-30.md. |

## Positive Observations

- Consistent fail-open semantics for unreadable `StoreID` at every surface (filter predicate and UI),
  matching the existing `FilePath` guard and `AppOlObjects.LoadInboxes` precedent.
- Uniform null-safe filter seam at all four bypass sites; no site-local filtering logic introduced,
  satisfying the centralization constraint (spec §6.1, §10).
- Additive, backward-compatible serialization; XML doc comments explain intent (why, not what) and cite
  the issue numbers (#328, #261).
- Deterministic tests using MSTest + Moq + FluentAssertions with positive, negative, edge, and
  fail-open/fail-safe scenarios; no banned timing/temp-file APIs.
- Partial-class splits into `*.Filtering.cs` keep parent files under the 500-line limit without
  changing behavior.
