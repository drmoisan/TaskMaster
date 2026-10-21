# Policy Compliance Audit: tracked-csproj-bak-files-carry-stale-project-content (#951)

**Audit Date:** 2026-10-01

**Code Under Test:** none. No production or test source file changed. The change set is eight file deletions
(`*.csproj.bak`), one added line in `.gitignore`, and files under `docs/features/`.

- Review timestamp: 2026-10-01T12-40
- Reviewer: feature-review agent
- Feature folder: `docs/features/active/2026-09-30-tracked-csproj-bak-files-carry-stale-project-content-951`
- Branch under review: `bug/tracked-csproj-bak-files-carry-stale-project-content-951`
- Head SHA: `cca7ad4438d6e7082a37d50160c53f6ca9cf5c76` (`git rev-parse HEAD`)
- Resolved base: `origin/main` at `6c710a45dd61710658ea8e60588fb4108cf15b6f`; `git merge-base --is-ancestor origin/main HEAD` exited 0, so origin/main is an ancestor of HEAD and the two-dot form `git diff origin/main` is the correct diff
- Work mode: `minor-audit` (marker `- Work Mode: minor-audit` at `issue.md` line 12)
- Acceptance-criteria source: the `## Acceptance Criteria` section of `issue.md` (AC-1 through AC-5)

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 0 files | 0 tests | N/A | N/A | N/A | N/A |
| PowerShell | 0 files | 0 tests | N/A | N/A | N/A | N/A |
| Python | 0 files | 0 tests | N/A | N/A | N/A | N/A |
| TypeScript | 0 files | 0 tests | N/A | N/A | N/A | N/A |

No coverage gate applies to this branch. No executable code changed in any coverage language, so the
coverage verification procedure has no changed file to evaluate. This is a statement about zero changed
files per language (confirmed by `git diff origin/main --name-status`), not a narrowing of audit scope.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `N/A - zero C# files changed`
- C# post-change coverage artifact: `N/A - zero C# files changed`
- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed`
- PowerShell baseline coverage artifact: `N/A - zero PowerShell files changed`
- PowerShell post-change coverage artifact: `N/A - zero PowerShell files changed`
- Python baseline coverage artifact: `N/A - zero Python files changed`
- Python post-change coverage artifact: `N/A - zero Python files changed`
- Per-language comparison summary: section 1.2.1 of this document

---

## Executive Summary

Issue #951 reports eight tracked `*.csproj.bak` files that carry superseded project content (two still
contain the altcover import removed by #929). The delivered change removes the eight files from the index
and working tree and adds the single line `*.csproj.bak` to `.gitignore`, immediately after the existing
`*.rptproj.bak` line (`.gitignore` line 258).

Footprint against `origin/main` (`git diff origin/main --name-status`): `M .gitignore`, eight `D`
entries for the `*.csproj.bak` paths, and `A` entries only under `docs/features/active/<feature>/`
and `docs/features/potential/promoted/`. `.gitignore` shows `1 0` in `--numstat` (one line added, none
removed). No source, project, workflow, script, or policy file changed.

Verdict: **PASS**, 0 blocking findings, 2 non-blocking observations (section 8). All five acceptance
criteria were verified independently with git commands run by the reviewer.

### Method and verification basis

The reviewer ran git commands directly (`diff`, `ls-files`, `check-ignore --no-index`, `grep`,
`merge-base`) and used Read, Grep and Glob. No build, test or coverage run was executed because no code
changed. Claims are labelled **re-derived** (reviewer-executed) unless stated otherwise.

## Rejected Scope Narrowing

**None detected.** The delegation named the full branch diff against `origin/main` and did not limit the
audit to a plan, task, phase or file subset. The statement "No coverage gate applies (no code changed)"
was verified against the diff (zero C#, PowerShell, Python, TypeScript files) and is accurate rather than
a narrowing.

## Evidence Location Compliance

All 28 evidence artifacts live under `<FEATURE>/evidence/{baseline,other,qa-gates}/`, which are canonical
kinds. The diff `--name-status` listing contains no path under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/` or `artifacts/coverage/`. `validate_evidence_locations.py` was not run (single
git commands only were permitted). **PASS** (re-derived from the diff listing).

Host-identifier hygiene: a Grep over the whole feature folder for drive-letter paths
(`[A-Za-z]:[\\/]`), the account name, `/Users/`, `AppData` and `Program Files` returned two matches, both
benign: the GitHub issue URL in `issue.md` (`https://github.com/...`, matched by the `:/` pattern) and
the fetch summary in `p0-t3-fetch` (`From https://github.com/...`). Neither is a host path or
identity. **PASS** (re-derived). This artifact set was also written without host paths.

---

## 1. General Unit Test Policy Compliance

Source: `.claude/rules/general-unit-test.md` and `CLAUDE.md` UT1 to UT5.

