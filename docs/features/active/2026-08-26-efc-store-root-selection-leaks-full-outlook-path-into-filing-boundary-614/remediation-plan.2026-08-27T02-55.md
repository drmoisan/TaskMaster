# Issue #614 — Remediation Cycle 3 Atomic Plan

- **Issue:** #614
- **Cycle:** 3
- **Last Updated:** 2026-08-27T02-55
- **Status:** Preflight pending
- **Work Mode:** `full-bug`
- **Requirements source:** `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/remediation-inputs.2026-08-27T02-55.md`
- **Entry HEAD:** `e8d8f52952f978a20ae056748e6fa9fd40b5fdb0`
- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
- **Branch:** `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`

## Execution Contract

All evidence must be written under `<FEATURE>/evidence/<kind>/`. Phase 0 evidence uses `<FEATURE>/evidence/remediation-baseline/`; regression evidence uses `<FEATURE>/evidence/regression-testing/`; final gates use `<FEATURE>/evidence/qa-gates/`; other audit evidence uses `<FEATURE>/evidence/other/`. Every command artifact must contain `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. An expected non-zero result must additionally use the exact field `ExpectedExitCode: 1` in its own artifact.

Use fabricated `C:\OneDrive` values only. Raw TRX and coverage files remain under the gitignored `coverage/` tree and must not be staged. Tests must not mutate process environment state, use temporary files, add static/global hooks, detect a test runner, or contact Outlook/network services.

The only permitted code/test modifications are the one production file and eight test files listed in the remediation inputs. The two user-waived documentation/evidence files must remain byte-identical. `spec.md` is not modified or staged in cycle 3. AC14 remains checked; AC24 remains unchecked because its literal `vstest.console.exe ... /EnableCodeCoverage` command and `<FEATURE>/evidence/qa/` path do not describe the canonical repository workflow that produced the truthful completed evidence. This is an accepted documentation/evidence wording risk only; it does not waive or weaken any code, test, coverage, CI, review, or orchestration-validation gate.

Production design is fixed for execution: chain the existing one- and two-argument `ApplicationGlobals` constructors through a new public three-argument overload accepting `Func<string, string> readEnvironmentVariable`; retain lazy/eager behavior; store the optional reader; and have `LoadBasicMethod` instantiate `AppFileSystemFolderPaths` with the reader only when supplied, otherwise use its existing public default constructor. `TaskMaster/ThisAddIn.cs` remains unchanged and therefore retains real-environment fail-fast behavior.

The eight test files are edited in 3/3/2 batches. No implementation batch may exceed three test files. If evidence requires any additional production or test file, stop with remediation-required status rather than expanding silently.

Final QA must run in one clean sequence: formatting, analyzer rebuild, nullable rebuild, then full MSTest with coverage. If formatting changes a file or any step fails, correct the defect and restart from formatting. No command task may be recorded as skipped.

For every scoped VSTest task, resolve the executable with the exact PowerShell statement `$vstest = Join-Path (& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath) "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`. Each scoped command uses `/InIsolation`, writes TRX only under its stated gitignored `coverage\trx\<task-id>` directory, and records the resolver statement, command, and exit code in the task artifact.

### Phase 0 — Policy, Exact-Head Baseline, and Scope Capture

