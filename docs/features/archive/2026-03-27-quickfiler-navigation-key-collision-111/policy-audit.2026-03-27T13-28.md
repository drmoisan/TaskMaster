# Policy Audit — quickfiler-navigation-key-collision-111 (2026-03-27T13-28)

- **Feature folder:** `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/`
- **Current branch inspected:** `bug/quickfiler-navigation-key-collision-111` @ `40f176c1cd207a5a5971698d0e9ae762080de926`
- **Base branch:** `main` @ `cb6a6edd11590c245d36ccba16ca5c4c6732ce8f`
- **Work mode source:** `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md` declares `- Work Mode: minor-audit`, so `issue.md` is the sole acceptance-criteria source.
- **Feature folder selection rule:** Used the user-specified active feature folder because it exists on disk, matches branch issue suffix `-111`, and contains the active `issue.md` plus `plan.2026-03-27T12-45.md`.
- **Supersedes:** `policy-audit.2026-03-27T13-11.md`, which was written before branch-scope remediation and before the current `main...HEAD` range was revalidated.
- **Template note:** No repository template matching `docs/features/templates/policy_audit/policy-audit.yyyy-MM-ddTHH-mm.md` was present. This audit uses the repository's established policy-audit artifact structure and records the missing template as a process note only.
- **PR context note:** The canonical `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` bundle remains stale for this branch/base pair. The VS Code collector command was not available from this tool environment, so this re-review used a direct git-based equivalent (`git log main..HEAD`, `git diff main...HEAD`, scoped diff hunks, and a clean working-tree check) as the baseline-diff source of truth. That fallback is sufficient and non-blocking for this review because the branch range is a single commit and the working tree is clean.

## Verdict

**PASS — Ready for PR review against `main`.**

The branch is now correctly scoped to the intended QuickFiler duplicate-key fix relative to `main`, the `minor-audit` requirements source is populated and already checked off consistently, the feature-folder plan/evidence chain is internally consistent, and the repository C# QA loop is green. No additional remediation is required from this review.

## Audit summary

| Area | Status | Result | Evidence |
|---|---|---|---|
| Policy reading order | ✅ | PASS | Reviewed `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, and `csharp-unit-test.instructions.md` before finalizing this audit. |
| Work mode / AC source selection | ✅ | PASS | `issue.md` declares `minor-audit`; this audit used `issue.md` only. |
| Minor-audit integrity: `issue.md` exists | ✅ | PASS | `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md` exists and contains concrete bug details plus an explicit `## Acceptance Criteria` section. |
| Minor-audit integrity: `spec.md` absent | ✅ | PASS | No `spec.md` exists in the feature folder. |
| Minor-audit integrity: `user-story.md` absent | ✅ | PASS | No `user-story.md` exists in the feature folder. |
| Minor-audit integrity: Phase 0 policy-read artifact | ✅ | PASS | `evidence/baseline/phase0-instructions-read.2026-03-27T12-52.md` exists and includes `Timestamp:`, `Policy Order:`, and the required file-read list. |
| Minor-audit integrity: baseline command artifacts | ✅ | PASS | `p0-t3-format`, `p0-t4-analyzers`, `p0-t5-nullable`, and `p0-t6-tests-with-coverage` all exist with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` fields. |
| Plan checklist vs artifact state | ✅ | PASS | `plan.2026-03-27T12-45.md` is evidence-backed as written: `P0-T3` only required schema-valid capture of the environment-specific formatter-command failure, and `P1-T2` explicitly allowed the direct test-platform fallback after the known single-assembly `Invoke-MSTest.ps1` script failure. |
| Baseline diff isolation relative to `main` | ✅ | PASS | `git log main..HEAD` shows a single scoped commit (`40f176c fix(quickfiler): prevent substring key collisions`), and `git diff --name-status main...HEAD` is limited to `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler.Test/Controllers/KbdActionsTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, and the matching feature-folder evidence/docs. |
| Production scope guardrails | ✅ | PASS | No `QfcCollectionController.cs` change appears in `main...HEAD`, matching the issue note that no compatibility edit was required outside `KbdActions.cs`. |
| C# unit-test conventions | ✅ | PASS | The new focused tests use MSTest attributes and FluentAssertions; no framework drift or dependency creep was introduced. |
| Formatter validation | ✅ | PASS | Canonical QA artifact `p2-t1-format.2026-03-27T13-08.md` records `EXIT_CODE: 0`; the live review-time `dotnet tool run csharpier check .` rerun also exited `0`. |
| Analyzer validation | ✅ | PASS | Canonical QA artifact `p2-t2-analyzers.2026-03-27T13-08.md` records `EXIT_CODE: 0` with `16 Warning(s)` and `0 Error(s)`; the live review-time analyzer rerun also completed successfully. |
| Nullable validation | ✅ | PASS | Canonical QA artifact `p2-t3-nullable.2026-03-27T13-08.md` records `EXIT_CODE: 0` with `0 Warning(s)` and `0 Error(s)`; the live review-time nullable rerun also completed successfully. |
| Test validation | ✅ | PASS | Canonical QA artifact `p2-t4-tests-with-coverage.2026-03-27T13-08.md` records `EXIT_CODE: 0`, `2877 total`, `2875 passed`, `2 skipped`, `0 failed`; the live review-time coverage rerun also completed successfully. |
| Coverage metrics availability | ✅ | PASS | Numeric baseline and post-fix coverage values are available in `p0-t6-tests-with-coverage.2026-03-27T12-52.md`, `p2-t4-tests-with-coverage.2026-03-27T13-08.md`, and the current `coverage/coverage.cobertura.xml`. |

