# Policy Compliance Audit: sort-email-attachment-test-creates-directory-at-repository-root (#945)

**Audit Date:** 2026-09-30

**Code Under Test:** `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (modified, production);
`UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` (modified, test).

- Review timestamp: 2026-09-30T13-00
- Reviewer: feature-review agent
- Feature folder: `docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945`
- Branch under review: `bug/sort-email-attachment-test-creates-directory-945`
- Head SHA: `cb502eeec14dc32ba087149bd90693ef4a5fc0a0` (read from the branch ref on disk)
- Resolved base: `origin/main` at merge, merge base `039cf779110df3313b3324299d019cabfccce980` (caller-supplied; consistent with the recorded footprint evidence)
- Work mode: `minor-audit` (marker `- Work Mode: minor-audit` at `issue.md` line 12)
- Acceptance-criteria source: the `## Acceptance Criteria` section of `issue.md` (AC1 through AC8)

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 2 files | 7331 tests | 7331 pass, 0 fail | 85.34% lines, 79.73% branches | 85.33% lines, 79.71% branches | 96.0% |
| PowerShell | 0 files | 0 tests | N/A | N/A | N/A | N/A |
| Python | 0 files | 0 tests | N/A | N/A | N/A | N/A |
| TypeScript | 0 files | 0 tests | N/A | N/A | N/A | N/A |

**Unit of the C# New Code Coverage figure.** The 96.0% figure is the per-file line coverage of
`SortEmail.cs` (24 of 25 measured lines), identical in the baseline and final Cobertura documents. The
56 changed production lines are not inside that denominator: both `TrySaveAttachmentAsync` overloads
carry a method-level `[ExcludeFromCodeCoverage]` (the attribute pre-existed on the two-parameter
method and was retained on both by the recorded scope decision). The figure therefore proves that the
change added no uncovered measured line (per-file uncovered-line delta 0); it is not a line
percentage of the changed lines. See section 5.3 and finding N-2.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `<FEATURE>/evidence/baseline/coverage-baseline.md` (projection of `coverage/baseline-945.cobertura.xml`, not committed)
- C# post-change coverage artifact: `<FEATURE>/evidence/qa-gates/coverage-final.md` (projection of `coverage/final-945.cobertura.xml`, not committed)
- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed`
- PowerShell baseline coverage artifact: `N/A - zero PowerShell files changed`
- PowerShell post-change coverage artifact: `N/A - zero PowerShell files changed`
- Python baseline coverage artifact: `N/A - zero Python files changed`
- Python post-change coverage artifact: `N/A - zero Python files changed`
- Per-language comparison summary: section 1.2.1 of this document

---

## Executive Summary

Issue #945 is a test-hygiene defect: `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile`
derived its destination from `GetRepositoryRoot()` and therefore reached the real
`Directory.CreateDirectory`, creating a directory in the working tree. The delivered change adds an
`internal static` overload of `TrySaveAttachmentAsync` that takes an `Action<string> createDirectory`
delegate, reduces the two-parameter method to a one-line delegation that supplies the real
`Directory.CreateDirectory`, and rewrites the test to use a rooted in-memory literal plus a recording
delegate. A second test proves that an `IOException` from the delegate propagates and that
`SaveAsFile` is never called.

Footprint: two source files (one production, one test) plus the feature folder. No project file,
policy document, analyzer configuration, coverage threshold, or exclusion list is changed.

Verdict: **PASS**, 0 blocking findings, 6 non-blocking findings (section 8). All eight acceptance
criteria are supported by evidence on disk. The C# toolchain passed in one consecutive pass. Final
first-party coverage is 85.33% lines and 79.71% branches, above the 85% and 75% floors. The
`SortEmail.cs` per-file uncovered-line delta is 0.

### Method and verification basis

This review used the Read, Grep and Glob tools only, as instructed. No `git diff`, test run, or
coverage run was executed by the reviewer. Each claim below is labelled **re-derived** (read directly
from the source or evidence on disk) or **evidence-attested** (taken from a committed evidence
artifact that cannot be re-derived without running the toolchain). The canonical coverage XML
documents under `coverage/` and `artifacts/` are not committed, consistent with the
"Committed Test Evidence Format" rule in `CLAUDE.md`, so every coverage figure is evidence-attested
from the two committed projections.

## Rejected Scope Narrowing

**None detected.** The delegation named the full branch footprint against the merge base and did not
limit the audit to a plan, task, phase or file subset. Caller statements that were checked and found
accurate rather than narrowing:

1. "This item writes no PowerShell file; no PowerShell gate applies." Verified: the recorded footprint
   (`evidence/qa-gates/p2-t10-scope-boundary.2026-09-30T12-40.md`) lists no `.ps1` path; PowerShell has
   zero changed files.
2. "The four local-stall classes are excluded locally and run in CI." Verified as a recorded
   measurement condition (same exclusion filter in both baseline and final); recorded as finding N-4,
   not treated as a reduction of audit scope.
3. "The canonical coverage XML files are not committed." Verified; recorded as finding N-5.

## Evidence Location Compliance

All 34 evidence artifacts live under `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,other}/`,
which are canonical kinds. A Glob over `artifacts/` inside the item worktree returned no
`pr_context*` or evidence files, so nothing exists under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/` or `artifacts/coverage/`. `validate_evidence_locations.py` was not run (Bash not
used in this review). **PASS** (re-derived by Glob).

