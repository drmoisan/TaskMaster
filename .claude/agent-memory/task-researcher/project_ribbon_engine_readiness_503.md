---
name: ribbon-engine-readiness-503
description: "#503 ribbon engine-readiness research: whole TaskMaster Ribbon layer is [ExcludeFromCodeCoverage]; net481 blocks default interface members; 5 orphan onAction callbacks found in RibbonExplorer.xml"
metadata:
  type: project
---

Issue #503 (ribbon engine-readiness guard) research, 2026-08-08. Recommended design: readiness computed from the existing `IAppItemEngines.InboxEngines` member in new host-neutral `internal sealed` types under `TaskMaster\Ribbon\`, with **zero** change to `AppItemEngines.cs` / `IAppItemEngines.cs`.

**Why:** Three non-obvious repo facts forced that choice, and each is expensive to rediscover.

1. **The entire TaskMaster Ribbon layer is coverage-excluded.** `[ExcludeFromCodeCoverage]` sits on `RibbonController.cs:36` and `RibbonViewer.cs:32`, and `AppItemEngines.cs:26`. Because the attribute is type-level, it also silences `RibbonController.Intelligence.cs` and `RibbonController.FolderTree.cs` (same partial type) — so `RibbonControllerTests.cs` runs but contributes no covered lines. Any "put the logic on the controller" or "put it on AppItemEngines" proposal is uncoverable by construction.
2. **net481 has no default interface members.** `TaskMaster.csproj` targets `v4.8.1` with `LangVersion=preview`; MS docs state DIM "require enhancements in the CLR … added in the CLR for .NET Core 3.0" (Roslyn CS8701). So a member added to `IAppItemEngines` can only be bodied on `AppItemEngines`, i.e. inside the excluded class. This is what kills the otherwise-clean `IsEngineReady(string)` interface option.
3. **`IAppItemEngines` ripple is tiny, not large.** Exactly ONE implementer (`AppItemEngines`). The ~7 other hits are `IApplicationGlobals` doubles that merely expose an `IAppItemEngines Engines` property; the 4 `Mock<IAppItemEngines>` sites compile unchanged. So "ripple size" is NOT the reason to avoid the interface change — coverage is.

**Coverable-seam precedent to copy:** `TaskMaster\AppGlobals\HookReadinessCoordinator.cs` and `EngineInitTimingProbe.cs` / `StartupDiagnosticsProbe.cs` each carry an explicit "intentionally NOT marked `[ExcludeFromCodeCoverage]`" remark and take `Action<string>` / interface delegates. Mirror that shape (and that doc-comment convention) for any new decision logic in TaskMaster.

**Latent defects found in `RibbonExplorer.xml` (out of scope for #503, promote separately):**
- Five `onAction` values with no matching `RibbonViewer` method: `BtnMigrateIDs_Click`, and a `_Clicked`-vs-`_Click` suffix mismatch on `MoveEntireConversation_Clicked`, `SaveAttachments_Clicked`, `SaveEmailCopy_Clicked`, `SavePictures_Clicked`. VSTO compiles fine and silently does nothing — all four Quick Filer settings check boxes are inert.
- `SpamBayesEnabled_GetPressed` / `TriageEnabled_GetPressed` are `async Task<bool>`; the required signature is `bool GetPressed(IRibbonControl)`, so the pressed state is never applied.
- A generalised "every ribbon callback resolves to a public RibbonViewer method with the documented signature" test in `RibbonExplorerXmlTests.cs` would catch this whole class, but it goes red on the five pre-existing orphans — ship it with the fix, not with an unrelated feature.

**How to apply:** When any future TaskMaster ribbon/globals work asks "where do I put testable logic", assume the obvious host class is coverage-excluded and check for the attribute FIRST; then place logic in a new non-excluded `internal sealed` type and leave a one-method shim behind. See [[qfc-item-controller-227-r2-denial]] for why `[ExcludeFromCodeCoverage]` cannot substitute for a real seam, and [[feedback-promote-latent-defects-to-issues]] for the orphan-callback handling.

Research artifact: `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/research/2026-08-08T12-45-ribbon-engine-readiness-guard-research.md`
