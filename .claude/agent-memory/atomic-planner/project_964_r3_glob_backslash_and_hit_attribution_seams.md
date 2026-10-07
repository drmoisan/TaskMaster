---
name: project-964-r3-glob-backslash-and-hit-attribution-seams
description: #964 R3 preflight seams - Glob returns backslash paths that git rev-parse HEAD:<path> cannot resolve; a totals-only sweep cannot decide a per-file exception branch; lower bounds must count every file known to exist
metadata:
  type: project
---

Three preflight defects on #964 round 3 (2026-10-02):

1. The Glob tool returns backslash-separated (relative or absolute) paths. `git rev-parse HEAD:<path>` fails on a backslash tree path. Any plan value recorded from Glob and later fed to a git tree lookup must be specified as repository-relative forward-slash form, and every later comparison against it must state the same conversion.
2. A sweep that prints only totals cannot decide a branch like "a hit in file X stops; elsewhere repair". Add a per-file row (no host token in it, e.g. parent-dir/file-name plus counts) and key the branch on that row.
3. A lower bound that excludes a file whose presence an earlier task established is a weaker gate than necessary; preflight wants it counted.

**Why:** each was a round of preflight churn on a hygiene/clearance-record mechanism added in R2.
**How to apply:** when a task records a path for git use, say "forward-slash, repository-relative"; when a gate has a per-file exception, make the command emit per-file attribution. Related: [[project-964-r2-preparation-record-closed-evidence-set]].
