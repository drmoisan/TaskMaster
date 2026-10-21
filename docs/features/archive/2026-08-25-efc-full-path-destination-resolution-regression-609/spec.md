# efc-full-path-destination-resolution-regression (Spec)

- **Issue:** #609
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-25T12-47
- **Status:** Draft
- **Version:** 0.1

## Context
Efc has two intentional representations for a destination: an archive-relative filing target such as `Clients\North`, and a full Outlook hierarchy path such as `\\mailbox@example.com\Archive\Clients\North`. The former is the Efc filing contract; the latter is only the lookup contract for `IFolderHierarchyProvider.ResolveLeafKeyAsync`.

Environment:
- OS/version: Windows with Outlook/MAPI folder paths.
- Python version: Not applicable; this is C# code and MSTest coverage.
- Command/flags used: Headless MSTest coverage for the affected test assemblies.
- Data source or fixture: `ArchiveRootPath = @"\\mailbox@example.com\Archive"` and a relative target of `@"Clients\North"`.

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Bind `Clients\North` as a search, suggestion, or presented row target with an archive root of `\\mailbox@example.com\Archive`.
2. Require a strict hierarchy-provider mock to resolve `\\mailbox@example.com\Archive\Clients\North`, then select the direct row, an ancestor segment, or an immediate child.
3. Supply the resulting selection to `EmailFilerConfig` as `DestinationOlStem` with the same `OlAncestor`.

Expected:
Only the provider lookup uses the full Outlook path. Direct and navigated selections remain archive-relative, and `EmailFilerConfig.ResolvePaths` produces one archive root plus one relative stem.

Actual:
`EmailFilerConfig.ResolvePaths` unconditionally constructs `DestinationOlPath` from `OlAncestor` and `DestinationOlStem`. If a full hierarchy value crosses the selection boundary, the archive root is duplicated and Outlook destination resolution returns no folder. The currently supplied evidence does not include a runtime failure log; it establishes an untested regression boundary, including mailbox identifiers containing `@`.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: No runtime log or screenshot was supplied. The code trace in `artifacts/research/2026-08-25T12-51-efc-full-path-destination-resolution-research.md` is the supporting evidence.


## Scope & Non-Goals
- In scope: Preserve the archive-relative filing value across Efc row selection, typed segment activation, and immediate-child activation; prove the full lookup and filing-construction boundaries with deterministic C# tests. If a direct fail-before `FolderPredictor.FolderArray` test establishes that an in-root persisted full Outlook value reaches startup presentation verbatim, apply the smallest archive-root-aware startup projection: remove only the matching archive root plus one separator, and project the aligned `FolderScore` key in the same scope.
- Out of scope / non-goals: Redesigning search-result generation, generic source-map normalization, persisting full hierarchy paths, changing Outlook COM integration, or introducing a new destination-path representation. The exact persisted source record remains unchanged; this scope permits only the proven startup presentation/filing projection.
- Explicitly excluded systems, integrations, or datasets: `Store.FilePath`, mailbox-name parsing around `@`, filesystem API redesign, and external Outlook, WebView2, network, or temporary-file test dependencies.

## Root Cause Analysis
`IFolderHierarchyProvider.ResolveLeafKeyAsync` correctly requires a full, case-insensitive Outlook `FolderPath`. `BreadcrumbBridgeRouter` is responsible for expanding an archive-relative row only for that lookup and for returning an archive-relative value after hierarchy navigation. `EfcDataModel` then forwards the selection unchanged to `EmailFilerConfig.DestinationOlStem`, whose `ResolvePaths` method prefixes `OlAncestor` without normalizing a full stem. The representation contract is therefore safety-critical but not explicitly guarded at the filing boundary, and existing regression coverage omits an `@` mailbox root.


## Proposed Fix

### Design summary (what changes where):
Add regression coverage first. Retain the router's dual-representation boundary and retain `EmailFilerConfig` as the only filing-time full-path constructor. If a direct `FolderPredictor.FolderArray` fail-before test proves that a persisted in-root full Outlook path reaches startup presentation verbatim, apply the smallest correction at the archive-root-aware `FolderPredictor` startup projection: remove exactly the matching root plus one separator, keep already-relative and out-of-root values unchanged, and project the matching `FolderScore` key with the display value. Do not alter `BreadcrumbBridgeRouter`, `EmailFilerConfig`, source-map storage, or propagate full hierarchy paths downstream.

