# Feature Audit — outlook-store-exclusion (Issue #328)

- Timestamp: 2026-07-16T03-00
- Reviewer: feature-review (remediation re-audit, pass 1)
- Work Mode: `full-feature`

## Scope and Baseline

- Base branch (resolved): `main` @ `26905c4b737b7fb20cf4e05b92d44fdefb18894e`.
- Head: `1090ae3b2dac3329f9118eb541202513a93cd2b7`.
- Audit scope: the full branch diff vs the merge-base (feature-vs-base), not any plan/task/phase subset.
- Acceptance-criteria sources (per `full-feature` marker in `issue.md`): `spec.md` (AC1–AC12) and
  `user-story.md` (four story-level ACs).
- Re-audit note: production/test C# code is unchanged since cycle 1 (`git diff c0414696..1090ae3b`);
  the remediation commit changed only scoping docs, evidence, prior review artifacts, and agent memory.
- Evidence: production/test diffs read directly; per-class coverage parsed from
  `evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml`; toolchain results from
  `evidence/qa-gates/*`; canonical C# coverage artifact `artifacts/csharp/coverage.xml` (present;
  re-scoped per policy-audit §5.1).

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
| AC1 | PASS | `StoresWrapper.cs`: `[JsonProperty] public List<string> ExcludedStoreIds { get; set; } = [];`. Exact-match `OrdinalIgnoreCase`, `!IsNullOrWhiteSpace` guards in `Decide`/`ShouldIncludeStore`/`StoreIsIncluded`. Covered by `StoresWrapperTests.StoreIdExclusion` incl. near-miss and whitespace cases. |
| AC2 | PASS | StoreID branch is the first short-circuit in all decision paths; `StoreFilterAttribution.Decide` returns `(false, StoreFilterRule.StoreId)`. `StoreFilterAttributionTests` assert precedence and attribution. |
| AC3 | PASS | `StoreFilterRule.StoreId` added as first enum member; `Decide` gains leading `storeId`/`excludedStoreIds` params with the first-branch check; remaining branches byte-preserved (diff inspection). |
| AC4 | PASS | Instance `ShouldIncludeStore` and static `StoreIsIncluded` carry the inline branch; `ShouldIncludeStoreInstrumented` reads `store.StoreID` (guarded) and passes it + `ExcludedStoreIds` into `Decide` (the `GetFilteredStores()` path). Verified in `StoresWrapper.cs` / `StoresWrapper.Filtering.cs` diffs. |
| AC5 | PASS | `store.StoreID` read wrapped in try/catch at every surface; null/whitespace `storeId` fails open. `StoreWrapperTests`/`StoresWrapperTests.StoreIdExclusion` cover the throw case. |
| AC6 | PASS | `TreeOfToDoItems.GetToDoList`/`GetToDoListAsync`, `ProjectData.Rebuild`, and `ToDoEvents.GetAsyncEnumerableOfToDoItemsInView` (live path) route through the shared predicate; no parallel filtering added. The two dead methods `GetListOfToDoItemsInView`/`GetToDoItemsInView` were DELETED under the maintainer-approved scope change (`resolved_at: 2026-07-15T23:35:00Z`), removing their bypass entirely; the scoping docs (spec §2.2, user-story Non-Goals, AC6) are reconciled to the delivered deletion (cycle-1 R3). Behavior covered by `StoreFilterRoutingTests` and `ProjectDataCoverageExpansionTests`. |
| AC7 | PASS | Uniform `storesWrapper is null || ...ShouldIncludeStore(...)` guard at all bypass sites. `ProjectDataCoverageExpansionTests`/`StoreFilterRoutingTests` cover the null-wrapper fail-open; the P4-T4 `OlObjectsProxy` fix exercises the not-yet-loaded proxy path. |
| AC8 | PASS | `BindExcludeStoreCheckbox` sets `Checked` from `Model.ExcludedStoreIds` membership (`OrdinalIgnoreCase`). Covered by `StoreWrapperController_Tests.ExcludeStore`. |
| AC9 | PASS | `ApplyExcludeStoreSelection` adds/removes idempotently; `SaveChanges` calls `Model.Serialize()`; no-serialize-when-unchanged is realized via `AnyChanges()`/`ExcludeStoreSelectionChanged()` gating. Covered by the ExcludeStore controller tests. |
| AC10 | PASS | `BindExcludeStoreCheckbox` disables and clears the checkbox when `Current.StoreId` is unreadable; `ExcludeStoreSelectionChanged`/`ApplyExcludeStoreSelection` early-return, so no mutation occurs. |
| AC11 | PASS | `ExcludedStoreIds` and `StoreWrapper.StoreId` are additive `[JsonProperty]` members round-tripping through the existing `"StoresWrapper"` key; legacy JSON without the keys deserializes to defaults. Round-trip asserted in `StoresWrapperTests.StoreIdExclusion`; no new config file/key. |
| AC12 | PASS | Toolchain passes in order (csharpier/analyzers/nullable EXIT 0; vstest 4611/4611 non-instrumented). New/changed-line coverage meets policy (touched non-exempt classes >= 95% line; new-code >= 90%; no changed-line regression); repo-wide first-party over instrumented assemblies 85.71% line / 79.34% branch. All touched files <= 500 lines except the pre-existing, non-grown `AppToDoObjects.cs` (503). Cycle-1 open items resolved: canonical artifact present (re-scoped, policy-audit §5.1); `StoreWrapper` branch floor dispositioned as ratified pre-existing exception (policy-audit §5.5). |
| US-AC1 | PASS | Exclusion by StoreID is honored across inbox loading (existing wiring), the to-do tree, to-do events (live path), and project-data scanning (routing verified in AC4/AC6). |
| US-AC2 | PASS | Persistence verified in AC11. |
| US-AC3 | PASS | UI checkbox toggle verified in AC8–AC10; no JSON hand-editing required. |
| US-AC4 | PASS | Same basis as AC12: toolchain green; new/changed-line coverage meets policy; the two cycle-1 coverage items (canonical artifact, `StoreWrapper` branch floor) are resolved/dispositioned. |

