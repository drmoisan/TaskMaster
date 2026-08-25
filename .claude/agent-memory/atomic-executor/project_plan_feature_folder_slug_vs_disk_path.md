---
name: plan-feature-folder-slug-vs-disk-path
description: Preflight must diff the plan's feature-folder path against the folder it actually lives in; a slug taken from the issue title silently redirects every evidence path and AC check-off.
metadata:
  type: project
---

Verify at preflight that the feature-folder path written throughout a plan resolves on disk, using
`ls` on the path and an occurrence count, not by eye.

**Why:** the 2026-08-24 quickfiler-bug-family-446 plan carried 45 occurrences of
`docs/features/active/quickfiler-queue-datamodel-defects-446/` (the slug from the `issue.md` H1
title) while living in `docs/features/active/quickfiler-bug-family-446/`. The MCP plan validator
passed it — schema validation does not resolve paths. Executing it would have created a second
parallel feature folder, written every evidence artifact and TRX `/ResultsDirectory:` there, and
checked AC boxes in a `spec.md` that does not exist at that path.

**How to apply:** first two checks of any plan preflight — `ls <feature-folder>`, then
`grep -c "<slug-used-in-plan>" <plan>` versus `grep -c "<actual-folder-name>" <plan>`. A zero on the
second is the tell. Feature slugs legitimately differ from folder names in prose, titles and commit
scopes; only PATHS must match. See also
[[project_preflight_selfderived_gate_thresholds_are_blind]] for the related class of gates that pass
without measuring anything.
