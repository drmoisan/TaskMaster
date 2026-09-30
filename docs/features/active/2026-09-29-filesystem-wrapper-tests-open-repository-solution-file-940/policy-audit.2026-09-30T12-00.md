# Policy Compliance Audit: filesystem-wrapper-tests-open-repository-solution-file (Issue #940)

- Component: `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`, `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` (test-only change; no production file changed)
- Branch: `bug/filesystem-wrapper-tests-open-repository-solution-file-940`
- Head under review: `bd71b8160e281054657280af8a7c54eeffe5c563` (read from the worktree's loose ref)
- Base: `origin/main` at `66afa6372fd82fc1ffd7c81f85a1ad65eebc5817`, merged into the branch at `40e587ce20bbd41cd6915707271faae937f1a8e2` (the merge changed no path under `UtilitiesCS/` or `UtilitiesCS.Test/`; P2-T7 `MERGE-UCS-DIFF-EXIT: 0`)
- Work mode: `minor-audit` (issue.md line 12); AC source is the `## Acceptance Criteria` section of issue.md only
- Review timestamp label: `2026-09-30T12-00` (assigned by the caller; later than every executor label, the last of which is `11-33`; the final commit's reflog epoch `1790782454` decodes to 2026-09-30T15:34:14Z, 11:34:14 local at UTC-4)
- Reviewer tooling: Read, Grep and Glob only; no shell was used in this review (caller instruction). Every git fact below is read from the worktree's git files (`HEAD`, the loose ref, `logs/HEAD`) and from the executor's verbatim `git diff --name-only origin/main...HEAD` listing in `evidence/qa-gates/p2-t10-scope-boundary.2026-09-30T11-27.md`.
- Blocking findings: 0. Non-blocking findings: 7 (CR-1 to CR-7 in `code-review.2026-09-30T12-00.md`). Gaps and exceptions: 3 (section 8).

## Executive Summary

- Overall verdict: **PASS**. Blocking count: **0**.
- The branch rewrites two MSTest classes so that no test locates the repository root, references `TaskMaster.sln`, catches `IOException`, creates a temporary file, or mutates a tracked repository entry. Every mutating call is directed at a Moq mock, at a path under the test assembly's own output directory that is asserted absent beforehand, or at an owned entry on which the call is a no-op by construction. Verified by reading both files in full and by the executor's token census (`evidence/qa-gates/p2-t8-post-format-census.2026-09-30T11-25.md`).
- C# is the only language with changed files. C# coverage verdict: **PASS**. First-party coverage on the merged head (MEASUREMENT 3): 85.33% lines (56092/65736) and 79.73% branches (13597/17054), against a baseline of 85.32% lines (56084/65736) and 79.73% branches (13597/17054). The three production files whose coverage the rewritten tests carry read not-lower on covered lines and covered branches; the reviewer re-read the class nodes of the two Cobertura documents on disk and confirmed the executor's figures.
- The C# toolchain passed in order: CSharpier check exit 0 (P2-T2, 1625 files), analyzer rebuild exit 0 with `SKIP_CORECOMPILE_LINES: 0` (post-merge P2-T7a), TreatWarningsAsErrors rebuild exit 0 (post-merge P2-T7b), MSTest with coverage 7327/7327 passed (MEASUREMENT 3). The CSharpier check was not re-run after the merge; the two changed files are byte-identical to the checked state (hashes equal), and the merged files arrive from origin/main whose CI format check runs on the PR. Recorded as a non-blocking gap (G-1).
- Eleven negative controls (one production-file mutation per rewritten test) each produced the predicted failing assertion and were reverted to the anchored hash; the pre-final tree was shown clean (P1-T31).
- AC8 is evaluated as amended by the coordinator ruling of 2026-09-30 (per-file lines-and-branches no-regression plus first-party floors). The amendment is treated as authorized per the caller's instruction and is recorded in issue.md line 73 and the plan's Revision Log entry A.

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The following caller statements were examined and found to be accurate descriptions of the branch diff rather than narrowing:

- "Code under review (the only source files changed against origin/main): `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` and `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs`" - confirmed against the executor's verbatim `git diff --name-only origin/main...HEAD` list (46 paths: the two `.cs` files, 43 paths under the feature folder, one path under `docs/features/potential/promoted/`).
- "This item writes no PowerShell file, so no PowerShell gate applies" - confirmed: zero `.ps1`, `.psm1` or `.psd1` paths in the diff.
- "Reduced (minor-audit) review" - this is the work mode recorded in issue.md, not a diff narrowing; the audit scope remains the full branch diff against the resolved base.

The full diff (all 46 paths) was audited: the two C# test files for policy compliance; the 43 feature-folder documents for evidence-location, hygiene and raw-document compliance; the promoted potential record as inherited content.

## Evidence Location Compliance

- Files in the branch diff under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`: **none** (0 of 46 diff paths begin with `artifacts/`; `artifacts/` is also gitignored in this repository, so no such path can be tracked).
- Every evidence artifact of this item lives under `docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/<kind>/` with `<kind>` in `baseline`, `regression-testing`, `qa-gates`, `other` (41 artifacts; each carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`; spot-checked on 30 of them).
- `validate_evidence_locations.py --root .` was not executed (no shell in this review); the manual inspection of the verbatim diff list is the substitute and found zero violations.
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none; the caller supplied no non-canonical evidence path.
- Raw tool documents: none committed. `evidence/qa-gates/p2-t11-hygiene-sweep.2026-09-30T11-29.md` records `RAW-DOCUMENT-FILES: 0` and `RAW-EXTENSION-PATHS: NONE`; the Cobertura and TRX documents remain under the git-ignored `coverage/` tree. The committed coverage evidence is the JaCoCo package projection plus the one-line first-party summary plus the TRX-derived summary, which is the form CLAUDE.md "Committed Test Evidence Format" requires.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles (UT1)

| Principle | Verdict | Evidence |
| --- | --- | --- |
| Independence | PASS | Fixtures are the running host's own loaded assembly image (`Assembly.Location`), its directory and its parent; missing paths carry a `__940_missing_` prefix and are asserted absent before every call. No test writes state another test reads. |
| Isolation | PASS (with note CR-2) | Each test targets one wrapper or adapter type; several tests assert many members in one method (pre-existing shape); FluentAssertions messages name the failing member expression. |
| Fast execution | PASS | Scoped run of the fifteen tests: `Test Run Successful`, no hang document (`test-run-final.md`); full suite collector `Total time: 54.7679 Seconds` (MEASUREMENT 3). |
| Determinism | PASS | Four full-suite runs (baseline, final, final2, final3) each passed 7323/7326/7326/7327 with `FAILED-SET: (empty)`; the fifteen scoped tests passed under the parallel regime (Workers 0, Scope ClassLevel) at P1-T7 and P2-T5. No `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize` or retry token in either file (P2-T8 census; confirmed by reading). |
| Readability | PASS | Descriptive names; each fixture block and each non-obvious act carries a why-comment. |

### 1.2 Coverage and Scenarios (UT2)

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/baseline-940.cobertura.xml` (git-ignored, on disk in the item worktree; root epoch 1790767237 = 2026-09-30T11:20:37Z) with its committed projection `evidence/baseline/coverage-baseline.md`
- C# post-change coverage artifact: `coverage/final3-940.cobertura.xml` (git-ignored, on disk; root epoch 1790781715 = 2026-09-30T15:21:55Z) with its committed projection in `evidence/qa-gates/coverage-final.md` (MEASUREMENT 3)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.32% lines (56084/65736) / 79.73% branches (13597/17054). Post-change: 85.33% lines (56092/65736) / 79.73% branches (13597/17054). Change: +0.01% lines (+8 covered lines, denominator unchanged) / +0.00% branches (0). New/changed-code coverage: 98.62%. Disposition: PASS. Evidence: `evidence/baseline/coverage-baseline.md`; `evidence/qa-gates/coverage-final.md` sections "Post-merge measurement (MEASUREMENT: 3, STAGE final3)" and "Comparison against coverage-baseline.md (MEASUREMENT: 3, STAGE final3)"; reviewer re-read of the `<coverage>` root and the three `<class>` nodes of `coverage/baseline-940.cobertura.xml` and `coverage/final3-940.cobertura.xml`.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

The C# "New/changed-code coverage" figure is the aggregate over the three production files whose coverage the rewritten tests carry, because this item changes zero executable production lines (P2-T7 `CHANGED-PRODUCTION-LINES: 0`; P2-T10 `PDA-DIFF-EXIT: 0`, `PFA-DIFF-EXIT: 0`, `DIWP-DIFF-EXIT: 0`): PhysicalDirectoryInfoAdapter.cs 91/91, PhysicalFileInfoAdapter.cs 71/75, DirectoryInfoWrapper.cs 123/123, total 285/289 = 98.62% lines; branches 45/58 = 77.59% (36/42, 6/12, 3/4), each file unchanged from baseline on branches.

C# coverage against the two floor statements in force:

- Repository rules (`.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`): line >= 85%, branch >= 75%. First-party 85.33% lines and 79.73% branches: PASS on both. The line margin is 0.33 points; this is a pre-existing repository property (baseline 85.32%), not an effect of this change.
- CLAUDE.md UT2 (maintainer-settled 2026-09-11, issue #563): line >= 80%, branch >= 75%. PASS on both.
- New modules, classes or methods (>= 90% target): none added; the two changed files are test code outside the denominator.
- No regression on changed lines: no production line changed; the per-file rule over the three carried files holds (covered lines 81 -> 91, 69 -> 71, 123 -> 123; covered branches 36 -> 36, 6 -> 6, 3 -> 3).

C# canonical artifact location note: the item worktree has no `artifacts/csharp/coverage.xml`; the session checkout carries a stale `artifacts/csharp/coverage.xml` (root epoch 1788642282 = 2026-09-05, line-rate 84.83%, branch-rate 79.14%) that belongs to an earlier branch. As C# coverage evidence for this branch that stale document is rated FAIL and was not used; it is superseded by the two on-disk Cobertura documents named in the checklist and their committed projections, which are the artifacts this audit reads. The disposition is non-blocking because the feature-folder evidence and the on-disk documents carry every figure the audit needs and were independently re-read.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 2 (test-only) | 15 scoped; 7327 full suite | 15/15 passed; 7327/7327 passed | 85.32% lines / 79.73% branches | 85.33% lines / 79.73% branches | 98.62% lines (285/289 across the three carried production files) |
| TypeScript | 0 | 0 | N/A - zero files changed | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A - zero files changed | N/A | N/A | N/A |
| Python | 0 | 0 | N/A - zero files changed | N/A | N/A | N/A |

### 1.3 Scenario Completeness

| Scenario class | Verdict | Evidence |
| --- | --- | --- |
| Positive flows | PASS | Accessor mirroring, enumeration wrapping, delegation through Moq strict mocks, delegate-seam sentinel identity. |
| Negative flows | PASS | Null constructor argument; every setter, `Delete`, `MoveTo`, `CopyTo`, `Replace` on a missing owned path throws the wrapped BCL exception type; `WrapFileSystemInfo` on an unsupported `FileSystemInfo` subtype throws `ArgumentException`. |
| Edge cases | PASS | `FileInfo.Delete()` on a missing file is asserted as the documented no-op; `Create`, `CreateSubdirectory`, `Refresh`, `SetAccessControl` on existing owned entries complete without exception. |
| Error handling | PASS | Exception types asserted with `Should().Throw<T>()`; no catch blocks. |
| Concurrency | PASS | Runs under the repository's parallel runsettings (hash `98EF03A8...` unchanged from P0-T4); no serialization attribute introduced. |
| State transitions | PASS | Moq `SetupSet` callbacks verify setter propagation on the wrapper. |

### 1.4 Test Structure (UT3) and External Dependencies (UT4)

- Arrange-Act-Assert: PASS for the seven rewritten PFS tests and the four rewritten DIW tests (explicit `// Arrange`, `// Act`, `// Assert` markers; guard preconditions sit in Arrange). The three Moq-based DIW tests, untouched by this diff, carry no markers (pre-existing; CR-3).
- Clear failure messages: PASS; FluentAssertions throughout; the `MemberCount` assertions carry a `because` clause to disambiguate from the `Length` assertion.
- External dependencies: PASS; no network, database, process or external service. File-system access is limited to read-only opens of the loaded assembly image with `FileShare.ReadWrite` or `FileShare.Read`, enumeration of the owned output directory and its parent, and the no-op and missing-path calls enumerated in `evidence/qa-gates/p2-t14-ac3-mutating-call-inventory.2026-09-30T11-31.md` (34 PFS call sites, 16 DIW call sites, 0 naming a repository path).
- Temporary files: PASS; none created (`Path.GetTemp`, `File.Create`, `File.WriteAll`, `Directory.CreateDirectory` all 0; every missing-path call throws before touching the disk or is a documented no-op; P2-T10 `ADDED-TEMP: 0`).
- Test file location: the files pre-exist under the repository's `UtilitiesCS.Test/` project layout and were neither created nor moved; the `tests/` mirror rule is not engaged by this change.
- Determinism infrastructure: no clock or RNG read; fixed `DateTime` literals only.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
| --- | --- | --- |
| Bugfix workflow (regression test first) | PASS with dossier | A committed failing test is structurally impossible (an external process holding `TaskMaster.sln` or a relocated assembly would be required); `evidence/regression-testing/fail-before-exception.2026-09-30T07-28.md` records `WhyFailingRunImpossible`, the static reproduction from the pre-edit census (`GetRepositoryRoot` 6 and 5 occurrences, `TaskMaster.sln` 13 and 3, `catch (IOException)` 2), the #906/#931 precedent, and the eleven deterministic negative controls as the observed-failing evidence. |
| Minimal targeted fix | PASS | Diff footprint is exactly the two test files plus feature-folder documents (`p2-t10-scope-boundary`: `THIS-ITEM-FOOTPRINT`, `UCS-TEST-CHANGED` = the two paths, `OUT-OF-SET-DIFF-EXIT: 0`). |
| Design principles | PASS | Fixtures reduced to three static properties; no new abstraction; the pre-existing internal delegate seam of `PhysicalFileInfoAdapter` is reused for write-mode members. |
| File size limit (500 lines, tests included) | PASS | PFS 446 lines (baseline 388), DIW 394 lines (baseline 393) (`p2-t8-post-format-census`; reviewer read both files to their final line). |
| Error handling (no swallowing) | PASS | `catch` token 0 in both files (baseline PFS 2). |
| Naming and comments | PASS | `PascalCase`/`camelCase` observed; comments explain why (owned-fixture rationale, sharing modes, seam routing). |
| Dependencies | PASS | No new package; `Moq`, `FluentAssertions`, MSTest already referenced. |
| Mandatory toolchain loop | PASS | `evidence/qa-gates/toolchain-pass.md`: `ITERATIONS: 1`, `LOOP: CLEAN PASS`, eight `EXPECTATION-MET: YES` lines. See section 7 and G-1 for the post-merge format-check caveat. |
| Supporting documents updated | PASS | issue.md AC check-off (8/8), plan check-off (64/64 tasks), Revision Log entries A and B. |

## 3. Language-Specific Code Change Policy Compliance

C# (`.claude/rules/csharp.md`, CLAUDE.md C#1 to C#7):

| Rule | Verdict | Evidence |
| --- | --- | --- |
| CSharpier via `dotnet tool run` | PASS | P2-T1 `dotnet tool run csharpier format .` exit 0, `REWRITTEN: 0`; P2-T2 `dotnet tool run csharpier check .` exit 0, `Checked 1625 files in 7661ms.` |
| Analyzer rebuild (`/t:Rebuild`, analyzers enforced) | PASS | `p2-t7-msbuild-analyzers.2026-09-30T11-19.md`: exit 0, `SKIP_CORECOMPILE_LINES: 0`, `UCS_TEST_CSC_OUT_LINES: 2`, `UCS_CSC_OUT_LINES: 2`, `WARNINGS: 0`, `ERRORS: 0`, `WRITESET_DIAGNOSTIC_LINES: 0` on head `d3f01551` (post-merge). |
| Nullable / TreatWarningsAsErrors rebuild (no `/p:Nullable=enable`, `/t:Rebuild`) | PASS | `p2-t7-msbuild-nullable.2026-09-30T11-20.md`: same field set, exit 0, zero warnings, zero errors. |
| Null safety in touched code | PASS | Test files are nullable-oblivious (no `#nullable enable`); the single `!` on the reflection lookup is in test code and guarded by the immediate `Invoke`. |
| No breaking public API change | PASS | No production file changed (three production hashes equal their `PRE-EDIT-HASH-` anchors). |
| Deterministic test rules (no PATH/cwd/profile dependence) | PASS | Fixture root is `Assembly.Location`; no working-directory assumption; no repository walk. |
| DI seams | PASS | Write-mode opens routed through the existing internal injectable-delegate seam (`PhysicalFileInfoAdapter` internal constructor) with test-owned sentinel streams. |
| Analyzer stack and suppressions | PASS | No `#pragma`, `SuppressMessage` or `.editorconfig` change in the diff. |
| Prohibited behaviors (sleeps, retries, weakened assertions) | PASS | None present; assertions strengthened (delegation now asserted by reference identity and by thrown type). |

TypeScript, Python, PowerShell: no changed files on this branch; the language-specific code-change policies are not engaged.

## 4. Language-Specific Unit Test Policy Compliance

C# Unit Test Policy (CUT1 to CUT3):

| Rule | Verdict | Evidence |
| --- | --- | --- |
| MSTest framework | PASS | `[TestClass]` and `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`; 7 + 8 test methods. |
| Moq for mocks | PASS | DIW uses `Mock<IDirectoryInfo>`, `Mock<IFileInfo>`, `Mock<IFileSystemInfo>` with `MockBehavior.Strict`; PFS exercises the physical adapters against owned fixtures and the delegate seam, which AC4 admits as "a test-owned read-only fixture". |
| FluentAssertions | PASS | Every assertion uses `.Should()`; no MSTest `Assert` call. |
| Toolchain command selection (CUT3) | PASS | Commands match CLAUDE.md character for character (section 7); the coverage route is the DIRECT `dotnet-coverage collect` wrapper around `vstest.console.exe` with the Code Coverage collector withheld, as the runner script does. |

## 5. Test Coverage Detail

Per-file coverage of the three production files whose coverage the rewritten tests carry (covered/valid; reviewer-verified against the `<class>` node `line-rate` and `branch-rate` attributes of both Cobertura documents):

| File | Baseline lines | Post-change lines | Baseline branches | Post-change branches | Verdict |
| --- | --- | --- | --- | --- | --- |
| `UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs` | 81/91 (line-rate 0.89011) | 91/91 (line-rate 1) | 36/42 (0.857143) | 36/42 (0.857143) | PASS (not lower; +10 lines) |
| `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs` | 69/75 (0.92) | 71/75 (0.946667) | 6/12 (0.5) | 6/12 (0.5) | PASS (not lower; +2 lines) |
| `UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs` | 123/123 (1) | 123/123 (1) | 3/4 (0.75) | 3/4 (0.75) | PASS (equal) |

Observations:

- The ten newly covered PDA lines recorded by the executor's read-only diagnostic are 30, 31, 42, 43, 48, 49, 54, 55, 60 and 61 (the getter and setter bodies of `CreationTimeUtc`, `LastAccessTime`, `LastAccessTimeUtc`, `LastWriteTime` and `LastWriteTimeUtc`); the two newly covered PFA lines are 52 and 53 (the `Attributes` getter and setter). These correspond to the missing-path setter assertions and the UTC accessor assertions the rewrite added (`coverage-final.md`, "Diagnostic observation"; the same twelve lines on MEASUREMENT 1 and 2).
- PFA branch coverage 6/12 (50%) is unchanged and pre-existing. The plan's revision-3 re-derivation reads the `.ctor` branch at line 27 as `50% (1/2)`; the twelve branches appear to be the six null-coalescing throw expressions of the two constructors, whose throw sides no test exercises. Below the 75% figure at file level, but not a regression and not a changed line; recorded as follow-up F-2 in the code review, non-blocking.
- Run-to-run variance in files this item does not touch (`PropertyStore.cs`, `OlTableExtensions.Etl.cs`, `SubjectMapSco.Orchestration.cs`; up to 6 lines between identical runs of the same tree) is documented in `coverage-final.md` "Two-measurement variance table" and motivated the coordinator's per-file ruling. The reviewer accepts the ruling's reasoning: the rewritten tests cannot reach those files, and MEASUREMENT 3 on the merged head restored the baseline figures for two of the three.
- Denominators are unchanged (65736 first-party lines, 17054 branches; `COMPARABILITY-MEASUREMENT-3: A`), so the +8 line delta is attributable to the two changed test files (10 + 2 gained in the carried files, minus the 4-line `SubjectMapSco.Orchestration.cs` variance recorded in `UNTOUCHED final3`).

## 6. Test Execution Metrics

| Run | Command route | Total | Passed | Failed | Evidence |
| --- | --- | --- | --- | --- | --- |
| Baseline scoped (two classes, pre-edit) | `vstest.console.exe` + CLI runsettings + `/InIsolation` | 12 | 12 | 0 | `evidence/baseline/test-run-baseline.md` |
| Baseline full suite (P0-T11) | DIRECT `dotnet-coverage collect` around vstest, 9 assemblies | 7323 | 7323 | 0 | `evidence/baseline/coverage-baseline.md` |
| Post-fix scoped (P1-T7, P2-T5) | as baseline scoped | 15 | 15 | 0 | `p1-t7-exception-type-observation`, `test-run-final.md` |
| Final full suite MEASUREMENT 1 / 2 (pre-merge) | DIRECT | 7326 / 7326 | 7326 / 7326 | 0 / 0 | `coverage-final.md` |
| Final full suite MEASUREMENT 3 (post-merge head, gating) | DIRECT | 7327 | 7327 | 0 | `coverage-final.md` |
| Negative controls C1 to C11 (mutated / reverted) | single-test vstest | 1 each | 0 / 1 each | 1 / 0 each | `evidence/regression-testing/mutation-*.md` |

Test count delta: +3 on the pre-merge tree (PFS grew from 4 to 7 test methods; DIW unchanged at 8), +1 more from the merged origin/main tree. `NEWLY-FAILING: NONE` on every full-suite run. Four UtilitiesCS.Test shell-icon classes and `OSBrowser_Tests` were excluded locally by the plan's fixed filter (an environmental hang on this workstation; CI runs them), as at #944.

## 7. Code Quality Checks

| Check | Command | Result |
| --- | --- | --- |
| Format check | `dotnet tool run csharpier check .` | exit 0, 1625 files (P2-T2, pre-merge tree; both changed files unchanged since: SHA-256 `C88A785C...` and `6650B332...` equal at P1-T5, P1-T8, P2-T1, P2-T8, P2-T10) |
| Analyzer rebuild | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, 0 warnings, 0 errors, CoreCompile ran (post-merge) |
| Nullable rebuild | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, 0 warnings, 0 errors, CoreCompile ran (post-merge) |
| Confidentiality masking scan | `CMD-SWEEP` over the feature folder (P2-T11, P2-T20) and `scripts/hygiene/Test-RepositoryHygiene.ps1` | 0 account, profile, machine, worktree-root or user-path matches over 48 and 51 files; `HYGIENE Findings=0`; reviewer Grep of the feature folder for drive-letter user paths found only the `C:\Repo\fixture` test literal |
| Suppression scan (added lines) | Grep of the diff for `#pragma`, `SuppressMessage`, `[ExcludeFromCodeCoverage]` | 0 in the two changed files |
| Workflow change scan | diff list inspection | no `.github/` path in the diff |
| Added-line prohibition census | `CMD-ADDED-LINES` (P2-T10) | 276 added lines; `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Timeout`, `Retry`, `Workers`, `Scope`, temp-file, `catch`, root-walk counts all 0 |
| Evidence timestamp fidelity | reflog epochs vs `Timestamp:` labels vs Cobertura root epochs | consistent to the minute at UTC-4: baseline collector end 07:20:37 (epoch 1790767237) vs label `07-22`; fix commit 07:34:59 vs `07-35`; MEASUREMENT 3 collector 11:21:55 vs `11-22`; closure commit 11:33:13 vs `11-33` |

## 8. Gaps and Exceptions

- G-1 (non-blocking): the CSharpier check was not re-run on the merged tree. The coordinator ruling scoped the post-merge re-run to the two rebuilds and the coverage route; the two changed files are byte-identical to the state P2-T2 checked, and the merged files come from origin/main. Disposition: CI `_format-check.yml` on the PR head is the closing gate; the orchestrator should confirm it before merge.
- G-2 (non-blocking): PR context artifacts (`artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`) are absent from the item worktree and the session checkout's copies belong to another branch (#936). The reviewer could not regenerate them without a shell. Scope was derived from the executor's verbatim three-dot diff list (P2-T10, P2-T11, P2-T20) and the caller's branch facts, all mutually consistent with the worktree reflog. The orchestrator's pr-author step regenerates the artifacts before `gh pr create`.
- G-3 (non-blocking): `validate_evidence_locations.py` was not executed (no shell); manual inspection of the 46-path diff list found no `artifacts/` path. The script run remains owed to the orchestrator's PR preflight.

No exception to the unit-test policy was claimed by the change and none is needed.

## 9. Summary of Changes

- `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` (+215 / -193 at the fix commit; 388 -> 446 lines): removed `GetRepositoryRoot`, `GetSolutionFile`, two `catch (IOException)` blocks and every mutating call on the repository root or on `TaskMaster.sln`; added three static owned-fixture properties and rewrote the four tests into seven (accessor mirroring with no-op creation; enumeration wrapping; directory setters on a missing path; missing-directory and unsupported-info branches; file properties, streams and accessors through the delegate seam; file setters on a missing path; missing-file branches).
- `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` (+32 / -31; 393 -> 394 lines): removed `GetRepositoryRoot`; retargeted the properties and `ToString` tests to the rooted test-owned literal `C:\Repo\fixture` and the two enumeration tests to the owned output directory and its parent; the constructor test and the three Moq-based tests are unchanged.
- Feature folder: 41 evidence artifacts, issue.md AC check-off and the AC8 amendment note, plan revision 1.3.
- No production file, project file, runsettings, `coverage.config` or workflow changed.

## 10. Compliance Verdict

- Overall: **PASS**.
- Blocking findings: **0**.
- Non-blocking findings: 7 (code review CR-1 to CR-7) plus 3 gaps (G-1 to G-3).
- Remediation required: **NO**. No `remediation-inputs` artifact is produced.
- PR-time gates owed to the orchestrator: CI format check on the merged head (G-1); PR context regeneration (G-2); `validate_evidence_locations.py` (G-3); follow-up promotion of the SortEmail_Tests `TrySaveAttachmentAsync` root-walk site (AC7 section 4, `same defect class`).

## Appendix A: Test Inventory

`UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` (7 tests; all Passed at P2-T5):

1. `PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory` - getters, `GetAccessControl` (both overloads), `GetObjectData`, `ToString`, `Create`, `Create(security)`, `CreateSubdirectory` (both), `Refresh`, `SetAccessControl` on the owned directory and its parent. Control C1.
2. `PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries` - the 18 enumeration overloads; wrapper types asserted. Control C2.
3. `PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries` - seven setters throw `FileNotFoundException`; `Exists` stays false. Control C3.
4. `PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected` - `Delete`, `Delete(true)`, `MoveTo` throw `DirectoryNotFoundException`; `WrapFileSystemInfo` throws `ArgumentException` for an unsupported subtype. Control C4.
5. `PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` - getters, read-only opens, seam-routed `AppendText`/`Open(mode)`/`Open(mode, access)`/`OpenWrite`, `GetAccessControl`, `GetObjectData`, `Refresh`, `SetAccessControl`, `ToString`. Control C5.
6. `PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles` - eight setters throw `FileNotFoundException`. Control C6.
7. `PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles` - `Delete` no-op; `CopyTo` (both), `MoveTo`, `Replace` (both) throw `FileNotFoundException`. Control C7.

`UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` (8 tests; all Passed at P2-T5):

1. `Constructor_WhenDirectoryInfoIsNull_ThrowsArgumentNullException` (unchanged).
2. `Properties_ShouldMirrorWrappedDirectoryInfo` (retargeted to the rooted literal). Control C8.
3. `GetDirectoriesAndGetFiles_ShouldReturnWrappedEntries` (retargeted to the owned directory and its parent). Control C9.
4. `EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles` (retargeted). Control C10.
5. `ToString_ShouldDelegateToWrappedDirectoryInfo` (retargeted). Control C11.
6. `PropertyDelegates_ShouldMirrorMockedIDirectoryInfo` (unchanged, Moq).
7. `EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo` (unchanged, Moq).
8. `LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo` (unchanged, Moq).

Rewritten tests: 11 of 15; negative controls: 11 of 11, one per rewritten test.

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` (P2-T1, P2-T2)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (P2-T3; post-merge P2-T7a)
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (P2-T4; post-merge P2-T7b)
4. `dotnet-coverage collect --output coverage\final3-940.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-940.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\940\final3" "/Logger:trx;LogFileName=final3-940.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` followed by the runner post-processing helpers (`ConvertTo-KoverageCoberturaXml`, threshold assertions, `Get-CoberturaFirstPartyCoverageReport`, `ConvertTo-JacocoPackageProjection`, `Get-TrxRunSummary`) (P0-T11 baseline, P2-T6 MEASUREMENT 1 and 2, P2-T7 MEASUREMENT 3)
5. Scoped: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests|FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests" ...` (P0-T10, P1-T7, P2-T5) and the single-test filters of the eleven controls

Template provenance: the MCP policy-audit template asset was not resolvable in this session (no MCP call available to the reviewer); the canonical heading set of `policy-audit-template-usage` was reproduced by hand, and the instruction block is omitted.
