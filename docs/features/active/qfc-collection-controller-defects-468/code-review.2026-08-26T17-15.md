# Code Review — qfc-collection-controller-defects-468 (issue #468 family)

- **Date:** 2026-08-26T17-15
- **Reviewer:** feature-review agent
- **Branch:** `bug/qfc-collection-controller-defects-468` @ `91943050`
- **Base:** `origin/epic/quickfiler-bug-family-integration` @ `141efcb8`
- **Scope:** full branch diff (10 C# source/build files; docs and evidence reviewed for hygiene only)

---

## Findings

| ID | Severity | Blocking? | Location | Finding |
|---|---|---|---|---|
| CR-1 | Minor | NON-BLOCKING | `QuickFiler/Controllers/QfcCollectionController.cs` (`GetMoveDiagnostics`) | `GetMoveDiagnostics` reads `_itemGroupsToMove.Count` without a null guard, while the sibling accessor `TryGetItemGroupByIndex` (added by this feature) does guard null. The shape is pre-existing — the pre-fix code performed the same unguarded read on the old dictionary — and is mitigated by `CleanupBackground` resetting the field to `Array.Empty` rather than null, so the inconsistency is stylistic. Consider aligning the two access paths in the #623 decomposition work. |
| CR-2 | Info | NON-BLOCKING | `QuickFiler/Controllers/QfcCollectionController.cs` | +88 lines on a file already 4.9x over the 500-line cap. Adjudicated in the policy audit (PA-2, non-blocking): the growth is spec-mandated documentation, seams, and guards, and the split remedy is prohibited by AC-25 and assigned to #623. Reported here so the decomposition owner sees the direction of travel. |
| CR-3 | Info | NON-BLOCKING | `QuickFiler.Test/Controllers/` | Three test files sit at 500, 497, and 494 lines (0, 3, and 6 lines of headroom against the cap). Any future test addition in these areas must extract into a new file first; the `Defects468` naming pattern plus the shared `TestSupport` helper file already provide the extraction template. |
| CR-4 | Info | NON-BLOCKING | `QuickFiler/Controllers/QfcCollectionController.cs` (`MoveEmailsAsync`) | `MoveEmailsAsync` uses the LINQ extension `?.Count()` on the `IReadOnlyList` snapshot where the property `?.Count` (used by `EmailsToMove`) would avoid an enumerator-path call. Functionally identical here; purely cosmetic. |

Zero blocking findings.

---

## Review Notes by Area

### Production changes (`QfcCollectionController.cs`, `IQfcCollectionController.cs`)

- **Dead-code removal (#468):** twelve members, one field, and one commented reference removed in a single isolated commit; all thirteen identifiers independently re-verified at review time to return zero hits in the file, and the five spec-named live members remain present. The isolation of the removal into its own commit keeps the line-renumbering reviewable, as the spec required.
- **Error-handling corrections follow fail-fast policy:** the `OperationCanceledException` rethrow clause is correctly ordered before the broad catch and commented with the reason; the broad catch that remains logs once with context and continues per the documented batch-move contract, which is a deliberate boundary behavior, not silent swallowing. The `finally`-based counter restore for #286 is the minimal correct construct.
- **Seam quality:** `ShrinkByRows` and `ResolveConversationInsertions` are pure static helpers with no field reads — correctly separated from I/O and UI per the separation-of-concerns principle, and each landed in a behavior-preserving commit verified by identical before/after suite counts. `TryGetMoveReadiness` + the lazily-initialized `_notifyNotReady` delegate preserves the exact prior `MessageBox.Show` production behavior while making the readiness decision testable.
- **Defensive guards (-1 index, null controller):** `PromoteFirstChild`, `ToggleUnGroupConv`, and `SetVisualDigits` guard sentinel and null states explicitly with a single `Warn` log each, matching the spec's recoverable-UI-path rationale (plan decision D4) rather than throwing on a VSTO event path.
- **Interface documentation (#469 defect 4):** the `stackMovedItems` XML contract on both interface and implementation states the true data flow (undo records travel via the email filer's push path, not the parameter) and the explicit `_ = stackMovedItems;` discard makes the retained-for-compatibility decision visible in code rather than as an unused-parameter accident.
- **Comment discipline:** new comments explain why (defect numbers, ordering constraints, contract reasoning), not what. The CS0618 suppression rationale for the retained `ForEachAwaitAsync` call is narrow and documented in place, consistent with the analyzer policy's preference for narrow, justified suppressions.

### Test changes

- 28 new methods across four test files plus a `TestSupport` helper; all use MSTest/Moq/FluentAssertions, Arrange-Act-Assert structure, and descriptive scenario names (verified by sampling and by the committed P14-T12 audit).
- Scenario coverage per changed behavior includes positive, negative, boundary (`-1` index, empty list, null controller), error-handling (throw paths, cancellation), and state-transition cases (counter restore, make-space/eliminate-space neutrality), satisfying the scenario-completeness policy for the changed units.
- The reflection-based structural test for the `_itemGroupsToMove` contract and the recorded rationale for avoiding a flaky behavioral red state on `ConcurrentDictionary` ordering show correct determinism judgment.
- The `[STATestClass]` layout tests dispose the `TableLayoutPanel` per test and never show a window, keeping the STA surface minimal.
- The csproj `Compile Include` entries sit exactly at the spec-mandated insertion point (verified in the diff).

### Process artifacts

- Evidence artifacts are complete, internally consistent, and host-identifier-clean; TRX files use task-keyed names. The executor's honest recording of two unmet/discrepant sub-clauses (P15-T7 growth statement, P14-T12 raw-search reading) instead of asserting them is the correct behavior and both are adjudicated non-blocking in the policy audit.

---

## Verdict

Zero blocking findings; one Minor and three Informational, all NON-BLOCKING. The change is a disciplined bug-family remediation: minimal targeted fixes, behavior-preserving seams, deterministic tests, and truthful evidence.
