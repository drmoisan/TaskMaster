# Batch 1 — Pragma-Only Nullable Build Verification (Issue #364)

- Timestamp: 2026-07-19T09-16
- Task: [P1-T9]

## Opted-in files (7)

- `UtilitiesCS/HelperClasses/BinaryFlags/GenericBitwise.cs` — removed redundant `= null` field initializers (reassigned in ctor).
- `UtilitiesCS/HelperClasses/MergeSortImplementations.cs` — 3-arg `MergeSort<T>` return annotated `IList<T>?` (documented null-when-inplace return).
- `UtilitiesCS/HelperClasses/ObjectSize.cs` — private `GetObjectSize(object? obj, ...)` (guarded).
- `UtilitiesCS/HelperClasses/ParamArray.cs` — `_args` fields annotated nullable; behavior-preserving `!` at instance-method dereferences.
- `UtilitiesCS/HelperClasses/SimpleRegex.cs` — pragma; already null-clean.
- `UtilitiesCS/HelperClasses/Tokenizer.cs` — pragma; already null-clean.
- `UtilitiesCS/HelperClasses/SegmentStopWatch.cs` — behavior-preserving `!` on the log4net `GetCurrentMethod()!.DeclaringType!` logger initializer; `GroupByActionName` return annotated `Stack<...>?` (documented null-when-inplace).

## Command (authoritative CS86xx verification)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- `/p:Nullable=enable` NOT passed (pragma-only).

### Verification method note

The full-solution `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` gate cannot reach UtilitiesCS at baseline (it halts on the pre-existing vendored `SVGControl` CS0649; see P0-T4 baseline and maintainer-flags). The authoritative CS86xx signal for this child is therefore obtained by compiling UtilitiesCS in isolation (`BuildProjectReferences=false`, against the pre-built SVGControl.dll) and counting CS86xx. Because CS86xx nullable diagnostics are emitted identically whether reported as warnings or (under TreatWarningsAsErrors) errors, and arise only from `#nullable enable` files (project default is oblivious), zero CS86xx warnings is a valid proof of "zero CS86xx under the pragma with TreatWarningsAsErrors".

## Output Summary

- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 — all pre-existing non-nullable (CS0618 obsolete-API, CS0168 unused-variable) in non-HelperClasses UtilitiesCS files; unchanged from the P0 baseline. No new diagnostics introduced by Batch 1.
- Result: PASS. All 7 Batch-1 opted-in files reach zero CS86xx; the full-solution gate result matches the P0-T4 baseline (pre-existing SVGControl halt, out of scope).
