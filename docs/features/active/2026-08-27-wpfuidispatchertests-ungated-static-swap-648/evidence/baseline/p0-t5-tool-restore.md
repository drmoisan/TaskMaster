# P0-T5 — Restore the Manifest-Pinned Formatter

Timestamp: 2026-09-01T13-25

Command: `dotnet tool restore` (run from the checkout root, with `PATH` and `DOTNET_ROOT` pointed at
the repository-local `.dotnet-sdk` directory that P0-T4 installed)

EXIT_CODE: 0

ManifestVersion: 1.2.6

Output Summary:

The manifest is `dotnet-tools.json` at the repository root. It declares the tool `csharpier` at
`dotnet-tools.json:5`, its `version` value `1.2.6` at `:6`, and `rollForward` false at `:10`. The
`ManifestVersion:` field above is read from `:6` rather than from a `--version` invocation, because
`CLAUDE.md:191` records that CSharpier v1 requires a subcommand, so the behavior of a root-level
`--version` option on 1.2.6 is not established by any observation this plan made.

Filtering the restore command's output for lines containing the substring `csharpier` returned one
line, recorded verbatim:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier
```

The command also printed `Restore was successful.` on a second line. The presence and wording of the
matching line are recorded, not asserted.
