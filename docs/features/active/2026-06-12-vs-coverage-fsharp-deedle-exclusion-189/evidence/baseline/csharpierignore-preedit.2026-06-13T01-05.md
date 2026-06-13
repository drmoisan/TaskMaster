# Baseline — `.csharpierignore` Pre-Edit Contents

Timestamp: 2026-06-13T01-05
Command: Read .csharpierignore (repo root)
EXIT_CODE: 0

## Output Summary

Pre-edit `.csharpierignore` contains the following existing globs:
`**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`.
No project-file globs (`*.csproj`, `*.props`, `*.targets`) are present.

### Full pre-edit file body

```
# CSharpier formats C# source only. Generated coverage and test-result
# artifacts are committed as audit-trail evidence (not source) and must not
# be subject to formatting checks (e.g. trailing-newline rules on tool output).
**/evidence/**
*.cobertura.xml
*.coverage
*.coveragexml
*.trx
```
