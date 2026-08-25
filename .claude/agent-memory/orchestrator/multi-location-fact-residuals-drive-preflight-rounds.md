---
name: multi-location-fact-residuals-drive-preflight-rounds
description: In multi-document feature preparation, nearly every blocking preflight defect is one fact corrected in some locations but not all; enumerate every location per fact before declaring clear
metadata:
  type: feedback
---

When a feature carries `issue.md` + `spec.md` + a plan + research, the dominant blocking-defect
class is **a fact stated in several documents and corrected in only some of them**. Do not treat a
correction as landed because the round that made it said so; sweep for residuals in text the
correcting round never opened.

**Why:** Epic child 484 (`qfc-item-controller-defects-484`) needed a five-round confirming preflight
at C3/opus. Of six blocking defects, five were residuals of an earlier one in a NEW location:
D6/D10 residuals of D2; D12 a residual of D7 in a third location; D16 and D19 residuals of D2 in a
fourth and fifth; D17/D18 residuals of round 3's routing rewrite. Round 4's residual sweep found a
round-1 correction that had never landed. Round 5's own findings (D23, D24) were residuals of round
4's fixes reaching only part of the surface they addressed.

**How to apply:**
- Arm each preflight delegation with the explicit instruction to **verify prior rounds' corrections
  landed on disk**, and to treat any that did not as still open. This is the single highest-yield
  check available.
- Name the two or three **recurring facts** (a count, an enumerated set, a budget) and require the
  agent to enumerate EVERY location stating each, across all four documents, and confirm mutual
  agreement — not just to fix the one it noticed.
- Require adversarial review of the **previous round's own edits**; they are the newest and least
  reviewed surface, and they are where the next residual lives.
- A correction that is a *mechanical rule* (e.g. "append below the citation") only protects the
  files it names. Ask what fraction of the affected surface it reaches; the remainder usually needs
  a *declarative* fix (e.g. "all citations are anchored at BASE_SHA") instead.
- Bound the pass (five rounds worked here) and require an explicit CONVERGING-vs-CHURNING judgement
  at the bound. Converging looks like: severity falls, findings move into newer text, and the last
  round's findings change no figure, file assignment, member table, or gate.
- **The residuals are MANUFACTURED by the correction directive, not just missed by the executor.**
  A correction prompt written as site-specific edits ("in P0-T10, change X") gets exactly that: the
  planner fixes P0-T10 and the identical fact at P11-T2 survives untouched, becoming next round's
  "residual". Child 489 round 3 produced three residual classes this way, all of them corrections
  rounds 1-2 had already made at the sites those rounds happened to notice. Site-specific phrasing
  is still correct — it is what stops a planner renumbering tasks and invalidating cross-references —
  so do not loosen it. Instead PAIR every site-specific edit with a mandatory sweep clause: "apply
  this at the named site, then re-derive every other site in the plan exhibiting the same underlying
  fact, apply it there too, and report the full site list you found." Arming only the executor with
  the sweep mandate catches residuals one round LATE; arming the planner prevents them.

**Diagnosing a rising defect count.** A round-over-round increase is not automatically divergence.
Separate the causes before judging the planner: (a) scope you widened this round (adding a defect
ledger, G1-G6, or a citation-verification mandate raises the count by construction — see
[[preflight-defect-trend-scope-confound]]); (b) environment facts only a probe finds, such as a
missing repo-local SDK, which no amount of planning prose detects; (c) staleness the WORLD caused,
e.g. a cited integration head that a mid-session merge moved; and only then (d) genuine planner
residuals. In 489 round 3, twelve defects decomposed as 2 non-planner, 4 routine hygiene, 3 true
residuals — with blockers still falling 6 → 1 → 3 and zero re-introductions throughout.

Related: [[remediation-loop-strict-handoff]], [[epic-kickoff-facts-need-independent-measurement]].
