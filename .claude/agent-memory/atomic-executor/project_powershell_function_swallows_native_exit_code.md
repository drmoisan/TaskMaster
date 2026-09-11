---
name: powershell-function-swallows-native-exit-code
description: A PowerShell helper function that runs a native command and then `return [int]$LASTEXITCODE` returns the command's captured stdout joined with the code, so the caller's `exit $code` silently becomes 0 and every red gate reads green.
metadata:
  type: project
---

In a PowerShell helper, this shape is wrong and fails silently:

```powershell
& $vsTest @args          # stdout joins the FUNCTION'S output stream
return [int]$LASTEXITCODE # the return value is an ARRAY: all console lines + the code
```

The caller's `$code = Invoke-Thing ...; exit $code` then exits **0** on a run that genuinely
failed. Measured on 2026-09-07 during issue #797: a vstest run with 16 real failures reported
`exit code 0`, and the transcript began `EXIT_CODE=VSTEST-EXE=...` — the concatenated array is
the tell. Fix by keeping the native command's stdout off the pipeline:

```powershell
Write-Host "VSTEST-EXE=$vsTest"   # Write-Host, not Write-Output
& $vsTest @args | Out-Host        # Out-Host, not bare invocation
return [int]$LASTEXITCODE
```

`| Out-Host` does not disturb `$LASTEXITCODE`, and the transcript is unchanged.

A second instance of the same class in the same run: `$null = Invoke-FormatPass` discarded the
function's own `Write-Output` diagnostic lines along with the return value, so a mode that was
supposed to print `FORMAT-REWRITTEN-COUNT=` printed nothing at all.

**Why:** an `[expect-fail]` task and every `EXIT_CODE:`/`ExpectedExitCode:` gate in an atomic plan
is decided by that integer. A helper that always returns 0 makes the red-before evidence
unfalsifiable and would have let a fabricated `EXIT_CODE: 1` into an artifact.

**How to apply:** before recording any exit code produced through a PowerShell wrapper function,
run the wrapper once on a known-failing input and confirm the process exit code is non-zero. If the
first characters of the transcript are `EXIT_CODE=` immediately followed by other output, the array
bug is present. Related: [[project_pwsh_command_quoting_from_bash]],
[[project_expectedexitcode_declared_from_baseline_not_observed_run]].
