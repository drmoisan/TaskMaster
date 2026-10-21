# P0-T9 — Coverage Collector Probe

Timestamp: 2026-09-01T08-08

## Invocation 1 — probe

Command: `dotnet-coverage --version`

EXIT_CODE: 0

Output Summary: stdout is the version string
`18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`. The tool is already present as a global .NET tool
on this machine and is on `PATH`.

## Install Determination

**Was the install step needed? No.** The initial probe exited 0 and printed a version string, so the
plan's conditional branch (`dotnet tool install --global dotnet-coverage`) was not taken. Only one
invocation was performed, and it is the final invocation.

`dotnet-coverage` 18.5.2 is used by P0-T10 and P3-T7 to produce the Cobertura reports that the
changed-line coverage comparison reads.

Acceptance: met. The artifact records a final `dotnet-coverage --version` invocation with
`EXIT_CODE: 0` and a printed version string, and records that the install step was not needed.
