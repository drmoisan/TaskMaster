---
name: qfcitemcontroller-pump-harness-needs-saveparameters
description: A QfcItemController test harness built with SetField-only injection leaves the ??= factory defaults null; call SaveParameters instead
metadata:
  type: project
---

When arranging a `QfcItemController` for a **full initialization** run, inject only
the behavioral seams (`_uiDispatcher`, `_webViewInitializer`) with
`QfcItemControllerTestSupport.SetField`, then call the real
`controller.SaveParameters(...)`. Do **not** inject every field one-by-one.

**Why:** `SaveParameters` is the single construction path every production route
funnels through, and its `??=` block is what supplies `_folderPredictorFactory`,
`_conversationResolverFactory`, `_folderPredictorEmptyFactory`, `_flagTasksFactory`,
`_emailFilerFactory` and `_mailActions`. A SetField-only harness leaves those null,
so the test fails deep inside `LoadFolderHandlerAsync` (NRE on
`_folderPredictorFactory`) instead of at the seam under test. #230 hit this on
`InitializeAsync`; `InitializeSequentialAsync`/`InitializeGraphicsAsync`/
`Initialize(bool)` did not reach that code and masked the gap, and the static
factories passed because they call `SaveParameters` themselves.

**How to apply:** also expect `InitializeAsync` (and therefore `CreateAsync`) to
drive `PopulateFolderComboBoxAsync`, which needs three extra `IApplicationGlobals`
mocks the other init members do not: `AF.CtfMap` (an empty `new CtfMap()` makes
`ContainsId` false), `AF.UseLcppnPredictor = true` + `AF.FolderPredictor` = a
`Mock<IFolderPredictor>` whose `Classify` returns an empty ordered sequence (this
selects the LCPPN seam and keeps the entire flat `Manager["Folder"]` Bayesian stack
out of the test), and `AF.RecentsList` = `new SloLinkedList<string>()` for
`FolderPredictor.FolderArray`. Widen the mock graph; never change production to
accommodate the test.
