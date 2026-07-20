---
name: constraint-propagation-waiver-cascades-serially
description: A ratified generic-base constraint propagates through cross-child consumers one file at a time; extend the cross-child waiver serially (re-escalate, never widen blind) and expect the ratified base-set wording to be factually imperfect
metadata:
  type: feedback
---

When a maintainer ratifies adding a generic type constraint (e.g. `where TKey : notnull`) to public
generic base types, and those bases have consumers owned by ALREADY-MERGED sibling children (per-file
opt-in annotation epic), the constraint propagation surfaces new CS8714 in the consumers ONE FILE AT A
TIME as each base is constrained. Do not try to enumerate the full cascade up front and grant a wide
waiver.

**Why:** each additional base you constrain can implicate a different consumer that only compiles-fail
once THAT base carries the constraint. In utilitiescs-nullable-remediation #366 the cross-child waiver
grew serially: 1 file (WrapperScoDictionary.cs, 22:40Z) → 2 files (+ScoDictionaryConverter.cs, 23:10Z)
→ 3 files (+WrapperScDictionary.cs when the ScDictionary base was constrained, next-day 01:07Z,
"Option A'") → 4 files (+ScDictionaryConverter.cs [no 'o'], 01:43Z, "Option A''", FINAL). Each grant is
the same in kind: mechanical propagation of the ratified constraint to a direct generic consumer;
smallest diff keeping the ratified contract internally consistent. Withdraw (partially reverses
ratification) and defer (leaves tree contract-inconsistent) are the standing rejected options.

**The cascade CAN be closed definitively — demand the enumeration.** The serial-extension caution
(don't grant a wide waiver up front) does NOT mean you can never bound it: once enough bases are
constrained, a child can run an assembly-wide grep to enumerate the COMPLETE consumer set and prove no
further consumer exists. In #366 the closed set was symmetric — each truly-generic base with cross-child
consumers (ScoDictionaryNew, ScDictionary) had exactly two consumers, a Wrapper and a Converter → a
closed set of four; the two People-namespace candidates were SAFE (concrete type args / commented-out
reference). Record the definitive-enumeration evidence with the final grant, state "no fifth possible /
no further escalation," and still instruct the child to HALT + re-escalate if an impossible consumer
somehow appears.

**How to apply:** instruct the child to re-escalate (STOP, do not widen the waiver unilaterally) each
time a new cross-child consumer surfaces; extend the epic-owned waiver by exactly the one newly-implicated
file per escalation and record it (checkpoint block + status projection + delegation receipt). Also expect
the ratified base-set WORDING to be factually imperfect: #366's "four generic bases" list included
ScoDictionaryStatic, which is a NON-GENERIC static class of extension methods — the constraint is
mechanically inapplicable there. Net effect was three truly-generic bases + three wrapper consumers.
Record such corrections and tell final QC to verify the real applicable set and DOCUMENT the plan-wording
deviation rather than fail on the literal plan text.

**Plan-literal acceptance targets recur as deviations to DOCUMENT, not fail.** #366 had two: (1) [P9-T9]'s
"four generic bases" wording (ScoDictionaryStatic is non-generic → verify three truly-generic bases,
document the deviation); (2) [P9-T3]'s literal "solution-wide 0 CS86xx/0 CS8714" — UNSATISFIABLE for a
per-cluster child because ~140 pre-existing cross-child CS86xx come from sibling-owned nullable-enabled
files ([[project_cross_child_annotation_fanin_debt]]). Ruling: the OPERATIVE per-child gate is the
isolated-cluster decomposition (0 CS86xx/0 CS8714 within the child's own opted-in cluster incl. the
waiver lines); the solution-wide-zero target is the CAPSTONE's obligation, not the child's. Document the
deviation in the dossier + feature-review artifacts; do not treat it as a child failure.
Related: [[project_cross_child_annotation_fanin_debt]].
