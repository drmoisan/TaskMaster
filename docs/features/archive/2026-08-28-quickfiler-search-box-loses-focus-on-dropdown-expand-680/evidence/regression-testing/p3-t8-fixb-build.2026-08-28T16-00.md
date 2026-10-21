# P3-T8 — Fix B Build (controller dismissal ownership)

Timestamp: 2026-08-28T16-00

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
(run with `/v:m`)

EXIT_CODE: 0

Output Summary:

- Build succeeded with 0 error lines.
- 5 warning lines, all the pre-existing `System.Reactive.PackagesConfigCheck.targets`
  `packages.config` advisory recorded at baseline. Warning count unchanged from baseline.
- Fix B is P3-T7, entirely inside `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`:
  the `_searchLeaveHandoffPending` latch field, the latch write in the `Keys.Down` branch
  immediately before `FocusFolderDropDown()`, the new `Keys.Escape` branch guarded by
  `_itemViewer.IsFolderDropDownOpen`, and the `TextBoxSearch_Leave` body.
- The latch field is now both written and read, so it raises no unread-field analyzer diagnostic
  under the warnings-as-errors gate — which is why the P2-T4 seam deliberately deferred it.

Acceptance: satisfied — `EXIT_CODE: 0`.
