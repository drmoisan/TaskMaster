---
name: preflight-citation-match-propagates-false-fact
description: A preflight delta that makes document A's requirement match document B's citation can codify B's error into A; verify the FACT, not the match. Also - an epic child's issue.md promise to siblings needs a matching plan constraint plus a verification task or it is unenforced.
metadata:
  type: project
---

Two preflight failure classes seen on the qfc-item-controller-defects-484 plan (round 3, after two
prior REVISIONS REQUIRED rounds).

**1. "Make the cited requirement exist" deltas can propagate a false fact.**
Round 2 found that `P7-T7` carve-out (b) cited a dossier requirement (`P5-T12`) that did not yet
exist, and asked the planner to add it. The planner added the requirement using `P7-T7`'s own
wording — "its body cannot execute without a live WebView2 runtime" — which was itself wrong: the
method (`DetachWebResourceRequestedHandler`) is called from `Cleanup()` via `UnwireEvents()`, and
several tests in the same plan call `Cleanup()`. Only the guarded `-=` statement inside it is
unreachable. The plan's own coverage task would have produced a nonzero line rate contradicting the
dossier the plan required the executor to write.

**Why:** a round-N finding phrased as "A should point at a requirement that exists in B" is
satisfied by copying A's prose into B. The copy inherits A's error and now reads as corroborated by
two documents.

**How to apply:** when validating a "citation now matches" fix, re-derive the cited FACT from the
research/source and from the plan's own task graph (which tests call which method), not from the
agreement between the two clauses. Reachability claims are the highest-risk class: trace the call
chain from every test the plan adds.

**2. An epic child's `issue.md` guarantee to siblings must be bound by a plan constraint AND a
verification task.**
Round 2 required `issue.md` to disclose that a shared test-support file is consumed by other epic
children and to state the change is additive-only. The applied fix added the sentence to
`issue.md`, but the plan's file-ownership constraint (`C1`) only said "helpers only; no test method
may be added" — nothing forbade renaming or editing an existing helper, and no task verified it.
17 other test files in `QuickFiler.Test` consume that file, one of them adjacent to a sibling
feature.

**Why:** the disclosure exists to protect concurrent siblings on the same integration branch. A
promise in `issue.md` with no counterpart in the plan is a claim the executor is never asked to
honour or check.

**How to apply:** for any shared/observed file an epic child touches, require three things, not
one: the disclosure in `issue.md`, a binding clause in the plan's ownership constraint, and an
acceptance clause on an existing task that mechanically verifies it (member-set comparison against
`<BASE_SHA>` is robust; a `git diff --numstat` deleted-count-of-zero check is not, because the
plan's own csharpier pass can rewrite existing lines).

Related: [[418-plan-rationale-clauses-are-evidence]], [[multipattern-gate-shared-qualifier-detachment]].
