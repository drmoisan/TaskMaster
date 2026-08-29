# P3-T5 — Fix A Build (AutoClose toggle threaded through the open lifetime)

Timestamp: 2026-08-28T15-55

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
(run with `/v:m`)

EXIT_CODE: 0

Output Summary:

- Build succeeded with 0 error lines.
- 5 warning lines, all the pre-existing `System.Reactive.PackagesConfigCheck.targets`
  `packages.config` advisory recorded at baseline. Warning count unchanged from baseline.
- This is the first build gate after the P3-T1/P3-T2 signature change. `ShowPopup` now takes
  `(Point location, bool takeFocus)` and its only caller, `BreadcrumbDropDownOpenLifetime.ShowCurrentSurface`,
  was updated in the same phase, so the solution compiles again.
- Fix A comprises P3-T1 (`DropDown.AutoClose = takeFocus;` before the show delegate),
  P3-T2 (`takeFocus` threaded from `OpenCoreAsync` through `ShowCurrentSurface`),
  P3-T3 (`() => DropDown.AutoClose = true,` first in `FinishClose`'s `CompleteAll`), and
  P3-T4 (the scheduled restore in the already-open `takeFocus` branch of `OpenWithFocusIntentAsync`).

Acceptance: satisfied — `EXIT_CODE: 0`.
