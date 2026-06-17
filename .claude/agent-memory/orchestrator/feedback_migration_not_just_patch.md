---
name: migration-not-just-patch
description: Before the PR gate, this user wants the migration/integration posture of a feature surfaced, not just a passing patch
metadata:
  type: feedback
---

Before moving to the PR-creation gate, surface how a feature integrates with and migrates from the prior methodology — not just that the patch passes its own audits.

**Why:** On #177 (hierarchical LCPPN folder prediction) the orchestration reported review-clean (blocking_count 0) and was ready for the PR, but the feature was additive, flag-gated, NOT wired into production call sites, and NOT persisted across restart — i.e. dark in production. The user pushed back: "It is one thing to have a working patch. It is another to migrate all of the ancillary functionality." A clean audit against a feature's own acceptance criteria does not mean the prior methodology is actually replaced.

**How to apply:** When a feature introduces a new mechanism that is meant to supersede an existing one, before recommending the PR gate, report: which consumers actually use it vs. the old path, whether it replaces or runs alongside, whether it is reachable in production (flag actually flipped at call sites), persistence/load parity, and an explicit "migration gaps" list. Offer ship-now-plus-follow-ups vs. expand-scope-before-PR as a choice rather than defaulting to PR. Relates to [[evidence-and-lifecycle-for-every-change]].
