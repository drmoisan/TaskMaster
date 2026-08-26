# Remediation Inputs — cycle 2

Entry timestamp: 2026-08-26T22-12
Issue: #614
Branch: `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
Branch head at entry: `b45e2a2d5b7f4d4219aa0caea4e63e24777feab1`
Base branch: `main` — merge-base `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`
Source audit: `policy-audit.2026-08-26T22-12.md`, `code-review.2026-08-26T22-12.md`,
`feature-audit.2026-08-26T22-12.md`
Prior cycle: `remediation-inputs.2026-08-26T21-00.md` / `remediation-plan.2026-08-26T21-00.md`

## Cycle 1 outcome

Cycle 1 was opened to fix two findings promoted to blocking by the orchestrator.

- **CR-1 is closed.** The minimum-length rule was removed from the filing predicate and confined to a new folder-creation predicate. Verified by this reviewer at `EfcSelectionGuard.cs:62-108` and at all three call sites (`EfcFormController.cs:468`, `:712`, `:758`, `:1044`), and pinned by `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` and `..._SingleCharacterRelativeStem_IsAccepted`. No further work is required on CR-1.
- **CR-2 is not closed.** The remedy satisfied the literal instruction ("the two guards must agree") by widening the filing predicate, but did not normalize the admitted value. The value class is still unfilable and now fails by unhandled exception instead of by dialog. Cycle 2 exists to close it properly.

Blocking count at cycle 1 exit: **1**.

## Blocking finding RC-1 — the widened filing guard admits a value the filing boundary throws on

**Defect introduced by cycle 1.** Verified against prior head `02092504` and merge base `c279d40b`.

`EfcSelectionGuard.IsValidFilingSelection(selection, archiveRoot)` (`EfcSelectionGuard.cs:75-81`)
now returns `true` for any rooted value that resolves against the archive root, including the
archive root itself. Two new tests pin this:

- `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` (`EfcSelectionGuardTests.cs:120-128`)
- `IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted` (`:134-141`), whose own comment calls it a "CR-2 recorded consequence"

Nothing between the guard and the filing boundary converts that value to a stem:

1. `EfcFormController.ActionOkAsync:722` calls `_homeController.ExecuteMovesAsync()`.
2. `EfcHomeController.ExecuteMovesCoreAsync` reads `_formController.SelectedFolder` verbatim.
3. `EfcDataModel.MoveToFolderAsync(string, ...)` assigns it verbatim: `DestinationOlStem = folderpath` (`EfcDataModel.cs:286`).
4. `EmailFiler.SortAsync` calls `EmailFilerConfig.ResolvePaths(Folder)`.
5. `ResolvePaths` calls `ArchiveStemContract.RequireArchiveRelativeStem(DestinationOlStem, ...)` (`EmailFilerConfig.cs:196-200`), which **throws `ArgumentException`** for any rooted value (`ArchiveStemContract.cs:79-86`).

The exception is not caught by `ExecuteMovesAsync` (try/finally, no catch) or by `ActionOkAsync`.
`ButtonOK_Click` (`EfcFormController.cs:429-443`) logs it and **rethrows** from an `async void`
handler, which on a WinForms synchronization context is an unhandled exception. It happens *after*
`_formViewer.Hide()` (`:718`), so the form has already disappeared.

Before cycle 1 the same value was rejected at the OK guard and produced only the dialog "Please
select a valid folder."

**Reachability is real, not theoretical.** `BreadcrumbRow.FilingTarget` is always the presented row
text (`BreadcrumbRowBuilder.cs:104-142` passes `presentedText` in all three branches). Presented rows
come from `FolderPredictor.FolderArray` / `FindFolder`. `ProjectSuggestionPath`
(`FolderPredictor.cs:845-858`) strips the archive prefix only when the suggestion is *strictly under*
it (`folderPath.Length > archivePrefix.Length`), so a suggestion whose folder **is** the archive root
is returned as a full rooted path verbatim. `BreadcrumbBridgeRouter.SelectRow` admits it, and the OK
guard now admits it too.

**Three guards, three answers for one value.** `SelectHierarchyPath` treats the archive root as a
deterministic non-selection (`stem.Length == 0` rejects, `BreadcrumbBridgeRouter.cs:487-494`);
`SelectRow` admits it verbatim; `IsValidFilingSelection` admits it; `RequireArchiveRelativeStem`
throws on it.

### Required outcome

The invariant `ArchiveStemContract` exists to establish is that `SelectedFolderPath` is always an
archive-relative stem. Enforce it **at the producer**, which is the only place it can be enforced
once before the value fans out to the OK path, the folder-creation path, the Find path, and the
recents list.

1. In `BreadcrumbBridgeRouter.SelectRow`, when `ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out var stem)` succeeds and `stem.Length != 0`, commit the **stem**: `CommitSelection(row, stem)`. When it succeeds with an empty stem (the value is the archive root itself), reject it exactly as `SelectHierarchyPath` already does — log the diagnostic and return without touching `SelectedFolderPath`. When `IsFullOutlookPath` is true and `TryMakeArchiveRelative` fails, keep the existing rejection. When the value is not rooted, keep the existing verbatim pass-through. When `_boundRoot` is empty, keep the existing unguarded pass-through.
2. Restore `EfcSelectionGuard.IsValidFilingSelection` to rejecting rootedness as such, so it agrees with `RequireArchiveRelativeStem`. Once step 1 lands, no producer emits a rooted value, so the guard's `archiveRoot` parameter becomes unnecessary; removing it and reverting to the single-argument signature is acceptable and preferred, provided `ResolveArchiveRootOrEmpty` and its call site are removed with it. Keep the CR-1 fix intact: the filing predicate must still carry no minimum-length rule.
3. Update `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` (`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs:165`) so that the asserted result is the stem rather than the rooted input. Quote the old and new assertion in the change description, as the delivery cycle did for the other #439 correction.
4. Delete or invert the two tests that pin the current behaviour: `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` and `IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted`. They must not survive as assertions that a rooted value is filable.
5. Add at least one **composition** test that fails before the fix and passes after: given a value that `IsValidFilingSelection` accepts, `EmailFilerConfig.ResolvePaths` must not throw. A pure-config test using the existing `Globals = null` seam and the parameterless `ResolvePaths()` overload is sufficient; it needs no COM. This is the test class that would have caught RC-1 and whose absence let it through.
6. Add a router test proving that selecting a row whose `FilingTarget` is the archive root exactly leaves `SelectedFolderPath` unchanged, matching the existing `SegmentActivate_ArchiveRootExactly_IsTreatedAsNonSelection` behaviour on the other selection path.

## Also in scope for this cycle

### RC-2 — amend AC16 in `spec.md`

AC16 states that `ActionOkAsync` and `IsValidSelection` "share one predicate" and that "OK rejects …
a non-relative selection". The cycle-1 split made the first clause false; the cycle-1 widening made
the second clause false. `spec.md` was not amended and AC16 is still checked `[x]`.

Required outcome: amend the AC16 text to describe the delivered design — two scope-specific
predicates in one shared guard type, with the filing predicate rejecting null, empty, whitespace,
banner sentinels and any rooted value, and the creation predicate additionally enforcing the
minimum-length rule. Clear the AC16 checkbox while the work is in progress and re-check it only when
the amended criterion is met. Record the amendment in the change description.

Do this in the same cycle as RC-1, because RC-1 changes the rootedness clause again.

## Optional, low-cost, adjacent

Include only if they add no risk to the RC-1 change:

- **RC-3.** `ResolveArchiveRootOrEmpty` guards one of nine `ArchiveRootPath` reads reachable from `EfcFormController`; two of the unguarded reads (`:777`, `:787`) run after `_formViewer.Hide()`. If step 2 above removes the resolver, RC-3 closes with it and its comment must be removed too. If the resolver is kept for any reason, the comment must be corrected so it does not claim a protection it provides on one path out of several.
- **RC-4.** `EmailFilerConfig.GetStem` (`:250-258`): the out-of-ancestor arm of the new ternary has no test, and it is the only measurable branch-coverage decrease on the branch (file branch coverage 70.0000% at the merge base, 60.0000% at this head). Add one test asserting that a `folderPath` outside `olAncestor` is returned with leading separators trimmed and does not throw.

## Explicitly out of scope for this cycle

- Prior CR-3: the `FolderConverter` alternative-folder-name cluster has no production entry point. Real observation, no user impact. Promote a separate issue; do not delete it here.
- Prior CR-4: `AppOlObjects.ArchiveRootPath` and `AppFileSystemFolderPaths.LoadFolders` now throw. Intended by AC13/AC14; the spec chose fail-fast over silent misfiling deliberately.
- The inert `internal AppFileSystemFolderPaths(Func<string,string>)` seam.
- The un-migrated `SortEmail.ResolvePaths` overload pair.
- The `FolderConverter.cs:265` `nameof(fsPath)` defect.
- The `IsLegalFolderName` versus `FindInvalidSegmentRule` rule-set asymmetry.
- The repository-wide line coverage shortfall (84.8790% against the 85% floor). Pre-existing, improved by this branch by +0.0993 points against the merge base, and below no numeric remediation trigger.
- The three files above the 500-line ceiling. Pre-existing, none grown against the merge base.
- AC26 live-Outlook validation. It cannot run headless; it remains a pre-release maintainer action.

## Constraints carried into this cycle

- Do not weaken D1, D4 or D9. A store-root, cross-store, above-archive, drive-rooted or separator-boundary-near-miss value must still be rejected at both the router and the filing boundary. Step 1 narrows what the router *emits*; it must not narrow what either guard *rejects*.
- Do not regress issue #609 or the #439 scenarios. `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` remains in its P3-T4-corrected form. Only the one assertion named in step 3 may change, and it must be quoted old-versus-new.
- Do not absorb or regress open issue #499: rejection paths must leave `SelectedFolderPath` unchanged, never set it to `null`.
- Do not modify `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`. RC-1's reachability runs through `ProjectSuggestionPath`, but the correct fix is at the router, not the projection; changing the projection would alter #609 behaviour.
- Redaction per issue #602: fabricated placeholders only, in code, tests, evidence and documents.
- Every new or changed behaviour needs a test that fails before the fix and passes after, with the fail-before run captured under `<FEATURE>/evidence/regression-testing/`.
- Evidence goes under `<FEATURE>/evidence/<kind>/` only. No `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/` or `artifacts/evidence/` path.
- File-size gates: `EfcFormController.cs` <= 1084, `BreadcrumbBridgeRouter.cs` <= 596, `BreadcrumbBridgeRouterIssue439Tests.cs` <= 694, `EfcSelectionGuard.cs` < 500.
- Full four-step toolchain in order, one clean pass, with `/t:Rebuild` on both MSBuild steps and without `/p:Nullable=enable`.
- Repository-wide line and branch coverage must not fall below this head's measured 84.8790% / 78.8523%. `EfcSelectionGuard.cs` and `ArchiveStemContract.cs` must remain at 100% line.

## Do not

- Do not "fix" RC-1 by catching `ArgumentException` at the OK path or anywhere downstream. Swallowing the boundary exception would restore the dialog but leave the value class silently unfilable and the invariant unenforced.
- Do not relax `RequireArchiveRelativeStem` to accept rooted values. It is the D4 filing boundary and the reason this feature exists.
- Do not mark any acceptance criterion `[x]` before its work is verified.
- Do not extend this cycle if a new finding surfaces during execution. Open cycle 3 instead.

## Handoff

Per `remediation-handoff-atomic-planner`, the remediation plan for this cycle is authored by
`atomic-planner`, not by this reviewer, and is expected at
`docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/remediation-plan.2026-08-26T22-12.md`
(flat timestamped form, matching the cycle-1 artifacts and the enforcement hook's expected shape).
It does not exist at the time this audit completes.

---

## Orchestrator addendum — why cycle 1 produced RC-1, and what cycle 2 must do differently

Appended by the orchestrator after reviewing the cycle-1 failure. The analysis above is the
reviewer's; this section is the orchestrator's own accounting and is binding on cycle 2.

### Root cause of RC-1 is the cycle-1 requirement wording, not the cycle-1 execution

The cycle-1 inputs told the planner: *"the two guards must agree."* That names a **symptom**. The
planner satisfied it the cheapest available way — widening the strict guard until it matched the
permissive one — which is a correct reading of what was asked and the wrong thing to build.

The requirement should have named the **invariant**: *the value that reaches the filing boundary
must already be an archive-relative stem.* Under that wording, widening a guard is obviously not a
solution, because it does not change the value.

Cycle 1's execution was not at fault. The plan was validated, preflight cleared it on the first
pass, and every gate it defined passed truthfully. The defect entered through the requirement.

### The verification gap that let it through

Cycle-1 preflight traced the guard **in isolation** and correctly confirmed that store-root,
cross-store, above-root, near-miss and drive-rooted values were all still rejected *at the guard*.
It never followed an **accepted** value forward to `RequireArchiveRelativeStem`. Every gate in the
cycle asked "does the guard reject what it should?" and none asked "is what the guard accepts
actually filable?"

That is why the reviewer's required outcome item 5 — a composition test asserting that a value
`IsValidFilingSelection` accepts does not cause `ResolvePaths` to throw — is the single most
important task in this cycle. It is the test whose absence permitted RC-1. It is mandatory, not
optional, and it must be a genuine fail-before/pass-after test.

### Binding constraints for cycle 2

1. **Enforce the invariant at the producer.** Normalize in `SelectRow` so `SelectedFolderPath` is a
   stem. Do not solve a guard disagreement by relaxing whichever guard is stricter — the stricter
   guard is the one stating the invariant.
2. **The composition test is mandatory** and must fail before the fix.
3. **Cycle 1's CR-1 fix must survive.** The filing predicate must still carry no minimum-length rule
   and `IsValidCreationSelection` must still back the folder-creation path. CR-1 was correctly
   resolved and is not reopened.
4. **Net effect versus the pre-remediation head must be an improvement on every path.** Cycle 1
   converted a benign "Please select a valid folder." dialog into an unhandled `ArgumentException`
   after `_formViewer.Hide()`. Cycle 2 must not leave any path worse than it was at `02092504`.
   State this comparison explicitly in the change description.
5. **Delete, do not preserve, the two tests that pin the RC-1 behaviour.** A test asserting that a
   rooted value is filable is asserting the defect.
6. RC-2, RC-3 and RC-4 as scoped by the reviewer above are in scope for this cycle.

### Not in scope

CR-3, CR-4, the Minor findings from the `2026-08-26T16-55` review, the pre-existing repo-wide
coverage shortfall, and the unexecutable live-Outlook AC26 steps all remain out of scope and retain
their recorded dispositions.
