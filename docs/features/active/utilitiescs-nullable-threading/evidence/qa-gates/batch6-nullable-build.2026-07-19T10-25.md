# Batch 6 — Pragma-Only Nullable Build Verification

- Timestamp: 2026-07-19T10-25
- Task: [P6-T6]
- Literal plan command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
- Executed equivalent (genuine recompile of the changed project): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true /m`
- EXIT_CODE: 1 (pre-existing first-party TWAE noise only; zero CS86xx)

## Opted-in Batch 6 files (4)

- UtilitiesCS/Threading/ProgressPackage.cs
- UtilitiesCS/Threading/ProgressTracker.cs
- UtilitiesCS/Threading/ProgressTrackerAsync.cs
- UtilitiesCS/Threading/ProgressTrackerPane.cs

## Output Summary

- **CS86xx for the 4 opted-in Batch 6 files: 0.** CS86xx count anywhere: 0.
- ProgressPackage: optional reference params annotated `T? = null`; mutually-exclusive `_progressTracker`/`_progressTrackerPane` fields plus `_cancelSource`/`_stopWatch` and their public properties annotated `?`; the four tuple return shapes (`ToTuple`/`ToTuplePane`/`CreateAsTupleAsync`/`CreateAsTuplePaneAsync`) made nullable-element-consistent. `SpawnChild` `?.` behavior unchanged; the shared `IProgress<(int Value, string JobName)>` tuple contract kept consistent.
- ProgressTracker: `_cancelSource`/`_screen` -> `?`; `_jobName`/`_uiDispatcher`/`_progressViewer` -> `= null!` (init-order invariants, keeps the non-null JobName tuple/`ProgressViewer` contract); ctor-2 `screen` param -> `Screen?`. One annotation-induced justified `!` at `_progressViewer.UiDispatcher!.InvokeAsync(...)` in `ReportAsync` (root viewer's UiDispatcher is set in `Initialize()`); the `Report`/`ReportAsync` close-on-100 logic is byte-unchanged.
- ProgressTrackerAsync: mirror of ProgressTracker (`_cancelSource`/`_screen` -> `?`; `_jobName`/`_uiDispatcher`/`_progressViewer` -> `= null!`; ctor-2 `screen` -> `Screen?`).
- ProgressTrackerPane: `_progressViewer` -> `ProgressPane?` (its `SafeAction` guard null-checks it — kept byte-unchanged) and property `ProgressViewer` -> `ProgressPane?`; justified `!` at the ctor/`ChangeBarColor` derefs that run after the synchronous `Dispatcher.Invoke` assignment or under the `SafeAction` guard; `_jobName` -> `= null!`. The `IAppAutoFileObjects.ProgressTracker`-returns-`ProgressTrackerPane` cross-module contract is behavior-compatible.
- First read surfaced 1 residual CS8602 (ProgressTracker line 202, `_progressViewer.UiDispatcher` — nullable since Batch 5) resolved with the justified `!` above; re-read is clean.
- Non-zero EXIT_CODE is the pre-existing first-party TWAE noise only (CS0618 x14 + CS0168 x2, unchanged from baseline). No new diagnostics elsewhere; vendored skipped. `/p:Nullable=enable` NOT passed.
