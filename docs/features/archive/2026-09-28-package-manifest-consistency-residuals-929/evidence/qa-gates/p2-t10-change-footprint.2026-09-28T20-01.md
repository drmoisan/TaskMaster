# P2-T10 — Change footprint (CMD-FOOTPRINT)

Timestamp: 2026-09-30T11-09
Command: git diff --name-only 481b33c594d8412cb64e604ff53295db215ac2f1 -- . together with git status --porcelain --untracked-files=all (P0-START anchor; both run in one pwsh invocation beginning with Set-Location "<execution-worktree-root>")
EXIT_CODE: 0
Output Summary:
- Every path in the union of the two captures is a Write Set member, lies under the feature folder, or lies under the agent-memory tree.
- Paths outside the feature folder and the agent-memory tree: exactly the eight non-feature-folder Write Set paths named in P1-T14:
  - .github/workflows/README.md
  - .github/workflows/dependabot-repair.yml
  - QuickFiler.Test/QuickFiler.Test.csproj
  - SVGControl/app.config
  - docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md
  - scripts/dependencies/ConsistencyVerifier.psm1
  - tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1
  - tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1
- Paths matching *.cs: 0
- Paths under .claude/rules, .github/instructions or the config directory: 0
- Other *.csproj, packages.config or app.config paths: 0 (the count the AC7 check-off cites as proof that no analyzer item or manifest was changed on this branch)
- Paths under docs/features/potential: 0
- Agent-memory entries present (not this change's work, not staged): .claude/agent-memory/atomic-executor/MEMORY.md and .claude/agent-memory/atomic-planner/MEMORY.md (modified, tracked), plus five untracked agent-memory notes.

Anchored name-only diff against P0-START, verbatim (git also printed two "LF will be replaced by CRLF" warnings, for the atomic-executor MEMORY.md and the plan file):
```
.claude/agent-memory/atomic-executor/MEMORY.md
.claude/agent-memory/atomic-planner/MEMORY.md
.github/workflows/README.md
.github/workflows/dependabot-repair.yml
QuickFiler.Test/QuickFiler.Test.csproj
SVGControl/app.config
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t1-worktree-anchor.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t10-msbuild-analyzers.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t11-msbuild-nullable.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t12-coverage-projection.2026-09-28T20-01.jacoco.xml
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t12-mstest-coverage.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t12-test-results.2026-09-28T20-01.summary.txt
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t13-poshqc-format.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t14-poshqc-analyze.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t15-poshqc-test-mcp.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t16-pester.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t17-altcover-and-verifier-prefix.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t18-redirect-prefix.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t19-workflow-prefix-and-actionlint.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t20-hygiene.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t21-commit.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t3-sdk-bootstrap.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t4-tool-restore.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t5-package-restore.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t6-analyzer-item-census.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t7-dotnet-coverage.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t8-pester-provision.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/p0-t9-csharpier-check.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/baseline/phase0-instructions-read.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t10-verifier-comment.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t12-verifier-postfix.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t4-altcover-imports-removed.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t5-redirects-fixed.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t6-workflow-client-id.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t7-actionlint.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t8-runbook-client-id.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t9-readme-client-id.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/regression-testing/p1-t1-tree-test-authored.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/regression-testing/p1-t11-tree-test-pass-after.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/regression-testing/p1-t2-tree-test-fail-before.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/regression-testing/p1-t3-verifier-import-tests.2026-09-28T20-01.md
docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/plan.2026-09-28T20-01.md
scripts/dependencies/ConsistencyVerifier.psm1
tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1
tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1
```

Porcelain capture, verbatim:
```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/plan.2026-09-28T20-01.md
?? .claude/agent-memory/atomic-executor/index_csharp_nullable_and_component_gotchas.md
?? .claude/agent-memory/atomic-executor/index_pwsh_git_and_gate_mechanics_misc.md
?? .claude/agent-memory/atomic-executor/index_test_isolation_and_coverage.md
?? .claude/agent-memory/atomic-executor/project_hygiene_pattern_array_comma_precedence_and_regex_token_hits.md
?? .claude/agent-memory/atomic-planner/project_929_manifest_residuals_plan_seams.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t14-commit.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t1-poshqc-format.iter1.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t1-poshqc-format.iter2.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t2-poshqc-analyze.iter1.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t2-poshqc-analyze.iter2.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t3-pester.iter1.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t3-pester.iter2.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t4-csharpier-check.iter1.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t4-csharpier-check.iter2.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t5-msbuild-analyzers.iter1.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t5-msbuild-analyzers.iter2.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t6-msbuild-nullable.iter1.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t6-msbuild-nullable.iter2.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t7-coverage-projection.2026-09-28T20-01.jacoco.xml
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t7-mstest-coverage.iter1.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t7-mstest-coverage.iter2.2026-09-28T20-01.md
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t7-test-results.2026-09-28T20-01.summary.txt
?? docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t8-attestation-and-coverage-delta.2026-09-28T20-01.md
```