Host-identifier hygiene: a Grep over the whole feature folder for the account name, the `C:\Users`
path and the profile tokens returned no matches. A drive-letter sweep (`[A-Za-z]:[\\/]`) returned only
the literal `C:\Sortemail945Sandbox\attachments` test path (research file, negative-control message,
census tokens) and one `https://` URL; none is a host path or identity. `Program Files` and `AppData`
returned no match; the word `vswhere` appears in command descriptions and is a tool name, not a path.
The executor's own sweep (`evidence/qa-gates/p2-t11-hygiene-sweep.2026-09-30T12-41.md`) reports 0 for
every token class and 0 raw documents. **PASS** (re-derived).

---

## 1. General Unit Test Policy Compliance

Source: `.claude/rules/general-unit-test.md` and `CLAUDE.md` UT1 to UT5.

### 1.1 Core Principles

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| Independence | **PASS** | Re-derived. Each new or rewritten test builds its own `events` list and mock. No test touches `_removeReadOnly` (the shared static in `SortEmail.cs`) because none throws `UnauthorizedAccessException`; a Grep over `UtilitiesCS.Test/EmailIntelligence` shows the only `UnauthorizedAccessException` reference in a throwing position is in `OSBrowser_Tests.cs`, unrelated to this path. |
| Isolation | **PASS** | Re-derived. One behaviour per test: ordered mkdir-then-save; exception propagation without save. |
| Fast execution | **PASS** | Evidence-attested. Scoped class run of 15 tests completed within the standard run (`evidence/regression-testing/test-run-final.md`). |
| Determinism | **PASS** | Re-derived. No `Thread.Sleep`, `Task.Delay`, clock or random API in the test file (Grep returned no match). The recorded events are appended on the calling thread before `Task.Run` and on the pool thread strictly after the awaited start, so the list is not raced. |
| Readability | **PASS** | Re-derived. Both tests carry an XML summary stating scenario and expectation, explicit Arrange, Act, Assert comments and descriptive names. |

### 1.2 Coverage and Scenarios

- Line coverage >= 85%: final first-party 85.33% (56104/65750), evidence-attested. **PASS**
- Branch coverage >= 75%: final first-party 79.71% (13594/17054), evidence-attested. **PASS**
- No regression on changed lines: the changed lines are attribute-exempt; per-file uncovered-line delta for `SortEmail.cs` is 0 (25 valid, 24 covered, 1 uncovered in both documents). **PASS**
- Scenario completeness: positive flow (save succeeds), error flow (directory creation fails). The `UnauthorizedAccessException` retry branch is intentionally not tested because it reaches the modal `YesNoToAll.ShowDialog`; the exception is stated in `issue.md` (AC4 and the scope decision). Recorded as called-out exception.

#### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.34% lines (56110/65750) -> Post-change: 85.33% lines (56104/65750). Change: -0.01% lines (-6 covered lines, identical denominator, inside the 0.10 collector-variance tolerance ratified for AC7; SortEmail.cs per-file uncovered-line delta 0). New/changed-code coverage: 96.0%. Disposition: PASS. Evidence: `<FEATURE>/evidence/baseline/coverage-baseline.md`, `<FEATURE>/evidence/qa-gates/coverage-final.md`.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.