- [x] [P0-T1] Read `AGENTS.md`, then `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, and `<FEATURE>/remediation-inputs.2026-08-27T02-55.md` in that order; write `<FEATURE>/evidence/remediation-baseline/phase0-instructions-read.md` listing the files and policy order.
  Acceptance: the artifact contains `Timestamp:`, `Policy Order:`, and the explicit seven-file list; no policy file is modified.
- [x] [P0-T2] Capture exact entry state with `git rev-parse HEAD`, `git branch --show-current`, `git status --porcelain`, and `git diff --check`; write `<FEATURE>/evidence/remediation-baseline/cycle3-entry-state.md`.
  Acceptance: the artifact records HEAD `e8d8f52952f978a20ae056748e6fa9fd40b5fdb0`, the Issue #614 branch, and `git diff --check` exit 0; porcelain may contain exactly the two intentionally untracked cycle-3 artifacts `<FEATURE>/remediation-inputs.2026-08-27T02-55.md` and `<FEATURE>/remediation-plan.2026-08-27T02-55.md`, and any other pre-existing entry change is a blocker.
- [x] [P0-T3] Record the exact-head CI causal baseline from `gh run view 33034033583 --job 98392718650 --log-failed` in `<FEATURE>/evidence/remediation-baseline/exact-head-ci-onedrive-failures.md` without copying host/account paths.
  Acceptance: the artifact records 6,586 total, 6,564 passed, 22 failed, all 22 mapped to `ResolveOneDriveRoot -> LoadFolders -> AppFileSystemFolderPaths..ctor -> ApplicationGlobals.LoadBasicMethod`, and the separate TaskMaster.Test lazy-force failure by name.
- [x] [P0-T4] Run the pre-change call-site census with `rg -n "new TaskMaster\.ApplicationGlobals\([^\n]*, true\)" UtilitiesCS.Test --glob "*.cs"` and `rg -n -C 20 "Constructor_WithoutLoadBasic_DoesNotMaterializeCollaboratorsUntilForceBasicLoad" TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`; write `<FEATURE>/evidence/remediation-baseline/application-globals-constructor-census.md`.
  Acceptance: the artifact lists ten eager UtilitiesCS.Test call sites across seven files and the separate one-argument-plus-force caller in `ApplicationGlobalsTests.cs`.
- [x] [P0-T5] Record SHA-256 hashes for `<FEATURE>/evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md` and `<FEATURE>/change-description.2026-08-26.md` using `Get-FileHash -Algorithm SHA256`; write `<FEATURE>/evidence/remediation-baseline/waived-documentation-hashes.md`.
  Acceptance: both paths and hashes are present and no source document is changed.
- [x] [P0-T6] Run `dotnet tool run csharpier check .`; write `<FEATURE>/evidence/remediation-baseline/csharpier-check.md`.
  Acceptance: the artifact records the exact command, exit code, and changed-file count; any non-zero result blocks implementation until the baseline is adjudicated.
- [x] [P0-T7] Run `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; write `<FEATURE>/evidence/remediation-baseline/analyzer-build.md`.
  Acceptance: the artifact records the exact command, exit code, error count, and warning count.
- [x] [P0-T8] Run `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; write `<FEATURE>/evidence/remediation-baseline/nullable-build.md`.
  Acceptance: the artifact records the exact command, exit code, error count, and warning count; `/p:Nullable=enable` is not added.
- [x] [P0-T9] Run `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`, preserve raw outputs only under `coverage/`, and write `<FEATURE>/evidence/remediation-baseline/full-test-coverage.md`.
  Acceptance: the artifact records exact total/passed/failed counts plus numeric filtered line and branch coverage with covered/valid numerators and denominators; any observed local/host difference from the exact-head CI baseline is stated explicitly.
- [x] [P0-T10] Capture `(Get-Content <path>).Count` for `TaskMaster/AppGlobals/ApplicationGlobals.cs` and all eight in-scope test files in `<FEATURE>/evidence/remediation-baseline/file-size-and-scope.md`.
  Acceptance: the artifact records each baseline line count, flags the existing 500-line headroom for every file, and confirms no in-scope file already exceeds 500 lines.

### Phase 1 — Deterministic Regression Red

- [x] [P1-T1] Add `ApplicationGlobals_WithInjectedEnvironmentReader_LoadsOneDriveWithoutProcessEnvironment` to `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs` before editing production code; call the new three-argument constructor with `loadBasic: true` and a pure reader returning `C:\OneDrive` only for `OneDriveCommercial`, then assert `globals.FS.SpecialFolders["OneDrive"]` equals that root.
  Acceptance: the test uses MSTest and FluentAssertions, contains Arrange/Act/Assert intent, does not read or mutate process environment state, and is unchanged between red and green runs.
- [x] [P1-T2] [expect-fail] Run `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug "/p:Platform=Any CPU"`; write `<FEATURE>/evidence/regression-testing/application-globals-injected-reader-fail-before.md` with `ExpectedExitCode: 1`.
  Acceptance: the artifact records the exact compiler diagnostic that no three-argument `ApplicationGlobals` constructor exists and proves the failure originates from P1-T1, not an unrelated baseline defect.
- [x] [P1-T3] Verify with `git diff -- TaskMaster/AppGlobals/ApplicationGlobals.cs UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs` that only the regression test changed and production remains byte-identical; append the diff conclusion to the P1-T2 artifact.
  Acceptance: `ApplicationGlobals.cs` has no diff and the only test diff is P1-T1.

### Phase 2 — Minimal Production Injection Seam

- [x] [P2-T1] Modify only `TaskMaster/AppGlobals/ApplicationGlobals.cs`: chain its existing constructors through a public three-argument overload accepting `Func<string, string> readEnvironmentVariable`, preserve the existing `loadBasic` lazy/eager semantics, and make `LoadBasicMethod` choose `new AppFileSystemFolderPaths(readEnvironmentVariable)` only when the reader is supplied and `new AppFileSystemFolderPaths()` otherwise.
  Acceptance: `TaskMaster/ThisAddIn.cs` and all existing production call sites are unchanged; no fallback value, static hook, test-host detection, environment mutation, new I/O, or exception swallowing is introduced; the file remains at most 500 lines after formatting.
- [x] [P2-T2] Run `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug "/p:Platform=Any CPU"`; resolve `$vstest` with the execution-contract statement; then run `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableLoader_Tests" "/Logger:trx;LogFileName=p2-t2.trx" "/ResultsDirectory:coverage\trx\p2-t2"`; write `<FEATURE>/evidence/regression-testing/application-globals-injected-reader-pass-after.md`.
  Acceptance: the artifact records both exact commands and exit codes; both exit 0; the P1-T1 test body is byte-identical to its red version; the new regression passes.
