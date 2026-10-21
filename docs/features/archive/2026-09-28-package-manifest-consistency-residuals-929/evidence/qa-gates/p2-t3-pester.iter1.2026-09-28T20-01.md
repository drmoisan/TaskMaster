# P2-T3 — PowerShell QC step 3, test, with coverage read from CI (iteration 1)

Timestamp: 2026-09-30T10-45
Command: pwsh -NoProfile -Command '[DateTime]::UtcNow.ToString("o")' (RUN-START); MCP mcp__drm-copilot__run_poshqc_test (workspace_root <execution-worktree-root>, scan_folders ["tests/scripts/dependencies"]); CMD-JUNIT-READ; CMD-CI-LIST; CMD-CI-DISPATCH; CMD-CI-LIST (re-run); CMD-CI-WATCH with RUN-ID 36722780748; CMD-CI-PESTER with RUN-ID 36722780748 and DIR coverage/ci-branch-pester-36722780748-1
EXIT_CODE: 0
Output Summary:

Local MCP test run:
- RUN-START: 2026-09-30T13:34:53.3059426Z
- MCP payload (host prefix replaced): {"ok":true,"tool":"run_poshqc_test","workspace_root":"<execution-worktree-root>","summary":"Ran bundled PoshQC test against '<execution-worktree-root>' with 1 selected scan folder(s)."}
- JUNIT-WRITTEN=2026-09-30T13:35:27.6912381Z (later than RUN-START)
- JUNIT-ROOT tests=137 failures=0 errors=0 disabled=0; no JUNIT-NOTPASSED line
- JUNIT-SUITE RepositoryTreeConsistency.Tests.ps1 tests=4 failures=0 skipped=0
- JUNIT-SUITE ConsistencyVerifier.Tests.ps1 tests=14 failures=0 skipped=0
- JUNIT-SUITE DependabotConfig.Tests.ps1 tests=17 failures=0 skipped=0
- Other suites: AnalyzerItemRepair 13, PackageCompatibility 8, PackageGraph 32, ProjectConsistency 18, Repair-PackageManifestConsistency 31, all failures=0

Push check: git rev-parse HEAD = b96926588d562f994430e7ba7301de5de86f206c equals PUSHED-HEAD from P1-T14, so no push was made in this task.

CI locate, dispatch and watch:
- CMD-CI-LIST (first): HEAD=b96926588d562f994430e7ba7301de5de86f206c; REMOTE=b96926588d562f994430e7ba7301de5de86f206c refs/heads/bug/package-manifest-consistency-residuals-929; RUNS-FOR-HEAD=0 (no pull request run exists for this head)
- CMD-CI-DISPATCH (once): printed the run URL for run 36722780748; DISPATCH-EXIT=0
- CMD-CI-LIST (re-run 1): HEAD and REMOTE as above; RUNS-FOR-HEAD=1; RUN id=36722780748 head=b96926588d562f994430e7ba7301de5de86f206c event=workflow_dispatch status=queued conclusion=
- Selected run: 36722780748 (greatest databaseId)
- CMD-CI-WATCH invocation 1: returned on its own after the run completed; WATCH-EXIT=0

CMD-CI-PESTER (run 36722780748, head b96926588d562f994430e7ba7301de5de86f206c):
- RUN id=36722780748 head=b96926588d562f994430e7ba7301de5de86f206c branch=bug/package-manifest-consistency-residuals-929 event=workflow_dispatch status=completed conclusion=failure workflow=CI
- PESTER-JOBS=1
- JOB id=109911885533 name=pester / Run Pester suite with coverage status=completed conclusion=success
- LOG-LINES=1097
- Log lines (verbatim after SGR stripping):
  - "Tests Passed: 379, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0"
  - "PESTER Passed=379 Failed=0 Skipped=0 Total=379"
  - "COVERAGE LinePercent=94.51 Covered=1721 Total=1821"
- Total 379 equals BASELINE-TOTAL 373 plus 6 (four tree tests and two Import-kind tests); Failed 0; Skipped 0
- DIR-PREEXISTS=False; DOWNLOAD-EXIT=0; ARTIFACT-FILES=1; DIR used: coverage/ci-branch-pester-36722780748-1
- REPORT-LINE covered=1721 missed=100; computed percent 1721 / 1821 * 100 = 94.51, equal to the log's LinePercent 94.51 and at least 80
- MEETS-85: true (observation only, convention 10)
- SOURCEFILE lines (run 36722780748, head b96926588):
  - AnalyzerItemRepair.psm1 covered=106 missed=0
  - ConsistencyVerifier.psm1 covered=158 missed=2
  - PackageCompatibility.psm1 covered=33 missed=0
  - PackageGraph.psm1 covered=164 missed=0
  - ProjectConsistency.psm1 covered=103 missed=0
  - Repair-PackageManifestConsistency.ps1 covered=213 missed=14
- ConsistencyVerifier.psm1 covered 158 is at least VERIFIER-COVERED 158 and missed 2 is at most VERIFIER-MISSED 2 (P0-T16, run 36666302259).

Run-level conclusion (recorded as measured, reported to the caller, not a failure of this task): failure. Every job:
- build-analyzers / Build with analyzers and code style enforcement: success
- mstest-coverage / Run MSTest suite with coverage: failure — "Total tests: 7346 / Passed: 7345 / Failed: 1"; the failed test is Transaction_SecondCallerCannotInstallUntilTheFirstRestores (QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs), message "Expected observedByB to refer to <null> because the first transaction restores before it releases the gate, so the waiter cannot observe the pre-restore value, but found System.Windows.Threading.Dispatcher ...". This change edits no C# source (the only QuickFiler.Test change removes two Exists()-guarded Import elements whose package was never restored), and the same test passed in the local P0-T12 run (7346 of 7346).
- hygiene / Repository hygiene guard: success
- pester / Run Pester suite with coverage: success
- actionlint / actionlint: success
- format-check / Verify formatting: success
- build-nullable / Build with nullable warnings treated as errors: success

Pester emits no branch counter, so no PowerShell branch figure is claimed. These figures stand in for a permitted evidence form that the committed-evidence section does not define for the Pester route. The downloaded JaCoCo document is left under the ignored coverage directory.

GATE-SUBSTITUTION: CI Pester job on the pushed head stands in for a local coverage run
