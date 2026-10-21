# Sanitisation-Record Hygiene Gate — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-46
- Task: `[P1-T5]`

Seven separate File-mode invocations were made, each supplying exactly one `-Path` operand. Passing
several space-separated operands to one `pwsh -File` invocation does not bind them into an array
parameter and would silently scan only the first, so a per-path invocation is used to make that
failure mode structurally impossible.

Command:

1. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/phase0-instructions-read.2026-09-03T12-23.md`
2. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-helper-bootstrap.2026-09-03T12-23.md`
3. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-mergebase.2026-09-03T12-23.md`
4. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-baseline.2026-09-03T12-23.md`
5. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-line5-baseline.2026-09-03T12-23.md`
6. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-secondary-sanitisation.2026-09-03T12-23.md`
7. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-squash-merge-note.2026-09-03T12-23.md`

EXIT_CODE:

1. `0`
2. `0`
3. `0`
4. `0`
5. `0`
6. `0`
7. `0`

Output Summary:

The seven `FILECOUNT:` lines, reproduced verbatim, one per invocation:

```
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/phase0-instructions-read.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-helper-bootstrap.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-mergebase.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-baseline.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-line5-baseline.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-secondary-sanitisation.2026-09-03T12-23.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-squash-merge-note.2026-09-03T12-23.md | COUNT: 0
```

Acceptance checks:

- Seven invocations were made, one per path.
- Each printed exactly one `FILECOUNT:` line, so seven `FILECOUNT:` lines were produced in total.
- Every one reports `COUNT: 0`, and no `FILEMATCH:` line was printed by any invocation. No repair was
  required and no invocation was re-run.
- This artifact carries all four schema fields and all seven `FILECOUNT:` lines.

This artifact records only repo-relative paths and counts, so it does not quote a removed value.
