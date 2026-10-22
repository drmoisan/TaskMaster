Timestamp: 2026-08-31T14:04:00-04:00
Source: `evidence/regression-testing/p5-t5-single-assertion-change.md`
Output Summary: This is the designated source text for the pull-request change description.

The issue #439 criterion that a rooted target survives selection is superseded by issue #614's archive-relative-stem invariant, which #614 enforced on the `SelectHierarchyPath` half and at the filing boundary but not on the `SelectRow` half. This is a deliberate spec correction and explicitly not a weakened test.
