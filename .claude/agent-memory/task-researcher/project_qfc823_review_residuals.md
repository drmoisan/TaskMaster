---
name: qfc823-review-residuals
description: "Issue #823 research (2026-09-08): the R3 null-tolerance rationale is FALSE (?. guards the receiver not the args), R2's follow-up already exists as #813, and the per-store retry rescope passes every existing #812 test unchanged"
metadata:
  type: project
---

Issue #823 batches six low-severity residuals from the #810/#812 reviews. Research artifact:
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/research/research.2026-09-08T23-50.md`.

Five facts that are not derivable by re-reading the file each finding names:

1. **R3's documented justification is factually false.** `BreadcrumbPopupOwnerRegistry.cs:31-33`
   claims null is ordinary input because "the registration hop runs from a form lookup that can
   legitimately find no form". The only production call site is
   `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:216`:
   `(FindForm() as QfcFormViewer)?.SetBreadcrumbPopupOwner(this, () => host.IsOpen);` — the `?.`
   makes the failed lookup a null RECEIVER, never a null ARGUMENT. `this` and a lambda literal can
   never be null. So the SIGNATURE is right and the CONTRACT (doc + test) is wrong.
2. **Deleting R3's guard without an explicit throw is a regression.** A null predicate would be
   stored successfully and become a deferred `NullReferenceException` at
   `BreadcrumbPopupOwnerRegistry.cs:59` (`AnyOpen`), on the #677 deactivation path. The fix must be
   an explicit `ArgumentNullException`, not a deletion.
3. **R2's "owed" follow-up already exists.** The #812 code review recorded the
   `QfcItemController.FolderHandling.cs:233` filing as owed because it could not be confirmed from
   the working tree; it WAS filed as **issue #813**
   (`docs/features/potential/promoted/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read.md`)
   and is a wave-0 sibling in the same epic. The exception type is `InvalidOperationException` from
   `ArchiveRootPathGuard.RequireResolvedArchiveRoot`, unchanged before and after; only the statement
   moved, and it still reaches the UI dispatcher unhandled.
4. **The R1 per-store rescope passes all six existing #812 tests unchanged**, but only if the latch
   stays an INSTANCE field. `StoreWrapperController_Tests.Display.cs:200` asserts
   `Times.Exactly(2)` across two controllers over one shared store; a `static` per-store latch would
   produce 1 and fail. Key by `StoreWrapper` reference (no `Equals`/`GetHashCode` override on the
   type), not by `StoreId` — `StoreId` is nullable and already has four fail-safe unreadable-value
   branches, so string keying collapses every unreadable store onto one shared budget, which is R1's
   own defect.
5. **No `#nullable enable` outside the registry file itself.** `QfcFormViewer.cs`,
   `ItemViewer.Breadcrumb.cs` and `BreadcrumbPopupOwnerRegistryTests.cs` are all oblivious, and no
   `.csproj` declares `<Nullable>`, so an annotation change on `Register` can produce no new CS86xx
   anywhere. Adding `#nullable enable` to the TEST file would raise CS8625 on its two null literals
   and fail the `TreatWarningsAsErrors` gate — do not add it.

Also: three sibling stale line-count comments in `QuickFiler/Viewers/` besides R4's
(`BreadcrumbBridgeCoordinator.Search.cs:11` says 487 / is 437;
`BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10` says 481 / is **497**, i.e. it understates a
file 3 lines from the cap). No accumulating flake register exists in this repo; the precedent for a
flaky test is a promoted potential record (#780, #`2026-08-15`, #`2026-08-08`), and the one in-code
precedent is an XML `<para>` at `QfcCollectionControllerDefects468MoveTests.cs:72-80`.

**Why:** each of these was found only by reading the caller, the sibling test, or the potential
folder — not the file the finding names. R3 in particular would have been "fixed" the wrong way by
trusting its own doc comment.

**How to apply:** on any follow-up in the #810/#812/#823 family, enumerate call sites before
accepting a null-tolerance rationale, and check `docs/features/potential/promoted/` before recording
a follow-up as unfiled. See [[qfc810-teardown-dropdown-residuals]] and
[[efc614-store-root-stem-leak]].

Session note: Bash was unavailable in this agent worktree (Read/Grep/Glob only), so no build, test or
csharpier evidence was produced; all nullable-diagnostic predictions are static readings.