#### 1.2.2 Coverage Artifact State and Disposition

C# coverage: lines 85.33%, branches 79.71% (first-party, final) **PASS** against the 85% and 75%
floors. The `UtilitiesCS` package line rate moved 0.894157 to 0.894019 and the branch rate 0.835152 to
0.834886. The six covered lines lost (38827 to 38821) and three covered branches lost (9413 to 9410)
are not in `SortEmail.cs`, whose per-file counters are unchanged, so the movement is attributed
to run-to-run collector variance (likely, not proven; see N-3). The AC7 tolerance of 0.10 percentage
points, ratified by the orchestrator at preflight, is respected: deficits are 0.0138, 0.0266 and
0.0091 percentage points.

### 1.3 Test Structure and Diagnostics

**PASS.** Re-derived. Arrange, Act, Assert comments are present. `events.Should().Equal(...)`
produces a readable sequence diff on failure; the negative control recorded the literal message
`Expected events to be equal to {"mkdir:...", "save:..."}, but {"save:..."} contains 1 item(s) less`.

### 1.4 External Dependencies and Environment

**PASS.** Re-derived. The test file contains no `Directory.`, `File.` write, `GetTemp*` or
`CreateDirectory` token (Grep over `SortEmail_Tests.cs` returned no match). `Path.Combine` and
`Path.GetDirectoryName` are pure string operations. The destination
`C:\Sortemail945Sandbox\attachments` is an in-memory literal only. The executor recorded
`SANDBOX-EXISTS-BEFORE: False` and `SANDBOX-EXISTS-AFTER: False` around the scoped run and around the
negative control (evidence-attested). No temporary file is created. The remaining four
`GetRepositoryRoot()` uses in the file (lines 196, 223, 295, 320) perform no write, per the recorded
scope decision and the research file; `GetAttachmentsInfoAsync` only enumerates and does not reach
`SaveAttachmentAsync` (re-derived from `SortEmail.cs` lines 662 to 763).

### 1.5 Policy Audit Requirement

**PASS.** Both tests were reviewed against UT1 to UT4 in this document.

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes and Bugfix Workflow

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| Plan documented | **PASS** | Re-derived. `plan.2026-09-30T07-20.md` exists; a Grep for unchecked task boxes returned none. |
| Failing test first | **PASS with a recorded substitute** | Evidence-attested. A runtime-red run of the pre-fix behaviour would execute the real `Directory.CreateDirectory` or reach a modal dialog, so the plan records a compile-red dossier (`fail-before-exception.2026-09-30T12-24.md`: exit 1, CS1501 four times) plus a side-effect-free negative control (`negative-control-createdirectory-removed.md`: 2 of 2 fail). The substitution is reasoned and recorded. |
| Minimal targeted fix | **PASS** | Re-derived. Production change is one new overload, one delegation, one call-site substitution, and one argument pass-through. No caller edited (call sites at `SortEmail.cs` lines 819, 864, 879 still use the two-argument form). |
| Open a new issue rather than widen scope | **PASS** | Re-derived. The four other `GetRepositoryRoot()` uses were left alone; follow-ups are listed in section 8, not filed. |

### 2.2 Design Principles

**PASS.** Re-derived. The injectable delegate seam is the second preference under
`.claude/rules/csharp.md` "DI Seams" and is the smallest seam that removes the file-system effect. The
default path remains `System.IO.Directory.CreateDirectory`, so behaviour is unchanged for production
callers. No static settable delegate or shared mutable seam is introduced (AC2).

### 2.3 Module and File Structure

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| 500-line file limit, production | **FAIL, pre-existing, non-blocking** | Re-derived. `SortEmail.cs` has 1454 lines (Grep line count). The caller records 1429 before the change, so this item added 25 lines to a file that already breached the limit by 929 lines. The breach is not introduced by this item. Recorded as N-1. |
| 500-line file limit, test | **PASS** | Re-derived. `SortEmail_Tests.cs` has 457 lines. |
| Small public surface | **PASS** | Re-derived. The new overload is `internal static`; `internal` was already the accessibility of the method. |

### 2.4 Naming, Docs and Comments

**PASS.** Re-derived. `createDirectory` is a descriptive camelCase parameter. The new overload carries an
XML summary (`SortEmail.cs` lines 906 to 911) that states the delegate contract and its purpose; the
wrapper carries a summary naming the overload as the test seam. The test file carries a comment
explaining why the rooted literal is safe (lines 236 to 237).

