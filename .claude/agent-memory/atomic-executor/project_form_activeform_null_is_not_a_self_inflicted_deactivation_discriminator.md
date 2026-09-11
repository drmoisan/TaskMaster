---
name: form-activeform-null-is-not-a-self-inflicted-deactivation-discriminator
description: Issue #796 runtime observation — Form.ActiveForm==null reads INVERTED as a self-inflicted-vs-genuine deactivation signal in the QuickFiler VSTO add-in; do not build a seam on it
metadata:
  type: project
---

In the QuickFiler VSTO add-in, `Form.ActiveForm == null` does NOT discriminate a
self-inflicted form deactivation (this add-in's own `ToolStripDropDown` popup taking focus)
from a genuine one (focus moving to a foreign window). Measured on a live Debug build for
issue #796, it reads the **opposite** of the predicted direction on all four observations:
all three popup-driven gestures reported `ActiveFormNull=False`, and the one deactivation
caused by focus leaving the form reported `ActiveFormNull=True`.

Related runtime reversal from the same run: `IsWebView2Focused` (the gate on
`ParkFocusOffWebView2`) reported **False** on the arrow-click gesture and **True** on the
Down-key-from-the-search-box gesture — also the reverse of what reading the source predicts.

**Why:** the spec and research artifacts for #796 both proposed the `ActiveForm` null check as
a "low-cost discriminator", reasoning that a `ToolStripDropDown` is not a `Form` so a
self-inflicted deactivation would show a null active form. That reasoning was recorded as
asserted-from-background-knowledge and unverified. The AC6 manual observation refuted it.
A seam built on it would have been silently inverted, and the WinForms/Win32 activation
behaviour here is evidently not what reading either the framework docs or the call site
suggests.

**How to apply:** when work touches QuickFiler form deactivation, the breadcrumb drop-down
close/cancel ordering, or any "was this deactivation ours?" decision, do not derive the
answer from `Form.ActiveForm`, and do not derive focus-leaf expectations from reading
`IsWebView2Focused` call sites. Supply the state explicitly from the code that opens the
popup. Treat any similar activation-state heuristic in this add-in as requiring a live
observation before it is relied on. Evidence and full derivation live in the #796 feature
folder under `evidence/other/close-ordering-decision.md` and the
`2026-09-07T12-19-dropdown-close-ordering-observation.md` transcript beside it.

See also [[feedback-never-predict-an-observation-into-an-artifact]].
