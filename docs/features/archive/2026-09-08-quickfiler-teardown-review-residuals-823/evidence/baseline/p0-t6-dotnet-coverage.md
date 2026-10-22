# Phase 0 — dotnet-coverage global tool

Timestamp: 2026-09-09T13-50

Task: [P0-T6]

Command: `dotnet-coverage --version`

EXIT_CODE: 0

DOTNET-COVERAGE-VERSION: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
BRANCH-TAKEN: probe-only

`dotnet-coverage` is a global tool and is not in the repository's local tool manifest, so
`dotnet tool restore` does not supply it. The probe succeeded on the first attempt, so the
install branch was not taken.

Output Summary: `dotnet-coverage --version` exited 0 and printed a single version line,
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342. No install was required.
