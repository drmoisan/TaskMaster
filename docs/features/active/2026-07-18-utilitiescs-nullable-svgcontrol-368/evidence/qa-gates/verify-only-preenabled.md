# Verify-Only Confirmation — Pre-Enabled Files

Timestamp: 2026-07-19T01-15

Command: `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
(platform-syntax note: see `evidence/baseline/baseline-nullable-pragma-gate.md` for why `AnyCPU`
(no space) is used for direct-`SVGControl.csproj` invocations in this plan's execution)

EXIT_CODE: 1 (pre-existing, unrelated `CS0649`-promoted-to-error in `SvgImageSelector.cs`; see
`evidence/baseline/baseline-nullable-pragma-gate.md`. Zero `CS86xx` in the full build log.)

Output Summary: The 3 already-`#nullable enable` files — `SVGControl/PathInternal.cs`,
`SVGControl/RelativePath.cs`, `SVGControl/ValueStringBuilder.cs` — emit **zero CS86xx**
diagnostics under the pragma-gate rebuild (confirmed via `grep -c "CS86" <log>` = 0, identical
to the Phase 0 baseline capture). No edits were made to any of the 3 files; they remain
byte-identical to their state at Phase 0. The only build errors present (`CS0649` x2 in
`SvgImageSelector.cs`) are pre-existing, unrelated to nullable reference types, and out of
scope for this task (they concern Batch C, Phase 3, not the verify-only files).

Confirmation: all 3 files remain unmodified — no diagnostic requiring an edit appeared for any
of them.
