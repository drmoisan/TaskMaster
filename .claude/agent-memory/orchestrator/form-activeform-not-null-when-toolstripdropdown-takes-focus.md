---
name: form-activeform-not-null-when-toolstripdropdown-takes-focus
description: Form.ActiveForm stays NON-null when an owned ToolStripDropDown takes focus and goes null when activation leaves the process — the opposite of what QuickFiler research assumed
metadata:
  type: project
---

Measured on issue #796 against a live Outlook process, from instrumented
`QfcFormController.ParkFocusAndCancelSelectors` log lines:

- Popup takes focus (self-inflicted deactivation): `Form.ActiveForm` is **NOT** null.
- Activation leaves the process (genuine deactivation): `Form.ActiveForm` **IS** null.

**Why:** `Form.ActiveForm` reports the active form *of this process*. A `ToolStripDropDown`
owned by the QuickFiler form does not clear it. Leaving the process does. The QuickFiler research
artifact and the Phase 1 instrumentation's own doc comment both asserted the inverse, reasoning
that a `ToolStripDropDown` is not a `Form` so a null active form indicates the popup took focus.
That reasoning is intuitive and wrong.

**How to apply:** Never use `Form.ActiveForm == null` as a self-inflicted-versus-genuine
deactivation discriminator in QuickFiler. It is usable, but with inverted polarity, and it is
fragile. Prefer an explicit popup-owns-activation signal assigned from the popup's own lifecycle
(the `ItemViewer` breadcrumb wiring), which is what #796 adopted.

The cost of getting this backwards is not a subtle bug: a guard built on the inverted reading
suppresses the selector cancel on *genuine* deactivations, breaking the issue #677 contract,
while still cancelling on self-inflicted ones, so the original defect survives. Both halves fail
while the code reads as though it implements them.

Related: the issue #677 deactivation contract, and [[project_epic_295_winforms_testability]].
