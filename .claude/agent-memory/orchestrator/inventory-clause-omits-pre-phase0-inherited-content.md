---
name: inventory-clause-omits-pre-phase0-inherited-content
description: A changed-file inventory gate anchored on a base ref admits only delivery paths, so promotion-era content inherited below the Phase 0 commit reads as a violation
metadata:
  type: feedback
---

A changed-file inventory acceptance clause that enumerates admitting conditions for each path
("named in the plan, or under the feature folder, or under agent-memory") will flag content
the BRANCH inherited before Phase 0 began, because that content is in the diff against the
base anchor but is not the delivery's footprint.

**Why:** On #873 P7-T9 the union carried the promotion rename
`docs/features/potential/... -> docs/features/potential/promoted/...`, which met none of the
three conditions. It was authored by the preparation commit BELOW the Phase 0 commit, so it
predates every plan task. The clause has no fourth condition for pre-Phase-0 inherited
content, so a literal reading turns ordinary promotion output into a footprint violation.

**How to apply:** When authoring or preflighting an inventory gate, add an explicit admitting
condition for paths whose authoring commit is an ancestor of the Phase 0 commit. When one
surfaces during execution, resolve it the way #873 did: establish provenance with
`git log --diff-filter=... -- <path>`, confirm the authoring commit is below the Phase 0
commit, and record it as a CLASSIFIED EXCEPTION carrying that provenance. Do not silently drop
it from the union and do not treat it as a real violation — both readings destroy the audit
trail. Related: [[stale-base-anchor-passes-ancestry-vacuously]],
[[absence-from-failure-list-is-not-a-pass-gate]].
