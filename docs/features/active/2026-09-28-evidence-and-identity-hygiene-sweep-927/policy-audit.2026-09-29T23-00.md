# Policy Compliance Audit: 2026-09-28-evidence-and-identity-hygiene-sweep-927

- Issue: #927
- Branch: bug/evidence-and-identity-hygiene-sweep-927
- Branch head: 32d93a367 (wip(927): plan revision 1.17)
- Base branch: origin/main (three-dot anchor origin/main...HEAD)
- Merge base at review time: ddbab26a0 (re-computed with git merge-base; equals the MERGE-BASE-NOW value recorded by P6-T11, P6-T13 and P6-T14)
- Work mode: full-bug (spec.md is the sole acceptance-criteria source)
- Review timestamp: 2026-09-29T23-00
- Reviewer: feature-review agent
- Plan reviewed: plan.2026-09-28T19-44.md, revision 1.17

## Executive Summary

Overall verdict: PASS with three PENDING-CI items and zero blocking findings.

- Blocking findings (FAIL plus blocking PARTIAL): 0.
- PENDING-CI items (non-blocking, listed separately): AC4 (CI Pester coverage figure, P6-T38), AC13 (CI mstest-coverage context and the P6-T39 re-anchored comparison), AC16 (hygiene context green on the pull-request head, P6-T37), and the modified-workflow-needs-green-run rule (a green run against the branch head after the pull request opens). No pull request exists for the branch at review time (gh pr list over the head branch returned an empty set), so none of these can be discharged locally.
- Non-blocking observations: the local C# coverage figure moved by 0.01 percentage points on both line and branch against the pre-merge Phase 0 baseline with zero production C# lines changed; the canonical PowerShell coverage artifact path holds a bundled PoshQC document that does not instrument scripts/hygiene; two Pester It blocks bundle more than one behaviour; the new cobertura-marker ignore pattern also matches the default projection stem the coverage route writes.
- Independent verification performed by this review: raw-document gates (name-based ls-files, root-element grep, JaCoCo child-element grep under docs/features) all return zero; identifier gates one, two, four and nine all return zero over the tracked tree with this feature folder in scope; the two new ignore patterns match the two hypothetical path shapes; the non-docs diff against origin/main consists of exactly the 27 paths the caller enumerated (verified with git diff --name-status); no path under .claude, .mcp.json, .codex, a solution, project, packages or app configuration file, a sibling 2026-09-28 feature folder or artifacts/ is in the diff.

## Rejected Scope Narrowing

None detected. The caller prompt supplied the full three-dot scope (origin/main...HEAD) and the complete non-docs change set. The caller's review note asking the agent to avoid a whole-tree diff is an operational constraint on command shape, not a narrowing of audit scope: the full branch diff was audited through git diff --name-status over the non-docs tree, a name-only listing of this feature folder, exclusion-pathspec listings for the forbidden path classes, and re-runs of the spec's gates over the whole tracked tree.

## Evidence Location Compliance

- Scan basis: git diff --name-only origin/main...HEAD -- artifacts (empty), plus the non-docs name-status listing (no path under artifacts/baselines/, artifacts/qa/, artifacts/evidence/ or artifacts/coverage/).
- validate_evidence_locations.py: not present in this repository (Glob over the tree returned nothing), so the scan was performed manually as above.
- Every evidence artifact this feature adds lives under docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/<kind>/ (baseline, regression-testing, qa-gates, other). P6-T13 records NON-MD=0, MISSING-FIELDS=0, ADDED-RAW=0, ADDED-RAW-UNTRACKED=0.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none (no caller instruction named a non-canonical evidence path).
- Verdict: PASS.

## PR Context Artifacts

- artifacts/pr_context.summary.txt and artifacts/pr_context.appendix.txt: absent from the item worktree (artifacts/ is gitignored there and the branch adds nothing under it). The PR-context generator is not among this agent's tools and the caller's Bash discipline prohibits a whole-tree diff over the roughly 1,750 docs paths, so the artifacts were not regenerated. Scope was derived instead from git diff --name-status over the non-docs tree, the name-only listing of this feature folder, and the spec's Write Set. This is a documented assumption; the non-docs listing matched the caller's enumeration exactly.

## 1. General Unit Test Policy Compliance

