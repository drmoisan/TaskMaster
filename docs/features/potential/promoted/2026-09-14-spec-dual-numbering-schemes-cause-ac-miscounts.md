# Bug: Two coexisting acceptance-criteria numbering schemes in one spec cause miscounts (Issue #899)

- Work Mode: full-bug
- Reported: 2026-09-14
- Source: run `bugs-2026-09-11`, observed on items 742 and 869

- Issue: #899
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/899
- Last Updated: 2026-09-14
- Status: Promoted -> docs/features/active/Bug_Two_coexisting_acceptance-criteria_numbering_schemes_in_one_spec_cause_miscounts/ (Issue #899)
## Summary

A single `spec.md` can carry **two different acceptance-criteria numbering schemes at once** — a set
of `(#<issue>)`-prefixed criteria scattered through the document, and a separate numbered
`## Acceptance Criteria` section. The counts do not agree, and neither scheme announces that the
other exists.

On item 869: **15** `(#869)`-prefixed criteria coexist with a **31**-entry `## Acceptance Criteria`
section.

The section-scoped count is the authoritative one. Nothing in the document says so.

## Measured consequences — this misleads readers, it is not merely untidy

Two real miscounts by an experienced reader during this run:

1. **Item 742.** A whole-file grep reported 18 checked / 4 unchecked, contradicting the child's
   reported 16/17. The child was right: the extra four checkboxes lived under `## Context` and
   `## Repro & Evidence`, outside the Acceptance Criteria section.
2. **Item 869, near-repeat.** A `(#869)`-prefixed regex reported `checked=15 remaining=0` while the
   authoritative section held 31 criteria. Had that figure been trusted, the item would have been
   reported complete with 16 criteria unexamined.

Both were caught, but only because the reader re-scoped and re-counted. The failure mode is a
confident, specific, wrong number — which is harder to doubt than a vague one.

## Proposed fix / validation ideas

1. Pick one scheme per spec. Prefer the `## Acceptance Criteria` section as the single authoritative
   list, since existing tooling and the `acceptance-criteria-tracking` skill already scope to it.
2. If `(#<issue>)`-prefixed criteria must remain for cross-referencing, require that they be a strict
   subset *inside* that section rather than scattered through the document.
3. Forbid checkbox syntax outside the Acceptance Criteria section, so a whole-file count cannot
   silently include narrative checkboxes from `## Context` or `## Repro & Evidence`.
4. Consider a validator that fails when a spec's whole-file checkbox count differs from its
   section-scoped count.

**Acceptance must be demonstrated, not argued:** per the lesson recorded on #895, any criterion added
here must be observed FAILING against a spec that currently exhibits the defect — item 869's spec is
a ready fixture — before it is accepted as a gate.

## Next step

Triage and schedule. Not promotable from item 869's branch — its write set is closed.
