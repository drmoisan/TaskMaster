# P6-T19 — #440 decision-D2 boundary preservation

Timestamp: 2026-08-26T10-28

Command:

```
pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~Boundary_EfcLeftAtRootAndRightOnChildlessNode_RemainSilentNoOps|FullyQualifiedName~Boundary_QfcUnhandledArrow_StillReachesBreadcrumbArrowFallThrough" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t19"; "EXIT_CODE: $LASTEXITCODE"'
```

EXIT_CODE: 0

TRX: `<repo-root>/docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p6-t19/results.trx`

Output Summary:

- TRX `<Counters total="2" executed="2" passed="2" failed="0" ... />`. Test Run Successful.
- `Boundary_EfcLeftAtRootAndRightOnChildlessNode_RemainSilentNoOps` — Passed (310 ms). Pins the Efc boundaries: Right on a childless ACTIVE node issues no `GetImmediateSubfoldersAsync` call and posts nothing (the `ExpandLeafAsync` early return now tests the active segment rather than the leaf), and after four Left presses walk the active node to the root and collapse the chain to the root, a fifth Left is refused and posts nothing.
- `Boundary_QfcUnhandledArrow_StillReachesBreadcrumbArrowFallThrough` — Passed (820 ms). Drives `QfcItemController.OnBreadcrumbUnhandledArrow` through `HarnessController` and `QfcItemControllerTestSupport.SetField`, and verifies the call site at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:190` reaches `IQfcKeyboardHandler.BreadcrumbArrowFallThrough` exactly once. Only the interface is mocked, so the modal `MyBox.ShowDialog` in the concrete `KeyboardHandler` is never constructed and cannot block.

Satisfies AC-23 and AC-24.
