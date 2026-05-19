# 2026-05-14-ci-format-and-vs-test-failures (Plan)

- **Issue:** #155
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-05-14T08-32
- **Status:** Draft
- **Version:** 0.1

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

---

## Plan Summary

This is a C# bug-fix effort for Issue #155 on combined branch `bug/ci-format-and-vs-test-failures-155`. It addresses five work streams:

- **Stream 1** — CI formatting failure: remove four committed git mergetool `.csproj` artifacts and three stray backup files; restore the trailing newline on `TaskMaster/TaskMaster.csproj`.
- **Stream 2** — Trace test `GetCallerByName_...`: test-only fix; remove the `ResolveCaller` helper indirection.
- **Stream 3** — Trace tests `GetMethodCallLogString_...` and `GetMethodTraceString_...`: the only true product defect. The `GetCallerMethod` fallback in `TraceUtility.cs` can return a non-project (mscorlib) frame. Regression-test-first per the Bugfix Workflow; production fix plus test `[MethodImpl(MethodImplOptions.NoInlining)]` annotations.
- **Stream 4** — SCODictionary deserialize test: test-only isolation fix; wrap the assertion in the established `SpinWait.SpinUntil` pattern.
- **Stream 5** — AngleSharp CodePages assembly mismatch: build-config fix across `packages.config`, `UtilitiesCS.Test.csproj`, and `app.config`.

**Regression-test ordering:** Stream 3 is the only product defect and follows regression-test-first ordering (Phase 2 authors an expect-fail test before Phase 3 applies the production fix). Streams 1, 2, 4, and 5 are test or configuration corrections; their existing tests / the `csharpier check .` gate serve as the regression gate, as stated explicitly in each stream's tasks below.

**C# toolchain loop (run in this exact order, restart from step 1 on any failure or file change):**

