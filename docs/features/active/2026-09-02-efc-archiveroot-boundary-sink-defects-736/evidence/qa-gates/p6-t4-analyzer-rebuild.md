# P6-T4 — Analyzer gate (post-change)

Timestamp: 2026-09-04T01-44

Command:

```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:LogFile=coverage\p6-t4-analyzer.detailed.log;Verbosity=detailed" /fl1 "/flp1:LogFile=docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\qa-gates\p6-t4-analyzer.min.log.txt;Verbosity=minimal"
@(Select-String -LiteralPath coverage\p6-t4-analyzer.detailed.log -SimpleMatch -CaseSensitive -Pattern 'Skipping target "CoreCompile"').Count
@(Select-String -LiteralPath coverage\p6-t4-analyzer.detailed.log -SimpleMatch -CaseSensitive -Pattern 'Task "Csc"').Count
git add -N docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t4-analyzer.min.log.txt
git ls-files -- docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t4-analyzer.min.log.txt
```

EXIT_CODE: 0

**This artifact records the second execution of P6-T4**, run after the toolchain-loop restart that
P6-T13 caused. The figures below supersede the first execution's and were measured on a fresh
`/t:Rebuild`, not carried forward.

## Non-vacuity observations

Read from the detailed-verbosity log, case-sensitively:

| Literal | Count | Required |
|---|---|---|
| `Skipping target "CoreCompile"` | **0** | 0 |
| `Task "Csc"` | **18** | at least 1 |

A warm `/t:Build` would have skipped `CoreCompile` on every project and run no analyzers at all; the
zero-skip count together with 18 compiler invocations is what establishes that this gate actually
compiled the tree. `/t:Rebuild` is used here rather than CI's `/t:Build` for the reason D1 records:
MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, and this worktree is
warm while a CI runner checkout is always cold.

## Detailed log

- Repository-relative path: `coverage/p6-t4-analyzer.detailed.log`
- Byte size: **10591793**
- SHA-256: `DBD43FBDF212592FE451C586351A58D398A04B58848B9CF3F987B4B2EFF404DB`

The detailed log is written under the gitignored `coverage` directory (`.gitignore` line 144 is
`coverage/*`) and is deliberately **not** committed: a detailed log of a 19-project rebuild is over
10 MB, and a prior session committed an 81 MB coverage artifact under `docs/features` which must not
recur.

## Minimal log — two separate observations, both required

1. **Exists on disk** at
   `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t4-analyzer.min.log.txt` —
   yes.
2. **Tracked by git.** `git add -N` on that path exited **0**, and the following
   `git ls-files --` span printed exactly that path:

```
docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t4-analyzer.min.log.txt
```

Neither observation substitutes for the other. A file matched by a `.gitignore` pattern sits on disk
exactly as a compliant one does, so an existence check passes for it, yet no `git add -A` in this
plan would stage it and it would never reach the delivery commit — AC13's retention conjunct would
fail while an existence-only gate recorded a pass. D9 records why this file's name ends `.log.txt`
and not `.log`: `.gitignore` line 84 is the bare pattern `*.log`, and `.gitignore` is not in the
ratified Write Set, so un-ignoring it is unavailable here. The `add` is deliberately `-N` and
deliberately not `-f`: an intent-to-add records the path in the index without staging content, and
an un-forced add of an ignored path exits non-zero rather than force-tracking it past the gate.

## Warning count

Read from the `N Warning(s)` summary line msbuild prints on the console at default verbosity, which
is where that figure lives — the retained minimal log carries no warning summary and no warning
lines, and the detailed log that does carry it is deliberately not committed.

```
    0 Warning(s)
    0 Error(s)
```

The P0-T4 baseline artifact records a warning count of **0**. This gate's count is **0**, which is
no greater than the baseline, so this item's edits introduced no new analyzer warning. A baseline of
0 makes this a zero-new-analyzer-warning budget.

Output Summary: analyzer rebuild exited 0 with `0 Warning(s)` and `0 Error(s)`, against a P0-T4
baseline of 0 warnings. Non-vacuity proven: 0 occurrences of `Skipping target "CoreCompile"` and 18
of `Task "Csc"` in the detailed log. The minimal log both exists at its `.log.txt` evidence path and
is tracked by git, the `git add -N` step having exited 0.
