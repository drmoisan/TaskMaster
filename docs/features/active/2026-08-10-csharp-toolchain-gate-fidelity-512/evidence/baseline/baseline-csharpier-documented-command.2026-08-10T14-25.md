# Baseline — documented CSharpier format command (#509)

Timestamp: 2026-08-10T14-25
Branch: bug/csharp-toolchain-gate-fidelity-512 (from origin/epic/build-ci-coverage-gate-fidelity-integration @ edf3d34c)
Worktree: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-af19fe9c37ece6a65

## Bootstrap performed first

Command: `pwsh -NoProfile ./scripts/vscode/Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0

Command: `./.dotnet-sdk/dotnet.exe tool restore`
EXIT_CODE: 0
Output Summary: `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` / `Restore was successful.`

## Measurement 1 — pinned version

Command: `./.dotnet-sdk/dotnet.exe tool run csharpier --version`
EXIT_CODE: 0
Output Summary: `1.2.6`. Confirms the manifest pin in `dotnet-tools.json` resolves to CSharpier 1.2.6.

## Measurement 2 — the documented command

Command: `./.dotnet-sdk/dotnet.exe tool run csharpier .`
EXIT_CODE: 1
Output Summary: The documented bare-path form is rejected by the pinned formatter. Verbatim output:

```
'.' was not matched. Did you mean one of the following?
-h
Required command was not provided.
Unrecognized command or argument '.'.

Description:

Usage:
  CSharpier [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  format <directoryOrFile>  Format files.
  check <directoryOrFile>   Check that files are formatted. Will not write any changes.
  pipe-files                Keep csharpier running so that multiples files can be piped to it via stdin.
  server                    Run CSharpier as a server so that multiple files may be formatted.
```

## Conclusion

Issue #509 is confirmed by direct measurement. The command documented at `CLAUDE.md:191`, `:192`,
`:381`, `:399`, `.claude/rules/csharp.md:14` and `.claude/skills/csharp-qa-gate/SKILL.md:30` returns a
non-zero exit code and performs no formatting against the version the repository pins. Step 1 of the
mandatory four-stage toolchain loop cannot be completed as documented.

`.github/workflows/ci.yml:93` already uses the correct form, `dotnet csharpier check .`.
