---
name: new-active-feature-folder-date-prefix
description: new_active_feature_folder date-prefix is context-dependent — standalone features get the YYYY-MM-DD- prefix automatically, but epic-child feature folders do not (git mv those)
metadata:
  type: feedback
---

`mcp__drm-copilot__new_active_feature_folder` date-prefix behavior depends on
context:

- A **standalone** feature promotion DOES get the prefix automatically. In #48
  (2026-06-01), `type: feature` produced
  `2026-06-01-pipeline-gui-hardening-schema-select-48` with the `YYYY-MM-DD-`
  prefix and no `git mv` was needed.
- **Epic-child** feature folders: behavior has CHANGED. In old epic #40
  (2026-05-30) the child folders came out WITHOUT a prefix (`schema-model-and-registry-41`).
  But as of 2026-07-10 the tool prefixes epic children too: swordfish-removal F5
  produced `2026-07-10-swordfish-interface-project-teardown-308` automatically,
  and the winforms-testability epic children on the integration branch all carry
  date prefixes (`2026-07-09-tagcontroller-testability-refactor-293`, `...-296`,
  `...-297`, `...-298`). No `git mv` was needed for the swordfish child.
  Reconfirmed 2026-07-15: folder-tree-percentage-ui child 9003 preparation
  produced `2026-07-15-quickfiler-folder-tree-percentage-325` automatically
  (passing `feature_name=quickfiler-folder-tree-percentage`, `issue_number=325`);
  the tool added both the date prefix and the trailing issue number. No `git mv`.
  Swordfish `feature_folder` manifest values retain the date prefix, so the
  epic-planner back-fills the manifest to the full date-prefixed+issue folder name.

**Why:** The canonical convention is the date-prefixed form for every active
folder (a user previously flagged missing prefixes on epic children). The current
tool now applies it automatically for both standalone and epic-child folders.

**How to apply:** After creating folders, verify the name carries the
`YYYY-MM-DD-...-<issue>` shape. As of 2026-07-10 the prefix is present
automatically for epic children too, so `git mv` is normally NOT needed — only
rename if the tool regresses to the prefix-less form. The trailing integer is the
issue number used for canonical-issue derivation, so keep it intact. Related:
[[potential-to-issue-creates-github-issue]].
