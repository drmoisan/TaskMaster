# P9-T60 independent nonnumeric-adapter remediation accounting

Timestamp: 2026-07-27T07-28
Command: Read-only `git merge-base HEAD origin/main`; `git show 314358197`; `git diff --quiet`; `git diff --unified=0`; `git status --short`; `Get-FileHash`; `Get-Content`; and `Select-String` inspection of P5-T104, P9-T57 through P9-T59 evidence, live source, source ranges, and deterministic test seams.
EXIT_CODE: 0

## Scope and source-current coverage evidence

Reviewed HEAD: `47dcc98a4991467187adadcb39e99a4c53c2ca58`.

Live merge base against `origin/main`: `e63ddc7c18ca71e2c968b3329e42d965d45af1eb`.

The P9-T58 source-current hashes exactly match the live committed reviewed sources:

| Source | SHA-256 | Lines |
| --- | --- | ---: |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | `32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31` | 481 |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `19811E252ED35AAA0292AB3942DDD02E7F2C5620066B81C256AA97A5F4F2F9DA` | 298 |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `1728E0A62E4B2B4775F20BD5460C5F365AFF8B097ED0AF6169F222A07ED86746` | 494 |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | `8EB6AB9FBA022EF16EF7D1A4FC00FB137F91170ADE37458DDB0D3D560659D3C3` | 327 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | `3EE05089236DEE9CA591ED1282FC6EE3F14D694B2CF82C7E566D1C4CE167237A` | 302 |

The completed P9-T57 evidence records the required workspace-relative `CoverageOutput`, terminal exit `0`, `6,075 / 6,075` passed, zero failed/skipped, no residual test or coverage process, and Cobertura SHA-256 `89DB6AC8BA9974515AF7D07A07B13F6BEAA08854DA645382005189F77971034C`. P9-T59 derives coverage only by exact Cobertura filename and source range: repository coverage is `92,380 / 109,252 = 84.5568%`; `BreadcrumbItemViewerLifecycleCoordinator.cs:29-479` is `288 / 318 = 90.5660%`; and `BreadcrumbPopupUiOperations.cs:53-490` is `234 / 258 = 90.6977%`.

The sole live unstaged path is `TaskMaster/TaskMaster.csproj`; it is an unrelated ApplicationVersion change and is outside this accounting review. It does not affect the reviewed source, coverage, exclusion, filter, threshold, runsettings, or configuration inputs.

## Origin/main and P5-T104 provenance

P5-T104's branch-local baseline is commit `314358197c4c309fc76af38de305bb2200ff8e82`. Its exclusion inventory was seven `BreadcrumbPopupUiOperations` method attributes, two `ItemViewer.Breadcrumb` method attributes, and the type attribute in `ItemViewer.cs`.

The provenance comparison is origin/main-accurate:

| Surface | `origin/main` | P5-T104 / `314358197` | Current HEAD | Result |
| --- | ---: | ---: | ---: | --- |
| `BreadcrumbPopupUiOperations.cs` exclusion attributes | 0; the helper source did not exist on `origin/main` | 7 | 7 | PASS: branch-local P5 set retained only through allowed narrowing/rebinding; not misrepresented as pre-branch exclusions. |
| `ItemViewer.Breadcrumb.cs` method exclusion attributes | 1 | 2 | 0 | PASS: both P5-T104 method exclusions were removed; no method exclusion remains. |
| `ItemViewer.cs` type exclusion at line 20 | 1 | 1 | 1 | PASS: pre-existing `origin/main` type attribute retained; no class-level attribute was added. |

`git diff --quiet 314358197 HEAD` and `git diff --quiet origin/main HEAD` both return exit `0` for `coverage.config`, `scripts/vscode/TaskMaster.cli.runsettings`, and `.csharpierignore`. The canonical coverage configuration SHA-256 remains `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`. Therefore no configuration, filter, threshold, or coverage-policy exclusion was added or widened.

## Exact current exclusion ranges and narrowing result

