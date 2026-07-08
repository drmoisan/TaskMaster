# AC13 Developer-Only Live-Outlook Integration Harness

Timestamp: 2026-06-22T00-00

Placement: A single `[TestMethod]` `[TestCategory("LiveOutlook")]` within `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`, hosted in the existing `TaskMaster.Test` project. Rationale: the project already references `Microsoft.Office.Interop.Outlook` (Version 15.0.0.0), so no new interop reference and no new test csproj are required; the live harness lives as a category within the existing test project. The harness pumps the STA via WinForms `Application.DoEvents()` (System.Windows.Forms is already referenced) rather than a WPF `Dispatcher`, deliberately avoiding a new `WindowsBase` assembly reference that is outside the fix's scope lock. The production poll uses a `System.Windows.Threading.DispatcherTimer` in `AppEvents.Hook()`; the harness verifies the same `HookReadinessCoordinator` + `OutlookReadinessGate` decision path that the timer drives.

RunCommand: `vstest.console.exe <TaskMaster.Test assembly path> /TestCaseFilter:"TestCategory=LiveOutlook"`

CIExclusion: The standard QC/CI run uses `/TestCaseFilter:"TestCategory!=LiveOutlook"`, so the `LiveOutlook` category is excluded from the gated CI run and from the coverage denominator. The harness never gates the build. It is a smoke/integration check, not a deadlock reproduction.

STAWiring: A dedicated `System.Threading.Thread` set to `ApartmentState.STA` constructs a live `Microsoft.Office.Interop.Outlook.Application`, wraps it in `OutlookReadinessGate`, and drives a `HookReadinessCoordinator` with a real hookup callback (reads `app.Session.DefaultStore.GetDefaultFolder(olFolderInbox)` once on ready) through an STA-pumped poll loop (`Application.DoEvents()` + `Thread.Yield()` between ticks). It logs `readinessWaitMs`, `hookupLatencyMs`, and `maxTickBlockMs` via `Stopwatch`, and asserts (a) the coordinator reaches `Completed` and (b) `maxTickBlockMs <= 2000` (no single tick blocks the STA beyond the threshold). This is a warm-Outlook smoke check performed by a developer, not a deadlock reproduction and not a CI unit test.
