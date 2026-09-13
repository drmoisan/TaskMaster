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

**An ALREADY-prefixed `feature_name` is not double-prefixed (verified 2026-08-29, #647).**
Passing `feature_name=2026-08-27-fileio2-write-retry-reports-success-on-final-failure` (the promoted
potential's filename, which already carries a date) produced
`2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647` — the tool appended the issue
number and left the existing prefix alone rather than prepending today's date. So when the promoted
record's own name is the `${long-name}`, pass it verbatim; you do not need to strip the date first,
and the resulting folder keeps the potential's ORIGINAL date, not the day the folder was created.

**Why:** The canonical convention is the date-prefixed form for every active
folder (a user previously flagged missing prefixes on epic children). The current
tool now applies it automatically for both standalone and epic-child folders.

**REGRESSED for a standalone bug folder, 2026-09-08 (#810).** `type: bug`,
`feature_name=quickfiler-teardown-and-dropdown-residuals`, `issue_number=810` produced
`docs/features/active/quickfiler-teardown-and-dropdown-residuals-810` with NO date prefix. The
tool appended the issue number and omitted the `YYYY-MM-DD-` prefix entirely, so the check below
is still worth running on every creation rather than assuming the 2026-07-10 behavior holds.

**REGRESSION CONFIRMED AGAIN, 2026-09-12 (#602).** `type: bug`,
`feature_name=host-identifier-leakage-sweep`, `issue_number=602` again produced the prefix-less
`docs/features/active/host-identifier-leakage-sweep-602`. Two bug-type data points now (#810, #602)
with no prefix, against feature-type and epic-child runs that do prefix. Treat the prefix-less form as
the EXPECTED outcome for `type: bug` and plan the rename into the flow rather than checking for it.

A cheaper recovery than renaming: if the scaffolded documents are still the untouched templates (a
skeleton `spec.md` and an empty timestamped plan file), do not `git mv` them. `git clean -fdx -- <old-folder>`
the whole thing and materialize the real documents straight into the correctly-named folder. Renaming
is only worth it once the folder holds authored content. Note also that the scaffolded plan file's
timestamp becomes a second plan path competing with the one you actually want as canonical, which is a
further reason to discard rather than move it.

Renaming it is awkward when `pwsh` is refused by the sandbox (see
[[worktree-isolation-blocks-pwsh-per-agent-type]]): `Move-Item` and `Rename-Item` are both
unavailable, and `git mv` refuses an untracked path. The two-step that works with `git` alone is
`git add -- <old-folder>` then `git mv <old-folder> <new-folder>`; staging first makes the files
tracked so `git mv` will move them on disk.

**How to apply:** After creating folders, verify the name carries the
`YYYY-MM-DD-...-<issue>` shape. As of 2026-07-10 the prefix is present
automatically for epic children too, so `git mv` is normally NOT needed — only
rename if the tool regresses to the prefix-less form. The trailing integer is the
issue number used for canonical-issue derivation, so keep it intact. Related:
[[potential-to-issue-creates-github-issue]].
