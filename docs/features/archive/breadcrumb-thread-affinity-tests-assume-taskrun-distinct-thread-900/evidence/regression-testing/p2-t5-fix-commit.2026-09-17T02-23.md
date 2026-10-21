# P2-T5 — Fix Commit

Timestamp: 2026-09-17T02-23

Command (separate invocations, one command segment each):

1. write `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/commit-message-fix.txt`
2. `git add -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900`
3. `git commit -F docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/commit-message-fix.txt`
4. `git diff --exit-code HEAD -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
5. `git show --name-only --format= HEAD`
6. `Get-FileHash -Algorithm SHA256 -LiteralPath QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`

EXIT_CODE: 0

The exit code recorded is that of the anchored `git diff --exit-code HEAD` span in step 4.

CHANNEL: COMMAND

Steps 1 through 5 involve no `pwsh` process; step 6 does.

## Output Summary

COMMIT-EXIT: 0

FIX-HEAD: `63142ec73b41588bd09dbcf76afe05c9a26939d7`

FIX-HASH: 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

Commit summary as reported: `11 files changed, 860 insertions(+), 39 deletions(-)`, subject
`test(quickfiler): run breadcrumb thread-affinity worker tests on a dedicated thread (#900)`.

### Anchored diff against the commit

`git diff --exit-code HEAD -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
exited 0. The observation was made by chaining the following `git rev-parse HEAD` with `&&`, so the
second span ran only because the diff span exited 0. The committed state therefore equals the
working file exactly.

### Paths in the commit

    QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/commit-message-fix.txt
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p0-t11-phase0-commit.2026-09-17T02-18.md
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/regression-testing/fail-before-exception.2026-09-17T02-19.md
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/regression-testing/p1-t1-token-census-before.2026-09-17T02-19.md
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/regression-testing/p2-t1-token-census-after-edit.2026-09-17T02-21.md
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/regression-testing/p2-t2-csharpier-scoped.2026-09-17T02-21.md
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/regression-testing/p2-t3-build-after-fix.2026-09-17T02-22.md
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/regression-testing/p2-t4-pair-run-before-mutation.2026-09-17T02-22.md
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/plan.2026-09-16T23-27.md
    docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/spec.md

Eleven paths. Exactly one lies outside
`docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/`, and it
is the Write Set file. No production file and no other test file is present.

## Acceptance

All four conditions hold.

- `COMMIT-EXIT: 0`.
- `EXIT_CODE: 0` for the anchored diff, proving the committed state equals the working file.
- The `git show` list contains `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`
  and no other path outside the feature folder.
- `FIX-HASH:` equals the `HASH-AFTER:` value recorded by P2-T2,
  `8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164`. Both are the `Hash` property
  of `Get-FileHash -Algorithm SHA256 -LiteralPath`, so the comparison is between values produced the
  same way.

## Why this commit precedes the mutations

`FIX-HASH:` and `FIX-HEAD:` are the anchors every Phase 3 revert check compares against. Committing
the fix before the mutations is what makes those checks anchored rather than self-referential: after
each revert, `git diff --exit-code HEAD -- <the test file>` must exit 0, the scoped porcelain span
must print nothing, and the file's SHA-256 must equal `FIX-HASH:`. If a manual removal ever leaves a
residual difference, the single authorized recovery is
`git checkout -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, which restores
exactly this committed state and nothing else, because that is the only path named.

## Gate note

This commit stages a path outside every exempt tree, so the issue #539 orchestration-bookkeeping
exemption does not apply and the pre-implementation gate consulted the checkpoint that P0-T3
recorded as ready. Neither the `git add` span nor the `git commit` span was refused;
`PRE-IMPLEMENTATION GATE BLOCKED` was not reached.

No attribution trailer is present in the message file, for the reason recorded in P0-T11: the gate
rejects any command line containing an angle bracket, a valid trailer requires them, and a
bracket-free substitute would not be valid trailer syntax. The commit message names no gated tool.
