# Baseline — Git context and `MERGE_BASE` resolution ([P0-T3])

Timestamp: 2026-08-10T22-35
Command: `git rev-parse --abbrev-ref HEAD`; `git rev-parse HEAD`; `git merge-base HEAD <candidate>`; `git show -s --format='%H %cI %s' <merge-base>`; `git status --porcelain`; `sha256sum CLAUDE.md .claude/rules/general-unit-test.md .claude/rules/quality-tiers.md`
EXIT_CODE: 0

## Current branch and HEAD

| Item | Value |
|---|---|
| Branch | `bug/csharp-toolchain-gate-fidelity-512` |
| `git rev-parse HEAD` | `a5e336e5ae3443d4197caf5f87036fae1d538f89` |
| HEAD subject | `docs(epic): seed epic-status.md projection for build-ci-coverage-gate-fidelity` |
| HEAD commit timestamp | `2026-08-10T22:24:11-04:00` |
| Worktree | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb` |

## `MERGE_BASE` resolution (per the `pr-base-branch-merge-base` procedure)

Candidates were enumerated from `git for-each-ref refs/heads refs/remotes`. The plausible candidate
set (the epic integration branch this feature branched from, the repository default branch, the
sibling epic-child branches, and the remote counterpart of this branch) was evaluated. The procedure
selects the candidate whose merge-base with `HEAD` has the **most recent** commit timestamp.

| Candidate branch | Merge-base SHA | Merge-base commit timestamp | Selected |
|---|---|---|---|
| `origin/epic/build-ci-coverage-gate-fidelity-integration` | `a5e336e5ae3443d4197caf5f87036fae1d538f89` | `2026-08-10T22:24:11-04:00` | **YES** |
| `origin/bug/csharp-toolchain-gate-fidelity-512` | `0a1e35efa1c948ebc24517b3e8a7dd30c02e01dc` | `2026-08-10T21:58:02-04:00` | no |
| `origin/bug/coverage-threshold-policy-reconciliation-494` | `c325fa9c6e1aa335c6c242bc4da138c210116dc6` | `2026-08-10T21:40:30-04:00` | no |
| `origin/main` | `a682c7a21a910800870e85c067086b448552caa4` | `2026-08-10T12:33:29-04:00` | no |

**`MERGE_BASE` = `a5e336e5ae3443d4197caf5f87036fae1d538f89`**
**Selected branch = `origin/epic/build-ci-coverage-gate-fidelity-integration`**
**Merge-base commit timestamp = `2026-08-10T22:24:11-04:00`**

No tie-breaker was required; the selected candidate's merge-base timestamp is strictly the maximum.

**Note on the HEAD relationship.** `MERGE_BASE` is identical to `HEAD` at the time of this capture,
because this branch carries no commits of its own yet. This is expected and is not a defect: every
diff gate in this plan is of the form `git diff <MERGE_BASE> -- <paths>`, which compares the
**working tree** against `MERGE_BASE` and therefore remains discriminating regardless of whether a
commit has been made. Per the plan's [P0-T3] acceptance, the HEAD SHA is **not** pinned as an
expectation anywhere else in the plan.

## `git status --porcelain` at capture time

```
 M docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/plan.2026-08-10T14-08.md
?? docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/evidence/baseline/phase0-feature-inputs-read.2026-08-10T22-32.md
?? docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/evidence/baseline/phase0-instructions-read.2026-08-10T22-30.md
```

The only modifications are this plan's own checkbox updates and the two Phase 0 evidence artifacts
already written by [P0-T1] and [P0-T2]. No source file is modified.

## SHA-256 of the protected files (pre-change fingerprints for AC9)

| File | SHA-256 |
|---|---|
| `CLAUDE.md` | `ed6ca760280cb5d2ed07d6771a7a0042487f920739f4517bf61d01234b8653e8` |
| `.claude/rules/general-unit-test.md` | `8c30af5a659b8bfa28195f77c90f712c8e0c2a6a6932c93ed143a475ee4f68b0` |
| `.claude/rules/quality-tiers.md` | `25c79d4380d208364534f75b234b2e4bdc35619e342301c02093fd0e8ec49654` |

`CLAUDE.md` is edited by this feature outside § UT2, so its hash is expected to change; the two
`.claude/rules/` hashes must be unchanged at [P5-T13]. The § UT2 section guard is [P3-T6].

## Output Summary

`MERGE_BASE` resolved to `a5e336e5ae3443d4197caf5f87036fae1d538f89` from
`origin/epic/build-ci-coverage-gate-fidelity-integration` (merge-base timestamp
`2026-08-10T22:24:11-04:00`), which is the maximum among all evaluated candidates. Every later diff
gate in this plan cites this artifact for that SHA. The working tree is otherwise clean of source
changes and the three protected-file fingerprints are recorded.
