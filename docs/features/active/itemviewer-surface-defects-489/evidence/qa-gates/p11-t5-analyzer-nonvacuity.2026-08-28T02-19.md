# P11-T5 — Analyzer-gate non-vacuity proof (loop iteration 1)

Timestamp: 2026-08-28T02-19
Command: (Select-String -SimpleMatch -Pattern 'Skipping target "CoreCompile"' -Path docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates\p11-t4-analyzer-build.2026-08-28T02-17.msbuild.txt | Measure-Object).Count
EXIT_CODE: 0

Loop iteration: **1**.

## Result

```
Skipping target "CoreCompile"  occurrences: 0
```

The count is **exactly 0**, which is the acceptance condition.

## Which file was searched

`evidence/qa-gates/p11-t4-analyzer-build.2026-08-28T02-17.msbuild.txt` — the `/v:normal` file log
P11-T4 wrote, 11880 lines. It is a `.msbuild.txt` file, not a `.log` file, because `.gitignore:84`
is `*.log` and a `.log` artifact under `FEATURE/evidence/` could never be committed. The search was
run over the log **after** P11-T4's path redaction; redaction replaced absolute path prefixes only
and altered no target-execution line, so the searched text is the text MSBuild emitted.

## Why the zero is a real measurement and not a broken search

A zero-hit search proves nothing unless the same search can produce hits. Three independent
observations establish that it can:

- The identical `Select-String -SimpleMatch` machinery, run over the same file with the shorter
  pattern `Skipping target`, returns **27** matching lines. The log genuinely contains
  skip-notification lines and the search finds them.
- Those 27 lines name exactly two targets, and neither is `CoreCompile`: `Skipping target
  "GenerateTargetFrameworkMonikerAttribute"` 18 times and `Skipping target "CopyMSTestV2Resources"`
  9 times. So the specific literal is absent, rather than the whole class of lines being absent.
- `CoreCompile` itself appears on **83** lines of the log, so the target was reached and executed
  repeatedly; it is present in the log and simply never skipped.

## Exit-code accounting

`Select-String` is a PowerShell cmdlet, not a native executable, so it sets no process exit code of
its own. Under `$ErrorActionPreference = 'Stop'` the pipeline completed with the automatic success
variable `True` and `$Error.Count` at `0`, and `$LASTEXITCODE` was unset (empty) for the whole
session because no native command ran in it. `EXIT_CODE: 0` is recorded on the basis of the success
variable and the zero error count, which is the correct success judgement for a cmdlet-only
pipeline, and the unset `$LASTEXITCODE` is recorded here explicitly rather than being silently
reported as a zero.

## The substitution the task forbids, and why

A `csc.exe` occurrence count is **not** used as a substitute and would not be acceptable: it is zero
even on a real compile in this log family. For the record this log does contain 36 `csc.exe` lines,
but that number gates nothing and is reported only as context. The gate is the
`Skipping target "CoreCompile"` count and nothing else.

## Loop consequence

The stage passed and rewrote nothing. No restart is triggered; the loop proceeds to P11-T6.

Output Summary: The analyzer gate is **non-vacuous**. The literal `Skipping target "CoreCompile"`
occurs **0** times in the 11880-line P11-T4 `/v:normal` log, which is the acceptance condition. The
zero is falsifiable, not an artefact of a broken search: the same search machinery finds 27
`Skipping target` lines in the same file, naming
`GenerateTargetFrameworkMonikerAttribute` (18) and `CopyMSTestV2Resources` (9) but never
`CoreCompile`, and `CoreCompile` itself appears on 83 lines. `EXIT_CODE: 0` is recorded from the
success variable and a zero `$Error.Count` under a `Stop` error preference; `$LASTEXITCODE` was unset
because the pipeline ran no native command. No `csc.exe` count was substituted.
