---
name: uithread-static-seam-belongs-to-584
description: The IUiDispatcher seam conversion (~62 refs / 29 production files) should attach to existing issue #584, not a new issue — #584 and #493 share the UiThread.Dispatcher static as root object
metadata:
  type: project
---

Do not promote a new issue for "replace the `UiThread.Dispatcher` static with the existing
`IUiDispatcher` seam". Record the scope on existing issue **#584** instead, and cross-link #493.

**Why:** #584 (`UiThread.Dispatcher null race`) was promoted by epic child #449 and its recorded
structural root cause is that `UiThread.Dispatcher` is "backed by a `null!`-initialised static with
no lazy initialisation" (`docs/features/archive/2026-08-07-quickfiler-explorer-controller-latent-defects-449/policy-audit.2026-08-22T10-58.md:75`;
listed as an open follow-up at `docs/features/epics/quickfiler-suite-determinism-foundation/epic-status.md:168`).
#493 is unrestored/unsynchronized *mutation* of the same static. Different symptoms, same root
object, and the seam conversion dissolves both. A third issue would fragment the tracking.

**How to apply:** When any feature touching `UtilitiesCS/Threading/UiThread.cs` recommends the seam
conversion as follow-up, check #584 first. The measured scope is ~62 references across 29
first-party production files (heaviest: `QuickFiler/Controllers/QfcCollectionController.cs`,
`QfcQueue.cs`, `QuickFiler/Helper Classes/ItemViewerQueue.cs`,
`TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`). Related: [[promote-latent-defects-to-issues]].
