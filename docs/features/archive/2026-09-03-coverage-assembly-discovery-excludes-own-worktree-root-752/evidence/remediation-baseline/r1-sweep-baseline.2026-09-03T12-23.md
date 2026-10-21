# Branch-Scoped Added-Line Sweep — Baseline (fail-before) — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-41
- Task: `[P0-T4]`

Command: `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode Diff -BaseSha 87233f867ad60c0a5c0d19b09cc121ae536d7ba1`

EXIT_CODE: `0`

Output Summary:

Full stdout of the run, verbatim:

```
MATCHFILE: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md | COUNT: 2
MATCHFILE: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md | COUNT: 1
MATCHFILE: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md | COUNT: 1
MATCHFILE: docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md | COUNT: 2
TOKENCOUNT: account | COUNT: 1
TOKENCOUNT: parentdir | COUNT: 1
TOKENCOUNT: winprofile | COUNT: 6
TOKENCOUNT: winprofilefs | COUNT: 0
TOKENCOUNT: posixprofile | COUNT: 0
TOTAL: 6
```

- `TOTAL: 6`
- `TOKENCOUNT: account | COUNT: 1`
- `TOKENCOUNT: parentdir | COUNT: 1`
- `TOKENCOUNT: winprofile | COUNT: 6`
- `TOKENCOUNT: winprofilefs | COUNT: 0`
- `TOKENCOUNT: posixprofile | COUNT: 0`

`MATCHFILE:` enumeration, recorded verbatim as the list that drives `[P1-T2]`:

1. `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md` | COUNT: 2
2. `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md` | COUNT: 1
3. `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md` | COUNT: 1
4. `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md` | COUNT: 2

Acceptance checks:

- `EXIT_CODE: 0` — satisfied.
- A `TOTAL:` line is present, so the run completed rather than terminating early.
- `TOKENCOUNT: account` is 1, which is at least 1; `TOKENCOUNT: parentdir` is 1, which is at least 1.
- The `MATCHFILE:` list includes
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`.
  That entry is the fail-before proof for R-1: the finding's cited file carries a matching added line
  in the committed branch diff before any Phase 1 edit is made.
- Every `MATCHFILE:` path ends in `.md` and begins with `docs/features/`. No non-markdown path and no
  path outside `docs/features/` appears, so the `[P0-T4]` stop-and-report condition is not triggered
  and this plan's write set is sufficient to reach the terminal zero-match gate in `[P2-T5]`.

`TABLE_RECONCILIATION:` The execution-time enumeration agrees with the planning-time table in the
plan's "Why this plan is larger than the single mandated line edit" section. Both identify the same
four files and the same total of six token-carrying added-line positions, distributed identically:
`research/research-findings.2026-09-03T00-00.md` 1, `spec.md` 1, `issue.md` 2, and the promoted
pre-promotion copy 2. No divergence is recorded. The enumeration above nonetheless governs `[P1-T2]`.

This artifact records only paths, token class names, line counts, and commit SHAs. It reproduces no
matched text, so it does not quote a removed value.
