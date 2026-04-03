---
name: pr-context-artifacts
description: 'PR context artifact locations and refresh rules. Use when generating, refreshing, or consuming pr_context summary and appendix artifacts in Codex.'
---

# PR Context Artifacts

Canonical locations and refresh rules for PR context artifacts.

## Canonical Locations

- Summary: `artifacts/pr_context.summary.txt`
- Appendix: `artifacts/pr_context.appendix.txt`

## Refresh Rule

If artifacts are missing or stale relative to the current branch state:

1. Use `repo-automation-adapter` to attempt the repository's canonical collector.
2. If no direct Codex-facing collector is available and the workflow only needs diff-scoped review evidence, use a deterministic git-based fallback.
3. Record the provenance of any fallback-generated evidence in the review artifact.

## Consumer Rule

Use the summary as the primary review source and the appendix as the secondary diff anchor.
