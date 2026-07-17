# Diagnostic TRX Whitespace Baseline

Timestamp: 2026-07-16T16-18

Command: `pwsh -NoProfile -Command '& { $p="docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx"; [xml]$x=Get-Content -Raw $p; $sha=(Get-FileHash -Algorithm SHA256 $p).Hash.ToLowerInvariant(); $c=$x.SelectSingleNode("//*[local-name()=''Counters'']"); $o=@(git diff --check bump-release...HEAD -- $p 2>&1); $e=$LASTEXITCODE; $findings=@($o | Where-Object { $_ -match ("^"+[regex]::Escape($p)+":(981|3844|3904|4014|4029|5897): trailing whitespace\.$") }); "XML_ROOT=$($x.DocumentElement.LocalName)"; "TRX_SHA256=$sha"; "TOTAL=$($c.GetAttribute(''total''))"; "EXECUTED=$($c.GetAttribute(''executed''))"; "PASSED=$($c.GetAttribute(''passed''))"; "FAILED=$($c.GetAttribute(''failed''))"; "DIFF_CHECK_EXIT_CODE=$e"; "WHITESPACE_FINDING_COUNT=$($findings.Count)"; $findings; if($x.DocumentElement.LocalName -ne "TestRun" -or $e -ne 2 -or $findings.Count -ne 6){exit 1} }'`

EXIT_CODE: 0

Output Summary:

XML_ROOT=TestRun
TRX_SHA256=5b3c91280351a610b84e3f99489f6113fd8c80b978d8bcedd9b1be6b1f052d17
TOTAL=4815
EXECUTED=4815
PASSED=4815
FAILED=0
DIFF_CHECK_EXIT_CODE=2
WHITESPACE_FINDING_COUNT=6
docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx:981: trailing whitespace.
docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx:3844: trailing whitespace.
docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx:3904: trailing whitespace.
docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx:4014: trailing whitespace.
docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx:4029: trailing whitespace.
docs/features/active/2026-07-16-progress-viewer-cancel-button-339/evidence/baseline/coverage-timeout-pair.2026-07-16T14-37.trx:5897: trailing whitespace.
