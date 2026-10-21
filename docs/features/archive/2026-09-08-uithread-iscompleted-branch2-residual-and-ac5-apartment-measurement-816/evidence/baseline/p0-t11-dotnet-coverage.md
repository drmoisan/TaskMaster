# P0-T11 — Cobertura collector availability (baseline)

Timestamp: 2026-09-13T23-02

Command: `dotnet-coverage --version`

EXIT_CODE: 0

Output Summary:

Printed version string:

```
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
```

The tool was already present on PATH, so no install was required and no second command was run.

This tool is required because the per-file line figures AC10 demands cannot be read from the binary
document that the test runner's own `/EnableCodeCoverage` switch produces. P0-T16 and P4-T11 use it
to emit a Cobertura document into the gitignored coverage directory, from which the per-file figures
are transcribed into the Markdown projections.
