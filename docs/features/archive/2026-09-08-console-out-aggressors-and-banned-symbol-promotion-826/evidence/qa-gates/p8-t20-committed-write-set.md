# Confirming write-set gate over the committed tree (issue #826, [P8-T20])

Timestamp: 2026-09-09T19-51

Command, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
git diff --name-only $Base HEAD -- . ":(exclude).claude"
git status --porcelain --untracked-files=all -- . ":(exclude).claude"
```

The first span is anchored on the base commit **and** on `HEAD`, so it enumerates what was actually
committed rather than what is merely present in the working tree. That is the difference between this
gate and [P8-T1], which measured the working tree before the commit existed.

EXIT_CODE: 0

## Committed footprint

| Measure | Value |
|---|---|
| paths in `git diff --name-only $Base HEAD` | 85 |
| write-set paths declared by the spec | 38 |
| write-set paths **missing** from the commit | **0** |
| committed paths **outside** the allow-list | **0** |

The allow-list is the 38 write-set paths plus `<FEATURE>/spec.md`,
`<FEATURE>/plan.2026-09-08T23-52.md` and any path beginning `<FEATURE>/evidence/`.

Both directions hold, which is what the acceptance condition requires: every path listed is inside the
[P8-T1] allow-list, **and** the listed set includes all 38 write-set paths. The change is therefore
neither wider nor narrower than the spec's write set.

The 85 committed paths decompose as 38 write-set paths, `spec.md`, `plan.2026-09-08T23-52.md` and 45
evidence artifacts under `<FEATURE>/evidence/`.

Every named out-of-scope path remains absent from the committed set, as it was from the working-tree set
[P8-T1] measured: `CLAUDE.md`, `<FEATURE>/issue.md`, anything under `<FEATURE>/research/`, anything under
`.claude/rules/`, `.github/instructions/` or `docs/features/epics/`, and the sibling-owned
`UtilitiesCS/Threading/TimeOutTask.cs` and
`QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`. None of them is in the 85.

## Porcelain span — recorded verbatim, not asserted empty

```
 M docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/plan.2026-09-08T23-52.md
?? docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/qa-gates/p8-t19-final-git-state.md
```

Two entries, and both are the residual [P8-T19] left behind rather than anything this task has yet
written. At the moment this block ran, [P8-T19]'s artifact `p8-t19-final-git-state.md` was untracked and
this plan file carried [P8-T19]'s check-off, while `p8-t20-committed-write-set.md` and this task's own
check-off did not exist yet and therefore do not appear.

Every entry is either this plan file or a path under `<FEATURE>/evidence/`, as the acceptance condition
requires. [P8-T21] clears both, together with its own artifact and check-off, and carries the terminal
clean-tree gate.

Output Summary: the committed footprint is 85 paths, containing all 38 write-set paths and nothing
outside the allow-list in either direction. AC16 is confirmed over the committed tree.
