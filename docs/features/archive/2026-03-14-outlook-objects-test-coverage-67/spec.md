# 2026-03-14-outlook-objects-test-coverage-67 — Spec

- **Issue:** #67
- **Parent (optional):** none
- **Owner:** Dan Moisan
- **Last Updated:** 2026-03-14T11-01
- **Status:** Draft
- **Version:** 0.1

## Overview

This feature defines the implementation target for raising unit-test coverage across `UtilitiesCS/OutlookObjects`, a verified 51-file C# surface spread across 15 subdirectories plus root `MailResolution.cs`. The intended implementation adds repo-standard MSTest coverage in `UtilitiesCS.Test/OutlookObjects`, mirrors the production folder layout, and drives each **testable** production file to at least 80% line coverage while preserving current runtime behavior on `.NET Framework 4.8.1`.

The scope is limited to the OutlookObjects module and the minimum production seams required to unit test deterministic logic. It explicitly includes wrappers, DTOs, comparers, parsers, reflection helpers, lazy-loading helpers, and selected orchestration code where isolation is practical, and it explicitly excludes any attempt to replace live Outlook integration testing or to perform a broad architectural rewrite of the Outlook interop layer. The provided `issue.md` and `research.md` are sufficient to support downstream C# atomic planning for this feature.

- Target users/personas and primary use cases:
	- Developers maintaining `UtilitiesCS/OutlookObjects` who need refactoring safety for Outlook wrapper/helper code in folders such as `Folder`, `MailItem`, `Store`, `Recipient`, `Table`, and `Fields`.
	- Reviewers enforcing repository coverage and C# validation policy who need mirrored test placement, explicit exclusions, project-file wiring, and per-file coverage evidence.
	- Downstream planning/execution agents that need a clear batchable map of which file groups are seam-ready, which require minimal seam insertion, and which branches are blocked under current repo policy.
- Success metrics or expected impact:
	- Per-file line coverage reaches `>= 80%` for each testable file under `UtilitiesCS/OutlookObjects`, measured from coverage artifacts rather than package-wide averages.
	- New tests follow MSTest + Moq + FluentAssertions conventions and mirror the production folder layout under `UtilitiesCS.Test/OutlookObjects`.
	- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains explicit `<Compile Include=...>` entries for every new OutlookObjects test file.
	- Blocked branches are called out precisely enough that downstream implementation can either introduce a minimal seam or defer the branch with a documented policy reason.

## Behavior