### 1.1 Test Structure and Principles

| Check | Verdict | Evidence |
|---|---|---|
| Independence and isolation (Pester) | PASS | Each Describe dot-sources the entry point in BeforeAll; Invoke-GitExe is mocked per It (BeforeEach in the orchestration file); content is injected through the ReadContent delegate keyed by path; no shared mutable state across It blocks. |
| No temporary files or disk I/O in tests | PASS | Grep over tests/scripts/hygiene for New-Item, Set-Content, Out-File, Remove-Item, TestDrive and $env: returned nothing; P6-T12 FILE-IO=0. |
| Deterministic, no wall clock, no environment | PASS | Grep over scripts/hygiene for $env:, Get-Date and [DateTime] returned nothing; P6-T12 ENV=0, CLOCK=0. |
| Arrange-Act-Assert and documented intent | PASS | Every It carries a -Because clause; the three test files open with a header comment stating the fixture rule (violations assembled by concatenation at run time). |
| One behaviour per It | PARTIAL (non-blocking) | "parses a NUL-separated eol listing into path records" also asserts the malformed-record throw; "returns a zero exit decision over clean content" carries nine fixture classes. Recorded in P1-T9 as a batch-budget deviation. See code-review finding CR-2. |
| Scenario completeness (positive, negative, edge, error) | PASS | Rules tests: 6 positive matches, 4 negative matches, line-number-only assertion, 8 classifier cases including DOCTYPE, byte-order mark and non-XML extension; git tests: parse, index-binary, UTF-16 and UTF-8 decode, wrapper throw; orchestration tests: governance exclusion, raw document, projection retention, non-zero and zero exit decisions, no-echo assertion, unreadable record. |
| C# tests unchanged in behaviour | PASS | The thirteen C# diffs change string literals only; every NotContain assertion on testuser, OneDrive, Contoso, fsAncestor and the mailbox token is untouched (verified by reading the post-change files). |
| Test file location mirrors production | PASS | scripts/hygiene/<name>.ps1 is tested by tests/scripts/hygiene/<name>.Tests.ps1. |

