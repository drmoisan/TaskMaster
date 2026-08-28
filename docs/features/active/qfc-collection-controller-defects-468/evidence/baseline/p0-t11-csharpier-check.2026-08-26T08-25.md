# [P0-T11] Formatter baseline (CSharpier)

Timestamp: 2026-08-26T08-25

Command: `pwsh -NoProfile -Command "Set-Location '<WS>'; dotnet tool run csharpier check ."`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Full output, verbatim (a single line):

```
Checked 1520 files in 8502ms.
```

**Number of files reported as needing formatting: 0.**

CSharpier 1.2.6 reports each unformatted file on its own line in the form
`Error <path> - Was not formatted`. The command's entire output is one line and contains no such
line, and the process exit code is `0`. The run was repeated with the output captured into a
variable to confirm both facts independently: `EXIT=0`, `LINES=1`, and a `Select-String` for
`Was not formatted|Error` matched nothing.

### Baseline established for later gates

- **1520** files are in CSharpier's scope at the base commit (`.csproj`, `.props` and `.targets` are
  excluded by `.csharpierignore`, not by CSharpier itself).
- The tree is **clean** with respect to formatting before any edit in this plan. Consequently, any
  non-zero `dotnet tool run csharpier check .` exit later in this plan is attributable to this
  feature's own edits, not to pre-existing drift. P1-T5, and every later format gate, is measured
  against this zero.

Result: PASS.
