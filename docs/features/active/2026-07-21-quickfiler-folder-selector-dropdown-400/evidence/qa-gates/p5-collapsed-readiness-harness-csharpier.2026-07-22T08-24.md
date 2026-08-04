# P5 collapsed-readiness harness CSharpier restart gate

Timestamp: `2026-07-22T08:24:00Z`

Command: `@('QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs') | & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' pipe-files`

EXIT_CODE: `0`

Output Summary: `PASS. The P5-T76 launcher-resolution failure triggered the required ordered restart. CSharpier again ran against exactly the authorized test file; both passes made no change, and the file remains 489 physical lines.`

- SHA-256 before and after both passes: `B53E9E091C461F835D900A8FB5DE0DB6B02080645EDFE18ACD0541E6E95E68F6`
- First-pass exit code: `0`
- Second-pass exit code: `0`
- Stable on second pass: `true`
- Physical lines: `489`
