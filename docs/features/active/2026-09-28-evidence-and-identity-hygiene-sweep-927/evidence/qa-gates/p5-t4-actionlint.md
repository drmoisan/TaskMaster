# P5-T4 Local actionlint over the workflow files

Timestamp: 2026-09-29T20-09
Command: pwsh -NoProfile -Command '& ./scripts/dev-tools/run-actionlint.ps1 2>&1 | Tee-Object -FilePath coverage/logs/927-actionlint.log; "ACTIONLINT-EXIT=" + $LASTEXITCODE; exit $LASTEXITCODE'
EXIT_CODE: 0
Output Summary:
- ACTIONLINT-EXIT=0
- Log line count: 0. actionlint printed nothing, which is its success case; because the pipeline was empty, Tee-Object did not create coverage/logs/927-actionlint.log.
- The wrapper invokes the tracked actionlint binary (actionlint-bin/actionlint.exe) with no arguments. actionlint then lints every workflow file under the repository's .github/workflows directory, including the new _hygiene.yml and the edited ci.yml and _pester.yml.
- One iteration; no finding was reported, so no fix was required.

Phase 5 probe results recorded for AC15 (P5-T1 to P5-T3):
- P5-T1 _hygiene.yml probe: LINES=24; KEY| workflow_call: | 1; KEY| workflow_dispatch: | 1; KEY| contents: read | 1; KEY| runs-on: ubuntu-latest | 1; KEY| shell: pwsh | 1; KEY| run: ./scripts/hygiene/Test-RepositoryHygiene.ps1 | 1; KEY| name: Repository hygiene guard | 1; KEY| uses: actions/checkout@v4 | 1; CONCURRENCY=0; NEEDS=0; LASTEXIT=0; PATTERN=0.
- P5-T2 ci.yml probe: git diff --numstat HEAD printed 3 added, 0 removed; the hygiene uses line 1; needs: 0; steps: 0; callee uses lines 7.
- P5-T3 README probe: ROW=1, CONTEXT-LINE=1, PREDICTED=1, PESTER-ROW=1, SIX=0, SEVEN=3; the zero-context diff hunks start at old lines 3, 17, 23, 174, 182 and 192, none between the "Pinned tool versions" line (65) and the "Dependabot repair workflow" line (82).
