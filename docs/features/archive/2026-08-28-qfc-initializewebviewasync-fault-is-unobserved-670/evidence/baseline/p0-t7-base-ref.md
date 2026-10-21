# P0-T7 — Delivery-run head and diff-base reachability

Timestamp: 2026-09-01T19-44
Command: `git rev-parse HEAD`, `git rev-parse --abbrev-ref HEAD`, `git merge-base --is-ancestor <BASE> HEAD`, `git merge-base HEAD <BASE>`, `git merge-base HEAD origin/main`
EXIT_CODE: 0

## Base-ref substitution (non-discretionary)

The plan's section 0 pins `BASE` to `2b85134b42872e405602e6064e02dc9cda6c319b`. That commit was `origin/main` at plan-authoring time. It has since been superseded: `origin/main` advanced to `988d35a8f8eb7436cc46a9f6424db917ed93807a`, carrying eight sibling deliveries, and that commit was merged into this branch before execution began.

**Every `git` command in this delivery run substitutes `988d35a8f8eb7436cc46a9f6424db917ed93807a` for the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`.** The plan-pinned SHA is a stale ancestor rather than the current merge base, which makes every diff gate that names it unsatisfiable for reasons unrelated to this change. The plan file's section 0 and its task prose are not edited; the substitution is recorded in the evidence artifacts instead, per the executing directive. The affected tasks are P0-T7, P1-T2, P1-T7, P2-T4, P2-T5, P3-T11, P4-T11 and P4-T29.

## Recorded outputs

    git rev-parse HEAD
    0869ca931fc131a39697bc6cf96189e1da61651a

    git rev-parse --abbrev-ref HEAD
    bug/qfc-initializewebviewasync-fault-is-unobserved-670

    git merge-base --is-ancestor 988d35a8f8eb7436cc46a9f6424db917ed93807a HEAD
    (exit 0)

    git merge-base HEAD 988d35a8f8eb7436cc46a9f6424db917ed93807a
    988d35a8f8eb7436cc46a9f6424db917ed93807a

    git merge-base HEAD origin/main
    988d35a8f8eb7436cc46a9f6424db917ed93807a

The `--is-ancestor` invocation exits 0, so every later `git diff 988d35a8f8eb7436cc46a9f6424db917ed93807a` gate in this delivery run is well-formed.

## Discrimination analysis of the two reachability checks

The executing directive required both the ancestry check and the merge-base equality check, on the stated ground that the ancestry check alone passes vacuously for any ancestor while the merge-base equality is what discriminates a current anchor from a stale one. The first half of that reasoning is correct and the second half is not, and the distinction is recorded here rather than left implicit.

Both checks were run against the superseded SHA as a control:

    git merge-base --is-ancestor 2b85134b42872e405602e6064e02dc9cda6c319b HEAD
    (exit 0)

    git merge-base HEAD 2b85134b42872e405602e6064e02dc9cda6c319b
    2b85134b42872e405602e6064e02dc9cda6c319b

The superseded SHA passes **both** checks. This is a property of the operation rather than an accident of this tree: `git merge-base HEAD X` returns `X` itself whenever `X` is an ancestor of `HEAD`, so the equality holds for every ancestor and cannot distinguish a current anchor from a stale one. The two checks are therefore equivalent in discriminating power, and neither one falsifies a stale pin.

The check that does discriminate is `git merge-base HEAD origin/main`, which names the merge base rather than testing a supplied candidate against it. It printed `988d35a8f8eb7436cc46a9f6424db917ed93807a`, which is not equal to the plan-pinned SHA, and that inequality is the evidence that the plan-pinned SHA is stale.

## Substantive confirmation of the re-anchor

The decisive observation is the diff itself, taken over the two directories this change touches:

    git diff --name-status 2b85134b42872e405602e6064e02dc9cda6c319b HEAD -- QuickFiler QuickFiler.Test
    (17 paths listed)

    git diff --name-status 988d35a8f8eb7436cc46a9f6424db917ed93807a HEAD -- QuickFiler QuickFiler.Test
    (no output)

Against the superseded SHA the diff already reports 17 contaminating paths before this delivery run has written a single line, which makes the P4-T11 five-path gate unsatisfiable and separately breaks its requirement that `QuickFiler.Test/QuickFiler.Test.csproj` print nothing, because `origin/main` modified that project file. Against the re-anchored SHA the same diff is empty, which is the clean starting state the Phase 4 gates require.

Output Summary: HEAD is `0869ca931fc131a39697bc6cf96189e1da61651a` on branch `bug/qfc-initializewebviewasync-fault-is-unobserved-670`. The re-anchored base `988d35a8f8eb7436cc46a9f6424db917ed93807a` is an ancestor of HEAD and is the merge base of HEAD with `origin/main`, and the diff between it and HEAD over `QuickFiler` and `QuickFiler.Test` is empty. The admission condition holds and the plan is not blocked. One defect beyond the supplied re-anchor is recorded above: the merge-base equality check the directive prescribed does not discriminate a stale ancestor, because the operation returns the candidate itself for any ancestor.
