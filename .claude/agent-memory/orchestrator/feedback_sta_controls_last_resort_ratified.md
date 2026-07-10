---
name: feedback-sta-controls-last-resort-ratified
description: Maintainer ratified (2026-07-09) in-memory never-shown WinForms CONTROLS on STA test threads as a LAST RESORT after seams, isolated in dedicated *.StaTests.cs files; Forms stay banned
metadata:
  type: feedback
---

For epic #295 (and as a general testing-policy precedent), the maintainer ratified
STA-thread testing of in-memory, never-shown WinForms controls (TableLayoutPanel,
Label, Panel, CheckBox, etc.) under strict conditions:

1. **Last resort only** — a seam (interface > delegate > adapter) is always the
   first approach; STA-bound coverage is permitted only where no seam can isolate
   the logic, and each STA test documents why.
2. **Separate files** — every STA-bound test lives in a dedicated test file
   (`*.StaTests.cs`, `[STATestClass]`/`[STATestMethod]`), keeping the STA surface
   limited to the essential.
3. Never `Show()`/`ShowDialog()`; no message-pump reliance (no PostMessage
   round-trip asserts, no DoEvents, no timers); dispose controls; popups remain a
   violation; `Form`-derived types remain prohibited even unshown.

**Why:** The #297 research proposed ~900 lines of file-level
[ExcludeFromCodeCoverage] on control-identity-bound accelerator/control-map code
(TipsController throws without a parented TableLayoutPanel). The maintainer asked
whether live TableLayoutPanels could be tested on the UI thread and ratified the
refinement to shrink those exemptions rather than waive coverage.

**How to apply:** When a plan proposes an exemption for control-identity or
parented-control logic, first check whether an unshown control on STA satisfies
it; convert file-level exemptions to line/method-level residue (PostMessage
round-trips, focus traversal, paint). Recorded in the epic manifest Shared Design
Pattern (`docs/features/epics/winforms-testability-refactor/epic.md`). See
[[project-epic-295-winforms-testability]].
