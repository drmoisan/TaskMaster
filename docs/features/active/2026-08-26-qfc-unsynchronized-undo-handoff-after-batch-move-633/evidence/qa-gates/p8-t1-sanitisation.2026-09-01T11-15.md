# Evidence sanitisation before the final commit (P8-T1)

Timestamp: 2026-09-01T11-15
Task: [P8-T1]
Working directory: WORKTREE

Command: `pwsh -NoProfile -File <scratchpad>/sanitise.ps1`
EXIT_CODE: 0

The sweep runs over every file under `FEATURE/evidence/`. It replaces, case-insensitively, all three
spellings of the absolute worktree path that occur in this evidence set — the backslash form, the
forward-slash form, and the doubled-backslash form — with the literal token `WORKTREE`. Each spelling is
built at run time from a character code rather than written as a literal in the helper, so the helper
does not itself become a match on a later sweep. The helper lives in the system scratchpad outside the
repository and adds no file to the change footprint.

Sanitisation is required because the eight TRX trees and the eight `.msbuild.txt` logs this plan commits
both embed this machine's full user-profile path, and repository artifact hygiene prohibits an absolute
host path in a committed artifact.

## Counts after the sweep

| Measure | Count |
|---|---|
| Remaining matches of the absolute worktree path, any spelling, case-insensitive | **0** |
| Files under `FEATURE/evidence/` whose file name contains the account token | **0** |
| Directories under `FEATURE/evidence/` whose directory name contains the account token | **0** |

All three recorded counts are 0.

Files scanned: 61. Files rewritten on the first pass: 10. A confirming second pass rewrote 0 files and
again reported 0 remaining matches and 0 for both account-token counts, so the sweep reached a fixed
point.

No matched path is quoted in this artifact, and the account token is not written here. Quoting either
would make this artifact a match on the next sweep.

## Directory-name check

The account-token directory-name count was already 0 on entry to this task, so no removal was required
on this pass. The one such directory that did appear during execution — an empty
`vstest.console.exe` per-run deployment directory inside the P2-T5 results directory, named after the
account and the host — was removed by the P2-T7 sweep and is recorded there. The removal is guarded: the
sweep deletes an account-token-named directory only when it contains no files at any depth, so a
directory holding evidence would be reported rather than silently discarded.

The check is applied to directory names as well as file names for the reason P2-T7 records: content
sanitisation cannot reach a name, and a directory whose name carries the account token puts that token
into a committed path just as a file name does.

Output Summary: The committed evidence tree carries no absolute host path in any file's content, no
account token in any file name, and no account token in any directory name.
