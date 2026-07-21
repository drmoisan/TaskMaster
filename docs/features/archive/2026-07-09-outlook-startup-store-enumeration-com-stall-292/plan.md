# outlook-startup-store-enumeration-com-stall (Atomic Plan)

- **Issue:** #292
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/292
- **Work Mode:** full-bug
- **Owner:** drmoisan
- **Last Updated:** 2026-07-09
- **Status:** Ready for preflight
- **Authoritative inputs:** `spec.md` (AC1-AC4, guard ordering, T1-T5), `research/2026-07-09-outlook-startup-store-enumeration-com-stall-research.md` (§3 Recommended Fix, §4 Behavior Semantics, §7 Testing Implications), `issue.md` (AC source)

## Conventions used throughout this plan

- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292`
- **Evidence root:** all evidence artifacts are written under `<FEATURE>/evidence/<kind>/` per `evidence-and-timestamp-conventions`. Non-canonical evidence paths (e.g. `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`) are prohibited and must not be used.
- **Raw coverage report:** the machine-readable coverage report consumed by feature-review is written to `artifacts/csharp/coverage.xml`. This is the raw tool output, not an evidence artifact; every coverage evidence `.md` summary (baseline, post-change, comparison) is still written under `<FEATURE>/evidence/<kind>/` with numeric headline values.
- **Timestamp token (`<TS>`):** ISO-8601 `yyyy-MM-ddTHH-mm` captured at the moment the task runs; substitute into each evidence filename.
- **C# toolchain gate (run in this exact order; restart from step 1 if any step changes files or fails):**
  1. `dotnet tool run csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe TaskMaster.Test\bin\<Configuration>\TaskMaster.Test.dll UtilitiesCS.Test\bin\<Configuration>\UtilitiesCS.Test.dll /EnableCodeCoverage`
- **Coverage thresholds (C# Unit Test Policy):** repository-wide line coverage `>= 80%`; new/changed code `>= 90%`; no coverage regression on changed lines.
- **Constraints:** MSTest + Moq + FluentAssertions; no live Outlook; no temporary files; no real waits/timers. Production file-size cap: 500 lines (`StoresWrapper.cs` is 449 lines and must stay `<= 500`).

**Fail-closed evidence rule:** if any required baseline, QA-gate, RED-capture, or coverage-comparison artifact is missing or has incomplete fields (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`), the outcome is BLOCKED/INCOMPLETE, never PASS.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy documents in the required order and write a policy-read evidence artifact to `<FEATURE>/evidence/baseline/phase0-instructions-read.<TS>.md`. Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and an explicit list of files read, exactly: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/architecture-boundaries.md`, `.claude/rules/quality-tiers.md`.
- [x] [P0-T2] Record the baseline branch and commit to `<FEATURE>/evidence/baseline/baseline-scm.<TS>.md`. Acceptance: artifact records `Timestamp:`, `Command:` (`git rev-parse HEAD` and `git branch --show-current`), `EXIT_CODE:`, and `Output Summary:` naming branch `TaskMaster-wt-2026-07-09T14-19` and the resolved HEAD commit SHA.
- [x] [P0-T3] Capture baseline formatting state: run `dotnet tool run csharpier --check .` and write `<FEATURE>/evidence/baseline/baseline-format.<TS>.md`. Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (clean or list of unformatted files).
- [x] [P0-T4] Capture baseline analyzer build: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `<FEATURE>/evidence/baseline/baseline-analyzers.<TS>.md`. Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, warning/error counts).
- [x] [P0-T5] Capture baseline nullable/type-check build: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `<FEATURE>/evidence/baseline/baseline-nullable.<TS>.md`. Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T6] Capture baseline test + coverage: run `vstest.console.exe TaskMaster.Test\bin\<Configuration>\TaskMaster.Test.dll UtilitiesCS.Test\bin\<Configuration>\UtilitiesCS.Test.dll /EnableCodeCoverage`, convert the result to `artifacts/csharp/coverage.xml`, and write `<FEATURE>/evidence/baseline/baseline-tests-coverage.<TS>.md`. Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric baseline repository-wide line-coverage percent and passed/failed test counts. This baseline is the no-regression reference for [P3-T6].

---

### Phase 1 — Compilation Prerequisite and RED Regression Tests

Order is RED-before-GREEN. The phase-identity constant is added first solely so the regression tests compile and can reference the identity; adding the constant alone does NOT make T1/T2/T3 pass, because the scope-wrapping (Phase 2) and the responder branch (Phase 2) are still absent. During this phase the format, analyzer, and nullable steps must pass (clean compile); the test step is EXPECTED to show T1, T2, and T3 failing. Do not implement the fix in this phase.

- [x] [P1-T1] Add the phase-identity constant to `UtilitiesCS/Threading/CurrentStoreContext.cs`: `public const string StoresEnumerationPhaseIdentity = "<Stores-enumeration>";`. Acceptance: the constant value is not `"<unavailable>"` (verified against the special-cased value in `CurrentStoreContext.Normalize`); the constant compiles; steps 1-3 of the toolchain pass; the existing test suite remains unchanged in pass/fail counts (the constant is additive and non-behavioral).
- [x] [P1-T2] Create the sibling focused test file `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` (new file so `StoresWrapperTests.cs` at 466 lines is not pushed past the 500-line cap) and add test **T1 — `Init()` attribution parity** `[expect-fail]`: a stores proxy (existing `ReflectionRealProxy` / `CreateStoresProxy` pattern from `TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs:359-401`) whose enumerator records `CurrentStoreContext.Current` on each `MoveNext()`; drive `StoresWrapper.Init()` and assert the recorded value equals `CurrentStoreContext.StoresEnumerationPhaseIdentity`. Acceptance: file created; test compiles; steps 1-3 of the toolchain pass.
- [x] [P1-T3] Wire the new test file into the legacy `packages.config` project by adding `<Compile Include="OutlookObjects\Store\StoresWrapperEnumerationScopeTests.cs" />` to `TaskMaster.Test/TaskMaster.Test.csproj` (no glob include exists; explicit wiring is mandatory). Acceptance: the file appears in the project's `<Compile>` item group; the analyzer build (step 2) compiles the new test type into `TaskMaster.Test.dll`.
- [x] [P1-T4] Add test **T2 — `RewireOlObjectsAsync` attribution parity** `[expect-fail]` to `StoresWrapperEnumerationScopeTests.cs`: same enumerator-observation seam, driven through `RewireAfterDeserializeAsync` so the `StoresWrapper.cs:89` materialization is exercised; assert the recorded value equals `CurrentStoreContext.StoresEnumerationPhaseIdentity`. Acceptance: test compiles; steps 1-3 of the toolchain pass.
- [x] [P1-T5] Add the behavior-preserving guardrail tests **T4** and **T5** to `StoresWrapperEnumerationScopeTests.cs` (these are GREEN before and after; not `[expect-fail]`): T4 — a healthy multi-store enumeration yields the identical included set and order, and `CurrentStoreContext.Current` is null after `Init()` returns (scope disposed); T5 — an enumerator that throws mid-enumeration leaves `CurrentStoreContext.Current` null afterwards. Acceptance: T4 and T5 compile and PASS on HEAD (they assert invariants the fix must preserve).
- [x] [P1-T6] Add test **T3 — responder phase-identity branch** `[expect-fail]` to the existing `UtilitiesCS.Test/Threading/StoreLockupResponderTests.cs` (203 lines; adding T3 keeps it under the 500-line cap, so no sibling file and no new csproj wiring are required): call `OnLockupDetected(new LockupAttribution(TimeSpan.FromSeconds(6), CurrentStoreContext.StoresEnumerationPhaseIdentity))` with a `MockBehavior.Strict` `IStoreDisableService`; assert exactly one WARN line via the injected `logSink` formatted with `autoDisabled: false`, and zero `IStoreDisableService` calls. Acceptance: test compiles; steps 1-3 of the toolchain pass.
- [x] [P1-T7] [expect-fail] Run the test step (`vstest.console.exe ... /EnableCodeCoverage`) and confirm the RED baseline: T1, T2, and T3 FAIL on HEAD (T1/T2 record null instead of the phase identity; T3's Strict mock fails because `IsDisabled`/`DisableSessionOnly` are invoked), while T4, T5, and all pre-existing tests PASS. Write the RED evidence to `<FEATURE>/evidence/regression-testing/red-before-fix.<TS>.md`. Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), and `Output Summary:` naming the three failing tests (T1, T2, T3) with their failure assertions, confirming the fail-before condition for AC4.

---

### Phase 2 — Minimal Production Fix (RED to GREEN)

- [x] [P2-T1] In `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, extract a private helper `MaterializeFilteredStores()` that returns `GetFilteredStores().ToList()` executed inside `using (CurrentStoreContext.Begin(CurrentStoreContext.StoresEnumerationPhaseIdentity))`, and replace the inline materialization in `Init()` (line 44) with a call to it. Preserve the existing Stopwatch and `[store-filter]` `logger.Debug` line and the included set/order exactly (the scope is observational only). Acceptance: T1 now PASSES; T4/T5 still PASS; `StoresWrapper.cs` remains `<= 500` lines; toolchain steps 1-3 pass.
- [x] [P2-T2] In `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, replace the inline materialization in `RewireOlObjectsAsync` (line 89) with a call to the same `MaterializeFilteredStores()` helper. Preserve the existing Stopwatch and `[Startup timing]` `logger.Debug` line and the included set/order exactly. Acceptance: T2 now PASSES; toolchain steps 1-3 pass.
- [x] [P2-T3] In `UtilitiesCS/Threading/StoreLockupResponder.cs`, add a phase-identity terminal branch to `OnLockupDetected` positioned to enforce the guard ordering exactly: blank-guard -> unresolved-guard -> **phase-identity guard** -> already-disabled guard -> disable/notify. On a match against `CurrentStoreContext.StoresEnumerationPhaseIdentity`, emit exactly one `[store-lockup]` WARN line via `_logSink` using `StoreLockupAttribution.FormatLine(..., autoDisabled: false)`, optionally dispatch an informational modeless notification via `_dispatcher.BeginInvoke`, and `return` WITHOUT calling `IsDisabled`, `DisableSessionOnly`, or the action-button wiring. Acceptance: T3 now PASSES (zero disable-service calls under the Strict mock); the phase guard precedes every `IStoreDisableService` call; toolchain steps 1-3 pass.
- [x] [P2-T4] Run the full test step and confirm the GREEN transition: T1, T2, T3 now PASS, T4/T5 still PASS, and the entire suite is green. Write the pass evidence to `<FEATURE>/evidence/regression-testing/green-after-fix.<TS>.md`. Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` (zero), and `Output Summary:` confirming the previously-failing T1/T2/T3 now pass and no pre-existing test regressed.

