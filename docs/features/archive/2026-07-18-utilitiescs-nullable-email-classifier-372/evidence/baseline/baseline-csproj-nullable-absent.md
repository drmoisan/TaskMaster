# Baseline — AC2 csproj `<Nullable>` Absence

Timestamp: 2026-07-19T00-52

Command: `grep -in '<Nullable' UtilitiesCS/UtilitiesCS.csproj`

Result: ZERO occurrences of any `<Nullable>` element in `UtilitiesCS/UtilitiesCS.csproj` at baseline.

This establishes the AC2 baseline: the project uses no project-level `<Nullable>` setting; remediation is per-file `#nullable enable` pragma opt-in only. The P8-T6 end-state check confirms no `<Nullable>` element was introduced.
