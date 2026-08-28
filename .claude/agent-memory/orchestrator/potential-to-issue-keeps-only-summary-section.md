---
name: potential-to-issue-keeps-only-summary-section
description: potential_to_issue maps the TEMPLATE'S OWN headings into the issue body and stubs any it cannot find with "(not provided in potential file)" — custom headings are dropped, but filling every canonical heading yields full fidelity
metadata:
  type: project
---

`mcp__drm-copilot__potential_to_issue` does NOT copy the promoted document verbatim. It maps the
**bug/feature template's own section headings** into the issue body and fills any heading it cannot
find with the literal string `(not provided in potential file)`. Content under a heading the template
does not know about is **silently dropped**.

## The failure case (custom headings)

Verified 2026-08-22 on issue #584: the promoted document was ~90 lines carrying `## Root Cause`,
`## Impact`, `## Proposed Direction`, and `## Verification Notes` — **none of which are template
headings**. The resulting issue body was 33 lines and contained exactly one of five content markers.
Root-cause analysis, the counter-example citation, the proposed remedy, and every `file:line`
verification pointer were all dropped.

## The success case (canonical headings) — verified 2026-08-28

Promoting seven follow-ups from epic child #464 produced issues #662–#668 with **zero**
`(not provided in potential file)` placeholders and bodies of 1776–2320 bytes each. The difference
was purely authorial: every section the template declares was filled with real content before
promotion. For the bug template that is `## Summary`, `## Environment`, `## Steps to Reproduce`,
`## Expected Behavior`, `## Actual Behavior`, `## Logs / Screenshots`, `## Impact / Severity`.

The bug template even says so in its own body: *"Keep the section headings below unchanged; the
promotion tooling maps each of them into the GitHub bug issue template."* Take that literally.

Note the feature template ships **without** a `## Summary` heading (it leads with `## Problem / Why`).
Adding an explicit `## Summary` section to a feature-type potential file is safe and worthwhile.

**How to apply.**

1. Before promoting, open the generated template and fill **every** heading it declares. Do not invent
   headings for load-bearing analysis — fold that analysis into the nearest canonical section, or add
   a trailing `## Provenance` section and accept it may be dropped.
2. After every `potential_to_issue` call, verify rather than assume:
   `gh issue view <N> --json body -q '.body' | grep -c 'not provided in potential file'` must be `0`.
3. If content was dropped, post the missing sections as an issue COMMENT
   (`gh issue comment <N> --body-file <file>`). A comment is durable, needs no repository write, and
   does not disturb an audited diff.

**Why this matters more than it looks.** The promotion lifecycle exists so an out-of-scope defect
survives archival of the feature folder that discovered it (see
[[feedback_promote_latent_defects_to_issues]]). The promoted markdown is **not** a reliable second
copy: inside an agent worktree it is untracked and dies with the worktree, and committing it needs a
whole extra PR. Put the durable content in the issue body itself. When a defect has a trap in it —
for example a naive "fix" that would silently relax a merged guard — write that warning INTO the
issue body, because the reader who picks the issue up will not have your feature folder.

Related: [[potential-to-issue-creates-github-issue]] (the tool opens the issue itself — never also
`gh issue create`), [[potential-to-issue-needs-absolute-path]],
[[promotion-potential-md-may-not-persist]].