### Boundaries and invariants to preserve:
- `ResolveLeafKeyAsync` receives the complete Outlook `FolderPath`.
- `BreadcrumbBridgeRouter` rows retain their original archive-relative `FilingTarget`.
- `EfcDataModel` receives an archive-relative selection, and `EmailFilerConfig.DestinationOlStem` is never a full hierarchy path.
- `EmailFilerConfig` constructs the Outlook destination exactly once as `OlAncestor + "\\" + DestinationOlStem`.
- `@` is ordinary data in a mailbox identifier; it is not a path delimiter. `Store.FilePath` is not an Outlook hierarchy identity.

### Dependencies or blocked work:
The existing `IFolderHierarchyProvider`, `IBreadcrumbWebHost`, `BreadcrumbBridgeRouterIssue439Tests`, and `EmailFilerConfig_Tests` seams are sufficient. No new package, external service, or manual test dependency is required.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` for the direct fail-before startup-projection regression and its aligned-score, already-relative, and out-of-root cases.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` only if that direct regression proves the startup projection is incorrect.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` for router compatibility regressions.
- `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs` for the destination-construction regression.

#### Functions/classes/CLI commands impacted:
- `FolderPredictor.FolderArray` and its startup suggestion projection, with the paired `FolderScore` projection kept aligned to the displayed row.
- `BreadcrumbBridgeRouter.BindRowsAsync`, its hierarchy-path conversion, direct-row selection, typed segment activation, and immediate-child activation as unchanged compatibility boundaries.
- `IFolderHierarchyProvider.ResolveLeafKeyAsync` as the full-path lookup contract.
- `EfcDataModel` and `EfcHomeController.ExecuteMoves` as unchanged consumers of an archive-relative selection.
- `EmailFilerConfig.ResolvePaths` and `DestinationOlStem` as the single-prefixing filing contract.

#### Data flow and validation changes:
For `Clients\North`, derive `\\mailbox@example.com\Archive\Clients\North` only for hierarchy lookup. When the direct fail-before test proves that startup input is instead `\\mailbox@example.com\Archive\Clients\North`, `FolderPredictor` projects it and its corresponding score key to `Clients\North` only when the archive root and following separator match; it leaves already-relative and out-of-root values unchanged. Convert hierarchy-navigation output back to `Clients\North` before the form and data model consume it. Assert that `EmailFilerConfig` constructs `\\mailbox@example.com\Archive\Clients\North`, not a duplicated-root variant.

#### Error handling and logging updates:
No new logging is required for the tested representation-preserving path. Do not silently normalize an already-full `DestinationOlStem`; that would conceal an upstream contract breach. If a production guard is proposed after a failing regression, its error behavior must be specified and tested before implementation.

#### Rollback/feature-flag considerations (if applicable):
No feature flag is required. If a production correction is needed, it is limited to the proven `FolderPredictor` startup projection and can be reverted independently of router behavior, persistence, Outlook COM, and filesystem behavior.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- Input to hierarchy lookup: full Outlook path, for example `\\mailbox@example.com\Archive\Clients\North`.
- Input to filing: archive-relative `DestinationOlStem`, for example `Clients\North`.
- Output from `EmailFilerConfig.ResolvePaths`: `DestinationOlPath` containing one archive root and one relative stem, plus the existing derived filesystem save path.

#### Required configuration keys and defaults:
Use the existing `OlAncestor`, `DestinationOlStem`, `ArchiveRootPath`, and save-path configuration behavior. No configuration key or default changes are required.

#### Backward-compatibility expectations:
Keep existing relative search, suggestion, direct-selection, banner, trash, and root-boundary behavior unchanged. Existing callers continue to supply archive-relative stems.

#### Performance constraints (latency/throughput/memory):
The fix and tests must remain in-process and deterministic. No additional Outlook COM calls, I/O, network access, caching, or allocation-heavy path processing is permitted.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): The checked-out source trace and existing test seams accurately represent the Efc destination flow; a local Outlook process is not required for the unit tests.
- Constraints (budget, performance, compatibility): Preserve existing public and configuration contracts; use the smallest boundary-local correction only if regression tests fail.
- External dependencies (services, libraries, releases): Existing MSTest, Moq, FluentAssertions, and repository test projects only.

## Data / API / Config Impact
- User-facing or API changes: None. The behavior is a correction to destination resolution for existing Efc selection flows.
- Data or migration considerations: None. Full hierarchy paths must not be persisted as filing targets.
- Logging/telemetry updates (if any): None planned; deterministic regression tests provide the validation evidence.
- Compatibility notes (CLI flags, config schemas, versioning): No CLI flags, schema changes, or version changes. Existing archive-relative `DestinationOlStem` callers remain compatible.

## Test Strategy
Seeded from issue:

- [ ] Add strict `IFolderHierarchyProvider` and `IBreadcrumbWebHost` regressions in `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`.
- [ ] Add an `@`-mailbox `EmailFilerConfig.ResolvePaths` regression in `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`.
- [ ] Retest existing banner, trash, and root-boundary scenarios through the current unit suite.

- Regression tests to add or update: Direct row selection, typed ancestor activation, and immediate-child activation with `archiveRoot = @"\\mailbox@example.com\Archive"` and `presentedTarget = @"Clients\North"`; verify the provider receives the full lookup string and each selected filing target is relative.
- Unit tests (MSTest) for the fixed behavior and boundaries: In `EmailFilerConfig_Tests.cs`, assert `DestinationOlPath` equals `\\mailbox@example.com\Archive\Clients\North` and assert the existing derived `SaveFsPath` expectation using the same mocked folder/global seams.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): Already-rooted hierarchy lookup input must not be prefixed again; hierarchy output outside the archive root must not be converted by the inverse helper; preserve existing banner, trash, and root-boundary behavior.
- Error handling and logging verification: Verify no unexpected logging or error behavior is introduced. Any proposed guard for a full `DestinationOlStem` requires a defined, tested error contract.
- Coverage impact and targets for changed lines/modules: Maintain repository-wide line coverage of at least 80%, target at least 90% for newly added testable behavior, and do not reduce coverage for changed lines.
- Toolchain commands to run (format → lint → type-check → test): `dotnet tool run csharpier format .`; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`. Restart at formatting if any step changes files or fails.
- Manual validation steps (if required): No manual validation is required for this testable boundary. If runtime validation is later requested, use a mailbox identifier containing `@` and confirm the selected destination is resolved once beneath the Archive root.


