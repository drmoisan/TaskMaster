# Phase 3 QC Step 8 — Canonical Coverage Gate Artifact Regenerated (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P3-T8]
Command: `pwsh -NoProfile -File <SCRATCH>\ConvertCoberturaToJacoco.ps1 -Source coverage\remediation-final.cobertura.xml -Destination artifacts\csharp\coverage.xml`
EXIT_CODE: 0

## What this artifact is

`artifacts\csharp\coverage.xml` is the canonical **gate** artifact read by `.claude\hooks\validate-feature-review-coverage.ps1`, which expects JaCoCo format. It is regenerated from the P3-T6 coverage run rather than committed: `artifacts/` is gitignored at `.gitignore:57`, confirmed by `git check-ignore -v` reporting `.gitignore:57:artifacts/`. It is local-only and is the single non-evidence exception to the evidence-path rules in plan section 2.1.

## Output Summary

The generated file is 1,534 bytes and carries the same nine first-party packages as the committed feature summary.

| Counter | Covered | Missed | Total | Percentage the hook derives |
|---|---|---|---|---|
| LINE | 95478 | 15729 | 111207 | **85.8561%** |
| BRANCH | 22137 | 5789 | 27926 | **79.2702%** |

### Threshold check

| Threshold | Required | Measured | Verdict |
|---|---|---|---|
| Repo-wide LINE (`.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`) | >= 85 | **85.8561** | **PASS** |
| Repo-wide BRANCH (same sources) | >= 75 | **79.2702** | **PASS** |
| Repo-wide line coverage (CLAUDE.md § UT2, `.claude/rules/csharp.md`) | >= 80 | **85.8561** | **PASS** |

Both derived percentages clear every stated floor. The documented conflict between the CLAUDE.md thresholds (80 percent repo-wide, 90 percent new code) and the `.claude/rules/general-unit-test.md` thresholds (85 percent line, 75 percent branch) is recorded in P3-T9 rather than silently resolved here; the measured figures satisfy both sets, so the conflict is not load-bearing for this cycle's verdict.

Binary outcome satisfied: `artifacts\csharp\coverage.xml` exists in JaCoCo format, its derived repo-wide LINE percentage is at or above 85 (85.8561) and BRANCH at or above 75 (79.2702).
