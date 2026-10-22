# Code Review: EFC and QFC folder tree selection and nesting (Issue #1002)

- Branch: `bug/efc-qfc-folder-tree-selection-and-nesting-1002`
- Head: `b72ac1dbbfdb6b45d01a14dde9e91428a6bf41ac`
- Base: `main` at merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7`
- Review timestamp: 2026-10-22T13-37 (re-audit after remediation cycle 1)
- Scope: full branch diff (12 production C# files, 1 JavaScript asset, 9 C# test files, 1 node suite, 4 project files, feature documents and evidence); the cycle-1 commit `b72ac1dbb` was read in full against `e96e65302`.

## Executive Summary

Cycle 1 resolves all four cycle-0 blocking findings with small, targeted changes. The CSS fix uses an attribute selector, preserving the AC9 decision to mark the selected descendant by `aria-selected` rather than a class; because the renderer emits an expanded node's `.children` container as a sibling after the node element, the highlight covers only the selected node's line. The `.child .name` rule restores ellipsis truncation for keyed descendants. The coverage exemption is applied at member level to exactly the five members the maintainer named, with an explanatory comment on each. The new tests are focused and readable.

Blocking findings: 1 (R-1, autonomous). Non-blocking findings: 5 new (N-1 to N-5) plus three carried from cycle 0 (CR-2, CR-3, CR-5).

R-1 is a requirements-traceability defect, not a code defect: spec AC23 still states "no coverage exclusion is added" and is checked, while the branch now adds five ratified `[ExcludeFromCodeCoverage]` attributes.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md` | `## Acceptance Criteria`, AC23 | R-1: AC23 states "no coverage exclusion is added" and is checked `[x]`, but the branch adds five member-level `[ExcludeFromCodeCoverage]` attributes. The maintainer ratification (B-3 decision, 2026-10-22T10:50Z) lifted the plan's prohibition but was not carried into the spec, so the merged criterion is false as written. | Amend AC23 to state the ratified exception (for example, "no coverage exclusion is added other than the five member-level `[ExcludeFromCodeCoverage]` attributes ratified by the maintainer under CLAUDE.md UT2, recorded in remediation-inputs.2026-10-22T10-39.md") and add a dated planner-amendment note in `spec.md` citing the decision; keep the box checked only after the amendment. | The AC record must agree with the delivered code; the substantive decision already exists, so the change is clerical. Remediability: autonomous. | `spec.md` line 298; `git diff 8e4ad1ecb...HEAD -- '*.cs'` shows 5 added `[ExcludeFromCodeCoverage]` lines; `remediation-inputs.2026-10-22T10-39.md` section "B-3 Maintainer decision". |
| Minor | `QuickFiler/Controllers/EfcFormController.Actions.cs` | comment above `ActionCancelAsync` | N-1: the exemption comment says the handler "closes the live WinForms viewer, whose MessageBox-bound form cannot run headless"; `ActionCancelAsync` calls no `MessageBox`. Its actual dependency is the concrete `EfcViewer` form (`UiSyncContext` awaiter and `Close`). | Reword to name the concrete `EfcViewer` dependency. | Exemption justifications are the reviewable record of why a member is outside the metric; they should be accurate. | `_formViewer` is declared `private EfcViewer _formViewer;` in `EfcFormController.cs`; method body lines 73-80. |
| Minor | `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | three async bind tests (lines 94, 117, 173) | N-3: each test awaits `delivered.Task`, completed only by the host mock's `NavigateToString` callback. `BindFolderRows` discards the bind task, and `BindBreadcrumbRowsAsync` catches every exception, so a future regression that faults before delivery leaves the test waiting indefinitely rather than failing. | Complete the `TaskCompletionSource` from the fault path as well (for example, fail it from a `TryReportBoundaryFault` seam), or assert through `BindBreadcrumbRowsAsync` directly where the bind must be observed. Do not add a wall-clock timeout. | A hung test is harder to diagnose than a failed one; the current behaviour is correct, so this is a robustness item. | Test methods starting at lines 94, 117 and 173; `BindFolderRows` line 175 `_ = BindBreadcrumbRowsAsync(...)`. |
| Minor | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`, `BreadcrumbHtmlRenderer.cs` | `BaseCss` `.child`; bridge `subfolderResult` handler; `AppendChildren` key-less branch | N-5: after CR-1, `.child` no longer carries `white-space: nowrap; overflow: hidden; text-overflow: ellipsis`, so `.child` elements whose text is bare (no `.name` span) wrap instead of truncating. Two emitters produce bare text: the bridge `subfolderResult` handler (transient; replaced by the row render that follows) and the renderer's key-less branch (reached only through `SetLeafChildren`, which no production caller uses; the router calls `SetLeafDescendants`). | Optional: wrap the text in `<span class="name">` in both emitters, or leave as is. | Low user impact: one path is transient and the other is not reached in production. | `evidence/other/child-name-markup-check.2026-10-22T13-28.md`; Grep: the only production caller is `BreadcrumbBridgeRouter.Arrows.cs:201` `SetLeafDescendants`. |
| Info | `QuickFiler/Controllers/EfcFormController.Actions.cs`, `.Breadcrumb.cs` | five attributed members | N-2: the decision asked for a `Justification` on each attribute; the property does not exist on the .NET Framework 4.8.1 attribute, so a comment is used. | None. | Verified by a compile probe (CS0246). | `evidence/other/exclude-attribute-form.2026-10-22T13-25.md`. |
| Info | `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | `InvokeIssue1002Bind` | N-4: `BindFolderRows` and `BindSourceFolderRows` are private and invoked by name through reflection; a rename fails the test at the `member.Should().NotBeNull(...)` guard rather than at compile time. | Optional: make the two members `internal` (the test project already reaches internal members) and call them directly. | Consistent with the file's existing `SetPrivateField` pattern; the guard message makes a rename failure clear. | Lines 246-258. |
| Minor | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | `AppendDescendants` | CR-2 (carried): a node shows the minus affordance over an empty container while its fetch is in flight. | No change required in this item. | Transient visual state only. | Cycle-0 code review. |
| Minor | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | `AppendDescendants` | CR-3 (carried): nested keyed nodes carry `data-child-index` relative to their own level; safe under the current click-branch order. | Keep the AC10 ordering test. | Future reorder risk only. | Cycle-0 code review. |
| Info | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | `HandleDescendantToggleAsync` | CR-5 (carried): expected races are logged at `Error`, as the spec requires. | Consider a lower level in a later change. | Log noise only. | Cycle-0 code review. |

## Cycle-0 Finding Resolution

| Cycle-0 finding | Status | Evidence |
|---|---|---|
| CR-B1 `EfcFormController.Breadcrumb.cs` coverage lowered | Resolved | 8 new tests; file 93/101 lines (92.08%), 34/42 branches (80.95%) against required 84 lines and 24 branches (reviewer recomputation). |
| CR-B2 `BreadcrumbMessages.cs` branch coverage | Resolved | 7 null-guard tests, one per guarded argument, each asserting `ParamName`; 14/14 branches. The `NullTypeOutboundMessage` test subclass is the only route to the protected base constructor and is commented as such. |
| CR-B3 pre-existing residue | Resolved by maintainer-ratified exemption | Attributes on exactly the five named members; no class-level attribute; no `coverage.config` change; `RefreshSuggestionsAsync` admitted only after evidence it stayed at 0 of 9 lines. |
| CR-B4 no descendant highlight | Resolved | `.child[aria-selected="true"]` rules in both theme blocks with the row selection colours; text-contract tests assert both rules beside the row rule. |
| CR-1 ellipsis on flex container | Resolved for keyed descendants | `.child .name` carries the truncation properties; `BaseCss_ChildName_CarriesEllipsisRule` also asserts `.child` no longer carries `text-overflow` and keeps `display: flex`. See N-5 for bare-text emitters. |
| CR-4 null-viewer arm | Resolved | `ApplyDeleteGestureAsync_NullViewer_RetainsTrashRowAndDoesNotBind`; covered branches 12 to 13. |

## Positive Observations

- The production change in cycle 1 is limited to CSS constants, attributes, comments and `using` directives; no behaviour of the delivered fix changed.
- Each null-guard test asserts the exception type and the parameter name, so a guard that throws for the wrong argument fails.
- The CSS contract tests pin the selected-descendant colour beside the row colour in the same assertion chain, which keeps the two in step.
- The RED-first projection for the CSS work fails exactly the three new tests and passes the ten existing ones.

## Verdict

**REMEDIATION_REQUIRED.** All code-level findings from cycle 0 are resolved. R-1 is an autonomous spec amendment that aligns AC23 with the ratified exemption; N-1, N-3 and N-5 are optional in-scope improvements.
