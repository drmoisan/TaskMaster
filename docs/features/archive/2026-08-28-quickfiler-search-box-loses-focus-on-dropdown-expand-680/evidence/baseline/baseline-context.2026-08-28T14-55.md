# Baseline Execution Context (Issue #680)

Timestamp: 2026-08-28T14-55

Command: `git branch --show-current` and `git rev-parse HEAD`

EXIT_CODE: 0

Output Summary:

- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680`
- BASELINE_COMMIT: `c2d683d51d907d5591e313a550099fc267c10da6`
- Spec status/version, quoted verbatim from `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/spec.md`:
  - `- **Status:** Draft`
  - `- **Version:** 0.1`

## Scope lock

`spec.md`'s `## Acceptance Criteria` section (AC-1 through AC-9) is the sole authoritative
acceptance-criteria source for this work. Work Mode is `full-bug` per `issue.md` metadata,
so `spec.md` is the AC source and `user-story.md` is absent by design. No other document —
including `issue.md`, the research artifact, or this plan — may add, remove, or reinterpret an
acceptance criterion. AC-1 and AC-2 are manual live-Outlook criteria and are not dischargeable
by any automated task in this plan.
