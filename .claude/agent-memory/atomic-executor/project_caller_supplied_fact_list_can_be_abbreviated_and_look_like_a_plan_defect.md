---
name: caller-supplied-fact-list-can-be-abbreviated-and-look-like-a-plan-defect
description: A delegation prompt's "do not re-verify" fact list can be abbreviated, so a correct plan citation reads as contradicting an established fact; read the tree before reporting it.
metadata:
  type: project
---

A preflight delegation prompt that supplies a "Facts established against this worktree — do not
re-verify, do not correct" list can carry abbreviated entries. When a plan citation disagrees with
one of those entries, read the file before reporting a defect: the plan may be right and the fact
line merely shortened.

Worked example, issue 816 round 3. The prompt stated ``UiThreadStateScope.cs``: namespace
`UtilitiesCS` at 9''. The plan's P1-T1 cited namespace `UtilitiesCS.Test` and argued reachability
from `UtilitiesCS.Test.Threading` without a using directive "because `UtilitiesCS.Test` is its
parent namespace". Line 9 of `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` is
`namespace UtilitiesCS.Test`. The plan was correct; the fact line had dropped the suffix. Reporting
it as a defect would have forced a fourth round and, worse, invited the planner to "correct" a
correct citation into a false one.

**Why:** the no-re-verify instruction exists to save tokens on facts the caller already derived, not
to make the caller's transcription authoritative over the tree. Under either spelling the
reachability claim holds (both `UtilitiesCS` and `UtilitiesCS.Test` are ancestor namespaces of
`UtilitiesCS.Test.Threading`), so the apparent conflict was purely in the stated reason.

**How to apply:** treat a caller fact list as a cache, not as the oracle. The no-re-verify boundary
applies to facts the plan AGREES with. The moment a plan citation and a supplied fact disagree, that
single point is back in scope: one targeted Read settles it. Related:
[[project_preflight_citation_match_propagates_false_fact]] covers the opposite direction, where
editing the tree to make a citation true spreads a false fact.
