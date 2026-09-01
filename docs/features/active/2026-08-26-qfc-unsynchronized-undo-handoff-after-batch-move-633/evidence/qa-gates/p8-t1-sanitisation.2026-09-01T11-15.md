# Evidence sanitisation before the final commit (P8-T1)

Timestamp: 2026-09-01T11-15
Task: [P8-T1]
Working directory: WORKTREE

> **Superseded in part.** The Output Summary of this record was wrong as written. See the
> "Correction (2026-09-01T11-47)" section at the end of this file before relying on any statement here.
> The record of what this pass actually did is retained unchanged.

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

## Correction (2026-09-01T11-47)

The Output Summary above is wrong as written, and the section that precedes it is narrower than its own
phrasing suggests. This correction states the defect plainly and leaves the original record intact.

**What the pass actually covered.** The sweep recorded above substituted the three spellings of one
value only: this worktree's own absolute path. Its remaining-match count of 0 was measured against that
one value. Within that scope the count was correct, and the file-name and directory-name counts were
correct as well.

**What the pass did not cover.** Every other absolute host path and every bare host identifier in the
evidence set was left in place, because none of them is a spelling of the worktree path and none was in
the pattern set. Three classes survived the pass and were still present when the correction was made:

| Class of surviving token | Files affected |
|---|---|
| Analyzer-configuration path into the main checkout, in the `.msbuild.txt` logs | 8 |
| Run-identity attributes naming the account and the machine, in the `.trx` files | 8 |
| A note in the plan file defining the `WORKTREE` constant by its literal absolute value | 1 |

The third class was introduced during this plan's own execution: the note that documents the `WORKTREE`
substitution wrote the substituted value out in full, so the act of recording the sanitisation reinstated
the identifier in a different file.

**Why the summary was wrong.** The claim "carries no absolute host path in any file's content" is a
statement about all absolute host paths. The measurement that supported it ranged over one path. A count
of 0 remaining matches for the swept value does not license a claim of 0 for the unswept ones, and
restating a scoped measurement in unscoped language is the specific error made here.

**Corrected status.** The surviving tokens were removed by a corrective sweep over the branch's changed
file set. That sweep, its command, its exit code, and its post-sweep counts are recorded in
`p8-t1-sanitisation-correction.2026-09-01T11-47.md` in this directory. No raw pre-substitution value is
quoted in either artifact; each substituted token is described by class only, because quoting a removed
identifier would write it back into a committed file.
