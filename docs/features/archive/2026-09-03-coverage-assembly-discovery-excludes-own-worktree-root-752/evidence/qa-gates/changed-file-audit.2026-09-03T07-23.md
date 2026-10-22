# Changed-File Audit ([P4-T8])

Timestamp: 2026-09-03T12-26

Command:
1. `git -C <repo-root> merge-base origin/main HEAD`
2. `git -C <repo-root> add -A -- scripts/vscode tests/scripts/vscode docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752`
3. `git -C <repo-root> status --porcelain -uall`
4. `git -C <repo-root> diff --name-only --cached 87233f867ad60c0a5c0d19b09cc121ae536d7ba1`

EXIT_CODE: 0

MERGE BASE SHA: 87233f867ad60c0a5c0d19b09cc121ae536d7ba1

This value is character-identical to the one recorded by `[P0-T12]` in `evidence/baseline/runsettings-tests-blob-hash.2026-09-03T07-23.md` and by `[P0-T4]` in `evidence/baseline/pre-change-tree-state.2026-09-03T07-23.md`.

## Output of command 2, verbatim

Command 2 emitted 28 `warning: in the working copy of '<path>', LF will be replaced by CRLF the next time Git touches it` lines, one per newly added Markdown artifact and per modified Markdown file. These are the ordinary `* text=auto` normalization notices configured by `.gitattributes` line 4 and are not errors. The paths they name are exactly the Markdown paths listed in command 3's output below.

## Output of command 3, verbatim

```
 M artifacts/orchestration/orchestrator-state.json
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/coverage-floor-position.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.xml
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/phase0-instructions-read.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/poshqc-analyze-baseline.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/poshqc-format-drift-probe.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/poshqc-test-baseline.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pre-change-tree-state.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pssa-diagnostic-set-baseline.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/runsettings-tests-blob-hash.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/issue-updates/issue-752.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/fix-diffstat.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/predicate-line-shape.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/coverage-delta.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/final-clean-pass.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-cleanpass.2026-09-03T07-23.xml
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.xml
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/poshqc-analyze.iter1.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/poshqc-format.iter1.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/poshqc-test.iter1.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pssa-diagnostic-set.iter1.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/runsettings-tests-unmodified.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/sibling-defect-sweep.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/regression-testing/getrelativepath-probe.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/regression-testing/post-fix-new-suite.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/regression-testing/pre-fix-new-suite.2026-09-03T07-23.md
A  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/regression-testing/preserved-original-test.2026-09-03T07-23.md
M  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md
M  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/plan.2026-09-03T07-23.md
M  docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md
M  scripts/vscode/Invoke-MSTestWithCoverage.ps1
A  tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
```

## Output of command 4, verbatim

```
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/coverage-floor-position.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.xml
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/phase0-instructions-read.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/poshqc-analyze-baseline.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/poshqc-format-drift-probe.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/poshqc-test-baseline.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pre-change-tree-state.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pssa-diagnostic-set-baseline.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/runsettings-tests-blob-hash.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/issue-updates/issue-752.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/fix-diffstat.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/predicate-line-shape.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/preflight-clearance.2026-09-03T09-30.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/coverage-delta.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/final-clean-pass.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-cleanpass.2026-09-03T07-23.xml
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pester-coverage-postchange.iter1.2026-09-03T07-23.xml
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/poshqc-analyze.iter1.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/poshqc-format.iter1.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/poshqc-test.iter1.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/pssa-diagnostic-set.iter1.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/runsettings-tests-unmodified.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/sibling-defect-sweep.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/regression-testing/getrelativepath-probe.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/regression-testing/post-fix-new-suite.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/regression-testing/pre-fix-new-suite.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/regression-testing/preserved-original-test.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/plan.2026-09-03T07-23.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md
docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md
docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md
scripts/vscode/Invoke-MSTestWithCoverage.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
```

Output Summary: The `--name-only` output lists 36 paths and every one of them is an allow-list entry. One is `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; one is `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`; 33 are under `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/`; and one is `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md`. Three of the 33 — `research/research-findings.2026-09-03T00-00.md`, `evidence/other/preflight-clearance.2026-09-03T09-30.md`, and the promotion record — appear because the diff is anchored at the merge base, which predates this branch's earlier documentation-only preparation commit; no task in this plan modified any of them. No path outside the allow-list appears, so the AC6 fallback clause does not fire and the `[P4-T6]` check-off stands.

The unscoped porcelain output additionally shows one unrelated session artifact, ` M artifacts/orchestration/orchestrator-state.json`, which is unstaged and was deliberately left unstaged: it is outside this plan's Write Set and outside the three staged pathspecs.
