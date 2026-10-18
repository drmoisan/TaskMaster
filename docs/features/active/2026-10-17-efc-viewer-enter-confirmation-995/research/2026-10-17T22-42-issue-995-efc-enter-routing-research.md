<!-- markdownlint-disable-file -->

# Task Research Notes: Issue 995 EFC Viewer Enter confirmation and workflow repair

## Research Executed

### File Analysis

- `.github/agents/task-researcher.agent.md` was read first, followed by execution-environment and evidence conventions skills, repository instructions, general code/test policies, applicable C#/PowerShell policies, and QA skills.
- Current checkpoint receipt `issue-995-research-2` selects `task-researcher-c3`, `gpt-6.1-sol`, high reasoning. Its profile SHA-256 is `a6fcb77a3c102c81bced469d275e7a93a25461943f6af880896758b98fdd05fe`.
- Launch attestation key `3b550d6776dcfb868278431d51dc2423387044fe94c85c796fbddc8380682f24`, agent ID `01a14c05-86c5-7503-b030-8861fe08cf83`, recorded `2026-10-17T18:39:55.2224849-04:00`, matches the current profile and receipt. Actual/expected model and effort match; routing, launch-authority, and provenance validity are true.
- EFC controller partials, viewer, breadcrumb router partials, WebView host, generated document assets, renderer, codec, message classes, and corresponding C# tests were inspected.
- Both inherited PowerShell runtime changes, current routing configuration, representative agent profiles, MCP configuration, actual TaskMaster CI workflows, and checked-in build/test scripts were inspected.
- Research performed no production, test, configuration, checkpoint, formatting, or QA mutations.

### Code Search Results

- `Enter`, `rowSelected`, and `BridgeJs` identify `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs:102-109`.
- `RenderDocument` confirms `BreadcrumbHtmlRenderer.cs:49` embeds those generated assets.
- `RowSelected` identifies the selection-only router branch at `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:434-436`.
- `ButtonOK_Click` identifies the existing guarded confirmation path at `EfcFormController.EventHandlers.cs:107-122`.
- `SelectorKey` shows `QuickFiler/Resources/FolderBreadcrumb.html` belongs to a separate selector protocol, rather than EFC’s generated document.
- Searches under `tests/` and hidden `.codex/` for `CodexDeployment`, `Get-CodexSubagentAttestation`, `record-subagent`, `Invoke-CodexSubagentAttestationCommand`, and `Resolve-CodexDeployment` found no existing Pester suites for the two modified routing runtime files.
- `QuickFiler.Test/QuickFiler.Test.csproj` uses explicit `Compile Include` entries; new test files require project entries.
- `BreadcrumbBridgeRouter.cs` contains 453 physical lines before implementation.

### External Research

- #githubRepo:"MicrosoftDocs/edge-developer WebView2 postMessage WebMessageReceived"
  - Microsoft reference source and samples describe the page-to-host bridge used by the repository.
- #fetch:https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.webmessagereceived
  - Page `postMessage` raises the host event; messages from the same page preserve posting order.
- #fetch:https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2controller.acceleratorkeypressed
  - A native accelerator interception alternative exists, but introduces another SDK event boundary.
- #fetch:https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/repeat
  - The repeat flag distinguishes key autorepeat and can prevent repeated confirmation messages.

### Project Conventions

- Add and execute a deterministic failing regression before the production fix.
- C# uses MSTest, Moq, and FluentAssertions; PowerShell uses Pester 5 and required PoshQC MCP tools.
- Tests must avoid live Outlook/WebView2, external executables, and filesystem fixtures.
- Capture baseline and final coverage; repository line coverage must remain at least 80%, new methods target at least 90%, and changed-file coverage must not regress.
- Implementation evidence belongs under the feature’s `evidence/<kind>/`.
- The earlier incomplete research draft contains superseded output-location and elevated-C3 reasoning statements.

## Key Discoveries

### Project Structure

`EfcFormController.ConfigureBreadcrumbControl` creates `WebView2BreadcrumbHost` and `BreadcrumbBridgeRouter`. The router generates its document through `BreadcrumbHtmlRenderer`, which embeds `BreadcrumbDocumentAssets.BridgeJs`. Host messages reach `ProcessInboundAsync`.

The router owns `SelectedFolderPath`; `EfcFormController.SelectedFolder` reads that property.

