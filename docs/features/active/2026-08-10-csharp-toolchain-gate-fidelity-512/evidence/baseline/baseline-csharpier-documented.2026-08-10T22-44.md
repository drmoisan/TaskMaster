# Baseline — the documented (defective) CSharpier format command ([P0-T7], Defect B)

Timestamp: 2026-08-10T22-44
Command: `./.dotnet-sdk/dotnet.exe tool run csharpier .`
EXIT_CODE: 1

This is the command documented at `CLAUDE.md` § C#1 item 1, `CLAUDE.md` § CUT3 step 1,
`CLAUDE.md` § "C# Toolchain (run in this exact order)" step 1, `.claude/rules/csharp.md` § Toolchain
item 1, and `.claude/skills/csharp-qa-gate/SKILL.md` § Toolchain Execution Sequence step 1.

## Verbatim rejection

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

Both required strings are present verbatim: `Required command was not provided.` and
`Unrecognized command or argument '.'`.

## Output Summary

The documented format command **fails**, returning `EXIT_CODE: 1` and formatting nothing. The
manifest-pinned CSharpier 1.2.6 ([P0-T5]) exposes only the subcommands `format`, `check`,
`pipe-files` and `server`; the documented bare-path form is CSharpier v0 syntax. Defect B is
reproduced at this branch head. `EXIT_CODE: 0` here would have contradicted the recorded defect and
halted the plan; it did not occur.
