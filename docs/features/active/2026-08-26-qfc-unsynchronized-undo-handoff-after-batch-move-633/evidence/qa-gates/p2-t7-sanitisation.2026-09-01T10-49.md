# Evidence sanitisation before the Phase 1 and Phase 2 commit (P2-T7)

Timestamp: 2026-09-01T10-49
Task: [P2-T7]
Working directory: WORKTREE

Command: `pwsh -NoProfile -File <scratchpad>/sanitise.ps1`
EXIT_CODE: 0

The sweep runs over every file under `FEATURE/evidence/`. It replaces, case-insensitively, all three
spellings of the absolute worktree path that occur in this evidence set — the backslash form, the
forward-slash form, and the doubled-backslash form — with the literal token `WORKTREE`. Each spelling is
constructed at run time from a character code rather than written as a literal in the helper, so the
helper itself does not become a match on a later sweep. The helper lives in the system scratchpad
outside the repository, so it adds no file to the change footprint.

Sanitisation is required because the TRX trees and the `.msbuild.txt` logs committed here embed this
machine's full user-profile path, and repository artifact hygiene prohibits an absolute host path in a
committed artifact.

## Counts after the sweep

| Measure | Count |
|---|---|
| Remaining matches of the absolute worktree path, any spelling, case-insensitive | **0** |
| Files under `FEATURE/evidence/` whose file name contains the account token | **0** |
| Directories under `FEATURE/evidence/` whose directory name contains the account token | **0** |

Files scanned: 25. Files rewritten on the first pass: 6. A confirming second pass rewrote 0 files and
found 0 remaining matches, so the sweep reached a fixed point.

No matched path is quoted in this artifact, and the account token is not written here. Quoting either
would make this artifact a match on the next sweep.

## One directory required removal, not rewriting

The first pass recorded an account-token directory-name count of 1. Content sanitisation cannot reach a
directory name, so a rewrite would not have cleared it.

The directory was an automatically generated `vstest.console.exe` per-run deployment directory created
inside the P2-T5 results directory `FEATURE/evidence/regression-testing/p2-t5`, named after the account
and the host. It contained **zero files**, so it held no evidence and its removal discarded nothing.

It was removed with `[System.IO.Directory]::Delete(path, true)`. The removal is guarded: the sweep
removes an account-token-named directory only when that directory contains no files at any depth, so a
directory that did hold evidence would be reported rather than silently deleted. The same guarded
removal is part of the P8-T1 sweep, because each of the remaining scoped runs produces one of these
directories.

After removal the directory-name count is 0, which is the value the acceptance condition requires.

Output Summary: The evidence tree carries no absolute host path in any file's content, no account token
in any file name, and no account token in any directory name. All three recorded counts are 0.
