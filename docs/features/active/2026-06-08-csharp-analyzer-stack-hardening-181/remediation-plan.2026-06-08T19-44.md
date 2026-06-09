# Remediation Plan: csharp-analyzer-stack-hardening (Issue #181) — Cycle 3

- Cycle entry timestamp: 2026-06-08T19-44
- Feature folder: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- Work mode: `full-feature` (resolved from `issue.md` metadata `- Work Mode: full-feature`)
- Base branch: `main` @ `2a522ed831865c2918ab02df153ef2929b0617dc`
- Head branch: `feature/csharp-analyzer-stack-181` @ `0883d0f7367844f16ede7d48972a91886aaff5be`
- PR: https://github.com/drmoisan/TaskMaster/pull/182
- Authoritative inputs: `remediation-inputs.2026-06-08T19-44.md`
- Evidence root (canonical, non-overridable): `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/<kind>/`

## Scope (single, narrow)

Apply CSharpier 1.2.6 formatter output to exactly ONE test file,
`ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs`, so the whole-repo CI step
`dotnet csharpier check .` ("Verify formatting", the first step of the required
"Format, build, analyze, and test" check) exits 0. The defect is a
feature-branch-introduced formatting violation: branch commit `0883d0f7`
re-enabled the `Constructor_WithOutlookItem_ShouldInitializeProperties` regression
test by commenting out its `[TestCategory("ProductionBugSuspected")]` and
`[Ignore("ProductionBugSuspected")]` markers, leaving the first commented line at
7-space indentation where CSharpier expects 8. Whole-repo `dotnet csharpier check .`
reports exactly ONE unformatted file (1057 checked). This is a pure formatting
change: no logic, behavior, attribute-state, comment-content, or public-API change.

## Downstream Risk (must verify locally before push)

Commit `0883d0f7` re-enabled two previously-ignored regression tests:
- `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` — `Constructor_WithOutlookItem_ShouldInitializeProperties`
- `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` — un-ignored regression test

CI failed at the formatting step (step 1), so build/analyze/test never ran. After
the formatting fix unblocks those steps, the two re-enabled tests execute for the
first time on CI. The local toolchain run (MSTest-with-coverage) MUST confirm both
tests pass before push. If either test fails, that is a NEW finding that triggers a
follow-up cycle per the scope-change rule — escalate it; do NOT re-ignore, weaken,
or skip those tests to force green.

## Scope Guard (do not do — from remediation-inputs.2026-06-08T19-44.md)

- Do NOT modify the logic, attributes, or comment content of `ToDoItemTests.cs`; apply only CSharpier whitespace formatting output.
- Do NOT re-add `[Ignore]`/`[TestCategory]` or otherwise re-disable the re-enabled regression tests to force green.
- Do NOT touch any other `.cs` source file; only this one file is unformatted per the whole-repo CSharpier check.
- Do NOT alter the analyzer-stack build-config delivered earlier in this feature.
- Do NOT introduce any CS8032 suppression or re-add SecurityCodeScan.
- Do NOT touch the four vendored projects (SVGControl, SVGControl.Test, UtilitiesSwordfish.NET.General, UtilitiesSwordfish.Test).
- Do NOT promote RS0030 or any analyzer rule from suggestion to warning/error.
- Do NOT modify `.claude/rules/` policy documents.
- Do NOT weaken or skip any CI gate to force green.

---

### Phase 0 — Policy Reads and Baseline Capture

