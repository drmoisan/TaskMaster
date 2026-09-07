# P4-T3 — Analyzer gate (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T03-58
Task: [P4-T3]
LoopIteration: 1
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl "/flp:LogFile=docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates\rem1-p4-t3-analyzer-build.2026-08-28T03-57.msbuild.txt;Verbosity=normal"
EXIT_CODE: 0

FinalAnalyzerWarningCount: 5

Solution build with the **spaced** platform spelling. `/t:Rebuild`, never `/t:Build`. No
`/p:Nullable=enable`. No `.csproj` was touched by this remediation, so no analyzer entry changed and
the pre-existing repo-wide analyzer HintPath skew recorded as out-of-scope finding E1 remains cleared
for this worktree by its gitignored `packages/` directory.

## Result

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

`EXIT_CODE: 0`, zero errors.

### Warning count and how it was derived

| Figure | Value |
|---|---:|
| Raw `warning : ` occurrences in the log | 10 |
| **Deduplicated** distinct warnings — `FinalAnalyzerWarningCount:` | **5** |
| MSBuild's own end-of-build summary | `5 Warning(s)` |

MSBuild prints each warning twice: once inline as the project builds, and once again in the
end-of-build warning summary. The raw occurrence count of 10 is therefore double the true figure. The
deduplicated count is **5**, which agrees with MSBuild's own `5 Warning(s)` summary line, and 5 is the
figure recorded as `FinalAnalyzerWarningCount:`. This matches the derivation the P0-T5 baseline used,
so the two figures are like-for-like.

All five are the same pre-existing non-Roslyn advisory — the System.Reactive v7.0 `packages.config`
unsupported-scenario message — one per project that still carries a `packages.config`: `QuickFiler`,
`TaskMaster`, `ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`. **Zero Roslyn analyzer diagnostics**
were emitted, and none of the five is attributable to either file this remediation edited.

### Against the baseline

| | Baseline (P0-T5) | P4-T3 |
|---|---:|---:|
| Warning count (deduplicated) | 5 | **5** |
| Errors | 0 | 0 |
| `EXIT_CODE` | 0 | 0 |

`FinalAnalyzerWarningCount: 5` is **not greater than** `BaselineAnalyzerWarningCount: 5`. The
remediation introduced no new analyzer diagnostic.

## Log

`FEATURE/evidence/qa-gates/rem1-p4-t3-analyzer-build.2026-08-28T03-57.msbuild.txt`, 11775 lines,
normal verbosity. `.msbuild.txt` extension, not `.log` — `.gitignore:84` is `*.log`, so a `.log` file
under `FEATURE/evidence/` could never be committed. Non-vacuity for this log is measured and recorded
separately in P4-T4.

Sanitised in place, case-insensitively: worktree root to `<repo-root>` (13631 occurrences), main
checkout root to `<main-checkout-root>` (36), machine name to `<host>` (0), account name to `<user>`
(0). A post-sanitisation search for all four tokens, in long and 8.3 forms, returns **0** residual
occurrences. The only absolute paths remaining are `C:\Program Files` and `C:\Program Files (x86)`,
which carry no account or machine identity.

**A note on this task's log filename.** A first invocation wrote to a log named
`...2026-08-28T03-58.msbuild.txt`, a value that had not come from a `date -u` reading. That file was
deleted and the build re-run against a filename taken from an actual `date -u` reading of
`2026-08-28T03-57`. The command quoted above is the one that produced the recorded log.

## Acceptance

| P4-T3 condition | Result |
|---|---|
| `EXIT_CODE: 0` | **Yes** — observed `0` |
| `FinalAnalyzerWarningCount:` not greater than 5 | **Yes** — 5, equal to the baseline |

Output Summary: The analyzer gate **passes**. `msbuild TaskMaster.sln /t:Rebuild` with
`EnableNETAnalyzers` and `EnforceCodeStyleInBuild`, spaced platform, no nullable property, exited
**0** with `Build succeeded.`, `5 Warning(s)`, `0 Error(s)`. `FinalAnalyzerWarningCount: 5`,
deduplicated from 10 raw log occurrences (MSBuild prints each warning inline and again in its summary)
and agreeing with MSBuild's own `5 Warning(s)` line; that is **not greater than** the P0-T5 baseline
of 5. All five are the pre-existing `System.Reactive` `packages.config` advisory across `QuickFiler`,
`TaskMaster`, `ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`; **zero** Roslyn diagnostics, and none
attributable to the two edited files. The 11775-line log is sanitised with zero residual host tokens.
