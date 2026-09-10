---
name: project-815-coverage-aggregation-exposure-plan-seams
description: Planning seams for issue #815 (expose a de-duplicated first-party Cobertura aggregation in scripts/vscode) - vacuous zero-hit AC, a hard-coded allowlist that zeroes the differential helper, a test tree tighter than the production tree, and an uncoverable entry-point line.
metadata:
  type: project
---

Planning seams found while authoring the atomic plan for issue #815, child F815 of epic
`review-residuals-2026-09-08`. The defect site was **plan prose, not code**: the committed helpers
already de-duplicate, and the fix is to *expose* a callable aggregation.

**Why:** each item below cost a re-derivation against the tree and would otherwise have shipped as an
unfalsifiable gate or an unsatisfiable one.

**How to apply:** when planning any PowerShell coverage-aggregation work under `scripts/vscode/`.

- **The AC3 zero-hit search was already zero before the change.** `.//line` has zero occurrences under
  both `scripts/` and `tests/`, so a bare "returns zero matches over `scripts/vscode/`" assertion
  passes vacuously whatever the executor does, and also passes if the search itself is broken. The fix
  is a controlled comparison in one command: search both folders at once, require the single hit in
  the AC6 differential helper under `tests/` as a positive control, then assert no line names a path
  under `scripts/`. Add a case-sensitivity control (`.//LINE` prints nothing; the `-i` variant prints
  the same single line). Use `git grep -F`, never `Select-String`: the leading `.` is a regex
  metacharacter and PowerShell quoting needs `\x5C` and `\x22` to survive. See
  [[zero-hit-grep-gates-need-carveouts]].
- **The pinned snippet's hard-coded allowlist zeroes any small fixture.** The descendant-axis snippet
  hard-codes nine production assembly names. A test-scoped helper that reproduces it *verbatim*
  returns all-zero counts over a fixture whose package is `Ns`, so the differential assertion would
  compare against zeros and demonstrate nothing. The helper must parameterise the allowlist and state
  that as its one deliberate deviation. Parameterising also keeps the nine names out of the delivered
  tree, which the "allowlist is derived, not hard-coded" gate depends on.
- **The test tree is tighter than the production tree.** `Invoke-MSTest.RunSettings.Tests.ps1` 496,
  `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` 494, `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`
  486. The spec listed production sizes only. Adding tests to Helpers.Tests.ps1 *reaches* 500 on the
  sixth added line and *breaches* it on the seventh (`.claude/rules/general-code-change.md:49` says
  "may exceed **500 lines**", so 500 is legal and 501 is not; round 3 corrected an off-by-one here that
  contradicted the plan's own "at or below 500" acceptance clause). Seven lines of headroom still
  cannot hold the fixture plus seven tests, so a new test file remains the only placement satisfying
  the file-placement AC. Production side: Helpers.ps1 469 (31 headroom),
  and `PackageRate.ps1:20-23` plus `Threshold.ps1:14-17` already record the split-into-a-sibling-file
  precedent in code.
- **A uniform-duplication fixture cannot discriminate, and neither can the research's fixture.** The
  double count leaves ratios invariant, so assert counts and never rates. The fixture additionally
  needs a row present **only** in the class-level view and a row present **only** in the method-level
  view; the research document's proposed fixture duplicated every row in both views and satisfied
  neither clause.
- **`Invoke-MSTestWithCoverageMain` returns at the `-NoExecute` guard, so nothing after it is
  coverable.** Every line added to the post-processing block is uncovered by construction. Confine the
  wiring to exactly one line by putting the parse, aggregate and format composition in a unit-tested
  pure function, then gate it: the `missed` LINE counter for that file may rise by at most 1 against
  the baseline. Do not lower a threshold to absorb it; record it as a finding.
- **`artifacts/pester/powershell-coverage.xml` did not exist in the assigned agent worktree.** The
  orchestrator's measurement of it (LINE missed 6403, covered 0, nine `.claude`/`.codex` packages)
  came from a different worktree. Plan the "declare it non-probative" task with an explicit
  file-exists branch plus a `SearchScope`/`SearchPatterns`/`SearchResult` negative-evidence record, or
  the task is unsatisfiable. See [[verify-citations-in-the-assigned-worktree]].
- **Research line counts drift by one.** The research artifact reported
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` at 351; the tree is 350, and the spec's 350 was
  right. Re-derive every count rather than inheriting it.
- **PoshQC analyze over `scripts/vscode` exits 1 on a 16-finding inherited baseline.** No gate may
  demand zero there. Over `tests/scripts/vscode` it returns ok, and a zero demand IS satisfiable, so
  put the zero-finding gate on the test folder where the new file lands. The tool returns an ok flag
  and a `PSScriptAnalyzer reported N issue(s)` sentence; a zero-finding run is not guaranteed to print
  that sentence, so assert the ok flag, not the absent sentence.
- **`Invoke-Pester` exits 0 on failing `It` blocks.** The `[expect-fail]` artifact for the
  test-authored-before-the-fix task records `EXIT_CODE: 0` and asserts `FailedCount` from `-PassThru`.
  Writing `ExpectedExitCode: 1` there is unsatisfiable. See [[pester-invoke-does-not-exit-nonzero]].
- **The committed Cobertura corroboration document is 671136 lines, not ~198k.** Round 3 corrected a
  figure that was wrong by a factor of about 3.4 in
  `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml`.
  A `pwsh` line count is refused in an isolated agent worktree; count it with a Grep over pattern `^`
  in `count` mode instead. The figure gated nothing, but a plan committed as audit evidence may not
  carry an unmeasured number.
- **"No absolute host paths" is a convention, not a codified rule.** It lives only in
  `.claude/agent-memory/_shared_no_absolute_host_paths.md`; nothing under `.claude/rules/`,
  `.claude/skills/` or `.claude/hooks/` prohibits it. Keep the requirement, but justify it as a
  convention the plan adopts (an absolute path records the operator's account and machine names)
  rather than citing a repository rule that does not exist — `.claude/rules/tonality.md` requires the
  wording to match the evidence. See [[../_shared_no_absolute_host_paths]].
- **A differential assertion inside a passing test is not an `[expect-fail]` task.** Supply the real
  fail-before separately by authoring the whole test file in its own phase before any production code
  exists, and schedule no whole-folder test gate in the window between that red run and the
  implementation.
