# Pragma-Only Nullable Build — Baseline (Issue #364)

- Timestamp: 2026-07-19T08-51
- Task: [P0-T4]

## Plan-literal gate command (full solution)

- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- `/p:Nullable=enable` was NOT passed (confirmed — pragma-only, per the critical deviation).
- EXIT_CODE: 1

### Result

The full-solution pragma-only build exits 1 at BASELINE (before any HelperClasses pragma is added), due to PRE-EXISTING, OUT-OF-SCOPE non-nullable warnings promoted to errors by `/p:TreatWarningsAsErrors=true`:

- `SVGControl/SvgImageSelector.cs(55,24)` and `(56,24)`: `error CS0649` — fields `_relativeImagePath` / `_absoluteImagePath` never assigned. SVGControl is a VENDORED project (no first-party analyzer includes); these fields date to a 2023 WIP commit. Out of scope for #364 (not under `UtilitiesCS/HelperClasses/`).

Because SVGControl is early in the solution build order and is a project reference of UtilitiesCS, the solution build halts at SVGControl before compiling UtilitiesCS. This pre-existing condition surfaced only after the recent HEAD commit `20d163ac` changed the nullable gate from `/t:Build` (a silent no-op) to `/t:Rebuild` (a genuine recompile).

Additional pre-existing out-of-scope warnings-as-errors exist in non-HelperClasses UtilitiesCS files (surfaced by the isolated build below): 28 CS0618 (obsolete API) and 2 CS0168 (unused variable) across `EmailIntelligence/` and `Extensions/`. These are pre-existing and cannot be touched under the #364 scope lock.

These pre-existing blockers are recorded in `evidence/other/maintainer-flags.*.md`.

## Supplementary isolated UtilitiesCS CS86xx verification (authoritative for #364 scope)

Because CS86xx nullable diagnostics are identical whether reported as warnings or (under TWAE) errors, and because they arise ONLY from files carrying a `#nullable enable` pragma (project default is oblivious — `UtilitiesCS.csproj` has no `<Nullable>` element), the authoritative CS86xx signal for this child is obtained by compiling UtilitiesCS in isolation (against the pre-built SVGControl.dll) without TreatWarningsAsErrors and counting CS86xx:

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="AnyCPU" /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 (all pre-existing non-nullable: CS0618/CS0168 outside HelperClasses)

## Output Summary

- Pre-opt-in CS86xx count across `UtilitiesCS/HelperClasses/`: 0 (no file carries a pragma yet; nullable context is oblivious).
- `/p:Nullable=enable` NOT passed (pragma-only deviation honored).
- Baseline for per-batch comparison: zero CS86xx in HelperClasses; each batch must keep this at zero for opted-in files and introduce NO new CS86xx elsewhere.
- Pre-existing out-of-scope warning-as-error blockers (SVGControl CS0649; non-HelperClasses UtilitiesCS CS0618/CS0168) prevent a literal exit-0 full-solution TWAE pass; flagged for the maintainer, not fixable within the #364 scope lock.