The feature should add or expand deterministic unit tests so the current behavior of `UtilitiesCS/OutlookObjects` becomes executable, reviewable, and safer to change. Each implementation batch begins by selecting a production file or tightly related folder slice, creating the matching test file under `UtilitiesCS.Test/OutlookObjects/<same-subfolder>/`, wiring the file into `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, then adding positive-path, invalid-input, boundary, and error-handling coverage using Moq or minimal test subclasses where Outlook COM behavior is otherwise hard to isolate.

The work is intentionally batched by **testability class**, not alphabetically. The expected order is:

1. **Pure and seam-ready files** — comparers, DTOs, parser helpers, `MailResolution.cs`, `Com/ComType.cs`, `Calendar` helpers, `Item/OutlookItem.cs`, `Item/OutlookItemTry*.cs`, and `Filter DASL` logic.
2. **Existing-pattern wrapper files** — `Store/StoresWrapper.cs`, `Store/StoreWrapper.cs`, `Recipient/RecipientStatic.cs`, `Recipient/RecipientInfo.cs`, `Folder/FolderMinimalWrapper.cs`, folder comparer files, and safe branches of `Attachment/AttachmentSerializable.cs`.
3. **Reflection and lazy-wrapper files** — `Item/OutlookItemFlaggable*.cs`, `Item/OlItemPseudoInterface.cs`, `MailItem/EmailDetails*.cs`, `MailItem/MailItemHelper.cs`, and `AppointmentItem/MeetingItemHelper.cs`.
4. **High-risk seam-needed files** — `Folder/FolderWrapper .cs`, `Folder/FolderPredictor.cs`, `Fields/UserDefinedFields.cs`, `Conversation/ConversationHelper.cs`, `Table/OlTableExtensions.cs`, `Store/StoreWrapperController.cs`, `Category/CreateCategory.cs`, and `Explorer/ExplorerActions.cs`.

- Main user flow (happy path):
	- A maintainer chooses a target file or small cluster such as `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` plus `StoresWrapper.cs`.
	- They add or extend the mirrored test files such as `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs` and `StoresWrapperTests.cs`.
	- They cover property delegation, filter logic, null-guard behavior, comparer logic, parser output, DTO serialization behavior, or other deterministic branches using MSTest, Moq, and FluentAssertions.
	- They update `UtilitiesCS.Test/UtilitiesCS.Test.csproj` with an explicit `<Compile Include=...>` entry for each new test file.
	- They run the required C# validation loop and inspect `.trx` plus coverage output, including `coverage.cobertura.xml`, to confirm the target file meets the per-file threshold.
- Alternate/edge flows:
	- Pure logic, DTO, comparer, and parser files are tested directly without additional seams.
	- Wrapper files with existing `virtual` or `internal virtual` helper methods are exercised through minimal test subclasses rather than broad production refactors.
	- High-risk files such as `Folder/FolderPredictor.cs`, `Store/StoreWrapperController.cs`, `Conversation/ConversationHelper.cs`, `Table/OlTableExtensions.cs`, and `Folder/FolderWrapper .cs` may require narrowly scoped seams for dialog, filesystem, namespace, timeout/retry, or RCW-lifetime-adjacent branches.
	- Namespace mismatches do not change file-placement rules. Even when a production class lives in an unexpected namespace, its tests still belong in the mirrored folder that matches the production file path.
	- Branches that still depend on forbidden temp files, live Outlook state, or WinForms interaction are documented explicitly as blocked, not silently omitted.
- Error handling and recovery behavior:
	- Tests must verify null, empty, malformed, exception, and COM-failure scenarios where the production contract exposes them.
	- If coverage cannot be raised for a branch without violating repo policy, the work item records the exact reason, the exact blocked file/function or branch type, and the minimum seam that would unblock it.
	- Missing project-file includes, incorrect mirrored paths, or coverage shortfalls are treated as failures of the feature scope and must be corrected before the batch is considered complete.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars)
	- Authoritative feature inputs:
		- `docs/features/active/2026-03-14-outlook-objects-test-coverage-67/issue.md`
		- `docs/features/active/2026-03-14-outlook-objects-test-coverage-67/research.md`
		- `docs/features/active/2026-03-14-outlook-objects-test-coverage-67/spec.md`
		- `docs/features/active/2026-03-14-outlook-objects-test-coverage-67/user-story.md`
	- Production code inputs:
		- `UtilitiesCS/OutlookObjects/**/*.cs` across `AppointmentItem`, `Attachment`, `Calendar`, `Category`, `Com`, `Conversation`, `Explorer`, `Fields`, `Filter DASL`, `Folder`, `Folder/MsgToMime`, `Item`, `MailItem`, `Recipient`, `Store`, `Table`, plus root `MailResolution.cs`
	- Test project inputs:
		- `UtilitiesCS.Test/OutlookObjects/**/*.cs`
		- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
		- Existing OutlookObjects tests that already demonstrate accepted patterns, especially `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`, `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticTests.cs`, and the flat legacy OutlookObjects test files that may be migrated into mirrored folders over time
	- Validation inputs:
		- `TaskMaster.sln`
		- repo C# toolchain commands for format, analyzer build, nullable build, and `vstest.console.exe` coverage execution
	- Environment constraints:
		- Windows execution environment
		- .NET Framework `4.8.1`
		- no environment variables or new secrets required
- Outputs (artifacts, logs, telemetry)
	- New or expanded test files under `UtilitiesCS.Test/OutlookObjects/` with subfolders mirroring production folders.
	- Updated explicit `<Compile Include=...>` entries in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
	- Coverage and validation artifacts, including `coverage.cobertura.xml` or the repo’s emitted Cobertura-equivalent coverage artifact, and `.trx` test results from `vstest.console.exe`.
	- Batch-level evidence identifying which production files reached the threshold and which files or branches remain blocked.
	- Feature documentation updates in this active feature folder describing accepted scope, blocked branches, and completion criteria.
- Config keys and defaults:
	- No new runtime config keys, feature flags, or environment variables.
	- Existing test-framework packages and solution configuration remain the default baseline.
- Versioning or backward-compatibility constraints:
	- No production API contract changes are intended.
	- Any seam added for testability must preserve existing public behavior and remain compatible with the .NET Framework 4.8.1 solution.

## API / CLI Surface

This feature does not add a new end-user CLI or runtime API. Its operational surface is the repository’s existing C# validation toolchain and the test project structure that downstream implementation work must update consistently.

- Example invocations with expected outputs (concise):
	- `dotnet restore TaskMaster.sln`
		- Restores the solution before format/build/test steps.
	- `dotnet format TaskMaster.sln --verify-no-changes --no-restore`
		- Confirms the edited C# and project files are formatter-clean after active editing uses `dotnet format TaskMaster.sln`.
	- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
		- Produces an analyzer-enforced build.
	- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
		- Produces a nullable-analysis build with warnings treated as errors.
	- `vstest.console.exe <discovered *.Test.dll assemblies> /EnableCodeCoverage /InIsolation /Logger:trx`
		- Runs test assemblies and emits coverage plus `.trx` output used to verify per-file coverage.
- Contracts and validation rules:
	- Each new test file must live in the mirrored `UtilitiesCS.Test/OutlookObjects/<folder>/` location and must be added explicitly to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
	- Tests must use MSTest attributes, Moq mocks/stubs, and FluentAssertions for new assertions unless a specific assertion shape requires MSTest `Assert`.
	- No unit test may rely on live Outlook COM, live UI rendering, network calls, mutable external state, or temporary-file creation.
	- Coverage success is judged per file, not by solution-wide aggregate percentages.
	- Any plan or implementation artifact must reference the exact file path when a path hazard exists, especially `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs` with the embedded space before `.cs`.

## Data & State

This feature is test- and evidence-focused; it does not introduce new production persistence or business-state changes. The main data flow is from production source files to matching test files and then into generated validation artifacts that demonstrate whether the coverage target was reached.

- Data transformations and invariants:
	- Production behavior in `UtilitiesCS/OutlookObjects` is translated into executable expectations in matching `UtilitiesCS.Test/OutlookObjects` files.
	- Coverage tooling maps test execution back to production line hits in `coverage.cobertura.xml`; the invariant is that every in-scope file is either above the 80% target or explicitly called out as blocked/excluded with justification.
	- The mirrored directory layout remains consistent between production and test folders even when namespaces differ from folder names.
	- The compile-item list in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` remains an authoritative part of test state because a missing include means a test file silently does not participate in builds or coverage.
