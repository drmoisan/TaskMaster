# Policy Audit — quickfiler-navigation-key-collision-111

- **Timestamp:** 2026-03-27T13-11
- **Feature folder:** `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111`
- **Feature folder selection rule:** Used the user-specified active feature folder because it exists on disk, matches branch issue suffix `-111`, and contains the expected minor-audit files `issue.md` and `plan.2026-03-27T12-45.md`.
- **Branch:** `bug/quickfiler-navigation-key-collision-111`
- **Base branch (PRBaseBranch):** `main`
- **Work mode:** `minor-audit`
- **Auditor:** feature_code_review_agent (2026-03-27T13-11)

## PRBaseBranch Resolution

Base branch: `main`.

This audit used the explicit user-supplied `PRBaseBranch` value `main`. No fallback was required.

## PR Context Artifact Status

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are stale for this audit run: both still point to a different branch/base combination and do not describe `bug/quickfiler-navigation-key-collision-111` relative to `main`.

The preferred collector command was not available in this tool environment, so this audit used direct git evidence instead:

- `git log --date=short --pretty=format:'%h %ad %an %s' main..HEAD`
- `git diff --name-status main...HEAD`
- `git diff --name-status main...HEAD -- 'QuickFiler/Controllers/KbdActions.cs' 'QuickFiler/Controllers/QfcCollectionController.cs' 'QuickFiler.Test/Controllers/KbdActionsTests.cs'`

## Policy Compliance Order Applied

1. `.github/copilot-instructions.md`
2. `.github/instructions/general-code-change.instructions.md`
3. `.github/instructions/general-unit-test.instructions.md`
4. `.github/instructions/csharp-code-change.instructions.md`
5. `.github/instructions/csharp-unit-test.instructions.md`

Evidence: `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/phase0-instructions-read.2026-03-27T12-52.md`

---

## Section A — C# Toolchain Loop

All four review-time steps were run fresh on the current branch at audit time.

| Step | Command | Status | Evidence |
|---|---|---|---|
| **A1 Format** | `dotnet tool run csharpier check .` | ✅ PASS | Exit code `0`; `Checked 971 files in 3176ms.`; invalid XML warning only for skipped `TaskMaster_BACKUP_1250.csproj`; git status unchanged after the check-only run. |
| **A2 Lint** | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` | ✅ PASS | Exit code `0`; `Build succeeded.`; `0 Warning(s)`; `0 Error(s)`. |
| **A3 Type-check** | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` | ✅ PASS | Exit code `0`; `Build succeeded.`; `0 Warning(s)`; `0 Error(s)`. |
| **A4 Test + Coverage** | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | ✅ PASS | Exit code `0`; `2877` total, `2875` passed, `0` failed, `2` skipped; Cobertura headline line coverage `61.61%` and branch coverage `46.46%`. |

**Toolchain verdict:** The current branch is build-clean and test-clean, but these results apply to the branch as currently composed, not to the requested small-path `KbdActions` fix scope.

---

## Section B — Minor-Audit Boundary and Evidence Integrity

| Check | Status | Notes |
|---|---|---|
| **B1 Minor-audit source boundary** — `issue.md` is the sole requirements source and unexpected scoping docs are absent | ✅ PASS | The active feature folder contains `issue.md` and `plan.2026-03-27T12-45.md`. No `spec.md` or `user-story.md` exists in the folder, so the branch satisfies the structural boundary requirement. |
| **B2 Requirements-source completeness** — `issue.md` contains concrete, reviewable requirements for issue `#111` | ❌ FAIL | `issue.md` is still an unfilled template: `## Summary` says `One or two sentences on what is broken.`, `## Steps to Reproduce` contains `1. ...`, `## Expected Behavior` and `## Actual Behavior` are placeholders, and there is no explicit acceptance-criteria section for the duplicate-key bug. In `minor-audit` mode this is the authoritative source, so the audit must fail closed. |
| **B3 Branch scope matches the documented small-path fix** | ❌ FAIL | `git diff --name-status main...HEAD` does not include `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler.Test/Controllers/KbdActionsTests.cs`, or any file under `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/`. The range instead contains `QfcQueue` files and archived-doc moves, and `git log main..HEAD` shows merge/content for issue `#106`, not `#111`. |
| **B4 Completed plan checklist items are evidence-backed** | ❌ FAIL | `plan.2026-03-27T12-45.md` marks `[x]` for `P0-T3`, but `evidence/baseline/p0-t3-format.2026-03-27T12-52.md` records `EXIT_CODE: 1` for the exact plan command `dotnet tool run csharpier .`; the artifact explicitly says the command failed because the required subcommand was missing. The checklist is therefore not evidence-backed as written. |
| **B5 Fail-before regression evidence satisfies the literal plan acceptance for `P1-T2`** | ❌ FAIL | `evidence/regression-testing/p1-t2-kbdactions-distinct-keys.2026-03-27T13-01.md` shows the approved focused command `Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test` failed before dispatching tests because of the repository script's single-assembly `.Count` bug. The actual regression failure was captured only by an immediate fallback invocation against `QuickFiler.Test.dll`, so the plan's exact acceptance wording was not met. |
| **B6 Final QA evidence exists for the active feature folder** | ✅ PASS | The folder contains clean Phase 2 QA artifacts for format, analyzer, nullable, and coverage-enabled MSTest passes under `evidence/qa-gates/`. |