## Key evidence

### Canonical feature evidence

- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/plan.2026-03-27T12-45.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/phase0-instructions-read.2026-03-27T12-52.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t3-format.2026-03-27T12-52.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t4-analyzers.2026-03-27T12-52.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t5-nullable.2026-03-27T12-52.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t6-tests-with-coverage.2026-03-27T12-52.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/regression-testing/p1-t2-kbdactions-distinct-keys.2026-03-27T13-01.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/p2-t1-format.2026-03-27T13-08.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/p2-t2-analyzers.2026-03-27T13-08.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/p2-t3-nullable.2026-03-27T13-08.md`
- `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/p2-t4-tests-with-coverage.2026-03-27T13-08.md`
- `coverage/coverage.cobertura.xml`

### Representative code evidence

- `QuickFiler/Controllers/KbdActions.cs`
- `QuickFiler.Test/Controllers/KbdActionsTests.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`

## Coverage metrics required by policy

| Metric | Value | Evidence |
|---|---:|---|
| Baseline overall line coverage | 61.54% | `evidence/baseline/p0-t6-tests-with-coverage.2026-03-27T12-52.md` |
| Final QA overall line coverage | 61.61% | `evidence/qa-gates/p2-t4-tests-with-coverage.2026-03-27T13-08.md` |
| Current review-time overall line coverage | 61.57% | `coverage/coverage.cobertura.xml` root `line-rate` |
| Baseline QuickFiler package line coverage | 22.18% | `evidence/baseline/p0-t6-tests-with-coverage.2026-03-27T12-52.md` |
| Final QA QuickFiler package line coverage | 22.50% | `evidence/qa-gates/p2-t4-tests-with-coverage.2026-03-27T13-08.md` |
| Current `KbdActions.cs` file line coverage | 46.43% | `coverage/coverage.cobertura.xml` class filename `QuickFiler\Controllers\KbdActions.cs` |
| Current `KbdActions.cs` file branch coverage | 31.25% | `coverage/coverage.cobertura.xml` class filename `QuickFiler\Controllers\KbdActions.cs` |

Coverage note: `KbdActions.cs` is a pre-existing low-coverage file. Consistent with prior small-path QuickFiler review precedent, this audit treats the targeted regression tests and positive coverage delta as sufficient for PASS because the changed behavior is directly exercised and overall coverage did not regress.

## Appendix A — live review-time commands run

Check-only / review commands executed from `C:\Users\DanMoisan\repos\TaskMaster` during this re-review:

1. `git branch --show-current`
2. `git rev-parse HEAD`
3. `git rev-parse main`
4. `git merge-base main HEAD`
5. `git status --short --branch`
6. `git log --date=short --pretty=format:'%h %ad %an %s' main..HEAD`
7. `git diff --name-status main...HEAD`
8. `git diff --stat main...HEAD`
9. `git diff --name-status main...HEAD -- QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler.Test/Controllers/KbdActionsTests.cs docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111`
10. `git diff --unified=3 main...HEAD -- QuickFiler/Controllers/KbdActions.cs`
11. `git diff --unified=3 main...HEAD -- QuickFiler.Test/Controllers/KbdActionsTests.cs`
12. `dotnet tool run csharpier check .`
13. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
14. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
15. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

## Appendix B — live review-time outputs

- Branch status: clean working tree (`## bug/quickfiler-navigation-key-collision-111`).
- Commit range: one scoped commit — `40f176c 2026-03-27 Dan Moisan fix(quickfiler): prevent substring key collisions`.
- Diff scope: changed code limited to `KbdActions.cs`, new `KbdActionsTests.cs`, the test project compile include, and feature-folder evidence/docs.
- Formatter check: exited `0`; CSharpier checked the repository and only warned that `TaskMaster_BACKUP_1250.csproj` is invalid XML and was skipped.
- Analyzer build: completed successfully; summary tail reported `16 Warning(s)` and `0 Error(s)`.
- Nullable build: completed successfully; summary tail reported `0 Warning(s)` and `0 Error(s)`.
- Coverage-enabled MSTest: completed successfully; review-time rerun exited successfully and refreshed `coverage/coverage.cobertura.xml`; canonical feature QA artifact records `2877 total`, `2875 passed`, `2 skipped`, `0 failed`, overall line coverage `61.61%`.

## Recommendation

**Ready for PR review against `main`.**

The branch now matches the intended `#111` feature scope, the `minor-audit` issue doc and plan are audit-grade, and the C# QA loop is green. The earlier `13-11` audit/remediation files are historical artifacts from the pre-remediation review and are no longer the authoritative assessment.