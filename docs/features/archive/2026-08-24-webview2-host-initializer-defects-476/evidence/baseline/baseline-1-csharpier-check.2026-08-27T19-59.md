# Baseline 1 of 4 — CSharpier Format Check ([P0-T8])

Timestamp: 2026-08-27T19-59

Command:
```
dotnet tool run csharpier check .
```
(run from the workspace root)

EXIT_CODE: 0

## Output Summary

Observed exit code: **0**.

Complete tool output:

```
Checked 1540 files in 5061ms.
```

**Pre-existing formatting debt: none.** CSharpier reported **no file** as unformatted. The list of
reported files is empty, so the Phase 4 formatter gate `[P4-T1]` is compared against an empty
baseline: after the change, `dotnet tool run csharpier check .` must again report zero files, and in
particular must not report any of this feature's six touched files.

1540 files were inspected. CSharpier 1.2.6 processes `*.cs`, `*.xml`, and `packages.config`;
`*.csproj`, `*.props`, and `*.targets` are excluded by `.csharpierignore`.
