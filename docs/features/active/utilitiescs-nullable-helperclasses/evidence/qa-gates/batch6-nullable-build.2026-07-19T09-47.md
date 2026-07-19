# Batch 6 — Pragma-Only Nullable Build Verification (Issue #364)

- Timestamp: 2026-07-19T09-47
- Task: [P6-T9]

## Opted-in files (7, Windows Forms)

- `ControlPosition.cs` — pragma; value-type members, clean.
- `ControlResizer.cs` — `ControlInfo` struct `name`/`parentName` annotated `string?`; `ctl.Parent` re-access null-flow satisfied by the existing guard; catch behavior unchanged.
- `ImageHelper.cs` — `GetEncoder` return annotated `ImageCodecInfo?` (returns null on no-match).
- `MouseDownFilter.cs` — `event EventHandler? FormClicked`; removed redundant `form = null` (reassigned in ctor).
- `OlvExtension.cs` — pragma; clean.
- `ScreenHelper.cs` — `TryGetScreen` `out Screen screen` uses `screen = default!` (net481 has no public `MaybeNullWhenAttribute`); callers check the bool result or null-guard downstream.
- `TableLayoutHelper.cs` — pragma; `GetControlFromPosition` nullables satisfied by existing guards.

## Command (authoritative CS86xx verification)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- `/p:Nullable=enable` NOT passed (pragma-only; isolated build is the authoritative CS86xx signal — see P0-T4).

## Net481 Attribute Note

An initial `[MaybeNullWhen(false)]` annotation on `TryGetScreen` failed with `CS0122: MaybeNullWhenAttribute is inaccessible` — the attribute is not publicly available on the net481 target (same class of limitation as `IsExternalInit`). Replaced with the behavior-preserving `default!` suppression, which is net481-compatible and changes no behavior.

## Output Summary

- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 (pre-existing non-nullable CS0618/CS0168, unchanged). No new diagnostics introduced by Batch 6.
- Result: PASS. All 7 Batch-6 opted-in files reach zero CS86xx.
