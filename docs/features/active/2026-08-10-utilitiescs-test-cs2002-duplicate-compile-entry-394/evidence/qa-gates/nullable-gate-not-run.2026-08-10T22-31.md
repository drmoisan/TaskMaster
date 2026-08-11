Timestamp: 2026-08-10T22-31

Determination: `CLAUDE.md`'s documented `/p:Nullable=enable` command
(`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`)
is not run for this change.

Rationale: This command is a known defect tracked as issue #522 (see project memory
`project_nullable_gate_diverges_from_ci` and `project_507_nullconditional_return_triggers_cs8603_under_genuine_nullable_check`).
Forcing `/p:Nullable=enable` repository-wide (rather than the per-file `#nullable` pragmas that
`.github/workflows/ci.yml` actually relies on) produces on the order of 200-414 spurious `CS86xx`
nullable-flow errors on an otherwise-clean `main`, unrelated to any change made in this feature.
Running this command against this feature's branch would report the same pre-existing,
out-of-scope nullable debt as a false failure for a single-line `.csproj` deletion that touches no
`.cs` source.

This feature's applicable type-check-equivalent gate is P2-T3's CI-equivalent solution build:
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`,
which reflects the command CI's own `TreatWarningsAsErrors` job actually runs (`.github/workflows/ci.yml`
lines 103-116, per `spec.md` Root Cause Analysis) and does not force whole-repository nullable
analysis outside of what per-file pragmas already opt into. See
`docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/evidence/qa-gates/solution-rebuild.*.md`
for that gate's evidence.