### 2.5 After Making Changes: Toolchain Execution

**PASS.** Evidence-attested. `evidence/qa-gates/toolchain-pass.md` records one iteration with a clean
pass: format (0 rewritten), format check, analyzer `/t:Rebuild` (`SKIP_CORECOMPILE_LINES: 0`),
nullable `/t:Rebuild` (`SKIP_CORECOMPILE_LINES: 0`), and the coverage-route test run (exit 0).

### 2.6 Summarize and Document

**PASS.** The plan, research file, evidence tree and `issue.md` check-offs are present and consistent.

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C#: C# Code Change Policy Compliance

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| CSharpier via `dotnet tool run` | **PASS** | Evidence-attested. `p2-t1-csharpier-format` (0 rewritten) and `p2-t2-csharpier-check` (exit 0). The source and test formatting is consistent with CSharpier output shape (re-derived by reading). |
| Analyzer gate with `/t:Rebuild` | **PASS** | Evidence-attested. `p2-t3-msbuild-analyzers.2026-09-30T12-31.md`; `SKIP_CORECOMPILE_LINES: 0`, so the gate was not vacuous. |
| Nullable gate without `/p:Nullable=enable` | **PASS** | Evidence-attested. `p2-t4-msbuild-nullable.2026-09-30T12-32.md`; `/t:Rebuild`, `SKIP_CORECOMPILE_LINES: 0`. |
| Explicit contracts, nullable discipline | **PASS** | Re-derived. `Action<string> createDirectory` is explicit. `Path.GetDirectoryName` may return null for a root path; that behaviour pre-exists and is unchanged. |
| Async and exception handling | **PASS** | Re-derived. The two-parameter method is now non-async and returns the core task directly, which avoids an extra state machine and preserves exception propagation. The recursive retry passes `createDirectory` through (line 961). The catch filter remains `UnauthorizedAccessException` only, so an `IOException` propagates as the test asserts. |
| Suppression scan | **PASS** | Re-derived. No `#pragma warning disable` and no `SuppressMessage` was added. The two `[ExcludeFromCodeCoverage]` attributes are discussed in section 8 (N-2). |

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C#: C# Unit Test Policy Compliance

| Requirement | Verdict | Evidence and basis |
|---|---|---|
| MSTest | **PASS** | Re-derived. `using Microsoft.VisualStudio.TestTools.UnitTesting;` (line 10); `[TestMethod]` on both tests (lines 245, 272). |
| Moq | **PASS** | Re-derived. `CreateAttachmentMock` returns a `Mock<Attachment>`; `Setup(...).Callback<string>(...)` and `Verify(..., Times.Once / Times.Never)`. |
| FluentAssertions | **PASS** | Re-derived. `saved.Should().BeTrue()`, `events.Should().Equal(...)`, `act.Should().ThrowAsync<IOException>()`. No MSTest `Assert.*` added. |
| Toolchain command selection | **PASS** | See sections 2.5 and 7. |

---

## 5. Test Coverage Detail

### 5.1 Coverage Artifact Resolution

- **C#** - 2 changed source files. The canonical `artifacts/csharp/coverage.xml` is not present in the item worktree and the raw Cobertura documents (`coverage/baseline-945.cobertura.xml`, `coverage/final-945.cobertura.xml`) are not committed. The committed projections `evidence/baseline/coverage-baseline.md` and `evidence/qa-gates/coverage-final.md` carry the JaCoCo package counters, the one-line first-party summary, and the `CMD-PACKAGE-COMPARE` output; they are the evidence of record, per the "Committed Test Evidence Format" rule. Figures were **read from those projections**, not re-derived from raw XML.
- **PowerShell, Python, TypeScript** - 0 changed files; no artifact required.

### 5.2 Repository-Wide C# Figures

| Metric | Baseline | Post-change | Floor | Row verdict |
|---|---|---|---|---|
| First-party line coverage | 85.34% (56110/65750) | 85.33% (56104/65750) | >= 85% | PASS |
| First-party branch coverage | 79.73% (13597/17054) | 79.71% (13594/17054) | >= 75% | PASS |
| UtilitiesCS package line rate | 0.894157 (38827/43423) | 0.894019 (38821/43423) | >= 0.85 | PASS |
| UtilitiesCS package branch rate | 0.835152 (9413/11271) | 0.834886 (9410/11271) | >= 0.75 | PASS |
| Denominators | 65750 | 65750 | equal | PASS (COMPARABILITY: A) |
| `SortEmail.cs` measured lines | 25 valid, 24 covered, 1 uncovered | 25 valid, 24 covered, 1 uncovered | delta <= 0 | PASS |

