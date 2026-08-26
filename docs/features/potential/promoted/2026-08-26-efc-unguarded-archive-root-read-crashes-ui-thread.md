# efc-unguarded-archive-root-read-crashes-ui-thread (Issue #638)

- Work Mode: full-bug

- Issue: #638
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/638
- Last Updated: 2026-08-26
- Status: Promoted -> docs/features/active/efc-unguarded-archive-root-read-crashes-ui-thread/ (Issue #638)
## Summary
`EfcDataModel.MoveToFolderAsync` reads `Globals.Ol.ArchiveRootPath` without a guard, and when the
archive root cannot be resolved that read throws `InvalidOperationException` all the way out to an
`async void` WinForms handler, which rethrows it as an unhandled UI-thread exception. The add-in
tears down instead of degrading.

This was confirmed by static tracing during issue #614 remediation. Every link in the chain was
verified; no frame between the throw site and the UI boundary catches the exception.

**Throw site.** `TaskMaster/AppGlobals/AppOlObjects.cs:253-267` exposes `ArchiveRootPath`, which
delegates to `ArchiveRootPathGuard.RequireResolvedArchiveRoot`. That helper throws
`InvalidOperationException` unconditionally on either of two conditions:
`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:44` when the archive folder does not resolve (the
profile has no `Archive` folder in the default store), and
`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:56` when the resolved folder path is not
case-insensitively equal to `Path.Combine(Root.FolderPath, "Archive")` (archive in a second store,
renamed, or a delegate mailbox).

**No negative caching.** The backing field `_archiveRootPath` is assigned only when the helper
returns. On throw it stays null, so every subsequent read throws again. For an affected profile the
failure is permanent and reproduces on every attempt; there is no throw-once-then-degrade behavior.

**Unguarded read.** `QuickFiler/Controllers/EfcDataModel.cs:289` performs
`OlAncestor = Globals.Ol.ArchiveRootPath` inside the `string` overload of `MoveToFolderAsync`. The
method body at `QuickFiler/Controllers/EfcDataModel.cs:259-297` contains no try/catch. The read is
reached unconditionally once the two early `return false` guards
(`QuickFiler/Controllers/EfcDataModel.cs:267-270` and `:277-281`) pass. The same unguarded pattern
appears at `QuickFiler/Controllers/EfcDataModel.cs:310` in `OpenOlFolderAsync` and at `:328` in
`OpenFsFolderAsync`.

**Propagation chain, no handler.** `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:86-109`
has no try. `ExecuteMovesCoreAsync` at `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:64-84`
has no try. `ExecuteMovesAsync` at
`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:31-46` wraps its core in `try` / `finally`
with **no catch**; the `finally` resets execution state and the exception continues unchanged. This
frame is the one most likely to be mistaken for a handler, and it is not one.
`EfcFormController.ActionOkAsync` has no try/catch around its await.

**Boundary.** `QuickFiler/Controllers/EfcFormController.cs:429-443` is
`public async void ButtonOK_Click(...)`, wired to a WinForms `Button.Click` at
`QuickFiler/Controllers/EfcFormController.cs:389`. Its catch logs and then **rethrows** at
`QuickFiler/Controllers/EfcFormController.cs:441`. A rethrow from `async void` reposts to the
captured WinForms `SynchronizationContext`, producing an unhandled UI-thread exception. The same
`async void` + log + rethrow shape appears at `:413-427`, `:445-459`, `:461-519` and `:521`.

**Relationship to other issues.** This is distinct from issue #637, which covers producer-side
normalization of rooted paths at `BreadcrumbBridgeRouter.SelectRow` and the half-wired D8
normalizer. This defect is an archive-root *resolution failure*, not a path-rootedness problem, and
it fires even when the selection is a perfectly well-formed archive-relative stem. It is also
pre-existing rather than introduced by issue #614: the crash path is present at pre-remediation head
`02092504`, was neither created nor removed by remediation cycle 1, and is unchanged by the cycle-2
partial revert.

One nuance worth recording. Remediation cycle 1 added
`EfcSelectionGuard.ResolveArchiveRootOrEmpty` at `QuickFiler/Controllers/EfcFormController.cs:708`,
which catches `InvalidOperationException` and degrades to an empty root. That made the OK path look
protected while leaving the second, unguarded read at
`QuickFiler/Controllers/EfcDataModel.cs:289` to crash anyway, because
`EfcSelectionGuard.IsValidFilingSelection` returns true for any non-rooted value regardless of the
archive root. The cycle-2 partial revert removes that call site, so the misleading appearance of
handling goes away, but the underlying crash does not.

**Suggested direction.** Route the reads at `QuickFiler/Controllers/EfcDataModel.cs:289`, `:310` and
`:328` through the same degrade-and-report path used elsewhere, or have the filing entry point
reject with a user-facing message when the archive root cannot be resolved, rather than allowing an
`InvalidOperationException` to reach an `async void` boundary. Separately, the rethrow at
`QuickFiler/Controllers/EfcFormController.cs:441` should be reconsidered: rethrowing from
`async void` converts every handled-and-logged error into a process-level crash.

## Environment
- OS/version: Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 VSTO add-in.
- Command/flags used: Static reachability tracing during issue #614 remediation cycle 2.
- Data source or fixture: Repository source on the issue #614 branch.

## Steps to Reproduce
1. Use an Outlook profile whose archive folder does not resolve to the default store's `Archive`
   folder - for example a profile with no `Archive` folder, or one whose archive lives in a second
   store such as `\mailbox@example.com\Archive`, or a renamed archive folder.
2. Open the QuickFiler email-filer form with `InitTypeEnum.Sort`.
3. Select a valid archive-relative destination stem such as `Clients\North` - a non-rooted value
   that the OK-path guard accepts.
4. Press OK.
5. Observe an unhandled `InvalidOperationException` on the UI thread rather than a message box.

## Expected Behavior
An unresolvable archive root produces a clear, user-facing diagnostic and leaves the add-in running.
No archive-root resolution failure reaches an `async void` handler as an unhandled exception.
