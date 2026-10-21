# P0-T16 — Pester coverage baseline from CI (CMD-CI-PESTER)

Timestamp: 2026-09-30T09-47
Command: CMD-CI-PESTER with RUN-ID 36666302259 and DIR coverage/ci-main-pester-36666302259-1 (pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; gh run view 36666302259 --repo drmoisan/TaskMaster --json databaseId,headSha,headBranch,event,status,conclusion,workflowName ...; gh run view --repo drmoisan/TaskMaster --job <pester job id> --log ...; gh run download 36666302259 --repo drmoisan/TaskMaster --name pester-coverage --dir "coverage/ci-main-pester-36666302259-1"; ...'), then git merge-base --is-ancestor 231e1c0b55105aeb626bf5a6e8d0266a567cacad HEAD
EXIT_CODE: 0
Output Summary:
- RUN id=36666302259 head=231e1c0b55105aeb626bf5a6e8d0266a567cacad branch=main event=push status=completed conclusion=success workflow=CI
- git merge-base --is-ancestor 231e1c0b55105aeb626bf5a6e8d0266a567cacad HEAD exited 0 (the run's head is an ancestor of the executor's HEAD)
- PESTER-JOBS=1
- JOB id=109731601928 name=pester / Run Pester suite with coverage status=completed conclusion=success
- LOG-LINES=1085
- Log lines (verbatim after SGR stripping; run 36666302259, head 231e1c0b5):
  - "Tests Passed: 373, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0"
  - "PESTER Passed=373 Failed=0 Skipped=0 Total=373"
  - "COVERAGE LinePercent=94.51 Covered=1721 Total=1821"
- BASELINE-TOTAL: 373 (Passed 373 + Failed 0 + Skipped 0, equal to the Total field 373)
- DIR-PREEXISTS=False; DOWNLOAD-EXIT=0; ARTIFACT-FILES=1; DIR used: coverage/ci-main-pester-36666302259-1
- REPORT-LINE covered=1721 missed=100; computed percent 1721 / 1821 * 100 = 94.51, equal to the log's LinePercent 94.51 and at least 80
- MEETS-85: true (observation only, convention 10)
- SOURCEFILE lines (run 36666302259, head 231e1c0b5):
  - AnalyzerItemRepair.psm1 covered=106 missed=0
  - ConsistencyVerifier.psm1 covered=158 missed=2
  - PackageCompatibility.psm1 covered=33 missed=0
  - PackageGraph.psm1 covered=164 missed=0
  - ProjectConsistency.psm1 covered=103 missed=0
  - Repair-PackageManifestConsistency.ps1 covered=213 missed=14
- VERIFIER-COVERED: 158
- VERIFIER-MISSED: 2
- No TRANSCRIPTION-MISMATCH: the run's figures equal the coordinator's (Tests Passed 373, LinePercent 94.51, Covered 1721, Total 1821).

Pester emits no branch counter, so no PowerShell branch figure is claimed. These figures stand in for a permitted evidence form that the committed-evidence section of CLAUDE.md does not define for the Pester route. The downloaded JaCoCo document stays under the ignored coverage directory and is not committed.

GATE-SUBSTITUTION: CI Pester job 109731601928 stands in for a local coverage run
