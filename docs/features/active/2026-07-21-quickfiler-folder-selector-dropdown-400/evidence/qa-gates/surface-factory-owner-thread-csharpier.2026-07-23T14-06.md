# Surface factory owner-thread CSharpier gate

- Timestamp: `2026-07-23T14-06Z`
- Command: `csharpier format QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs; csharpier check QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs`
- EXIT_CODE: `0`
- Output Summary: `Formatted 1 file; checked 1 file; both commands exited zero; formatting produced no byte delta; final file is exactly 480 physical lines.`

| Measurement | Value |
|---|---|
| Pre-format SHA-256 | `3FE231161F91AB05FE28F4E99AE047B5D56B95FC8C09EF263B2FC4FB39676D38` |
| Post-format SHA-256 | `3FE231161F91AB05FE28F4E99AE047B5D56B95FC8C09EF263B2FC4FB39676D38` |
| Post-check SHA-256 | `3FE231161F91AB05FE28F4E99AE047B5D56B95FC8C09EF263B2FC4FB39676D38` |
| Format exit code | `0` |
| Check exit code | `0` |
| Physical lines | `480` |

An earlier attempt used `dotnet tool run csharpier`, but the repository-local `dotnet`
shim cannot execute SDK tools in this checkout. That attempt exited nonzero and changed
no file. P8-T31 was restarted from formatting with the installed, approved
`csharpier.exe`; only the passing restart is current gate evidence.

