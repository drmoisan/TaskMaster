---
name: potential-to-issue-keeps-only-summary-section
description: The promotion tool maps sections by HEADING NAME against the issue template — template-matching headings survive verbatim, non-template headings are dropped, missing ones become "(not provided in potential file)"; fill every canonical heading and you get full fidelity
metadata:
  type: project
---

The potential-to-issue promotion tool does not copy the promoted document verbatim, and it does not
unconditionally reduce it to `## Summary` either. It maps **section by section, matching on the
heading name** against the issue template it is emitting:

- A heading that matches a template section is carried through **with its full content intact**.
- A heading that does *not* appear in the template is **silently dropped**.
- A template section with no matching heading in the source is emitted as the literal string
  `(not provided in potential file)`.

The scaffolded potential-bug template states this itself, in a line that is easy to skim past:
"Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them
into the GitHub bug issue template."

**Why the earlier reading was wrong.** This memory previously recorded the rule as "keeps ONLY the
Summary section", generalised from issue #584 on 2026-08-22. That document carried `## Root Cause`,
`## Impact`, `## Proposed Direction` and `## Verification Notes` — **none** of which is a bug-template
heading. Every one was dropped for that reason, and `## Summary` survived only because it happens to
be a template heading. The observation was correct; the causal explanation was not, and the wrong
explanation predicts data loss even when there will be none.

## Two success cases (canonical headings)

Verified 2026-08-27 on issue #644 (`qfc-unregister-navigation-count-mismatch-orphan`, the AC-472-10
follow-up promoted out of epic child 444): the source was scaffolded by the potential-bug entry tool,
its scaffolded headings were left unchanged, and **all ten sections landed in the issue body with
full content** — `## Summary`, `## Environment`, `## Steps to Reproduce`, `## Expected Behavior`,
`## Actual Behavior`, `## Logs / Screenshots`, `## Impact / Severity`, `## Suspected Cause / Notes`,
`## Proposed Fix / Validation Ideas`, `## Next Step`. No follow-up comment was needed.

Verified again 2026-08-28: promoting seven follow-ups from epic child #464 produced issues #662–#668
with **zero** `(not provided in potential file)` placeholders and bodies of 1776–2320 bytes each. The
difference was purely authorial — every section the template declares was filled with real content
before promotion.

Note the feature template ships **without** a `## Summary` heading (it leads with `## Problem / Why`).
Adding an explicit `## Summary` section to a feature-type potential file is safe and worthwhile.

**How to apply.**

1. Scaffold with the potential-bug / potential-entry tool and **fill the template headings in place**.
   Do not invent your own section names — that, not the tool, is what loses content.
2. When analysis has no template home, fold it into the nearest template section (root cause fits
   `## Suspected Cause / Notes`; remedy fits `## Proposed Fix / Validation Ideas`) rather than adding
   a new heading.
3. Still verify after every call, in either direction — confirm, don't assume:
   `gh issue view <N> --json body -q '.body' | grep -c 'not provided in potential file'` must be `0`.
4. If content was genuinely dropped, post the missing sections as an issue COMMENT
   (`gh issue comment <N> --body-file <file>`): durable, needs no repository write, does not disturb
   an audited diff.

**Why this matters more than it looks.** The promotion lifecycle exists so an out-of-scope defect
survives archival of the feature folder that discovered it (see
[[feedback_promote_latent_defects_to_issues]]). The promoted markdown is **not** a reliable second
copy: inside an agent worktree it is untracked and dies with the worktree unless committed, and
committing it needs a whole extra PR. Note also that a source resolved directly from
`docs/features/potential/` is **moved** into `promoted/`, not copied. Put the durable content in the
issue body itself. When a defect has a trap in it — for example a naive "fix" that would silently
relax a merged guard — write that warning INTO the issue body, because the reader who picks the issue
up will not have your feature folder.

Related: [[potential-to-issue-creates-github-issue]] (the tool opens the issue itself — never also
`gh issue create`), [[potential-to-issue-needs-absolute-path]],
[[promotion-potential-md-may-not-persist]], [[feedback_promote_latent_defects_to_issues]].