### 1.1 Core Principles

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| Independence, Isolation, Fast execution, Determinism, Readability | **PASS (not applicable to content)** | Re-derived. The diff adds and modifies no test file (`git diff origin/main --name-status` lists no `*.Test/` path other than deleted `*.Test.csproj.bak` backups). No test behavior changes. |

### 1.2 Coverage and Scenarios

- Line and branch coverage floors: no executable code changed; no coverage figure is affected. **PASS (no changed code)**
- No regression on changed lines: there are no changed code lines. **PASS**
- Scenario completeness: no new behavior to test. The verification is repository-state checks (AC-1 to AC-5), which are reproduced in section 7.

#### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A - zero C# files changed. Evidence: `git diff origin/main --name-status` lists no `.cs` file.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A - zero PowerShell files changed. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A - zero Python files changed. Evidence: N/A - zero Python files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A - zero TypeScript files changed. Evidence: N/A - zero TypeScript files changed on this branch.

#### 1.2.2 Coverage Artifact State and Disposition

Not applicable. No language has a changed file, so no coverage artifact is required and none was
inspected. No `exclude` entry or coverage configuration changed (`coverage.config`, runsettings and
workflows are absent from the diff), so the Coverage Exclusion Policy is not engaged.

### 1.3 Test Structure and Diagnostics

Not applicable; no test changed.

### 1.4 External Dependencies and Environment

**PASS.** No test, temporary file or external dependency introduced.

### 1.5 Policy Audit Requirement

**PASS.** No unit test was added or modified, so no per-test review is owed.

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes and Bugfix Workflow

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| Plan documented | **PASS** | Re-derived. `plan.2026-10-01T07-10.md` exists; a Grep for unchecked `- [ ]` boxes returned 0 matches. |
| Failing test first | **PASS with a recorded substitute** | The defect is repository hygiene (tracked files), not runtime behavior; no test can express it. The plan records baseline state captures (`evidence/baseline/p0-t5-ac1-baseline`, `p0-t12-check-ignore-baseline`) showing the eight files tracked and not ignored before the change, which serve as the before-state. |
| Minimal targeted fix | **PASS** | Re-derived. Eight deletions plus one `.gitignore` line (`--numstat` `1 0`). |
| Open a new issue rather than widen scope | **PASS** | The optional hygiene-guard extension named in `issue.md` "Proposed Fix" was not implemented, and no script changed. |

### 2.2 Design Principles

**PASS.** Re-derived. The rule is the narrowest ignore pattern that satisfies the issue
(`*.csproj.bak`, not `*.bak`), so it cannot shadow the three tracked non-csproj backups (AC-3, section 7).

### 2.3 Module and File Structure

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| 500-line file limit | **PASS** | No production, test or script file changed. `.gitignore` and Markdown evidence are outside the limit's scope. |
| Small public surface | **PASS** | Not applicable; no code surface. |

### 2.4 Naming, Docs and Comments

**PASS.** The ignore rule sits adjacent to `*.rptproj.bak` and follows its existing naming pattern.

### 2.5 After Making Changes: Toolchain Execution

**PASS (no code in scope).** The C# toolchain (format, analyzer build, nullable build, tests) targets
`*.cs`, `*.csproj`, `*.props` and `*.targets` content. The eight deleted files are not part of any build
(AC-4: no reader exists), and no `*.csproj` changed, so the build graph is unchanged. The executor ran the
repository hygiene guard (`evidence/qa-gates/p2-t14-hygiene-guard.2026-10-01T12-16.md`; evidence-attested).

### 2.6 Summarize and Document

