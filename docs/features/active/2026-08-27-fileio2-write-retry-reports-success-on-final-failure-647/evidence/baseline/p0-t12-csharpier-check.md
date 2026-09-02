# P0-T12 — Formatter Baseline (read-only check)

Timestamp: 2026-08-31T18-52
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
ExpectedExitCode: 0

Output Summary: The tool's final summary line, transcribed verbatim:

```
Checked 1565 files in 4607ms.
```

`check` is read-only and returns a non-zero exit code when any target file is unformatted. It exited 0, so the branch head is formatter-clean across the whole CSharpier target set (`*.cs`, non-excluded `*.xml`, and `packages.config`, minus the `.csharpierignore` exclusions).

PRE_EXISTING_FORMAT_DRIFT: none. No path was reported unformatted, so no drift list exists.

Consequences fixed by this observation, for the tasks that branch on it:

- P2-T2, P4-T7 and P5-T8 have no `CARRIED_BASELINE_FORMAT_DRIFT:` branch available; their `check` exit code must be 0.
- P6-T1's repository-wide `format .` has no pre-existing drift to repair, so it cannot widen the change footprint beyond the five footprint files.
- P7-T19's AC19 disposition clause for carried formatter drift is inapplicable; the criterion is evaluated against the footprint alone.