## Acceptance Criteria
- [x] `BreadcrumbBridgeRouterIssue439Tests.cs` verifies that `ResolveLeafKeyAsync` receives exactly `\\mailbox@example.com\Archive\Clients\North` for a `Clients\North` row under the specified archive root.
- [x] Direct row selection for that row returns `Clients\North` to the Efc filing flow and never returns a full Outlook hierarchy path.
- [x] Typed ancestor-segment activation and immediate-child activation for that row return archive-relative filing targets only.
- [x] `EmailFilerConfig_Tests.cs` verifies that an `@` mailbox root plus a relative `DestinationOlStem` produces exactly one archive-root prefix and the expected save-path mapping.
- [x] Existing banner, trash, root-boundary, and relative search/suggestion behavior remains covered and unchanged.
- [x] No implementation parses `@` as a mailbox delimiter or substitutes `Store.FilePath` for Outlook `FolderPath` in this flow.
- [x] If a direct `FolderPredictor.FolderArray` fail-before test proves an in-root persisted full Outlook value reaches startup presentation verbatim, any production correction is limited to an archive-root-aware `FolderPredictor` projection that removes only the matching root plus one separator and projects the aligned score key; `BreadcrumbBridgeRouter`, `EmailFilerConfig`, `EfcDataModel`, `EfcHomeController`, generic source-map normalization, `@` parsing, `Store.FilePath`, Outlook COM calls, persistence, and filesystem behavior remain unchanged.
- [x] The final C# formatting, analyzer, nullable-analysis, and coverage-enabled MSTest pass completes without new failures, with evidence written only under this feature's `evidence/<kind>/` folders.

## Risks & Mitigations
- Technical or operational risks: A full value can be mistaken for an archive-relative stem at a boundary not covered by current tests; a broad normalization change could hide the breach or alter existing navigation behavior.
- Mitigations and rollbacks: Use strict mocks and exact-string assertions at both contracts. Make no production edit until a deterministic regression fails; keep any correction local and retain the existing test suite as compatibility coverage.

## Rollout & Follow-up
- Release/rollout steps: Deliver the regression tests and any necessary boundary-local correction through the normal C# QA loop. No migration, feature flag, or configuration rollout is required.
- Post-fix monitoring or clean-up tasks: Review test evidence for the `@` mailbox case and retain the scenario as coverage for future path-conversion changes.
- Links: Issue #609; `artifacts/research/2026-08-25T12-51-efc-full-path-destination-resolution-research.md`; `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`; `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs`.
