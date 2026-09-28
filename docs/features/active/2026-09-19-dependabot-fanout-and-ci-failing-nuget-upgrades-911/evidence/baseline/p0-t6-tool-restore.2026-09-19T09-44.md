# P0-T6 — dotnet Local Tool Restore

Timestamp: 2026-09-19T12-28

Command:
```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>";
  dotnet tool restore'
```

EXIT_CODE: 0

## Output

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Manifest location and contents

The manifest is at the **repository root**, `dotnet-tools.json`, not under `.config/`:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "csharpier": {
      "version": "1.2.6",
      "commands": [ "csharpier" ],
      "rollForward": false
    }
  }
}
```

`.github/workflows/_format-check.yml:31` hashes the same root path, so the local restore and the CI
format check are pinned by the same file.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS.
- The `Output Summary:` names CSharpier at the version the repository-root manifest pins: the
  manifest pins `1.2.6` and the restore output names `'csharpier' (version '1.2.6')`. PASS.

**Failing-condition reachability.** The failing condition is that the manifest cannot be restored,
which leaves every later CSharpier command unrunnable. It is reachable on a fresh worktree, where
the tool package is absent from the local NuGet cache and the restore is the step that fetches it;
`rollForward: false` additionally makes the restore fail rather than silently substitute a different
version if 1.2.6 were unavailable.

Output Summary: `dotnet tool restore` exited 0 and restored CSharpier 1.2.6, matching the version
pinned by the repository-root `dotnet-tools.json`. CSharpier is available for CMD-CSHARPIER-CHECK
and CMD-CSHARPIER-FORMAT via `dotnet tool run csharpier`.
