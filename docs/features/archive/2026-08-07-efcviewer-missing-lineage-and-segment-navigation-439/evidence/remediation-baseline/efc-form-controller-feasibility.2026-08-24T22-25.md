Timestamp: 2026-08-24T22-25
Command: Parse `evidence/remediation-baseline/issue-439-remediation-baseline.normalized.cobertura.xml` for the merged `QuickFiler/Controllers/EfcFormController.cs` class; map each line sequence point to EfcFormController.cs method regions; inspect UI/COM boundary references with `rg -n 'System\.Windows\.Forms|EfcViewer|FormViewer|Control|Invoke|BeginInvoke|Show|Outlook|File\.|Directory\.|Process\.|WebView|Application\.' QuickFiler/Controllers/EfcFormController.cs`.
EXIT_CODE: 0
Output Summary: The controller has 721 instrumented sequence points, of which 81 are covered and 640 are uncovered. Reaching 80% requires 577 covered points. The only relevant existing headless binding seam and the limited candidate headless action factories account for at most 95 additional uncovered points, projecting 176/721 = 24.410541%. Reaching 80% after that would require extracting at least 501 currently instrumented points while retaining forwarding adapters, which is not a cohesive Issue #439 change.

Sequence-point inventory:
- Initialization and setup, lines 80-285: 126 total; 4 covered; 122 uncovered. Includes Initialize, InitializeWithoutData, InitializeDataFields, CaptureConfigureItemViewer, Cleanup, ConfigureFind, ResolveControlGroups, SetupThemes, and LoadTheme. These use EfcViewer controls, TableLayoutPanel, themes, or Outlook globals.
- Events and action/navigation, lines 347-831: 352 total; 0 covered; 352 uncovered. Includes key-action registration, WinForms event wiring, button handlers, message boxes, folder operations, keyboard actions, and navigation. These depend on controls, EfcHomeController/EfcItemController, and Outlook or UI operations.
- Breadcrumb boundary, lines 832-902: 53 total; 8 covered; 45 uncovered. ConfigureBreadcrumbControl and InitializeBreadcrumbHostAsync construct WebView2/Outlook boundary objects; BindFolderRows reads EfcViewer state; the preserved BindBreadcrumbRowsAsync seam has 8 uncovered deterministic error/cancellation points.
- Window, toggle, and settings behavior, lines 903-1082: 126 total; 5 covered; 121 uncovered. Includes window state, menu display, controls, keyboard navigation, UI-thread Invoke/BeginInvoke, user settings, combobox population, and layout resizing.

Existing headless-testable seam:
- BindBreadcrumbRowsAsync, lines 884-902: 16 sequence points; 8 already covered; 8 remaining points are testable through the existing strict mock seams for IApplicationGlobals, IOlObjects, IFolderHierarchyProvider, and IBreadcrumbWebHost.

Candidate narrow seam inventory:
- GetAsyncCharacterActions, lines 570-626: 32 uncovered action-factory points. Testing its produced actions without exercising the EfcViewer-backed delegates would not validate their UI behavior and is unrelated to Issue #439.
- GetKbdActions, lines 627-676: 48 uncovered action-factory points with the same UI/controller coupling.
- ToggleTipsAsync, lines 989-1006: 7 uncovered points over IQfcTipsDetails, but it is unrelated EfcViewer presentation behavior.
- Combined candidate total: 103 sequence points, 8 already covered, 95 additional points potentially executable only by expanding unrelated controller coverage.

Coverage calculation:
- Current: 81/721 = 11.234397%.
- Minimum for 80%: 577/721; additional current-file coverage required: 496 points.
- Maximum narrow-seam projection: (81 + 95)/721 = 176/721 = 24.410541%.
- To reach 80% after the narrow-seam projection, the remaining instrumented controller denominator would have to be at most floor(176/0.80) = 220 points. This requires moving at least 721 - 220 = 501 points, or 78.281250% of the 640 currently uncovered points, out of EfcFormController.cs.

Required-but-rejected collaborator ownership:
- EfcFormInitializationAndLayoutController: lines 80-285; EfcViewer, TableLayoutPanel, theme, and Outlook setup.
- EfcFormCommandsAndEventsController: lines 347-831; event wiring, keyboard/action dispatch, dialogs, folders, and item-controller calls.
- EfcFormPresentationStateController: lines 903-1082; window, menu, controls, UI invocation, settings, combobox, and layout behavior.
- EfcBreadcrumbBoundaryController: lines 832-902; WebView2/Outlook router composition and the existing internal binding seam.

These four groups are the minimum ownership split implied by the sequence-point inventory. Extracting them would refactor unrelated EfcViewer lifecycle, command, keyboard, settings, layout, and UI-thread behavior; it would not be a narrow Issue #439 remediation and would require exercising or reshaping prohibited WinForms, WebView2, Outlook COM, and filesystem-facing boundaries. Retaining public forwarding adapters would also introduce untested UI-bound forwarding points, increasing the required extraction further.

Headless-test design considered: strict Moq/fake collaborators for the existing BindBreadcrumbRowsAsync seam and pure breadcrumb router collaborators. This design remains valid for Issue #439, but it cannot cover or safely extract the required 501 controller points within the stated scope.

REMEDIATION_REQUIRED: EFC_FORM_CONTROLLER_HEADLESS_80_PERCENT_INFEASIBLE