---

### Phase 3 — Final QA Loop, Coverage, and Documentation

Run the full toolchain as one loop in the exact order below; if any step changes files or fails, restart from the format step until a single clean pass completes. Each command step writes its own QA-gate evidence artifact (no aggregate-only artifact).

- [x] [P3-T1] Format: run `dotnet tool run csharpier .` and write `<FEATURE>/evidence/qa-gates/qa-format.<TS>.md`. Acceptance: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recorded; if files were reformatted, the loop restarts from this step.
- [x] [P3-T2] Lint/analyzers: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `<FEATURE>/evidence/qa-gates/qa-analyzers.<TS>.md`. Acceptance: `Timestamp:`, `Command:`, `EXIT_CODE:` (zero), `Output Summary:` (zero analyzer errors) recorded.
- [x] [P3-T3] Type-check/nullable: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `<FEATURE>/evidence/qa-gates/qa-nullable.<TS>.md`. Acceptance: `Timestamp:`, `Command:`, `EXIT_CODE:` (zero), `Output Summary:` recorded.
- [x] [P3-T4] Test + coverage: run `vstest.console.exe TaskMaster.Test\bin\<Configuration>\TaskMaster.Test.dll UtilitiesCS.Test\bin\<Configuration>\UtilitiesCS.Test.dll /EnableCodeCoverage`, regenerate `artifacts/csharp/coverage.xml`, and write `<FEATURE>/evidence/qa-gates/qa-tests-coverage.<TS>.md`. Acceptance: `Timestamp:`, `Command:`, `EXIT_CODE:` (zero), and `Output Summary:` with numeric post-change repository-wide line-coverage percent, the new/changed-code coverage percent for the three touched production files, and passed/failed counts.
- [x] [P3-T5] Coverage comparison / threshold verification: compare the [P0-T6] baseline against the [P3-T4] post-change coverage and write `<FEATURE>/evidence/qa-gates/coverage-comparison.<TS>.md`. Acceptance: artifact reports baseline repository-wide line coverage, post-change repository-wide line coverage (`>= 80%`), new/changed-code coverage for `CurrentStoreContext.cs`, `StoresWrapper.cs`, and `StoreLockupResponder.cs` (`>= 90%`), and confirms no coverage regression on changed lines. If any threshold is unmet, the outcome is remediation-required, not PASS.
- [x] [P3-T6] Update `<FEATURE>/spec.md` and `<FEATURE>/issue.md` to check off AC1-AC4 with evidence references, and mirror the issue update to `<FEATURE>/evidence/issue-updates/issue-292.<TS>.md`. Acceptance: AC1 (attributed WARN at both `Init()` and `RewireOlObjectsAsync` sites) references T1/T2; AC2 (non-null phase identity, no disable write, no crash) references T3 and the guard ordering in [P2-T3]; AC3 (behavior-preserving, `Current` null after materialization) references T4/T5; AC4 (deterministic RED-before-GREEN coverage) references [P1-T7] and [P2-T4]; the issue-update mirror records `Timestamp:`, exact text, and `PostedAs:`.

