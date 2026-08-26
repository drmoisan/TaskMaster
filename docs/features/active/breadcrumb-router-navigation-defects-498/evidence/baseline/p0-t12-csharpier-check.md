# P0-T12 — Baseline Formatting Gate (CSharpier check)

Timestamp: 2026-08-26T08-37

Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

Observed exit code: **0**.

Raw output, verbatim:

```
Checked 1520 files in 7430ms.
```

CSharpier reported **no unformatted files**. The pre-existing unformatted set consumed by the
"Baseline-Comparison Rule for Whole-Solution Gates" is therefore **EMPTY**.

Consequence for `P8-T2`: the conditional degradation in that task is permitted ONLY IF this baseline recorded
a non-zero exit code. It did not, so `P8-T2` must meet its primary acceptance condition `EXIT_CODE: 0`
unconditionally, with no degradation available.

`ExpectedExitCode:` is not declared, because the observed exit code is 0 and the field defaults to 0 when
absent.

CSharpier version in use: 1.2.6 (manifest-pinned by the repository-root `dotnet-tools.json`), invoked through
`dotnet tool run` exactly as `.github/workflows/ci.yml` does.
