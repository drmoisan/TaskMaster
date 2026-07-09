# Remediation Atomic Plan — Issue #292 Cycle 1 (Blocking CI test-isolation regression)

- **Canonical issue number:** 292
- **Feature folder:** `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/`
- **Plan path (update in place):** `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/remediation-plan.2026-07-09T16-05.md`
- **Remediation inputs (authority):** `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/remediation-inputs.2026-07-09T16-05.md`
- **Cycle timestamp:** 2026-07-09T16-05
- **Work Mode:** full-bug (remediation of a Blocking CI defect; production behavior unchanged, test-isolation only)
- **Language in scope:** C# (test files only)
- **Evidence root (canonical, non-overridable):** `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/evidence/`

## Objective

Make the entire `*.Test.dll` suite pass deterministically under the CI invocation
`vstest.console.exe <all *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
(`.github/workflows/ci.yml:127-140`) by removing the process-global-static test-isolation race that this PR introduced,
without changing production `CurrentStoreContext`, without weakening reader assertions, without removing the
enumeration-phase attribution scope, and with no sleeps/retries/timing hacks and no coverage regression.

## Root Cause (confirmed from source)

- `CurrentStoreContext` is a process-global `static volatile string _current`
  (`UtilitiesCS/Threading/CurrentStoreContext.cs:32`). It is deliberately NOT `AsyncLocal`/`ThreadStatic`: the #260
  watchdog reads `Current` from an independent background thread, which requires the process-global static
  (research §1.2/§3.1). This must not change.
- This PR added scope-opening writers in `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`:
  - `MaterializeFilteredStores()` opens `CurrentStoreContext.Begin(CurrentStoreContext.StoresEnumerationPhaseIdentity)`
    (`"<Stores-enumeration>"`) at line 181, called by `Init()` (line 44) and `RewireOlObjectsAsync()` (line 89).
  - `AddOrRestoreStore()` opens `CurrentStoreContext.Begin(storeDisplayName)` at line 146.
  - `StoreWrapper.Init()` opens `CurrentStoreContext.Begin(DisplayName)` at `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:47`.
- `UtilitiesCS.Test` runs `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]`
  (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`). Reader classes `CurrentStoreContextTests` and
  `ThreadMonitorTests` are already `[DoNotParallelize]` and assert `CurrentStoreContext.Current == null`. Because the
  MSTest non-parallel bucket runs concurrently with the parallel bucket, the `[DoNotParallelize]` readers observe
  `_current == "<Stores-enumeration>"` written by store test classes still in the parallel bucket. This is a real,
  recurring race (10 failures at CI run 29046195330), not a one-off flake.

## Selected Approach and Justification (evaluated options)

Three provably-correct options were evaluated:

- **(A) Move every scope-opening test class into the serialized non-parallel bucket** by adding `[DoNotParallelize]`
  to each `UtilitiesCS.Test` class that (transitively) opens a `CurrentStoreContext` scope. MSTest guarantees that all
  `[DoNotParallelize]` classes run sequentially and never concurrently with each other; the readers are already in that
  bucket. Once every writer is also in that bucket, no reader can overlap any writer, and the remaining parallel bucket
  contains no `CurrentStoreContext` writer. This is a structural guarantee (mutual exclusion), not a probability
  reduction, and it is not a timing hack.
- **(B) Remove/disable `[assembly: Parallelize]` for `UtilitiesCS.Test`.** Provably correct and single-file, but it
  serializes the entire (largest) test assembly, a broad wall-clock regression across thousands of unrelated tests.
- **(C) Shared explicit lock acquired by every reader and writer test body.** Also provably correct but requires
  editing every writer test body (larger surface than (A)) and adds a hand-maintained synchronization primitive.

**Selected: (A).** It provably removes the overlap while remaining surgical — only the small set of
`CurrentStoreContext`-touching classes is serialized, preserving parallel throughput for the majority of the assembly.
(A)'s only correctness dependency is completeness of the writer-class enumeration; the plan closes that gap with an
explicit assembly-wide census (Phase 1) and a completeness-verification gate (Phase 2) that fails if any scope-opening
class remains unmarked. Residual durability risk (a future store test class added without the attribute) is recorded as
follow-up in the decision evidence; it is out of scope for this remediation.

## Hard Constraints (must hold for every task)

