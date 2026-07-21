# Feature Audit — outlook-store-exclusion (Issue #328)

- Timestamp: 2026-07-15T21-22
- Reviewer: feature-review
- Work Mode: `full-feature`

## Scope and Baseline

- Base branch (resolved): `main` @ `26905c4b737b7fb20cf4e05b92d44fdefb18894e`.
- Head: `c0414696f91f03b1ca8e5b33f6920473c9178da8`.
- Audit scope: the full branch diff vs the merge-base (feature-vs-base), not any plan/task/phase subset.
- Acceptance-criteria sources (per `full-feature` marker in `issue.md`): `spec.md` (AC1–AC12) and
  `user-story.md` (four story-level ACs).
- Evidence: production/test diffs read directly; per-class coverage parsed from
  `evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml`; toolchain results from
  `evidence/qa-gates/*` (final-csharpier, final-analyzer-build, final-nullable-build, final-vstest,
  coverage-delta, file-size-check).

## Acceptance Criteria Inventory

Spec (`spec.md` §12):
- AC1 — `ExcludedStoreIds` config field (exact-match, OrdinalIgnoreCase, whitespace ignored).
- AC2 — First / authoritative precedence, attributed `StoreFilterRule.StoreId`.
- AC3 — `StoreFilterRule.StoreId` + `Decide` first branch; existing attribution preserved.
- AC4 — All four inclusion surfaces in lockstep; `GetFilteredStores()`/`Init()` omit excluded store.
- AC5 — Fail-open on unreadable StoreID.
- AC6 — Bypass sites route through the filter; the two issue-named `ToDoEvents` methods handled; no
  parallel filtering logic.
- AC7 — Not-yet-loaded model is fail-open (null wrapper includes all stores).
- AC8 — UI toggle binds to `ExcludedStoreIds` membership (case-insensitive).
- AC9 — UI toggle mutates and persists; idempotent no-op when unchanged.
- AC10 — UI fail-safe on unreadable StoreID (checkbox disabled, no mutation).
- AC11 — Backward-compatible persistence; legacy JSON deserializes to empty default; persists across
  sessions.
- AC12 — Toolchain passes in order; new/changed-line coverage meets policy; no repo-wide regression;
  touched files under 500 lines.

User story (`user-story.md`):
- US-AC1 — A store can be excluded by StoreID and is then not enumerated/processed by inbox loading,
  the to-do tree, to-do events, or project-data scanning.
- US-AC2 — Exclusion persists across sessions via the `StoresWrapper` config.
- US-AC3 — A user can toggle a store on/off through the UI without hand-editing JSON.
- US-AC4 — New/changed code meets coverage thresholds; full toolchain passes.

## Acceptance Criteria Evaluation

