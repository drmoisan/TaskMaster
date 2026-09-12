---
name: check-for-prior-ratified-exemption-boundary
description: Before planning any coverage/exemption-removal work, search docs/features/archive for a prior maintainer-decision + exemption-boundary artifact covering the same type; a ratified boundary overrides an epic's "treat as unratified" instruction
metadata:
  type: project
---

Before planning `[ExcludeFromCodeCoverage]` removal for any type, search
`docs/features/archive/**` for `maintainer-decision.*.md` and
`evidence/other/exemption-boundary.*.md` covering that type. A ratified boundary is the governing
authority and an epic-level instruction to "treat existing attributes as unratified" does NOT
override it.

**Why:** Epic #136 instructs children to treat QuickFiler's existing attributes as unratified until
F1's ledger adjudicates them. For `QfcItemController` that was already false: issue #227 reduced the
boundary 103 -> 19 across five cycles (the maintainer rejecting each intermediate count) and ratified
the residual on 2026-07-02. Nine of the 19 are blocked by an unbuilt WinForms message-pump seam
tracked as open issue #230, explicitly deferred and explicitly *not* a merge condition. A child that
took the epic instruction literally would have tried to drive 19 -> 0, overturned a maintainer
decision, and attempted a large piece of deferred test infrastructure.

**How to apply:**
- Find the ratified artifact first; reconcile the epic's AC against it in `spec.md` and say plainly
  that the epic's AC cannot be satisfied literally.
- Diff the ratified member list against a live `grep` of the attribute. A count mismatch means
  post-ratification drift — those attributes are genuinely unratified and are the child's to resolve.
  For F10, 19 sites vs 18 ratified members isolated exactly one drifted attribute.
- **Read the ratified rationale, not the in-code comment.** They diverge. F10's `Navigation.cs`
  comments cite a `TlpCellSnapShot` barrier that a later retrofit removed, so the comments are stale —
  but the ratified rationale is "deliberate virtual test seam" and was written with that retrofit
  explicitly in view ("now de-exempted at the leaf via R2"). A retrofit the ratification anticipated
  cannot be evidence the ratification lapsed. Correct the comment; retain the attribute; refer the
  weakened-rationale observation to the maintainer.
- Dead code is the one clean reduction: deleting a member with zero call sites removes its exemption
  without weakening the boundary, and beats both testing it and exempting it.

Related: [[feedback_verify_reducibility_before_accepting_exemption_count]],
[[feedback_no_coverage_exemption_when_purpose_is_testability]],
[[feature-review-coverage-85-floor-trap]]