## Summary

All sixteen acceptance criteria PASS. The two coverage/toolchain meta-criteria (AC12 and US-AC4) were
graded PARTIAL in cycle 1 on two coverage items — an absent canonical C# coverage artifact and the
`StoreWrapper` sub-floor branch coverage — both of which the remediation cycle resolved: the canonical
artifact is now present at `artifacts/csharp/coverage.xml` (re-scoped by this re-audit to the
instrumented first-party assemblies, reading 85.71% line / 79.34% branch), and the `StoreWrapper`
branch floor (64.81%) is a ratified, documented pre-existing exception with no regression on any
changed line. AC6's dead-method deletion is reconciled with the scoping docs under the maintainer-
approved scope change. No code-correctness defect is outstanding.

Go/no-go: GO. The feature is functionally complete and policy-compliant on code quality, the toolchain,
file-size limits, evidence locations, and coverage. The only residual is the ratified `StoreWrapper`
pre-existing branch-floor exception, which does not block merge. The full all-first-party repository-wide
C# coverage figure is confirmed by the PR CI coverage run per policy-audit §5.4.

## Acceptance Criteria Check-off

- Source files already carry all AC checkboxes as `[x]` (executor-authored): `spec.md` 12/12,
  `user-story.md` 4/4.
- Reviewer action: all sixteen criteria are confirmed PASS by this re-audit; the AC12 and US-AC4
  checkboxes that cycle 1 held as PARTIAL are now confirmed and remain `[x]`. No checkbox text was
  modified.

### Acceptance Criteria Status
- Source: `spec.md` (AC1–AC12) and `user-story.md` (US-AC1–US-AC4)
- Total AC items: 16
- Checked off (delivered): 16
- PASS: 16 (AC1–AC12, US-AC1–US-AC4)
- PARTIAL / FAIL / UNVERIFIED: 0
- Items remaining: none.
