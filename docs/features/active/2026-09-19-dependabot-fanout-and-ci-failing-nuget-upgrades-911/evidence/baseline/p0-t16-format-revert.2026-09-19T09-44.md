# P0-T16 — Revert of Out-of-Scope Formatter Rewrites

Timestamp: 2026-09-19T23-04

Command: CMD-REVERT-OUT-OF-SCOPE-FORMAT, `git checkout -- <derived-pathspec>`.

**Not run.** The derived pathspec is empty, and the Command Reference states that when the derived
set is empty the command is not run and the task records `REVERT-SET: empty`.

EXIT_CODE: 0

## Derivation of the revert pathspec

The pathspec is derived at run time and is never hard-coded. It is the set of paths whose SHA-256
changed across the P0-T15 format run, **minus** every member of the spec `## Write Set`.

| Derivation input | Value |
|---|---|
| Paths whose SHA-256 changed across the P0-T15 format run | none — the hash-difference list is empty, 0 of 32 files |
| Minus the spec `## Write Set` members under these folders (`scripts/vscode/Sync-PackageReferences.ps1`, `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`) | no effect on an empty set |
| **Derived pathspec** | **empty** |

```
REVERT-SET: empty
```

The P0-T15 hash-difference list the derivation consumed is the empty list. All 32 SHA-256 values
recorded before the format run are identical to the 32 recorded after it; the two full hash sets are
enumerated in `evidence/baseline/p0-t15-poshqc-format.2026-09-19T09-44.md` and are not duplicated
here.

## Pre-revert capture

```
git status --porcelain --untracked-files=all -- scripts/vscode
```

produced **no output**. The capture is empty.

## Post-revert capture

No revert was performed, because the derived set is empty. The same command was re-run to record the
state at the point the revert would have completed:

```
git status --porcelain --untracked-files=all -- scripts/vscode
```

produced **no output**. The capture is empty.

## Write Set members the formatter rewrote

**None.** `scripts/vscode/Sync-PackageReferences.ps1` is a Write Set member and was **not** rewritten
by the P0-T15 format run: its SHA-256 is
`FF7FE7F77E0D1F2272AD69ED5614F52772DB737EC8C2911B9283FEB82450345D` in both the before and the after
capture. It is therefore excluded from the revert trivially — there is nothing to revert — and it does
not appear in the post-revert capture, because it is unmodified rather than because it was reverted.

This is the point at which the plan expected a Write Set member to be carried forward modified.
Scope Decision 8 records that the file "sits modified from the P0-T15 format run onward" and is kept
rather than reverted so that P2-T8 can commit it. That premise is not met: the file is clean. The
divergence, its measured basis and its consequence for P2-T8's and P2-T9's assertions are recorded in
full in the P0-T15 artifact under "Discrepancy against the plan's Measured Tree Facts". Nothing is
adjusted here.

## Follow-up-issue candidates

The plan directs that each reverted path be recorded as a follow-up-issue candidate, to be carried
into the P8-T5 follow-up issue, together with the statement that those files remain unformatted on
`main` and that this change deliberately does not fix them.

**No path was reverted, so the candidate list is empty.** Recorded explicitly rather than omitted,
because an absent section and an empty section are not the same evidence.

The two files Scope Decision 8 named as the expected candidates —
`scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — are clean
under the PoshQC formatter's bundled settings and were not rewritten, so they are not candidates on
the basis of this run. The plan's Measured Tree Facts row that named them was measured with
`Invoke-Formatter` under PSScriptAnalyzer defaults, a different rule set, and under that rule set
they do drift. Both statements remain true of `main`: the two files are unformatted with respect to
PSScriptAnalyzer defaults, and this change deliberately does not fix them. They are recorded here as
follow-up-issue candidates on that basis, qualified by the rule set under which the drift is
observable, and carried to P8-T5 as such.

## Acceptance evaluation

- The derived set is recorded explicitly, including the empty case as `REVERT-SET: empty`. PASS.
- The post-revert capture lists no member of the derived set — the derived set is empty and the
  capture is empty. PASS.
- Every path appearing in the pre-revert capture and absent from the post-revert capture is a member
  of the derived set — the pre-revert capture is empty, so the set of disappearing paths is empty and
  the condition holds with no path to test. PASS.
- Any Write Set member the formatter rewrote is recorded as excluded from the revert and still listed
  in the post-revert capture — the formatter rewrote no Write Set member, which is recorded above
  with the measured hash equality that establishes it. PASS.
- An empty pre-revert capture is explicitly not a failure. Applied.
- The reverted paths are recorded as follow-up-issue candidates with the required sentence — the
  reverted set is empty and is recorded as empty; the two Scope Decision 8 candidates are carried
  forward qualified by the rule set that makes their drift observable. PASS.

Output Summary: the derived revert pathspec is empty — `REVERT-SET: empty` — because the P0-T15
PoshQC format run rewrote 0 of 32 files, so CMD-REVERT-OUT-OF-SCOPE-FORMAT was not run, as the
Command Reference directs for the empty case. Both the pre-revert and the post-revert
`git status --porcelain --untracked-files=all -- scripts/vscode` captures are empty. No Write Set
member was rewritten, so none was excluded from a revert;
`scripts/vscode/Sync-PackageReferences.ps1` is unmodified at hash
`FF7FE7F77E0D1F2272AD69ED5614F52772DB737EC8C2911B9283FEB82450345D`, which is the premise Scope
Decision 8 assumed otherwise and which is flagged in the P0-T15 artifact for the coordinator. The
follow-up-issue candidate list carries the two files Scope Decision 8 named, qualified by the rule
set under which their drift is observable, and carries no reverted path.
