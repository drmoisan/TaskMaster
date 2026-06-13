---
name: evidence-path-normalization
description: How to handle spec/caller-supplied non-canonical coverage evidence paths in TaskMaster plans
metadata:
  type: feedback
---

When a TaskMaster spec or delegation prompt names a coverage evidence sub-path such as
`<FEATURE>/evidence/coverage/`, do not use it verbatim. The canonical scheme
(`evidence-and-timestamp-conventions` skill) only defines `evidence/baseline/`,
`evidence/regression-testing/`, `evidence/qa-gates/`, `evidence/issue-updates/`,
`evidence/other/`, `evidence/remediation-baseline/`. The `enforce-evidence-locations.ps1`
PreToolUse hook rejects non-canonical paths.

**Why:** baseline coverage and post-change coverage are distinct evidence kinds; the canonical
scheme has no `coverage/` sub-path. Mapping baseline coverage -> `evidence/baseline/` and
post-change/QA coverage -> `evidence/qa-gates/` keeps artifacts in hook-approved locations.

**How to apply:** in the plan, record an `EVIDENCE_LOCATION_OVERRIDE_REJECTED:` line noting the
supplied path and the canonical substitution, and route every coverage artifact task to a
canonical sub-path. Used in the issue #197 com-vsto-coverage-exemption plan.
