# Evidence hygiene and committed-format gate ([P2-T11])

Timestamp: 2026-09-29T09-25
Command: CMD-SANITIZE with LOGSTAGE = final and BASE = ac819907f479ee18026993054e714dc2e056142f (verbatim from the plan Command Reference)
Command: pwsh -NoProfile -Command 'foreach ($p in "docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/evidence/baseline/baseline-coverage.jacoco.xml", "docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/evidence/qa-gates/final-coverage.jacoco.xml") { [xml]$x = Get-Content -LiteralPath $p -Raw; "PARSE_OK $p PACKAGES=$(@($x.report.package).Count)" }'
EXIT_CODE: 0
Output Summary (counts only; no matched value is recorded):
- FEATURE_FILES=43 (scan scope: every file under the feature folder, including issue.md and this plan)
- ACCOUNT_HITS=0
- HOST_HITS=0
- ROOT_HITS=0
- DRIVE_USERS_HITS=0
- RAW_TOOL_DOCS=0 (no .trx, .coverage, .coveragexml, .cobertura.xml or .log under the feature folder)
- CODE_DIFF_IDENTITY_HITS=0
- Positive controls: CONTROL_ACCOUNT_HITS=15, CONTROL_ROOT_HITS=15, CONTROL_HOST_HITS=7325, CONTROL_DRIVE_HITS=15 (each at least 1; the raw final coverage log carries the absolute worktree path and the raw final trx carries the machine name, so each search can match)
- PARSE_OK EVIDENCE/baseline/baseline-coverage.jacoco.xml PACKAGES=9
- PARSE_OK EVIDENCE/qa-gates/final-coverage.jacoco.xml PACKAGES=9
- Committed tool-derived evidence is limited to the two JaCoCo projections and the two trx-derived summaries, per the CLAUDE.md Committed Test Evidence Format section.