Against parent-confirmed merge-base `438436893d2e7500057b8a4f871e23a702ab9b34`, the observed inherited tracked inventory increased from 75 to 76 files after the MCP pin repair:

- Six `.agents/skills/` files: codex-model-routing, epic-orchestrate, epic-plan, execution-environment, orchestrate, and orchestrator-workflow.
- Sixty-two `.codex/agents/` profiles: ten families each containing base, C1, C2, C3, elevated C3, and C4 profiles, plus two forced epic personas.
- Four runtime/configuration files: `.codex/config.toml`, `.codex/hooks/record-subagent-routing-attestation.ps1`, `.codex/lib/codex-routing/CodexDeployment.psm1`, and `config/orchestration-routing.json`.
- Four existing feature documents: issue, scaffolded plan, incomplete research, and promoted potential entry.

The ten profile families are atomic-executor, atomic-planner, commit-steward, csharp-typed-engineer, feature-reviewer, orchestrator, powershell-typed-engineer, pr-author, prd-feature, and task-researcher.

Subsequent requirements, planning, implementation, and test edits must also remain in final branch-wide review.

### Implementation Patterns

Verified defect: Enter posts `rowSelected` and prevents default browser handling. The router handles `rowSelected` by selecting the row, without any confirmation callback. Controller router wiring subscribes to focus navigation only.

The existing OK path is:

`ButtonOK_Click` → awaited `ButtonOkClickAsync` → `ActionOkAsync`.

`ButtonOkClickAsync` catches and reports faults through the existing boundary sink. `ActionOkAsync` rejects null, banner, and invalid/rooted selections through `EfcSelectionGuard`, then performs Sort or Find and cleanup.

Important selection constraint: `SelectRow` sets the path to the original `row.FilingTarget`. Segment or rendered-child activation keeps the same selected row ID but changes `SelectedFolderPath` to the activated target. Reselecting during confirmation would overwrite that target. Confirmation must use the current selected path.

Unknown row IDs are rejected before dispatch. Banner selection is ignored. Rebinding clears row selection and selected path. A new confirmation operation must not revive stale selection or confirm a different row.

### Complete Examples

Current generated JavaScript:

    if (e.key === 'Enter') {
      var selected = document.querySelector('.rowwrap.selected');
      var id = selected ? selected.getAttribute('data-row-id') : '';
      post({ type: 'rowSelected', rowId: id });
      e.preventDefault();
      return;
    }

Current router dispatch:

    case BreadcrumbMessageTypes.RowSelected:
        SelectRow(row);
        break;

These excerpts show the missing confirmation intent.

### API and Schema Documentation

Current inbound schema:

`{ type, rowId, segmentIndex?, childIndex?, key? }`

The codec validates the known discriminator and required string row ID, plus type-specific fields. Malformed input raises `BreadcrumbMessageException`, contained by the host-event boundary.

Recommended additive message:

`{ "type": "rowConfirmed", "rowId": "row-N" }`

Retain ordinary `rowSelected` semantics. Emit a separate router confirmation event only for the matching current selection, preserving its path. Wire that event to the existing OK boundary.

### Configuration Examples

Current configuration and PowerShell resolver agree:

| Profile | Model | Reasoning |
|---|---|---|
| C1 | gpt-6-luna | low |
| C2 | gpt-6.1-sol | medium |
| C3 standalone | gpt-6.1-sol | high |
| C3 elevated | gpt-6-astra | high |
| C4 | gpt-6-astra | max |
| Forced epics | gpt-6-astra | ultra |

Elevated C3 uses high, not max. The observed MCP pin now names `@danmoisan/drm-copilot-mcp@1.1.21`. Upgraded live validation is parent-owned; this researcher did not call the obsolete cached validator.

### Technical Requirements

The inherited attestation hook includes production changes beyond model names:

- `OpenStream`, `EnsureDirectory`, and `FlushStream` injectable boundaries.
- New `Invoke-CodexSubagentAttestationCommand` orchestration wrapper.

Test its output/error behavior, exclusive-access arguments, stream disposal, single-use root-receipt consumption, malformed payload, invalid HEAD, and routing/provenance marker paths. Use memory-backed streams and mocked boundary functions.

Resolver tests must cover all bands, C3 overlays, forced personas, alias handling, invalid inputs, unsupported agents, and unavailable-model rejection.