- [x] [P2-T3] Resolve `$vstest` with the execution-contract statement and run `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.AppGlobals.AppFileSystemFolderPathsOneDriveResolutionTests" "/Logger:trx;LogFileName=p2-t3.trx" "/ResultsDirectory:coverage\trx\p2-t3"`; write `<FEATURE>/evidence/regression-testing/runtime-onedrive-fail-fast-contract.md`.
  Acceptance: the artifact records the exact command and exit code; exit 0; all tests pass, including `ResolveOneDriveRoot_NoVariableSet_FailsExplicitlyWithARedactedDiagnostic`; the artifact states that existing one-/two-argument runtime construction still uses the real environment and explicit D7 failure.

### Phase 3 — Test Caller Adaptation in 3/3/2 Batches

- [x] [P3-T1] Batch 1A: update `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs` so its eager `ApplicationGlobals` construction passes a pure in-memory OneDrive reader.
  Acceptance: no assertion or subject-under-test behavior changes; no process environment access is added; the file remains at most 500 lines.
- [x] [P3-T2] Batch 1B: update `UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs` by the same constructor-injection rule.
  Acceptance: no assertion or converter behavior changes; the file remains at most 500 lines.
- [x] [P3-T3] Batch 1C: update `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs` by the same constructor-injection rule.
  Acceptance: all existing integration assertions remain byte-identical; the file remains at most 500 lines.
- [x] [P3-T4] Verify batch 1 by running `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug "/p:Platform=Any CPU"`; resolve `$vstest` with the execution-contract statement; then run `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.PeopleScoConverter_Tests|FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.ScDictionaryConverter_Tests|FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.ScoDictionaryConverterTests" "/Logger:trx;LogFileName=p3-t4.trx" "/ResultsDirectory:coverage\trx\p3-t4"`; write `<FEATURE>/evidence/regression-testing/caller-batch-1.md`.
  Acceptance: the artifact records both exact commands and exit codes; both exit 0; exact totals are recorded; zero test in the three classes fails.
- [x] [P3-T5] Batch 2A: update `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs` by the same constructor-injection rule.
  Acceptance: wrapper composition assertions remain unchanged and the formatted file is at most 500 lines.
- [x] [P3-T6] Batch 2B: update `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs` by the same constructor-injection rule.
  Acceptance: wrapper composition assertions remain unchanged and the formatted file is at most 500 lines.
- [x] [P3-T7] Batch 2C: adapt all eager constructor calls in `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs`, without altering the P1-T1 regression body.
  Acceptance: every eager call in the file uses the injected reader, existing loader assertions are unchanged, and the file remains at most 500 lines.
