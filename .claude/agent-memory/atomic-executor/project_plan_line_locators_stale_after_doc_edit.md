---
name: plan-line-locators-stale-after-doc-edit
description: When a referenced doc (spec.md, research) is edited — by the plan reviser OR by an external actor between authoring and preflight — every later line-number locator the plan cites shifts; re-derive the whole citation family, and re-check any "observed corrections" prose that recorded the now-fixed defect.
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

**Second instance, issue #895 (fsharp-core-hintpath-netstandard21-skew), 2026-09-17.** Two
wrinkles the #434 instance did not show:

- **The editor was not the reviser.** The orchestrator corrected a `spec.md` bullet in
  § Dependencies or blocked work *between* plan authoring and preflight, adding four lines.
  Every one of the five AC citations in Phase 5 (`line 418/426/440/446/453`), the `[P5-T6]`
  counting range (`418-461`) and all five traceability-table entries were uniformly stale by
  exactly 4. Nothing in the plan recorded that an edit had happened, so the planner's own
  self-review could not have caught it — only re-deriving against the tree does. A *uniform*
  offset across the whole family (rather than the #434 partial split) is the signature when
  the insertion point precedes every citation.
- **The "observed corrections" list inverted.** The plan's `## Observed Corrections` item 1
  recorded, as an outstanding correction, that `spec.md` denied the existence of issue #879's
  feature folder. The orchestrator's edit had already applied that correction, so the item now
  asserted the opposite of the document's actual text — and `[P0-T1]` required the executor to
  reproduce it verbatim into a durable evidence artifact. A stale corrections list does not
  merely go inert; it launders a false claim about a requirements document into the audit
  trail. Re-read every "observed correction" against the current doc, not just the AC block.

Also worth knowing: the AC line numbers stayed stable *through* execution here because the
Phase 5 check-off tasks only flip `- [ ]` to `- [x] `, which preserves line counts. Verify that
property before accepting raw line numbers in check-off tasks; a task that rewraps or appends
to the AC block would invalidate its own later siblings.

Related: [[project_418_plan_rationale_clauses_are_evidence]],
[[project_preflight_blanket_assertion_and_forward_dependency]],
[[project_preflight_citation_match_propagates_false_fact]].