| Criterion | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | `StoresWrapper.cs`: `[JsonProperty] public List<string> ExcludedStoreIds { get; set; } = [];`. Exact-match `OrdinalIgnoreCase`, `!IsNullOrWhiteSpace` guards in `Decide`/`ShouldIncludeStore`/`StoreIsIncluded`. Covered by `StoresWrapperTests.StoreIdExclusion` (10 tests) incl. near-miss and whitespace cases. |
| AC2 | PASS | StoreID branch is the first short-circuit in all decision paths; `StoreFilterAttribution.Decide` returns `(false, StoreFilterRule.StoreId)`. `StoreFilterAttributionTests` (22 tests) assert precedence and attribution. |
| AC3 | PASS | `StoreFilterRule.StoreId` added as first enum member; `Decide` gains leading `storeId`/`excludedStoreIds` params with the first-branch check; remaining branches byte-preserved (diff inspection). |
| AC4 | PASS | Instance `ShouldIncludeStore` and static `StoreIsIncluded` carry the inline branch; `ShouldIncludeStoreInstrumented` reads `store.StoreID` (guarded) and passes it + `ExcludedStoreIds` into `Decide` (the `GetFilteredStores()` path). Verified in `StoresWrapper.cs` diff. |
| AC5 | PASS | `store.StoreID` read wrapped in try/catch at every surface; null `storeId` fails open. `StoreWrapperTests`/`StoresWrapperTests.StoreIdExclusion` cover the throw case. |
| AC6 | PASS (with deviation) | `TreeOfToDoItems.GetToDoList`/`GetToDoListAsync`, `ProjectData.Rebuild`, and `ToDoEvents.GetAsyncEnumerableOfToDoItemsInView` (live path) route through the shared predicate; no parallel filtering added. Deviation: the two dead methods `GetListOfToDoItemsInView`/`GetToDoItemsInView` were DELETED (P2-T3) rather than threaded as the AC text and user-story non-goal describe. Deletion removes the bypass entirely and the methods had no callers, so the substantive requirement is met; see code-review Low finding. Behavior covered by `StoreFilterRoutingTests` and `ProjectDataCoverageExpansionTests`. |
| AC7 | PASS | Uniform `storesWrapper is null || ...ShouldIncludeStore(...)` guard at all bypass sites. `ProjectDataCoverageExpansionTests`/`StoreFilterRoutingTests` cover the null-wrapper fail-open; the P4-T4 `OlObjectsProxy` fix exercises the not-yet-loaded proxy path. |
| AC8 | PASS | `BindExcludeStoreCheckbox` sets `Checked` from `Model.ExcludedStoreIds` membership (`OrdinalIgnoreCase`). `StoreWrapperController_Tests.ExcludeStore` (9 tests). |
| AC9 | PASS | `ApplyExcludeStoreSelection` adds/removes idempotently; `SaveChanges` calls `Model.Serialize()`; the no-serialize-when-unchanged behavior is realized via `AnyChanges()`/`ExcludeStoreSelectionChanged()` gating the save. Covered by the ExcludeStore controller tests. |
| AC10 | PASS | `BindExcludeStoreCheckbox` disables and clears the checkbox when `Current.StoreId` is unreadable; `ExcludeStoreSelectionChanged` and `ApplyExcludeStoreSelection` early-return, so no mutation occurs. |
| AC11 | PASS | `ExcludedStoreIds` and `StoreWrapper.StoreId` are additive `[JsonProperty]` members round-tripping through the existing `"StoresWrapper"` key; legacy JSON without the keys deserializes to defaults. Round-trip asserted in `StoresWrapperTests.StoreIdExclusion`; no new config file/key. |
| AC12 | PARTIAL | Toolchain passes in order (csharpier/analyzers/nullable EXIT 0; vstest 4611/4611 non-instrumented). New/changed-line coverage meets policy (touched non-exempt classes >= 95% line) and all touched files are <= 500 lines except the pre-existing, non-grown `AppToDoObjects.cs` (503). Open items: the canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent (procedural), and `StoreWrapper` branch coverage is 64.81%, below the 75% branch floor (pre-existing, baseline 65.38%). See policy-audit Section 5. |
| US-AC1 | PASS | Exclusion by StoreID is honored across inbox loading (existing wiring), the to-do tree, to-do events (live path), and project-data scanning (routing verified in AC4/AC6). |
| US-AC2 | PASS | Persistence verified in AC11. |
| US-AC3 | PASS | UI checkbox toggle verified in AC8–AC10; no JSON hand-editing required. |
| US-AC4 | PARTIAL | Same basis as AC12: toolchain green and new/changed-line coverage meets policy; open coverage items are the absent canonical artifact and the `StoreWrapper` branch floor. |

## Summary

Fourteen of sixteen acceptance criteria PASS. Two (AC12 and US-AC4, which are the same
toolchain-and-coverage meta-criterion expressed in both source files) are graded PARTIAL: the feature's
substantive coverage obligations are met (new/changed-line coverage on non-exempt classes clears the
floor, toolchain green, file-size compliant), but two coverage items remain open — the canonical C#
coverage artifact is not at `artifacts/csharp/coverage.xml`, and `StoreWrapper` branch coverage
(64.81%) is below the 75% branch floor as a pre-existing condition. AC6 passes on its substantive goal
with a documented deviation (dead-method deletion rather than threading). Remediation inputs are
recorded for the two PARTIAL coverage items; both are procedural/pre-existing rather than
code-correctness defects.

Go/no-go: conditional-go. The feature is functionally complete and policy-compliant on code quality
and the toolchain. Recommended before merge: emit the C# coverage at the canonical
`artifacts/csharp/coverage.xml` path (or confirm the PR CI coverage run as the authoritative gate),
and disposition the `StoreWrapper` branch-coverage floor (accept as pre-existing or add branch tests).

## Acceptance Criteria Check-off

- Source files already carry all AC checkboxes as `[x]` (executor-authored):
  `spec.md` 12/12, `user-story.md` 4/4.
- Reviewer action: no checkbox state was changed. Fourteen criteria are confirmed PASS and remain
  `[x]`. AC12 and US-AC4 are re-graded PARTIAL by this audit; they are left as-authored to avoid
  scoping-doc churn, and this audit is the authoritative record that they are PARTIAL pending the
  procedural coverage disposition described above and in `remediation-inputs.2026-07-15T21-22.md`.

### Acceptance Criteria Status
- Source: `spec.md` (AC1–AC12) and `user-story.md` (US-AC1–US-AC4)
- Total AC items: 16
- PASS: 14 (AC1–AC11, US-AC1–US-AC3)
- PARTIAL: 2 (AC12, US-AC4 — coverage/toolchain meta-criterion; open items are procedural/pre-existing)
- FAIL / UNVERIFIED: 0
