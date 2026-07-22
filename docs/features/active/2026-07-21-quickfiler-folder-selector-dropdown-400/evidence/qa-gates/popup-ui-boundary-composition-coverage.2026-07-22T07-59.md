# Popup UI-boundary composition coverage gate - nonpassing diagnostic

Timestamp suffix: `2026-07-22T07-59`

Status: **FAIL**. This run did not complete and the resulting Cobertura values are provisional diagnostics only. P5-T67 remains unchecked.

## Exact command

dotnet-coverage version: `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`

```powershell
$coverageArgs = @(
    'collect',
    '--output',
    'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\qa-gates\coverage-popup-ui-boundary-composition.2026-07-22T07-59.cobertura.xml',
    '--output-format',
    'cobertura',
    '--settings',
    'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\coverage.config',
    '--',
    'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe',
    'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll',
    '/Settings:C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\scripts\vscode\TaskMaster.cli.runsettings',
    '/InIsolation',
    '/TestCaseFilter:FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests'
)
& dotnet-coverage @coverageArgs
```

Only `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` was supplied as a test assembly. The repository coverage wrapper, other test assemblies, unfinished P6/P7 filters, and full-repository coverage were not run.

`coverage.config` SHA-256 before and after: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`. The configuration was unchanged.

## Noncompletion and progress boundary

The filtered process tree stopped making progress and remained active until the command timed out at `124.2` seconds. The verified workspace-owned processes were:

- `dotnet-coverage.exe` PID `23500`
- child `vstest.console.exe` PID `74404`

PID `74404` was terminated only after its command line was verified to contain the current worktree, exact QuickFiler test assembly, and exact nine-class filter. PID `23500` exited after its child ended. No workspace-owned coverage, VSTest, or testhost process remained afterward.

The runsettings enable class-level parallelism, so the partial artifact cannot establish a serial global last-passed console line. Within the blocking class, `LaterNavigation_InvalidatesEarlierGenerationAndPublishesOnlyCurrentMessenger` was the fifth and last completed `BreadcrumbCollapsedSurfaceReadinessTests` case. `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce` then reached source line 215 and stalled awaiting `first`; source lines 216 through 231 were unhit. The next four cases in that class were unstarted. All other selected classes show their final test method executed, which provisionally indicates 65 of 70 cases completed and the 66th was active. This is an inference from partial instrumentation, not an authoritative VSTest result. Therefore zero failures/skips and complete nine-class discovery cannot be accepted for P5-T67, even though the preceding non-coverage P5-T66 run passed 70/70.

dotnet-coverage emitted a valid but partial Cobertura file only as the terminated process unwound:

- XML size: `16,977,332` bytes
- XML SHA-256: `F3236B98224F4D28027AAC67BA28BA5BC12E4536D2FF12DD138A2CE334ABD64D`
- XML root line rate: `6.5697%` across all loaded instrumented first-party modules; this is not the focused P5 acceptance value.

## Provisional P5 source coverage

The following values aggregate unique source lines across the primary type and compiler-generated classes for each file. They are provisional because the run did not complete. Each measured required source is below the 90% gate, and `ItemViewer.Breadcrumb.cs` is absent.

| Required P5 source | Covered/valid unique lines | Provisional rate | Result |
| --- | ---: | ---: | --- |
| `BreadcrumbUiDispatcher.cs` | 166/185 | 89.73% | FAIL |
| `BreadcrumbWebViewSurfaceFactory.cs` | 117/156 | 75.00% | FAIL |
| `BreadcrumbPopupUiOperations.cs` | 216/244 | 88.52% | FAIL |
| `BreadcrumbDropDownOpenLifetime.cs` | 249/302 | 82.45% | FAIL |
| `BreadcrumbDropDownHost.cs` | 215/280 | 76.79% | FAIL |
| `ItemViewer.Breadcrumb.cs` | no Cobertura class or line entry | unavailable | FAIL |

Required below-threshold type/member/state-machine examples include:

- `BreadcrumbUiDispatcher` primary type: 89.44%; `Dispatch(Action)`: 80.43%; dispatch callback class: 80.95%.
- `BreadcrumbWebViewSurfaceFactory` primary type: 56.00%; `CreateSurfaceAsync` state machine: 80.00%; `BreadcrumbNavigationReadiness`: 78.12%.
- `BreadcrumbPopupUiOperations` source union: 88.52%; `NormalizeFactory` state machine: 66.67%. The primary type alone is 94.74%, which does not cure the below-threshold generated and source-union entries.
- `BreadcrumbDropDownOpenLifetime` primary type: 78.86%; `CompleteOpenAsync` state machine: 85.71%; `EnsureSurfaceAsync` state machine: 65.12%.
- `BreadcrumbDropDownHost` primary type: 70.78%; `OpenAsync`: 66.67%; `Close`: 83.33%.

`ItemViewer` is absent because its primary partial declaration in `ItemViewer.cs` has a class-level `ExcludeFromCodeCoverage` attribute. This omits both bounded direct adapters and changed host-neutral breadcrumb members. The omission is rejected rather than treated as unavailable or passing.

## Bounded direct adapters recorded separately

The production sources identify these direct WebView2/WinForms adapters separately from host-neutral behavior:

- `BreadcrumbPopupUiOperations.ShowOwnedPopup`
- `BreadcrumbPopupUiOperations.CreateProductionControl`
- `BreadcrumbPopupUiOperations.BeginProductionInitialization`
- `BreadcrumbPopupUiOperations.ReadProductionCore`
- `BreadcrumbPopupUiOperations.BeginProductionNavigation`
- `BreadcrumbPopupUiOperations.DisposeProductionSurface`
- `BreadcrumbPopupUiOperations.NavigateToDocument`
- `BreadcrumbWebViewSurfaceFactory.NavigateToDocument`
- `ItemViewer.AttachBreadcrumbWebViewAsync()` production wrapper
- `ItemViewer.CreateCollapsedBreadcrumbCandidate`

The method-level exclusions on bounded adapters were not changed. They do not justify the absence of changed host-neutral ItemViewer members or any of the numeric shortfalls above.

## Gate conclusion

P5-T67 fails for three independent reasons: the exact coverage run did not complete; the partial artifact has multiple required numeric values below 90%; and changed ItemViewer breadcrumb members are omitted entirely. No threshold, filter, package, project, runsettings, or coverage configuration was weakened. P5-T68 is not eligible to run because its required completed focused-coverage input does not exist. P9-T4 full-repository coverage remains mandatory after P6/P7 and is not satisfied by this diagnostic.
