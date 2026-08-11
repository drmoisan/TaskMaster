---
name: project_494_threshold_reconciliation_plan_seams
description: "#494 revision-pass seams: coverage runner throws before Koverage post-processing (out-of-band ConvertTo-KoverageCoberturaXml per run), reported-and-tracked floor must not become hook-Blocking, dangling-citation dispositions are valid, D8 wants committed producer not artifact"
metadata:
  type: project
---

Plan seams from the #494 coverage-threshold-policy-reconciliation revision pass (2026-08-11), epic
`build-ci-coverage-gate-fidelity` wave 2. Twelve blocking findings, all instances of "a gate that cannot
fail or cannot pass".

- **`Invoke-MSTestWithCoverage.ps1` throws at line 236 on non-zero vstest exit, BEFORE the
  `ConvertTo-KoverageCoberturaXml` post-processing at 326-342.** With #511's two pre-existing
  `*ThroughThePumpHost*` failures, every run exits non-zero, so the on-disk Cobertura is the RAW artifact —
  it has root `line-rate`/`lines-valid` attributes (satisfying a naive acceptance) but pre-#441/#478/#457
  arithmetic. Remeasurement tasks must apply the post-processing out of band per run
  (`. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; ConvertTo-KoverageCoberturaXml -XmlContent
  ... -RepoRoot ...`), read root attributes AFTER it, name every failing test with a per-name #511
  determination (a third name halts), and treat non-zero exit as expected. The runner hard-codes
  `/TestCaseFilter:TestCategory!=LiveOutlook` at line 76 and exposes no filter parameter.
  **Why:** the poisoned figure trips no acceptance criterion — the classic silently-vacuous gate.
  **How to apply:** any plan measuring C# coverage on a branch with known failing tests.
- **Do not make the repository-wide floor Blocking in the hook.** Spec D5 / Appendix A5 semantics: below-floor
  with artifact present fails ONLY when no coverage row in the policy audit carries a FAIL verdict; only the
  artifact-absent/malformed path is unconditionally fail-closed. Wording the below-floor clause as itself
  blocking contradicts the authority text this feature installs (found as B5). Hook tests need all four cases:
  absent artifact, branch-below-line-above, below-floor-with-FAIL-verdict (passing), below-floor-without (failing).
- **A knowingly-dangling citation with a recorded disposition is a valid outcome.** `.claude/rules/powershell.md:63`
  cites quality-tiers 85% content this feature deletes, but that file is out of edit scope (deferred FU-A). The
  acceptance must scope "no hit points at deleted content" to the authorized edit path list and record the
  dangling citation as `deferred to FU-A` resolved interim by the authority conflict-resolution rule.
- **`spec.md` D8 item 1 requires a committed *producer*, not a committed *artifact*** — both post-change
  coverage artifact paths (`coverage/*`, `artifacts/*`) are gitignored. `.NOTES` producer inventories must
  cover every path the hook reads post-edit (four, incl. TypeScript/Python `NO PRODUCER` rows that fail closed
  by design).
- **Dot-source guard polarity:** the repo pattern is `if ($MyInvocation.InvocationName -ne '.')`
  (`Invoke-MSTestWithCoverage.ps1:346`) — `-eq` inverts it and breaks every `BeforeAll` dot-source.
- **A supplied delta can itself be arithmetically wrong:** the A4 carve-out said "nineteen [x] boxes plus
  AC10 [ ] in both files" — 20 total minus 2 unchecked is eighteen. Applied as eighteen and reported the
  deviation. Check delta arithmetic before transcribing.
- CLAUDE.md spans in this worktree: § UT2 298-313 (Scenario Completeness 314), C#1 181-217, CUT3 383-395,
  C# Toolchain 403-411 (trailing sentence 410, Key Skills Reference 412). feature-review.md:126-128 holds the
  literal prose forms ("If repo-wide coverage is below 80%...", "if line coverage is below 90%...",
  "or is below 80%").