| Current direct adapter range | P5-T104 range / predecessor | Accounting conclusion |
| --- | --- | --- |
| `BreadcrumbPopupUiOperations.cs:105-110`, `ShowOwnedPopup` | `97-102`, same direct `ToolStripDropDown.Show` expression | PASS: unchanged semantic adapter; line movement only. |
| `BreadcrumbPopupUiOperations.cs:380-381`, `CreateProductionControl` | `377-378`, same `new WebView2` expression | PASS: unchanged direct SDK construction adapter. |
| `BreadcrumbPopupUiOperations.cs:383-388`, `BeginProductionInitialization` | `380-385`, same WebView2 initialization call | PASS: unchanged direct SDK initialization adapter. |
| `BreadcrumbPopupUiOperations.cs:390-392`, `ReadProductionCore` | `387-392` | PASS: narrowed by removing the null/error body from the exclusion; the null contract is now unexcluded and covered. |
| `BreadcrumbPopupUiOperations.cs:394-410`, `BeginProductionNavigation` | `394-419` | PASS: narrowed to direct WebView2 navigation and messenger construction; readiness and cleanup behavior is unexcluded in `BreadcrumbPopupLifecycleOperations`. |
| `BreadcrumbPopupUiOperations.cs:412-417`, `DisposeProductionSurface` | `421-423` | PASS: only the direct messenger/control disposal delegates remain at the native boundary; primary-error preservation and all-resource cleanup are unexcluded in `DisposeTwoResources`. |
| `BreadcrumbPopupUiOperations.cs:457-492`, `BindProductionNavigation` | `431-478`, `NavigateToDocument` | PASS: replacement is narrower. Dispatcher/core/owner validation, readiness construction, event translation, and cleanup are unexcluded in `NavigateToDocument`, `NavigateToDocumentCore`, and `BreadcrumbPopupLifecycleOperations.NavigateWithSubscription`; the excluded range only attaches/detaches native WebView2 and WinForms events. |
| `QuickFiler/Viewers/ItemViewer.cs:20`, `ItemViewer` type attribute | `ItemViewer.cs:20`, pre-existing at P5-T104 and `origin/main` | PASS: no new or widened class exclusion. The remaining partial contains native compatibility wrappers only; host-neutral lifecycle state is in the unexcluded coordinator. |

The P5-T104 `ItemViewer.Breadcrumb` method exclusions are absent at HEAD. The direct ItemViewer boundaries covered by the retained pre-existing type exclusion are limited to `ItemViewer.Breadcrumb.cs:82-97` (CoreWebView2 retrieval, WebView2 messenger creation, and `NavigateToString`), `155-176` (native WebView control/host construction and rectangle/screen providers), and `211-220` (native focus). The host-neutral lifecycle, messenger ownership, configuration state, reset/disposal, subscriptions, selector state, and cleanup are implemented in unexcluded `BreadcrumbItemViewerLifecycleCoordinator.cs:62-327` and its unexcluded lifecycle helpers.

## Deterministic production-seam mapping

| Direct adapter boundary | Production wiring | Deterministic seam and evidence |
| --- | --- | --- |
| Popup show | `BreadcrumbDropDownHost` defaults `_showPopup` to `BreadcrumbPopupUiOperations.ShowOwnedPopup`. | `BreadcrumbDropDownCoverageThresholdTests` and `BreadcrumbDropDownLifecycleCoverageTests` inject and assert the host show delegate, preserving placement and ownership behavior without a native handle. |
| Popup control creation, initialization, core access, navigation, and disposal | The production constructor at `BreadcrumbPopupUiOperations.cs:52-60` wires `CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore`, `BeginProductionNavigation`, and `DisposeProductionSurface`. | The explicit constructor at `62-78` accepts primitive control, initialization, core, navigation, and disposal delegates. `BreadcrumbPopupUiOperationsDirectAdapterTests` deterministically verifies absent/present core handling, initializer failure/null-task handling, messenger construction cleanup, navigation subscription translation, and two-resource failure ordering. |
| Popup native navigation event binding | `NavigateToDocument` calls `NavigateToDocumentCore` with `BindProductionNavigation`. | `NavigateToDocumentCore_InjectedBinderReturnsReadiness` injects the `NavigationBinder` and deterministically verifies invocation, readiness completion, and detachment without COM/native event delivery. |
| ItemViewer native WebView, navigation, geometry, and focus calls | `CreateCollapsedBreadcrumbCandidate`, the native `ConfigureBreadcrumbDropDown` wrapper, and `FocusBreadcrumbCore` supply native delegates to the lifecycle coordinator. | `ItemViewerBreadcrumbDropDownContractTests` and `BreadcrumbDropDownIntegrationTests` supply candidate, host, geometry, working-area, and focus seams to verify the coordinator-facing production contract without live WebView2 or WinForms execution. |

The mappings exercise production-owned seam boundaries rather than relying on ambient synchronization context or a coverage/configuration waiver. No excluded host-neutral configuration or lifecycle body remains: the excluded popup ranges contain only direct SDK calls or event attachment/detachment, and ItemViewer delegates lifecycle state to the unexcluded coordinator.

## Severity summary

| Severity | Count |
| --- | ---: |
| Blocker | 0 |
| Major | 0 |
| Medium | 0 |
| Low | 0 |

Output Summary: PASS. P9-T57 relative-output coverage and P9-T59 source-range accounting are source-current; each exclusion has origin/main and P5-T104 provenance, exact inclusive ranges, no post-P5 addition or widening, no excluded host-neutral/configuration/lifecycle body, and a deterministic production-seam mapping.

P9_T7_AUDIT: PASS
