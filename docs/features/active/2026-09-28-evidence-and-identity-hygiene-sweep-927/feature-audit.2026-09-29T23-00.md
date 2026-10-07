# Feature Audit: 2026-09-28-evidence-and-identity-hygiene-sweep-927

- Issue: #927
- Branch: bug/evidence-and-identity-hygiene-sweep-927 at 32d93a367
- Base: origin/main (three-dot anchor; merge base ddbab26a0)
- Review timestamp: 2026-09-29T23-00
- Work mode: full-bug; AC source: spec.md only (AC1 to AC20; no user-story.md exists)

## Scope and Baseline

- Baseline tree: main at 177b6d78e (the Phase 0 merge base recorded in p0-t2-tree-state.md; BASE-SHA cfbb2bd61 is the branch's own starting commit). The branch merged origin/main twice; the current merge base is ddbab26a0, and the P6-T14 ancestry checks show 177b6d78e is an ancestor of it, cfbb2bd61 is an ancestor of the head, and the pushed tip is an ancestor of the head (no force push), with a negative control exiting 1.
- Baseline defect measurements (identifier-and-raw-document-baseline.md, counts only): GATE1=1213, GATE2=183, GATE3=6, GATE4=1213, GATE5A=580, GATE5B=598, GATE5C=270, GATE9=88; RAW-POPULATION=625 (243 cobertura, 23 dotnet-coverage, 27 jacoco-raw, 332 trx); 18 projections; TRACKED-TOTAL=16590. The profile-path population was re-measured at 1214 by P1-T12 (MEASUREMENT-CORRECTION: one UTF-16 file the Phase 0 census missed because of a collapsed separator class).
- Change set: 27 non-docs paths (verified by git diff --name-status) plus this feature folder, six promoted records, one research note, and the evidence sweep (1052 files rewritten, 625 raw documents and one root run log removed).
- Post-change measurements re-run by this review over the tracked tree at the head: GATE1=0, GATE2=0, GATE4=0, GATE9=0; name-based raw-document listing empty; root-element grep empty; JaCoCo child-element grep under docs/features empty; both new ignore patterns match their hypothetical shapes; no path under .claude, .mcp.json, .codex, project or solution files, artifacts, or a sibling 2026-09-28 feature folder in the diff.

## Acceptance Criteria Inventory

Source: docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/spec.md, section "## Acceptance Criteria", 20 checkbox items (AC1 to AC20). At review time 17 are checked and 3 are unchecked (AC4, AC13, AC16), matching the ledger in evidence/qa-gates/ac-check-off-ledger.md.

| AC | Short title | Source checkbox state |
|---|---|---|
| AC1 | Regression observation, guard | [x] |
| AC2 | Regression observation, Pester | [x] |
| AC3 | Pester green | [x] |
| AC4 | Pester coverage | [ ] |
| AC5 | Guard hygiene | [x] |
| AC6 | Guard output contract | [x] |
| AC7 | Raw documents removed | [x] |
| AC8 | Ignore rules | [x] |
| AC9 | Identifier gates | [x] |
| AC10 | Legacy tokens normalised | [x] |
| AC11 | Redaction fidelity | [x] |
| AC12 | Fixtures triaged | [x] |
| AC13 | C# toolchain | [ ] |
| AC14 | PowerShell toolchain | [x] |
| AC15 | Workflow wiring | [x] |
| AC16 | CI outcome | [ ] |
| AC17 | Guard green over the final tree | [x] |
| AC18 | Evidence form | [x] |
| AC19 | Scope containment | [x] |
| AC20 | Invariant and trace | [x] |

## Acceptance Criteria Evaluation

| AC | Status | Evidence and reviewer verification |
|---|---|---|
| AC1 | PASS | guard-pre-sweep-run.md: ExpectedExitCode 1, EXIT_CODE 1, HYGIENE Findings=1839 = RAW 625 + PROFILE 1214 (the MEASUREMENT-CORRECTION figure the amended criterion names). Baseline counts for gates one to five and nine, the tracked-file total and the encoding survey (GATE10=44) are in identifier-and-raw-document-baseline.md as counts only. |
| AC2 | PASS | pester-hygiene-fail-before.md: PROD-EXISTS=False precondition, Passed=0 Failed=31, ExpectedExitCode 1, EXIT_CODE 1; strict-mode parity run identical. |
| AC3 | PASS | pester-hygiene-pass-after.md: Passed=31 Failed=0 Skipped=0; all nineteen rules, five git and six orchestration It names from the Test Strategy are listed and Passed (plus the unreadable test); p6-t3-pester-test.md repeats the enumeration through the PoshQC route. |
| AC4 | PENDING-CI | Local figures: Git.ps1 91.89%, entry point 90.91%, Rules.ps1 100.00%, package 94.06% (p1-t9-hygiene-coverage-interim.md; corroborated by the sourcefile counters in the ignored working-tree JaCoCo document); derived three-folder aggregate 94.47%. The criterion binds to the pinned Pester version with the extended path arrays as recorded in pester-coverage-projection.md, which P6-T38 writes from the CI Pester job after the pull request opens (Ruling 1). No pull request exists yet. Not a FAIL: every local figure clears both the 90 per-file and the 80 aggregate thresholds. |
| AC5 | PASS | p6-t12-guard-hygiene.md: PATTERN=0, ACCOUNT=0, HOST=0, SHORT=0, ENV=0, CLOCK=0, ALLOW=0, FILE-IO=0, MOCK-GIT=0 over the seven files; p6-t11-file-size-audit.md OVER-500=0. Reviewer read of the six PowerShell files and the callee confirms: no environment or clock read, no allowlist, fixtures assembled by concatenation, no disk write in any test. |
| AC6 | PASS | The three named orchestration tests pass (P1-T8, P6-T3). Reviewer read of Test-RepositoryHygiene.ps1 lines 55, 59, 69 and 73 to 75 confirms the line shapes and the exit decision. |
| AC7 | PASS | raw-document-removal.md: GATE5A=0, GATE5B=0, GATE5C=0, PROJECTIONS-TRACKED=18, RUNLOG-TRACKED=0, TRX-TRACKED=0. Re-verified by this review: git ls-files over the four raw extensions and test-output.txt is empty; the root-element grep and the docs/features JaCoCo child-element grep are empty; test-output.txt is a D entry in the name-status listing. |
| AC8 | PASS | .gitignore lines 143 to 147 read as specified (three comment lines naming the content guard, the trx pattern, the cobertura-marker pattern), placed after the coverage and coveragexml patterns (lines 140 and 141, unchanged) and before coverage/*. git check-ignore -v matched both hypothetical paths (this review). |
| AC9 | PASS | identifier-residual-scan.md: GATE1=0, GATE2=0, GATE3=0, GATE4=0, PLAN-FILE-HITS=0, scope statement present, UTF-16 census 1 before and 0 after. Re-verified by this review with identifiers read from the environment: GATE1=0, GATE2=0, GATE4=0 over the tracked tree with this feature folder and the plan file in scope. |
| AC10 | PASS | GATE9=0 in identifier-residual-scan.md; re-verified by this review (GATE9=0). The D18 residual of 9 any-case files is recorded, not gated. |
| AC11 | PASS | redaction-fidelity.md: EOL-MISMATCH=0 and BOM-MISMATCH=0 over 1052 files; XML-REWRITTEN=0; second run FILES-WRITTEN=0 with RULE1 to RULE8 all 0 and equal numstat hashes; per-file "only substituted lines changed" established by the multiset comparison (MULTISET-UNMATCHED=0, LINECOUNT-MISMATCH=0) with an in-memory negative control of 1, per Ruling 2; the eight diff-reported CoreClean lines are explained as aligner moves with identical per-file counts. |
| AC12 | PASS | Reviewer read of the thirteen C# diffs: every hunk rebases a fixture root onto the fixtures root; the three Store tests carry testuser and retain the Google Workspace Sync and Google Apps Sync tokens; NotContain assertions on testuser, OneDrive, Contoso and fsAncestor are present and unchanged. The helper test uses the existing repo fixture root. The research note's three citations use the user-profile placeholder with the repos suffix. P3-T16: 190 passed over the fourteen matched classes; P3-T17: both named helper tests Passed. |
| AC13 | PENDING-CI | Local: csharpier check exit 0; both Rebuild passes exit 0 with SKIP-CORECOMPILE=0 and 0 warnings; 7346 passed, 0 failed, not below 7343. Coverage clause: 85.92 / 80.08 against the pre-merge Phase 0 baseline 85.93 / 80.09, 0.01 below on each with zero production C# lines changed (csharp-coverage-projection.md; the working-tree Cobertura root confirms the counts). The criterion names the CI mstest-coverage context on the pull-request head as the authoritative pass, and plan revision 1.17 re-anchors the comparison to main's CI at the merge base (P6-T39, with negative control). The caller reports that run (36651909330, verified by this review as a completed successful push run on main at ddbab26a0) prints 85.92 / 80.08 with identical denominators; the figures themselves were not read from the log by this review. Recorded as PENDING-CI, not FAIL. |
| AC14 | PASS | powershell-toolchain-pass.md: iter1 final, REWRITTEN=0, "PoshQC analyze: pass (0 findings); tool reports no count", all MCP channel lines ok=true. |
| AC15 | PASS | Reviewer read of _hygiene.yml (workflow_call, workflow_dispatch, contents: read, no concurrency, one ubuntu-latest job, checkout then one pwsh step running the guard, no exit-code reset), ci.yml (one uses-job, no needs, no steps), _pester.yml (Run.Path and CodeCoverage.Path include the hygiene folders in alphabetical position), and the README diff (callee row, Pester row naming three folders, seventh context marked predicted). p5-t4-actionlint.md ACTIONLINT-EXIT=0. |
| AC16 | PENDING-CI | No pull request exists for the branch (gh pr list over the head branch is empty), so no check-runs query is possible; ci-hygiene-context.md is written by P6-T37 after the pull request opens. The ruleset-not-modified statement is already present in issue.md's delivery note. |
| AC17 | PASS | guard-post-sweep-run.md: EXIT_CODE 0, FINDING-LINES=0, HYGIENE Findings=0, ENUMERATED-FEATURE-FOLDER=61, enumeration statement present. This review's gate re-runs over the same head tree return zero for every gate the guard implements. |
| AC18 | PASS | p6-t13-evidence-form.md: NON-MD=0, MISSING-FIELDS=0, ADDED-RAW=0, ADDED-RAW-UNTRACKED=0; raw outputs stay under the scratch expression or the ignored coverage/ and artifacts/ directories. This review's name-status listing shows no added xml, trx or coverage path. The three audit artifacts of this review are Markdown with placeholders only. |
| AC19 | PASS | p6-t14-scope-containment.md: OUTSIDE-WRITE-SET=0, GOVERNANCE=0, MCP-CONFIG=0, BUILD-INPUTS=0, PROD-CS=0, SIBLING-2026-09-28=0, three ancestry checks exit 0, control exits 1, no force push. Re-verified by this review: pathspec listings over .claude, .mcp.json, .codex, csproj, sln, packages.config, app.config, props, targets and the sibling 2026-09-28 folders are all empty; merge base re-computed as ddbab26a0. |
| AC20 | PASS | The four named orchestration tests pass; the guard carries no exemption mechanism (ALLOW=0, PREFIX-LITERAL=1 is the governance exclusion the spec requires). Reviewer read confirms a raw record and a profile-path line each produce exactly one finding and a package-level projection produces none. |

## Acceptance Criteria Check-off

- Reviewer check-off protocol: items evaluated PASS are already checked in spec.md (17 of 17), so no source-file edit was required or made. The caller directed this review not to edit spec.md; the plan's check-off tasks own the three remaining boxes.
- AC4, AC13 and AC16 remain unchecked. Each is PENDING-CI with its evidence artifact assigned to a post-pull-request task (P6-T38, P6-T39, P6-T37); none is a FAIL against delivered work.
- Newly checked-off items by this review: none.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/spec.md
- Total AC items: 20
- Checked off (delivered): 17
- Remaining (unchecked): 3
- Items remaining:
  - AC4 Pester coverage: line coverage for each of the three new production files is at or above ninety percent, and the aggregate line figure over the three listed script folders is at or above eighty percent, measured with the pinned Pester version and the extended path arrays and recorded as a Markdown projection in the qa-gates artifact pester-coverage-projection (PENDING-CI, P6-T38).
  - AC13 C# toolchain: the csharpier check exits zero; both msbuild Rebuild passes exit zero and their captured logs contain no line reporting the CoreCompile target as skipped; the MSTest-with-coverage route reports zero failed and a passed total not lower than the baseline passed total, with first-party line and branch coverage not below the pre-change figures; the evidence is the test-result summary in the qa-gates artifact csharp-toolchain-pass and the projection in the qa-gates artifact csharp-coverage-projection; the CI mstest-coverage context on the pull request head is the authoritative pass (PENDING-CI, P6-T39).
  - AC16 CI outcome: on the pull request head, the check-runs query lists exactly one context whose name begins with the hygiene caller job id and the separator, its conclusion is success, every previously required context also reports success, and the captured string (not a typed one) is recorded in the qa-gates artifact ci-hygiene-context together with a statement that the branch ruleset was not modified by this change (PENDING-CI, P6-T37).

## Summary

- Verdict: PASS on delivered work; 17 of 20 criteria PASS, 3 PENDING-CI, 0 FAIL, 0 PARTIAL.
- Blocking count: 0.
- PENDING-CI: AC4, AC13, AC16, and the modified-workflow-needs-green-run rule (policy audit). All four resolve on the first CI run against the branch head after the pull request opens; P6-T37, P6-T38 and P6-T39 then write ci-hygiene-context.md, pester-coverage-projection.md and csharp-coverage-reanchor.md and tick the three boxes.
- Manual maintainer follow-up, out of scope: add the hygiene context to the main branch ruleset after the first green run; the sibling items of bugs-2026-09-28 must be green on that context before they merge.
- Related artifacts: policy-audit.2026-09-29T23-00.md and code-review.2026-09-29T23-00.md in this folder. No remediation-inputs artifact is produced because no blocking finding exists.
