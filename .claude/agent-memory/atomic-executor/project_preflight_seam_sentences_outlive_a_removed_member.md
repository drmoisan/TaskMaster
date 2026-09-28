---
name: preflight-seam-sentences-outlive-a-removed-member
description: When a plan decision declines an interface member (reads via concrete type instead), the spec's Test Strategy "Seam per criterion" sentences still describe a Moq double of the interface returning that member; a sweep for "widened"/"written" wording misses them
metadata:
  type: project
---

A residual class the interface-reduction sweep misses: spec Test Strategy seam sentences that
presuppose the removed member. On #792 round 3 (2026-09-12), after correction 8 moved the folder
handler read from the item-controller interface to an internal accessor on the concrete type,
spec 222/253/195 were fixed (R2-R4) but spec 263 still said the AC-U3 seam is "a Moq double of the
QuickFiler item controller interface returning a stub folder-search handler" — unrealisable, because
the interface has no such member (verified: only `ItemHelper` at 41 and `LoadFolderHandlerAsync` at 77).
Likewise spec 261/264 kept "an injectable breadcrumb host" after D2 settled an injectable delegate.

**Why:** the planner's sweep was keyword-driven ("widened", "written path", "interface member").
A seam sentence uses none of those words; it names a test double and what it returns, so it
survives every keyword pass while contradicting the settled design. It surfaces later when a
feature-reviewer audits spec Test Strategy against the tests actually written.

**How to apply:** whenever a correction/design decision removes or declines a member, also Grep the
spec for `Moq double of the .* interface` and `injectable` and read each hit against the plan's
D-decisions. Classify per [[confirmatory-preflight-proportionate-bar]]: the plan overrides the spec
for execution and no gate depends on the sentence, so on a confirmatory round it is a non-blocking
observation with a verbatim replacement offered, not a REVISIONS REQUIRED. On a first or second
round, include it in the enumerated delta so it does not become a late finding.
