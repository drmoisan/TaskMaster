# Baseline AC2 — No `<Nullable>` Element in UtilitiesCS.csproj

Timestamp: 2026-07-19T01-25

Command: `grep -c "<Nullable" UtilitiesCS/UtilitiesCS.csproj` and `grep -niE "nullable" UtilitiesCS/UtilitiesCS.csproj`

Result:
- `<Nullable` element occurrences: 0.
- The only line matching case-insensitive "nullable" is line 1280, a comment: `<!-- Issue #181: analyzer-only references ... so none break the nullable TreatWarningsAsErrors build. -->`. This is a comment, not a `<Nullable>` MSBuild property.

Confirmation: UtilitiesCS/UtilitiesCS.csproj contains zero `<Nullable>` elements at baseline. AC2 baseline established: the feature must keep it that way (per-file pragma opt-in only).
