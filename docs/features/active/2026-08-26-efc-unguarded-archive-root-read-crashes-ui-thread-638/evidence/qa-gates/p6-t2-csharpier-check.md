# [P6-T2] CSharpier check (Issue 638)

Timestamp: 2026-08-29T12-36

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Output Summary:

Final summary line, quoted verbatim:

```
Checked 1561 files in 4097ms.
```

The unformatted count is derived, as in [P0-T9], from the
`Error <path> - Was not formatted.` lines rather than from that summary line, whose N is a
processed count. The run emitted **0** such lines.

[P0-T9] recorded `BASELINE_UNFORMATTED_COUNT: 0`, so this task's acceptance is the
`EXIT_CODE: 0` branch, which is met. The subset branch does not apply and no
`REMEDIATION-REQUIRED:` line is appended here; [P8-T15] therefore takes the AC13 check-off
branch.

The file count rose from the 1560 checked at baseline to 1561 because
`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` is new.
`QuickFiler.Test/QuickFiler.Test.csproj` is excluded from the check by `.csharpierignore:12`
(`*.csproj`), and the feature folder's evidence artifacts by `.csharpierignore:4`
(`**/evidence/**`), so neither is subject to this gate.
