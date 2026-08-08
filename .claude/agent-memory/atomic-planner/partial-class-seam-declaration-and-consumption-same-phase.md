---
name: partial-class-seam-declaration-and-consumption-same-phase
description: Under the #136 one-phase-per-production-file mandate, a seam declared in one partial and consumed in its sibling partial must have BOTH edits in the earlier phase, not split across the two file phases
metadata:
  type: feedback
---

When a per-file plan (issue #136 mandate: one atomic-plan phase per production file) hits a seam whose
declaration lives in partial A and whose consumption sites live in partial B, put **both** edits in
phase A and leave phase B tests-only. Do not split declaration into phase A and consumption into
phase B.

**Why:** a declared-but-unconsumed `internal` member is dead code — it fails the analyzer build under
`/p:EnforceCodeStyleInBuild=true`, and it depresses phase A's own per-file coverage gate because the
member has no caller. Worse, phase A's own test cases usually reach the consumption site through the
partial boundary (F4 #434: `ConversationResolver.cs` test T25 needs the `IUiDispatcher` substitution
that physically lives at `ConversationResolver.Loading.cs:150`), so a split ordering makes phase A
un-passable. Both partials belong to the same child, so grouping the edits creates no cross-child
conflict.

**How to apply:** in the earlier phase, emit one task per seam declaration and one task per
consumption-site substitution, each naming its exact file, plus a build-verification task listing the
sibling-owned call sites that must still compile byte-identically. State the grouping rationale in the
phase preamble so a reviewer does not read it as a violation of the per-file mandate. Related:
[[research-claims-as-acceptance-clauses]], [[literal-call-clauses-block-file-size-tightening]].
