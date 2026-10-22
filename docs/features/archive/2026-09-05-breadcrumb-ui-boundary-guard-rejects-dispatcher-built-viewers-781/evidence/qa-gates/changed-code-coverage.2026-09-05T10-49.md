# Changed-Code Coverage Determination (issue #781)

Timestamp: 2026-09-05T17-10

Task: [P2-T7]

Command: `pwsh -NoProfile -Command` over the [P0-T9] post-processing block with
`.\coverage\final-781.cobertura.xml` substituted for the baseline path. That block dot-sources
`.\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1`, rewrites the document in place with
`ConvertTo-KoverageCoberturaXml`, prints the six root attribute values, and runs the class-count
query `//class[contains(@filename,'ItemViewer.Breadcrumb.cs')]` against the processed document.

EXIT_CODE: 0

## Output Summary

Six root Cobertura attribute values of the post-processed final document:

- `line-rate` = 0.848316
- `branch-rate` = 0.791421
- `lines-covered` = 54920
- `lines-valid` = 64740
- `branches-covered` = 13174
- `branches-valid` = 16646

Observed `ItemViewerBreadcrumbClassCount` = **0**

### CHANGED-CODE COVERAGE: NOT MEASURABLE

The determination is selected by the observed class count and by nothing else. The count is `0`,
so this is the first of the two branches the task defines.

`QuickFiler/Viewers/ItemViewer.cs` line 20 carries `[ExcludeFromCodeCoverage]` on the
`ItemViewer` partial class declaration. The attribute applies to the whole type, including the
members declared in the other part of that type, `ItemViewer.Breadcrumb.cs`. The collector
therefore emits no `<class>` element whose `filename` ends `ItemViewer.Breadcrumb.cs`, which the
query confirms empirically on this feature's own final run rather than by inference. The changed
production lines are consequently outside the coverage denominator, and **no percentage exists to
compare against 90 percent**. The same query returned `0` on the baseline run recorded in
`FEATURE/evidence/baseline/mstest-coverage.2026-09-05T10-49.md`, so the property held before and
after this change and was not introduced by it.

The class-level exemption is the ratified WinForms UserControl exemption under the CLAUDE.md UT2
COM/VSTO/WinForms clause. Removing it is out of scope for this issue, as the orchestrator's
version 1.1 decision 3 records.

### Substitute behavioral evidence

Every outcome of both conditionals in the rewritten `ThrowIfOffUiBoundary` is exercised by a named
test recorded Passed in
`FEATURE/evidence/regression-testing/regression-pass-after.2026-09-05T10-49.md`:

| Branch outcome | Covering test(s) |
| --- | --- |
| Null owner: `UiDispatcher` is null, method returns without effect | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` |
| Non-null owner, `CheckAccess()` true: method returns without throwing | `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow`, `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow`, `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext`, `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` |
| Non-null owner, `CheckAccess()` false: method throws `InvalidOperationException` | `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic`, `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` |

All seven are recorded Passed, and five of them were recorded Failed against the unfixed guard in
`FEATURE/evidence/regression-testing/regression-fail-before.2026-09-05T10-49.md`, so they
discriminate rather than merely corroborate.

`ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` is deliberately **not**
cited as null-owner evidence. It nulls `_context`, not `_uiDispatcher`, so after this change the
viewer still has an owning dispatcher and that test exercises the `CheckAccess()`-true path
instead.

Acceptance for the task as a whole: this artifact exists, records the observed class count of 0,
and carries exactly one of the two determination lines, namely
`CHANGED-CODE COVERAGE: NOT MEASURABLE`.
