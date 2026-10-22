# Base-Ref Anchor — Remediation Cycle 1

- Timestamp: 2026-09-02T01-02
- Issue: #678
- Task: [P0-T2]
- Branch: `bug/quickfiler-carry-folder-predictor-to-item-controller-678`

## Command: git rev-parse HEAD

```
git rev-parse HEAD
```

EXIT_CODE: 0

Output, verbatim:

```
4b43e31d042da2b3f670d131bc225fdb30972069
```

## Command: git merge-base

```
git merge-base 807fb0bb6e5e49f43efa6b256b05960bf078ca19 HEAD
```

EXIT_CODE: 0

Output, verbatim:

```
807fb0bb6e5e49f43efa6b256b05960bf078ca19
```

## Anchors this cycle uses

- `R_BASE_SHA` = `807fb0bb6e5e49f43efa6b256b05960bf078ca19`. The merge-base output equals
  this literal exactly, so the branch has not diverged from the recorded base and no
  re-anchoring is required.
- `R_HEAD_AT_CYCLE_START` = `4b43e31d042da2b3f670d131bc225fdb30972069`. Several tasks in
  this plan (P2-T7 second D5 run, P2-T9) name "the HEAD SHA that P0-T2 recorded" as their
  ref operand rather than the base SHA, because the issue #678 fix commits and two artifact
  commits sit between the base SHA and this HEAD. A base-anchored diff at those tasks would
  report the previous cycle's work rather than this cycle's.

## Ref-name rule

Every anchored diff in this plan uses one of the two literal SHAs above. The ref name
`origin/main` is never written into a git command in this cycle: MSYS path conversion
mangles it under the bash tool, and a concurrent fetch can advance it mid-run. `origin/main`
was re-fetched at the start of this run and resolved to
`807fb0bb6e5e49f43efa6b256b05960bf078ca19`, identical to the recorded base SHA.

## Output Summary

`git rev-parse HEAD` = `4b43e31d042da2b3f670d131bc225fdb30972069`.
`git merge-base 807fb0bb6e5e49f43efa6b256b05960bf078ca19 HEAD` =
`807fb0bb6e5e49f43efa6b256b05960bf078ca19`, which equals the literal base SHA. No
divergence. Anchoring proceeds as planned.
