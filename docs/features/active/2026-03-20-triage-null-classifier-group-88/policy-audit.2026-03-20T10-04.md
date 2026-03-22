# Policy Audit — triage-null-classifier-group-88 (2026-03-20T10-04)

- **Feature folder:** `docs/features/active/2026-03-20-triage-null-classifier-group-88/`
- **Current branch inspected:** `bug/triage-null-classifier-group-88`
- **Base branch:** `development` (resolved by merge-base recency against `origin/development`, `origin/main`, `development`, and `main`)
- **Work mode source:** `docs/features/active/2026-03-20-triage-null-classifier-group-88/issue.md` declares `- Work Mode: minor-audit`, so `issue.md` was treated as the sole acceptance-criteria source.
- **Feature folder selection rule:** Used `docs/features/active/2026-03-20-triage-null-classifier-group-88/` because it matches the user-supplied active folder, matches issue `#88`, and contains the active `issue.md`, plan, and canonical evidence folders for this bug.
- **PR context note:** The canonical `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` bundle was stale for a different branch (`feature/outlook-folder-wrapper-tests-82`). Per best-effort review constraints, this audit used direct git evidence plus the issue `#88` feature folder artifacts instead of the stale PR-context bundle.
- **Template note:** The preferred template `docs/features/templates/policy_audit/policy-audit.yyyy-MM-ddTHH-mm.md` was not present in this workspace, so this artifact uses the repo’s established policy-audit structure from recent feature folders.

## Verdict

**PASS — Code-level policy compliance is satisfied for the issue #88 implementation, with environment-limited validation caveats documented.**

The touched C# files follow the repo’s C# change and unit-test policies: the fix is minimal, null-safety improved, the new tests use MSTest plus FluentAssertions, and the new test file is explicitly included in `UtilitiesCS.Test.csproj`. The strict repo-wide toolchain loop could not be cleanly re-established in this review session because the environment hit a VSTO `FindRibbons` Application Control failure and the full MSTest sweep hit a pre-existing `StackOverflowException`, but focused post-change build/test evidence for the affected project passed and no diagnostics were reported in the changed files.

## Audit summary

| Area | Status | Result | Evidence |
|---|---|---|---|
| Policy reading order | ✅ | PASS | `evidence/baseline/phase0-instructions-read.md` records the required order: `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, `csharp-unit-test.instructions.md`. |
| Minor-audit AC source selection | ✅ | PASS | `issue.md` declares `minor-audit`; this audit used `issue.md` as the sole requirements source. |
| Minimal targeted production change | ✅ | PASS | `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage.cs` swaps `new BayesianClassifierGroup()` for `CreateClassifier()` inside `CreateNewTriageClassifierGroupAsync()`. |
| Defensive null-safety change | ✅ | PASS | `TaskMaster/AppGlobals/AppItemEngines.cs` adds `.Where(tup => tup.Engine is not null)` before `ToConcurrentDictionaryAsync(...)`, preventing null engine storage. |
| C# unit-test framework/library policy | ✅ | PASS | `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/TriageCreationTests.cs` uses MSTest `[TestClass]` / `[TestMethod]` and FluentAssertions; no new mocking dependency was needed. |
| Explicit test compile registration | ✅ | PASS | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` explicitly includes `EmailIntelligence\ClassifierGroups\Triage\TriageCreationTests.cs`. |
| Changed-file diagnostics | ✅ | PASS | `get_errors` reported no editor diagnostics in the four touched files. |
| Formatter policy execution | ⚠️ | PASS WITH CAVEAT | `final-qc-format.md` shows the touched files were formatted successfully, but repo-wide `csharpier check .` still fails on pre-existing unrelated formatting debt already present in `baseline-csharpier.md`. |
| Analyzer policy execution | ⚠️ | PASS WITH CAVEAT | `final-qc-analyzer-build.md` failed on an environment-specific VSTO `FindRibbons` / Application Control block after reaching `TaskMaster.dll`; focused `UtilitiesCS.Test` build still passed in `focused-utilitiescs-test-build.md`. |
| Nullable/type-safety execution | ⚠️ | PASS WITH CAVEAT | `final-qc-nullable-build.md` hit the same environment-specific VSTO block before reporting changed-file nullability issues; baseline nullable build had previously passed and no editor diagnostics exist in changed files. |
| Test execution policy | ⚠️ | PASS WITH CAVEAT | `final-qc-test.md` aborted because of a pre-existing `StackOverflowException` and an Application Control block while loading another test assembly, but `focused-triage-regression-tests.md` confirms the two new regression tests passed and `focused-utilitiescs-test-build.md` confirms they compiled. |

## Key evidence

### Canonical feature evidence

- `docs/features/active/2026-03-20-triage-null-classifier-group-88/issue.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/plan.2026-03-20T09-38.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/baseline/phase0-instructions-read.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/baseline/baseline-csharpier.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/baseline/baseline-analyzer-build.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/baseline/baseline-nullable-build.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/baseline/baseline-test.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/qa-gates/final-qc-format.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/qa-gates/final-qc-analyzer-build.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/qa-gates/final-qc-nullable-build.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/qa-gates/final-qc-test.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/qa-gates/focused-utilitiescs-test-build.md`
- `docs/features/active/2026-03-20-triage-null-classifier-group-88/evidence/qa-gates/focused-triage-regression-tests.md`

### Representative code evidence

- `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage.cs`
- `TaskMaster/AppGlobals/AppItemEngines.cs`
- `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/TriageCreationTests.cs`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

## Appendix A — commands reviewed from evidence

1. `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`
5. `& <MSBuild.exe> .\UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug "/p:Platform=AnyCPU" /p:BuildProjectReferences=false`
6. `& <vstest.console.exe> .\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:CreateClassifier_ReturnsGroupWithClassifiersABC,CreateClassifier_ReturnsGroupWithNonNullSharedTokenBase /InIsolation`

## Appendix B — git scope note

Direct git inspection against merge-base `7e8a585ce6d1db1ae02334aede0977149be18ab1` showed this branch currently contains additional unrelated changes relative to `development` outside issue `#88`. Those extra changes were not evaluated as part of this bug’s code-level policy compliance; they should be isolated before opening a branch-wide PR to `development` if the intent is a single-issue review.

## Recommendation

**Pass this small-path policy audit for the issue #88 implementation itself.**

Before opening a PR from the whole branch to `development`, isolate or rebase away the unrelated branch changes so the PR scope matches this audit.