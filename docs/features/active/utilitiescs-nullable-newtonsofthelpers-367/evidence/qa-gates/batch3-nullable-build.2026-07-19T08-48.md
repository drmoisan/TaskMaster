# Batch 3 Nullable Build Verification (P3-T5)

- Timestamp: 2026-07-19T08-48
- Opted-in files (2): `UtilitiesCS/NewtonsoftHelpers/NConsoleTraceWriter.cs`, `NLogTraceWriter.cs`

## Genuine nullable gate

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded, zero errors, zero `CS86xx` in the two trace-writer files. CS86xx remains fatal, so EXIT 0 proves both files are nullable-clean under their pragmas.

## Exact plan solution command (invariant, per baseline)

Invariant with P0-T4 (SVGControl-blocked; edits confined to two `NewtonsoftHelpers/` files). Executed in full at P9-T3.

## Edits applied (annotation-only)

- `NConsoleTraceWriter.cs`: `#nullable enable`; `Trace(TraceLevel, string, Exception ex)` -> `Exception? ex` (matches `ITraceWriter`); `message` stays non-null; public `Log` property `Action<string, Exception>` -> `Action<string, Exception?>? Log` (deliberate public contract resolving the CS8618 uninitialized field and the nullable-`ex` invoke, `// why` comment).
- `NLogTraceWriter.cs`: `#nullable enable`; NO `namespace` block added (GLOBAL namespace preserved, see P3-T3 flag); `Trace` `Exception? ex`; `GetLogFunction` return `Action<string, Exception>` -> `Action<string, Exception?>?` (the `TraceLevel.Off` path returns null; the nullable exception parameter matches ITraceWriter and lets `logFunction?.Invoke(message, ex)` pass the nullable `ex` without a forgiveness operator; the log4net method-group targets are oblivious and convert cleanly); behavior-preserving `!` on `GetCurrentMethod()!.DeclaringType!`.

Note: the plan text for `GetLogFunction` suggested `Action<string, Exception>?`; the honest `Action<string, Exception?>?` was used instead because it compiles clean without an added `!` at the `Invoke` call site and correctly reflects that the ITraceWriter exception argument is nullable. This is annotation-only and behavior-identical (the delegate body and the returned method groups are unchanged).
