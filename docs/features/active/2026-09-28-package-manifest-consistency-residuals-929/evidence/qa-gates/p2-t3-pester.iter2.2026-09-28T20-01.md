# P2-T3 — PowerShell QC step 3, test, with coverage read from CI (iteration 2)

Timestamp: 2026-09-30T10-58
Command: pwsh -NoProfile -Command '[DateTime]::UtcNow.ToString("o")' (RUN-START); MCP mcp__drm-copilot__run_poshqc_test (workspace_root <execution-worktree-root>, scan_folders ["tests/scripts/dependencies"]); CMD-JUNIT-READ; CMD-CI-LIST (twice); CMD-CI-WATCH with RUN-ID 36722780748; CMD-CI-PESTER with RUN-ID 36722780748 and DIR coverage/ci-branch-pester-36722780748-2
EXIT_CODE: 0
Output Summary:

Local MCP test run:
- RUN-START: 2026-09-30T13:46:37.9133883Z
- MCP payload (host prefix replaced): {"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}
- JUNIT-WRITTEN=2026-09-30T13:47:26.1351476Z (later than RUN-START)
- JUNIT-ROOT tests=137 failures=0 errors=0 disabled=0; no JUNIT-NOTPASSED line
- JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0
- JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
- JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
- Other suites: AnalyzerItemRepair 13, PackageCompatibility 8, PackageGraph 32, ProjectConsistency 18, Repair-PackageManifestConsistency 31, all failures=0

Push check: HEAD b96926588d562f994430e7ba7301de5de86f206c equals PUSHED-HEAD (no P2-T1 or P2-T2 commit in either iteration), so no push was made.

CI locate and watch (no dispatch: CMD-CI-DISPATCH was already made once for this pushed head in iteration 1, and the rule permits at most one dispatch per pushed head):
- CMD-CI-LIST (first): HEAD=b96926588d562f994430e7ba7301de5de86f206c; REMOTE names HEAD; RUNS-FOR-HEAD=0 (a transient empty list from gh; the run was listed in iteration 1)
- CMD-CI-LIST (re-run 1): HEAD and REMOTE as above; RUNS-FOR-HEAD=1; RUN id=36722780748 head=b96926588d562f994430e7ba7301de5de86f206c event=workflow_dispatch status=completed conclusion=failure; an unfiltered listing of the branch's ci.yml runs shows the same single run
- Selected run: 36722780748
- CMD-CI-WATCH invocation 1: "Run CI (36722780748) has already completed with 'failure'"; WATCH-EXIT=0

CMD-CI-PESTER (second invocation against run 36722780748, head b96926588):
- RUN id=36722780748 head=b96926588d562f994430e7ba7301de5de86f206c branch=bug/package-manifest-consistency-residuals-929 event=workflow_dispatch status=completed conclusion=failure workflow=CI
- PESTER-JOBS=1; JOB id=109911885533 name=pester / Run Pester suite with coverage status=completed conclusion=success
- LOG-LINES=1097
- "Tests Passed: 379, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0"
- "PESTER Passed=379 Failed=0 Skipped=0 Total=379"
- "COVERAGE LinePercent=94.51 Covered=1721 Total=1821"
- Total 379 equals BASELINE-TOTAL 373 plus 6; Failed 0; Skipped 0
- DIR-PREEXISTS=False; DOWNLOAD-EXIT=0; ARTIFACT-FILES=1; DIR used: coverage/ci-branch-pester-36722780748-2
- REPORT-LINE covered=1721 missed=100; computed percent 94.51, equal to the log's LinePercent and at least 80
- MEETS-85: true (observation only, convention 10)
- SOURCEFILE AnalyzerItemRepair.psm1 covered=106 missed=0
- SOURCEFILE ConsistencyVerifier.psm1 covered=158 missed=2 (covered at least VERIFIER-COVERED 158; missed at most VERIFIER-MISSED 2)
- SOURCEFILE PackageCompatibility.psm1 covered=33 missed=0
- SOURCEFILE PackageGraph.psm1 covered=164 missed=0
- SOURCEFILE ProjectConsistency.psm1 covered=103 missed=0
- SOURCEFILE Repair-PackageManifestConsistency.ps1 covered=213 missed=14

Run-level conclusion failure, recorded and reported as in iteration 1: every job succeeded except mstest-coverage / Run MSTest suite with coverage (1 of 7346 failed: Transaction_SecondCallerCannotInstallUntilTheFirstRestores, QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs). build-analyzers, hygiene, pester, actionlint, format-check and build-nullable: success.

Pester emits no branch counter, so no PowerShell branch figure is claimed. These figures stand in for a permitted evidence form that the committed-evidence section does not define for the Pester route. The downloaded JaCoCo document is left under the ignored coverage directory.

GATE-SUBSTITUTION: CI Pester job on the pushed head stands in for a local coverage run
