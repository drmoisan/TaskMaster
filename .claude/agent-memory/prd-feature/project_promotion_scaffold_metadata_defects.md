---
name: promotion-scaffold-metadata-defects
description: Promotion tooling scaffolds issue.md with a Status path missing the date prefix and -NNN issue suffix, a wrong Last Updated date, and AC continuation lines turned into phantom checkboxes — verify and fix all three when filling feature docs
metadata:
  type: project
---

The feature-promotion scaffold can emit `issue.md` metadata defects that the prd-feature agent must correct when filling documents:

- `- Status: Promoted -> docs/features/active/<slug>/` omitting the `YYYY-MM-DD-` prefix and `-<issue#>` suffix of the actual active folder (seen on issue #424, 2026-08-06).
- `- Last Updated:` dated in the future relative to the current date (424 scaffold said 2026-08-07 on 2026-08-06).
- **Phantom acceptance criteria.** When the scaffold copies a multi-line `- [ ]` item from `issue.md` into `user-story.md`/`spec.md`, it prefixes every wrapped continuation line with `- [ ]` too, so 7 criteria become 12 checkboxes (seen on issue #436, 2026-08-08). Author each AC as **one** `- [ ]` item with continuation lines indented by spaces and no dash, then count checkboxes per file to confirm they equal the intended AC count.

**Why:** The delegating orchestrator requires all paths/cross-references to use the canonical issue number and folder name; stale scaffold metadata would fail review.

**How to apply:** On every fill-in-place task, diff the `Status` path against the real active folder name and sanity-check `Last Updated` before writing spec/user-story content. In full-bug mode the `- Work Mode: full-bug` marker must sit above the first `##` heading in `issue.md`; check it is already present before adding one.