Actual TaskMaster CI is `.github/workflows/ci.yml`, invoking `_format-check`, `_build-analyzers`, `_build-nullable`, `_mstest-coverage`, `_pester`, `_hygiene`, and `_actionlint`.

The copied execution-environment brief describes drm-copilot CI. Its upstream Python generator, bundle manifests, and corresponding contract suites are absent from this checkout and must not be treated as local requirements that can be executed.

Actual Pester CI scans:

- `tests/scripts/dependencies`
- `tests/scripts/hygiene`
- `tests/scripts/vscode`

Its coverage population is the matching three `scripts/` directories. New routing suites placed under `tests/scripts/vscode/` are discovered, but changed-file coverage must separately include the two touched `.codex` PowerShell files.

Existing C# bridge tests explicitly document the absence of a headless-browser/JavaScript-engine dependency. Static assertions establish emitted bridge text; router/controller behavioral tests must establish host-side confirmation. Do not claim live browser execution from string assertions.

No objective has been proven unachievable.

## Recommended Approach

Add a dedicated confirmation intent to EFC’s existing generated bridge. Plain, nonrepeating Enter emits one `rowConfirmed` message and consumes default handling. Modified Enter does not file. Preserve ordinary selection, arrow routing, Alt behavior, and the separate selector.

The router confirms the matching current selection without calling `SelectRow`. Connect the event to the existing OK wrapper, retaining validation and error reporting.

Expected production targets:

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs`
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`, if a helper is extracted
- `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`

New test files also require `QuickFiler.Test/QuickFiler.Test.csproj` entries.

Rejected alternatives: native accelerator interception introduces another SDK boundary; form AcceptButton/ProcessCmdKey alone does not repair the existing bridge message; treating selection as confirmation would file on clicks; reselecting before confirmation would lose activated ancestor/child targets.

## Implementation Guidance

- **Objectives**: Fix Enter confirmation and validate the entire inherited workflow repair.
- **Key Tasks**: Capture failing bridge regression first, add behavioral coverage, implement the minimal additive message/event wiring, execute ordered full gates, and preserve exact plan/artifact paths.
- **Dependencies**: Existing codec, host/provider interfaces, OK boundary, test frameworks, and published MCP package; no added product dependencies.
- **Success Criteria**: Valid highlighted destination confirms once; clicks do not confirm; invalid, missing, unknown, mismatched, or rebound selection cannot file; active ancestor/child paths survive; modifiers/repeat do not produce filing; existing navigation/cancellation passes; C# and PowerShell gates and coverage comparisons pass.

Proposed tests:

1. Update `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`, whose current `Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView` pins the defect. Capture the failing run before production edits. Suggested replacement: `Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView`.
2. Extend existing `BreadcrumbMessageCodecTests.cs` with valid confirmation parsing and missing/wrong row ID rejection.
3. Add `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs` with:
   - `ValidSelectedRow_ConfirmsExactlyOnce`
   - `RowSelected_DoesNotConfirm`
   - `MissingUnknownBannerOrMismatchedRow_DoesNotConfirm`
   - `RebindThenConfirm_DoesNotUseStaleSelection`
   - `ConfirmAfterSegmentActivation_PreservesTarget`
   - `ConfirmAfterChildActivation_PreservesTarget`
4. Add `EfcFormControllerIssue995Tests.cs` if a narrowly extracted wiring seam is necessary. Verify the callback reaches the existing OK boundary once and uses the existing error sink. Avoid live MessageBox/Outlook.
5. Add `tests/scripts/vscode/CodexDeployment.Tests.ps1` and `CodexSubagentRoutingAttestation.Tests.ps1` for pure resolver and memory-backed hook tests.

Checked-in C# gate commands:

    dotnet tool restore
    dotnet tool run csharpier format .
    dotnet tool run csharpier check .
    pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild
    pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -TreatWarningsAsErrors
    pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
    pwsh -NoProfile -File scripts/hygiene/Test-RepositoryHygiene.ps1

The checked-in QA skill and actual nullable CI use per-file nullable opt-in and warnings as errors, with Rebuild to prevent incremental skipping.

PowerShell requires PoshQC MCP format → analyze → test, plus direct result/coverage inspection. Capture exact commands, timestamps, exit codes, summaries, and baseline comparisons under feature evidence folders. This researcher ran no toolchain gates.
