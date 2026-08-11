---
name: 441-review-residuals-and-494-handoff
description: "#441/#478 review (epic build-ci-coverage-gate-fidelity wave 0): PASS 0 blocking; residuals NF-1 (uncovered max-hits line 220) + NF-2 (poshqc zero-coverage artifact); corrected repo line-rate 85.0317% handoff owned by #494"
metadata:
  type: project
---

Review of `bug/cobertura-coverage-arithmetic-441` (2026-08-10T23-35, base edf3d34c): PASS, 0 blocking, all 20 spec ACs independently verified. Facts later epic-sibling reviews (#457 wave 1, #494 wave 2, #512) will need:

- **Threshold margin handoff:** the corrected package-filtered line rate for the #424 sample is **62345 valid / 53013 covered = 0.850317 (85.0317%)** vs the uniform 85% floor — a 0.03 pp margin. Pre-fix inflated figure was 0.856453. #494 owns any threshold decision; #441 changed no threshold (verified empty diff on CLAUDE.md/.claude/rules/coverage.config). If #494's review sees a threshold edit, this is its provenance.
- **Coverage-arithmetic oracle now available:** post-fix `Get-CoberturaCoverageSummary` reproduces a raw dotnet-coverage document's own root attributes exactly (79957/56124/23109/13472 on the #424 baseline). Any future review can use generator parity as a cheap correctness probe.
- **Residual NF-1:** `Helpers.ps1:220` (`$existing.Hits = $hits`, the later-entry-larger-hits dedup update) has no covering test; recommended fixture rides a later change (#529/#530 work). If a later diff touches `Get-CoberturaClassLineSummary`, check whether the gap got closed.
- **Residual NF-2:** filed as a recommendation only — the `run_poshqc_test` zero-coverage capture defect (see [[poshqc-bundled-coverage-artifact-reads-zero]]) was NOT promoted to an issue by the reviewer (no promotion tools in-session); the orchestrator was asked to file it or fold into #512. Verify it did not evaporate.
- Follow-ups #529 (package rates stale), #530 (merged class keeps only primary `<methods>`), #531 (discovery lacks `\.claude\` exclusion — same defect as the local-vstest worktree pollution memory), #532 (wrong agent-memory generalization) are OPEN and deliberately unfixed in #441.

**How to apply:** when reviewing #457/#494/#512, read `<441-FEATURE>/evidence/other/threshold-handoff-494.2026-08-10T23-15.md` before adjudicating any threshold or coverage-figure claim, and expect coverage baselines recaptured after b52874d6 to be non-comparable with pre-fix history (denominators shrank).