- Caching or persistence details:
	- No application cache or runtime persistence is introduced.
	- Only test result and coverage artifacts are persisted as build outputs/evidence.
	- Serialization-focused wrappers may require state-primed objects or JSON round-trip style assertions, but those tests must remain in-memory and deterministic.
- Migration or backfill requirements (if any):
	- No schema or data migration is required.
	- Existing flat OutlookObjects tests may be moved or expanded into mirrored subfolders as part of implementation when the compile-include list is updated in the same batch and no file is left orphaned from the project file.

## Constraints & Risks

The module is heavily Outlook-interop oriented, so the main implementation risk is not writing test syntax but reaching the required coverage without violating repo test policy or destabilizing production code. The research identified both seam-ready files and several hotspot files whose remaining branches are coupled to COM lifetime management, dialogs, filesystem access, or large orchestration methods.

- Limits (latency/throughput/memory) and acceptable trade-offs:
	- Tests should remain fast, isolated, and deterministic even if that means emphasizing pure/helper branches before expensive orchestration-heavy branches.
	- Small, local seams are acceptable when they unlock meaningful branch coverage; a large adapter rewrite across all Outlook types is not.
	- Each test file must stay under the repo’s 500-line file limit, so broad surfaces such as `OlTableExtensions` or `MailItemHelper` may need multiple focused test files.
- Security/privacy considerations:
	- Tests must not access real Outlook profiles, mailboxes, recipient data, or local temp-file content.
	- Any fixtures or assertions must use synthetic in-memory data only.
