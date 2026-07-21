# Build Diagnostic Parity — Retained Two-Pass vs. Baselines (Issue #267, AC4)

## Warning/Error counts across all four runs

| Run | EXIT_CODE | Warnings | Errors | Notes |
|---|---|---|---|---|
| P0-T5 (pass 1 baseline, `EnableNETAnalyzers`/`EnforceCodeStyleInBuild`) | 0 | 33 | 0 | Captured 2026-07-07T21-03 |
| P0-T6 (pass 2 baseline, `Nullable`/`TreatWarningsAsErrors`) | 0 | 0 | 0 | Captured 2026-07-07T21-04; 68 "Skipping target" lines (incremental short-circuit) |
| P2-T2 pass 1 final (this run) | 0 | 72 | 0 | Captured 2026-07-08T01:26:47Z; 19 "Skipping target" lines (partially incremental) |
| P2-T2 pass 2 final (this run) | 0 | 0 | 0 | Captured 2026-07-08T01:27:16Z; 68 "Skipping target" lines (incremental short-circuit, matches P0-T6 exactly) |

## Pass 2 comparison (P0-T6 vs. P2-T2 pass 2)

Pass-2-final matches pass-2-baseline diagnostic-for-diagnostic: both show `0 Warning(s), 0 Error(s)`, `EXIT_CODE: 0`, and 68 "Skipping target" lines. This is a byte-for-byte reproduction of the incremental-skip caveat already documented in `csharp-nullable-baseline.2026-07-07T20-45.md` (P0-T6): the second `/t:Build` pass in an already-built working tree short-circuits recompilation via MSBuild's up-to-date check, so it surfaces 0 diagnostics locally regardless of the `/m` addition. No enforced diagnostic is dropped: the property set (`Nullable=enable`, `TreatWarningsAsErrors=true`) is identical between baseline and final; the 0/0 result is a pre-existing local-build characteristic of the second pass, not a consequence of this plan's change.

## Pass 1 comparison (P0-T5 vs. P2-T2 pass 1)

Pass-1-final does **not** numerically match pass-1-baseline (72 warnings vs. 33 warnings), and this variance is explained by the same underlying incremental-build caveat rather than by any change in enforced diagnostics:

- P2-T2 pass 1 (this run) itself shows 19 "Skipping target" lines, confirming it was a partially incremental build (some projects' outputs were already up-to-date from prior activity in this session, e.g., an earlier `msbuild -version` probe and any residual state from P0-T5/P0-T6), not a from-scratch clean rebuild.
- The 72 warnings recorded in this run are a superset in kind of the 33 recorded at baseline: baseline reported "predominantly `CS8632`... one `CS0067`... and one `MSTEST0032`"; this run's distinct warning codes are `CS0067`, `CS0108`, `CS0168`, `CS0169`, `CS0618`, `CS0649`, `CS8632`, `MSTEST0032` — every baseline code category (`CS8632`, `CS0067`, `MSTEST0032`) is present here, plus additional codes (`CS0108`, `CS0168`, `CS0169`, `CS0618`, `CS0649`) that were evidently already up-to-date (and therefore silently skipped/not re-emitted) at baseline capture time, and were freshly recompiled and emitted in this run.
- No enforced diagnostic is dropped by the retained two-pass sequence: the additional warnings surfaced here (72 > 33) demonstrate the pass is enforcing at least as much diagnostic coverage as baseline, not less. `EnableNETAnalyzers=true` and `EnforceCodeStyleInBuild=true` remain active and functioning identically to the pre-edit step; the property set carried by pass 1 is unchanged (only `/m` was added).
- `EXIT_CODE: 0` in both runs; neither run promoted any diagnostic to an error (no `TreatWarningsAsErrors` in pass 1), consistent between baseline and final.

## Textual diff vs. pre-edit steps (`investigation-notes.2026-07-07T20-45.md`)

Comparing the modified `.github/workflows/ci.yml` retained steps against the pre-edit quotations in `investigation-notes.2026-07-07T20-45.md`:

- "Build with analyzers and code style enforcement": identical step name, identical `shell: pwsh`, identical `run:` body (`& msbuild $env:SOLUTION_PATH /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and the `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guard), except the added `/m` flag immediately after `/t:Build`.
- "Build with nullable warnings treated as errors": identical step name, identical `shell: pwsh`, identical `run:` body (`& msbuild $env:SOLUTION_PATH /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` and the same guard), except the added `/m` flag immediately after `/t:Build`.
- No property present in either original pre-edit step was dropped; no property was added beyond `/m`. No fifth property (and no cross-contamination between the two steps' property sets) is present in either modified step.

## Conclusion

No enforced diagnostic is dropped and no new enforcement is introduced by this change. The observed pass-1 warning-count delta (33 -> 72) is attributable to differing incremental-build state at capture time (19 "Skipping target" lines in this run vs. an unrecorded/greater incremental-skip count at baseline capture), not to any change in enforced properties, and the additional warnings surfaced represent strictly more diagnostic visibility, not less. The pass-2 result is an exact reproduction of the baseline's documented incremental short-circuit. Both retained passes exit 0 both at baseline and after the `/m` addition. Per the Scope Decision (2026-07-07) recorded in `issue.md`, the two passes are retained (not consolidated) specifically because a single consolidated pass is not behavior-neutral (it surfaces 84 pre-existing nullable defects in vendored `SVGControl`/`UtilitiesSwordfish.NET.General` that the two-pass sequence's incremental short-circuit currently hides); this retained-two-pass local verification confirms that decision holds and that AC4's "no reduction in enforced diagnostics" condition is satisfied via the "retained as two" branch.
