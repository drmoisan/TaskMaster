# P8-T8 — Host-token sanitisation sweep

Timestamp: 2026-09-08T10-42
Task: [P8-T8]
Command: pwsh -NoProfile -File coverage/plan811-helper.ps1 — `Select-String` over every file under `<FEATURE>/evidence` for the run-time-derived account and machine tokens and the drive-rooted profile-path shape, plus a name scan over the same tree
EXIT_CODE: 0

The account and machine tokens are derived at run time
(`[regex]::Escape((Split-Path -Leaf $env:USERPROFILE))` and
`[regex]::Escape($env:COMPUTERNAME)`) and their values are never written into this artifact.

## Counts

| Check | Count | Verdict |
|---|---|---|
| Files scanned under `<FEATURE>/evidence` | 40 | — |
| Content matches for the account token (case-insensitive) | 0 | OK |
| Content matches for the machine token (case-insensitive) | 0 | OK |
| Content matches for the drive-rooted profile-path shape | 0 | OK |
| File or directory names containing either token | 0 | OK |

All four final counts are 0.

## Substitutions performed

None. No file required rewriting, so the per-file substitution count is 0 for every file. This is
the observation, not the exit code: the rewrite step exits 0 whether or not it changes anything,
so the counts above are what establish the result.

Two artifacts quote redacted stack traces that originally contained absolute worktree paths, and
both were redacted at authoring time rather than by this sweep:

- `evidence/regression-testing/p2-t4-ac2-fail-before.md` — the `NullReferenceException` trace, with
  the worktree path written as `<worktree>`.
- `evidence/regression-testing/p8-t4-ac4-runs.md` — the `MethodBodyReader_Tests` failure trace,
  likewise written as `<worktree>`.

`evidence/baseline/p0-t5-tool-resolution.md` writes the Visual Studio installation root as
`<vs-install>` and the user profile root as `<user>` for the same reason.

## Coverage of this sweep

The sweep covers every artifact produced by P0-T1 through P8-T7, which is the 40 files counted
above. The artifacts produced by P8-T9 through P8-T11 are written after this task from plan text
only and are re-counted by P8-T12, which re-runs these same four checks over `$Feature` and
`docs/features/potential/`.

## Acceptance evaluation

- All four final counts are 0. PASS
- The artifact never writes either token's value. PASS
- The sweep covers every artifact produced by P0-T1 through P8-T7, and P8-T12 re-counts the later
  ones. PASS

## Output Summary

40 evidence files scanned. No account name, machine name, drive-rooted profile path, or offending
file name found. No substitution was required.