- Operational/rollout risks and mitigations:
	- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` uses explicit compile includes; mitigation is to treat each new test file and project entry as a paired change.
	- `.NET Framework 4.8.1` limits newer test utilities and can make async/serialization behavior more brittle; mitigation is to prefer existing repo patterns and conservative seams.
	- Some files have repo-specific hazards, including `Folder/FolderWrapper .cs` with a literal space before `.cs`; mitigation is to reference exact paths in coverage filters, compile includes, and plan tasks.
	- Temp-file branches in `Attachment/AttachmentSerializable.cs` cannot be fully unit tested under current policy because temporary-file creation is explicitly forbidden; mitigation is to document those branches as blocked until a narrow file-system seam is introduced or policy changes.
	- UI/dialog branches in `Store/StoreWrapperController.cs`, `Folder/FolderPredictor.cs`, and dialog-dependent portions of `Folder/FolderConverter.cs` cannot be covered honestly without a narrow dialog/picker seam; mitigation is to test only the logic already behind existing interfaces or helper methods and to document the remaining branches precisely.
	- COM-lifetime branches around `Marshal.ReleaseComObject` or `Marshal.FinalReleaseComObject`, especially in `Folder/FolderWrapper .cs`, should not be over-specified by unit tests; mitigation is to prioritize deterministic behavioral coverage over exact RCW-release mechanics.
	- `Conversation/ConversationHelper.cs`, `Fields/UserDefinedFields.cs`, and `Table/OlTableExtensions.cs` combine data shaping with namespace resolution, retry logic, or property-accessor calls; mitigation is to split batches so pure transforms are covered before any seam insertion work.

**Currently blocked or only partially unblockable under present repo policy:**

- `Attachment/AttachmentSerializable.cs` branches that require saving/loading temp files are blocked until a seam or policy exception exists.
- Direct WinForms dialog interaction branches in `Store/StoreWrapperController.cs`, `Folder/FolderPredictor.cs`, and user-prompt portions of `Folder/FolderConverter.cs` are blocked until injectable dialog abstractions exist.
- Live Outlook namespace/profile resolution paths that require real `Globals.Ol.NamespaceMAPI`, real `GetItemFromID`, or live `MAPIFolder` graphs are only unit-testable where existing virtual methods or interfaces permit isolation.
- These blocked areas are still in feature scope for documentation and planning, but not automatically in scope for immediate full branch coverage without targeted production seam work.

## Implementation Strategy

- Implementation scope (what changes, not sequencing):
	- Expand `UtilitiesCS.Test/OutlookObjects` so it mirrors the production folder layout and covers the current behavior of the 15 verified OutlookObjects subdirectories plus root `MailResolution.cs`.
	- Focus coverage on public methods, public properties with logic, static helper methods, comparers, DTO serialization hooks, reflection helpers, wrapper initialization/state behavior, and error paths that can be isolated with Moq or minimal subclass seams.
	- Keep production changes minimal and localized to otherwise untestable branches, especially in `Folder`, `Table`, `Conversation`, `Fields`, `Store`, and `MailItem` hotspots.
	- Treat folder/class-type boundaries as part of scope control: DTO/comparer/parser files should be batched separately from namespace-heavy Outlook orchestration files so downstream atomic tasks remain small and verifiable.
- New classes/functions/commands to add or update:
	- Add or expand test files such as:
		- `UtilitiesCS.Test/OutlookObjects/Attachment/AttachmentSerializableTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Item/OutlookItemTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Item/OutlookItemTryTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Item/OutlookItemFlaggableTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/MailItem/EmailDetailsTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientInfoTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Fields/UserDefinedFieldsTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelperTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTests.cs`
		- `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`
	- Update `UtilitiesCS.Test/UtilitiesCS.Test.csproj` for every new test file.
	- Where coverage remains blocked, add only narrow seams around dialog selection, filesystem calls, namespace/item resolution, retry-loop helpers, or RCW-adjacent behavior rather than re-architecting the Outlook abstraction layer.
	- Downstream atomic planning should batch work roughly by testability:
		1. Pure/seam-ready files (`Filter DASL`, DTOs, comparers, `MailResolution.cs`, `OutlookItem` helpers).
		2. Existing-pattern wrapper files (`StoreWrapper`, `StoresWrapper`, `RecipientStatic`, `FolderMinimalWrapper`).
		3. Reflection/lazy wrapper files (`MailItemHelper`, `MeetingItemHelper`, `OutlookItemFlaggable*`).
		4. High-risk seam-needed files (`FolderPredictor`, `StoreWrapperController`, `ConversationHelper`, `OlTableExtensions`, `UserDefinedFields`, `FolderWrapper .cs`).
	- Likely file-to-test mappings should be treated as the default planning map unless later code inspection proves a better grouping:
		- `Attachment/AttachmentSerializable.cs` -> `UtilitiesCS.Test/OutlookObjects/Attachment/AttachmentSerializableTests.cs`
		- `Folder/FolderMinimalWrapper.cs`, folder comparers, `FolderConverter.cs`, `FolderWrapper .cs` -> `UtilitiesCS.Test/OutlookObjects/Folder/*Tests.cs`
		- `Item/OutlookItem.cs`, `OutlookItemTry.cs`, `OutlookItemTryGet.cs`, `OutlookItemFlaggable*.cs`, `OlItemPseudoInterface.cs` -> `UtilitiesCS.Test/OutlookObjects/Item/*Tests.cs`
		- `MailItem/MailItemHelper.cs`, `EmailDetails.cs`, `EmailDetailsWrapper.cs`, `MailResolution.cs` -> `UtilitiesCS.Test/OutlookObjects/MailItem/*Tests.cs`
		- `Recipient/RecipientStatic.cs`, `RecipientInfo.cs` -> `UtilitiesCS.Test/OutlookObjects/Recipient/*Tests.cs`
		- `Store/StoresWrapper.cs`, `StoreWrapper.cs`, `StoreWrapperController.cs` -> `UtilitiesCS.Test/OutlookObjects/Store/*Tests.cs`
		- `Conversation/ConversationHelper.cs` -> `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelperTests.cs`
		- `Table/OlTableExtensions.cs`, `OlToDoTable.cs` -> `UtilitiesCS.Test/OutlookObjects/Table/*Tests.cs`
		- `Fields/UserDefinedFields.cs`, `MAPIFields.cs` -> `UtilitiesCS.Test/OutlookObjects/Fields/*Tests.cs`
- Dependency changes (new/removed packages) and rationale:
	- No dependency changes are expected.
	- Existing MSTest, Moq, FluentAssertions, Newtonsoft.Json, and Outlook interop references are sufficient.
- Logging/telemetry additions and locations:
	- No new production telemetry is required for the feature itself.
	- If targeted seam work needs diagnostics during implementation, use existing project logging patterns rather than console output, and keep such additions narrowly scoped to the affected production file.
- Rollout plan (feature flags, staged deploys, fallback path):
	- No feature flag or runtime rollout is required because this is test/evidence work.
	- Delivery should occur in small reviewed batches so coverage gains and blocked branches are visible after each batch.
	- Fallback for an individual hotspot file is to stop at the highest safe deterministic coverage level, document the remaining blockers, and queue a follow-up seam task rather than widening scope silently.

## Definition of Done

- [ ] Acceptance criteria are mapped to concrete test suites under `UtilitiesCS.Test/OutlookObjects`, including mirrored folder placement, explicit project-file includes, and the intended production-file coverage target for each batch.
- [ ] Scope boundaries are explicit by folder and class type: pure helpers/DTOs/comparers, wrapper files, reflection/lazy-wrapper files, and high-risk seam-needed files are distinguished clearly enough for downstream atomic planning.
- [ ] Behavior matches acceptance criteria in the documented environment: Windows, `.NET Framework 4.8.1`, MSTest + Moq + FluentAssertions, no live Outlook/WinForms/temp-file dependencies.
- [ ] Tests are updated or added for each completed batch, with named suites covering the verified file-to-test mappings from the research-backed planning map.
- [ ] `UtilitiesCS.Test/UtilitiesCS.Test.csproj` is updated for every new or moved OutlookObjects test file, and no mirrored test file is left uncompiled.
- [ ] Edge cases and error handling are covered by tests for null, empty, malformed, exception, and boundary conditions where the production contract exposes them.
- [ ] Blocked branches are listed explicitly by file and branch type, including temp-file, dialog/UI, live namespace/profile, and RCW-lifetime hazards, with the minimum unblock seam or policy dependency noted.
- [ ] Repo-specific hazards are accounted for in the execution plan and evidence, including explicit compile includes and the exact `FolderWrapper .cs` path with its literal pre-extension space.
- [ ] Docs are updated in `docs/features/active/2026-03-14-outlook-objects-test-coverage-67/` so downstream planning can trace intended scope, batching, risks, evidence requirements, and blocked branches without additional research.
- [ ] Telemetry/logging is added or updated only where a minimal seam requires it, with no ad-hoc console logging introduced.
- [ ] A full C# toolchain pass is completed with evidence from `dotnet format`, analyzer build, nullable build, and `vstest.console.exe /EnableCodeCoverage /InIsolation /Logger:trx`, plus per-file verification from `coverage.cobertura.xml` or the emitted Cobertura-equivalent coverage artifact.
