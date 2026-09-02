# Phase 0 — dotnet tool restore (P0-T4)

Timestamp: 2026-09-01T21-28

Command: `dotnet tool restore`
EXIT_CODE: 0

Output Summary:

The run printed `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`
followed by `Restore was successful.`

The CSharpier version pinned by the tool manifest is **1.2.6**, read directly from the
repository-root file `dotnet-tools.json` rather than inferred from any tool output. The manifest
`tools.csharpier.version` value is the literal string `1.2.6`, with `rollForward` set to `false` and
a single command entry `csharpier`.

`dotnet-tools.json` at the repository root is the manifest present in this tree.
`.config/dotnet-tools.json` is confirmed ABSENT, so there is no second manifest that could pin a
different version.

Because `rollForward` is `false` and the manifest is the root manifest (`isRoot: true`), every
`dotnet tool run csharpier ...` invocation in this plan resolves to 1.2.6, matching the version
`.github/workflows/ci.yml` restores. No globally installed CSharpier is invoked anywhere in this
plan.

## Orchestrator-supplied preconditions

The following were performed by the orchestrator before delegation and are recorded here rather
than repeated: the repo-local `.dotnet-sdk` install (`dotnet --version` reports `8.0.205`), the
`packages/` restore (172 packages, analyzer versions verified in agreement with the csproj
`<Analyzer Include>` items at Meziantou.Analyzer 3.0.194 and Roslynator.Analyzers 5.0.0), and the
presence of the `dotnet-coverage` global tool at 18.10.0.
