# coverage-cobertura-mstest-powershell-tooling-defects (Plan)

- **Issue:** #733
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T12-01
- **Status:** Ready for preflight validation
- **Version:** 1.0
- **Work Mode:** full-bug — spec.md is the sole acceptance-criteria source. user-story.md does not exist for this item and is not required.
- **Branch:** bug/coverage-cobertura-mstest-powershell-tooling-defects-733, based on origin/main.

## Conventions

- FEATURE = docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733.
- Every evidence artifact resolves under FEATURE/evidence/ followed by exactly one of these four kind segments: baseline, regression-testing, qa-gates, other (for example FEATURE/evidence/baseline/... or FEATURE/evidence/qa-gates/...). Any artifacts/-rooted evidence path is invalid and must be rejected; if any upstream instruction names one, substitute the canonical path and record EVIDENCE_LOCATION_OVERRIDE_REJECTED.
- Every evidence filename below ends in a TIMESTAMP segment, a literal placeholder standing for the ISO-8601 yyyy-MM-ddTHH-mm value captured at artifact-write time (for example poshqc-format.iter1.TIMESTAMP.md), starting at 1 and incrementing the iter suffix on every restart of a loop.
- Every command-step artifact carries Timestamp, Command, EXIT_CODE (or the MCP tool's ok/summary payload when no exit code exists), and Output Summary.
- Line numbers cited in this plan are descriptive baseline references only, measured directly against the current origin/main-derived tree state on 2026-09-02 during this planning pass. They will shift once this plan's own edits land within a phase; every implementation task targets its edit by function name first and cites the pre-edit line range only as supporting context, never as a literal edit locator to be trusted after an earlier task in the same phase has already run.
- mcp__drm-copilot__run_poshqc_test returns only an {ok, tool, workspace_root, summary} payload: no exit code, no pass/fail/skip counts, no per-test names, and no coverage figure. This was measured and recorded in docs/features/archive/2026-08-10-excludefromcodecoverage-nested-lambdas-457/plan.2026-08-10T14-08.md (Conventions section). Every task in this plan that needs numeric or per-test PowerShell test evidence runs the MCP tool for the policy record AND pairs it with a direct Pester run that supplies the numbers, using this shape: a pwsh -NoProfile -Command invocation with the OUTER wrapper single-quoted and the INNER script double-quoted only (never \" and never a double-quoted outer wrapper — the archived plan measured the double-quoted-outer form failing under sh/git-bash because the shell expands $_ before pwsh parses it), building a New-PesterConfiguration with Run.Path, Run.PassThru = $true, Output.Verbosity = "Detailed", CodeCoverage.Enabled = $true, CodeCoverage.Path, and CodeCoverage.OutputPath under the applicable FEATURE/evidence/ kind-segment folder (baseline, regression-testing, or qa-gates as named by the calling task), then an explicit trailing "if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }" (Run.Exit defaults to $false in Pester 5 and Invoke-Pester never sets a process exit code on its own, so this trailing branch is what makes EXIT_CODE load-bearing).
- Per-file coverage is read from $r.CodeCoverage.CommandsExecuted / $r.CodeCoverage.CommandsMissed filtered by their .File property, because $r.CodeCoverage.CoveragePercent is an aggregate across every analyzed file and cannot render a per-file verdict. Pester 5 reports command/line coverage only; record "branch coverage: not emitted by Pester 5" as a measured fact, never a placeholder.
- Every mcp__drm-copilot__run_poshqc_format, mcp__drm-copilot__run_poshqc_analyze, mcp__drm-copilot__run_poshqc_analyze_autofix, and mcp__drm-copilot__run_poshqc_test call in this plan uses scan_folders = ["scripts/vscode", "tests/scripts/vscode"] and workspace_root = the repository root, per the delegation scope for this item. No full-repository scan is used anywhere in this plan.
- Because the scan_folders scope is folder-level and both folders contain files outside this plan's write set (for example scripts/vscode/Sync-PackageReferences.ps1, scripts/vscode/Invoke-VSBuild.ps1, tests/scripts/vscode/Invoke-VSBuild.Tests.ps1), every format run in this plan is followed by a git status --porcelain -- scripts/vscode tests/scripts/vscode check; any rewritten path outside this plan's write set is reverted with git checkout -- followed by that path, and the reversion is recorded, so unrelated formatter drift is never mistaken for a scope violation or silently committed.
- This plan's write set (the only paths any task may create or modify, besides FEATURE/evidence/ and FEATURE/plan.2026-09-02T12-01.md itself): scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, scripts/vscode/Invoke-MSTestWithCoverage.ps1, scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1, scripts/vscode/Invoke-MSTest.ps1, scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 (new, introduced by Phase 1 per the file-size analysis below), tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1, tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 (new, paired with the new production file), and conditionally tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 (only if Phase 4's size check requires the split). The new production file is a necessary consequence of the 500-line ceiling in .claude/rules/general-code-change.md and .claude/rules/powershell.md, which is a hard, non-negotiable constraint that takes precedence over spec.md's stated file-placement preference when the two conflict; spec.md's substantive requirement (one new pure per-package rate helper, reused by both the document-level summarizer and the merge function) is fully honored, only its file placement is adjusted for size.

### Change Budget Override

.claude/rules/powershell.md's Change Budget section states a per-batch cap in all modes of at most 3 production files and 3 test files unless an explicit override has been approved. This plan's write set exceeds that cap: it spans 5 production files (scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, scripts/vscode/Invoke-MSTestWithCoverage.ps1, scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1, scripts/vscode/Invoke-MSTest.ps1, and the new scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1) and up to 5 test files (tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1, tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1, and the conditional tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1).

