---
name: remediate-related-defects-in-item
description: Maintainer directive (2026-10-02) - related defects found during an item are fixed inside that item, never handed to the coordinator as follow-ups; every child prompt must carry the directive block
metadata:
  type: feedback
---

Related defects found by review, executor or research (same files, same component, same root cause, or a sibling call site; including comment drift, file-size splits, missing disposal or try/finally, and test-quality nits in touched files) are remediated INSIDE the item: an in-place planner widening, a failing regression test first for behaviour defects, then the normal toolchain and review. Only a completely unrelated defect is reported for filing. This overrides the CLAUDE.md "open a new issue instead of widening scope" line for related defects, by explicit maintainer ruling.

**Why:** the bugs-2026-09-28 run produced long lists of "follow-ups for the coordinator to file" per item (956, 948, 950, 952, 953), most of them in the files the item had just touched; the maintainer wants them closed while the context is loaded, not queued.

**How to apply:** paste the RELATED-DEFECT REMEDIATION block verbatim into every child prompt (it also scopes the standing-authority STOP clause so related widening proceeds while AC weakening and unrelated scope still stop). Before merging, check the child's follow-up list: any related item still listed means the item is not done; send it back for an in-item remediation pass rather than merging. Fold already-filed related issues into the item the coordinator names at launch. A child launched before the directive cannot be messaged by the parent; apply it at a pre-merge remediation pass.
