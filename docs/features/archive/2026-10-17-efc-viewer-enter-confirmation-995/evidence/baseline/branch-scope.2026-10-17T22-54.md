# Issue 995 branch-scope

Timestamp: 2026-10-17T22-54
Command: git merge-base main HEAD; git diff <merge-base> --name-only; git status --short; inspect change-plan and asserting tests
EXIT_CODE: 0
Output Summary: Base branch main; merge-base 438436893d2e7500057b8a4f871e23a702ab9b34; branch bug/efc-viewer-enter-confirmation-995. Exact plan plan.2026-10-17T13-44.md; full-bug sole AC spec.md eight unchecked. Lifecycle numeric995 branch readiness and valid same-worker attestation verified in parent checkpoint. Existing change-plan.md describes earlier MCP migration; current issue plan governs this work. Existing asserting test dispositions validated by source searches. Committed/staged/unstaged full inventory:
.agents/skills/codex-model-routing/SKILL.md
.agents/skills/epic-orchestrate/SKILL.md
.agents/skills/epic-plan/SKILL.md
.agents/skills/execution-environment/SKILL.md
.agents/skills/orchestrate/SKILL.md
.agents/skills/orchestrator-workflow/SKILL.md
.codex/agents/atomic-executor-c1.toml
.codex/agents/atomic-executor-c2.toml
.codex/agents/atomic-executor-c3-elevated.toml
.codex/agents/atomic-executor-c3.toml
.codex/agents/atomic-executor-c4.toml
.codex/agents/atomic-executor.toml
.codex/agents/atomic-planner-c1.toml
.codex/agents/atomic-planner-c2.toml
.codex/agents/atomic-planner-c3-elevated.toml
.codex/agents/atomic-planner-c3.toml
.codex/agents/atomic-planner-c4.toml
.codex/agents/atomic-planner.toml
.codex/agents/commit-steward-c1.toml
.codex/agents/commit-steward-c2.toml
.codex/agents/commit-steward-c3-elevated.toml
.codex/agents/commit-steward-c3.toml
.codex/agents/commit-steward-c4.toml
.codex/agents/commit-steward.toml
.codex/agents/csharp-typed-engineer-c1.toml
.codex/agents/csharp-typed-engineer-c2.toml
.codex/agents/csharp-typed-engineer-c3-elevated.toml
.codex/agents/csharp-typed-engineer-c3.toml
.codex/agents/csharp-typed-engineer-c4.toml
.codex/agents/csharp-typed-engineer.toml
.codex/agents/epic-orchestrator.toml
.codex/agents/epic-planner.toml
.codex/agents/feature-reviewer-c1.toml
.codex/agents/feature-reviewer-c2.toml
.codex/agents/feature-reviewer-c3-elevated.toml
.codex/agents/feature-reviewer-c3.toml
.codex/agents/feature-reviewer-c4.toml
.codex/agents/feature-reviewer.toml
.codex/agents/orchestrator-c1.toml
.codex/agents/orchestrator-c2.toml
.codex/agents/orchestrator-c3-elevated.toml
.codex/agents/orchestrator-c3.toml
.codex/agents/orchestrator-c4.toml
.codex/agents/orchestrator.toml
.codex/agents/powershell-typed-engineer-c1.toml
.codex/agents/powershell-typed-engineer-c2.toml
.codex/agents/powershell-typed-engineer-c3-elevated.toml
.codex/agents/powershell-typed-engineer-c3.toml
.codex/agents/powershell-typed-engineer-c4.toml
.codex/agents/powershell-typed-engineer.toml
.codex/agents/pr-author-c1.toml
.codex/agents/pr-author-c2.toml
.codex/agents/pr-author-c3-elevated.toml
.codex/agents/pr-author-c3.toml
.codex/agents/pr-author-c4.toml
.codex/agents/pr-author.toml
.codex/agents/prd-feature-c1.toml
.codex/agents/prd-feature-c2.toml
.codex/agents/prd-feature-c3-elevated.toml
.codex/agents/prd-feature-c3.toml
.codex/agents/prd-feature-c4.toml
.codex/agents/prd-feature.toml
.codex/agents/task-researcher-c1.toml
.codex/agents/task-researcher-c2.toml
.codex/agents/task-researcher-c3-elevated.toml
.codex/agents/task-researcher-c3.toml
.codex/agents/task-researcher-c4.toml
.codex/agents/task-researcher.toml
.codex/config.toml
.codex/hooks/record-subagent-routing-attestation.ps1
.codex/lib/codex-routing/CodexDeployment.psm1
config/orchestration-routing.json
docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/issue.md
docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/plan.2026-10-17T13-44.md
docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/research/2026-10-17T17-49-issue-995-efc-enter-routing-research.md
docs/features/potential/promoted/2026-10-17-efc-viewer-enter-confirmation.md
Working tree including intended untracked docs/evidence:
 M .codex/config.toml
 M docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/issue.md
 M docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/plan.2026-10-17T13-44.md
 M docs/features/potential/promoted/2026-10-17-efc-viewer-enter-confirmation.md
?? docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/evidence/
?? docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/research/2026-10-17T22-42-issue-995-efc-enter-routing-research.md
?? docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/spec.md
Planned additional tests: BreadcrumbBridgeRouterIssue995Tests.cs, EfcFormControllerIssue995Tests.cs, CodexDeployment.Tests.ps1, CodexSubagentRoutingAttestation.Tests.ps1; explicit legacy Compile updates required. Planned EFC source includes message/codec/assets/router/selection/controller breadcrumb and Cleanup owner; existing selector stays unchanged.