An explicit override has been approved by the orchestrator for this run, recorded in artifacts/orchestration/orchestrator-state.json under change_budget_override. The rationale: issue 733 is a single, user-directed, pre-scoped consolidation of seven independently-reported findings (source issues 529, 530, 531, 537, 559, 560, 713) merged into one GitHub issue since all seven are small, same-subsystem fixes confined to the scripts/vscode PowerShell MSTest/Cobertura coverage tooling. The four-file production scope and single branch/issue framing were fixed before planning began, and splitting this pre-scoped item into multiple batches would fragment one already-consolidated issue into multiple PRs, which is outside this planning pass's authority to redefine. The fifth production file is not scope creep but a mechanical consequence of the independently-verified 500-line file-size ceiling on scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 (492 of 500 lines already used before this change, per P0-T4's measured baseline).

## Scope Prohibitions (binding on every task in this plan)

- Do NOT change any coverage threshold value or wire a CI coverage gate. That ownership belongs to issues #561, #562, and #563.
- Do NOT modify any file outside scripts/vscode/ and tests/scripts/vscode/, other than this plan's own evidence and plan-file paths under FEATURE.
- Do NOT apply finding 7's @(...) extraction to Invoke-MSTestWithCoverage.ps1's existing discovery block; it is already @()-wrapped (current lines 296-302) and needs no change. The extraction applies only to Invoke-MSTest.ps1.
- Do NOT attempt a signature-based re-key of the ClosureFilter.ps1 presence set for finding 6. This was evaluated and rejected as infeasible in spec.md's Root Cause Analysis: Get-CoberturaClosureDeclaringMemberName can never recover a signature from Roslyn's closure-naming convention, and forcing a signature key would flip the failure direction from safe under-exclusion to forbidden over-exclusion.
- Do NOT introduce a deduplication key into finding 2's union-append loop.
- Do NOT create temporary files anywhere, in production code, in tests, or in evidence capture.
- Do NOT modify CLAUDE.md or anything under .claude/rules/.
- Do NOT modify coverage.config, TaskMaster.runsettings, or scripts/vscode/TaskMaster.cli.runsettings.
- Do NOT modify any C# source file.

### Phase 0 — Policy reads, feature-document reads, and PowerShell toolchain baseline

- [ ] [P0-T1] Read CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, and .claude/rules/powershell.md in that order, per policy-compliance-order, and write FEATURE/evidence/baseline/phase0-instructions-read.TIMESTAMP.md.
  - Acceptance: the artifact exists and contains Timestamp, Policy Order (the four files in the order above), and an explicit Files Read list naming each file by its repository-relative path.

- [ ] [P0-T2] Read the feature requirement documents and the current production and test files this plan touches, and write FEATURE/evidence/baseline/phase0-feature-documents-read.TIMESTAMP.md.
  - Documents: FEATURE/issue.md, FEATURE/spec.md, FEATURE/research/research-findings.2026-09-02T13-15.md, scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, scripts/vscode/Invoke-MSTestWithCoverage.ps1, scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1, scripts/vscode/Invoke-MSTest.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1, tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1.
  - Acceptance: the artifact exists, contains Timestamp and the explicit file list, and records "Work Mode: full-bug" and "AC Source: FEATURE/spec.md (sole source)".

- [ ] [P0-T3] Record the branch and commit baseline in FEATURE/evidence/baseline/branch-commit-baseline.TIMESTAMP.md.
  - Commands: git rev-parse --abbrev-ref HEAD, git rev-parse HEAD, git status --porcelain.
  - Acceptance: the artifact records the branch name, the full HEAD SHA, and the verbatim porcelain output. The recorded SHA is a record of state, never an expectation any later task asserts against.

- [ ] [P0-T4] Record the current line count of every file read in P0-T2 (excluding issue.md, spec.md, and research-findings.2026-09-02T13-15.md) in FEATURE/evidence/baseline/file-size-headroom.TIMESTAMP.md, together with the remaining headroom against the 500-line ceiling in .claude/rules/general-code-change.md and .claude/rules/powershell.md.
  - Acceptance: the artifact records, for each of the seven files, its current line count and computed headroom (500 minus the current count). It explicitly flags that scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 (492 lines measured during this planning pass) has only 8 lines of headroom, and states that Phase 1 addresses this by extracting the new Get-CoberturaPackageLineSummary helper into a new sibling production file, scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1, rather than adding it inline.

- [ ] [P0-T5] Capture the PoshQC format baseline over scan_folders = ["scripts/vscode", "tests/scripts/vscode"] and write FEATURE/evidence/baseline/poshqc-format.TIMESTAMP.md.
  - Command: mcp__drm-copilot__run_poshqc_format with the scan_folders and workspace_root values defined in Conventions.
  - Immediately after the run, capture git status --porcelain -- scripts/vscode tests/scripts/vscode. If any rewritten path is not one of the seven files named in P0-T4, revert that path with git checkout -- followed by the path itself, and record the reversion; baseline formatting churn in an unrelated file must not be mistaken for a feature edit by any later scope audit.
  - Acceptance: the artifact carries Timestamp, Command, the MCP ok/summary payload, and Output Summary naming which of the seven in-scope files (if any) were rewritten, plus the reversion record for any out-of-scope file touched.

- [ ] [P0-T6] Capture the PoshQC analyze baseline over the same scan_folders and write FEATURE/evidence/baseline/poshqc-analyze.TIMESTAMP.md.
  - Command: mcp__drm-copilot__run_poshqc_analyze with the scan_folders and workspace_root values defined in Conventions, paired with a direct pwsh -NoProfile -Command invocation of Invoke-ScriptAnalyzer -Path (single-quoted outer, double-quoted inner) run individually against each of the seven files named in P0-T4, because the MCP payload reports only a count.
  - Acceptance: the artifact carries the four required fields; Output Summary records the diagnostic count by severity and the full diagnostic list (rule name, severity, file, line) for each of the seven files. This verbatim list is the baseline set P5-T2 compares against.