### 5.3 Per-File Coverage of New and Modified Code

| File | Classification | Coverage evidence |
|---|---|---|
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | modified, production | 24 of 25 measured lines (96.0%) in both documents; per-file uncovered-line delta 0 (evidence-attested, `coverage-final.md` `SORTEMAIL-UNCOVERED-DELTA: 0`). Both changed methods carry `[ExcludeFromCodeCoverage]`, so the 56 changed lines are outside the measured denominator. Behavioural coverage of the seamed core is supplied by the two tests for the success and IOException paths. |
| `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | modified, test | Test assembly, excluded from the production denominator by design. |

---

## 6. Test Execution Metrics

- Scoped baseline, `SortEmail_Tests`: 14 tests (evidence `test-run-baseline.md`).
- Compile-red before the fix: exit 1, CS1501 four times (`fail-before-exception.2026-09-30T12-24.md`).
- Negative control (createDirectory call removed): 2 total, 2 failed, both on the expected assertion (`negative-control-createdirectory-removed.md`); the sandbox directory did not exist before or after.
- Post-restore confirmation: 2 total, 2 passed (`p1-t17-post-restore-run.2026-09-30T12-30.md`); the restore note records that the first build after the restore skipped compilation because of an older last-write time, and the run was repeated after refreshing the timestamp (metadata only, SHA-256 unchanged).
- Final scoped run: 15 total, 15 passed, 0 failed (`regression-testing/test-run-final.md`).
- Final repository-wide run, DIRECT route: 7331 total, 7331 passed, 0 failed, 0 skipped (derived), baseline 7330 (net +1, the new test).

All figures are evidence-attested; no test was executed by the reviewer.

---

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | Exit 0, 0 files rewritten (evidence-attested) |
| Format check | `dotnet tool run csharpier check .` | Exit 0 (evidence-attested) |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Exit 0, 0 skipped CoreCompile lines (evidence-attested) |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | Exit 0, 0 skipped CoreCompile lines (evidence-attested) |
| Test run with coverage | DIRECT route (`dotnet-coverage collect` over `vstest.console.exe`, 9 assemblies) | Exit 0, 7331 of 7331 passed (evidence-attested) |
| File size audit | line count of the changed files | `SortEmail.cs` 1454 (pre-existing breach); `SortEmail_Tests.cs` 457 (re-derived) |
| Evidence location scan | Glob of `artifacts/` and the feature tree | Zero non-canonical evidence paths (re-derived) |
| Host path scan | Grep of the feature folder for account, machine and `C:\Users` tokens | Zero matches (re-derived) |
| Workflow change scan | footprint listing in `p2-t10-scope-boundary` | Zero `.github/` paths; the modified-workflow rule is not triggered |
| Suppression scan (added lines) | Grep for `pragma warning disable` and `SuppressMessage` | Zero additions (re-derived) |

The local DIRECT route excluded four UtilitiesCS.Test classes (`ShellUtilities_Tests`,
`ShellUtilitiesStatic_Tests`, `SysImageListHelperTests`, `OSBrowser_Tests`) plus `LiveOutlook`
tests, identically in baseline and final. They run in CI. See N-4.

---

## 8. Gaps and Exceptions

### Identified Gaps (all non-blocking)

**N-1 - `SortEmail.cs` exceeds the 500-line limit (pre-existing; Medium).** 1454 lines at head
(`SortEmail.cs`), 1429 at the merge base per the caller. The item adds 25 lines (the new overload, its
summary, and the wrapper). Rule: `.claude/rules/general-code-change.md` "File Size Limit". The breach
pre-dates this item and the fix is a targeted seam, so splitting the file belongs to a separate
refactor. Recommendation: owe a follow-up to split the partial responsibilities (attachment saving,
message saving, folder cleanup) out of `SortEmail.cs`.

**N-2 - New overload is attribute-exempt from coverage (Low).** The new core overload carries
`[ExcludeFromCodeCoverage]`. The rationale recorded in the scope decision (the `YesNoToAll.ShowDialog`
WinForms branch remains inside the core) applies to the catch block only; the success and IOException
paths are now unit-tested but are invisible to the coverage metric, so the changed lines cannot regress
measurably. The attribute is a per-method attribute, reviewable in the PR, not a `coverage.config`
glob, so it is not a Blocking finding under the Coverage Exclusion Policy. Recommendation: extract the
`UnauthorizedAccessException` read-only prompt behind a seam so the attribute can be dropped and the
core measured.

**N-3 - Unattributed movement inside the `UtilitiesCS` package (Low).** Covered lines 38827 to 38821
(-6) and covered branches 9413 to 9410 (-3) with identical denominators, while `SortEmail.cs` is
unchanged per file. The evidence states the D-7 variance rule was not triggered and no second
measurement ran. The movement is in the magnitude range reported for collector run-to-run variance in
earlier reviews of this repository and inside the ratified 0.10 tolerance, so it is treated as variance
(likely, not proven). The pull-request CI run is the authoritative repository-wide gate.

**N-4 - Local measurement excludes four test classes (Low).** The four shell-icon and OS-browser
classes are excluded by the test-case filter in both runs, so local repository-wide figures exclude
their contribution. Baseline and final are comparable (same filter). CI runs them.

**N-5 - Raw coverage documents are not committed (Low, by design).** Figures in this audit come from
the committed projections, so a later auditor cannot re-derive them from raw XML. The AC5 byte-identical
restore is likewise evidence-attested by equal SHA-256 values
(`p2-t10-scope-boundary`: source hash equals the post-fix hash recorded at P1-T7).

**N-6 - Platform-specific literal and no null guard (Nit).** The rooted literal `C:\...` is valid on the
Windows-only net48 solution; a `createDirectory` null guard was not added to an `internal` method.
No action required.

### Approved Exceptions

- No test throws `UnauthorizedAccessException` (AC4), so the `ShowDialog` branch is unreached by design.
- Compile-red plus negative control is used in place of a runtime-red fail-before run (recorded in `fail-before-exception`).

### Removed/Skipped Tests

None removed; one test rewritten, one added (14 to 15 in the class).

### Unresolved items

None. Every review claim is labelled re-derived or evidence-attested above.

---

## 9. Summary of Changes

### Files Modified

| Path | Status | Notes |
|---|---|---|
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | Modified | Two-parameter overload becomes a non-async delegation; new `internal static` overload with `Action<string> createDirectory`; retry passes the delegate through. 1429 to 1454 lines. |
| `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | Modified | Success test rewritten with a rooted literal and a recording delegate; new IOException propagation test. 457 lines. |
| `docs/features/active/2026-09-30-...-945/**` | Added/Modified | Plan, research, `issue.md` check-offs, 34 evidence artifacts. |