---

## Acceptance Criteria Traceability

- **AC1 — Attributed watchdog action at both enumeration sites** -> [P2-T1] (Init site), [P2-T2] (Rewire site), verified by T1/T2 in [P1-T2]/[P1-T4]/[P2-T4].
- **AC2 — Non-null phase identity handled safely (no disable write, no crash)** -> [P1-T1] (constant, distinct from `"<unavailable>"`), [P2-T3] (responder phase branch + guard ordering), verified by T3 in [P1-T6]/[P2-T4].
- **AC3 — Behavior-preserving for healthy stores** -> [P2-T1]/[P2-T2] (set/order preserved, scope disposed), verified by T4/T5 in [P1-T5].
- **AC4 — Deterministic RED-before-GREEN regression coverage** -> [P1-T7] (RED capture), [P2-T4] (GREEN), [P3-T4]/[P3-T5] (>= 90% new-code coverage).

## Files Changed

- Production: `UtilitiesCS/Threading/CurrentStoreContext.cs`, `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, `UtilitiesCS/Threading/StoreLockupResponder.cs`.
- Tests: `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` (new; T1/T2/T4/T5), `UtilitiesCS.Test/Threading/StoreLockupResponderTests.cs` (extended; T3).
- Build wiring: `TaskMaster.Test/TaskMaster.Test.csproj` (`<Compile Include>` for the new sibling test file).

## Out of Scope (do not implement)

Per spec Scope & Non-Goals and research §2/§6: no worker-thread offload of the enumeration; no timeout/cancellation wrapper on the blocked `Next()`; no indexed `Stores.Count`/`Stores[i]` access; no readiness-gating as the primary fix; no pre-enumeration detection of the stalling store. The fix cannot prevent, shorten, cancel, or bound the STA block; end-to-end reproduction of the real stall is post-merge manual validation, not an acceptance gate. The optional §3.4 secondary hardening (routing the fresh-build loop through `AddOrRestoreStore`) is deferred and not included in this plan.