- [x] [P3-T8] Verify batch 2 by running `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug "/p:Platform=Any CPU"`; resolve `$vstest` with the execution-contract statement; then run `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.WrapperScoDictionaryTest|FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.WrapperScDictionaryTest|FullyQualifiedName~UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableLoader_Tests" "/Logger:trx;LogFileName=p3-t8.trx" "/ResultsDirectory:coverage\trx\p3-t8"`; write `<FEATURE>/evidence/regression-testing/caller-batch-2.md`.
  Acceptance: the artifact records both exact commands and exit codes; both exit 0; exact totals are recorded; zero test in the three classes fails.
- [x] [P3-T9] Batch 3A: update `UtilitiesCS.Test/EmailIntelligence/PeopleScoDictionaryNew_Tests.cs` by the same constructor-injection rule.
  Acceptance: dictionary assertions remain unchanged and the file remains at most 500 lines.
- [x] [P3-T10] Batch 3B: update `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` so `Constructor_WithoutLoadBasic_DoesNotMaterializeCollaboratorsUntilForceBasicLoad` supplies the deterministic reader while retaining its one-step deferred construction, explicit force, and all collaborator-materialization assertions.
  Acceptance: the test still proves `BasicLoaded.IsValueCreated` is false before force and true afterward; no assertion is removed or weakened; the file remains at most 500 lines.
- [x] [P3-T11] Verify batch 3 by running `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`; resolve `$vstest` with the execution-contract statement; run `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.PeopleScoDictionaryNew_Tests" "/Logger:trx;LogFileName=p3-t11-utilities.trx" "/ResultsDirectory:coverage\trx\p3-t11-utilities"`; then run `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.AppGlobals.ApplicationGlobalsTests.Constructor_WithoutLoadBasic_DoesNotMaterializeCollaboratorsUntilForceBasicLoad" "/Logger:trx;LogFileName=p3-t11-taskmaster.trx" "/ResultsDirectory:coverage\trx\p3-t11-taskmaster"`; write `<FEATURE>/evidence/regression-testing/caller-batch-3.md`.
  Acceptance: the artifact records all three exact commands and exit codes; all exit 0; exact totals are recorded; both formerly hosted-CI-failing paths pass without OneDrive environment variables.

### Phase 4 — Causal Closure, Scope, and Acceptance Checks

- [x] [P4-T1] Re-run the constructor-call census using the P0-T4 commands plus `rg -n "ApplicationGlobals\("` over the eight in-scope test files; write `<FEATURE>/evidence/other/application-globals-constructor-census-post-change.md`.
  Acceptance: zero unadapted eager two-argument UtilitiesCS.Test call remains; the lazy-force TaskMaster.Test caller is explicitly mapped to the injected three-argument path; every one of the 22 CI failures is assigned to an adapted caller.
- [x] [P4-T2] Resolve `$vstest` with the execution-contract statement; run `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p4-t2-utilities.trx" "/ResultsDirectory:coverage\trx\p4-t2-utilities"`; then run `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p4-t2-taskmaster.trx" "/ResultsDirectory:coverage\trx\p4-t2-taskmaster"`; write `<FEATURE>/evidence/regression-testing/affected-assemblies.md`.
  Acceptance: the artifact records both exact commands and exit codes; both exit 0; exact totals are recorded; all 22 formerly failing test names pass.
- [x] [P4-T3] Run `git diff --name-only e8d8f52952f978a20ae056748e6fa9fd40b5fdb0` and `git diff --check`; write `<FEATURE>/evidence/qa-gates/cycle3-scope-lock.md`.
  Acceptance: code/test paths are limited to `TaskMaster/AppGlobals/ApplicationGlobals.cs` and the eight named test files; other paths are under `<FEATURE>/**`; `spec.md` is allowed only for the later AC24 checkbox flip and has no diff at this checkpoint; diff check exits 0; no workflow, PR, or checkpoint file is edited by the executor.
- [x] [P4-T4] Establish the pre-final-QA acceptance baseline with `rg -n "^- \[x\] \*\*AC14|^- \[ \] \*\*AC24" <FEATURE>/spec.md`, verify `git diff --exit-code -- <FEATURE>/spec.md`, and verify the checkpoint contains `issue-614-approved-documentation-findings-scope-change`; write `<FEATURE>/evidence/other/acceptance-and-scope-change-preservation.md`.
  Acceptance: AC14 is checked, AC24 is initially unchecked, the human scope-change entry is present, and `spec.md` has no premature cycle-3 diff.