- [ ] [P0-T7] Capture the PoshQC Pester test baseline and write FEATURE/evidence/baseline/poshqc-test.TIMESTAMP.md.
  - Command: mcp__drm-copilot__run_poshqc_test with the scan_folders and workspace_root values defined in Conventions, paired with a direct Pester run per Conventions, with Run.Path covering every *.Tests.ps1 file under tests/scripts/vscode, CodeCoverage.Enabled = $true, and CodeCoverage.Path = the four existing production files (scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, scripts/vscode/Invoke-MSTestWithCoverage.ps1, scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1, scripts/vscode/Invoke-MSTest.ps1). Write the resulting coverage XML to FEATURE/evidence/baseline/pester-coverage.TIMESTAMP.xml.
  - Acceptance: the artifact carries the four required fields. Output Summary records: (a) overall Passed/Failed/Skipped counts; (b) individual Passed/Failed/Skipped counts for exactly tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1, and tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1; (c) the numeric line/command-coverage percent for each of the four production files individually, derived from $r.CodeCoverage.CommandsExecuted and $r.CodeCoverage.CommandsMissed filtered by .File; (d) "branch coverage: not emitted by Pester 5" as a measured fact.

### Phase 1 — Merge-CoberturaClassesByFilename fixes (findings 1, 2, 4)

Scope: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, the new scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1, and the new tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1. Per the bugfix workflow in CLAUDE.md's General Code Change Policy, the regression tests for findings 1 and 2 are written first and are expected to fail; finding 4 is a test-only addition against already-correct behavior and is not expected to fail.

- [ ] [P1-T1] [expect-fail] Create tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 with Set-StrictMode -Version Latest, a BeforeAll that dot-sources scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 resolved from $PSScriptRoot (mirroring the BeforeAll pattern at tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 current lines 3-7), and a Describe 'Get-CoberturaPackageLineSummary' block containing one It that builds a two-class <package> fixture and asserts the returned object's LineRate, BranchRate, LinesCovered, LinesValid, BranchesCovered, and BranchesValid values match hand-computed totals across both classes.
  - Expected pre-fix failure: CommandNotFoundException, because Get-CoberturaPackageLineSummary does not exist yet.
  - Evidence: FEATURE/evidence/regression-testing/case-01-package-summary-basic.TIMESTAMP.md.

- [ ] [P1-T2] [expect-fail] Add a second It to the Describe 'Get-CoberturaPackageLineSummary' block in tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1: a <package> fixture whose classes carry no <lines> elements, asserting LineRate and BranchRate both fall back to the string '0', matching Get-CoberturaCoverageSummary's existing zero-denominator fallback convention (Helpers.ps1 current lines 132-133).
  - Expected pre-fix failure: CommandNotFoundException.
  - Evidence: FEATURE/evidence/regression-testing/case-02-package-summary-zero-denominator.TIMESTAMP.md.

- [ ] [P1-T3] [expect-fail] Extend the existing test "computes the merged per-file line-rate from the merged rollup alone" in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 (current lines 238-271) with an assertion that the surviving <package> node's line-rate and branch-rate attributes equal the values computed from the merged package's classes, read via $resultXml.SelectSingleNode('//package').'line-rate' and .'branch-rate'.
  - Expected pre-fix failure: the package node's line-rate and branch-rate attributes remain at the fixture's stale input value ('0'), because no code path currently writes them after a merge.
  - Evidence: FEATURE/evidence/regression-testing/case-03-package-rate-stale.TIMESTAMP.md.

- [ ] [P1-T4] [expect-fail] Update the existing test "preserves the primary class methods subtree and every hits value when merging" in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 (current lines 316-349) to assert the union-merge outcome: $methodNodes.Count | Should -Be 2, with the retained method names containing both 'M' and 'N'. Update the test's own comment (current line 317, "Locks the decision not to merge or strip <methods>.") to state that this now locks the union-merge decision instead. This is a deliberate, spec-approved reversal of the test's prior assertion per spec.md's Risks & Mitigations section, not an unintended regression.
  - Expected pre-fix failure: $methodNodes.Count remains 1, containing only 'M', against the updated assertion of 2.
  - Evidence: FEATURE/evidence/regression-testing/case-04-methods-union-existing-test.TIMESTAMP.md.

