# efc-dead-code-and-latent-nre-traps (Issue #466)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-dead-code-and-latent-nre-traps/ (Issue #466)
- Work Mode: full-bug

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #466
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/466
- Last Updated: 2026-08-08
## Summary

`EfcViewer.SetController` is never called, so `EfcViewer._formController` is permanently null and
`EditFiltersMenuItem_Click` carries a latent `NullReferenceException` that a routine Designer
regeneration would arm. Several other EFC members and one orphaned file are dead code.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- UI path: `QuickFiler/Viewers/EfcViewer.cs`, `QuickFiler/Controllers/EfcItemController.cs`
- Data source or fixture: n/a

## Steps to Reproduce

The trap is currently unreachable by design; it is armed by a maintenance action:

1. Open `EfcViewer` in the Visual Studio Designer and re-generate `EfcViewer.Designer.cs`, or otherwise
   wire `EditFiltersMenuItem.Click` to `EfcViewer.EditFiltersMenuItem_Click`.
2. Open the Email Filer viewer and choose the Edit Filters menu item.
3. Observe `NullReferenceException`.

## Expected Behavior

Either the controller is wired into the viewer so the Edit Filters command works, or the dead member
and its unreachable handler are removed so no trap remains.

## Actual Behavior

**A — dead `SetController`, permanently null field.** `EfcViewer.cs:50-53` declares
`internal void SetController(EfcFormController controller)`. A repository-wide search for
`SetController` finds call sites only in `QfcFormController.cs:44`, `QfcFormViewer.cs:46`,
`QfcFormViewerDark.cs:31`, `QfcFormViewerExpanded.cs:31`, and the non-compiled `Legacy/` and
`EfcViewer3.cs` files. **`EfcFormController` never calls it**, unlike its QFC twin. Consequently
`EfcViewer._formController` (`EfcViewer.cs:48`) is always null and
`EfcViewer.EditFiltersMenuItem_Click` (`EfcViewer.cs:157-160`) would throw.

The handler is currently unreachable: `EfcViewer.Designer.cs` never wires `EditFiltersMenuItem.Click`.
Verified — across 4,277 lines the only references are the declaration at `:67`, the `DropDownItems`
add at `:4123`, three property assignments at `:4136-4138`, and the field at `:4275`. There is no
`+=` at all. So this is dead code carrying a latent trap, not a live crash.

**B — zero-call-site members in `EfcItemController`.** `InitializeWebView()` (`:174-205`) and
`RegisterActions(...)` (`:680-692`) have no call sites repo-wide (verified by grep across `QuickFiler/`
and `QuickFiler.Test/`). `_selectorsCtrls` (`:381`) is initialized to `null` and never assigned before
being passed to `SetupThemes` as the `selectors` argument (`:97`, `:144`).

**C — unused constructor overload.** The `EfcItemController`
`(globals, homeController, parent, itemViewer, dataModel, bool async, token)` overload (`:44-57`) has
zero call sites; only the 6-argument (`:30`) and 5-argument (`:59`) forms are used, by
`EfcFormController.cs:87` and `:69`.

**D — orphaned uncompiled files.** `QuickFiler/Viewers/EfcViewer3.cs` and its siblings are present in
the working tree but carry no `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj`. `EfcViewer3.cs`
nonetheless carries an `[ExcludeFromCodeCoverage]` attribute at `:17`, which is misleading — an
uncompiled file needs no exemption.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence recorded above (verified 2026-08-07 against the working tree).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Nothing fails today. The severity is the maintenance trap: the Edit Filters command is silently
non-functional, and the natural act of re-wiring it in the Designer converts that into a crash.

## Suspected Cause / Notes

The EFC viewer appears to have been modelled on the QFC viewer, which does call `SetController`, but
the corresponding call was never added on the EFC side. The dead members in `EfcItemController` look
like superseded initialization paths left behind after refactors.

Deciding between "wire it up" and "delete it" requires knowing whether the Edit Filters command is
intended to be available on this form — a product question, not a mechanical one, which is why this is
promoted rather than fixed opportunistically.

Note that `EfcViewer3.cs`'s attribute contributed to an inflated exemption count in earlier surveys;
the epic manifest for #136 records the correction (21 real attributes on compiled files, not 33).

Discovered during preparation of issue #452 (epic #136) per-file coverage research. Out of scope there
under that feature's no-behavior-change constraint.

## Proposed Fix / Validation Ideas

- [ ] Decide whether the Edit Filters command should be available on `EfcViewer`; wire or remove accordingly
- [ ] Remove the zero-call-site members and constructor overload, or document why they are retained
- [ ] Resolve `_selectorsCtrls` — assign it or stop passing it
- [ ] Delete the orphaned `EfcViewer3.*` files, or add them to the project if they are still wanted
- [ ] Unit coverage: whichever disposition is chosen for the Edit Filters path
- [ ] Manual verification: Edit Filters menu item behaves as decided

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
