# Phase 0 — CSharpier formatting baseline

Timestamp: 2026-09-07T00-50
Task: [P0-T5]
Issue: #798

Host-specific absolute paths are redacted to a `<worktree>` token.

## Command

Command: `dotnet tool run csharpier check .`, executed with the working directory set to
`<worktree>` and invoked through the repo-local SDK so the manifest-pinned CSharpier 1.2.6 is used.
EXIT_CODE: 0

## Full summary line printed by CSharpier 1.2.6

Checked 1587 files in 7826ms.

The line begins with the literal `Checked ` and ends with the literal `ms.`, which is the
success-case shape the later csharpier gates in this plan assert.

## Unformatted files at baseline

None. The exit code is 0, so CSharpier reported no file as unformatted and there is no pre-existing
formatting drift to enumerate. `UtilitiesCS/Extensions/DfDeedle.cs`, the file this change relocates
methods out of, is included in the 1587 files checked and is formatted at the base commit.

## Consequence for P7-T2

P7-T2 carries an adjudication branch that applies only when this task records a non-zero exit and
enumerates pre-existing unformatted files. This task recorded `EXIT_CODE: 0` and enumerated none, so
that branch does not apply. Under P7-T2's own wording, no extra path can be added to the write-set
diff by any of the whole-tree csharpier passes mandated by P1-T12, P3-T6, P4-T4, P5-T4, P6-T5,
P7-T8 and P8-T1, and any extra path observed at P7-T2 is a defect in this change rather than
pre-existing drift.

Output Summary: `dotnet tool run csharpier check .` exited 0 over 1587 files with the summary line
"Checked 1587 files in 7826ms." The tree is fully formatted at the base commit and there is no
pre-existing unformatted-file set. The P7-T2 adjudication branch is inactive.
