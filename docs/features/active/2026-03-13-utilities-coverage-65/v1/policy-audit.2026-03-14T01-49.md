# Policy Audit — utilities-coverage (2026-03-14T01-49)

- **Feature folder:** `docs/features/active/2026-03-13-utilities-coverage-65/`
- **Current branch inspected:** `feature/utilities-coverage-65`
- **Base branch assumption:** `main` (no `${input:PRBaseBranch}` was provided in the prompt; this assumption is required by the review mode)
- **Work mode source:** `issue.md` declares `- Work Mode: full-feature`, so acceptance criteria were taken from `spec.md` and `user-story.md`
- **Feature folder selection rule:** Used the user-specified folder `docs/features/active/2026-03-13-utilities-coverage-65/`, which matches the issue number and contains the active scoping docs (`issue.md`, `spec.md`, `user-story.md`, `research.md`, `plan.2026-03-13T22-21.md`)

## Verdict

**Recommendation:** **Needs revision** before PR readiness.

The branch now passes format, analyzer, nullable, and full MSTest validation on re-run during this review, but the feature does **not** satisfy the documented coverage policy/acceptance target. The canonical coverage artifact records only **72 / 256** non-excluded `UtilitiesCS` files at or above **80%** coverage, and the feature plan’s Phase 8 loop is not fully reconciled with that failure.

## Audit summary

| Area | Status | Result | Evidence |
|---|---|---|---|
| Policy reading order | ✅ | PASS | Reviewed `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, `csharp-unit-test.instructions.md` before auditing. |
| Work mode / AC source selection | ✅ | PASS | `docs/features/active/2026-03-13-utilities-coverage-65/issue.md` contains `- Work Mode: full-feature`; audit used `spec.md` + `user-story.md` as AC sources. |
| Feature folder selection | ✅ | PASS | User-provided folder matches issue 65 and contains the feature scoping docs. |
| PR context artifact freshness | ⚠️ | PARTIAL | `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` are stale for this review: they describe `development` and older PRs, not this feature branch. Direct git inspection and canonical feature evidence were used instead. |
| C# test framework selection | ✅ | PASS | Test corpus uses MSTest and Moq throughout; examples include `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` and `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs`. |
| FluentAssertions preference for new/updated tests | ⚠️ | PARTIAL | Many new/updated tests use FluentAssertions, but changed files still rely on MSTest `Assert` and `[ExpectedException]`, e.g. `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs:66,274,297,319,465,486,514,532`. |
| Unit-test isolation / determinism | ⚠️ | PARTIAL | Some tests rely on real filesystem state and wall-clock timing rather than mocks or purely deterministic synchronization: `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs:92`, `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs:83`, `UtilitiesCS.Test/HelperClasses/MyFileSystemInfoTests.cs:28-29`, `UtilitiesCS.Test/ReusableTypeClasses/TimedBatchAction_Tests.cs:84`. |
| No external dependency / no file-I/O expectation in feature docs | ❌ | FAIL | `DirectoryInfoWrapper_Tests` and `FileInfoWrapper_Tests` traverse the live repo filesystem, which conflicts with the feature/user-story claim that tests avoid file I/O and external dependencies. |
| 500-line file size limit | ❌ | FAIL | Oversized test files were found: `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` (**1529** lines), `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` (**580**), `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs` (**559**), `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` (**527**). |
| Coverage policy / documented feature target | ❌ | FAIL | `evidence/qa-gates/final-per-file-coverage.2026-03-14T01-42.md` reports **184** non-excluded files below 80% and only **72** meeting target; package line-rate is **26.95%**, not the documented ≥80% per-file outcome. |
| Toolchain validation on current branch | ✅ | PASS | Review reran format, analyzer build, nullable build, and full MSTest successfully on `feature/utilities-coverage-65`. |
| Phase 8 audit-trail integrity | ⚠️ | PARTIAL | `evidence/qa-gates/final-format.2026-03-14T01-36.md` states formatting initially failed and required file modifications; `plan.2026-03-13T22-21.md` still leaves `P8-T5`, `P8-T6`, and `P8-T7` unresolved even though evidence exists showing `P8-T5` failed. |

## Key evidence

### Canonical review evidence

- `docs/features/active/2026-03-13-utilities-coverage-65/issue.md`
- `docs/features/active/2026-03-13-utilities-coverage-65/spec.md`
- `docs/features/active/2026-03-13-utilities-coverage-65/user-story.md`
- `docs/features/active/2026-03-13-utilities-coverage-65/plan.2026-03-13T22-21.md`
- `evidence/qa-gates/final-format.2026-03-14T01-36.md`
- `evidence/qa-gates/final-analyzers.2026-03-14T01-37.md`
- `evidence/qa-gates/final-nullable.2026-03-14T01-37.md`
- `evidence/qa-gates/final-test-coverage.2026-03-14T01-39.md`
- `evidence/qa-gates/final-per-file-coverage.2026-03-14T01-42.md`
- `evidence/qa-gates/final-coverage-delta.2026-03-14T01-42.md`

### Representative code evidence

- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs:22`
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs:71`
- `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs:18`
- `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs:66`
- `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs:90-92`
- `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs:83`
- `UtilitiesCS.Test/ReusableTypeClasses/TimedBatchAction_Tests.cs:10,84`

## Appendix A — commands run during this review

Check-only / validation commands executed from `C:\Users\DanMoisan\repos\TaskMaster`:

1. `dotnet format TaskMaster.sln --verify-no-changes --no-restore`
2. `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`

## Appendix B — review-time outputs

- Format verification passed in the current review run.
- Analyzer build passed: **0 warnings, 0 errors**.
- Nullable build passed: **0 warnings, 0 errors**.
- MSTest passed: **1006 total**, **1003 passed**, **0 failed**, **3 skipped**.
- Canonical coverage artifact still fails the feature target: **72 / 256** non-excluded files ≥80%; `UtilitiesCS` package line-rate **26.95296651165694%**.

## Final recommendation

Do **not** mark this feature ready for merge as-is. The current branch is build-clean and test-clean, but it is **policy-incomplete** because the feature’s documented coverage goal is not met, the 500-line file limit is exceeded in multiple test files, and a subset of tests still depend on real filesystem / timing behavior instead of fully isolated patterns.
