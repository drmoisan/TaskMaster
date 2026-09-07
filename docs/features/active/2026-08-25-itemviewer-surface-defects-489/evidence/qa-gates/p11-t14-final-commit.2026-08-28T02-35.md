# P11-T14 — Commit of every artifact and source change produced in Phase 11

Timestamp: 2026-08-28T02-35
Command: git add -- docs/features/active/itemviewer-surface-defects-489/ && git commit
EXIT_CODE: 0

## Resulting commit SHAs

Phase 11 is committed in **three** commits on `bug/itemviewer-surface-defects-489`:

| # | SHA | Contents |
|---|---|---|
| A | `ac4a996a109c01ad927c451cf4188be746ff929e` | P11-T1 through P11-T7: the format-form selection, the format pass, the read-only check, the analyzer build and its `.msbuild.txt` log, the non-vacuity proof, the nullable build, the scoped vstest artifact and `p11-t7.trx`, plus the P11-T1..T7 plan check-offs |
| B | `7f219eb7c059adea5fd17025063ae3e1beac2f80` | P11-T8 through P11-T13: the repo-wide coverage artifact, the new-member coverage figure, the exclusion-attribute recount, the final line-count audit, the unfiltered guard artifact and `p11-t12.trx`, the toolchain-loop history, plus the P11-T8..T13 plan check-offs |
| C | terminal commit, SHA reported by the executor | this artifact, the P11-T15 clean-tree artifact, and the P11-T14 and P11-T15 plan check-offs |

Commit B is the commit this task's command produced and is the one recorded as its immediate result:
`7f219eb7c059adea5fd17025063ae3e1beac2f80`.

Two earlier commits rather than one were made deliberately, per the instruction to commit after each
gate completes: the entire phase was not held in the working tree across the two full solution
rebuilds and the three test runs.

**Why commit C's SHA is not printed inside this file.** A commit cannot record its own hash. This
artifact and the P11-T15 artifact are themselves Phase 11 outputs, and the last two plan check-offs
post-date the text that would have to name them, so the terminal commit necessarily carries content
that cannot know its own SHA. The SHA of commit C is therefore reported by the executor in its
completion report rather than asserted here. This is stated rather than worked around, so that a
later audit does not read the absence as an omission.

## Scope of what was committed

Every commit uses the explicit pathspec `docs/features/active/itemviewer-surface-defects-489/`.
`.claude/agent-memory/` is tracked rather than gitignored and is deliberately outside every pathspec
in this plan, per § Execution conventions.

**No source change was produced in Phase 11 to commit.** The format pass rewrote no file — P11-T2
measured 0 of 1868 hashed files with a changed SHA-256 — and no other stage in this phase edits
tracked source. `git status --porcelain` over the full nineteen-directory C# project set plus
`scripts/` and `coverage/` returned **zero** lines both before and after these commits. Phase 11 is
therefore an evidence-only phase, which is the expected shape for a final QC loop that passes on its
first iteration.

`coverage/coverage.cobertura.xml` is not committed and is not an evidence artifact: `coverage/` is
gitignored by a directory rule, deliberately, as § Execution conventions records.

## Artifacts committed by this phase

Twelve markdown artifacts, one MSBuild file log and two TRX files, all under
`FEATURE/evidence/qa-gates/`:

```
p11-t1-format-form-selection.2026-08-28T02-11.md
p11-t2-csharpier-format.2026-08-28T02-14.md
p11-t3-csharpier-check.2026-08-28T02-16.md
p11-t4-analyzer-build.2026-08-28T02-17.msbuild.txt
p11-t4-analyzer-build.2026-08-28T02-18.md
p11-t5-analyzer-nonvacuity.2026-08-28T02-19.md
p11-t6-nullable-build.2026-08-28T02-20.md
p11-t7-vstest-quickfiler.2026-08-28T02-22.md
p11-t7.trx
p11-t8-repo-coverage.2026-08-28T02-28.md
p11-t9-new-member-coverage.2026-08-28T02-29.md
p11-t10-excludefromcodecoverage-count.2026-08-28T02-30.md
p11-t11-final-line-counts.2026-08-28T02-31.md
p11-t12-noliveform-guard.2026-08-28T02-33.md
p11-t12.trx
p11-t13-toolchain-loop.2026-08-28T02-34.md
p11-t14-final-commit.2026-08-28T02-35.md          (commit C)
p11-t15-clean-tree.<timestamp>.md                  (commit C)
```

The analyzer log carries the `.msbuild.txt` extension and not `.log`, so `.gitignore:84` (`*.log`)
does not match it; `git check-ignore` confirmed it is committable before it was added. Both TRX files
were sanitised and re-parsed before being committed.

## Acceptance

**`EXIT_CODE: 0`.** Both `git add` and `git commit` exited `0` for every commit. `git rev-parse HEAD`
after this task's commit returns `7f219eb7c059adea5fd17025063ae3e1beac2f80`.

Output Summary: Every Phase 11 artifact is committed on `bug/itemviewer-surface-defects-489`.
`EXIT_CODE: 0`. This task's commit is **`7f219eb7c059adea5fd17025063ae3e1beac2f80`**, preceded by
`ac4a996a109c01ad927c451cf4188be746ff929e` for the P11-T1..T7 gates and followed by a terminal commit
carrying this artifact, the P11-T15 artifact and the last two check-offs, whose SHA is reported by
the executor because a commit cannot record its own hash. No source change was produced in Phase 11
to commit: the format pass rewrote nothing, and the porcelain over the full C# project set plus
`scripts/` and `coverage/` was empty throughout. All commits used the explicit feature-folder
pathspec, leaving the tracked `.claude/agent-memory/` outside every pathspec as § Execution
conventions requires.
