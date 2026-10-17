# 2026-10-17-efc-viewer-enter-confirmation (Spec)

- **Issue:** #995
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-17T18-42
- **Status:** Implemented and verified with targeted tests; final QA in progress
- **Version:** 0.1

## Context

- Summary of the bug and its impact (link to repro/playbook entry): Pressing Enter after highlighting an EFC Viewer destination does not invoke OK. The reproduction is recorded in `issue.md` and [issue #995](https://github.com/drmoisan/TaskMaster/issues/995).
- Observed environment(s): TaskMaster QuickFiler EFC Viewer on Windows, using the hosted WebView2 breadcrumb document. The user's exact deployed build was not supplied.
- Customer impact and severity (who is affected, how often, how bad): Medium; users cannot confirm the highlighted destination through the expected Enter interaction. Clicking OK remains the existing confirmation path.
- First observed date and version(s) impacted: Report captured 2026-10-17. The affected release range is unknown; the checked-out source contains the identified routing gap.

## Repro & Evidence

- Steps to reproduce (with data/flags/inputs): Open EFC Viewer for an email, highlight a valid destination folder in the breadcrumb list, and press plain Enter while the hosted list has focus.
- Expected vs actual behavior: One Enter key press invokes the same guarded OK action as clicking OK exactly once, using the currently activated destination. The reported behavior is that nothing happens. Source review shows Enter is consumed by the document and only reselects a row.
- Logs/screenshots/error snippets: No user logs or screenshots were supplied. Research findings identify the bridge, router, and controller event gap; automated failing-regression and final QA evidence must be captured under this feature's `evidence/regression-testing/` and `evidence/qa-gates/` directories.
- Frequency / determinism (always, intermittent, data-dependent): Source routing is deterministic for Enter handled by the breadcrumb document. Behavior under other focus locations and the user's exact installed version has not been independently observed.

## Scope & Non-Goals

- In scope: EFC breadcrumb Enter confirmation, typed inbound message decoding, selection validation, and wiring to the existing OK boundary. Include regression tests for current-row identity, invalid selections, activated ancestor/child targets, repeated and modified keys, and boundary exception handling.
- In scope: Complete the user-authorized workflow repair already present in the branch: Codex profile/configuration parity, the deployment resolver, the attestation hook, supporting skill references, and the MCP package pin. Verify the complete branch diff against the resolved merge base, including inherited changes.
- Out of scope / non-goals: Broad EFC navigation redesign, new folder selection policies, replacing the filing pipeline, new user-facing settings, and a `user-story.md` artifact. This item uses `full-bug` mode and `spec.md` is the authoritative acceptance-criteria source.
- Explicitly excluded systems, integrations, or datasets: Unrelated Outlook features, mailbox-data migrations, live Outlook/WebView dependencies in unit tests, and new third-party runtime libraries.

## Root Cause Analysis

- Current hypothesis or confirmed root cause: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` defines the generated document's `BridgeJs`. Its Enter handler posts `rowSelected` and calls `preventDefault`. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` handles `rowSelected` in `ProcessInboundAsync` through `SelectRow`, with no confirmation signal. `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` configures focus handling but does not connect document confirmation to OK.
- Signals/evidence supporting it: The existing `Issue737BridgeJsPostsRowSelectedOnEnterAndScrollsSelectedRowIntoView` test in `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs` asserts the selection-only behavior. The existing confirmation chain is `ButtonOK_Click` -> `ButtonOkClickAsync` -> `ActionOkAsync`; it provides validation and the asynchronous exception boundary required for filing.
- Affected components/modules (paths, services, pipelines): The generated breadcrumb bridge, `BreadcrumbMessageTypes`, `BreadcrumbMessageCodec`, `BreadcrumbBridgeRouter`, its selection partial, and `EfcFormController` breadcrumb configuration. `FolderBreadcrumb.html` is a separate selector and is not the generated document asset responsible for this report.
- The research worker's verified source findings are sufficient to complete this specification. The final research artifact will be referenced by the plan; source inspection establishes the cause, while regression execution remains required before implementation.

## Proposed Fix

### Design summary (what changes where):

Introduce a distinct `rowConfirmed` inbound message for plain Enter. Decode it through the existing typed codec, validate the current selected row in the router, and raise a narrow `ConfirmationRequested` event. Subscribe the controller to its existing `ButtonOK_Click` boundary so keyboard confirmation uses the existing filing and validation behavior.

Complete and verify the authorized native-model repair across checked-in profiles, routing policy, resolver, attestation hook, and supporting references. Pin `.codex/config.toml` to the compatible `@danmoisan/drm-copilot-mcp@1.1.21` package and prove validation through that package's MCP protocol surface. Preserve historical receipts as historical evidence rather than rewriting them to fabricate current attestations.

### Boundaries and invariants to preserve:

- A single eligible Enter dispatch requests OK once; repeated keydown events and Enter combined with Alt, Ctrl, Shift, or Meta do not request confirmation.
- Confirm only the matching current selected nonbanner row with a valid retained destination. Reject absent, unknown, stale, banner, or invalid selections without filing.
- Confirmation must not call `SelectRow` again. Reselecting would replace a destination previously activated through an ancestor segment or rendered child.
- Clicking or selecting a row remains selection-only. Arrow navigation, breadcrumb activation, focus transfer, Escape/cancel, and existing modifier behavior remain unchanged.
- Preserve UI-thread ownership and the existing OK exception boundary. Do not call `ActionOkAsync` directly from the bridge or add a second independent filing implementation.
- Keep routing/profile attestation, validation-only preflight, coverage, review, and completion gates enforced. Profile unavailability and invalid receipts continue to fail closed.

### Dependencies or blocked work:

The workflow repair must demonstrate that the package serving MCP validation matches the repository's native-model policy; a cached older session connection is insufficient evidence. The assigned protocol worker owns that proof. Implementation remains gated by the orchestration checkpoint and approved plan, and requirements authoring does not satisfy those gates.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`: replace the Enter selection message with guarded confirmation dispatch.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` and `BreadcrumbMessageCodec.cs`: add and validate the `rowConfirmed` discriminator.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` and `BreadcrumbBridgeRouter.Selection.cs`: route confirmation and validate retained selection; keep each file within the 500-line limit.
- `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`: wire `ConfirmationRequested` to the existing OK boundary with lifecycle-appropriate event ownership.
- Existing renderer/codec test homes and the research-proposed `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs` and `EfcFormControllerIssue995Tests.cs`: prove routing, retained targets, invalid-input guards, and controller behavior.
- `QuickFiler.Test/QuickFiler.Test.csproj`: include any new QuickFiler test files explicitly, following the existing project's `Compile Include` convention.
- Workflow repair inventory: the inherited changed `.codex/agents/*.toml` profiles, `config/orchestration-routing.json`, `.codex/config.toml`, `.codex/lib/codex-routing/CodexDeployment.psm1`, `.codex/hooks/record-subagent-routing-attestation.ps1`, and the six changed `.agents/skills/` references identified by the branch diff. Routing regression coverage belongs under the existing CI-scanned `tests/scripts/vscode/` directory.

#### Functions/classes/CLI commands impacted:

`BreadcrumbMessageTypes`, `BreadcrumbMessageCodec.DeserializeInbound`, `BreadcrumbBridgeRouter.ProcessInboundAsync`, the router's selection-validation helpers, `EfcFormController.ConfigureBreadcrumbControl`, and the `ButtonOK_Click` entry boundary are the EFC interfaces in scope. The existing `Resolve-CodexDeployment` and subagent-routing attestation functions are the workflow interfaces in scope. No end-user CLI command is added.

#### Data flow and validation changes:

The generated document posts `{ "type": "rowConfirmed", "rowId": "<selected-row-id>" }` for one eligible key press and consumes that Enter event. The codec produces a typed inbound message; the router verifies its identity against current selected state and verifies the retained destination before raising `ConfirmationRequested`. The controller invokes its existing OK boundary, which performs final destination validation and filing. Rebinding the list invalidates stale row identities; confirmation cannot restore an older selection.

#### Error handling and logging updates:

Keep malformed payload handling through `BreadcrumbMessageException` and the codec's existing log4net diagnostics. Preserve the controller's existing asynchronous boundary error sink when OK fails. Invalid/stale confirmations produce no filing side effect. Add logging only where the existing pattern requires it; no new telemetry system or mailbox-content logging is needed.

#### Rollback/feature-flag considerations (if applicable):

No feature flag is required. Revert the EFC confirmation changes as one compatible bridge/controller change if filing behavior regresses. Revert workflow repair only as a consistent profile/policy/resolver/package set; an isolated model-string rollback would recreate the mismatch.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

Input is the existing camelCase JSON bridge envelope with the new `rowConfirmed` discriminator and required string `rowId`. No segment/child index is needed because confirmation uses the router's already retained target. Output is one controller confirmation request for an eligible selection, followed by the existing OK outcome. Other inbound/outbound message formats remain supported.

#### Required configuration keys and defaults:

No EFC configuration key is introduced. Keep native routing policy consistent: C1 `gpt-6-luna`/low, C2 `gpt-6.1-sol`/medium, C3 `gpt-6.1-sol`/high, C4 `gpt-6-astra`/max, C3 elevated `gpt-6-astra`/high, and forced epic personas `gpt-6-astra`/ultra. `.codex/config.toml` selects MCP package version `1.1.21`; the default routing and topology rules otherwise remain unchanged.

#### Backward-compatibility expectations:

The generated document and host codec ship together. Preserve existing `rowSelected` behavior and all established message types. No persisted mailbox, model, or feature-document schema migration is required. Historical delegation receipts retain their recorded identity; new receipts must match the actual profile hash, model, and reasoning effort.

#### Performance constraints (latency/throughput/memory):

Confirmation performs current-row and destination checks over the router's existing state without enumerating Outlook folders or issuing network requests. Suppressing repeated keydown events prevents extra confirmation dispatch. Keep the existing async filing pipeline and avoid adding timers, sleeps, retries, or mailbox-wide scans.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): The report refers to the Enter key after highlighting an EFC breadcrumb destination. The existing OK path is the intended filing behavior. Source research supplies exact component and test homes; the deployed release remains unknown.
- Constraints (budget, performance, compatibility): This is the large mixed-scope route because authorized workflow repair extends beyond the C# direct-mode budget. Preserve legacy .NET Framework/VSTO compatibility, deterministic tests, no temporary test files, file-size limits, and all required baseline/QA evidence.
- External dependencies (services, libraries, releases): Existing WebView2 and Outlook/VSTO runtime dependencies remain in place. Workflow validation depends on the compatible published MCP package; the repair must not replace mandatory MCP validation with an unsupported local substitute.

## Data / API / Config Impact

- User-facing or API changes: Enter confirms the currently activated valid destination exactly as OK. A bridge discriminator and router event are added; no additional visible control or setting is introduced.
- Data or migration considerations: Selection state remains in memory; confirmation retains the active ancestor/child destination. No data-format or mailbox migration occurs.
- Logging/telemetry updates (if any): Existing codec diagnostics and controller boundary error handling remain authoritative. Evidence must distinguish inspected source, a failing regression, passing tests, and actual MCP validation.
- Compatibility notes (CLI flags, config schemas, versioning): Native model strings and the MCP package version are coordinated across runtime surfaces. Supporting skill references must match the final implementation. Preserve all existing routing-policy keys and CLI contracts.

## Test Strategy

- Regression tests to add or update: First change the renderer expectation in `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs` from selection-only Enter to confirmation dispatch and capture its failure before production changes. Extend `BreadcrumbMessageCodecTests.cs` for valid and malformed confirmation envelopes. Proposed router tests in `BreadcrumbBridgeRouterIssue995Tests.cs` are `ValidSelectedRow_ConfirmsExactlyOnce`, `RowSelected_DoesNotConfirm`, `MissingUnknownBannerOrStaleRow_DoesNotConfirm`, `ConfirmAfterSegmentOrChildActivation_PreservesTarget`, and `RebindThenConfirm_DoesNotUseStaleSelection`. Use `EfcFormControllerIssue995Tests.cs` to prove existing-boundary wiring and exception handling with the established uninitialized-controller and `BoundaryErrorSink` idiom.
- Unit tests (pytest) for the fixed behavior and boundaries: The product path uses MSTest, Moq, and FluentAssertions. Routing/hook coverage uses the repository's Pester tooling and the actual TaskMaster CI-scanned `tests/scripts/vscode/` home; do not infer test discovery from the copied drm-copilot environment brief.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): Null/malformed/unknown message values; absent, unknown, banner, and stale row IDs; invalid destinations; current selection after segment/child activation; list rebind; plain versus modified Enter; repeated keydown; row click/selection without confirmation; arrows, focus transfer, and cancel.
- Error handling and logging verification: Mock external boundaries and assert failed OK handling reaches the established boundary sink. Preserve malformed-payload diagnostics. Routing tests must reject incorrect actual model/effort/hash, unavailable required models, and invalid launch authority while accepting the configured profiles and valid attestations.
- Coverage impact and targets for changed lines/modules: Capture C# and PowerShell baselines and final reports, exclude tests, maintain repository line coverage at or above 80%, target at least 90% for new methods/classes/modules, and prevent coverage loss on changed lines. Required comparisons and evidence are blocking review inputs.
- Toolchain commands to run (format → lint → type-check → test): C# uses `dotnet tool run csharpier format .` followed by `dotnet tool run csharpier check .`; `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild`; `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors`; and `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`. PowerShell uses the required PoshQC MCP format/analyze/test surfaces with direct findings, counts, coverage, and compatibility evidence. Restart each affected language's pass from formatting after failures or code changes. Validate the complete inherited profile/configuration/document diff and actual TaskMaster CI contracts, not only the EFC files.
- Manual validation steps (if required): The reported flow is reproducible in a running EFC Viewer, but completion evidence must be obtained through deterministic unit tests and automated workflow checks without adding an unrecorded manual dependency. Any live Outlook check is supplementary and must be identified separately from automated verification.

## Acceptance Criteria

Verification progress: the renderer regression failed before the production edit and passed afterward with all 232 targeted C# cases. All 49 deterministic routing/attestation cases pass, and native MCP protocol validation confirms version `1.1.21` and current receipts. Canonical evidence is linked in `issue.md`; full final gates and coverage comparisons remain pending. JavaScript checks verify generated text, while router/controller tests exercise host behavior without live WebView or Outlook.

- [x] One plain, nonrepeated Enter press with a valid highlighted current destination requests the existing guarded OK action exactly once, and a row-selection message alone does not request OK.
- [x] Missing, unknown, banner, stale, or invalid destinations never file an email through confirmation; the existing OK validation remains effective.
- [x] Confirmation after ancestor-segment or rendered-child activation retains that activated destination, and list rebinding prevents stale selection confirmation.
- [x] Modified Enter and repeated keydown events do not request confirmation; existing arrow navigation, focus transfer, row selection, breadcrumb activation, and cancellation behavior remain unchanged.
- [x] A deterministic Enter regression fails before the production fix and passes afterward; codec, router, and controller boundary tests cover the positive, invalid, stale-selection, retained-target, and exception scenarios with no live Outlook/WebView or temporary-file dependencies.
- [x] The complete inherited model-workflow repair has consistent profile/configuration/resolver values, preserved attestation rejection behavior, and compatible MCP package version `1.1.21`; deterministic routing tests and actual MCP protocol validation establish those results without bypassing required gates.
- [ ] The final C# and PowerShell formatting, analysis, type/compatibility checks, tests, and required coverage comparisons pass with canonical evidence for the entire changed-language scope.
- [x] Issue, spec, promoted-record links, and supporting workflow references match the final behavior and `full-bug` work mode; all required evidence is stored under this feature's canonical evidence directories.

## Risks & Mitigations

- Technical or operational risks: Reselecting at confirmation could file to a row's original leaf instead of an activated ancestor/child. A queued stale message could confirm the wrong current row. Host default-button handling or key repeats could dispatch twice. An older cached MCP process could report the previous model policy despite a corrected package pin.
- Mitigations and rollbacks: Keep confirmation separate from selection, validate current identity and retained destination, guard modified/repeated key events, and verify one invocation through controller wiring tests. Prove MCP package/protocol identity and preserve authoritative validation. Keep EFC and workflow repair changes internally consistent for targeted rollback.

## Rollout & Follow-up

- Release/rollout steps: Complete the approved atomic plan, failing-first regression, local QA, full branch review, and the existing orchestration PR/CI/completion gates. The executor and reviewer check off each criterion only after corresponding evidence exists.
- Post-fix monitoring or clean-up tasks: Monitor subsequent EFC reports for Enter confirmation and destination preservation. Keep useful regression coverage and the package-version proof. Do not expand into unrelated keyboard changes without a separate issue and evidence.
- Links: issue, PRs, related docs: [Issue #995](https://github.com/drmoisan/TaskMaster/issues/995), `issue.md`, `plan.2026-10-17T13-44.md`, and this feature's `research/` and `evidence/` directories. The orchestrator records the PR link when created.
