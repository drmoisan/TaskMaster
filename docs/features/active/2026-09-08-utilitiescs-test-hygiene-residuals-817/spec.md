# 2026-09-08-utilitiescs-test-hygiene-residuals (Spec)

- **Issue:** #817
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09
- **Status:** Ready for Planning
- **Version:** 0.1

## Context
Standing residuals issue for low-severity hygiene findings in `UtilitiesCS.Test` that reviews surface
but that do not each warrant their own issue. Opened with one entry: the pre-existing 1067-line
`FolderPredictorTests.cs`, which is more than twice the repository's 500-line file cap.

Environment:
- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Python version: not applicable
- Command/flags used: file line counts taken during issue 809's `[P4-T5]` gate
- Data source or fixture: not applicable

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: a maintainability cost, with no behavioural defect. Recorded so it is not rediscovered by every
subsequent review of this assembly.


## Repro & Evidence
Steps to Reproduce:
1. Count the lines in `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`.
2. Compare against the 500-line cap in `.claude/rules/general-code-change.md`, "File Size Limit".

Expected:
No test file exceeds 500 lines. The cap has no test-code exemption; the listed exemptions are
throwaway agent scripts, raw text fixtures for language-processing test data, and Markdown.

Actual:
`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` is 1067 lines.

This is pre-existing and was not introduced by issue 809; that item's line-count gate simply recorded
it. Splitting the file along its existing behavioural groupings is the obvious remedy, but it touches
a file several in-flight items depend on, so it should be sequenced rather than done opportunistically.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: issue 809 `evidence/qa-gates/p4-t5-line-counts.md`.


