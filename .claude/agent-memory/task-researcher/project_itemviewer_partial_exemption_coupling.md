---
name: itemviewer-partial-exemption-coupling
description: F14/#456 — one [ExcludeFromCodeCoverage] at ItemViewer.cs:20 hides all six ItemViewer partials plus the 6,224-line Designer; and ~8 QuickFiler.Test files already drive a headless ItemViewer.
metadata:
  type: project
---

Verified 2026-08-07 during F14 (`quickfiler-itemviewer-coverage`, issue #456) research for epic #136.

**1. One attribute, seven files.** `[ExcludeFromCodeCoverage]` sits on the *type* at
`QuickFiler/Viewers/ItemViewer.cs:20`. Because a partial type has one identity, it suppresses
instrumentation for all six partials (`ItemViewer.cs`, `.DisplayState.cs`, `.Commands.cs`,
`.Breadcrumb.cs`, `.FolderSearch.cs`, `.WebViewThread.cs`) **plus `ItemViewer.Designer.cs` (6,224
lines)**. Proven: the committed Cobertura at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
contains no `filename="...Viewers\ItemViewer*.cs"` entry at all, while sibling files in the same folder
(`Viewers\BreadcrumbUiDispatcher.cs`, `Viewers\BreadcrumbItemViewerLifecycleCoordinator.cs`) are present.

**Why it matters:** you cannot exempt only the Designer partial by re-adding the attribute there — that
re-applies it to the whole type and re-hides everything. Designer exemption must be filename-based
(harness / `coverage.config`), not attribute-based.

**2. "Assume 0% coverage" is a trap for `.Breadcrumb.cs`.** Absent-from-report != untested. At least
eight `QuickFiler.Test` files construct a **live headless `ItemViewer`** in a plain `[TestClass]` and
drive `ItemViewer.Breadcrumb.cs` members today:
`Viewers/BreadcrumbDropDownIntegrationTests.cs:338`, `Viewers/BreadcrumbCoordinatorLifecycleTests.cs:477`,
`Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs:413`, `Viewers/BreadcrumbPendingOpenCloseTests.cs:363`,
`Viewers/BreadcrumbSelectorOpenRetryTests.cs:255`, `Viewers/BreadcrumbSubfolderActivationTests.cs:305`,
`Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:373`,
`Controllers/QfcItemController.EventWiringTests.cs:236,327`.
`ItemViewer.WebViewThread.cs`, by contrast, genuinely is at zero — its members are only ever hit through
`Mock<IItemViewer>`.

**Why:** measure before authoring. Planning "from zero" against `.Breadcrumb.cs` would produce a large
duplicate-test volume.

**3. Two injection techniques, not interchangeable.**
- Full: `new QuickFiler.ItemViewer()` inside `SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())`
  (required — the ctor calls `TaskScheduler.FromCurrentSynchronizationContext()`). Gives the real Designer tree.
- Uninitialized: `FormatterServices.GetUninitializedObject` + public property assignment, per
  `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:247-265`. No `components`, no captured context.

**4. Do not retype Designer-backed properties.** `L0vhBreadcrumb_WebView2`, `L0v2h2_WebView2`,
`TopicThread`, `SentDate`, `MoveOptionsMenu` are injected *by concrete type* at
`QfcThemeHelperTests.cs:256-258`, and `ItemViewerBreadcrumbDropDownContractTests.cs:22-28` pins
`L0vhBreadcrumb_WebView2`'s exact `PropertyType`. Retyping breaks both plus
`QfcItemController.ViewerSetup.cs:109`. The working seam style is an **added sibling overload accepting
the collaborator** — see `ItemViewer.Breadcrumb.cs:40-43`, `:65-67`, `:179-183`, all introduced by issue
#400's P9-T12/P9-T28 remediation.

**How to apply:** re-verify item 1 with a grep for `ExcludeFromCodeCoverage` in
`QuickFiler/Viewers/ItemViewer*.cs` before relying on it; F14 is expected to remove that attribute.

Related: [[qfc227-headless-itemviewer-and-tlpcellsnapshot]], [[net481-timeprovider-available]],
[[quickfiler-percoverage-epic-136]].
