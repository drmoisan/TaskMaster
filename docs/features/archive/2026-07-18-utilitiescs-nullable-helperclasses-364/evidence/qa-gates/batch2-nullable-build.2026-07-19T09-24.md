# Batch 2 — Pragma-Only Nullable Build Verification (Issue #364)

- Timestamp: 2026-07-19T09-24
- Task: [P2-T6]

## Opted-in files (4, Logging)

- `UtilitiesCS/HelperClasses/Logging/DebugTextLogger.cs` — behavior-preserving `!` on the log4net `GetCurrentMethod()!.DeclaringType!` initializer.
- `UtilitiesCS/HelperClasses/Logging/DebugTextWriter.cs` — pragma; no nullable surface.
- `UtilitiesCS/HelperClasses/Logging/VerboseLogger.cs` — behavior-preserving `!` on the logger initializer.
- `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` — settled the cross-module extension-method return-nullability contracts: `GetCallerMethod` (x2) and `GetFirstMethodOfMine` return `MethodBase?`; `GetCallerParameters` (x2) return `ParameterInfo[]?`; `Pop<T>` returns `T?`; `GetParameterString` accepts `this MethodBase?`; nullable local declarations for resolved methods; behavior-preserving `!` on documented post-resolution reflection reads (`DeclaringType`, `AssemblyName.Name`, `StackFrame.GetMethod()`), all wrapped by the existing try/catch so the null-throws paths are unchanged; lazy `_projectNames` annotated `List<string>?`.

## Command (authoritative CS86xx verification)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- `/p:Nullable=enable` NOT passed (pragma-only). See P0-T4 / batch1 notes for why the isolated build is the authoritative CS86xx signal (full-solution TWAE gate halts on pre-existing vendored SVGControl).

## Output Summary

- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 (pre-existing non-nullable CS0618/CS0168, unchanged from baseline). No new diagnostics introduced by Batch 2.
- Result: PASS. All 4 Batch-2 opted-in files reach zero CS86xx; TraceUtility's public extension-method signatures remain behavior-compatible (nullable-return annotations reflect actual null behavior).