### Phase 5 — Final C# QA Loop

If P5-T1 changes any file, or P5-T1 through P5-T4 fails, correct the issue and restart at P5-T1. Completion requires one uninterrupted clean pass.

**R3 coordinator adjudication (supersedes only prospective AC24-edit wording).** P4-T3 and P4-T4 are completed historical tasks whose executed result was that `spec.md` had no cycle-3 diff and AC24 was unchecked. Their prospective allowance for a later AC24 checkbox flip is withdrawn. The canonical coverage gate is the already-completed P5-T4 command `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`: its outer `dotnet-coverage --settings` invocation produces coverage while its inner VSTest invocation intentionally does not add the redundant `/EnableCodeCoverage` collector. Evidence remains under the mandatory `<FEATURE>/evidence/qa-gates/` path. The completed P5-T1 through P5-T5 results are preserved, including 6,587/6,587 tests passing and increased coverage. No additional coverage run is required solely to match stale literal wording.

- [x] [P5-T1] Run `dotnet tool run csharpier format .` and then `dotnet tool run csharpier check .`; write `<FEATURE>/evidence/qa-gates/cycle3-final-csharpier.md`.
  Acceptance: both exact commands are recorded, check exits 0, and any formatter mutation triggered a restart before continuing.
- [x] [P5-T2] Run `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; write `<FEATURE>/evidence/qa-gates/cycle3-final-analyzer-build.md`.
  Acceptance: exit 0, zero errors, warning count recorded, and compilation was not an up-to-date skip.
- [x] [P5-T3] Run `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; write `<FEATURE>/evidence/qa-gates/cycle3-final-nullable-build.md`.
  Acceptance: exit 0, zero errors, warning count recorded, and `/p:Nullable=enable` is not added.
- [x] [P5-T4] Run `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`; write `<FEATURE>/evidence/qa-gates/cycle3-final-test-coverage.md` and retain raw/filtered coverage only under `coverage/`.
  Acceptance: exit 0; 0 failed; the artifact records exact total/passed/failed counts and numeric filtered line and branch coverage with covered/valid numerators and denominators; all 22 hosted-CI failures pass.
- [x] [P5-T5] Compare P0-T9 and P5-T4 coverage in `<FEATURE>/evidence/qa-gates/cycle3-coverage-delta.md` using deduplicated Cobertura lines by filename and maximum hit count per line.
  Acceptance: the artifact records baseline and post-change line/branch values, no-regression deltas, and line/branch coverage for the new three-argument constructor and changed `LoadBasicMethod` branch; new/changed code is at least 90% line coverage and changed lines do not lose coverage; unavailable numeric values are remediation-required.
- [x] [P5-T6] Reconcile AC24 without editing `<FEATURE>/spec.md`: verify P5-T1 through P5-T5 evidence, run `rg -n "^- \[x\] \*\*AC14|^- \[ \] \*\*AC24|vstest\.console\.exe <test-assembly-paths> /EnableCodeCoverage|<FEATURE>/evidence/qa/" <FEATURE>/spec.md`, and run `git diff --exit-code -- <FEATURE>/spec.md`; update `<FEATURE>/evidence/other/acceptance-criteria-status-cycle3.md` with the full-bug acceptance summary and the R3 disposition.
  Acceptance: the artifact records 6,587/6,587 passing, the canonical P5-T4 outer `dotnet-coverage --settings` coverage result, increased numeric coverage from P5-T5, and canonical `<FEATURE>/evidence/qa-gates/` locations; AC14 remains `[x]`; AC24 remains `[ ]` solely because its stale literal command/path wording is not satisfied; `spec.md` has no diff; the artifact states explicitly that this accepted documentation/evidence risk does not waive code, test, coverage, CI, review, or orchestration-validation gates and is not evidence that AC24 passed.
