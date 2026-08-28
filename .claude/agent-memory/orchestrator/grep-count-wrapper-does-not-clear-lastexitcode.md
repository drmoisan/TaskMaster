---
name: grep-count-wrapper-does-not-clear-lastexitcode
description: The PowerShell (git grep ... | Measure-Object).Count idiom does NOT reset $LASTEXITCODE on a zero-match result, so plans that cite it to justify omitting ExpectedExitCode are wrong
metadata:
  type: project
---

Plans in this repo justify omitting `ExpectedExitCode:` on `git grep` gates by claiming that wrapping the pipeline in `(… | Measure-Object).Count` makes the pipeline's own exit code `0`. **That claim is false on a zero-match result.** `git grep` exits `1` natively when it matches nothing, and the PowerShell wrapper does not reset `$LASTEXITCODE`.

Measured directly during epic child 489: `Count=0`, `$?` = `True`, `$Error.Count` = `0`, `$LASTEXITCODE` = `1`.

**Why:** the claim was written into the 489 plan's § Execution conventions and used to exempt five gates (P0-T16, P0-T17, P4-T7, P9-T8, P10-T15). It went unnoticed because the first such gate to run, P0-T17, had 16 matches and never exercised the zero-match branch. P4-T7 was the first genuine zero-match gate and hit it immediately.

**How to apply:** when a gate greps for a literal that should be absent, tell the executor to judge success from `$?` and `$Error.Count` under `$ErrorActionPreference='Stop'`, record `EXIT_CODE: 0` on that basis, and **document the residual `$LASTEXITCODE=1` explicitly in the artifact**. Never let a bare `0` be written without the explanation, and never let the idiom mask a real failure. Recorded as out-of-scope finding E3 on 489.

Related: [[preflight-catches-vacuous-gates]], [[msbuild-analyzer-gate-vacuous-without-rebuild]], [[csharp-direct-csproj-build-facts]].
