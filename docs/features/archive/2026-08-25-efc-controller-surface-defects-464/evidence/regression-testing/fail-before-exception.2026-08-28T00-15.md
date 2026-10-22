Subject: RC4 — #459 surviving async expansion path

# Fail-before exception dossier

Timestamp: 2026-08-28T00-15
Task: [P1-T15]
Command: source and evidence inspection; no command gate
EXIT_CODE: 0

## WhyFailingRunImpossible

`AsyncExpansionPath_OnOffOn_LeavesCharActionsKeysUnchanged` pins a **post-change contract that also held
pre-change**, so no pre-change state of the repository makes it fail.

The assertion is that driving the surviving asynchronous expansion path On, Off, On neither throws nor
touches the `CharActions` registry. Before Phase 1, the dispatched bodies of that path —
`ToggleExpansionOn` and `ToggleExpansionOff` — already contained no reference to `_keyboardHandler` at
all. The registry writes that #459 B and #459 C describe lived exclusively in the **synchronous**
overload `ToggleExpansion(Enums.ToggleState)`, which the async path never called. A pre-change run of
this test would therefore have passed, and a passing run is not fail-before evidence.

Two mechanisms nevertheless keep the test falsifiable going forward rather than tautological:
`MockBehavior.Strict` throws on any `IQfcKeyboardHandler` member invoked without a set-up, and
`VerifyNoOtherCalls()` fails on any unverified invocation of the mock. A future change that made either
dispatched body touch the keyboard handler would turn this test red.

## What discharges the defect instead

The defect this test guards is discharged by the **five red structural results** recorded in `[P1-T5]`,
whose evidence artifact is
`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/459-466-structural-fail.md`.
That run executed 5 tests and failed 5, and each failure message named the still-present member.

**Five is `[P1-T5]`'s own executed-and-failed count.** Six is `[P1-T12]`'s total, and `[P1-T12]` is a
**green** run, so it is not fail-before evidence for anything. The two counts are not interchangeable and
are stated separately here so that a later reader cannot conflate them.

## The second substitution recorded here

`ToggleExpansionAsync(Enums.ToggleState)` itself **cannot be awaited in any test**. Its body marshals
through `_itemViewer.UiDispatcher`, which `[P0-T17]` establishes from source is a WPF
`System.Windows.Threading.Dispatcher` (`QuickFiler/Viewers/ItemViewer.cs:72`, via
`using System.Windows.Threading;` at `:13`). On the dispatcher's own thread `InvokeAsync` queues the
delegate rather than running it inline, so an `await` of it never completes without a running message
loop — and constraint C3 prohibits a message loop in any test this feature writes.

The test therefore invokes the two dispatched bodies directly by reflection instead of awaiting the
marshal. This substitution is restated in the `[P11-T15]` reconciliation artifact so that it stays
visible rather than concealed.

## Negative-evidence search record

SearchScope:
- `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/`
- `docs/features/active/efc-controller-surface-defects-464/evidence/` (feature root, all `<kind>` folders)

SearchPatterns:
- `fail-before-exception.*.md`
- `*-fail.md`

SearchResult:
- `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/459-466-structural-fail.md` — the `[P1-T5]` red run for the five structural removals.
- `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/fail-before-exception.2026-08-28T00-15.md` — this dossier.
- No other `fail-before-exception.*.md` existed in either scope before this file was written. The
  `[P7-T14]` dossier is not yet present; when it is written it will land in this same folder under the
  same name shape, which is why this file carries a `Subject:` line on its first content line.

Output Summary: `AsyncExpansionPath_OnOffOn_LeavesCharActionsKeysUnchanged` cannot be observed red,
because the async expansion path never touched the `CharActions` registry pre-change either. The defect
is discharged instead by the five red structural results of `[P1-T5]`. `ToggleExpansionAsync(ToggleState)`
cannot be awaited at all because `ItemViewer.UiDispatcher` is an unpumped WPF dispatcher, so the
dispatched bodies are invoked directly.