- [x] [P5-T7] Re-run `(Get-Content <path>).Count` for the one production and eight test files and re-run `git diff --name-only e8d8f52952f978a20ae056748e6fa9fd40b5fdb0`; write `<FEATURE>/evidence/qa-gates/cycle3-final-size-scope.md`.
  Acceptance: all nine C# files are at most 500 lines; `spec.md` has no cycle-3 diff; the changed path set otherwise remains within P4-T3 and the R3 adjudication.
- [x] [P5-T8] Write `<FEATURE>/evidence/qa-gates/cycle3-toolchain-clean-pass.md` from P5-T1 through P5-T4.
  Acceptance: the artifact lists all four exact commands, exit codes 0, restart count, and states that the commands passed in one uninterrupted final sequence.

### Phase 6 — Final Integrity and Commit-Context Handoff

- [x] [P6-T1] Recompute SHA-256 for the two waived documentation files using the P0-T5 commands and compare with `<FEATURE>/evidence/remediation-baseline/waived-documentation-hashes.md`; write `<FEATURE>/evidence/qa-gates/cycle3-waived-documentation-integrity.md`.
  Acceptance: both hashes are byte-identical to baseline and neither file appears in the cycle-3 diff.
- [x] [P6-T2] Run a redaction sweep over all cycle-3 source/test diffs and new evidence; write `<FEATURE>/evidence/qa-gates/cycle3-redaction-sweep.md` with `SearchScope:`, `SearchPatterns:`, and `SearchResult:`.
  Acceptance: no real mailbox, user-profile, host, or organization identifier is present; only fabricated `example.com`, `testuser`, `Contoso`, or `C:\OneDrive` values are allowed.
- [x] [P6-T3] Audit all modified tests against `AGENTS.md` unit-test policy in `<FEATURE>/evidence/qa-gates/cycle3-test-policy-audit.md`.
  Acceptance: each test is deterministic, isolated, fast, independent, and uses no external service, process environment mutation, static/global hook, temporary file, sleep, or wall clock.
- [x] [P6-T4] Stage only `TaskMaster/AppGlobals/ApplicationGlobals.cs`, the eight named test files, this remediation plan/input, and cycle-3 evidence under `<FEATURE>/evidence/**`; then run `git diff --cached --check` and `git diff --cached --name-only`.
  Acceptance: cached diff check exits 0; staged paths match the declared scope; neither waived documentation file, `coverage/`, `artifacts/orchestration/orchestrator-state.json`, nor any PR/workflow file is staged.
- [x] [P6-T5] Use the repository automation adapter's authoritative `collect_commit_context` operation on the staged diff and write its canonical output to `artifacts/commit_context.txt`.
  Acceptance: the adapter exits successfully; the context names Issue #614, the one-production/eight-test scope, the red/green evidence, final QA results, and the user-approved documentation exclusions without claiming that normalized evidence row passed.
- [x] [P6-T6] Validate final handoff state with `git status --short`, `git diff --cached --check`, and `git rev-parse HEAD`; write `<FEATURE>/evidence/qa-gates/cycle3-staging-handoff.md`.
  Acceptance: HEAD is still the entry commit, only declared changes are staged, unstaged output contains no executor-owned code/test change, and no commit, push, PR edit, merge, or publication occurred.

## Completion Checklist

- [x] Exact-head CI failure evidence records 6,586 total, 6,564 passed, and 22 OneDrive-root failures.
- [x] The deterministic regression failed before implementation and passed afterward unchanged.
- [x] Existing production constructors and `ThisAddIn` retain real-environment D7 fail-fast behavior.
- [x] All eight test files use explicit in-memory injection for the affected construction paths.
- [x] The post-change census accounts for every one of the 22 hosted failures.
- [x] CSharpier, analyzer rebuild, nullable rebuild, and full coverage tests passed in one clean sequence.
- [x] Numeric baseline, post-change, and new/changed-code coverage evidence is complete.
- [x] All nine modified C# files remain at or below 500 lines.
- [x] AC14 remains checked; AC24 remains unchecked and is reported as an accepted documentation/evidence wording risk without weakening any delivery gate; the checkpoint scope-change record remains present.
- [x] The two waived documentation/evidence files are byte-identical to baseline.
- [x] Staged paths and canonical commit context are ready for orchestrator commit stewardship.
- [x] No commit, push, PR edit, merge, or publication was performed by the executor.
