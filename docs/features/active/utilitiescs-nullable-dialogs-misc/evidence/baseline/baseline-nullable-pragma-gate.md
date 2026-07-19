# Phase 0 — Baseline Per-File Nullable Pragma Gate

- Timestamp: 2026-07-19T10-53
- Task: [P0-T6]

## Command 1 (plan-mandated, solution-wide)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`)
- EXIT_CODE: 1
- CS86xx count: 0

### Output Summary (Command 1)

The mandated solution-wide command exits 1, but NOT on any CS86xx and NOT on any file in this
cluster. It aborts on 2 pre-existing vendored diagnostics in `SVGControl/SvgImageSelector.cs`:
`error CS0649: Field '_relativeImagePath'/'_absoluteImagePath' is never assigned` (a non-nullable
warning promoted to error by `TreatWarningsAsErrors`). Because `UtilitiesCS.csproj` has a
`<ProjectReference>` to `SVGControl.csproj`, the SVGControl failure short-circuits the solution
build before `UtilitiesCS` compiles (only `VBFunctions` compiled). Consequently the solution-wide
command alone CANNOT observe CS86xx in the cluster — it never reaches the cluster's compilation.
This is a pre-existing blocker unrelated to issue #374 (vendored SVGControl debt), flagged for the
maintainer. Zero CS86xx were emitted anywhere in the output.

## Command 2 (supplementary authoritative CS86xx detector, scoped isolated build)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /p:BuildProjectReferences=false` (dependency DLLs pre-built by a prior non-TWAE solution build)
- EXIT_CODE: 0
- CS86xx count: 0

### Output Summary (Command 2)

The scoped isolated build actually compiles the `UtilitiesCS` cluster against already-built
reference DLLs, so it is the authoritative CS86xx detector. It excludes the three pre-existing
non-nullable warning codes that plain TWAE would otherwise promote to errors — CS0649 (vendored
SVGControl-class, also present as pre-existing UtilitiesCS debt), CS0618 (28 pre-existing obsolete
-API usages), CS0168 (2 pre-existing unused-var) — none of which is a nullable diagnostic. Result:
EXIT_CODE 0, `0 Error(s)`, `15 Warning(s)` (the demoted pre-existing codes), **zero CS86xx**. This
is the expected baseline: none of the 14 cluster files is yet opted into `#nullable enable`, so no
pragma-gated nullable diagnostic can fire.

## Verification method for subsequent batch/final gates

Each batch and Final QC gate records BOTH: (1) the plan-mandated solution-wide command for the
record, and (2) the scoped isolated `UtilitiesCS` build as the authoritative per-file CS86xx signal
(the only command that actually compiles the opted-in cluster past the pre-existing SVGControl
short-circuit). AC1 is judged on the CS86xx count, which must remain zero.

## Escalation

The plan's exact solution-wide verification command cannot compile the cluster because of two
pre-existing, out-of-scope blockers (vendored SVGControl CS0649 short-circuit; pre-existing
UtilitiesCS CS0618/CS0168 under plain TWAE). The scoped isolated build is used as the
mechanically-necessary supplement to actually verify AC1. This is flagged for the
maintainer/epic-planner and is consistent with the epic's documented per-file pragma verification
convention; no `.claude/rules/*` file was edited to reconcile it.
