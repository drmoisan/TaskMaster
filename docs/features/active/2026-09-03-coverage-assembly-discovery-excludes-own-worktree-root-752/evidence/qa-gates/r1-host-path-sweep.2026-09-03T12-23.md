# Staged-Index Host-Path Sweep — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-48
- Task: `[P2-T2]`

Command:

1. `git -C <repo-root> add -- <explicit pathspec list>` — the research file, the three files changed
   by `[P1-T2]`, this remediation's plan file, and the feature folder's `evidence/` tree. Neither
   `git add -A` nor `git add .` was used, so no untracked audit artifact of this loop and no file
   under `.claude/agent-memory/` was swept in. No path under `.claude/agent-memory/` appears in the
   pathspec list.
2. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode Index -BaseSha 87233f867ad60c0a5c0d19b09cc121ae536d7ba1`

EXIT_CODE:

1. `0`
2. `0`

Note on command 1: git emitted its standard line-ending normalisation advisories
(`LF will be replaced by CRLF the next time Git touches it`) for each staged markdown file. Those are
advisories from the repository's configured `autocrlf` behaviour, not errors; the command exited `0`.

Staged set, from `git -C <repo-root> diff --cached --name-status` (14 paths, all `.md` under `docs/`):

```
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-secondary-sanitisation.2026-09-03T12-23.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-squash-merge-note.2026-09-03T12-23.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-artifact-hygiene.2026-09-03T12-23.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-changed-file-class.2026-09-03T12-23.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/phase0-instructions-read.2026-09-03T12-23.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-line5-baseline.2026-09-03T12-23.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-mergebase.2026-09-03T12-23.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-baseline.2026-09-03T12-23.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-helper-bootstrap.2026-09-03T12-23.md
M	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md
A	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md
M	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md
M	docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md
M	docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md
```

Output Summary:

Full stdout of the Index-mode sweep, verbatim:

```
TOKENCOUNT: account | COUNT: 0
TOKENCOUNT: parentdir | COUNT: 0
TOKENCOUNT: winprofile | COUNT: 0
TOKENCOUNT: winprofilefs | COUNT: 0
TOKENCOUNT: posixprofile | COUNT: 0
TOTAL: 0
```

- `TOTAL: 0`
- `TOKENCOUNT: account | COUNT: 0`
- `TOKENCOUNT: parentdir | COUNT: 0`
- `TOKENCOUNT: winprofile | COUNT: 0`
- `TOKENCOUNT: winprofilefs | COUNT: 0`
- `TOKENCOUNT: posixprofile | COUNT: 0`
- Remaining `MATCHFILE:` lines: none. The sweep printed no `MATCHFILE:` line at all.

Acceptance checks:

- A `TOTAL:` line is present, so the run completed rather than terminating early.
- `TOKENCOUNT: account` is `COUNT: 0`.
- `TOKENCOUNT: parentdir` is `COUNT: 0`.
- No remaining `MATCHFILE:` entry names a `.md` file under `docs/features/`, and no `MATCHFILE:`
  entry of any other kind appears, so the stop-and-report BLOCKED condition is not triggered.

This artifact records only repo-relative paths, token class names, counts, and a commit SHA. It
reproduces no matched text, so it does not quote a removed value.

## Post-artifact scan

The sweep recorded above is re-run twice in `[P2-T5]`:

- **`[P2-T5]` step 3**, in `Index` mode, comparing the merge base against the staged index — that is,
  against the tree the step-7 commit creates.
- **`[P2-T5]` step 8**, in `Diff` mode over `<MERGE_BASE>..HEAD`, after the step-7 commit.

The input to those re-runs differs from the run recorded above only by this artifact itself, by the
checkbox characters of the plan file, and — for step 8 only — by the final-gate artifact that
`[P2-T5]` step 5 writes, which `[P2-T5]` step 6 proves token-free before it is committed.

This artifact and the plan file are the two files carrying the remaining File-mode hygiene obligation
at this point. This artifact is the only file whose content enters the `[P2-T4]` commit without
having been present in the `[P2-T2]` staged snapshot; the plan file was staged by `[P2-T2]` but is
modified again by `[P2-T5]` step 1, so it is re-scanned here as well.

Two separate File-mode invocations were made, one path each. Passing both paths to a single
invocation would scan only the first, silently, per the `[P0-T2]` invocation warning.

Command:

1. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-host-path-sweep.2026-09-03T12-23.md`
2. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md`

EXIT_CODE:

1. `0`
2. `0`

Result — the two `FILECOUNT:` lines, verbatim, one per invocation, with no `FILEMATCH:` line printed
by either:

```
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-host-path-sweep.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md | COUNT: 0
```
