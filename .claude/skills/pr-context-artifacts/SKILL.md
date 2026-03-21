---
name: pr-context-artifacts
description: 'PR context artifact locations and refresh rules. Use when generating, reading, or inlining pr_context summary/appendix artifacts.'
user-invocable: false
---

# PR Context Artifacts

Canonical locations and usage rules for PR context artifacts.

## When to Use This Skill

Use this skill when:
- You generate or refresh PR context artifacts.
- You reference PR context summary/appendix as evidence.
- You inline PR context artifacts into remediation handoffs.

## Canonical Artifact Locations

- Summary: `artifacts/pr_context.summary.txt`
- Appendix: `artifacts/pr_context.appendix.txt`

## Refresh Rule

If the artifacts are missing or stale relative to the current branch state, re-generate them using the repo's PR context collector:

```bash
# Via VS Code extension (preferred)
drmCopilotExtension.collectPrContext --base <resolved-PRBaseBranch>
```

Resolve `PRBaseBranch` using the `pr-base-branch-merge-base` skill before invoking the collector.