---

## Section C — C# Code and Test Policy Review

| Check | Status | Notes |
|---|---|---|
| **C1 Targeted production/test scope present in `main...HEAD`** | ❌ FAIL | There is no `main...HEAD` diff for the planned small-path files, so the requested bug fix cannot be reviewed as branch content relative to `main`. |
| **C2 C# QA commands appropriate and passing** | ✅ PASS | The branch passed formatter check, analyzer build, nullable build, and MSTest with coverage using repository-approved commands. |
| **C3 Minor-audit evidence chain is complete enough for merge review** | ❌ FAIL | Requirements are incomplete in `issue.md`, the documented plan has at least one checked task with failing evidence, and the fail-before evidence for the focused regression does not satisfy the literal task acceptance. |

---

## Section D — Baseline Facts from `main...HEAD`

### Commits in range

```
e5fdba7 2026-03-27 drmoisan Merge pull request #110 from drmoisan:bug/qfc-queue-remove-item-cancellation-106
f4a799c 2026-03-27 Dan Moisan (fix(qfc-queue)): handle pre-cancelled RemoveItem cleanup
dc7dcc8 2026-03-27 drmoisan Merge pull request #109 from drmoisan:chore/archive-docs
da79776 2026-03-27 Dan Moisan (chore): archived docs from completed features
0664cd1 2026-03-27 drmoisan Merge pull request #108 from drmoisan:main
```

### Requested-scope diff check

```
git diff --name-status main...HEAD -- QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler.Test/Controllers/KbdActionsTests.cs

(no output)
```

### Feature-folder diff check

```
git diff --name-status main...HEAD -- docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/**

(no output)
```

---

## Appendix A — Commands Run for This Audit

| Purpose | Command |
|---|---|
| Timestamp + branch/base/diff snapshot | `git rev-parse --abbrev-ref HEAD; git rev-parse main; git rev-parse HEAD; git merge-base main HEAD; git diff --name-status main...HEAD; git diff --unified=80 main...HEAD -- 'QuickFiler/Controllers/KbdActions.cs'; git diff --unified=80 main...HEAD -- 'QuickFiler.Test/Controllers/KbdActionsTests.cs'` |
| Commit verification | `git log --date=short --pretty=format:'%h %ad %an %s' main..HEAD` |
| Scoped diff verification | `git diff --name-status main...HEAD -- 'docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/**'` and `git diff --name-status main...HEAD -- 'QuickFiler/Controllers/KbdActions.cs' 'QuickFiler/Controllers/QfcCollectionController.cs' 'QuickFiler.Test/Controllers/KbdActionsTests.cs'` |
| Format check | `dotnet tool run csharpier check .` |
| Analyzer build | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` |
| Nullable build | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` |
| Test + coverage | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` |

---

## Recommendation

**❌ BLOCKED / Needs revision before PR review.**

The current branch is not review-ready for the requested QuickFiler duplicate-key fix relative to `main`. The `main...HEAD` range does not contain the planned `KbdActions` production/test changes or the active feature-folder changes, the sole `minor-audit` requirements source is still an unfilled template, and the checked plan state is not fully supported by its own evidence. Remediation is required before a reliable feature review against `main` can pass.