- [x] [P0-T1] Read the repository policy documents in the mandatory order defined by `policy-compliance-order`: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, and `.claude/rules/ci-workflows.md`. Record an evidence artifact at `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/phase0-instructions-read.2026-06-08T19-44.md` containing `Timestamp:`, `Policy Order:` (the ordered list), and an explicit list of files read. Acceptance: artifact exists and lists all five files in the policy order.
- [x] [P0-T2] Capture the current branch HEAD SHA and clean working-tree state. Run `git rev-parse HEAD` and `git status --porcelain`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-git-state.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the HEAD SHA and whether the tree is clean. Acceptance: artifact records HEAD SHA equal to `0883d0f7367844f16ede7d48972a91886aaff5be` (or the current head if advanced) and a clean tree prior to the fix.
- [x] [P0-T3] Capture the fail-before formatting baseline. Run `dotnet tool restore` then `dotnet tool run csharpier check .`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-format.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: artifact records a non-zero `EXIT_CODE`, names `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` as "Was not formatted" (CSharpier 1.2.6), and records exactly one unformatted file out of the total checked. This is the pre-fix failing-gate proof for the formatting AC.
- [x] [P0-T4] Capture the pre-fix `*.cs` diff scope against base. Run `git diff --name-only main..HEAD -- "*.cs"`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-diff-scope.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` listing the changed `.cs` files (which must include `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` as the file carrying the re-enable edit). Acceptance: artifact records the pre-fix changed-`.cs` set as the diff-scope baseline.
- [x] [P0-T5] Capture the pre-fix re-enabled-test state baseline. Run `git diff main..HEAD -- "ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs" "ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs"`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-reenabled-tests.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording that commit `0883d0f7` commented out the `[Ignore]`/`[TestCategory]` markers re-enabling `Constructor_WithOutlookItem_ShouldInitializeProperties` (and the un-ignored `PeopleScoDictionaryNewTests` regression test). Acceptance: artifact records the two re-enabled test identifiers as the downstream-risk baseline to be validated after the formatting fix.

---

### Phase 1 — Apply CSharpier Formatting to the Single File

- [x] [P1-T1] Apply CSharpier 1.2.6 formatter output to `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` only, by running `dotnet tool run csharpier "ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs"`. Do not hand-edit; the formatter output is authoritative (correct the line-111 comment indentation from 7 to 8 spaces). Acceptance: the command exits 0 and `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` is the only file modified (whitespace only — no logic, attribute-state, comment-content, or public-API token changes).
- [x] [P1-T2] Verify the formatting gate now passes repo-wide. Run `dotnet tool run csharpier check .`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/regression-testing/format-check-after-fix.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `EXIT_CODE: 0` and the summary reports no unformatted files (pass-after proof; pairs with the P0-T3 fail-before baseline).
- [x] [P1-T3] Verify diff scope is exactly one test file and is formatting-only. Run `git diff --name-only main..HEAD -- "*.cs"` and `git diff main..HEAD -- "ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs"`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/regression-testing/diff-scope-after-fix.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: the formatting fix at `ToDoItemTests.cs` changes only the line-111 comment indentation (7→8 spaces); the `[Ignore]`/`[TestCategory]` markers remain commented out (re-enabled state preserved); no token, identifier, attribute-state, or comment-content change is introduced by the formatting application.

---

### Phase 2 — Final QA Loop (Full Toolchain) and AC Reconciliation

- [x] [P2-T1] Final-QC step 1 (format). Run `dotnet tool run csharpier check .`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-format.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `EXIT_CODE: 0`. If this step changes any file, restart the loop from P2-T1.
- [x] [P2-T2] Final-QC restore. Run `nuget restore TaskMaster.sln`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-restore.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `EXIT_CODE: 0` and packages restore without error.
- [x] [P2-T3] Final-QC step 2 (analyzer / code-style build). Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-analyzer-build.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `EXIT_CODE: 0` (build succeeds; analyzer diagnostics remain at suggestion severity per the delivered config; no new first-party diagnostics introduced by the formatting change).
- [x] [P2-T4] Final-QC step 3 (nullable type-check, warnings-as-errors). Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-nullable-build.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: the result holds at the established vendored-only baseline — zero first-party errors, zero CS8032 — with no new errors attributable to the formatting change (AC5: nullable gate does NOT regress).
- [ ] [P2-T5] Final-QC step 4 (MSTest suite with coverage). Discover first-party `*.Test.dll` assemblies under `bin\Debug` (excluding `obj`/`ref`, and excluding the vendored SVGControl.Test and UtilitiesSwordfish.Test) and run `vstest.console.exe <first-party test dlls> /EnableCodeCoverage /InIsolation /Logger:trx`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-test-coverage.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including the numeric repository-wide line-coverage headline and pass/fail counts. Acceptance: no test regression beyond the documented flaky wall-clock-timer tests; repository-wide line coverage remains `>= 80%`; the formatting-only change does not reduce coverage on the touched file's lines. **EXECUTED — ACCEPTANCE NOT MET (HALTED at P2-T6):** the command ran (TRX + .coverage recorded; 4054 passed / 10 failed); one failure is the re-enabled regression test `People_Deserialize_CanDeserializePatternCorrectly` (genuine assertion failure, not a flaky timer), so the acceptance condition is not satisfied and this task remains unchecked.
- [ ] [P2-T6] Verify the two re-enabled regression tests execute and PASS. From the P2-T5 `.trx` results (or by re-running the two test methods with `vstest.console.exe <ToDoModel.Test dll> /Tests:Constructor_WithOutlookItem_ShouldInitializeProperties` and the targeted `PeopleScoDictionaryNewTests` method), confirm `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs::Constructor_WithOutlookItem_ShouldInitializeProperties` and the un-ignored `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` regression test both ran (not Skipped/Ignored) and both passed. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/regression-testing/reenabled-tests-result.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording per-test outcome (Passed/Failed) and confirming neither was Skipped. Acceptance: both re-enabled tests show `Passed` and neither is `Skipped`/`Ignored`. If either test fails, STOP, do NOT re-ignore the test, and escalate as a new finding per the scope-change rule. **HALTED — NEW FINDING:** `Constructor_WithOutlookItem_ShouldInitializeProperties` = Passed (ran, not Skipped); `People_Deserialize_CanDeserializePatternCorrectly` = **Failed** (ran, not Skipped; `Assert.AreEqual` expected `"pplkey.json"` but `people.Config.Disk.FileName` was empty). Per the Scope-Change Escalation Rule, execution is STOPPED here. The test was NOT re-ignored/weakened/skipped; the formatting fix is applied in the working tree but NOT committed/pushed. P2-T7 through P2-T10 are NOT executed. Escalating for a follow-up remediation cycle.
- [ ] [P2-T7] Coverage delta reconciliation. Compare the P2-T5 numeric coverage against the prior accepted final coverage (`evidence/qa-gates/final-test-coverage.2026-06-08T18-06.md`) and the cycle-1 baseline (`evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`). Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/coverage-delta.2026-06-08T19-44.md` with `Timestamp:`, baseline coverage, post-change coverage, and changed-line coverage for `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs`. Acceptance: no coverage regression on changed lines; repository-wide line coverage `>= 80%` (the change is whitespace-only in a test file; re-enabled tests may increase covered production lines but must not reduce coverage).
- [ ] [P2-T8] Commit and push the formatting fix. Run `git add "ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs"`, then `git commit -m "style(todoitemtests): apply CSharpier formatting to unblock CI (#181)"`, then `git push origin feature/csharp-analyzer-stack-181`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/commit-push.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the new commit SHA and the push result. Acceptance: commit contains only `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs`; push succeeds and updates the PR #182 head branch.
- [ ] [P2-T9] Verify the required CI check is GREEN (cycle exit gate). After the push, watch the PR #182 required check "Format, build, analyze, and test" for run #215's successor run (run against the new head SHA). Run `gh pr checks 182 --watch` (or `gh run watch <run-id>`), then `gh pr checks 182` to capture the per-check final status. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/ci-green.2026-06-08T19-44.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, the CI run URL, and a per-check status table including the "Verify formatting", build, analyze, and test steps. Acceptance: the required "Format, build, analyze, and test" check concludes `success` (all steps GREEN, including the now-executing re-enabled tests). This is the authoritative AC6 confirmation and the cycle exit gate.
- [ ] [P2-T10] AC reconciliation and exit-state note. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/acceptance-summary.2026-06-08T19-44.md` with `Timestamp:` recording: the formatting gate now passes locally (P1-T2, P2-T1) and on CI (P2-T9); diff scope is exactly one test file, formatting-only (P1-T3); AC5 (nullable gate does NOT regress) corroborated by P2-T4; the two re-enabled regression tests pass (P2-T6); AC6 (PR #182 CI GREEN, all steps) confirmed by P2-T9. Acceptance: artifact records the local-gate results, the re-enabled-test results, and the CI-green confirmation; `blocking_count == 0` is supported.

---

## Toolchain Loop Restart Rule

Treat Phase 2 steps P2-T1 through P2-T5 as one toolchain pass in CLAUDE.md order
(format -> restore -> analyzer build -> nullable build -> test). If any step fails
or auto-fixes/changes any file, restart the pass from P2-T1. Do not stop the loop
while any step is failing or changing files. P2-T6 through P2-T10 (re-enabled-test
verification, coverage delta, commit/push, CI watch, AC reconciliation) run only
after a clean toolchain pass completes.

## Scope-Change Escalation Rule

If P2-T6 shows either re-enabled regression test FAILING, the failure is a new
finding outside this formatting-only cycle. Do NOT re-add `[Ignore]`/`[TestCategory]`
or otherwise re-disable the test to force green. Stop the cycle, record the failing
test evidence under `evidence/regression-testing/`, and escalate for a follow-up
remediation cycle per `remediation-handoff-atomic-planner`.

## Out-of-Scope (handled by orchestrator at cycle exit)

- `code-review.<exit-ts>.md`, `feature-audit.<exit-ts>.md`, `policy-audit.<exit-ts>.md`
  authored by `feature-review` at cycle exit after CI is green.
- Exit gate: `blocking_count == 0` (PR #182 CI GREEN; AC6 PASS, AC5 corroborated;
  both re-enabled regression tests pass).
