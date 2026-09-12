---
name: plan-line-locators-stale-after-doc-edit
description: When a plan revision edits a referenced doc (spec.md, research), any line-number locator the plan cites for that doc shifts; re-derive locators after the edit or preflight fails on an unverifiable acceptance clause.
metadata:
  type: project
---

A plan-revision cycle that both (a) edits an external document and (b) writes an acceptance
clause citing line numbers in that document must re-read the document AFTER the edit and
re-derive the locators. Expanding a sentence by N lines shifts every later citation by N.

**Why:** On issue #434 (quickfiler-helper-classes-coverage) the reviser expanded a
`spec.md` sentence in § Sequencing by two lines, then wrote `[P1-T15]` acceptance as
"`spec.md` (lines 387 and 532) ... are updated from `thirteen` to `fourteen`". Post-edit the
second site had moved to 534-535; line 532 held unrelated prose. Line 387 (the earlier site)
was still correct because it precedes the insertion point. The clause was therefore
half-unverifiable and required a second preflight cycle for a one-token fix.

**How to apply:**
- During preflight, do not accept a line-number citation in an acceptance clause on trust.
  Open the cited file at that exact line and confirm the asserted text is present.
- When a citation is stale, check whether the offset equals the number of lines the revision
  added earlier in the same file — that confirms a pre-edit locator was reused and tells you
  the corrected number without guessing.
- Locators BEFORE the edit point stay valid; only locators AFTER it shift. A partially
  correct pair is the signature of this defect.
- Prefer section-anchored locators ("§ Shared-file conflict surface") over raw line numbers
  when proposing a delta, since they survive later edits.

Related: [[project_418_plan_rationale_clauses_are_evidence]],
[[project_preflight_blanket_assertion_and_forward_dependency]].