- [ ] [P1-T5] [expect-fail] Add a new, isolated 3-member merge fixture to tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1: one declaring class contributing method 'M', and two distinct closure classes sharing the same filename contributing methods 'N' and 'O' respectively (spot-checking spec.md's Assumptions section that distinct group members never legitimately share an identical method name). Assert the merged class's <methods> node contains all three method names with no duplication.
  - Expected pre-fix failure: only method 'M' is present (today's clone-primary-only behavior).
  - Evidence: FEATURE/evidence/regression-testing/case-05-methods-union-three-way.TIMESTAMP.md.

- [ ] [P1-T6] Add a new, minimal, focused fixture to tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 isolating the max(hits) second-seen-strictly-higher merge branch (finding 4): exactly two classes sharing one filename, exactly one overlapping line number, with only the hits value varying and the second-seen class strictly higher. Assert the merged line's hits attribute equals the higher value.
  - Not tagged [expect-fail]: the existing production code at Helpers.ps1 current line 329 already handles this branch correctly; this task closes a test-coverage gap identified by finding 4, per spec.md's corrected scope, with no production code change.
  - Evidence: FEATURE/evidence/regression-testing/case-06-max-hits-second-seen.TIMESTAMP.md.

- [ ] [P1-T7] Run the Phase 1 regression additions (P1-T1 through P1-T6) via mcp__drm-copilot__run_poshqc_test scoped as in Conventions, paired with a direct Pester run whose Run.Path is tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 and tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1, and write FEATURE/evidence/regression-testing/expect-fail-run-phase1.TIMESTAMP.md.
  - Acceptance: the artifact names each of P1-T1 through P1-T5 individually and records its observed CommandNotFoundException or assertion-mismatch failure exactly as predicted in that task, and confirms P1-T6 passes.

- [ ] [P1-T8] Create scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 with Set-StrictMode -Version Latest and the pure function Get-CoberturaPackageLineSummary, accepting a mandatory PackageNode parameter typed [System.Xml.XmlElement].
  - Contract: pure, no I/O, mutates nothing. Accumulates over $PackageNode.SelectNodes('.//class') using Get-CoberturaClassLineSummary per class (the same per-class accumulation pattern already used inside Get-CoberturaCoverageSummary at Helpers.ps1 current lines 117-129), and returns the identical pscustomobject shape (LineRate, BranchRate, LinesCovered, LinesValid, BranchesCovered, BranchesValid) using the identical rounding and zero-denominator fallback expression Get-CoberturaCoverageSummary already uses (Helpers.ps1 current lines 132-133).
  - Acceptance: the file exists with [CmdletBinding()] and [OutputType([pscustomobject])] on the function, and the P1-T1 and P1-T2 assertions pass against it in isolation.

- [ ] [P1-T9] Add a dot-source line for scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 to the top of scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, immediately after the existing dot-source of Invoke-MSTestWithCoverage.ClosureFilter.ps1 (current line 2), mirroring that line's Join-Path $PSScriptRoot pattern exactly.
  - Acceptance: dot-sourcing Helpers.ps1 alone makes Get-CoberturaPackageLineSummary callable, verified by tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1's existing BeforeAll (current lines 3-7), which dot-sources only Helpers.ps1.

- [ ] [P1-T10] Refactor Get-CoberturaCoverageSummary in scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 (current lines 99-139) to call Get-CoberturaPackageLineSummary once per package node and sum its LinesCovered, LinesValid, BranchesCovered, and BranchesValid outputs into the existing document-level totals, replacing the current inline per-class accumulation loop (current lines 117-129).
  - Acceptance: every existing Describe 'ConvertTo-KoverageCoberturaXml' test in tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 that asserts document-level lines-covered, lines-valid, line-rate, branches-covered, or branches-valid (current lines 53-236) continues to pass unchanged, proving the refactor is behavior-preserving at the document level.

- [ ] [P1-T11] Add the union-append loop to Merge-CoberturaClassesByFilename in scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1: after the existing methods-node-existence check (current lines 297-301), iterate every member of $group other than $primaryNode and append a deep clone of each of its ./methods/method children into $methodsNode.
  - Acceptance: P1-T4 and P1-T5's assertions pass against this change alone (before P1-T12 lands).

- [ ] [P1-T12] Add package-level rate recomputation to Merge-CoberturaClassesByFilename in scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1: immediately before the outer foreach ($packageNode in $XmlDocument.SelectNodes('//package')) loop's closing brace (current line 390), call Get-CoberturaPackageLineSummary on $packageNode and set its line-rate and branch-rate attributes using the identical rounding and zero-denominator fallback expression already used for the merged class's own rate (current lines 371-372). In the same task, correct the comment at current lines 367-370 ("the spec specifies exactly one new helper"), which becomes inaccurate once this fix lands a second helper: replace it with an accurate explanation of why the merged CLASS rate still duplicates the rounding expression inline (Get-CoberturaPackageLineSummary is package-scoped, aggregating every class in the package, and is not a substitute for a single merged class's own rate).
  - Acceptance: P1-T3's assertion passes against this change.

- [ ] [P1-T13] Re-run the Phase 1 regression scope (same Run.Path as P1-T7) and confirm all of P1-T1 through P1-T6 now pass, and write FEATURE/evidence/regression-testing/pass-after-phase1.TIMESTAMP.md.
  - Acceptance: the artifact records Passed count equal to the total number of It cases added or updated across P1-T1 through P1-T6, with zero Failed and zero Skipped among them.

- [ ] [P1-T14] Measure the resulting line count of scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1, tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1, and tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1, and record the result in FEATURE/evidence/other/phase1-file-size-check.TIMESTAMP.md.
  - Acceptance: the artifact records each file's line count and confirms every one is at or under 500 lines. If any file exceeds 500 lines, this task's acceptance is not met until the most recently added self-contained block in that file (a Describe block, or a single function with its doc comment) is extracted into a further sibling file, that extraction is recorded in the same artifact, and the recount confirms all files are at or under 500 lines before Phase 2 begins.

### Phase 2 — Assembly-discovery .claude exclusion (finding 3)

Scope: scripts/vscode/Invoke-MSTestWithCoverage.ps1 and tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1.

- [ ] [P2-T1] [expect-fail] Add a new It to the existing Describe 'Invoke-MSTestWithCoverageMain' block in tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 (current lines 345-414), mocking Get-ChildItem to return two items — one ordinary path such as C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll and one under a .claude segment such as C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll — and capturing Invoke-DotnetCoverageCollection's -TestAssembly parameter (mirroring the existing Mock Invoke-DotnetCoverageCollection pattern at current lines 366-368), then calling Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir and asserting the captured -TestAssembly array contains only the ordinary path and excludes the .claude path.
  - Expected pre-fix failure: both paths are present in the captured array, because no .claude exclusion clause exists yet.
  - Evidence: FEATURE/evidence/regression-testing/case-07-claude-path-exclusion.TIMESTAMP.md.

- [ ] [P2-T2] Run the P2-T1 test via mcp__drm-copilot__run_poshqc_test scoped as in Conventions, paired with a direct Pester run whose Run.Path is tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1, and write FEATURE/evidence/regression-testing/expect-fail-run-phase2.TIMESTAMP.md.
  - Acceptance: the artifact records the P2-T1 test failing with both paths present in the captured -TestAssembly array.

- [ ] [P2-T3] Add a fourth -and clause to the Where-Object predicate inside Invoke-MSTestWithCoverageMain in scripts/vscode/Invoke-MSTestWithCoverage.ps1 (current lines 296-302): $_.FullName -notmatch '\\\.claude\\', placed alongside the existing \bin\, \obj\, and \ref\ clauses in the same style.
  - Acceptance: the file still parses and the outer @(...) wrapping (current line 296) is unchanged.

- [ ] [P2-T4] Re-run the P2-T1 test and confirm it now passes, and write FEATURE/evidence/regression-testing/pass-after-phase2.TIMESTAMP.md.
  - Acceptance: the artifact records the captured -TestAssembly array containing only the ordinary path.

### Phase 3 — ClosureFilter.ps1 documentation clarifications (findings 5, 6)

Scope: scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 and tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1. No production behavior change in this phase; per spec.md's corrected scope for findings 5 and 6, no task in this phase is tagged [expect-fail].

- [ ] [P3-T1] Add a docstring addendum to Get-CoberturaInstrumentedMemberName's .DESCRIPTION block in scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 (current lines 154-157, the "deliberately NOT admitted" paragraph), stating that the local-function exclusion is an asserted design choice, ratified by issue #733's research because no over-exclusion counter-example was found or constructed, and that it should be revisited if a genuine non-exempt-method-with-only-a-g__-entry case is ever observed.
  - Acceptance: the addendum is present in the function's comment-based help; no assertion, parameter, or return-value change is made. The existing test "removes a closure class outright when every method resolves to an absent member" (tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 current lines 152-190, Part B) still passes unchanged.

- [ ] [P3-T2] Add a second docstring addendum to the same .DESCRIPTION block documenting the bare-name overload-collision limitation for finding 6: the presence set is keyed by bare member name, so two overloads sharing a name under the same declaring type and file collide; state explicitly that the resulting failure direction is safe/under-exclusion (an exempt overload's closures are wrongly retained in the coverage denominator, permanently uncovered) rather than the forbidden over-exclusion direction, and cite that a signature-based re-key was evaluated and rejected as infeasible per spec.md's Root Cause Analysis, because Get-CoberturaClosureDeclaringMemberName can never recover a parameter signature from Roslyn's closure-naming convention.
  - Acceptance: the addendum is present and explicitly names both failure directions (safe/under-exclusion vs. forbidden/over-exclusion) and the reason a re-key is not proposed.

- [ ] [P3-T3] Add a new pinning regression test to tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1, inside the existing Describe 'Remove-CoberturaExemptClosureCoverage' block (current lines 9-324): a fixture with one declaring class carrying a single plain method named Overloaded (representing the non-exempt overload, which is the only one of the pair that emits a <method> element, since the exempt overload emits none), plus a sibling closure class carrying a method named <Overloaded>b__0. Assert that after Remove-CoberturaExemptClosureCoverage runs, the closure's lines survive (are retained) even though, under the exempt overload's true attribution, they should have been excluded — documenting the current, safe, under-exclusion collision behavior. Add a comment citing issue #733 finding 6 and the safe-direction rationale from P3-T2.
  - Acceptance: the test is present, passes without any production code change in this phase, and its comment cites issue #733 finding 6.
  - Evidence: FEATURE/evidence/regression-testing/case-08-overload-collision-pin.TIMESTAMP.md.

- [ ] [P3-T4] Run the P3-T3 test via mcp__drm-copilot__run_poshqc_test scoped as in Conventions, paired with a direct Pester run whose Run.Path is tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1, and write FEATURE/evidence/regression-testing/pass-after-phase3.TIMESTAMP.md.
  - Acceptance: the artifact records the P3-T3 test passing on this run, with no other test in the file regressing relative to the P0-T7 baseline.

- [ ] [P3-T5] Measure the resulting line count of scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 and tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1, and record the result in FEATURE/evidence/other/phase3-file-size-check.TIMESTAMP.md.
  - Acceptance: both files are at or under 500 lines.

### Phase 4 — Invoke-MSTest.ps1 discovery-pipeline extraction (finding 7)

Scope: scripts/vscode/Invoke-MSTest.ps1 and tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1, with a conditional split to tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1.

- [ ] [P4-T1] Measure the current line count of tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 (459 lines measured during this planning pass, before Phase 2's P2-T1 addition), add the number of lines P2-T1 already added, and project the additional size of a new Describe 'Get-MSTestAssemblyPathList' block with three It cases (zero matches, exactly one match, multiple matches). Record the projection and the resulting decision in FEATURE/evidence/other/phase4-test-file-placement.TIMESTAMP.md: if the projected total exceeds 500 lines, the new Describe block is placed in a new file, tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1, with its own BeforeAll dot-sourcing scripts/vscode/Invoke-MSTest.ps1 via the same . $script:mstestScript -NoExecute pattern used at tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 current line 10; otherwise the block is added to tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 directly.
  - Acceptance: the artifact records the measured line count, the projected total, and the chosen target file path in unambiguous prose, and every later Phase 4 task targets exactly that file.

- [ ] [P4-T2] [expect-fail] Add the Describe 'Get-MSTestAssemblyPathList' block, with three It cases, to the file chosen by P4-T1: (a) zero matches — Get-ChildItem mocked to return an empty array, asserting the returned array's Count equals 0 without throwing; (b) exactly one match — Get-ChildItem mocked to return a single item, asserting the returned array's Count equals 1 without throwing (the StrictMode regression case for finding 7); (c) multiple matches — Get-ChildItem mocked to return three items, asserting the returned array's Count equals 3.
  - Expected pre-fix failure: CommandNotFoundException on Get-MSTestAssemblyPathList in all three It cases, because the function does not exist yet.
  - Evidence: FEATURE/evidence/regression-testing/case-09-assembly-discovery-array-safety.TIMESTAMP.md.

- [ ] [P4-T3] Run the three P4-T2 It cases via mcp__drm-copilot__run_poshqc_test scoped as in Conventions, paired with a direct Pester run whose Run.Path is the file chosen by P4-T1, and write FEATURE/evidence/regression-testing/expect-fail-run-phase4.TIMESTAMP.md.
  - Acceptance: the artifact records all three cases failing with CommandNotFoundException.

- [ ] [P4-T4] Add the function Get-MSTestAssemblyPathList to scripts/vscode/Invoke-MSTest.ps1, placed alongside the file's other function definitions, after Invoke-VsTestExe (current lines 57-75) and before the Set-StrictMode -Version Latest at current line 77.
  - Contract: mandatory SearchRoot and Configuration string parameters; returns the existing discovery pipeline (current lines 107-113) wrapped in @(...), matching the pattern already used by Invoke-MSTestWithCoverage.ps1's equivalent discovery block (current lines 296-302).
  - Acceptance: the function is defined with [CmdletBinding()] and [OutputType([System.Array])] or an equivalent array-typed output attribute.

- [ ] [P4-T5] Replace the top-level assignment at current line 107 of scripts/vscode/Invoke-MSTest.ps1 with a call to Get-MSTestAssemblyPathList -SearchRoot $resolvedSearchRoot -Configuration $Configuration, removing the now-redundant inline pipeline (current lines 107-113).
  - Acceptance: scripts/vscode/Invoke-MSTest.ps1's top-level body no longer contains a bare, un-wrapped Get-ChildItem | Where-Object | Select-Object -ExpandProperty FullName pipeline.

- [ ] [P4-T6] Re-run the three P4-T2 It cases and confirm all three now pass, including the exactly-one-match case not throwing under Set-StrictMode -Version Latest, and write FEATURE/evidence/regression-testing/pass-after-phase4.TIMESTAMP.md.
  - Acceptance: the artifact records all three cases passing, with the exactly-one-match case's returned array Count explicitly recorded as 1.

- [ ] [P4-T7] Measure the resulting line count of scripts/vscode/Invoke-MSTest.ps1 and the file chosen by P4-T1, and record the result in FEATURE/evidence/other/phase4-file-size-check.TIMESTAMP.md.
  - Acceptance: both files are at or under 500 lines.

### Phase 5 — Final QA loop and acceptance-criteria check-off

- [ ] [P5-T1] Run mcp__drm-copilot__run_poshqc_format scoped as in Conventions, capture git status --porcelain -- scripts/vscode tests/scripts/vscode immediately after, revert any rewritten path outside this plan's write set (per Conventions), and write FEATURE/evidence/qa-gates/poshqc-format.iter1.TIMESTAMP.md.
  - Acceptance: the artifact carries the four required fields and names every file rewritten within this plan's write set, or states "no file rewritten".

- [ ] [P5-T2] Run mcp__drm-copilot__run_poshqc_analyze scoped as in Conventions and write FEATURE/evidence/qa-gates/poshqc-analyze.iter1.TIMESTAMP.md.
  - Acceptance: the artifact records the diagnostic count by severity and the full diagnostic list (rule, severity, file, line) for every file in this plan's write set, compared explicitly against the P0-T6 baseline list.

- [ ] [P5-T3] If P5-T2 reports one or more diagnostics whose rule PSScriptAnalyzer documents as auto-fixable, run mcp__drm-copilot__run_poshqc_analyze_autofix scoped as in Conventions, then restart this Final QA Loop from P5-T1 (incrementing the iter suffix on every artifact). If P5-T2 reports zero diagnostics, or only non-autofixable diagnostics, record that this task did not run and proceed to P5-T4.
  - Acceptance: either FEATURE/evidence/qa-gates/poshqc-analyze-autofix.iter1.TIMESTAMP.md exists recording the autofix run and the loop restart, or the artifact from P5-T2 explicitly states no autofixable diagnostics were present and this task is marked not-run for that reason.

- [ ] [P5-T4] Run mcp__drm-copilot__run_poshqc_test scoped as in Conventions, paired with a direct Pester run per Conventions: Run.Path covering every file in this plan's write set under tests/scripts/vscode (including tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 if P4-T1 selected it), CodeCoverage.Enabled = $true, CodeCoverage.Path covering every file in this plan's write set under scripts/vscode, PassThru = $true, and the explicit trailing exit-code branch. Write the resulting coverage XML to FEATURE/evidence/qa-gates/pester-coverage.final-qc.iter1.TIMESTAMP.xml and write FEATURE/evidence/qa-gates/poshqc-test.iter1.TIMESTAMP.md.
  - Acceptance: the artifact carries the four required fields with EXIT_CODE 0 from the direct Pester run, and Output Summary records the numeric Passed/Failed/Skipped counts and the per-file coverage percent for every production file in this plan's write set. If any test fails, or if P5-T1 or P5-T3 changed a file on this iteration, restart the Final QA Loop from P5-T1 with the next iter suffix.

- [ ] [P5-T5] Compare the final P5-T4 counts and per-file coverage percentages against the P0-T7 baseline, and write FEATURE/evidence/qa-gates/toolchain-delta.TIMESTAMP.md.
  - Acceptance: the artifact names, individually: (a) the net new It-case count added across P1-T1 through P1-T6, P2-T1, P3-T3, and P4-T2; (b) the deliberate assertion-count change in "preserves the primary class methods subtree and every hits value when merging" (tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1), confirming it is now counted as passing under the union-merge assertion (methodNodes.Count equal to 2) rather than the pre-fix assertion (methodNodes.Count equal to 1), so this gate is not vacuous with respect to that specific behavior change; (c) that Skipped equals 0 for every named test file; (d) that the per-production-file coverage percentage for every file in this plan's write set is at or above 85 percent, per the uniform line-coverage floor in .claude/rules/powershell.md and .claude/rules/quality-tiers.md, and that no file's coverage percentage for lines that existed before this plan's changes decreased relative to the P0-T7 baseline.

- [ ] [P5-T6] [AC1] Check off spec.md's Acceptance Criteria item "Repro steps now produce the expected behavior in all documented environments." against the evidence recorded by P1-T13, P2-T4, P3-T4, and P4-T6, and record the check-off in FEATURE/evidence/qa-gates/acceptance-criteria-status.TIMESTAMP.md.
  - Acceptance: the artifact cites all four pass-after evidence paths and states the expected behavior for each of findings 1, 2, 3, and 7 now holds.

- [ ] [P5-T7] [AC2] Check off "Regression test(s) added and passing (list file path and test name)." in the same artifact, listing every new or updated test by file path and It description added across P1-T1 through P1-T6, P2-T1, P3-T3, and P4-T2, each confirmed passing by its corresponding pass-after task.
  - Acceptance: the artifact enumerates every listed test individually; no test is omitted from the list.

- [ ] [P5-T8] [AC3] Check off "Edge cases and invalid inputs are handled with correct errors or fallbacks." in the same artifact, citing the zero-denominator fallback fixture in P1-T2, the zero-match and multiple-match cases in P4-T2, and the fail-safe under-exclusion direction pinned in P3-T3.
  - Acceptance: the artifact names all three cited items.

- [ ] [P5-T9] [AC4] Check off "No unintended behavior changes outside the defined scope." in the same artifact, citing the P5-T1 drift-detection-and-revert safeguard, the P5-T5 per-file coverage listing confined to this plan's write set, and the output of git status --porcelain run at the repository root (not scoped to scripts/vscode or tests/scripts/vscode, so the check can catch a stray change anywhere in the tree, which is the entire point of the AC4 gate). No task in this plan stages or commits the plan's own changes before this task runs, so an anchored git diff against a ref would report nothing regardless of what the executor touched; git status --porcelain instead surfaces every staged, unstaged, and untracked path, including brand-new untracked files such as scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 and tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1. Confirm every reported path falls under one of exactly three allowed prefixes: scripts/vscode/, tests/scripts/vscode/, or docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/ (the last prefix covers this task's own and sibling P5-T6 through P5-T13 AC check-off edits to the acceptance-criteria-status artifact, and any plan checkbox updates the executor itself makes).
  - Acceptance: the artifact records the verbatim git status --porcelain output and confirms every reported path (staged, unstaged, and untracked) falls under one of the three allowed prefixes named above; any path outside those three prefixes fails this task's acceptance.

- [ ] [P5-T10] [AC5] Check off "Required logs/telemetry updated and validated (if applicable)." in the same artifact as Not Applicable, citing spec.md's Data / API / Config Impact section ("Logging/telemetry updates (if any): None.").
  - Acceptance: the artifact records the Not Applicable determination with its citation.

- [ ] [P5-T11] [AC6] Check off "Performance constraints met or explicitly waived with rationale." in the same artifact as explicitly waived, citing spec.md's Proposed Fix section ("Performance constraints (latency/throughput/memory): N/A") and noting no new I/O or expensive operation is introduced by any of the seven findings.
  - Acceptance: the artifact records the waiver with its citation and rationale.

- [ ] [P5-T12] [AC7] Check off "Full toolchain pass completed (format → lint → type-check → test)." in the same artifact, citing the clean, non-file-changing, non-failing final iteration of P5-T1, P5-T2, and P5-T4, and recording that type-check is Not Applicable for PowerShell per .claude/rules/powershell.md.
  - Acceptance: the artifact cites the specific final-iteration artifact paths for format, analyze, and test.

- [ ] [P5-T13] [AC8] Check off "Docs/config references updated to match the new behavior." in the same artifact, citing the P3-T1 and P3-T2 docstring clarifications in scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 and the P1-T12 comment correction in scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, and confirming FEATURE/spec.md already documents the corrected, post-fix scope with no further edit required.
  - Acceptance: the artifact cites both docstring/comment locations and the spec.md determination.

## Planner Adversarial Self-Review

Revision round: this pass applies two preflight-directed deltas (Change Budget Override subsection; P5-T9 acceptance-condition replacement) to the plan previously validated at CITATION-TO-TREE PASS. Per atomic-plan-contract, every citation this pass's edits touched is re-derived below directly against current repository state, and the sibling region around each edit is re-checked for invalidated assumptions. Citations unaffected by this round's two deltas (the Phase 0-4 findings-specific line citations) were not re-touched by this pass's edits and are not re-asserted here; they remain part of the plan's overall citation set as already recorded in the Planner Internal Review Record below.

SELF-REVIEW: RE-DERIVED THIS PASS
- .claude/rules/powershell.md — read this pass (Change Budget section, lines 37-41); confirmed the exact per-batch cap text "at most 3 production files and 3 test files unless an explicit override has been approved" (line 40) that the new Change Budget Override subsection cites. Re-derived directly against current tree state for this pass's edit.
- artifacts/orchestration/orchestrator-state.json — read this pass (change_budget_override block, lines 67-73); confirmed approved: true, the 5-production/5-test scope description, and the rationale text (source issues 529, 530, 531, 537, 559, 560, 713; fixed four-file scope; fifth file as a mechanical consequence of the Helpers.ps1 size ceiling) that the new Change Budget Override subsection paraphrases. Re-derived directly against current tree state for this pass's edit.
- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 — re-read this pass (lines 480-492, tail of file); confirmed 492 total lines, matching the file-size figure the Change Budget Override subsection now cites (492 of 500, not the 491 given in the orchestrator-state.json rationale prose, which is corrected here to match the plan's own P0-T4 measured baseline rather than carried forward from the delegation prompt).
- FEATURE/plan.2026-09-02T12-01.md, Conventions section (sibling region for delta 1) — re-read lines 12-23 this pass; the existing write-set enumeration (line 23: 5 production files, up to 5 test files) is unchanged by this round and matches the counts newly stated in the Change Budget Override subsection with no discrepancy; no invalidated assumption found in the sibling Conventions bullets (lines 14-22).
- FEATURE/plan.2026-09-02T12-01.md, whole-document search for "git commit" and "git add " (basis for the P5-T9 edit) — searched this pass; zero matches found anywhere in the plan, confirming no task stages or commits the plan's own changes before P5-T9 runs, which is the load-bearing fact for replacing the anchored git diff with an unstaged-aware git status --porcelain check.
- FEATURE/plan.2026-09-02T12-01.md, Phase 5 sibling AC check-off tasks (sibling region for delta 2) — re-read P5-T1 through P5-T8 and P5-T10 through P5-T13 (lines 191-228) this pass; none of them cite the removed anchored git diff, none assume a prior commit or staging step, and none is invalidated by P5-T9's replacement. P5-T1's existing scoped git status --porcelain -- scripts/vscode tests/scripts/vscode (line 191, Conventions line 22) is a distinct, narrower, drift-detection invocation from P5-T9's new unscoped repository-root git status --porcelain; the two do not conflict.
- FEATURE/plan.2026-09-02T12-01.md, revised P5-T9 text itself — re-read after editing; confirmed the three allowed prefixes (scripts/vscode/, tests/scripts/vscode/, docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/) collectively cover every path this plan's own tasks write to (evidence artifacts, plan checkbox edits, and the new PackageRate production/test files), so the AC4 gate is satisfiable on a compliant run and can fail on a genuine out-of-scope write.

## Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS

CITATION-TO-TREE: PASS
- CITATION: docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/issue.md | findings 1-7, lines 34-47
- CITATION: docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/spec.md | Proposed Fix design summary lines 78-92, Root Cause Analysis correction lines 72-75, Write Set lines 196-204, Acceptance Criteria lines 172-180
- CITATION: docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/research/research-findings.2026-09-02T13-15.md | per-finding fix proposals, section 3, lines 32-147
- CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | Get-CoberturaCoverageSummary lines 99-139; Merge-CoberturaClassesByFilename lines 262-391; methods-node handling lines 295-301; max(hits) line 329; stale comment lines 367-370; merged-class rate lines 371-375; tail/line-count check lines 480-492
- CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | Invoke-MSTestWithCoverageMain lines 248-345; discovery Where-Object filter lines 296-302
- CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | Get-CoberturaInstrumentedMemberName lines 134-209; local-function exclusion doc lines 154-157
- CITATION: scripts/vscode/Invoke-MSTest.ps1 | function definitions lines 12-75; unwrapped discovery pipeline lines 107-113
- CITATION: tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | "computes the merged per-file line-rate..." lines 238-271; "preserves the primary class methods subtree..." lines 316-349
- CITATION: tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | Describe 'Remove-CoberturaExemptClosureCoverage' lines 9-324, Part B lines 152-190
- CITATION: tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | BeforeAll lines 3-25; Describe 'Invoke-MSTestWithCoverageMain' lines 345-414
- CITATION: .claude/rules/powershell.md | toolchain order lines 13-20; 500-line ceiling line 35; coverage floor lines 63-65; Change Budget section lines 37-41
- CITATION: docs/features/archive/2026-08-10-excludefromcodecoverage-nested-lambdas-457/plan.2026-08-10T14-08.md | Conventions section lines 12-28
- CITATION: artifacts/orchestration/orchestrator-state.json | change_budget_override block lines 67-73

AC-TRACEABILITY: PASS

SCOPE-BOUNDARY: PASS — every write task in this plan targets a path under scripts/vscode/ or tests/scripts/vscode/, or an evidence/plan path under FEATURE, matching spec.md's Scope & Non-Goals section (lines 55-70) and the Scope Prohibitions section above. This round's two deltas add no new write-target paths: the Change Budget Override subsection is documentation-only prose under FEATURE/plan.2026-09-02T12-01.md, and P5-T9's revised check reads (never writes outside) the repository-root git status --porcelain output.

AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8

AC-MAPPING: AC1 | IMPLEMENTATION: P1-T8, P1-T9, P1-T10, P1-T11, P1-T12, P2-T3, P4-T4, P4-T5 | TESTS: P1-T13, P2-T4, P3-T4, P4-T6 | EVIDENCE: P5-T6
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T1, P1-T2, P1-T3, P1-T4, P1-T5, P1-T6, P2-T1, P3-T3, P4-T2 | TESTS: P1-T13, P2-T4, P3-T4, P4-T6 | EVIDENCE: P5-T7
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T2, P3-T3, P4-T2 | TESTS: P1-T13, P3-T4, P4-T6 | EVIDENCE: P5-T8
AC-MAPPING: AC4 | IMPLEMENTATION: P5-T1 | TESTS: P5-T5 | EVIDENCE: P5-T9
AC-MAPPING: AC5 | IMPLEMENTATION: N/A (spec.md Data / API / Config Impact: None) | TESTS: N/A (no telemetry surface exists) | EVIDENCE: P5-T10
AC-MAPPING: AC6 | IMPLEMENTATION: N/A (spec.md Proposed Fix Performance constraints: N/A) | TESTS: N/A (no performance-sensitive change introduced) | EVIDENCE: P5-T11
AC-MAPPING: AC7 | IMPLEMENTATION: P5-T1, P5-T2, P5-T3 | TESTS: P5-T4 | EVIDENCE: P5-T12
AC-MAPPING: AC8 | IMPLEMENTATION: P3-T1, P3-T2, P1-T12 | TESTS: N/A (documentation-only change, pinned by existing tests re-confirmed in P3-T4/P1-T13) | EVIDENCE: P5-T13

UNRESOLVED-GAPS: NONE

## Validator Status

VALIDATOR NOT RUN: tool unavailable in this agent's tool surface. This planner subagent's tool surface for this session is file-only (Read, Grep, Glob, Edit, Write); mcp__drm-copilot__validate_orchestration_artifacts is not present in it. A structural self-check was performed instead: every phase heading matches the exact "### Phase N — <Title>" form; every task line matches "- [ ] [P#-T#] <description>" with sequential, digit-only task numbering restarting at T1 per phase; every evidence path resolves under FEATURE/evidence/ followed by one of the four kind segments (baseline, regression-testing, qa-gates, other); no repository-relative file path anywhere in this document is backtick-delimited; the new Change Budget Override subsection uses plain-prose paths with no backticks and no placeholder brackets; and this document does not itself carry a second "## Write Set" section. The calling orchestrator must run mcp__drm-copilot__validate_orchestration_artifacts with artifact_type "plan" and artifact_path docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/plan.2026-09-02T12-01.md, or route this plan through atomic-executor preflight, before it is treated as approved.

PREFLIGHT: NOT APPLICABLE — this signal belongs to atomic-executor preflight review, which this planner subagent does not perform on its own behalf.