1. `dotnet tool run csharpier .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Most relevant test assembly: `UtilitiesCS.Test`. The full suite must still pass (CI baseline: 3989 passing / 2 skipped before this branch's failures).

**Evidence locations (canonical, non-overridable):**

- Baseline artifacts: `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/<ISO-8601-UTC-timestamp>/`
- Final-QA artifacts: `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/qa-gates/<ISO-8601-UTC-timestamp>/`

---

### Phase 0 — Context, Inputs & Baseline Capture

- [ ] [P0-T1] Read repo policy `CLAUDE.md` sections: General Code Change Policy, C# Code Change Policy, General Unit Test Policy, C# Unit Test Policy, Bugfix Workflow, and C# Toolchain order. Acceptance: a short note in the executor log confirming each section was read and the four-step toolchain order is recorded verbatim.
- [ ] [P0-T2] Read the authoritative inputs: `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/issue.md`, `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/spec.md`, and `artifacts/research/2026-05-14-vstest-failures-research.md`. Acceptance: executor log records the five work streams and the seven named failing tests/files from the research document.
- [ ] [P0-T3] Confirm the working branch is `bug/ci-format-and-vs-test-failures-155` and record the current commit SHA. Acceptance: branch name and SHA recorded in the executor log; if the branch does not exist, create it from `development`.
- [ ] [P0-T4] Run `dotnet tool run csharpier .` in check mode (`dotnet tool run csharpier check .`) and capture the full output to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/<timestamp>/csharpier-check.txt`. Acceptance: file exists and records the current non-zero exit code and the `TaskMaster.csproj` / `TaskMaster_BACKUP_1250.csproj` findings.
- [ ] [P0-T5] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and capture output to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/<timestamp>/msbuild-analyzers.txt`. Acceptance: file exists and records the analyzer build result.
- [ ] [P0-T6] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and capture output to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/<timestamp>/msbuild-nullable.txt`. Acceptance: file exists and records the nullable/type-check build result.
- [ ] [P0-T7] Run `vstest.console.exe` against the built `UtilitiesCS.Test` assembly (and the full suite assemblies) with `/EnableCodeCoverage`, capturing the test result summary and coverage to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/<timestamp>/vstest-baseline.txt` and the `.coverage`/`.coveragexml` file alongside it. Acceptance: file exists and records the seven named failing tests (`GetCallerByName_...`, `GetMethodCallLogString_...`, `GetMethodTraceString_...`, `ExtractLinks_StateUnderTest_ExpectedBehavior`, `FilterLinksByDomain_StateUnderTest_ExpectedBehavior`, `Deserialize_WhenInvalidJsonSimpleOverloadAndPromptDisabled_SerializesCurrentState`) plus the total pass/fail/skip count and overall coverage percentage as the comparison baseline.

### Phase 1 — Stream 1: CI Formatting Failure (test/config correction; `csharpier check .` is the regression gate)

- [ ] [P1-T1] Verify the four git mergetool artifacts are not referenced by any solution or project file: run `git grep -n "TaskMaster_BACKUP_1250\|TaskMaster_BASE_1250\|TaskMaster_LOCAL_1250\|TaskMaster_REMOTE_1250"` and inspect `TaskMaster.sln`. Acceptance: executor log confirms zero references in `TaskMaster.sln` and any `*.csproj`.
- [ ] [P1-T2] Delete `TaskMaster/TaskMaster_BACKUP_1250.csproj` using `git rm TaskMaster/TaskMaster_BACKUP_1250.csproj`. Acceptance: `git status` shows the file staged for deletion and it no longer exists on disk.
- [ ] [P1-T3] Delete `TaskMaster/TaskMaster_BASE_1250.csproj` using `git rm TaskMaster/TaskMaster_BASE_1250.csproj`. Acceptance: `git status` shows the file staged for deletion and it no longer exists on disk.
- [ ] [P1-T4] Delete `TaskMaster/TaskMaster_LOCAL_1250.csproj` using `git rm TaskMaster/TaskMaster_LOCAL_1250.csproj`. Acceptance: `git status` shows the file staged for deletion and it no longer exists on disk.
- [ ] [P1-T5] Delete `TaskMaster/TaskMaster_REMOTE_1250.csproj` using `git rm TaskMaster/TaskMaster_REMOTE_1250.csproj`. Acceptance: `git status` shows the file staged for deletion and it no longer exists on disk.
- [ ] [P1-T6] Verify the three stray backup files are unreferenced: run `git grep -n "ToDoItem_Backup\|PeopleScoDictionaryNewBackup"` and inspect every `*.csproj`. Acceptance: executor log confirms no `*.csproj` references `ToDoModel/ToDoItem_Backup.bkp`, `ToDoModel/Data Model/People/PeopleScoDictionaryNewBackup.cs`, or `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs` by file name or class name.
- [ ] [P1-T7] Delete `ToDoModel/ToDoItem_Backup.bkp` using `git rm "ToDoModel/ToDoItem_Backup.bkp"`. Acceptance: `git status` shows the file staged for deletion and it no longer exists on disk.
- [ ] [P1-T8] Delete `ToDoModel/Data Model/People/PeopleScoDictionaryNewBackup.cs` using `git rm "ToDoModel/Data Model/People/PeopleScoDictionaryNewBackup.cs"`. Acceptance: `git status` shows the file staged for deletion and it no longer exists on disk.
- [ ] [P1-T9] Delete `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs` using `git rm "UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs"`. Acceptance: `git status` shows the file staged for deletion and it no longer exists on disk.
- [ ] [P1-T10] Edit `TaskMaster/TaskMaster.csproj` so the file ends with exactly one trailing newline. Acceptance: the last byte of the file is a single newline with no trailing blank lines.
- [ ] [P1-T11] Run `dotnet tool run csharpier check .` and confirm exit code 0. Acceptance: command exits 0 with no "Was not formatted" or "invalid xml" findings; if any file is reformatted, restart the toolchain loop from step 1.

### Phase 2 — Stream 3: Product Defect Regression Test (must fail first)

- [ ] [P2-T1] [expect-fail] Add a regression test to `UtilitiesCS.Test/HelperClasses/TraceUtility_Tests.cs` that asserts `TraceUtility.GetCallerMethod` does not return a frame from a non-project assembly when all project frames are exhausted. The test must construct or drive a scenario in which the `GetCallerMethod` fallback path is reached and assert the returned `MethodBase` is either `null` or belongs to a project assembly per the `ProjectNames` filter. Use MSTest (`[TestClass]`/`[TestMethod]`), Moq if a dependency must be isolated, and FluentAssertions for assertions. Acceptance: the test compiles and is present in the file.
- [ ] [P2-T2] [expect-fail] Run the new regression test in isolation under `vstest.console.exe` and confirm it fails, demonstrating the `GetCallerMethod` fallback currently returns a non-project (mscorlib) frame. Capture the failing output to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/<timestamp>/regression-test-expect-fail.txt`. Acceptance: file exists and shows the test failing for the documented root cause.

