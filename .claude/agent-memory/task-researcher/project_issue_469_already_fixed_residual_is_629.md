---
name: issue-469-already-fixed-residual-is-629
description: Issue #469's four defects verified fixed on main (2026-08-29); the only residual is parameter removal, already tracked as issue #629 — plus the AppAutoFileObjects.Initialized<T> non-memoization defect found while verifying the undo-stack doc claim
metadata:
  type: project
---

Verification of issue #469 (`qfc-collection-move-diagnostics-defects`) against the tree at
`origin/main` = `ecdb1c84` on 2026-08-29. Full artifact:
`docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/research/2026-08-29T12-31-qfc-collection-move-diagnostics-defects-469.md`.

**1. All four #469 defects are resolved; the only residual action is already issue #629.**
Defects 1-3 (null guard, trailing null element, ConcurrentDictionary ordering) are fixed with
regression tests. Defect 4's remaining option — deleting the `stackMovedItems` parameter — was
promoted on 2026-08-26 as **issue #629**
(`docs/features/potential/promoted/2026-08-26-qfc-remove-stackmoveditems-parameter.md:11-12`).
**Why:** #469 looks open and its Expected Behavior item 4 reads unsatisfied, so it invites a
duplicate fix branch that would also breach the #468 scope lock on
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs`.
**How to apply:** before planning any #469 work, check for the #629 promoted document. Removal
touches only 8 code lines in 4 files; zero Moq expressions and zero reflection sites name the method.

**2. `AppAutoFileObjects.Initialized<T>` does not memoize** —
`TaskMaster/AppGlobals/AppAutoFileObjects.cs:43-50` takes the backing field **by value** and never
assigns it, so while `_movedMails` is null every read of `AF.MovedMails` re-runs
`LoadMovedMails()`, and `SloStack.Static.Deserialize` returns a **new** object each call. Backs
`MovedMails`, `Encoder`, `SubjectMap`.
**Why:** the shipped doc comment on `MoveEmailsAsync` asserts the caller's stack and the filer's push
target are "the same instance"; that is true only after `LoadMovedMailsAsync` has cached the field,
which the comment does not state. Two existing tests already pin the null case
(`TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs:94`, `:113`).
**How to apply:** never accept "same globals object, therefore same member instance" for an
`IApplicationGlobals` lazy property without checking whether the initializer writes back.

**3. The prompt-level premise "EmailFiler uses a static Globals" is FALSE.**
`EmailFiler.Globals` is an instance property (`EmailFiler.cs:71-76`) fed from `Config.Globals`
(`:373`), which QuickFiler sets to its own injected `_globals`
(`QfcItemController.MailActions.cs:131`). Likewise `QfcHomeController.Globals` and
`RibbonController.Globals` are instance properties. The only static `Globals` in the repo is the
VSTO-generated `TaskMaster/ThisAddIn.Designer.cs:171`, untouched by the move path.

**4. Defect 1/2 numbering is INVERTED between the issue and the shipped code.**
`issue.md` and `docs/features/active/qfc-collection-controller-defects-468/spec.md:92-93` call the
null guard "defect 1"; `QfcCollectionController.cs:2362`/`:2372` and
`QfcCollectionControllerDefects468MoveTests.cs:275`/`:351` call it "defect 2".
**How to apply:** any acceptance criterion naming "#469 defect 1" is ambiguous — state the behaviour,
not the number.

**5. Two live stale comments assert the pre-fix trailing-null behaviour:**
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:171-173` and
`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:397-400`. The `IsNullOrWhiteSpace`
filter they justify is vacuous against the production implementation but is still exercised through a
`Mock<IQfcCollectionController>`, so **deleting it fails**
`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`.

**6. Non-compiled files that look live:** `QuickFiler/Notes/notes_interfaces.cs` (declares a second
`IQfcCollectionController`) and the whole `QuickFiler/Legacy/` folder (still carries the identical
`new string[EmailsLoaded + 1]` off-by-one at `QfcGroupOperationsLegacy.cs:1272`). Neither appears in
`QuickFiler/QuickFiler.csproj`, which is a legacy non-SDK project with explicit `Compile Include`
items — absence means exclusion.

Also confirmed: `QfcCollectionController.cs` is 2,437 lines, ~4.9x the 500-line cap, adjudicated
non-blocking as PA-2 and delegated to issue #623 under an AC-25 no-split constraint, so changes there
should be net-neutral or net-negative in lines. `QfcCollectionControllerTests.cs` is exactly 500
lines and can take no new methods.

See also [[qfc-collection-controller-defects-468]] and [[qfc-collection-defects-468]].
