---
name: project-452-efc-form-item-viewer-plan-seams
description: F9 (#452) EFC form/item/viewer coverage plan — cross-file seam ordering that forces a shared Phase 1, the DEC-1 Form-construction branch, and the IEfcFormViewer forward-member coverage trap
metadata:
  type: project
---

Planning epic #136 child F9 (#452, `EfcItemController.cs` / `EfcFormController.cs` / `EfcViewer.cs` /
`EfcViewer.Designer.cs`) forced three structural decisions that are not obvious from the "one phase
per production file" mandate.

**Why:** the four files' seams are mutually dependent, so a naive four-phase plan does not compile at
any intermediate commit.

**How to apply:**

1. **A shared Phase 1 is mandatory before the per-file phases.** `EfcFormController` must gain
   `IEfcExpansionStyleHost` and `IEfcViewerCommands` on its base list, and `EfcFormLayoutMath.cs`
   (holding `EfcItemViewerLayoutSnapshot`) must exist, before `EfcViewer` can implement
   `IEfcFormViewer.CaptureItemViewerLayout()`. Order the per-file phases
   **EfcViewer.cs → EfcFormController.cs → EfcItemController.cs**, not the manifest order: the form
   controller retypes `_formViewer` to `IEfcFormViewer`, which the viewer must already implement.
2. **DEC-1 is a real maintainer gate, not a formality.** `[ExcludeFromCodeCoverage]` on
   `EfcViewer.cs:20` sits on the partial *type*, so removing it also exposes the 4,277-line
   `EfcViewer.Designer.cs`. Approach A (one STA-constructed, never-shown, disposed `EfcViewer`) adds
   ~2,000 covered lines; Approach B forfeits them and requires editing generated code. Scope the
   branch by explicit task-ID IN/OUT lists in a Phase 0 task so a reversal costs one phase.
3. **The `IEfcFormViewer` 1:1 forwards are a coverage trap under Approach B.** Adding ~168 lines of
   intent-member forwards to `EfcViewer.cs` (162 → ~330) is fine under Approach A (the constructed
   viewer exercises them) but leaves every *setter*-shaped forward unreachable on a
   `GetUninitializedObject` instance. Plan an explicit measure-and-halt task rather than letting the
   file silently fall under 80%.

Related: [[project_437_efc_home_controller_plan_seams]],
[[partial-class-seam-declaration-and-consumption-same-phase]],
[[never-assert-method-name-on-lambda-valued-delegate]],
[[project_planner_mcp_validator_not_in_tool_surface]].
