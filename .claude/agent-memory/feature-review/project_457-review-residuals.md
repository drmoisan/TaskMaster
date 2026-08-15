---
name: 457-review-residuals
description: "#457 closure-filter review: PASS/0 blocking; CR-1 line-map rebuild drift vs merge path; AC15 potential_to_issue owed at epic close; Helpers.Tests.ps1 at 490 lines"
metadata:
  type: project
---

Review of `bug/excludefromcodecoverage-nested-lambdas-457` (2026-08-11, vs epic/build-ci-coverage-gate-fidelity-integration) closed with 16/16 AC PASS and 0 blocking findings. Residuals deliberately left open:

- **CR-1 (Minor):** `Remove-CoberturaExemptClosureCoverage`'s retained-lines rebuild (ClosureFilter.ps1:337-372) duplicates `Merge-CoberturaClassesByFilename`'s line-map loop but omits the stale `condition-coverage` removal and `<conditions>` child copy that the merge path has (Helpers.ps1:345-355). Recommended extraction into a shared helper; check whether later epic children fixed it.
- **AC15 follow-through:** the three residual potential entries (exempt-async lambdas, local functions, overload collisions; `docs/features/potential/2026-08-11-*.md`) are NOT yet GitHub issues — `potential_to_issue` is owed by the epic orchestrator at epic close. Verify at epic-merge review.
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` is at 490/500 lines — next addition breaches the ceiling.
- Post-#457 corrected repo baseline (input to #494): line 0.855355 (53375/62401), branch 0.790134 (12541/15872).

**Why:** these are the items later reviews of the coverage-pipeline epic (#494, epic merge) must re-check.
**How to apply:** when reviewing #494 or the epic integration merge, verify the promotions happened and use 0.855355/0.790134 as the inherited baseline; treat CR-1 as pre-existing if unfixed.
