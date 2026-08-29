---
name: merging-main-invalidates-plan-base-anchor
description: In a serial parallel run, merging origin/main at execution start silently invalidates every git-diff span the plan anchors to its recorded base commit; substitute the merge commit as the anchor before delegating
metadata:
  type: feedback
---

When a parallel-run item is directed to merge the current `origin/main` tip before execution, every
acceptance clause in its committed plan that anchors a `git diff` to the plan's recorded **base
commit** becomes wrong. Substitute the **merge commit** as the anchor, and say so in the delegation
prompt before the executor starts.

**Why:** an atomic plan's footprint gates are written as `git diff <base-sha> -- <paths>` and assert
an exact or membership set over the result. The base SHA was correct when the plan was authored. Once
you merge a sibling item's already-merged work into the branch, that diff additionally lists every
path the sibling touched, so:

- a `[P1-T2]`-style gate demanding "exactly one added line and zero removed lines" in a shared file
  (typically the test `.csproj`, which sibling bug fixes also edit to register their test files) fails
  even though your edit is exactly one line;
- a footprint-containment gate reports the sibling's whole changed-file set as out-of-footprint and
  routes the executor into a spurious `REMEDIATION-REQUIRED` branch.

The merge commit is the true pre-change state of the run, so anchoring to it restores exactly the
semantics the plan author intended. It narrows the diff rather than widening any acceptance, which is
what makes the substitution safe to authorize.

**How to apply:** decide this at execution start, not mid-run — the executor cannot be course-corrected
once launched. Steps:

1. Fetch, merge `origin/main`, capture the merge commit SHA.
2. Record a `local_execution_overrides` entry in the checkpoint naming the plan value, the substituted
   value, who authorized it, and why. The checkpoint validator accepts the array.
3. In the delegation prompt, name every task ID that carries the literal base SHA and instruct the
   executor to record the substitution in each affected evidence artifact.
4. Work out in advance which *previously expected* paths will now be ABSENT from the anchored listing,
   and tell the executor those absences are correct. Paths the preparation commit added are already
   tracked at the merge commit, so an anchored diff no longer lists them. A gate written as a
   membership test ("no path other than ...") still passes on a smaller listing; a gate written as an
   equality would not, and would need the same substitution reasoning applied in the other direction.

Resolve the merge conflict in a shared test `.csproj` by keeping BOTH sets of registrations. Dropping
either silently unregisters a sibling's test file, and a legacy non-SDK project compiles no `.cs` file
that is not listed, so the loss surfaces as a missing test rather than a build error.

See [[epic-child-stale-local-integration-ref]] and [[stale-figure-sweep-by-changed-file-set]].
