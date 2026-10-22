# Baseline — CSharpier Check (P0-T7)

Timestamp: 2026-09-01T12-14

Working directory: repository root (worktree for branch
`bug/qfc-metrics-flush-writes-empty-session-file-646`)
HEAD: `8a2054cd6c857195712c7db6cee0a34b631f3ca7`

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

## Verbatim Printed Summary Line

```
Checked 1566 files in 4451ms.
```

## Output Summary

Baseline formatting state is clean. CSharpier 1.2.6 (the version pinned by
`dotnet-tools.json`) checked 1566 files and listed no file as needing formatting; in
check mode CSharpier prints one `Error ...` line per non-compliant file before the summary,
and no such line was printed. Exit code 0.

This is a baseline capture, not a gate; it is recorded whatever the exit code, and in this
run the exit code was 0.

## Precondition Micro-Action Recorded

`dotnet tool run` initially failed with exit 155 and the repository's own `global.json`
`errorMessage`: the repo-local .NET SDK required by `global.json` (version `8.0.205`, with
`paths` limited to `.dotnet-sdk` and `$host$`) was absent from this fresh worktree, and the
host SDK is `10.0.400`, which `rollForward: latestFeature` does not accept for an `8.0.x`
pin. The repository's own provisioning script was run to satisfy the precondition:

Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0
Output: `Installed repo-local .NET SDK 8.0.205 to <worktree-root>/.dotnet-sdk.`

Command: `dotnet tool restore`
EXIT_CODE: 0
Output: `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`

`.dotnet-sdk/` is excluded from version control by `.gitignore` line 350 (`.dotnet*/`),
confirmed by `git check-ignore -v .dotnet-sdk/`, so this provisioning step adds nothing to
the change footprint checked by P2-T8.
