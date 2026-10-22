# Phase 0 — dotnet tool restore ([P0-T8])

Timestamp: 2026-08-28T05-12

Command: `dotnet tool restore`, run from the worktree root.
EXIT_CODE: 0

## Output

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Version verification

Command: `dotnet tool run csharpier --version`
EXIT_CODE: 0

```
1.2.6
```

## Acceptance checks

| Check | Required | Observed | Result |
| --- | --- | --- | --- |
| `dotnet tool restore` exit code | 0 | 0 | pass |
| `dotnet tool run csharpier --version` | `1.2.6` | `1.2.6` | pass |

## Notes carried forward

- `1.2.6` is the version pinned by `.config/dotnet-tools.json` and is the version
  `.github/workflows/ci.yml` runs after its own `dotnet tool restore`. Invoking through
  `dotnet tool run` is what selects it; a globally installed CSharpier would be a different version
  and would produce diffs that disagree with CI.
- CSharpier 1.x requires a subcommand, per decision D-5. The two forms used by this plan are
  `dotnet tool run csharpier format <paths>` (mutating, scope-locked to the seven owned files in
  `[P8-T1]`) and `dotnet tool run csharpier check .` (read-only, repository-wide, the gate in
  `[P0-T9]` and `[P8-T2]`). The bare-path form does not run and `pipe-files` writes to stdout and
  enforces nothing.

Output Summary: `dotnet tool restore` exited 0 and reported `Restore was successful`. The
manifest-pinned CSharpier resolves through `dotnet tool run` and reports version `1.2.6`, matching the
required pin.
