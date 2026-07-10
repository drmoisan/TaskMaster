---
name: winforms-sta-refinement-exemption-rule
description: Epic winforms-testability-refactor STA refinement — which coverage exemptions it removes vs. retains when planning #293/#296/#297/#298
metadata:
  type: project
---

Epic winforms-testability-refactor (#295) Shared Design Pattern item 4 carries a maintainer-ratified refinement (2026-07-09, "last-resort STA controls"): in-memory never-shown WinForms **controls** MAY be constructed on an STA thread in dedicated `*.StaTests.cs` files (`[STATestClass]`/`[STATestMethod]`), no `Show()`/`ShowDialog()`, no message pump, dispose per test, no popups, `Form`-derived types prohibited even unshown.

Applied to #293 (see `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/`). The disposition pattern that generalizes to sibling children #296/#297/#298:

- REMOVE exemption for a control-backed default body that only forces an HWND (e.g. `DrawFocus` = `Graphics.FromHwnd(cbx.Handle)` + `ControlPaint.DrawFocusRectangle`): a `CheckBox`/control constructed unshown on STA, read `.Handle` to force invisible handle creation, invoke, assert no-throw + `IsHandleCreated`. Seam-first is satisfied because the injectable delegate already covers arithmetic; STA covers only the production default body no seam can reach.
- NARROW event-wiring exemptions: subscribe/unsubscribe wiring and `Click` handlers are STA-coverable via public `CheckBox.PerformClick()` (no pump). RETAIN exemption only for members needing real focus traversal (`GotFocus`/`LostFocus` color swap) or protected raisers (`OnKeyDown`/`OnPreviewKeyDown` — no public unshown-control raiser).
- KEEP exemption unchanged for: dialog adapters (`MessageBox.Show`/`InputBox.ShowDialog` = shown UI + popup), `Form`-derived viewer + Designer code (condition d), and live-form/Outlook-globals launchers.

**Why:** the refinement lets these UI projects hit the >=80% floor without exempting bodies that are actually deterministically executable against unshown STA controls. **How to apply:** when revising exemption registers for sibling children, do NOT renumber exemption labels (preserves epic-assessment traceability); confine STA to dedicated `*.StaTests.cs`; verify `[STATestClass]`/`[STATestMethod]` in the test project's MSTest version (Tags.Test has MSTest 4.2.2 which includes them; fallback is `.runsettings` `ExecutionThreadApartmentState=STA`, which widens STA to the whole assembly run). Update the final-phase determinism scan to sanction only the dedicated STA files. See [[project_legacy_csproj_explicit_compile_include]] — new StaTests files need explicit `<Compile Include>` in the legacy test csproj.
