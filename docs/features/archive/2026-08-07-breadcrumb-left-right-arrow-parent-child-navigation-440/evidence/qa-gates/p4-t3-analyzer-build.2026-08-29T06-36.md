# Phase 4 — Analyzer Step (issue #440, plan task P4-T3)

Timestamp: 2026-08-29T06-36

The log directory was created first as a precaution with
`New-Item -ItemType Directory -Force -Path coverage\logs`. No claim is made here that
the MSBuild file logger fails or exits non-zero when its log directory is absent;
that rationale was tested during preflight round 2 and found false, as recorded in
`<FEATURE>/evidence/other/preflight-round-2.2026-08-29T02-20.md`.

Command:

```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl "/flp:LogFile=coverage\logs\p4-t3-analyzer.msbuild.txt;Verbosity=normal"
```

EXIT_CODE: 0

The log path sits under the gitignored `coverage/` tree per Global rule 8, because an
msbuild log embeds absolute host paths and must never be written under
`<FEATURE>/evidence/`.

## Output Summary

MSBuild summary lines, read from the tail of the file-logger output:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:11.97
```

- Error count: **0**. Gate requires 0. PASS.
- Warning count: **5**. P0-T11 baseline warning count: 5. The gate requires at or
  below the baseline, and 5 is at the baseline. PASS.

The five warnings are the same pre-existing System.Reactive 7.0.0 packages-config
advisories the baseline recorded, one per packages-config project. No new diagnostic
was introduced by this change.

## Non-vacuity counts, read from `coverage\logs\p4-t3-analyzer.msbuild.txt`

Both counted with `Select-String -SimpleMatch`. The log carries 11622 lines.

| Literal | Count | Gate | Result |
| --- | --- | --- | --- |
| `Skipping target "CoreCompile"` | **0** | must be 0 | PASS |
| `(Rebuild target(s))` | **40** | must be at least 1 | PASS |

The first count is the negative half of the non-vacuity proof: no project skipped
`CoreCompile`, so analyzers ran on every project rather than the build exiting 0 on a
warm incremental up-to-date check.

The second count is the positive half and is what fails if the log is empty or was
never written. The parenthesised `(s)` is part of the string MSBuild emits in its
ProjectStarted and `Done Building Project` messages at normal verbosity, which appear
on every run. The shorter spelling `(Rebuild target)` appears only inside the terminal
warning and error summary block, which MSBuild emits only when the build produced
diagnostics, so counting that spelling would fail exactly when the build is clean.

This is the analyzer half of AC-14.
