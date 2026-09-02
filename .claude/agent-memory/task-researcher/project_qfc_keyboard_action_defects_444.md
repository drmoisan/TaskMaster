---
name: qfc-keyboard-action-defects-444
description: Issue #444/#472/#482 research (2026-08-24) — #468 removes WireUpKeyboardHandler so #444's duplicate registration is already gone; #482's filed trigger is unreachable; the real trigger is Right/Down/Right
metadata:
  type: project
---

Research for epic child `quickfiler-keyboard-action-defects` (issues #444, #472, #482), completed
2026-08-24 against `988e819b`. Four findings that are expensive to re-derive:

1. **#468 dissolves #444's call-site coupling.** #468's P1-T2 deletes `WireUpKeyboardHandler`
   (`QuickFiler/Controllers/QfcCollectionController.cs:1254-1273`), which is the *only* site in the
   solution constructing a `KbdActions<>` with a duplicate `(SourceId, Key)` pair. It has zero
   callers today. Post-#468 the `IEnumerable` ctor guard is a **zero-call-site-impact** change, so
   the promoted document's "the guard must land together with the Keys.Down product decision" is no
   longer true.

2. **#482's filed trigger is wrong.** `ActivateBySelectionAsync` -> synchronous `ToggleExpansion()`
   at `:1439` is guarded by `blExpanded`, and both async callers pass `false` (`ToggleOffActiveItemAsync`
   has its expansion branch commented out). The live trigger is **Right, Down, Right**: Right uses
   the async registry, Down routes `SelectNextItemAsync` -> synchronous `SelectNextItem` ->
   `ToggleOffActiveItem` -> synchronous `ToggleExpansion()`, and the third keystroke double-adds.

3. **Only the ASYNC registries are read on the ordinary keystroke path.** `KeyDownTaskAsync` consults
   `AlwaysOnKeyActionsAsync`/`KeyActionsAsync`/`CharActionsAsync`/`StringActionsAsync` only. The sync
   `CharActions` registry is reached solely via `ProcessCmdKey` on an **Alt-key** command. Also,
   `RegisterFocusActions`/`UnregisterFocusActions` are commented out in the QuickFiler surface, so
   the sync focus registrations never run there.

4. **A "capture Digits once" fix does NOT fix #472.** Hoisting still computes the *current* width.
   The fix must record the width used at registration. There is also a second, unfiled orphan source:
   `UnregisterNavigation` bounds its loop with the current `_itemGroups.Count`, and
   `RemoveSpecificControlGroup(int)` mutates `_itemGroups` with no unregister/register bracket
   (reached unbracketed from `RemoveBelowThresholdAsync` and the `'R'` char action).

**Why:** these four points each contradict the text of a promoted bug document or of the upstream
feature's plan, so a later reader who trusts the filed text will plan the wrong fix.

**How to apply:** when planning or reviewing any of #444/#472/#482, verify each against the code
before accepting the issue text. Also: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
is exactly 500 lines and #468 pins its `[TestMethod]` count, so no #472 test can go there.

Related: [[qfc-item-controller-227-r2-denial]], [[feedback-exemption-audit-check-proven-techniques]]
