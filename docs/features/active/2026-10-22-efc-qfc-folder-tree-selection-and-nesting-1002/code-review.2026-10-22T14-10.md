# Code Review: EFC and QFC folder tree selection and nesting (Issue #1002)

- Branch: `bug/efc-qfc-folder-tree-selection-and-nesting-1002`
- Head: `54b36112abf0a4ac26384226694b02520842061b`
- Base: `main` at merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7`
- Review timestamp: 2026-10-22T14-10 (re-audit after remediation cycle 2)
- Scope: full branch diff (12 production C# files, 1 JavaScript asset, 9 C# test files, 1 node suite, 4 project files, feature documents and evidence); the cycle-2 commits were read in full against `9ccf91971`.

## Executive Summary

Cycle 2 resolves the single cycle-1 blocking finding R-1 and the three recommended in-scope items. The AC23 amendment changes only the named clause and records its authority in a dated `## Planner Amendments` entry. The fault exit for the delivered waits is wired once in the shared test helper, so every caller gains it, and the new proof test observes the fault synchronously with no timer. The `subfolderResult` handler now places the display name in a `span.name`, which brings the transient render under the cycle-1 truncation rule; because every bridge click branch resolves its target with `closest(...)`, wrapping the text does not change click routing. The renderer's key-less branch was left unedited under the recorded AC9 ruling, which is consistent with AC9's "key-less children render exactly as before" clause.

Blocking findings: 0. Non-blocking findings: 2 new (N-6, N-7) plus five carried (N-2, N-4, CR-2, CR-3, CR-5).

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | `CreateIssue1002Controller`, `BoundaryErrorSink` assignment | N-6: the sink calls `delivered?.TrySetException(exception)`; if a future caller reported a fault with a null exception, `TrySetException` would throw `ArgumentNullException` inside the sink. The production `TryReportBoundaryFault` wrapper invokes the sink defensively, and every current report passes a non-null exception, so no test is affected today. | Optional: guard with `exception ?? new InvalidOperationException(message)`. | Robustness of a test helper only. | `EfcFormController.cs` `BoundaryErrorSink` and its defensive invoker `TryReportBoundaryFault`; cycle-2 diff of the helper. |
| Info | `QuickFiler/Controllers/EfcFormController.Actions.cs` | comment above `ActionCancelAsync` | N-7: the reworded exemption comment is a single 140-character line, longer than the sibling comments. CSharpier does not wrap comments and the format check passes. | Optional: wrap to match the neighbouring comment width. | Readability only. | Cycle-2 diff; `cycle2-format-check.2026-10-22T14-06.md`. |
| Info | `QuickFiler/Controllers/EfcFormController.Actions.cs`, `.Breadcrumb.cs` | five attributed members | N-2 (carried): net48 `ExcludeFromCodeCoverage` has no `Justification` property, so a comment is used. | None. | Verified by a compile probe (CS0246). | `evidence/other/exclude-attribute-form.2026-10-22T13-25.md`. |
| Info | `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | `InvokeIssue1002Bind` | N-4 (carried): `BindFolderRows` and `BindSourceFolderRows` are invoked by name through reflection; a rename fails at the `member.Should().NotBeNull(...)` guard rather than at compile time. | Optional: make the two members `internal` and call them directly. | The guard message makes a rename failure clear. | `InvokeIssue1002Bind` helper. |
| Minor | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | `AppendDescendants` | CR-2 (carried): a node shows the minus affordance over an empty container while its fetch is in flight. | No change required in this item. | Transient visual state only. | Cycle-0 code review. |
| Minor | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | `AppendDescendants` | CR-3 (carried): nested keyed nodes carry `data-child-index` relative to their own level; safe under the current click-branch order. | Keep the AC10 ordering test. | Future reorder risk only. | Cycle-0 code review. |
| Info | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | `HandleDescendantToggleAsync` | CR-5 (carried): expected races are logged at `Error`, as the spec requires. | Consider a lower level in a later change. | Log noise only. | Cycle-0 code review. |

## Cycle-1 Finding Resolution

| Cycle-1 finding | Status | Evidence |
|---|---|---|
| R-1 AC23 text contradicts the ratified exemption | Resolved | `spec.md` AC23 now reads "no coverage exclusion is added other than the five member-level `[ExcludeFromCodeCoverage]` attributes on `ActionOkAsync`, `ActionCancelAsync`, `CreateFolderAsync`, `RefreshSuggestionsAsync` and `ConfigureBreadcrumbControl` ratified by the maintainer ...; no `coverage.config` entry and no class-level exclusion is added"; the spec diff in cycle 2 is exactly that clause plus the `## Planner Amendments` section; reviewer count of added attributes is 5; `coverage.config` unchanged. |
| N-1 inaccurate exemption comment | Resolved | Comment names the concrete `EfcViewer` `UiSyncContext` awaiter and `Close`; attribute and body unchanged. |
| N-3 delivered waits had no failure exit | Resolved | `CreateIssue1002Controller` sets `BoundaryErrorSink` to complete the delivered source with the fault; `BindFolderRows_FaultBeforeDelivery_FaultsDeliveredTaskPromptly` asserts `IsFaulted` synchronously; fail-before projection shows an assertion failure, not a hang. `ActionDeleteAsync_BindFault_IsReportedAtBindBoundary` installs its own sink after creation and is unaffected. |
| N-5 bare `.child` text | Resolved as narrowed | `subfolderResult` handler wraps the display name in `span.name`; `BridgeJs_SubfolderResultHandler_WrapsChildNameInNameSpan` pins the three statements and the absence of `div.textContent`. Renderer key-less branch untouched per the AC9 ruling. |

## Positive Observations

- The fault exit is placed in the shared helper, so the three existing waits gain it without edits to their bodies.
- The new fault test exploits the synchronous fault path (`_globals` unset faults before the first await), which removes any need for a timeout or wall-clock wait.
- The `subfolderResult` text-contract test anchors on the branch marker with `StringComparison.Ordinal` and asserts the negative (`div.textContent` absent), so a regression to bare text fails.
- The scope-fence and R-1 verification evidence counts the attributes and the `coverage.config` stat on the final tree, not only before the edit.

## Verdict

**PASS.** All blocking findings from cycles 0 and 1 are resolved, and the cycle-2 changes introduce no blocking defect. The remaining items are optional robustness and readability notes.
