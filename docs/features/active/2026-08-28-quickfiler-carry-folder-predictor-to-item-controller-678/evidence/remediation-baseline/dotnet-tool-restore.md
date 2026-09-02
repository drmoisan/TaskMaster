# Baseline — `dotnet tool restore`

- Timestamp: 2026-09-02T01-03
- Issue: #678
- Task: [P0-T4]

Command: `dotnet tool restore`

EXIT_CODE: 0

## Manifest-pinned CSharpier version

Read directly from the repository-root file `dotnet-tools.json`, which is the manifest
present in this tree (there is no `.config/dotnet-tools.json`):

```json
"csharpier": {
  "version": "1.2.6",
  "commands": [ "csharpier" ],
  "rollForward": false
}
```

The manifest pins CSharpier **1.2.6**. This value is taken from the manifest file itself,
not inferred from any tool output.

## Output Summary

`dotnet tool restore` exited 0 and printed
`Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` followed by
`Restore was successful.`. The manifest-pinned CSharpier version, read from the
repository-root `dotnet-tools.json`, is **1.2.6**, which agrees with the restored version the
command reported.
