---
name: potential-to-issue-keeps-only-summary-section
description: potential_to_issue copies ONLY the "## Summary" section into the GitHub issue body and stubs every other bug-template section with "(not provided in potential file)" — all other analysis is silently dropped
metadata:
  type: project
---

`mcp__drm-copilot__potential_to_issue` does NOT copy the promoted document verbatim into the GitHub
issue. It extracts the `## Summary` section only, then emits the bug template with every remaining
section filled in as the literal string `(not provided in potential file)`:

```
## Environment
(not provided in potential file)

## Steps to Reproduce
(not provided in potential file)
...
## Source
From: docs/features/potential/<name>.md
```

Verified 2026-08-22 on issue #584: the promoted document was ~90 lines carrying `## Root Cause`,
`## Impact`, `## Proposed Direction`, and `## Verification Notes`; the resulting issue body was 33
lines and contained exactly one of five content markers. Root-cause analysis, the counter-example
citation, the proposed remedy, and every `file:line` verification pointer were all dropped.

**Why this matters more than it looks.** The promotion lifecycle exists so that an out-of-scope
defect survives the archival of the feature folder that discovered it (see
[[feedback_promote_latent_defects_to_issues]]). If the local document is then deleted or never
committed — which is exactly the case inside an epic child, where the plan's hard constraints forbid
writing under `docs/features/potential/**` — the analysis is lost entirely and the issue retains only
a summary paragraph. The promotion appears to have succeeded while silently discarding the reasoning
that made it worth filing.

**How to apply.** After every `potential_to_issue` call, diff what you wrote against what landed:

1. `gh issue view <N> --json body -q '.body' | wc -l` and compare against the source document.
2. Grep the issue body for two or three distinctive markers from your analysis sections.
3. If content was dropped, post the missing sections as an issue COMMENT
   (`gh issue comment <N> --body-file <file>`). A comment is durable, needs no repository write, and
   does not disturb an audited diff.

Put the load-bearing content in `## Summary` when the document is short enough, or plan on the
follow-up comment when it is not. Do not assume the promoted markdown file is a durable second copy:
inside an agent worktree it is untracked and dies with the worktree.

Related: [[potential-to-issue-creates-github-issue]] (the tool opens the issue itself — never also
`gh issue create`), [[potential-to-issue-needs-absolute-path]],
[[promotion-potential-md-may-not-persist]].
