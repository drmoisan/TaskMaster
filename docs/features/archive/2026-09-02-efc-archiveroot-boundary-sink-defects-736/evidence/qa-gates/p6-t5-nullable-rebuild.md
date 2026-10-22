# P6-T5 — Nullable gate (post-change)

Timestamp: 2026-09-04T01-50

Command:

```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fl "/flp:LogFile=coverage\p6-t5-nullable.detailed.log;Verbosity=detailed" /fl1 "/flp1:LogFile=docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\qa-gates\p6-t5-nullable.min.log.txt;Verbosity=minimal"
@(Select-String -LiteralPath coverage\p6-t5-nullable.detailed.log -SimpleMatch -CaseSensitive -Pattern 'Skipping target "CoreCompile"').Count
@(Select-String -LiteralPath coverage\p6-t5-nullable.detailed.log -SimpleMatch -CaseSensitive -Pattern 'Task "Csc"').Count
git add -N docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t5-nullable.min.log.txt
git ls-files -- docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t5-nullable.min.log.txt
```

EXIT_CODE: 0

**This artifact records the second execution of P6-T5**, run after the toolchain-loop restart that
P6-T13 caused. The figures below supersede the first execution's and were measured on a fresh
`/t:Rebuild`.

`/p:Nullable=enable` is deliberately absent, for the reason P0-T5 records: no project in this
repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property
is a solution-wide opt-in that conscripts every file which has never adopted the `#nullable enable`
pragma. Nullable enforcement here is per-file opt-in, and `/p:TreatWarningsAsErrors=true` promotes
the `CS86xx` diagnostics of the files that have opted in to build errors. The target and property
set are those of the CI nullable step at `.github/workflows/_build-nullable.yml` lines 73-75, which
itself already uses `/t:Rebuild`, so unlike the analyzer step this task substitutes no target. The
two remaining differences are the two D9 file loggers and the vswhere-resolved `$msbuild` in place
of CI's PATH lookup.

## Non-vacuity observations

| Literal | Count | Required |
|---|---|---|
| `Skipping target "CoreCompile"` | **0** | 0 |
| `Task "Csc"` | **18** | at least 1 |

## Detailed log

- Repository-relative path: `coverage/p6-t5-nullable.detailed.log`
- Byte size: **10627425**
- SHA-256: `B8C0BCF1A73488B5A97AF8179498B2D102FBB32AF2976A42B2C35F3A2C2943BA`

Written under the gitignored `coverage` directory and deliberately not committed.

## Minimal log — two separate observations, both required

1. **Exists on disk** at
   `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t5-nullable.min.log.txt` —
   yes.
2. **Tracked by git.** `git add -N` on that path exited **0**, and the following
   `git ls-files --` span printed exactly that path:

```
docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t5-nullable.min.log.txt
```

This task's command block carries its own copies of both spans, so the inherited clause is evaluated
against this task's own data source rather than P6-T4's. The reasoning for why existence and
trackedness are two different observations is P6-T4's and applies unchanged.

## Warning count

Read from the `N Warning(s)` summary line msbuild printed on the console at default verbosity:

```
    0 Warning(s)
    0 Error(s)
```

The P0-T5 baseline artifact records a warning count of **0**. This gate's count is **0**, which is
no greater than the baseline.

Output Summary: nullable rebuild exited 0 with `0 Warning(s)` and `0 Error(s)`, against a P0-T5
baseline of 0 warnings, so no file that has opted into nullable analysis gained a `CS86xx`
diagnostic. Non-vacuity proven: 0 occurrences of `Skipping target "CoreCompile"` and 18 of
`Task "Csc"` in the detailed log. The minimal log both exists at its `.log.txt` evidence path and is
tracked by git, the `git add -N` step having exited 0.
