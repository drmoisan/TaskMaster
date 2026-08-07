---
name: promotion-scaffold-metadata-defects
description: Promotion tooling scaffolds issue.md with a Status path missing the date prefix and -NNN issue suffix, and sometimes a wrong Last Updated date — verify and fix both when filling feature docs
metadata:
  type: project
---

The feature-promotion scaffold can emit `issue.md` metadata defects that the prd-feature agent must correct when filling documents:

- `- Status: Promoted -> docs/features/active/<slug>/` omitting the `YYYY-MM-DD-` prefix and `-<issue#>` suffix of the actual active folder (seen on issue #424, 2026-08-06).
- `- Last Updated:` dated in the future relative to the current date (424 scaffold said 2026-08-07 on 2026-08-06).

**Why:** The delegating orchestrator requires all paths/cross-references to use the canonical issue number and folder name; stale scaffold metadata would fail review.

**How to apply:** On every fill-in-place task, diff the `Status` path against the real active folder name and sanity-check `Last Updated` before writing spec/user-story content. In full-bug mode the `- Work Mode: full-bug` marker must sit above the first `##` heading in `issue.md`; check it is already present before adding one.
