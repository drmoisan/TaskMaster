---
name: github-issue-search-without-gh
description: "When the Bash tool (and therefore gh) is unavailable, search this repo's open GitHub issues with WebFetch against github.com/drmoisan/TaskMaster/issues?q=... — it works and returns numbers plus titles"
metadata:
  type: reference
---

Several task-researcher sessions in this repo run **without a Bash tool**, so `gh issue list --state open
--search ...` cannot be run even though delegation prompts routinely require an open-issue keyword scan.

**WebFetch against the public GitHub issue pages works as a substitute** (verified 2026-08-07):

- List/search: `https://github.com/drmoisan/TaskMaster/issues?q=is%3Aissue+is%3Aopen+<terms>` — returns
  issue numbers and titles. Multi-term OR works (`focus+OR+viewer+OR+%22folder+search%22`).
- Single issue: `https://github.com/drmoisan/TaskMaster/issues/<N>` — returns title, state, labels, and
  body.

**Why:** the epic-#136 delegation prompts require searching open issues by keyword because a
promoted-but-not-yet-active issue is invisible to a `docs/features/active/` scan (this is how #426 was
missed at decomposition time). Two sibling researchers on the same feature recorded "could not verify —
Bash disabled" and left the scan incomplete; that was avoidable.

**How to apply:** when a delegation prompt asks for a `gh issue list` scan and no Bash tool is present,
reach for WebFetch instead of declaring the scan unverifiable. Mark the findings `[V-web]` to distinguish
them from local-file evidence. Note the results are only what GitHub renders on the first page, so run
several narrow term sets rather than one broad one.

Related: [[qfc-itemviewer-coverage-456]].
