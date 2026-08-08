---
name: winformspumphost-tests-load-flaky
description: QuickFiler.Test WinFormsPumpHost tests fail nondeterministically when the machine is CPU-saturated; not a red baseline
metadata:
  type: project
---

`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`-based tests (notably `QfcItemController_InitializationTests.Initialize*_ThroughThePumpHost_*`) fail nondeterministically with either `InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the window handle has been created` or a 60s `[Timeout]` expiry. Observed 2026-08-08 at HEAD 904b4c38 with a byte-clean source tree.

**Why:** The pump host creates a real WinForms control and drives a message loop. With MSTest `Workers: 0` (24 on this box, `scripts/vscode/TaskMaster.cli.runsettings`) plus `dotnet-coverage` instrumentation, handle creation can lose the race when the machine is CPU-saturated. Measured across five full-suite runs on a box at ~96% CPU (a `node` process at 207k CPU-seconds, several VS Code Insiders windows, a second `claude` session, Docker): run 1 hung outright, run 2 = 1 failure in 40.7s, run 3 = 7 timeout failures in 5.9min, runs 4-5 = the same 2 failures in ~40s. Three consecutive isolated runs of the class failed 2/9; a fourth, minutes later, passed 9/9.

**How to apply:** Do not classify these as a pre-existing red baseline on the first red run. Check machine load (`Get-CimInstance Win32_Processor | Measure-Object LoadPercentage -Average`) and re-run when it drops. A `FullyQualifiedName~QfcItemController` scoped gate includes this class, so a plan gate demanding EXIT 0 on that filter inherits the flakiness — narrow the filter or re-run. Related: [[project_utilitiescs_test_parallelism_flakiness]], [[project_uithread_dispatcher_static_swap_race]], [[project_configcontroller_sta_pump_deadlock]], and the GUI-seam rule in [[tests-must-mock-gui-no-visible-window]].
