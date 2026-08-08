---
name: qfc-framebuilding-436
description: "#436 F5 QfcDatamodel.FrameBuilding.cs is Deedle data-frames, NOT WinForms (issue.md was wrong); DfDeedle.AddQfcColumns pops modal dialogs QuickFiler.Test cannot suppress"
metadata:
  type: project
---

`QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` (F5 / issue #436, epic #136) has **zero
WinForms coupling**. "Frame" means `Deedle.Frame<int, string>`, not a layout frame.
`docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/issue.md:80-82` asserted
"WinForms layout" and that error propagated into agent delegation prompts.

**Why:** the file name collides with WinForms terminology. Anyone reading only the file name or
issue.md will design a `TableLayoutPanel` seam and an STA test plan that has no subject.

**How to apply:** for any QuickFiler `*.FrameBuilding.*` or `Frame`-typed member, check the `using`
block before assuming UI. If `Deedle` is imported and `System.Windows.Forms` is not, it is data-frame
work. The STA last-resort clause does not apply.

Two further verified findings worth reusing:

1. **The real blocker is a modal dialog behind an InternalsVisibleTo wall.** Both
   `DfDeedle.GetEmailDataInView` and `GetEmailDataInViewAsync` funnel into
   `DfDeedle.AddQfcColumns` (`UtilitiesCS/Extensions/DfDeedle.cs:296-316`), which calls
   `DfDeedle.MessageBoxInvoker` and shows **two** real modal dialogs before throwing, whenever the
   folder has no `Triage` user-defined property (which a Moq'd `MAPIFolder` never does).
   `MessageBoxInvoker` is `internal static` (`DfDeedle.cs:54-60`) and
   `UtilitiesCS/Properties/AssemblyInfo.cs:19-20` grants IVT only to `UtilitiesCS.Test` and
   `ToDoModel.Test` — **not `QuickFiler.Test`**. So no QuickFiler test can neutralize it; a delegate
   seam on the `DfDeedle` call in the consuming file is the only route. Same wall blocks
   `DfDeedle.TableEtlInvoker`.
2. **`[STATestClass]`/`[STATestMethod]` need no package.** They ship in `MSTest.TestFramework`, and
   `QuickFiler.Test/packages.config:119` already pins 4.3.3 — the same version `Tags.Test` uses for
   its STA files. Enabling the first `*.StaTests.cs` in a test project is one `<Compile Include>`
   entry; there is no runsettings and no `apartmentState` config anywhere in this repo.

Related: [[qfc-datamodel-coverage-436]], [[qfc-queueprocessing-436]], [[efcdatamodel-coverage-436]],
[[committed-cobertura-baselines]].