### Phase 3 — Stream 3: Product Defect Minimal Fix

- [ ] [P3-T1] Edit `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` (~line 217) so the `GetCallerMethod` fallback `methodCalledBy = st.GetFrame(frameLevel).GetMethod();` does not return a non-project-assembly frame. The fallback must return `null` instead of a frame whose assembly is not in `ProjectNames`, consistent with the `GetFirstMethodOfMine` / `IsMine()` project-name filter. Make the smallest change needed; no opportunistic refactors. Acceptance: the fallback returns `null` when the frame at `frameLevel` is a non-project assembly; the regression test from P2-T1 passes.
- [ ] [P3-T2] Run the regression test from P2-T1 under `vstest.console.exe` and confirm it now passes. Acceptance: the test passes; capture is deferred to the Phase 6 final-QA artifact.

### Phase 4 — Streams 2, 3, 4, 5: Test and Configuration Corrections

- [ ] [P4-T1] Edit `UtilitiesCS.Test/Extensions/TraceExtensions_Tests.cs` (Stream 2) to remove the `ResolveCaller` helper indirection. Capture the `StackTrace` directly inside the test method body of `GetCallerByName_ReturnsMatchingMethodAndHandlesEmptySpecialAndMissingNames` so the searched-for frame is the test method's own frame, making the test runner-independent. Acceptance: the `ResolveCaller` helper is removed; the test captures `new StackTrace()` in its own body and the test compiles. The existing test is the regression gate for this stream.
- [ ] [P4-T2] Edit `UtilitiesCS.Test/HelperClasses/TraceUtility_Tests.cs` (Stream 3, test side) to add `[MethodImpl(MethodImplOptions.NoInlining)]` to the `CaptureMethodCallLogString`, `CaptureMethodTraceString`, and `FinishMethodTraceStringCapture` helper methods, and add the `using System.Runtime.CompilerServices;` directive if not already present. Acceptance: all three helper methods carry the attribute; the file compiles. The existing `GetMethodCallLogString_...` and `GetMethodTraceString_...` tests are the regression gate for this stream.
- [ ] [P4-T3] Edit `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Additional_Tests.cs` (Stream 4, ~line 244) to wrap the `dict.HasWrittenPath("simple-broken.json").Should().BeTrue();` assertion in the `SpinWait.SpinUntil(() => dict.HasWrittenPath("simple-broken.json"), TimeSpan.FromSeconds(1)).Should().BeTrue();` pattern already used at lines 43-50 of the same file. Acceptance: the assertion uses `SpinWait.SpinUntil` with a 1-second timeout; the file compiles. No production change; the existing `Deserialize_WhenInvalidJsonSimpleOverloadAndPromptDisabled_SerializesCurrentState` test is its own regression gate.
- [ ] [P4-T4] Edit `UtilitiesCS.Test/packages.config` (Stream 5) to add `<package id="System.Text.Encoding.CodePages" version="10.0.7" targetFramework="net481" />`. Acceptance: the entry is present in `packages.config`.
- [ ] [P4-T5] Edit `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (Stream 5, ~lines 765-766) to change the `System.Text.Encoding.CodePages` Reference `Version=10.0.0.5` to `Version=10.0.0.7` and the `HintPath` from `...System.Text.Encoding.CodePages.10.0.5\lib\net462\...` to `...System.Text.Encoding.CodePages.10.0.7\lib\net462\...`. Acceptance: the Reference and HintPath both name version `10.0.7`/`10.0.0.7`.
- [ ] [P4-T6] Edit `UtilitiesCS.Test/app.config` (Stream 5, ~line 499) to change the `System.Text.Encoding.CodePages` `bindingRedirect` from `oldVersion="0.0.0.0-10.0.0.5" newVersion="10.0.0.5"` to `oldVersion="0.0.0.0-10.0.0.7" newVersion="10.0.0.7"`. Acceptance: the bindingRedirect names version `10.0.0.7`. Streams 5's existing AngleSharp tests (`ExtractLinks_StateUnderTest_ExpectedBehavior`, `FilterLinksByDomain_StateUnderTest_ExpectedBehavior`) are the regression gate for this stream.

### Phase 5 — Documentation & Status

- [ ] [P5-T1] Update `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/spec.md` and `issue.md` to record outcomes: the five streams resolved, the seven backup/artifact files removed, the one product defect fixed in `TraceUtility.cs`, and any deviations from scope. Acceptance: both documents reflect the final state and check off their respective fix/validation items.

### Phase 6 — Final QA Loop

- [ ] [P6-T1] Run `dotnet tool run csharpier .` across the repository. Acceptance: command completes; if any file is reformatted, apply the changes and restart the toolchain loop from this task.
- [ ] [P6-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and capture output to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/qa-gates/<timestamp>/msbuild-analyzers.txt`. Acceptance: build succeeds with zero new analyzer findings versus the P0-T5 baseline; file exists.
- [ ] [P6-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and capture output to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/qa-gates/<timestamp>/msbuild-nullable.txt`. Acceptance: build succeeds with zero new nullable/type-check diagnostics versus the P0-T6 baseline; file exists.
- [ ] [P6-T4] Run `vstest.console.exe` against the `UtilitiesCS.Test` assembly and the full suite with `/EnableCodeCoverage`, capturing the result summary and coverage to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/qa-gates/<timestamp>/vstest-final.txt` and the `.coverage`/`.coveragexml` file alongside it. Acceptance: file exists; all seven previously failing tests pass; total pass count is >= 3989 + 1 (the new regression test) with 2 skipped and zero failures; no net regression versus the P0-T7 baseline.
- [ ] [P6-T5] Run `dotnet tool run csharpier check .` and confirm exit code 0. Acceptance: command exits 0 with no findings.
- [ ] [P6-T6] Produce a coverage comparison: compare the P6-T4 coverage to the P0-T7 baseline and write the delta to `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/qa-gates/<timestamp>/coverage-comparison.txt`. Acceptance: file exists; overall line coverage is >= 80% and >= the baseline; coverage for every touched file is >= its baseline; the new regression test's target unit (`GetCallerMethod` path in `TraceUtility.cs`) has coverage >= 90% for the changed lines.
- [ ] [P6-T7] Confirm the toolchain loop completed in a single clean pass (P6-T1 through P6-T5 all green with no file changes on the final pass). Acceptance: executor log states the four-step toolchain passed in order without errors or file modifications on the final pass, and lists the exact commands run.

### Phase 7 — PR & Handoff

- [ ] [P7-T1] Prepare PR notes for branch `bug/ci-format-and-vs-test-failures-155`: summary of the five streams, the one product defect, risks, validation performed, and links to the regression test and evidence artifacts. Acceptance: PR notes written and review requested.
- [ ] [P7-T2] Record traceability links (Issue #155, PR, `artifacts/research/2026-05-14-vstest-failures-research.md`, spec, plan) in the spec's Rollout & Follow-up section. Acceptance: links recorded in `spec.md`.