**PASS.** Plan, research note, evidence tree and `issue.md` check-offs are present and consistent.

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C#: C# Code Change Policy Compliance

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| CSharpier, analyzer, nullable gates | **PASS (no C# source or project file changed)** | Re-derived. The diff contains no `.cs`, `.csproj`, `.props` or `.targets` file; only `.csproj.bak` deletions, which are not compiled. |
| Suppression scan | **PASS** | No added line contains a suppression; the only added source-tree line is `*.csproj.bak`. |

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C#: C# Unit Test Policy Compliance

**PASS (not applicable).** No test file changed. MSTest, Moq and FluentAssertions conventions are not engaged.

---

## 5. Test Coverage Detail

### 5.1 Coverage Artifact Resolution

- **C#, PowerShell, Python, TypeScript** - 0 changed files each; no artifact required and none inspected.

### 5.2 Repository-Wide Figures

Not applicable. No executable code changed, so repository-wide coverage is unchanged by construction. The
pull-request CI run remains the authoritative repository-wide gate.

### 5.3 Per-File Coverage of New and Modified Code

| File | Classification | Coverage evidence |
|---|---|---|
| `.gitignore` | modified, configuration | Not code; no coverage applies. |
| Eight `*.csproj.bak` | deleted | Deleted backup copies; not compiled; no coverage applies. |

---

## 6. Test Execution Metrics

No test was run: no code changed. The verification commands for AC-1 to AC-5 were run by the reviewer and
are listed in section 7 with their results.

---

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| AC-1 tracked count | `git ls-files -- "*.csproj.bak"` (via `git ls-files -- "*.bak"`) | Only `TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak`, `TaskVisualization/TaskVisualization.vbproj.bak`; zero `*.csproj.bak` (re-derived) |
| AC-1 working tree | Glob `*.csproj.bak` under the worktree | No files found (re-derived) |
| AC-1 deletions | `git diff origin/main --name-status` | Eight `D` entries for the listed paths (re-derived) |
| AC-2 rule text | `git diff origin/main -- .gitignore` and `--numstat` | One added line `+*.csproj.bak`; numstat `1 0` (re-derived) |
| AC-2 attribution | `git check-ignore -v --no-index <eight paths>` | All eight report `.gitignore:258:*.csproj.bak` (re-derived) |
| AC-3 shadowing | `git ls-files -ci --exclude-standard -- "*.bak"` | No output (re-derived) |
| AC-3 untouched | `git diff origin/main --stat` for the three remaining `.bak` paths | No output, so unmodified (re-derived) |
| AC-4 readers | `git grep -n -i -E "csproj\.bak\|\*\.bak"` over `scripts`, `.github`, `.claude/lib`, `*.csproj`, `*.sln`, `*.targets` | No output (re-derived). Control: `git grep -c -F "csproj.bak" -- .gitignore` returned 1, so the search form matches when a hit exists. |
| AC-5 footprint | `git diff origin/main --name-status` | Only `.gitignore`, eight deletions, and `docs/features/**` (re-derived) |
| Evidence location scan | diff listing | Zero non-canonical evidence paths (re-derived) |
| Host path scan | Grep over the feature folder | Zero host paths (re-derived) |
| Spec/user-story absence | Glob over the feature folder | No `spec.md` or `user-story.md`; consistent with `minor-audit` (re-derived) |
| Working tree cleanliness | `git status --short` | No output (re-derived) |

---

## 8. Gaps and Exceptions

### Identified Gaps (all non-blocking)

**N-1 - Three other tracked backup files remain (Low, by design).** `TaskMaster.sln.bak`,
`TaskTree/TaskTree.vbproj.bak` and `TaskVisualization/TaskVisualization.vbproj.bak` stay tracked because
the issue scope names only `*.csproj.bak` (AC-3 requires them to remain tracked and unmodified). A broader
`*.bak` rule would have shadowed them. Recommendation: file a separate follow-up if their removal is wanted.

**N-2 - Optional hygiene-guard extension not delivered (Low, by design).** The issue lists extending the
#927 repository hygiene guard to flag tracked backup files as optional and it is not an acceptance
criterion. Recurrence of tracked backups is prevented only by the ignore rule, which does not stop
`git add -f`. Recommendation: owe a follow-up if guard coverage is desired.

### Approved Exceptions

- Compile-and-test gates are not run because the diff contains no compiled content.

### Removed/Skipped Tests

None.

### Unresolved items

None.

---

## 9. Summary of Changes

### Files Modified

| Path | Status | Notes |
|---|---|---|
| `.gitignore` | Modified | One line added: `*.csproj.bak` (line 258). |
| Eight `*.csproj.bak` files (`QuickFiler`, `QuickFiler.Test`, `Tags`, `TaskTree`, `TaskVisualization`, `TaskVisualization.Test`, `ToDoModel`, `ToDoModel.Test`) | Deleted | Stale backups removed from index and working tree. |
| `docs/features/active/<feature>/**`, `docs/features/potential/promoted/<entry>.md` | Added | Issue, plan, research note, 28 evidence artifacts, promoted potential entry. |

## 10. Compliance Verdict

### Overall Status: COMPLIANT - PASS, 0 blocking findings

| Policy | Verdict |
|---|---|
| General Unit Test Policy | PASS (no test changed) |
| General Code Change Policy | PASS |
| C# Code Change Policy | PASS (no C# source or project file changed) |
| C# Unit Test Policy | PASS (no test changed) |
| Coverage (all languages) | N/A - zero changed files in every coverage language; no coverage gate applies |
| Evidence Location Compliance | PASS |
| Tonality | PASS |

### Recommendation

Merge after the pull-request CI run is green. No remediation inputs are produced.

---

## Appendix A: Test Inventory

No test was added, modified or removed by this branch.

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .` (not engaged; no C# source change)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (not engaged)
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (not engaged)
4. MSTest with coverage via the Koverage task (not engaged)

Reviewer verification commands: `git diff origin/main --name-status`, `git ls-files`, `git ls-files -ci --exclude-standard`,
`git check-ignore -v --no-index`, `git grep -n -i -E`, `git merge-base --is-ancestor`, `git status --short`.
