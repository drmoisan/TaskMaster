---
name: pr-context-artifacts
description: 'PR context artifact locations and refresh rules. Use when generating, reading, or inlining pr_context summary/appendix artifacts.'
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

If the artifacts are missing or stale relative to the current branch state, re-generate them using the repo’s PR context collector.

- When `PRBaseBranch` is supplied explicitly, use that exact branch for refresh.
- When `PRBaseBranch` is missing or ambiguous, resolve it first with `pr-base-branch-merge-base` before running the collector.
- Do not infer the refresh base from the repository default branch unless merge-base resolution fails for all candidates.
- Treat an already-fresh artifact pair as authoritative; do not refresh solely because no explicit `PRBaseBranch` input was provided.

### Freshness Cross-Check

Both artifacts open with a `Context generated` section carrying the generation timestamp and a
`Head SHA:` line. Decide freshness from those two values in two steps, and from nothing else.

1. **Pair identity.** The generated-context timestamp must be byte-identical in the summary and in
   the appendix. A mismatch proves the two files came from different invocations — a summary
   refreshed while a stale appendix persists, or the reverse — so the pair does not describe one
   run and must be regenerated.
2. **Head binding.** The head SHA recorded in both files must equal the current head of the branch
   under review. A mismatch proves the pair predates the current head, so it describes a different
   diff than the one being reviewed and must be regenerated.

File existence and file modification time are not freshness signals. A file left at the expected
path by a previous invocation satisfies an existence check, and a stale file that was copied or
touched satisfies a modification-time check. Both operands of the cross-check above are read from
the artifacts themselves and from git, so the check is deterministic and does not depend on a wall
clock.

When the head SHA renders the `(unknown)` token, the collected context carried no head SHA. Head
binding cannot be established in that case, so treat the pair as unverified and regenerate it.