## Scope & Non-Goals
- In scope:
  - Splitting `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` (1067 lines) into 5
    `partial class FolderPredictorTests` files along the existing behavioral groupings documented in
    the research artifact, each file at most 500 lines.
  - Adding one `<Compile Include>` entry per new file to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`,
    inserted immediately after the existing `FolderPredictorTests.cs` entry, with no reordering or
    reformatting of any other entry in the file.
  - Verifying, via a real test-discovery run, that the full set of 39 `[TestMethod]`s is preserved
    exactly (same fully-qualified names, unchanged bodies) after the split.
- Out of scope / non-goals:
  - Any new test scenarios or new coverage areas. Issue #817's "Proposed Fix / Validation Ideas"
    section is explicit: "none new."
  - Any change to `.editorconfig`, `.csharpierignore`, `BannedSymbols.txt`, `CLAUDE.md`, or any file
    under `.claude/rules/` or `.github/instructions/`. No proposal to raise the 500-line cap.
  - Lowering any coverage threshold, analyzer severity, or policy requirement.
- Explicitly excluded systems, integrations, or datasets:
  - No production code under `UtilitiesCS/` or any other assembly is touched; this change is
    confined to `UtilitiesCS.Test/OutlookObjects/Folder/` and
    `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
  - Files owned by sibling in-flight features sharing this assembly or csproj are explicitly out of
    scope:
    - `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (#813)
    - `QuickFiler/Controllers/QfcHomeController.cs`, `UtilitiesCS/Threading/ProgressViewer.cs`,
      `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` (#821)
    - `UtilitiesCS/OutlookObjects/Store/StoreWrapperController*.cs`,
      `QuickFiler/Viewers/Breadcrumb*` (#823)
    - `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/**` and its tests (#824)
    - `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.*`, `UtilitiesCS/Threading/TimeOutTask.cs`,
      `UtilitiesCS/Extensions/DfDeedle.cs` (#825)
    - Any `Console.SetOut` restore work (#826) — not applicable here since `FolderPredictorTests.cs`
      has no `Console.SetOut` call.

## Root Cause Analysis

`FolderPredictorTests.cs` grew past the repository's 500-line file cap over time, one
`[TestMethod]` at a time, with no automated size gate in place to catch the file crossing the
threshold. The cap in `.claude/rules/general-code-change.md` ("File Size Limit") carries no
test-code exemption — the only exemptions are throwaway agent scripts, raw text fixtures for
language-processing test data, and Markdown documentation — so the file has been out of policy
compliance since it first exceeded 500 lines, independent of any behavioral defect. Per issue #817,
this condition is pre-existing and was not introduced by issue #809; #809's `[P4-T5]` line-count
gate simply recorded the existing violation during an unrelated review pass.

This entry is intended as the standing residuals record for the `UtilitiesCS.Test` subsystem. Append
further low-severity findings here as checklist items rather than opening a new issue for each, per
the 2026-09-07 ruling that residuals be batched by blast radius and standalone issues reserved for
Medium severity or higher.

Sequencing note: issue #811 (`utilitiescs-test-determinism-780-803-594-811`) was scheduled on run
`bugs-2026-09-06` and touched this assembly, including `FolderPredictorTests.cs`, by way of the
shared `UtilitiesCS.Test.csproj`. That item has since merged (visible in the current branch's recent
history as the "utilitiescs-test-determinism-780-803-594-811" merge commit), so the sequencing
precondition in issue #817 is satisfied and this split may proceed.


## Proposed Fix

### Design summary (what changes where):

Split `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` into 5 files using the C#
`partial class` pattern already established elsewhere in this repository (live precedents:
`StoreWrapperController_Tests.cs` + 4 siblings, `AppEventsTests.cs` + `AppEventsTests.Helpers.cs`,
`OutlookFolderTreeServiceTraversalCancellationTests.cs` + `.Coverage.cs`,
`MeetingItemHelperTests.cs` + `.Part2.cs`). All 5 files declare the identical type name
`partial class FolderPredictorTests` in the identical namespace
(`UtilitiesCS.Test.OutlookObjects.Folder`); the compiler merges them into one type before MSTest's
reflection-based discovery runs, so class-level attributes and `private`/`private static` members
declared in any one file are visible from, and apply to, all other files. Only file names differ; no
test is renamed.

Concrete file list, per the research artifact's §2 line-range analysis of the current 1067-line
file:

1. `FolderPredictorTests.cs` (primary — retains `[TestClass]`, `[DoNotParallelize]`, and the
   `using` block from the original header, lines 1-21). Behavior group: construction,
   initialization, and array-seeding (14 test methods, original source lines 22-165). Estimated
   ~167 lines after the split.
2. `FolderPredictorTests.SuggestionsAndRecents.cs` (partial, no class attributes). Behavior group:
   suggestions, `FolderArray` projection, and recents (5 test methods, original source lines
   166-286). Estimated ~140 lines.
3. `FolderPredictorTests.FolderLookupAndUiSeams.cs` (partial, no class attributes). Behavior group:
   folder search/lookup, matching, and prompt/UI seam hooks (11 test methods, original source lines
   287-595, kept as one contiguous slice to avoid reordering method bodies). Estimated ~328 lines.
4. `FolderPredictorTests.CreateFolderWorkflows.cs` (partial, no class attributes). Behavior group:
   folder creation, directory-path creation, and folder-name prompting (9 test methods, original
   source lines 596-920). Estimated ~344 lines.
5. `FolderPredictorTests.TestSupport.cs` (partial, no class attributes, no `[TestMethod]`s).
   Contains all 5 shared private static helper methods (`CreateApplication`, `CreateFolder`,
   `CreateFoldersCollection`, `CreateGlobals`, `GetLeafName`, original source lines 922-1011) and
   both nested private classes (`TestableFolderPredictor`, original lines 1013-1057;
   `ImmediateSynchronizationContext`, original lines 1059-1065). This mirrors the
   `AppEventsTests.Helpers.cs` precedent exactly. Estimated ~163 lines.

Every file stays well under the 500-line cap even with a full duplicated `using` block and
namespace/class open+close in each file (~19 lines of unavoidable per-file overhead). The 5-file
total (~1142 lines) is expected to exceed the original 1067 lines for this reason, consistent with
every existing split example in the repo (e.g. the `StoreWrapperController_Tests` family).

### Boundaries and invariants to preserve:

- The type name `FolderPredictorTests` is identical, verbatim, across all 5 files. Only file names
  differ.
- Only the primary file (`FolderPredictorTests.cs`) carries the `[TestClass]` and
  `[DoNotParallelize]` attributes. No other partial file repeats these attributes.
- All 39 `[TestMethod]`s are preserved with unchanged bodies — this is a pure structural
  (cut-and-paste) split, not a behavioral change. No test method is renamed, reordered within its
  file, merged, or split.
- Both nested helper classes (`TestableFolderPredictor`, `ImmediateSynchronizationContext`) and all
  5 shared private static helper methods (`CreateApplication`, `CreateFolder`,
  `CreateFoldersCollection`, `CreateGlobals`, `GetLeafName`) are preserved exactly, relocated in full
  to `FolderPredictorTests.TestSupport.cs`. Because they remain `private`/`private static` members of
  the same merged partial type, no visibility changes (e.g. to `internal` or `protected`) are needed
  for any of the other 4 files to call them.
- No new test scenarios or coverage areas are introduced (issue #817's own validation notes: "none
  new").

### Dependencies or blocked work:

- Issue #811 (`utilitiescs-test-determinism-780-803-594-811`) touched this same assembly by way of
  the shared `UtilitiesCS.Test.csproj` and has since merged; this split is unblocked.
- Sibling features #813, #821, #823, #824, #825, and #826 may concurrently modify
  `UtilitiesCS.Test.csproj` or adjacent files in the same assembly. The csproj insertion rule below
  (insert-after, no reordering) exists specifically to keep this feature's diff to that file a pure
  line-insertion, minimizing fan-in merge conflicts with those siblings.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` (retained, reduced to the primary
  file's content described above).
- New: `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.SuggestionsAndRecents.cs`
- New: `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.FolderLookupAndUiSeams.cs`
- New: `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.CreateFolderWorkflows.cs`
- New: `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.TestSupport.cs`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — 4 new `<Compile Include>` lines only.

#### Functions/classes/CLI commands impacted:

- `partial class FolderPredictorTests` (merged across all 5 files; no public surface change — this
  is a test class, not a production API).
- `private sealed class TestableFolderPredictor`, `private sealed class
  ImmediateSynchronizationContext` (relocated, unchanged).
- No CLI commands or production classes are impacted.

#### Data flow and validation changes:

Not applicable — no data flow, validation, or business logic changes. This is a file-structure-only
reorganization of test source.

#### Error handling and logging updates:

Not applicable — no error handling or logging behavior changes.

#### Rollback/feature-flag considerations (if applicable):

No feature flag is needed. Rollback is a straightforward `git revert` of the split commit(s);
because the split is behaviorally inert (same merged type, same test bodies, same csproj-listed
files), a revert restores the single 1067-line file with no other side effects.

### Technical specifications (interfaces/contracts):

Insert exactly 4 new `<Compile Include>` lines into `UtilitiesCS.Test/UtilitiesCS.Test.csproj`,
immediately after the existing `<Compile Include="OutlookObjects\Folder\FolderPredictorTests.cs" />`
entry (currently line 411, between `FolderPredictorCoverageExpansionTests.cs` and
`FolderRowTests.cs`), in the same order the files are introduced above. Do not reorder, re-sort, or
reformat any other `<Compile Include>` entry in the file — the project's ~470 entries are not
alphabetically sorted and must stay in their existing append order so that sibling features #813,
#821, #823, #824, #825, and #826, which may also touch this csproj concurrently, can fan-in against
a pure line-insertion diff.

#### Inputs/outputs and formats:

Not applicable — no I/O contract changes. Test method signatures, `[TestMethod]` attributes, and
assertions are unchanged.

#### Required configuration keys and defaults:

Not applicable.

#### Backward-compatibility expectations:

The merged `FolderPredictorTests` type's test-visible surface (its `[TestMethod]`s) is unchanged;
any tooling that references tests by fully-qualified name
(`UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.<MethodName>`) continues to resolve
identically after the split, because file name is not part of a test's fully-qualified name.

#### Performance constraints (latency/throughput/memory):

Not applicable — no runtime behavior changes; test execution time is expected to be unchanged.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - The working tree's `FolderPredictorTests.cs` matches the 1067-line, 39-`[TestMethod]` state
    documented in the research artifact (established there by a full file read at research time).
  - `vstest.console.exe` is resolvable via `vswhere` in the build/verification environment, per the
    pattern already used elsewhere in this repo.
- Constraints (budget, performance, compatibility):
  - Every resulting file must be at most 500 lines (`.claude/rules/general-code-change.md`, "File
    Size Limit"), with no exception requested or granted for test code.
  - The `UtilitiesCS.Test.csproj` diff must be a pure insertion (no reordering) to avoid conflicting
    with the 5 concurrently in-flight sibling features sharing the same file.
- External dependencies (services, libraries, releases):
  - No new external dependency is introduced. MSTest, Moq, and FluentAssertions usage is unchanged —
    this split touches no assertion or mocking code, only file boundaries.

## Data / API / Config Impact
Not applicable — pure test-file structural split, no API/data/config surface changes.

## Test Strategy
Seeded from issue:

- [ ] Unit coverage areas: none new; a split must preserve the existing test set exactly, verified by
      comparing test counts and names before and after.
- [ ] Integration scenario to retest: a full `UtilitiesCS.Test` run with unchanged pass count.
- [ ] Manual verification notes: confirm every resulting file is under 500 lines and that
      `UtilitiesCS.Test.csproj` lists each new file.

This is a structural-only change; issue #817's own "Proposed Fix / Validation Ideas" section states
the unit coverage areas are "none new." The verification burden is entirely regression-proof: confirm
that nothing was lost, renamed, or altered during the split.

Non-vacuous test-count verification (per the research artifact's §4, following the methodology
already proven in issue #230's `compile-include-wiring.2026-08-07T23-40.md`):

1. **Static baseline (pre-split)**: enumerate every `[TestMethod]`-preceded method name in the
   original `FolderPredictorTests.cs` by direct source read. The research artifact's full read
   already establishes this at 39 methods (exact start lines listed in its §2).
2. **Dynamic baseline (pre-split)**: full rebuild of `UtilitiesCS.Test.csproj`
   (`MSBuild.exe UtilitiesCS.Test/UtilitiesCS.Test.csproj -t:Rebuild -p:Configuration=Debug
   -p:Platform="AnyCPU" -v:m` — a full `/t:Rebuild`, not an incremental `/t:Build`, so the discovered
   assembly reflects the current `<Compile>` set exactly), then run
   `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /ListTests` and filter the
   output to lines ending in `.FolderPredictorTests.<MethodName>`. This must also show 39 tests.
3. Perform the split.
4. **Static count (post-split)**: re-enumerate every `[TestMethod]`-preceded method name, this time
   summed across all 5 files. Must still be 39.
5. **Dynamic count (post-split)**: repeat the full rebuild and `/ListTests` filtered discovery. Must
   still show 39 `FolderPredictorTests.<MethodName>` entries, with the same set of fully-qualified
   names as step 2.
6. Compare all four results: static-pre == dynamic-pre == static-post == dynamic-post == 39, and the
   member sets (method names) match exactly between pre- and post-split on both the static and
   dynamic sides. A source grep alone (steps 1 and 4, without steps 2 and 5) is **not sufficient** —
   only a real `vstest.console.exe /ListTests` discovery run against the rebuilt assembly proves the
   `<Compile Include>` wiring is correct and every file is actually compiled into the test binary.

**Known trap to avoid** (research artifact §1/§4, confirmed instance from issue #498): any
`/TestCaseFilter:FullyQualifiedName~<X>` used elsewhere in the implementation plan must filter on the
**type name** `FolderPredictorTests`, never on any of the new **file names**
(`SuggestionsAndRecents`, `FolderLookupAndUiSeams`, `CreateFolderWorkflows`, `TestSupport`). A
partial-class split means file name is not the same as type name; a filter built from a file name
matches zero tests and exits `0` with "No test matches," silently masking a broken split.

- Regression tests to add or update: none — no new test methods; all 39 existing methods relocated
  verbatim.
- Unit tests (pytest) for the fixed behavior and boundaries: not applicable — this is a C# repository
  using MSTest, and no test bodies change.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): not applicable —
  no logic changes; existing edge-case coverage in the 39 tests is preserved unchanged.
- Error handling and logging verification: not applicable — no production error-handling or logging
  code is touched.
- Coverage impact and targets for changed lines/modules: not applicable — test files are excluded
  from the coverage denominator by policy (`.claude/rules/general-unit-test.md`, "Configure coverage
  tooling to exclude test files"); the COM/VSTO/WinForms coverage exemption does not apply here since
  it governs production code, not test files.
- Toolchain commands to run (format → lint → type-check → test):
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
     /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
     /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage` (full
     `UtilitiesCS.Test` run; pass count must be unchanged from the pre-split baseline)
- Manual validation steps (if required): confirm the line count of each of the 5 resulting files
  directly (e.g. `(Get-Content <file>).Count`), each at most 500.


## Acceptance Criteria
- [ ] `FolderPredictorTests.cs` and all 4 new sibling files
      (`FolderPredictorTests.SuggestionsAndRecents.cs`,
      `FolderPredictorTests.FolderLookupAndUiSeams.cs`,
      `FolderPredictorTests.CreateFolderWorkflows.cs`, `FolderPredictorTests.TestSupport.cs`) are
      each at most 500 lines.
- [ ] All 39 original `[TestMethod]`s are preserved with unchanged method bodies, verified by both a
      static source enumeration and a real `vstest.console.exe /ListTests` discovery run against a
      full rebuild of `UtilitiesCS.Test.dll`, showing exactly 39 `FolderPredictorTests.*` tests
      before the split and exactly 39 `FolderPredictorTests.*` tests after the split, with identical
      fully-qualified method names on both sides. A source grep alone, without the discovery run, is
      not sufficient evidence.
- [x] The type name `FolderPredictorTests` is identical, verbatim, across all 5 files, and only
      `FolderPredictorTests.cs` carries the `[TestClass]` and `[DoNotParallelize]` attributes.
- [x] Both nested helper classes (`TestableFolderPredictor`, `ImmediateSynchronizationContext`) and
      all 5 shared private static helper methods (`CreateApplication`, `CreateFolder`,
      `CreateFoldersCollection`, `CreateGlobals`, `GetLeafName`) are preserved exactly, relocated to
      `FolderPredictorTests.TestSupport.cs`, with no visibility changes required for other split
      files to reference them.
- [ ] `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains exactly one new `<Compile Include>` entry
      per new file (4 total), inserted immediately after the existing
      `<Compile Include="OutlookObjects\Folder\FolderPredictorTests.cs" />` entry, with no
      reordering or reformatting of any other `<Compile Include>` entry in the file.
- [ ] A full `UtilitiesCS.Test` test run (`vstest.console.exe ... /EnableCodeCoverage`) passes with a
      pass count unchanged from the pre-split baseline.
- [ ] The full C# toolchain — CSharpier format/check, the analyzer rebuild
      (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), and the nullable rebuild
      (`/p:TreatWarningsAsErrors=true`) — passes cleanly on all 5 changed/new `.cs` files and the
      modified `.csproj`.
- [ ] No new test scenarios, coverage areas, or behavioral changes are introduced; every one of the
      39 test bodies is unchanged aside from its relocation to a new file.

## Risks & Mitigations
- Technical or operational risks:
  - Fan-in conflicts with sibling features #813, #821, #823, #824, #825, #826, which touch the same
    `UtilitiesCS.Test.csproj`, if the insertion is not a pure line-insertion.
  - A `/TestCaseFilter` written against a new file name instead of the `FolderPredictorTests` type
    name could silently match zero tests and mask a broken split (confirmed prior incident, #498).
  - An incremental `/t:Build` (instead of `/t:Rebuild`) during verification could return a stale
    discovery result that does not reflect the new `<Compile>` entries.
- Mitigations and rollbacks:
  - Insert the 4 new `<Compile Include>` lines immediately after the existing entry with no other
    edits to the csproj, keeping the diff to a minimal, reviewable insertion.
  - Always filter `/ListTests` output by the type name `FolderPredictorTests`, never by a file name.
  - Always use `/t:Rebuild`, never `/t:Build`, for both the analyzer/nullable gates and the
    pre-verification build.
  - Rollback is a straightforward revert of the split commit(s); the change is behaviorally inert.

## Rollout & Follow-up
- Release/rollout steps: standard PR merge; no deployment, feature flag, or migration is involved.
- Post-fix monitoring or clean-up tasks: none anticipated; if the `FolderLookupAndUiSeams` group
  later needs finer separation, the research artifact notes it can be split again without touching
  any other file.
- Links: issue #817 (https://github.com/drmoisan/TaskMaster/issues/817); research artifact at
  `docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/research/research-2026-09-08T23-58.md`.
