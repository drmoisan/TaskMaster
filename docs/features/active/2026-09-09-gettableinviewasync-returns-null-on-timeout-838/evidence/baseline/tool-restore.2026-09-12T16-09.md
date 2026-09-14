# P0-T11 — Local .NET tool manifest restore (csharpier pinned at 1.2.6)

Timestamp: 2026-09-13T02-18

Command: `pwsh -NoProfile -Command '& ".\.dotnet-sdk\dotnet.exe" tool restore; exit $LASTEXITCODE'`

EXIT_CODE: 0

Output Summary: the repo-local muxer restored the manifest at the repository root. Printed output, transcribed verbatim:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

The output contains the case-sensitive text `csharpier` and the restored version is the manifest-pinned 1.2.6, so the formatter will resolve at the version the repository's own format-check workflow uses. Both acceptance clauses hold.
