---
name: qfc-conversation-seam-ratified-453
description: F10 (#453) cycle-2 correction — DoLoadConversationResolverCoreAsync (Conversation.cs:79) is one of #227's ratified "deliberate virtual test seam" exemptions, not removable; same bucket also covers Navigation.cs's ToggleExpansion/ToggleExpansionAsync
metadata:
  type: project
---

Cycle-2 correction to my own F10 (issue #453, epic #136) per-file research, 2026-08-07. My three
assigned artifacts in `docs/features/active/2026-08-07-quickfiler-item-controller-coverage-453/research/`
are `file-QfcItemController.Conversation.md`, `file-QfcItemController.FolderHandling.md`, and
`file-IQfcItemController.md` (two production files + the interface file — confirmed by internal
cross-references: FolderHandling.md says "the file in my assignment"/"both files in my assignment",
Conversation.md says "not one of my three files", IQfcItemController.md says "F10's two production
files" when referring to Conversation+FolderHandling). Two other tracks exist in the same folder:
{base QfcItemController.cs, Initialization.cs, ViewerSetup.cs} (explicitly "three F10 artifacts" per
their own text) and {Navigation.cs, EventHandlers.cs, EventWiring.cs} (explicitly "three assigned
event/navigation files"); MailActions.cs+FocusAndTheme.cs are a fourth, 2-file track.

**The correction:** my initial pass recommended removing `[ExcludeFromCodeCoverage]` from
`DoLoadConversationResolverCoreAsync` (`Conversation.cs:79`) as `removable-with-seam`. This was wrong
context, not wrong fact — the member IS reachable, but the maintainer had already ratified this exact
exemption on 2026-07-02 (issue #227) as one of 3 "deliberate virtual test seams" ("the override point
IS the test seam by design; the base body is intentionally unexercised directly" —
`docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`
§3). Re-verified against current code (2026-08-07): still `protected virtual`, still overridden by
exactly 2 test fixtures (`ConversationTests.cs:37`, `QfcItemControllerTests.cs:46`), base body still
never exercised by any test — no drift, rationale still holds. Withdrew the removal recommendation.

**Same bucket ("deliberate virtual test seams", 3 total, all in #227's ratified 19) also covers
`ToggleExpansion(Enums.ToggleState)` and `ToggleExpansionAsync(Enums.ToggleState)` in
`Navigation.cs`** — NOT my file, but the cross-cutting companion artifact
(`cross-cutting-exemption-and-coverage-analysis.md` §1.2, sites 15-16) and `file-QfcItemController.Navigation.md`
both classify those as `removable-as-is` and recommend de-exemption. That recommendation likely has
the same "reachable therefore removable" flaw mine had. Flagged in Conversation.md §12 but not
corrected there — out of scope for my assignment.

**Measurement-methodology note:** for an expression-bodied member, Cobertura emits exactly ONE `<line>`
entry regardless of source span (positive control: `get_TopFolderScore`, `QfcItemController.cs:251-254`
emits a single `<line number="254">`). The cross-cutting doc's `Δlines` column uses a cruder heuristic
(counting physical non-blank/non-comment lines in the member span) and gives `+8` for this site; the
true denominator cost if ever de-exempted is `+1` (102 -> 103). Use the per-file arithmetic-proof
method, not the cross-cutting `Δlines` column, when a member is expression-bodied.

**Only 1 of the 19 ratified exemptions is genuinely unratified** (per orchestrator, confirmed by the
2026-07-02T17-00 evidence's own 19-member enumeration, which does not include it):
`EnsureBreadcrumbPipeline` at `ViewerSetup.cs:132` — entered after the 2026-07-02 ratification.

**Open-issue landscape as of 2026-08-07 (post `gh` verification by orchestrator):** #400 and #424 are
CLOSED (despite epic.md listing them as live conflict risks — folder-scan false positive, opposite
direction from the epic's own warning). Open and relevant to F10: #230 (WinForms message-pump seam,
blocks 9 members), #427, #438, #440, #426, #441 (Cobertura double-count, already filed), #457
(`[ExcludeFromCodeCoverage]` doesn't suppress nested lambdas, already filed), #463 (WebView2 en-dash
arg, already filed), #444. Do not re-promote #441/#457/#463 as new latent defects.

**How to apply:** before recommending removal of any `[ExcludeFromCodeCoverage]` in the
`QfcItemController` family, check it against the 2026-07-02T17-00 ratified-boundary evidence first —
reachability alone is not sufficient grounds; the maintainer has already adjudicated the "virtual
seam is the test pattern, not a barrier" argument for 3 specific members. Related:
[[qfc-item-controller-227-r2-denial]], [[qfc-item-controller-230-pump-seam-blocks-exemption-removal]],
[[qfc-item-controller-f10-init-viewersetup-453]] (the OTHER F10 track — base/Init/ViewerSetup, not
mine), [[cobertura-exemption-and-branchrate-gotchas]].
