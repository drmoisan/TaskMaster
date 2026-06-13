# Phase 2 — Minor-Audit Reconciliation (Issue #194)

Timestamp: 2026-06-13T11-34

AC source: docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/issue.md (`## Acceptance Criteria`, AC1-AC4)

## AC1 — PASS
global.json sdk.version reverted from 10.0.200 to 8.0.205; rollForward, allowPrerelease, paths unchanged.
Evidence:
- evidence/baseline/global-json-baseline.md (baseline 10.0.200)
- git diff (P1-T1): single changed line, version 10.0.200 -> 8.0.205; rollForward=latestFeature, allowPrerelease=false, paths=[".dotnet-sdk","$host$"], errorMessage unchanged.
- Current global.json sdk.version = 8.0.205.

## AC2 — PASS
Install-RepoDotNetSdk.Tests.ps1 passes, including `global.json SDK selection` assertions.
Evidence:
- evidence/regression-testing/baseline-pester-2026-06-13T09-00.md (fail-before: Failed 1, version assertion expected 8.0.205 was 10.0.200)
- evidence/qa-gates/final-qa-pester-2026-06-13T09-00.md (pass-after: Passed 2, Failed 0; MCP gate ok=true; version 8.0.205, rollForward latestFeature, allowPrerelease false, paths contains '.dotnet-sdk' and '$host$').

## AC3 — PASS
No other global.json keys or unrelated files modified (scope limited to the one-field revert).
Evidence:
- git diff -- global.json: exactly one changed line (the version value); no other key changed.
- The only production/config file modified by the implementation task is global.json. Other working-tree entries (new feature/evidence folder; a pre-existing deletion of the promoted potential-feature markdown from feature promotion) are not part of P1-T1's code/config change.

## AC4 — PASS
PowerShell toolchain (PoshQC format, PSScriptAnalyzer, Pester) passes with no new findings on changed/related files.
Evidence:
- evidence/qa-gates/final-qa-format-2026-06-13T09-00.md (format ok=true, no file changes)
- evidence/qa-gates/final-qa-analyze-2026-06-13T09-00.md (16 findings post-change = 16 baseline; delta 0; no new findings on changed/related files; regression test file has zero findings)
- evidence/qa-gates/final-qa-pester-2026-06-13T09-00.md (suite passes; MCP gate ok=true)
- Note: the pre-existing 16 analyzer findings are in unrelated scripts/vscode production scripts and are unchanged by this JSON config revert. Coverage on changed lines does not regress because no PowerShell lines changed.

## Overall: PASS (all AC1-AC4 confirmed from evidence on disk)
