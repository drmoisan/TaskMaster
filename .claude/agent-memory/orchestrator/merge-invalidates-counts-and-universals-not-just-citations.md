---
name: merge-invalidates-counts-and-universals-not-just-citations
description: After merging main into a branch whose plan already cleared preflight, a citation sweep is not enough — counts and universal/negative claims go stale too, and each sweep class is structurally blind to the next
metadata:
  type: feedback
---

When `origin/main` moves under a plan that has already cleared preflight, re-anchoring the
diff gates and re-deriving the line-number citations does NOT finish the job. Sweep three
classes, in this order, and expect each to find what the previous one could not:

1. **Line-number citations.** The obvious class. Also re-derive the *rule* you use to
   explain the shift — see [[merging-main-invalidates-plan-base-anchor]].
2. **Counts.** "N partial parts", "N test methods", "N compile entries". A line-number
   sweep never looks at these.
3. **Universals and negatives.** "every X", "no X", "the only X", "none of", "always",
   "never". A count sweep never looks at these either.

**Why:** on issue #796 (2026-09-07) two sibling merges landed under a cleared plan. Round 4
of preflight found a blocking *count* defect (`QfcItemController` was `internal partial`
across eleven parts; the merge added a twelfth), which the planner's own thorough
line-number sweep had just missed because it is not a line number. The planner then ran a
count sweep and found a false universal and an unsupported numeral. Round 5 then swept
universals and found three more, including that the plan (and `CLAUDE.md`) claimed
`Directory.Build.props` does not exist when it does. Five rounds, three classes, each one
invisible to the sweep before it.

**How to apply:**

- Name all three classes explicitly in the revision prompt. A planner told to "re-derive
  citations" will do exactly that and report success while counts rot.
- **Triage by whether an acceptance gate reads it.** Merge damage to a gate anchor or a
  gate's permitted-path set is blocking. A stale count or false universal sitting in a
  *rationale* position that no `Acceptance:` clause reads is not — correct it once if exact
  replacement text already exists, and do not let it open another review cycle. Without
  this rule the sweep widens forever: each round finds a rarer class and the plan's
  executability stops improving. State the stopping rule in the checkpoint before you need
  it, so closing the loop is a recorded decision rather than fatigue.
- **Test the anchor, don't read it.** Run the gate's diff at the new anchor and at the old
  one. New anchor should list nothing pre-implementation; old anchor should list the
  sibling file set. If the two do not differ categorically, the defect is not what you think.
- Give the closing pass an explicit **no-sweep** constraint, or it will find a fourth class
  and reopen the loop.
- A correction that names a path in a negative sentence must be **unbackticked** — see
  [[footprint-ac-forbids-onbranch-followup-promotion]] for why an extra write claim is
  expensive.

**Related trap.** A repo-doc defect found this way (here, `CLAUDE.md`'s false
`Directory.Build.props` claim) must NOT be fixed on the item branch: it adds a path outside
the declared write set and falsifies the checked write-set AC. Report it for a separate
change. See [[footprint-ac-forbids-onbranch-followup-promotion]].
