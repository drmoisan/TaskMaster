---
name: project-468-qfc-collection-controller-plan-seams
description: "#468 QfcCollectionController seven-issue bugfix — ToggleUnGroupConv cannot be driven COM-free, MakeSpaceForItems never touches Size, the ShrinkByRows seam must land sign-preserving, and LoadItemGroup is a substring of a live member"
metadata:
  type: project
---

Planning seams found while writing the `#468` plan (closes #286, #468, #469, #470, #471, #473, #474;
all in `QuickFiler/Controllers/QfcCollectionController.cs`, 2349 lines).

1. **`ToggleUnGroupConv` cannot be driven COM-free.** Its first two statements are
   `SafeSetTlpLayout(false)` and `UnregisterNavigation()`, and `MakeSpaceForItems` reaches
   `TableLayoutHelper.InsertSpecificRow` on `_itemTlp`. So the `#470-2` above/equal/below-reservation
   cases and the `baseEmailIndex == -1` guard have no permanent red-then-green test at that level.
   Move the assertions onto pure static helpers (`ResolveConversationInsertions`,
   `ReconcileInsertionCount`) and put the behavioural pre-fix red state in the dossier. Research
   §3.5 assumed the post-fix reconciliation would throw before the loop; the spec chose
   log-and-proceed instead, which removes that assumption — check which one the spec adopted before
   copying a research test recipe.

2. **`MakeSpaceForItems` adjusts `MinimumSize` only; `EliminateSpaceForItems` adjusts both
   `MinimumSize` and `Size`.** AC-11's "make-then-eliminate is height-neutral" is therefore only
   true for `MinimumSize`. Scope the neutrality assertion and record the `Size` asymmetry as
   pre-existing, or the acceptance clause is false after a correct fix.

3. **A behaviour-preserving seam over a sign defect must land carrying the defect.** `ShrinkByRows`
   extracted from `EliminateSpaceForItems` has to keep the inverted argument at the call site so the
   seam commit changes nothing observable (AC-20). The fail-before for `#471` therefore lives at the
   CALL SITE (the STA panel test), never on the pure helper — the helper is correct by construction
   the moment it exists.

4. **`LoadItemGroup` is a substring of the live `LoadItemGroupsAndViewers_02`.** A fixed-string
   zero-hit gate on the dead member must assert `LoadItemGroup(` with the paren. The same trap
   almost applies to `AnyOpenDropDowns` / `AnyOpenDropDownsAsync` and
   `LoadConversationsAndFolders_04` / `LoadConversationsAndFoldersAsync`; only the `Async` suffix
   saves those two.

5. **Scope every `#468` identifier sweep to the single production file.** All thirteen identifiers
   appear in `docs/features/**` (spec, both research docs, the promoted potential entries, and the
   plan itself), so a repository-wide zero-hit gate is unsatisfiable by construction.

6. **`QuickFiler.Test.csproj` line facts (base commit `988e819b`):** 116 is the
   `QfcCollectionControllerTests.cs` entry, 117 the `QfcCollectionControllerDarkModeTests.cs` entry,
   118 the `QfcDatamodelTests.cs` entry. New `QfcCollectionController*` entries go between 117 and
   118. `QfcCollectionControllerTests.cs` is exactly 500 lines and takes no new method;
   `QfcCollectionControllerDarkModeTests.cs` is 155.

7. **The class carries `[ExcludeFromCodeCoverage]` at `:21` and removing it is out of scope.** Never
   author an acceptance clause claiming the feature raises coverage on that file — it cannot fail.
   Capture baseline and final coverage numerically anyway and state the non-attribution explicitly.

Related: [[trx-needs-resultsdirectory]], [[agent-worktrees-need-sdk-and-nuget-bootstrap]],
[[worktree-root-breaks-dotclaude-exclusion]], [[feedback_ac_checkoff_one_per_task]],
[[diff-gates-need-a-commit-task]].
