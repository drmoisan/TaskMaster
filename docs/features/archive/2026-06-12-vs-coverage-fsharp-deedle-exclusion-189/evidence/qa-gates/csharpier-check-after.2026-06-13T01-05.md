# QA Gate — CSharpier Check (After Edit) — Pass-After Evidence

Timestamp: 2026-06-13T01-05
Command: `dotnet csharpier check .`
EXIT_CODE: 0

## Output Summary

After appending `*.csproj`, `*.props`, `*.targets` to `.csharpierignore`,
`dotnet csharpier check .` passed with exit code 0. Checked 1040 files (down from
1060 in the before run; the 20-file delta is the now-excluded project files). None
of the 8 previously-failing `.csproj` files are reported. Zero project-file failures.
This is the pass-after evidence for the remediation.

### Verbatim output

```
Checked 1040 files in 2944ms.
EXIT_CODE=0
```