### 1.2 Coverage

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/baseline/p0-t14-mstest-coverage-baseline.md` (package-level projection and first-party summary line, pre-merge tree at merge base 177b6d78e)
- C# post-change coverage artifact: `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/qa-gates/csharp-coverage-projection.md` (package-level projection and first-party summary line; the ignored working-tree Cobertura document under coverage/ was read for corroboration and reports the same covered and valid counts)
- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed on this branch`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed on this branch`
- PowerShell baseline coverage artifact: `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/baseline/p0-t15-pester-coverage-baseline.md` (94.49% lines over scripts/dependencies and scripts/vscode)
- PowerShell post-change coverage artifact: `docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/evidence/regression-testing/p1-t9-hygiene-coverage-interim.md` (per-file figures for the three new production files; the ignored working-tree JaCoCo document under coverage/ was read for corroboration and carries the same sourcefile counters)
- Python baseline coverage artifact: `N/A - zero Python files changed on this branch`
- Python post-change coverage artifact: `N/A - zero Python files changed on this branch`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.93% lines (56486/65737) / 80.09% branches (13657/17052). Post-change: 85.92% lines (56479/65736) / 80.08% branches (13656/17054). Change: -0.01% lines / -0.01% branches, entirely inside the UtilitiesCS package (7 covered lines and 1 covered branch), with zero production C# lines changed on the branch. Disposition: PASS. Evidence: p0-t14-mstest-coverage-baseline.md, csharp-coverage-projection.md, csharp-toolchain-pass.md; the thirteen changed C# files are test files outside the coverage denominator, so there is no changed-line figure to compare; both post-change figures are above the 85% line and 75% branch floors of the rules directory and the 80% line and 75% branch floors of CLAUDE.md; the movement is inside the run-to-run band this repository has recorded for the collector, and the caller reports main's own CI at the merge base ddbab26a0 (run 36651909330, verified by this review as a completed successful push run on main at that SHA) prints 85.92 / 80.08 with identical denominators; the re-anchored comparison is P6-T39, PENDING-CI.
- PowerShell: Baseline: 94.49% lines (1613/1707, scripts/dependencies and scripts/vscode, P0-T15). Post-change: 94.06% lines (95/101, the new scripts/hygiene package, P1-T9 iter2 JaCoCo; the three-folder aggregate derived by summing the two runs is 94.47% lines, 1708/1808). Change: one new package added; no pre-existing PowerShell production file changed, so no pre-existing figure moved. New/changed-code coverage: 94.06%. Disposition: PASS. Evidence: p0-t15-pester-coverage-baseline.md, p1-t9-hygiene-coverage-interim.md, the JaCoCo document under the ignored coverage/ directory (sourcefile counters Test-RepositoryHygiene.Git.ps1 34/37, Test-RepositoryHygiene.ps1 30/33, Test-RepositoryHygiene.Rules.ps1 31/31), P6-T1 hashes showing the production files unchanged since that run; the CI Pester job figure over all three folders is the authoritative AC4 figure per Ruling 1 and is PENDING-CI (P6-T38).
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

Per-language verdict lines (one line each):

- C# coverage verdict: PASS (repo-wide 85.92% lines and 80.08% branches, above both floor sets; zero changed production lines; 0.01-point movement with no production change, dispositioned as collector noise pending the P6-T39 re-anchored CI comparison).
- PowerShell coverage verdict: PASS (each new production file at or above 90% lines: 91.89%, 90.91%, 100.00%; new package 94.06% lines; derived three-folder aggregate 94.47% lines; Pester measures no branch figure, so no branch threshold applies).
- Canonical-path artifact fidelity for PowerShell, artifacts/pester/powershell-coverage.xml: FAIL, non-blocking. The document at that path in the item worktree is the bundled PoshQC JaCoCo written by run_poshqc_test; it instruments only the governance hooks folder, reports zero covered lines for every file there, and carries no package for scripts/hygiene, so it cannot evidence the changed language. The figures above were taken from the direct-Pester JaCoCo document under coverage/ and the committed projection; CI is the authoritative producer per Ruling 1.
- Canonical-path artifact for C#, artifacts/csharp/coverage.xml: absent in the item worktree. The committed package-level projection under this feature's evidence tree and the ignored working-tree Cobertura document under coverage/ carry the figures (repository precedent: a committed feature-evidence projection counts as the artifact). Non-blocking.

| Language | Coverage artifact read | Verdict | Disposition |
|---|---|---|---|
| C# | evidence/qa-gates/csharp-coverage-projection.md; coverage/coverage.cobertura.xml (ignored, corroboration) | PASS | non-blocking 0.01-point movement, PENDING-CI re-anchor (P6-T39) |
| PowerShell | evidence/regression-testing/p1-t9-hygiene-coverage-interim.md; coverage/pester-coverage.xml (ignored, corroboration) | PASS | AC4 check-off PENDING-CI (P6-T38) |
| TypeScript | none | N/A | zero files changed |
| Python | none | N/A | zero files changed |

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 13 (test files only) | 7346 | 7346 passed, 0 failed | 85.93% lines / 80.09% branches | 85.92% lines / 80.08% branches | N/A (zero changed production C# lines) |
| PowerShell | 7 (3 new production, 3 new test, 1 modified test) | 373 (PoshQC run over six folders); 31 hygiene | 373 passed, 0 failed; 31 passed, 0 failed | 94.49% lines | 94.06% lines (new package); 94.47% lines (derived aggregate) | 94.06% lines |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

Threshold note: the rules directory states 85% line / 75% branch; CLAUDE.md states 80% line / 75% branch for C#, 80% line for PowerShell, and 90% for new code. Every post-change figure above satisfies both sets. The spec's Resolved tensions item 1 adopts the CLAUDE.md figures for the Pester callee (80 floor, 90 new-code target).

### 1.3 External Dependencies and Environment

| Check | Verdict | Evidence |
|---|---|---|
| No external executable reached from tests | PASS | Only Invoke-GitExe reaches git; every test mocks Invoke-GitExe (signature param([string[]]$GitArgs) matches production); P6-T12 MOCK-GIT=0 (no mock of git itself). |
| No mutable global state | PASS | Script-scoped test fixtures are reset in BeforeEach; production functions carry no script-scoped state. |
| Temporary files | PASS | None created; the redaction helper and its log live outside the repository (SCRATCH expression only). |

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression first | PASS | pester-hygiene-fail-before.md (Failed=31, ExpectedExitCode 1, EXIT_CODE 1); guard-pre-sweep-run.md (HYGIENE Findings=1839 = 625 + 1214, exit 1). |
| Minimal targeted fix, no opportunistic refactor | PASS | Production change is three new PowerShell files, one new callee workflow, three-line orchestrator edit, two array entries in the Pester callee, six ignore lines, literal-only C# fixture rewrites, and the content sweep. No production C# changed (PROD-CS=0, re-verified by pathspec listing). |
| Separation of concerns (pure logic vs I/O) | PASS | Rules part file is pure (text in, records out); git part file holds the executable seam and the byte adapter; the entry point orchestrates. |
| Fail fast, no silent catch | PASS | Assert-GitExitCode throws with the argument list; a content-reader throw is reported as an unreadable finding, never skipped; the only try/catch converts the throw into a finding and increments the count. |
| 500-line limit (production, test, script) | PASS | P6-T11 OVER-500=0; largest touched file is the pre-existing helper test at 494 lines (unchanged count; literal-only edit); AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs grew from 186 to 195 lines through csharpier reflow. |
| Naming and documentation | PASS | Approved verbs (Get-, Find-, Invoke-, Read-, Assert-); every function has a comment-based help block stating contract, parameters and outputs; header comments explain why (identifier secrecy, mock-the-wrapper rule). |
| Dependencies | PASS | No new library; Pester 5.6.1 pinned as before; actionlint pinned as before. |
| Public API stability | PASS | No existing function signature changed; the Pester callee's gate semantics unchanged (arrays widened only). |
| Toolchain loop closure | PASS | p6-t10-loop-closure.md: LOOP: CLEAN PASS on iter1 (format, analyze, test for PowerShell; format, check, analyzers, nullable, test for C#), no rewrite, no failed step. |
| Supporting documents updated | PASS | .github/workflows/README.md gains the callee row, corrects the Pester row to name all three test folders, and adds the seventh predicted context with the ruleset follow-up paragraph. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 PowerShell (.claude/rules/powershell.md)

| Check | Verdict | Evidence |
|---|---|---|
| Toolchain through PoshQC MCP: format, analyze, test | PASS | p6-t1-powershell-format.md (REWRITTEN=0, BOM-MISSING=0, ok=true twice); p6-t2-powershell-analyze.md ("PoshQC analyze: pass (0 findings); tool reports no count", ok=true twice); p6-t3-pester-test.md (ok=true, 373 passed, 31 hygiene passed). Recorded per Ruling 1; not re-run by this review (no PoshQC tools in this session). |
| PowerShell 7 compatibility | PASS | Get-Content -AsByteStream (PS7 form) in the default reader; no Windows-only cmdlet; the guard runs on the Ubuntu runner by design. |
| Advanced functions with CmdletBinding and typed parameters | PASS | All seven functions declare [CmdletBinding()], typed parameters, Mandatory where required, and [OutputType]. |
| Wrapper seam Invoke-GitExe -GitArgs [string[]] splatting git @GitArgs 2>&1 | PASS | Test-RepositoryHygiene.Git.ps1 lines 32 to 53; the only git invocation in the guard. |
| Injectable delegate seam only where a wrapper is insufficient | PASS | Read-TrackedFileText -ReadContent [scriptblock] with a byte-reading default; narrow, no runner framework. |
| No Invoke-Expression, no hard-coded credentials or paths | PASS | None present; the governance prefix constant is the only literal path fragment (PREFIX-LITERAL=1 by design). |
| Change budget (3 production + 3 test per batch) | PASS | Exactly three production and three test files in the hygiene batch; the helper test edit is the one modified-test slot recorded by the plan. |
| No PSScriptAnalyzer debt deferred | PASS | Analyze pass with zero findings on the final iteration. |

### 3.2 C# (CLAUDE.md C# Code Change Policy)

| Check | Verdict | Evidence |
|---|---|---|
| csharpier format then check via dotnet tool run | PASS | P6-T5 REWRITTEN=0, porcelain empty; P6-T6 exit 0, "Checked 1625 files". |
| msbuild /t:Rebuild analyzers command, non-vacuous | PASS | P6-T7 exit 0, SUCCEEDED=1, ZERO-ERRORS=1, 0 Warning(s), OUT-LINES=36, SKIP-CORECOMPILE=0. |
| msbuild /t:Rebuild TreatWarningsAsErrors command, non-vacuous, no /p:Nullable=enable | PASS | P6-T8 exit 0, SUCCEEDED=1, ZERO-ERRORS=1, 0 Warning(s), OUT-LINES=36, SKIP-CORECOMPILE=0; the artifact states no solution-wide nullable property was added. |
| MSTest-with-coverage route | PASS | P6-T9: 7346 passed, 0 failed (baseline 7343); summary derived from the trx; raw trx and Cobertura kept under the ignored coverage/ directory. |
| No production C#, csproj, sln, packages.config or app.config change | PASS | Pathspec listing over those classes against origin/main...HEAD is empty. |
| Formatter output wins over hand formatting | PASS | Three dictionary initializers in AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs were reflowed by csharpier after the literal change; check exit 0. |

### 3.3 Workflow files (.claude/rules/ci-workflows.md and the feature-review workflow rule)

| Check | Verdict | Evidence |
|---|---|---|
| Deliberately-failing nested command pattern | PASS | The new pwsh step invokes the guard once with no expected-failure nested command, so no exit-code reset is required and none is added (LASTEXIT=0 in the P5-T1 probe). |
| Orchestrator stays pure (no inline steps, no needs edges, caller-owned concurrency) | PASS | ci.yml diff adds one uses-job of three lines; callee declares no concurrency block. |
| actionlint | PASS | p5-t4-actionlint.md ACTIONLINT-EXIT=0 over every workflow file including _hygiene.yml, ci.yml and _pester.yml. |
| modified-workflow-needs-green-run | PENDING-CI (non-blocking) | Three workflow files changed (.github/workflows/_hygiene.yml new, ci.yml, _pester.yml). ci.yml triggers on push to main and development and on pull_request only, so the fast-forward push of this branch produced no run; no pull request exists yet (gh pr list empty). The rule is satisfied only by a green run on the branch head after the pull request opens (P6-T37 records it). |

## 4. Language-Specific Unit Test Policy Compliance

### 4.1 PowerShell (Pester)

| Check | Verdict | Evidence |
|---|---|---|
| Pester 5.x, *.Tests.ps1 naming, Describe/It structure | PASS | Three files under tests/scripts/hygiene; Pester 5.6.1 pinned; Describe per function with It per behaviour (two bundled Its noted in section 1.1). |
| Never mock git directly; mock the wrapper with parity signature | PASS | Every mock is Mock Invoke-GitExe { param([string[]]$GitArgs) ... }; the Get-Content mock in the UTF-8 decode test mocks a cmdlet, not an executable. |
| Mock registration before command resolution | PASS | Mocks are registered in BeforeEach or at the start of the It, after the BeforeAll dot-source and before the call under test. |
| No network, PATH, working-directory or profile dependence | PASS | Fixtures are in-memory strings and byte arrays; the dot-source path is relative to $PSScriptRoot. |
| Coverage regression on changed lines | PASS | No pre-existing PowerShell production line changed; the modified helper test is a test file. |
| Every spec-named It present and passing | PASS | pester-hygiene-pass-after.md and p6-t3-pester-test.md list all thirty spec-named It blocks plus the unreadable-branch test, each Passed. |

### 4.2 C# (MSTest, Moq, FluentAssertions)

| Check | Verdict | Evidence |
|---|---|---|
| No test method added, removed or renamed | PASS | Diffs are literal-only; P3-T16 ran 190 tests over the fourteen matched classes with 0 failed. |
| Framework and library selection unchanged | PASS | No attribute or using change in any of the thirteen files. |
| Fixture semantics preserved (drive-rooted, user-segment token retained) | PASS | The fixtures root keeps drive-rooted semantics; testuser, test, Test and user segments survive; the three-letter real-account prefix is gone from the three Store tests (verified in the post-change files and the diff). |

## 5. Test Coverage Detail

- New PowerShell production files (P1-T9 iter2 JaCoCo, corroborated by the sourcefile counters in the ignored working-tree document):
  - scripts/hygiene/Test-RepositoryHygiene.Git.ps1: 34/37 = 91.89% lines; missed lines are the three-line body of Invoke-GitExe (the only place git runs; tests mock the wrapper by policy).
  - scripts/hygiene/Test-RepositoryHygiene.ps1: 30/33 = 90.91% lines; missed lines are the three-line script-entry block that runs only when the file is invoked rather than dot-sourced.
  - scripts/hygiene/Test-RepositoryHygiene.Rules.ps1: 31/31 = 100.00% lines.
  - Package aggregate: 95/101 = 94.06% lines.
- Provenance: the JaCoCo session under coverage/ is stamped 2026-09-29 17:39 local; the production files were last committed at 7e6ecbd0e and the P6-T1 SHA-256 hashes show no rewrite afterwards, so the figures describe the files at the branch head. The CI Pester job over all three folders is the authoritative producer for the AC4 check-off (Ruling 1; P6-T38 PENDING-CI).
- C# per-file coverage of changed files: not measurable by design; all thirteen changed files are test classes excluded from the coverage denominator. The per-class Cobertura document that would attribute the 7-line UtilitiesCS movement was discarded by the route after it wrote the summary (csharp-coverage-projection.md); the cause of the movement is therefore recorded as unknown, with no production change on either side of the merge.
- Escape-set review: the six uncovered PowerShell lines are the executable seam and the entry guard, both of which are reachable only by running git or invoking the script as a program; the guard-pre-sweep-run and guard-post-sweep-run artifacts exercise both paths end to end (exit 1 with 1839 findings, then exit 0 with 0 findings).

## 6. Test Execution Metrics

| Suite | Command route | Result (passed / failed / skipped) | Baseline or expectation | Evidence |
|---|---|---|---|---|
| C# full suite with coverage | scripts/vscode/Invoke-MSTestWithCoverage.ps1 (P6-T9) | 7346 / 0 / 0 | 7343 passed (P0-T14) | csharp-toolchain-pass.md |
| C# fixture-scoped vstest (13 classes, 14 matched) | vstest.console with FullyQualifiedName filter (P3-T16) | 190 / 0 / 0 | scoped run; every named class present | p3-t16-fixture-tests.md |
| Pester, six-folder PoshQC run | run_poshqc_test (P6-T3) | 373 / 0 / 0 | 320 direct (P0-T15); 342 CI on main plus 31 new = 373 expected | p6-t3-pester-test.md |
| Pester, hygiene folder direct run | Invoke-Pester (P1-T8) | 31 / 0 / 0 | 0 passed and 31 failed before the production files existed (P1-T4) | pester-hygiene-pass-after.md, pester-hygiene-fail-before.md |
| Pester, helper test folder | run_poshqc_test (P3-T17) | 211 / 0 / 0 | the two named helper tests Passed | p3-t17-helper-test-run.md |
| Guard over the tracked tree, pre-sweep | scripts/hygiene/Test-RepositoryHygiene.ps1 (P1-T12) | exit 1, Findings=1839 | expected exit 1 | guard-pre-sweep-run.md |
| Guard over the tracked tree, post-sweep | scripts/hygiene/Test-RepositoryHygiene.ps1 (P6-T16) | exit 0, Findings=0, 39 s | expected exit 0 | guard-post-sweep-run.md |

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Confidentiality masking scan | Gates one, two, four and nine re-run by this review over the tracked tree (identifiers read from the environment, counts only); gate three and the UTF-16 census taken from identifier-residual-scan.md | GATE1=0, GATE2=0, GATE4=0, GATE9=0 (this review); GATE3=0, UTF16-PROFILE-FILES=0 (artifact). Raw-document gates re-run by this review: name-based ls-files empty, root-element grep empty, JaCoCo child-element grep under docs/features empty. Ignore rules verified with git check-ignore -v (both hypothetical paths matched by .gitignore lines 146 and 147). PASS. |
| Suppression scan (added lines) | Read of the six new PowerShell files and the workflow | No SuppressMessage attribute, no analyzer suppression, no ExcludeFromCodeCoverage, no exemption or allowlist mechanism in the guard (P6-T12 ALLOW=0). PASS. |
| Workflow change scan | git diff --name-status origin/main...HEAD over .github | _hygiene.yml added; ci.yml, _pester.yml and README.md modified; actionlint exit 0; green run on the branch head PENDING-CI. |

## Appendix A: Test Inventory

New Pester It blocks (31):

- Test-RepositoryHygiene.Rules.Tests.ps1 (19): the eleven Find-UserProfilePathMatch cases (six matching, four non-matching, one line-number-only) and the eight Get-RawEvidenceDocumentKind cases (trx extension, coverage root on its own line, results root, report root with class elements, package-only report root, report root behind a DOCTYPE, byte-order-mark prefixed document, ps1 file containing a TestRun element).
- Test-RepositoryHygiene.Git.Tests.ps1 (5): NUL-separated eol listing parse (plus malformed-record throw), index-binary flag, UTF-16 little-endian decode, UTF-8 decode without mark, wrapper non-zero exit throw.
- Test-RepositoryHygiene.Tests.ps1 (7): governance exclusion, raw document finding, projection retention, non-zero exit decision (five findings across one profile path and four raw-document shapes), zero exit decision over clean content in several encodings and XML shapes, path-and-line-only output, unreadable record.

Modified Pester test file (1): tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1, two It blocks with literal path fixtures rebased onto the existing fixture root; both pass (P3-T17).

Modified C# test classes (13, literal-only): EfcSelectionGuardTests, AppAutoFileObjectsFolderPredictorTests, AppFileSystemFolderPathsMatchBestSpecialFolderTests, AppFileSystemFolderPathsOneDriveResolutionTests, PeopleScoDictionaryNewTests, EmailFilerConfig_Tests, LcppnFolderPredictorStore_Tests, FilePathHelperConverterTests, ArchiveStemContractTests, FolderConverterIssue614Tests, StoreFilterAttributionTests, StoresWrapperDisableTests, StoresWrapperTests. All 190 scoped tests pass (P3-T16); the full suite passes 7346 (P6-T9).

## Appendix B: Toolchain Commands Reference

| Step | Command (as recorded) | Exit / result | Artifact |
|---|---|---|---|
| PowerShell format | mcp__drm-copilot__run_poshqc_format over scripts/hygiene, tests/scripts/hygiene, tests/scripts/vscode | ok=true; REWRITTEN=0 | p6-t1-powershell-format.md |
| PowerShell analyze | mcp__drm-copilot__run_poshqc_analyze over the same folders | PoshQC analyze: pass (0 findings); tool reports no count | p6-t2-powershell-analyze.md |
| PowerShell test | mcp__drm-copilot__run_poshqc_test over the six Pester callee folders | 373 passed, 0 failed | p6-t3-pester-test.md |
| C# format | dotnet tool run csharpier format . | REWRITTEN=0 | p6-t5-csharpier-format.md |
| C# format check | dotnet tool run csharpier check . | exit 0, 1625 files | p6-t6-csharpier-check.md |
| C# analyzers | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | exit 0, 0 warnings, SKIP-CORECOMPILE=0 | p6-t7-msbuild-analyzers.md |
| C# nullable | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | exit 0, 0 warnings, SKIP-CORECOMPILE=0 | p6-t8-msbuild-nullable.md |
| C# test with coverage | scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug | 7346 passed; 85.92% lines, 80.08% branches | csharp-toolchain-pass.md, csharp-coverage-projection.md |
| Workflow lint | scripts/dev-tools/run-actionlint.ps1 | exit 0 | p5-t4-actionlint.md |
| Guard, final tree | scripts/hygiene/Test-RepositoryHygiene.ps1 | exit 0, Findings=0 | guard-post-sweep-run.md |

## PENDING-CI Items (non-blocking; listed separately from blocking_count)

1. AC4: per-file and aggregate Pester coverage from the CI Pester job on the pull-request head (P6-T38; pester-coverage-projection.md not yet written).
2. AC13: the CI mstest-coverage context on the pull-request head and the P6-T39 re-anchored comparison against main's CI at the merge base, with negative control (csharp-coverage-reanchor.md not yet written).
3. AC16: the check-runs query on the pull-request head listing the hygiene context with conclusion success and every previously required context green (P6-T37; ci-hygiene-context.md not yet written).
4. modified-workflow-needs-green-run: a green ci.yml run against the branch head after the pull request opens.

Manual maintainer follow-up, out of scope for this item: adding the hygiene context to the main branch ruleset after the first green run.
