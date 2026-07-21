# Phase 0 — AC2 Baseline: csproj `<Nullable>` Absence (P0-T7)

Timestamp: 2026-07-19T09-06

Command: `grep -cE "<Nullable" UtilitiesCS/UtilitiesCS.csproj`
Result: 0

Command: `grep -inE "Nullable" UtilitiesCS/UtilitiesCS.csproj`
Result: a single match on line 1280 — a COMMENT ("...none break the nullable TreatWarningsAsErrors
build.") referencing the word "nullable" in prose. There is NO `<Nullable>` MSBuild property
element in the csproj.

Output Summary: Zero `<Nullable>` elements exist in `UtilitiesCS/UtilitiesCS.csproj` at baseline
(AC2 baseline confirmed). The per-file `#nullable enable` pragma opt-in architecture is the only
nullable mechanism this feature introduces; the project-level `<Nullable>` element must remain
absent through end state (verified again at P9-T6, out of this run's scope).
