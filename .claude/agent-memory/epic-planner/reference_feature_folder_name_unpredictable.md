---
name: feature-folder-name-unpredictable
description: new_active_feature_folder derives the folder date from the potential document, not today, and sometimes omits it entirely, so manifest feature_folder values must always be back-filled from the promotion receipt
metadata:
  type: reference
---

`mcp__drm-copilot__new_active_feature_folder` does NOT name the active folder
`<today>-<slug>-<issue>`. Measured across four children created in one batch on 2026-08-21:

- Three folders took the **potential document's capture date**, not today's:
  `2026-08-07-quickfiler-keyboard-action-contract-defects-445`,
  `2026-08-07-quickfiler-test-form1-live-form-491`,
  `2026-08-07-quickfiler-explorer-controller-latent-defects-449`.
- One took **no date prefix at all**: `winformspumphost-suite-determinism-511`.

So the name is neither today-dated nor uniformly shaped, and it cannot be predicted at
manifest-authoring time. All four guessed `2026-08-21-<slug>-<issue>` values in that run's epic
manifest were wrong, and every one would have failed `feature_folder` resolution at execution.

**How to apply:** write the manifest's `feature_folder` as a placeholder, require every preparation
child to report the exact path the tool created, and back-fill from that receipt before the kickoff
artifact is written. Verify each back-filled value resolves to a real folder rather than trusting the
child's text:
`git ls-tree -r --name-only origin/<child-branch> -- docs/features/active/<folder>`
A correct entry returns the folder's files; a wrong one returns nothing.

The `epic-orchestrate` skill already calls `feature_folder` "a resolvable hint, not a stable
identifier" — this is the concrete reason why.

Related: [[recover-dead-prep-child-by-committing-then-relaunching]],
[[epic-planner-state-required-fields]].