Inherited and not attributable to this item: five `.claude/agent-memory/**` files and the promoted
potential-entry file, listed in `p2-t10-scope-boundary`.

## 10. Compliance Verdict

### Overall Status: COMPLIANT - PASS, 0 blocking findings

| Policy | Verdict |
|---|---|
| General Unit Test Policy | PASS |
| General Code Change Policy | PASS (file size FAIL is pre-existing, non-blocking, N-1) |
| C# Code Change Policy | PASS |
| C# Unit Test Policy | PASS |
| Coverage (C#) | PASS |
| Evidence Location Compliance | PASS |
| Tonality | PASS (plain, evidence-labelled wording; no figurative language) |

### Recommendation

Merge after the pull-request CI run confirms repository-wide coverage and the four locally excluded
test classes. No remediation inputs are produced.

---

## Appendix A: Test Inventory

`UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` - 15 `[TestMethod]`, all passing (evidence-attested).

| Test | Line | Role |
|---|---|---|
| `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` | 246 | Rewritten (AC3): ordered mkdir then save, result true, save once |
| `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave` | 273 | New (AC4): IOException propagates, no save |
| 13 other tests | various | Unchanged; four call `GetRepositoryRoot()` for read-only path values |

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .`
2. `dotnet tool run csharpier check .`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. MSTest with coverage, realised as the DIRECT route (`dotnet-coverage collect` over `vstest.console.exe` with `TaskMaster.cli.runsettings` and `/InIsolation`).