- Do NOT modify `UtilitiesCS/Threading/CurrentStoreContext.cs` or convert it to `AsyncLocal`/`ThreadStatic`.
- Do NOT modify or weaken the reader assertions in `CurrentStoreContextTests` or `ThreadMonitorTests`.
- Do NOT remove or weaken the enumeration-phase scope in `StoresWrapper.MaterializeFilteredStores()` or any other
  production scope-opening site.
- No sleeps, retries, `Thread.Sleep`, `Task.Delay`, or timing hacks. No temp files. No live Outlook.
- Changes are limited to adding `[DoNotParallelize]` attributes to existing `UtilitiesCS.Test` test classes. No new
  files (so no `packages.config`/`<Compile Include>` csproj wiring is required). No production `*.cs`/`*.csproj` change.
- Evidence paths resolve only to `<FEATURE>/evidence/<kind>/`. Any non-canonical path supplied downstream is rejected
  and replaced with the canonical path, recording `EVIDENCE_LOCATION_OVERRIDE_REJECTED`.

---

### Phase 0 — Context, Policy, and Baseline Capture

- [x] [P0-T1] Read the four core policies in required order (`CLAUDE.md`; `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`; `.claude/rules/csharp.md`) plus `remediation-inputs.2026-07-09T16-05.md`, and write `evidence/other/phase0-instructions-read.2026-07-09T16-05.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read. Acceptance: the artifact exists and lists all five files.
- [x] [P0-T2] Record the branch and commit baseline: run `git rev-parse --abbrev-ref HEAD` and `git rev-parse HEAD`, and write `evidence/remediation-baseline/branch-commit.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (branch name and full commit SHA; note the PR head under remediation is `9ae5c0e3952f9ff29febd825b8def21a1981caff`). Acceptance: artifact records the current branch and commit SHA.
- [x] [P0-T3] Capture the format baseline: run `dotnet tool run csharpier --check .` (or `csharpier --check .`) and write `evidence/remediation-baseline/format-baseline.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records the exit code and whether any file would be reformatted.
- [x] [P0-T4] Capture the analyzer baseline: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `evidence/remediation-baseline/analyzer-baseline.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (analyzer warning/error counts). Acceptance: artifact records build result and diagnostic counts.
- [x] [P0-T5] Capture the nullable/type-check baseline: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `evidence/remediation-baseline/nullable-baseline.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records the build result.
- [x] [P0-T6] Capture the test+coverage baseline on the current (pre-fix) state by running the CI-equivalent full assembly set: collect every `*.Test.dll` under `**/bin/Debug/**` (excluding `obj`/`ref`) and run `vstest.console.exe <all *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`. Write `evidence/remediation-baseline/test-coverage-baseline.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including total/passed/failed counts and the numeric repository-wide line-coverage headline percent. Acceptance: artifact records numeric coverage and the pass/fail totals (a non-zero failure count from the #292 race is expected here and is the pre-fix signal).

---

### Phase 1 — Root-Cause Confirmation and Writer-Class Census

- [x] [P1-T1] Confirm the authoritative fail-before evidence. Cite CI run 29046195330 (`Total tests: 5141 / Failed: 10`, all in `UtilitiesCS.Test`) and the P0-T6 local result. Write `evidence/regression-testing/fail-before.2026-07-09T16-05.md` with `Timestamp:`, the CI job URL, the enumerated 10 failing test names, and — if the P0-T6 local run did not reproduce the failure in a single pass — a `WhyFailingRunImpossible:`/nondeterminism note plus `SearchScope:`/`SearchPatterns:`/`SearchResult:` for the local repro attempts. Acceptance: artifact records the authoritative red evidence and the local repro outcome.
- [x] [P1-T2] Enumerate every production site that opens a `CurrentStoreContext` scope. Grep `UtilitiesCS/` for `CurrentStoreContext.Begin` and record each site (expected: `StoresWrapper.MaterializeFilteredStores` line 181, `StoresWrapper.AddOrRestoreStore` line 146, `StoreWrapper.Init` line 47). Write the site list into `evidence/other/scope-open-census.2026-07-09T16-05.md`. Acceptance: artifact lists every production `Begin` call site with file and line.
- [x] [P1-T3] Produce the authoritative writer-class census for `UtilitiesCS.Test`: grep the assembly for every `[TestClass]` that transitively opens a scope, i.e. any class that references `StoresWrapper` (constructing it or calling `Init`/`RewireOlObjectsAsync`/`AddOrRestoreStore`), `StoreWrapper` (calling `Init`/`Restore`), or `CurrentStoreContext.Begin` directly. Append to `evidence/other/scope-open-census.2026-07-09T16-05.md`: (a) the full list of scope-opening test classes with file paths, (b) which already carry `[DoNotParallelize]` (expected: `StoreWrapperInitClockTests`, plus readers `CurrentStoreContextTests`, `ThreadMonitorTests`), and (c) a note confirming no `CurrentStoreContext.Current` null-baseline reader class exists in any other `*.Test` assembly that shares a vstest host process (verify vstest `/InIsolation` process boundaries). Acceptance: artifact contains the complete writer-class list, the already-marked set, and the cross-assembly note.
- [x] [P1-T4] Record the approach decision. Write `evidence/other/approach-decision.2026-07-09T16-05.md` capturing the evaluation of options (A)/(B)/(C), the selection of (A) with the mutual-exclusion justification, and the residual durability risk (future store test class added without `[DoNotParallelize]`) noted as follow-up. Acceptance: artifact states the selected approach and justification.

---

### Phase 2 — Apply Deterministic Test Isolation

Each edit adds `[DoNotParallelize]` immediately above the target class declaration (below its existing `[TestClass]`), using the attribute already available via the file's `Microsoft.VisualStudio.TestTools.UnitTesting` import. For partial classes, the attribute is added to exactly ONE part (the part bearing `[TestClass]`) to avoid CS0579 duplicate-attribute. Per-batch cap: 5 file edits per batch, followed by one batch toolchain gate. Every edit must be confirmed present in the P1-T3 census; a file not on the census is not edited.

Batch 1:

- [x] [P2-T1] Add `[DoNotParallelize]` to class `StoresWrapperTests` in `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`. Acceptance: the attribute is present on the class and the file still compiles-shape (single attribute, correct placement).
- [x] [P2-T2] Add `[DoNotParallelize]` to class `StoresWrapperRehookTests` in `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperRehookTests.cs`. Acceptance: attribute present exactly once on the class.
- [x] [P2-T3] Add `[DoNotParallelize]` to class `StoresWrapperDisableTests` in `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs`. Acceptance: attribute present exactly once on the class.
- [x] [P2-T4] Add `[DoNotParallelize]` to class `StoreWrapperTests` in `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs`. Acceptance: attribute present exactly once on the class.
- [x] [P2-T5] Add `[DoNotParallelize]` to class `StoreWrapperViewerTests` in `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperViewerTests.cs`. Acceptance: attribute present exactly once on the class.
- [x] [P2-T6] Batch 1 toolchain gate in order: `csharpier .` -> `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` -> `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` -> `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`. If any step changes files or fails, fix and restart the gate from `csharpier .`. Write `evidence/qa-gates/isolation-batch1.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` per step. Acceptance: all four steps pass in a single final pass; artifact records the numeric UtilitiesCS.Test coverage headline.

Batch 2:

- [x] [P2-T7] Add `[DoNotParallelize]` to class `StoreWrapperInitProbeTests` in `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperInitProbeTests.cs`. Acceptance: attribute present exactly once on the class.
- [x] [P2-T8] Add `[DoNotParallelize]` to the partial class `StoreWrapperController_Tests` in `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs` (the part bearing `[TestClass]`), and confirm the attribute is NOT added to the other partial parts (`StoreWrapperController_Tests.Launch.cs`, `StoreWrapperController_Tests.ButtonAndPopulate.cs`). Acceptance: attribute present on exactly one partial part; no duplicate-attribute build error.
- [x] [P2-T9] Add `[DoNotParallelize]` to class `StoreWrapperControllerTests` in `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs`. Acceptance: attribute present exactly once on the class.
- [x] [P2-T10] If and only if the P1-T3 census confirms `StoreDisableServiceTests` transitively opens a scope, add `[DoNotParallelize]` to class `StoreDisableServiceTests` in `UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs`; otherwise record `N/A — census shows no scope-open path` with the census citation in the task evidence. Acceptance: either the attribute is present exactly once, or an explicit census-backed N/A note is recorded.
- [x] [P2-T11] Batch 2 toolchain gate in the same four-step order and restart rule as P2-T6, writing `evidence/qa-gates/isolation-batch2.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` per step. Acceptance: all four steps pass in a single final pass; artifact records the numeric UtilitiesCS.Test coverage headline.

Completeness and proof:

- [x] [P2-T12] Completeness-verification gate. Verify `StoreWrapperInitClockTests` (and the reader classes) already carry `[DoNotParallelize]` (no edit needed), then grep the whole `UtilitiesCS.Test` assembly to prove that every scope-opening `[TestClass]` from the P1-T3 census now carries `[DoNotParallelize]`. Write `evidence/other/completeness-verification.2026-07-09T16-05.md` listing each census writer class and its `[DoNotParallelize]` presence. If any scope-opening class remains unmarked, add the attribute (respecting the 5-per-batch cap and re-running the batch toolchain gate) until the grep shows zero unmarked scope-opening classes. Acceptance: artifact proves zero scope-opening test classes remain unmarked assembly-wide.
- [x] [P2-T13] Deterministic green proof (CI-equivalent). Run the full assembly set exactly as CI does: `vstest.console.exe <all *.Test.dll under bin/Debug> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`, and repeat the full-suite run 3 additional passes to confirm determinism (the pre-fix race was schedule-dependent, so a single pass is insufficient proof). Write `evidence/regression-testing/green-after-fix.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` for each of the passes (total/passed/failed and the previously-failing 10 test names now passing). Acceptance: all passes report `Failed: 0` and the 10 previously-failing tests pass in every pass. A note explains that completeness (P2-T12) plus MSTest serial-bucket mutual exclusion — not repeated-run luck — is the correctness guarantee; the repeated runs are confirmation only.

---

### Phase 3 — Final QA Loop and Coverage Verification

Run the full C# toolchain loop in order; if any step changes files or fails, restart from step 1 (`csharpier .`).

- [x] [P3-T1] Formatting: run `dotnet tool run csharpier .` (or `csharpier .`). Write `evidence/qa-gates/final-format.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: no files reformatted in the final pass.
- [x] [P3-T2] Linting/analyzers: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `evidence/qa-gates/final-analyzer.2026-07-09T16-05.md` with the schema fields. Acceptance: build succeeds with zero new analyzer errors versus the P0-T4 baseline.
- [x] [P3-T3] Type-check/nullable: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `evidence/qa-gates/final-nullable.2026-07-09T16-05.md` with the schema fields. Acceptance: build succeeds with no warnings-as-errors.
- [x] [P3-T4] Testing with coverage, CI-equivalent: run `vstest.console.exe <all *.Test.dll under bin/Debug> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`. Write `evidence/qa-gates/final-test-coverage.2026-07-09T16-05.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including total/passed/failed and the numeric repository-wide line-coverage headline percent. Acceptance: `Failed: 0` and numeric coverage recorded.
- [x] [P3-T5] Coverage delta verification. Compare P0-T6 baseline coverage to P3-T4 post-change coverage and report baseline percent, post-change percent, and changed-code coverage. Because the only changes are `[DoNotParallelize]` attributes on existing test classes (no production code, no new module), the expected changed-production-code coverage denominator is empty and repository-wide coverage must not regress and must remain `>= 80%` on the testable denominator. Write `evidence/qa-gates/coverage-delta.2026-07-09T16-05.md` with the three numeric values and the no-regression conclusion. Acceptance: no coverage regression and `>= 80%` testable-denominator coverage confirmed with numeric values (no placeholders).
- [x] [P3-T6] Issue-update mirror. Write `evidence/issue-updates/issue-292.2026-07-09T16-05.md` per the issue-update mirroring convention: `Timestamp:`, the exact remediation summary text (root cause, selected approach A, files changed, green-after-fix and coverage evidence references), and `PostedAs:` (`comment`/`body`/`unknown`). Acceptance: mirror artifact exists with the required fields.

---

## Preflight

DIRECTIVE: PREFLIGHT VALIDATION ONLY

Return one of: `PREFLIGHT: ALL CLEAR` or `PREFLIGHT: REVISIONS REQUIRED` (with a precise plan delta). The plan target path
`docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/remediation-plan.2026-07-09T16-05.md`
is reused for all revision iterations; no sibling timestamped plan files are created.
